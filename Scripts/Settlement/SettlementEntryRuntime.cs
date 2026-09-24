using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;

namespace GodotSyxPort.Settlement;

public enum ArrivalCause : byte { Born, Immigrated, Emancipated, Parole, SoldierReturn, Cured }
public enum LeaveCause : byte
{
    Army, Emigrated, Starved, Sacrificed, Slain, Animal, Age, Accident, Heat, Cold,
    Murder, Disease, Executed, Punished, Drowned, Deserted, Exiled, Brawl, Other, Insanity, Sold
}

public sealed record SettlementEntryPoint(int Index, GridCoord Cell, GridCoord OutsideDirection)
{
    public bool Active { get; internal set; } = true;
    public bool Reachable { get; internal set; }
}

public readonly record struct IncomingPopulation(string Race, HumanoidType Type);

/// <summary>EntryPoints + EntryUpdater + PeopleSpawner without their render/debug surfaces.</summary>
public sealed class SettlementEntryRuntime
{
    private readonly WorldGridData _world;
    private readonly List<SettlementEntryPoint> _points = new();
    private readonly Dictionary<IncomingPopulation, int> _incoming = new();
    private double _spawnTime;
    private double _reachabilityTime;
    private bool _reachabilityDirty;
    private int _roundRobin;
    // Reused scratch buffers keep the reachability refresh off the GC hot path.
    // A refresh can cover the full 768x768 settlement map after loading walls.
    private readonly int[] _reachabilityQueue;
    private readonly int[] _reachabilityMarks;
    private int _reachabilityGeneration;
    private int _reachabilityHead;
    private int _reachabilityTail;
    private bool _reachabilityInProgress;
    private const int ReachabilityCellsPerTick = 4096;
    public IReadOnlyList<SettlementEntryPoint> Points => _points;
    public IEnumerable<SettlementEntryPoint> Reachable => _points.Where(point => point.Active && point.Reachable);
    public bool Besieged { get; private set; }
    public bool IsClosed => Besieged || !Reachable.Any();
    public double SiegeSeconds { get; private set; }
    public int IncomingTotal => _incoming.Values.Sum();
    public ImmigrationRuntime Immigration { get; }

    public SettlementEntryRuntime(WorldGridData world, int spacing = 32)
    {
        _world = world;
        _reachabilityQueue = new int[world.Width * world.Height];
        _reachabilityMarks = new int[world.Width * world.Height];
        Immigration = new ImmigrationRuntime();
        spacing = Math.Max(8, spacing);
        if (!AddGeneratedRoadEntryPoints()) AddEdgePoints(spacing);
        BeginReachabilityRefresh();
    }

    public void Add(string race, HumanoidType type, int amount)
    {
        if (amount < 0) return;
        if (amount > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(amount));
        var key = new IncomingPopulation(race, type);
        _incoming[key] = Math.Min(short.MaxValue, _incoming.GetValueOrDefault(key) + amount);
    }

    public int OnTheirWay(string? race = null, HumanoidType? type = null) => _incoming
        .Where(pair => (race is null || pair.Key.Race.Equals(race, StringComparison.OrdinalIgnoreCase)) &&
                       (type is null || pair.Key.Type == type))
        .Sum(pair => pair.Value);

    public int UpdateImmigration(
        RaceRule race, double delta, double secondsPerDay, double happiness,
        double expectedPopulation, int currentPopulation, double immigrationBoost = 1,
        bool campAvailable = false, int campPopulation = 0, double standingPower = 1,
        double campReplenishmentPerDay = 0)
    {
        var incoming = OnTheirWay(race.Key, HumanoidType.Subject);
        var admitted = Immigration.Update(
            race, delta, secondsPerDay, happiness, expectedPopulation,
            currentPopulation + incoming, immigrationBoost, campAvailable, campPopulation, standingPower,
            incoming, campReplenishmentPerDay);
        if (admitted > 0) Add(race.Key, HumanoidType.Subject, admitted);
        return admitted;
    }

    public void SetBesieged(bool besieged)
    {
        Besieged = besieged;
        if (!besieged) SiegeSeconds = 0;
    }

    public void InvalidateReachability()
    {
        _reachabilityDirty = true;
        _reachabilityTime = 0;
        _reachabilityInProgress = false;
    }

    public void Tick(double delta, CitizenSystem citizens)
    {
        if (Besieged) SiegeSeconds += delta;
        if (_reachabilityInProgress) AdvanceReachabilityRefresh();
        if (_reachabilityDirty) _reachabilityTime += delta;
        if (_reachabilityDirty && _reachabilityTime >= 5)
        {
            _reachabilityTime = 0;
            _reachabilityDirty = false;
            BeginReachabilityRefresh();
        }
        if (IsClosed || IncomingTotal == 0) return;
        _spawnTime += delta;
        // PeopleSpawner.update starts a spawn as soon as positive elapsed time is available.
        while (_spawnTime > 0 && IncomingTotal > 0)
        {
            _spawnTime -= 1;
            var pending = _incoming.Where(pair => pair.Value > 0)
                .OrderBy(pair => pair.Key.Race).ThenBy(pair => pair.Key.Type).ToArray();
            if (pending.Length == 0) return;
            var selected = pending[_roundRobin++ % pending.Length];
            var entries = Reachable.ToArray();
            if (entries.Length == 0) return;
            var entry = entries[_roundRobin % entries.Length];
            var socialClass = HumanoidTypeRules.ClassOf(selected.Key.Type);
            citizens.SpawnCitizen(
                entry.Cell, selected.Key.Race, socialClass, selected.Key.Type,
                CitizenOrigin.Immigrant, ArrivalCause.Immigrated);
            _incoming[selected.Key]--;
        }
    }

    public GridCoord? NearestReachable(GridCoord from) => Reachable
        .OrderBy(point => Math.Abs(point.Cell.X - from.X) + Math.Abs(point.Cell.Z - from.Z))
        .Select(point => (GridCoord?)point.Cell).FirstOrDefault();

    public bool TryBeginEmigration(CitizenSystem citizens, int citizenId, GridCoord from)
    {
        if (IsClosed) return false;
        var destination = NearestReachable(from);
        return destination is not null && citizens.TryBeginEmigration(citizenId, destination.Value);
    }

    private void AddEdgePoints(int spacing)
    {
        var index = 0;
        for (var x = spacing / 2; x < _world.Width; x += spacing)
        {
            _points.Add(new SettlementEntryPoint(index++, new GridCoord(x, 0), new GridCoord(0, -1)));
            _points.Add(new SettlementEntryPoint(index++, new GridCoord(x, _world.Height - 1), new GridCoord(0, 1)));
        }
        for (var z = spacing / 2; z < _world.Height; z += spacing)
        {
            _points.Add(new SettlementEntryPoint(index++, new GridCoord(0, z), new GridCoord(-1, 0)));
            _points.Add(new SettlementEntryPoint(index++, new GridCoord(_world.Width - 1, z), new GridCoord(1, 0)));
        }
    }

    private bool AddGeneratedRoadEntryPoints()
    {
        var index = 0;
        void AddRuns(int length, Func<int, GridCoord> cellAt, GridCoord outside)
        {
            var runStart = -1;
            for (var offset = 0; offset <= length; offset++)
            {
                var road = offset < length && _world.Has(cellAt(offset), TileFlags.Road);
                if (road && runStart < 0) runStart = offset;
                if (road || runStart < 0) continue;
                var middle = (runStart + offset - 1) / 2;
                _points.Add(new SettlementEntryPoint(index++, cellAt(middle), outside));
                runStart = -1;
            }
        }
        AddRuns(_world.Width, x => new GridCoord(x, 0), new GridCoord(0, -1));
        AddRuns(_world.Width, x => new GridCoord(x, _world.Height - 1), new GridCoord(0, 1));
        AddRuns(_world.Height, z => new GridCoord(0, z), new GridCoord(-1, 0));
        AddRuns(_world.Height, z => new GridCoord(_world.Width - 1, z), new GridCoord(1, 0));
        return _points.Count > 0;
    }

    private void BeginReachabilityRefresh()
    {
        var generation = ++_reachabilityGeneration;
        // The generation counter avoids clearing a 1.44M element bool array on
        // every refresh while retaining the same flood-fill semantics.
        if (generation == int.MaxValue)
        {
            Array.Clear(_reachabilityMarks, 0, _reachabilityMarks.Length);
            generation = _reachabilityGeneration = 1;
        }
        _reachabilityHead = 0;
        _reachabilityTail = 0;
        var center = FindReachabilityStart();
        var centerIndex = center.Z * _world.Width + center.X;
        if (!_world.IsBlocked(center))
        {
            _reachabilityMarks[centerIndex] = generation;
            _reachabilityQueue[_reachabilityTail++] = centerIndex;
        }
        _reachabilityInProgress = true;
    }

    private GridCoord FindReachabilityStart()
    {
        var center = new GridCoord(_world.Width / 2, _world.Height / 2);
        if (!_world.IsBlocked(center)) return center;
        for (var radius = 1; radius < Math.Max(_world.Width, _world.Height); radius++)
        for (var dx = -radius; dx <= radius; dx++)
        {
            var dz = radius - Math.Abs(dx);
            var first = new GridCoord(center.X + dx, center.Z + dz);
            if (_world.IsInside(first) && !_world.IsBlocked(first)) return first;
            if (dz == 0) continue;
            var second = new GridCoord(center.X + dx, center.Z - dz);
            if (_world.IsInside(second) && !_world.IsBlocked(second)) return second;
        }
        return center;
    }

    private void AdvanceReachabilityRefresh()
    {
        var remaining = ReachabilityCellsPerTick;
        var generation = _reachabilityGeneration;
        Span<int> neighbors = stackalloc int[4];
        while (_reachabilityHead < _reachabilityTail && remaining-- > 0)
        {
            var index = _reachabilityQueue[_reachabilityHead++];
            var x = index % _world.Width;
            var z = index / _world.Width;
            neighbors[0] = z > 0 ? index - _world.Width : -1;
            neighbors[1] = x > 0 ? index - 1 : -1;
            neighbors[2] = x + 1 < _world.Width ? index + 1 : -1;
            neighbors[3] = z + 1 < _world.Height ? index + _world.Width : -1;
            foreach (var nextIndex in neighbors)
            {
                if (nextIndex < 0 || _reachabilityMarks[nextIndex] == generation)
                    continue;
                var next = new GridCoord(nextIndex % _world.Width, nextIndex / _world.Width);
                if (_world.IsBlocked(next)) continue;
                _reachabilityMarks[nextIndex] = generation;
                _reachabilityQueue[_reachabilityTail++] = nextIndex;
            }
        }
        if (_reachabilityHead < _reachabilityTail) return;
        foreach (var point in _points)
            point.Reachable = _reachabilityMarks[point.Cell.Z * _world.Width + point.Cell.X] == generation;
        _reachabilityInProgress = false;
    }
}
