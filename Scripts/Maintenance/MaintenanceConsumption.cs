using System.Collections.Generic;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.Maintenance;

/// <summary>
/// Compile-safe MConsumption port. Values are expected resource units per game day;
/// they describe maintenance demand rather than resources already consumed.
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
        _roads.AccumulateExpectedDailyResourceUse(_daily);
        _rooms.AccumulateExpectedDailyMaintenanceUse(_daily);
    }
}
