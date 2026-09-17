using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.World;

public enum LandmarkKind : byte
{
    Natural,
    Ancient,
    Religious,
    Civic,
    Trade,
    Scenic
}

public sealed class WorldLandmarkState
{
    public int Id { get; init; }
    public int RegionId { get; init; }
    public LandmarkKind Kind { get; init; }
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    public double BaseAttraction { get; init; }
    public double Quality { get; internal set; }
    public double Accessibility { get; internal set; }
    public double Fame { get; internal set; }
    public int Visits { get; internal set; }
    public int PositiveReviews { get; internal set; }
    public int NegativeReviews { get; internal set; }
    public bool Discovered { get; internal set; }
    public bool Active { get; internal set; } = true;

    public double Attraction => Active
        ? Math.Clamp(BaseAttraction * Quality * Accessibility * (0.75 + Fame * 0.25), 0, 4)
        : 0;
}

public sealed record LandmarkVisit(
    long VisitId,
    int LandmarkId,
    int VisitorId,
    int Day,
    double Experience,
    bool Reviewed);

public sealed record LandmarkSnapshot(
    int Id,
    int RegionId,
    LandmarkKind Kind,
    string Key,
    string Name,
    double BaseAttraction,
    double Quality,
    double Accessibility,
    double Fame,
    int Visits,
    int PositiveReviews,
    int NegativeReviews,
    bool Discovered,
    bool Active);

/// <summary>
/// WorldLandmark/WorldLandmarks semantic runtime. Visual placers and sprite classes
/// are represented by deterministic data generation and discovery state.
/// </summary>
public sealed class WorldLandmarkRuntime
{
    private readonly StrategicWorldRuntime _world;
    private readonly WorldRegionRuntime _regions;
    private readonly Dictionary<int, WorldLandmarkState> _landmarks = new();
    private readonly Dictionary<int, List<int>> _byRegion = new();
    private readonly Dictionary<long, LandmarkVisit> _visits = new();
    private int _nextId = 1;
    private long _nextVisitId = 1;

    public WorldLandmarkRuntime(StrategicWorldRuntime world, WorldRegionRuntime regions)
    {
        _world = world;
        _regions = regions;
    }

    public IReadOnlyCollection<WorldLandmarkState> Landmarks => _landmarks.Values;
    public IReadOnlyCollection<LandmarkVisit> Visits => _visits.Values;

    public WorldLandmarkState? Get(int id) => _landmarks.GetValueOrDefault(id);

    public IEnumerable<WorldLandmarkState> AtRegion(int regionId) =>
        _byRegion.TryGetValue(regionId, out var ids)
            ? ids.Select(id => _landmarks[id])
            : Enumerable.Empty<WorldLandmarkState>();

    public void Generate(int seed)
    {
        _landmarks.Clear();
        _byRegion.Clear();
        _nextId = 1;
        foreach (var source in _world.Landmarks)
        {
            var region = _world.RegionAtTile(source.CenterTileX, source.CenterTileY);
            if (region is null || !region.Habitable) continue;
            var terrainInterest = Math.Max(region.Mountain, Math.Max(region.Water, region.Forest));
            Add(region.Id, ToSimulationKind(source.Kind), $"WORLD_LANDMARK_{source.Id}",
                source.Name, 0.5 + terrainInterest * 1.5,
                Math.Clamp(0.7 + source.Area / 4000.0, 0.7, 1));
        }
    }

    public WorldLandmarkState Add(
        int regionId,
        LandmarkKind kind,
        string key,
        string name,
        double baseAttraction,
        double quality)
    {
        var region = _world.Region(regionId) ??
            throw new ArgumentOutOfRangeException(nameof(regionId));
        if (!region.Habitable) throw new InvalidOperationException("Landmark requires habitable region");
        var landmark = new WorldLandmarkState
        {
            Id = _nextId++,
            RegionId = regionId,
            Kind = kind,
            Key = key,
            Name = name,
            BaseAttraction = Math.Max(0, baseAttraction),
            Quality = Math.Clamp(quality, 0, 2),
            Accessibility = 0.5,
            Fame = 0,
            Discovered = region.OwnerFactionId == _world.PlayerFactionId
        };
        _landmarks.Add(landmark.Id, landmark);
        if (!_byRegion.TryGetValue(regionId, out var ids))
        {
            ids = new List<int>();
            _byRegion.Add(regionId, ids);
        }
        ids.Add(landmark.Id);
        return landmark;
    }

    public bool Discover(int id)
    {
        if (!_landmarks.TryGetValue(id, out var landmark)) return false;
        landmark.Discovered = true;
        return true;
    }

    public bool SetActive(int id, bool active)
    {
        if (!_landmarks.TryGetValue(id, out var landmark)) return false;
        landmark.Active = active;
        return true;
    }

    public LandmarkVisit? BeginVisit(int landmarkId, int visitorId, int day)
    {
        if (!_landmarks.TryGetValue(landmarkId, out var landmark) ||
            !landmark.Active || !landmark.Discovered) return null;
        var experience = Math.Clamp(landmark.Quality * 0.6 + landmark.Accessibility * 0.2 +
                                    landmark.Fame * 0.2, 0, 2);
        var visit = new LandmarkVisit(
            _nextVisitId++,
            landmarkId,
            visitorId,
            day,
            experience,
            false);
        _visits.Add(visit.VisitId, visit);
        landmark.Visits++;
        return visit;
    }

    public bool Review(long visitId, double score)
    {
        if (!_visits.TryGetValue(visitId, out var visit) || visit.Reviewed ||
            !_landmarks.TryGetValue(visit.LandmarkId, out var landmark)) return false;
        score = Math.Clamp(score, 0, 1);
        if (score >= 0.6) landmark.PositiveReviews++;
        else if (score < 0.4) landmark.NegativeReviews++;
        var reviewCount = landmark.PositiveReviews + landmark.NegativeReviews;
        var reputation = reviewCount == 0 ? 0.5 : landmark.PositiveReviews / (double)reviewCount;
        landmark.Fame = Math.Clamp(landmark.Fame * 0.95 + reputation * 0.05, 0, 1);
        _visits[visitId] = visit with { Reviewed = true };
        TrimVisits();
        return true;
    }

    public double RegionAttraction(int regionId)
    {
        return AtRegion(regionId).Where(value => value.Discovered).Sum(value => value.Attraction);
    }

    public double FactionAttraction(int factionId)
    {
        return _world.Regions.Where(region => region.OwnerFactionId == factionId)
            .Sum(region => RegionAttraction(region.Id));
    }

    public WorldLandmarkState? ChooseForVisitor(
        int originRegionId,
        int destinationFactionId,
        Func<int, bool>? visible = null)
    {
        return _landmarks.Values.Where(landmark =>
                landmark.Active &&
                landmark.Discovered &&
                _world.Region(landmark.RegionId)?.OwnerFactionId == destinationFactionId &&
                (visible?.Invoke(landmark.RegionId) ?? true))
            .Select(landmark => new
            {
                Landmark = landmark,
                Route = _world.FindRoute(originRegionId, landmark.RegionId)
            })
            .Where(value => value.Route.Count > 0)
            .OrderByDescending(value => value.Landmark.Attraction / Math.Max(1, value.Route.Count - 1))
            .Select(value => value.Landmark)
            .FirstOrDefault();
    }

    public void Tick(double days)
    {
        if (days <= 0) return;
        foreach (var landmark in _landmarks.Values)
        {
            var region = _regions.CapturePopulation(landmark.RegionId);
            var routeAccess = _world.Roads.Count(road =>
                road.FirstRegionId == landmark.RegionId || road.SecondRegionId == landmark.RegionId);
            var targetAccess = Math.Clamp(0.25 + routeAccess * 0.15 + region.Health * 0.35 -
                                          region.Devastation * 0.5, 0, 1);
            landmark.Accessibility = MoveTowards(landmark.Accessibility, targetAccess, days / 8.0);
            if (landmark.Visits == 0)
                landmark.Fame = Math.Max(0, landmark.Fame - days / 256.0);
        }
    }

    public IReadOnlyList<LandmarkSnapshot> Capture() => _landmarks.Values.Select(value => new LandmarkSnapshot(
        value.Id,
        value.RegionId,
        value.Kind,
        value.Key,
        value.Name,
        value.BaseAttraction,
        value.Quality,
        value.Accessibility,
        value.Fame,
        value.Visits,
        value.PositiveReviews,
        value.NegativeReviews,
        value.Discovered,
        value.Active)).ToArray();

    public void Restore(IEnumerable<LandmarkSnapshot> snapshots)
    {
        _landmarks.Clear();
        _byRegion.Clear();
        _nextId = 1;
        foreach (var snapshot in snapshots)
        {
            var landmark = new WorldLandmarkState
            {
                Id = snapshot.Id,
                RegionId = snapshot.RegionId,
                Kind = snapshot.Kind,
                Key = snapshot.Key,
                Name = snapshot.Name,
                BaseAttraction = snapshot.BaseAttraction,
                Quality = snapshot.Quality,
                Accessibility = snapshot.Accessibility,
                Fame = snapshot.Fame,
                Visits = snapshot.Visits,
                PositiveReviews = snapshot.PositiveReviews,
                NegativeReviews = snapshot.NegativeReviews,
                Discovered = snapshot.Discovered,
                Active = snapshot.Active
            };
            _landmarks.Add(landmark.Id, landmark);
            if (!_byRegion.TryGetValue(landmark.RegionId, out var ids))
            {
                ids = new List<int>();
                _byRegion.Add(landmark.RegionId, ids);
            }
            ids.Add(landmark.Id);
            _nextId = Math.Max(_nextId, landmark.Id + 1);
        }
    }

    private void TrimVisits()
    {
        const int limit = 1024;
        if (_visits.Count <= limit) return;
        foreach (var id in _visits.Keys.OrderBy(value => value).Take(_visits.Count - limit).ToArray())
            _visits.Remove(id);
    }

    private static LandmarkKind ToSimulationKind(StrategicLandmarkKind kind)
    {
        return kind switch
        {
            StrategicLandmarkKind.Mountain => LandmarkKind.Natural,
            StrategicLandmarkKind.Lake => LandmarkKind.Scenic,
            StrategicLandmarkKind.River => LandmarkKind.Trade,
            StrategicLandmarkKind.Ocean => LandmarkKind.Scenic,
            _ => LandmarkKind.Ancient
        };
    }

    private static double MoveTowards(double current, double target, double delta)
    {
        if (current < target) return Math.Min(target, current + delta);
        return Math.Max(target, current - delta);
    }
}
