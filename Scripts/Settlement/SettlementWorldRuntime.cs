using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Rooms;
using GodotSyxPort.Tourism;
using GodotSyxPort.World;
namespace GodotSyxPort.Settlement;
public sealed record SettlementWorldTickResult(
    int Day,
    int RegionalImmigrantsQueued,
    int HavenImmigrantsQueued,
    int TouristParties,
    int ActiveTourists,
    int DepartedTourists,
    int ActiveShipments,
    double TourismIncome);
public sealed record SettlementWorldDiagnostics(
    bool Connected,
    int CapitalRegionId,
    int RegionalPopulation,
    int RegionalRaces,
    int WorldEntities,
    int ActiveShipments,
    int Havens,
    int PlayerHavens,
    int Landmarks,
    int DiscoveredLandmarks,
    int TouristParties,
    int ActiveTourists,
    int Reviews,
    double Reputation,
    double TourismIncome,
    IReadOnlyList<string> Problems);
public sealed record SettlementWorldSnapshot(
    int LastDay,
    double WorldDayRemainder,
    SettlementWorldProfile? Profile,
    IReadOnlyDictionary<string, int> SettlementPopulation,
    IReadOnlyDictionary<string, double> ArrivalRemainders,
    IReadOnlyList<WorldEntitySnapshot> Entities,
    IReadOnlyList<HavenSnapshot> Havens,
    IReadOnlyList<LandmarkSnapshot> Landmarks,
    IReadOnlyList<TouristVisitSnapshot> Tourists,
    IReadOnlyList<TourismReview> Reviews,
    IReadOnlyList<ShipmentSnapshot> Shipments,
    IReadOnlyList<WorldArmySnapshot> Armies,
    IReadOnlyList<WorldBattleSnapshot> Battles,
    WorldFactionSnapshot Factions,
    WorldDiplomacySnapshot? Diplomacy);
/// <summary>
/// Simulation coordinator for the world-to-settlement boundary. This class does not
/// render or own simulation data; it orders deterministic updates and exposes state to UI.
/// </summary>
public sealed class SettlementWorldRuntime
{
    private readonly StrategicWorldRuntime _world;
    private readonly RegionalEconomyRuntime _economy;
    private readonly SettlementEntryRuntime _entry;
    private readonly HospitalityRuntime _hospitality;
    private readonly WorldRegionRuntime _regions;
    private readonly WorldEntityRuntime _entities;
    private readonly WorldFactionRuntime _factions;
    private readonly WorldDiplomacyRuntime _diplomacy;
    private readonly WorldHavenRuntime _havens;
    private readonly WorldLandmarkRuntime _landmarks;
    private readonly WorldSettlementBridge _bridge;
    private readonly TourismReviewRuntime _reviews;
    private readonly TourismRuntime _tourism;
    private readonly TouristPlannerRuntime _touristPlanner;
    private readonly WorldTradeShipmentRuntime _shipments;
    private readonly WorldArmyRuntime _armies;
    private readonly WorldBattleRuntime _battles;
    private double _worldDayRemainder;
    private int _lastDay;
    public SettlementWorldRuntime(
        StrategicWorldRuntime world,
        RegionalEconomyRuntime economy,
        SettlementEntryRuntime entry,
        HospitalityRuntime hospitality)
    {
        _world = world;
        _economy = economy;
        _entry = entry;
        _hospitality = hospitality;
        _regions = new WorldRegionRuntime(world, economy);
        _entities = new WorldEntityRuntime(world);
        _factions = new WorldFactionRuntime(world, _regions);
        _diplomacy = new WorldDiplomacyRuntime(world, economy, _factions);
        _havens = new WorldHavenRuntime(world, _regions);
        _landmarks = new WorldLandmarkRuntime(world, _regions);
        _bridge = new WorldSettlementBridge(world, _regions, _havens, entry);
        _reviews = new TourismReviewRuntime();
        _tourism = new TourismRuntime(_landmarks, _reviews);
        _touristPlanner = new TouristPlannerRuntime(
            world,
            _regions,
            _entities,
            _landmarks,
            _tourism,
            _reviews);
        _shipments = new WorldTradeShipmentRuntime(
            world,
            _entities,
            _regions,
            _factions);
        _armies = new WorldArmyRuntime(world, _entities);
        _battles = new WorldBattleRuntime(world, economy, _entities, _armies);
    }
    public WorldRegionRuntime Regions => _regions;
    public WorldEntityRuntime Entities => _entities;
    public WorldFactionRuntime Factions => _factions;
    public WorldDiplomacyRuntime Diplomacy => _diplomacy;
    public WorldHavenRuntime Havens => _havens;
    public WorldLandmarkRuntime Landmarks => _landmarks;
    public WorldSettlementBridge Bridge => _bridge;
    public TourismReviewRuntime Reviews => _reviews;
    public TourismRuntime Tourism => _tourism;
    public TouristPlannerRuntime TouristPlanner => _touristPlanner;
    public WorldTradeShipmentRuntime Shipments => _shipments;
    public WorldArmyRuntime Armies => _armies;
    public WorldBattleRuntime Battles => _battles;
    public SettlementWorldTickResult? LastTick { get; private set; }
    public bool Initialize(int seed = 0x535958)
    {
        if (!_bridge.ConnectToPlayerCapital()) return false;
        _landmarks.Generate(seed);
        SeedPlayerLandmark();
        _havens.Generate(seed);
        return true;
    }
    public SettlementWorldTickResult Tick(
        double deltaSeconds,
        int secondsPerDay,
        IReadOnlyDictionary<string, int> settlementPopulation,
        double settlementAttraction,
        double settlementSafety,
        double settlementServiceQuality)
    {
        if (deltaSeconds <= 0 || secondsPerDay <= 0)
            return LastTick ?? EmptyResult();
        var days = deltaSeconds / secondsPerDay;
        _worldDayRemainder += days;
        var day = _lastDay + (int)Math.Floor(_worldDayRemainder);
        if (day > _lastDay)
        {
            _worldDayRemainder -= day - _lastDay;
            _lastDay = day;
        }
        var incomingBefore = _entry.IncomingTotal;
        _regions.Tick(days);
        _economy.Tick(days);
        _factions.Tick(days, day);
        _diplomacy.Tick(days, day);
        _landmarks.Tick(days);
        _bridge.Tick(days, day, settlementPopulation, settlementAttraction);
        var capitalId = _bridge.Profile?.CapitalRegionId ?? -1;
        if (capitalId >= 0)
        {
            _touristPlanner.Tick(days, day, capitalId, !_entry.IsClosed);
            _shipments.Tick(days, day);
            _armies.Tick(days);
            _battles.Tick();
            _tourism.AdmitQueued(capitalId, day);
        }
        _tourism.Tick(days, day, _hospitality, settlementSafety, settlementServiceQuality);
        var incomingAfter = _entry.IncomingTotal;
        LastTick = new SettlementWorldTickResult(
            day,
            Math.Max(0, incomingAfter - incomingBefore),
            _bridge.Notices.Count(notice => notice.Kind == "HAVEN_IMMIGRATION" && notice.Day == day),
            _touristPlanner.Parties.Count,
            _tourism.ActiveVisitors,
            _tourism.Departures.Count,
            _shipments.ActiveCount,
            _tourism.TotalIncome);
        return LastTick;
    }
    public int PullRegionalMigrants(
        string race,
        double amount,
        HumanoidType type = HumanoidType.Subject)
    {
        return _bridge.QueueRegionalImmigration(race, amount, type, _lastDay);
    }
    public SettlementWorldSnapshot Capture()
    {
        return new SettlementWorldSnapshot(
            _lastDay,
            _worldDayRemainder,
            _bridge.Profile,
            _bridge.CaptureSettlementPopulation(),
            _bridge.CaptureArrivalRemainders(),
            _entities.Capture(),
            _havens.Capture(),
            _landmarks.Capture(),
            _tourism.Capture(),
            _reviews.CaptureReviews(),
            _shipments.Capture(),
            _armies.Capture(),
            _battles.Capture(),
            _factions.Capture(),
            _diplomacy.Capture());
    }
    public void Restore(SettlementWorldSnapshot snapshot)
    {
        _lastDay = Math.Max(0, snapshot.LastDay);
        _worldDayRemainder = Math.Clamp(snapshot.WorldDayRemainder, 0, 1);
        _bridge.Restore(snapshot.Profile, snapshot.SettlementPopulation, snapshot.ArrivalRemainders);
        _havens.Restore(snapshot.Havens);
        _landmarks.Restore(snapshot.Landmarks);
        _tourism.Restore(snapshot.Tourists);
        _reviews.Restore(snapshot.Reviews);
        _factions.Restore(snapshot.Factions);
        if (snapshot.Diplomacy is not null) _diplomacy.Restore(snapshot.Diplomacy);
        foreach (var entity in snapshot.Entities) _entities.Restore(entity);
        _shipments.Restore(snapshot.Shipments);
        _armies.Restore(snapshot.Armies ?? Array.Empty<WorldArmySnapshot>());
        _battles.Restore(snapshot.Battles ?? Array.Empty<WorldBattleSnapshot>());
    }
    public SettlementWorldDiagnostics Diagnose()
    {
        var problems = new List<string>();
        var profile = _bridge.Profile;
        if (profile is null)
        {
            problems.Add("Settlement is not connected to a player capital.");
        }
        else
        {
            var region = _world.Region(profile.CapitalRegionId);
            if (region is null) problems.Add("Capital region does not exist.");
            else
            {
                if (!region.Capital) problems.Add("Connected region is no longer a capital.");
                if (region.OwnerFactionId != _world.PlayerFactionId)
                    problems.Add("Connected capital is not owned by the player.");
                if (!region.Habitable) problems.Add("Connected capital is not habitable.");
            }
            if (profile.WorldPopulation != profile.RacePopulation.Values.Sum())
                problems.Add("Capital population differs from its race population sum.");
        }
        foreach (var entity in _entities.Entities)
        {
            if (entity.IsTerminal) continue;
            if (_world.Region(entity.CurrentRegionId) is null)
                problems.Add($"Entity {entity.Id} is in an unknown region.");
            if (entity.Route.Count == 0)
                problems.Add($"Entity {entity.Id} has no route.");
            if (entity.RouteIndex < 0 || entity.RouteIndex >= entity.Route.Count)
                problems.Add($"Entity {entity.Id} has an invalid route index.");
        }
        foreach (var shipment in _shipments.Shipments.Where(value => !value.IsTerminal))
        {
            if (shipment.Amount <= 0) problems.Add($"Shipment {shipment.Id} has no cargo.");
            if (shipment.Route.Count - 1 > WorldTradeShipmentRuntime.MaximumRouteDistance)
                problems.Add($"Shipment {shipment.Id} exceeds the route limit.");
            if (shipment.State is ShipmentState.Travelling or ShipmentState.Returning &&
                _entities.Get(shipment.EntityId) is null)
                problems.Add($"Shipment {shipment.Id} lost its world entity.");
        }
        foreach (var visit in _tourism.Visits.Where(value => value.Active))
        {
            if (visit.Budget < visit.Spent)
                problems.Add($"Tourist {visit.Id} spent more than the visit budget.");
            if (visit.State is TouristState.Resting or TouristState.Sightseeing or TouristState.Shopping &&
                visit.InnRoomId == 0)
                problems.Add($"Tourist {visit.Id} has no reserved inn.");
        }
        var regionalPopulation = profile?.WorldPopulation ?? 0;
        return new SettlementWorldDiagnostics(
            profile is not null,
            profile?.CapitalRegionId ?? -1,
            regionalPopulation,
            profile?.RacePopulation.Count ?? 0,
            _entities.Entities.Count,
            _shipments.ActiveCount,
            _havens.Havens.Count,
            _havens.Havens.Count(value => value.PlayerControlled),
            _landmarks.Landmarks.Count,
            _landmarks.Landmarks.Count(value => value.Discovered),
            _touristPlanner.Parties.Count,
            _tourism.ActiveVisitors,
            _reviews.Reviews.Count,
            _reviews.Reputation,
            _tourism.TotalIncome,
            problems);
    }
    public IReadOnlyList<string> DescribeState()
    {
        var diagnostics = Diagnose();
        var lines = new List<string>
        {
            diagnostics.Connected
                ? $"Capital region #{diagnostics.CapitalRegionId}: {diagnostics.RegionalPopulation} population across {diagnostics.RegionalRaces} races"
                : "No world capital connection",
            $"World entities {diagnostics.WorldEntities}, active shipments {diagnostics.ActiveShipments}",
            $"Havens {diagnostics.Havens} ({diagnostics.PlayerHavens} player), landmarks {diagnostics.DiscoveredLandmarks}/{diagnostics.Landmarks} discovered",
            $"Tourist parties {diagnostics.TouristParties}, visitors {diagnostics.ActiveTourists}, reviews {diagnostics.Reviews}",
            $"Tourism reputation {diagnostics.Reputation:P0}, income {diagnostics.TourismIncome:0.##}"
        };
        lines.AddRange(diagnostics.Problems.Select(problem => "Problem: " + problem));
        return lines;
    }
    public void RegisterTouristCitizen(int visitId, int citizenId)
    {
        _tourism.BindCitizen(visitId, citizenId);
    }
    public bool RemoveTouristCitizen(int citizenId)
    {
        var visit = _tourism.ByCitizen(citizenId);
        return visit is not null && _tourism.ForceDeparture(visit.Id, _lastDay, _hospitality);
    }
    public IEnumerable<WorldEntityRecord> VisibleWorldEntities(Func<int, bool> regionVisible)
    {
        return _entities.Entities.Where(entity => !entity.IsTerminal && regionVisible(entity.CurrentRegionId));
    }
    private void SeedPlayerLandmark()
    {
        var capital = _bridge.Profile;
        if (capital is null || _landmarks.AtRegion(capital.CapitalRegionId).Any()) return;
        var landmark = _landmarks.Add(
            capital.CapitalRegionId,
            LandmarkKind.Civic,
            "PLAYER_CAPITAL",
            "Player capital",
            1,
            0.75);
        _landmarks.Discover(landmark.Id);
    }
    private SettlementWorldTickResult EmptyResult() => new(
        _lastDay,
        0,
        0,
        _touristPlanner.Parties.Count,
        _tourism.ActiveVisitors,
        0,
        _shipments.ActiveCount,
        _tourism.TotalIncome);
}
