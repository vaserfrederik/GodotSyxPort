using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class FoodVenueInstanceRuntime
{
    public int RoomId { get; init; }
    public string RoomKey { get; set; } = "";
    public GridCoord Cell { get; set; }
    public int Stock { get; set; }
    public int Maximum { get; set; }
    public bool SupplyPending { get; set; }
}

/// <summary>Shared stored-food/drink distribution for canteens, eateries and taverns.</summary>
public sealed class FoodVenueRuntime
{
    private readonly Dictionary<int, FoodVenueInstanceRuntime> _instances = new();
    public IReadOnlyCollection<FoodVenueInstanceRuntime> Instances => _instances.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex,
        RoomServiceRuntime services)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational && IsVenue(room.DefinitionKey)))
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == room.Id);
            if (service is null) continue;
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var venue))
            {
                venue = new FoodVenueInstanceRuntime { RoomId = room.Id };
                _instances[room.Id] = venue;
                for (var slot = 0; slot < service.Total; slot++) service.SuspendAvailable();
            }
            venue.RoomKey = room.DefinitionKey;
            venue.Cell = fromIndex(room.Cells.First());
            venue.Maximum = Math.Max(service.Total, service.Total * 2);
        }
        foreach (var id in _instances.Keys.Where(id => !live.Contains(id)).ToArray()) _instances.Remove(id);
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var venue in _instances.Values) venue.SupplyPending = false;
        foreach (var job in jobs.Where(job => job.Kind == BuildKind.VenueSupply &&
                     job.State is not (JobState.Completed or JobState.Cancelled)))
            if (_instances.TryGetValue(job.RoomId, out var venue)) venue.SupplyPending = true;
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source)
    {
        if (source is null) return;
        foreach (var venue in _instances.Values)
        {
            if (venue.SupplyPending || venue.Stock >= venue.Maximum) continue;
            var resource = SourceResource(venue.RoomKey, resources);
            var amount = Math.Min(BuildJob.MaximumFetchAmount,
                Math.Min(venue.Maximum - venue.Stock, resources.Get(resource)));
            if (amount <= 0) continue;
            if (jobs.Add(BuildJob.VenueSupply(venue.Cell, source.Value, venue.RoomId, resource, amount)))
                venue.SupplyPending = true;
        }
    }

    public int CompleteSupply(BuildJob job, int amount, RoomServiceRuntime services)
    {
        if (!_instances.TryGetValue(job.RoomId, out var venue)) return amount;
        var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == job.RoomId);
        var accepted = Math.Min(amount, venue.Maximum - venue.Stock);
        venue.Stock += accepted;
        venue.SupplyPending = false;
        if (service is not null)
            while (service.Unavailable > 0 && venue.Stock > service.Available + service.Reserved)
                service.RestoreAvailable();
        return amount - accepted;
    }

    public bool Consume(RoomServiceInstanceRuntime service)
    {
        if (!_instances.TryGetValue(service.RoomId, out var venue) || venue.Stock <= 0) return false;
        venue.Stock--;
        if (venue.Stock < service.Available + service.Reserved) service.SuspendAvailable();
        return true;
    }

    public bool IsVenue(int roomId) => _instances.ContainsKey(roomId);

    private static bool IsVenue(string key) => key is "CANTEEN_NORMAL" or "EATERY_NORMAL" or "TAVERN_NORMAL";

    private static ResourceKind SourceResource(string roomKey, ResourceLedger resources)
    {
        if (!roomKey.Equals("TAVERN_NORMAL", StringComparison.OrdinalIgnoreCase)) return ResourceKind.Food;
        return resources.Get(ResourceKind.Beer) >= resources.Get(ResourceKind.Wine)
            ? ResourceKind.Beer : ResourceKind.Wine;
    }
}
