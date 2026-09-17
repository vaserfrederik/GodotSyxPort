using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class ActivityServiceInstanceRuntime
{
    public int RoomId { get; init; }
    public string RoomKey { get; set; } = "";
    public GridCoord Cell { get; set; }
    public IReadOnlyList<GridCoord> WorkCells { get; set; } = Array.Empty<GridCoord>();
    public int ActiveWorkers { get; set; }
    public int PunishmentsReserved { get; set; }
    public int PunishmentMaximum => RoomKey.StartsWith("ARENAG_", StringComparison.OrdinalIgnoreCase) ? 4 :
        RoomKey.StartsWith("FIGHTPIT_", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
}

/// <summary>
/// Common worker/opening runtime for spectator rooms. Animation-only cheer/boo state stays
/// deferred, while availability, shifts, employment and service quality are simulated here.
/// </summary>
public sealed class ActivityServiceRuntime
{
    private readonly Dictionary<int, ActivityServiceInstanceRuntime> _instances = new();
    private readonly Dictionary<int, int> _punishmentBySubject = new();

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     IsActivityKey(room.DefinitionKey)))
        {
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var activity))
            {
                activity = new ActivityServiceInstanceRuntime { RoomId = room.Id };
                _instances[room.Id] = activity;
            }
            activity.RoomKey = room.DefinitionKey;
            activity.Cell = fromIndex(room.Cells.First());
            activity.WorkCells = room.Cells.Select(fromIndex).ToArray();
        }
        foreach (var id in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
        {
            _instances.Remove(id);
            foreach (var subject in _punishmentBySubject.Where(pair => pair.Value == id)
                         .Select(pair => pair.Key).ToArray())
                _punishmentBySubject.Remove(subject);
        }
    }

    public void Reconcile(IEnumerable<BuildJob> jobs, IEnumerable<RoomRecord> rooms)
    {
        var active = jobs.Where(job => job.Kind == BuildKind.ActivityWork &&
            job.State is not (JobState.Completed or JobState.Cancelled))
            .GroupBy(job => job.RoomId).ToDictionary(group => group.Key, group => group.Count());
        foreach (var room in rooms.Where(room => _instances.ContainsKey(room.Id)))
        {
            var employed = Math.Min(room.Employment.Needed, active.GetValueOrDefault(room.Id));
            room.Employment.SetEmployed(employed);
            _instances[room.Id].ActiveWorkers = employed;
        }
    }

    public void Schedule(JobBoard jobs, IEnumerable<RoomRecord> rooms)
    {
        foreach (var room in rooms.Where(room => _instances.ContainsKey(room.Id)))
        {
            var activity = _instances[room.Id];
            var active = jobs.All.Count(job => job.Kind == BuildKind.ActivityWork &&
                job.RoomId == room.Id && job.State is not (JobState.Completed or JobState.Cancelled));
            for (var worker = active; worker < room.Employment.Needed; worker++)
                if (!jobs.Add(BuildJob.ActivityWork(
                        activity.WorkCells[worker % activity.WorkCells.Count], room.Id))) break;
        }
    }

    public bool IsOpen(int roomId, double dayFraction)
    {
        if (!_instances.TryGetValue(roomId, out var activity)) return true;
        if (activity.ActiveWorkers <= 0 && !activity.RoomKey.StartsWith(
                "ARENAG_", StringComparison.OrdinalIgnoreCase)) return false;
        if (!IsArena(activity.RoomKey)) return true;
        var hour = (int)Math.Floor((dayFraction - Math.Floor(dayFraction)) * 24.0);
        return hour > 11 || hour < 6;
    }

    public double Quality(int roomId, int maximum) =>
        _instances.TryGetValue(roomId, out var activity)
            ? Math.Clamp(activity.ActiveWorkers / (double)Math.Max(1, maximum), 0, 1)
            : 0;

    public int PunishmentsReserved => _punishmentBySubject.Count;
    public int PunishmentCapacity => _instances.Values.Sum(value =>
        value.ActiveWorkers > 0 ? value.PunishmentMaximum : 0);

    public int? TryReservePunishment(int subjectId, int? preferredRoomId = null)
    {
        if (_punishmentBySubject.TryGetValue(subjectId, out var existing)) return existing;
        var candidates = _instances.Values.Where(value => value.PunishmentMaximum > 0 &&
            value.ActiveWorkers > 0 && value.PunishmentsReserved < value.PunishmentMaximum);
        var selected = candidates
            .OrderByDescending(value => preferredRoomId is int preferred && value.RoomId == preferred)
            .ThenBy(value => value.RoomId).FirstOrDefault();
        if (selected is null) return null;
        selected.PunishmentsReserved++;
        _punishmentBySubject[subjectId] = selected.RoomId;
        return selected.RoomId;
    }

    public bool CancelPunishment(int subjectId)
    {
        if (!_punishmentBySubject.Remove(subjectId, out var roomId)) return false;
        if (_instances.TryGetValue(roomId, out var instance))
            instance.PunishmentsReserved = Math.Max(0, instance.PunishmentsReserved - 1);
        return true;
    }

    public bool CompletePunishment(int subjectId, AsylumRuntime asylums)
    {
        if (!CancelPunishment(subjectId)) return false;
        return asylums.Release(subjectId);
    }

    private static bool IsActivityKey(string key) => IsArena(key) ||
        key.StartsWith("SPEAKER_", StringComparison.OrdinalIgnoreCase) ||
        key.StartsWith("STAGE_", StringComparison.OrdinalIgnoreCase);

    private static bool IsArena(string key) =>
        key.StartsWith("ARENAG_", StringComparison.OrdinalIgnoreCase) ||
        key.StartsWith("FIGHTPIT_", StringComparison.OrdinalIgnoreCase);

}
