using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;
using GodotSyxPort.Bootstrap;

namespace GodotSyxPort.Stats;

public readonly record struct StandingContribution(
    string Key, double Current, double Default, double Maximum, bool Dismissed = false,
    bool Inverted = false, double Multiplier = 1, double Exponent = 1);

public sealed class StandingGroupRuntime
{
    public string Race { get; init; } = "";
    public SocialClass Class { get; init; }
    public double Expectation { get; internal set; }
    public double Fulfillment { get; internal set; }
    public double Happiness { get; internal set; }
    public double LoyaltyTarget { get; internal set; }
    public double Loyalty { get; internal set; }
    public double EmergencySeconds { get; internal set; }
}

/// <summary>
/// Numerical core of STANDINGS, StandingCitizen and StandingBuff. Stat discovery remains
/// data-driven: callers supply each race/class contribution instead of hard-coded room lists.
/// </summary>
public sealed class SettlementStandingRuntime
{
    public const int EntityMaximum = 40000;
    public const double MaximumPopulationFactor = 0.7143;
    public const double FulfillmentExponent = 2.65;
    public const double LoyaltyChangeDays = 100.0;
    public const double EmergencyDefaultDays = 8.0;
    private readonly Dictionary<(string Race, SocialClass Class), StandingGroupRuntime> _groups = new();

    public IReadOnlyCollection<StandingGroupRuntime> Groups => _groups.Values;

    public StandingGroupRuntime Get(string race, SocialClass socialClass)
    {
        var key = (race.ToUpperInvariant(), socialClass);
        if (!_groups.TryGetValue(key, out var group))
            _groups.Add(key, group = new StandingGroupRuntime { Race = race, Class = socialClass });
        return group;
    }

    public void Emergency(SocialClass socialClass, double secondsPerDay, double? seconds = null)
    {
        foreach (var group in _groups.Values.Where(group => group.Class == socialClass))
            group.EmergencySeconds = Math.Max(group.EmergencySeconds,
                seconds ?? secondsPerDay * EmergencyDefaultDays);
    }

    public StandingGroupRuntime Update(
        RaceRule race, SocialClass socialClass, int racePopulation, int totalPopulation,
        bool playerRace, IEnumerable<StandingContribution> sourceContributions,
        double delta, double secondsPerDay)
    {
        var group = Get(race.Key, socialClass);
        var contributions = sourceContributions.Where(item => !item.Dismissed).ToArray();
        group.Expectation = Expectation(race, racePopulation,
            Math.Max(0, totalPopulation - racePopulation), playerRace);
        group.Fulfillment = Fulfillment(race, contributions, playerRace);
        group.Happiness = group.Fulfillment <= 0 ? 0 : group.Expectation == 0
            ? 1 : Math.Clamp(group.Fulfillment / group.Expectation, 0, 10);
        group.Happiness = Math.Clamp(GameSession.TitleBonuses.Apply(
            "BEHAVIOUR_HAPPINESS", group.Happiness), 0, 10);
        group.LoyaltyTarget = Math.Clamp(group.Happiness / 10.0, 0, 1);
        if (group.EmergencySeconds > 0)
        {
            group.EmergencySeconds = Math.Max(0, group.EmergencySeconds - delta);
            group.LoyaltyTarget = Math.Clamp(group.LoyaltyTarget +
                group.EmergencySeconds / (secondsPerDay * EmergencyDefaultDays), 0, 1);
        }
        group.Loyalty = MoveLoyalty(
            group.Loyalty, group.LoyaltyTarget, delta, secondsPerDay);
        return group;
    }

    public static double Expectation(
        RaceRule race, int amount, int other, bool playerRace)
    {
        var total = 1.0 + amount + other;
        var occurrence = race.PopulationMaximum <= 0 ? 1.0 : race.PopulationMaximum;
        var value = Math.Sqrt(amount / total) *
                    (total / (EntityMaximum * MaximumPopulationFactor)) / occurrence;
        return playerRace ? value : value * 2.0;
    }

    public static double Fulfillment(
        RaceRule race, IReadOnlyCollection<StandingContribution> contributions, bool playerRace)
    {
        var maximum = contributions.Sum(item => item.Maximum);
        var defaults = contributions.Sum(item => item.Default);
        var current = contributions.Sum(item => Normalize(item));
        if (maximum <= 0) return 1;
        double normalized;
        if (current < defaults)
            // StandingCitizen.prognosis treats values below the default as a deficit,
            // not as a partial positive bonus.
            normalized = defaults <= 0 ? 0 : -current / defaults;
        else
        {
            current -= defaults;
            maximum -= defaults;
            normalized = maximum <= 0 ? 1 : Math.Pow(current / maximum, FulfillmentExponent);
        }
        var minimum = Expectation(race, playerRace ? 6 : 2, 0, playerRace);
        if (normalized < 0) return minimum * -normalized;
        return Math.Clamp(minimum + normalized, 0, 10);
    }

    public static double MoveLoyalty(double current, double target, double delta, double secondsPerDay)
    {
        var difference = (int)(target * 100) - (int)(current * 100);
        var multiplier = 1.0 + Math.Abs(difference) / 25.0;
        var change = difference * delta / (LoyaltyChangeDays * secondsPerDay) * multiplier;
        var result = current + change;
        if (change < 0 && result < target || change > 0 && result > target) return target;
        return Math.Clamp(result, 0, 1);
    }

    public static StandingContribution FromRaceRule(RaceStandingRule rule, SocialClass socialClass,
        double input, double defaultInput = 0) =>
        new(rule.Key, input, defaultInput, rule.Maximum.GetValueOrDefault(socialClass), rule.Dismiss,
            rule.Inverted, rule.Multiplier, rule.Exponent);

    private static double Normalize(StandingContribution contribution)
    {
        var value = Math.Clamp(contribution.Current * contribution.Multiplier, 0, 1);
        if (contribution.Exponent != 1) value = Math.Pow(value, contribution.Exponent);
        value *= contribution.Maximum;
        return contribution.Inverted ? contribution.Maximum - value : value;
    }
}
