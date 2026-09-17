using System;
using System.Linq;
using GodotSyxPort.Data;

namespace GodotSyxPort.Rooms;

/// <summary>Source-order FurnisherStat evaluation after raw item-stat accumulation.</summary>
public static class FurnisherStatRuntime
{
    private enum Transform : byte { Relative, Efficiency, EmployeesRelative }
    private readonly record struct Rule(int Index, Transform Kind, int Other, double Multiplier);

    public static double[] Evaluate(RoomRecord room)
        => Evaluate(room.DefinitionKey, room.ItemGroupAmounts);

    public static double[] Evaluate(
        string definitionKey, System.Collections.Generic.IReadOnlyDictionary<int, double> itemAmounts)
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
        return values;
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
