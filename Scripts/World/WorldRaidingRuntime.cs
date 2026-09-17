using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.World;

public enum RaiderVisibility : byte { Distant, Hiding, AtLarge, Raiding, Defeated }

public sealed class WorldRaiderRuntime
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Race { get; init; } = "";
    public double Worth { get; init; }
    public int Power { get; init; }
    public double Quality { get; init; }
    public int Raids { get; set; }
    public bool Defeated { get; set; }
}

/// <summary>
/// Source-shaped strategic threat catalogue from game.raiding.RAIDING/Raider/RaidingUtil.
/// Invasion deployment remains owned by the later world-army/battle port; interest,
/// visibility, ransom and relative-strength rules are complete and usable by IManager.
/// </summary>
public sealed class WorldRaidingRuntime
{
    public const int Amount = 100;
    public const int PopulationThreshold = 200;
    public const int PopulationRaiderWorth = 600;
    public const int MaximumPopulation = 40000;
    public const int AveragePrice = 400;
    public const int MenPerArmy = 200 * 120;
    public const double HighPower = 5.0;
    public const int RaidIntervalDays = 6 * 16;
    private readonly List<WorldRaiderRuntime> _raiders = new(Amount);
    private RoomSystem _rooms = null!;
    private CitizenSystem _citizens = null!;

    public IReadOnlyList<WorldRaiderRuntime> All => _raiders;
    public WorldRaiderRuntime? Current { get; private set; }
    public IReadOnlyList<WorldRaiderRuntime> Active => _raiders.Where(value =>
        Status(value) is RaiderVisibility.AtLarge or RaiderVisibility.Raiding).ToArray();

    public void Initialize(int seed, RoomSystem rooms, CitizenSystem citizens)
    {
        _rooms = rooms;
        _citizens = citizens;
        _raiders.Clear();
        var random = new Random(seed ^ 0x52414944);
        var races = OriginalGameData.Current.Races.Values
            .OrderBy(value => value.Key).ToArray();
        if (races.Length == 0) return;
        var playable = races.Where(value => value.Playable).ToArray();
        var maximumWealth = PopulationRaiderWorth * MaximumPopulation +
                            25_000_000.0 / PopulationRaiderWorth;
        for (var i = 0; i < Amount; i++)
        {
            var fraction = i / (double)Amount;
            var race = WeightedRace(races, playable, random);
            var first = Pick(race.FirstNames, random, race.Name);
            var title = Pick(race.RaiderNames, random, "{0} the Raider");
            _raiders.Add(new WorldRaiderRuntime
            {
                Id = i,
                Name = title.Replace("{0}", first, StringComparison.Ordinal),
                Race = race.Key,
                Worth = AveragePrice * 0.75 * PopulationRaiderWorth +
                        Math.Pow(fraction, 2.1) * maximumWealth,
                Power = (int)(5 + MenPerArmy * (1 + (HighPower - 1) / 2) *
                    Math.Pow(fraction, 2.75)),
                Quality = Math.Clamp(i / 10.0, 0, 1)
            });
        }
    }

    public RaiderVisibility Status(WorldRaiderRuntime raider)
    {
        if (raider.Defeated) return RaiderVisibility.Defeated;
        if (ReferenceEquals(Current, raider)) return RaiderVisibility.Raiding;
        if (!HasInterest(raider)) return RaiderVisibility.Distant;
        return raider.Power < PlayerPower() ? RaiderVisibility.Hiding : RaiderVisibility.AtLarge;
    }

    // UIRaiding.statsVisible: known bandits expose their strength; one distant
    // entry remains visible as the next unknown threat in the original list.
    public bool StatsVisible(WorldRaiderRuntime raider)
    {
        if (raider.Defeated || raider.Raids > 0 || ReferenceEquals(Current, raider)) return true;
        if (Status(raider) is RaiderVisibility.Hiding or RaiderVisibility.AtLarge) return true;
        var firstDistant = _raiders.FirstOrDefault(value =>
            !value.Defeated && value.Raids == 0 && Status(value) == RaiderVisibility.Distant);
        return ReferenceEquals(firstDistant, raider);
    }

    public bool PortraitVisible(WorldRaiderRuntime raider) =>
        raider.Defeated || raider.Raids > 0 || ReferenceEquals(Current, raider);

    public bool HasInterest(WorldRaiderRuntime raider) =>
        _rooms.CanCreateRoom("BARRACKS_VANILLA") && _citizens.Count >= PopulationThreshold &&
        CurrentRansom() > raider.Worth;

    public double CurrentRansom(double raidSecurity = 1) =>
        (_citizens.Count * PopulationRaiderWorth +
         _rooms.Governance.Treasury.Balance / PopulationRaiderWorth) /
        Math.Max(0.01, raidSecurity);

    public int PlayerPower()
    {
        var power = 0.0;
        foreach (var division in _rooms.Military.Divisions)
        {
            var training = division.Members.Count == 0 ? 0 : division.Members.Average(id =>
                _citizens.PersonalStats.Ensure(id).BasicTraining / 255.0);
            var equipment = division.EquipmentPerSoldier.Count == 0 ? 0 :
                division.IssuedEquipment.Values.Sum() /
                (double)Math.Max(1, division.EquipmentPerSoldier.Values.Sum() * division.Members.Count);
            power += division.Members.Count * (1 + training * 2 + Math.Clamp(equipment, 0, 1) * 2);
        }
        return (int)power;
    }

    private static RaceRule WeightedRace(IReadOnlyList<RaceRule> races,
        IReadOnlyList<RaceRule> playable, Random random)
    {
        var total = races.Sum(value => Math.Max(0, value.RaidMercenary));
        if (total <= 0)
        {
            var fallback = playable.Count > 0 ? playable : races;
            return fallback[random.Next(fallback.Count)];
        }
        var selected = random.NextDouble() * total;
        foreach (var race in races)
        {
            selected -= Math.Max(0, race.RaidMercenary);
            if (selected <= 0) return race;
        }
        return races[^1];
    }

    private static string Pick(IReadOnlyList<string> values, Random random, string fallback) =>
        values.Count == 0 ? fallback : values[random.Next(values.Count)];
}
