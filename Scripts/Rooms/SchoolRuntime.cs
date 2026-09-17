using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

public sealed class SchoolRuntimeInstance
{
    public int RoomId { get; init; }
    public int Seats { get; set; }
    public int AvailableLessons { get; set; }
    public double Quality { get; set; } = 1.0;
    public double PaperProgress { get; set; }
}

/// <summary>Shared ROOM_SCHOOL/SchoolStation/RoomEducationHelper numerical runtime.</summary>
public sealed class SchoolRuntime
{
    public const int EducationMaximum = 100;
    public const int DefaultChildLimit = EducationMaximum / 6;
    public const int StationPreparationSteps = 3;
    public const int WorkCyclesPerDay = 40;
    public const double PaperPerLesson = 0.1;
    public const int MissingSchoolDaysBeforeGrowth = 3;
    private readonly Dictionary<int, SchoolRuntimeInstance> _schools = new();
    private readonly Dictionary<(string Race, SocialClass Class), EducationTrack> _policy = new();
    private readonly Dictionary<(string Race, SocialClass Class), int> _childLimits = new();

    public IReadOnlyCollection<SchoolRuntimeInstance> Schools => _schools.Values;
    public int TotalSeats => _schools.Values.Sum(school => school.Seats);
    public bool HasService => _schools.Values.Any(school => school.Seats > 0);

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<RoomRecord, int, double> stat)
    {
        var active = rooms.Where(room => room.State == RoomState.Operational &&
            room.DefinitionKey.Equals("SCHOOL_NORMAL", StringComparison.OrdinalIgnoreCase)).ToArray();
        var ids = active.Select(room => room.Id).ToHashSet();
        foreach (var id in _schools.Keys.Where(id => !ids.Contains(id)).ToArray()) _schools.Remove(id);
        foreach (var room in active)
        {
            if (!_schools.TryGetValue(room.Id, out var school))
                _schools.Add(room.Id, school = new SchoolRuntimeInstance { RoomId = room.Id });
            school.Seats = Math.Max(1, (int)Math.Ceiling(stat(room, 0)));
            school.AvailableLessons = Math.Min(school.AvailableLessons, school.Seats);
            school.Quality = Math.Clamp((1.0 - room.Degradation) *
                room.Employment.TotalEfficiency * stat(room, 1), 0, 1);
        }
    }

    public void BeginDay()
    {
        foreach (var school in _schools.Values) school.AvailableLessons = school.Seats;
    }

    public void SetPolicy(string race, SocialClass socialClass, EducationTrack track) =>
        _policy[(race, socialClass)] = track;

    public EducationTrack Policy(string race, SocialClass socialClass) =>
        _policy.GetValueOrDefault((race, socialClass), EducationTrack.Education);

    public void SetChildLimit(string race, SocialClass socialClass, int value) =>
        _childLimits[(race, socialClass)] = Math.Clamp(value, 0, EducationMaximum);

    public int ChildLimit(string race, SocialClass socialClass) =>
        _childLimits.GetValueOrDefault((race, socialClass), DefaultChildLimit);

    public bool CanEducate(CitizenPersonalStatsRuntime stats, int citizenId,
        string race, SocialClass socialClass) =>
        stats.EducationValue(citizenId, Policy(race, socialClass), EducationAge.Childhood) <
        ChildLimit(race, socialClass);

    public bool TryAttend(CitizenPersonalStatsRuntime stats, int citizenId,
        string race, SocialClass socialClass, ResourceLedger resources)
    {
        var school = _schools.Values.FirstOrDefault(candidate => candidate.AvailableLessons > 0);
        if (school is null || !CanEducate(stats, citizenId, race, socialClass)) return false;
        school.PaperProgress += PaperPerLesson;
        if (school.PaperProgress >= 1.0)
        {
            if (!resources.TryTake(ResourceKind.Paper, 1)) return false;
            school.PaperProgress -= 1.0;
        }
        school.AvailableLessons--;
        stats.Educate(citizenId, Policy(race, socialClass), EducationAge.Childhood,
            Math.Max(0.01, school.Quality));
        return true;
    }
}
