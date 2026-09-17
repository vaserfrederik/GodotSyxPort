using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Hauling;

public readonly record struct LooseResource(
    GridCoord Cell, ResourceKind Kind, int Amount, bool AlreadyAccounted = false);

public sealed class HaulingSystem
{
    private readonly GridWorld _world;
    private readonly RoomSystem _rooms;
    private readonly Dictionary<(GridCoord Cell, ResourceKind Kind), LooseResource> _loose = new();
    private readonly Dictionary<(GridCoord Cell, ResourceKind Kind), int> _looseReservations = new();
    private readonly Dictionary<ResourceKind, int> _accountedInTransit = new();

    public int LooseCount => _loose.Count;
    public int StoredUnits => _rooms.Logistics.StockpileStored;
    public int ReservedStorageUnits => _rooms.Logistics.StockpileReservedSpace;

    public HaulingSystem(GridWorld world, RoomSystem rooms)
    {
        _world = world;
        _rooms = rooms;
        _rooms.Logistics.ExcludedFromStockpile = UnstoredAccounted;
    }

    public void Spawn(GridCoord cell, ResourceKind kind, int amount, JobBoard jobs)
        => Spawn(cell, kind, amount, jobs, false);

    public void SpawnAccounted(GridCoord cell, ResourceKind kind, int amount, JobBoard jobs)
        => Spawn(cell, kind, amount, jobs, true);

    private void Spawn(
        GridCoord cell, ResourceKind kind, int amount, JobBoard jobs, bool alreadyAccounted)
    {
        var key = (cell, kind);
        if (_loose.TryGetValue(key, out var existing))
            _loose[key] = existing with { Amount = existing.Amount + amount };
        else
            _loose[key] = new LooseResource(cell, kind, amount, alreadyAccounted);
        TryCreateJob(cell, kind, jobs);
    }

    public int TakeAccounted(GridCoord cell, ResourceKind kind, int amount)
    {
        var key = (cell, kind);
        if (!_loose.TryGetValue(key, out var loose) || !loose.AlreadyAccounted) return 0;
        var taken = System.Math.Min(System.Math.Max(0, amount), loose.Amount);
        if (taken == loose.Amount) _loose.Remove(key);
        else _loose[key] = loose with { Amount = loose.Amount - taken };
        return taken;
    }

    public bool HasAccounted(GridCoord cell, ResourceKind kind) =>
        _loose.TryGetValue((cell, kind), out var loose) && loose.AlreadyAccounted && loose.Amount > 0;

    public bool Pickup(BuildJob job)
    {
        if (job.PickedUp) return true;
        if (job.Kind == BuildKind.LogisticsTransfer)
        {
            if (!_rooms.Logistics.Pickup(job)) return false;
            job.PickedUp = true;
            return true;
        }
        if (job.Kind == BuildKind.RoomOutputHaul)
        {
            var slot = _rooms.InternalStorage.At(job.RoomId, job.Cell);
            if (slot is null) return false;
            var picked = slot.Pickup(job.OutputAmount);
            if (picked != job.OutputAmount)
            {
                if (picked > 0) slot.DepositUnreserved(job.OutputResource, picked);
                return false;
            }
            job.PickedUp = true;
            return true;
        }
        var key = (job.Cell, job.OutputResource);
        if (!_loose.TryGetValue(key, out var loose) ||
            loose.Amount - ReservedAt(job.Cell, job.OutputResource) < job.OutputAmount) return false;
        var remaining = loose.Amount - job.OutputAmount;
        if (remaining == 0) _loose.Remove(key);
        else _loose[key] = loose with { Amount = remaining };
        if (loose.AlreadyAccounted)
            _accountedInTransit[job.OutputResource] =
                _accountedInTransit.GetValueOrDefault(job.OutputResource) + job.OutputAmount;
        job.PickedUp = true;
        return true;
    }

    public void Deliver(BuildJob job, ResourceLedger resources, JobBoard jobs)
    {
        if (job.Kind == BuildKind.LogisticsTransfer)
        {
            _rooms.Logistics.Deliver(job);
            RetryUnassigned(jobs);
            return;
        }
        if (job.Kind is BuildKind.Haul or BuildKind.RoomOutputHaul &&
            !_rooms.Logistics.DepositStockpileHaul(job))
        {
            CancelStorageReservation(job);
            if (job.OutputAlreadyAccounted)
                SpawnAccounted(job.Destination, job.OutputResource, job.OutputAmount, jobs);
            else
                Spawn(job.Destination, job.OutputResource, job.OutputAmount, jobs);
            RetryUnassigned(jobs);
            return;
        }
        if (!job.OutputAlreadyAccounted)
            resources.Add(job.OutputResource, job.OutputAmount);
        else if (job.Kind == BuildKind.Haul)
            CompleteAccountedTransit(job.OutputResource, job.OutputAmount);
        if (job.Kind == BuildKind.RoomOutputHaul) ScheduleRoomExports(job.RoomId, jobs);
        RetryUnassigned(jobs);
    }

    public void RetryUnassigned(JobBoard jobs)
    {
        foreach (var key in _loose.Keys.ToArray()) TryCreateJob(key.Cell, key.Kind, jobs);
        foreach (var roomId in _rooms.InternalStorage.AllSlots.Select(item => item.RoomId).Distinct())
            if (_rooms.CanExportInternalStorage(roomId)) ScheduleRoomExports(roomId, jobs);
    }

    public LooseResource[] Capture() => _loose.Values.ToArray();

    public void Restore(IEnumerable<LooseResource> items)
    {
        foreach (var item in items) _loose[(item.Cell, item.Kind)] = item;
    }

    public void RestoreAccountedInTransit(IEnumerable<BuildJob> jobs)
    {
        _accountedInTransit.Clear();
        foreach (var job in jobs.Where(job => job.Kind == BuildKind.Haul && job.PickedUp &&
                                              job.OutputAlreadyAccounted))
            _accountedInTransit[job.OutputResource] =
                _accountedInTransit.GetValueOrDefault(job.OutputResource) + job.OutputAmount;
    }

    public void CancelStorageReservation(BuildJob job)
    {
        if (job.Kind == BuildKind.ProductionSupply)
        {
            _rooms.CancelProductionSupplyReservation(job);
            return;
        }
        if (job.Kind == BuildKind.LogisticsTransfer)
        {
            _rooms.Logistics.Cancel(job);
            return;
        }
        if (job.Kind == BuildKind.RoomOutputHaul && !job.PickedUp)
            _rooms.InternalStorage.At(job.RoomId, job.Cell)?.CancelPickup(job.OutputAmount);
        if (job.Kind is BuildKind.Haul or BuildKind.RoomOutputHaul)
            _rooms.Logistics.CancelStockpileSpace(job);
        if (job.Kind == BuildKind.Haul && job.PickedUp && job.OutputAlreadyAccounted)
            CompleteAccountedTransit(job.OutputResource, job.OutputAmount);
    }

    public void ScheduleRoomExports(int roomId, JobBoard jobs)
    {
        if (!_rooms.CanExportInternalStorage(roomId)) return;
        foreach (var slot in _rooms.InternalStorage.ForRoom(roomId))
        {
            if (slot.Resource is null || slot.ReservableResource <= 0) continue;
            var maximum = System.Math.Min(slot.ReservableResource, BuildJob.MaximumFetchAmount);
            if (!slot.ReservePickup(maximum)) continue;
            var destination = _rooms.Logistics.ReserveStockpileSpace(
                slot.Resource.Value, slot.Cell, maximum, roomId);
            if (destination is null || destination.Value.Cell == slot.Cell)
            {
                slot.CancelPickup(maximum);
                continue;
            }
            var amount = destination.Value.Amount;
            if (amount < maximum) slot.CancelPickup(maximum - amount);
            var job = BuildJob.RoomOutputHaul(
                slot.Cell, destination.Value.Cell, roomId, destination.Value.RoomId,
                slot.Resource.Value, amount);
            if (!jobs.Add(job))
            {
                slot.CancelPickup(amount);
                _rooms.Logistics.CancelStockpileSpace(job);
            }
        }
    }

    public GridCoord? FindReservable(ResourceKind kind, GridCoord origin)
    {
        GridCoord? best = null;
        var bestDistance = int.MaxValue;
        foreach (var item in _loose.Values)
        {
            if (item.Kind != kind || item.Amount - ReservedAt(item.Cell, item.Kind) <= 0) continue;
            var distance = System.Math.Abs(item.Cell.X - origin.X) +
                           System.Math.Abs(item.Cell.Z - origin.Z);
            if (distance >= bestDistance) continue;
            best = item.Cell;
            bestDistance = distance;
        }
        return best;
    }

    public GridCoord? FindAccounted(ResourceKind kind, GridCoord origin)
    {
        GridCoord? best = null;
        var bestDistance = int.MaxValue;
        foreach (var item in _loose.Values)
        {
            if (item.Kind != kind || !item.AlreadyAccounted || item.Amount <= 0) continue;
            var distance = System.Math.Abs(item.Cell.X - origin.X) +
                           System.Math.Abs(item.Cell.Z - origin.Z);
            if (distance >= bestDistance) continue;
            best = item.Cell;
            bestDistance = distance;
        }
        return best;
    }

    public bool ReserveLoose(GridCoord cell, ResourceKind kind)
    {
        var key = (cell, kind);
        if (!_loose.TryGetValue(key, out var item) ||
            item.Amount - ReservedAt(cell, kind) <= 0) return false;
        _looseReservations[key] = ReservedAt(cell, kind) + 1;
        return true;
    }

    public bool HasUnreservedAt(GridCoord cell) => _loose.Any(pair =>
        pair.Key.Cell == cell && pair.Value.Amount - ReservedAt(cell, pair.Key.Kind) > 0);

    public bool RemoveOneUnreservedAt(GridCoord cell, out ResourceKind kind)
    {
        foreach (var pair in _loose.ToArray())
        {
            if (pair.Key.Cell != cell || pair.Value.Amount - ReservedAt(cell, pair.Key.Kind) <= 0) continue;
            kind = pair.Key.Kind;
            if (pair.Value.Amount == 1) _loose.Remove(pair.Key);
            else _loose[pair.Key] = pair.Value with { Amount = pair.Value.Amount - 1 };
            return true;
        }
        kind = default;
        return false;
    }

    public bool ConsumeReservedLoose(GridCoord cell, ResourceKind kind)
    {
        var key = (cell, kind);
        if (ReservedAt(cell, kind) <= 0) return false;
        if (!_loose.TryGetValue(key, out var item) || item.Amount <= 0)
        {
            ReleaseLooseReservation(cell, kind);
            return false;
        }
        ReleaseLooseReservation(cell, kind);
        if (item.Amount == 1) _loose.Remove(key);
        else _loose[key] = item with { Amount = item.Amount - 1 };
        return true;
    }

    public void ReleaseLooseReservation(GridCoord cell, ResourceKind kind)
    {
        var key = (cell, kind);
        var reserved = ReservedAt(cell, kind);
        if (reserved <= 1) _looseReservations.Remove(key);
        else _looseReservations[key] = reserved - 1;
    }

    private int ReservedAt(GridCoord cell, ResourceKind kind) =>
        _looseReservations.GetValueOrDefault((cell, kind));

    private void TryCreateJob(GridCoord source, ResourceKind kind, JobBoard jobs)
    {
        if (!_loose.TryGetValue((source, kind), out var loose)) return;
        var available = loose.Amount - ReservedAt(source, kind);
        if (available <= 0) return;
        var maximum = System.Math.Min(available, BuildJob.MaximumFetchAmount);
        var destination = _rooms.Logistics.ReserveStockpileSpace(
            kind, source, maximum, 0, requireFetching: true);
        if (destination is null) return;
        var job = BuildJob.Haul(source, destination.Value.Cell, destination.Value.RoomId,
            loose.Kind, destination.Value.Amount, loose.AlreadyAccounted);
        if (!jobs.Add(job)) _rooms.Logistics.CancelStockpileSpace(job);
    }

    private int UnstoredAccounted(ResourceKind resource) =>
        _loose.Values.Where(item => item.Kind == resource && item.AlreadyAccounted).Sum(item => item.Amount) +
        _accountedInTransit.GetValueOrDefault(resource);

    private void CompleteAccountedTransit(ResourceKind resource, int amount)
    {
        var remaining = _accountedInTransit.GetValueOrDefault(resource) - System.Math.Max(0, amount);
        if (remaining <= 0) _accountedInTransit.Remove(resource);
        else _accountedInTransit[resource] = remaining;
    }
}
