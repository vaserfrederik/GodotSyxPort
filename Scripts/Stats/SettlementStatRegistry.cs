using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;

namespace GodotSyxPort.Stats;

public readonly record struct StatGroup(string Race, SocialClass Class);

/// <summary>
/// Data-oriented replacement for STAT/STATData/StatCollection/StatsEvent. It avoids
/// per-stat Java object trees while preserving named values, race/class projections,
/// event flags, decree bounds and a short history suitable for later UI graphs.
/// </summary>
public sealed class SettlementStatRegistry
{
    public const int HistoryDays = 48;
    private readonly Dictionary<string, double> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string Key, StatGroup Group), double> _groups = new();
    private readonly Dictionary<string, Queue<double>> _history = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string Key, int CitizenId), bool> _events = new();
    private readonly Dictionary<string, (double Minimum, double Maximum, double Value)> _decrees =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, double> Values => _values;
    public void Set(string key, double value) => _values[key] = value;
    public void Add(string key, double value) => Set(key, Get(key) + value);
    public double Get(string key, double fallback = 0) => _values.GetValueOrDefault(key, fallback);

    public void Set(string key, string race, SocialClass socialClass, double value) =>
        _groups[(key, new StatGroup(race.ToUpperInvariant(), socialClass))] = value;
    public double Get(string key, string race, SocialClass socialClass, double fallback = 0) =>
        _groups.GetValueOrDefault((key, new StatGroup(race.ToUpperInvariant(), socialClass)), fallback);
    public double Booster(string key, string race, SocialClass socialClass) =>
        Math.Clamp(Get(key, race, socialClass), 0, 1);

    public void SetEvent(string key, int citizenId, bool active) =>
        _events[(key, citizenId)] = active;
    public bool Event(string key, int citizenId) => _events.GetValueOrDefault((key, citizenId));
    public void ClearEventsFor(int citizenId)
    {
        foreach (var key in _events.Keys.Where(key => key.CitizenId == citizenId).ToArray())
            _events.Remove(key);
    }

    public void DefineDecree(string key, double minimum, double maximum, double defaultValue)
    {
        _decrees.TryAdd(key, (minimum, maximum, Math.Clamp(defaultValue, minimum, maximum)));
    }
    public bool SetDecree(string key, double value)
    {
        if (!_decrees.TryGetValue(key, out var decree)) return false;
        _decrees[key] = (decree.Minimum, decree.Maximum, Math.Clamp(value, decree.Minimum, decree.Maximum));
        return true;
    }
    public double Decree(string key, double fallback = 0) =>
        _decrees.TryGetValue(key, out var decree) ? decree.Value : fallback;

    public IReadOnlyList<double> History(string key) => _history.GetValueOrDefault(key)?.ToArray() ?? Array.Empty<double>();
    public void BeginDay()
    {
        foreach (var pair in _values)
        {
            if (!_history.TryGetValue(pair.Key, out var history))
                _history[pair.Key] = history = new Queue<double>(HistoryDays);
            history.Enqueue(pair.Value);
            while (history.Count > HistoryDays) history.Dequeue();
        }
    }
}
