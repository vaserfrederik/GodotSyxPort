using System;
using Godot;
using GodotSyxPort.World;

namespace GodotSyxPort.Rendering;

/// <summary>
/// Builds the strategic terrain from the original 16px world atlases. This keeps the
/// in-game world view visually consistent with world creation instead of replacing it
/// with a one-colour pixel per world tile.
/// </summary>
public static class OriginalWorldMapTextureBuilder
{
    public const int TilePixels = 16;
    private const string Root = "res://Data/Original/assets/sprite/world/map/";
    private static readonly int[] HouseOffsetX =
        { 52, 52, 0, 0, 52, 52, 0, 0, 32, 32, 16, 16, 32, 32, 16, 16 };
    private static readonly int[] HouseOffsetY =
        { 52, 32, 52, 32, 0, 16, 0, 16, 52, 32, 52, 32, 0, 16, 0, 16 };
    // ComposerSources.house2 offsets used by WorldMountain. A raw source house
    // occupies 96x64 pixels and contains all 16 corner-join masks.
    private static readonly int[] MountainOffsetX =
        { 0, 0, 0, 0, 72, 54, 36, 18, 72, 36, 18, 54, 72, 18, 54, 36 };
    private static readonly int[] MountainOffsetY =
        { 0, 20, 40, 0, 40, 20, 20, 40, 20, 0, 20, 0, 0, 0, 40, 40 };
    private static readonly Color[] GroundPalette =
    {
        new(132 / 255f, 128 / 255f, 80 / 255f),
        new(174 / 255f, 154 / 255f, 122 / 255f),
        new(174 / 255f, 154 / 255f, 122 / 255f),
        new(218 / 255f, 190 / 255f, 170 / 255f)
    };

    public static ImageTexture Build(StrategicWorldRuntime world)
    {
        var ground = Load("Ground.png");
        var waterAtlas = Load("Water.png");
        var mountain = Load("Mountain.png");
        var forest = Load("Forest.png");
        var river = Load("RiverBig.png");
        var riverSmall = Load("RiverSmall.png");
        var tiles = StrategicWorldRuntime.TileDimension;
        var size = tiles * TilePixels;
        var pixels = new byte[size * size * 3];
        for (var ty = 0; ty < tiles; ty++)
        for (var tx = 0; tx < tiles; tx++)
        {
            var water = world.Terrain.Water(tx, ty);
            var waterMask = water == StrategicWaterKind.None ? 0 : Mask(tx, ty, tiles,
                (x, y) => world.Terrain.Water(x, y) != StrategicWaterKind.None);
            var riverMask = water is StrategicWaterKind.River or StrategicWaterKind.Delta
                ? Mask(tx, ty, tiles, (x, y) => world.Terrain.Water(x, y) is
                    StrategicWaterKind.River or StrategicWaterKind.Delta)
                : water == StrategicWaterKind.SmallRiver
                    ? Mask(tx, ty, tiles, (x, y) => world.Terrain.Water(x, y) == StrategicWaterKind.SmallRiver)
                    : 0;
            var forceGround = water is StrategicWaterKind.River or StrategicWaterKind.SmallRiver or
                StrategicWaterKind.Delta || water != StrategicWaterKind.None && waterMask != 15;
            var baseColor = GroundColor(world.Terrain, tx, ty, forceGround);
            for (var py = 0; py < TilePixels; py++)
            for (var px = 0; px < TilePixels; px++)
            {
                var color = GroundDetail(ground, baseColor, world, tx, ty, px, py);
                if (water is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake or
                    StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean)
                {
                    if (waterMask != 15)
                        color = Modulate(color, House(waterAtlas, 2, tx, ty, waterMask, px, py),
                            new Color("9a9b82"), 0.52f);
                    var sample = House(waterAtlas, water is StrategicWaterKind.DeepLake or
                        StrategicWaterKind.DeepOcean ? 210 : 100, tx, ty, waterMask, px, py);
                    color = Modulate(color, sample, water is StrategicWaterKind.DeepOcean or
                        StrategicWaterKind.DeepLake ? new Color("344f6d") : new Color("587895"), 0.78f);
                }
                else if (water is StrategicWaterKind.River or StrategicWaterKind.Delta)
                    color = Modulate(color, House(river, 74, tx, ty, riverMask, px, py),
                        new Color("668b9d"), 0.82f);
                else if (water == StrategicWaterKind.SmallRiver)
                    color = Modulate(color, House(riverSmall, 74, tx, ty, riverMask, px, py),
                        new Color("7395a2"), 0.78f);
                if (world.Terrain.Mountain(tx, ty))
                {
                    var height = world.Terrain.MountainHeight(tx, ty);
                    var mask = MountainCornerMask(world.Terrain, tx, ty, height);
                    var sample = MountainTile(mountain, tx, ty, mask, height, px, py);
                    var shade = new Color(80 / 255f, 80 / 255f, 80 / 255f)
                        .Lerp(new Color(210 / 255f, 210 / 255f, 210 / 255f), height / 15f);
                    color = CompositeTinted(color, sample, shade, 0.96f);
                }
                Put(pixels, size, tx * TilePixels + px, ty * TilePixels + py, color);
            }
        }
        AddForest(world, forest, pixels, size);
        return ImageTexture.CreateFromImage(Image.CreateFromData(size, size, false, Image.Format.Rgb8, pixels));
    }

    private static Image? Load(string name) => GD.Load<Texture2D>(Root + name)?.GetImage();

    private static Color GroundColor(StrategicTerrainRuntime terrain, int x, int y, bool forceGround)
    {
        var water = terrain.Water(x, y);
        if (!forceGround && water is (StrategicWaterKind.DeepOcean or StrategicWaterKind.DeepLake))
            return new Color("334e6c");
        if (!forceGround && water is (StrategicWaterKind.Ocean or StrategicWaterKind.Lake))
            return new Color("506d8b");
        // WorldGround.java does not paint one generic grass colour. GeneratorSeasoner
        // selects one of nine ground bands and WorldGround interpolates Ground.txt's
        // wet -> dry palette. Keep those broad meadow/steppe/desert patches visible;
        // forest and mountain sprites are composited later and must not flatten them.
        var dry = 1f - (float)terrain.Moisture(x, y);
        var palettePosition = Math.Clamp(dry, 0f, 1f) * 3f;
        var paletteIndex = Math.Min(2, (int)palettePosition);
        var paletteMix = palettePosition - paletteIndex;
        var color = GroundPalette[paletteIndex].Lerp(GroundPalette[paletteIndex + 1], paletteMix);
        if (terrain.Climate(x, y) == "COLD")
            color = color.Lerp(new Color(90 / 255f, 105 / 255f, 127 / 255f), 0.18f);
        return color;
    }

    private static Color GroundDetail(Image? atlas, Color tint, StrategicWorldRuntime world,
        int tx, int ty, int px, int py)
    {
        if (atlas is null) return tint;
        var origin = world.Terrain.IsWater(tx, ty) ? new Vector2I(146, 216) :
            world.Terrain.Mountain(tx, ty) ? new Vector2I(6, 216) :
            world.Terrain.Forest(tx, ty) > 0.55 ? new Vector2I(6, 356) : new Vector2I(6, 76);
        var sample = Pixel(atlas, origin.X + (tx & 7) * 16 + px, origin.Y + (ty & 7) * 16 + py);
        var luminance = sample.R * 0.299f + sample.G * 0.587f + sample.B * 0.114f;
        var factor = Mathf.Lerp(1f, 0.58f + luminance * 0.58f, sample.A);
        return new Color(Math.Clamp(tint.R * factor, 0, 1), Math.Clamp(tint.G * factor, 0, 1),
            Math.Clamp(tint.B * factor, 0, 1));
    }

    private static void AddForest(StrategicWorldRuntime world, Image? atlas, byte[] pixels, int size)
    {
        if (atlas is null) return;
        for (var ty = 0; ty < StrategicWorldRuntime.TileDimension; ty++)
        for (var tx = 0; tx < StrategicWorldRuntime.TileDimension; tx++)
        {
            var amount = world.Terrain.Forest(tx, ty);
            if (amount <= 0 || world.Terrain.IsWater(tx, ty)) continue;
            var level = Math.Clamp((int)Math.Ceiling(amount * 3), 1, 3);
            var sx0 = 4 + Variant(tx, ty, 16) * 30;
            var sy0 = 4 + (level + 2) * 31;
            var foliage = world.Terrain.Climate(tx, ty) == "COLD" ? new Color("53614d") :
                world.Terrain.Climate(tx, ty) == "HOT" ? new Color("59633a") : new Color("315425");
            for (var sy = 0; sy < 24; sy++)
            for (var sx = 0; sx < 24; sx++)
            {
                var dx = tx * TilePixels - 4 + sx; var dy = ty * TilePixels - 4 + sy;
                if ((uint)dx >= (uint)size || (uint)dy >= (uint)size) continue;
                var sample = Pixel(atlas, sx0 + sx, sy0 + sy);
                if (sample.A <= 0) continue;
                var lum = Math.Clamp(sample.R * 0.299f + sample.G * 0.587f + sample.B * 0.114f, 0.12f, 1f);
                Blend(pixels, size, dx, dy, new Color(foliage.R * lum, foliage.G * lum,
                    foliage.B * lum, sample.A), Math.Clamp(0.62f + (float)amount * 0.34f, 0, 0.96f));
            }
        }
    }

    private static Color House(Image? atlas, int row, int tx, int ty, int mask, int px, int py)
    {
        if (atlas is null) return Colors.Transparent;
        var runtime = Variant(tx, ty, 8); var rotated = (runtime & 1) != 0;
        var sourceMask = rotated ? (mask >> 1) | ((mask & 1) << 3) : mask;
        var sourceX = rotated ? py : px; var sourceY = rotated ? TilePixels - 1 - px : py;
        return Pixel(atlas, 2 + (runtime >> 1) * 72 + HouseOffsetX[sourceMask] + sourceX,
            row + HouseOffsetY[sourceMask] + sourceY);
    }

    private static Color MountainTile(Image? atlas, int tx, int ty, int mask,
        int height, int px, int py)
    {
        if (atlas is null) return Colors.Transparent;
        var variation = Variant(tx, ty, height <= 1 ? 2 : 3);
        var baseRow = height <= 1 ? 0 : 3;
        var lower = MountainSourcePixel(atlas, baseRow + variation, mask, px, py);
        if (height <= 1) return lower;
        // WorldMountain renders FULLS and then TOPS for raised cells.
        var upper = MountainSourcePixel(atlas, 6 + variation, mask, px, py);
        return upper.A > 0.04f ? upper : lower;
    }

    private static Color MountainSourcePixel(Image atlas, int variant, int mask, int px, int py) =>
        // ComposerSources.house2 surrounds every 16px payload with a four-pixel
        // authoring guide. Sampling from the house origin made that guide appear as
        // a square grid over mountain ranges.
        Pixel(atlas, variant % 3 * 96 + MountainOffsetX[mask] + 4 + px,
            variant / 3 * 64 + MountainOffsetY[mask] + 4 + py);

    private static int MountainCornerMask(StrategicTerrainRuntime terrain, int x, int y, int height)
    {
        var mask = 0;
        // DIR.NORTHO mask order from the Java source: NE=1, SE=2, SW=4, NW=8.
        if (MountainCorner(terrain, x, y, 1, -1, height)) mask |= 1;
        if (MountainCorner(terrain, x, y, 1, 1, height)) mask |= 2;
        if (MountainCorner(terrain, x, y, -1, 1, height)) mask |= 4;
        if (MountainCorner(terrain, x, y, -1, -1, height)) mask |= 8;
        return mask;
    }

    private static bool MountainCorner(StrategicTerrainRuntime terrain, int x, int y,
        int dx, int dy, int height)
    {
        var threshold = Math.Max(1, height - 1);
        return terrain.Mountain(x + dx, y) && terrain.Mountain(x, y + dy) &&
               terrain.Mountain(x + dx, y + dy) &&
               terrain.MountainHeight(x + dx, y) >= threshold &&
               terrain.MountainHeight(x, y + dy) >= threshold &&
               terrain.MountainHeight(x + dx, y + dy) >= threshold;
    }

    private static int Mask(int x, int y, int size, Func<int, int, bool> same)
    {
        var result = 0;
        if (y > 0 && same(x, y - 1)) result |= 1;
        if (x + 1 < size && same(x + 1, y)) result |= 2;
        if (y + 1 < size && same(x, y + 1)) result |= 4;
        if (x > 0 && same(x - 1, y)) result |= 8;
        return result;
    }

    private static Color Pixel(Image? image, int x, int y)
    {
        if (image is null || (uint)x >= (uint)image.GetWidth() || (uint)y >= (uint)image.GetHeight())
            return Colors.Transparent;
        var sample = image.GetPixel(x, y);
        var guide = sample.R > 0.88f && sample.G < 0.18f && sample.B < 0.18f ||
            sample.G > 0.88f && sample.R < 0.18f && sample.B < 0.18f ||
            sample.R < 0.08f && sample.G < 0.16f && sample.B > 0.34f;
        return guide ? Colors.Transparent : sample;
    }

    private static Color Modulate(Color current, Color mask, Color layer, float strength)
    {
        if (mask.A <= 0) return current;
        var lum = Math.Clamp(mask.R * 0.299f + mask.G * 0.587f + mask.B * 0.114f, 0, 1);
        return current.Lerp(layer, lum * strength * mask.A);
    }

    private static Color CompositeTinted(Color current, Color sprite, Color tint, float strength)
    {
        if (sprite.A <= 0) return current;
        // Java binds the height colour and then renders the diffuse sprite. Dark
        // crevices therefore stay dark; treating luminance as opacity erased them.
        var source = new Color(sprite.R * tint.R, sprite.G * tint.G,
            sprite.B * tint.B, sprite.A);
        return current.Lerp(source, sprite.A * strength);
    }

    private static void Put(byte[] pixels, int size, int x, int y, Color color)
    {
        var offset = (x + y * size) * 3;
        pixels[offset] = (byte)color.R8; pixels[offset + 1] = (byte)color.G8; pixels[offset + 2] = (byte)color.B8;
    }

    private static void Blend(byte[] pixels, int size, int x, int y, Color source, float strength)
    {
        var offset = (x + y * size) * 3; var alpha = Math.Clamp(source.A * strength, 0, 1);
        pixels[offset] = (byte)Math.Clamp(pixels[offset] * (1 - alpha) + source.R8 * alpha, 0, 255);
        pixels[offset + 1] = (byte)Math.Clamp(pixels[offset + 1] * (1 - alpha) + source.G8 * alpha, 0, 255);
        pixels[offset + 2] = (byte)Math.Clamp(pixels[offset + 2] * (1 - alpha) + source.B8 * alpha, 0, 255);
    }

    private static int Variant(int x, int y, int count) =>
        (int)((uint)(x * 73856093 ^ y * 19349663) % (uint)count);
}
