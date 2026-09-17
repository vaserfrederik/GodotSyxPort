using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;

namespace GodotSyxPort.Rooms;

/// <summary>One reservable service capacity pool for a room instance.</summary>
public sealed class RoomServiceInstanceRuntime
{
    public int RoomId { get; }
    public RoomServiceRule Rule { get; }
    public int Total { get; private set; }
    public int Available { get; private set; }
    public int Reserved { get; private set; }
    public int Unavailable { get; private set; }
    public byte CurrentHigh { get; private set; }
    public byte LastHigh { get; private set; }
    public double Load => LastHigh / 127.0;

    public RoomServiceInstanceRuntime(int roomId, RoomServiceRule rule, int total)
    {
        RoomId = roomId;
        Rule = rule;
        Total = Available = Math.Max(0, total);
    }

    public bool Reserve()
    {
        if (Available <= 0) return false;
        Available--;
        Reserved++;
        RecordLoad();
        return true;
    }

    public bool Cancel()
    {
        if (Reserved <= 0) return false;
        Reserved--;
        Available = Math.Min(Total - Reserved - Unavailable, Available + 1);
        return true;
    }

    public bool Consume()
    {
        if (Reserved <= 0) return false;
        Reserved--;
        Available = Math.Min(Total - Reserved - Unavailable, Available + 1);
        RecordLoad();
        return true;
    }

    public bool SuspendAvailable()
    {
        if (Available <= 0) return false;
        Available--;
        Unavailable++;
        RecordLoad();
        return true;
    }

    public bool RestoreAvailable()
    {
        if (Unavailable <= 0) return false;
        Unavailable--;
        Available = Math.Min(Total - Reserved - Unavailable, Available + 1);
        return true;
    }

    public void Reconfigure(int total)
    {
        Total = Math.Max(0, total);
        Reserved = Math.Min(Reserved, Total);
        Unavailable = Math.Min(Unavailable, Math.Max(0, Total - Reserved));
        Available = Math.Max(0, Total - Reserved - Unavailable);
        RecordLoad();
    }

    public void UpdateDay()
    {
        LastHigh = CurrentHigh;
        CurrentHigh = 0;
    }

    private void RecordLoad()
    {
        if (Total <= 0) return;
        var high = (byte)Math.Clamp((int)(127.0 * (Total - Available) / Total), 0, 127);
        if (high > CurrentHigh) CurrentHigh = high;
        if (high > LastHigh) LastHigh = high;
    }
}

/// <summary>Shared RoomService/RoomServiceInstance registry for every SERVICE definition.</summary>
public sealed class RoomServiceRuntime
{
    private readonly Dictionary<int, RoomServiceInstanceRuntime> _instances = new();
    private double _dayLeft;
    public IReadOnlyCollection<RoomServiceInstanceRuntime> Instances => _instances.Values;
    public IEnumerable<RoomServiceInstanceRuntime> AvailableInstances =>
        _instances.Values.Where(instance => instance.Available > 0);
    public bool IsRegistered(RoomServiceInstanceRuntime instance) =>
        _instances.GetValueOrDefault(instance.RoomId) == instance;

    public void Synchronize(
        IEnumerable<RoomRecord> rooms, RoomBlueprintCatalog blueprints,
        Func<RoomRecord, int, double> stat)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            var blueprint = blueprints.Get(room.DefinitionKey);
            var rule = blueprint?.Rule.Service;
            if (rule is null) continue;
            live.Add(room.Id);
            var capacity = ServiceCapacity(room, stat, room.Employment.Maximum, room.Cells.Count);
            if (_instances.TryGetValue(room.Id, out var instance)) instance.Reconfigure(capacity);
            else _instances.Add(room.Id, new RoomServiceInstanceRuntime(room.Id, rule, capacity));
        }
        foreach (var roomId in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(roomId);
    }

    public RoomServiceInstanceRuntime? Find(string need, int distance = 0) =>
        _instances.Values.FirstOrDefault(instance =>
            instance.Available > 0 && distance <= instance.Rule.Radius &&
            instance.Rule.Need.Equals(need, StringComparison.OrdinalIgnoreCase));

    public int Available(string need) => _instances.Values
        .Where(instance => instance.Rule.Need.Equals(need, StringComparison.OrdinalIgnoreCase))
        .Sum(instance => instance.Available);

    public int Total(string need) => _instances.Values
        .Where(instance => instance.Rule.Need.Equals(need, StringComparison.OrdinalIgnoreCase))
        .Sum(instance => instance.Total);

    public double Load(string need)
    {
        var matching = _instances.Values.Where(instance =>
            instance.Rule.Need.Equals(need, StringComparison.OrdinalIgnoreCase)).ToArray();
        return matching.Length == 0 ? 1.0 : matching.Max(instance => instance.Load);
    }

    public void UpdateDay()
    {
        foreach (var instance in _instances.Values) instance.UpdateDay();
    }

    public void Tick(double delta, double secondsPerDay)
    {
        if (_dayLeft <= 0) _dayLeft = Math.Max(1, secondsPerDay);
        _dayLeft -= Math.Max(0, delta);
        if (_dayLeft > 0) return;
        UpdateDay();
        _dayLeft += Math.Max(1, secondsPerDay);
    }

    private static int ServiceCapacity(
        RoomRecord room,
        Func<RoomRecord, int, double> stat,
        int furniture,
        int area)
    {
        var statCapacity = Math.Max(0, stat(room, 0));
        if (statCapacity > 0)
            return Math.Max(1, (int)Math.Ceiling(statCapacity));
        return Math.Max(1, furniture > 0 ? furniture : area);
    }
}
