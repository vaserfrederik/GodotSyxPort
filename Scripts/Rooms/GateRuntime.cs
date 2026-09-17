using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rooms;

public sealed class GateRuntimeInstance
{
    public int RoomId { get; init; }
    public IReadOnlySet<int> Cells { get; init; } = new HashSet<int>();
    public bool LockedForSubjects { get; internal set; }
    public bool Operational { get; internal set; }
}

/// <summary>Settlement-side state of ROOM_GATE. Enemy passage is a final battle-layer
/// concern; this runtime retains the independently useful subject lock and topology data.</summary>
public sealed class GateRuntime
{
    private readonly GridWorld _world;
    private readonly Dictionary<int, GateRuntimeInstance> _gates = new();
    public GateRuntime(GridWorld world) => _world = world;
    public IReadOnlyCollection<GateRuntimeInstance> Gates => _gates.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.DefinitionKey.Equals(
                     "GATEHOUSE_NORMAL", StringComparison.OrdinalIgnoreCase)))
        {
            active.Add(room.Id);
            if (!_gates.TryGetValue(room.Id, out var gate))
                _gates[room.Id] = gate = new GateRuntimeInstance
                {
                    RoomId = room.Id,
                    Cells = new HashSet<int>(room.Cells)
                };
            gate.Operational = room.State == RoomState.Operational;
        }
        foreach (var id in _gates.Keys.Where(id => !active.Contains(id)).ToArray())
            _gates.Remove(id);
    }

    public bool SetLocked(int roomId, bool locked)
    {
        if (!_gates.TryGetValue(roomId, out var gate) || !gate.Operational) return false;
        gate.LockedForSubjects = locked;
        return true;
    }

    public bool CanSubjectPass(GridCoord cell) => !_gates.Values
        .Where(gate => gate.Operational && gate.Cells.Contains(_world.CellToIndex(cell)))
        .Any(gate => gate.LockedForSubjects);
}
