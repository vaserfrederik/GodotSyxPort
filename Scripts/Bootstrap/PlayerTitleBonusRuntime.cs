using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using GodotSyxPort.Data;

namespace GodotSyxPort.Bootstrap;

/// <summary>Selected StagePickTitles BOOST entries, using the source ADD/MUL order.</summary>
public sealed class PlayerTitleBonusRuntime
{
    private readonly List<(string Pattern, string Operation, double Value)> _entries = new();

    public static PlayerTitleBonusRuntime Load(IEnumerable<string>? titles)
    {
        var result = new PlayerTitleBonusRuntime();
        foreach (var title in titles ?? Array.Empty<string>())
        {
            var titleValue = PlayerProfileProgressRuntime.TitleBoostValue(title);
            if (titleValue <= 0) continue;
            var path = $"res://Data/Original/init/player/titles/{title}.txt";
            if (!FileAccess.FileExists(path)) continue;
            var boost = SyxDataParser.Parse(FileAccess.GetFileAsString(path)).Get("BOOST")?.Fields;
            if (boost is null) continue;
            foreach (var pair in boost)
            {
                var separator = pair.Key.LastIndexOf('>');
                if (separator <= 0 || !double.TryParse(pair.Value.Text(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var value)) continue;
                var operation = pair.Key[(separator + 1)..].ToUpperInvariant();
                var scaled = operation == "MUL" ? 1 + (value - 1) * titleValue : value * titleValue;
                result._entries.Add((pair.Key[..separator], operation, scaled));
            }
        }
        return result;
    }

    public double Apply(string metric, double value)
    {
        foreach (var entry in _entries.Where(entry => Matches(entry.Pattern, metric)))
            value = entry.Operation == "MUL" ? value * entry.Value : value + entry.Value;
        return value;
    }

    private static bool Matches(string pattern, string metric) => pattern.EndsWith('*')
        ? metric.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase)
        : metric.Equals(pattern, StringComparison.OrdinalIgnoreCase);
}
