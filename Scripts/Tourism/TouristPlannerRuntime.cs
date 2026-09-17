using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.World;

namespace GodotSyxPort.Tourism;

public sealed record TouristParty(
    long EntityId,
    int OriginRegionId,
    int DestinationRegionId,
    string Race,
    int Amount,
    double BudgetPerVisitor,
    int LandmarkId,
    int CreatedDay);

public sealed record TourismDemand(
    int OriginRegionId,
    string Race,
    double Attraction,
    double Reputation,
    double DistancePenalty,
    double ExpectedVisitors,
    int LandmarkId);

public sealed record TourismForecast(
    int PlayerCapitalRegionId,
    int ReachableOrigins,
    int EligibleRaces,
    int ActiveParties,
    int TravellingVisitors,
    double ExpectedVisitorsPerDay,
    double AverageDistancePenalty,
    double AverageReputation,
    IReadOnlyDictionary<string, double> ExpectedByRace,
    IReadOnlyList<string> Problems);

/// <summary>
/// World-side tourist planner. It converts peaceful regional population and landmark
/// attraction into bounded travelling parties and hands arrived visitors to TourismRuntime.
/// </summary>
public sealed class TouristPlannerRuntime
{
    public const int MaximumTravelDistance = 550;
    public const int MaximumParties = 32;
    public const double BaseVisitorsPerDay = 0.0025;
    public const double MinimumAttraction = 0.05;

    private readonly StrategicWorldRuntime _world;
    private readonly WorldRegionRuntime _regions;
    private readonly WorldEntityRuntime _entities;
    private readonly WorldLandmarkRuntime _landmarks;
    private readonly TourismRuntime _tourism;
    private readonly TourismReviewRuntime _reviews;
    private readonly Dictionary<long, TouristParty> _parties = new();
    private readonly Dictionary<(int Region, string Race), double> _remainders = new();
    private readonly List<TourismDemand> _demands = new();

    public TouristPlannerRuntime(
        StrategicWorldRuntime world,
        WorldRegionRuntime regions,
        WorldEntityRuntime entities,
        WorldLandmarkRuntime landmarks,
        TourismRuntime tourism,
        TourismReviewRuntime reviews)
    {
        _world = world;
        _regions = regions;
        _entities = entities;
        _landmarks = landmarks;
        _tourism = tourism;
        _reviews = reviews;
    }

    public IReadOnlyCollection<TouristParty> Parties => _parties.Values;
    public IReadOnlyList<TourismDemand> Demands => _demands;

    public void Tick(double days, int day, int playerCapitalRegionId, bool settlementOpen)
    {
        if (days <= 0) return;
        _demands.Clear();
        ProcessArrivals(day);
        RemoveDeadParties();
        if (!settlementOpen || _parties.Count >= MaximumParties) return;
        GenerateDemand(days, day, playerCapitalRegionId);
    }

    public bool CancelParty(long entityId)
    {
        if (!_parties.Remove(entityId)) return false;
        _entities.Cancel(entityId);
        return true;
    }

    public IReadOnlyDictionary<(int Region, string Race), double> CaptureRemainders() =>
        new Dictionary<(int Region, string Race), double>(_remainders);

    public void RestoreRemainders(IReadOnlyDictionary<(int Region, string Race), double> values)
    {
        _remainders.Clear();
        foreach (var pair in values)
            _remainders[pair.Key] = Math.Clamp(pair.Value, 0, 1);
    }

    public TourismForecast Forecast(int playerCapitalRegionId)
    {
        var problems = new List<string>();
        if (_world.Region(playerCapitalRegionId) is null)
            problems.Add("Player capital region does not exist.");
        var demands = new List<TourismDemand>();
        foreach (var region in _world.Regions.Where(value =>
                     value.Id != playerCapitalRegionId && value.Habitable))
        {
            var route = _world.FindRoute(region.Id, playerCapitalRegionId);
            var distance = route.Count - 1;
            if (distance < 1 || distance > MaximumTravelDistance) continue;
            var population = _regions.CapturePopulation(region.Id);
            var landmark = _landmarks.ChooseForVisitor(region.Id, _world.PlayerFactionId);
            if (landmark is null) continue;
            var distancePenalty = 1.0 / (1 + distance / 32.0);
            foreach (var race in population.Races.Where(pair => pair.Value > 0))
            {
                var reputation = _reviews.RaceScore(race.Key);
                var expected = race.Value * BaseVisitorsPerDay * landmark.Attraction *
                               distancePenalty * _reviews.AttractionMultiplier(race.Key);
                demands.Add(new TourismDemand(
                    region.Id,
                    race.Key,
                    landmark.Attraction,
                    reputation,
                    distancePenalty,
                    expected,
                    landmark.Id));
            }
        }
        foreach (var party in _parties.Values)
        {
            var entity = _entities.Get(party.EntityId);
            if (entity is null)
                problems.Add($"Tourist party {party.EntityId} has no world entity.");
            else if (entity.Kind != WorldEntityKind.TouristParty)
                problems.Add($"Tourist party {party.EntityId} references the wrong entity kind.");
            if (party.Amount <= 0)
                problems.Add($"Tourist party {party.EntityId} has no visitors.");
            if (party.BudgetPerVisitor <= 0)
                problems.Add($"Tourist party {party.EntityId} has no budget.");
        }
        var byRace = demands.GroupBy(value => value.Race, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(value => value.ExpectedVisitors),
                StringComparer.OrdinalIgnoreCase);
        return new TourismForecast(
            playerCapitalRegionId,
            demands.Select(value => value.OriginRegionId).Distinct().Count(),
            byRace.Count,
            _parties.Count,
            _parties.Values.Sum(value => value.Amount),
            demands.Sum(value => value.ExpectedVisitors),
            demands.Count == 0 ? 0 : demands.Average(value => value.DistancePenalty),
            demands.Count == 0 ? _reviews.Reputation : demands.Average(value => value.Reputation),
            byRace,
            problems);
    }

    public IReadOnlyList<string> DescribeForecast(int playerCapitalRegionId)
    {
        var forecast = Forecast(playerCapitalRegionId);
        var lines = new List<string>
        {
            $"Reachable origins {forecast.ReachableOrigins}, eligible races {forecast.EligibleRaces}",
            $"Expected visitors/day {forecast.ExpectedVisitorsPerDay:0.###}",
            $"Active parties {forecast.ActiveParties}, travelling visitors {forecast.TravellingVisitors}",
            $"Average distance factor {forecast.AverageDistancePenalty:P0}, reputation {forecast.AverageReputation:P0}"
        };
        lines.AddRange(forecast.ExpectedByRace.OrderByDescending(pair => pair.Value)
            .Select(pair => $"{pair.Key}: {pair.Value:0.###} visitors/day"));
        lines.AddRange(forecast.Problems.Select(problem => "Problem: " + problem));
        return lines;
    }

    private void GenerateDemand(double days, int day, int playerCapitalRegionId)
    {
        foreach (var region in _world.Regions)
        {
            if (region.Id == playerCapitalRegionId || !region.Habitable) continue;
            var route = _world.FindRoute(region.Id, playerCapitalRegionId);
            var distance = route.Count - 1;
            if (distance < 1 || distance > MaximumTravelDistance) continue;
            var population = _regions.CapturePopulation(region.Id);
            if (population.Population <= 0) continue;
            var landmark = _landmarks.ChooseForVisitor(region.Id, _world.PlayerFactionId);
            if (landmark is null || landmark.Attraction < MinimumAttraction) continue;
            var distancePenalty = 1.0 / (1 + distance / 32.0);
            foreach (var race in population.Races.Where(pair => pair.Value > 0))
            {
                var reputation = _reviews.RaceScore(race.Key);
                var expected = race.Value * BaseVisitorsPerDay * days * landmark.Attraction *
                               distancePenalty * _reviews.AttractionMultiplier(race.Key);
                _demands.Add(new TourismDemand(
                    region.Id,
                    race.Key,
                    landmark.Attraction,
                    reputation,
                    distancePenalty,
                    expected,
                    landmark.Id));
                CreateFromDemand(region.Id, playerCapitalRegionId, race.Key, expected, landmark.Id, day, route);
                if (_parties.Count >= MaximumParties) return;
            }
        }
    }

    private void CreateFromDemand(
        int originRegionId,
        int destinationRegionId,
        string race,
        double expected,
        int landmarkId,
        int day,
        IReadOnlyList<int> route)
    {
        var key = (originRegionId, race);
        var accumulated = expected + _remainders.GetValueOrDefault(key);
        var amount = Math.Min(TourismRuntime.MaximumPartySize, (int)Math.Floor(accumulated));
        _remainders[key] = accumulated - amount;
        if (amount <= 0) return;
        var budget = Budget(originRegionId, race, landmarkId);
        var entity = _entities.Create(
            WorldEntityKind.TouristParty,
            _world.Region(originRegionId)?.OwnerFactionId ?? -1,
            originRegionId,
            destinationRegionId,
            $"TOURISM:{race}:{amount}:{landmarkId}",
            1,
            route);
        _parties[entity.Id] = new TouristParty(
            entity.Id,
            originRegionId,
            destinationRegionId,
            race,
            amount,
            budget,
            landmarkId,
            day);
    }

    private void ProcessArrivals(int day)
    {
        foreach (var party in _parties.Values.ToArray())
        {
            var entity = _entities.Get(party.EntityId);
            if (entity?.State != WorldEntityState.Arrived) continue;
            _tourism.QueueArrivals(new TouristArrivalRequest(
                party.OriginRegionId,
                party.Race,
                party.Amount,
                party.BudgetPerVisitor,
                party.LandmarkId,
                day));
            _entities.Complete(party.EntityId);
            _parties.Remove(party.EntityId);
        }
    }

    private void RemoveDeadParties()
    {
        foreach (var party in _parties.Values.ToArray())
        {
            var state = _entities.Get(party.EntityId)?.State;
            if (state is WorldEntityState.Cancelled or WorldEntityState.Completed || state is null)
                _parties.Remove(party.EntityId);
        }
    }

    private double Budget(int originRegionId, string race, int landmarkId)
    {
        var region = _regions.CapturePopulation(originRegionId);
        var landmark = _landmarks.Get(landmarkId);
        var prosperity = Math.Clamp(region.Health * region.Loyalty * (1 - region.Devastation), 0.1, 1);
        var attraction = landmark?.Attraction ?? 0.5;
        var reputation = _reviews.RaceScore(race);
        return Math.Max(TourismRuntime.InnPricePerDay,
            80 + 320 * prosperity + 80 * attraction + 80 * reputation);
    }
}
