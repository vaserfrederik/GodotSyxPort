using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;
using GodotSyxPort.Trade;

namespace GodotSyxPort.World;

public sealed class WorldFactionState
{
    public int FactionId { get; init; }
    public string Key { get; init; } = "";
    public string Race { get; init; } = "";
    public int CapitalRegionId { get; set; }
    public double Credits { get; set; }
    public double Trust { get; set; }
    public double Opinion { get; set; }
    public double Stability { get; set; } = 1;
    public Dictionary<ResourceKind, int> Stock { get; } = new();
    public Dictionary<ResourceKind, int> Production { get; } = new();
    public Dictionary<ResourceKind, int> Consumption { get; } = new();
    public Dictionary<int, double> Relations { get; } = new();

    public int Available(ResourceKind resource) => Stock.GetValueOrDefault(resource);
    public double Relation(int factionId) => Relations.GetValueOrDefault(factionId);
}

public sealed record FactionTradeReservation(
    long Id,
    int FactionId,
    ResourceKind Resource,
    int Amount,
    bool Selling,
    int CreatedDay,
    string Purpose);

public sealed record FactionAgreement(
    int FirstFactionId,
    int SecondFactionId,
    DiplomacyStance Stance,
    int SinceDay,
    double Trust,
    bool AllowsTrade,
    bool AllowsMigration);
public sealed class RoyalCourtMemberState
{
    public long Id { get; init; }
    public int FactionId { get; init; }
    public int Succession { get; set; }
    public double DaysRemaining { get; set; }
    public double Competence { get; init; }
    public double Pride { get; init; }
    public double Honour { get; init; }
}
public sealed record RoyalCourtMemberSnapshot(
    long Id, int FactionId, int Succession, double DaysRemaining,
    double Competence, double Pride, double Honour);
public sealed record WorldFactionStateSnapshot(
    int FactionId, string Key, string Race, int CapitalRegionId,
    double Credits, double Trust, double Opinion, double Stability,
    IReadOnlyDictionary<ResourceKind, int> Stock,
    IReadOnlyDictionary<ResourceKind, int> Production,
    IReadOnlyDictionary<ResourceKind, int> Consumption,
    IReadOnlyDictionary<int, double> Relations);
public sealed record WorldFactionSnapshot(
    long NextReservationId,
    IReadOnlyList<WorldFactionStateSnapshot> States,
    IReadOnlyList<FactionTradeReservation> Reservations,
    IReadOnlyList<FactionAgreement> Agreements,
    long NextRoyalId = 1,
    IReadOnlyList<RoyalCourtMemberSnapshot>? Court = null);

/// <summary>
/// Peaceful NPC faction stock, trust and quote provider. It replaces the relevant
/// FactionNPC, NPCResource, diplomacy and royalty opinion update paths while battle
/// state remains deferred.
/// </summary>
public sealed class WorldFactionRuntime
{
    public const int MaximumCourtMembers = 4;
    public const int SourceDaysPerYear = 16;
    private readonly StrategicWorldRuntime _world;
    private readonly WorldRegionRuntime _regions;
    private readonly Dictionary<int, WorldFactionState> _states = new();
    private readonly Dictionary<long, FactionTradeReservation> _reservations = new();
    private readonly Dictionary<(int A, int B), FactionAgreement> _agreements = new();
    private readonly Dictionary<int, List<RoyalCourtMemberState>> _courts = new();
    private readonly List<string> _events = new();
    private long _nextReservationId = 1;
    private long _nextRoyalId = 1;

    public WorldFactionRuntime(StrategicWorldRuntime world, WorldRegionRuntime regions)
    {
        _world = world;
        _regions = regions;
        foreach (var faction in world.Factions)
        {
            var state = new WorldFactionState
            {
                FactionId = faction.Id,
                Key = faction.Key,
                Race = faction.Race,
                CapitalRegionId = faction.CapitalRegionId,
                Credits = faction.Player ? 0 : SettlementTradeRuntime.AveragePrice * 100.0,
                Trust = faction.Player ? 1 : 0.25,
                Opinion = faction.Player ? 1 : 0,
                Stability = 1
            };
            SeedStock(state);
            _states.Add(faction.Id, state);
            if (!faction.Player) SeedCourt(faction.Id);
        }
        RebuildAgreements(0);
    }

    public IReadOnlyDictionary<int, WorldFactionState> States => _states;
    public IReadOnlyCollection<FactionTradeReservation> Reservations => _reservations.Values;
    public IReadOnlyCollection<FactionAgreement> Agreements => _agreements.Values;
    public IReadOnlyList<string> Events => _events;
    public IReadOnlyList<RoyalCourtMemberState> Court(int factionId) =>
        _courts.TryGetValue(factionId, out var court) ? court : Array.Empty<RoyalCourtMemberState>();
    public RoyalCourtMemberState? King(int factionId) => Court(factionId)
        .OrderBy(value => value.Succession).FirstOrDefault();

    public WorldFactionState? State(int factionId) => _states.GetValueOrDefault(factionId);

    public WorldFactionSnapshot Capture() => new(
        _nextReservationId,
        _states.Values.Select(value => new WorldFactionStateSnapshot(
            value.FactionId, value.Key, value.Race, value.CapitalRegionId,
            value.Credits, value.Trust, value.Opinion, value.Stability,
            new Dictionary<ResourceKind, int>(value.Stock),
            new Dictionary<ResourceKind, int>(value.Production),
            new Dictionary<ResourceKind, int>(value.Consumption),
            new Dictionary<int, double>(value.Relations))).ToArray(),
        _reservations.Values.ToArray(),
        _agreements.Values.ToArray(),
        _nextRoyalId,
        _courts.Values.SelectMany(value => value).Select(value => new RoyalCourtMemberSnapshot(
            value.Id, value.FactionId, value.Succession, value.DaysRemaining,
            value.Competence, value.Pride, value.Honour)).ToArray());

    public void Restore(WorldFactionSnapshot snapshot)
    {
        foreach (var value in snapshot.States)
        {
            if (!_states.TryGetValue(value.FactionId, out var state)) continue;
            state.CapitalRegionId = value.CapitalRegionId;
            state.Credits = value.Credits;
            state.Trust = Math.Clamp(value.Trust, -1, 1);
            state.Opinion = Math.Clamp(value.Opinion, -1, 1);
            state.Stability = Math.Clamp(value.Stability, 0, 1);
            Replace(state.Stock, value.Stock);
            Replace(state.Production, value.Production);
            Replace(state.Consumption, value.Consumption);
            Replace(state.Relations, value.Relations);
        }
        _reservations.Clear();
        foreach (var value in snapshot.Reservations) _reservations[value.Id] = value;
        _agreements.Clear();
        foreach (var value in snapshot.Agreements)
            _agreements[Pair(value.FirstFactionId, value.SecondFactionId)] = value;
        _nextReservationId = Math.Max(
            Math.Max(1, snapshot.NextReservationId),
            _reservations.Count == 0 ? 1 : _reservations.Keys.Max() + 1);
        if (snapshot.Court is not null)
        {
            _courts.Clear();
            foreach (var value in snapshot.Court.Where(value => _states.ContainsKey(value.FactionId)))
            {
                if (!_courts.TryGetValue(value.FactionId, out var court))
                    _courts[value.FactionId] = court = new List<RoyalCourtMemberState>();
                if (court.Count >= MaximumCourtMembers) continue;
                court.Add(new RoyalCourtMemberState
                {
                    Id = value.Id, FactionId = value.FactionId, Succession = value.Succession,
                    DaysRemaining = Math.Max(1, value.DaysRemaining),
                    Competence = Math.Clamp(value.Competence, 0, 1),
                    Pride = Math.Clamp(value.Pride, 0, 1), Honour = Math.Clamp(value.Honour, 0, 1)
                });
            }
            foreach (var factionId in _states.Keys.Where(value => value != _world.PlayerFactionId))
                EnsureCourt(factionId);
            _nextRoyalId = Math.Max(Math.Max(1, snapshot.NextRoyalId),
                _courts.Values.SelectMany(value => value).Select(value => value.Id).DefaultIfEmpty(0).Max() + 1);
        }
    }

    private static void Replace<TKey, TValue>(
        IDictionary<TKey, TValue> target, IReadOnlyDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        target.Clear();
        foreach (var pair in source) target[pair.Key] = pair.Value;
    }

    public FactionAgreement Agreement(int firstFactionId, int secondFactionId)
    {
        var key = Pair(firstFactionId, secondFactionId);
        return _agreements.GetValueOrDefault(key) ?? new FactionAgreement(
            key.A,
            key.B,
            _world.Stance(key.A, key.B),
            0,
            0,
            false,
            false);
    }

    public void RebuildAgreements(int day)
    {
        _agreements.Clear();
        foreach (var first in _world.Factions)
        foreach (var second in _world.Factions.Where(value => value.Id > first.Id))
        {
            var stance = _world.Stance(first.Id, second.Id);
            var trust = (_states.GetValueOrDefault(first.Id)?.Relation(second.Id) ?? 0) * 0.5 +
                        (_states.GetValueOrDefault(second.Id)?.Relation(first.Id) ?? 0) * 0.5;
            _agreements[(first.Id, second.Id)] = new FactionAgreement(
                first.Id,
                second.Id,
                stance,
                day,
                trust,
                stance is DiplomacyStance.Trade or DiplomacyStance.Pact or
                    DiplomacyStance.Allied or DiplomacyStance.Vassal or DiplomacyStance.Overlord,
                stance is not DiplomacyStance.War);
        }
    }

    public bool SetStance(int firstFactionId, int secondFactionId, DiplomacyStance stance, int day)
    {
        if (!_world.SetStance(firstFactionId, secondFactionId, stance)) return false;
        var key = Pair(firstFactionId, secondFactionId);
        var previous = _agreements.GetValueOrDefault(key);
        _agreements[key] = new FactionAgreement(
            key.A,
            key.B,
            stance,
            day,
            previous?.Trust ?? 0,
            stance is DiplomacyStance.Trade or DiplomacyStance.Pact or
                DiplomacyStance.Allied or DiplomacyStance.Vassal or DiplomacyStance.Overlord,
            stance is not DiplomacyStance.War);
        Record($"stance:{key.A}:{key.B}:{stance}");
        return true;
    }

    public long ReserveExport(
        int factionId,
        ResourceKind resource,
        int requested,
        int day,
        string purpose)
    {
        if (requested <= 0 || !_states.TryGetValue(factionId, out var state)) return 0;
        var alreadyReserved = _reservations.Values.Where(value =>
            value.FactionId == factionId && value.Resource == resource && value.Selling).Sum(value => value.Amount);
        var available = Math.Max(0, state.Stock.GetValueOrDefault(resource) - alreadyReserved);
        var amount = Math.Min(requested, available);
        if (amount <= 0) return 0;
        var reservation = new FactionTradeReservation(
            _nextReservationId++,
            factionId,
            resource,
            amount,
            true,
            day,
            purpose);
        _reservations.Add(reservation.Id, reservation);
        return reservation.Id;
    }

    public long ReserveImport(
        int factionId,
        ResourceKind resource,
        int amount,
        int day,
        string purpose)
    {
        if (amount <= 0 || !_states.ContainsKey(factionId)) return 0;
        var reservation = new FactionTradeReservation(
            _nextReservationId++,
            factionId,
            resource,
            amount,
            false,
            day,
            purpose);
        _reservations.Add(reservation.Id, reservation);
        return reservation.Id;
    }

    public FactionTradeReservation? Reservation(long id) => _reservations.GetValueOrDefault(id);

    public bool CommitReservation(long id, double creditDelta)
    {
        if (!_reservations.Remove(id, out var reservation) ||
            !_states.TryGetValue(reservation.FactionId, out var state)) return false;
        if (reservation.Selling)
        {
            var current = state.Stock.GetValueOrDefault(reservation.Resource);
            if (current < reservation.Amount) return false;
            SetStock(state, reservation.Resource, current - reservation.Amount);
        }
        else
        {
            SetStock(state, reservation.Resource,
                state.Stock.GetValueOrDefault(reservation.Resource) + reservation.Amount);
        }
        state.Credits += creditDelta;
        Record($"trade:{reservation.FactionId}:{reservation.Resource}:{reservation.Amount}:{creditDelta:0.##}");
        return true;
    }

    public bool CancelReservation(long id) => _reservations.Remove(id);

    public IReadOnlyList<TradePartnerQuote> QuotesForPlayer(int playerFactionId)
    {
        var player = _states.GetValueOrDefault(playerFactionId);
        if (player is null) return Array.Empty<TradePartnerQuote>();
        var result = new List<TradePartnerQuote>();
        foreach (var faction in _states.Values.Where(value => value.FactionId != playerFactionId))
        {
            var agreement = Agreement(playerFactionId, faction.FactionId);
            if (!agreement.AllowsTrade) continue;
            var distance = _world.DistanceBetweenCapitals(playerFactionId, faction.FactionId);
            if (distance < 0) continue;
            var buyerPrices = new Dictionary<ResourceKind, int>();
            var buyerAmounts = new Dictionary<ResourceKind, int>();
            var sellerPrices = new Dictionary<ResourceKind, int>();
            var sellerAmounts = new Dictionary<ResourceKind, int>();
            foreach (var resource in Enum.GetValues<ResourceKind>())
            {
                var stock = faction.Stock.GetValueOrDefault(resource);
                var production = faction.Production.GetValueOrDefault(resource);
                var consumption = faction.Consumption.GetValueOrDefault(resource);
                var pressure = consumption - production;
                var reference = SettlementTradeRuntime.AveragePrice;
                if (stock > Math.Max(16, consumption * 4))
                {
                    sellerAmounts[resource] = stock;
                    sellerPrices[resource] = Math.Max(1, reference - Math.Min(reference / 2, stock));
                }
                if (pressure > 0 || stock < consumption)
                {
                    buyerAmounts[resource] = Math.Max(1, pressure * 8);
                    buyerPrices[resource] = reference + Math.Min(reference, Math.Max(1, pressure) * 10);
                }
            }
            result.Add(new TradePartnerQuote(
                faction.Key,
                distance,
                1,
                1,
                1,
                sellerPrices,
                sellerAmounts,
                buyerPrices,
                buyerAmounts));
        }
        return result;
    }

    public void Tick(double days, int day)
    {
        if (days <= 0) return;
        foreach (var state in _states.Values)
        {
            foreach (var resource in Enum.GetValues<ResourceKind>())
            {
                var delta = (state.Production.GetValueOrDefault(resource) -
                             state.Consumption.GetValueOrDefault(resource)) * days;
                if (Math.Abs(delta) < 0.0001) continue;
                var current = state.Stock.GetValueOrDefault(resource);
                SetStock(state, resource, Math.Max(0, current + (int)Math.Truncate(delta)));
            }
            var capital = _world.Region(state.CapitalRegionId);
            var regional = capital is null ? null : _regions.CapturePopulation(capital.Id);
            var targetStability = regional is null
                ? 0.5
                : Math.Clamp(regional.Loyalty * regional.Health * (1 - regional.Devastation), 0, 1);
            state.Stability = MoveTowards(state.Stability, targetStability, days / 16.0);
            var stance = _world.Stance(_world.PlayerFactionId, state.FactionId);
            var stanceTrust = stance switch
            {
                DiplomacyStance.War => 0.0,
                DiplomacyStance.Neutral => 0.75,
                DiplomacyStance.Trade => 1.0,
                DiplomacyStance.Pact => 1.1,
                DiplomacyStance.Allied => 1.2,
                DiplomacyStance.Vassal or DiplomacyStance.Overlord => 1.0,
                _ => 1.0
            };
            var honour = King(state.FactionId)?.Honour ?? 0.5;
            var targetTrust = Math.Clamp((0.5 + state.Opinion * 0.5) * stanceTrust *
                                         (0.5 + honour * 1.5), 0, 1);
            state.Trust = MoveTowards(state.Trust, targetTrust, days / 100.0);
        }
        TickCourts(days, day);
        ExpireReservations(day);
        if (day % 4 == 0) RebuildAgreements(day);
    }

    public void ConfigureFlow(int factionId, ResourceKind resource, int production, int consumption)
    {
        var state = _states.GetValueOrDefault(factionId) ??
            throw new ArgumentOutOfRangeException(nameof(factionId));
        SetStock(state.Production, resource, Math.Max(0, production));
        SetStock(state.Consumption, resource, Math.Max(0, consumption));
    }

    public void AdjustOpinion(int factionId, int otherFactionId, double delta)
    {
        if (!_states.TryGetValue(factionId, out var state)) return;
        state.Relations[otherFactionId] = Math.Clamp(state.Relations.GetValueOrDefault(otherFactionId) + delta, -1, 1);
        if (otherFactionId == _world.PlayerFactionId)
            state.Opinion = state.Relations[otherFactionId];
    }

    public void ReceiveGift(int factionId, double credits, int day)
    {
        if (credits <= 0 || !_states.TryGetValue(factionId, out var state)) return;
        state.Credits += credits;
        Record($"gift:{day}:{factionId}:{credits:0.##}");
    }

    public double GiftOpinionMultiplier(int factionId)
    {
        var pride = King(factionId)?.Pride ?? 0.5;
        return 1.0 / (0.25 + 0.75 * pride);
    }

    private void SeedCourt(int factionId)
    {
        _courts[factionId] = new List<RoyalCourtMemberState>();
        EnsureCourt(factionId);
    }

    private void EnsureCourt(int factionId)
    {
        if (!_courts.TryGetValue(factionId, out var court))
            _courts[factionId] = court = new List<RoyalCourtMemberState>();
        while (court.Count < MaximumCourtMembers)
        {
            var id = _nextRoyalId++;
            var seed = StableHash(factionId, unchecked((int)id));
            court.Add(new RoyalCourtMemberState
            {
                Id = id, FactionId = factionId, Succession = court.Count,
                DaysRemaining = SourceDaysPerYear * (20 + seed % 40),
                Competence = (seed % 101) / 100.0,
                Pride = ((seed / 101) % 101) / 100.0,
                Honour = ((seed / 10201) % 101) / 100.0
            });
        }
    }

    private void TickCourts(double days, int day)
    {
        foreach (var pair in _courts)
        {
            if (pair.Value.Count == 0) { EnsureCourt(pair.Key); continue; }
            var king = pair.Value.MinBy(value => value.Succession)!;
            king.DaysRemaining -= days;
            if (king.DaysRemaining > 0) continue;
            var oldId = king.Id;
            pair.Value.Remove(king);
            foreach (var member in pair.Value) member.Succession--;
            EnsureCourt(pair.Key);
            Record($"succession:{day}:{pair.Key}:{oldId}:{King(pair.Key)?.Id ?? 0}");
        }
    }

    private void SeedStock(WorldFactionState state)
    {
        var capital = _regions.PlayerCapitalRegionId == state.CapitalRegionId
            ? _regions.PlayerCapital()
            : state.CapitalRegionId >= 0 ? _regions.CaptureCapital(state.CapitalRegionId) : null;
        if (capital is not null)
            foreach (var pair in capital.Stock) state.Stock[pair.Key] = pair.Value;
        foreach (var resource in Enum.GetValues<ResourceKind>())
        {
            var seed = StableHash(state.FactionId, (int)resource);
            state.Stock[resource] = Math.Max(state.Stock.GetValueOrDefault(resource), 20 + seed % 180);
            state.Production[resource] = seed % 7;
            state.Consumption[resource] = (seed / 7) % 6;
        }
    }

    private void ExpireReservations(int day)
    {
        foreach (var reservation in _reservations.Values.Where(value => day - value.CreatedDay > 8).ToArray())
            _reservations.Remove(reservation.Id);
    }

    private void Record(string value)
    {
        _events.Add(value);
        if (_events.Count > 256) _events.RemoveRange(0, _events.Count - 256);
    }

    private static void SetStock(WorldFactionState state, ResourceKind resource, int amount) =>
        SetStock(state.Stock, resource, amount);

    private static void SetStock(IDictionary<ResourceKind, int> values, ResourceKind resource, int amount)
    {
        if (amount <= 0) values.Remove(resource);
        else values[resource] = amount;
    }

    private static (int A, int B) Pair(int first, int second) =>
        first < second ? (first, second) : (second, first);

    private static int StableHash(int first, int second)
    {
        unchecked
        {
            var value = first * 73856093 ^ second * 19349663;
            return value == int.MinValue ? int.MaxValue : Math.Abs(value);
        }
    }

    private static double MoveTowards(double current, double target, double delta)
    {
        if (current < target) return Math.Min(target, current + delta);
        return Math.Max(target, current - delta);
    }
}
