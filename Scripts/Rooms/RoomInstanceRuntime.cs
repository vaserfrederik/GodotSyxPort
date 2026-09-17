using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rooms;

public readonly record struct RoomBounds(int X, int Z, int Width, int Height);
public sealed record RoomRuntimeState(int WorkerTarget, int RecipeIndex, int UpgradeLevel, bool Enabled);

/// <summary>Shared simulation port for Room, ROOMA, RoomInstance, RoomInit and RoomState.</summary>
public sealed class RoomInstanceRuntime
{
    public const int MaximumArea = 2048;
    public const int MaximumDimension = 55;

    private readonly GridWorld _world;
    public RoomRecord Record { get; }
    public RoomBlueprintRuntime Blueprint { get; }
    public RoomBounds Bounds { get; }
    public GridCoord Anchor { get; }
    public string Name { get; }
    public bool Exists { get; private set; } = true;
    public bool Enabled { get; private set; }
    public bool Reachable { get; private set; } = true;
    public bool Active => Exists && Enabled && Reachable;
    public int Area => Record.Cells.Count;
    public double Isolation
    {
        get => Record.Isolation;
        set => Record.Isolation = Math.Clamp(value, 0, 1);
    }

    public RoomInstanceRuntime(GridWorld world, RoomRecord record, RoomBlueprintRuntime blueprint)
    {
        _world = world;
        Record = record;
        Blueprint = blueprint;
        if (record.Cells.Count is <= 0 or > MaximumArea)
            throw new InvalidOperationException($"Room area must be 1..{MaximumArea}");
        var cells = record.Cells.Select(world.FromIndex).ToArray();
        var minX = cells.Min(cell => cell.X);
        var maxX = cells.Max(cell => cell.X);
        var minZ = cells.Min(cell => cell.Z);
        var maxZ = cells.Max(cell => cell.Z);
        Bounds = new RoomBounds(minX, minZ, maxX - minX + 1, maxZ - minZ + 1);
        if (Bounds.Width > MaximumDimension || Bounds.Height > MaximumDimension)
            throw new InvalidOperationException($"Room dimensions exceed {MaximumDimension}");
        Anchor = cells.FirstOrDefault(cell => !_world.Data.IsBlocked(cell), cells[0]);
        Name = $"{blueprint.Rule.Name} #{record.Id:000}";
    }

    public bool Contains(GridCoord cell) => Record.Cells.Contains(_world.CellToIndex(cell));
    public bool IsSame(GridCoord first, GridCoord second) => Contains(first) && Contains(second);

    public void SetOperational(bool operational) => Enabled = operational;
    public void SetReachable(bool reachable) => Reachable = reachable;
    public void Remove() { Exists = false; Enabled = false; }

    public void SetUpgrade(int level) =>
        Record.UpgradeLevel = Math.Clamp(level, 0, Blueprint.MaximumUpgrade);

    public IReadOnlyDictionary<ResourceKind, int> ConstructionCost(
        IReadOnlyDictionary<int, double> itemGroupAmounts) =>
        Blueprint.Furnisher.ConstructionCost(Area, itemGroupAmounts, Record.UpgradeLevel);

    public RoomRuntimeState CaptureState() => new(
        Record.WorkerLimit,
        Record.RecipeIndex,
        Record.UpgradeLevel,
        Enabled);

    public void ApplyState(RoomRuntimeState state)
    {
        Record.WorkerLimit = state.WorkerTarget;
        Record.RecipeIndex = Math.Clamp(state.RecipeIndex, 0, Math.Max(0, Blueprint.Rule.Recipes.Count - 1));
        SetUpgrade(state.UpgradeLevel);
        Enabled = state.Enabled;
    }
}
