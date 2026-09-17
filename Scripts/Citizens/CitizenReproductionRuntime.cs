using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;

namespace GodotSyxPort.Citizens;

public enum CitizenLifeStage : byte { Infant, Child, Adult }

public readonly record struct ReproductionGroup(string Race, SocialClass Class);
public readonly record struct ReproductionSnapshot(
    int Infants, int Children, int Adults, int FertileAdults, double ChildrenPerYear);

/// <summary>
/// Shared numerical core adapted from StatsAge, StatsReproduction, AIModule_Parent and
/// AIModule_Child. Entity behaviour stays in CitizenSystem; all race day thresholds and
/// population-limit calculations live here.
/// </summary>
public sealed class CitizenReproductionRuntime
{
    public const int SourceYearDays = 16;
    public const int ChecksPerYear = 4;
    public const double BaseDeathAgeYears = 100.0;
    public const double BaseReproductionAge = 0.5;
    public const double BaseReproductionSpeed = 0.1;
    private readonly IReadOnlyDictionary<string, RaceRule> _races;
    private readonly Dictionary<ReproductionGroup, int> _populationLimits = new();

    public CitizenReproductionRuntime(IReadOnlyDictionary<string, RaceRule> races) => _races = races;

    public CitizenLifeStage Stage(string raceKey, int ageDays)
    {
        var race = _races[raceKey];
        if (race.BabyDays > 0 && ageDays < race.BabyDays) return CitizenLifeStage.Infant;
        return ageDays < race.BabyDays + race.ChildDays
            ? CitizenLifeStage.Child
            : CitizenLifeStage.Adult;
    }

    public HumanoidType ChildTypeForAge(string raceKey, SocialClass socialClass, int ageDays)
    {
        if (ageDays < AdultAgeDays(raceKey))
            return socialClass == SocialClass.Slave
                ? HumanoidType.ChildSlave : HumanoidType.Child;
        return socialClass == SocialClass.Slave ? HumanoidType.Slave : HumanoidType.Subject;
    }

    public int AdultAgeDays(string raceKey)
    {
        var race = _races[raceKey];
        return race.BabyDays + race.ChildDays;
    }

    public bool Propagates(string raceKey, SocialClass socialClass) =>
        socialClass is SocialClass.Citizen or SocialClass.Slave &&
        _races[raceKey].BabyDays > 0 && _races[raceKey].ReproductionSpeedMultiplier > 0;

    public bool IsFertile(string raceKey, int ageDays)
    {
        var race = _races[raceKey];
        var from = (int)Math.Ceiling(1.5 * AdultAgeDays(raceKey));
        var lifespan = (int)Math.Ceiling(
            BaseDeathAgeYears * SourceYearDays * race.DeathAgeMultiplier);
        var to = from + (int)((lifespan - from) *
            BaseReproductionAge * race.ReproductionAgeMultiplier);
        return ageDays >= from && ageDays < to;
    }

    public double ReproductionSpeed(string raceKey, int citizenPopulation, int setting = 50)
    {
        var race = _races[raceKey];
        var normalized = Math.Clamp((setting - 50) / 50.0, -1.0, 1.0);
        var forced = normalized < 0 ? 1.0 + 0.8 * normalized : 1.0 + 3.0 * normalized;
        var population = citizenPopulation >= 2000
            ? 1.0 : 0.2 + 0.8 * citizenPopulation / 2000.0;
        return BaseReproductionSpeed * race.ReproductionSpeedMultiplier * forced * population;
    }

    public double ChancePerCheck(string raceKey, int citizenPopulation, int setting = 50) =>
        ReproductionSpeed(raceKey, citizenPopulation, setting) / ChecksPerYear;

    public void SetPopulationLimit(string raceKey, SocialClass socialClass, int limit) =>
        _populationLimits[new ReproductionGroup(raceKey, socialClass)] = Math.Max(0, limit);

    public int NewInfantsAllowed(
        string raceKey, SocialClass socialClass, int currentAndIncomingPopulation)
    {
        if (!Propagates(raceKey, socialClass)) return 0;
        var group = new ReproductionGroup(raceKey, socialClass);
        // Source exposes negative debt to standing/forced-propagation consumers.
        return _populationLimits.GetValueOrDefault(group, int.MaxValue) -
               currentAndIncomingPopulation;
    }

    public ReproductionSnapshot Snapshot(
        IEnumerable<(string Race, SocialClass Class, HumanoidType Type, int Age)> population,
        string raceKey, SocialClass socialClass)
    {
        var group = population.Where(item => item.Race.Equals(raceKey, StringComparison.OrdinalIgnoreCase) &&
                                             item.Class == socialClass).ToArray();
        var infants = group.Count(item => item.Type is HumanoidType.Parent or HumanoidType.ParentSlave);
        var children = group.Count(item => item.Type is HumanoidType.Child or HumanoidType.ChildSlave);
        var adults = group.Length - children;
        var fertile = Propagates(raceKey, socialClass)
            ? group.Count(item => item.Type is HumanoidType.Subject or HumanoidType.Slave &&
                                  IsFertile(raceKey, item.Age))
            : 0;
        return new ReproductionSnapshot(infants, children, adults, fertile,
            fertile * ReproductionSpeed(raceKey,
                group.Count(item => item.Class == SocialClass.Citizen)));
    }
}
