using System;
using System.Collections.Generic;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Maintenance;

/// <summary>Compile-safe MFloor port for source road wear and repair jobs.</summary>
public sealed class RoadMaintenanceSystem
{
    private readonly GridWorld _world;
    private readonly Dictionary<int, double> _wear = new();
    private readonly HashSet<int> _pending = new();
    private int _updateCursor;
    private const int TilesPerTick = 128;

    public RoadMaintenanceSystem(GridWorld world) => _world = world;

    public void Tick(double delta, JobBoard jobs, Func<GridCoord, bool> isRoomCell)
    {
        var roads = _world.RoadCells;
        if (roads.Count == 0) return;
        // MFloor uses a distributed updater. Do the same instead of scanning every
        // generated approach-road tile twenty times per second.
        var updates = Math.Min(TilesPerTick, roads.Count);
        var dayPart = delta / OriginalGameData.Current.SecondsPerDay * roads.Count / updates;
        for (var update = 0; update < updates; update++)
        {
            if (_updateCursor >= roads.Count) _updateCursor = 0;
            var index = roads[_updateCursor++];
            var cell = _world.FromIndex(index);
            if (_world.Data.IsBlocked(cell) || isRoomCell(cell)) continue;
            var road = OriginalGameData.Current.Floor(
                _world.RoadKeys.GetValueOrDefault(index, "DIRT"));
            var rate = (MaintenanceRuntime.TilesPerDay +
                        0.25 * MaintenanceRuntime.ResourceRate * road.ResourceAmount) *
                       (1.0 - road.Durability);
            var wear = _wear.GetValueOrDefault(index) + rate * dayPart;
            if (wear >= 1.0)
            {
                wear -= 1.0;
                var current = _world.Data.RoadDegradation(cell);
                var increase = current > 0 ? 1 + (int)(GD.Randi() % 3) : 1;
                _world.Data.SetRoadDegradation(cell, current + increase);
                if ((GD.Randi() & 1) == 0)
                    _world.Data.GrowVegetation(cell, 1 + (int)(GD.Randi() % 2));
            }
            _wear[index] = wear;
            if (_world.Data.RoadDegradation(cell) <= 0 || _pending.Contains(index)) continue;
            var resourcePart = 0.25 * MaintenanceRuntime.ResourceRate * road.ResourceAmount;
            var total = resourcePart + MaintenanceRuntime.TilesPerDay;
            var resource = default(ResourceKind);
            var requiresResource = road.ResourceAmount > 0 && GD.Randf() * total < resourcePart &&
                                   OriginalGameData.TryMapResource(road.Resource, out resource);
            var job = BuildJob.RoadMaintenance(
                cell,
                requiresResource ? resource : null);
            if (jobs.Add(job)) _pending.Add(index);
        }
    }

    public void Complete(BuildJob job)
    {
        var index = _world.CellToIndex(job.Cell);
        _world.Data.SetRoadDegradation(job.Cell, 0);
        _world.Data.SetVegetation(job.Cell, 0);
        _pending.Remove(index);
    }

    public void Vandalize(GridCoord cell)
    {
        if (!_world.Data.Has(cell, TileFlags.Road)) return;
        _world.Data.SetRoadDegradation(
            cell, _world.Data.RoadDegradation(cell) + 3 + (int)(GD.Randi() % 2));
    }

    public void AccumulateExpectedDailyResourceUse(IDictionary<ResourceKind, double> totals,
        Func<int, bool> isRoomCell)
    {
        foreach (var index in _world.RoadCells)
        {
            if (_world.Data.IsBlocked(_world.FromIndex(index)) || isRoomCell(index)) continue;
            var road = OriginalGameData.Current.Floor(
                _world.RoadKeys.GetValueOrDefault(index, "DIRT"));
            if (road.ResourceAmount <= 0 ||
                !OriginalGameData.TryMapResource(road.Resource, out var resource)) continue;
            var rate = 0.25 * MaintenanceRuntime.ResourceRate *
                       road.ResourceAmount * (1.0 - road.Durability);
            totals.TryGetValue(resource, out var current);
            totals[resource] = current + rate;
        }
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        _pending.Clear();
        foreach (var job in jobs)
            if (job.Kind == BuildKind.Maintenance && job.RoomId == 0 &&
                job.State is not (JobState.Completed or JobState.Cancelled))
                _pending.Add(_world.CellToIndex(job.Cell));
    }
}
