using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

public sealed class RoomPlanner
{
    private readonly HashSet<GridCoord> _area = new();
    private readonly HashSet<GridCoord> _perimeter = new();
    private readonly HashSet<GridCoord> _doors = new();
    private readonly GridWorld _world;
    private readonly RoomSystem _rooms;

    private bool _autoWalls = true;
    public bool AutoWalls
    {
        get => _autoWalls;
        set
        {
            _autoWalls = value;
            ApplyDefinitionAutoWalls();
            RefreshPreview();
        }
    }
    public bool HasDraft => Area > 0;
    public bool BuildOnExistingStructures
    {
        get => Definitions.BuildOnExistingStructures;
        set => Definitions.BuildOnExistingStructures = value;
    }
    public int Area => UsesDefinition ? Definitions.Area.Count : _area.Count;
    public int Doors => UsesDefinition ? Definitions.Doors.Count : _doors.Count;
    public RoomType Type { get; set; } = RoomType.Storage;
    public RoomPlacementRuntime Definitions { get; }
    public bool UsesDefinition { get; private set; }
    public string SelectedDefinition => UsesDefinition ? Definitions.DefinitionKey : Type.ToString();
    public string PlacementStatus { get; private set; } = "";
    public string StructureKey
    {
        get => Definitions.StructureKey;
        set => Definitions.StructureKey = value;
    }
    public IReadOnlyDictionary<int, double> DefinitionItems => Definitions.ItemGroups;
    public int DefinitionItemCount => Definitions.Placements.Count;
    public IReadOnlyDictionary<GridCoord, int> DefinitionFurniture => Definitions.Furniture;
    public bool UsesFixedItemPlacement => UsesDefinition &&
        (Definitions.DefinitionKey.Equals("_HOME", System.StringComparison.OrdinalIgnoreCase) ||
         Definitions.DefinitionKey.Equals("_HOME_CHAMBER", System.StringComparison.OrdinalIgnoreCase));

    public RoomPlanner(GridWorld world, RoomSystem rooms)
    {
        _world = world;
        _rooms = rooms;
        Definitions = new RoomPlacementRuntime(world, rooms);
    }

    public void SelectDefinition(string definitionKey)
    {
        Definitions.Select(definitionKey);
        ApplyDefinitionAutoWalls();
        UsesDefinition = true;
        PlacementStatus = "";
        _area.Clear(); _perimeter.Clear(); _doors.Clear();
        _world.ClearRoomPreview();
    }

    public void SelectLegacy(RoomType type)
    {
        Type = type;
        UsesDefinition = false;
        PlacementStatus = "";
        Definitions.Clear();
        _world.ClearRoomPreview();
    }

    public void SetDefinitionItemAmount(int group, int amount)
    {
        if (!UsesDefinition) return;
        Definitions.SetItemGroupAmount(group, amount);
        PlacementStatus = Definitions.Validate().Error;
    }

    public void SetDefinitionUpgrade(int upgrade)
    {
        if (!UsesDefinition) return;
        if (!_rooms.CanSetDefinitionUpgrade(Definitions.DefinitionKey, upgrade))
        {
            PlacementStatus = "Улучшение комнаты ещё не открыто технологией";
            return;
        }
        Definitions.SetUpgrade(upgrade);
        PlacementStatus = Definitions.Validate().Error;
    }

    public bool CanSetDefinitionUpgrade(int upgrade) => UsesDefinition &&
        _rooms.CanSetDefinitionUpgrade(Definitions.DefinitionKey, upgrade);

    public IReadOnlyDictionary<ResourceKind, int> DefinitionCost() =>
        UsesDefinition ? Definitions.Cost() : new Dictionary<ResourceKind, int>();

    public double[] DefinitionStats() => UsesDefinition ? Definitions.Stats() : System.Array.Empty<double>();

    public double[] DefinitionStatsWithAdditionalItem(int group, double statMultiplier) =>
        UsesDefinition ? Definitions.StatsWithAdditionalItem(group, statMultiplier) :
            System.Array.Empty<double>();

    public void SetArea(IEnumerable<GridCoord> cells)
    {
        if (UsesDefinition)
        {
            Definitions.SetArea(cells);
            // Full validation includes flood-fill and access checks. It belongs to the
            // confirmation step, not to every mouse pixel while the outline is moving.
            PlacementStatus = "";
            RefreshPreview();
            return;
        }
        _area.Clear();
        _perimeter.Clear();
        _doors.Clear();
        foreach (var cell in cells)
        {
            if (!_world.IsInside(cell) || _world.Data.Has(cell, TileFlags.Wall) || _rooms.Contains(cell)) continue;
            _area.Add(cell);
        }
        foreach (var cell in _area)
        {
            foreach (var offset in GridCoord.Cardinal)
            {
                var edge = cell + offset;
                if (_world.IsInside(edge) && !_area.Contains(edge)) _perimeter.Add(edge);
            }
        }
        foreach (var cell in _area)
            foreach (var diagonal in new[] { new GridCoord(-1, -1), new GridCoord(1, -1), new GridCoord(-1, 1), new GridCoord(1, 1) })
            {
                var corner = cell + diagonal;
                var sideA = new GridCoord(cell.X + diagonal.X, cell.Z);
                var sideB = new GridCoord(cell.X, cell.Z + diagonal.Z);
                if (_world.IsInside(corner) && !_area.Contains(corner) && !_area.Contains(sideA) && !_area.Contains(sideB)) _perimeter.Add(corner);
            }
        RefreshPreview();
    }

    public void ExpandArea(IEnumerable<GridCoord> cells)
    {
        if (!UsesDefinition) { SetArea(cells); return; }
        Definitions.ExpandArea(cells);
        PlacementStatus = "";
        RefreshPreview();
    }

    public void ShrinkArea(IEnumerable<GridCoord> cells)
    {
        if (!UsesDefinition) { SetArea(_area.Except(cells)); return; }
        Definitions.ShrinkArea(cells);
        PlacementStatus = "";
        RefreshPreview();
    }

    public void PreviewArea(IEnumerable<GridCoord> cells, bool shrinking)
    {
        if (UsesDefinition) Definitions.PreviewArea(cells, shrinking);
        else _world.ShowRoomPreview(shrinking ? _area.Except(cells) : _area.Concat(cells),
            System.Array.Empty<GridCoord>(), System.Array.Empty<GridCoord>());
    }

    public bool Undo()
    {
        if (!UsesDefinition || !Definitions.Undo()) return false;
        PlacementStatus = "";
        RefreshPreview();
        return true;
    }

    public bool HasHistory => UsesDefinition && Definitions.HasHistory;

    public bool ToggleDoor(GridCoord cell)
    {
        if (UsesDefinition)
        {
            var changed = Definitions.ToggleDoor(cell);
            if (!changed) return false;
            // PlacerDoor performs a local edge/opening check while editing. Connected
            // area and external-access validation belongs to the final confirmation.
            PlacementStatus = "";
            _world.ShowDoorPreview(Definitions.Doors);
            return changed;
        }
        if (!_perimeter.Contains(cell)) return false;
        if (!_doors.Add(cell)) _doors.Remove(cell);
        PlacementStatus = "";
        _world.ShowDoorPreview(_doors);
        return true;
    }

    public bool ToggleFurniture(GridCoord cell, int group, int variant = 0, int rotation = 0)
    {
        if (!UsesDefinition) return false;
        var changed = Definitions.ToggleFurniture(cell, group, variant, rotation);
        PlacementStatus = Definitions.Validate().Error;
        RefreshPreview();
        return changed;
    }

    public void PreviewFurniture(GridCoord anchor, int group, int variant, int rotation)
    {
        if (!UsesDefinition || !HasDraft) return;
        var variants = FurnisherLayoutCatalog.Variants(Definitions.DefinitionKey, group);
        var layout = variants[System.Math.Clamp(variant, 0, variants.Count - 1)];
        var origin = layout.OriginAtCursor(anchor, rotation);
        var ghost = layout.RotatedCells(rotation).Select(offset => origin + offset).ToArray();
        var invalid = ghost.Where(cell => !Definitions.Area.Contains(cell) ||
            Definitions.Furniture.ContainsKey(cell)).ToArray();
        var valid = ghost.Except(invalid);
        _world.ShowRoomPreview(Definitions.Area,
            Definitions.AutoWalls ? Definitions.Perimeter : System.Array.Empty<GridCoord>(),
            Definitions.AutoWalls ? Definitions.Doors : System.Array.Empty<GridCoord>(),
            Definitions.Furniture.Keys.Concat(valid), invalid,
            DefinitionVisuals().Append(new FurnitureVisualPlacement(
                Definitions.DefinitionKey, group, variant, rotation, origin, Definitions.Upgrade)));
    }

    public void PreviewFixedFurniture(GridCoord anchor, int group, int variant, int rotation)
    {
        if (!UsesFixedItemPlacement) return;
        var geometry = Definitions.FixedItemGeometry(anchor, group, variant, rotation);
        if (geometry is null) return;
        var ghost = geometry.Occupied;
        var invalid = ghost.Where(cell => !_world.IsInside(cell) ||
            _world.Data.Has(cell, TileFlags.Wall) || _rooms.Contains(cell)).ToArray();
        var valid = ghost.Except(invalid).ToArray();
        var chamber = Definitions.DefinitionKey.Equals(
            "_HOME_CHAMBER", System.StringComparison.OrdinalIgnoreCase);
        var placements = chamber
            ? new[] { new FurnitureVisualPlacement(Definitions.DefinitionKey, group,
                geometry.Variant, rotation, geometry.Origin, Definitions.Upgrade) }
            : System.Array.Empty<FurnitureVisualPlacement>();
        // PlacableFixed renders the item itself as the dark movable placeholder;
        // its generated structure and openings are drawn over that placeholder.
        _world.ShowRoomPreview(System.Array.Empty<GridCoord>(),
            Definitions.AutoWalls ? geometry.Perimeter : System.Array.Empty<GridCoord>(),
            Definitions.AutoWalls ? geometry.Doors : System.Array.Empty<GridCoord>(),
            valid, invalid, placements);
    }

    public bool PlaceFixedFurniture(
        GridCoord anchor, int group, int variant, int rotation, JobBoard jobs)
    {
        if (!UsesFixedItemPlacement)
        {
            PlacementStatus = "Здесь нельзя разместить выбранный дом";
            return false;
        }
        var created = Definitions.CommitFixedItem(anchor, group, variant, rotation, jobs);
        if (created.Count == 0)
        {
            PlacementStatus = "Здесь нельзя разместить выбранный дом";
            return false;
        }
        _world.ClearRoomPreview();
        PlacementStatus = created.Count == 1
            ? "Дом запланирован"
            : $"Запланировано жилых секций: {created.Count}";
        return true;
    }

    public int Commit(JobBoard jobs)
    {
        if (UsesDefinition)
        {
            var validation = Definitions.Validate();
            PlacementStatus = validation.Error;
            if (!validation.Valid) return 0;
            var walls = Definitions.AutoWalls
                ? Definitions.Perimeter.Count - Definitions.Doors.Count : 0;
            Definitions.Commit(jobs);
            _world.ClearRoomPreview();
            PlacementStatus = "Комната запланирована";
            return walls;
        }
        if (_area.Count == 0) return 0;
        var created = AutoWalls ? _perimeter.Count - _doors.Count : 0;
        foreach (var cell in _area) _world.SetZone(cell);
        foreach (var cell in _doors) _world.SetDoor(cell);
        _rooms.Create(Type, _area, _perimeter, _doors, AutoWalls, jobs);
        _area.Clear();
        _perimeter.Clear();
        _doors.Clear();
        _world.ClearRoomPreview();
        return created;
    }

    public void CancelDraft()
    {
        _area.Clear();
        _perimeter.Clear();
        _doors.Clear();
        Definitions.Clear();
        PlacementStatus = "";
        _world.ClearRoomPreview();
    }

    private void RefreshPreview()
    {
        if (UsesDefinition)
            _world.ShowRoomPreview(Definitions.Area,
                Definitions.AutoWalls ? Definitions.Perimeter : System.Array.Empty<GridCoord>(),
                Definitions.AutoWalls ? Definitions.Doors : System.Array.Empty<GridCoord>(),
                Definitions.Furniture.Keys, furniturePlacements: DefinitionVisuals());
        else
            _world.ShowRoomPreview(_area,
                AutoWalls ? _perimeter : System.Array.Empty<GridCoord>(), _doors);
    }

    private IEnumerable<FurnitureVisualPlacement> DefinitionVisuals() =>
        Definitions.Placements.Select(pair => new FurnitureVisualPlacement(
            Definitions.DefinitionKey, pair.Value.Group, pair.Value.Variant,
            pair.Value.Rotation, pair.Key, Definitions.Upgrade));

    private void ApplyDefinitionAutoWalls()
    {
        var indoors = _rooms.Blueprints.Get(Definitions.DefinitionKey)?
            .Rule.Construction.Indoors == true;
        Definitions.AutoWalls = _autoWalls && indoors;
    }
}
