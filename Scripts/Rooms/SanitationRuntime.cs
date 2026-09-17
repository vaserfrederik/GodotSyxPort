using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class SanitationSlotRuntime
{
    public int Id { get; init; }
    public int RoomId { get; init; }
    public GridCoord Cell { get; init; }
    public int Usage { get; set; }
    public bool CleaningReserved { get; set; }
    public bool Suspended { get; set; }
}

/// <summary>
/// Lavatory service lifecycle: four uses per seat, cleaning becomes available
/// above two uses, and a cleaning job takes the source 45 seconds.
/// </summary>
public sealed class SanitationRuntime
{
    public const int MaximumUses = 4;
    public const int CleaningThreshold = 3;
    public const float CleaningSeconds = 45f;

    private readonly Dictionary<int, SanitationSlotRuntime> _slots = new();
    private readonly Dictionary<(int RoomId, int Cell), int> _slotKeys = new();
    private readonly Dictionary<int, RoomServiceInstanceRuntime> _services = new();
    private int _nextId = 1;
    public IReadOnlyCollection<SanitationSlotRuntime> Slots => _slots.Values;

    public void Synchronize(
        IEnumerable<RoomRecord> rooms,
        Func<int, GridCoord> fromIndex,
        RoomServiceRuntime services)
    {
        var live = new HashSet<(int RoomId, int Cell)>();
        _services.Clear();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     room.DefinitionKey.Equals("LAVATORY_NORMAL", StringComparison.OrdinalIgnoreCase)))
        {
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == room.Id);
            if (service is null) continue;
            _services[room.Id] = service;
            foreach (var cellIndex in room.Cells.Take(service.Total))
            {
                var key = (room.Id, cellIndex);
                live.Add(key);
                if (_slotKeys.ContainsKey(key)) continue;
                var slot = new SanitationSlotRuntime
                {
                    Id = _nextId++, RoomId = room.Id, Cell = fromIndex(cellIndex)
                };
                _slots.Add(slot.Id, slot);
                _slotKeys.Add(key, slot.Id);
            }
        }
        foreach (var key in _slotKeys.Keys.Where(key => !live.Contains(key)).ToArray())
        {
            _slots.Remove(_slotKeys[key]);
            _slotKeys.Remove(key);
        }
    }

    public void RecordUse(int roomId)
    {
        var slot = _slots.Values.Where(slot => slot.RoomId == roomId &&
                !slot.CleaningReserved && slot.Usage < MaximumUses)
            .OrderBy(slot => slot.Usage).FirstOrDefault();
        if (slot is null) return;
        slot.Usage++;
        if (slot.Usage < MaximumUses || slot.Suspended) return;
        if (_services.GetValueOrDefault(roomId)?.SuspendAvailable() == true)
            slot.Suspended = true;
    }

    public void Schedule(JobBoard jobs)
    {
        foreach (var slot in _slots.Values.Where(slot =>
                     slot.Usage >= CleaningThreshold && !slot.CleaningReserved).ToArray())
        {
            if (!slot.Suspended && _services.GetValueOrDefault(slot.RoomId)?.SuspendAvailable() == true)
                slot.Suspended = true;
            if (!jobs.Add(BuildJob.Sanitation(slot.Id, slot.RoomId, slot.Cell)))
            {
                if (slot.Suspended && slot.Usage < MaximumUses)
                {
                    _services.GetValueOrDefault(slot.RoomId)?.RestoreAvailable();
                    slot.Suspended = false;
                }
                continue;
            }
            slot.CleaningReserved = true;
        }
    }

    public void Complete(int slotId)
    {
        if (!_slots.TryGetValue(slotId, out var slot)) return;
        slot.Usage = 0;
        slot.CleaningReserved = false;
        if (!slot.Suspended) return;
        _services.GetValueOrDefault(slot.RoomId)?.RestoreAvailable();
        slot.Suspended = false;
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        var active = jobs.Where(job => job.Kind == BuildKind.Sanitation &&
                              job.State is not (JobState.Completed or JobState.Cancelled))
            .Select(job => job.FacilitySlotId).ToHashSet();
        foreach (var slot in _slots.Values.Where(slot => slot.CleaningReserved &&
                     !active.Contains(slot.Id)))
        {
            slot.CleaningReserved = false;
            if (!slot.Suspended || slot.Usage >= MaximumUses) continue;
            _services.GetValueOrDefault(slot.RoomId)?.RestoreAvailable();
            slot.Suspended = false;
        }
    }
}
