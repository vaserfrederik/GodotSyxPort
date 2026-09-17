using System;
using System.Collections.Generic;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

/// <summary>Simulation-facing Furnisher/FurnisherItem/FurnisherItemGroup port.</summary>
public sealed class FurnisherRuntime
{
    public const int MaximumResources = 4;
    public RoomBlueprintRuntime Blueprint { get; }
    public RoomConstructionRule Construction => Blueprint.Rule.Construction;
    public IReadOnlyList<FurnisherItemGroupRule> ItemGroups => Blueprint.Rule.FurnisherItems;

    public FurnisherRuntime(RoomBlueprintRuntime blueprint)
    {
        Blueprint = blueprint;
        if (Construction.Resources.Count > MaximumResources)
            throw new InvalidOperationException($"{blueprint.Key} declares more than four resources");
        if (Construction.AreaCosts.Count != 0 &&
            Construction.AreaCosts.Count != Construction.Resources.Count)
            throw new InvalidOperationException($"{blueprint.Key} AREA_COSTS does not match RESOURCES");
    }

    public string? Floor(int upgrade)
    {
        if (Construction.Floors.Count == 0) return null;
        return Construction.Floors[Math.Clamp(upgrade, 0, Construction.Floors.Count - 1)];
    }

    public double AreaCost(int resourceIndex, int upgrade)
    {
        if ((uint)resourceIndex >= (uint)Construction.Resources.Count) return 0;
        var flat = Construction.AreaCosts.Count == 0 ? 0 : Construction.AreaCosts[resourceIndex];
        return flat * Blueprint.UpgradeResourceMask(upgrade, resourceIndex);
    }

    public double ItemCost(int groupIndex, int resourceIndex, int upgrade, double multiplier = 1.0)
    {
        if ((uint)groupIndex >= (uint)ItemGroups.Count) return 0;
        var costs = ItemGroups[groupIndex].Costs;
        if ((uint)resourceIndex >= (uint)costs.Count) return 0;
        return costs[resourceIndex] * multiplier *
               Blueprint.UpgradeResourceMask(upgrade, resourceIndex);
    }

    public double ItemStat(int groupIndex, int statIndex, double multiplier = 1.0)
    {
        if ((uint)groupIndex >= (uint)ItemGroups.Count) return 0;
        var stats = ItemGroups[groupIndex].Stats;
        return (uint)statIndex < (uint)stats.Count ? stats[statIndex] * multiplier : 0;
    }

    // FurnisherItem: ceil(group.costs[resource] * multiplierCosts / item.area).
    public int BrokenResourceAmount(
        int groupIndex, int resourceIndex, double costMultiplier, int itemArea)
    {
        if (itemArea <= 0 || (uint)groupIndex >= (uint)ItemGroups.Count) return 0;
        var costs = ItemGroups[groupIndex].Costs;
        if ((uint)resourceIndex >= (uint)costs.Count) return 0;
        return checked((int)Math.Ceiling(
            costs[resourceIndex] * costMultiplier / itemArea));
    }

    public IReadOnlyDictionary<ResourceKind, int> ConstructionCost(
        int area,
        IReadOnlyDictionary<int, double> itemGroupAmounts,
        int upgrade)
    {
        var totals = new Dictionary<ResourceKind, int>();
        for (var resourceIndex = 0; resourceIndex < Construction.Resources.Count; resourceIndex++)
        {
            if (!OriginalGameData.TryMapResource(
                    Construction.Resources[resourceIndex], out var resource)) continue;
            var amount = Math.Max(0, area) * AreaCost(resourceIndex, upgrade);
            foreach (var item in itemGroupAmounts)
                amount += Math.Max(0, item.Value) * ItemCost(item.Key, resourceIndex, upgrade);
            if (amount > 0) totals[resource] = checked((int)Math.Ceiling(amount));
        }
        return totals;
    }

    public static double RelativeStat(double value, double denominator, double multiplier = 1.0)
    {
        if (denominator == 0) return value == 0 ? 0 : 1;
        return Math.Clamp(multiplier * value / denominator, 0, 1);
    }

    public static double EfficiencyStat(double value, double workers, double multiplier = 1.0)
    {
        if (workers == 0) return value == 0 ? 0.5 : 1.0;
        return Math.Clamp(0.5 + multiplier * 0.5 * value / workers, 0, 1);
    }
}
