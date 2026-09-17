using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed record RoomPlacementValidation(bool Valid, string Error)
{
    public static readonly RoomPlacementValidation Success = new(true, "");
}

/// <summary>Data-driven PLACEMENT/RoomPlacer area and door model.</summary>
public sealed class RoomPlacementRuntime
{
    private readonly GridWorld _world;
    private readonly RoomSystem _rooms;
    private readonly HashSet<GridCoord> _area = new();
    private readonly HashSet<GridCoord> _perimeter = new();
    private readonly HashSet<GridCoord> _doors = new();
    private readonly Dictionary<GridCoord, int> _furniture = new();
    private readonly Dictionary<GridCoord, FurniturePlacement> _placements = new();
    private readonly Dictionary<int, double> _itemGroups = new();
    private readonly Dictionary<int, double> _itemCosts = new();
    private readonly Stack<RoomPlacementSnapshot> _history = new();

    public string DefinitionKey { get; private set; }
    public int Upgrade { get; private set; }
    public bool AutoWalls { get; set; } = true;
    public bool BuildOnExistingStructures { get; set; }
    public IReadOnlyCollection<GridCoord> Area => _area;
    public IReadOnlyCollection<GridCoord> Perimeter => _perimeter;
    public IReadOnlyCollection<GridCoord> Doors => _doors;
    public IReadOnlyDictionary<GridCoord, int> Furniture => _furniture;
    public IReadOnlyDictionary<GridCoord, FurniturePlacement> Placements => _placements;
    public IReadOnlyDictionary<int, double> ItemGroups => _itemGroups;
    public IReadOnlyDictionary<int, double> ItemCosts => _itemCosts;
    public bool HasHistory => _history.Count > 0;

    public RoomPlacementRuntime(GridWorld world, RoomSystem rooms, string definitionKey = "_STOCKPILE")
    {
        _world = world;
        _rooms = rooms;
        DefinitionKey = definitionKey;
    }

    public void Select(string definitionKey, int upgrade = 0)
    {
        var blueprint = _rooms.Blueprints.Get(definitionKey) ??
            throw new InvalidOperationException($"Unknown room definition: {definitionKey}");
        DefinitionKey = blueprint.Key;
        Upgrade = Math.Clamp(upgrade, 0, blueprint.MaximumUpgrade);
        _itemGroups.Clear();
        _itemCosts.Clear();
        _history.Clear();
    }

    public void SetUpgrade(int upgrade)
    {
        var blueprint = _rooms.Blueprints.Get(DefinitionKey) ??
            throw new InvalidOperationException($"Unknown room definition: {DefinitionKey}");
        Upgrade = Math.Clamp(upgrade, 0, blueprint.MaximumUpgrade);
    }

    public void SetArea(IEnumerable<GridCoord> cells)
    {
        PushHistory();
        _area.Clear();
        _doors.Clear();
        _furniture.Clear();
        _placements.Clear();
        _itemGroups.Clear();
        foreach (var cell in cells)
            if (CanAddArea(cell)) _area.Add(cell);
        RebuildPerimeter();
    }

    public void ExpandArea(IEnumerable<GridCoord> cells)
    {
        PushHistory();
        foreach (var cell in cells)
            if (CanAddArea(cell)) _area.Add(cell);
        RebuildPerimeter();
    }

    public void ShrinkArea(IEnumerable<GridCoord> cells)
    {
        PushHistory();
        _area.ExceptWith(cells);
        foreach (var anchor in _placements.Where(pair =>
                     pair.Value.Cells.Any(cell => !_area.Contains(cell))).Select(pair => pair.Key).ToArray())
        {
            foreach (var occupied in _placements[anchor].Cells) _furniture.Remove(occupied);
            _placements.Remove(anchor);
        }
        RecountFurniture();
        RebuildPerimeter();
    }

    public void PreviewArea(IEnumerable<GridCoord> cells, bool shrinking)
    {
        var previewArea = _area.ToHashSet();
        if (shrinking) previewArea.ExceptWith(cells);
        else foreach (var cell in cells)
            if (CanAddArea(cell)) previewArea.Add(cell);
        var previewPerimeter = CalculatePerimeter(previewArea);
        _world.ShowRoomPreview(previewArea,
            AutoWalls ? previewPerimeter : Array.Empty<GridCoord>(),
            _doors.Where(previewPerimeter.Contains),
            _furniture.Keys.Where(previewArea.Contains));
    }

    private bool CanAddArea(GridCoord cell) => _world.IsInside(cell) &&
        (BuildOnExistingStructures || !_world.Data.Has(cell, TileFlags.Wall)) &&
        !_rooms.Contains(cell);

    private void RebuildPerimeter()
    {
        _perimeter.Clear();
        _perimeter.UnionWith(CalculatePerimeter(_area));
        _doors.IntersectWith(_perimeter);
    }

    private HashSet<GridCoord> CalculatePerimeter(IReadOnlySet<GridCoord> area)
    {
        var perimeter = new HashSet<GridCoord>();
        foreach (var cell in area)
        {
            foreach (var offset in GridCoord.Cardinal)
            {
                var edge = cell + offset;
                if (_world.IsInside(edge) && !area.Contains(edge)) perimeter.Add(edge);
            }
            foreach (var diagonal in GridCoord.AllDirections.Skip(4))
            {
                var corner = cell + diagonal;
                var sideA = new GridCoord(cell.X + diagonal.X, cell.Z);
                var sideB = new GridCoord(cell.X, cell.Z + diagonal.Z);
                if (_world.IsInside(corner) && !area.Contains(corner) &&
                    !area.Contains(sideA) && !area.Contains(sideB)) perimeter.Add(corner);
            }
        }
        return perimeter;
    }

    public bool ToggleDoor(GridCoord cell)
    {
        if (!_perimeter.Contains(cell)) return false;
        PushHistory();
        if (!_doors.Add(cell)) _doors.Remove(cell);
        return true;
    }

    public bool ToggleFurniture(GridCoord cell, int group, int variant = 0, int rotation = 0)
    {
        var count = _rooms.Blueprints.Get(DefinitionKey)?.Rule.FurnisherItems.Count ?? 0;
        if ((uint)group >= (uint)count) return false;
        if (_furniture.ContainsKey(cell))
        {
            PushHistory();
            var anchor = _placements.First(pair => pair.Value.Cells.Contains(cell)).Key;
            foreach (var occupied in _placements[anchor].Cells) _furniture.Remove(occupied);
            _placements.Remove(anchor);
            RecountFurniture();
            return true;
        }
        var variants = FurnisherLayoutCatalog.Variants(DefinitionKey, group);
        var layout = variants[Math.Clamp(variant, 0, variants.Count - 1)];
        var origin = layout.OriginAtCursor(cell, rotation);
        var occupiedCells = layout.RotatedCells(rotation).Select(offset => origin + offset).ToArray();
        if (occupiedCells.Any(occupied => !_area.Contains(occupied) || _furniture.ContainsKey(occupied))) return false;
        PushHistory();
        var blockerCells = layout.RotatedBlockerCells(rotation).Select(offset => origin + offset).ToArray();
        var reachableCells = layout.RotatedReachableCells(rotation).Select(offset => origin + offset).ToArray();
        var workCells = layout.RotatedWorkCells(rotation).Select(offset => origin + offset).ToArray();
        var storageCells = layout.RotatedStorageCells(rotation).Select(offset => origin + offset).ToArray();
        var placement = new FurniturePlacement(
            group, layout.CostMultiplier, layout.StatMultiplier,
            occupiedCells, blockerCells, reachableCells,
            workCells, storageCells);
        _placements[origin] = placement;
        foreach (var occupied in occupiedCells) _furniture[occupied] = group;
        RecountFurniture();
        return true;
    }

    public void SetItemGroupAmount(int group, int amount)
    {
        var count = _rooms.Blueprints.Get(DefinitionKey)?.Rule.FurnisherItems.Count ?? 0;
        if ((uint)group >= (uint)count) throw new ArgumentOutOfRangeException(nameof(group));
        if (amount <= 0)
        {
            _itemGroups.Remove(group);
            _itemCosts.Remove(group);
        }
        else
        {
            _itemGroups[group] = amount;
            _itemCosts[group] = amount;
        }
    }

    public RoomPlacementValidation Validate()
    {
        if (_area.Count == 0) return new(false, "Площадь комнаты не задана");
        if (_area.Count > RoomInstanceRuntime.MaximumArea) return new(false, "Превышена максимальная площадь комнаты");
        var width = _area.Max(cell => cell.X) - _area.Min(cell => cell.X) + 1;
        var height = _area.Max(cell => cell.Z) - _area.Min(cell => cell.Z) + 1;
        if (width > RoomInstanceRuntime.MaximumDimension || height > RoomInstanceRuntime.MaximumDimension)
            return new(false, "Превышен максимальный размер комнаты");
        if (!Connected()) return new(false, "Все клетки комнаты должны быть соединены");
        var blueprint = _rooms.Blueprints.Get(DefinitionKey);
        if (blueprint is null) return new(false, "Неизвестный тип комнаты");
        if (!_rooms.CanSetDefinitionUpgrade(DefinitionKey, Upgrade))
            return new(false, "Улучшение комнаты ещё не открыто технологией");
        if (blueprint.Rule.HasWork && blueprint.Rule.FurnisherItems.Count > 0 &&
            _placements.Count == 0)
            return new(false, "Рабочему помещению требуется хотя бы один предмет обстановки");
        for (var group = 0; group < blueprint.Rule.FurnisherItems.Count; group++)
        {
            var constraint = FurnisherConstraintCatalog.Group(DefinitionKey, group);
            if (constraint is null) continue;
            var placed = _placements.Values.Count(item => item.Group == group);
            if (placed < constraint.Minimum)
                return new(false, $"Для группы {group + 1} требуется минимум предметов: {constraint.Minimum}");
            if (placed > constraint.Maximum)
                return new(false, $"Для группы {group + 1} разрешено максимум предметов: {constraint.Maximum}");
        }
        var stats = FurnisherStatRuntime.Evaluate(DefinitionKey, _itemGroups);
        var minimums = FurnisherConstraintCatalog.StatMinimums(DefinitionKey);
        for (var index = 0; index < minimums.Count; index++)
            if (minimums[index] > 0 &&
                ((uint)index >= (uint)stats.Length || stats[index] < minimums[index]))
                return new(false,
                    $"Для показателя {index + 1} требуется минимум {minimums[index]:0.####}");
        var furnitureBlockers = _placements.Values.SelectMany(item => item.BlockerCells).ToHashSet();
        foreach (var reachable in _placements.Values.SelectMany(item => item.ReachableCells))
        {
            var accessible = GridCoord.Cardinal.Any(offset =>
            {
                var neighbor = reachable + offset;
                return _area.Contains(neighbor) && !furnitureBlockers.Contains(neighbor) &&
                       !_world.Data.IsBlocked(neighbor);
            });
            if (!accessible) return new(false, "К предмету должен оставаться доступный проход");
        }
        if (blueprint.Rule.Construction.Indoors && AutoWalls &&
            !_doors.Any(door => !_world.Data.IsBlocked(door)))
            return new(false, "Закрытому помещению требуется дверной проём");
        if (!HasExternalAccess()) return new(false, "У комнаты нет внешнего доступа");
        return RoomPlacementValidation.Success;
    }

    public IReadOnlyDictionary<ResourceKind, int> Cost()
    {
        var blueprint = _rooms.Blueprints.Get(DefinitionKey) ??
            throw new InvalidOperationException($"Unknown room definition: {DefinitionKey}");
        return blueprint.Furnisher.ConstructionCost(_area.Count, _itemCosts, Upgrade);
    }

    public RoomRecord Commit(JobBoard jobs)
    {
        var validation = Validate();
        if (!validation.Valid) throw new InvalidOperationException(validation.Error);
        foreach (var cell in _area) _world.SetZone(cell);
        var room = _rooms.CreateFromDefinition(
            DefinitionKey, _area, _perimeter, _doors, AutoWalls, _itemGroups, _itemCosts,
            Upgrade, jobs,
            _furniture, _placements);
        Clear();
        return room;
    }

    public void Clear()
    {
        _area.Clear();
        _perimeter.Clear();
        _doors.Clear();
        _furniture.Clear();
        _placements.Clear();
        _itemGroups.Clear();
        _itemCosts.Clear();
        _history.Clear();
    }

    public bool Undo()
    {
        if (_history.Count == 0) return false;
        var snapshot = _history.Pop();
        _area.Clear(); _area.UnionWith(snapshot.Area);
        _doors.Clear(); _doors.UnionWith(snapshot.Doors);
        _furniture.Clear();
        foreach (var pair in snapshot.Furniture) _furniture[pair.Key] = pair.Value;
        _placements.Clear();
        foreach (var pair in snapshot.Placements) _placements[pair.Key] = pair.Value;
        RecountFurniture();
        RebuildPerimeter();
        return true;
    }

    private void PushHistory() => _history.Push(new RoomPlacementSnapshot(
        _area.ToArray(), _doors.ToArray(),
        new Dictionary<GridCoord, int>(_furniture),
        new Dictionary<GridCoord, FurniturePlacement>(_placements)));

    private void RecountFurniture()
    {
        _itemGroups.Clear();
        _itemCosts.Clear();
        foreach (var placement in _placements.Values)
        {
            _itemGroups[placement.Group] = _itemGroups.GetValueOrDefault(placement.Group) +
                                           placement.StatMultiplier;
            _itemCosts[placement.Group] = _itemCosts.GetValueOrDefault(placement.Group) +
                                          placement.CostMultiplier;
        }
    }

    private bool Connected()
    {
        var visited = new HashSet<GridCoord>();
        var queue = new Queue<GridCoord>();
        queue.Enqueue(_area.First());
        visited.Add(_area.First());
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            foreach (var offset in GridCoord.Cardinal)
            {
                var next = cell + offset;
                if (_area.Contains(next) && visited.Add(next)) queue.Enqueue(next);
            }
        }
        return visited.Count == _area.Count;
    }

    private bool HasExternalAccess() => _area.Any(cell => GridCoord.Cardinal.Any(offset =>
    {
        var next = cell + offset;
        return !_area.Contains(next) && _world.IsInside(next) && !_world.Data.IsBlocked(next);
    }));
}

public sealed record FurniturePlacement(
    int Group,
    double CostMultiplier,
    double StatMultiplier,
    IReadOnlyList<GridCoord> Cells,
    IReadOnlyList<GridCoord> BlockerCells,
    IReadOnlyList<GridCoord> ReachableCells,
    IReadOnlyList<GridCoord> WorkCells,
    IReadOnlyList<GridCoord> StorageCells);
public sealed record RoomPlacementSnapshot(
    IReadOnlyList<GridCoord> Area,
    IReadOnlyList<GridCoord> Doors,
    IReadOnlyDictionary<GridCoord, int> Furniture,
    IReadOnlyDictionary<GridCoord, FurniturePlacement> Placements);
