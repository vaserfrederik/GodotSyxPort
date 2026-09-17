using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Trade;

namespace GodotSyxPort.World;

public sealed record WorldBuildingLevelRule(
    int Level, int Credits, IReadOnlyDictionary<string, double> LocalBoosts,
    IReadOnlyDictionary<string, double> GlobalBoosts,
    IReadOnlyDictionary<string, double> RequirementsLess);
public sealed record WorldBuildingRule(
    string Key, string Category, bool AiBuilds, IReadOnlyList<WorldBuildingLevelRule> Levels);
public enum RegionalEdict : byte { None, Sanction, Exile, Massacre }

/// <summary>Loads explicit init/world/building definitions through the shared relaxed parser.</summary>
public sealed class WorldBuildingCatalog
{
    private readonly Dictionary<string, WorldBuildingRule> _all = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, WorldBuildingRule> All => _all;

    public static WorldBuildingCatalog Load()
    {
        var catalog = new WorldBuildingCatalog();
        var root = ProjectSettings.GlobalizePath("res://Data/Original/init/world/building");
        if (!Directory.Exists(root)) return catalog;
        foreach (var path in Directory.EnumerateFiles(root, "*.txt", SearchOption.AllDirectories)
                     .Where(path => !Path.GetFileName(path).StartsWith("_", StringComparison.Ordinal)))
        {
            var node = SyxDataParser.Parse(File.ReadAllText(path));
            var category = Path.GetFileName(Path.GetDirectoryName(path) ?? "").ToUpperInvariant();
            var key = category + "_" + Path.GetFileNameWithoutExtension(path).ToUpperInvariant();
            var levels = (node.Get("LEVELS")?.Items ?? new List<SyxDataNode>()).Select((level, index) =>
                new WorldBuildingLevelRule(index + 1, level.Get("CREDITS")?.Integer() ?? 0,
                    Numbers(level.Get("BOOST")), Numbers(level.Get("BOOST_GLOBAL")),
                    Numbers(level.Get("REQUIRES")?.Get("LESS")))).ToArray();
            if (levels.Length > 0)
                catalog._all[key] = new WorldBuildingRule(
                    key, category, node.Get("AI_BUILDS")?.Boolean(true) ?? true, levels);
        }
        foreach (var path in Directory.EnumerateFiles(root, "_GEN.txt", SearchOption.AllDirectories))
            catalog.LoadGenerated(path);
        return catalog;
    }

    private void LoadGenerated(string path)
    {
        var category = Path.GetFileName(Path.GetDirectoryName(path) ?? "").ToUpperInvariant();
        var root = SyxDataParser.Parse(File.ReadAllText(path));
        foreach (var generator in root.Get("GENS")?.Items ?? new List<SyxDataNode>())
        {
            var patterns = generator.Get("INDUSTRIES")?.Items?.Select(value => value.Text()).ToArray() ??
                Array.Empty<string>();
            var rooms = OriginalGameData.Current.Rooms.Values.Where(room => patterns.Any(pattern =>
                pattern.EndsWith('*') ? room.Key.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase) :
                room.Key.Equals(pattern, StringComparison.OrdinalIgnoreCase)));
            var count = Math.Clamp(generator.Get("LEVELS")?.Integer(1) ?? 1, 1, 10);
            var output = generator.Get("OUTPUT")?.Number() ?? 0;
            var credits = generator.Get("CREDITS")?.Integer() ?? 0;
            var yearly = generator.Get("YEARLY")?.Boolean() ?? false;
            foreach (var room in rooms.Where(room => room.Recipes.Count > 0))
            {
                var key = category + "_" + room.Key.TrimStart('_');
                var levels = Enumerable.Range(1, count).Select(level =>
                {
                    var fraction = level / (double)count;
                    var boosts = Numbers(generator.Get("BOOST")).ToDictionary(
                        pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
                    foreach (var produced in room.Recipes[0].Outputs)
                        boosts[$"WORLD_PRODUCTION_{produced.Resource}>{(yearly ? "YEARLY" : "ADD")}"] =
                            output * fraction * produced.Rate / 1.0025;
                    return new WorldBuildingLevelRule(level,
                        (int)(400 * credits * fraction), boosts,
                        new Dictionary<string, double>(), new Dictionary<string, double>());
                }).ToArray();
                _all[key] = new WorldBuildingRule(key, category, true, levels);
            }
        }
    }

    private static IReadOnlyDictionary<string, double> Numbers(SyxDataNode? node) =>
        node?.Fields?.ToDictionary(pair => pair.Key, pair => pair.Value.Number(),
            StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, double>();
}

public sealed class RegionalEconomyState
{
    public int RegionId { get; init; }
    public string MajorityRace { get; set; } = "";
    public int Population { get; set; }
    public int PopulationTarget { get; set; }
    public int NaturalPopulationTarget { get; set; }
    public int BasePopulationCapacity { get; set; }
    public double Loyalty { get; set; } = 1;
    public double Devastation { get; set; }
    public double TaxSqueeze { get; set; }
    public bool Besieged { get; set; }
    public int Workforce { get; set; }
    public double Health { get; set; } = 1;
    public double HealthTarget { get; set; } = 1;
    public bool DiseaseOutbreak { get; set; }
    public double TaxIncome { get; set; }
    public double AccruedTaxes { get; set; }
    public double Garrison { get; set; }
    public double Fortification { get; set; }
    public double Proximity { get; set; }
    public double VisualRoads { get; set; }
    public double VisualWalls { get; set; }
    public double VisualMines { get; set; }
    public Dictionary<string, int> Buildings { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<ResourceKind, double> DailyOutputs { get; } = new();
    public Dictionary<ResourceKind, int> Stock { get; } = new();
    public Dictionary<ResourceKind, double> Accumulation { get; } = new();
    public Dictionary<string, int> Prospects { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, RegionalEdict> Edicts { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> RacePopulation { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> RaceTargets { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> RaceLoyalty { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> RaceGrowthRemainder { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Religions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public double ReligiousOpposition { get; set; }
    public double PlayerSupport { get; set; }
}

public sealed class PeacefulCaravan
{
    public long Id { get; init; }
    public int FactionId { get; init; }
    public int OriginRegionId { get; init; }
    public int DestinationRegionId { get; init; }
    public int Distance { get; init; }
    public double Progress { get; set; }
    public int CurrentRegionId { get; set; }
    public List<int> Route { get; } = new();
    public Dictionary<ResourceKind, int> Cargo { get; } = new();
}

public sealed record RegionalEconomyStateSnapshot(
    int RegionId, string MajorityRace, int Population, int PopulationTarget,
    int NaturalPopulationTarget, int BasePopulationCapacity, double Loyalty, double Devastation,
    double TaxSqueeze, bool Besieged, int Workforce,
    double Health, double HealthTarget, bool DiseaseOutbreak, double TaxIncome, double AccruedTaxes,
    double Garrison, double Fortification, double Proximity,
    double VisualRoads, double VisualWalls, double VisualMines,
    IReadOnlyDictionary<string, int> Buildings,
    IReadOnlyDictionary<ResourceKind, double> DailyOutputs,
    IReadOnlyDictionary<ResourceKind, int> Stock,
    IReadOnlyDictionary<ResourceKind, double> Accumulation,
    IReadOnlyDictionary<string, int> Prospects,
    IReadOnlyDictionary<string, RegionalEdict> Edicts,
    IReadOnlyDictionary<string, int> RacePopulation,
    IReadOnlyDictionary<string, int> RaceTargets,
    IReadOnlyDictionary<string, double> RaceLoyalty,
    IReadOnlyDictionary<string, double> RaceGrowthRemainder,
    IReadOnlyDictionary<string, double> Religions,
    double ReligiousOpposition,
    double PlayerSupport);
public sealed record PeacefulCaravanSnapshot(
    long Id, int FactionId, int OriginRegionId, int DestinationRegionId, int Distance,
    double Progress, int CurrentRegionId, IReadOnlyList<int> Route,
    IReadOnlyDictionary<ResourceKind, int> Cargo);
public sealed record RealmEdictSnapshot(int FactionId, string Race, RegionalEdict Edict, double Strength);
public sealed record RegionalEconomySnapshot(
    double DayAccumulator, long ProcessedDays, long NextCaravanId,
    IReadOnlyList<RegionalEconomyStateSnapshot> States,
    IReadOnlyList<PeacefulCaravanSnapshot> Caravans,
    IReadOnlyList<RealmEdictSnapshot> RealmEdicts);

/// <summary>
/// Non-combat RD population/output and Shipper consolidation. Region outputs accumulate,
/// then tax caravans carry bounded cargo to the faction capital. NPC capital stock is the
/// sole source for trade markets; no stock is fabricated by the trade adapter.
/// </summary>
public sealed class RegionalEconomyRuntime
{
    public const int PopulationCapacityPerTile = 100;
    public const int EntityMaximum = 40000;
    public const int PlayerRegionPopulation = 200;
    public const int TradeRouteShipmentMaximum = 50;
    public const int SourceDaysPerYear = 16;
    private readonly StrategicWorldRuntime _world;
    private readonly SettlementTradeRuntime _trade;
    private readonly Dictionary<int, RegionalEconomyState> _states = new();
    private readonly List<PeacefulCaravan> _caravans = new();
    private readonly Dictionary<(int Faction, string Race, RegionalEdict Edict), double> _realmEdicts = new();
    private long _nextCaravanId;
    private long _processedDays;
    private double _dayAccumulator;
    public WorldBuildingCatalog Buildings { get; }
    public IReadOnlyDictionary<int, RegionalEconomyState> States => _states;
    public IReadOnlyList<PeacefulCaravan> Caravans => _caravans;

    public RegionalEconomyRuntime(StrategicWorldRuntime world, SettlementTradeRuntime trade)
    {
        _world = world;
        _trade = trade;
        Buildings = WorldBuildingCatalog.Load();
        foreach (var region in world.Regions)
        {
            var faction = world.Faction(region.OwnerFactionId);
            var population = faction?.Player == true && region.Capital
                ? PlayerRegionPopulation : InitialPopulationCapacity(region, faction);
            _states[region.Id] = new RegionalEconomyState
            {
                RegionId = region.Id, MajorityRace = faction?.Race ?? "",
                Population = population, PopulationTarget = population,
                NaturalPopulationTarget = population, BasePopulationCapacity = population
            };
        }
        GenerateProspects();
        PrimeNpcBuildings();
        PrimePopulation();
    }

    public RegionalEconomyState? State(int regionId) => _states.GetValueOrDefault(regionId);

    public RegionalEconomySnapshot Capture() => new(
        _dayAccumulator,
        _processedDays,
        _nextCaravanId,
        _states.Values.Select(value => new RegionalEconomyStateSnapshot(
            value.RegionId, value.MajorityRace, value.Population, value.PopulationTarget,
            value.NaturalPopulationTarget, value.BasePopulationCapacity, value.Loyalty, value.Devastation,
            value.TaxSqueeze, value.Besieged, value.Workforce, value.Health, value.HealthTarget, value.DiseaseOutbreak,
            value.TaxIncome, value.AccruedTaxes, value.Garrison, value.Fortification,
            value.Proximity, value.VisualRoads, value.VisualWalls, value.VisualMines,
            new Dictionary<string, int>(value.Buildings, StringComparer.OrdinalIgnoreCase),
            new Dictionary<ResourceKind, double>(value.DailyOutputs),
            new Dictionary<ResourceKind, int>(value.Stock),
            new Dictionary<ResourceKind, double>(value.Accumulation),
            new Dictionary<string, int>(value.Prospects, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, RegionalEdict>(value.Edicts, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, int>(value.RacePopulation, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, int>(value.RaceTargets, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, double>(value.RaceLoyalty, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, double>(value.RaceGrowthRemainder, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, double>(value.Religions, StringComparer.OrdinalIgnoreCase),
            value.ReligiousOpposition,
            value.PlayerSupport)).ToArray(),
        _caravans.Select(value => new PeacefulCaravanSnapshot(
            value.Id, value.FactionId, value.OriginRegionId, value.DestinationRegionId,
            value.Distance, value.Progress, value.CurrentRegionId, value.Route.ToArray(),
            new Dictionary<ResourceKind, int>(value.Cargo))).ToArray(),
        _realmEdicts.Select(value => new RealmEdictSnapshot(
            value.Key.Faction, value.Key.Race, value.Key.Edict, value.Value)).ToArray());

    public void Restore(RegionalEconomySnapshot snapshot)
    {
        _dayAccumulator = Math.Max(0, snapshot.DayAccumulator);
        _processedDays = Math.Max(0, snapshot.ProcessedDays);
        _nextCaravanId = Math.Max(0, snapshot.NextCaravanId);
        foreach (var value in snapshot.States)
        {
            if (!_states.TryGetValue(value.RegionId, out var state)) continue;
            state.MajorityRace = value.MajorityRace;
            state.Population = Math.Max(0, value.Population);
            state.PopulationTarget = Math.Max(0, value.PopulationTarget);
            state.NaturalPopulationTarget = Math.Max(0, value.NaturalPopulationTarget);
            state.BasePopulationCapacity = Math.Max(0, value.BasePopulationCapacity > 0
                ? value.BasePopulationCapacity : value.NaturalPopulationTarget);
            state.Loyalty = Math.Clamp(value.Loyalty, 0, 1);
            state.Devastation = Math.Clamp(value.Devastation, 0, 1);
            state.TaxSqueeze = Math.Clamp(value.TaxSqueeze, 0, 1);
            state.Besieged = value.Besieged;
            state.Workforce = Math.Max(0, value.Workforce);
            state.Health = Math.Clamp(value.Health, 0, 1);
            state.HealthTarget = Math.Clamp(value.HealthTarget, 0, 1);
            state.DiseaseOutbreak = value.DiseaseOutbreak;
            state.TaxIncome = Math.Max(0, value.TaxIncome);
            state.AccruedTaxes = Math.Max(0, value.AccruedTaxes);
            state.Garrison = Math.Max(0, value.Garrison);
            state.Fortification = Math.Max(0, value.Fortification);
            state.Proximity = Math.Max(0, value.Proximity);
            state.VisualRoads = Math.Clamp(value.VisualRoads, 0, 1);
            state.VisualWalls = Math.Clamp(value.VisualWalls, 0, 1);
            state.VisualMines = Math.Clamp(value.VisualMines, 0, 1);
            Replace(state.Buildings, value.Buildings);
            Replace(state.DailyOutputs, value.DailyOutputs);
            Replace(state.Stock, value.Stock);
            Replace(state.Accumulation, value.Accumulation);
            Replace(state.Prospects, value.Prospects);
            Replace(state.Edicts, value.Edicts);
            Replace(state.RacePopulation, value.RacePopulation);
            Replace(state.RaceTargets, value.RaceTargets);
            Replace(state.RaceLoyalty, value.RaceLoyalty);
            Replace(state.RaceGrowthRemainder, value.RaceGrowthRemainder);
            Replace(state.Religions, value.Religions);
            state.ReligiousOpposition = Math.Clamp(value.ReligiousOpposition, 0, 1);
            state.PlayerSupport = Math.Clamp(value.PlayerSupport, 0, 1);
            var region = _world.Region(value.RegionId);
            if (region is not null) region.Population = state.Population;
        }
        _caravans.Clear();
        foreach (var value in snapshot.Caravans)
        {
            var caravan = new PeacefulCaravan
            {
                Id = value.Id, FactionId = value.FactionId,
                OriginRegionId = value.OriginRegionId, DestinationRegionId = value.DestinationRegionId,
                Distance = value.Distance, Progress = value.Progress, CurrentRegionId = value.CurrentRegionId
            };
            caravan.Route.AddRange(value.Route);
            foreach (var pair in value.Cargo) caravan.Cargo[pair.Key] = pair.Value;
            _caravans.Add(caravan);
        }
        _realmEdicts.Clear();
        foreach (var value in snapshot.RealmEdicts)
            _realmEdicts[(value.FactionId, value.Race, value.Edict)] = value.Strength;
        PublishNpcMarkets();
        _world.PublishTradeQuotes(_trade);
    }

    private static void Replace<TKey, TValue>(
        IDictionary<TKey, TValue> target, IReadOnlyDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        target.Clear();
        foreach (var pair in source) target[pair.Key] = pair.Value;
    }

    public double RealmEdictStrength(int factionId, string race, RegionalEdict edict) =>
        _realmEdicts.GetValueOrDefault((factionId, race.ToUpperInvariant(), edict));

    public void AddPlayerSupport(int regionId, double amount)
    {
        if (!_states.TryGetValue(regionId, out var state) || amount <= 0) return;
        state.PlayerSupport = Math.Clamp(state.PlayerSupport + amount, 0, 1);
    }

    public void SetPopulationTarget(int regionId, string race, int target)
    {
        if (!_states.TryGetValue(regionId, out var state)) return;
        state.MajorityRace = race;
        state.BasePopulationCapacity = Math.Max(0, target);
        Recalculate(state);
        RecalculateRaceTargets(state);
    }

    public void SetEdict(int regionId, string race, RegionalEdict edict)
    {
        if (!_states.TryGetValue(regionId, out var state)) return;
        state.Edicts[race] = edict;
        if (edict == RegionalEdict.Massacre)
        {
            state.RacePopulation[race] = 0;
            SynchronizePopulation(state);
        }
        RecalculateRaceTargets(state);
    }

    public bool SetBuildingLevel(int regionId, string buildingKey, int level)
    {
        if (!_states.TryGetValue(regionId, out var state) ||
            !Buildings.All.TryGetValue(buildingKey, out var building)) return false;
        var old = state.Buildings.GetValueOrDefault(buildingKey);
        var clamped = Math.Clamp(level, 0, building.Levels.Count);
        if (clamped <= old) { state.Buildings[buildingKey] = clamped; Recalculate(state); return true; }
        var cost = building.Levels.Skip(old).Take(clamped - old).Sum(value => value.Credits);
        if (clamped > 0 && !RequirementsPass(regionId, building.Levels[clamped - 1])) return false;
        if (_trade.Credits < cost) return false;
        _trade.Credits -= cost;
        state.Buildings[buildingKey] = clamped;
        Recalculate(state);
        return true;
    }

    public void ConfigureDailyOutput(int regionId, ResourceKind resource, double amount)
    {
        if (!_states.TryGetValue(regionId, out var state)) return;
        state.DailyOutputs[resource] = Math.Max(0, amount);
    }

    public void SetBesieged(int regionId, bool besieged)
    {
        if (_states.TryGetValue(regionId, out var state)) state.Besieged = besieged;
    }

    /// <summary>Regional part of Util.conquer: devastation, deaths and building damage.</summary>
    public void ApplyConquest(int regionId, double devastation, double death)
    {
        if (!_states.TryGetValue(regionId, out var state)) return;
        var damage = Math.Clamp(devastation, 0, 1);
        var mortality = Math.Clamp(death, 0, 1);
        state.Devastation = Math.Clamp(state.Devastation + damage, 0, 1);
        state.Population = Math.Clamp((int)(1 + state.Population * (1 - mortality)),
            state.Population > 0 ? 1 : 0, state.Population);
        foreach (var race in state.RacePopulation.Keys.ToArray())
        {
            var population = state.RacePopulation[race];
            state.RacePopulation[race] = Math.Clamp((int)(1 + population * (1 - mortality)),
                population > 0 ? 1 : 0, population);
        }
        foreach (var building in state.Buildings.Keys.ToArray())
        {
            var remaining = state.Buildings[building] * (1 - damage);
            state.Buildings[building] = Math.Max(0, (int)Math.Floor(remaining));
        }
        state.Besieged = false;
    }

    /// <summary>RDOutputs.squeze: collect four days immediately and add 50% devastation/squeeze.</summary>
    public bool SqueezeRegion(int regionId)
    {
        if (!_states.TryGetValue(regionId, out var state) ||
            _world.Region(regionId)?.OwnerFactionId < 0) return false;
        const double days = 4;
        state.AccruedTaxes += state.TaxIncome * days;
        foreach (var output in state.DailyOutputs)
        {
            var amount = (int)Math.Floor(output.Value * days);
            if (amount > 0) state.Stock[output.Key] = state.Stock.GetValueOrDefault(output.Key) + amount;
        }
        state.Devastation = Math.Clamp(state.Devastation + 0.5, 0, 1);
        state.TaxSqueeze = Math.Clamp(state.TaxSqueeze + 0.5, 0, 1);
        Recalculate(state);
        RecalculateRaceTargets(state);
        return true;
    }

    public void Tick(double days)
    {
        if (days <= 0) return;
        _dayAccumulator += days;
        while (_dayAccumulator >= 1)
        {
            _dayAccumulator -= 1;
            ProcessDay();
        }
    }

    private void ProcessDay()
    {
        _processedDays++;
        if (_processedDays % 2 == 0) PrimeNpcBuildings();
        UpdateRealmEdicts();
        MoveExiles();
        foreach (var state in _states.Values)
        {
            RecalculateRaceTargets(state);
            UpdateRacePopulation(state);
            UpdateReligions(state);
            UpdateHealthAndDevastation(state);
            SynchronizePopulation(state);
            AccumulateTaxes(state);
            foreach (var output in state.DailyOutputs)
            {
                state.Accumulation[output.Key] = state.Accumulation.GetValueOrDefault(output.Key) +
                    output.Value * RegionalCondition(state);
                var whole = (int)Math.Floor(state.Accumulation[output.Key]);
                if (whole <= 0) continue;
                state.Accumulation[output.Key] -= whole;
                state.Stock[output.Key] = state.Stock.GetValueOrDefault(output.Key) + whole;
            }
        }
        DispatchTaxCaravans(_processedDays % 2 == 0);
        AdvanceCaravans(1);
        PublishNpcMarkets();
        _world.PublishTradeQuotes(_trade);
    }

    private void UpdateRealmEdicts()
    {
        foreach (var faction in _world.Factions)
        foreach (var race in OriginalGameData.Current.Races.Keys)
        foreach (var edict in new[] { RegionalEdict.Sanction, RegionalEdict.Exile, RegionalEdict.Massacre })
        {
            var active = _states.Values.Count(state =>
                _world.Region(state.RegionId)?.OwnerFactionId == faction.Id &&
                state.Edicts.GetValueOrDefault(race) == edict);
            var key = (faction.Id, race.ToUpperInvariant(), edict);
            var value = _realmEdicts.GetValueOrDefault(key);
            value += active > 0 ? active * 0.5 : -1.0 / (2 * SourceDaysPerYear);
            _realmEdicts[key] = Math.Clamp(value, 0, 1);
        }
    }

    private void UpdateRacePopulation(RegionalEconomyState state)
    {
        var owner = _world.Region(state.RegionId)?.OwnerFactionId ?? -1;
        foreach (var raceKey in OriginalGameData.Current.Races.Keys)
        {
            var race = OriginalGameData.Current.Races[raceKey];
            var population = state.RacePopulation.GetValueOrDefault(raceKey);
            var target = state.RaceTargets.GetValueOrDefault(raceKey);
            var edict = state.Edicts.GetValueOrDefault(raceKey);
            var growth = edict switch
            {
                RegionalEdict.Sanction => 0.5,
                RegionalEdict.Exile => 0.4,
                RegionalEdict.Massacre => 0,
                _ => 1
            };
            growth *= RegionalCondition(state);
            var expected = population < target ? (population + 10) * Math.Max(0, race.PopulationGrowth) * growth :
                population > target ? -(population + 10.0) : 0;
            var remainder = state.RaceGrowthRemainder.GetValueOrDefault(raceKey) + expected;
            var change = remainder >= 0 ? (int)Math.Floor(remainder) : (int)Math.Ceiling(remainder);
            state.RaceGrowthRemainder[raceKey] = remainder - change;
            state.RacePopulation[raceKey] = Math.Clamp(population + change,
                Math.Min(population, target), Math.Max(population, target));

            var localMultiplier = edict switch
            {
                RegionalEdict.Sanction => 0.25,
                RegionalEdict.Exile or RegionalEdict.Massacre => 0,
                _ => 1
            };
            var distant = 1.0;
            foreach (var distantEdict in new[] { RegionalEdict.Sanction, RegionalEdict.Exile, RegionalEdict.Massacre })
            {
                var floor = distantEdict == RegionalEdict.Sanction ? 0.25 : 0;
                distant *= 1 - RealmEdictStrength(owner, raceKey, distantEdict) * (1 - floor);
            }
            var squeezeLoyalty = 1 - state.TaxSqueeze * 0.75;
            var targetLoyalty = localMultiplier * distant * squeezeLoyalty *
                                (1 - state.ReligiousOpposition * 0.25);
            var loyalty = state.RaceLoyalty.GetValueOrDefault(raceKey, 1);
            state.RaceLoyalty[raceKey] = Math.Clamp(loyalty +
                Math.Clamp(targetLoyalty - loyalty, -8.0 / 255.0, 8.0 / 255.0), 0, 1);
        }
    }

    private void MoveExiles()
    {
        foreach (var source in _states.Values)
        foreach (var pair in source.RacePopulation.ToArray())
        {
            if (pair.Value <= 0 || source.Edicts.GetValueOrDefault(pair.Key) != RegionalEdict.Exile) continue;
            var destination = _world.Region(source.RegionId)!.Neighbours.Select(State).Where(state => state is not null)
                .Select(state => state!).Where(state =>
                    state.Edicts.GetValueOrDefault(pair.Key) is not (RegionalEdict.Exile or RegionalEdict.Massacre))
                .OrderByDescending(state => state.NaturalPopulationTarget - state.Population).FirstOrDefault();
            if (destination is null) continue;
            var amount = pair.Value;
            source.RacePopulation[pair.Key] -= amount;
            destination.RacePopulation[pair.Key] = destination.RacePopulation.GetValueOrDefault(pair.Key) + amount;
            SynchronizePopulation(source);
            SynchronizePopulation(destination);
        }
    }

    private static void InitializeReligions(RegionalEconomyState state)
    {
        var religions = OriginalGameData.Current.Religions.Values.ToArray();
        if (religions.Length == 0) return;
        var race = OriginalGameData.Current.Races.GetValueOrDefault(state.MajorityRace);
        var weights = religions.ToDictionary(religion => religion.Key, religion => Math.Max(0,
            religion.DefaultSpread + (race?.ReligionInclination.GetValueOrDefault("*") ?? 0) +
            (race?.ReligionInclination.GetValueOrDefault(religion.Key) ?? 0)), StringComparer.OrdinalIgnoreCase);
        var total = weights.Values.Sum();
        foreach (var religion in religions)
            state.Religions[religion.Key] = total > 0 ? weights[religion.Key] / total : 1.0 / religions.Length;
        CalculateReligiousOpposition(state);
    }

    private static void UpdateReligions(RegionalEconomyState state)
    {
        var religions = OriginalGameData.Current.Religions.Values.ToArray();
        if (religions.Length == 0) return;
        var weights = religions.ToDictionary(religion => religion.Key, religion => 0.0,
            StringComparer.OrdinalIgnoreCase);
        foreach (var population in state.RacePopulation.Where(value => value.Value > 0))
        {
            var race = OriginalGameData.Current.Races.GetValueOrDefault(population.Key);
            foreach (var religion in religions)
                weights[religion.Key] += population.Value * Math.Max(0, religion.DefaultSpread +
                    (race?.ReligionInclination.GetValueOrDefault("*") ?? 0) +
                    (race?.ReligionInclination.GetValueOrDefault(religion.Key) ?? 0));
        }
        var total = weights.Values.Sum();
        foreach (var religion in religions)
        {
            var target = total > 0 ? weights[religion.Key] / total : 1.0 / religions.Length;
            var current = state.Religions.GetValueOrDefault(religion.Key, target);
            state.Religions[religion.Key] = Math.Clamp(current + (target - current) / SourceDaysPerYear,
                Math.Min(current, target), Math.Max(current, target));
        }
        CalculateReligiousOpposition(state);
    }

    private static void CalculateReligiousOpposition(RegionalEconomyState state)
    {
        var opposition = 0.0;
        foreach (var first in state.Religions)
        foreach (var second in state.Religions)
            opposition += first.Value * second.Value *
                (OriginalGameData.Current.Religions.GetValueOrDefault(first.Key)?.Opposition
                    .GetValueOrDefault(second.Key) ?? 0);
        state.ReligiousOpposition = Math.Clamp(opposition, 0, 1);
    }

    private void SynchronizePopulation(RegionalEconomyState state)
    {
        state.Population = state.RacePopulation.Values.Sum();
        state.MajorityRace = state.RacePopulation.OrderByDescending(value => value.Value).FirstOrDefault().Key ?? "";
        var weightedLoyalty = state.RacePopulation.Sum(pair => pair.Value * state.RaceLoyalty.GetValueOrDefault(pair.Key, 1));
        state.Loyalty = state.Population <= 0 ? 0 : weightedLoyalty / state.Population;
        _world.Region(state.RegionId)!.Population = state.Population;
    }

    private void RecalculateRaceTargets(RegionalEconomyState state)
    {
        var region = _world.Region(state.RegionId)!;
        var weights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var race in OriginalGameData.Current.Races.Values)
        {
            var climate = race.PopulationClimate.GetValueOrDefault(region.Climate, 1);
            var mountain = Math.Clamp(region.Mountain, 0, 1);
            var forest = Math.Clamp(region.Forest * (1 - mountain), 0, 1 - mountain);
            var ordinary = Math.Max(0, 1 - mountain - forest);
            var terrain = race.PopulationTerrain.Count == 0 ? 1 :
                mountain * race.PopulationTerrain.GetValueOrDefault("MOUNTAIN", 1) +
                forest * race.PopulationTerrain.GetValueOrDefault("FOREST", 1) +
                ordinary * race.PopulationTerrain.GetValueOrDefault("NONE", 1);
            var rulingRace = _world.Faction(region.OwnerFactionId)?.Race;
            var rulingBoost = string.Equals(rulingRace, race.Key, StringComparison.OrdinalIgnoreCase) ? 1.2 : 1;
            weights[race.Key] = Math.Max(0, race.PopulationMaximum * climate * terrain * rulingBoost);
        }
        var total = weights.Values.Sum();
        foreach (var race in weights.Keys)
        {
            var natural = total <= 0 ? 0 : (int)Math.Round(state.NaturalPopulationTarget * weights[race] / total);
            var edict = state.Edicts.GetValueOrDefault(race);
            state.RaceTargets[race] = edict is RegionalEdict.Exile or RegionalEdict.Massacre ? 0 : natural;
        }
        state.PopulationTarget = state.RaceTargets.Values.Sum();
    }

    private void Recalculate(RegionalEconomyState state)
    {
        state.Workforce = Math.Max(0, (int)Math.Round(BuildingValue(state, "WORLD_POINT_WORKFORCE", 0)));
        state.NaturalPopulationTarget = (int)Math.Clamp(Math.Round(BuildingValue(
            state, "WORLD_POPULATION_CAPACITY", state.BasePopulationCapacity) * RegionalCondition(state)),
            0, EntityMaximum);
        state.HealthTarget = Math.Clamp(BuildingValue(state, "WORLD_HEALTH", 1), 0, 1);
        state.TaxIncome = Math.Max(0, BuildingValue(state, "WORLD_TAX_INCOME", 0));
        state.Garrison = Math.Max(0, BuildingValue(state, "WORLD_GARRISON", 0));
        state.Fortification = Math.Max(0, BuildingValue(state, "WORLD_FORTIFICATION", 0));
        state.Proximity = Math.Max(0, BuildingValue(state, "WORLD_PROXIMITY", 0));
        state.VisualRoads = Math.Clamp(BuildingValue(state, "WORLD_VISUAL_ROADS", 0), 0, 1);
        state.VisualWalls = Math.Clamp(BuildingValue(state, "WORLD_VISUAL_WALL", 0), 0, 1);
        state.VisualMines = Math.Clamp(BuildingValue(state, "WORLD_VISUAL_MINE", 0), 0, 1);
        state.DailyOutputs.Clear();
        foreach (var pair in state.Buildings)
        {
            if (!Buildings.All.TryGetValue(pair.Key, out var building) || pair.Value <= 0) continue;
            var level = building.Levels[Math.Clamp(pair.Value - 1, 0, building.Levels.Count - 1)];
            var prospect = state.Prospects.GetValueOrDefault(building.Key);
            var prospectEfficiency = prospect <= 0 ? 1 : 0.75 + 0.5 * prospect;
            foreach (var boost in level.LocalBoosts.Where(value =>
                         value.Key.StartsWith("WORLD_PRODUCTION_", StringComparison.OrdinalIgnoreCase)))
            {
                var tail = boost.Key["WORLD_PRODUCTION_".Length..];
                var separator = tail.IndexOf('>');
                var resourceKey = separator < 0 ? tail : tail[..separator];
                if (!Enum.TryParse<ResourceKind>(resourceKey.TrimStart('_'), true, out var resource)) continue;
                var yearly = separator >= 0 && tail[(separator + 1)..].Equals(
                    "YEARLY", StringComparison.OrdinalIgnoreCase);
                state.DailyOutputs[resource] = state.DailyOutputs.GetValueOrDefault(resource) +
                    boost.Value * prospectEfficiency / (yearly ? SourceDaysPerYear : 1);
            }
        }
    }

    private double BuildingValue(RegionalEconomyState state, string boostKey, double baseValue)
    {
        var add = 0.0;
        var multiply = 1.0;
        ApplySelectedBoosts(state.Buildings, false);
        var factionId = _world.Region(state.RegionId)?.OwnerFactionId ?? -1;
        if (factionId >= 0)
        foreach (var other in _states.Values.Where(value => value.RegionId != state.RegionId &&
                     _world.Region(value.RegionId)?.OwnerFactionId == factionId))
            ApplySelectedBoosts(other.Buildings, true);
        return (baseValue + add) * multiply;

        void ApplySelectedBoosts(IReadOnlyDictionary<string, int> selected, bool globalOnly)
        {
            foreach (var pair in selected)
            {
                if (!Buildings.All.TryGetValue(pair.Key, out var building) || pair.Value <= 0) continue;
                var level = building.Levels[Math.Clamp(pair.Value - 1, 0, building.Levels.Count - 1)];
                Apply(globalOnly ? level.GlobalBoosts : level.LocalBoosts);
                if (!globalOnly) Apply(level.GlobalBoosts);
            }
        }

        void Apply(IReadOnlyDictionary<string, double> boosts)
        {
            foreach (var pair in boosts)
            {
                var separator = pair.Key.IndexOf('>');
                var key = separator < 0 ? pair.Key : pair.Key[..separator];
                if (!key.Equals(boostKey, StringComparison.OrdinalIgnoreCase)) continue;
                var operation = separator < 0 ? "ADD" : pair.Key[(separator + 1)..];
                if (operation.Equals("MUL", StringComparison.OrdinalIgnoreCase)) multiply *= pair.Value;
                else add += pair.Value;
            }
        }
    }

    private void PrimeNpcBuildings()
    {
        foreach (var state in _states.Values)
        {
            var region = _world.Region(state.RegionId)!;
            var faction = _world.Faction(region.OwnerFactionId);
            if (faction is null || faction.Player) continue;
            BuildNpcRegion(state, faction);
            Recalculate(state);
        }
    }

    /// <summary>
    /// Port of updating.Builder: clear the NPC plan, then independently distribute
    /// population-derived build points among resources, military and remaining civic
    /// buildings. A building claimed by an earlier source group is not considered again.
    /// </summary>
    private void BuildNpcRegion(RegionalEconomyState state, StrategicFaction faction)
    {
        state.Buildings.Clear();
        var available = Buildings.All.Values.Where(value => value.AiBuilds && value.Levels.Count > 0)
            .OrderBy(value => value.Key, StringComparer.OrdinalIgnoreCase).ToArray();
        var resources = available.Where(IsResourceBuilding).ToArray();
        var military = available.Where(value => !resources.Contains(value) && IsMilitaryBuilding(value)).ToArray();
        var civic = available.Where(value => !resources.Contains(value) && !military.Contains(value)).ToArray();
        var competence = Math.Clamp(0.5 + StableUnit(faction.Id, 81) * 0.5, 0, 1);
        var points = Math.Max(0, (int)(competence * state.NaturalPopulationTarget / 100.0));
        AllocateBuildingGroup(state, resources, points, value => ResourcePriority(state, value));
        AllocateBuildingGroup(state, military, points, value => MilitaryPriority(state, faction, value));
        AllocateBuildingGroup(state, civic, points, value => CivicPriority(state, value));
    }

    private void AllocateBuildingGroup(RegionalEconomyState state, IEnumerable<WorldBuildingRule> source,
        int points, Func<WorldBuildingRule, double> priority)
    {
        var ranked = source.Select(building => (Building: building, Value: Math.Max(0, priority(building))))
            .Where(value => value.Value > 0).OrderByDescending(value => value.Value).ToArray();
        var middle = ranked.Sum(value => value.Value);
        foreach (var candidate in ranked)
        {
            if (points <= 0) break;
            var level = (int)Math.Ceiling(candidate.Building.Levels.Count * candidate.Value / middle);
            level = Math.Clamp(level, 0, Math.Min(candidate.Building.Levels.Count, points));
            if (level <= 0) continue;
            state.Buildings[candidate.Building.Key] = level;
            points -= level;
        }
    }

    private static bool IsResourceBuilding(WorldBuildingRule building) => building.Levels.Any(level =>
        level.LocalBoosts.Keys.Any(key => key.StartsWith("WORLD_PRODUCTION_", StringComparison.OrdinalIgnoreCase)));

    private static bool IsMilitaryBuilding(WorldBuildingRule building) => building.Levels.Any(level =>
        level.LocalBoosts.Keys.Any(key => key.StartsWith("WORLD_GARRISON", StringComparison.OrdinalIgnoreCase) ||
                                              key.StartsWith("WORLD_FORTIFICATION", StringComparison.OrdinalIgnoreCase)) ||
        level.GlobalBoosts.Keys.Any(key => key.StartsWith("WORLD_GARRISON", StringComparison.OrdinalIgnoreCase) ||
                                             key.StartsWith("WORLD_FORTIFICATION", StringComparison.OrdinalIgnoreCase)));

    private double ResourcePriority(RegionalEconomyState state, WorldBuildingRule building)
    {
        var region = _world.Region(state.RegionId)!;
        var output = building.Levels[^1].LocalBoosts.Concat(building.Levels[^1].GlobalBoosts).Where(value =>
            value.Key.StartsWith("WORLD_PRODUCTION_", StringComparison.OrdinalIgnoreCase))
            .Sum(value => Math.Max(0, value.Value));
        return output * RegionSuitability(region, building);
    }

    private double MilitaryPriority(RegionalEconomyState state, StrategicFaction faction,
        WorldBuildingRule building)
    {
        var output = building.Levels[^1].LocalBoosts.Where(value =>
            value.Key.StartsWith("WORLD_GARRISON", StringComparison.OrdinalIgnoreCase) ||
            value.Key.StartsWith("WORLD_FORTIFICATION", StringComparison.OrdinalIgnoreCase))
            .Sum(value => Math.Max(0, value.Value));
        return output * (0.75 + StableUnit(faction.Id, 9) * 0.25);
    }

    private double CivicPriority(RegionalEconomyState state, WorldBuildingRule building)
    {
        // Builder.BOther uses a fixed 256-value priority table indexed by building and RD.RAN.
        return StableUnit(state.RegionId, StableTextHash(building.Key) & 255);
    }

    private static int StableTextHash(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var c in value) { hash ^= c; hash *= 16777619; }
            return (int)hash;
        }
    }

    /// <summary>
    /// RD.prime initializes regional updatables three times after BUILD. Population is
    /// initialized directly at each race's biome-weighted target, not grown from a flat seed.
    /// </summary>
    private void PrimePopulation()
    {
        for (var pass = 0; pass < 3; pass++)
        foreach (var state in _states.Values)
        {
            Recalculate(state);
            RecalculateRaceTargets(state);
            state.RacePopulation.Clear();
            state.RaceLoyalty.Clear();
            foreach (var pair in state.RaceTargets)
            {
                if (pair.Value > 0) state.RacePopulation[pair.Key] = pair.Value;
                state.RaceLoyalty[pair.Key] = 1;
            }
            SynchronizePopulation(state);
        }
        foreach (var state in _states.Values) InitializeReligions(state);
    }

    /// <summary>
    /// RDRaces.capacity: mCapacity * area * moisture/random basis, followed by the
    /// NPC realm-size/king-competence limits. The player capital remains settlement-driven.
    /// </summary>
    private int InitialPopulationCapacity(StrategicRegion region, StrategicFaction? faction)
    {
        var regionalRandom = StableUnit(region.Id, 0);
        var full = PopulationCapacityPerTile * Math.Max(1, region.Area) *
                   (0.2 + region.Moisture * 0.8) * (0.5 + regionalRandom);
        if (faction is null) return (int)Math.Clamp(Math.Round(full * 0.1), 0, EntityMaximum);

        var realm = _world.Regions.Where(value => value.OwnerFactionId == faction.Id).ToArray();
        var fertileArea = realm.Sum(value => Math.Max(1, value.Area) * value.Fertility);
        var empireSize = Math.Clamp(fertileArea /
            (10.0 * StrategicWorldRuntime.AverageRegionArea * 0.5), 0, 1);
        // King.size() is competence-derived and clamped to 0..1. Court individuals are not
        // settlement entities in this runtime, so retain its deterministic generated value.
        var competence = Math.Clamp(0.5 + StableUnit(faction.Id, 81) * 0.5, 0, 1);
        var minimum = full * 0.1;
        var maximum = full;
        var target = region.Capital
            ? minimum + (EntityMaximum - minimum) * competence * empireSize
            : minimum + (maximum - minimum) * competence * Math.Sqrt(empireSize);
        return (int)Math.Clamp(Math.Round(target), 0, EntityMaximum);
    }

    private double StableUnit(int first, int second)
    {
        unchecked
        {
            uint value = (uint)_world.GenerationSeed;
            value ^= (uint)first * 0x9E3779B9u;
            value ^= (uint)second * 0x85EBCA6Bu;
            value ^= value >> 16; value *= 0x7FEB352Du;
            value ^= value >> 15; value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215.0;
        }
    }

    private void GenerateProspects()
    {
        var production = Buildings.All.Values.Where(building => building.Levels.Any(level =>
            level.LocalBoosts.Keys.Any(key => key.StartsWith("WORLD_PRODUCTION_",
                StringComparison.OrdinalIgnoreCase)))).OrderBy(value => value.Key).ToArray();
        if (production.Length == 0) return;
        foreach (var state in _states.Values)
        {
            var region = _world.Region(state.RegionId)!;
            var selected = production.OrderByDescending(building =>
                RegionSuitability(region, building) +
                StableKeyUnit(region.Id, building.Key) * 0.1)
                .Take(2).ToArray();
            for (var slot = 0; slot < selected.Length; slot++)
                state.Prospects[selected[slot].Key] = 3 - slot;
        }
        foreach (var state in _states.Values) Recalculate(state);
    }

    private static double RegionSuitability(StrategicRegion region, WorldBuildingRule building)
    {
        if (building.Key.Contains("FISH", StringComparison.OrdinalIgnoreCase)) return region.Water * 1.4;
        if (building.Category == "AGRICULTURE") return region.Fertility * 1.2;
        if (building.Category == "PASTURE") return 0.5 * region.Fertility + 0.5 * region.Moisture;
        if (building.Category == "MINE") return 1.0 - region.Fertility * 0.4;
        return 0.5;
    }

    private static double StableKeyUnit(int regionId, string key)
    {
        unchecked
        {
            uint hash = 2166136261u ^ (uint)regionId;
            foreach (var character in key.ToUpperInvariant())
                hash = (hash ^ character) * 16777619u;
            hash ^= hash >> 16; hash *= 0x7FEB352Du;
            hash ^= hash >> 15;
            return (hash & 0x00FFFFFFu) / 16777215.0;
        }
    }

    private bool RequirementsPass(int regionId, WorldBuildingLevelRule level)
    {
        var factionId = _world.Region(regionId)?.OwnerFactionId ?? -1;
        foreach (var requirement in level.RequirementsLess)
        {
            if (!requirement.Key.StartsWith("BUILDING_", StringComparison.OrdinalIgnoreCase) ||
                !requirement.Key.EndsWith("_KINGDOM", StringComparison.OrdinalIgnoreCase)) continue;
            var key = requirement.Key["BUILDING_".Length..^"_KINGDOM".Length];
            var amount = _states.Values.Count(state =>
                _world.Region(state.RegionId)?.OwnerFactionId == factionId &&
                state.Buildings.GetValueOrDefault(key) > 0);
            if (amount >= requirement.Value) return false;
        }
        return true;
    }

    private void UpdateHealthAndDevastation(RegionalEconomyState state)
    {
        var region = _world.Region(state.RegionId)!;
        var faction = _world.Faction(region.OwnerFactionId);
        if (faction is null || !faction.Player)
        {
            state.Health = 1;
            state.DiseaseOutbreak = false;
        }
        else
        {
            state.Health = MoveTowards(state.Health, state.HealthTarget, 0.5);
            if (!region.Capital && !state.DiseaseOutbreak && state.Health < 120.0 / 255.0 &&
                state.HealthTarget < 120.0 / 255.0) state.DiseaseOutbreak = true;
            else if (state.DiseaseOutbreak && state.Health > 128.0 / 255.0 &&
                     state.HealthTarget > 128.0 / 255.0) state.DiseaseOutbreak = false;
        }
        if (faction is not null) state.Devastation = Math.Max(0, state.Devastation - 1.0 / 32.0);
        state.TaxSqueeze = Math.Max(0, state.TaxSqueeze - 0.1);
    }

    private void AccumulateTaxes(RegionalEconomyState state)
    {
        var region = _world.Region(state.RegionId)!;
        var faction = _world.Faction(region.OwnerFactionId);
        if (faction is null || state.Besieged || faction.Player && region.Capital) return;
        state.AccruedTaxes += state.TaxIncome * RegionalCondition(state);
        if (region.Capital) return;
        var capital = _states.GetValueOrDefault(faction.CapitalRegionId);
        if (capital is null) return;
        capital.AccruedTaxes += state.AccruedTaxes;
        state.AccruedTaxes = 0;
    }

    private static double RegionalCondition(RegionalEconomyState state) =>
        (state.DiseaseOutbreak ? 0 : 1) * (1 - Math.Clamp(state.Devastation, 0, 1) * 0.75);

    private static double MoveTowards(double current, double target, double maximumDelta) =>
        current < target ? Math.Min(current + maximumDelta, target) : Math.Max(current - maximumDelta, target);

    private void DispatchTaxCaravans(bool includeNpc)
    {
        foreach (var state in _states.Values)
        {
            var region = _world.Region(state.RegionId)!;
            var faction = _world.Faction(region.OwnerFactionId);
            if (faction is null || state.Besieged || !includeNpc && !faction.Player ||
                state.RegionId == faction.CapitalRegionId ||
                _caravans.Any(value => value.OriginRegionId == state.RegionId)) continue;
            var available = state.Stock.Where(value => value.Value > 0).ToArray();
            if (available.Length == 0) continue;
            var caravan = new PeacefulCaravan
            {
                Id = ++_nextCaravanId, FactionId = faction.Id, OriginRegionId = state.RegionId,
                DestinationRegionId = faction.CapitalRegionId,
                Distance = Math.Max(1, Distance(state.RegionId, faction.CapitalRegionId)),
                CurrentRegionId = state.RegionId
            };
            caravan.Route.AddRange(BuildRoute(state.RegionId, faction.CapitalRegionId));
            var remaining = TradeRouteShipmentMaximum;
            foreach (var pair in available)
            {
                var amount = Math.Min(remaining, pair.Value);
                if (amount <= 0) continue;
                caravan.Cargo[pair.Key] = amount;
                state.Stock[pair.Key] -= amount;
                if ((remaining -= amount) <= 0) break;
            }
            if (caravan.Cargo.Count > 0) _caravans.Add(caravan);
        }
    }

    private void AdvanceCaravans(double days)
    {
        foreach (var caravan in _caravans.ToArray())
        {
            caravan.Progress += days;
            while (caravan.Progress >= 1 && caravan.Route.Count > 0)
            {
                caravan.Progress -= 1;
                caravan.CurrentRegionId = caravan.Route[0];
                caravan.Route.RemoveAt(0);
            }
            if (caravan.CurrentRegionId != caravan.DestinationRegionId) continue;
            var destination = _states[caravan.DestinationRegionId];
            foreach (var pair in caravan.Cargo)
                destination.Stock[pair.Key] = destination.Stock.GetValueOrDefault(pair.Key) + pair.Value;
            _caravans.Remove(caravan);
        }
    }

    private void PublishNpcMarkets()
    {
        foreach (var faction in _world.Factions.Where(value => !value.Player))
        {
            var capital = _states[faction.CapitalRegionId];
            var realmPopulation = _states.Values.Where(state =>
                _world.Region(state.RegionId)?.OwnerFactionId == faction.Id).Sum(state => state.Population);
            var stockpileWorkforce = (capital.Population * 0.25 + realmPopulation * 0.15) *
                                     9.0 / ResourceLedger.KindCount;
            var sellers = capital.Stock.Where(value => value.Value > 0)
                .ToDictionary(value => value.Key, value => value.Value);
            var targets = Enum.GetValues<ResourceKind>().ToDictionary(resource => resource, resource =>
                Math.Max(1, (int)Math.Ceiling(1 + ResourceRate(resource) * stockpileWorkforce)));
            var buyers = targets.ToDictionary(pair => pair.Key, pair =>
                Math.Max(0, pair.Value - capital.Stock.GetValueOrDefault(pair.Key)))
                .Where(pair => pair.Value > 0).ToDictionary(pair => pair.Key, pair => pair.Value);
            var sellerPrices = sellers.Keys.ToDictionary(resource => resource, resource =>
                PriceAt(resource, capital.Stock.GetValueOrDefault(resource), targets[resource], -1));
            var buyerPrices = buyers.Keys.ToDictionary(resource => resource, resource =>
                PriceAt(resource, capital.Stock.GetValueOrDefault(resource), targets[resource], 1));
            _world.SetMarket(faction.Id, new StrategicMarket(
                sellerPrices, sellers, buyerPrices, buyers));
        }
    }

    private static double ResourceRate(ResourceKind resource) => OriginalGameData.Current.Rooms.Values
        .SelectMany(room => room.Recipes).SelectMany(recipe => recipe.Outputs)
        .Where(output => output.Resource.Equals(resource.ToString(), StringComparison.OrdinalIgnoreCase))
        .Select(output => output.Rate).DefaultIfEmpty(0.01).Max();

    private static int PriceAt(ResourceKind resource, int amount, int target, int added)
    {
        var after = Math.Max(0, amount + added);
        var multiplier = after <= 0 ? 10.0 : Math.Clamp(target / (double)after, 0.1, 10.0);
        multiplier = added > 0
            ? multiplier < 1 ? multiplier : Math.Clamp(multiplier - 0.4, 1, 2)
            : added < 0 ? Math.Clamp(multiplier + 0.4, 0.5, 1) : multiplier;
        var basePrice = 400 * OriginalGameData.Current.TradeResource(resource.ToString()).PriceMultiplier;
        return Math.Max(1, added > 0 ? (int)(basePrice * multiplier) - 1 :
            (int)Math.Ceiling(basePrice * multiplier) + 1);
    }

    private int Distance(int first, int second)
    {
        var a = _world.Region(first)!; var b = _world.Region(second)!;
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private IEnumerable<int> BuildRoute(int first, int second)
    {
        foreach (var regionId in _world.FindRoute(first, second)) yield return regionId;
    }
}
