using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;
using GodotSyxPort.Trade;

namespace GodotSyxPort.Governance;

public enum TreasuryCategory : byte
{
    Trade, Inflation, Misc, Tribute, Diplomacy, Mercenaries,
    Tourism, Construction, Tax, Slaves
}

public sealed record TreasuryFlow(TreasuryCategory Category, double Income, double Expense);
public sealed record TreasurySnapshot(double Balance, IReadOnlyList<TreasuryFlow> History);
public sealed record NobleSnapshot(int CitizenId, int Rank, string OfficeKey);
public sealed record PlayerProgressionSnapshot(int Level, double LevelTime, IReadOnlyList<string> Titles);
public sealed record SettlementGovernanceSnapshot(
    TreasurySnapshot Treasury,
    IReadOnlyList<NobleSnapshot> Nobles,
    PlayerProgressionSnapshot Progression);

/// <summary>FCredits/PCredits without the old UI histories.</summary>
public sealed class TreasuryRuntime
{
    public const int HistoryDays = 48;
    public const int SourceDaysPerYear = 16;
    public const double AnnualInflation = 0.2;
    private readonly Dictionary<TreasuryCategory, double> _income = new();
    private readonly Dictionary<TreasuryCategory, double> _expense = new();
    private readonly Queue<TreasuryFlow> _history = new();
    private double _lastTradeCredits;
    private int _daysInYear;

    public double Balance { get; private set; }
    public double YearlyProfit => _income.Values.Sum() - _expense.Values.Sum();
    public double YearlyTurnover => _income.Values.Sum() + _expense.Values.Sum();
    public IReadOnlyCollection<TreasuryFlow> History => _history;

    public TreasurySnapshot Capture() => new(Balance, _history.ToArray());
    public void Restore(TreasurySnapshot snapshot, SettlementTradeRuntime trade)
    {
        _income.Clear(); _expense.Clear(); _history.Clear();
        foreach (var flow in snapshot.History.Take(HistoryDays * Enum.GetValues<TreasuryCategory>().Length))
            _history.Enqueue(flow);
        _daysInYear = 0;
        Set(snapshot.Balance, trade);
    }

    public void Set(double amount, SettlementTradeRuntime trade)
    {
        Balance = amount;
        trade.Credits = amount;
        _lastTradeCredits = amount;
    }

    public void Add(double amount, TreasuryCategory category, SettlementTradeRuntime trade)
    {
        Balance += amount;
        if (amount >= 0) _income[category] = _income.GetValueOrDefault(category) + amount;
        else _expense[category] = _expense.GetValueOrDefault(category) - amount;
        trade.Credits = Balance;
        _lastTradeCredits = Balance;
    }

    public void BeginDay(SettlementTradeRuntime trade, double deflation = 1)
    {
        var traded = trade.Credits - _lastTradeCredits;
        if (Math.Abs(traded) > 0.0001)
        {
            Balance = trade.Credits;
            if (traded >= 0) _income[TreasuryCategory.Trade] =
                _income.GetValueOrDefault(TreasuryCategory.Trade) + traded;
            else _expense[TreasuryCategory.Trade] =
                _expense.GetValueOrDefault(TreasuryCategory.Trade) - traded;
        }
        var inflation = Balance * AnnualInflation /
                        (SourceDaysPerYear * Math.Max(0.01, deflation));
        Add(-inflation, TreasuryCategory.Inflation, trade);
        foreach (var category in Enum.GetValues<TreasuryCategory>())
            _history.Enqueue(new TreasuryFlow(category,
                _income.GetValueOrDefault(category), _expense.GetValueOrDefault(category)));
        while (_history.Count > HistoryDays * Enum.GetValues<TreasuryCategory>().Length)
            _history.Dequeue();
        if (++_daysInYear >= SourceDaysPerYear)
        {
            _daysInYear = 0;
            _income.Clear();
            _expense.Clear();
        }
        _lastTradeCredits = trade.Credits;
    }
}

public sealed record NobleOfficeRuntime(string Key, string RoomKey, double Boost, bool Governor);

public sealed class NobleRuntime
{
    public int CitizenId { get; init; }
    public int Rank { get; set; }
    public string OfficeKey { get; set; } = "";
}

/// <summary>NOBLES/Noble/NobleOffice shared allocation model.</summary>
public sealed class NobilityRuntime
{
    public const int MaximumNobles = 256;
    public const int RankAllocationIncrease = 2;
    public const int WorkersPerAllocation = 50;
    public const int GovernorPointsPerAllocation = 20;
    private readonly Dictionary<int, NobleRuntime> _nobles = new();
    private readonly Dictionary<string, NobleOfficeRuntime> _offices =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<NobleRuntime> Active => _nobles.Values;
    public IReadOnlyCollection<NobleOfficeRuntime> Offices => _offices.Values;
    public int RankLimit { get; set; } = 10;
    public int NobleCapacity { get; set; } = MaximumNobles;
    public int PromotionCapacity { get; set; } = int.MaxValue;
    public int RanksAllocated => _nobles.Values.Sum(value => value.Rank);

    public void SynchronizeOffices(IEnumerable<RoomRecord> rooms)
    {
        _offices.Clear();
        _offices["GOVERNOR"] = new NobleOfficeRuntime("GOVERNOR", "", 20_000, true);
        foreach (var key in rooms.Where(room => room.State == RoomState.Operational)
                     .Select(room => room.DefinitionKey).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var rule = OriginalGameData.Current.Room(key);
            if (rule is null || rule.Archetype is not (RoomArchetype.Industry or
                    RoomArchetype.Agriculture or RoomArchetype.Extraction or RoomArchetype.Civic)) continue;
            var officeKey = "MASTER_" + key;
            _offices[officeKey] = new NobleOfficeRuntime(officeKey, key, 2.5, false);
        }
    }

    public bool Appoint(int citizenId, string officeKey = "")
    {
        if (_nobles.ContainsKey(citizenId) || _nobles.Count >= Math.Min(MaximumNobles, NobleCapacity))
            return false;
        if (officeKey.Length > 0 && !_offices.ContainsKey(officeKey)) return false;
        _nobles[citizenId] = new NobleRuntime { CitizenId = citizenId, OfficeKey = officeKey };
        return true;
    }

    public bool Dismiss(int citizenId) => _nobles.Remove(citizenId);

    public bool Promote(int citizenId)
    {
        if (!_nobles.TryGetValue(citizenId, out var noble) || noble.Rank >= RankLimit - 1 ||
            RanksAllocated >= PromotionCapacity) return false;
        noble.Rank++;
        return true;
    }

    public bool SetOffice(int citizenId, string officeKey)
    {
        if (!_nobles.TryGetValue(citizenId, out var noble) || !_offices.ContainsKey(officeKey)) return false;
        noble.OfficeKey = officeKey;
        return true;
    }

    public int Allocations(string officeKey) => _nobles.Values.Where(value =>
            value.OfficeKey.Equals(officeKey, StringComparison.OrdinalIgnoreCase))
        .Sum(value => 1 + RankAllocationIncrease * value.Rank);

    public double OfficeCoverage(string officeKey, int employed)
    {
        var allocations = Allocations(officeKey);
        return employed <= 0 ? (allocations > 0 ? 1 : 0) :
            Math.Clamp(allocations * WorkersPerAllocation / (double)employed, 0, 1);
    }

    public int GovernorPoints => Allocations("GOVERNOR") * GovernorPointsPerAllocation;

    public IReadOnlyList<NobleSnapshot> Capture() => _nobles.Values
        .Select(value => new NobleSnapshot(value.CitizenId, value.Rank, value.OfficeKey)).ToArray();

    public void Restore(IEnumerable<NobleSnapshot> snapshots)
    {
        _nobles.Clear();
        foreach (var value in snapshots.Take(MaximumNobles))
            _nobles[value.CitizenId] = new NobleRuntime
            {
                CitizenId = value.CitizenId,
                Rank = Math.Clamp(value.Rank, 0, Math.Max(0, RankLimit - 1)),
                OfficeKey = _offices.ContainsKey(value.OfficeKey) ? value.OfficeKey : ""
            };
    }
}

public sealed record MonumentRuntime(int RoomId, string Key, int Area, double Degradation,
    double Upgrade, int MaximumEnvironment, GridCoord Anchor);

/// <summary>Settlement administration, throne and monument aggregation.</summary>
public sealed class SettlementGovernanceRuntime
{
    private readonly KnowledgeRuntime _administration;
    private readonly SettlementTradeRuntime _trade;
    private readonly Dictionary<int, MonumentRuntime> _monuments = new();
    public TreasuryRuntime Treasury { get; } = new();
    public NobilityRuntime Nobility { get; } = new();
    public PlayerProgressionRuntime Progression { get; } = new();
    public GridCoord? Throne { get; private set; }
    public IReadOnlyCollection<MonumentRuntime> Monuments => _monuments.Values;
    public double Administration => _administration.Currency("CIVIC_ADMIN") + Nobility.GovernorPoints;
    public double Diplomacy => _administration.Currency("CIVIC_DIPLOMACY") +
                               _administration.Currency("DIPLOMACY");

    public SettlementGovernanceRuntime(KnowledgeRuntime administration, SettlementTradeRuntime trade)
    {
        _administration = administration;
        _trade = trade;
        Nobility.RankLimit = Math.Max(1, OriginalGameData.Current.NobleRankNames.Count);
    }

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var active = rooms.Where(room => room.State == RoomState.Operational).ToArray();
        var throne = active.FirstOrDefault(room => room.DefinitionKey == "_THRONE");
        Throne = throne?.Cells.Count > 0 ? fromIndex(throne.Cells.First()) : null;
        _monuments.Clear();
        foreach (var room in active.Where(room => room.DefinitionKey.StartsWith(
                     "MONUMENT_", StringComparison.OrdinalIgnoreCase)))
        {
            var rule = OriginalGameData.Current.Room(room.DefinitionKey);
            var maximum = room.DefinitionKey switch
            {
                "MONUMENT_TORCH" => 1, "MONUMENT_DEATH" => 4, _ => 8
            };
            _monuments[room.Id] = new MonumentRuntime(room.Id, room.DefinitionKey,
                room.Cells.Count, room.Degradation,
                rule is null || rule.Upgrades.Count == 0 ? 1 :
                    (room.UpgradeLevel + 1.0) / (rule.Upgrades.Count + 1.0),
                maximum, room.Cells.Count == 0 ? default : fromIndex(room.Cells.First()));
        }
        Nobility.SynchronizeOffices(active);
    }

    public void BeginDay(double deflation = 1) => Treasury.BeginDay(_trade, deflation);

    public SettlementGovernanceSnapshot Capture() => new(
        Treasury.Capture(), Nobility.Capture(), Progression.Capture());

    public void Restore(SettlementGovernanceSnapshot snapshot)
    {
        Treasury.Restore(snapshot.Treasury, _trade);
        Nobility.Restore(snapshot.Nobles);
        Progression.Restore(snapshot.Progression);
    }
}

/// <summary>Data-driven PLevels/PTitles requirement and unlock state.</summary>
public sealed class PlayerProgressionRuntime
{
    private readonly HashSet<string> _titles = new(StringComparer.OrdinalIgnoreCase);
    private double _levelTime;
    public int Level { get; private set; }
    public IReadOnlySet<string> Titles => _titles;
    public PlayerMilestoneRule? CurrentLevel => OriginalGameData.Current.PlayerLevels.Count == 0
        ? null : OriginalGameData.Current.PlayerLevels[Math.Clamp(
            Level, 0, OriginalGameData.Current.PlayerLevels.Count - 1)];

    public PlayerProgressionSnapshot Capture() => new(Level, _levelTime, _titles.ToArray());
    public void Restore(PlayerProgressionSnapshot snapshot)
    {
        Level = Math.Clamp(snapshot.Level, 0, Math.Max(0, OriginalGameData.Current.PlayerLevels.Count - 1));
        _levelTime = Math.Max(0, snapshot.LevelTime);
        _titles.Clear();
        foreach (var title in snapshot.Titles)
            if (OriginalGameData.Current.PlayerTitles.ContainsKey(title)) _titles.Add(title);
    }

    public void Tick(double delta, IReadOnlyDictionary<string, double> values)
    {
        var levels = OriginalGameData.Current.PlayerLevels;
        if (Level + 1 < levels.Count && Passes(levels[Level + 1], values))
        {
            _levelTime += Math.Max(0, delta);
            if (_levelTime >= OriginalGameData.Current.SecondsPerDay)
            {
                Level++;
                _levelTime = 0;
            }
        }
        else _levelTime = 0;
        foreach (var title in OriginalGameData.Current.PlayerTitles.Values)
            if (Passes(title, values)) _titles.Add(title.Key);
    }

    public IEnumerable<string> FactionUnlocks() => OriginalGameData.Current.PlayerLevels
        .Take(Level + 1).SelectMany(value => value.FactionUnlocks)
        .Concat(_titles.SelectMany(key => OriginalGameData.Current.PlayerTitles[key].FactionUnlocks))
        .Distinct(StringComparer.OrdinalIgnoreCase);

    public double Boost(string key, bool multiplicative = false)
    {
        var values = OriginalGameData.Current.PlayerLevels.Take(Level + 1)
            .Concat(_titles.Select(title => OriginalGameData.Current.PlayerTitles[title]))
            .SelectMany(rule => rule.Boosts)
            .Where(pair => pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Value).ToArray();
        return multiplicative ? values.Aggregate(1.0, (value, next) => value * next) : values.Sum();
    }

    private static bool Passes(PlayerMilestoneRule rule, IReadOnlyDictionary<string, double> values)
    {
        foreach (var requirement in rule.Requirements)
        {
            var split = requirement.Key.Split('>');
            var operation = split.Length > 1 ? split[^2] : "GREATERE";
            var key = split[^1];
            var actual = values.GetValueOrDefault(key);
            if (operation.StartsWith("LESS", StringComparison.OrdinalIgnoreCase))
            {
                if (actual > requirement.Value) return false;
            }
            else if (actual < requirement.Value) return false;
        }
        return true;
    }
}
