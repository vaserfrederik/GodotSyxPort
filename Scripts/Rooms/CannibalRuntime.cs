using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

/// <summary>
/// Runtime equivalent of ROOM_CANNIBAL. A cage is represented by one employment
/// position, matching the source room's prisoner limit. Harvesting is requested
/// by the law subsystem and yields the RESOURCE map of the prisoner's race.
/// </summary>
public sealed class CannibalRuntime
{
    private readonly Dictionary<int, CannibalInstanceRuntime> _instances = new();
    private readonly Dictionary<string, bool> _racePermission =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _cannibalism =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<CannibalInstanceRuntime> Instances => _instances.Values;
    public int Prisoners => _instances.Values.Sum(value => value.Prisoners);
    public int Capacity => _instances.Values.Sum(value => value.Capacity);

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GodotSyxPort.Core.GridCoord> fromIndex)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     room.DefinitionKey.Equals("_CANNIBAL", StringComparison.OrdinalIgnoreCase)))
        {
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
                _instances.Add(room.Id, instance = new CannibalInstanceRuntime { RoomId = room.Id });
            // CannibalInstance sets employees.max to cage count. Normalized rooms retain
            // that number as the employment maximum after construction.
            instance.Capacity = Math.Max(1, room.Employment.Maximum);
            instance.Workers = room.Employment.Employed;
            instance.Anchor = fromIndex(room.Cells.First());
        }
        foreach (var id in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(id);
    }

    public void SetRacePermission(string race, bool enabled) => _racePermission[race] = enabled;

    public bool CanHarvest(string race) => _racePermission.GetValueOrDefault(race, true) &&
        _instances.Values.Any(value => value.Prisoners < value.Capacity);

    public bool Reserve(string race, out int roomId)
    {
        roomId = 0;
        if (!CanHarvest(race)) return false;
        var instance = _instances.Values.First(value => value.Prisoners < value.Capacity);
        instance.Prisoners++;
        roomId = instance.RoomId;
        return true;
    }

    public bool Complete(int roomId, string race, int population, ResourceLedger resources)
    {
        if (!_instances.TryGetValue(roomId, out var instance) || instance.Prisoners <= 0) return false;
        instance.Prisoners--;
        if (!OriginalGameData.Current.Races.TryGetValue(race, out var rule)) return false;
        foreach (var output in rule.HarvestResources) resources.Add(output.Key, output.Value);
        // ROOM_CANNIBAL.reportCannibal: 100 / current population; its source
        // update then decays this value over two years.
        _cannibalism[race] = _cannibalism.GetValueOrDefault(race) +
            100.0 / Math.Max(1, population);
        return true;
    }

    public void Cancel(int roomId)
    {
        if (_instances.TryGetValue(roomId, out var instance) && instance.Prisoners > 0)
            instance.Prisoners--;
    }

    public double Cannibalism(string race) => Math.Clamp(_cannibalism.GetValueOrDefault(race), 0, 1);

    public void Tick(double delta, double secondsPerYear, int raceCount)
    {
        foreach (var race in _cannibalism.Keys.ToArray())
        {
            var value = Math.Max(1, _cannibalism[race]);
            _cannibalism[race] = Math.Clamp(_cannibalism[race] -
                value * delta / Math.Max(1, secondsPerYear * 2 * Math.Max(1, raceCount)), 0, 1.5);
        }
    }
}

public sealed class CannibalInstanceRuntime
{
    public int RoomId { get; init; }
    public GodotSyxPort.Core.GridCoord Anchor { get; set; }
    public int Capacity { get; set; }
    public int Prisoners { get; set; }
    public int Workers { get; set; }
}
