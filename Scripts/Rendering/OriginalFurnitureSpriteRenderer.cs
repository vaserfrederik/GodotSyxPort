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

    private enum FishSprite
    {
        Storage,
        Candle,
        Misc,
        Work,
        AuxEdge,
        AuxMid,
        AuxBig
    }

    public static bool Supports(string roomKey)
    {
        var family = FurnisherLayoutCatalog.FamilyForRoom(roomKey);
        return string.Equals(family, "food/hunter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(family, "food/fish", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(family, "home/chamber", StringComparison.OrdinalIgnoreCase);
    }

    public void Add(FurnitureVisualPlacement placement)
    {
        Remove(placement.Origin);
        if (!Supports(placement.RoomKey)) return;
        var root = new Node3D { Name = $"Furniture_{placement.Origin.X}_{placement.Origin.Z}" };
        AddChild(root);
        _placements[placement.Origin] = root;
        var family = FurnisherLayoutCatalog.FamilyForRoom(placement.RoomKey);
        if (string.Equals(family, "home/chamber", StringComparison.OrdinalIgnoreCase))
            RenderChamber(root, placement);
        else if (string.Equals(family, "food/fish",
                StringComparison.OrdinalIgnoreCase))
            RenderFish(root, placement);
        else
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
            var family = FurnisherLayoutCatalog.FamilyForRoom(placement.RoomKey);
            if (string.Equals(family, "home/chamber", StringComparison.OrdinalIgnoreCase))
                RenderChamberPlaceholder(root, placement, invalidCells);
            else if (string.Equals(family, "food/fish",
                    StringComparison.OrdinalIgnoreCase))
                RenderFishPlaceholder(root, placement, invalidCells);
            else
                RenderHunterPlaceholder(root, placement, invalidCells);
        }
    }

    private void RenderChamber(Node3D root, FurnitureVisualPlacement placement)
    {
        var layout = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group)[0];
        GridCoord Cell(int x, int z) => placement.Origin +
            layout.RotateCell(new GridCoord(x, z), placement.Rotation);
        var carpetSources = new HashSet<GridCoord>();
        for (var z = 0; z <= 4; z++)
        {
            carpetSources.Add(new GridCoord(1, z));
            carpetSources.Add(new GridCoord(4, z));
        }
        foreach (var source in carpetSources)
        {
            var mask = 0;
            if (carpetSources.Contains(source + new GridCoord(0, -1))) mask |= 1;
            if (carpetSources.Contains(source + new GridCoord(1, 0))) mask |= 2;
            if (carpetSources.Contains(source + new GridCoord(0, 1))) mask |= 4;
            if (carpetSources.Contains(source + new GridCoord(-1, 0))) mask |= 8;
            AddTile(root, Cell(source.X, source.Z), "combo/COMBO_CARPETS.png",
                2 + HouseX[mask], 4 * 72 + 2 + HouseY[mask], 0.12f);
        }

        foreach (var (x, z, row) in new[]
        {
            (0, 0, 3), (5, 0, 3), (0, 3, 3), (5, 3, 3),
            (0, 1, 4), (0, 2, 4), (5, 1, 5), (5, 2, 6),
            (2, 0, 7), (3, 0, 8)
        })
            AddOneByOne(root, Cell(x, z), "SPECIAL.png", row,
                StableHash(x, z, row), 0.14f, placement.Rotation);

        foreach (var source in new[]
                 {
                     new GridCoord(2, 1), new GridCoord(3, 1),
                     new GridCoord(2, 2), new GridCoord(3, 2)
                 })
            AddTwoByTwo(root, Cell(source.X, source.Z), "BEDS.png", 0,
                source.X - 2, source.Z - 1, 0.13f, placement.Rotation);

        foreach (var origin in new[] { new GridCoord(0, 5), new GridCoord(4, 5) })
        for (var dz = 0; dz < 2; dz++)
        for (var dx = 0; dx < 2; dx++)
            AddTwoByTwo(root, Cell(origin.X + dx, origin.Z + dz), "MONUMENT.png", 1,
                dx, dz, 0.135f, placement.Rotation);
    }

    private void RenderChamberPlaceholder(Node3D root, FurnitureVisualPlacement placement,
        IReadOnlySet<GridCoord> invalidCells)
    {
        var layout = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group)[0];
        foreach (var source in layout.Cells)
        {
            var cell = placement.Origin + layout.RotateCell(source, placement.Rotation);
            var mask = 0;
            foreach (var (offset, bit) in CardinalMasks())
                if (layout.Cells.Contains(source + offset)) mask |= bit;
            AddConstructionMask(root, cell, mask, invalidCells.Contains(cell));
        }
    }

    /// <summary>
    /// Direct counterpart of settlement.room.food.fish.Constructor. The role at every
    /// source coordinate is the Java ss/cc/ms/ww/m1/m2/ml FurnisherItemTile matrix.
    /// </summary>
    private void RenderFish(Node3D root, FurnitureVisualPlacement placement)
    {
        var variants = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group);
        var variant = Math.Clamp(placement.Variant, 0, variants.Count - 1);
        var layout = variants[variant];
        var upgrade = placement.Upgrade > 0 ? 1 : 0;
        foreach (var source in layout.Cells)
        {
            var cell = placement.Origin + layout.RotateCell(source, placement.Rotation);
            var role = FishRole(placement.Group, variant, layout.Width, source.X, source.Z);
            var hash = StableHash(cell.X, cell.Z, placement.Group * 31 + variant);
            var direction = FishDirection(layout, placement.Group, variant, source, role, hash);
            var rotation = (direction + placement.Rotation) & 3;
            switch (role)
            {
                case FishSprite.Storage:
                    AddOneByOne(root, cell, "STORAGE.png", upgrade == 0 ? 5 : 6,
                        hash, 0.13f, 0);
                    if (upgrade > 0)
                        AddOneByOne(root, cell, "STORAGE.png", 7, hash >> 3, 0.145f, 0);
                    break;
                case FishSprite.Candle:
                    AddOneByOne(root, cell, upgrade == 0 ? "NATURE.png" : "TABLES.png",
                        upgrade == 0 ? 13 : 2, hash, 0.13f, 0);
                    // Constructor.renderAbove renders miscTop while no candle occupies the tile.
                    AddOneByOne(root, cell, "NATURE.png", Math.Abs(hash >> 4) % 5,
                        hash >> 2, 0.145f, rotation);
                    break;
                case FishSprite.Misc:
                    var miscRows = upgrade == 0 ? new[] { 0, 1, 2, 3, 4 } : new[] { 0, 1, 5, 10 };
                    AddOneByOne(root, cell, upgrade == 0 ? "NATURE.png" : "STORAGE.png",
                        miscRows[Math.Abs(hash) % miscRows.Length], hash >> 3, 0.13f, rotation);
                    break;
                case FishSprite.Work:
                    AddOneByOne(root, cell, "TABLES.png", upgrade == 0 ? 0 : 6,
                        hash, 0.13f, rotation);
                    break;
                case FishSprite.AuxEdge:
                    AddOneByOne(root, cell, "2xROOF.png", 0, hash, 0.13f, rotation);
                    break;
                case FishSprite.AuxMid:
                    AddOneByOne(root, cell, "2xROOF.png", 1, hash, 0.13f, rotation);
                    break;
                case FishSprite.AuxBig:
                    var origin = FishBigOrigin(layout, placement.Group, variant, source);
                    var dx = source.X - origin.X;
                    var dz = source.Z - origin.Z;
                    var blockHash = StableHash(placement.Origin.X, placement.Origin.Z,
                        placement.Group * 31 + variant);
                    AddTwoByTwo(root, cell, "HUT.png", blockHash, dx, dz,
                        0.13f, placement.Rotation);
                    break;
            }
        }
    }

    private void RenderFishPlaceholder(Node3D root, FurnitureVisualPlacement placement,
        IReadOnlySet<GridCoord> invalidCells)
    {
        var variants = FurnisherLayoutCatalog.Variants(placement.RoomKey, placement.Group);
        var variant = Math.Clamp(placement.Variant, 0, variants.Count - 1);
        var layout = variants[variant];
        var roles = layout.Cells.ToDictionary(source =>
                placement.Origin + layout.RotateCell(source, placement.Rotation),
            source => FishRole(placement.Group, variant, layout.Width, source.X, source.Z));
        foreach (var pair in roles)
        {
            var mask = 0;
            if (pair.Value == FishSprite.AuxBig)
            {
                foreach (var (offset, bit) in CardinalMasks())
                    if (roles.TryGetValue(pair.Key + offset, out var role) && role == FishSprite.AuxBig)
                        mask |= bit;
            }
            else if (pair.Value is FishSprite.AuxEdge or FishSprite.AuxMid)
            {
                var other = pair.Value == FishSprite.AuxEdge ? FishSprite.AuxMid : FishSprite.AuxEdge;
                var found = new List<int>();
                var maximum = Math.Max(layout.Width, layout.Height);
                foreach (var (offset, bit) in CardinalMasks())
                    for (var distance = 1; distance < maximum; distance++)
                        if (roles.TryGetValue(pair.Key + new GridCoord(offset.X * distance,
                                    offset.Z * distance), out var role) && role == other)
                        {
                            found.Add(bit);
                            break;
                        }
                if (found.Count > 0) mask = found[0];
                if (found.Count > 1) mask |= OppositeMask(found[0]);
            }
            AddConstructionMask(root, pair.Key, mask, invalidCells.Contains(pair.Key));
        }
    }

    private static IEnumerable<(GridCoord Offset, int Bit)> CardinalMasks()
    {
        yield return (new GridCoord(0, -1), 1);
        yield return (new GridCoord(1, 0), 2);
        yield return (new GridCoord(0, 1), 4);
        yield return (new GridCoord(-1, 0), 8);
    }

    private static int OppositeMask(int mask) => mask switch
    {
        1 => 4,
        2 => 8,
        4 => 1,
        _ => 2
    };

    private static FishSprite FishRole(int group, int variant, int width, int x, int z)
    {
        if (group == 0)
        {
            if (variant == 0) return z == 0 ? FishSprite.Storage :
                x == 0 ? FishSprite.Candle : FishSprite.Misc;
            if (variant == 1) return x < 2 ? FishSprite.Storage :
                z == 0 ? FishSprite.Misc : FishSprite.Candle;
            return x < width - 1 ? FishSprite.Storage :
                z == 0 ? FishSprite.Candle : FishSprite.Misc;
        }

        return (variant, z, x) switch
        {
            (1, 0, 1) => FishSprite.Candle,
            (2, 0, 1) or (3, 0, 1) or (4, 0, 1) or (5, 0, 1) or
                (6, 0, 1) => FishSprite.Work,
            (2, 0, 2) or (3, 0, 2) or (5, 0, 2) or (6, 0, 2) => FishSprite.Candle,
            (4, 1, 0) => FishSprite.Candle,
            (6, 1, 3) => FishSprite.Work,
            (6, 1, 1) or (6, 1, 2) => FishSprite.AuxEdge,
            (7, 0, 0) or (7, 1, 0) or (7, 1, 3) or
                (8, 0, 0) or (8, 0, 3) or (8, 1, 0) or
                (9, 0, 0) or (9, 0, 3) or (9, 0, 6) or (9, 1, 0) => FishSprite.Work,
            (7, 0, 1) or (7, 0, 2) or (7, 1, 1) or (7, 1, 2) or
                (8, 0, 1) or (8, 0, 2) or (8, 1, 1) or (8, 1, 2) or
                (9, 0, 1) or (9, 0, 2) or (9, 1, 1) or (9, 1, 2) => FishSprite.AuxBig,
            (7, 0, 4) or (8, 0, 5) or (9, 0, 5) => FishSprite.Candle,
            (8, 1, 3) or (8, 1, 4) or (9, 1, 3) or (9, 1, 5) => FishSprite.AuxEdge,
            (9, 1, 4) => FishSprite.AuxMid,
            _ => FishSprite.Misc
        };
    }

    private static GridCoord FishBigOrigin(FurnisherLayout layout, int group, int variant,
        GridCoord source)
    {
        var cells = layout.Cells.Where(value =>
            FishRole(group, variant, layout.Width, value.X, value.Z) == FishSprite.AuxBig).ToArray();
        return cells.Length == 0 ? source : new GridCoord(cells.Min(value => value.X),
            cells.Min(value => value.Z));
    }

    private static int FishDirection(FurnisherLayout layout, int group, int variant,
        GridCoord source, FishSprite role, int hash)
    {
        if (role is FishSprite.AuxEdge or FishSprite.AuxMid)
        {
            var other = role == FishSprite.AuxEdge ? FishSprite.AuxMid : FishSprite.AuxEdge;
            var maximum = Math.Max(layout.Width, layout.Height);
            foreach (var index in Enumerable.Range(0, 4).Select(value => (value + hash) & 3))
            {
                var offset = index switch
                {
                    0 => new GridCoord(0, -1),
                    1 => new GridCoord(1, 0),
                    2 => new GridCoord(0, 1),
                    _ => new GridCoord(-1, 0)
                };
                for (var distance = 1; distance < maximum; distance++)
                {
                    var target = source + new GridCoord(offset.X * distance, offset.Z * distance);
                    if (!layout.Cells.Contains(target)) continue;
                    if (FishRole(group, variant, layout.Width, target.X, target.Z) == other)
                        return index;
                }
            }
            return 0;
        }

        if (role is not (FishSprite.Misc or FishSprite.Work)) return 0;
        var first = hash & 3;
        for (var i = 0; i < 4; i++)
        {
            var direction = (first + i) & 3;
            var d = direction switch
            {
                0 => new GridCoord(0, -1),
                1 => new GridCoord(1, 0),
                2 => new GridCoord(0, 1),
                _ => new GridCoord(-1, 0)
            };
            var neighbour = source + d;
            // Exact joins override from Constructor: a neighbour exists and the tile
            // two steps behind it on x / two steps forward on y does not.
            var probe = new GridCoord(neighbour.X - d.X * 2, neighbour.Z + d.Z * 2);
            if (layout.Cells.Contains(neighbour) && !layout.Cells.Contains(probe)) return direction;
        }
        return 0;
    }

    private void AddOneByOne(Node3D root, GridCoord cell, string source, int row,
        int hash, float elevation, int quarterTurns)
    {
        var texture = GetTexture($"1x1/{source}");
        if (texture is null) return;
        var variations = Math.Max(1, texture.GetWidth() / 2 / 22);
        AddTile(root, cell, $"1x1/{source}", Math.Abs(hash) % variations * 22 + 3,
            row * 22 + 3, elevation, quarterTurns);
    }

    private void AddTwoByTwo(Node3D root, GridCoord cell, string source, int hash,
        int dx, int dz, float elevation, int quarterTurns)
    {
        var texture = GetTexture($"2x2/{source}");
        if (texture is null) return;
        const int bodySize = 44;
        var variations = Math.Max(1, texture.GetWidth() / 2 / bodySize);
        var variation = Math.Abs(hash) % variations;
        AddTile(root, cell, $"2x2/{source}", variation * bodySize + 6 + dx * 16,
            6 + dz * 16, elevation, quarterTurns);
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
        var texture = GetTexture(source);
        if (texture is null) return;
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
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Mesh = new PlaneMesh { Size = Vector2.One, Material = material }
        });
    }

    private Texture2D? GetTexture(string source)
    {
        if (_textures.TryGetValue(source, out var texture)) return texture;
        var path = source.StartsWith("ui/", StringComparison.OrdinalIgnoreCase)
            ? $"res://Data/Original/assets/sprite/{source}"
            : $"{SpriteRoot}/{source}";
        texture = GD.Load<Texture2D>(path);
        if (texture is not null) _textures[source] = texture;
        return texture;
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
