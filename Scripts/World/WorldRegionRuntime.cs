using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;

namespace GodotSyxPort.World;

public sealed record RegionPopulationSnapshot(
    int RegionId,
    int Population,
    IReadOnlyDictionary<string, int> Races,
    IReadOnlyDictionary<string, double> Religions,
    double Loyalty,
    double Health,
    double Devastation);

public sealed record PlayerCapitalSnapshot(
    int RegionId,
    string Climate,
    double Fertility,
    double Moisture,
    double Water,
    int Population,
    IReadOnlyDictionary<string, int> Races,
    IReadOnlyDictionary<ResourceKind, int> Stock,
    int Revision);

public sealed record PopulationTransfer(
    long Id,
    int OriginRegionId,
    int DestinationRegionId,
    string Race,
    int Requested,
    int Accepted,
    string Cause,
    int Day);

/// <summary>
/// Live regional data facade used by the settlement bridge. It consolidates Region,
/// RData, owner, population and update semantics without reproducing world UI classes.
/// </summary>
public sealed class WorldRegionRuntime
{
    private readonly StrategicWorldRuntime _world;
    private readonly RegionalEconomyRuntime _economy;
    private readonly Dictionary<int, double> _health = new();
    private readonly Dictionary<int, double> _devastation = new();
    private readonly Dictionary<int, int> _regionRevisions = new();
    private readonly List<PopulationTransfer> _transfers = new();
    private long _nextTransferId = 1;

    public WorldRegionRuntime(StrategicWorldRuntime world, RegionalEconomyRuntime economy)
    {
        _world = world;
        _economy = economy;
        foreach (var region in world.Regions)
        {
            _health[region.Id] = 1;
            _devastation[region.Id] = 0;
            _regionRevisions[region.Id] = 0;
        }
    }

    public int? PlayerCapitalRegionId
    {
        get
        {
            var id = _world.Faction(_world.PlayerFactionId)?.CapitalRegionId ?? -1;
            return id >= 0 ? id : null;
        }
    }

    public IReadOnlyList<PopulationTransfer> Transfers => _transfers;

    public PlayerCapitalSnapshot? PlayerCapital()
    {
        var id = PlayerCapitalRegionId;
        return id is null ? null : CaptureCapital(id.Value);
    }

    public PlayerCapitalSnapshot CaptureCapital(int regionId)
    {
        var region = RequireRegion(regionId);
        var state = RequireState(regionId);
        return new PlayerCapitalSnapshot(
            regionId,
            region.Climate,
            region.Fertility,
            region.Moisture,
            region.Water,
            state.Population,
            new Dictionary<string, int>(state.RacePopulation, StringComparer.OrdinalIgnoreCase),
            new Dictionary<ResourceKind, int>(state.Stock),
            _regionRevisions.GetValueOrDefault(regionId));
    }

    public RegionPopulationSnapshot CapturePopulation(int regionId)
    {
        var state = RequireState(regionId);
        return new RegionPopulationSnapshot(
            regionId,
            state.Population,
            new Dictionary<string, int>(state.RacePopulation, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, double>(state.Religions, StringComparer.OrdinalIgnoreCase),
            state.Loyalty,
            _health.GetValueOrDefault(regionId, 1),
            _devastation.GetValueOrDefault(regionId));
    }

    public bool SetPlayerCapital(int regionId)
    {
        var region = RequireRegion(regionId);
        if (region.OwnerFactionId != _world.PlayerFactionId || !region.Habitable) return false;
        var faction = _world.Faction(_world.PlayerFactionId);
        if (faction is null) return false;
        foreach (var candidate in _world.Regions) candidate.Capital = false;
        region.Capital = true;
        faction.CapitalRegionId = regionId;
        Touch(regionId);
        return true;
    }

    public PopulationTransfer TransferPopulation(
        int originRegionId,
        int destinationRegionId,
        string race,
        int requested,
        string cause,
        int day)
    {
        RequireRegion(originRegionId);
        RequireRegion(destinationRegionId);
        if (requested < 0) throw new ArgumentOutOfRangeException(nameof(requested));
        var origin = RequireState(originRegionId);
        var destination = RequireState(destinationRegionId);
        var available = origin.RacePopulation.GetValueOrDefault(race);
        var accepted = Math.Min(requested, available);
        if (accepted > 0)
        {
            SetRacePopulation(origin, race, available - accepted);
            SetRacePopulation(destination, race, destination.RacePopulation.GetValueOrDefault(race) + accepted);
            RecalculatePopulation(origin);
            RecalculatePopulation(destination);
            Touch(originRegionId);
            Touch(destinationRegionId);
        }
        var transfer = new PopulationTransfer(
            _nextTransferId++,
            originRegionId,
            destinationRegionId,
            race,
            requested,
            accepted,
            cause,
            day);
        _transfers.Add(transfer);
        TrimHistory();
        return transfer;
    }

    public int AddPopulation(int regionId, string race, int amount, string cause, int day)
    {
        if (amount <= 0) return 0;
        var state = RequireState(regionId);
        SetRacePopulation(state, race, state.RacePopulation.GetValueOrDefault(race) + amount);
        RecalculatePopulation(state);
        _transfers.Add(new PopulationTransfer(
            _nextTransferId++,
            -1,
            regionId,
            race,
            amount,
            amount,
            cause,
            day));
        Touch(regionId);
        TrimHistory();
        return amount;
    }

    public int RemovePopulation(int regionId, string race, int amount, string cause, int day)
    {
        if (amount <= 0) return 0;
        var state = RequireState(regionId);
        var removed = Math.Min(amount, state.RacePopulation.GetValueOrDefault(race));
        SetRacePopulation(state, race, state.RacePopulation.GetValueOrDefault(race) - removed);
        RecalculatePopulation(state);
        _transfers.Add(new PopulationTransfer(
            _nextTransferId++,
            regionId,
            -1,
            race,
            amount,
            removed,
            cause,
            day));
        Touch(regionId);
        TrimHistory();
        return removed;
    }

    public int WithdrawStock(int regionId, ResourceKind resource, int amount)
    {
        if (amount <= 0) return 0;
        var state = RequireState(regionId);
        var available = state.Stock.GetValueOrDefault(resource);
        var withdrawn = Math.Min(amount, available);
        SetStock(state, resource, available - withdrawn);
        if (withdrawn > 0) Touch(regionId);
        return withdrawn;
    }

    public int DepositStock(int regionId, ResourceKind resource, int amount)
    {
        if (amount <= 0) return 0;
        var state = RequireState(regionId);
        var current = state.Stock.GetValueOrDefault(resource);
        var accepted = Math.Min(amount, int.MaxValue - current);
        SetStock(state, resource, current + accepted);
        if (accepted > 0) Touch(regionId);
        return accepted;
    }

    public bool ReserveStock(int regionId, ResourceKind resource, int amount)
    {
        return amount >= 0 && RequireState(regionId).Stock.GetValueOrDefault(resource) >= amount;
    }

    public void SetHealth(int regionId, double value)
    {
        RequireRegion(regionId);
        _health[regionId] = Math.Clamp(value, 0, 1);
        Touch(regionId);
    }

    public void SetDevastation(int regionId, double value)
    {
        RequireRegion(regionId);
        _devastation[regionId] = Math.Clamp(value, 0, 1);
        var state = RequireState(regionId);
        state.Devastation = _devastation[regionId];
        Touch(regionId);
    }

    public void Tick(double days)
    {
        if (days <= 0) return;
        foreach (var region in _world.Regions)
        {
            var state = _economy.State(region.Id);
            if (state is null) continue;
            var targetHealth = Math.Clamp(state.Health * (1 - state.Devastation * 0.75), 0, 1);
            _health[region.Id] = MoveTowards(_health.GetValueOrDefault(region.Id, 1), targetHealth, days / 16.0);
            _devastation[region.Id] = Math.Clamp(state.Devastation, 0, 1);
            region.Population = state.Population;
        }
    }

    public IReadOnlyDictionary<string, double> RaceShares(int regionId)
    {
        var state = RequireState(regionId);
        var total = Math.Max(1, state.RacePopulation.Values.Sum());
        return state.RacePopulation.ToDictionary(
            pair => pair.Key,
            pair => pair.Value / (double)total,
            StringComparer.OrdinalIgnoreCase);
    }

    public string MajorityRace(int regionId)
    {
        var state = RequireState(regionId);
        return state.RacePopulation.Count == 0
            ? state.MajorityRace
            : state.RacePopulation.MaxBy(pair => pair.Value).Key;
    }

    private StrategicRegion RequireRegion(int regionId) =>
        _world.Region(regionId) ?? throw new ArgumentOutOfRangeException(nameof(regionId));

    private RegionalEconomyState RequireState(int regionId) =>
        _economy.State(regionId) ?? throw new InvalidOperationException($"No economy state for region {regionId}");

    private void Touch(int regionId)
    {
        _regionRevisions[regionId] = _regionRevisions.GetValueOrDefault(regionId) + 1;
    }

    private static void SetRacePopulation(RegionalEconomyState state, string race, int amount)
    {
        if (amount <= 0) state.RacePopulation.Remove(race);
        else state.RacePopulation[race] = amount;
    }

    private static void RecalculatePopulation(RegionalEconomyState state)
    {
        state.Population = state.RacePopulation.Values.Sum();
        state.MajorityRace = state.RacePopulation.Count == 0
            ? ""
            : state.RacePopulation.MaxBy(pair => pair.Value).Key;
    }

    private static void SetStock(RegionalEconomyState state, ResourceKind resource, int amount)
    {
        if (amount <= 0) state.Stock.Remove(resource);
        else state.Stock[resource] = amount;
    }

    private void TrimHistory()
    {
        const int historyLimit = 512;
        if (_transfers.Count > historyLimit)
            _transfers.RemoveRange(0, _transfers.Count - historyLimit);
    }

    private static double MoveTowards(double current, double target, double maximumDelta)
    {
        if (current < target) return Math.Min(target, current + maximumDelta);
        return Math.Max(target, current - maximumDelta);
    }
}
