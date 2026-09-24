using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rooms;

public sealed class BuilderInfrastructureInstance
{
    public const int DefaultRadius = 32;
    public const int MaximumWorkers = 20;
    public int RoomId { get; init; }
    public GridCoord Anchor { get; internal set; }
    public int Radius { get; set; } = DefaultRadius;
    public int Workers { get; internal set; }
    public bool Operational { get; internal set; }
}

/// <summary>State-only port of ROOM_BUILDER/BuilderInstance/ROOM_RADIUS. It owns each
/// building office's source radius and staffing; construction-job routing remains an
/// explicit later integration so legacy global construction behaviour is preserved.</summary>
public sealed class BuilderInfrastructureRuntime
{
    private readonly GridWorld _world;
    private readonly Dictionary<int, BuilderInfrastructureInstance> _instances = new();
    public BuilderInfrastructureRuntime(GridWorld world) => _world = world;
    public IReadOnlyCollection<BuilderInfrastructureInstance> Instances => _instances.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.DefinitionKey.Equals(
                     "_BUILDER", StringComparison.OrdinalIgnoreCase)))
        {
            active.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
                _instances[room.Id] = instance = new BuilderInfrastructureInstance { RoomId = room.Id };
            instance.Operational = room.State == RoomState.Operational;
            instance.Workers = room.Employment.Employed;
            instance.Anchor = room.Cells.Count == 0 ? default : _world.FromIndex(room.Cells.First());
        }
        foreach (var id in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
            _instances.Remove(id);
    }

    public bool SetRadius(int roomId, int radius)
    {
        if (!_instances.TryGetValue(roomId, out var builder)) return false;
        // BuilderInstance.radiusRawSet receives Java's signed byte; positive radii stop at 127.
        builder.Radius = Math.Clamp(radius, 1, sbyte.MaxValue);
        return true;
    }

    public bool Covers(GridCoord cell) => _instances.Values.Any(builder => builder.Operational &&
        Math.Abs(builder.Anchor.X - cell.X) + Math.Abs(builder.Anchor.Z - cell.Z) <= builder.Radius);
}
