using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.Rooms;

public enum HospitalityKind : byte { Inn, Resthome }

public sealed class HospitalityInstanceRuntime
{
    public int RoomId { get; init; }
    public HospitalityKind Kind { get; init; }
    public int Capacity { get; internal set; }
    public int Workers { get; internal set; }
    public double Quality { get; internal set; }
    public HashSet<int> Occupants { get; } = new();
    public int Free => Math.Max(0, Capacity - Occupants.Count);
}

/// <summary>
/// Non-visual core shared by inn beds and resthome stations. A bed/station is reserved
/// for one citizen, capacity is constrained by real furniture and staffing, and release
/// is explicit so future tourism and retirement systems can use the same boundary.
/// </summary>
public sealed class HospitalityRuntime
{
    public const double InnWorkersPerBed = 1.0 / 8.0;
    private readonly Dictionary<int, HospitalityInstanceRuntime> _instances = new();
    private readonly Dictionary<int, int> _occupantRoom = new();
    public IReadOnlyCollection<HospitalityInstanceRuntime> Instances => _instances.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<RoomRecord, int, double> stat)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms)
        {
            var kind = KindOf(room.DefinitionKey);
            if (kind is null) continue;
            active.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
                _instances[room.Id] = instance = new HospitalityInstanceRuntime
                {
                    RoomId = room.Id,
                    Kind = kind.Value
                };
            var furniture = Math.Max(0, (int)Math.Ceiling(stat(room, 0)));
            instance.Workers = room.State == RoomState.Operational ? room.Employment.Employed : 0;
            instance.Capacity = room.State == RoomState.Operational
                ? kind == HospitalityKind.Inn
                    ? Math.Min(furniture, instance.Workers <= 0 ? 0 : furniture)
                    : Math.Min(furniture, instance.Workers)
                : 0;
            instance.Quality = furniture == 0 ? 0 : Math.Clamp(
                (1 - room.Degradation) * room.Employment.TotalEfficiency *
                Math.Max(0.5, stat(room, 1)), 0, 1);
            Trim(instance);
        }
        foreach (var id in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
        {
            foreach (var citizen in _instances[id].Occupants) _occupantRoom.Remove(citizen);
            _instances.Remove(id);
        }
    }

    public bool TryReserve(int roomId, int citizenId)
    {
        if (!_instances.TryGetValue(roomId, out var instance) || instance.Free <= 0) return false;
        Release(citizenId);
        if (instance.Free <= 0) return false;
        instance.Occupants.Add(citizenId);
        _occupantRoom[citizenId] = roomId;
        return true;
    }

    public void Release(int citizenId)
    {
        if (!_occupantRoom.Remove(citizenId, out var roomId)) return;
        if (_instances.TryGetValue(roomId, out var instance)) instance.Occupants.Remove(citizenId);
    }

    public void RemoveRoom(int roomId)
    {
        if (!_instances.Remove(roomId, out var instance)) return;
        foreach (var citizenId in instance.Occupants) _occupantRoom.Remove(citizenId);
    }

    public HospitalityInstanceRuntime? RoomOf(int citizenId) =>
        _occupantRoom.TryGetValue(citizenId, out var id) ? _instances.GetValueOrDefault(id) : null;

    private void Trim(HospitalityInstanceRuntime instance)
    {
        foreach (var citizen in instance.Occupants.Skip(instance.Capacity).ToArray()) Release(citizen);
    }

    private static HospitalityKind? KindOf(string key) => key.ToUpperInvariant() switch
    {
        "_INN" => HospitalityKind.Inn,
        "RESTHOME_NORMAL" => HospitalityKind.Resthome,
        _ => null
    };
}
