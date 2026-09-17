using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class BathInstanceRuntime
{
    public int RoomId { get; init; }
    public GridCoord Cell { get; set; }
    public double Heat { get; set; }
    public double CoalPerServiceDay { get; set; }
    public bool PumpPending { get; set; }
    public bool FuelPending { get; set; }
}

/// <summary>Shared pool, crank and oven behavior for every bath definition.</summary>
public sealed class BathRuntime
{
    private readonly Dictionary<int, BathInstanceRuntime> _instances = new();

    public void Synchronize(
        IEnumerable<RoomRecord> rooms,
        Func<int, GridCoord> fromIndex,
        RoomServiceRuntime services)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     room.DefinitionKey.StartsWith("BATH_", StringComparison.OrdinalIgnoreCase)))
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == room.Id);
            var rule = OriginalGameData.Current.Room(room.DefinitionKey);
            if (service is null || rule is null) continue;
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var bath))
            {
                bath = new BathInstanceRuntime { RoomId = room.Id };
                _instances[room.Id] = bath;
                for (var capacity = 0; capacity < service.Total; capacity++)
                    service.SuspendAvailable();
            }
            bath.Cell = fromIndex(room.Cells.First());
            bath.CoalPerServiceDay = rule.Recipes.FirstOrDefault()?.Inputs
                .FirstOrDefault(input => input.Resource.Equals(
                    "COAL", StringComparison.OrdinalIgnoreCase))?.Rate ?? 0;
        }
        foreach (var id in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(id);
    }

    public void RecordUse(RoomServiceInstanceRuntime service)
    {
        if (_instances.ContainsKey(service.RoomId)) service.SuspendAvailable();
    }

    public void Tick(double delta, int secondsPerDay, RoomServiceRuntime services)
    {
        foreach (var bath in _instances.Values)
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == bath.RoomId);
            if (service is null) continue;
            bath.Heat = Math.Max(0, bath.Heat - delta * service.Total *
                bath.CoalPerServiceDay / Math.Max(1, secondsPerDay));
        }
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var bath in _instances.Values)
        {
            bath.PumpPending = false;
            bath.FuelPending = false;
        }
        foreach (var job in jobs.Where(job => job.State is not (JobState.Completed or JobState.Cancelled)))
        {
            if (!_instances.TryGetValue(job.RoomId, out var bath)) continue;
            if (job.Kind == BuildKind.BathPump) bath.PumpPending = true;
            if (job.Kind == BuildKind.BathFuel) bath.FuelPending = true;
        }
    }

    public void Schedule(
        JobBoard jobs,
        ResourceLedger resources,
        GridCoord? source,
        RoomServiceRuntime services)
    {
        foreach (var bath in _instances.Values)
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == bath.RoomId);
            if (service is null) continue;
            if (service.Unavailable > 0 && !bath.PumpPending &&
                jobs.Add(BuildJob.BathPump(bath.Cell, bath.RoomId))) bath.PumpPending = true;
            if (source is null || bath.FuelPending || bath.Heat >= service.Total * 2) continue;
            var target = Math.Max(1, (int)Math.Ceiling(service.Total * 2 - bath.Heat));
            var amount = Math.Min(BuildJob.MaximumFetchAmount,
                Math.Min(target, resources.Get(ResourceKind.Coal)));
            if (amount > 0 && jobs.Add(BuildJob.BathFuel(
                    bath.Cell, source.Value, bath.RoomId, amount))) bath.FuelPending = true;
        }
    }

    public void CompletePump(BuildJob job, RoomServiceRuntime services)
    {
        if (!_instances.TryGetValue(job.RoomId, out var bath)) return;
        services.Instances.FirstOrDefault(service => service.RoomId == job.RoomId)?.RestoreAvailable();
        bath.PumpPending = false;
    }

    public int CompleteFuel(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var bath)) return amount;
        bath.Heat += amount;
        bath.FuelPending = false;
        return 0;
    }

    public double Quality(int roomId, int total) => _instances.TryGetValue(roomId, out var bath)
        ? Math.Clamp(bath.Heat / Math.Max(1, total), 0, 1)
        : 0;
}
