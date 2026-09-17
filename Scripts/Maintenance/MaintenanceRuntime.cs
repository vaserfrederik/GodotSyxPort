using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Maintenance;

/// <summary>Numerical core adapted from MAINTENANCE, ROOM_DEGRADER and MRoom.</summary>
public static class MaintenanceRuntime
{
    public const double TilesPerDay = 1.0 / 48.0;
    public const double ResourceRate = 1.0 / 64.0;
    public const double MinimumJobs = 4.0;

    public static double RoomRate(
        double boost,
        double baseRate,
        double isolation,
        double resourceAmount,
        int area)
    {
        boost *= 1.0 + (1.0 - Math.Clamp(isolation, 0.0, 1.0)) * 2.0;
        return baseRate * boost * (TilesPerDay * Math.Max(0, area) +
                                   ResourceRate * Math.Max(0.0, resourceAmount));
    }

    public static double VisibleDegradation(double maintenanceDebt) =>
        Math.Clamp(maintenanceDebt / MinimumJobs, 0.0, 1.0);

    public static double ResourceJobChance(double resourceAmount, int area)
    {
        var resourceRate = ResourceRate * Math.Max(0.0, resourceAmount);
        var totalRate = TilesPerDay * Math.Max(0, area) + resourceRate;
        return totalRate <= 0 ? 0 : resourceRate / totalRate;
    }

    public static ResourceKind? SelectResource(
        IReadOnlyDictionary<ResourceKind, int> amounts, int area, Random random)
    {
        var total = amounts.Values.Sum(value => Math.Max(0, value));
        if (total <= 0 || random.NextDouble() >= ResourceJobChance(total, area)) return null;
        var target = random.NextDouble() * total;
        foreach (var pair in amounts.OrderBy(pair => pair.Key))
        {
            target -= Math.Max(0, pair.Value);
            if (target < 0) return pair.Key;
        }
        return null;
    }
}
