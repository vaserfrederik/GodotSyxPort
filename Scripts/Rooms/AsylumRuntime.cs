using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

/// <summary>
/// Room-side detention state from ROOM_ASYLUM: reservable cells, per-station
/// rations and guard-dependent treatment. It deliberately does not manufacture
/// citizens: capture/crime AI will admit real subjects through TryAdmit later.
/// </summary>
public sealed class AsylumRuntime
{
    private const int FoodMaximum = 15; // Food.amount = Bits(0b001111)
    private readonly Dictionary<int, AsylumInstanceRuntime> _instances = new();
    private int _nextSubjectId = 1;

    public IReadOnlyCollection<AsylumInstanceRuntime> Instances => _instances.Values;
    public int Prisoners => _instances.Values.Sum(instance => instance.Prisoners.Count);
    public int PrisonersMaximum => _instances.Values.Sum(instance => instance.Cells.Count);

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     room.DefinitionKey.Equals("_ASYLUM", StringComparison.OrdinalIgnoreCase)))
        {
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
            {
                instance = new AsylumInstanceRuntime { RoomId = room.Id };
                _instances.Add(room.Id, instance);
            }
            instance.Degradation = room.Degradation;
            instance.GuardsMaximum = room.Employment.Maximum;
            instance.GuardsEmployed = room.Employment.Employed;
            var capacity = Math.Max(1, room.Cells.Count);
            // The legacy constructor marks explicit entrance tiles as cells. That
            // furnishing tile-code is not represented in the normalized blueprint,
            // so retain reservations and use each placed room tile as a conservative
            // candidate until the individual furnisher layer is imported.
            while (instance.Cells.Count < capacity)
                instance.Cells.Add(new AsylumCellRuntime { Index = instance.Cells.Count });
            while (instance.Cells.Count > capacity)
            {
                var removable = instance.Cells.FindLastIndex(cell => !cell.Reserved);
                if (removable < 0) break;
                instance.Cells.RemoveAt(removable);
            }
            instance.FoodStation = fromIndex(room.Cells.First());
        }
        foreach (var roomId in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(roomId);
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var instance in _instances.Values) instance.FoodSupplyPending = false;
        foreach (var job in jobs.Where(job => job.Kind == BuildKind.AsylumFoodSupply &&
                     job.State is not (JobState.Completed or JobState.Cancelled)))
            if (_instances.TryGetValue(job.RoomId, out var instance)) instance.FoodSupplyPending = true;
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source)
    {
        if (source is null) return;
        foreach (var instance in _instances.Values)
        {
            if (instance.Prisoners.Count == 0 || instance.FoodSupplyPending ||
                instance.Food >= FoodMaximum || resources.Get(ResourceKind.Ration) <= 0) continue;
            var amount = Math.Min(BuildJob.MaximumFetchAmount,
                Math.Min(FoodMaximum - instance.Food, resources.Get(ResourceKind.Ration)));
            if (amount > 0 && jobs.Add(BuildJob.AsylumFoodSupply(
                    instance.FoodStation, source.Value, instance.RoomId, amount)))
                instance.FoodSupplyPending = true;
        }
    }

    public int CompleteSupply(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var instance)) return amount;
        var accepted = Math.Min(amount, FoodMaximum - instance.Food);
        instance.Food += accepted;
        instance.FoodSupplyPending = false;
        return amount - accepted;
    }

    public DetainedSubjectRuntime? TryAdmit(int? preferredRoomId = null)
        => TryAdmitCore(null, preferredRoomId);

    /// <summary>Reserves an asylum cell for an existing Deranged citizen.</summary>
    public DetainedSubjectRuntime? TryAdmitCitizen(int citizenId, int? preferredRoomId = null)
    {
        if (citizenId <= 0 || Find(citizenId) is not null) return null;
        return TryAdmitCore(citizenId, preferredRoomId);
    }

    private DetainedSubjectRuntime? TryAdmitCore(int? citizenId, int? preferredRoomId)
    {
        var candidates = preferredRoomId is int roomId && _instances.TryGetValue(roomId, out var preferred)
            ? new[] { preferred }.Concat(_instances.Values.Where(value => value.RoomId != roomId))
            : _instances.Values;
        foreach (var instance in candidates)
        {
            var cell = instance.Cells.FirstOrDefault(value => !value.Reserved);
            if (cell is null) continue;
            cell.Reserved = true;
            var subject = new DetainedSubjectRuntime(citizenId ?? NextGeneratedSubjectId(),
                instance.RoomId, cell.Index);
            instance.Prisoners.Add(subject);
            return subject;
        }
        return null;
    }

    public DetainedSubjectRuntime? Find(int subjectId) => _instances.Values
        .SelectMany(instance => instance.Prisoners).FirstOrDefault(value => value.Id == subjectId);

    private int NextGeneratedSubjectId()
    {
        while (Find(_nextSubjectId) is not null) _nextSubjectId++;
        return _nextSubjectId++;
    }

    public bool Release(int subjectId)
    {
        foreach (var instance in _instances.Values)
        {
            var subject = instance.Prisoners.FirstOrDefault(value => value.Id == subjectId);
            if (subject is null) continue;
            instance.Prisoners.Remove(subject);
            var cell = instance.Cells.FirstOrDefault(value => value.Index == subject.CellIndex);
            if (cell is not null) cell.Reserved = false;
            return true;
        }
        return false;
    }

    public bool TryFeed(int subjectId)
    {
        var instance = _instances.Values.FirstOrDefault(value => value.Prisoners.Any(p => p.Id == subjectId));
        if (instance is null || instance.Food <= 0) return false;
        instance.Food--;
        return true;
    }

    public double TreatmentFactor(int roomId)
    {
        if (!_instances.TryGetValue(roomId, out var instance) || instance.GuardsMaximum <= 0) return 0.25;
        var guards = (double)instance.GuardsEmployed / instance.GuardsMaximum;
        return Math.Clamp(0.25 + 0.75 * (1.0 - instance.Degradation) * guards, 0.0, 1.0);
    }

    public void Tick(double delta) { }
}

public sealed class AsylumInstanceRuntime
{
    public int RoomId { get; init; }
    public GridCoord FoodStation { get; set; }
    public List<AsylumCellRuntime> Cells { get; } = new();
    public List<DetainedSubjectRuntime> Prisoners { get; } = new();
    public int Food { get; set; }
    public bool FoodSupplyPending { get; set; }
    public int GuardsMaximum { get; set; }
    public int GuardsEmployed { get; set; }
    public double Degradation { get; set; }
}

public sealed class AsylumCellRuntime
{
    public int Index { get; init; }
    public bool Reserved { get; set; }
}

public sealed record DetainedSubjectRuntime(int Id, int RoomId, int CellIndex);
