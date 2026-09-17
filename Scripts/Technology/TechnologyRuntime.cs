using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.Technology;

public sealed record TechnologySnapshot(
    IReadOnlyDictionary<string, int> Levels,
    IReadOnlyDictionary<string, double> FrozenCurrencies);

public sealed class TechnologyCurrencyRuntime
{
    public string Key { get; init; } = "";
    public int Allocated { get; set; }
    public double Frozen { get; set; }
    public double Penalty { get; set; }
    public int Total { get; set; }
    public int Available => Total - (int)Math.Ceiling(Frozen) - Allocated;
}

/// <summary>Semantic PTech/TECHS runtime without the old boost/UI graph.</summary>
public sealed class TechnologyRuntime
{
    public const double ForgetThreshold = 0.8;
    private readonly KnowledgeRuntime _knowledge;
    private readonly Dictionary<string, int> _levels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TechnologyCurrencyRuntime> _currencies =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _penalties = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> Levels => _levels;
    public IReadOnlyDictionary<string, TechnologyCurrencyRuntime> Currencies => _currencies;

    public TechnologySnapshot Capture() => new(
        new Dictionary<string, int>(_levels, StringComparer.OrdinalIgnoreCase),
        _currencies.ToDictionary(pair => pair.Key, pair => pair.Value.Frozen,
            StringComparer.OrdinalIgnoreCase));

    public void Restore(TechnologySnapshot snapshot)
    {
        _levels.Clear();
        foreach (var pair in snapshot.Levels)
        {
            var technology = OriginalGameData.Current.Technology(pair.Key);
            if (technology is not null)
                _levels[pair.Key] = Math.Clamp(pair.Value, 0, technology.LevelMaximum);
        }
        foreach (var currency in _currencies.Values) currency.Frozen = 0;
        foreach (var pair in snapshot.FrozenCurrencies)
            if (_currencies.TryGetValue(pair.Key, out var currency))
                currency.Frozen = Math.Max(0, pair.Value);
        Recalculate();
    }

    public TechnologyRuntime(KnowledgeRuntime knowledge)
    {
        _knowledge = knowledge;
        foreach (var key in OriginalGameData.Current.Technologies.Values
                     .SelectMany(technology => technology.Costs.Keys).Distinct(StringComparer.OrdinalIgnoreCase))
            _currencies[key] = new TechnologyCurrencyRuntime { Key = key };
        ValidateRequirements();
        Recalculate();
    }

    public int Level(string technology) => _levels.GetValueOrDefault(technology);
    public double Penalty(string technology) => _penalties.GetValueOrDefault(technology);
    public double EffectiveLevel(string technology) =>
        Level(technology) * (1.0 - Penalty(technology));

    public bool CanUnlockNext(string key)
    {
        var technology = OriginalGameData.Current.Technology(key);
        if (technology is null || Level(key) >= technology.LevelMaximum) return false;
        var costs = RequiredPurchaseCosts(technology);
        return costs.All(pair => _currencies.GetValueOrDefault(pair.Key)?.Available >= pair.Value);
    }

    public bool UnlockNext(string key)
    {
        var technology = OriginalGameData.Current.Technology(key);
        if (technology is null || !CanUnlockNext(key)) return false;
        foreach (var requirement in TransitiveRequirements(technology)
                     .OrderBy(requirement => requirement.Technology))
        {
            var required = OriginalGameData.Current.Technology(requirement.Technology);
            if (required is null) continue;
            _levels[required.Key] = Math.Max(Level(required.Key),
                Math.Clamp(requirement.Level, 0, required.LevelMaximum));
        }
        _levels[key] = Level(key) + 1;
        Recalculate();
        return true;
    }

    public bool SetLevel(string key, int level)
    {
        var technology = OriginalGameData.Current.Technology(key);
        if (technology is null) return false;
        var old = Level(key);
        var clamped = Math.Clamp(level, 0, technology.LevelMaximum);
        if (clamped < old)
        {
            foreach (var cost in technology.Costs)
            {
                var currency = Currency(cost.Key);
                currency.Frozen += 0.5 * Math.Max(0,
                    CostTotal(cost.Value, technology, old) -
                    CostTotal(cost.Value, technology, clamped));
            }
        }
        _levels[key] = clamped;
        Recalculate();
        return true;
    }

    public void Tick(double delta)
    {
        var secondsPerDay = Math.Max(1, OriginalGameData.Current.SecondsPerDay);
        foreach (var currency in _currencies.Values)
        {
            currency.Total = Math.Max(0, (int)_knowledge.Currency(currency.Key));
            if (currency.Frozen > 0)
            {
                var thaw = Math.Max(currency.Frozen / (secondsPerDay * 4.0), 100.0 / secondsPerDay);
                currency.Frozen = Math.Max(0, currency.Frozen - thaw * Math.Max(0, delta));
            }
        }
        RecalculatePenalties();
    }

    public bool IsContentUnlocked(string contentKey)
    {
        var locking = OriginalGameData.Current.Technologies.Values
            .Where(technology => technology.Unlocks.Contains(contentKey, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        return locking.Length == 0 || locking.Any(technology =>
            Level(technology.Key) > 0 && Penalty(technology.Key) <= 0);
    }

    public double AdditiveBoost(string boostKey)
    {
        var total = 0.0;
        foreach (var technology in OriginalGameData.Current.Technologies.Values)
        {
            var level = EffectiveLevel(technology.Key);
            if (level <= 0) continue;
            foreach (var boost in technology.Boosts.Where(pair =>
                         pair.Key.Equals(boostKey, StringComparison.OrdinalIgnoreCase) ||
                         pair.Key.StartsWith(boostKey + ">", StringComparison.OrdinalIgnoreCase)))
                total += boost.Value * level;
        }
        return total;
    }

    public int CostOfNext(string key, string currencyKey)
    {
        var technology = OriginalGameData.Current.Technology(key);
        if (technology is null) return 0;
        var own = CostLevel(technology.Costs.GetValueOrDefault(currencyKey), technology, Level(key) + 1);
        var prerequisites = TransitiveRequirements(technology).Sum(requirement =>
        {
            var required = OriginalGameData.Current.Technology(requirement.Technology);
            if (required is null) return 0;
            var baseCost = required.Costs.GetValueOrDefault(currencyKey);
            return Math.Max(0, CostTotal(baseCost, required, requirement.Level) -
                               CostTotal(baseCost, required, Level(required.Key)));
        });
        return own + prerequisites;
    }

    public static int CostLevel(double amount, TechnologyRule technology, int level)
    {
        if (amount == 0) return 0;
        if (level > 1)
            amount += Math.Round(technology.LevelCostIncrease * Math.Clamp(level - 1, 0, level));
        return (int)Math.Ceiling(amount);
    }

    public static int CostTotal(double amount, TechnologyRule technology, int level)
    {
        var count = Math.Max(0, level);
        var result = (int)amount * count;
        if (count <= 1) return result;
        count--;
        return result + (int)technology.LevelCostIncrease * count * (count + 1) / 2;
    }

    private Dictionary<string, int> RequiredPurchaseCosts(TechnologyRule technology)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var currency in _currencies.Keys)
            result[currency] = CostOfNext(technology.Key, currency);
        return result;
    }

    private IEnumerable<TechnologyRequirementRule> TransitiveRequirements(TechnologyRule technology)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        void Visit(TechnologyRule current)
        {
            foreach (var requirement in current.Requirements)
            {
                result[requirement.Technology] = Math.Max(
                    result.GetValueOrDefault(requirement.Technology), requirement.Level);
                var required = OriginalGameData.Current.Technology(requirement.Technology);
                if (required is not null) Visit(required);
            }
        }
        Visit(technology);
        return result.Select(pair => new TechnologyRequirementRule(pair.Key, pair.Value));
    }

    private TechnologyCurrencyRuntime Currency(string key)
    {
        if (!_currencies.TryGetValue(key, out var currency))
            _currencies.Add(key, currency = new TechnologyCurrencyRuntime { Key = key });
        return currency;
    }

    private void Recalculate()
    {
        foreach (var currency in _currencies.Values) currency.Allocated = 0;
        foreach (var technology in OriginalGameData.Current.Technologies.Values)
        {
            var level = Level(technology.Key);
            foreach (var cost in technology.Costs)
                Currency(cost.Key).Allocated += CostTotal(cost.Value, technology, level);
        }
        foreach (var currency in _currencies.Values)
            currency.Total = Math.Max(0, (int)_knowledge.Currency(currency.Key));
        RecalculatePenalties();
    }

    private void RecalculatePenalties()
    {
        foreach (var currency in _currencies.Values)
        {
            var maintained = (currency.Frozen + currency.Allocated) * ForgetThreshold;
            currency.Penalty = maintained <= currency.Total || maintained <= 0
                ? 0 : Math.Pow(1.0 - currency.Total / maintained, 2);
        }
        foreach (var technology in OriginalGameData.Current.Technologies.Values)
        {
            var costTotal = technology.Costs.Values.Sum();
            _penalties[technology.Key] = costTotal <= 0 ? 0 : technology.Costs.Sum(cost =>
                Currency(cost.Key).Penalty * cost.Value / costTotal);
        }
    }

    private static void ValidateRequirements()
    {
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(TechnologyRule technology)
        {
            if (visited.Contains(technology.Key)) return;
            if (!visiting.Add(technology.Key))
                throw new InvalidOperationException($"Cyclic technology requirement: {technology.Key}");
            foreach (var requirement in technology.Requirements)
                if (OriginalGameData.Current.Technology(requirement.Technology) is { } required)
                    Visit(required);
            visiting.Remove(technology.Key);
            visited.Add(technology.Key);
        }
        foreach (var technology in OriginalGameData.Current.Technologies.Values) Visit(technology);
    }
}
