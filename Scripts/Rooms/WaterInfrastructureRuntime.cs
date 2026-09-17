using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rooms;

public enum WaterInfrastructureKind : byte { Pump, Canal, Drain, Pool }

public sealed class WaterInfrastructureInstance
{
    public int RoomId { get; init; }
    public WaterInfrastructureKind Kind { get; init; }
    public IReadOnlySet<int> Cells { get; init; } = new HashSet<int>();
    public double Output { get; internal set; }
    public bool Operational { get; internal set; }
}

/// <summary>
/// Shared replacement for ROOM_WATER, pump/canal/drain instances and Updater. Water nodes
/// form orthogonally-connected networks; a staffed pump distributes a finite 0..100 source
/// output through its component. Canals irrigate adjacent tiles, while drains retain the
/// source radius of ten tiles and remove excess moisture.
/// </summary>
public sealed class WaterInfrastructureRuntime
{
    public const double PumpMaximumOutput = 100;
    public const double PumpDegradationPenalty = 0.8;
    public const int DrainRadius = 10;
    public const double TilesPerSecond = 1;
    private readonly GridWorld _world;
    private readonly Dictionary<int, WaterInfrastructureInstance> _instances = new();
    private readonly Dictionary<int, double> _irrigation = new();
    private readonly Queue<int> _dirty = new();
    private double _work;

    public WaterInfrastructureRuntime(GridWorld world) => _world = world;
    public IReadOnlyCollection<WaterInfrastructureInstance> Instances => _instances.Values;
    public double Irrigation(GridCoord cell) => _irrigation.GetValueOrDefault(_world.CellToIndex(cell));

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        var changed = false;
        foreach (var room in rooms)
        {
            var kind = KindOf(room.DefinitionKey);
            if (kind is null) continue;
            active.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
            {
                _instances[room.Id] = instance = new WaterInfrastructureInstance
                {
                    RoomId = room.Id,
                    Kind = kind.Value,
                    Cells = new HashSet<int>(room.Cells)
                };
                changed = true;
            }
            var operational = room.State == RoomState.Operational;
            var output = kind == WaterInfrastructureKind.Pump && operational
                ? PumpOutput(room) : 0;
            if (instance.Operational != operational || Math.Abs(instance.Output - output) > 0.001)
                changed = true;
            instance.Operational = operational;
            instance.Output = output;
        }
        foreach (var id in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
        {
            _instances.Remove(id);
            changed = true;
        }
        if (changed) Rebuild();
    }

    public void Tick(double delta)
    {
        _work += delta * TilesPerSecond;
        while (_work >= 1 && _dirty.Count > 0)
        {
            _work -= 1;
            Apply(_dirty.Dequeue());
        }
    }

    private void Rebuild()
    {
        var changed = _irrigation.Keys.ToHashSet();
        _irrigation.Clear();
        _dirty.Clear();
        var nodes = _instances.Values.Where(instance => instance.Operational)
            .SelectMany(instance => instance.Cells.Select(cell => (Cell: cell, instance.Kind, instance.Output)))
            .ToDictionary(value => value.Cell, value => (value.Kind, value.Output));
        var visited = new HashSet<int>();
        foreach (var root in nodes.Keys)
        {
            if (!visited.Add(root)) continue;
            var component = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                component.Add(cell);
                foreach (var next in Orthogonal(cell))
                    if (nodes.ContainsKey(next) && visited.Add(next)) queue.Enqueue(next);
            }
            var output = component.Sum(cell => nodes[cell].Output);
            if (output <= 0) continue;
            var supplied = Math.Min(1, output / Math.Max(1, component.Count));
            foreach (var cell in component)
            {
                _irrigation[cell] = supplied;
                foreach (var neighbour in Orthogonal(cell))
                    _irrigation[neighbour] = Math.Max(_irrigation.GetValueOrDefault(neighbour), supplied * 0.75);
            }
        }
        foreach (var drain in _instances.Values.Where(value => value.Operational &&
                     value.Kind == WaterInfrastructureKind.Drain))
        foreach (var root in drain.Cells)
        {
            var centre = _world.FromIndex(root);
            for (var dz = -DrainRadius + 1; dz < DrainRadius; dz++)
            for (var dx = -DrainRadius + 1; dx < DrainRadius; dx++)
            {
                if (Math.Abs(dx) + Math.Abs(dz) >= DrainRadius - 2) continue;
                var cell = new GridCoord(centre.X + dx, centre.Z + dz);
                if (_world.Data.IsInside(cell)) _irrigation[_world.CellToIndex(cell)] = 0;
            }
        }
        changed.UnionWith(_irrigation.Keys);
        foreach (var cell in changed) _dirty.Enqueue(cell);
    }

    private void Apply(int index)
    {
        var cell = _world.FromIndex(index);
        var baseMoisture = _world.Data.BaseMoisture(cell);
        var irrigation = _irrigation.GetValueOrDefault(index);
        var target = irrigation > 0
            ? Math.Max(baseMoisture, (int)Math.Round(irrigation * 15))
            : baseMoisture;
        _world.Data.SetMoisture(cell, target);
    }

    private IEnumerable<int> Orthogonal(int index)
    {
        var cell = _world.FromIndex(index);
        foreach (var offset in new[] { new GridCoord(-1, 0), new GridCoord(1, 0),
                                       new GridCoord(0, -1), new GridCoord(0, 1) })
        {
            var next = new GridCoord(cell.X + offset.X, cell.Z + offset.Z);
            if (_world.Data.IsInside(next)) yield return _world.CellToIndex(next);
        }
    }

    private static WaterInfrastructureKind? KindOf(string key) => key.ToUpperInvariant() switch
    {
        "_WATERPUMP" => WaterInfrastructureKind.Pump,
        "_WATERCANAL" => WaterInfrastructureKind.Canal,
        "_WATERDRAIN" => WaterInfrastructureKind.Drain,
        "POOL_MOAT" or "POOL_POND" or "POOL_STONE" => WaterInfrastructureKind.Pool,
        _ => null
    };

    private static double PumpOutput(RoomRecord room)
    {
        var maximum = Math.Max(1, room.Employment.Maximum);
        return Math.Ceiling((1 - PumpDegradationPenalty * room.Degradation) *
            PumpMaximumOutput * room.Employment.Employed / maximum);
    }
}
