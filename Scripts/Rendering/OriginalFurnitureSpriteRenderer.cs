using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.Rendering;

/// <summary>
/// Runtime counterpart of RoomSpriteCombo/RoomSprite1x1 for source furniture.
/// A raw Songs of Syx game sheet contains colour sprites in its left half and
/// shadows in its right half; ComposerSources.house/full2 select 16x16 regions.
/// </summary>
public sealed partial class OriginalFurnitureSpriteRenderer : Node3D
{
    private const string SpriteRoot = "res://Data/Original/assets/sprite/game";
    private readonly Dictionary<GridCoord, Node3D> _placements = new();
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    private int _mapWidth;
    private int _mapHeight;

    private static readonly int[] HouseX =
        { 52, 52, 0, 0, 52, 52, 0, 0, 32, 32, 16, 16, 32, 32, 16, 16 };
    private static readonly int[] HouseY =
        { 52, 32, 52, 32, 0, 16, 0, 16, 52, 32, 52, 32, 0, 16, 0, 16 };

    public void Initialize(int mapWidth, int mapHeight)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    public static bool Supports(string roomKey) => string.Equals(
        FurnisherLayoutCatalog.FamilyForRoom(roomKey), "food/hunter",
        StringComparison.OrdinalIgnoreCase);

    public void Add(FurnitureVisualPlacement placement)
    {
        Remove(placement.Origin);
        if (!Supports(placement.RoomKey)) return;
        var root = new Node3D { Name = $"Furniture_{placement.Origin.X}_{placement.Origin.Z}" };
        AddChild(root);
        _placements[placement.Origin] = root;
        RenderHunter(root, placement);
    }

    public void Remove(GridCoord origin)
    {
        if (!_placements.Remove(origin, out var node)) return;
        node.QueueFree();
    }

    public void Clear()
    {
        foreach (var node in _placements.Values) node.QueueFree();
        _placements.Clear();
    }

    public void SetPreview(IEnumerable<FurnitureVisualPlacement> placements,
        IReadOnlySet<GridCoord> invalidCells)
    {
        Clear();
        foreach (var placement in placements.Where(value => Supports(value.RoomKey)))
        {
            var root = new Node3D { Name = $"Preview_{placement.Origin.X}_{placement.Origin.Z}" };
            AddChild(root);
            _placements[placement.Origin] = root;
            RenderHunterPlaceholder(root, placement, invalidCells);
        }
    }

    private void RenderHunter(Node3D root, FurnitureVisualPlacement placement)
    {
        var combo = new HashSet<GridCoord>();
        var storage = new HashSet<GridCoord>();
        var nick = new HashSet<GridCoord>();
        var layout = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group)
            [Math.Clamp(placement.Variant, 0,
                FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group).Count - 1)];
        foreach (var source in layout.Cells)
        {
            var offset = layout.RotateCell(source, placement.Rotation);
            var cell = placement.Origin + offset;
            if (placement.Group == 0)
            {
                combo.Add(cell);
                if (source.X == layout.Width - 1) nick.Add(cell);
            }
            else
            {
                var isStorage = layout.Width > 1 &&
                    (source.X == 0 || layout.Width > 2 && source.X == layout.Width - 1);
                if (isStorage) storage.Add(cell); else combo.Add(cell);
                nick.Add(cell);
            }
        }

        foreach (var cell in combo)
        {
            var mask = 0;
            if (combo.Contains(cell + new GridCoord(0, -1))) mask |= 1;
            if (combo.Contains(cell + new GridCoord(1, 0))) mask |= 2;
            if (combo.Contains(cell + new GridCoord(0, 1))) mask |= 4;
            if (combo.Contains(cell + new GridCoord(-1, 0))) mask |= 8;
            var hash = StableHash(cell.X, cell.Z, placement.Group);
            var row = (hash & 1) == 0 ? 1 : 8;
            var variation = (hash >> 1) & 1;
            AddTile(root, cell, "combo/COMBO_TABLES.png",
                variation * 72 + 2 + HouseX[mask], row * 72 + 2 + HouseY[mask], 0.12f);
        }
        foreach (var cell in storage)
        {
            var hash = StableHash(cell.X, cell.Z, placement.Group);
            var row = (hash & 1) == 0 ? 1 : 5;
            AddTile(root, cell, "1x1/STORAGE.png", ((hash >> 1) & 3) * 22 + 3,
                row * 22 + 3, 0.13f, placement.Rotation);
        }
        foreach (var cell in nick)
        {
            var hash = StableHash(cell.X, cell.Z, placement.Group + 17);
            var animal = (hash & 2) != 0;
            var row = hash & 1;
            AddTile(root, cell, animal ? "1x1/ANIMAL.png" : "1x1/TOP.png",
                ((hash >> 2) & 3) * 22 + 3, row * 22 + 3, 0.14f);
        }
    }

    private void RenderHunterPlaceholder(Node3D root, FurnitureVisualPlacement placement,
        IReadOnlySet<GridCoord> invalidCells)
    {
        var combo = new HashSet<GridCoord>();
        var singles = new HashSet<GridCoord>();
        var variants = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group);
        var layout = variants[Math.Clamp(placement.Variant, 0, variants.Count - 1)];
        foreach (var source in layout.Cells)
        {
            var cell = placement.Origin + layout.RotateCell(source, placement.Rotation);
            var storage = placement.Group == 1 && layout.Width > 1 &&
                (source.X == 0 || layout.Width > 2 && source.X == layout.Width - 1);
            if (storage) singles.Add(cell); else combo.Add(cell);
        }
        foreach (var cell in combo)
        {
            var mask = 0;
            if (combo.Contains(cell + new GridCoord(0, -1))) mask |= 1;
            if (combo.Contains(cell + new GridCoord(1, 0))) mask |= 2;
            if (combo.Contains(cell + new GridCoord(0, 1))) mask |= 4;
            if (combo.Contains(cell + new GridCoord(-1, 0))) mask |= 8;
            AddConstructionMask(root, cell, mask, invalidCells.Contains(cell));
        }
        foreach (var cell in singles)
            AddConstructionMask(root, cell, 0, invalidCells.Contains(cell));
    }

    private void AddConstructionMask(Node3D root, GridCoord cell, int mask, bool invalid)
    {
        // UIConses.Big.filled is source house variant 9. Big houses begin at y=40;
        // ComposerSources.house adds the two-pixel body margin and mask offsets.
        const int variant = 9;
        var bodyX = variant % 7 * 72 + 2;
        var bodyY = 40 + variant / 7 * 72 + 2;
        AddTile(root, cell, "ui/Cons.png", bodyX + HouseX[mask],
            bodyY + HouseY[mask], invalid ? 0.165f : 0.155f, 0,
            invalid ? new Color(0.92f, 0.16f, 0.12f, 0.9f) :
                new Color(0.12f, 0.48f, 1f, 0.86f));
    }

    private void AddTile(Node3D root, GridCoord cell, string source, int x, int y,
        float elevation, int quarterTurns = 0, Color? tint = null)
    {
        if (!_textures.TryGetValue(source, out var texture))
        {
            var path = source.StartsWith("ui/", StringComparison.OrdinalIgnoreCase)
                ? $"res://Data/Original/assets/sprite/{source}"
                : $"{SpriteRoot}/{source}";
            texture = GD.Load<Texture2D>(path);
            if (texture is null) return;
            _textures[source] = texture;
        }
        var atlas = new AtlasTexture { Atlas = texture, Region = new Rect2(x, y, 16, 16) };
        var material = new StandardMaterial3D
        {
            AlbedoTexture = atlas,
            AlbedoColor = tint ?? Colors.White,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest
        };
        root.AddChild(new MeshInstance3D
        {
            Position = new Vector3(cell.X - _mapWidth / 2f + 0.5f, elevation,
                cell.Z - _mapHeight / 2f + 0.5f),
            RotationDegrees = new Vector3(0, quarterTurns * 90, 0),
            Mesh = new PlaneMesh { Size = Vector2.One, Material = material }
        });
    }

    private static int StableHash(int x, int z, int salt)
    {
        unchecked
        {
            var value = x * 73856093 ^ z * 19349663 ^ salt * 83492791;
            value ^= value >> 16;
            return value & int.MaxValue;
        }
    }
}
