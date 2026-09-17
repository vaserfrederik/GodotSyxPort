using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.Stats;

namespace GodotSyxPort.Events;

public enum SettlementEventKind : byte
{
    Scripted, Emigration, RaceBrawl, ReligiousBrawl, Strike, Riot,
    Epidemic, MildDisease, Temperature, WorkAccident, SerialKiller, SlaveUprising
}

public enum EventActionKind : byte
{
    Resource, SubjectsAdd, SubjectsKill, Outbreak, Pardon, Credits,
    Boost, PermanentBoost, RemovePermanentBoost, Opinion, ClearOpinion,
    TriggerEvent, AlterSelection, Orchard, Pasture, Destruction, Earthquake,
    Weather, RegionPopulation, Invasion, AmbientSound, Color
}

public enum EventComparison : byte { Less, LessOrEqual, Equal, GreaterOrEqual, Greater }

public sealed record EventCondition(string Value, EventComparison Comparison, double Amount)
{
    public bool Evaluate(IReadOnlyDictionary<string, double> values)
    {
        var actual = values.GetValueOrDefault(Value);
        return Comparison switch
        {
            EventComparison.Less => actual < Amount,
            EventComparison.LessOrEqual => actual <= Amount,
            EventComparison.Equal => Math.Abs(actual - Amount) < 0.000001,
            EventComparison.GreaterOrEqual => actual >= Amount,
            _ => actual > Amount
        };
    }
}

public sealed record EventAction(
    EventActionKind Kind, string Key = "", double Amount = 0,
    IReadOnlyDictionary<string, double>? Values = null);
public sealed record EventChoice(
    string Key, IReadOnlyList<EventCondition> Conditions, IReadOnlyList<EventAction> Actions);
public sealed record EventDefinition(
    string Key, IReadOnlySet<string> Tags, IReadOnlyList<EventCondition> Conditions,
    IReadOnlyList<EventAction> Actions, IReadOnlyList<EventChoice> Choices,
    double CooldownDays = 0, double DurationDays = 0);
public sealed record EventNotice(
    long Sequence, SettlementEventKind Kind, string Key, int Amount,
    double StartedAt, double EndsAt, IReadOnlyDictionary<string, string> Context);
public sealed record EventActionResult(EventAction Action, int Affected, bool Deferred, string Reason = "");

/// <summary>
/// Settlement-side equivalent of game.event.engine and the non-UI portions of the citizen,
/// disaster, killer and uprising event families. Messages are immutable notices; presentation
/// and world-map-only actions deliberately remain outside this runtime.
/// </summary>
public sealed class SettlementEventRuntime
{
    public const double CitizenBreakPoint = 0.85;
    private readonly Random _random;
    private readonly Dictionary<string, EventDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _cooldowns = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<EventNotice> _notices = new();
    private readonly HashSet<int> _rioters = new();
    private readonly HashSet<int> _uprisers = new();
    private double _citizenCheckLeft = 15;
    private double _diseaseTimer = 1;
    private double _mildDiseaseTimer;
    private double _uprisingAccumulator;
    private double _riotDelay;
    private double _strikeLeft;
    private double _killerTimer;
    private double _killerRate;
    private int _killerId;
    private int _killerVictims;
    private long _sequence;
    private WorkProfession? _strikingProfession;

    public SettlementEventRuntime(int seed = 0x535958) => _random = new Random(seed);
    public IReadOnlyList<EventNotice> Notices => _notices;
    public IReadOnlyCollection<int> Rioters => _rioters;
    public IReadOnlyCollection<int> Uprisers => _uprisers;
    public WorkProfession? StrikingProfession => _strikeLeft > 0 ? _strikingProfession : null;
    public int SerialMurders => _killerVictims;

    public void Register(EventDefinition definition) => _definitions[definition.Key] = definition;

    public bool Trigger(string key, IReadOnlyDictionary<string, double> values,
        CitizenSystem citizens, ResourceLedger resources, JobBoard jobs, GridCoord exit,
        out IReadOnlyList<EventActionResult> results)
    {
        results = Array.Empty<EventActionResult>();
        if (!_definitions.TryGetValue(key, out var definition) ||
            _cooldowns.GetValueOrDefault(key) > 0 ||
            definition.Conditions.Any(condition => !condition.Evaluate(values))) return false;
        results = Execute(definition.Actions, citizens, resources, jobs, exit);
        _cooldowns[key] = definition.CooldownDays;
        Notice(SettlementEventKind.Scripted, key, results.Sum(result => result.Affected), 0,
            new Dictionary<string, string>());
        return true;
    }

    public IReadOnlyList<EventActionResult> Choose(string eventKey, string choiceKey,
        IReadOnlyDictionary<string, double> values, CitizenSystem citizens,
        ResourceLedger resources, JobBoard jobs, GridCoord exit)
    {
        if (!_definitions.TryGetValue(eventKey, out var definition)) return Array.Empty<EventActionResult>();
        var choice = definition.Choices.FirstOrDefault(value =>
            value.Key.Equals(choiceKey, StringComparison.OrdinalIgnoreCase));
        return choice is null || choice.Conditions.Any(condition => !condition.Evaluate(values))
            ? Array.Empty<EventActionResult>()
            : Execute(choice.Actions, citizens, resources, jobs, exit);
    }

    public void Tick(double delta, double playedSeconds, int secondsPerDay,
        CitizenSystem citizens, ResourceLedger resources, JobBoard jobs,
        SettlementStatsRuntime stats, RoomSystem rooms, GridCoord exit, double temperatureDeviation = 0)
    {
        var days = delta / secondsPerDay;
        foreach (var key in _cooldowns.Keys.ToArray()) _cooldowns[key] = Math.Max(0, _cooldowns[key] - days);
        _strikeLeft = Math.Max(0, _strikeLeft - delta);
        if (_strikeLeft == 0) _strikingProfession = null;
        UpdateRiot(delta, playedSeconds, secondsPerDay, citizens, stats, rooms);
        UpdateUprising(delta, playedSeconds, secondsPerDay, citizens, stats);
        UpdateDisease(delta, playedSeconds, secondsPerDay, citizens, stats);
        UpdateKiller(delta, playedSeconds, secondsPerDay, citizens, resources, jobs);
        UpdateTemperature(playedSeconds, secondsPerDay, temperatureDeviation);
        _citizenCheckLeft -= delta;
        if (_citizenCheckLeft <= 0)
        {
            _citizenCheckLeft += 15;
            UpdateCitizenPressure(playedSeconds, secondsPerDay, citizens, stats, exit);
        }
    }

    public bool IsStriking(EventCitizenSnapshot citizen) =>
        _strikeLeft > 0 && citizen.Profession == _strikingProfession;

    private void UpdateCitizenPressure(double now, int secondsPerDay, CitizenSystem citizens,
        SettlementStatsRuntime stats, GridCoord exit)
    {
        var population = citizens.EventPopulation();
        if (population.Count == 0) return;
        var unhappy = population.Where(person => person.Class == SocialClass.Citizen &&
            stats.Standing.Get(person.Race, SocialClass.Citizen).Loyalty < CitizenBreakPoint).ToArray();
        if (unhappy.Length < 5) return;
        var worst = unhappy.GroupBy(person => person.Race).OrderByDescending(group => group.Count()).First();
        var loyalty = stats.Standing.Get(worst.Key, SocialClass.Citizen).Loyalty;
        if (_random.NextDouble() > Math.Clamp((CitizenBreakPoint - loyalty) * worst.Count() /
            Math.Max(5.0, population.Count), 0, 1)) return;
        var roll = _random.Next(3);
        if (roll == 0)
        {
            var ids = Sample(worst.Select(person => person.Id), Math.Max(1, worst.Count() / 10));
            var amount = citizens.BeginEventEmigration(ids, exit);
            Notice(SettlementEventKind.Emigration, "CITIZEN_EMIGRATION", amount, now, Race(worst.Key));
        }
        else if (roll == 1 && _strikeLeft <= 0)
        {
            var workers = worst.Where(person => person.Profession != WorkProfession.Laborer).ToArray();
            if (workers.Length == 0) return;
            _strikingProfession = workers.GroupBy(person => person.Profession)
                .OrderByDescending(group => group.Count()).First().Key;
            _strikeLeft = secondsPerDay * 1.5;
            Notice(SettlementEventKind.Strike, "CITIZEN_STRIKE", workers.Length, now,
                new Dictionary<string, string> { ["race"] = worst.Key, ["profession"] = _strikingProfession.ToString()! });
        }
        else if (_rioters.Count == 0)
        {
            foreach (var id in Sample(worst.Select(person => person.Id),
                         Math.Max(1, (int)Math.Ceiling(worst.Count() * _random.NextDouble())))) _rioters.Add(id);
            _riotDelay = secondsPerDay * (0.25 + _random.NextDouble() * 0.75);
        }
    }

    private void UpdateRiot(double delta, double now, int secondsPerDay, CitizenSystem citizens,
        SettlementStatsRuntime stats, RoomSystem rooms)
    {
        _rioters.IntersectWith(citizens.EventPopulation().Select(person => person.Id));
        if (_rioters.Count == 0) return;
        _riotDelay -= delta;
        if (_riotDelay > 0) return;
        if (!_notices.Any(notice => notice.Key == "CITIZEN_RIOT" && notice.EndsAt > now))
        {
            Notice(SettlementEventKind.Riot, "CITIZEN_RIOT", _rioters.Count, now,
                now + secondsPerDay * 4, new Dictionary<string, string>());
            stats.Standing.Emergency(SocialClass.Citizen, secondsPerDay, secondsPerDay * 4);
        }
        var suppression = rooms.Military.Recruits + rooms.Law.GuardWorkers;
        if (_rioters.Count <= Math.Max(1, suppression * 3 / 10) || _random.NextDouble() < delta / (secondsPerDay * 4))
            _rioters.Clear();
    }

    private void UpdateUprising(double delta, double now, int secondsPerDay,
        CitizenSystem citizens, SettlementStatsRuntime stats)
    {
        var slaves = citizens.EventPopulation().Where(person => person.Class == SocialClass.Slave).ToArray();
        _uprisers.IntersectWith(slaves.Select(person => person.Id));
        if (_uprisers.Count > 0) return;
        var loyalty = slaves.Length == 0 ? 1 : slaves.Average(person =>
            stats.Standing.Get(person.Race, SocialClass.Slave).Loyalty);
        var speed = 5.0 / (secondsPerDay * 8);
        _uprisingAccumulator += delta / 5.0 * speed * Math.Sqrt(Math.Clamp(1 - loyalty / 0.8, 0, 1));
        if (_uprisingAccumulator < 1 || slaves.Length == 0) return;
        _uprisingAccumulator = 0;
        var fraction = 0.2 + _random.NextDouble() * 0.8;
        foreach (var id in Sample(slaves.Select(person => person.Id), (int)(slaves.Length * fraction)))
            _uprisers.Add(id);
        Notice(SettlementEventKind.SlaveUprising, "SLAVE_UPRISING", _uprisers.Count, now,
            new Dictionary<string, string>());
    }

    private void UpdateDisease(double delta, double now, int secondsPerDay,
        CitizenSystem citizens, SettlementStatsRuntime stats)
    {
        _diseaseTimer -= delta * Math.Max(0, 1 - stats.Hunger);
        if (_diseaseTimer <= 0)
        {
            var epidemic = GodotSyxPort.Data.OriginalGameData.Current.Diseases.Values
                .Where(value => value.Epidemic).OrderBy(value => value.Key).ToArray();
            if (epidemic.Length > 0 && citizens.StartEpidemic(epidemic[_random.Next(epidemic.Length)].Key,
                    0.6 + 0.4 * _random.NextDouble()))
            {
                Notice(SettlementEventKind.Epidemic, "EPIDEMIC", citizens.SickCount, now,
                    now + secondsPerDay * 8, new Dictionary<string, string>());
                stats.Standing.Emergency(SocialClass.Citizen, secondsPerDay, secondsPerDay * 8);
            }
            _diseaseTimer = secondsPerDay * 8 * (1 + _random.NextDouble() * 2);
        }
        _mildDiseaseTimer -= delta;
        if (_mildDiseaseTimer <= 0)
        {
            _mildDiseaseTimer = secondsPerDay * 16 * 16;
            Notice(SettlementEventKind.MildDisease, "DISTANT_DISEASE", 0, now,
                new Dictionary<string, string>());
        }
    }

    private void UpdateKiller(double delta, double now, int secondsPerDay,
        CitizenSystem citizens, ResourceLedger resources, JobBoard jobs)
    {
        var population = citizens.EventPopulation().Where(person => person.Class == SocialClass.Citizen).ToArray();
        if (_killerId == 0)
        {
            _killerTimer -= delta;
            if (_killerTimer > 0 || population.Length < 2) return;
            _killerId = population[_random.Next(population.Length)].Id;
            _killerTimer = secondsPerDay * 16 * 24 * (1 + 0.5 * _random.NextDouble());
            _killerRate = 0;
        }
        _killerRate += delta / (secondsPerDay * 8);
        if (_killerRate < 1) return;
        _killerRate = 0;
        var victims = population.Where(person => person.Id != _killerId).ToArray();
        if (victims.Length == 0) { _killerId = 0; return; }
        var victim = victims[_random.Next(victims.Length)];
        citizens.KillForEvent(new[] { victim.Id }, resources, jobs, "MURDER");
        _killerVictims++;
        Notice(SettlementEventKind.SerialKiller, "SERIAL_MURDER", 1, now,
            new Dictionary<string, string> { ["victimRace"] = victim.Race, ["murders"] = _killerVictims.ToString() });
    }

    private double _temperatureDay = -1;
    private void UpdateTemperature(double now, int secondsPerDay, double deviation)
    {
        var day = Math.Floor(now / secondsPerDay);
        if (day <= 6 || day == _temperatureDay || Math.Abs(deviation) < 0.2) return;
        _temperatureDay = day;
        Notice(SettlementEventKind.Temperature, deviation < 0 ? "EXTREME_COLD" : "EXTREME_HEAT",
            0, now, new Dictionary<string, string>());
    }

    private IReadOnlyList<EventActionResult> Execute(IEnumerable<EventAction> actions,
        CitizenSystem citizens, ResourceLedger resources, JobBoard jobs, GridCoord exit)
    {
        var results = new List<EventActionResult>();
        foreach (var action in actions)
        {
            var amount = (int)Math.Round(action.Amount);
            if (action.Kind == EventActionKind.Resource && Enum.TryParse<ResourceKind>(action.Key, true, out var resource))
            {
                if (amount >= 0) resources.Add(resource, amount);
                else resources.TryTake(resource, -amount);
                results.Add(new EventActionResult(action, Math.Abs(amount), false));
            }
            else if (action.Kind == EventActionKind.SubjectsKill)
            {
                var ids = Sample(citizens.EventPopulation().Select(person => person.Id), Math.Max(0, amount));
                results.Add(new EventActionResult(action, citizens.KillForEvent(ids, resources, jobs), false));
            }
            else if (action.Kind == EventActionKind.Outbreak)
                results.Add(new EventActionResult(action, citizens.StartEpidemic(action.Key, action.Amount) ? citizens.SickCount : 0, false));
            else
                results.Add(new EventActionResult(action, 0, true,
                    "Requires world, boost registry, settlement creation, or presentation adapter"));
        }
        return results;
    }

    private int[] Sample(IEnumerable<int> source, int amount) => source.OrderBy(_ => _random.Next())
        .Take(Math.Max(0, amount)).ToArray();
    private static IReadOnlyDictionary<string, string> Race(string race) =>
        new Dictionary<string, string> { ["race"] = race };
    private void Notice(SettlementEventKind kind, string key, int amount, double now,
        IReadOnlyDictionary<string, string> context) => Notice(kind, key, amount, now, now, context);
    private void Notice(SettlementEventKind kind, string key, int amount, double now, double endsAt,
        IReadOnlyDictionary<string, string> context) =>
        _notices.Add(new EventNotice(++_sequence, kind, key, amount, now, endsAt, context));
}
