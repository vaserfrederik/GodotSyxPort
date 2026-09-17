using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.World;

public enum WorldEntityKind : byte
{
    Caravan,
    ImportShipment,
    ExportShipment,
    Haven,
    TouristParty,
    Army
}

public enum WorldEntityState : byte
{
    Waiting,
    Travelling,
    Arrived,
    Returning,
    Completed,
    Cancelled
}

public sealed class WorldEntityRecord
{
    public long Id { get; init; }
    public WorldEntityKind Kind { get; init; }
    public WorldEntityState State { get; internal set; }
    public int OwnerFactionId { get; init; }
    public int OriginRegionId { get; init; }
    public int DestinationRegionId { get; internal set; }
    public int CurrentRegionId { get; internal set; }
    public int RouteIndex { get; internal set; }
    public double EdgeProgress { get; internal set; }
    public double SpeedRegionsPerDay { get; set; } = 1;
    public double WaitingDays { get; internal set; }
    public string Purpose { get; init; } = "";
    public List<int> Route { get; } = new();
    public Dictionary<string, string> Tags { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsTerminal => State is WorldEntityState.Completed or WorldEntityState.Cancelled;
    public bool IsMoving => State is WorldEntityState.Travelling or WorldEntityState.Returning;
    public int NextRegionId => RouteIndex + 1 < Route.Count ? Route[RouteIndex + 1] : CurrentRegionId;

    public WorldEntitySnapshot Snapshot() => new(
        Id,
        Kind,
        State,
        OwnerFactionId,
        OriginRegionId,
        DestinationRegionId,
        CurrentRegionId,
        RouteIndex,
        EdgeProgress,
        SpeedRegionsPerDay,
        WaitingDays,
        Purpose,
        Route.ToArray(),
        new Dictionary<string, string>(Tags, StringComparer.OrdinalIgnoreCase));
}

public sealed record WorldEntitySnapshot(
    long Id,
    WorldEntityKind Kind,
    WorldEntityState State,
    int OwnerFactionId,
    int OriginRegionId,
    int DestinationRegionId,
    int CurrentRegionId,
    int RouteIndex,
    double EdgeProgress,
    double SpeedRegionsPerDay,
    double WaitingDays,
    string Purpose,
    IReadOnlyList<int> Route,
    IReadOnlyDictionary<string, string> Tags);

public sealed record WorldEntityArrival(
    long EntityId,
    WorldEntityKind Kind,
    int RegionId,
    bool FinalDestination,
    string Purpose);

/// <summary>
/// Compact simulation replacement for WEntity, WEntities, their constructor/map and
/// quadrant bookkeeping. Entities remain data records and never become Godot nodes.
/// </summary>
public sealed class WorldEntityRuntime
{
    private readonly StrategicWorldRuntime _world;
    private readonly Dictionary<long, WorldEntityRecord> _entities = new();
    private readonly Dictionary<int, HashSet<long>> _byRegion = new();
    private readonly List<WorldEntityArrival> _arrivals = new();
    private long _nextId = 1;

    public WorldEntityRuntime(StrategicWorldRuntime world)
    {
        _world = world;
    }

    public IReadOnlyCollection<WorldEntityRecord> Entities => _entities.Values;
    public IReadOnlyList<WorldEntityArrival> Arrivals => _arrivals;
    public int Revision { get; private set; }

    public WorldEntityRecord Create(
        WorldEntityKind kind,
        int ownerFactionId,
        int originRegionId,
        int destinationRegionId,
        string purpose,
        double speedRegionsPerDay = 1,
        IReadOnlyList<int>? route = null)
    {
        RequireRegion(originRegionId);
        RequireRegion(destinationRegionId);
        var path = NormalizeRoute(originRegionId, destinationRegionId,
            route ?? _world.FindRoute(originRegionId, destinationRegionId));
        ValidateRoute(originRegionId, destinationRegionId, path);
        var entity = new WorldEntityRecord
        {
            Id = _nextId++,
            Kind = kind,
            State = originRegionId == destinationRegionId
                ? WorldEntityState.Arrived
                : WorldEntityState.Travelling,
            OwnerFactionId = ownerFactionId,
            OriginRegionId = originRegionId,
            DestinationRegionId = destinationRegionId,
            CurrentRegionId = originRegionId,
            SpeedRegionsPerDay = Math.Max(0.01, speedRegionsPerDay),
            Purpose = purpose
        };
        entity.Route.AddRange(path);
        _entities.Add(entity.Id, entity);
        AddToRegion(entity.Id, entity.CurrentRegionId);
        Revision++;
        return entity;
    }

    public WorldEntityRecord Restore(WorldEntitySnapshot snapshot)
    {
        RequireRegion(snapshot.OriginRegionId);
        RequireRegion(snapshot.DestinationRegionId);
        ValidateRoute(snapshot.OriginRegionId, snapshot.DestinationRegionId, snapshot.Route);
        if (_entities.ContainsKey(snapshot.Id))
            throw new InvalidOperationException($"Duplicate world entity id {snapshot.Id}");
        var entity = new WorldEntityRecord
        {
            Id = snapshot.Id,
            Kind = snapshot.Kind,
            State = snapshot.State,
            OwnerFactionId = snapshot.OwnerFactionId,
            OriginRegionId = snapshot.OriginRegionId,
            DestinationRegionId = snapshot.DestinationRegionId,
            CurrentRegionId = snapshot.CurrentRegionId,
            RouteIndex = Math.Clamp(snapshot.RouteIndex, 0, Math.Max(0, snapshot.Route.Count - 1)),
            EdgeProgress = Math.Clamp(snapshot.EdgeProgress, 0, 1),
            SpeedRegionsPerDay = Math.Max(0.01, snapshot.SpeedRegionsPerDay),
            WaitingDays = Math.Max(0, snapshot.WaitingDays),
            Purpose = snapshot.Purpose
        };
        entity.Route.AddRange(snapshot.Route);
        foreach (var pair in snapshot.Tags) entity.Tags[pair.Key] = pair.Value;
        _entities.Add(entity.Id, entity);
        if (!entity.IsTerminal) AddToRegion(entity.Id, entity.CurrentRegionId);
        _nextId = Math.Max(_nextId, entity.Id + 1);
        Revision++;
        return entity;
    }

    public WorldEntityRecord? Get(long id) => _entities.GetValueOrDefault(id);

    public IEnumerable<WorldEntityRecord> AtRegion(int regionId)
    {
        return _byRegion.TryGetValue(regionId, out var ids)
            ? ids.Select(id => _entities[id])
            : Enumerable.Empty<WorldEntityRecord>();
    }

    public IEnumerable<WorldEntityRecord> OfKind(WorldEntityKind kind) =>
        _entities.Values.Where(entity => entity.Kind == kind && !entity.IsTerminal);

    public void Tick(double days)
    {
        if (days <= 0) return;
        _arrivals.Clear();
        foreach (var entity in _entities.Values.Where(value => !value.IsTerminal).ToArray())
        {
            if (!entity.IsMoving)
            {
                entity.WaitingDays += days;
                continue;
            }
            Advance(entity, days);
        }
        TrimTerminal();
    }

    public bool BeginReturn(long id)
    {
        if (!_entities.TryGetValue(id, out var entity) || entity.IsTerminal) return false;
        var found = _world.FindRoute(entity.CurrentRegionId, entity.OriginRegionId);
        if (found.Count == 0 && entity.CurrentRegionId != entity.OriginRegionId) return false;
        var route = NormalizeRoute(entity.CurrentRegionId, entity.OriginRegionId, found);
        entity.Route.Clear();
        entity.Route.AddRange(route);
        entity.RouteIndex = 0;
        entity.EdgeProgress = 0;
        entity.DestinationRegionId = entity.OriginRegionId;
        entity.State = entity.CurrentRegionId == entity.OriginRegionId
            ? WorldEntityState.Completed
            : WorldEntityState.Returning;
        Revision++;
        return true;
    }

    public bool Complete(long id)
    {
        if (!_entities.TryGetValue(id, out var entity) || entity.IsTerminal) return false;
        RemoveFromRegion(entity.Id, entity.CurrentRegionId);
        entity.State = WorldEntityState.Completed;
        Revision++;
        return true;
    }

    public bool Cancel(long id)
    {
        if (!_entities.TryGetValue(id, out var entity) || entity.IsTerminal) return false;
        RemoveFromRegion(entity.Id, entity.CurrentRegionId);
        entity.State = WorldEntityState.Cancelled;
        Revision++;
        return true;
    }

    public IReadOnlyList<WorldEntitySnapshot> Capture() =>
        _entities.Values.Where(entity => !entity.IsTerminal).Select(entity => entity.Snapshot()).ToArray();

    private void Advance(WorldEntityRecord entity, double days)
    {
        var remaining = days * entity.SpeedRegionsPerDay;
        while (remaining > 0 && entity.IsMoving)
        {
            if (entity.Route.Count <= 1 || entity.RouteIndex >= entity.Route.Count - 1)
            {
                Arrive(entity);
                break;
            }
            var edgeLeft = 1 - entity.EdgeProgress;
            var step = Math.Min(edgeLeft, remaining);
            entity.EdgeProgress += step;
            remaining -= step;
            if (entity.EdgeProgress < 1) continue;
            RemoveFromRegion(entity.Id, entity.CurrentRegionId);
            entity.RouteIndex++;
            entity.CurrentRegionId = entity.Route[entity.RouteIndex];
            entity.EdgeProgress = 0;
            AddToRegion(entity.Id, entity.CurrentRegionId);
            var final = entity.RouteIndex >= entity.Route.Count - 1;
            _arrivals.Add(new WorldEntityArrival(
                entity.Id,
                entity.Kind,
                entity.CurrentRegionId,
                final,
                entity.Purpose));
            Revision++;
            if (final) Arrive(entity);
        }
    }

    private void Arrive(WorldEntityRecord entity)
    {
        entity.EdgeProgress = 0;
        entity.WaitingDays = 0;
        entity.State = entity.State == WorldEntityState.Returning
            ? WorldEntityState.Completed
            : WorldEntityState.Arrived;
        Revision++;
    }

    private void TrimTerminal()
    {
        foreach (var entity in _entities.Values.Where(value => value.IsTerminal && value.WaitingDays > 1).ToArray())
        {
            RemoveFromRegion(entity.Id, entity.CurrentRegionId);
            _entities.Remove(entity.Id);
        }
    }

    private void AddToRegion(long id, int regionId)
    {
        if (!_byRegion.TryGetValue(regionId, out var ids))
        {
            ids = new HashSet<long>();
            _byRegion.Add(regionId, ids);
        }
        ids.Add(id);
    }

    private void RemoveFromRegion(long id, int regionId)
    {
        if (!_byRegion.TryGetValue(regionId, out var ids)) return;
        ids.Remove(id);
        if (ids.Count == 0) _byRegion.Remove(regionId);
    }

    private void RequireRegion(int regionId)
    {
        if (_world.Region(regionId) is null)
            throw new ArgumentOutOfRangeException(nameof(regionId), regionId, "Unknown world region");
    }

    private void ValidateRoute(int origin, int destination, IReadOnlyList<int> route)
    {
        if (route.Count == 0 || route[0] != origin || route[^1] != destination)
            throw new InvalidOperationException("World entity route must connect origin to destination");
        for (var i = 1; i < route.Count; i++)
        {
            var previous = _world.Region(route[i - 1]);
            if (previous is null || !previous.Neighbours.Contains(route[i]))
                throw new InvalidOperationException("World entity route contains a disconnected edge");
        }
    }

    private static IReadOnlyList<int> NormalizeRoute(
        int origin, int destination, IReadOnlyList<int> route)
    {
        if (origin == destination) return new[] { origin };
        if (route.Count == 0) return route;
        if (route[0] == origin) return route;
        var normalized = new int[route.Count + 1];
        normalized[0] = origin;
        for (var i = 0; i < route.Count; i++) normalized[i + 1] = route[i];
        return normalized;
    }
}
