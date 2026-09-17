using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Data;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Stats;

/// <summary>
/// Compact settlement-level projection of the already simulated individual data. It is the
/// common consumer for the source StatsAccess/StatsFood/StatsStored/StatsBurial/StatsEnv
/// families until race/class history storage is ported.
/// </summary>
public sealed class SettlementStatsRuntime
{
    private static readonly ResourceKind[] EdibleResources =
    {
        ResourceKind.Egg, ResourceKind.Fish, ResourceKind.Food, ResourceKind.Meat,
        ResourceKind.Mushroom, ResourceKind.Ration, ResourceKind.Fruit, ResourceKind.Vegetable
    };
    private double _updateLeft;
    private double _historyDayLeft;
    public int Population { get; private set; }
    public double Hunger { get; private set; }
    public int Starving { get; private set; }
    public double FoodDays { get; private set; }
    public double Unburied { get; private set; }
    public double BurialAccess { get; private set; }
    public double BurialDisturbance { get; private set; }
    public IReadOnlyDictionary<ResourceKind, double> StoredPerCapita => _storedPerCapita;
    public IReadOnlyDictionary<string, double> ServiceAccess => _serviceAccess;
    public IReadOnlyDictionary<string, int> ReligionFollowers => _religionFollowers;
    public IReadOnlyDictionary<string, double> TempleAccessByReligion => _templeAccess;
    public IReadOnlyDictionary<string, double> TempleQualityByReligion => _templeQuality;
    public IReadOnlyDictionary<string, double> ShrineAccessByReligion => _shrineAccess;
    public IReadOnlyDictionary<string, double> ShrineQualityByReligion => _shrineQuality;
    public double ReligiousOpposition { get; private set; }
    public int Housed { get; private set; }
    public int HousingCapacity { get; private set; }
    public double HousingAccess { get; private set; }
    public double AverageEducation { get; private set; }
    public double AverageCombatExperience { get; private set; }
    public int EnemyKills { get; private set; }
    public int Prisoners { get; private set; }
    public IReadOnlyDictionary<(string Race, SocialClass Class), double> Law => _law;
    public IReadOnlyDictionary<(string Race, SocialClass Class), double> Tyranny => _tyranny;
    public double Administration { get; private set; }
    public double Diplomacy { get; private set; }
    public double Treasury { get; private set; }
    public int Nobles { get; private set; }
    public double MonumentEnvironment { get; private set; }
    public int Divisions { get; private set; }
    public int Recruits { get; private set; }
    public int LoadedArtillery { get; private set; }
    public SettlementStandingRuntime Standing { get; } = new();
    public SettlementStatRegistry Registry { get; } = new();
    private readonly Dictionary<ResourceKind, double> _storedPerCapita = new();
    private readonly Dictionary<string, double> _serviceAccess = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _religionFollowers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _templeAccess = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _templeQuality = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _shrineAccess = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _shrineQuality = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string Race, SocialClass Class), double> _law = new();
    private readonly Dictionary<(string Race, SocialClass Class), double> _tyranny = new();

    public void Tick(double delta, int secondsPerDay, CitizenSystem citizens, ResourceLedger resources,
        RoomSystem rooms, CorpseRuntime corpses)
    {
        _historyDayLeft -= delta;
        _updateLeft -= delta;
        if (_updateLeft > 0) return;
        _updateLeft += Math.Max(1, secondsPerDay / 16.0);
        Population = citizens.Count;
        Hunger = citizens.AverageHunger;
        Starving = citizens.StarvingCount;
        var dailyFood = Math.Max(0.5, Population * 0.5);
        var edibleStored = EdibleResources.Sum(resources.Get) +
                           rooms.FoodVenues.Instances.Where(value =>
                               value.RoomKey != "TAVERN_NORMAL").Sum(value => value.Stock);
        FoodDays = Math.Clamp(edibleStored / dailyFood, 0, 24);
        // StatsEnv.UNBURRIED is 40 × corpses-created-today / (1 + population).
        Unburied = corpses.UnburiedPressure(Population);
        BurialAccess = Population <= 0 ? 0 : Math.Clamp(rooms.Burials.AvailableFormal / (double)Population, 0, 1);
        BurialDisturbance = rooms.Burials.Disturbance;
        _storedPerCapita.Clear();
        foreach (var resource in Enum.GetValues<ResourceKind>())
            _storedPerCapita[resource] = Population <= 0 ? 0 : resources.Get(resource) / (double)Population;
        _serviceAccess.Clear();
        foreach (var need in new[] { "BATH", "WELL", "HEARTH", "DOCTOR", "GROOMING", "MASSAGE", "TEMPLE", "SHRINE", "THIRST" })
            _serviceAccess[need] = citizens.AverageServiceAccess(need);
        _religionFollowers.Clear();
        _templeAccess.Clear();
        _templeQuality.Clear();
        _shrineAccess.Clear();
        _shrineQuality.Clear();
        foreach (var pair in citizens.ReligionFollowers())
        {
            _religionFollowers[pair.Key] = pair.Value;
            _templeAccess[pair.Key] = citizens.ReligionServiceAccess(pair.Key, "TEMPLE");
            _templeQuality[pair.Key] = citizens.ReligionServiceQuality(pair.Key, "TEMPLE");
            _shrineAccess[pair.Key] = citizens.ReligionServiceAccess(pair.Key, "SHRINE");
            _shrineQuality[pair.Key] = citizens.ReligionServiceQuality(pair.Key, "SHRINE");
        }
        ReligiousOpposition = citizens.ReligiousOpposition();
        Housed = rooms.Housing.Occupants;
        HousingCapacity = rooms.Housing.Capacity;
        HousingAccess = Population <= 0 ? 1 : Math.Clamp(Housed / (double)Population, 0, 1);
        AverageEducation = citizens.PersonalStats.AverageEducation;
        AverageCombatExperience = citizens.PersonalStats.AverageCombatExperience;
        EnemyKills = citizens.PersonalStats.EnemyKills;
        Prisoners = rooms.Law.InCustody;
        _law.Clear();
        _tyranny.Clear();
        foreach (var race in OriginalGameData.Current.Races.Keys)
        foreach (var socialClass in new[] { SocialClass.Citizen, SocialClass.Slave })
        {
            _law[(race, socialClass)] = rooms.Law.Law(race, socialClass);
            _tyranny[(race, socialClass)] = rooms.Law.Tyranny(race, socialClass);
        }
        Administration = rooms.Governance.Administration;
        Diplomacy = rooms.Governance.Diplomacy;
        Treasury = rooms.Governance.Treasury.Balance;
        Nobles = rooms.Governance.Nobility.Active.Count;
        MonumentEnvironment = rooms.Governance.Monuments.Count == 0 ? 0 :
            rooms.Governance.Monuments.Average(monument =>
                Math.Clamp((1.0 - monument.Degradation) * monument.Upgrade, 0, 1));
        Divisions = rooms.Military.Divisions.Count;
        Recruits = rooms.Military.Recruits;
        LoadedArtillery = rooms.Military.Artillery.Count(value => value.Loaded);
        Registry.Set("POPULATION", Population);
        Registry.Set("HUNGER", Hunger);
        Registry.Set("FOOD_DAYS", FoodDays);
        Registry.Set("HOUSING", HousingAccess);
        Registry.Set("EDUCATION", AverageEducation);
        Registry.Set("ADMINISTRATION", Administration);
        Registry.Set("TREASURY", Treasury);
        Registry.Set("SOCIAL", citizens.AverageSocialRelation);
        foreach (var group in citizens.PopulationByRaceAndClass())
            Registry.Set("POPULATION", group.Key.Race, group.Key.Class, group.Value);
        if (_historyDayLeft <= 0)
        {
            _historyDayLeft += Math.Max(1, secondsPerDay);
            Registry.BeginDay();
        }
        rooms.Governance.Progression.Tick(secondsPerDay / 16.0,
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["POPULATION"] = Population,
                ["POPULATION_CITIZEN"] = citizens.PopulationByClass().GetValueOrDefault(SocialClass.Citizen),
                ["POPULATION_SLAVE"] = citizens.PopulationByClass().GetValueOrDefault(SocialClass.Slave),
                ["POPULATION_KINGDOM"] = Population,
                ["FOOD_FOOD_DAYS"] = FoodDays,
                ["HOME_HOUSED"] = HousingAccess,
                ["EDUCATION_EDUCATION"] = AverageEducation,
                ["COUNT_EXECUTIONS_GAME"] = citizens.LeaveCounts.GetValueOrDefault(LeaveCause.Executed)
            });
        UpdateStanding(secondsPerDay / 16.0, citizens, resources, rooms);
    }

    private void UpdateStanding(double delta, CitizenSystem citizens, ResourceLedger resources, RoomSystem rooms)
    {
        var byGroup = citizens.PopulationByRaceAndClass();
        foreach (var race in OriginalGameData.Current.Races.Values)
        foreach (var socialClass in new[] { SocialClass.Citizen, SocialClass.Slave })
        {
            var population = byGroup.GetValueOrDefault((race.Key, socialClass));
            var contributions = race.StandingRules.Values.Select(rule =>
                SettlementStandingRuntime.FromRaceRule(rule, socialClass,
                    StandingInput(rule.Key, race.Key, socialClass, resources, citizens, rooms, Unburied)));
            Standing.Update(race, socialClass, population, citizens.Count,
                race.Key.Equals("HUMAN", StringComparison.OrdinalIgnoreCase), contributions,
                delta, OriginalGameData.Current.SecondsPerDay);
        }
    }

    private static double StandingInput(
        string key, string race, SocialClass socialClass, ResourceLedger resources,
        CitizenSystem citizens, RoomSystem rooms, double unburied)
    {
        var population = citizens.Count;
        if (key.StartsWith("STORED_", StringComparison.OrdinalIgnoreCase) &&
            OriginalGameData.TryMapResource(key["STORED_".Length..], out var resource))
            return population == 0 ? (resources.Get(resource) > 0 ? 1 : 0) :
                resources.Get(resource) / (double)population;
        if (key.Equals("HOME_FURNITURE", StringComparison.OrdinalIgnoreCase))
            return citizens.AverageHomeFurniture(race, socialClass);
        if (key.Equals("HOME_HOUSED", StringComparison.OrdinalIgnoreCase))
            return citizens.HousingAccess(race, socialClass);
        if (key.StartsWith("SERVICE_", StringComparison.OrdinalIgnoreCase))
            return citizens.AverageRoomServiceAccess(
                key["SERVICE_".Length..], race, socialClass);
        if (key.Equals("FOOD_STARVATION", StringComparison.OrdinalIgnoreCase))
            return citizens.StarvingFraction(race, socialClass);
        if (key.Equals("ENVIRONMENT_UNBURRIED", StringComparison.OrdinalIgnoreCase))
            return unburied;
        return 0;
    }
}
