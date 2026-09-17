using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public enum SpecialProductionKind : byte
{
    Pasture,
    Orchard,
    Fishery,
    Hunter,
    Woodcutter
}

public sealed class SpecialProductionInstanceRuntime
{
    public int RoomId { get; init; }
    public SpecialProductionKind Kind { get; init; }
    public string Animal { get; init; } = "";
    public int AnimalMaximum { get; set; }
    public int Animals { get; set; }
    public int Cubs { get; set; }
    public double AnimalsToDie { get; set; }
    public int WorkCyclesToday { get; set; }
    public int RequiredCyclesPerDay { get; set; }
    public double PreviousTending { get; set; } = 1.0;
    public int GrowthDays { get; set; }
    public double HunterLuck { get; set; } = 1.0;
    public int HunterYear { get; set; } = -1;
    public double FishAccess { get; set; }
    public bool LivestockSupplyPending { get; set; }
    public HashSet<int> WorkedCellsToday { get; } = new();
    public HashSet<int> HarvestedCellsThisYear { get; } = new();
    public Dictionary<int, int> WoodWorkByCell { get; } = new();
    public Dictionary<int, int> CubCohorts { get; } = new();

    public double Tending => RequiredCyclesPerDay <= 0
        ? 1.0
        : Math.Clamp((double)WorkCyclesToday / RequiredCyclesPerDay, 0.0, 1.0);
}

public sealed class SpecialProductionState
{
    public int RoomId { get; set; }
    public byte Kind { get; set; }
    public int Animals { get; set; }
    public int Cubs { get; set; }
    public double AnimalsToDie { get; set; }
    public int WorkCyclesToday { get; set; }
    public double PreviousTending { get; set; }
    public int GrowthDays { get; set; }
    public double HunterLuck { get; set; }
    public int HunterYear { get; set; }
    public int[] WorkedCellsToday { get; set; } = Array.Empty<int>();
    public int[] HarvestedCellsThisYear { get; set; } = Array.Empty<int>();
    public int[] WoodCells { get; set; } = Array.Empty<int>();
    public int[] WoodWork { get; set; } = Array.Empty<int>();
    public int[] CubDays { get; set; } = Array.Empty<int>();
    public int[] CubAmounts { get; set; } = Array.Empty<int>();
}

public sealed class SpecialProductionSnapshot
{
    public double DayTime { get; set; }
    public int ElapsedDays { get; set; }
    public SpecialProductionState[] Instances { get; set; } = Array.Empty<SpecialProductionState>();
}

/// <summary>
/// Shared, data-driven runtime for the source game's outdoor food rooms. The original
/// implementations have different job classes, but share room lifecycle, work and output
/// plumbing; only their ecological state and production multiplier differ.
/// </summary>
public sealed class SpecialProductionRuntime
{
    private const int DaysPerYear = 16;
    private readonly GridWorld _world;
    private readonly Dictionary<int, SpecialProductionInstanceRuntime> _instances = new();
    private readonly Dictionary<int, RoomRecord> _rooms = new();
    private double _dayTime;
    private int _elapsedDays;

    public SpecialProductionRuntime(GridWorld world) => _world = world;

    public IReadOnlyDictionary<int, SpecialProductionInstanceRuntime> Instances => _instances;

    public SpecialProductionSnapshot Capture() => new()
    {
        DayTime = _dayTime,
        ElapsedDays = _elapsedDays,
        Instances = _instances.Values.Select(instance => new SpecialProductionState
        {
            RoomId = instance.RoomId,
            Kind = (byte)instance.Kind,
            Animals = instance.Animals,
            Cubs = instance.Cubs,
            AnimalsToDie = instance.AnimalsToDie,
            WorkCyclesToday = instance.WorkCyclesToday,
            PreviousTending = instance.PreviousTending,
            GrowthDays = instance.GrowthDays,
            HunterLuck = instance.HunterLuck,
            HunterYear = instance.HunterYear,
            WorkedCellsToday = instance.WorkedCellsToday.ToArray(),
            HarvestedCellsThisYear = instance.HarvestedCellsThisYear.ToArray(),
            WoodCells = instance.WoodWorkByCell.Keys.ToArray(),
            WoodWork = instance.WoodWorkByCell.Values.ToArray(),
            CubDays = instance.CubCohorts.Keys.ToArray(),
            CubAmounts = instance.CubCohorts.Values.ToArray()
        }).ToArray()
    };

    public void Restore(SpecialProductionSnapshot snapshot)
    {
        _dayTime = Math.Max(0, snapshot.DayTime);
        _elapsedDays = Math.Max(0, snapshot.ElapsedDays);
        foreach (var state in snapshot.Instances)
        {
            if (!_instances.TryGetValue(state.RoomId, out var instance) ||
                instance.Kind != (SpecialProductionKind)state.Kind) continue;
            instance.Animals = Math.Clamp(state.Animals, 0, instance.AnimalMaximum);
            instance.Cubs = Math.Clamp(state.Cubs, 0, instance.Animals);
            instance.AnimalsToDie = Math.Max(0, state.AnimalsToDie);
            instance.WorkCyclesToday = Math.Max(0, state.WorkCyclesToday);
            instance.PreviousTending = Math.Clamp(state.PreviousTending, 0, 1);
            instance.GrowthDays = Math.Max(0, state.GrowthDays);
            instance.HunterLuck = Math.Max(0, state.HunterLuck);
            instance.HunterYear = state.HunterYear;
            instance.WorkedCellsToday.Clear();
            foreach (var cell in state.WorkedCellsToday) instance.WorkedCellsToday.Add(cell);
            instance.HarvestedCellsThisYear.Clear();
            foreach (var cell in state.HarvestedCellsThisYear) instance.HarvestedCellsThisYear.Add(cell);
            instance.WoodWorkByCell.Clear();
            foreach (var pair in state.WoodCells.Zip(state.WoodWork))
                instance.WoodWorkByCell[pair.First] = pair.Second;
            instance.CubCohorts.Clear();
            foreach (var pair in state.CubDays.Zip(state.CubAmounts))
                instance.CubCohorts[pair.First] = pair.Second;
        }
    }

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms)
        {
            var rule = OriginalGameData.Current.Room(room.DefinitionKey)?.SpecialProduction;
            if (rule is null || !TryKind(rule.Kind, out var kind)) continue;
            active.Add(room.Id);
            _rooms[room.Id] = room;
            if (!_instances.TryGetValue(room.Id, out var instance))
            {
                instance = new SpecialProductionInstanceRuntime
                {
                    RoomId = room.Id,
                    Kind = kind,
                    Animal = rule.Animal,
                    GrowthDays = kind == SpecialProductionKind.Orchard ? 0 : rule.DaysTillGrowth,
                    HunterLuck = HunterLuck(room.Id, 0),
                    HunterYear = 0
                };
                _instances.Add(room.Id, instance);
            }
            Refresh(room, instance, rule);
        }
        foreach (var roomId in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
        {
            _instances.Remove(roomId);
            _rooms.Remove(roomId);
        }
    }

    public void Tick(double delta, int secondsPerDay)
    {
        if (secondsPerDay <= 0 || delta <= 0) return;
        _dayTime += delta;
        while (_dayTime >= secondsPerDay)
        {
            _dayTime -= secondsPerDay;
            _elapsedDays++;
            AdvanceDay();
        }
    }

    public bool IsWorkCell(RoomRecord room, GridCoord cell)
    {
        if (!_instances.TryGetValue(room.Id, out var instance)) return false;
        var cellIndex = _world.CellToIndex(cell);
        return instance.Kind switch
        {
            SpecialProductionKind.Fishery => FishAccess(cell) > 0,
            SpecialProductionKind.Woodcutter => _world.Data.VegetationAmount(cell) > 0,
            SpecialProductionKind.Orchard => !instance.WorkedCellsToday.Contains(cellIndex) &&
                _world.Data.Has(cell, TileFlags.Furniture) &&
                !_world.Data.Has(cell, TileFlags.Wall | TileFlags.DeepWater | TileFlags.Mountain),
            SpecialProductionKind.Pasture =>
                !_world.Data.Has(cell, TileFlags.Wall | TileFlags.DeepWater | TileFlags.Mountain),
            _ => _world.Data.Has(cell, TileFlags.Furniture)
        };
    }

    public double ProductionMultiplier(RoomRecord room, GridCoord cell)
    {
        if (!_instances.TryGetValue(room.Id, out var instance)) return 1.0;
        return instance.Kind switch
        {
            SpecialProductionKind.Pasture => PastureMultiplier(room, instance),
            SpecialProductionKind.Orchard => OrchardMultiplier(room, instance, cell),
            SpecialProductionKind.Fishery => FishAccess(cell),
            SpecialProductionKind.Hunter => HunterMultiplier(room, instance),
            SpecialProductionKind.Woodcutter => Math.Clamp(
                _world.Data.VegetationAmount(cell) / 15.0, 0.0, 1.0),
            _ => 1.0
        };
    }

    public void RecordCompletedCycle(RoomRecord room, GridCoord cell)
    {
        if (!_instances.TryGetValue(room.Id, out var instance)) return;
        instance.WorkCyclesToday++;
        var cellIndex = _world.CellToIndex(cell);
        instance.WorkedCellsToday.Add(cellIndex);
        if (instance.Kind == SpecialProductionKind.Orchard && OrchardIsRipe(room, instance))
            instance.HarvestedCellsThisYear.Add(cellIndex);
        if (instance.Kind == SpecialProductionKind.Woodcutter)
        {
            var work = instance.WoodWorkByCell.GetValueOrDefault(cellIndex) + 1;
            // Source Job.workPerDay = ceil(workSeconds / 60): vegetation changes only
            // after a complete day of chopping a marked cell, not after every swing.
            var workPerDay = (int)Math.Ceiling(
                OriginalGameData.Current.SecondsPerDay * 0.5 / 60.0);
            if (work >= workPerDay)
            {
                _world.Data.ClearVegetationStep(cell);
                work = 0;
            }
            instance.WoodWorkByCell[cellIndex] = work;
        }
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source)
    {
        if (source is null || resources.Get(ResourceKind.Livestock) <= 0) return;
        foreach (var instance in _instances.Values.Where(value =>
                     value.Kind == SpecialProductionKind.Pasture &&
                     value.Animals < value.AnimalMaximum && !value.LivestockSupplyPending))
        {
            if (!_rooms.TryGetValue(instance.RoomId, out var room) ||
                room.State != RoomState.Operational || room.Cells.Count == 0) continue;
            if (jobs.All.Any(job => job.Kind == BuildKind.PastureLivestockSupply &&
                                    job.RoomId == room.Id &&
                                    job.State is not (JobState.Completed or JobState.Cancelled)))
            {
                instance.LivestockSupplyPending = true;
                continue;
            }
            var amount = Math.Min(BuildJob.MaximumFetchAmount,
                Math.Min(instance.AnimalMaximum - instance.Animals,
                    resources.Get(ResourceKind.Livestock)));
            if (amount <= 0) continue;
            foreach (var index in room.Cells)
            {
                var cell = _world.FromIndex(index);
                if (!jobs.Add(BuildJob.PastureLivestockSupply(cell, source.Value, room.Id, amount))) continue;
                instance.LivestockSupplyPending = true;
                break;
            }
        }
    }

    public int CompleteLivestockSupply(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var instance)) return amount;
        var accepted = Math.Min(amount, Math.Max(0, instance.AnimalMaximum - instance.Animals));
        instance.Animals += accepted;
        instance.Cubs += accepted;
        instance.CubCohorts[_elapsedDays] = instance.CubCohorts.GetValueOrDefault(_elapsedDays) + accepted;
        instance.LivestockSupplyPending = false;
        return amount - accepted;
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        var active = jobs.Where(job => job.Kind == BuildKind.PastureLivestockSupply &&
                                      job.State is not (JobState.Completed or JobState.Cancelled))
            .Select(job => job.RoomId).ToHashSet();
        foreach (var instance in _instances.Values)
            instance.LivestockSupplyPending = active.Contains(instance.RoomId);
    }

    private void AdvanceDay()
    {
        foreach (var instance in _instances.Values)
        {
            if (!_rooms.TryGetValue(instance.RoomId, out var room)) continue;
            if (instance.Kind == SpecialProductionKind.Pasture)
            {
                foreach (var cohort in instance.CubCohorts.Where(pair =>
                             _elapsedDays - pair.Key >= 14).ToArray())
                {
                    instance.Cubs = Math.Max(0, instance.Cubs - cohort.Value);
                    instance.CubCohorts.Remove(cohort.Key);
                }
                var needed = instance.AnimalMaximum <= 0 ? 0 : (int)Math.Ceiling(
                    instance.RequiredCyclesPerDay * instance.Animals / (double)instance.AnimalMaximum);
                var tending = needed <= 0 ? 1.0 : Math.Clamp(
                    instance.WorkCyclesToday / (double)needed, 0.0, 1.0);
                var toDie = (1.0 - tending) * instance.Animals;
                var deaths = (int)Math.Min(toDie, instance.AnimalsToDie);
                if (deaths > 0)
                {
                    instance.Animals = Math.Max(0, instance.Animals - deaths);
                    var cubDeaths = Math.Min(instance.Cubs, deaths);
                    instance.Cubs -= cubDeaths;
                }
                instance.AnimalsToDie = toDie;
                instance.PreviousTending = tending;
            }
            else if (instance.Kind == SpecialProductionKind.Orchard)
            {
                var rule = OriginalGameData.Current.Room(room.DefinitionKey)?.SpecialProduction;
                if (rule is not null && instance.Tending >= 0.5)
                    instance.GrowthDays = Math.Min(rule.DaysTillGrowth, instance.GrowthDays + 1);
                else if (instance.GrowthDays > 0 && instance.Tending < 0.25)
                    instance.GrowthDays--;
                if (rule is not null)
                {
                    var ripeDay = Math.Clamp(
                        (int)Math.Floor(DaysPerYear * rule.RipeAtPartOfYear), 0, DaysPerYear - 1);
                    var deadDay = (ripeDay + 3) % DaysPerYear;
                    if (_elapsedDays % DaysPerYear == deadDay)
                        instance.HarvestedCellsThisYear.Clear();
                }
            }
            if (instance.Kind == SpecialProductionKind.Hunter)
            {
                var year = _elapsedDays / DaysPerYear;
                if (year != instance.HunterYear)
                {
                    instance.HunterYear = year;
                    instance.HunterLuck = HunterLuck(room.Id, year);
                }
            }
            instance.WorkCyclesToday = 0;
            instance.WorkedCellsToday.Clear();
        }
    }

    private void Refresh(RoomRecord room, SpecialProductionInstanceRuntime instance, SpecialProductionRule rule)
    {
        var workSeconds = instance.Kind == SpecialProductionKind.Pasture ? 20.0 :
            instance.Kind is SpecialProductionKind.Fishery or SpecialProductionKind.Woodcutter
                ? 60.0 : 45.0;
        var workers = Math.Max(1, room.Employment.Needed);
        instance.RequiredCyclesPerDay = Math.Max(1,
            (int)Math.Ceiling(workers * OriginalGameData.Current.SecondsPerDay * 0.5 / workSeconds));
        if (instance.Kind == SpecialProductionKind.Pasture)
        {
            var animalsPerTile = Math.Clamp(2.5 / (rule.AnimalMass + 10.0), 0.0, 1.0 / 9.0);
            instance.AnimalMaximum = Math.Max(1, (int)Math.Floor(room.Cells.Count * animalsPerTile));
            instance.Animals = Math.Min(instance.Animals, instance.AnimalMaximum);
        }
        else if (instance.Kind == SpecialProductionKind.Fishery)
        {
            instance.FishAccess = room.Cells.Count == 0 ? 0 : room.Cells
                .Select(index => FishAccess(_world.FromIndex(index))).DefaultIfEmpty().Max();
        }
    }

    private double PastureMultiplier(RoomRecord room, SpecialProductionInstanceRuntime instance)
    {
        if (instance.AnimalMaximum <= 0 || instance.Animals <= 0) return 0;
        var occupancy = Math.Clamp((double)instance.Animals / instance.AnimalMaximum, 0.0, 1.0);
        var adultShare = Math.Clamp(
            (double)(instance.Animals - instance.Cubs) / instance.Animals, 0.0, 1.0);
        var adults = 0.1 + 0.9 * adultShare;
        var isolation = OriginalGameData.Current.Room(room.DefinitionKey)?.Construction.Indoors == true
            ? room.Isolation
            : 1.0;
        return occupancy * adults * instance.PreviousTending * isolation;
    }

    private double OrchardMultiplier(
        RoomRecord room, SpecialProductionInstanceRuntime instance, GridCoord cell)
    {
        var rule = OriginalGameData.Current.Room(room.DefinitionKey)?.SpecialProduction;
        if (rule is null || instance.GrowthDays < rule.DaysTillGrowth) return 0;
        if (!OrchardIsRipe(room, instance) ||
            instance.HarvestedCellsThisYear.Contains(_world.CellToIndex(cell))) return 0;
        // ROOM_ORCHARD.AmountPerTile = daysPerYear / TILES_PER_WORKER, where
        // TILES_PER_WORKER = workSeconds / (45 + walk-next 3). Undo the generic
        // per-cycle factor so one annual tree harvest receives that exact amount.
        var workDaySeconds = OriginalGameData.Current.SecondsPerDay * 0.5;
        var tilesPerWorker = workDaySeconds / (45.0 + 3.0);
        var amountPerTile = DaysPerYear / tilesPerWorker;
        var cycleFactor = 2.0 * 45.0 / OriginalGameData.Current.SecondsPerDay;
        return cycleFactor <= 0 ? 0 : amountPerTile / cycleFactor;
    }

    private bool OrchardIsRipe(RoomRecord room, SpecialProductionInstanceRuntime instance)
    {
        var rule = OriginalGameData.Current.Room(room.DefinitionKey)?.SpecialProduction;
        if (rule is null || instance.GrowthDays < rule.DaysTillGrowth) return false;
        var ripeDay = Math.Clamp((int)Math.Floor(DaysPerYear * rule.RipeAtPartOfYear), 0, DaysPerYear - 1);
        var day = _elapsedDays % DaysPerYear;
        return (day - ripeDay + DaysPerYear) % DaysPerYear < 3;
    }

    private static double HunterMultiplier(RoomRecord room, SpecialProductionInstanceRuntime instance)
    {
        var maximum = OriginalGameData.Current.Room(room.DefinitionKey)?.SpecialProduction?.MaximumEmployed ?? 0;
        if (maximum <= 0 || room.Employment.Employed <= maximum) return instance.HunterLuck;
        var excess = room.Employment.Employed - maximum;
        return instance.HunterLuck / (1.0 + excess / (maximum * 4.0));
    }

    private double FishAccess(GridCoord cell)
    {
        var best = _world.Data.FishAmount(cell);
        foreach (var offset in GridCoord.Cardinal)
        {
            var neighbour = cell + offset;
            if (_world.Data.Has(neighbour, TileFlags.FishSpot | TileFlags.Water))
                best = Math.Max(best, _world.Data.FishAmount(neighbour));
        }
        return Math.Clamp(best / 15.0, 0.0, 1.0);
    }

    private static bool TryKind(string source, out SpecialProductionKind kind) =>
        Enum.TryParse(source, true, out kind);

    private static double HunterLuck(int roomId, int year)
    {
        unchecked
        {
            var hash = (uint)(roomId * 1103515245 + year * 12345 + 0x51f15e);
            hash ^= hash >> 16;
            return 0.6 + (hash & 0xffff) / 65535.0 * 0.8;
        }
    }
}
