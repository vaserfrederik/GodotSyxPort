using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Trade;

namespace GodotSyxPort.World;

public enum DiplomacyStance : byte { Neutral, War, Trade, Pact, Allied, Vassal, Overlord }

public sealed class StrategicRegion
{
    public int Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int CenterTileX { get; init; }
    public int CenterTileY { get; init; }
    public string Name { get; set; } = "";
    public int OwnerFactionId { get; set; } = -1;
    public bool Capital { get; set; }
    public int Population { get; set; }
    public int Area { get; set; }
    public double Moisture { get; set; }
    public double Fertility { get; set; }
    public double Water { get; set; }
    public double Elevation { get; set; }
    public double Forest { get; set; }
    public double Mountain { get; set; }
    public double Ocean { get; set; }
    public double River { get; set; }
    public string Climate { get; set; } = "TEMPERATE";
    public bool Habitable => Ocean < 0.55 && Mountain < 0.80;
    public List<int> Neighbours { get; } = new();
}

public sealed class StrategicFaction
{
    public int Id { get; init; }
    public string Key { get; init; } = "";
    public string Name { get; set; } = "";
    public string Race { get; set; } = "";
    public int CapitalRegionId { get; set; } = -1;
    public bool Player { get; init; }
    public bool Sanctified { get; set; }
}

public sealed record StrategicMarket(
    IReadOnlyDictionary<ResourceKind, int> SellerPrices,
    IReadOnlyDictionary<ResourceKind, int> SellerAmounts,
    IReadOnlyDictionary<ResourceKind, int> BuyerPrices,
    IReadOnlyDictionary<ResourceKind, int> BuyerAmounts,
    double ImportTariff = 0, double ExportTariff = 0);
public sealed record StrategicRoad(int FirstRegionId, int SecondRegionId, double Cost, bool BridgeOrPort)
{
    public IReadOnlyList<int> TilePath { get; init; } = Array.Empty<int>();
}
public sealed record StrategicCity(int Id, int RegionId, int FactionId, bool Capital);
public enum StrategicRuralSiteKind : byte { Farm, Village }
public sealed record StrategicRuralSite(int TileX, int TileY, int RegionId, StrategicRuralSiteKind Kind);
public enum StrategicLandmarkKind : byte { Mountain, Lake, River, Ocean }
public sealed record StrategicLandmark(
    int Id, StrategicLandmarkKind Kind, string Name, int CenterTileX, int CenterTileY, int Area);
public sealed record StrategicRegionOwnershipSnapshot(int RegionId, int OwnerFactionId, bool Capital);
public sealed record StrategicFactionSnapshot(int FactionId, string Race, int CapitalRegionId);
public sealed record StrategicDiplomacySnapshot(int FirstFactionId, int SecondFactionId, DiplomacyStance Stance);
public sealed record StrategicConquestResult(
    int RegionId, int PreviousFactionId, int NewFactionId,
    bool CapitalRelocated, int NewCapitalRegionId, bool FactionDefeated);
public sealed record StrategicWorldSnapshot(
    IReadOnlyList<StrategicRegionOwnershipSnapshot> Regions,
    IReadOnlyList<StrategicFactionSnapshot> Factions,
    IReadOnlyList<StrategicDiplomacySnapshot> Diplomacy,
    StrategicTerrainSnapshot? Terrain = null);

/// <summary>
/// Non-combat world/region/faction core. Generation keeps the source world size,
/// average region area, 64-faction cap, eight-region average realm and 3/4 fill target.
/// Markets are injected by regional economy code; this runtime never invents stock.
/// </summary>
public sealed class StrategicWorldRuntime
{
    public const int TileDimension = 256;
    // world.map.regions.centre.WCentre.TILE_DIM in the authoritative Java source.
    public const int CapitalFootprintDimension = 3;
    public const int AverageRegionArea = 50;
    public const int MaximumRegions = 1023;
    public const int MaximumFactions = 64;
    public const int AverageRealmSize = 8;
    public const double InitialRealmFill = 0.75;
    // GenAssign uses (WCentre.TILE_DIM + 2)^2 + REGION_SIZE as its placement budget.
    public static readonly int TargetRegionArea =
        (CapitalFootprintDimension + 2) * (CapitalFootprintDimension + 2) + AverageRegionArea;
    public static readonly int RegionsAcross = (int)Math.Round(
        Math.Sqrt(TileDimension * TileDimension / (double)TargetRegionArea));

    private readonly List<StrategicRegion> _regions = new();
    private readonly int[] _regionByTile = new int[TileDimension * TileDimension];
    private readonly int[] _regionByCoarseCell = new int[RegionsAcross * RegionsAcross];
    private readonly List<StrategicFaction> _factions = new();
    private readonly Dictionary<(int A, int B), DiplomacyStance> _diplomacy = new();
    private readonly Dictionary<int, StrategicMarket> _markets = new();
    private readonly Dictionary<(int A, int B), StrategicRoad> _roads = new();
    private readonly HashSet<int> _roadTiles = new();
    private int _generationSeed;
    private readonly List<StrategicCity> _cities = new();
    private readonly List<StrategicRuralSite> _ruralSites = new();
    private readonly List<StrategicLandmark> _landmarks = new();
    private readonly int[] _landmarkByTile = new int[TileDimension * TileDimension];
    private readonly List<StrategicHavenPlacement> _havenPlacements = new();
    public IReadOnlyList<StrategicRegion> Regions => _regions;
    public IReadOnlyList<StrategicFaction> Factions => _factions;
    public IReadOnlyCollection<StrategicRoad> Roads => _roads.Values;
    public IReadOnlyCollection<int> RoadTiles => _roadTiles;
    public IReadOnlyList<StrategicCity> Cities => _cities;
    public IReadOnlyList<StrategicRuralSite> RuralSites => _ruralSites;
    public IReadOnlyList<StrategicLandmark> Landmarks => _landmarks;
    public IReadOnlyList<StrategicHavenPlacement> HavenPlacements => _havenPlacements;
    public StrategicTerrainRuntime Terrain { get; private set; } = new(TileDimension);
    public int PlayerFactionId => 0;
    public int GenerationSeed => _generationSeed;
    public int Revision { get; private set; }

    public void Generate(IReadOnlyDictionary<string, RaceRule> races, int seed = 0x535958,
        StrategicTerrainTemplate? template = null, double latitude = 0.5)
    {
        GenerateTerrain(seed, template, latitude);
        var center = _regions.Where(value => value.Habitable).OrderBy(value =>
            Math.Abs(value.X - RegionsAcross / 2) + Math.Abs(value.Y - RegionsAcross / 2)).First();
        var race = races.Values.Where(value => value.Playable).Select(value => value.Key)
            .DefaultIfEmpty("HUMAN").OrderBy(value => value).First();
        GenerateCivilizations(races, seed, center.Id, race);
    }

    /// <summary>StageTerrain: terrain and neutral placement samples only.</summary>
    public void GenerateTerrain(int seed = 0x535958,
        StrategicTerrainTemplate? template = null, double latitude = 0.5)
    {
        _regions.Clear(); _factions.Clear(); _diplomacy.Clear(); _markets.Clear(); _roads.Clear();
        _cities.Clear(); _ruralSites.Clear(); _havenPlacements.Clear();
        Terrain = new StrategicTerrainRuntime(TileDimension);
        _generationSeed = seed;
        Terrain.Generate(seed, template, latitude);
        RebuildRegionGeometry();
        Revision++;
    }

    /// <summary>
    /// Rebuild the neutral region map from the current terrain. This is the equivalent
    /// of rerunning the Java region generation stage after terrain-editor changes.
    /// </summary>
    private void RebuildRegionGeometry()
    {
        _regions.Clear();
        GenerateLandmarks(_generationSeed);
        Array.Fill(_regionByTile, -1);
        Array.Fill(_regionByCoarseCell, -1);
        var random = new Random(_generationSeed ^ 0x524547);
        var coarseCells = new List<(int X, int Y)>(RegionsAcross * RegionsAcross);
        for (var y = 0; y < RegionsAcross; y++)
        for (var x = 0; x < RegionsAcross; x++) coarseCells.Add((x, y));
        Shuffle(coarseCells, random);
        foreach (var cell in coarseCells)
        {
            if (_regions.Count >= MaximumRegions) break;
            var x = cell.X; var y = cell.Y;
            var center = FindRegionCentre(x, y, random);
            if (center is null) continue;
            var id = _regions.Count;
            _regions.Add(new StrategicRegion
            {
                Id = id, X = x, Y = y,
                CenterTileX = center.Value.X, CenterTileY = center.Value.Y
            });
            _regionByCoarseCell[x + y * RegionsAcross] = id;
        }
        AssignRegionTiles();
        BuildRegionAdjacency();
        AggregateAllTerrain();
        GenerateRegionNames(_generationSeed);
    }

    /// <summary>StageCapitol.generate: factions, capitals, realms and roads after player placement.</summary>
    public void GenerateCivilizations(IReadOnlyDictionary<string, RaceRule> races, int seed,
        int playerRegionId, string playerRace, string? playerFactionName = null)
    {
        if (!CanSetPlayerStart(playerRegionId))
            throw new InvalidOperationException("Player capital must be placed on habitable terrain");
        foreach (var region in _regions)
        {
            region.OwnerFactionId = -1;
            region.Capital = false;
        }
        _factions.Clear(); _diplomacy.Clear(); _markets.Clear();
        GenerateRoads();
        var raceKeys = races.Values.Where(value => value.Playable).Select(value => value.Key)
            .DefaultIfEmpty("HUMAN").OrderBy(value => value).ToArray();
        var center = Region(playerRegionId)!;
        _factions.Add(new StrategicFaction
        {
            Id = 0, Key = "PLAYER", Name = playerFactionName ?? "PLAYER",
            Race = playerRace, Player = true, CapitalRegionId = center.Id
        });
        center.OwnerFactionId = 0; center.Capital = true;
        center.Name = playerFactionName ?? center.Name;

        var random = new Random(seed);
        var handsOff = new HashSet<int>();
        var aroundPlayer = RegionsByDistance(center.Id, handsOff, -1);
        if (aroundPlayer.Count > 2) handsOff.Add(aroundPlayer[random.Next(aroundPlayer.Count)]);

        foreach (var spreadMaximum in new[] { 1, 3, 5 })
        {
            var created = 0;
            foreach (var regionId in RegionsByDistance(center.Id, handsOff, -1))
            {
                var capital = _regions[regionId];
                if (capital.OwnerFactionId >= 0 || !CreateNpcFaction(capital, raceKeys, true)) continue;
                SpreadRealm(capital, capital.OwnerFactionId, random.Next(spreadMaximum), handsOff);
                if (++created >= 2 + spreadMaximum / 2) break;
            }
        }

        var candidates = _regions.Where(region => region.Habitable)
            .OrderBy(_ => random.Next()).Select(region => region.Id).ToList();
        var remaining = 3 * candidates.Count / 4;
        while (remaining > 0 && candidates.Count > 0)
        {
            var regionId = candidates[^1]; candidates.RemoveAt(candidates.Count - 1);
            var capital = _regions[regionId];
            if (capital.OwnerFactionId >= 0 || handsOff.Contains(regionId)) continue;
            if (!CreateNpcFaction(capital, raceKeys, false)) break;
            remaining -= SpreadRealm(capital, capital.OwnerFactionId,
                random.Next(AverageRealmSize * 2), handsOff);
        }
        _havenPlacements.Clear();
        _havenPlacements.AddRange(WorldHavenRuntime.GeneratePlacements(this, seed));
        GenerateRuralSites();
        GenerateCities();
        Revision++;
    }

    /// <summary>
    /// StageCapitol.clear: remove the generated political world while retaining
    /// terrain, landmarks and the neutral region partition used by capital placement.
    /// </summary>
    public void ClearGeneratedCivilizations()
    {
        foreach (var region in _regions)
        {
            region.OwnerFactionId = -1;
            region.Capital = false;
        }
        _factions.Clear();
        _diplomacy.Clear();
        _markets.Clear();
        _roads.Clear();
        _roadTiles.Clear();
        _cities.Clear();
        _ruralSites.Clear();
        _havenPlacements.Clear();
        Revision++;
    }

    public StrategicRegion? At(int x, int y)
    {
        if (x < 0 || y < 0 || x >= RegionsAcross || y >= RegionsAcross) return null;
        return Region(_regionByCoarseCell[x + y * RegionsAcross]);
    }
    public StrategicRegion? RegionAtTile(int tileX, int tileY)
    {
        if ((uint)tileX >= TileDimension || (uint)tileY >= TileDimension) return null;
        return Region(_regionByTile[tileX + tileY * TileDimension]);
    }

    public int RegionIdAtTile(int tileX, int tileY) =>
        (uint)tileX < TileDimension && (uint)tileY < TileDimension
            ? _regionByTile[tileX + tileY * TileDimension] : -1;

    public StrategicLandmark? LandmarkAtTile(int tileX, int tileY)
    {
        if ((uint)tileX >= TileDimension || (uint)tileY >= TileDimension) return null;
        var id = _landmarkByTile[tileX + tileY * TileDimension];
        return id > 0 && id <= _landmarks.Count ? _landmarks[id - 1] : null;
    }

    /// <summary>Equivalent startup guard for the source world resource validators.</summary>
    public IReadOnlyList<string> ValidateGeneratedWorld(int playerRegionId)
    {
        var problems = new List<string>();
        var player = Faction(PlayerFactionId);
        if (player is null) problems.Add("Player faction was not generated.");
        if (player?.CapitalRegionId != playerRegionId || Region(playerRegionId)?.OwnerFactionId != PlayerFactionId)
            problems.Add("Selected region is not the player capital.");
        foreach (var faction in _factions)
        {
            var owned = _regions.Where(region => region.OwnerFactionId == faction.Id)
                .Select(region => region.Id).ToHashSet();
            if (owned.Count == 0) continue;
            var capital = Region(faction.CapitalRegionId);
            if (capital is null || !capital.Capital || capital.OwnerFactionId != faction.Id)
                problems.Add($"Faction {faction.Id} has no valid capital.");
            var reached = new HashSet<int>();
            var queue = new Queue<int>(); queue.Enqueue(faction.CapitalRegionId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!owned.Contains(current) || !reached.Add(current)) continue;
                foreach (var next in _regions[current].Neighbours) queue.Enqueue(next);
            }
            if (reached.Count != owned.Count) problems.Add($"Faction {faction.Id} realm is disconnected.");
        }
        if (_regions.Any(region => region.OwnerFactionId >= 0 && Faction(region.OwnerFactionId) is null))
            problems.Add("A region references a missing faction.");
        if (_regions.Any(region => string.IsNullOrWhiteSpace(region.Name)))
            problems.Add("At least one active region has no name.");
        if (_regions.Count > MaximumRegions)
            problems.Add($"The world exceeds the source limit of {MaximumRegions} regions.");
        if (_regions.Count(region => region.Habitable) > 1)
        {
            var connected = new HashSet<int>();
            var start = _regions.First(region => region.Habitable).Id;
            var queue = new Queue<int>(); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!connected.Add(current)) continue;
                foreach (var road in _roads.Values)
                    if (road.FirstRegionId == current) queue.Enqueue(road.SecondRegionId);
                    else if (road.SecondRegionId == current) queue.Enqueue(road.FirstRegionId);
            }
            if (_regions.Any(region => region.Habitable && !connected.Contains(region.Id)))
                problems.Add("The generated region road graph is disconnected.");
        }
        if (_ruralSites.Any(site => Region(site.RegionId) is null || Terrain.Mountain(site.TileX, site.TileY) ||
                                    IsCoveredWater(site.TileX, site.TileY)))
            problems.Add("A rural site occupies invalid terrain.");
        if (_havenPlacements.Count > WorldHavenRuntime.MaximumHavens ||
            _havenPlacements.Any(haven => Region(haven.RegionId) is null ||
                Terrain.IsWater(haven.TileX, haven.TileY) || Terrain.Mountain(haven.TileX, haven.TileY)))
            problems.Add("A world haven occupies invalid terrain.");
        return problems;
    }

    /// <summary>
    /// WorldCentrePlacablity.terrain for the source 3x3 capital footprint: the
    /// centre must be traversable and must have at least one clear cardinal exit.
    /// A river centre is allowed, while mountains and large bodies of water are not.
    /// </summary>
    public bool CanPlaceCapitalAt(int centerX, int centerY)
    {
        // StageCapitol.placableWhole invokes WorldCentrePlacablity.terrain, not
        // region/regionMini: political regions are generated after this choice.
        return CanPlaceCapitalOnTerrain(centerX, centerY);
    }

    private bool CanPlaceCapitalOnTerrain(int centerX, int centerY)
    {
        const int half = CapitalFootprintDimension / 2;
        var topLeftX = centerX - half;
        var topLeftY = centerY - half;
        if (topLeftX < 1 || topLeftY < 1 ||
            topLeftX + CapitalFootprintDimension >= TileDimension ||
            topLeftY + CapitalFootprintDimension >= TileDimension)
            return false;
        if (!CapitalCentreTraversable(centerX, centerY)) return false;
        return new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) }
            .Any(offset => CapitalCentreTraversable(centerX + offset.X, centerY + offset.Y));
    }

    private bool CapitalCentreTraversable(int x, int y)
    {
        if (Terrain.Mountain(x, y)) return false;
        return Terrain.Water(x, y) is not (StrategicWaterKind.Ocean or
            StrategicWaterKind.DeepOcean or StrategicWaterKind.Lake or StrategicWaterKind.DeepLake);
    }
    public StrategicRegion? Region(int id) => (uint)id < (uint)_regions.Count ? _regions[id] : null;
    public StrategicFaction? Faction(int id) => (uint)id < (uint)_factions.Count ? _factions[id] : null;

    public bool CanSetPlayerStart(int regionId)
    {
        var region = Region(regionId);
        if (region is null || !region.Habitable) return false;
        return !region.Capital || region.OwnerFactionId == PlayerFactionId;
    }

    public bool ConfigurePlayerStart(int regionId, string race)
    {
        if (!CanSetPlayerStart(regionId) || string.IsNullOrWhiteSpace(race)) return false;
        var faction = Faction(PlayerFactionId);
        var selected = Region(regionId);
        if (faction is null || selected is null) return false;
        var previous = Region(faction.CapitalRegionId);
        if (previous is not null)
        {
            previous.Capital = false;
            if (previous.OwnerFactionId == PlayerFactionId) previous.OwnerFactionId = -1;
        }
        selected.OwnerFactionId = PlayerFactionId;
        selected.Capital = true;
        faction.CapitalRegionId = regionId;
        faction.Race = race;
        Revision++;
        return true;
    }

    public bool ApplyTerrainEdit(StrategicTerrainEdit edit, int x, int y, int radius) =>
        Terrain.ApplyEdit(edit, x, y, radius);

    public void FinishTerrainEditing()
    {
        RebuildRegionGeometry();
        _roads.Clear();
        if (_factions.Count > 0 && _regions.Any(region => region.Habitable))
        {
            GenerateRoads();
            GenerateRuralSites();
            GenerateCities();
        }
        Revision++;
    }

    public void RestoreTerrain(StrategicTerrainSnapshot snapshot, int seed)
    {
        _generationSeed = seed;
        Terrain = new StrategicTerrainRuntime(TileDimension);
        Terrain.Restore(snapshot);
        _factions.Clear(); _diplomacy.Clear(); _markets.Clear(); _roads.Clear();
        _roadTiles.Clear(); _cities.Clear(); _ruralSites.Clear(); _havenPlacements.Clear();
        RebuildRegionGeometry();
        Revision++;
    }

    public StrategicWorldSnapshot Capture() => new(
        _regions.Select(value => new StrategicRegionOwnershipSnapshot(
            value.Id, value.OwnerFactionId, value.Capital)).ToArray(),
        _factions.Select(value => new StrategicFactionSnapshot(
            value.Id, value.Race, value.CapitalRegionId)).ToArray(),
        _diplomacy.Select(value => new StrategicDiplomacySnapshot(
            value.Key.A, value.Key.B, value.Value)).ToArray(),
        Terrain.Capture());

    public void Restore(StrategicWorldSnapshot snapshot)
    {
        foreach (var value in snapshot.Regions)
        {
            var region = Region(value.RegionId);
            if (region is null) continue;
            region.OwnerFactionId = value.OwnerFactionId;
            region.Capital = value.Capital;
        }
        foreach (var value in snapshot.Factions)
        {
            var faction = Faction(value.FactionId);
            if (faction is null) continue;
            faction.Race = value.Race;
            faction.CapitalRegionId = value.CapitalRegionId;
        }
        _diplomacy.Clear();
        foreach (var value in snapshot.Diplomacy)
            if (value.FirstFactionId != value.SecondFactionId &&
                Faction(value.FirstFactionId) is not null && Faction(value.SecondFactionId) is not null)
                _diplomacy[Pair(value.FirstFactionId, value.SecondFactionId)] = value.Stance;
        GenerateCities();
        Revision++;
    }

    public DiplomacyStance Stance(int first, int second)
    {
        if (first == second) return DiplomacyStance.Allied;
        return _diplomacy.GetValueOrDefault(Pair(first, second), DiplomacyStance.Neutral);
    }

    public bool SetStance(int first, int second, DiplomacyStance stance)
    {
        if (first == second || Faction(first) is null || Faction(second) is null) return false;
        _diplomacy[Pair(first, second)] = stance;
        Revision++;
        return true;
    }

    public bool IsFactionActive(int factionId) =>
        _regions.Any(region => region.OwnerFactionId == factionId);

    /// <summary>Util.conquer: transfer a region, relocate a surviving capital or clear a defeated realm.</summary>
    public StrategicConquestResult? ConquerRegion(
        int regionId, int newFactionId, double attackerFactionPower, double defenderFactionPower)
    {
        var region = Region(regionId);
        if (region is null || Faction(newFactionId) is null ||
            region.OwnerFactionId < 0 || region.OwnerFactionId == newFactionId) return null;
        var previousFactionId = region.OwnerFactionId;
        var previous = Faction(previousFactionId)!;
        var capitalRelocated = false;
        var defeated = false;
        var newCapitalId = previous.CapitalRegionId;
        if (region.Capital)
        {
            var alternatives = _regions.Where(value => value.OwnerFactionId == previousFactionId &&
                                                        value.Id != regionId).ToArray();
            var random = new Random(_generationSeed ^ regionId * 397 ^ Revision);
            defeated = alternatives.Length == 0 ||
                       defenderFactionPower < attackerFactionPower / 2.0 &&
                       random.Next(alternatives.Length + 1) == 0;
            region.Capital = false;
            if (defeated)
            {
                foreach (var owned in alternatives)
                {
                    owned.OwnerFactionId = -1;
                    owned.Capital = false;
                }
                previous.CapitalRegionId = -1;
                newCapitalId = -1;
            }
            else
            {
                var replacement = alternatives[random.Next(alternatives.Length)];
                replacement.Capital = true;
                previous.CapitalRegionId = replacement.Id;
                newCapitalId = replacement.Id;
                capitalRelocated = true;
            }
        }
        region.OwnerFactionId = newFactionId;
        GenerateCities();
        Revision++;
        return new StrategicConquestResult(regionId, previousFactionId, newFactionId,
            capitalRelocated, newCapitalId, defeated);
    }

    public void SetMarket(int factionId, StrategicMarket market)
    {
        if (Faction(factionId) is null || factionId == PlayerFactionId) return;
        _markets[factionId] = market;
        Revision++;
    }

    public void PublishTradeQuotes(SettlementTradeRuntime trade, double proximityTollBoost = 1)
    {
        trade.ClearQuotes();
        foreach (var pair in _markets)
        {
            var stance = Stance(PlayerFactionId, pair.Key);
            if (stance is not (DiplomacyStance.Trade or DiplomacyStance.Pact or
                DiplomacyStance.Allied or DiplomacyStance.Vassal or DiplomacyStance.Overlord)) continue;
            var faction = Faction(pair.Key)!;
            var market = pair.Value;
            trade.SetQuote(new TradePartnerQuote(faction.Key,
                DistanceBetweenCapitals(PlayerFactionId, pair.Key), proximityTollBoost,
                market.ImportTariff, market.ExportTariff,
                market.SellerPrices, market.SellerAmounts, market.BuyerPrices, market.BuyerAmounts));
        }
    }

    public int DistanceBetweenCapitals(int first, int second)
    {
        var a = Region(Faction(first)?.CapitalRegionId ?? -1);
        var b = Region(Faction(second)?.CapitalRegionId ?? -1);
        return a is null || b is null ? int.MaxValue : FindRoute(a.Id, b.Id).Count;
    }

    public IReadOnlyList<int> FindRoute(int firstRegionId, int secondRegionId)
    {
        if (firstRegionId == secondRegionId) return Array.Empty<int>();
        var distance = new Dictionary<int, double> { [firstRegionId] = 0 };
        var previous = new Dictionary<int, int>();
        var queue = new PriorityQueue<int, double>(); queue.Enqueue(firstRegionId, 0);
        while (queue.TryDequeue(out var current, out var currentDistance))
        {
            if (current == secondRegionId) break;
            if (currentDistance > distance.GetValueOrDefault(current, double.MaxValue)) continue;
            foreach (var next in _regions[current].Neighbours)
            {
                var key = Pair(current, next);
                var step = _roads.TryGetValue(key, out var road) ? Math.Max(0.4, road.Cost * 0.35) :
                    RegionalTravelCost(_regions[next]);
                var candidate = currentDistance + step;
                if (candidate >= distance.GetValueOrDefault(next, double.MaxValue)) continue;
                distance[next] = candidate; previous[next] = current; queue.Enqueue(next, candidate);
            }
        }
        if (!previous.ContainsKey(secondRegionId)) return Array.Empty<int>();
        var route = new List<int>();
        for (var current = secondRegionId; current != firstRegionId; current = previous[current]) route.Add(current);
        route.Reverse(); return route;
    }

    private bool CreateNpcFaction(StrategicRegion capital, IReadOnlyList<string> raceKeys, bool sanctified)
    {
        if (_factions.Count >= MaximumFactions) return false;
        var id = _factions.Count;
        _factions.Add(new StrategicFaction
        {
            Id = id, Key = $"NPC_{id:00}", Race = raceKeys[(id - 1) % raceKeys.Count],
            Name = capital.Name, CapitalRegionId = capital.Id, Sanctified = sanctified
        });
        capital.OwnerFactionId = id;
        capital.Capital = true;
        return true;
    }

    private int SpreadRealm(StrategicRegion capital, int factionId, int amount, IReadOnlySet<int> handsOff)
    {
        if (amount <= 0) return 1;
        var assigned = 0;
        foreach (var id in RegionsByDistance(capital.Id, handsOff, factionId))
        {
            var region = _regions[id];
            if (!region.Habitable || region.OwnerFactionId >= 0) continue;
            region.OwnerFactionId = factionId;
            if (++assigned >= amount) break;
        }
        return 1 + assigned;
    }

    /// <summary>WRegFinder.all ordering used by world.region.Gen.</summary>
    private List<int> RegionsByDistance(int originId, IReadOnlySet<int> handsOff, int allowedFaction)
    {
        var distance = new Dictionary<int, double> { [originId] = 0 };
        var queue = new PriorityQueue<int, double>(); queue.Enqueue(originId, 0);
        while (queue.TryDequeue(out var current, out var value))
        {
            if (value > distance.GetValueOrDefault(current, double.MaxValue)) continue;
            foreach (var next in _regions[current].Neighbours)
            {
                var region = _regions[next];
                if (!region.Habitable || handsOff.Contains(next)) continue;
                if (region.OwnerFactionId >= 0 && region.OwnerFactionId != allowedFaction) continue;
                var road = _roads.GetValueOrDefault(Pair(current, next));
                var step = road is null ? RegionalTravelCost(region) : Math.Max(0.4, road.Cost * 0.35);
                var candidate = value + step;
                if (candidate >= distance.GetValueOrDefault(next, double.MaxValue)) continue;
                distance[next] = candidate;
                queue.Enqueue(next, candidate);
            }
        }
        return distance.Where(pair => pair.Key != originId).OrderBy(pair => pair.Value)
            .ThenBy(pair => pair.Key).Select(pair => pair.Key).ToList();
    }

    private (int X, int Y)? FindRegionCentre(int coarseX, int coarseY, Random random)
    {
        var x0 = coarseX * TileDimension / RegionsAcross;
        var y0 = coarseY * TileDimension / RegionsAcross;
        var x1 = Math.Max(x0 + 1, (coarseX + 1) * TileDimension / RegionsAcross);
        var y1 = Math.Max(y0 + 1, (coarseY + 1) * TileDimension / RegionsAcross);
        (int X, int Y)? best = null;
        var bestValue = double.MinValue;
        for (var y = y0; y < y1; y++)
        for (var x = x0; x < x1; x++)
        {
            if (!CanPlaceCapitalOnTerrain(x, y)) continue;
            var value = Terrain.Fertility(x, y) + Terrain.Forest(x, y) * 0.25 + random.NextDouble() * 0.25;
            if (value <= bestValue) continue;
            bestValue = value; best = (x, y);
        }
        return best;
    }

    private void AssignRegionTiles()
    {
        var distances = new double[_regionByTile.Length];
        Array.Fill(distances, double.PositiveInfinity);
        var queue = new PriorityQueue<(int Tile, int Region), double>();
        foreach (var region in _regions)
        {
            var tile = region.CenterTileX + region.CenterTileY * TileDimension;
            distances[tile] = 0;
            _regionByTile[tile] = region.Id;
            queue.Enqueue((tile, region.Id), 0);
        }
        while (queue.TryDequeue(out var current, out var distance))
        {
            if (_regionByTile[current.Tile] != current.Region || distance > distances[current.Tile]) continue;
            var x = current.Tile % TileDimension;
            var y = current.Tile / TileDimension;
            foreach (var offset in new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) })
            {
                var nx = x + offset.X; var ny = y + offset.Y;
                if ((uint)nx >= TileDimension || (uint)ny >= TileDimension ||
                    !RegionExpandable(x, y, nx, ny)) continue;
                var next = nx + ny * TileDimension;
                var step = TerrainClass(x, y) == TerrainClass(nx, ny) ? 1.0 : 5.0;
                var candidate = distance + step;
                if (candidate >= distances[next]) continue;
                distances[next] = candidate;
                _regionByTile[next] = current.Region;
                queue.Enqueue((next, current.Region), candidate);
            }
        }
    }

    private bool RegionExpandable(int fromX, int fromY, int x, int y)
    {
        if (Terrain.Mountain(fromX, fromY) && Terrain.Mountain(x, y)) return false;
        if (Terrain.Mountain(x, y)) return HasCardinalOpenLand(x, y);
        var water = Terrain.Water(x, y);
        if (water is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
            StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)
            return HasCardinalOpenLand(x, y);
        return true;
    }

    private bool HasCardinalOpenLand(int x, int y)
    {
        foreach (var offset in new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) })
        {
            var nx = x + offset.X; var ny = y + offset.Y;
            if ((uint)nx >= TileDimension || (uint)ny >= TileDimension) continue;
            if (!Terrain.Mountain(nx, ny) && Terrain.Water(nx, ny) is not
                (StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                 StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)) return true;
        }
        return false;
    }

    private int TerrainClass(int x, int y)
    {
        if (Terrain.Mountain(x, y)) return 1;
        if (Terrain.Water(x, y) != StrategicWaterKind.None) return 2;
        if (Terrain.Forest(x, y) >= 0.999) return 3;
        return 0;
    }

    private void BuildRegionAdjacency()
    {
        foreach (var region in _regions) region.Neighbours.Clear();
        var neighbours = _regions.Select(_ => new HashSet<int>()).ToArray();
        for (var y = 0; y < TileDimension; y++)
        for (var x = 0; x < TileDimension; x++)
        {
            var id = RegionIdAtTile(x, y);
            if (id < 0) continue;
            if (x + 1 < TileDimension) AddRegionEdge(id, RegionIdAtTile(x + 1, y), neighbours);
            if (y + 1 < TileDimension) AddRegionEdge(id, RegionIdAtTile(x, y + 1), neighbours);
        }
        for (var id = 0; id < _regions.Count; id++)
            _regions[id].Neighbours.AddRange(neighbours[id].OrderBy(value => value));
    }

    private static void AddRegionEdge(int first, int second, IReadOnlyList<HashSet<int>> neighbours)
    {
        if (first < 0 || second < 0 || first == second) return;
        neighbours[first].Add(second);
        neighbours[second].Add(first);
    }

    private void AggregateAllTerrain()
    {
        var count = new int[_regions.Count];
        var height = new double[_regions.Count];
        var fertility = new double[_regions.Count];
        var forest = new double[_regions.Count];
        var water = new int[_regions.Count];
        var ocean = new int[_regions.Count];
        var river = new int[_regions.Count];
        var mountain = new int[_regions.Count];
        var climates = _regions.Select(_ => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)).ToArray();
        for (var y = 0; y < TileDimension; y++)
        for (var x = 0; x < TileDimension; x++)
        {
            var id = RegionIdAtTile(x, y);
            if (id < 0) continue;
            count[id]++; height[id] += Terrain.Height(x, y); fertility[id] += Terrain.Fertility(x, y);
            forest[id] += Terrain.Forest(x, y); if (Terrain.Mountain(x, y)) mountain[id]++;
            var kind = Terrain.Water(x, y); if (kind != StrategicWaterKind.None) water[id]++;
            if (kind is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean) ocean[id]++;
            if (kind is StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta) river[id]++;
            var climate = Terrain.Climate(x, y);
            climates[id][climate] = climates[id].GetValueOrDefault(climate) + 1;
        }
        foreach (var region in _regions)
        {
            var n = Math.Max(1, count[region.Id]);
            region.Area = count[region.Id];
            region.Elevation = height[region.Id] / n; region.Fertility = fertility[region.Id] / n;
            region.Forest = forest[region.Id] / n; region.Mountain = mountain[region.Id] / (double)n;
            region.Water = water[region.Id] / (double)n; region.Ocean = ocean[region.Id] / (double)n;
            region.River = river[region.Id] / (double)n;
            region.Moisture = Math.Clamp(region.Water + region.Fertility * 0.65 + region.Forest * 0.25, 0, 1);
            region.Climate = climates[region.Id].OrderByDescending(value => value.Value)
                .Select(value => value.Key).FirstOrDefault() ?? "TEMPERATE";
        }
    }

    private sealed record LandmarkNames(List<string> Names, List<string> Addons, List<string> Specials);

    /// <summary>
    /// GeneratorLandmark: split mountain, lake, river and ocean terrain into at most
    /// 255 connected named areas. Source size limits are preserved.
    /// </summary>
    private void GenerateLandmarks(int seed)
    {
        _landmarks.Clear();
        Array.Fill(_landmarkByTile, 0);
        var catalogs = LoadLandmarkNames();
        var random = new Random(seed ^ 0x4c414e44);
        foreach (var catalog in catalogs.Values)
        {
            Shuffle(catalog.Names, random);
            Shuffle(catalog.Specials, random);
        }
        var nameIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var specialIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var tile = 0; tile < _landmarkByTile.Length && _landmarks.Count < 255; tile++)
        {
            if (_landmarkByTile[tile] != 0) continue;
            var x = tile % TileDimension; var y = tile / TileDimension;
            var kind = LandmarkTerrain(x, y);
            if (kind is null) continue;
            var (minimum, maximum) = kind.Value switch
            {
                StrategicLandmarkKind.Mountain => (40, 1000),
                StrategicLandmarkKind.Lake => (20, 10000),
                StrategicLandmarkKind.River => (25, 100),
                _ => (50, 5000)
            };
            var tiles = FloodLandmark(tile, kind.Value, maximum, out var neighbourId);
            if (tiles.Count < minimum)
            {
                foreach (var rejected in tiles) _landmarkByTile[rejected] = neighbourId > 0 ? neighbourId : -1;
                continue;
            }
            var id = _landmarks.Count + 1;
            foreach (var member in tiles) _landmarkByTile[member] = id;
            var centerX = (int)Math.Round(tiles.Average(member => member % TileDimension));
            var centerY = (int)Math.Round(tiles.Average(member => member / TileDimension));
            var center = tiles.OrderBy(member => Math.Abs(member % TileDimension - centerX) +
                                                Math.Abs(member / TileDimension - centerY)).First();
            var key = LandmarkNameKey(kind.Value);
            var name = NextLandmarkName(catalogs, nameIndices, specialIndices, key, id, random);
            _landmarks.Add(new StrategicLandmark(id, kind.Value, name,
                center % TileDimension, center / TileDimension, tiles.Count));
        }
        for (var tile = 0; tile < _landmarkByTile.Length; tile++)
            if (_landmarkByTile[tile] < 0) _landmarkByTile[tile] = 0;
    }

    private List<int> FloodLandmark(int start, StrategicLandmarkKind kind, int maximum, out int neighbourId)
    {
        var result = new List<int>();
        var distance = new Dictionary<int, double> { [start] = 0 };
        var queue = new PriorityQueue<int, double>(); queue.Enqueue(start, 0);
        neighbourId = 0;
        while (queue.TryDequeue(out var tile, out var value) && result.Count <= maximum)
        {
            if (_landmarkByTile[tile] != 0 || LandmarkTerrain(tile % TileDimension, tile / TileDimension) != kind)
                continue;
            _landmarkByTile[tile] = -1;
            result.Add(tile);
            var x = tile % TileDimension; var y = tile / TileDimension;
            foreach (var offset in new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) })
            {
                var nx = x + offset.X; var ny = y + offset.Y;
                if ((uint)nx >= TileDimension || (uint)ny >= TileDimension) continue;
                var next = nx + ny * TileDimension;
                if (_landmarkByTile[next] > 0) { neighbourId = _landmarkByTile[next]; continue; }
                if (_landmarkByTile[next] != 0 || LandmarkTerrain(nx, ny) != kind) continue;
                var candidate = value + (PolymapOwner(x, y) == PolymapOwner(nx, ny) ? 1 : 101);
                if (candidate >= distance.GetValueOrDefault(next, double.MaxValue)) continue;
                distance[next] = candidate; queue.Enqueue(next, candidate);
            }
        }
        return result;
    }

    private StrategicLandmarkKind? LandmarkTerrain(int x, int y)
    {
        if (Terrain.Mountain(x, y) && !Terrain.IsWater(x, y)) return StrategicLandmarkKind.Mountain;
        if (Terrain.Water(x, y) is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)
            return StrategicLandmarkKind.Lake;
        if (Terrain.Water(x, y) is StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta)
            return StrategicLandmarkKind.River;
        if (Terrain.Water(x, y) is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean)
            return StrategicLandmarkKind.Ocean;
        return null;
    }

    private static string LandmarkNameKey(StrategicLandmarkKind kind) => kind switch
    {
        StrategicLandmarkKind.Mountain => "MOUNTAIN",
        StrategicLandmarkKind.Lake => "LAKE",
        StrategicLandmarkKind.River => "RIVER",
        _ => "OCEAN"
    };

    private static Dictionary<string, LandmarkNames> LoadLandmarkNames()
    {
        var result = new Dictionary<string, LandmarkNames>(StringComparer.OrdinalIgnoreCase);
        var path = Godot.ProjectSettings.GlobalizePath("res://Data/Original/text/names/WorldLandmarks.txt");
        if (!File.Exists(path)) return result;
        var text = File.ReadAllText(path);
        foreach (var key in new[] { "MOUNTAIN", "LAKE", "RIVER", "OCEAN" })
        {
            var body = ExtractObject(text, key);
            result[key] = new LandmarkNames(ExtractArray(body, "NAMES"), ExtractArray(body, "ADDONS"),
                Regex.Matches(body, "\\bNAME\\s*:\\s*\"([^\"]+)\"")
                    .Select(match => match.Groups[1].Value).ToList());
        }
        return result;
    }

    private static string ExtractObject(string text, string key)
    {
        var start = text.IndexOf(key + ":", StringComparison.OrdinalIgnoreCase);
        if (start < 0 || (start = text.IndexOf('{', start)) < 0) return "";
        var depth = 0;
        for (var index = start; index < text.Length; index++)
        {
            if (text[index] == '{') depth++;
            else if (text[index] == '}' && --depth == 0) return text[(start + 1)..index];
        }
        return "";
    }

    private static List<string> ExtractArray(string text, string key)
    {
        var match = Regex.Match(text, @"(?ms)\b" + Regex.Escape(key) + @"\s*:\s*\[(.*?)\]");
        return match.Success ? Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\"")
            .Select(value => value.Groups[1].Value).ToList() : new List<string>();
    }

    private static string NextLandmarkName(IReadOnlyDictionary<string, LandmarkNames> catalogs,
        IDictionary<string, int> names, IDictionary<string, int> specials,
        string key, int id, Random random)
    {
        if (!catalogs.TryGetValue(key, out var catalog)) return $"{key} {id}";
        var special = specials.TryGetValue(key, out var specialPosition) ? specialPosition : 0;
        if (special < catalog.Specials.Count)
        {
            specials[key] = special + 1;
            return catalog.Specials[special];
        }
        if (catalog.Names.Count == 0) return $"{key} {id}";
        var index = (names.TryGetValue(key, out var namePosition) ? namePosition : 0) % catalog.Names.Count;
        names[key] = index + 1;
        var name = catalog.Names[index];
        if (catalog.Addons.Count > 0)
            name = catalog.Addons[random.Next(catalog.Addons.Count)].Replace("{0}", name, StringComparison.Ordinal);
        return name;
    }

    /// <summary>GenName: choose a shuffled source name from the dominant terrain family.</summary>
    private void GenerateRegionNames(int seed)
    {
        var source = LoadWorldAreaNames();
        var random = new Random(seed ^ 0x4e414d45);
        foreach (var values in source.Values) Shuffle(values, random);
        var positions = source.Keys.ToDictionary(key => key, _ => 0, StringComparer.OrdinalIgnoreCase);
        var tileCounts = new int[_regions.Count];
        var lakeCounts = new int[_regions.Count];
        var islands = Enumerable.Repeat(true, _regions.Count).ToArray();
        for (var tile = 0; tile < _regionByTile.Length; tile++)
        {
            var regionId = _regionByTile[tile];
            if (regionId < 0) continue;
            tileCounts[regionId]++;
            var x = tile % TileDimension; var y = tile / TileDimension;
            if (Terrain.Water(x, y) is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)
                lakeCounts[regionId]++;
            if (Terrain.IsWater(x, y)) continue;
            foreach (var offset in new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) })
                if (RegionIdAtTile(x + offset.X, y + offset.Y) != regionId)
                    islands[regionId] = false;
        }
        foreach (var region in _regions)
        {
            var family = "MISC";
            var best = 1.0;
            foreach (var candidate in new[]
                     {
                         (Key: "MOUNTAIN", Value: region.Mountain, Threshold: 0.2),
                         (Key: "FOREST", Value: region.Forest, Threshold: 0.4),
                         (Key: "RIVER", Value: region.River, Threshold: 0.1),
                         (Key: "OCEAN", Value: region.Ocean, Threshold: 0.15),
                         (Key: "LAKE", Value: lakeCounts[region.Id] /
                             (double)Math.Max(1, tileCounts[region.Id]), Threshold: 0.15),
                         (Key: region.CenterTileY < TileDimension / 2 ? "STEPPE" : "DESERT",
                             Value: region.Moisture < 0.2 ? 1.0 : 0.0, Threshold: 0.6)
                     })
            {
                var fit = candidate.Value / candidate.Threshold;
                if (fit <= best) continue;
                best = fit; family = candidate.Key;
            }
            var name = NextWorldName(source, positions, family, $"Region {region.Id}");
            if (islands[region.Id])
            {
                var format = NextWorldName(source, positions, "ISLAND_ADDONS", "{0} Island");
                name = format.Replace("{0}", name, StringComparison.Ordinal);
            }
            region.Name = name;
        }
    }

    private static Dictionary<string, List<string>> LoadWorldAreaNames()
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var path = Godot.ProjectSettings.GlobalizePath("res://Data/Original/text/names/WorldAreas.txt");
        if (!File.Exists(path)) return result;
        var text = File.ReadAllText(path);
        foreach (Match match in Regex.Matches(text,
                     @"(?ms)^\s*([A-Z_]+)\s*:\s*\[(.*?)\]"))
            result[match.Groups[1].Value] = Regex.Matches(match.Groups[2].Value, "\"([^\"]+)\"")
                .Select(value => value.Groups[1].Value).ToList();
        return result;
    }

    private static string NextWorldName(IReadOnlyDictionary<string, List<string>> source,
        IDictionary<string, int> positions, string key, string fallback)
    {
        if (!source.TryGetValue(key, out var names) || names.Count == 0) return fallback;
        var index = (positions.TryGetValue(key, out var position) ? position : 0) % names.Count;
        positions[key] = index + 1;
        return names[index];
    }

    private static void Shuffle<T>(IList<T> values, Random random)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var other = random.Next(index + 1);
            (values[index], values[other]) = (values[other], values[index]);
        }
    }

    private void GenerateRoads()
    {
        _roads.Clear();
        _roadTiles.Clear();
        var components = LandComponents();
        if (components.Count == 0) return;
        foreach (var component in components)
        {
            var connected = new HashSet<int> { component[0] };
            var queue = new PriorityQueue<(int From, int To), double>();
            EnqueueRoadCandidates(component[0], connected, queue);
            while (queue.TryDequeue(out var edge, out _))
            {
                if (connected.Contains(edge.To) || !component.Contains(edge.To)) continue;
                // A region must only enter the connected set after an actual tile
                // route was built. The old port marked it connected even when the
                // pathfinder returned no path, producing a disconnected road graph.
                if (!AddRoad(edge.From, edge.To, false) && !AddRoad(edge.From, edge.To, true))
                    continue;
                connected.Add(edge.To);
                EnqueueRoadCandidates(edge.To, connected, queue);
            }
        }
        // GenPort/GenPlayer join otherwise disconnected land groups through the
        // cheapest usable water crossing, then retain a single connected network.
        var joined = new List<int>(components[0]);
        for (var index = 1; index < components.Count; index++)
        {
            var best = ClosestRegions(joined, components[index]);
            AddRoad(best.First, best.Second, true);
            joined.AddRange(components[index]);
        }
        ConnectRemainingRoadComponents();
        PolishRoads();
    }

    private void ConnectRemainingRoadComponents()
    {
        var habitable = _regions.Where(region => region.Habitable).Select(region => region.Id).ToList();
        if (habitable.Count < 2) return;
        while (true)
        {
            var reached = new HashSet<int> { habitable[0] };
            var queue = new Queue<int>(); queue.Enqueue(habitable[0]);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var road in _roads.Values)
                {
                    var next = road.FirstRegionId == current ? road.SecondRegionId :
                        road.SecondRegionId == current ? road.FirstRegionId : -1;
                    if (next >= 0 && reached.Add(next)) queue.Enqueue(next);
                }
            }
            var missing = habitable.Where(id => !reached.Contains(id)).ToList();
            if (missing.Count == 0) return;
            var best = ClosestRegions(reached.ToList(), missing);
            if (!AddRoad(best.First, best.Second, true))
                throw new InvalidOperationException("Unable to connect generated region road graph.");
        }
    }

    /// <summary>
    /// GenPolish.randomRoad: extend the centre network along the edges of a
    /// six-tile polymap, bounded by region area multiplied by moisture.
    /// </summary>
    private void PolishRoads()
    {
        RemoveUnusedRoadTiles();
        var regionArea = new int[_regions.Count];
        foreach (var id in _regionByTile)
            if (id >= 0) regionArea[id]++;
        foreach (var region in _regions)
        {
            var remaining = (int)(regionArea[region.Id] * region.Moisture);
            if (remaining <= 0) continue;
            var start = region.CenterTileX + region.CenterTileY * TileDimension;
            var distance = new Dictionary<int, double> { [start] = 0 };
            var queue = new PriorityQueue<int, double>(); queue.Enqueue(start, 0);
            var visited = new HashSet<int>();
            while (queue.TryDequeue(out var tile, out var value) && remaining > 0)
            {
                if (!visited.Add(tile) || RegionIdAtTile(tile % TileDimension, tile / TileDimension) != region.Id)
                    continue;
                var x = tile % TileDimension; var y = tile / TileDimension;
                if (RegionInterior(region.Id, x, y) && _roadTiles.Add(tile)) remaining--;
                foreach (var offset in new[] { (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1) })
                {
                    var nx = x + offset.X; var ny = y + offset.Y;
                    if ((uint)nx >= TileDimension || (uint)ny >= TileDimension ||
                        RegionIdAtTile(nx, ny) != region.Id) continue;
                    var next = nx + ny * TileDimension;
                    var step = _roadTiles.Contains(next) ? 0.0 : PolymapEdge(nx, ny) &&
                        Terrain.Water(nx, ny) is not (StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                            StrategicWaterKind.Lake or StrategicWaterKind.DeepLake) ? 1.0 : -1.0;
                    if (step < 0) continue;
                    var candidate = value + step;
                    if (candidate >= distance.GetValueOrDefault(next, double.MaxValue)) continue;
                    distance[next] = candidate; queue.Enqueue(next, candidate);
                }
            }
        }
    }

    /// <summary>GenPolish.removeUnusedRoads: remove redundant straight road fill.</summary>
    private void RemoveUnusedRoadTiles()
    {
        var centres = _regions.Select(region =>
            region.CenterTileX + region.CenterTileY * TileDimension).ToHashSet();
        var remove = new List<int>();
        var directions = new[] { (X: 1, Y: 0), (X: 0, Y: 1), (X: -1, Y: 0), (X: 0, Y: -1) };
        foreach (var tile in _roadTiles)
        {
            if (centres.Contains(tile)) continue;
            var x = tile % TileDimension; var y = tile / TileDimension;
            var needed = false; var canBeRemoved = false;
            for (var index = 0; index < directions.Length; index++)
            {
                var direction = directions[index];
                var opposite = directions[(index + 2) % directions.Length];
                var side = directions[(index + 1) % directions.Length];
                if (!HasRoad(x + direction.X, y + direction.Y) ||
                    !HasRoad(x + opposite.X, y + opposite.Y)) continue;
                if (!HasRoad(x + side.X, y + side.Y) ||
                    RegionIdAtTile(x, y) != RegionIdAtTile(x + direction.X, y + direction.Y))
                {
                    needed = true;
                    break;
                }
                canBeRemoved = true;
            }
            if (!needed && canBeRemoved) remove.Add(tile);
        }
        foreach (var tile in remove) _roadTiles.Remove(tile);
    }

    public bool HasRoad(int x, int y) => (uint)x < TileDimension && (uint)y < TileDimension &&
        _roadTiles.Contains(x + y * TileDimension);

    private bool RegionInterior(int regionId, int x, int y)
    {
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
            if (RegionIdAtTile(x + dx, y + dy) != regionId) return false;
        return true;
    }

    private bool PolymapEdge(int x, int y)
    {
        var owner = PolymapOwner(x, y);
        return owner != PolymapOwner(x - 1, y) || owner != PolymapOwner(x + 1, y) ||
               owner != PolymapOwner(x, y - 1) || owner != PolymapOwner(x, y + 1);
    }

    private int PolymapOwner(int x, int y)
    {
        const int cellSize = 6;
        var cellX = FloorDiv(x, cellSize); var cellY = FloorDiv(y, cellSize);
        var bestId = 0; var bestDistance = long.MaxValue;
        for (var cy = cellY - 1; cy <= cellY + 1; cy++)
        for (var cx = cellX - 1; cx <= cellX + 1; cx++)
        {
            var hash = StableHash(cx, cy, _generationSeed);
            var px = cx * cellSize + 1 + (int)(hash & 3);
            var py = cy * cellSize + 1 + (int)((hash >> 2) & 3);
            var dx = x - px; var dy = y - py; var d = (long)dx * dx + (long)dy * dy;
            if (d >= bestDistance) continue;
            bestDistance = d; bestId = unchecked(cx * 73856093 ^ cy * 19349663);
        }
        return bestId;
    }

    private static int FloorDiv(int value, int divisor) => value >= 0
        ? value / divisor : -((-value + divisor - 1) / divisor);

    private static uint StableHash(int x, int y, int seed)
    {
        var value = unchecked((uint)(x * 0x1f123bb5 ^ y * 0x5f356495 ^ seed));
        value ^= value >> 16; value *= 0x7feb352d; value ^= value >> 15;
        value *= 0x846ca68b; value ^= value >> 16;
        return value;
    }

    private List<List<int>> LandComponents()
    {
        var result = new List<List<int>>();
        var seen = new HashSet<int>();
        foreach (var region in _regions.Where(value => value.Habitable))
        {
            if (!seen.Add(region.Id)) continue;
            var component = new List<int>();
            var queue = new Queue<int>(); queue.Enqueue(region.Id);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue(); component.Add(current);
                foreach (var next in _regions[current].Neighbours)
                    if (_regions[next].Habitable && seen.Add(next)) queue.Enqueue(next);
            }
            result.Add(component);
        }
        return result.OrderByDescending(value => value.Count).ToList();
    }

    private (int First, int Second) ClosestRegions(IReadOnlyList<int> first, IReadOnlyList<int> second)
    {
        var best = (First: first[0], Second: second[0]);
        var bestDistance = int.MaxValue;
        foreach (var aId in first)
        foreach (var bId in second)
        {
            var a = _regions[aId]; var b = _regions[bId];
            var dx = a.CenterTileX - b.CenterTileX; var dy = a.CenterTileY - b.CenterTileY;
            var distance = dx * dx + dy * dy;
            if (distance >= bestDistance) continue;
            bestDistance = distance; best = (aId, bId);
        }
        return best;
    }

    private bool AddRoad(int firstRegionId, int secondRegionId, bool allowWater)
    {
        if (firstRegionId == secondRegionId) return true;
        if (_roads.ContainsKey(Pair(firstRegionId, secondRegionId))) return true;
        var path = FindRoadTilePath(firstRegionId, secondRegionId, allowWater);
        if (path.Count == 0) return false;
        var cost = 0.0; var bridgeOrPort = false;
        foreach (var tile in path)
        {
            _roadTiles.Add(tile);
            var x = tile % TileDimension; var y = tile / TileDimension;
            cost += RoadTerrainCost(x, y);
            bridgeOrPort |= Terrain.Water(x, y) is StrategicWaterKind.Ocean or
                StrategicWaterKind.DeepOcean or StrategicWaterKind.Lake or StrategicWaterKind.DeepLake;
        }
        _roads[Pair(firstRegionId, secondRegionId)] = new StrategicRoad(
            firstRegionId, secondRegionId, cost, bridgeOrPort) { TilePath = path };
        return true;
    }

    private IReadOnlyList<int> FindRoadTilePath(int firstRegionId, int secondRegionId, bool allowWater)
    {
        var first = _regions[firstRegionId]; var second = _regions[secondRegionId];
        var start = first.CenterTileX + first.CenterTileY * TileDimension;
        var goal = second.CenterTileX + second.CenterTileY * TileDimension;
        var distance = new Dictionary<int, double> { [start] = 0 };
        var previous = new Dictionary<int, int>();
        var queue = new PriorityQueue<int, double>(); queue.Enqueue(start, 0);
        while (queue.TryDequeue(out var current, out var priority))
        {
            if (current == goal) break;
            var currentDistance = distance.GetValueOrDefault(current, double.MaxValue);
            var cx = current % TileDimension; var cy = current / TileDimension;
            var heuristicAtCurrent = Math.Abs(cx - second.CenterTileX) + Math.Abs(cy - second.CenterTileY);
            if (priority > currentDistance + heuristicAtCurrent) continue;
            foreach (var offset in new[]
                     {
                         (X: 0, Y: -1), (X: -1, Y: 0), (X: 1, Y: 0), (X: 0, Y: 1),
                         (X: -1, Y: -1), (X: 1, Y: -1), (X: -1, Y: 1), (X: 1, Y: 1)
                     })
            {
                var x = cx + offset.X; var y = cy + offset.Y;
                if ((uint)x >= TileDimension || (uint)y >= TileDimension) continue;
                var regionId = RegionIdAtTile(x, y);
                if (!allowWater && regionId != firstRegionId && regionId != secondRegionId) continue;
                var water = Terrain.Water(x, y);
                if (!allowWater && water is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                    StrategicWaterKind.Lake or StrategicWaterKind.DeepLake) continue;
                var next = x + y * TileDimension;
                var diagonal = offset.X != 0 && offset.Y != 0;
                var step = RoadTerrainCost(x, y) * (diagonal ? 1.41421356237 : 1.0);
                if (_roadTiles.Contains(next)) step *= 0.5;
                if (regionId != RegionIdAtTile(cx, cy)) step *= 2.0;
                var candidate = currentDistance + step;
                if (candidate >= distance.GetValueOrDefault(next, double.MaxValue)) continue;
                distance[next] = candidate; previous[next] = current;
                var heuristic = Math.Abs(x - second.CenterTileX) + Math.Abs(y - second.CenterTileY);
                queue.Enqueue(next, candidate + heuristic);
            }
        }
        if (start != goal && !previous.ContainsKey(goal)) return Array.Empty<int>();
        var path = new List<int> { goal };
        for (var current = goal; current != start;)
        {
            current = previous[current]; path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private double RoadTerrainCost(int x, int y)
    {
        if (Terrain.Water(x, y) is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
            StrategicWaterKind.Lake or StrategicWaterKind.DeepLake) return 1;
        if (Terrain.Mountain(x, y)) return 12;
        if (Terrain.Forest(x, y) >= 0.999) return 6;
        return 3;
    }

    /// <summary>
    /// WorldGeneratorBuildings: mark rural farm/village sites after roads exist.
    /// The original stores both branches in its world village bitmap; Kind is retained
    /// here so the Godot renderer can select the corresponding original-style marker.
    /// </summary>
    private void GenerateRuralSites()
    {
        const double neighbourWeightMaximum = 6.8;
        _ruralSites.Clear();
        var centres = _regions.Select(region =>
            region.CenterTileX + region.CenterTileY * TileDimension).ToHashSet();
        var random = new Random(_generationSeed ^ 0x4255494c);
        for (var y = 0; y < TileDimension; y++)
        for (var x = 0; x < TileDimension; x++)
        {
            var tile = x + y * TileDimension;
            if (centres.Contains(tile) || IsCoveredWater(x, y)) continue;
            var connectivity = 0.0;
            var freshWater = 0.0;
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var nx = x + dx; var ny = y + dy;
                if ((uint)nx >= TileDimension || (uint)ny >= TileDimension) continue;
                var weight = 1.0 / Math.Sqrt(dx * dx + dy * dy);
                if (HasRoad(nx, ny)) connectivity += weight;
                if (IsFertileWater(nx, ny)) freshWater += weight;
            }
            connectivity = Math.Sqrt(connectivity / neighbourWeightMaximum);
            freshWater = Math.Sqrt(freshWater / neighbourWeightMaximum);
            if (Terrain.Mountain(x, y)) continue;
            var moisture = Terrain.Moisture(x, y);
            var climate = OriginalGameData.Current.Climates.GetValueOrDefault(Terrain.Climate(x, y));
            var seasonChange = climate?.SeasonalChange ?? 0;
            var farmChance = (moisture - 0.25) / 0.75;
            farmChance *= 0.1 + 0.9 * (1.0 - seasonChange);
            farmChance += freshWater * (0.5 + random.NextDouble());
            var farmRoll = random.NextDouble();
            StrategicRuralSiteKind? kind = farmRoll * farmRoll < farmChance
                ? StrategicRuralSiteKind.Farm : null;
            if (kind is null)
            {
                var villageChance = 0.1 + 0.9 * connectivity;
                villageChance *= 0.25 + 2.5 * (moisture - 0.25) / 0.75;
                villageChance *= Terrain.SettlementPattern(x, y, _generationSeed);
                if (random.NextDouble() < villageChance) kind = StrategicRuralSiteKind.Village;
            }
            var regionId = RegionIdAtTile(x, y);
            if (kind is not null && regionId >= 0)
                _ruralSites.Add(new StrategicRuralSite(x, y, regionId, kind.Value));
        }
    }

    private bool IsCoveredWater(int x, int y) => Terrain.Water(x, y) is
        StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
        StrategicWaterKind.Lake or StrategicWaterKind.DeepLake;

    private bool IsFertileWater(int x, int y) => Terrain.Water(x, y) is
        StrategicWaterKind.Lake or StrategicWaterKind.DeepLake or StrategicWaterKind.River or
        StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta;

    private void GenerateCities()
    {
        _cities.Clear();
        // WorldRaceSheet.Town renders a settlement centre for every populated realm region;
        // Region.Capitol selects the larger capital variant.
        foreach (var region in _regions.Where(region => region.OwnerFactionId >= 0).OrderBy(region => region.Id))
            _cities.Add(new StrategicCity(_cities.Count, region.Id, region.OwnerFactionId, region.Capital));
    }

    private void EnqueueRoadCandidates(int regionId, HashSet<int> connected,
        PriorityQueue<(int From, int To), double> queue)
    {
        foreach (var next in _regions[regionId].Neighbours)
            if (!connected.Contains(next))
            {
                var a = _regions[regionId]; var b = _regions[next];
                var distance = Math.Abs(a.CenterTileX - b.CenterTileX) + Math.Abs(a.CenterTileY - b.CenterTileY);
                queue.Enqueue((regionId, next), distance * RegionalTravelCost(b));
            }
    }

    private static double RegionalTravelCost(StrategicRegion region)
    {
        var cost = region.Mountain >= 0.08 ? 12 : region.Forest >= 0.45 ? 6 : 3;
        if (region.Water >= 0.35) cost *= region.Ocean >= 0.35 ? 6 : 2;
        return cost;
    }

    private static (int A, int B) Pair(int first, int second) =>
        first < second ? (first, second) : (second, first);

    private static double UnitHash(int id, int seed, int salt) =>
        ((uint)HashCode.Combine(id, seed, salt) & 0x00FFFFFF) / 16777215.0;
}
