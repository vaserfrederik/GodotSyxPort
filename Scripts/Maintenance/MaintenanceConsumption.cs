using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.Maintenance;

/// <summary>
/// Partial MConsumption estimate: values are expected resource units per game day.
/// The Java source uses dirty 32x32 chunks, disabled tiles and 1/16384-unit rounding;
/// this aggregate has none of those behaviors and supplies the janitor stock target.
/// </summary>
public sealed class MaintenanceConsumption
{
    private readonly RoadMaintenanceSystem _roads;
    private readonly RoomSystem _rooms;
    private readonly Dictionary<ResourceKind, double> _daily = new();
    private double _refreshLeft;

    public MaintenanceConsumption(RoadMaintenanceSystem roads, RoomSystem rooms)
    {
        _roads = roads;
        _rooms = rooms;
        Refresh();
    }

    public void Tick(double delta)
    {
        _refreshLeft -= delta;
        if (_refreshLeft > 0) return;
        Refresh();
        _refreshLeft = 1.0;
    }

    public double EstimateGlobalRaw(ResourceKind resource) =>
        _daily.GetValueOrDefault(resource);

    public double EstimateGlobal(ResourceKind resource, double maintenanceSpeed = 1.0) =>
        EstimateGlobalRaw(resource) * maintenanceSpeed;

    public IReadOnlyDictionary<ResourceKind, double> All => _daily;

    public void Refresh()
    {
        _daily.Clear();
        var roomCells = _rooms.All.SelectMany(room => room.Cells).ToHashSet();
        _roads.AccumulateExpectedDailyResourceUse(_daily, roomCells.Contains);
        _rooms.AccumulateExpectedDailyMaintenanceUse(_daily);
    }
}
