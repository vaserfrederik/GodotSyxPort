using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace GodotSyxPort.Rendering;

public static class OriginalWorldPalette
{
    private static readonly Lazy<IReadOnlyList<Color>> FactionColors = new(LoadFactionColors);

    public static Color Faction(int id)
    {
        var colors = FactionColors.Value;
        if (colors.Count == 0)
            return Color.FromHsv(Mathf.PosMod(id * 0.618034f, 1f), 0.52f, 0.82f);
        return colors[Math.Abs(id) % colors.Count];
    }

    private static IReadOnlyList<Color> LoadFactionColors()
    {
        var result = new List<Color>();
        var path = ProjectSettings.GlobalizePath("res://Data/Original/init/world/config/Faction.txt");
        if (!File.Exists(path)) return result;
        foreach (Match match in Regex.Matches(File.ReadAllText(path), @"(?m)(\d{1,3})_(\d{1,3})_(\d{1,3})"))
        {
            var r = Math.Clamp(int.Parse(match.Groups[1].Value), 0, 255) / 255f;
            var g = Math.Clamp(int.Parse(match.Groups[2].Value), 0, 255) / 255f;
            var b = Math.Clamp(int.Parse(match.Groups[3].Value), 0, 255) / 255f;
            result.Add(new Color(r, g, b));
        }
        return result;
    }
}
