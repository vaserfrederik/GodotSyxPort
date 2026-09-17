using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.UI;

public enum SettlementMapLayer : byte
{
    Terrain,
    Infrastructure,
    Fertility,
    Moisture,
    Minerals,
    Edibles,
    Foundation,
    Maintenance,
    RoomProblems,
    Workload,
    Unemployment,
    Homeless,
    Noise,
    Light,
    Space,
    Urbanisation,
    Guard,
    Punishment,
    FreshWater,
    SaltWater
}

/// <summary>
/// Godot-native overview of the complete 768×768 settlement. The sampled map is
/// cached as an ImageTexture and rebuilt only when its layer or WorldGridData revision
/// changes; camera/selection markers remain cheap vector draws.
/// </summary>
public sealed partial class GlobalMapOverlay : Control
{
    private const int TexturePixels = 300;
    private const float Margin = 12f;
    private static readonly SettlementMapLayer[] Layers = Enum.GetValues<SettlementMapLayer>();
    private static readonly StyleBoxFlat Panel = new()
    {
        BgColor = new Color(0.025f, 0.035f, 0.045f, 0.96f),
        BorderColor = new Color(0.28f, 0.38f, 0.42f, 0.9f),
        BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1
    };

    private WorldGridData _data = null!;
    private RoomSystem _rooms = null!;
    private CitizenSystem _citizens = null!;
    private ImageTexture? _texture;
    private GridCoord _cameraCell;
    private GridCoord? _selectedCell;
    private ulong _renderedRevision = ulong.MaxValue;
    private double _revisionCheckLeft;
    private Dictionary<int, RoomRecord> _roomByCell = new();
    private HashSet<int> _homeless = new();
    private HashSet<int> _unemployed = new();
    private byte[]? _environment;

    public bool Expanded { get; private set; }
    public SettlementMapLayer Layer { get; private set; }
    public event Action<GridCoord>? CellSelected;
    public event Action<SettlementMapLayer>? LayerChanged;
    public event Action<SettlementMapLayer>? LayerRefreshed;
    public static IReadOnlyList<SettlementMapLayer> AvailableLayers => Layers;

    public void Initialize(WorldGridData data, RoomSystem rooms, CitizenSystem citizens)
    {
        _data = data;
        _rooms = rooms;
        _citizens = citizens;
        MouseFilter = MouseFilterEnum.Stop;
        SetCompactLayout();
        RebuildTexture(false);
    }

    public void SetCameraCell(GridCoord cell)
    {
        if (_cameraCell == cell) return;
        _cameraCell = cell;
        QueueRedraw();
    }

    public void SetSelectedCell(GridCoord? cell)
    {
        if (_selectedCell == cell) return;
        _selectedCell = cell;
        QueueRedraw();
    }

    public Color ColorAt(GridCoord cell) => ColorFor(cell, 1, 1);

    public void ToggleExpanded()
    {
        Expanded = !Expanded;
        if (Expanded) SetExpandedLayout(); else SetCompactLayout();
        QueueRedraw();
    }

    public void CycleLayer(int direction = 1)
    {
        var index = ((int)Layer + direction) % Layers.Length;
        if (index < 0) index += Layers.Length;
        SetLayer(Layers[index]);
    }

    public void SetLayer(SettlementMapLayer layer)
    {
        if (Layer == layer && _texture is not null) return;
        Layer = layer;
        _renderedRevision = ulong.MaxValue;
        _environment = null;
        RebuildTexture(false);
        LayerChanged?.Invoke(layer);
    }

    public override void _Process(double delta)
    {
        _revisionCheckLeft -= delta;
        if (_revisionCheckLeft > 0) return;
        _revisionCheckLeft = Layer == SettlementMapLayer.Infrastructure ? 0.5 : 2.0;
        if (IsDynamicLayer(Layer) || CurrentRevision() != _renderedRevision) RebuildTexture();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouse || !mouse.Pressed) return;
        if (mouse.ButtonIndex == MouseButton.Right)
        {
            CycleLayer();
            AcceptEvent();
            return;
        }
        if (mouse.ButtonIndex != MouseButton.Left) return;
        var map = MapRect();
        if (!map.HasPoint(mouse.Position)) return;
        var normalized = (mouse.Position - map.Position) / map.Size;
        var cell = new GridCoord(
            Math.Clamp(Mathf.FloorToInt(normalized.X * _data.Width), 0, _data.Width - 1),
            Math.Clamp(Mathf.FloorToInt(normalized.Y * _data.Height), 0, _data.Height - 1));
        SetSelectedCell(cell);
        CellSelected?.Invoke(cell);
        AcceptEvent();
    }

    public override void _Draw()
    {
        if (_data is null) return;
        DrawStyleBox(Panel, new Rect2(Vector2.Zero, Size));
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(Margin, 21), Expanded
            ? "Карта поселения — M свернуть · ПКМ следующий слой"
            : "Карта — M развернуть", HorizontalAlignment.Left, -1, 14, Colors.White);
        DrawString(font, new Vector2(Margin, 43), $"Слой: {LayerTitle(Layer)}",
            HorizontalAlignment.Left, Size.X - Margin * 2, 12, new Color("d7e3e6"));
        var map = MapRect();
        DrawRect(map, new Color("121719"));
        if (_texture is not null) DrawTextureRect(_texture, map, false);
        DrawRect(map, new Color(0.62f, 0.72f, 0.74f, 0.7f), false, 1.5f);
        DrawMarker(map, _cameraCell, new Color("ffdd67"), Expanded ? 4f : 3f);
        if (_selectedCell is { } selected)
            DrawMarker(map, selected, new Color("ff704d"), Expanded ? 6f : 4f, false);
    }

    private void RebuildTexture(bool notify = true)
    {
        if (_data is null) return;
        PrepareDynamicLayer();
        var image = Image.CreateEmpty(TexturePixels, TexturePixels, false, Image.Format.Rgb8);
        var stepX = _data.Width / (float)TexturePixels;
        var stepZ = _data.Height / (float)TexturePixels;
        for (var z = 0; z < TexturePixels; z++)
        for (var x = 0; x < TexturePixels; x++)
        {
            var cell = new GridCoord(Math.Min(_data.Width - 1, (int)((x + 0.5f) * stepX)),
                Math.Min(_data.Height - 1, (int)((z + 0.5f) * stepZ)));
            image.SetPixel(x, z, ColorFor(cell, (int)Math.Ceiling(stepX), (int)Math.Ceiling(stepZ)));
        }
        if (_texture is null) _texture = ImageTexture.CreateFromImage(image);
        else _texture.Update(image);
        _renderedRevision = CurrentRevision();
        QueueRedraw();
        if (notify && IsDynamicLayer(Layer)) LayerRefreshed?.Invoke(Layer);
    }

    private ulong CurrentRevision() => Layer switch
    {
        SettlementMapLayer.Infrastructure or SettlementMapLayer.Urbanisation or
            SettlementMapLayer.Space => _data.InfrastructureRevision,
        SettlementMapLayer.Fertility => _data.FertilityRevision,
        SettlementMapLayer.Moisture => _data.MoistureRevision,
        SettlementMapLayer.Minerals => _data.MineralRevision,
        SettlementMapLayer.Foundation or SettlementMapLayer.Maintenance => _data.Revision,
        SettlementMapLayer.Homeless or SettlementMapLayer.Unemployment =>
            _data.Revision + (ulong)_citizens.Count,
        SettlementMapLayer.RoomProblems or SettlementMapLayer.Workload or
            SettlementMapLayer.Noise or SettlementMapLayer.Light or SettlementMapLayer.Guard or
            SettlementMapLayer.Punishment => _data.InfrastructureRevision + (ulong)_rooms.All.Count,
        _ => _data.TerrainRevision
    };

    private Color ColorFor(GridCoord cell, int spanX, int spanZ) => Layer switch
    {
        SettlementMapLayer.Fertility => HeatColor(_data.Fertility(cell) / 15f,
            new Color("392f25"), new Color("77b84b")),
        SettlementMapLayer.Moisture => HeatColor(_data.Moisture(cell) / 15f,
            new Color("4e3e2d"), new Color("348bc2")),
        SettlementMapLayer.Minerals => MineralColor(cell),
        SettlementMapLayer.Infrastructure => InfrastructureColor(cell, spanX, spanZ),
        SettlementMapLayer.Edibles => HeatColor(Math.Max(_data.GrowableAmount(cell),
                Math.Max(_data.FishAmount(cell), _data.VegetationAmount(cell))) / 15f,
            new Color("24251e"), new Color("b8d957")),
        SettlementMapLayer.Foundation => HeatColor((float)_data.FoundationD(cell),
            new Color("9a342e"), new Color("62b761")),
        SettlementMapLayer.Maintenance => _data.Has(cell, TileFlags.Road)
            ? HeatColor(_data.RoadDegradation(cell) / 15f, new Color("4b8d4d"), new Color("d44738"))
            : TerrainColor(cell).Darkened(0.72f),
        SettlementMapLayer.RoomProblems => RoomProblemColor(cell),
        SettlementMapLayer.Workload => WorkloadColor(cell),
        SettlementMapLayer.Unemployment => _unemployed.Contains(cell.Z * _data.Width + cell.X)
            ? new Color("ed493f") : TerrainColor(cell).Darkened(0.76f),
        SettlementMapLayer.Homeless => _homeless.Contains(cell.Z * _data.Width + cell.X)
            ? new Color("ed493f") : TerrainColor(cell).Darkened(0.76f),
        SettlementMapLayer.Noise or SettlementMapLayer.Light or SettlementMapLayer.Guard or
            SettlementMapLayer.Punishment => EnvironmentColor(cell),
        SettlementMapLayer.Space => HeatColor(_data.IsBlocked(cell) ? 0 : 1,
            new Color("3b2830"), new Color("65a4c8")),
        SettlementMapLayer.Urbanisation => _data.Has(cell, TileFlags.Zone | TileFlags.Road |
                TileFlags.Wall | TileFlags.Furniture)
            ? new Color("d6b35d") : TerrainColor(cell).Darkened(0.72f),
        SettlementMapLayer.FreshWater => HeatColor(_data.Has(cell, TileFlags.GroundWater) ? 1 : 0,
            new Color("2c2723"), new Color("49a9d8")),
        SettlementMapLayer.SaltWater => HeatColor(_data.Has(cell, TileFlags.SaltWater) ? 1 : 0,
            new Color("2c2723"), new Color("647ad0")),
        _ => TerrainColor(cell)
    };

    private Color RoomProblemColor(GridCoord cell)
    {
        if (!_roomByCell.TryGetValue(cell.Z * _data.Width + cell.X, out var room))
            return TerrainColor(cell).Darkened(0.76f);
        if (room.State != RoomState.Operational) return new Color("df493c");
        if (room.Degradation > 0.5 || room.Employment.Employed < room.Employment.Needed)
            return new Color("dd983d");
        return new Color("5dae62");
    }

    private Color WorkloadColor(GridCoord cell)
    {
        if (!_roomByCell.TryGetValue(cell.Z * _data.Width + cell.X, out var room))
            return TerrainColor(cell).Darkened(0.76f);
        var ratio = room.Employment.Needed == 0 ? 1f :
            room.Employment.Employed / (float)room.Employment.Needed;
        return HeatColor(ratio, new Color("cf453b"), new Color("4fad63"));
    }

    private Color EnvironmentColor(GridCoord cell)
    {
        var value = _environment?[cell.Z * _data.Width + cell.X] / 255f ?? 0;
        return HeatColor(value, new Color("282523"), Layer switch
        {
            SettlementMapLayer.Light => new Color("f1d46b"),
            SettlementMapLayer.Guard => new Color("59a66e"),
            SettlementMapLayer.Punishment => new Color("c75b48"),
            _ => new Color("d75f55")
        });
    }

    private void PrepareDynamicLayer()
    {
        _roomByCell = _rooms.All.SelectMany(room => room.Cells.Select(cell => (cell, room)))
            .GroupBy(value => value.cell).ToDictionary(group => group.Key, group => group.Last().room);
        _homeless.Clear();
        _unemployed.Clear();
        if (Layer == SettlementMapLayer.Homeless)
            _citizens.ForEachOverlayCitizen((cell, homeless) =>
            {
                if (homeless) _homeless.Add(cell.Z * _data.Width + cell.X);
            });
        if (Layer == SettlementMapLayer.Unemployment)
            _citizens.ForEachUnemployedCitizen(cell =>
                _unemployed.Add(cell.Z * _data.Width + cell.X));
        var key = Layer switch
        {
            SettlementMapLayer.Noise => "_NOISE", SettlementMapLayer.Light => "_LIGHT",
            SettlementMapLayer.Guard => "_GUARD", SettlementMapLayer.Punishment => "_PUNISHMENT",
            _ => ""
        };
        if (key.Length == 0) return;
        _environment = new byte[_data.Width * _data.Height];
        foreach (var room in _rooms.All.Where(value => value.State == RoomState.Operational))
        {
            var emit = OriginalGameData.Current.Room(room.DefinitionKey)?.EnvironmentEmits
                .GetValueOrDefault(key);
            if (emit is null || emit.Value <= 0 || emit.Radius <= 0) continue;
            var radius = Math.Max(1, (int)Math.Round(emit.Radius * 15));
            foreach (var source in room.Cells.Select(index => new GridCoord(index % _data.Width, index / _data.Width)))
            for (var dz = -radius; dz <= radius; dz++)
            for (var dx = -radius; dx <= radius; dx++)
            {
                var target = new GridCoord(source.X + dx, source.Z + dz);
                if (!_data.IsInside(target)) continue;
                var distance = Math.Sqrt(dx * dx + dz * dz);
                if (distance >= radius) continue;
                var value = emit.Value * (radius - distance) / radius;
                var index = target.Z * _data.Width + target.X;
                _environment[index] = (byte)Math.Clamp(_environment[index] +
                    (int)Math.Ceiling(value * 255), 0, 255);
            }
        }
    }

    private Color InfrastructureColor(GridCoord origin, int spanX, int spanZ)
    {
        var terrain = TerrainColor(origin).Darkened(0.45f);
        for (var z = 0; z < spanZ; z++)
        for (var x = 0; x < spanX; x++)
        {
            var cell = new GridCoord(origin.X + x - spanX / 2, origin.Z + z - spanZ / 2);
            if (_data.Has(cell, TileFlags.Wall | TileFlags.Furniture)) return new Color("d1c69e");
            if (_data.Has(cell, TileFlags.Door)) return new Color("f0be55");
            if (_data.Has(cell, TileFlags.Road)) return new Color("bc8652");
            if (_data.Has(cell, TileFlags.Zone)) terrain = new Color("397ab0");
        }
        return terrain;
    }

    private Color MineralColor(GridCoord cell)
    {
        var amount = _data.MineralAmount(cell);
        if (amount <= 0) return TerrainColor(cell).Darkened(0.62f);
        var colors = new[] { new Color("9da6a8"), new Color("ca8948"), new Color("d7bd55"),
            new Color("6fc0c9"), new Color("b073d1") };
        return colors[Math.Abs(_data.MineralType(cell)) % colors.Length]
            .Lerp(Colors.White, amount / 126f);
    }

    private Color TerrainColor(GridCoord cell)
    {
        if (_data.Has(cell, TileFlags.DeepWater)) return new Color("174d73");
        if (_data.Has(cell, TileFlags.Water)) return new Color("287cad");
        return _data.Ground(cell) switch
        {
            GroundKind.Mountain => new Color("59605c"),
            GroundKind.Forest => new Color("173d28"),
            GroundKind.Wet => new Color("456340"),
            GroundKind.Sand => new Color("b5a16b"),
            GroundKind.Infertile => new Color("554f45"),
            GroundKind.Pasture => new Color("788153"),
            GroundKind.Soil => new Color("57723b"),
            _ => new Color("384d33")
        };
    }

    private static Color HeatColor(float value, Color low, Color high) =>
        low.Lerp(high, Mathf.Clamp(value, 0f, 1f));

    private static bool IsDynamicLayer(SettlementMapLayer layer) => layer is
        SettlementMapLayer.RoomProblems or SettlementMapLayer.Workload or
        SettlementMapLayer.Unemployment or SettlementMapLayer.Homeless or
        SettlementMapLayer.Noise or SettlementMapLayer.Light or
        SettlementMapLayer.Guard or SettlementMapLayer.Punishment;

    private void DrawMarker(Rect2 map, GridCoord cell, Color color, float radius, bool filled = true)
    {
        var marker = map.Position + new Vector2(
            (cell.X + 0.5f) / _data.Width * map.Size.X,
            (cell.Z + 0.5f) / _data.Height * map.Size.Y);
        DrawCircle(marker, radius, color, filled, -1, true);
    }

    private Rect2 MapRect() => new(Margin, 50, Size.X - Margin * 2, Size.Y - 62);

    private static string LayerTitle(SettlementMapLayer layer) => layer switch
    {
        SettlementMapLayer.Infrastructure => "Стройки",
        SettlementMapLayer.Fertility => "Плод.",
        SettlementMapLayer.Moisture => "Влага",
        SettlementMapLayer.Minerals => "Руды",
        SettlementMapLayer.Edibles => "Съедобные ресурсы",
        SettlementMapLayer.Foundation => "Фундамент",
        SettlementMapLayer.Maintenance => "Обслуживание",
        SettlementMapLayer.RoomProblems => "Проблемы помещений",
        SettlementMapLayer.Workload => "Рабочая нагрузка",
        SettlementMapLayer.Unemployment => "Безработица",
        SettlementMapLayer.Homeless => "Бездомность",
        SettlementMapLayer.Noise => "Шум",
        SettlementMapLayer.Light => "Освещение",
        SettlementMapLayer.Space => "Пространство",
        SettlementMapLayer.Urbanisation => "Урбанизация",
        SettlementMapLayer.Guard => "Охрана",
        SettlementMapLayer.Punishment => "Наказание",
        SettlementMapLayer.FreshWater => "Пресная вода",
        SettlementMapLayer.SaltWater => "Солёная вода",
        _ => "Рельеф"
    };

    private void SetCompactLayout()
    {
        Position = new Vector2(1000, 308);
        Size = new Vector2(268, 268);
    }

    private void SetExpandedLayout()
    {
        Position = new Vector2(328, 34);
        Size = new Vector2(624, 652);
    }
}
