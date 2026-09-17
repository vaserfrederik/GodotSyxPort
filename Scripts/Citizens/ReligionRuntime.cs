using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Data;

namespace GodotSyxPort.Citizens;

/// <summary>
/// Runtime affiliation layer for StatsReligion. Affiliation is selected from the original
/// DEFAULT_SPREAD weights; opposition and boosts remain entirely data-driven.
/// </summary>
public sealed class ReligionRuntime
{
    private readonly IReadOnlyDictionary<string, ReligionRule> _rules;
    private readonly ReligionRule[] _ordered;

    public ReligionRuntime(IReadOnlyDictionary<string, ReligionRule> rules)
    {
        _rules = rules;
        _ordered = rules.Values.OrderBy(rule => rule.Key, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public string ChooseAffiliation()
    {
        if (_ordered.Length == 0) return "";
        var total = _ordered.Sum(rule => Math.Max(0, rule.DefaultSpread));
        if (total <= 0) return _ordered[0].Key;
        var draw = GD.Randf() * total;
        foreach (var rule in _ordered)
        {
            draw -= Math.Max(0, rule.DefaultSpread);
            if (draw <= 0) return rule.Key;
        }
        return _ordered[^1].Key;
    }

    public double Opposition(string religion, string other) =>
        _rules.TryGetValue(religion, out var rule) ? rule.Opposition.GetValueOrDefault(other) : 0;

    public IReadOnlyDictionary<string, double> Boosts(string religion) =>
        _rules.TryGetValue(religion, out var rule)
            ? rule.Boosts
            : new Dictionary<string, double>();

    // StatsReligion.opposition(): follower-weighted opposition of every
    // affiliation against every other affiliation, normalized by population.
    public double SettlementOpposition(IReadOnlyDictionary<string, int> followers)
    {
        var population = followers.Values.Sum();
        if (population <= 0) return 1;
        var value = 0.0;
        foreach (var religion in followers)
        {
            var against = 0.0;
            foreach (var other in followers)
                against += other.Value / (double)population * Opposition(religion.Key, other.Key);
            value += against * religion.Value;
        }
        return Math.Clamp(value / population, 0.0, 1.0);
    }
}
