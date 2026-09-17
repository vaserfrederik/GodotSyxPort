using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.World;

public sealed record SettlementWorldProfile(
    int CapitalRegionId,
    string Climate,
    double Fertility,
    double Moisture,
    double Water,
    int WorldPopulation,
    IReadOnlyDictionary<string, int> RacePopulation,
    IReadOnlyDictionary<ResourceKind, int> RegionalStock,
    int Revision);

public sealed record SettlementPopulationDelta(
    string Race,
    int SettlementCount,
    int PreviousSettlementCount,
    int WorldCount,
    int Delta,
    string Cause);

public sealed record SettlementWorldNotice(
    int Day,
    string Kind,
    string Message,
    int RegionId,
    string Race,
    int Amount);

/// <summary>
/// Connects the selected player capital to the 768x768 settlement. The bridge owns
/// transfer accounting, not citizens or world regions, so either simulation can be
/// saved and rebuilt independently.
/// </summary>
public sealed class WorldSettlementBridge
{
    private readonly StrategicWorldRuntime _world;
    private readonly WorldRegionRuntime _regions;
    private readonly WorldHavenRuntime _havens;
    private readonly SettlementEntryRuntime _entry;
    private readonly Dictionary<string, int> _lastSettlementPopulation =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _arrivalRemainders =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SettlementPopulationDelta> _populationDeltas = new();
    private readonly List<SettlementWorldNotice> _notices = new();
    private int _lastCapitalRevision = -1;

    public WorldSettlementBridge(
        StrategicWorldRuntime world,
        WorldRegionRuntime regions,
        WorldHavenRuntime havens,
        SettlementEntryRuntime entry)
    {
        _world = world;
        _regions = regions;
        _havens = havens;
        _entry = entry;
    }

    public SettlementWorldProfile? Profile { get; private set; }
    public IReadOnlyList<SettlementPopulationDelta> PopulationDeltas => _populationDeltas;
    public IReadOnlyList<SettlementWorldNotice> Notices => _notices;
    public bool Connected => Profile is not null;

    public bool ConnectToPlayerCapital()
    {
        var capital = _regions.PlayerCapital();
        if (capital is null) return false;
        ApplyCapital(capital);
        return true;
    }

    public bool ConnectToRegion(int regionId)
    {
        var region = _world.Region(regionId);
        if (region is null || region.OwnerFactionId != _world.PlayerFactionId || !region.Capital)
            return false;
        ApplyCapital(_regions.CaptureCapital(regionId));
        return true;
    }

    public void Tick(
        double days,
        int day,
        IReadOnlyDictionary<string, int> settlementPopulation,
        double settlementAttraction)
    {
        if (days <= 0 || Profile is null) return;
        RefreshProfile();
        SynchronizePopulation(day, settlementPopulation);
        _havens.Tick(days, Profile.CapitalRegionId, settlementAttraction);
        foreach (var offer in _havens.ConsumeOffers())
        {
            _entry.Add(offer.Race, offer.Type, offer.Amount);
            _notices.Add(new SettlementWorldNotice(
                day,
                "HAVEN_IMMIGRATION",
                $"{offer.Amount} {offer.Race} are travelling from haven #{offer.HavenId}",
                offer.RegionId,
                offer.Race,
                offer.Amount));
        }
        TrimHistory();
    }

    public int QueueRegionalImmigration(
        string race,
        double requested,
        HumanoidType type,
        int day,
        string cause = "WORLD_MIGRATION")
    {
        if (Profile is null || requested <= 0) return 0;
        var available = Profile.RacePopulation.GetValueOrDefault(race);
        if (available <= 0) return 0;
        var total = requested + _arrivalRemainders.GetValueOrDefault(race);
        var amount = Math.Min(available, (int)Math.Floor(total));
        _arrivalRemainders[race] = total - amount;
        if (amount <= 0) return 0;
        var removed = _regions.RemovePopulation(Profile.CapitalRegionId, race, amount, cause, day);
        if (removed <= 0) return 0;
        _entry.Add(race, type, removed);
        _notices.Add(new SettlementWorldNotice(
            day,
            "REGIONAL_IMMIGRATION",
            $"{removed} {race} entered the settlement migration queue",
            Profile.CapitalRegionId,
            race,
            removed));
        RefreshProfile(true);
        return removed;
    }

    public int ReturnEmigrantsToWorld(string race, int amount, int day)
    {
        if (Profile is null || amount <= 0) return 0;
        var added = _regions.AddPopulation(
            Profile.CapitalRegionId,
            race,
            amount,
            "SETTLEMENT_EMIGRATION",
            day);
        if (added > 0)
        {
            _notices.Add(new SettlementWorldNotice(
                day,
                "SETTLEMENT_EMIGRATION",
                $"{added} {race} returned to the capital region",
                Profile.CapitalRegionId,
                race,
                added));
            RefreshProfile(true);
        }
        return added;
    }

    public int ExportPopulation(string race, int amount, int destinationRegionId, int day)
    {
        if (Profile is null || amount <= 0) return 0;
        var transfer = _regions.TransferPopulation(
            Profile.CapitalRegionId,
            destinationRegionId,
            race,
            amount,
            "PLAYER_ORDER",
            day);
        if (transfer.Accepted > 0)
        {
            _notices.Add(new SettlementWorldNotice(
                day,
                "POPULATION_TRANSFER",
                $"{transfer.Accepted} {race} transferred to region #{destinationRegionId}",
                destinationRegionId,
                race,
                transfer.Accepted));
            RefreshProfile(true);
        }
        return transfer.Accepted;
    }

    public int WithdrawRegionalResource(ResourceKind resource, int amount)
    {
        if (Profile is null) return 0;
        var withdrawn = _regions.WithdrawStock(Profile.CapitalRegionId, resource, amount);
        if (withdrawn > 0) RefreshProfile(true);
        return withdrawn;
    }

    public int DepositRegionalResource(ResourceKind resource, int amount)
    {
        if (Profile is null) return 0;
        var deposited = _regions.DepositStock(Profile.CapitalRegionId, resource, amount);
        if (deposited > 0) RefreshProfile(true);
        return deposited;
    }

    public bool CanReachWorld()
    {
        return Profile is not null && !_entry.IsClosed &&
               _world.FindRoute(Profile.CapitalRegionId, Profile.CapitalRegionId).Count > 0;
    }

    public double ClimateCompatibility(string race)
    {
        if (Profile is null) return 0;
        var rule = Data.OriginalGameData.Current.Races.GetValueOrDefault(race);
        if (rule is null) return 0.5;
        return rule.PopulationClimate.GetValueOrDefault(Profile.Climate, 0.5);
    }

    public IReadOnlyDictionary<string, double> RegionalRaceShares()
    {
        return Profile is null
            ? new Dictionary<string, double>()
            : _regions.RaceShares(Profile.CapitalRegionId);
    }

    public void Restore(
        SettlementWorldProfile? profile,
        IReadOnlyDictionary<string, int>? lastSettlementPopulation,
        IReadOnlyDictionary<string, double>? arrivalRemainders)
    {
        Profile = profile;
        _lastCapitalRevision = profile?.Revision ?? -1;
        _lastSettlementPopulation.Clear();
        if (lastSettlementPopulation is not null)
            foreach (var pair in lastSettlementPopulation)
                _lastSettlementPopulation[pair.Key] = Math.Max(0, pair.Value);
        _arrivalRemainders.Clear();
        if (arrivalRemainders is not null)
            foreach (var pair in arrivalRemainders)
                _arrivalRemainders[pair.Key] = Math.Clamp(pair.Value, 0, 1);
    }

    public IReadOnlyDictionary<string, int> CaptureSettlementPopulation() =>
        new Dictionary<string, int>(_lastSettlementPopulation, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, double> CaptureArrivalRemainders() =>
        new Dictionary<string, double>(_arrivalRemainders, StringComparer.OrdinalIgnoreCase);

    private void ApplyCapital(PlayerCapitalSnapshot capital)
    {
        Profile = new SettlementWorldProfile(
            capital.RegionId,
            capital.Climate,
            capital.Fertility,
            capital.Moisture,
            capital.Water,
            capital.Population,
            capital.Races,
            capital.Stock,
            capital.Revision);
        _lastCapitalRevision = capital.Revision;
    }

    private void RefreshProfile(bool force = false)
    {
        if (Profile is null) return;
        var capital = _regions.CaptureCapital(Profile.CapitalRegionId);
        if (!force && capital.Revision == _lastCapitalRevision) return;
        ApplyCapital(capital);
    }

    private void SynchronizePopulation(int day, IReadOnlyDictionary<string, int> current)
    {
        if (Profile is null) return;
        var races = current.Keys.Concat(_lastSettlementPopulation.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var race in races)
        {
            var previous = _lastSettlementPopulation.GetValueOrDefault(race);
            var now = Math.Max(0, current.GetValueOrDefault(race));
            var delta = now - previous;
            if (delta == 0) continue;
            var cause = delta > 0 ? "SETTLEMENT_GAIN" : "SETTLEMENT_LOSS";
            _populationDeltas.Add(new SettlementPopulationDelta(
                race,
                now,
                previous,
                Profile.RacePopulation.GetValueOrDefault(race),
                delta,
                cause));
            if (delta < 0)
                _notices.Add(new SettlementWorldNotice(
                    day,
                    cause,
                    $"Settlement population changed by {delta} for {race}",
                    Profile.CapitalRegionId,
                    race,
                    delta));
            _lastSettlementPopulation[race] = now;
        }
    }

    private void TrimHistory()
    {
        const int limit = 256;
        if (_populationDeltas.Count > limit)
            _populationDeltas.RemoveRange(0, _populationDeltas.Count - limit);
        if (_notices.Count > limit)
            _notices.RemoveRange(0, _notices.Count - limit);
    }
}
