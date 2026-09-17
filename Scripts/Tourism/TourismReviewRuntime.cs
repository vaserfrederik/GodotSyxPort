using System;
using System.Collections.Generic;
using System.Linq;
namespace GodotSyxPort.Tourism;
public enum TourismReviewTopic : byte
{
    Inn,
    Landmark,
    Safety,
    Services,
    Value,
    Overall
}
public sealed record TourismReview(
    long Id,
    int TouristId,
    string Race,
    int OriginRegionId,
    TourismReviewTopic Topic,
    double Score,
    double Weight,
    int Day,
    string Subject,
    string Text);
public sealed record TourismReputationSnapshot(
    double Reputation,
    double RecentAverage,
    int Reviews,
    int Positive,
    int Negative,
    IReadOnlyDictionary<TourismReviewTopic, double> Topics,
    IReadOnlyDictionary<string, double> RaceScores);
/// <summary>
/// Deterministic review and reputation aggregation for Review/Text/TOURISM.
/// Text is generated from simulation facts and does not depend on legacy UI assets.
/// </summary>
public sealed class TourismReviewRuntime
{
    public const int HistoryLimit = 512;
    public const int RecentWindow = 64;
    public const double PositiveThreshold = 0.65;
    public const double NegativeThreshold = 0.35;
    private readonly List<TourismReview> _reviews = new();
    private readonly Dictionary<TourismReviewTopic, double> _topicScores = new();
    private readonly Dictionary<string, double> _raceScores = new(StringComparer.OrdinalIgnoreCase);
    private long _nextId = 1;
    public IReadOnlyList<TourismReview> Reviews => _reviews;
    public double Reputation { get; private set; } = 0.5;
    public int Positive => _reviews.Count(review => review.Score >= PositiveThreshold);
    public int Negative => _reviews.Count(review => review.Score <= NegativeThreshold);
    public TourismReview Add(
        int touristId,
        string race,
        int originRegionId,
        TourismReviewTopic topic,
        double score,
        double weight,
        int day,
        string subject)
    {
        score = Math.Clamp(score, 0, 1);
        weight = Math.Clamp(weight, 0.05, 4);
        var review = new TourismReview(
            _nextId++,
            touristId,
            race,
            originRegionId,
            topic,
            score,
            weight,
            day,
            subject,
            ComposeText(topic, score, subject));
        _reviews.Add(review);
        if (_reviews.Count > HistoryLimit) _reviews.RemoveRange(0, _reviews.Count - HistoryLimit);
        Recalculate();
        return review;
    }
    public IReadOnlyList<TourismReview> AddVisitSummary(
        int touristId,
        string race,
        int originRegionId,
        int day,
        double inn,
        double landmark,
        double safety,
        double services,
        double value,
        string innName,
        string landmarkName)
    {
        var result = new List<TourismReview>
        {
            Add(touristId, race, originRegionId, TourismReviewTopic.Inn, inn, 1, day, innName),
            Add(touristId, race, originRegionId, TourismReviewTopic.Landmark, landmark, 1, day, landmarkName),
            Add(touristId, race, originRegionId, TourismReviewTopic.Safety, safety, 0.75, day, "settlement"),
            Add(touristId, race, originRegionId, TourismReviewTopic.Services, services, 0.75, day, "services"),
            Add(touristId, race, originRegionId, TourismReviewTopic.Value, value, 0.5, day, "visit")
        };
        var overall = WeightedAverage(result.Select(review => (review.Score, review.Weight)));
        result.Add(Add(touristId, race, originRegionId, TourismReviewTopic.Overall,
            overall, 2, day, "settlement"));
        return result;
    }
    public double TopicScore(TourismReviewTopic topic) => _topicScores.GetValueOrDefault(topic, 0.5);
    public double RaceScore(string race) => _raceScores.GetValueOrDefault(race, Reputation);
    public double RecentAverage(int count = RecentWindow)
    {
        return WeightedAverage(_reviews.TakeLast(Math.Max(1, count)).Select(review =>
            (review.Score, review.Weight)));
    }
    public double AttractionMultiplier(string race)
    {
        var blended = Reputation * 0.6 + RaceScore(race) * 0.4;
        return Math.Clamp(0.25 + blended * 1.5, 0.25, 1.75);
    }
    public TourismReputationSnapshot CaptureSummary()
    {
        return new TourismReputationSnapshot(
            Reputation,
            RecentAverage(),
            _reviews.Count,
            Positive,
            Negative,
            new Dictionary<TourismReviewTopic, double>(_topicScores),
            new Dictionary<string, double>(_raceScores, StringComparer.OrdinalIgnoreCase));
    }
    public IReadOnlyList<TourismReview> CaptureReviews() => _reviews.ToArray();
    public void Restore(IEnumerable<TourismReview> reviews)
    {
        _reviews.Clear();
        _reviews.AddRange(reviews.OrderBy(review => review.Id).TakeLast(HistoryLimit));
        _nextId = _reviews.Count == 0 ? 1 : _reviews.Max(review => review.Id) + 1;
        Recalculate();
    }
    public void Decay(double days)
    {
        if (days <= 0 || _reviews.Count == 0) return;
        var neutralPull = 1 - Math.Pow(0.5, days / 64.0);
        Reputation += (0.5 - Reputation) * neutralPull;
        foreach (var topic in _topicScores.Keys.ToArray())
            _topicScores[topic] += (0.5 - _topicScores[topic]) * neutralPull;
        foreach (var race in _raceScores.Keys.ToArray())
            _raceScores[race] += (0.5 - _raceScores[race]) * neutralPull;
    }
    private void Recalculate()
    {
        if (_reviews.Count == 0)
        {
            Reputation = 0.5;
            _topicScores.Clear();
            _raceScores.Clear();
            return;
        }
        var recent = _reviews.TakeLast(RecentWindow).ToArray();
        Reputation = WeightedAverage(recent.Where(review => review.Topic == TourismReviewTopic.Overall)
            .DefaultIfEmpty(recent[^1]).Select(review => (review.Score, review.Weight)));
        _topicScores.Clear();
        foreach (var group in recent.GroupBy(review => review.Topic))
            _topicScores[group.Key] = WeightedAverage(group.Select(review => (review.Score, review.Weight)));
        _raceScores.Clear();
        foreach (var group in recent.GroupBy(review => review.Race, StringComparer.OrdinalIgnoreCase))
            _raceScores[group.Key] = WeightedAverage(group.Select(review => (review.Score, review.Weight)));
    }
    private static double WeightedAverage(IEnumerable<(double Score, double Weight)> values)
    {
        var materialized = values.ToArray();
        var weight = materialized.Sum(value => Math.Max(0, value.Weight));
        return weight <= 0
            ? 0.5
            : Math.Clamp(materialized.Sum(value => value.Score * Math.Max(0, value.Weight)) / weight, 0, 1);
    }
    private static string ComposeText(TourismReviewTopic topic, double score, string subject)
    {
        var sentiment = score >= PositiveThreshold
            ? "excellent"
            : score <= NegativeThreshold
                ? "poor"
                : "adequate";
        return topic switch
        {
            TourismReviewTopic.Inn => $"The stay at {subject} was {sentiment}.",
            TourismReviewTopic.Landmark => $"The visit to {subject} was {sentiment}.",
            TourismReviewTopic.Safety => $"Safety in {subject} felt {sentiment}.",
            TourismReviewTopic.Services => $"The available {subject} were {sentiment}.",
            TourismReviewTopic.Value => $"The value of the {subject} was {sentiment}.",
            _ => $"The overall experience in {subject} was {sentiment}."
        };
    }
}
