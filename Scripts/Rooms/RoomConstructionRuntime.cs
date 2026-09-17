using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class ResourceUnderflowRuntime
{
    private readonly Dictionary<ResourceKind, int> _underflow = new();
    public int Get(ResourceKind resource) => _underflow.GetValueOrDefault(resource);

    public int Withdraw(ResourceKind resource, int target, int maximum)
    {
        if (target <= maximum) return target;
        _underflow[resource] = Get(resource) + target - maximum;
        return maximum;
    }

    public int Deposit(ResourceKind resource, int amount)
    {
        var accepted = Math.Min(Math.Max(0, amount), Get(resource));
        _underflow[resource] = Get(resource) - accepted;
        return amount - accepted;
    }

    public void Clear() => _underflow.Clear();
}

public sealed class RoomConstructionOrder
{
    private readonly Dictionary<ResourceKind, int> _required;
    private readonly Dictionary<ResourceKind, int> _delivered = new();
    public int RoomId { get; }
    public IReadOnlyDictionary<ResourceKind, int> Required => _required;
    public IReadOnlyDictionary<ResourceKind, int> Delivered => _delivered;
    public bool SupplyPending { get; set; }
    public bool Complete => _required.All(pair => Underflow(pair.Key) == 0);

    public RoomConstructionOrder(int roomId, IReadOnlyDictionary<ResourceKind, int> required)
    {
        RoomId = roomId;
        _required = required.Where(pair => pair.Value > 0)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public int Underflow(ResourceKind resource) => Math.Max(
        0, _required.GetValueOrDefault(resource) - _delivered.GetValueOrDefault(resource));

    public KeyValuePair<ResourceKind, int>? NextUnderflow(int maximum)
    {
        foreach (var resource in _required.Keys.OrderBy(kind => (int)kind))
        {
            var amount = Math.Min(maximum, Underflow(resource));
            if (amount > 0) return new KeyValuePair<ResourceKind, int>(resource, amount);
        }
        return null;
    }

    public int Deliver(ResourceKind resource, int amount)
    {
        var accepted = Math.Min(Math.Max(0, amount), Underflow(resource));
        _delivered[resource] = _delivered.GetValueOrDefault(resource) + accepted;
        SupplyPending = false;
        return amount - accepted;
    }
}

/// <summary>Multi-resource ConstructionBlueprint/ConstructionInstance adapter.</summary>
public sealed class RoomConstructionRuntime
{
    private readonly Dictionary<int, RoomConstructionOrder> _orders = new();
    public IReadOnlyDictionary<int, RoomConstructionOrder> Orders => _orders;

    public RoomConstructionOrder Begin(
        RoomInstanceRuntime room,
        IReadOnlyDictionary<int, double> itemGroupAmounts)
    {
        var order = new RoomConstructionOrder(room.Record.Id, room.ConstructionCost(itemGroupAmounts));
        _orders[room.Record.Id] = order;
        return order;
    }

    public RoomConstructionOrder Restore(
        RoomInstanceRuntime room,
        IReadOnlyDictionary<ResourceKind, int> required,
        IReadOnlyDictionary<ResourceKind, int> delivered)
    {
        var order = new RoomConstructionOrder(room.Record.Id, required);
        foreach (var pair in delivered)
            order.Deliver(pair.Key, pair.Value);
        _orders[room.Record.Id] = order;
        return order;
    }

    public void Schedule(RoomSystem rooms, JobBoard jobs, ResourceLedger resources)
    {
        foreach (var order in _orders.Values)
        {
            var room = rooms.RuntimeInstance(order.RoomId);
            if (room is null || jobs.HasPendingRoomClears(order.RoomId)) continue;
            jobs.ActivateRoomWalls(order.RoomId);
            if (rooms.CompletedWallCount(room.Record) < room.Record.RequiredWalls.Count ||
                rooms.CompletedDoorCount(room.Record) < room.Record.RequiredDoors.Count) continue;
            jobs.ActivateRoomRoofs(order.RoomId);
            if (jobs.HasPendingRoomRoofs(order.RoomId)) continue;
            if (order.Complete)
            {
                jobs.ActivateRoomConstruction(order.RoomId);
                continue;
            }
            if (order.SupplyPending) continue;
            var underflow = order.NextUnderflow(BuildJob.MaximumFetchAmount);
            if (underflow is null) continue;
            // ConstructionInstance.setFetch follows clearing and structure/roof work.
            // Do not deliver the room's area/item resources through unfinished walls.
            var source = rooms.FindSupplyCell(underflow.Value.Key);
            if (source is null) continue;
            var delivery = rooms.ConstructionDeliveryCell(room.Record, jobs);
            if (delivery is null) continue;
            var amount = Math.Min(underflow.Value.Value, resources.Get(underflow.Value.Key));
            if (amount <= 0) continue;
            var job = BuildJob.RoomConstructionSupply(
                delivery.Value, source.Value, room.Record.Id, underflow.Value.Key, amount);
            if (jobs.Add(job)) order.SupplyPending = true;
        }
    }

    public int Deliver(BuildJob job, int amount)
    {
        if (!_orders.TryGetValue(job.RoomId, out var order)) return amount;
        return order.Deliver(job.Resource, amount);
    }

    public bool Remove(int roomId) => _orders.Remove(roomId);

    public void CancelSupply(BuildJob job)
    {
        if (job.Kind == BuildKind.RoomConstructionSupply &&
            _orders.TryGetValue(job.RoomId, out var order))
            order.SupplyPending = false;
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var order in _orders.Values) order.SupplyPending = false;
        foreach (var job in jobs)
            if (job.Kind == BuildKind.RoomConstructionSupply &&
                job.State is not (JobState.Completed or JobState.Cancelled) &&
                _orders.TryGetValue(job.RoomId, out var order))
                order.SupplyPending = true;
    }
}
