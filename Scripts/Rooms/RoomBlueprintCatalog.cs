using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;

namespace GodotSyxPort.Rooms;

public sealed class RoomBlueprintRuntime
{
    public int Index { get; }
    public string Key => Rule.Key;
    public RoomRule Rule { get; }
    public RoomCategoryRuntime Category { get; }
    public FurnisherRuntime Furnisher { get; internal set; } = null!;
    public int Instances { get; internal set; }
    public int TotalArea { get; internal set; }
    public double AverageDegradation { get; internal set; }
    public double AverageUpgrade { get; internal set; }

    internal RoomBlueprintRuntime(int index, RoomRule rule, RoomCategoryRuntime category)
    {
        Index = index;
        Rule = rule;
        Category = category;
    }

    public bool SupportsIndustry => Rule.Recipes.Any(recipe => recipe.Outputs.Count > 0) ||
                                    (!string.IsNullOrWhiteSpace(Rule.Minable) &&
                                     Rule.YieldWorkerDaily > 0);
    public bool SupportsService => Rule.Service is not null;
    public bool SupportsEmployment => Rule.HasWork;
    public int MaximumUpgrade => Math.Max(0, Rule.Upgrades.Count - 1);

    public double UpgradeBoost(int level) =>
        Rule.Upgrades[Math.Clamp(level, 0, MaximumUpgrade)].Boost;

    public double UpgradeResourceMask(int level, int resourceIndex)
    {
        var mask = Rule.Upgrades[Math.Clamp(level, 0, MaximumUpgrade)].ResourceMask;
        if (mask.Count == 0) return 1.0;
        return mask[Math.Clamp(resourceIndex, 0, mask.Count - 1)];
    }

    public double UpgradeCompletion(int level) =>
        (1.0 + Math.Clamp(level, 0, MaximumUpgrade)) / (MaximumUpgrade + 1.0);
}

public sealed class RoomCategoryRuntime
{
    private readonly List<RoomBlueprintRuntime> _rooms = new();
    public RoomArchetype Kind { get; }
    public IReadOnlyList<RoomBlueprintRuntime> Rooms => _rooms;

    internal RoomCategoryRuntime(RoomArchetype kind) => Kind = kind;
    internal void Add(RoomBlueprintRuntime room) => _rooms.Add(room);
}

/// <summary>
/// Shared RoomBlueprint/RoomBlueprintImp/RoomBlueprintIns catalog. Presentation,
/// audio and lockable UI data stay outside the simulation adapter.
/// </summary>
public sealed class RoomBlueprintCatalog
{
    private readonly Dictionary<string, RoomBlueprintRuntime> _byKey =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<RoomArchetype, RoomCategoryRuntime> _categories = new();

    public IReadOnlyCollection<RoomBlueprintRuntime> All => _byKey.Values;
    public IReadOnlyDictionary<RoomArchetype, RoomCategoryRuntime> Categories => _categories;

    public RoomBlueprintCatalog(IEnumerable<RoomRule> rules)
    {
        foreach (var kind in Enum.GetValues<RoomArchetype>())
            _categories[kind] = new RoomCategoryRuntime(kind);
        var index = 0;
        foreach (var rule in rules.OrderBy(rule => rule.Key, StringComparer.OrdinalIgnoreCase))
        {
            var category = _categories[rule.Archetype];
            var blueprint = new RoomBlueprintRuntime(index++, rule, category);
            blueprint.Furnisher = new FurnisherRuntime(blueprint);
            _byKey.Add(rule.Key, blueprint);
            category.Add(blueprint);
        }
    }

    public RoomBlueprintRuntime? Get(string key) => _byKey.GetValueOrDefault(key);

    public IEnumerable<RoomBlueprintRuntime> WithIndustry() => All.Where(room => room.SupportsIndustry);
    public IEnumerable<RoomBlueprintRuntime> WithService() => All.Where(room => room.SupportsService);
    public IEnumerable<RoomBlueprintRuntime> WithEmployment() => All.Where(room => room.SupportsEmployment);

    public void Synchronize(IEnumerable<RoomRecord> instances)
    {
        foreach (var blueprint in All)
        {
            blueprint.Instances = 0;
            blueprint.TotalArea = 0;
            blueprint.AverageDegradation = 0;
            blueprint.AverageUpgrade = 0;
        }
        var degradationTotals = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var upgradeTotals = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var room in instances.Where(room => room.State == RoomState.Operational))
        {
            var blueprint = Get(room.DefinitionKey);
            if (blueprint is null) continue;
            blueprint.Instances++;
            blueprint.TotalArea += room.Cells.Count;
            degradationTotals[blueprint.Key] =
                degradationTotals.GetValueOrDefault(blueprint.Key) + room.Degradation;
            upgradeTotals[blueprint.Key] = upgradeTotals.GetValueOrDefault(blueprint.Key) +
                                           (long)room.UpgradeLevel * room.Cells.Count;
        }
        foreach (var blueprint in All.Where(room => room.Instances > 0))
            blueprint.AverageDegradation = degradationTotals.GetValueOrDefault(blueprint.Key) /
                                           blueprint.Instances;
        foreach (var blueprint in All.Where(room => room.TotalArea > 0))
            blueprint.AverageUpgrade = (double)upgradeTotals.GetValueOrDefault(blueprint.Key) /
                                       blueprint.TotalArea;
    }
}
