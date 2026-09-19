using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Data;

public sealed record StructureRule(string Key, string Resource, int ResourceAmount, double BuildTime);
public sealed record FloorRule(string Key, string Resource, int ResourceAmount, int Speed, double Durability);
public sealed record IndustryAmount(string Resource, double Rate);
public sealed record MinableRule(
    string Key,
    string Resource,
    bool OnEveryMap,
    double FertilityIncrease,
    IReadOnlyDictionary<string, double> Terrain);
public sealed record GrowableRule(
    string Key,
    string Resource,
    double SeasonalOffset,
    double GrowthValue,
    IReadOnlyDictionary<string, double> ClimateBonus);
public sealed record LandingResourceRule(ResourceKind Resource, int Amount);
public sealed record ClimateRule(
    string Key, double SeasonalChange, double TempCold, double TempWarm, double Fertility);
public sealed record AnimalRule(
    string Key, double Mass, IReadOnlyList<string> Resources, IReadOnlyList<double> ResourceAmounts);
public sealed record SpecialProductionRule(
    string Kind, string Animal, double AnimalMass, string ExtraResource, int ExtraResourceAmount,
    int DaysTillGrowth, double RipeAtPartOfYear, int MaximumEmployed);
public sealed record LogisticsRule(
    string Kind, int DefaultRadius, int PullOrders, int CrateCapacity,
    int MaximumLoad, int WorkersPerCrate, double NeededWorkersPerCrate);
public sealed record KnowledgeRoomRule(
    string Kind, string Currency, double ValuePerWorker, double DegradePerYear,
    double WorkSpeed, double LearningSpeed,
    IReadOnlyDictionary<string, double> ConsumptionRates,
    IReadOnlyDictionary<string, double> ConsumptionBoosts);
public sealed record TradeResourceRule(string Key, double PriceMultiplier, double PriceCap);
public sealed record TechnologyRequirementRule(string Technology, int Level);
public sealed record TechnologyRule(
    string Key, string Tree, string Name, string Description, int LevelMaximum,
    double LevelCostIncrease, double AiAmount,
    IReadOnlyDictionary<string, double> Costs,
    IReadOnlyList<TechnologyRequirementRule> Requirements,
    IReadOnlyList<string> Unlocks,
    IReadOnlyDictionary<string, double> Boosts);
public sealed record CrimeRule(
    string Key, SocialClass Class, double Freedom, double Law, bool Judged, bool Criminal);
public sealed record PunishmentRule(
    string Key, double Value, double Cruelty, double Mercy,
    IReadOnlySet<SocialClass> AvailableClasses);
public sealed record PlayerMilestoneRule(
    string Key, IReadOnlyDictionary<string, double> Requirements,
    IReadOnlyList<string> FactionUnlocks, IReadOnlyList<string> RegionUnlocks,
    IReadOnlyDictionary<string, double> Boosts);
public sealed record MilitaryRoomRule(
    string Kind, int FullTrainingDays, string ProjectileResource,
    double ReloadSeconds, int ProjectileStorage,
    IReadOnlyDictionary<string, double> Boosts);
public sealed record RaceRule(
    string Key, string Name, string Names, bool Playable,
    int Height, int Width, int BabyDays, int ChildDays, bool CorpseDecay, bool Sleeps,
    double DeathAgeMultiplier, double ReproductionAgeMultiplier, double ReproductionSpeedMultiplier,
    double SlavePrice, double SlavePriceRecovery, double PopulationMaximum, double PopulationGrowth,
    double RaidMercenary,
    string Home,
    IReadOnlyDictionary<ResourceKind, int> HarvestResources,
    IReadOnlyDictionary<SocialClass, IReadOnlyDictionary<string, int>> HomeFurniture,
    IReadOnlyList<string> PreferredFood, IReadOnlyList<string> PreferredDrink,
    IReadOnlyDictionary<string, double> WorkPreferences,
    IReadOnlyDictionary<string, double> OtherRacePreferences,
    IReadOnlyDictionary<string, double> PopulationClimate,
    IReadOnlyDictionary<string, double> PopulationTerrain,
    IReadOnlyDictionary<string, double> ReligionInclination,
    IReadOnlyDictionary<string, double> TraitChances,
    IReadOnlyDictionary<string, RaceStandingRule> StandingRules,
    IReadOnlyList<string> FirstNames, IReadOnlyList<string> Surnames,
    IReadOnlyList<string> RaiderNames,
    IReadOnlyList<string> PronounHe, IReadOnlyList<string> PronounHim,
    IReadOnlyList<string> PronounHis, IReadOnlyList<string> PronounHimself,
    IReadOnlyList<string> PronounChild);
public sealed record RaceStandingRule(
    string Key,
    IReadOnlyDictionary<SocialClass, double> Maximum,
    bool Inverted, double Multiplier, double Exponent, int Priority,
    bool Dismiss, bool Child);
public sealed record DiseaseRule(
    string Key,
    bool Epidemic,
    bool Regular,
    int IncubationDays,
    double FatalityRate,
    double Spread,
    int InfectionDays);
public sealed record IndustryRecipe(IReadOnlyList<IndustryAmount> Inputs, IReadOnlyList<IndustryAmount> Outputs);
public sealed record RoomWorkRule(double ShiftOffset, bool NightShift, double Fulfillment, double AccidentsPerYear);
public enum RoomArchetype : byte
{
    Infrastructure,
    Industry,
    Agriculture,
    Extraction,
    Service,
    Military,
    Civic
}
public sealed record RoomServiceRule(
    string Need,
    int Radius,
    double DefaultAccess,
    double DefaultValue,
    double Usage,
    IReadOnlyDictionary<string, double> Standing,
    IReadOnlyDictionary<string, double> Boosts);
public sealed record ReligionRule(
    string Key,
    double DefaultSpread,
    IReadOnlyDictionary<string, double> Opposition,
    IReadOnlyDictionary<string, double> Boosts);
public sealed record TempleSacrificeRule(string Type, string Resource, double Time);
public sealed record RoomConstructionRule(
    IReadOnlyList<string> Resources,
    IReadOnlyList<double> AreaCosts,
    IReadOnlyList<string> Floors,
    bool Indoors,
    int ItemVariants,
    int UpgradeLevels);
public sealed record RoomUpgradeRule(
    IReadOnlyList<double> ResourceMask,
    double Boost,
    double Ai);
public sealed record FurnisherItemGroupRule(
    IReadOnlyList<double> Costs,
    IReadOnlyList<double> Stats);
public sealed record WorkEquipmentRule(
    string Resource,
    double WearPerDay,
    int DefaultTarget,
    double MaximumBoost,
    bool Multiplicative,
    IReadOnlyDictionary<string, int> Amounts);
public sealed record RoomEnvironmentEmitRule(string Key, double Value, double Radius);
public sealed record RoomRule(
    string Key,
    string Name,
    IReadOnlyList<IndustryRecipe> Recipes,
    int Storage,
    RoomWorkRule Work,
    bool HasWork,
    RoomArchetype Archetype,
    RoomConstructionRule Construction,
    IReadOnlyList<FurnisherItemGroupRule> FurnisherItems,
    IReadOnlyList<RoomUpgradeRule> Upgrades,
    RoomServiceRule? Service,
    string Religion,
    TempleSacrificeRule? Sacrifice,
    string Growable,
    string Minable,
    double YieldWorkerDaily,
    double DegradeRate,
    SpecialProductionRule? SpecialProduction,
    LogisticsRule? Logistics,
    KnowledgeRoomRule? Knowledge,
    IReadOnlyDictionary<string, RoomEnvironmentEmitRule> EnvironmentEmits)
{
    public bool HasIndustry => Recipes.Count > 0;
    public bool IsService => Service is not null;
}
public sealed record RuntimeRecipe(
    string RoomKey,
    string Name,
    IReadOnlyDictionary<ResourceKind, double> Inputs,
    IReadOnlyDictionary<ResourceKind, double> Outputs,
    double SourceWorkSeconds,
    double WorkFactor)
{
    public ResourceKind PrimaryOutput => Outputs.Keys.First();
    public double PrimaryOutputRate => Outputs[PrimaryOutput];
    // RoomResDeposit requires at least one of every input at the workstation.
    // Reserve enough for the largest integer crossing possible in one cycle.
    public IReadOnlyDictionary<ResourceKind, int> ReservationInputs => Inputs.ToDictionary(
        pair => pair.Key,
        pair => System.Math.Max(1, (int)System.Math.Ceiling(pair.Value * WorkFactor)));

    public int MaximumOutput(ResourceKind resource) => System.Math.Max(
        1, (int)System.Math.Ceiling(Outputs.GetValueOrDefault(resource) * WorkFactor));
}

public sealed class OriginalGameData
{
    public static OriginalGameData Current { get; private set; } = new();

    public int SecondsPerHour { get; private set; } = 48;
    public int HoursPerDay { get; private set; } = 24;
    public int SecondsPerDay => SecondsPerHour * HoursPerDay;
    public int OriginalDimension { get; private set; } = 768;
    public double HungerRate { get; private set; } = 0.5;
    public IReadOnlyDictionary<string, double> NeedRates => _needRates;
    public IReadOnlyDictionary<string, DiseaseRule> Diseases => _diseases;
    public IReadOnlyDictionary<string, ReligionRule> Religions => _religions;
    public int RegularSicknessDayInterval { get; private set; } = 64;
    public IReadOnlyDictionary<string, StructureRule> Structures => _structures;
    public IReadOnlyDictionary<string, FloorRule> Floors => _floors;
    public IReadOnlyList<FloorRule> Roads => _floors.Values.OrderBy(rule => rule.Key).ToArray();
    public IReadOnlyDictionary<string, RoomRule> Rooms => _rooms;
    public IReadOnlyCollection<string> Resources => _resources;
    public IReadOnlyList<MinableRule> Minables => _minables;
    public IReadOnlyList<GrowableRule> Growables => _growables;
    public IReadOnlyList<LandingResourceRule> LandingResources => _landingResources;
    public IReadOnlyDictionary<string, ClimateRule> Climates => _climates;
    public IReadOnlyDictionary<string, AnimalRule> Animals => _animals;
    public IReadOnlyDictionary<string, TradeResourceRule> TradeResources => _tradeResources;
    public IReadOnlyDictionary<string, TechnologyRule> Technologies => _technologies;
    public IReadOnlyDictionary<string, CrimeRule> Crimes => _crimes;
    public IReadOnlyDictionary<string, PunishmentRule> Punishments => _punishments;
    public IReadOnlyList<string> NobleRankNames { get; private set; } = Array.Empty<string>();
    public IReadOnlyList<PlayerMilestoneRule> PlayerLevels => _playerLevels;
    public IReadOnlyDictionary<string, PlayerMilestoneRule> PlayerTitles => _playerTitles;
    public IReadOnlyDictionary<string, MilitaryRoomRule> MilitaryRooms => _militaryRooms;
    public IReadOnlyDictionary<string, RaceRule> Races => _races;
    public IReadOnlyList<string> Warnings => _warnings;
    public WorkEquipmentRule? WorkEquipment { get; private set; }

    private readonly Dictionary<string, StructureRule> _structures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FloorRule> _floors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RoomRule> _rooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _resources = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MinableRule> _minables = new();
    private readonly List<GrowableRule> _growables = new();
    private readonly List<LandingResourceRule> _landingResources = new();
    private readonly Dictionary<string, ClimateRule> _climates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AnimalRule> _animals = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TradeResourceRule> _tradeResources = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TechnologyRule> _technologies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CrimeRule> _crimes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PunishmentRule> _punishments = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<PlayerMilestoneRule> _playerLevels = new();
    private readonly Dictionary<string, PlayerMilestoneRule> _playerTitles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MilitaryRoomRule> _militaryRooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RaceRule> _races = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _warnings = new();
    private readonly Dictionary<string, double> _needRates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DiseaseRule> _diseases = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ReligionRule> _religions = new(StringComparer.OrdinalIgnoreCase);

    public static OriginalGameData Load()
    {
        var result = new OriginalGameData();
        var root = ProjectSettings.GlobalizePath("res://Data/Original");
        result.LoadConfig(Path.Combine(root, "init", "config", "Sett.txt"));
        result.LoadClimates(Path.Combine(root, "init", "config", "CLIMATE.txt"));
        result.LoadLaw(Path.Combine(root, "init", "config", "LAW.txt"));
        result.LoadPlayerProgression(Path.Combine(root, "init", "player"));
        result.NobleRankNames = ReadTextArray(result.ParseFile(
            Path.Combine(root, "text", "player", "noble", "_RANKS.txt"), false)?.Get("RANKS"));
        result.LoadAnimals(Path.Combine(root, "init", "animal"));
        result.LoadRaces(Path.Combine(root, "init", "race"), Path.Combine(root, "text"));
        result.LoadNeeds(Path.Combine(root, "init", "stats", "need"));
        result.LoadDiseases(Path.Combine(root, "init", "disease"));
        result.LoadReligions(Path.Combine(root, "init", "religion"));
        result.LoadResources(Path.Combine(root, "init", "resource"));
        result.LoadLandingParty(Path.Combine(root, "init", "config", "LandingParty.txt"));
        result.LoadTechnologies(Path.Combine(root, "init", "tech"), Path.Combine(root, "text", "tech"));
        result.LoadMinables(Path.Combine(root, "init", "resource", "minable"));
        result.LoadGrowables(Path.Combine(root, "init", "resource", "growable"));
        result.LoadWorkEquipment(Path.Combine(root, "init", "resource", "work", "TOOL.txt"));
        result.LoadStructures(Path.Combine(root, "init", "settlement", "structure"));
        result.LoadFloors(Path.Combine(root, "init", "settlement", "floor"));
        result.LoadRooms(Path.Combine(root, "init", "room"), Path.Combine(root, "text", "room"));
        Current = result;
        GD.Print($"Original data: {result._resources.Count} resources, {result._structures.Count} structures, " +
                 $"{result._floors.Count} floors, {result._rooms.Count} rooms, " +
                 $"{result._races.Count} races, {result._religions.Count} religions, " +
                 $"{result._warnings.Count} warnings");
        if (!result._races.ContainsKey("HUMAN"))
        {
            var raceWarnings = string.Join(" | ", result._warnings.Where(value =>
                value.Contains("HUMAN", StringComparison.OrdinalIgnoreCase)));
            throw new InvalidDataException(
                $"Required source race HUMAN was not loaded from '{root}'. {raceWarnings}");
        }
        return result;
    }

    private void LoadLandingParty(string path)
    {
        var resources = ParseFile(path)?.Get("RESOURCES")?.Fields ??
            throw new InvalidDataException($"LandingParty.txt has no RESOURCES object: '{path}'");
        foreach (var pair in resources)
        {
            if (!Enum.TryParse<ResourceKind>(pair.Key.TrimStart('_'), true, out var resource))
            {
                _warnings.Add($"LandingParty resource '{pair.Key}' is not represented by ResourceKind.");
                continue;
            }
            var amount = pair.Value.Integer();
            if (amount is < 1 or > 500)
                throw new InvalidDataException(
                    $"LandingParty amount for '{pair.Key}' must be in the original 1..500 range.");
            _landingResources.Add(new LandingResourceRule(resource, amount));
        }
        if (_landingResources.Count == 0)
            throw new InvalidDataException($"LandingParty.txt defines no usable resources: '{path}'");
        if (_landingResources.Count > 10)
            throw new InvalidDataException("The original landing grid supports at most ten resource stacks.");
    }

    public StructureRule Structure(string key) => _structures.TryGetValue(key, out var rule)
        ? rule
        : new StructureRule(key, "STONE", 1, 0.1);

    public FloorRule DefaultRoad()
    {
        var defaultFile = ParseFile(Path.Combine(ProjectSettings.GlobalizePath("res://Data/Original/init/settlement/floor"), "_DEFAULT_ROAD.txt"));
        var key = defaultFile?.Get("FILE")?.Text("DIRT") ?? "DIRT";
        return _floors.TryGetValue(key, out var rule) ? rule : new FloorRule("DIRT", "", 0, 1, 0.2);
    }

    public FloorRule Floor(string key) => _floors.TryGetValue(key, out var rule)
        ? rule
        : DefaultRoad();

    public RoomRule? Room(string key) => _rooms.GetValueOrDefault(key);

    public int MinableIndex(string resource)
    {
        for (var index = 0; index < _minables.Count; index++)
            if (_minables[index].Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)) return index;
        return -1;
    }

    public int MaximumToolsPerWorker(string roomKey)
    {
        if (WorkEquipment is null) return 0;
        foreach (var pair in WorkEquipment.Amounts)
        {
            var pattern = pair.Key;
            if (pattern.EndsWith('*') && roomKey.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase))
                return pair.Value;
            if (pattern.Equals(roomKey, StringComparison.OrdinalIgnoreCase)) return pair.Value;
        }
        return 0;
    }

    public TradeResourceRule TradeResource(string key) =>
        _tradeResources.GetValueOrDefault(key.TrimStart('_')) ??
        new TradeResourceRule(key.TrimStart('_'), 1.0, 0);

    public TechnologyRule? Technology(string key) =>
        _technologies.GetValueOrDefault(key);

    public CrimeRule? Crime(string key) => _crimes.GetValueOrDefault(key);
    public PunishmentRule? Punishment(string key) => _punishments.GetValueOrDefault(key);

    public RuntimeRecipe? RuntimeRecipe(string roomKey, int recipeIndex = 0)
    {
        var room = Room(roomKey);
        if (room is null || recipeIndex < 0) return null;
        IndustryRecipe recipe;
        if (recipeIndex < room.Recipes.Count)
            recipe = room.Recipes[recipeIndex];
        else if (recipeIndex == 0 && room.Recipes.Count == 0 &&
                 !string.IsNullOrWhiteSpace(room.Minable) && room.YieldWorkerDaily > 0)
            recipe = new IndustryRecipe(
                Array.Empty<IndustryAmount>(),
                new[] { new IndustryAmount(room.Minable, room.YieldWorkerDaily) });
        else return null;
        if (recipe.Outputs.Count == 0) return null;
        var inputs = new Dictionary<ResourceKind, double>();
        foreach (var input in recipe.Inputs)
        {
            if (!TryMapResource(input.Resource, out var kind)) return null;
            inputs[kind] = input.Rate;
        }
        var outputs = new Dictionary<ResourceKind, double>();
        foreach (var output in recipe.Outputs)
        {
            if (!TryMapResource(output.Resource, out var outputKind)) return null;
            outputs[outputKind] = output.Rate;
        }
        var workHours = HoursPerDay * 8 / 16;
        var workPerDayInverse = workHours > 0 ? (double)HoursPerDay / workHours : 1;
        var workSeconds = room.SpecialProduction?.Kind switch
        {
            "PASTURE" => 20.0,
            "FISHERY" => 60.0,
            "WOODCUTTER" => 60.0,
            _ when room.Key.StartsWith("FARM_", StringComparison.OrdinalIgnoreCase) => 4.0,
            _ => 45.0
        };
        return new RuntimeRecipe(
            roomKey,
            room.Name,
            inputs,
            outputs,
            workSeconds,
            SecondsPerDay > 0 ? workPerDayInverse * workSeconds / SecondsPerDay : 0);
    }

    public static bool TryMapResource(string sourceKey, out ResourceKind kind)
    {
        switch (sourceKey.TrimStart('_').ToUpperInvariant())
        {
            case "WOOD": kind = ResourceKind.Wood; return true;
            case "STONE": kind = ResourceKind.Stone; return true;
            case "STONE_CUT": kind = ResourceKind.CutStone; return true;
            case "GRAIN": kind = ResourceKind.Grain; return true;
            case "BREAD": kind = ResourceKind.Food; return true;
            case "TOOL": kind = ResourceKind.Tools; return true;
            case "FURNITURE": kind = ResourceKind.Furniture; return true;
            case "ALCO_BEER": kind = ResourceKind.Beer; return true;
            case "ALCO_WINE": kind = ResourceKind.Wine; return true;
            case "ARMOUR_LEATHER": kind = ResourceKind.ArmourLeather; return true;
            case "ARMOUR_PLATE": kind = ResourceKind.ArmourPlate; return true;
            case "BOW": kind = ResourceKind.Bow; return true;
            case "CLAY": kind = ResourceKind.Clay; return true;
            case "CLOTHES": kind = ResourceKind.Clothes; return true;
            case "COAL": kind = ResourceKind.Coal; return true;
            case "COTTON": kind = ResourceKind.Cotton; return true;
            case "EGG": kind = ResourceKind.Egg; return true;
            case "FABRIC": kind = ResourceKind.Fabric; return true;
            case "FISH": kind = ResourceKind.Fish; return true;
            case "FRUIT": kind = ResourceKind.Fruit; return true;
            case "GEM": kind = ResourceKind.Gem; return true;
            case "HERB": kind = ResourceKind.Herb; return true;
            case "JEWELRY": kind = ResourceKind.Jewelry; return true;
            case "LEATHER": kind = ResourceKind.Leather; return true;
            case "MACHINERY": kind = ResourceKind.Machinery; return true;
            case "MEAT": kind = ResourceKind.Meat; return true;
            case "METAL": kind = ResourceKind.Metal; return true;
            case "MUSHROOM": kind = ResourceKind.Mushroom; return true;
            case "OPIATES": kind = ResourceKind.Opiates; return true;
            case "ORE": kind = ResourceKind.Ore; return true;
            case "PAPER": kind = ResourceKind.Paper; return true;
            case "POTTERY": kind = ResourceKind.Pottery; return true;
            case "RATION": kind = ResourceKind.Ration; return true;
            case "SITHILON": kind = ResourceKind.Sithilon; return true;
            case "VEGETABLE": kind = ResourceKind.Vegetable; return true;
            case "WEAPON_HAMMER": kind = ResourceKind.WeaponHammer; return true;
            case "WEAPON_MOUNT": kind = ResourceKind.WeaponMount; return true;
            case "WEAPON_SHIELD": kind = ResourceKind.WeaponShield; return true;
            case "WEAPON_SHORT": kind = ResourceKind.WeaponShort; return true;
            case "WEAPON_SLASH": kind = ResourceKind.WeaponSlash; return true;
            case "WEAPON_SPEAR": kind = ResourceKind.WeaponSpear; return true;
            case "LIVESTOCK": kind = ResourceKind.Livestock; return true;
            default: kind = default; return false;
        }
    }

    private void LoadConfig(string path)
    {
        var root = ParseFile(path);
        if (root is null) return;
        SecondsPerHour = root.Get("SECONDS_PER_HOUR")?.Integer(SecondsPerHour) ?? SecondsPerHour;
        HoursPerDay = root.Get("HOURS_PER_DAY")?.Integer(HoursPerDay) ?? HoursPerDay;
        OriginalDimension = root.Get("DIMENSION")?.Integer(OriginalDimension) ?? OriginalDimension;
    }

    private void LoadLaw(string path)
    {
        if (!File.Exists(path)) return;
        var root = SyxDataParser.Parse(File.ReadAllText(path));
        foreach (var pair in root.Get("CRIMES")?.Fields ?? new Dictionary<string, SyxDataNode>())
        {
            var slave = pair.Key.StartsWith("S_", StringComparison.OrdinalIgnoreCase);
            var socialClass = pair.Key == "WAR" ? SocialClass.Other :
                slave ? SocialClass.Slave : SocialClass.Citizen;
            var criminal = !pair.Key.Equals("PLEASURE", StringComparison.OrdinalIgnoreCase) &&
                           !pair.Key.Equals("S_PLEASURE", StringComparison.OrdinalIgnoreCase);
            _crimes[pair.Key] = new CrimeRule(
                pair.Key, socialClass,
                pair.Value.Get("FREEDOM")?.Number() ?? 0,
                pair.Value.Get("LAW")?.Number() ?? 0,
                pair.Key != "WAR", criminal);
        }
        var availability = new Dictionary<string, SocialClass[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["PARDON"] = new[] { SocialClass.Citizen, SocialClass.Slave, SocialClass.Other },
            ["NONE"] = new[] { SocialClass.Citizen, SocialClass.Slave, SocialClass.Other },
            ["BANISH"] = new[] { SocialClass.Citizen, SocialClass.Slave, SocialClass.Other },
            ["PRISON"] = new[] { SocialClass.Citizen, SocialClass.Slave },
            ["EXECUTE"] = new[] { SocialClass.Citizen, SocialClass.Slave, SocialClass.Other },
            ["HARVEST"] = new[] { SocialClass.Citizen, SocialClass.Slave, SocialClass.Other },
            ["ENSLAVE"] = new[] { SocialClass.Citizen, SocialClass.Other }
        };
        foreach (var pair in root.Get("PUNISHMENTS")?.Fields ?? new Dictionary<string, SyxDataNode>())
            _punishments[pair.Key] = new PunishmentRule(
                pair.Key, pair.Value.Get("VALUE")?.Number() ?? 0,
                pair.Value.Get("CRUELTY")?.Number() ?? 0,
                pair.Value.Get("MERCY")?.Number() ?? 0,
                new HashSet<SocialClass>(availability.GetValueOrDefault(pair.Key) ?? Array.Empty<SocialClass>()));
    }

    private void LoadPlayerProgression(string path)
    {
        foreach (var file in Directory.Exists(Path.Combine(path, "level"))
                     ? Directory.EnumerateFiles(Path.Combine(path, "level"), "*.txt").OrderBy(value => value)
                     : Enumerable.Empty<string>())
        {
            var data = ParseFile(file);
            if (data is not null) _playerLevels.Add(ReadMilestone(Path.GetFileNameWithoutExtension(file), data));
        }
        foreach (var file in Directory.Exists(Path.Combine(path, "titles"))
                     ? Directory.EnumerateFiles(Path.Combine(path, "titles"), "*.txt").OrderBy(value => value)
                     : Enumerable.Empty<string>())
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var milestone = ReadMilestone(Path.GetFileNameWithoutExtension(file), data);
            _playerTitles[milestone.Key] = milestone;
        }
    }

    private static PlayerMilestoneRule ReadMilestone(string key, SyxDataNode data)
    {
        var requirements = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        FlattenNumbers(data.Get("REQUIRES"), "", requirements);
        return new PlayerMilestoneRule(key, requirements,
            ReadTextArray(data.Get("UNLOCKS_FACTION")),
            ReadTextArray(data.Get("UNLOCKS_REGION")),
            ReadNumberFields(data.Get("BOOST")));
    }

    private static void FlattenNumbers(
        SyxDataNode? node, string prefix, IDictionary<string, double> output)
    {
        if (node?.Fields is null) return;
        foreach (var pair in node.Fields)
        {
            var key = prefix.Length == 0 ? pair.Key : prefix + ">" + pair.Key;
            if (pair.Value.Fields is not null) FlattenNumbers(pair.Value, key, output);
            else output[key] = pair.Value.Number();
        }
    }

    private void LoadNeeds(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var key = Path.GetFileNameWithoutExtension(file).TrimStart('_');
            var rate = ParseFile(file)?.Get("RATE")?.Number() ?? 0;
            _needRates[key] = rate;
        }
        HungerRate = _needRates.GetValueOrDefault("HUNGER", HungerRate);
    }

    private void LoadDiseases(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var key = Path.GetFileNameWithoutExtension(file);
            var data = ParseFile(file);
            if (data is null) continue;
            if (key.Equals("_CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                RegularSicknessDayInterval = data.Get("REGULAR_SICKNESS_DAY_INVERVAL")?
                    .Integer(RegularSicknessDayInterval) ?? RegularSicknessDayInterval;
                continue;
            }
            _diseases[key] = new DiseaseRule(
                key,
                data.Get("EPIDEMIC")?.Boolean() ?? false,
                data.Get("REGULAR")?.Boolean() ?? false,
                data.Get("INCUBATION_DAYS")?.Integer(1) ?? 1,
                data.Get("FATALITY_RATE")?.Number() ?? 0,
                data.Get("SPREAD")?.Number() ?? 0,
                data.Get("INFECTION_DAYS")?.Integer(1) ?? 1);
        }
    }

    private void LoadResources(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt", SearchOption.TopDirectoryOnly))
        {
            var key = Path.GetFileNameWithoutExtension(file).TrimStart('_');
            _resources.Add(key);
            var data = ParseFile(file);
            _tradeResources[key] = new TradeResourceRule(
                key,
                data?.Get("PRICE_MUL")?.Number(1) ?? 1,
                data?.Get("PRICE_CAP")?.Number() ?? 0);
        }
    }

    private void LoadMinables(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt").OrderBy(value => value))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var terrain = data.Get("TERRAIN")?.Fields?.ToDictionary(
                pair => pair.Key.ToUpperInvariant(), pair => pair.Value.Number(),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, double>();
            _minables.Add(new MinableRule(
                Path.GetFileNameWithoutExtension(file),
                data.Get("RESOURCE")?.Text() ?? "",
                data.Get("ON_EVERY_MAP")?.Boolean() ?? false,
                data.Get("FERTILITY_INCREASE")?.Number() ?? 0,
                terrain));
        }
    }

    private void LoadGrowables(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt").OrderBy(value => value))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var climate = data.Get("CLIMATE_BONUS")?.Fields?.ToDictionary(
                pair => pair.Key.ToUpperInvariant(), pair => pair.Value.Number(),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, double>();
            _growables.Add(new GrowableRule(
                Path.GetFileNameWithoutExtension(file),
                data.Get("RESOURCE")?.Text() ?? "",
                data.Get("SEASONAL_OFFSET")?.Number() ?? 0,
                data.Get("GROWTH_VALUE")?.Number() ?? 0,
                climate));
        }
    }

    private void LoadClimates(string path)
    {
        var data = ParseFile(path);
        if (data?.Fields is null) return;
        foreach (var key in new[] { "COLD", "TEMPERATE", "HOT" })
        {
            var climate = data.Get(key);
            if (climate is null) continue;
            _climates[key] = new ClimateRule(
                key,
                climate.Get("SEASONAL_CHANGE")?.Number() ?? 0,
                climate.Get("TEMP_COLD")?.Number() ?? 0,
                climate.Get("TEMP_WARM")?.Number() ?? 0,
                climate.Get("FERTILITY")?.Number() ?? 0);
        }
    }

    private void LoadRaces(string initRoot, string textRoot)
    {
        if (!Directory.Exists(initRoot)) return;
        foreach (var file in Directory.EnumerateFiles(initRoot, "*.txt").OrderBy(value => value))
        {
            var data = ParseFile(file);
            var key = Path.GetFileNameWithoutExtension(file);
            var text = ParseFile(Path.Combine(textRoot, "race", key + ".txt"));
            var sprite = ParseFile(Path.Combine(initRoot, "sprite", key + ".txt"), false);
            if (data is null || text is null) continue;
            var properties = data.Get("PROPERTIES");
            var population = data.Get("POPULATION");
            var boosts = data.Get("BOOST");
            var firstSet = FindScalar(sprite, "NAMESET_FILE_FIRST") ?? "StdFirst";
            var surnameSet = FindScalar(sprite, "NAMESET_FILE_SURNAME") ?? "StdLast";
            var homeKey = data.Get("HOME")?.Text(key) ?? key;
            _races[key] = new RaceRule(
                key, text.Get("NAME")?.Text(key) ?? key, text.Get("NAMES")?.Text(key) ?? key,
                data.Get("PLAYABLE")?.Boolean() ?? false,
                properties?.Get("HEIGHT")?.Integer() ?? 0, properties?.Get("WIDTH")?.Integer() ?? 0,
                properties?.Get("BABY_DAYS")?.Integer() ?? 0, properties?.Get("CHILD_DAYS")?.Integer() ?? 0,
                properties?.Get("CORPSE_DECAY")?.Boolean() ?? true, properties?.Get("SLEEPS")?.Boolean() ?? true,
                boosts?.Get("PHYSICS_DEATH_AGE>MUL")?.Number() ?? 1.0,
                boosts?.Get("PHYSICS_REPRODUCTION_AGE>MUL")?.Number() ?? 1.0,
                boosts?.Get("PHYSICS_REPRODUCTION_SPEED>MUL")?.Number() ?? 1.0,
                properties?.Get("SLAVE_PRICE")?.Number() ?? 0,
                properties?.Get("SLAVE_PRICE_RECOVERY")?.Number() ?? 0,
                population?.Get("MAX")?.Number() ?? 0, population?.Get("GROWTH")?.Number() ?? 0,
                properties?.Get("RAID_MERCINARY")?.Number() ?? 0,
                homeKey, ReadRaceResources(data.Get("RESOURCE")),
                ReadHomeFurniture(Path.Combine(initRoot, "home", homeKey + ".txt")),
                ReadTextArray(data.Get("PREFERRED")?.Get("FOOD")),
                ReadTextArray(data.Get("PREFERRED")?.Get("DRINK")),
                ReadNumberFields(data.Get("PREFERRED")?.Get("WORK")),
                ReadNumberFields(data.Get("PREFERRED")?.Get("OTHER_RACES")),
                ReadNumberFields(population?.Get("CLIMATE")), ReadNumberFields(population?.Get("TERRAIN")),
                ReadReligionInclination(boosts),
                ReadNumberFields(data.Get("TRAITS")),
                ReadStandingRules(data.Get("STATS")),
                LoadNameSet(textRoot, firstSet), LoadNameSet(textRoot, surnameSet),
                ReadTextArray(ParseFile(Path.Combine(textRoot, "race", "raider", "name",
                    (data.Get("RAIDER_NAME_FILE")?.Text("Normal") ?? "Normal") + ".txt"), false)?.Get("NAMES")),
                ReadTextArray(text.Get("PRONOUN_HE")), ReadTextArray(text.Get("PRONOUN_HIM")),
                ReadTextArray(text.Get("PRONOUN_HIS")), ReadTextArray(text.Get("PRONOUN_HIMSELF")),
                ReadTextArray(text.Get("PRONOUN_CHILD")));
        }
    }

    private static IReadOnlyDictionary<string, double> ReadReligionInclination(SyxDataNode? boosts)
    {
        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var common = 0.0;
        foreach (var pair in boosts?.Fields ?? new Dictionary<string, SyxDataNode>())
        {
            var key = pair.Key.ToUpperInvariant();
            if (key.StartsWith("RELIGION*", StringComparison.Ordinal)) common += pair.Value.Number();
            else if (key.StartsWith("RELIGION_", StringComparison.Ordinal))
            {
                var end = key.IndexOf('>');
                var religion = key["RELIGION_".Length..(end < 0 ? key.Length : end)].TrimEnd('*');
                if (religion.Length > 0) result[religion] = result.GetValueOrDefault(religion) + pair.Value.Number();
            }
        }
        result["*"] = common;
        return result;
    }

    private static IReadOnlyDictionary<ResourceKind, int> ReadRaceResources(SyxDataNode? node)
    {
        var result = new Dictionary<ResourceKind, int>();
        foreach (var pair in node?.Fields ?? new Dictionary<string, SyxDataNode>())
        {
            if (!TryMapResource(pair.Key, out var resource)) continue;
            var amount = pair.Value.Integer();
            if (amount > 0) result[resource] = amount;
        }
        return result;
    }

    private void LoadAnimals(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var key = Path.GetFileNameWithoutExtension(file);
            _animals[key] = new AnimalRule(key, data.Get("MASS")?.Number() ?? 0,
                ReadTextArray(data.Get("RESOURCES")), ReadNumberArray(data.Get("RESOURCE_AMOUNT")));
        }
    }

    private IReadOnlyDictionary<SocialClass, IReadOnlyDictionary<string, int>> ReadHomeFurniture(string path)
    {
        var data = ParseFile(path, false);
        var result = new Dictionary<SocialClass, IReadOnlyDictionary<string, int>>();
        foreach (var socialClass in new[] { SocialClass.Citizen, SocialClass.Noble, SocialClass.Slave })
        {
            var maxima = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var classNode = data?.Get(socialClass.ToString().ToUpperInvariant());
            if (classNode?.Fields is not null)
            {
                foreach (var definition in classNode.Fields.Values)
                foreach (var variant in definition.Items ?? Enumerable.Empty<SyxDataNode>())
                {
                    var resources = variant.Get("RESOURCES")?.Fields;
                    if (resources is null) continue;
                    foreach (var resource in resources)
                    {
                        if (!_resources.Contains(resource.Key) && !TryMapResource(resource.Key, out _)) continue;
                        maxima[resource.Key] = Math.Max(maxima.GetValueOrDefault(resource.Key),
                            Math.Clamp(resource.Value.Integer(1), 1, 15));
                    }
                }
            }
            if (maxima.Count > 8)
                _warnings.Add($"Home furniture '{Path.GetFileNameWithoutExtension(path)}/{socialClass}' has " +
                              $"{maxima.Count} resource kinds; source limit is 8");
            result[socialClass] = maxima.Take(8).ToDictionary(pair => pair.Key, pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
        }
        result[SocialClass.Other] = new Dictionary<string, int>();
        return result;
    }

    private IReadOnlyList<string> LoadNameSet(string textRoot, string key) =>
        ReadTextArray(ParseFile(Path.Combine(textRoot, "names", "nameset", key + ".txt"), false)?.Get("NAMES"));

    private static string? FindScalar(SyxDataNode? node, string key)
    {
        if (node is null) return null;
        if (node.Fields is not null && node.Fields.TryGetValue(key, out var found)) return found.Text();
        foreach (var child in (IEnumerable<SyxDataNode>?)node.Fields?.Values ?? Enumerable.Empty<SyxDataNode>())
        {
            var nested = FindScalar(child, key);
            if (!string.IsNullOrWhiteSpace(nested)) return nested;
        }
        if (node.Items is not null)
            foreach (var child in node.Items)
            {
                var nested = FindScalar(child, key);
                if (!string.IsNullOrWhiteSpace(nested)) return nested;
            }
        return null;
    }

    private void LoadStructures(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var key = Path.GetFileNameWithoutExtension(file);
            _structures[key] = new StructureRule(
                key,
                data.Get("RESOURCE")?.Text() ?? "",
                data.Get("RESOURCE_AMOUNT")?.Integer() ?? 0,
                data.Get("BUILD_TIME")?.Number() ?? 0);
        }
    }

    private void LoadWorkEquipment(string path)
    {
        var data = ParseFile(path);
        if (data is null) return;
        var amounts = data.Get("EQUIP_AMOUNTS")?.Fields?.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Integer()) ?? new Dictionary<string, int>();
        WorkEquipment = new WorkEquipmentRule(
            data.Get("RESOURCE")?.Text("TOOL") ?? "TOOL",
            data.Get("WEAR_PER_DAY")?.Number() ?? 0,
            data.Get("DEFAULT_TARGET")?.Integer() ?? 0,
            data.Get("BOOST_MAX_VALUE")?.Number() ?? 0,
            data.Get("BOOST_MUL")?.Boolean() ?? false,
            amounts);
    }

    private void LoadFloors(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var data = ParseFile(file);
            var road = data?.Get("ROAD");
            if (road is null) continue;
            var key = Path.GetFileNameWithoutExtension(file);
            _floors[key] = new FloorRule(
                key,
                road.Get("RESOURCE")?.Text() ?? "",
                road.Get("RESOURCE_AMOUNT")?.Integer() ?? 0,
                road.Get("SPEED")?.Integer() ?? 0,
                road.Get("DURABILITY")?.Number(1) ?? 1);
        }
    }

    private void LoadTechnologies(string initRoot, string textRoot)
    {
        if (!Directory.Exists(initRoot)) return;
        foreach (var path in Directory.EnumerateFiles(initRoot, "*.txt"))
        {
            var tree = Path.GetFileNameWithoutExtension(path);
            try
            {
                var data = SyxDataParser.Parse(File.ReadAllText(path));
                var textPath = Path.Combine(textRoot, tree + ".txt");
                var texts = File.Exists(textPath)
                    ? SyxDataParser.Parse(File.ReadAllText(textPath)).Get("TECHS")
                    : null;
                var definitions = data.Get("TECHS")?.Fields;
                if (definitions is null) continue;
                foreach (var pair in definitions)
                {
                    var key = tree + "_" + pair.Key;
                    var node = pair.Value;
                    var text = texts?.Get(pair.Key);
                    var requirements = new List<TechnologyRequirementRule>();
                    foreach (var requirement in node.Get("REQUIRES_TECH_LEVEL")?.Fields ??
                                 new Dictionary<string, SyxDataNode>())
                    {
                        var requiredKey = requirement.Key.Contains('_')
                            ? requirement.Key : tree + "_" + requirement.Key;
                        requirements.Add(new TechnologyRequirementRule(
                            requiredKey, Math.Max(0, requirement.Value.Integer())));
                    }
                    var unlocks = ReadTextArray(node.Get("UNLOCKS_FACTION"))
                        .Concat(ReadTextArray(node.Get("UNLOCKS_REGION"))).Distinct().ToArray();
                    _technologies[key] = new TechnologyRule(
                        key, tree,
                        text?.Get("NAME")?.Text(key) ?? key,
                        text?.Get("DESC")?.Text() ?? "",
                        Math.Clamp(node.Get("LEVEL_MAX")?.Integer(1) ?? 1, 1, 10000),
                        Math.Clamp(node.Get("LEVEL_COST_INC")?.Number() ?? 0, 0, 100000),
                        Math.Clamp(node.Get("AI_AMOUNT")?.Number(1) ?? 1, 0, 1),
                        ReadNumberFields(node.Get("COSTS")), requirements, unlocks,
                        ReadNumberFields(node.Get("BOOST")));
                }
            }
            catch (Exception error)
            {
                _warnings.Add($"Technology data {tree}: {error.Message}");
            }
        }
    }

    private void LoadRooms(string initPath, string textPath)
    {
        if (!Directory.Exists(initPath)) return;
        foreach (var file in Directory.EnumerateFiles(initPath, "*.txt"))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var key = Path.GetFileNameWithoutExtension(file);
            var text = ParseFile(Path.Combine(textPath, key + ".txt"), false);
            var name = text?.Get("INFO")?.Get("NAME")?.Text(key) ?? key;
            var recipes = new List<IndustryRecipe>();
            foreach (var wrapper in data.Get("INDUSTRIES")?.Items ?? Enumerable.Empty<SyxDataNode>())
            {
                var industry = wrapper.Get("INDUSTRY");
                if (industry is null) continue;
                recipes.Add(new IndustryRecipe(ReadAmounts(industry.Get("IN")), ReadAmounts(industry.Get("OUT"))));
            }
            var singleIndustry = data.Get("INDUSTRY");
            if (singleIndustry is not null)
                recipes.Add(new IndustryRecipe(
                    ReadAmounts(singleIndustry.Get("IN")), ReadAmounts(singleIndustry.Get("OUT"))));
            var work = data.Get("WORK");
            var service = data.Get("SERVICE");
            var animal = data.Get("ANIMAL")?.Text() ?? "";
            _rooms[key] = new RoomRule(
                key,
                name,
                recipes,
                data.Get("STORAGE")?.Integer() ?? 0,
                new RoomWorkRule(
                    work?.Get("SHIFT_OFFSET")?.Number() ?? 0,
                    work?.Get("NIGHT_SHIFT")?.Boolean() ?? false,
                    work?.Get("FULFILLMENT")?.Number(0.5) ?? 0.5,
                    (work?.Get("ACCIDENTS_PER_YEAR")?.Number() ?? 0) / 2.0),
                work is not null,
                ClassifyRoom(key, data),
                new RoomConstructionRule(
                    ReadTextArray(data.Get("RESOURCES")),
                    ReadNumberArray(data.Get("AREA_COSTS")),
                    ReadTextArray(data.Get("FLOOR")),
                    data.Get("INDOORS")?.Boolean(true) ?? true,
                    data.Get("ITEMS")?.Items?.Count ?? 0,
                    data.Get("UPGRADES")?.Items?.Count ?? 0),
                ReadFurnisherItems(data.Get("ITEMS")),
                ReadUpgrades(data.Get("UPGRADES")),
                service is null ? null : new RoomServiceRule(
                    ServiceNeed(key, service),
                    service.Get("RADIUS")?.Integer() ?? 150,
                    service.Get("DEFAULT_ACCESS")?.Number() ?? 0,
                    service.Get("DEFAULT_VALUE")?.Number() ?? 0,
                    service.Get("USAGE")?.Number() ?? 1,
                    ReadNumberFields(service.Get("STANDING")),
                    ReadNumberFields(service.Get("BOOST"))),
                data.Get("RELIGION")?.Text() ?? "",
                ReadTempleSacrifice(data),
                data.Get("GROWABLE")?.Text() ?? "",
                data.Get("MINABLE")?.Text() ?? "",
                data.Get("YEILD_WORKER_DAILY")?.Number() ??
                    data.Get("YIELD_WORKER_DAILY")?.Number() ?? 0,
                data.Get("DEGRADE_RATE")?.Number(0.75) ?? 0.75,
                ReadSpecialProduction(key, data, animal),
                ReadLogistics(key),
                ReadKnowledge(key, data),
                ReadEnvironmentEmits(data.Get("ENVIRONMENT_EMIT")));
            var military = ReadMilitary(key, data);
            if (military is not null) _militaryRooms[key] = military;
        }
    }

    private static MilitaryRoomRule? ReadMilitary(string key, SyxDataNode data)
    {
        var upper = key.ToUpperInvariant();
        var kind = upper.StartsWith("BARRACKS_") ? "BARRACKS" :
            upper.StartsWith("ARCHERY_") ? "ARCHERY" :
            upper.StartsWith("ARTILLERY_") ? "ARTILLERY" :
            upper == "_MILITARY_SUPPLY" ? "SUPPLY" : "";
        if (kind.Length == 0) return null;
        var training = data.Get("TRAINING");
        var projectile = data.Get("PROJECTILE");
        return new MilitaryRoomRule(kind,
            training?.Get("FULL_TRAINING_IN_DAYS")?.Integer() ??
                data.Get("FULL_TRAINING_IN_DAYS")?.Integer() ?? 0,
            data.Get("PROJECTILE_RESOURCE")?.Text() ?? "",
            projectile?.Get("FROM")?.Get("RELOAD_SECONDS")?.Number() ?? 0,
            kind == "ARTILLERY" ? 5 : 0,
            ReadNumberFields(training?.Get("BOOST")));
    }

    private static LogisticsRule? ReadLogistics(string key) => key.ToUpperInvariant() switch
    {
        "_STOCKPILE" => new LogisticsRule("STOCKPILE", 840, 4, 80, 0, 8, 0),
        "_HAULER" => new LogisticsRule("HAULER", 840, 2, 80, 0, 5, 1),
        "_TRANSPORT" => new LogisticsRule("TRANSPORT", 200, 4, 0, 400, 1, 1),
        "_EXPORT" => new LogisticsRule("EXPORT", 40, 4, 500, 0, 1, 0.05),
        "_IMPORT" => new LogisticsRule("IMPORT", 0, 0, 600, 0, 0, 0),
        "MARKET_NORMAL" => new LogisticsRule("MARKET", 100, 0, 0, 0, 1, 0.075),
        "_MILITARY_SUPPLY" => new LogisticsRule("MILITARY_SUPPLY", 300, 2, 80, 0, 4, 1),
        _ => null
    };

    private static KnowledgeRoomRule? ReadKnowledge(string key, SyxDataNode data)
    {
        var kind = key.ToUpperInvariant() switch
        {
            "LABORATORY_NORMAL" => "LABORATORY",
            "LIBRARY_NORMAL" => "LIBRARY",
            "UNIVERSITY_NORMAL" => "UNIVERSITY",
            "ADMIN_NORMAL" => "ADMINISTRATION",
            "_EMBASSY" => "DIPLOMACY",
            _ => ""
        };
        if (kind.Length == 0) return null;
        var rates = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var boosts = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in data.Get("CONSUMPTION")?.Fields ??
                     new Dictionary<string, SyxDataNode>())
        {
            rates[pair.Key] = pair.Value.Get("RATE")?.Number() ?? 0;
            boosts[pair.Key] = pair.Value.Get("BONUS")?.Number() ?? 0;
        }
        return new KnowledgeRoomRule(
            kind,
            data.Get("BOOST")?.Text() ?? (kind == "DIPLOMACY" ? "CIVIC_DIPLOMACY" : ""),
            data.Get("VALUE_PER_WORKER")?.Number() ?? 0,
            data.Get("VALUE_DEGRADE_PER_YEAR")?.Number() ?? 0,
            data.Get("VALUE_WORK_SPEED")?.Number() ?? 0,
            data.Get("LEARNING_SPEED")?.Number() ?? 0,
            rates, boosts);
    }

    private SpecialProductionRule? ReadSpecialProduction(string key, SyxDataNode data, string animal)
    {
        var kind = key.StartsWith("PASTURE_", StringComparison.OrdinalIgnoreCase) ? "PASTURE" :
            key.StartsWith("ORCHARD_", StringComparison.OrdinalIgnoreCase) ? "ORCHARD" :
            key.StartsWith("FISHERY_", StringComparison.OrdinalIgnoreCase) ? "FISHERY" :
            key.StartsWith("HUNTER_", StringComparison.OrdinalIgnoreCase) ? "HUNTER" :
            key.Equals("_WOODCUTTER", StringComparison.OrdinalIgnoreCase) ? "WOODCUTTER" : "";
        if (kind.Length == 0) return null;
        return new SpecialProductionRule(kind, animal,
            _animals.GetValueOrDefault(animal)?.Mass ?? 0,
            data.Get("EXTRA_RESOURCE")?.Text() ?? "",
            data.Get("EXTRA_RESOURCE_AMOUNT")?.Integer() ?? 0,
            data.Get("DAYS_TILL_GROWTH")?.Integer() ?? 0,
            data.Get("RIPE_AT_PART_OF_YEAR")?.Number() ?? 0,
            data.Get("MAX_EMPLOYED")?.Integer() ?? 0);
    }

    private void LoadReligions(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*.txt"))
        {
            var data = ParseFile(file);
            if (data is null) continue;
            var key = Path.GetFileNameWithoutExtension(file);
            _religions[key] = new ReligionRule(
                key,
                data.Get("DEFAULT_SPREAD")?.Number(1) ?? 1,
                ReadNumberFields(data.Get("OPPOSITION")),
                ReadNumberFields(data.Get("BOOST")));
        }
    }

    private static TempleSacrificeRule? ReadTempleSacrifice(SyxDataNode data)
    {
        var type = data.Get("SACRIFICE_TYPE")?.Text() ?? "";
        if (string.IsNullOrWhiteSpace(type)) return null;
        var resource = data.Get("SACRIFICE_RESOURCE")?.Text() ?? "";
        if (type.Equals("ANIMAL", StringComparison.OrdinalIgnoreCase)) resource = "LIVESTOCK";
        return new TempleSacrificeRule(
            type.ToUpperInvariant(), resource, data.Get("SACRIFICE_TIME")?.Number() ?? 0);
    }

    private static string ServiceNeed(string roomKey, SyxDataNode service)
    {
        var need = service.Get("NEED")?.Text() ?? "";
        if (!string.IsNullOrWhiteSpace(need)) return need.TrimStart('_');
        if (roomKey.StartsWith("TEMPLE_", StringComparison.OrdinalIgnoreCase)) return "TEMPLE";
        if (roomKey.StartsWith("SHRINE_", StringComparison.OrdinalIgnoreCase)) return "SHRINE";
        if (roomKey.StartsWith("CANTEEN_", StringComparison.OrdinalIgnoreCase) ||
            roomKey.StartsWith("EATERY_", StringComparison.OrdinalIgnoreCase)) return "HUNGER";
        if (roomKey.Equals("_HOSPITAL", StringComparison.OrdinalIgnoreCase)) return "HOSPITAL";
        if (roomKey.Equals("_DUMP_CORPSE", StringComparison.OrdinalIgnoreCase)) return "CORPSE";
        return "";
    }

    private static IReadOnlyList<IndustryAmount> ReadAmounts(SyxDataNode? node)
    {
        if (node?.Fields is null) return Array.Empty<IndustryAmount>();
        return node.Fields.Select(pair => new IndustryAmount(pair.Key, ReadRate(pair.Value))).ToArray();
    }

    private static double ReadRate(SyxDataNode node) =>
        node.Scalar is not null ? node.Number() : node.Get("PLAYER")?.Number() ?? 0;

    private static IReadOnlyList<string> ReadTextArray(SyxDataNode? node)
    {
        if (node?.Items is not null) return node.Items.Select(item => item.Text()).ToArray();
        var scalar = node?.Text();
        return string.IsNullOrWhiteSpace(scalar) ? Array.Empty<string>() : new[] { scalar };
    }

    private static IReadOnlyList<double> ReadNumberArray(SyxDataNode? node) =>
        node?.Items?.Select(item => item.Number()).ToArray() ?? Array.Empty<double>();

    private static IReadOnlyDictionary<string, double> ReadNumberFields(SyxDataNode? node) =>
        node?.Fields?.ToDictionary(pair => pair.Key, pair => pair.Value.Number()) ??
        new Dictionary<string, double>();

    private static IReadOnlyDictionary<string, RoomEnvironmentEmitRule> ReadEnvironmentEmits(
        SyxDataNode? node)
    {
        if (node?.Fields is null)
            return new Dictionary<string, RoomEnvironmentEmitRule>(StringComparer.OrdinalIgnoreCase);
        return node.Fields.ToDictionary(pair => pair.Key, pair => new RoomEnvironmentEmitRule(
            pair.Key, pair.Value.Get("VALUE")?.Number() ?? 0,
            pair.Value.Get("RADIUS")?.Number() ?? 0), StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, RaceStandingRule> ReadStandingRules(SyxDataNode? node)
    {
        if (node?.Fields is null) return new Dictionary<string, RaceStandingRule>();
        var result = new Dictionary<string, RaceStandingRule>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in node.Fields)
        {
            var value = pair.Value;
            var maximum = new Dictionary<SocialClass, double>
            {
                [SocialClass.Citizen] = value.Get("CITIZEN")?.Number() ?? 0,
                [SocialClass.Slave] = value.Get("SLAVE")?.Number() ?? 0,
                [SocialClass.Noble] = value.Get("NOBLE")?.Number() ?? 0,
                [SocialClass.Other] = value.Get("OTHER")?.Number() ?? 0
            };
            result.Add(pair.Key, new RaceStandingRule(pair.Key, maximum,
                value.Get("INVERTED")?.Boolean() ?? false,
                value.Get("MULTIPLIER")?.Number() ?? 1,
                value.Get("EXPONENT")?.Number(1) ?? 1,
                value.Get("PRIO")?.Integer(1) ?? 1,
                value.Get("DISMISS")?.Boolean() ?? false,
                value.Get("CHILD")?.Boolean() ?? false));
        }
        return result;
    }

    private static IReadOnlyList<RoomUpgradeRule> ReadUpgrades(SyxDataNode? node)
    {
        if (node?.Items is null)
            return new[] { new RoomUpgradeRule(new[] { 1.0 }, 1.0, 0.5) };
        var upgrades = node.Items.Select(item =>
        {
            var boost = item.Get("BOOST")?.Number() ?? 0;
            return new RoomUpgradeRule(
                ReadNumberArray(item.Get("RESOURCE_MASK")),
                boost,
                item.Get("AI")?.Number(boost * 0.5) ?? boost * 0.5);
        }).ToArray();
        var width = upgrades.Select(upgrade => upgrade.ResourceMask.Count).DefaultIfEmpty(1).Max();
        return upgrades.Select(upgrade => upgrade.ResourceMask.Count >= width
            ? upgrade
            : upgrade with
            {
                ResourceMask = upgrade.ResourceMask.Concat(
                    Enumerable.Repeat(1.0, width - upgrade.ResourceMask.Count)).ToArray()
            }).ToArray();
    }

    private static IReadOnlyList<FurnisherItemGroupRule> ReadFurnisherItems(SyxDataNode? node) =>
        node?.Items?.Select(item => new FurnisherItemGroupRule(
            ReadNumberArray(item.Get("COSTS")),
            ReadNumberArray(item.Get("STATS")))).ToArray() ??
        Array.Empty<FurnisherItemGroupRule>();

    private static RoomArchetype ClassifyRoom(string key, SyxDataNode data)
    {
        if (data.Get("SERVICE") is not null) return RoomArchetype.Service;
        if (data.Get("GROWABLE") is not null || key.StartsWith("FARM_", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("PASTURE_", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("ORCHARD_", StringComparison.OrdinalIgnoreCase)) return RoomArchetype.Agriculture;
        if (data.Get("MINABLE") is not null || key.StartsWith("MINE_", StringComparison.OrdinalIgnoreCase))
            return RoomArchetype.Extraction;
        if (data.Get("INDUSTRIES") is not null || key.StartsWith("WORKSHOP_", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("REFINER_", StringComparison.OrdinalIgnoreCase)) return RoomArchetype.Industry;
        if (key.Contains("BARRACK", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("ARCHERY", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("ARTILLERY", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("GATEHOUSE", StringComparison.OrdinalIgnoreCase)) return RoomArchetype.Military;
        if (data.Get("WORK") is not null) return RoomArchetype.Civic;
        return RoomArchetype.Infrastructure;
    }

    private SyxDataNode? ParseFile(string path, bool warnMissing = true)
    {
        if (!File.Exists(path))
        {
            if (warnMissing) _warnings.Add($"Missing {path}");
            return null;
        }
        try
        {
            return SyxDataParser.Parse(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            _warnings.Add($"{Path.GetFileName(path)}: {exception.Message}");
            return null;
        }
    }
}
