using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using GodotSyxPort.Data;

namespace GodotSyxPort.Bootstrap;

/// <summary>Godot profile equivalent of PTitles Titles2: unlocked title -> race keys.</summary>
public static class PlayerProfileProgressRuntime
{
    private const string PathName = "user://player_titles.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };
    private static Dictionary<string, HashSet<string>>? _titles;

    public static IReadOnlyCollection<string> TitleRaces(string title)
        => Load().TryGetValue(title, out var races) ? races : Array.Empty<string>();

    public static bool TitleUnlocked(string title) => TitleRaces(title).Count > 0;

    public static double TitleBoostValue(string title)
    {
        var playable = OriginalGameData.Current.Races.Values.Count(value => value.Playable);
        var count = TitleRaces(title).Count;
        return count == 0 ? 0.5 : 0.5 + 0.5 * count / Math.Max(1.0, playable);
    }

    public static bool UnlockTitle(string title, string race)
    {
        if (!OriginalGameData.Current.PlayerTitles.ContainsKey(title) ||
            !OriginalGameData.Current.Races.TryGetValue(race, out var rule) || !rule.Playable) return false;
        var all = Load();
        if (!all.TryGetValue(title, out var races)) all[title] = races = new(StringComparer.OrdinalIgnoreCase);
        if (!races.Add(race)) return false;
        Save(all); return true;
    }

    private static Dictionary<string, HashSet<string>> Load()
    {
        if (_titles is not null) return _titles;
        _titles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var path = ProjectSettings.GlobalizePath(PathName);
        if (!File.Exists(path)) return _titles;
        try
        {
            var stored = JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                File.ReadAllText(path), Options) ?? new Dictionary<string, string[]>();
            foreach (var pair in stored)
                _titles[pair.Key] = new HashSet<string>(pair.Value, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            GD.PushWarning($"Не удалось прочитать профиль титулов: {exception.Message}");
        }
        return _titles;
    }

    private static void Save(Dictionary<string, HashSet<string>> titles)
    {
        try
        {
            var stored = titles.ToDictionary(pair => pair.Key, pair => pair.Value.OrderBy(value => value).ToArray(),
                StringComparer.OrdinalIgnoreCase);
            File.WriteAllText(ProjectSettings.GlobalizePath(PathName), JsonSerializer.Serialize(stored, Options));
        }
        catch (IOException exception)
        {
            GD.PushWarning($"Не удалось сохранить профиль титулов: {exception.Message}");
        }
    }
}
