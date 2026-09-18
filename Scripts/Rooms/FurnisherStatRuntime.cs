using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rooms;

/// <summary>Source-order FurnisherStat evaluation after raw item-stat accumulation.</summary>
public static class FurnisherStatRuntime
{
    private enum Transform : byte { Relative, Efficiency, EmployeesRelative }
    private readonly record struct Rule(int Index, Transform Kind, int Other, double Multiplier);

    public static double[] Evaluate(RoomRecord room)
        => Evaluate(room.DefinitionKey, room.ItemGroupAmounts);

    public static double[] Evaluate(
        string definitionKey, IReadOnlyDictionary<int, double> itemAmounts)
        => EvaluateSource(definitionKey, itemAmounts, null, null, 0);

    public static double[] EvaluatePlacement(
        string definitionKey,
        IReadOnlyDictionary<int, double> itemAmounts,
        IReadOnlyCollection<GridCoord> area,
        WorldGridData world,
        int existingEmployees = 0)
        => EvaluateSource(definitionKey, itemAmounts, area, world, existingEmployees);

    private static double[] EvaluateSource(
        string definitionKey,
        IReadOnlyDictionary<int, double> itemAmounts,
        IReadOnlyCollection<GridCoord>? area,
        WorldGridData? world,
        int existingEmployees)
    {
        var source = OriginalGameData.Current.Room(definitionKey);
        if (source is null) return Array.Empty<double>();
        var count = source.FurnisherItems.Select(item => item.Stats.Count).DefaultIfEmpty().Max();
        var values = new double[count];
        foreach (var item in itemAmounts)
        {
            if ((uint)item.Key >= (uint)source.FurnisherItems.Count) continue;
            var stats = source.FurnisherItems[item.Key].Stats;
            for (var index = 0; index < stats.Count; index++)
                values[index] += stats[index] * item.Value;
        }
        foreach (var rule in Rules(definitionKey))
        {
            if ((uint)rule.Index >= (uint)values.Length ||
                (uint)rule.Other >= (uint)values.Length) continue;
            values[rule.Index] = rule.Kind switch
            {
                Transform.Relative => FurnisherRuntime.RelativeStat(
                    values[rule.Index], values[rule.Other], rule.Multiplier),
                Transform.Efficiency => FurnisherRuntime.EfficiencyStat(
                    values[rule.Index], values[rule.Other], rule.Multiplier),
                Transform.EmployeesRelative => values[rule.Other] * rule.Multiplier,
                _ => values[rule.Index]
            };
        }
        ApplyConstructorOverrides(source, values, area, world, existingEmployees);
        return values;
    }

    private static void ApplyConstructorOverrides(
        RoomRule source,
        double[] values,
        IReadOnlyCollection<GridCoord>? area,
        WorldGridData? world,
        int existingEmployees)
    {
        if (source.Key.Equals("HUNTER_NORMAL", StringComparison.OrdinalIgnoreCase))
        {
            if (values.Length < 3) return;
            var hunterWorkers = values[0];
            var rate = source.Recipes.FirstOrDefault()?.Outputs.FirstOrDefault()?.Rate ?? 0;
            var maximum = source.SpecialProduction?.MaximumEmployed ?? 0;
            var employees = existingEmployees + (int)Math.Ceiling(hunterWorkers);
            var employedBonus = maximum <= 0 || employees < maximum
                ? 1.0
                : 1.0 / (1.0 + (employees - maximum) / (maximum * 4.0));
            // Constructor.output.get(): workers * first output rate * race bonus * eBonus.
            // The player-race bonus is neutral until a race-specific bonus is selected.
            values[2] = hunterWorkers * rate * employedBonus;
            return;
        }

        if (!source.Key.Equals("FISHERY_NORMAL", StringComparison.OrdinalIgnoreCase) ||
            values.Length < 5) return;

        var shallow = 0;
        var fish = 0.0;
        var deepAccess = 0.0;
        if (area is not null && world is not null)
            foreach (var cell in area)
            {
                var amount = world.FishAmount(cell);
                deepAccess += amount / 15.0;
                if (!world.Has(cell, TileFlags.Water) || world.Has(cell, TileFlags.DeepWater)) continue;
                shallow++;
                fish += amount / 15.0;
            }

        var workers = fish + shallow / 64.0;
        var auxiliary = values[2];
        var divisor = workers <= 0 ? 1.0 : workers;
        values[0] = workers;
        values[1] *= 31.0; // RoomResStorage(0b011111).max()
        values[2] = Math.Clamp(0.5 + 0.5 * auxiliary / divisor, 0, 1);
        values[3] = deepAccess;
        values[4] = values[2] * (int)workers;
    }

    public static double Value(RoomRecord room, int index)
    {
        var values = Evaluate(room);
        return (uint)index < (uint)values.Length ? values[index] : 0;
    }

    public static double Efficiency(RoomRecord room)
    {
        var index = EfficiencyIndex(room.DefinitionKey);
        return index < 0 ? 1.0 : Value(room, index);
    }

    private static int EfficiencyIndex(string key) => key.ToUpperInvariant() switch
    {
        "UNIVERSITY_NORMAL" or "HUNTER_NORMAL" or "SCHOOL_NORMAL" or
            "_EMBASSY" or "_JANITOR" or "_POLICE" => 1,
        "LIBRARY_NORMAL" or "ADMIN_NORMAL" => 2,
        "FISHERY_NORMAL" => 2,
        var value when value.StartsWith("PASTURE_") || value.StartsWith("MINE_") => 2,
        var value when value.StartsWith("WORKSHOP_") || value.StartsWith("REFINER_") => 1,
        _ => -1
    };

    private static Rule[] Rules(string key) => key.ToUpperInvariant() switch
    {
        "UNIVERSITY_NORMAL" or "HUNTER_NORMAL" or "SCHOOL_NORMAL" or
            "_EMBASSY" or "_JANITOR" or "_POLICE" =>
            new[] { new Rule(1, Transform.Efficiency, 0, 1) },
        "LIBRARY_NORMAL" or "ADMIN_NORMAL" =>
            new[] { new Rule(2, Transform.Efficiency, 0, 1) },
        "GRAVEYARD_NORMAL" => new[] { new Rule(2, Transform.Relative, 1, 1) },
        "TAVERN_NORMAL" or "_INN" or "BREEDER_GARTHIMI" or
            "RESTHOME_NORMAL" or "PLEASURE_NORMAL" =>
            new[] { new Rule(1, Transform.Relative, 0, 1) },
        "CANTEEN_NORMAL" => new[] { new Rule(2, Transform.Relative, 0, 1) },
        "BARBER_NORMAL" => new[]
        {
            new Rule(1, Transform.EmployeesRelative, 0, 1),
            new Rule(2, Transform.Relative, 0, 1)
        },
        "LAVATORY_NORMAL" => new[]
        {
            new Rule(1, Transform.EmployeesRelative, 0, 1.0 / 8.0),
            new Rule(2, Transform.Relative, 0, 1)
        },
        "NURSERY_NORMAL" => new[]
        {
            new Rule(1, Transform.EmployeesRelative, 0, 0.2),
            new Rule(2, Transform.Relative, 0, 1)
        },
        "PHYSICIAN_NORMAL" => new[] { new Rule(2, Transform.Relative, 1, 1) },
        "BATH_NORMAL" => new[] { new Rule(1, Transform.Relative, 0, 1.5) },
        var value when value.StartsWith("PASTURE_") || value.StartsWith("MINE_") =>
            new[] { new Rule(2, Transform.Efficiency, 0, 1) },
        var value when value.StartsWith("WORKSHOP_") || value.StartsWith("REFINER_") =>
            new[] { new Rule(1, Transform.Efficiency, 0, 1) },
        _ => Array.Empty<Rule>()
    };
}
