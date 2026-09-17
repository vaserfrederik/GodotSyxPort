using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class PreparedServiceInstanceRuntime
{
    public int RoomId { get; init; }
    public string RoomKey { get; set; } = "";
    public IReadOnlyList<GridCoord> WorkCells { get; set; } = Array.Empty<GridCoord>();
    public int Prepared { get; set; }
    public int WorkProgress { get; set; }
}

/// <summary>
/// Shared prepare-then-consume stations used by barbers and physicians. The source stores
/// counters in tile bits; the dense runtime stores the same bounded counters per room.
/// </summary>
public sealed class PreparedServiceRuntime
{
    private readonly Dictionary<int, PreparedServiceInstanceRuntime> _instances = new();

    public void Synchronize(
        IEnumerable<RoomRecord> rooms,
        Func<int, GridCoord> fromIndex,
        RoomServiceRuntime services)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     IsPreparedService(room.DefinitionKey)))
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == room.Id);
            if (service is null) continue;
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var prepared))
            {
                prepared = new PreparedServiceInstanceRuntime { RoomId = room.Id };
                _instances[room.Id] = prepared;
                for (var capacity = 0; capacity < service.Total; capacity++)
                    service.SuspendAvailable();
            }
            prepared.RoomKey = room.DefinitionKey;
            prepared.WorkCells = room.Cells.Select(fromIndex).ToArray();
        }
        foreach (var id in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(id);
    }

    public void Reconcile(IEnumerable<BuildJob> jobs, IEnumerable<RoomRecord> rooms)
    {
        var active = jobs.Where(job => job.Kind == BuildKind.ServicePreparation &&
            job.State is not (JobState.Completed or JobState.Cancelled))
            .GroupBy(job => job.RoomId).ToDictionary(group => group.Key, group => group.Count());
        foreach (var room in rooms.Where(room => _instances.ContainsKey(room.Id)))
            room.Employment.SetEmployed(Math.Min(
                room.Employment.Needed, active.GetValueOrDefault(room.Id)));
    }

    public void Schedule(JobBoard jobs, IEnumerable<RoomRecord> rooms)
    {
        foreach (var room in rooms.Where(room => _instances.ContainsKey(room.Id)))
        {
            var prepared = _instances[room.Id];
            var maximum = UsesPerStation(prepared.RoomKey) * Math.Max(1, room.Employment.Maximum);
            if (prepared.Prepared >= maximum) continue;
            var active = jobs.All.Count(job => job.Kind == BuildKind.ServicePreparation &&
                job.RoomId == room.Id && job.State is not (JobState.Completed or JobState.Cancelled));
            for (var worker = active; worker < room.Employment.Needed; worker++)
                if (!jobs.Add(BuildJob.ServicePreparation(
                        prepared.WorkCells[worker % prepared.WorkCells.Count], room.Id,
                        WorkSeconds(prepared.RoomKey)))) break;
        }
    }

    public void Complete(BuildJob job, RoomServiceRuntime services)
    {
        if (!_instances.TryGetValue(job.RoomId, out var prepared)) return;
        prepared.WorkProgress++;
        if (prepared.WorkProgress < CyclesPerUse(prepared.RoomKey)) return;
        prepared.WorkProgress = 0;
        prepared.Prepared++;
        services.Instances.FirstOrDefault(service => service.RoomId == job.RoomId)?.RestoreAvailable();
    }

    public void RecordUse(RoomServiceInstanceRuntime service)
    {
        if (!_instances.TryGetValue(service.RoomId, out var prepared) || prepared.Prepared <= 0) return;
        prepared.Prepared--;
        if (prepared.Prepared < service.Total) service.SuspendAvailable();
    }

    private static bool IsPreparedService(string key) =>
        key.Equals("BARBER_NORMAL", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("PHYSICIAN_NORMAL", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("PLEASURE_NORMAL", StringComparison.OrdinalIgnoreCase);

    private static int UsesPerStation(string key) => key.Equals(
        "PHYSICIAN_NORMAL", StringComparison.OrdinalIgnoreCase) ? 7 :
        key.Equals("PLEASURE_NORMAL", StringComparison.OrdinalIgnoreCase) ? 1 : 15;

    private static int CyclesPerUse(string key) => key.Equals(
        "BARBER_NORMAL", StringComparison.OrdinalIgnoreCase) ? 16 : 1;

    private static float WorkSeconds(string key) => key.Equals(
        "BARBER_NORMAL", StringComparison.OrdinalIgnoreCase) ? 7.2f :
        key.Equals("PLEASURE_NORMAL", StringComparison.OrdinalIgnoreCase) ? 57.6f : 20f;
}
