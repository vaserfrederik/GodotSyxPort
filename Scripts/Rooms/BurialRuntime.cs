using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;

namespace GodotSyxPort.Rooms;

public enum BurialFacilityKind : byte
{
    Graveyard,
    Tomb,
    CorpseDump
}

public sealed class BurialSlotRuntime
{
    public int Id { get; init; }
    public int RoomId { get; init; }
    public GridCoord Cell { get; init; }
    public BurialFacilityKind Kind { get; init; }
    public bool Reserved { get; set; }
    public bool MourningReserved { get; set; }
    public int OccupiedDays { get; set; }
    public string Cause { get; set; } = "";
    public bool Available => !Reserved && OccupiedDays <= 0;
    public bool Formal => Kind is BurialFacilityKind.Graveyard or BurialFacilityKind.Tomb;
    public int ComposeDays => Kind switch
    {
        BurialFacilityKind.Graveyard => 20,
        BurialFacilityKind.Tomb => 40,
        _ => 16
    };
}

public readonly record struct BurialDestination(
    int SlotId,
    GridCoord Cell,
    bool Formal,
    float WorkSeconds);

public readonly record struct MourningDestination(int SlotId, GridCoord Cell);

/// <summary>
/// Shared Grave/GraveData/Dump runtime. Formal graves are always preferred;
/// the corpse dump is the source fallback when no permitted grave is free.
/// Race/class permissions stay neutral until population identities are ported.
/// </summary>
public sealed class BurialRuntime
{
    private readonly Dictionary<int, BurialSlotRuntime> _slots = new();
    private readonly Dictionary<(int RoomId, int Cell), int> _slotKeys = new();
    private int _nextId = 1;
    private double _dayProgress;

    public IReadOnlyCollection<BurialSlotRuntime> Slots => _slots.Values;
    public int BuriedCount { get; private set; }
    public int DumpedCount { get; private set; }
    public double Disturbance { get; private set; }
    public int AvailableFormal => _slots.Values.Count(slot => slot.Formal && slot.Available);

    public void Synchronize(
        IEnumerable<RoomRecord> rooms,
        Func<int, GridCoord> fromIndex,
        Func<RoomRecord, int> furnitureCount,
        RoomServiceRuntime services)
    {
        var live = new HashSet<(int RoomId, int Cell)>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            if (!TryKind(room.DefinitionKey, out var kind)) continue;
            var service = services.Instances.FirstOrDefault(candidate => candidate.RoomId == room.Id);
            var capacity = kind == BurialFacilityKind.CorpseDump
                ? service?.Total ?? room.Cells.Count
                : Math.Max(1, furnitureCount(room));
            var cells = room.Cells.Take(Math.Max(0, capacity)).ToArray();
            foreach (var cellIndex in cells)
            {
                var key = (room.Id, cellIndex);
                live.Add(key);
                if (_slotKeys.ContainsKey(key)) continue;
                var slot = new BurialSlotRuntime
                {
                    Id = _nextId++, RoomId = room.Id,
                    Cell = fromIndex(cellIndex), Kind = kind
                };
                _slots.Add(slot.Id, slot);
                _slotKeys.Add(key, slot.Id);
            }
        }

        foreach (var key in _slotKeys.Keys.Where(key => !live.Contains(key)).ToArray())
        {
            var slot = _slots[_slotKeys[key]];
            if (slot.Formal && slot.OccupiedDays > 0) Disturbance += 1;
            _slots.Remove(slot.Id);
            _slotKeys.Remove(key);
        }
    }

    public BurialDestination? Reserve(GridCoord origin)
    {
        var slot = _slots.Values.Where(slot => slot.Available && slot.Formal)
            .OrderBy(slot => Distance(origin, slot.Cell)).FirstOrDefault()
            ?? _slots.Values.Where(slot => slot.Available && !slot.Formal)
                .OrderBy(slot => Distance(origin, slot.Cell)).FirstOrDefault();
        if (slot is null) return null;
        slot.Reserved = true;
        return new BurialDestination(slot.Id, slot.Cell, slot.Formal, slot.Formal ? 25f : 0.1f);
    }

    public void Cancel(int slotId)
    {
        if (_slots.TryGetValue(slotId, out var slot) && slot.OccupiedDays <= 0)
            slot.Reserved = false;
    }

    public MourningDestination? ReserveMourning(GridCoord origin, int radius = 500)
    {
        var slot = _slots.Values.Where(slot => slot.Formal && slot.OccupiedDays > 0 &&
                                               !slot.MourningReserved && Distance(origin, slot.Cell) <= radius)
            .OrderBy(slot => Distance(origin, slot.Cell)).FirstOrDefault();
        if (slot is null) return null;
        slot.MourningReserved = true;
        return new MourningDestination(slot.Id, slot.Cell);
    }

    public void CancelMourning(int slotId)
    {
        if (_slots.TryGetValue(slotId, out var slot)) slot.MourningReserved = false;
    }

    public bool CompleteMourning(int slotId)
    {
        if (!_slots.TryGetValue(slotId, out var slot) || !slot.MourningReserved ||
            !slot.Formal || slot.OccupiedDays <= 0) return false;
        slot.MourningReserved = false;
        return true;
    }

    public bool Complete(int slotId, string cause)
    {
        if (!_slots.TryGetValue(slotId, out var slot) || !slot.Reserved) return false;
        slot.Reserved = false;
        slot.OccupiedDays = slot.ComposeDays;
        slot.Cause = cause;
        if (slot.Formal) BuriedCount++;
        else DumpedCount++;
        return true;
    }

    public void Tick(double delta, double secondsPerDay)
    {
        _dayProgress += Math.Max(0, delta) / Math.Max(1, secondsPerDay);
        while (_dayProgress >= 1)
        {
            _dayProgress -= 1;
            foreach (var slot in _slots.Values.Where(slot => slot.OccupiedDays > 0))
            {
                slot.OccupiedDays--;
                if (slot.OccupiedDays == 0)
                {
                    slot.Cause = "";
                    slot.MourningReserved = false;
                }
            }
        }

        // GraveData: remove max(dist/(day*16), 1/(day*16)) each second.
        if (Disturbance <= 0) return;
        var rate = 1.0 / (Math.Max(1, secondsPerDay) * 16.0);
        Disturbance = Math.Max(0, Disturbance - Math.Max(Disturbance * rate, rate) * delta);
    }

    private static int Distance(GridCoord a, GridCoord b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Z - b.Z);

    private static bool TryKind(string key, out BurialFacilityKind kind)
    {
        if (key.Equals("GRAVEYARD_NORMAL", StringComparison.OrdinalIgnoreCase))
        {
            kind = BurialFacilityKind.Graveyard;
            return true;
        }
        if (key.Equals("TOMB_NORMAL", StringComparison.OrdinalIgnoreCase))
        {
            kind = BurialFacilityKind.Tomb;
            return true;
        }
        if (key.Equals("_DUMP_CORPSE", StringComparison.OrdinalIgnoreCase))
        {
            kind = BurialFacilityKind.CorpseDump;
            return true;
        }
        kind = default;
        return false;
    }
}
