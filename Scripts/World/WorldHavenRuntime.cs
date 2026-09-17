using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;

namespace GodotSyxPort.World;

public enum HavenKind : byte
{
    RefugeeCamp,
    FreeSettlement,
    PilgrimCamp,
    TradingPost
}

public sealed class WorldHavenState
{
    public int Id { get; init; }
    public int RegionId { get; init; }
    public int TileX { get; init; }
    public int TileY { get; init; }
    public string TypeKey { get; init; } = "";
    public string Name { get; init; } = "";
    public HavenKind Kind { get; init; }
    public int OwnerFactionId { get; internal set; } = -1;
    public string MajorityRace { get; internal set; } = "HUMAN";
    public int Population { get; internal set; }
    public double Safety { get; internal set; } = 0.5;
    public double Prosperity { get; internal set; } = 0.5;
    public double MigrationAccumulator { get; internal set; }
    public Dictionary<string, int> RacePopulation { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool PlayerControlled { get; internal set; }
    public bool Active => Population > 0 && Safety > 0.05;
}

public sealed record HavenMigrationOffer(
    int HavenId,
    int RegionId,
    string Race,
    HumanoidType Type,
    int Amount,
    double TravelDays,
    string Cause);
public sealed record StrategicHavenPlacement(
    int TileX, int TileY, int RegionId, string TypeKey, string Race, string Name, int Population);

public sealed record HavenSnapshot(
    int Id,
    int RegionId,
    HavenKind Kind,
    int OwnerFactionId,
    string MajorityRace,
    int Population,
    double Safety,
    double Prosperity,
    double MigrationAccumulator,
    IReadOnlyDictionary<string, int> RacePopulation,
    bool PlayerControlled,
    int TileX = -1,
    int TileY = -1,
    string TypeKey = "",
    string Name = "");

/// <summary>
/// Simulation-only WHaven/WHavens adaptation. Captured havens provide bounded,
/// race-aware immigration instead of spawning invented citizens directly.
/// </summary>
public sealed class WorldHavenRuntime
{
    public const int MaximumHavens = 64;
    public const int MaximumOffer = 16;
    public const double BaseMigrationPerDay = 0.125;

    private readonly StrategicWorldRuntime _world;
    private readonly WorldRegionRuntime _regions;
    private readonly Dictionary<int, WorldHavenState> _havens = new();
    private readonly Dictionary<int, List<int>> _byRegion = new();
    private readonly List<HavenMigrationOffer> _offers = new();
    private int _nextId = 1;

    public WorldHavenRuntime(StrategicWorldRuntime world, WorldRegionRuntime regions)
    {
        _world = world;
        _regions = regions;
    }

    public IReadOnlyCollection<WorldHavenState> Havens => _havens.Values;
    public IReadOnlyList<HavenMigrationOffer> Offers => _offers;

    public WorldHavenState? Get(int havenId) => _havens.GetValueOrDefault(havenId);

    public IEnumerable<WorldHavenState> AtRegion(int regionId) =>
        _byRegion.TryGetValue(regionId, out var ids)
            ? ids.Select(id => _havens[id])
            : Enumerable.Empty<WorldHavenState>();

    private sealed record HavenTypeRule(
        string Key, string Race, int PopulationFrom, int PopulationTo,
        IReadOnlyDictionary<string, double> Climates,
        IReadOnlyDictionary<string, double> Terrains,
        IReadOnlyList<string> Names);
    private sealed class HavenPlacementWork
    {
        public required HavenTypeRule Rule { get; init; }
        public double Amount { get; set; }
        public required List<(int Tile, double Score)> Candidates { get; init; }
        public int Index { get; set; }
    }

    /// <summary>world.entity.Generator: place source worldcamp types on ranked world tiles.</summary>
    public void Generate(int seed)
    {
        _havens.Clear(); _byRegion.Clear(); _offers.Clear(); _nextId = 1;
        foreach (var placement in _world.HavenPlacements)
        {
            var owner = _world.Region(placement.RegionId)?.OwnerFactionId ?? -1;
            Create(placement.RegionId, HavenKind.FreeSettlement, placement.Race,
                placement.Population, owner, placement.TileX, placement.TileY,
                placement.TypeKey, placement.Name);
        }
    }

    public static IReadOnlyList<StrategicHavenPlacement> GeneratePlacements(
        StrategicWorldRuntime world, int seed)
    {
        var result = new List<StrategicHavenPlacement>();
        var rules = LoadTypeRules();
        var random = new Random(seed ^ 0x48415645);
        var occupied = new HashSet<int>();
        var work = new List<HavenPlacementWork>();
        foreach (var rule in rules)
        {
            var candidates = new List<(int Tile, double Score)>();
            var amount = 0.0;
            for (var y = 0; y < StrategicWorldRuntime.TileDimension; y++)
            for (var x = 0; x < StrategicWorldRuntime.TileDimension; x++)
            {
                var region = world.RegionAtTile(x, y);
                if (region is null || x == region.CenterTileX && y == region.CenterTileY ||
                    world.Terrain.Mountain(x, y) || world.Terrain.IsWater(x, y) ||
                    world.Terrain.Forest(x, y) > 0.25) continue;
                var climate = rule.Climates.GetValueOrDefault(world.Terrain.Climate(x, y), 1);
                var forest = world.Terrain.Forest(x, y);
                var wet = world.Terrain.Moisture(x, y);
                var none = Math.Max(0, 1 - forest - wet);
                var suitability = climate * (rule.Terrains.GetValueOrDefault("FOREST", 1) * forest +
                    rule.Terrains.GetValueOrDefault("WET", 1) * wet +
                    rule.Terrains.GetValueOrDefault("NONE", 1) * none);
                amount += suitability;
                if (suitability <= 0) continue;
                candidates.Add((x + y * StrategicWorldRuntime.TileDimension,
                    suitability * random.NextDouble()));
            }
            work.Add(new HavenPlacementWork { Rule = rule, Amount = amount,
                Candidates = candidates.OrderByDescending(value => value.Score).Take(1024).ToList() });
        }
        while (work.Count > 0 && result.Count < MaximumHavens)
        {
            var workIndex = random.Next(work.Count);
            var current = work[workIndex];
            if (current.Amount <= 0 || current.Index >= current.Candidates.Count)
            { work.RemoveAt(workIndex); continue; }
            var candidate = current.Candidates[current.Index++];
            var x = candidate.Tile % StrategicWorldRuntime.TileDimension;
            var y = candidate.Tile / StrategicWorldRuntime.TileDimension;
            var blocked = false;
            for (var dy = -1; dy <= 1 && !blocked; dy++)
            for (var dx = -1; dx <= 1; dx++)
                if (x + dx >= 0 && x + dx < StrategicWorldRuntime.TileDimension &&
                    y + dy >= 0 && y + dy < StrategicWorldRuntime.TileDimension &&
                    occupied.Contains(x + dx + (y + dy) * StrategicWorldRuntime.TileDimension))
                { blocked = true; break; }
            if (blocked) continue;
            var rule = current.Rule;
            var region = world.RegionAtTile(x, y)!;
            var race = OriginalGameData.Current.Races.GetValueOrDefault(rule.Race);
            var noble = race?.Surnames.Count > 0
                ? race.Surnames[random.Next(race.Surnames.Count)] : rule.Race;
            var template = rule.Names.Count > 0
                ? rule.Names[random.Next(rule.Names.Count)] : "{0}";
            var population = random.Next(rule.PopulationFrom, rule.PopulationTo + 1);
            result.Add(new StrategicHavenPlacement(x, y, region.Id, rule.Key, rule.Race,
                template.Replace("{0}", noble, StringComparison.Ordinal), population));
            occupied.Add(candidate.Tile);
            current.Amount -= 1;
        }
        return result;
    }

    private static IReadOnlyList<HavenTypeRule> LoadTypeRules()
    {
        var root = ProjectSettings.GlobalizePath("res://Data/Original/init/race/worldcamp");
        var textRoot = ProjectSettings.GlobalizePath("res://Data/Original/text/race/worldcamp");
        if (!Directory.Exists(root)) return Array.Empty<HavenTypeRule>();
        var result = new List<HavenTypeRule>();
        foreach (var file in Directory.EnumerateFiles(root, "*.txt").OrderBy(value => value))
        {
            var data = File.ReadAllText(file);
            var key = Path.GetFileNameWithoutExtension(file);
            var textPath = Path.Combine(textRoot, key + ".txt");
            var names = File.Exists(textPath) ? ParseArray(File.ReadAllText(textPath), "NAMES") : new List<string>();
            result.Add(new HavenTypeRule(
                key,
                ParseText(data, "RACE", key),
                ParseInt(data, "CAMP_SIZE_FROM", 1),
                ParseInt(data, "CAMP_SIZE_TO", 1),
                ParseMap(data, "CLIMATE"),
                ParseMap(data, "TERRAIN"),
                names));
        }
        return result;
    }

    private static string ParseText(string text, string key, string fallback)
    {
        var match = Regex.Match(text, @"(?m)^\s*" + Regex.Escape(key) + @"\s*:\s*([A-Za-z0-9_]+)");
        return match.Success ? match.Groups[1].Value : fallback;
    }

    private static int ParseInt(string text, string key, int fallback)
    {
        var match = Regex.Match(text, @"(?m)^\s*" + Regex.Escape(key) + @"\s*:\s*(\d+)");
        return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : fallback;
    }

    private static Dictionary<string, double> ParseMap(string text, string key)
    {
        var match = Regex.Match(text, @"(?ms)^\s*" + Regex.Escape(key) + @"\s*:\s*\{(.*?)\}");
        return match.Success ? Regex.Matches(match.Groups[1].Value,
                @"(?m)^\s*([A-Z_]+)\s*:\s*([-+]?[0-9]*\.?[0-9]+)")
            .ToDictionary(value => value.Groups[1].Value,
                value => double.Parse(value.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture),
                StringComparer.OrdinalIgnoreCase) : new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> ParseArray(string text, string key)
    {
        var match = Regex.Match(text, @"(?ms)^\s*" + Regex.Escape(key) + @"\s*:\s*\[(.*?)\]");
        return match.Success ? Regex.Matches(match.Groups[1].Value, "\"([^\"]+)\"")
            .Select(value => value.Groups[1].Value).ToList() : new List<string>();
    }

    public WorldHavenState Create(
        int regionId,
        HavenKind kind,
        string majorityRace,
        int population,
        int ownerFactionId = -1,
        int tileX = -1,
        int tileY = -1,
        string typeKey = "",
        string name = "")
    {
        var region = _world.Region(regionId) ??
            throw new ArgumentOutOfRangeException(nameof(regionId));
        if (!region.Habitable && (tileX < 0 || tileY < 0))
            throw new InvalidOperationException("Haven requires habitable land");
        if (_havens.Count >= MaximumHavens)
            throw new InvalidOperationException("World haven capacity reached");
        var haven = new WorldHavenState
        {
            Id = _nextId++,
            RegionId = regionId,
            TileX = tileX,
            TileY = tileY,
            TypeKey = typeKey,
            Name = name,
            Kind = kind,
            OwnerFactionId = ownerFactionId,
            MajorityRace = majorityRace,
            Population = Math.Max(0, population),
            PlayerControlled = ownerFactionId == _world.PlayerFactionId,
            Safety = ownerFactionId < 0 ? 0.45 : 0.7,
            Prosperity = kind == HavenKind.TradingPost ? 0.7 : 0.45
        };
        haven.RacePopulation[majorityRace] = haven.Population;
        _havens.Add(haven.Id, haven);
        if (!_byRegion.TryGetValue(regionId, out var ids))
        {
            ids = new List<int>();
            _byRegion.Add(regionId, ids);
        }
        ids.Add(haven.Id);
        return haven;
    }

    public bool Capture(int havenId, int factionId)
    {
        if (!_havens.TryGetValue(havenId, out var haven)) return false;
        haven.OwnerFactionId = factionId;
        haven.PlayerControlled = factionId == _world.PlayerFactionId;
        haven.Safety = Math.Max(haven.Safety, 0.6);
        return true;
    }

    public bool Abandon(int havenId)
    {
        if (!_havens.TryGetValue(havenId, out var haven)) return false;
        haven.OwnerFactionId = -1;
        haven.PlayerControlled = false;
        haven.Safety = Math.Min(haven.Safety, 0.4);
        return true;
    }

    public void SetRacePopulation(int havenId, string race, int amount)
    {
        var haven = _havens.GetValueOrDefault(havenId) ??
            throw new ArgumentOutOfRangeException(nameof(havenId));
        if (amount <= 0) haven.RacePopulation.Remove(race);
        else haven.RacePopulation[race] = amount;
        haven.Population = haven.RacePopulation.Values.Sum();
        haven.MajorityRace = haven.RacePopulation.Count == 0
            ? ""
            : haven.RacePopulation.MaxBy(pair => pair.Value).Key;
    }

    public void Tick(double days, int playerCapitalRegionId, double settlementAttraction)
    {
        if (days <= 0) return;
        _offers.Clear();
        foreach (var haven in _havens.Values.Where(value => value.Active))
        {
            haven.OwnerFactionId = _world.Region(haven.RegionId)?.OwnerFactionId ?? -1;
            haven.PlayerControlled = haven.OwnerFactionId == _world.PlayerFactionId;
            UpdateLocalState(haven, days);
            if (!haven.PlayerControlled || haven.Population <= 0) continue;
            var route = _world.FindRoute(haven.RegionId, playerCapitalRegionId);
            if (route.Count == 0) continue;
            var attraction = Math.Clamp(settlementAttraction, 0, 1);
            var safetyPressure = 1 - haven.Safety;
            var rate = BaseMigrationPerDay * haven.Population *
                       (0.25 + attraction * 0.75) * (0.5 + safetyPressure);
            haven.MigrationAccumulator += rate * days;
            var amount = Math.Min(MaximumOffer, (int)Math.Floor(haven.MigrationAccumulator));
            if (amount <= 0) continue;
            var race = SelectRace(haven);
            amount = Math.Min(amount, haven.RacePopulation.GetValueOrDefault(race));
            if (amount <= 0) continue;
            haven.MigrationAccumulator -= amount;
            SetRacePopulation(haven.Id, race, haven.RacePopulation.GetValueOrDefault(race) - amount);
            _offers.Add(new HavenMigrationOffer(
                haven.Id,
                haven.RegionId,
                race,
                HumanoidType.Subject,
                amount,
                Math.Max(1, route.Count - 1),
                "HAVEN"));
        }
    }

    public IReadOnlyList<HavenMigrationOffer> ConsumeOffers()
    {
        var copy = _offers.ToArray();
        _offers.Clear();
        return copy;
    }

    public IReadOnlyList<HavenSnapshot> Capture() => _havens.Values.Select(haven => new HavenSnapshot(
        haven.Id,
        haven.RegionId,
        haven.Kind,
        haven.OwnerFactionId,
        haven.MajorityRace,
        haven.Population,
        haven.Safety,
        haven.Prosperity,
        haven.MigrationAccumulator,
            new Dictionary<string, int>(haven.RacePopulation, StringComparer.OrdinalIgnoreCase),
        haven.PlayerControlled, haven.TileX, haven.TileY, haven.TypeKey, haven.Name)).ToArray();

    public void Restore(IEnumerable<HavenSnapshot> snapshots)
    {
        _havens.Clear();
        _byRegion.Clear();
        _nextId = 1;
        foreach (var snapshot in snapshots)
        {
            var haven = new WorldHavenState
            {
                Id = snapshot.Id,
                RegionId = snapshot.RegionId,
                TileX = snapshot.TileX,
                TileY = snapshot.TileY,
                TypeKey = snapshot.TypeKey,
                Name = snapshot.Name,
                Kind = snapshot.Kind,
                OwnerFactionId = snapshot.OwnerFactionId,
                MajorityRace = snapshot.MajorityRace,
                Population = snapshot.Population,
                Safety = Math.Clamp(snapshot.Safety, 0, 1),
                Prosperity = Math.Clamp(snapshot.Prosperity, 0, 1),
                MigrationAccumulator = Math.Max(0, snapshot.MigrationAccumulator),
                PlayerControlled = snapshot.PlayerControlled
            };
            foreach (var pair in snapshot.RacePopulation.Where(pair => pair.Value > 0))
                haven.RacePopulation[pair.Key] = pair.Value;
            _havens.Add(haven.Id, haven);
            if (!_byRegion.TryGetValue(haven.RegionId, out var ids))
            {
                ids = new List<int>();
                _byRegion.Add(haven.RegionId, ids);
            }
            ids.Add(haven.Id);
            _nextId = Math.Max(_nextId, haven.Id + 1);
        }
    }

    private void UpdateLocalState(WorldHavenState haven, double days)
    {
        var region = _regions.CapturePopulation(haven.RegionId);
        var targetSafety = Math.Clamp(region.Health * (1 - region.Devastation), 0, 1);
        var targetProsperity = Math.Clamp(region.Loyalty * (0.5 + region.Health * 0.5), 0, 1);
        haven.Safety = MoveTowards(haven.Safety, targetSafety, days / 32.0);
        haven.Prosperity = MoveTowards(haven.Prosperity, targetProsperity, days / 32.0);
    }

    private static string SelectRace(WorldHavenState haven)
    {
        return haven.RacePopulation.Count == 0
            ? haven.MajorityRace
            : haven.RacePopulation.MaxBy(pair => pair.Value).Key;
    }

    private static double MoveTowards(double current, double target, double delta)
    {
        if (current < target) return Math.Min(target, current + delta);
        return Math.Max(target, current - delta);
    }
}
