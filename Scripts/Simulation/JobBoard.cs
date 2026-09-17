using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;
using GodotSyxPort.Core;
using GodotSyxPort.Navigation;

namespace GodotSyxPort.Simulation;

/// <summary>Central registry with O(1) reservable-job assignment.</summary>
public sealed class JobBoard
{
    private readonly Dictionary<(GridCoord Region, JobPriority Priority), Queue<BuildJob>>
        _reservableByRegion = new();
    private readonly List<BuildJob> _all = new();
    private readonly Dictionary<GridCoord, BuildJob> _at = new();

    public int Count => _all.Count;
    public Action<BuildJob>? PrepareConstruction { get; set; }
    public Action<BuildJob>? JobCancelled { get; set; }
    public IReadOnlyList<BuildJob> All => _all;
    public BuildJob? GetAt(GridCoord cell) => _at.GetValueOrDefault(cell);

    public bool Add(BuildJob job)
    {
        var occupied = job.OccupiedCells.ToArray();
        if (occupied.Any(_at.ContainsKey)) return false;
        if (job.IsConstruction && !job.ConstructionPreparationInitialized)
            PrepareConstruction?.Invoke(job);
        _all.Add(job);
        foreach (var cell in occupied) _at[cell] = job;
        if (job.State == JobState.Reservable) Enqueue(job);
        return true;
    }

    public int ActivateRoomConstruction(int roomId)
    {
        var activated = 0;
        var floorJobs = _all.Where(job => job.RoomId == roomId &&
            job.Kind == BuildKind.RoomFloor &&
            job.State is not (JobState.Completed or JobState.Cancelled)).ToArray();
        foreach (var job in floorJobs.Where(job => job.State == JobState.Dormant))
        {
            PrepareConstruction?.Invoke(job);
            job.State = JobState.Reservable;
            Enqueue(job);
            activated++;
        }
        if (floorJobs.Length > 0) return activated;
        foreach (var job in _all.Where(job => job.RoomId == roomId &&
                     job.Kind == BuildKind.Furniture && job.State == JobState.Dormant))
        {
            PrepareConstruction?.Invoke(job);
            job.State = JobState.Reservable;
            Enqueue(job);
            activated++;
        }
        return activated;
    }

    public int ActivateRoomRoofs(int roomId)
    {
        var activated = 0;
        foreach (var job in _all.Where(job => job.RoomId == roomId &&
                     job.Kind == BuildKind.RoomRoof && job.State == JobState.Dormant))
        {
            PrepareConstruction?.Invoke(job);
            job.State = JobState.Reservable;
            Enqueue(job);
            activated++;
        }
        return activated;
    }

    public bool HasPendingRoomRoofs(int roomId) => _all.Any(job =>
        job.RoomId == roomId && job.Kind == BuildKind.RoomRoof &&
        job.State is not (JobState.Completed or JobState.Cancelled));

    public bool HasPendingRoomClears(int roomId) => _all.Any(job =>
        job.RoomId == roomId && job.Kind == BuildKind.RoomClear &&
        job.State is not (JobState.Completed or JobState.Cancelled));

    public bool HasPendingRoomConstruction(int roomId) => _all.Any(job =>
        job.RoomId == roomId && job.IsConstruction &&
        job.State is not (JobState.Completed or JobState.Cancelled));

    public int ActivateRoomWalls(int roomId)
    {
        var activated = 0;
        foreach (var job in _all.Where(job => job.RoomId == roomId &&
                     (job.Kind is BuildKind.Wall or BuildKind.RoomDoor) &&
                     job.State == JobState.Dormant))
        {
            PrepareConstruction?.Invoke(job);
            job.State = JobState.Reservable;
            Enqueue(job);
            activated++;
        }
        return activated;
    }

    public BuildJob? TryClaim(
        GridCoord worker,
        ResourceLedger resources,
        Func<BuildJob, bool>? accepts = null)
    {
        var center = Region(worker);
        var maximumRegionRadius =
            (BuildJob.MaximumFetchDistance + HierarchicalGridPathfinder.RegionSize - 1) /
            HierarchicalGridPathfinder.RegionSize;
        for (var radius = 0; radius <= maximumRegionRadius; radius++)
        {
            for (var dz = -radius; dz <= radius; dz++)
            {
                for (var dx = -radius; dx <= radius; dx++)
                {
                    if (radius > 0 && System.Math.Abs(dx) != radius && System.Math.Abs(dz) != radius) continue;
                    var job = TryClaimRegion(
                        new GridCoord(center.X + dx, center.Z + dz), worker, resources, accepts);
                    if (job is not null) return job;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Source-faithful local continuation order from PlanOddjobber.Multi:
    /// C, N, W, S, E, SW, SE. The completed center job is skipped, so a
    /// laborer continues an adjacent compatible material job before scanning
    /// the global regional queues again.
    /// </summary>
    public BuildJob? TryClaimAdjacentCompatible(BuildJob completed, ResourceLedger resources)
    {
        if (!completed.IsConstruction) return null;
        var offsets = new[]
        {
            new GridCoord(0, 0),
            new GridCoord(0, -1),
            new GridCoord(-1, 0),
            new GridCoord(0, 1),
            new GridCoord(1, 0),
            new GridCoord(-1, 1),
            new GridCoord(1, 1)
        };
        foreach (var offset in offsets)
        {
            if (!_at.TryGetValue(completed.Cell + offset, out var candidate) ||
                ReferenceEquals(candidate, completed) ||
                candidate.State != JobState.Reservable ||
                !candidate.IsConstruction ||
                candidate.Resource != completed.Resource) continue;
            if (!resources.TryReserve(candidate))
            {
                candidate.State = JobState.Blocked;
                continue;
            }
            candidate.State = JobState.Reserved;
            return candidate;
        }
        return null;
    }

    public IReadOnlyList<BuildJob> ClaimAdjacentMaterialBatch(
        BuildJob primary,
        ResourceLedger resources,
        int remainingCapacity)
    {
        var claimed = new List<BuildJob>();
        if (!primary.IsConstruction || remainingCapacity <= 0) return claimed;
        var offsets = new[]
        {
            new GridCoord(0, -1), new GridCoord(-1, 0), new GridCoord(0, 1),
            new GridCoord(1, 0), new GridCoord(-1, 1), new GridCoord(1, 1)
        };
        foreach (var offset in offsets)
        {
            if (!_at.TryGetValue(primary.Cell + offset, out var candidate) ||
                candidate.State != JobState.Reservable ||
                !candidate.IsConstruction ||
                candidate.Resource != primary.Resource ||
                candidate.MaterialsNeeded <= 0 ||
                candidate.MaterialsNeeded > remainingCapacity) continue;
            if (!resources.TryReserve(candidate, remainingCapacity)) continue;
            candidate.State = JobState.Reserved;
            claimed.Add(candidate);
            remainingCapacity -= candidate.ReservedAmount;
            if (remainingCapacity <= 0) break;
        }
        return claimed;
    }

    public void Release(BuildJob job, ResourceLedger resources)
    {
        if (job.State != JobState.Reserved) return;
        resources.Refund(job);
        job.State = JobState.Reservable;
        Enqueue(job);
    }

    public bool Cancel(GridCoord cell, ResourceLedger resources)
    {
        if (!_at.TryGetValue(cell, out var job)) return false;
        if (job.State is JobState.Completed or JobState.Cancelled) return false;
        resources.Refund(job);
        job.ReleaseOutputReservations();
        JobCancelled?.Invoke(job);
        job.State = JobState.Cancelled;
        return true;
    }

    public int CancelRoom(int roomId, ResourceLedger resources)
    {
        var cancelled = 0;
        foreach (var job in _all.Where(job => job.RoomId == roomId || job.DestinationRoomId == roomId)
                     .Where(job => job.State is not (JobState.Completed or JobState.Cancelled)).ToArray())
        {
            resources.Refund(job);
            job.ReleaseOutputReservations();
            JobCancelled?.Invoke(job);
            job.State = JobState.Cancelled;
            cancelled++;
        }
        return cancelled;
    }

    public void RemoveCompleted()
    {
        foreach (var job in _all)
            if (job.State is JobState.Completed or JobState.Cancelled)
                foreach (var cell in job.OccupiedCells)
                    _at.Remove(cell);
        _all.RemoveAll(job => job.State is JobState.Completed or JobState.Cancelled);
    }

    public void RetryBlocked()
    {
        foreach (var job in _all)
        {
            if (job.State != JobState.Blocked) continue;
            job.State = JobState.Reservable;
            Enqueue(job);
        }
    }

    public int TrimProductionForRoom(int roomId, int keep, ResourceLedger resources)
    {
        var retained = 0;
        foreach (var job in _all)
        {
            if (job.Kind is not (BuildKind.Production or BuildKind.ProductionSupply) || job.RoomId != roomId ||
                job.State is JobState.Completed or JobState.Cancelled) continue;
            if (retained < keep)
            {
                retained++;
                continue;
            }
            resources.Refund(job);
            job.ReleaseOutputReservations();
            JobCancelled?.Invoke(job);
            job.State = JobState.Cancelled;
        }
        return retained;
    }

    private BuildJob? TryClaimRegion(
        GridCoord region,
        GridCoord worker,
        ResourceLedger resources,
        Func<BuildJob, bool>? accepts)
    {
        for (var value = (int)JobPriority.Production; value >= (int)JobPriority.Road; value--)
        {
            var key = (region, (JobPriority)value);
            if (!_reservableByRegion.TryGetValue(key, out var queue)) continue;
            var candidates = queue.Count;
            while (candidates-- > 0)
            {
                var job = queue.Dequeue();
                if (job.State != JobState.Reservable) continue;
                var fetchDistance = System.Math.Abs(job.Cell.X - worker.X) +
                                    System.Math.Abs(job.Cell.Z - worker.Z);
                if (fetchDistance > BuildJob.MaximumFetchDistance)
                {
                    queue.Enqueue(job);
                    continue;
                }
                if (accepts is not null && !accepts(job))
                {
                    queue.Enqueue(job);
                    continue;
                }
                if (job.Kind != BuildKind.ProductionSupply && !resources.TryReserve(job))
                {
                    job.State = JobState.Blocked;
                    continue;
                }
                job.State = JobState.Reserved;
                return job;
            }
        }
        return null;
    }

    private void Enqueue(BuildJob job)
    {
        var key = (Region(job.Cell), job.Priority);
        if (!_reservableByRegion.TryGetValue(key, out var queue))
        {
            queue = new Queue<BuildJob>();
            _reservableByRegion[key] = queue;
        }
        queue.Enqueue(job);
    }

    private static GridCoord Region(GridCoord cell) =>
        new(cell.X / HierarchicalGridPathfinder.RegionSize, cell.Z / HierarchicalGridPathfinder.RegionSize);
}
