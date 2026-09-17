using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

namespace GodotSyxPort.Rooms;

public sealed record FurnisherGroupConstraint(int Minimum, int Maximum);

/// <summary>Literal Java flush(min,max,rotations) and FurnisherStat.min catalog.</summary>
public static class FurnisherConstraintCatalog
{
    private static Dictionary<(string Family, int Group), FurnisherGroupConstraint>? _groups;
    private static Dictionary<string, double[]>? _statMinimums;

    public static FurnisherGroupConstraint? Group(string roomKey, int group)
    {
        EnsureLoaded();
        var family = FurnisherLayoutCatalog.FamilyForRoom(roomKey);
        return family is not null && _groups!.TryGetValue((family, group), out var value)
            ? value : null;
    }

    public static IReadOnlyList<double> StatMinimums(string roomKey)
    {
        EnsureLoaded();
        var family = FurnisherLayoutCatalog.FamilyForRoom(roomKey);
        return family is not null && _statMinimums!.TryGetValue(family, out var values)
            ? values : Array.Empty<double>();
    }

    private static void EnsureLoaded()
    {
        if (_groups is not null) return;
        _groups = new Dictionary<(string, int), FurnisherGroupConstraint>();
        _statMinimums = new Dictionary<string, double[]>();
        var source = FileAccess.GetFileAsString("res://Data/Original/furnisher_constraints.tsv");
        foreach (var line in source.Split('\n').Skip(1))
        {
            var parts = line.Trim().Split('\t');
            if (parts.Length != 5 || !int.TryParse(parts[1], out var group) ||
                !int.TryParse(parts[2], out var minimum) ||
                !int.TryParse(parts[3], out var maximum)) continue;
            _groups[(parts[0], group)] = new FurnisherGroupConstraint(minimum, maximum);
            if (_statMinimums.ContainsKey(parts[0])) continue;
            _statMinimums[parts[0]] = string.IsNullOrWhiteSpace(parts[4])
                ? Array.Empty<double>()
                : parts[4].Split(',').Select(value => double.Parse(
                    value, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
