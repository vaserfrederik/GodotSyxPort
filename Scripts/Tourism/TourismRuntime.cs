using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Rooms;
using GodotSyxPort.World;
namespace GodotSyxPort.Tourism;
public enum TouristState : byte
{
    Planned,
    Travelling,
    Entering,
    SeekingInn,
    Resting,
    Sightseeing,
    Shopping,
    Reviewing,
    Departing,
    Left,
    Rejected
}
public sealed class TouristVisit
{
    public int Id { get; init; }
    public int CitizenId { get; internal set; }
    public string Race { get; init; } = "HUMAN";
    public int OriginRegionId { get; init; }
    public int TargetRegionId { get; init; }
    public TouristState State { get; internal set; }
    public int InnRoomId { get; internal set; }
    public int LandmarkId { get; internal set; }
    public int ArrivalDay { get; internal set; } = -1;
    public int DepartureDay { get; internal set; } = -1;
    public double StayDays { get; internal set; }
    public double StateProgress { get; internal set; }
    public double Budget { get; internal set; }
    public double Spent { get; internal set; }
    public double InnQuality { get; internal set; }
    public double LandmarkQuality { get; internal set; }
    public double Safety { get; internal set; } = 0.5;
    public double ServiceQuality { get; internal set; } = 0.5;
    public bool ReviewWritten { get; internal set; }
    public List<long> LandmarkVisits { get; } = new();
    public bool Active => State is not TouristState.Left and not TouristState.Rejected;
}
public sealed record TouristArrivalRequest(
    int OriginRegionId,
    string Race,
    int Amount,
    double BudgetPerVisitor,
    int TargetLandmarkId,
    int Day);
public sealed record TouristIncome(
    int TouristId,
    int Day,
    double Inn,
    double Services,
    double Shopping,
    double Total);

public sealed record TouristVisitSnapshot(
    int Id,
    int CitizenId,
    string Race,
    int OriginRegionId,
    int TargetRegionId,
    TouristState State,
    int InnRoomId,
    int LandmarkId,
    int ArrivalDay,
    int DepartureDay,
    double StayDays,
    double StateProgress,
    double Budget,
    double Spent,
    double InnQuality,
    double LandmarkQuality,
    double Safety,
    double ServiceQuality,
    bool ReviewWritten,
    IReadOnlyList<long> LandmarkVisits);

/// <summary>
/// Shared TOURISM/TourismRace/Updater runtime. Tourists enter through the settlement
/// boundary, reserve real inn capacity, visit world landmarks, spend money, review and
/// leave instead of becoming permanent citizens.
/// </summary>
public sealed class TourismRuntime
{
    // Tourism remains a peaceful settlement subsystem.
    // Visitors never enter the permanent labor pool.
    // Inn capacity is reserved before a stay begins.
    // Spending is recorded before departure and review generation.
    // Rendering consumes snapshots and does not own simulation state.
    public const int MaximumVisitors = 256;
    public const int MaximumPartySize = 16;
    public const double MinimumStayDays = 1;
    public const double MaximumStayDays = 8;
    public const double InnPricePerDay = 40;
    public const double ServiceBudgetFraction = 0.25;
    public const double ShoppingBudgetFraction = 0.20;

    private readonly WorldLandmarkRuntime _landmarks;
    private readonly TourismReviewRuntime _reviews;
    private readonly Dictionary<int, TouristVisit> _visits = new();
    private readonly Queue<TouristArrivalRequest> _requests = new();
    private readonly List<TouristIncome> _income = new();
    private readonly List<int> _departures = new();
    private int _nextId = 1;

    public TourismRuntime(WorldLandmarkRuntime landmarks, TourismReviewRuntime reviews)
    {
        _landmarks = landmarks;
        _reviews = reviews;
    }

    public IReadOnlyCollection<TouristVisit> Visits => _visits.Values;
    public IReadOnlyList<TouristIncome> Income => _income;
    public IReadOnlyList<int> Departures => _departures;
    public int ActiveVisitors => _visits.Values.Count(value => value.Active);
    public double TotalIncome { get; private set; }

    public int QueueArrivals(TouristArrivalRequest request)
    {
        if (request.Amount <= 0 || request.BudgetPerVisitor <= 0) return 0;
        var free = Math.Max(0, MaximumVisitors - ActiveVisitors - _requests.Sum(value => value.Amount));
        var accepted = Math.Min(Math.Min(request.Amount, MaximumPartySize), free);
        if (accepted <= 0) return 0;
        _requests.Enqueue(request with { Amount = accepted });
        return accepted;
    }

    public IReadOnlyList<TouristVisit> AdmitQueued(int targetRegionId, int day)
    {
        var admitted = new List<TouristVisit>();
        while (_requests.Count > 0 && ActiveVisitors < MaximumVisitors)
        {
            var request = _requests.Dequeue();
            for (var index = 0; index < request.Amount && ActiveVisitors < MaximumVisitors; index++)
            {
                var visit = new TouristVisit
                {
                    Id = _nextId++,
                    Race = request.Race,
                    OriginRegionId = request.OriginRegionId,
                    TargetRegionId = targetRegionId,
                    State = TouristState.Entering,
                    LandmarkId = request.TargetLandmarkId,
                    ArrivalDay = day,
                    StayDays = StableStayDays(request.OriginRegionId, request.Day, index),
                    Budget = request.BudgetPerVisitor
                };
                _visits.Add(visit.Id, visit);
                admitted.Add(visit);
            }
        }
        return admitted;
    }

    public bool BindCitizen(int visitId, int citizenId)
    {
        if (!_visits.TryGetValue(visitId, out var visit) || citizenId <= 0 || visit.CitizenId != 0)
            return false;
        visit.CitizenId = citizenId;
        visit.State = TouristState.SeekingInn;
        visit.StateProgress = 0;
        return true;
    }

    public bool ReserveInn(int visitId, HospitalityRuntime hospitality)
    {
        if (!_visits.TryGetValue(visitId, out var visit) ||
            visit.State != TouristState.SeekingInn || visit.CitizenId <= 0) return false;
        var room = hospitality.Instances
            .Where(value => value.Kind == HospitalityKind.Inn && value.Free > 0)
            .OrderByDescending(value => value.Quality)
            .ThenBy(value => value.RoomId)
            .FirstOrDefault();
        if (room is null || !hospitality.TryReserve(room.RoomId, visit.CitizenId)) return false;
        visit.InnRoomId = room.RoomId;
        visit.InnQuality = room.Quality;
        visit.State = TouristState.Resting;
        visit.StateProgress = 0;
        return true;
    }

    public void RejectWithoutInn(int visitId, int day)
    {
        if (!_visits.TryGetValue(visitId, out var visit) || !visit.Active) return;
        visit.State = TouristState.Rejected;
        visit.DepartureDay = day;
        _departures.Add(visit.Id);
    }

    public void Tick(
        double days,
        int day,
        HospitalityRuntime hospitality,
        double settlementSafety,
        double serviceQuality)
    {
        if (days <= 0) return;
        _departures.Clear();
        _reviews.Decay(days);
        foreach (var visit in _visits.Values.Where(value => value.Active).ToArray())
        {
            visit.Safety = Math.Clamp(settlementSafety, 0, 1);
            visit.ServiceQuality = Math.Clamp(serviceQuality, 0, 1);
            switch (visit.State)
            {
                case TouristState.Entering:
                    visit.StateProgress += days;
                    if (visit.StateProgress >= 0.25)
                    {
                        visit.State = TouristState.SeekingInn;
                        visit.StateProgress = 0;
                    }
                    break;
                case TouristState.SeekingInn:
                    if (!ReserveInn(visit.Id, hospitality))
                    {
                        visit.StateProgress += days;
                        if (visit.StateProgress >= 1) RejectWithoutInn(visit.Id, day);
                    }
                    break;
                case TouristState.Resting:
                    TickResting(visit, days);
                    break;
                case TouristState.Sightseeing:
                    TickSightseeing(visit, days, day);
                    break;
                case TouristState.Shopping:
                    TickShopping(visit, days);
                    break;
                case TouristState.Reviewing:
                    FinishReview(visit, day, hospitality);
                    break;
                case TouristState.Departing:
                    visit.StateProgress += days;
                    if (visit.StateProgress >= 0.25)
                    {
                        hospitality.Release(visit.CitizenId);
                        visit.State = TouristState.Left;
                        visit.DepartureDay = day;
                        _departures.Add(visit.Id);
                    }
                    break;
            }
        }
        TrimHistory(day);
    }

    public bool ForceDeparture(int visitId, int day, HospitalityRuntime hospitality)
    {
        if (!_visits.TryGetValue(visitId, out var visit) || !visit.Active) return false;
        hospitality.Release(visit.CitizenId);
        visit.State = TouristState.Left;
        visit.DepartureDay = day;
        _departures.Add(visit.Id);
        return true;
    }

    public TouristVisit? ByCitizen(int citizenId) =>
        _visits.Values.FirstOrDefault(value => value.CitizenId == citizenId && value.Active);

    public TouristVisit? Get(int visitId) => _visits.GetValueOrDefault(visitId);

    public IReadOnlyDictionary<string, (int Active, int Completed, double Income)> SummaryByRace()
    {
        var result = new Dictionary<string, (int Active, int Completed, double Income)>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var group in _visits.Values.GroupBy(value => value.Race, StringComparer.OrdinalIgnoreCase))
        {
            var ids = group.Select(value => value.Id).ToHashSet();
            result[group.Key] = (
                group.Count(value => value.Active),
                group.Count(value => value.State == TouristState.Left),
                _income.Where(value => ids.Contains(value.TouristId)).Sum(value => value.Total));
        }
        return result;
    }

    public IReadOnlyList<string> DescribeVisitors()
    {
        var lines = new List<string>();
        foreach (var visit in _visits.Values.Where(value => value.Active).OrderBy(value => value.Id))
        {
            lines.Add(
                $"Tourist #{visit.Id} {visit.Race}: {visit.State}, " +
                $"inn #{visit.InnRoomId}, landmark #{visit.LandmarkId}, " +
                $"spent {visit.Spent:0.##}/{visit.Budget:0.##}");
        }
        if (lines.Count == 0) lines.Add("No active tourists.");
        return lines;
    }

    public IReadOnlyList<TouristVisitSnapshot> Capture() => _visits.Values
        .Where(value => value.Active || value.DepartureDay >= 0)
        .Select(value => new TouristVisitSnapshot(
            value.Id,
            value.CitizenId,
            value.Race,
            value.OriginRegionId,
            value.TargetRegionId,
            value.State,
            value.InnRoomId,
            value.LandmarkId,
            value.ArrivalDay,
            value.DepartureDay,
            value.StayDays,
            value.StateProgress,
            value.Budget,
            value.Spent,
            value.InnQuality,
            value.LandmarkQuality,
            value.Safety,
            value.ServiceQuality,
            value.ReviewWritten,
            value.LandmarkVisits.ToArray()))
        .ToArray();

    public void Restore(IEnumerable<TouristVisitSnapshot> snapshots)
    {
        _visits.Clear();
        _nextId = 1;
        foreach (var snapshot in snapshots)
        {
            var visit = new TouristVisit
            {
                Id = snapshot.Id,
                CitizenId = snapshot.CitizenId,
                Race = snapshot.Race,
                OriginRegionId = snapshot.OriginRegionId,
                TargetRegionId = snapshot.TargetRegionId,
                State = snapshot.State,
                InnRoomId = snapshot.InnRoomId,
                LandmarkId = snapshot.LandmarkId,
                ArrivalDay = snapshot.ArrivalDay,
                DepartureDay = snapshot.DepartureDay,
                StayDays = snapshot.StayDays,
                StateProgress = snapshot.StateProgress,
                Budget = snapshot.Budget,
                Spent = snapshot.Spent,
                InnQuality = snapshot.InnQuality,
                LandmarkQuality = snapshot.LandmarkQuality,
                Safety = snapshot.Safety,
                ServiceQuality = snapshot.ServiceQuality,
                ReviewWritten = snapshot.ReviewWritten
            };
            visit.LandmarkVisits.AddRange(snapshot.LandmarkVisits);
            _visits.Add(visit.Id, visit);
            _nextId = Math.Max(_nextId, visit.Id + 1);
        }
    }

    private void TickResting(TouristVisit visit, double days)
    {
        visit.StateProgress += days;
        var charge = Math.Min(visit.Budget - visit.Spent, InnPricePerDay * days);
        if (charge > 0)
        {
            visit.Spent += charge;
            AddIncome(visit.Id, 0, charge, 0, 0);
        }
        if (visit.StateProgress < 0.5) return;
        visit.State = TouristState.Sightseeing;
        visit.StateProgress = 0;
    }

    private void TickSightseeing(TouristVisit visit, double days, int day)
    {
        visit.StateProgress += days;
        var landmark = _landmarks.Get(visit.LandmarkId);
        if (landmark is not null && visit.LandmarkVisits.Count == 0)
        {
            var result = _landmarks.BeginVisit(landmark.Id, visit.Id, day);
            if (result is not null)
            {
                visit.LandmarkVisits.Add(result.VisitId);
                visit.LandmarkQuality = result.Experience;
            }
        }
        if (visit.StateProgress < 0.5) return;
        visit.State = TouristState.Shopping;
        visit.StateProgress = 0;
    }

    private void TickShopping(TouristVisit visit, double days)
    {
        visit.StateProgress += days;
        var available = Math.Max(0, visit.Budget - visit.Spent);
        var service = Math.Min(available, visit.Budget * ServiceBudgetFraction * days);
        available -= service;
        var shopping = Math.Min(available, visit.Budget * ShoppingBudgetFraction * days);
        visit.Spent += service + shopping;
        AddIncome(visit.Id, 0, 0, service, shopping);
        var elapsed = visit.StateProgress + 1;
        if (elapsed < visit.StayDays && visit.Spent < visit.Budget)
        {
            if (visit.StateProgress >= 1)
            {
                visit.State = TouristState.Resting;
                visit.StateProgress = 0;
            }
            return;
        }
        visit.State = TouristState.Reviewing;
        visit.StateProgress = 0;
    }

    private void FinishReview(TouristVisit visit, int day, HospitalityRuntime hospitality)
    {
        if (!visit.ReviewWritten)
        {
            var value = visit.Budget <= 0 ? 0.5 : Math.Clamp(1 - visit.Spent / visit.Budget * 0.5, 0, 1);
            var landmark = _landmarks.Get(visit.LandmarkId);
            var summary = _reviews.AddVisitSummary(
                visit.Id,
                visit.Race,
                visit.OriginRegionId,
                day,
                visit.InnQuality,
                visit.LandmarkQuality,
                visit.Safety,
                visit.ServiceQuality,
                value,
                visit.InnRoomId == 0 ? "no inn" : $"inn #{visit.InnRoomId}",
                landmark?.Name ?? "settlement");
            var landmarkReview = summary.FirstOrDefault(review => review.Topic == TourismReviewTopic.Landmark);
            foreach (var landmarkVisit in visit.LandmarkVisits)
                _landmarks.Review(landmarkVisit, landmarkReview?.Score ?? 0.5);
            visit.ReviewWritten = true;
        }
        hospitality.Release(visit.CitizenId);
        visit.State = TouristState.Departing;
        visit.StateProgress = 0;
    }

    private void AddIncome(int touristId, int day, double inn, double services, double shopping)
    {
        var total = inn + services + shopping;
        if (total <= 0) return;
        TotalIncome += total;
        _income.Add(new TouristIncome(touristId, day, inn, services, shopping, total));
        if (_income.Count > 512) _income.RemoveRange(0, _income.Count - 512);
    }

    private void TrimHistory(int day)
    {
        foreach (var visit in _visits.Values.Where(value =>
                     !value.Active && value.DepartureDay >= 0 && day - value.DepartureDay > 32).ToArray())
            _visits.Remove(visit.Id);
    }

    private static double StableStayDays(int origin, int day, int index)
    {
        unchecked
        {
            uint value = (uint)(origin * 73856093 ^ day * 19349663 ^ index * 83492791);
            value ^= value >> 16;
            var unit = (value & 0xffff) / 65535.0;
            return MinimumStayDays + unit * (MaximumStayDays - MinimumStayDays);
        }
    }
}
