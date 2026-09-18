using System;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Rendering;

/// <summary>
/// CPU composer for the original settlement atlases. Songs of Syx composes several
/// variants, masks and 1x1/2x2/3x3 sprites per logical tile; reproducing that into one
/// 4-pixel-per-tile texture retains the source look without 589,824 scene nodes.
/// </summary>
public static class OriginalSettlementTerrainTextureBuilder
{
    private sealed class Atlas
    {
        public int Width { get; }
        public int Height { get; }
        private readonly byte[] _pixels;

        public Atlas(Image image)
        {
            image.Convert(Image.Format.Rgba8);
            Width = image.GetWidth();
            Height = image.GetHeight();
            _pixels = image.GetData();
        }

        public Color Pixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Width - 1); y = Math.Clamp(y, 0, Height - 1);
            var offset = (y * Width + x) * 4;
            return new Color(_pixels[offset] / 255f, _pixels[offset + 1] / 255f,
                _pixels[offset + 2] / 255f, _pixels[offset + 3] / 255f);
        }
    }

    public const int PixelsPerTile = 4;
    private const string MapRoot = "res://Data/Original/assets/sprite/settlement/map/";
    private const string TextureRoot = "res://Data/Original/assets/sprite/textures/";

    public static Image Build(WorldGridData data, int seed, ClimateRule? climate = null,
        double ice = 0, double weatherMoisture = 0.75)
    {
        var ground = Load(MapRoot + "Ground.png");
        var mountain = Load(MapRoot + "Mountain.png");
        var waterStencil = Load(MapRoot + "Water.png");
        var tree = Load(MapRoot + "Tree.png");
        var rock = Load(MapRoot + "Rock.png");
        var water = Load(TextureRoot + "Water.png");
        var width = data.Width * PixelsPerTile;
        var height = data.Height * PixelsPerTile;
        var pixels = new byte[width * height * 4];

        for (var z = 0; z < data.Height; z++)
        for (var x = 0; x < data.Width; x++)
        {
            var cell = new GridCoord(x, z);
            var variant = Hash(x, z, seed) & 63;
            var mountainMask = NeighborMask(data, cell, TileFlags.Mountain);
            var waterMask = NeighborMask(data, cell, TileFlags.Water);
            for (var py = 0; py < PixelsPerTile; py++)
            for (var px = 0; px < PixelsPerTile; px++)
            {
                // TWater.Sprites uses ComposerSources.house (four variants, sixteen
                // orthogonal masks).  Sampling that stencil is essential: blending a
                // boolean water field produces square shores and discards the source
                // atlas' edge segments.
                var waterCoverage = data.Has(cell, TileFlags.Water)
                    ? WaterStencilCoverage(waterStencil, waterMask, variant, px, py)
                    : 0f;
                var mountainCoverage = SmoothFlag(data, x, z, px, py, TileFlags.Mountain);
                Color color;
                if (waterCoverage > 0.46f)
                {
                    var sample = water.Pixel(
                        6 + PositiveMod(x * PixelsPerTile + px, 128),
                        6 + PositiveMod(z * PixelsPerTile + py, 128));
                    var deep = data.Has(cell, TileFlags.DeepWater);
                    var tint = new Color(60 / 255f, 150 / 255f, 200 / 255f).Lerp(
                        new Color(60 / 255f, 140 / 255f, 160 / 255f),
                        (float)Math.Clamp(ice, 0, 1));
                    if (deep) tint = tint.Darkened(0.25f);
                    color = TextureTint(sample, tint, 1f);
                    var iceLimit = Math.Min(0xffff,
                        (int)(Math.Clamp(ice, 0, 1) * 0x1ffff));
                    if ((Hash(x, z, seed ^ 0x1ce5) & 0xffff) < iceLimit)
                        color = AlphaOver(color,
                            IcePixel(waterStencil, waterMask, variant, px, py));
                }
                else if (mountainCoverage > 0.44f)
                {
                    var baseSample = GroundSample(ground, GroundKind.Mountain, variant, px, py);
                    color = TextureTint(baseSample, Colors.White, 1f);
                    // ComposerSources.house2 does not store the 16 masks in a row.  It
                    // uses a 5x3 house layout with 2/4-pixel gutters.  Reading it as a
                    // linear strip sampled the magenta/cyan authoring guides (and, for
                    // the later masks, the normal-map half of Mountain.png).
                    var overlay = MountainPixel(mountain, mountainMask, variant, px, py);
                    color = AlphaOver(color, overlay);
                }
                else
                {
                    var kind = data.Ground(cell);
                    // Water is a terrain layer over ground in Java. The compact C#
                    // storage currently has no separate under-ground slot, so soil is
                    // the correct neutral backing; forcing sand created the yellow
                    // one-pixel halo visible around every fresh-water shore.
                    if (kind is GroundKind.FreshWater or GroundKind.SaltWater or GroundKind.Mountain)
                        kind = GroundKind.Soil;
                    var sample = GroundSample(ground, kind, variant, px, py);
                    color = TextureTint(sample,
                        GroundTint(data, cell, kind, climate, weatherMoisture), 1f);
                }
                Write(pixels, width, x * PixelsPerTile + px, z * PixelsPerTile + py, color);
            }
        }

        ComposeMountainRocks(data, rock, pixels, width, seed);
        ComposeVegetation(data, tree, pixels, width, seed);
        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, pixels);
    }

    private static void ComposeVegetation(
        WorldGridData data, Atlas tree, byte[] pixels, int pixelWidth, int seed)
    {
        var occupied = new bool[data.Width * data.Height];
        for (var size = 3; size >= 1; size--)
        {
            var minimum = size == 3 ? 9 : size == 2 ? 6 : 3;
            for (var z = 0; z <= data.Height - size; z++)
            for (var x = 0; x <= data.Width - size; x++)
            {
                var valid = true;
                var total = 0;
                for (var dz = 0; dz < size && valid; dz++)
                for (var dx = 0; dx < size; dx++)
                {
                    var cell = new GridCoord(x + dx, z + dz);
                    var index = cell.Z * data.Width + cell.X;
                    if (occupied[index] || data.Has(cell, TileFlags.Water | TileFlags.Mountain) ||
                        data.VegetationAmount(cell) < minimum) { valid = false; break; }
                    total += data.VegetationAmount(cell);
                }
                if (!valid || (size == 1 && (Hash(x, z, seed ^ 0x5271) & 15) >= total)) continue;
                for (var dz = 0; dz < size; dz++)
                for (var dx = 0; dx < size; dx++)
                    occupied[(z + dz) * data.Width + x + dx] = true;

                var variant = Math.Abs(Hash(x, z, seed ^ 0x194f));
                int sourceX, sourceY, sourceSize;
                if (size == 3)
                {
                    // combo(6,3,3): 48px sprite in a 60px cell, 6px source margin.
                    sourceX = 6 + variant % 6 * 60;
                    sourceY = 122 + variant / 6 % 3 * 60;
                    sourceSize = 48;
                }
                else if (size == 2)
                {
                    // combo(8,2,2), immediately below the 28px singles block.
                    sourceX = 6 + variant % 8 * 44;
                    sourceY = 34 + variant / 8 % 2 * 44;
                    sourceSize = 32;
                }
                else
                {
                    // singles(16,1): 16px sprites separated by the source 6px gutter.
                    sourceX = 6 + variant % 16 * 22;
                    sourceY = 6;
                    sourceSize = 16;
                }
                BlendSprite(tree, sourceX, sourceY, sourceSize,
                    pixels, pixelWidth, x * PixelsPerTile, z * PixelsPerTile,
                    size * PixelsPerTile);
            }
        }
    }

    private static void ComposeMountainRocks(
        WorldGridData data, Atlas rock, byte[] pixels, int pixelWidth, int seed)
    {
        for (var z = 0; z < data.Height; z++)
        for (var x = 0; x < data.Width; x++)
        {
            var cell = new GridCoord(x, z);
            var mountainEdge = data.Has(cell, TileFlags.Mountain) &&
                               NeighborMask(data, cell, TileFlags.Mountain) != 15;
            var mineral = data.MineralAmount(cell) > 0;
            if (!mineral && (!mountainEdge || (Hash(x, z, seed ^ 0x7351) & 3) != 0)) continue;
            var variant = Math.Abs(Hash(x, z, seed ^ 0x315d)) & 127;
            // ComposerSources.singles has a 6px leading margin and 6px gutters.
            BlendSprite(rock, 6 + variant % 16 * 22, 6 + variant / 16 * 22, 16,
                pixels, pixelWidth, x * PixelsPerTile, z * PixelsPerTile, PixelsPerTile);
        }
    }

    private static Color GroundSample(Atlas atlas, GroundKind kind, int variant, int px, int py)
    {
        var row = kind switch
        {
            GroundKind.Mountain => 1,
            GroundKind.Sand => 2,
            GroundKind.Forest => 3,
            GroundKind.Pasture => 4,
            GroundKind.Infertile => 5,
            _ => 0
        };
        // GroundTypes.java composes six 16x4 Full blocks.  Every block is 76px
        // high (64px content + 6px margins), not 72px.  The diffuse half starts
        // at the Full source margin x=6/y=34; sample the centre of each 4px bin.
        var sourceX = 8 + variant % 16 * 16 + px * 4;
        var sourceY = 36 + row * 76 + variant / 16 * 16 + py * 4;
        return SafePixel(atlas, sourceX, sourceY);
    }

    private static Color MountainPixel(Atlas atlas, int mask, int variant, int px, int py)
    {
        ReadOnlySpan<int> offsetsX = stackalloc int[]
        {
            0, 0, 0, 0, 72, 54, 36, 18, 72, 36, 18, 54, 72, 18, 54, 36
        };
        ReadOnlySpan<int> offsetsY = stackalloc int[]
        {
            0, 20, 40, 0, 40, 20, 20, 40, 20, 0, 20, 0, 0, 0, 40, 40
        };
        mask &= 15;
        // TMountain.java loads three 96x64 house2 variants.  Its destination
        // repeats variants 1 and 2; use terrain variation to retain that mix.
        var set = variant % 5 switch { 0 => 0, 1 or 2 => 1, _ => 2 };
        return SafePixel(atlas,
            set * 96 + 4 + offsetsX[mask] + px * 4 + 2,
            4 + offsetsY[mask] + py * 4 + 2);
    }

    private static float WaterStencilCoverage(
        Atlas atlas, int mask, int variant, int px, int py)
    {
        // ComposerSources.house with a 16px destination has a 72x72 body and a
        // two-pixel source margin. These are the exact mask offsets from
        // ComposerSources.House, used by TWater.Sprites.stencil.
        ReadOnlySpan<int> offsetsX = stackalloc int[]
        {
            52, 52, 0, 0, 52, 52, 0, 0, 32, 32, 16, 16, 32, 32, 16, 16
        };
        ReadOnlySpan<int> offsetsY = stackalloc int[]
        {
            52, 32, 52, 32, 0, 16, 0, 16, 52, 32, 52, 32, 0, 16, 0, 16
        };
        mask &= 15;
        var source = SafePixel(atlas,
            (variant & 3) * 72 + 2 + offsetsX[mask] + px * 4 + 2,
            2 + offsetsY[mask] + py * 4 + 2);
        return Math.Clamp(source.R * 0.299f + source.G * 0.587f + source.B * 0.114f, 0f, 1f);
    }

    private static Color IcePixel(Atlas atlas, int mask, int variant, int px, int py)
    {
        ReadOnlySpan<int> offsetsX = stackalloc int[]
        {
            52, 52, 0, 0, 52, 52, 0, 0, 32, 32, 16, 16, 32, 32, 16, 16
        };
        ReadOnlySpan<int> offsetsY = stackalloc int[]
        {
            52, 32, 52, 32, 0, 16, 0, 16, 52, 32, 52, 32, 0, 16, 0, 16
        };
        mask &= 15;
        // TWater.Sprites composes four ice houses after the animation/full rows.
        return SafePixel(atlas,
            (variant & 3) * 72 + 2 + offsetsX[mask] + px * 4 + 2,
            258 + offsetsY[mask] + py * 4 + 2);
    }

    private static Color GroundTint(WorldGridData data, GridCoord cell, GroundKind kind,
        ClimateRule? climate, double weatherMoisture)
    {
        var moisture = Math.Clamp(data.Moisture(cell) / 15.0 + weatherMoisture * 0.4, 0, 1);
        if (kind == GroundKind.Sand)
            return new Color(208 / 255f, 194 / 255f, 142 / 255f).Lerp(
                new Color(150 / 255f, 126 / 255f, 102 / 255f), (float)moisture);
        if (kind == GroundKind.Mountain) return Colors.White;
        var dry = climate?.GroundDry ?? new Color(193 / 255f, 181 / 255f, 135 / 255f);
        var wet = climate?.GroundWet ?? new Color(85 / 255f, 52 / 255f, 52 / 255f);
        return dry.Lerp(wet, (float)moisture);
    }

    private static float SmoothFlag(
        WorldGridData data, int x, int z, int px, int py, TileFlags flag)
    {
        var u = (px + 0.5f) / PixelsPerTile;
        var v = (py + 0.5f) / PixelsPerTile;
        var left = u < 0.5f ? x - 1 : x;
        var top = v < 0.5f ? z - 1 : z;
        var tx = u < 0.5f ? u + 0.5f : u - 0.5f;
        var tz = v < 0.5f ? v + 0.5f : v - 0.5f;
        float V(int sx, int sz) => data.Has(new GridCoord(sx, sz), flag) ? 1f : 0f;
        var a = Mathf.Lerp(V(left, top), V(left + 1, top), tx);
        var b = Mathf.Lerp(V(left, top + 1), V(left + 1, top + 1), tx);
        return Mathf.Lerp(a, b, tz);
    }

    private static int NeighborMask(WorldGridData data, GridCoord cell, TileFlags flag)
    {
        var mask = 0;
        if (data.Has(cell + new GridCoord(0, -1), flag)) mask |= 1;
        if (data.Has(cell + new GridCoord(1, 0), flag)) mask |= 2;
        if (data.Has(cell + new GridCoord(0, 1), flag)) mask |= 4;
        if (data.Has(cell + new GridCoord(-1, 0), flag)) mask |= 8;
        return mask;
    }

    private static void BlendSprite(
        Atlas source, int sourceX, int sourceY, int sourceSize,
        byte[] destination, int destinationWidth, int destinationX, int destinationY,
        int destinationSize)
    {
        for (var y = 0; y < destinationSize; y++)
        for (var x = 0; x < destinationSize; x++)
        {
            var sx = sourceX + Math.Clamp((int)((x + 0.5) * sourceSize / destinationSize), 0, sourceSize - 1);
            var sy = sourceY + Math.Clamp((int)((y + 0.5) * sourceSize / destinationSize), 0, sourceSize - 1);
            var overlay = SafePixel(source, sx, sy);
            if (overlay.A < 0.08f) continue;
            var offset = ((destinationY + y) * destinationWidth + destinationX + x) * 4;
            var under = new Color(destination[offset] / 255f, destination[offset + 1] / 255f,
                destination[offset + 2] / 255f, 1);
            Write(destination, destinationWidth, destinationX + x, destinationY + y,
                AlphaOver(under, overlay));
        }
    }

    private static Color TextureTint(Color texture, Color tint, float strength)
    {
        var luminance = texture.R * 0.299f + texture.G * 0.587f + texture.B * 0.114f;
        var shade = Mathf.Lerp(0.58f, 1.18f, luminance);
        var textured = new Color(tint.R * shade, tint.G * shade, tint.B * shade, 1);
        return tint.Lerp(textured, strength);
    }

    private static Color AlphaOver(Color under, Color over)
    {
        var alpha = Math.Clamp(over.A, 0f, 1f);
        return new Color(Mathf.Lerp(under.R, over.R, alpha),
            Mathf.Lerp(under.G, over.G, alpha), Mathf.Lerp(under.B, over.B, alpha), 1);
    }

    private static Atlas Load(string resourcePath)
    {
        var path = ProjectSettings.GlobalizePath(resourcePath);
        var image = Image.LoadFromFile(path);
        if (image.IsEmpty()) throw new InvalidOperationException($"Cannot load original sprite atlas: {path}");
        return new Atlas(image);
    }

    private static Color SafePixel(Atlas image, int x, int y) => image.Pixel(x, y);

    private static void Write(byte[] pixels, int width, int x, int y, Color color)
    {
        var offset = (y * width + x) * 4;
        pixels[offset] = (byte)Math.Clamp((int)Math.Round(color.R * 255), 0, 255);
        pixels[offset + 1] = (byte)Math.Clamp((int)Math.Round(color.G * 255), 0, 255);
        pixels[offset + 2] = (byte)Math.Clamp((int)Math.Round(color.B * 255), 0, 255);
        pixels[offset + 3] = 255;
    }

    private static int PositiveMod(int value, int divisor) => (value % divisor + divisor) % divisor;

    private static int Hash(int x, int z, int seed)
    {
        unchecked
        {
            uint value = (uint)(x * 0x1f123bb5 ^ z * 0x5f356495 ^ seed);
            value ^= value >> 16; value *= 0x7feb352d; value ^= value >> 15;
            value *= 0x846ca68b; value ^= value >> 16;
            return (int)(value & 0x7fffffff);
        }
    }
}
