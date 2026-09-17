using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace GodotSyxPort.UI;

/// <summary>Reads the padded 32px icon atlases shipped with the source game data.</summary>
public static class OriginalUiIcons
{
    private const string AtlasRoot = "res://Data/Original/assets/sprite/icon/32";
    private const string SmallRoot = "res://Data/Original/assets/sprite/icon/16";
    private const string MediumRoot = "res://Data/Original/assets/sprite/icon/24";
    private const string ResourceRoot = "res://Data/Original/assets/sprite/icon/24/resource";
    private static readonly Dictionary<string, Texture2D?> Cache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Regex IconLine = new(
        @"(?m)^\s*ICON\s*:\s*32->(?<sheet>[A-Za-z0-9_/-]+)->(?<index>\d+)\s*,?",
        RegexOptions.Compiled);
    private static readonly Regex IconBlock = new(
        @"(?ms)^\s*ICON\s*:\s*\{(?<body>.*?)^\s*\}\s*,?",
        RegexOptions.Compiled);
    private static readonly Regex IconLayer = new(
        @"(?m)^\s*(?<layer>BG|FG)\s*:\s*(?<size>\d+)->(?<sheet>.+?)->(?<index>\d+)\s*,?\s*$",
        RegexOptions.Compiled);

    public static Texture2D? MainCategory(int index) => FromAtlas("_UI", index);
    public static Texture2D? Category(int index) => FromAtlas("_ICONS", index);
    public static Texture2D? Small(int index) => FromPaddedAtlas(SmallRoot, "_Icons", index, 16);
    public static Texture2D? Medium(int index) => FromPaddedAtlas(MediumRoot, "_Icons", index, 24);
    public static Texture2D? MediumWithBadge(int baseIndex, int badgeIndex)
    {
        var cacheKey = $"medium-badge:{baseIndex}:{badgeIndex}";
        if (Cache.TryGetValue(cacheKey, out var cached)) return cached;
        var baseImage = Medium(baseIndex)?.GetImage();
        var badgeImage = Medium(badgeIndex)?.GetImage();
        if (baseImage is null) return Medium(badgeIndex);
        var canvas = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        canvas.Fill(Colors.Transparent);
        canvas.BlendRect(baseImage, new Rect2I(Vector2I.Zero, baseImage.GetSize()), new Vector2I(4, 4));
        if (badgeImage is not null)
        {
            badgeImage.Resize(13, 13, Image.Interpolation.Nearest);
            canvas.BlendRect(badgeImage, new Rect2I(Vector2I.Zero, badgeImage.GetSize()), new Vector2I(18, 18));
        }
        var icon = ImageTexture.CreateFromImage(canvas);
        Cache[cacheKey] = icon;
        return icon;
    }
    public static Texture2D? Resource(string sheet) =>
        FromPaddedAtlas(ResourceRoot, sheet, 0, 24);

    public static Texture2D? PlayerTitle(int index)
    {
        var cacheKey = $"player-title:{index}";
        if (Cache.TryGetValue(cacheKey, out var cached)) return cached;
        const string path = "res://Data/Original/assets/sprite/ui/Titles.png";
        if (!Godot.FileAccess.FileExists(path)) return null;
        var atlas = GD.Load<Texture2D>(path);
        if (atlas is null || index < 0 || index >= 35) return null;
        var icon = new AtlasTexture
        {
            Atlas = atlas,
            // IconMaker: five 48x32 compositions in each row, with six-pixel padding.
            Region = new Rect2(6 + index % 5 * 54, 6 + index / 5 * 38, 48, 32)
        };
        Cache[cacheKey] = icon;
        return icon;
    }

    public static Texture2D? Room(string roomKey)
    {
        var cacheKey = $"room:{roomKey}";
        if (Cache.TryGetValue(cacheKey, out var cached)) return cached;
        var definitionPath = $"res://Data/Original/init/room/{roomKey}.txt";
        var source = Godot.FileAccess.GetFileAsString(definitionPath);
        var match = IconLine.Match(source);
        if (match.Success)
        {
            var sheet = match.Groups["sheet"].Value;
            var slash = sheet.LastIndexOf('/');
            if (slash >= 0) sheet = sheet[(slash + 1)..];
            var atlasIcon = FromAtlas(sheet, int.Parse(match.Groups["index"].Value));
            Cache[cacheKey] = atlasIcon;
            return atlasIcon;
        }

        var block = IconBlock.Match(source);
        var icon = block.Success ? Compose(block.Groups["body"].Value) : null;
        Cache[cacheKey] = icon;
        return icon;
    }

    public static Texture2D? FromAtlas(string sheet, int index)
        => FromPaddedAtlas(AtlasRoot, sheet, index, 32);

    private static Texture2D? FromPaddedAtlas(string root, string sheet, int index, int size)
    {
        var cacheKey = $"atlas:{size}:{sheet}:{index}";
        if (Cache.TryGetValue(cacheKey, out var cached)) return cached;
        var path = $"{root}/{sheet}.png";
        // ResourceLoader prints two engine errors for every missing dynamic path.
        // Dynamic source-data lookups are therefore checked before loading; a missing
        // optional icon remains a null icon without polluting the runtime console.
        if (!Godot.FileAccess.FileExists(path))
        {
            Cache[cacheKey] = null;
            return null;
        }
        var atlas = GD.Load<Texture2D>(path);
        if (atlas is null)
        {
            Cache[cacheKey] = null;
            return null;
        }
        var stride = size + 6;
        var columns = Math.Max(1, (atlas.GetWidth() / 2 - 6) / stride);
        var x = 6 + index % columns * stride;
        var y = 6 + index / columns * stride;
        if (x + size > atlas.GetWidth() / 2 || y + size > atlas.GetHeight())
        {
            Cache[cacheKey] = null;
            return null;
        }
        var icon = new AtlasTexture
        {
            Atlas = atlas,
            Region = new Rect2(x, y, size, size)
        };
        Cache[cacheKey] = icon;
        return icon;
    }

    private static Texture2D? Compose(string body)
    {
        var canvas = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        canvas.Fill(Colors.Transparent);
        var drewLayer = false;
        foreach (Match layer in IconLayer.Matches(body))
        {
            var size = int.Parse(layer.Groups["size"].Value);
            var sheet = layer.Groups["sheet"].Value.Replace("->", "/");
            var index = int.Parse(layer.Groups["index"].Value);
            var source = ExtractImage(size, sheet, index);
            if (source is null) continue;
            var destination = new Vector2I((32 - size) / 2, (32 - size) / 2);
            canvas.BlitRect(source, new Rect2I(Vector2I.Zero, source.GetSize()), destination);
            drewLayer = true;
        }
        return drewLayer ? ImageTexture.CreateFromImage(canvas) : null;
    }

    private static Image? ExtractImage(int size, string sheet, int index)
    {
        var root = size switch
        {
            16 => SmallRoot,
            24 => "res://Data/Original/assets/sprite/icon/24",
            32 => AtlasRoot,
            _ => ""
        };
        if (root.Length == 0) return null;
        var sourceSize = size;
        var path = $"{root}/{sheet}.png";
        var bytes = Godot.FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0 && sheet.Contains('/'))
            bytes = Godot.FileAccess.GetFileAsBytes($"{root}/{sheet[(sheet.LastIndexOf('/') + 1)..]}.png");
        if (bytes.Length == 0 && size == 24)
        {
            sourceSize = 32;
            var baseName = sheet[(sheet.LastIndexOf('/') + 1)..];
            bytes = Godot.FileAccess.GetFileAsBytes($"{AtlasRoot}/{baseName}.png");
        }
        if (bytes.Length == 0) return null;
        var atlas = new Image();
        if (atlas.LoadPngFromBuffer(bytes) != Error.Ok) return null;
        var stride = sourceSize + 6;
        var columns = Math.Max(1, (atlas.GetWidth() / 2 - 6) / stride);
        var x = 6 + index % columns * stride;
        var y = 6 + index / columns * stride;
        if (x + sourceSize > atlas.GetWidth() / 2 || y + sourceSize > atlas.GetHeight()) return null;
        var result = atlas.GetRegion(new Rect2I(x, y, sourceSize, sourceSize));
        if (sourceSize != size) result.Resize(size, size, Image.Interpolation.Nearest);
        return result;
    }
}
