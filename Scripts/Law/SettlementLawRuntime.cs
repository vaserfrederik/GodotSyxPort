using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Law;

public enum LawFacilityKind : byte
{
    Court, Guard, Police, Prison, Stockade, Stocks, Execution, Cannibal
}

public enum LawCaseState : byte
{
    Suspected, Reported, Caught, AwaitingJudgement, Hearing, Sentenced, Punishing, Completed
}

public sealed class LawCaseRuntime
{
    public int CitizenId { get; init; }
    public string Race { get; init; } = "";
    public SocialClass Class { get; init; }
    public string Crime { get; set; } = "";
    public string Punishment { get; set; } = "NONE";
    public LawCaseState State { get; set; }
    public int FacilityRoomId { get; set; }
    public int DaysInCustody { get; set; }
    public int SentenceDaysLeft { get; set; }
}

public sealed class LawFacilityRuntime
{
    public int RoomId { get; init; }
    public LawFacilityKind Kind { get; init; }
    public GridCoord Anchor { get; set; }
    public int Capacity { get; set; }
    public int Used { get; set; }
    public int Workers { get; set; }
    public int MaximumWorkers { get; set; }
    public double Power { get; set; }
    public int Free => Math.Max(0, Capacity - Used);
}

public sealed record LawEffect(int CitizenId, string Crime, string Punishment, int FacilityRoomId);
public sealed record LawDecreeSnapshot(string Crime, string Race, SocialClass Class, string Punishment);
public sealed record LawCountSnapshot(string Key, string Race, SocialClass Class, double Value);
public sealed record LawCaseSnapshot(
    int CitizenId, string Race, SocialClass Class, string Crime, string Punishment,
    LawCaseState State, int FacilityRoomId, int DaysInCustody, int SentenceDaysLeft);
public sealed record SettlementLawSnapshot(
    int CurfewDays,
    IReadOnlyList<LawCaseSnapshot> Cases,
    IReadOnlyList<LawDecreeSnapshot> Decrees,
    IReadOnlyList<LawCountSnapshot> Committed,
    IReadOnlyList<LawCountSnapshot> Caught,
    IReadOnlyList<LawCountSnapshot> Recent);

/// <summary>
/// Shared crime statistics, guard reporting and punishment-room state. It replaces
/// the individual packed-bit room implementations while keeping their capacities
/// and source law/tyranny formulas.
/// </summary>
public sealed class SettlementLawRuntime
{
    public const int GuardRadius = 90;
    public const int PatrolMaximumTiles = 120;
    public const double CourtFreeRate = 0.2;
    public const double PrisonWorkersPerPrisoner = 0.25;
    public const double StockadePrisonersPerTile = 0.25;
    public const int ExecutionServicesPerStation = 8;
    // AIModule_Prisoner.PRISON_DAYS = 2 source years; the source calendar is 16 days/year.
    public const int PrisonSentenceDays = 32;

    private readonly Dictionary<int, LawCaseRuntime> _cases = new();
    private readonly Dictionary<int, LawFacilityRuntime> _facilities = new();
    private readonly Dictionary<(string Crime, string Race, SocialClass Class), string> _decrees = new();
    private readonly Dictionary<(string Crime, string Race, SocialClass Class), int> _committed = new();
    private readonly Dictionary<(string Crime, string Race, SocialClass Class), int> _caught = new();
    private readonly Dictionary<(string Punishment, string Race, SocialClass Class), double> _recent = new();
    private readonly Queue<LawEffect> _effects = new();
    private int _curfewDays;

    // Provided by RoomSystem after it has synchronized ROOM_CANNIBAL instances.
    // The reservation is made at sentencing, as getPrisonerCage() does in source.
    public Func<string, int?>? TryReserveHarvest { get; set; }

    public IReadOnlyCollection<LawCaseRuntime> Cases => _cases.Values;
    public IReadOnlyCollection<LawFacilityRuntime> Facilities => _facilities.Values;
    public bool Curfew => _curfewDays > 0;
    public int InCustody => _cases.Values.Count(value => value.State is
        LawCaseState.Caught or LawCaseState.AwaitingJudgement or
        LawCaseState.Hearing or LawCaseState.Sentenced or LawCaseState.Punishing);
    public int GuardWorkers => _facilities.Values.Where(value => value.Kind == LawFacilityKind.Guard)
        .Sum(value => value.Workers);

    public SettlementLawSnapshot Capture() => new(
        _curfewDays,
        _cases.Values.Select(value => new LawCaseSnapshot(
            value.CitizenId, value.Race, value.Class, value.Crime, value.Punishment,
            value.State, value.FacilityRoomId, value.DaysInCustody, value.SentenceDaysLeft)).ToArray(),
        _decrees.Select(value => new LawDecreeSnapshot(
            value.Key.Crime, value.Key.Race, value.Key.Class, value.Value)).ToArray(),
        _committed.Select(value => new LawCountSnapshot(
            value.Key.Crime, value.Key.Race, value.Key.Class, value.Value)).ToArray(),
        _caught.Select(value => new LawCountSnapshot(
            value.Key.Crime, value.Key.Race, value.Key.Class, value.Value)).ToArray(),
        _recent.Select(value => new LawCountSnapshot(
            value.Key.Punishment, value.Key.Race, value.Key.Class, value.Value)).ToArray());

    public void Restore(SettlementLawSnapshot snapshot)
    {
        _cases.Clear(); _decrees.Clear(); _committed.Clear(); _caught.Clear(); _recent.Clear();
        _effects.Clear();
        _curfewDays = Math.Max(0, snapshot.CurfewDays);
        foreach (var value in snapshot.Cases)
            _cases[value.CitizenId] = new LawCaseRuntime
            {
                CitizenId = value.CitizenId, Race = value.Race, Class = value.Class,
                Crime = value.Crime, Punishment = value.Punishment, State = value.State,
                FacilityRoomId = value.FacilityRoomId, DaysInCustody = value.DaysInCustody,
                SentenceDaysLeft = value.SentenceDaysLeft
            };
        foreach (var value in snapshot.Decrees)
            _decrees[(value.Crime, value.Race, value.Class)] = value.Punishment;
        foreach (var value in snapshot.Committed)
            _committed[(value.Key, value.Race, value.Class)] = (int)Math.Max(0, value.Value);
        foreach (var value in snapshot.Caught)
            _caught[(value.Key, value.Race, value.Class)] = (int)Math.Max(0, value.Value);
        foreach (var value in snapshot.Recent)
            _recent[(value.Key, value.Race, value.Class)] = Math.Max(0, value.Value);
        RecountFacilityUse();
    }

    public string CrimeOf(int citizenId) => _cases.GetValueOrDefault(citizenId)?.Crime ?? "";

    public void Synchronize(
        IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex,
        Func<RoomRecord, int, double> stat)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            if (!TryKind(room.DefinitionKey, out var kind)) continue;
            active.Add(room.Id);
            var stations = Math.Max(0, (int)Math.Ceiling(stat(room, 0)));
            var secondary = Math.Max(0, (int)Math.Ceiling(stat(room, 1)));
            var capacity = kind switch
            {
                LawFacilityKind.Court => Math.Max(1, stations),
                LawFacilityKind.Guard => Math.Max(1, stations),
                LawFacilityKind.Police => Math.Max(1, stations),
                LawFacilityKind.Prison => Math.Max(1, stations),
                LawFacilityKind.Stockade => Math.Max(1,
                    (int)Math.Ceiling(room.Cells.Count * StockadePrisonersPerTile)),
                LawFacilityKind.Stocks => Math.Max(1, stations),
                LawFacilityKind.Execution => Math.Max(1, stations * ExecutionServicesPerStation),
                _ => 0
            };
            if (!_facilities.TryGetValue(room.Id, out var facility))
                _facilities.Add(room.Id, facility = new LawFacilityRuntime { RoomId = room.Id, Kind = kind });
            facility.Anchor = room.Cells.Count == 0 ? default : fromIndex(room.Cells.First());
            facility.Capacity = capacity;
            facility.Workers = room.Employment.Employed;
            facility.MaximumWorkers = room.Employment.Maximum;
            facility.Power = kind == LawFacilityKind.Guard
                ? (1.0 - room.Degradation * 0.5) * (room.UpgradeLevel + 1.0) *
                  room.Employment.Employed / Math.Max(1.0, room.Employment.Maximum)
                : kind == LawFacilityKind.Police
                    ? Math.Sqrt(Math.Clamp(room.Employment.Employed / Math.Max(1.0, secondary), 0, 1))
                    : room.Employment.TotalEfficiency;
        }
        foreach (var id in _facilities.Keys.Where(id => !active.Contains(id)).ToArray())
            _facilities.Remove(id);
        RecountFacilityUse();
    }

    public void SetCurfewForDay() => _curfewDays = Math.Max(_curfewDays, 1);

    public void SetDecree(string crime, string race, SocialClass socialClass, string punishment)
    {
        var rule = OriginalGameData.Current.Punishment(punishment);
        if (rule is null || !rule.AvailableClasses.Contains(socialClass)) return;
        _decrees[(crime, race, socialClass)] = punishment;
    }

    public string Decree(string crime, string race, SocialClass socialClass) =>
        _decrees.GetValueOrDefault((crime, race, socialClass), "NONE");

    public bool TryCommitDailyCrime(
        int citizenId, string race, SocialClass socialClass, double lawfulness,
        int population, int guards, int day)
    {
        if (Curfew || _cases.TryGetValue(citizenId, out var existing) &&
            existing.State != LawCaseState.Completed) return false;
        var adjusted = Math.Max(0, lawfulness);
        if (socialClass == SocialClass.Citizen)
            adjusted *= (Math.Max(0, population) + Math.Max(0, guards) + 2.0) /
                        (Math.Max(0, population) + 1.0);
        var denominator = Math.Max(16, (int)(adjusted * 16 * 16 * 5 + 16));
        if (Hash(citizenId, day, 17) % (uint)denominator >= 4) return false;
        var crime = SelectCrime(socialClass, citizenId, day);
        if (crime is null) return false;
        ReportCrime(citizenId, race, socialClass, crime.Key, false);
        return true;
    }

    public void ReportCrime(int citizenId, string race, SocialClass socialClass,
        string crime, bool witnessed)
    {
        var rule = OriginalGameData.Current.Crime(crime);
        if (rule is null) return;
        _committed[(crime, race, socialClass)] =
            _committed.GetValueOrDefault((crime, race, socialClass)) + 1;
        _cases[citizenId] = new LawCaseRuntime
        {
            CitizenId = citizenId, Race = race, Class = socialClass, Crime = crime,
            State = witnessed ? LawCaseState.Reported : LawCaseState.Suspected
        };
    }

    public bool TryCatch(int citizenId, GridCoord location, bool policeDetected = false)
    {
        if (!_cases.TryGetValue(citizenId, out var lawCase) ||
            lawCase.State is LawCaseState.Completed or LawCaseState.Punishing) return false;
        var power = GuardPowerAt(location);
        if (lawCase.State == LawCaseState.Suspected && power <= 0 && !policeDetected) return false;
        lawCase.State = LawCaseState.Caught;
        _caught[(lawCase.Crime, lawCase.Race, lawCase.Class)] =
            _caught.GetValueOrDefault((lawCase.Crime, lawCase.Race, lawCase.Class)) + 1;
        return true;
    }

    public void TickDay()
    {
        if (_curfewDays > 0) _curfewDays--;
        foreach (var key in _recent.Keys.ToArray())
            _recent[key] = _recent[key] > 1 ? _recent[key] * 0.85 : _recent[key];
        foreach (var lawCase in _cases.Values.Where(value => value.State != LawCaseState.Completed))
        {
            lawCase.DaysInCustody++;
            if (lawCase.State == LawCaseState.Punishing && lawCase.Punishment == "PRISON")
            {
                if (--lawCase.SentenceDaysLeft <= 0) Complete(lawCase);
                continue;
            }
            if (lawCase.State == LawCaseState.Punishing && lawCase.Punishment == "HARVEST")
            {
                if (lawCase.SentenceDaysLeft > 0) lawCase.SentenceDaysLeft--;
                continue;
            }
            if (lawCase.State == LawCaseState.Caught)
            {
                var crime = OriginalGameData.Current.Crime(lawCase.Crime);
                lawCase.State = crime?.Judged == true
                    ? LawCaseState.AwaitingJudgement : LawCaseState.Sentenced;
            }
            if (lawCase.State == LawCaseState.AwaitingJudgement &&
                TryAssign(lawCase, LawFacilityKind.Court))
                lawCase.State = LawCaseState.Hearing;
            if (lawCase.State != LawCaseState.Sentenced) continue;
            lawCase.Punishment = _decrees.GetValueOrDefault(
                (lawCase.Crime, lawCase.Race, lawCase.Class), "NONE");
            if (lawCase.Punishment is "PARDON" or "BANISH" or "ENSLAVE")
            {
                Complete(lawCase);
                continue;
            }
            if (lawCase.Punishment == "HARVEST")
            {
                var roomId = TryReserveHarvest?.Invoke(lawCase.Race);
                if (roomId is null) continue;
                lawCase.FacilityRoomId = roomId.Value;
                lawCase.State = LawCaseState.Punishing;
                lawCase.SentenceDaysLeft = 2;
                continue;
            }
            var facility = lawCase.Punishment switch
            {
                "PRISON" => LawFacilityKind.Prison,
                "EXECUTE" => LawFacilityKind.Execution,
                _ => LawFacilityKind.Stocks
            };
            if (TryAssign(lawCase, facility))
            {
                lawCase.State = LawCaseState.Punishing;
                lawCase.SentenceDaysLeft = lawCase.Punishment == "PRISON"
                    ? PrisonSentenceDays : 0;
            }
        }
        RecountFacilityUse();
    }

    public void Schedule(JobBoard jobs)
    {
        foreach (var lawCase in _cases.Values.Where(value =>
                     value.State == LawCaseState.Hearing ||
                     value.State == LawCaseState.Punishing && value.Punishment != "PRISON" &&
                     (value.Punishment != "HARVEST" || value.SentenceDaysLeft <= 0)))
        {
            if (jobs.All.Any(job => job.Kind == BuildKind.LawProcess &&
                    job.FacilitySlotId == lawCase.CitizenId &&
                    job.State is not (JobState.Cancelled or JobState.Completed))) continue;
            if (!_facilities.TryGetValue(lawCase.FacilityRoomId, out var facility)) continue;
            var hearing = lawCase.State == LawCaseState.Hearing;
            var seconds = hearing ? 20f : lawCase.Punishment == "HARVEST" ? 45f :
                facility.Kind == LawFacilityKind.Stockade ? 60f : 20f;
            jobs.Add(BuildJob.LawProcess(facility.Anchor, facility.RoomId,
                lawCase.CitizenId, seconds, hearing
                    ? LawProcessKind.Hearing : LawProcessKind.Punishment));
        }
    }

    public bool CompleteProcess(int citizenId, LawProcessKind process)
    {
        if (!_cases.TryGetValue(citizenId, out var lawCase)) return false;
        if (process == LawProcessKind.Hearing)
        {
            if (lawCase.State != LawCaseState.Hearing) return false;
            lawCase.FacilityRoomId = 0;
            // CourtStation frees one fifth of heard prisoners in the source.
            if (Hash(lawCase.CitizenId, lawCase.DaysInCustody, 79) /
                    (double)uint.MaxValue < CourtFreeRate)
            {
                lawCase.Punishment = "PARDON";
                Complete(lawCase);
            }
            else lawCase.State = LawCaseState.Sentenced;
            RecountFacilityUse();
            return true;
        }
        if (lawCase.State != LawCaseState.Punishing || lawCase.Punishment == "PRISON")
            return false;
        Complete(lawCase);
        RecountFacilityUse();
        return true;
    }

    public bool TryTakeEffect(out LawEffect effect)
    {
        if (_effects.Count > 0) { effect = _effects.Dequeue(); return true; }
        effect = null!;
        return false;
    }

    public double GuardPowerAt(GridCoord location) => _facilities.Values
        .Where(value => value.Kind == LawFacilityKind.Guard &&
            Math.Abs(value.Anchor.X - location.X) + Math.Abs(value.Anchor.Z - location.Z) <= GuardRadius)
        .Sum(value => value.Power);

    public double PoliceCoverage(int population) => Math.Sqrt(Math.Clamp(
        _facilities.Values.Where(value => value.Kind == LawFacilityKind.Police)
            .Sum(value => value.Workers) / Math.Max(1.0, population), 0, 1));

    public double Tyranny(string race, SocialClass socialClass)
    {
        var crimes = OriginalGameData.Current.Crimes.Values.Where(value => value.Class == socialClass);
        return Math.Clamp(crimes.Sum(crime => crime.Freedom * PunishmentRate(
            crime.Key, race, socialClass)), 0, 1);
    }

    public double Law(string race, SocialClass socialClass)
    {
        var crimes = OriginalGameData.Current.Crimes.Values.Where(value => value.Class == socialClass);
        var value = crimes.Sum(crime => crime.Law * Math.Sqrt(Math.Max(0,
            PunishmentRate(crime.Key, race, socialClass))));
        return Math.Pow(Math.Clamp(value, 0, 1), 1.5);
    }

    private double PunishmentRate(string crime, string race, SocialClass socialClass)
    {
        var punishment = _decrees.GetValueOrDefault((crime, race, socialClass), "NONE");
        return OriginalGameData.Current.Punishment(punishment)?.Value ?? 0;
    }

    private void Complete(LawCaseRuntime lawCase)
    {
        var key = (lawCase.Punishment, lawCase.Race, lawCase.Class);
        _recent[key] = _recent.GetValueOrDefault(key) + 1;
        var available = OriginalGameData.Current.Punishments.Values.Count(value =>
            value.AvailableClasses.Contains(lawCase.Class));
        var decrement = available <= 0 ? 0 : 1.0 / available;
        foreach (var punishment in OriginalGameData.Current.Punishments.Values)
        {
            var other = (punishment.Key, lawCase.Race, lawCase.Class);
            _recent[other] = Math.Max(0, _recent.GetValueOrDefault(other) - decrement);
        }
        lawCase.State = LawCaseState.Completed;
        _effects.Enqueue(new LawEffect(
            lawCase.CitizenId, lawCase.Crime, lawCase.Punishment, lawCase.FacilityRoomId));
    }

    private bool TryAssign(LawCaseRuntime lawCase, LawFacilityKind kind)
    {
        var facility = _facilities.Values.FirstOrDefault(value => value.Kind == kind && value.Free > 0);
        if (facility is null && kind == LawFacilityKind.Prison)
            facility = _facilities.Values.FirstOrDefault(value =>
                value.Kind == LawFacilityKind.Stockade && value.Free > 0);
        if (facility is null) return false;
        lawCase.FacilityRoomId = facility.RoomId;
        facility.Used++;
        return true;
    }

    private void RecountFacilityUse()
    {
        foreach (var facility in _facilities.Values) facility.Used = 0;
        foreach (var lawCase in _cases.Values.Where(value => value.FacilityRoomId != 0 &&
                     value.State is LawCaseState.Hearing or LawCaseState.Punishing))
            if (_facilities.TryGetValue(lawCase.FacilityRoomId, out var facility)) facility.Used++;
    }

    private static CrimeRule? SelectCrime(SocialClass socialClass, int citizenId, int day)
    {
        var candidates = OriginalGameData.Current.Crimes.Values
            .Where(value => value.Class == socialClass && value.Criminal).ToArray();
        if (candidates.Length == 0) return null;
        var total = candidates.Sum(value => value.Freedom);
        var pick = Hash(citizenId, day, 31) / (double)uint.MaxValue * total;
        foreach (var crime in candidates)
        {
            pick -= crime.Freedom;
            if (pick <= 0) return crime;
        }
        return candidates[^1];
    }

    private static bool TryKind(string key, out LawFacilityKind kind)
    {
        kind = key.ToUpperInvariant() switch
        {
            "_COURT" => LawFacilityKind.Court,
            "_GUARD" => LawFacilityKind.Guard,
            "_POLICE" => LawFacilityKind.Police,
            "_PRISON" => LawFacilityKind.Prison,
            "_STOCKADE" => LawFacilityKind.Stockade,
            "_STOCKS" => LawFacilityKind.Stocks,
            "_EXECUTION" => LawFacilityKind.Execution,
            "_CANNIBAL" => LawFacilityKind.Cannibal,
            _ => (LawFacilityKind)byte.MaxValue
        };
        return (byte)kind != byte.MaxValue;
    }

    private static uint Hash(int citizenId, int day, int salt)
    {
        unchecked
        {
            uint value = (uint)(citizenId * 0x1f123bb5 ^ day * 0x5f356495 ^ salt);
            value ^= value >> 16; value *= 0x7feb352d; value ^= value >> 15;
            return value;
        }
    }
}
