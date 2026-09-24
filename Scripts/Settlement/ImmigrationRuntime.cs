using System;
using System.Collections.Generic;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;
using GodotSyxPort.Stats;

namespace GodotSyxPort.Settlement;

public sealed class RaceImmigrationState
{
    public double Timer { get; internal set; }
    public int AutoAdmit { get; set; }
    public double Emigrants { get; internal set; }
}

/// <summary>Numerical core of Immigration; happiness/standing are explicit inputs.</summary>
public sealed class ImmigrationRuntime
{
    public const double HappinessThreshold = 0.9;
    private readonly Dictionary<string, RaceImmigrationState> _states =
        new(StringComparer.OrdinalIgnoreCase);
    public RaceImmigrationState State(string race) =>
        _states.TryGetValue(race, out var state) ? state : _states[race] = new RaceImmigrationState();

    public int Update(
        RaceRule race, double delta, double secondsPerDay, double happiness,
        double expectedPopulation, int currentAndIncoming, double immigrationBoost = 1,
        bool campAvailable = false, int campPopulation = 0, double standingPower = 1,
        int incomingSubjects = 0, double campReplenishmentPerDay = 0)
    {
        var state = State(race.Key);
        var wanted = WantedUltimately(
            race, happiness, expectedPopulation, currentAndIncoming,
            campAvailable, campPopulation, standingPower, incomingSubjects);
        if (wanted < 0)
        {
            state.Emigrants += -delta * wanted / (2 * secondsPerDay);
            state.Timer = 0;
            return 0;
        }
        state.Emigrants = 0;
        var arrivalsPerDay = campAvailable ? campReplenishmentPerDay : immigrationBoost;
        state.Timer = Math.Clamp(state.Timer + Math.Max(0, arrivalsPerDay) * delta / secondsPerDay, 0, wanted);
        var autoAdmit = race.PopulationMaximum == 0 ? SettlementStandingRuntime.EntityMaximum : state.AutoAdmit;
        var admittedCapacity = autoAdmit - currentAndIncoming;
        if (admittedCapacity <= 0) return 0;
        var amount = Math.Clamp(Math.Min((int)state.Timer, wanted), 0, admittedCapacity);
        state.Timer -= amount;
        return amount;
    }

    public bool ShouldEmigrate(string race)
    {
        var state = State(race);
        if (state.Emigrants <= 1) return false;
        state.Emigrants--;
        return true;
    }

    public static int WantedUltimately(
        RaceRule race, double happiness, double expectedPopulation, int currentAndIncoming,
        bool campAvailable = false, int campPopulation = 0, double standingPower = 1,
        int incomingSubjects = 0)
    {
        if (campAvailable)
            return Math.Max(campPopulation - currentAndIncoming, 0) - incomingSubjects;
        if (expectedPopulation == 0)
            return (int)Math.Ceiling(happiness - 0.1) - incomingSubjects;
        happiness = Math.Clamp(happiness, 0, 2);
        var attraction = happiness - HappinessThreshold;
        if (attraction <= 0)
            return (int)(expectedPopulation * attraction / HappinessThreshold) - incomingSubjects;
        attraction *= 0.5;
        var amount = attraction * race.PopulationMaximum * expectedPopulation;
        if (amount > 1)
        {
            var blend = amount / (amount + expectedPopulation);
            amount = amount * (1 - blend) + blend * Math.Pow(amount, 1 / Math.Max(0.01, standingPower));
        }
        return Math.Max((int)Math.Ceiling(amount), 0) - incomingSubjects;
    }
}
