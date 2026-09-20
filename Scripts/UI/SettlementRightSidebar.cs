using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.UI;

/// <summary>
/// Persistent settlement side panel based on UIPanelRightSett, UIMiniResources and
/// UIMinimapPanel: local minimap, two-column resource tally and collapsible mini panels.
/// </summary>
public sealed partial class SettlementRightSidebar : ColorRect
{
    private sealed class Hotspot
    {
        public int Id { get; init; }
        public GridCoord Cell { get; set; }
        public string Name { get; set; } = "?";
        public Color Color { get; set; }
    }

    private const int MaximumHotspots = 32;
    private readonly Dictionary<ResourceKind, Label> _resourceValues = new();
    private readonly Dictionary<ResourceKind, Button> _resourceButtons = new();
    private readonly Dictionary<string, Label> _raceValues = new(StringComparer.OrdinalIgnoreCase);
    private ResourceLedger _resources = null!;
    private CitizenSystem _citizens = null!;
    private RoomSystem _rooms = null!;
    private SettlementMiniMap _miniMap = null!;
    private Control _resourcePanel = null!;
    private Control _racePanel = null!;
    private Control _hotspotPanel = null!;
    private ScrollContainer _resourceScroll = null!;
    private ScrollContainer _hotspotScroll = null!;
    private ColorRect _overlayPanel = null!;
    private readonly List<Hotspot> _hotspots = new(MaximumHotspots);
    private VBoxContainer _hotspotRows = null!;
    private ColorRect _hotspotEditor = null!;
    private LineEdit _hotspotName = null!;
    private ColorPickerButton _hotspotColor = null!;
    private Hotspot? _editingHotspot;
    private int _nextHotspotId = 1;

    public event Action<GridCoord>? CellSelected;
    public event Action<float>? ZoomRequested;
    public event Action? MapRequested;
    public event Action? EconomyRequested;
    public event Action? HideUiRequested;
    public event Action? HotspotPlacementRequested;
    public event Action<SettlementMapLayer>? OverlayRequested;

    private static readonly Dictionary<ResourceKind, string> ResourceSheets = new()
    {
        [ResourceKind.Wood] = "Wood", [ResourceKind.Stone] = "Stone",
        [ResourceKind.Grain] = "Grain", [ResourceKind.Food] = "Bread",
        [ResourceKind.Tools] = "Tool", [ResourceKind.Furniture] = "Furniture",
        [ResourceKind.Beer] = "Alcohol", [ResourceKind.Wine] = "Alcohol",
        [ResourceKind.ArmourLeather] = "Armour_leather", [ResourceKind.ArmourPlate] = "Armour_plate",
        [ResourceKind.Bow] = "Bow", [ResourceKind.Clay] = "Clay",
        [ResourceKind.Clothes] = "Clothes", [ResourceKind.Coal] = "Coal",
        [ResourceKind.Cotton] = "Cotton", [ResourceKind.Egg] = "Egg",
        [ResourceKind.Fabric] = "Fabric", [ResourceKind.Fish] = "Fish",
        [ResourceKind.Fruit] = "Fruit", [ResourceKind.Gem] = "Gem",
        [ResourceKind.Herb] = "Herb", [ResourceKind.Jewelry] = "Jewelry",
        [ResourceKind.Leather] = "Leather", [ResourceKind.Machinery] = "Machinery",
        [ResourceKind.Meat] = "Meat", [ResourceKind.Metal] = "Metal",
        [ResourceKind.Mushroom] = "Mushroom", [ResourceKind.Opiates] = "Opiates",
        [ResourceKind.Ore] = "Ore", [ResourceKind.Paper] = "Paper",
        [ResourceKind.Pottery] = "Pottery", [ResourceKind.Ration] = "Ration",
        [ResourceKind.Sithilon] = "Sithilon", [ResourceKind.CutStone] = "StoneCut",
        [ResourceKind.Vegetable] = "Vegetable", [ResourceKind.WeaponHammer] = "Weapon_hammer",
        [ResourceKind.WeaponMount] = "Weapon_mount", [ResourceKind.WeaponShield] = "Weapon_shield",
        [ResourceKind.WeaponShort] = "Weapon_short", [ResourceKind.WeaponSlash] = "Weapon_slash",
        [ResourceKind.WeaponSpear] = "Weapon_spear", [ResourceKind.Livestock] = "Livestock"
    };

    private static readonly (string Key, string Sheet)[] Races =
    {
        ("HUMAN", "Human"), ("CRETONIAN", "Cretonian"), ("DONDORIAN", "Dondorian"),
        ("GARTHIMI", "Garthimi"), ("TILAPI", "Tilapi"), ("AMEVIA", "Amevia"),
        ("ARGONOSH", "Argonosh"), ("CANTOR", "Cantor")
    };

    public void Initialize(WorldGridData data, ResourceLedger resources,
        CitizenSystem citizens, RoomSystem rooms)
    {
        _resources = resources;
        _citizens = citizens;
        _rooms = rooms;
        Size = new Vector2(242, 596);
        Color = new Color(0.055f, 0.058f, 0.055f, 0.96f);
        MouseFilter = MouseFilterEnum.Stop;

        _miniMap = new SettlementMiniMap
        {
            Position = new Vector2(3, 3),
            Size = new Vector2(236, 116)
        };
        _miniMap.Initialize(data);
        _miniMap.CellSelected += cell => CellSelected?.Invoke(cell);
        AddChild(_miniMap);
        BuildMapButtons();
        BuildOverlayPanel();
        BuildRacePanel();
        BuildResourcePanel();
        BuildHotspotPanel();
        ApplyResponsiveLayout();
        GetViewport().SizeChanged += ApplyResponsiveLayout;
    }

    public void UpdateState(GridCoord cameraCell, float cameraSize)
    {
        _miniMap.SetView(cameraCell, cameraSize);
        _miniMap.SetCitizens(_citizens);
        var produced = _resources.CaptureTotalProduced();
        var consumed = _resources.CaptureTotalConsumed();
        foreach (var kind in Enum.GetValues<ResourceKind>())
        {
            var amount = _resources.Get(kind);
            _resourceValues[kind].Text = Compact(amount);
            _resourceValues[kind].AddThemeColorOverride("font_color",
                amount > 0 ? new Color("b8d7a8") : new Color("b06a68"));
            _resourceButtons[kind].TooltipText =
                $"{ResourceTitle(kind)}: {amount}\nПроизведено: {produced[(int)kind]} · израсходовано: {consumed[(int)kind]}";
        }
        var population = _citizens.PopulationByRace();
        foreach (var race in Races)
            _raceValues[race.Key].Text = Compact(population.GetValueOrDefault(race.Key));
        TooltipText = $"Жители: {_citizens.Count} · голодают: {_citizens.StarvingCount}\n" +
                      $"Комнаты: {_rooms.OperationalCount}/{_rooms.All.Count}";
    }

    public void UpdateCameraView(GridCoord cameraCell, float cameraSize) =>
        _miniMap.SetView(cameraCell, cameraSize);

    private void BuildMapButtons()
    {
        var row = new HBoxContainer
        {
            Position = new Vector2(3, 121),
            Size = new Vector2(236, 34)
        };
        row.AddThemeConstantOverride("separation", 1);
        AddChild(row);
        AddSmallButton(row, OriginalUiIcons.Small(2), "Большая карта", () => MapRequested?.Invoke());
        AddSmallButton(row, OriginalUiIcons.Small(1), "Отдалить", () => ZoomRequested?.Invoke(2f));
        AddSmallButton(row, OriginalUiIcons.Small(0), "Приблизить", () => ZoomRequested?.Invoke(-2f));
        AddSmallButton(row, OriginalUiIcons.Small(6), "Снимок экрана", () => TakeScreenshot(false));
        AddSmallButton(row, OriginalUiIcons.Small(81), "Снимок всей карты", () => TakeScreenshot(true));
        AddSmallButton(row, OriginalUiIcons.Small(5),
            "Кинематографический режим: скрыть интерфейс (Esc или правая кнопка — вернуть)",
            () => HideUiRequested?.Invoke());
        AddSmallButton(row, OriginalUiIcons.Small(25), "Слои карты", () =>
            _overlayPanel.Visible = !_overlayPanel.Visible);
    }

    private void BuildOverlayPanel()
    {
        _overlayPanel = new ColorRect
        {
            Position = new Vector2(-510, 108), Size = new Vector2(504, 326),
            Color = new Color("151815fa"), MouseFilter = MouseFilterEnum.Stop, Visible = false
        };
        AddChild(_overlayPanel);
        var grid = new GridContainer
        {
            Columns = 2, Position = new Vector2(4, 4), Size = new Vector2(496, 318)
        };
        grid.AddThemeConstantOverride("h_separation", 2);
        grid.AddThemeConstantOverride("v_separation", 2);
        _overlayPanel.AddChild(grid);
        foreach (var layer in SettlementMapLayerOrder())
        {
            var button = new Button
            {
                Text = LayerTitle(layer), CustomMinimumSize = new Vector2(247, 30),
                Alignment = HorizontalAlignment.Left, FocusMode = FocusModeEnum.None
            };
            button.Pressed += () =>
            {
                OverlayRequested?.Invoke(layer);
                _overlayPanel.Visible = false;
            };
            grid.AddChild(button);
        }
    }

    private void TakeScreenshot(bool fullMap)
    {
        var directory = "user://screenshots";
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(directory));
        var path = $"{directory}/settlement_{DateTime.Now:yyyyMMdd_HHmmss}" +
                   (fullMap ? "_full.png" : ".png");
        if (!fullMap)
        {
            GetViewport().GetTexture().GetImage().SavePng(path);
            return;
        }
        var image = Image.CreateEmpty(_miniMap.DataWidth, _miniMap.DataHeight, false, Image.Format.Rgb8);
        for (var z = 0; z < _miniMap.DataHeight; z++)
        for (var x = 0; x < _miniMap.DataWidth; x++)
            image.SetPixel(x, z, _miniMap.ColorAt(new GridCoord(x, z)));
        image.SavePng(path);
    }

    private static IEnumerable<SettlementMapLayer> SettlementMapLayerOrder() =>
        GlobalMapOverlay.AvailableLayers.OrderBy(LayerTitle);

    private static string LayerTitle(SettlementMapLayer layer) => layer switch
    {
        SettlementMapLayer.Terrain => "Рельеф", SettlementMapLayer.Infrastructure => "Стройки",
        SettlementMapLayer.Fertility => "Плодородие", SettlementMapLayer.Moisture => "Влажность",
        SettlementMapLayer.Minerals => "Ресурсы", SettlementMapLayer.Edibles => "Съедобные ресурсы",
        SettlementMapLayer.Foundation => "Фундамент", SettlementMapLayer.Maintenance => "Обслуживание",
        SettlementMapLayer.RoomProblems => "Проблемы помещений", SettlementMapLayer.Workload => "Рабочая нагрузка",
        SettlementMapLayer.Unemployment => "Безработица",
        SettlementMapLayer.Homeless => "Бездомность", SettlementMapLayer.Noise => "Шум",
        SettlementMapLayer.Light => "Освещение", SettlementMapLayer.Space => "Пространство",
        SettlementMapLayer.Urbanisation => "Урбанизация", SettlementMapLayer.Guard => "Охрана",
        SettlementMapLayer.Punishment => "Наказание", SettlementMapLayer.FreshWater => "Пресная вода",
        SettlementMapLayer.SaltWater => "Солёная вода", _ => layer.ToString()
    };

    private void BuildRacePanel()
    {
        _racePanel = new ColorRect
        {
            Position = new Vector2(3, 158), Size = new Vector2(42, 435),
            Color = new Color("171918"), MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(_racePanel);
        var column = new VBoxContainer { Position = Vector2.Zero, Size = _racePanel.Size };
        column.AddThemeConstantOverride("separation", 1);
        _racePanel.AddChild(column);
        foreach (var race in Races)
        {
            var row = new Control { CustomMinimumSize = new Vector2(42, 44), TooltipText = race.Key };
            var icon = new TextureRect
            {
                Texture = OriginalUiIcons.FromAtlas(race.Sheet, 0),
                Position = new Vector2(5, 2), Size = new Vector2(32, 32),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore
            };
            row.AddChild(icon);
            var value = new Label
            {
                Text = "0", Position = new Vector2(2, 27), Size = new Vector2(38, 16),
                HorizontalAlignment = HorizontalAlignment.Right,
                MouseFilter = MouseFilterEnum.Ignore
            };
            value.AddThemeFontSizeOverride("font_size", 11);
            row.AddChild(value);
            column.AddChild(row);
            _raceValues[race.Key] = value;
        }
    }

    private void BuildResourcePanel()
    {
        _resourcePanel = new ColorRect
        {
            Position = new Vector2(47, 158), Size = new Vector2(153, 435),
            Color = new Color("181a19"), MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(_resourcePanel);
        _resourceScroll = new ScrollContainer
        {
            Position = new Vector2(1, 1), Size = new Vector2(151, 433),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        _resourcePanel.AddChild(_resourceScroll);
        var grid = new GridContainer { Columns = 2 };
        grid.AddThemeConstantOverride("h_separation", 1);
        grid.AddThemeConstantOverride("v_separation", 1);
        _resourceScroll.AddChild(grid);
        foreach (var kind in Enum.GetValues<ResourceKind>())
        {
            var button = new Button
            {
                Icon = OriginalUiIcons.Resource(ResourceSheets[kind]),
                CustomMinimumSize = new Vector2(71, 27),
                FocusMode = FocusModeEnum.None,
                Alignment = HorizontalAlignment.Left
            };
            button.AddThemeStyleboxOverride("normal", ResourceStyle(new Color("202420")));
            button.AddThemeStyleboxOverride("hover", ResourceStyle(new Color("343a31")));
            var value = new Label
            {
                Text = "0", Position = new Vector2(27, 4), Size = new Vector2(40, 19),
                HorizontalAlignment = HorizontalAlignment.Right,
                MouseFilter = MouseFilterEnum.Ignore
            };
            value.AddThemeFontSizeOverride("font_size", 12);
            button.AddChild(value);
            button.Pressed += () => EconomyRequested?.Invoke();
            grid.AddChild(button);
            _resourceValues[kind] = value;
            _resourceButtons[kind] = button;
        }
    }

    private void BuildHotspotPanel()
    {
        _hotspotPanel = new ColorRect
        {
            Position = new Vector2(202, 158), Size = new Vector2(37, 435),
            Color = new Color("171918"), MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(_hotspotPanel);
        var add = new Button
        {
            Icon = OriginalUiIcons.Medium(9), TooltipText = "Поставить метку",
            Position = new Vector2(1, 1), Size = new Vector2(35, 35), FocusMode = FocusModeEnum.None
        };
        add.Pressed += () =>
        {
            if (_hotspots.Count < MaximumHotspots) HotspotPlacementRequested?.Invoke();
        };
        _hotspotPanel.AddChild(add);
        _hotspotScroll = new ScrollContainer
        {
            Position = new Vector2(1, 38), Size = new Vector2(35, 396),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        _hotspotPanel.AddChild(_hotspotScroll);
        _hotspotRows = new VBoxContainer();
        _hotspotRows.AddThemeConstantOverride("separation", 1);
        _hotspotScroll.AddChild(_hotspotRows);

        _hotspotEditor = new ColorRect
        {
            Position = new Vector2(-256, 160), Size = new Vector2(250, 172),
            Color = new Color("151815fa"), MouseFilter = MouseFilterEnum.Stop, Visible = false
        };
        AddChild(_hotspotEditor);
        _hotspotName = new LineEdit { Position = new Vector2(8, 8), Size = new Vector2(190, 34), MaxLength = 20 };
        _hotspotName.TextChanged += value =>
        {
            if (_editingHotspot is null) return;
            _editingHotspot.Name = value;
            RebuildHotspots();
        };
        _hotspotEditor.AddChild(_hotspotName);
        _hotspotColor = new ColorPickerButton
        {
            Position = new Vector2(204, 8), Size = new Vector2(38, 34), TooltipText = "Цвет метки"
        };
        _hotspotColor.ColorChanged += value =>
        {
            if (_editingHotspot is null) return;
            _editingHotspot.Color = value;
            RebuildHotspots();
        };
        _hotspotEditor.AddChild(_hotspotColor);
        AddEditorButton("Вверх", 8, () => MoveHotspot(-1));
        AddEditorButton("Вниз", 86, () => MoveHotspot(1));
        AddEditorButton("Удалить", 164, DeleteHotspot);
    }

    public bool AddHotspot(GridCoord cell)
    {
        if (_hotspots.Count >= MaximumHotspots) return false;
        _hotspots.Add(new Hotspot
        {
            Id = _nextHotspotId++, Cell = cell, Name = "?",
            Color = new Color(GD.Randf() * 0.5f, GD.Randf() * 0.5f, GD.Randf() * 0.5f)
        });
        RebuildHotspots();
        return true;
    }

    private void RebuildHotspots()
    {
        foreach (var child in _hotspotRows.GetChildren())
        {
            _hotspotRows.RemoveChild(child);
            child.QueueFree();
        }
        foreach (var hotspot in _hotspots)
        {
            var button = new Button
            {
                Text = hotspot.Name, TooltipText =
                    $"{hotspot.Name}\nКлетка {hotspot.Cell.X}:{hotspot.Cell.Z}\n" +
                    "ЛКМ — перейти, ПКМ — изменить",
                CustomMinimumSize = new Vector2(35, 32), FocusMode = FocusModeEnum.None,
                Modulate = hotspot.Color.Lightened(0.45f)
            };
            button.Pressed += () => CellSelected?.Invoke(hotspot.Cell);
            button.GuiInput += inputEvent =>
            {
                if (inputEvent is InputEventMouseButton mouse && mouse.Pressed &&
                    mouse.ButtonIndex == MouseButton.Right)
                {
                    OpenHotspotEditor(hotspot);
                    button.AcceptEvent();
                }
            };
            _hotspotRows.AddChild(button);
        }
    }

    private void OpenHotspotEditor(Hotspot hotspot)
    {
        _editingHotspot = hotspot;
        _hotspotName.Text = hotspot.Name;
        _hotspotColor.Color = hotspot.Color;
        _hotspotEditor.Visible = true;
    }

    private void MoveHotspot(int direction)
    {
        if (_editingHotspot is null) return;
        var index = _hotspots.IndexOf(_editingHotspot);
        var target = Math.Clamp(index + direction, 0, _hotspots.Count - 1);
        if (index == target) return;
        _hotspots.RemoveAt(index);
        _hotspots.Insert(target, _editingHotspot);
        RebuildHotspots();
    }

    private void DeleteHotspot()
    {
        if (_editingHotspot is null) return;
        _hotspots.Remove(_editingHotspot);
        _editingHotspot = null;
        _hotspotEditor.Visible = false;
        RebuildHotspots();
    }

    private void AddEditorButton(string text, float x, Action action)
    {
        var button = new Button { Text = text, Position = new Vector2(x, 54), Size = new Vector2(72, 34) };
        button.Pressed += action;
        _hotspotEditor.AddChild(button);
    }

    private static void AddSmallButton(Control parent, Texture2D? icon, string tip, Action action)
    {
        var button = new Button
        {
            Icon = icon, TooltipText = tip, CustomMinimumSize = new Vector2(32, 34),
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeStyleboxOverride("normal", SmallButtonStyle(new Color("1c1e1b"), new Color("51544b")));
        button.AddThemeStyleboxOverride("hover", SmallButtonStyle(new Color("34372f"), new Color("a99d70")));
        button.AddThemeStyleboxOverride("pressed", SmallButtonStyle(new Color("414438"), new Color("d7c27e")));
        button.Pressed += action;
        parent.AddChild(button);
    }

    private void ApplyResponsiveLayout()
    {
        var viewport = GetViewportRect().Size;
        Position = new Vector2(Mathf.Max(0f, viewport.X - 242f), 52f);
        Size = new Vector2(242f, Mathf.Max(300f, viewport.Y - 52f));
        var lowerHeight = Mathf.Max(137f, Size.Y - 161f);
        _racePanel.Size = new Vector2(42f, lowerHeight);
        _resourcePanel.Size = new Vector2(153f, lowerHeight);
        _resourceScroll.Size = new Vector2(151f, Mathf.Max(1f, lowerHeight - 2f));
        _hotspotPanel.Size = new Vector2(37f, lowerHeight);
        _hotspotScroll.Size = new Vector2(35f, Mathf.Max(1f, lowerHeight - 39f));
    }

    private static StyleBoxFlat ResourceStyle(Color color) => new()
    {
        BgColor = color,
        BorderColor = new Color("3d423b"),
        BorderWidthLeft = 1, BorderWidthTop = 1,
        BorderWidthRight = 1, BorderWidthBottom = 1
    };

    private static StyleBoxFlat SmallButtonStyle(Color color, Color border) => new()
    {
        BgColor = color,
        BorderColor = border,
        BorderWidthLeft = 1, BorderWidthTop = 1,
        BorderWidthRight = 1, BorderWidthBottom = 1
    };

    private static string Compact(int amount) => amount switch
    {
        >= 1_000_000 => $"{amount / 1_000_000f:0.#}M",
        >= 10_000 => $"{amount / 1_000f:0.#}K",
        _ => amount.ToString()
    };

    private static string ResourceTitle(ResourceKind kind) => kind switch
    {
        ResourceKind.Wood => "Дерево", ResourceKind.Stone => "Камень",
        ResourceKind.Grain => "Зерно", ResourceKind.Food => "Еда",
        ResourceKind.Tools => "Инструменты", ResourceKind.Furniture => "Мебель",
        ResourceKind.Beer => "Пиво", ResourceKind.Wine => "Вино",
        ResourceKind.Coal => "Уголь", ResourceKind.Ore => "Руда",
        ResourceKind.Metal => "Металл", ResourceKind.Ration => "Рационы",
        _ => kind.ToString()
    };
}

public sealed partial class SettlementMiniMap : Control
{
    private WorldGridData _data = null!;
    private ImageTexture? _texture;
    private GridCoord _center;
    private float _cameraSize = 20f;
    private ulong _revision = ulong.MaxValue;
    private readonly List<(GridCoord Cell, string Race)> _citizens = new();
    public event Action<GridCoord>? CellSelected;
    public int DataWidth => _data.Width;
    public int DataHeight => _data.Height;
    public Color ColorAt(GridCoord cell) => MiniColor(cell);

    public void Initialize(WorldGridData data)
    {
        _data = data;
        MouseFilter = MouseFilterEnum.Stop;
        _center = new GridCoord(data.Width / 2, data.Height / 2);
        Rebuild();
    }

    public void SetView(GridCoord center, float cameraSize)
    {
        if (_center == center && Math.Abs(_cameraSize - cameraSize) < 0.001f &&
            _revision == _data.InfrastructureRevision) return;
        _cameraSize = cameraSize;
        _center = center;
        if (_revision != _data.InfrastructureRevision) Rebuild();
        QueueRedraw();
    }

    public void SetCitizens(CitizenSystem citizens)
    {
        _citizens.Clear();
        citizens.ForEachMiniMapCitizen((cell, race) => _citizens.Add((cell, race)));
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouse || !mouse.Pressed ||
            mouse.ButtonIndex != MouseButton.Left) return;
        var cell = new GridCoord(
            Math.Clamp((int)(mouse.Position.X / Math.Max(1f, Size.X) * _data.Width), 0, _data.Width - 1),
            Math.Clamp((int)(mouse.Position.Y / Math.Max(1f, Size.Y) * _data.Height), 0, _data.Height - 1));
        if (_data.IsInside(cell)) CellSelected?.Invoke(cell);
        AcceptEvent();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("0b0d0c"));
        if (_texture is not null) DrawTextureRect(_texture, new Rect2(Vector2.Zero, Size), false);
        foreach (var citizen in _citizens)
        {
            var point = new Vector2(citizen.Cell.X * Size.X / _data.Width,
                citizen.Cell.Z * Size.Y / _data.Height);
            if (point.X >= 0 && point.Y >= 0 && point.X < Size.X && point.Y < Size.Y)
                DrawRect(new Rect2(point, new Vector2(2, 2)), RaceColor(citizen.Race));
        }
        var viewHeight = Mathf.Clamp(_cameraSize / _data.Height * Size.Y, 2f, Size.Y);
        var viewWidth = Mathf.Clamp(_cameraSize * 16f / 9f / _data.Width * Size.X, 3f, Size.X);
        var center = new Vector2(_center.X * Size.X / _data.Width,
            _center.Z * Size.Y / _data.Height);
        var view = new Rect2(center.X - viewWidth * 0.5f, center.Y - viewHeight * 0.5f,
            viewWidth, viewHeight);
        DrawRect(view, new Color(1f, 1f, 1f, 0.5f), false, 1f);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("777568"), false, 2f);
    }

    private void Rebuild()
    {
        var width = Math.Max(1, (int)Size.X);
        var height = Math.Max(1, (int)Size.Y);
        var image = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var cell = new GridCoord(
                Math.Clamp((int)((x + 0.5) * _data.Width / width), 0, _data.Width - 1),
                Math.Clamp((int)((y + 0.5) * _data.Height / height), 0, _data.Height - 1));
            image.SetPixel(x, y, MiniColor(cell));
        }
        if (_texture is null) _texture = ImageTexture.CreateFromImage(image);
        else _texture.Update(image);
        _revision = _data.InfrastructureRevision;
        QueueRedraw();
    }

    private Color MiniColor(GridCoord cell)
    {
        if (!_data.IsInside(cell)) return Colors.Black;
        if (_data.Has(cell, TileFlags.Wall | TileFlags.Furniture)) return new Color("c6bca0");
        if (_data.Has(cell, TileFlags.Door)) return new Color("e8b64a");
        if (_data.Has(cell, TileFlags.Road)) return new Color("a77e55");
        if (_data.Has(cell, TileFlags.Zone)) return new Color("527b83");
        if (_data.Has(cell, TileFlags.DeepWater)) return new Color("153d58");
        if (_data.Has(cell, TileFlags.Water)) return new Color("286a8a");
        return _data.Ground(cell) switch
        {
            GroundKind.Mountain => new Color("4e4c49"),
            GroundKind.Forest => new Color("273a2a"),
            GroundKind.Wet => new Color("45533c"),
            GroundKind.Sand => new Color("b5a16b"),
            GroundKind.Infertile => new Color("554f45"),
            GroundKind.Pasture => new Color("788153"),
            GroundKind.Soil => new Color("696851"),
            _ => new Color("4e5941")
        };
    }

    private static Color RaceColor(string race) => race.ToUpperInvariant() switch
    {
        "DONDORIAN" => new Color("d0b77d"), "GARTHIMI" => new Color("8db66b"),
        "CRETONIAN" => new Color("d49b7c"), "TILAPI" => new Color("77a77d"),
        "AMEVIA" => new Color("72a9c7"), "ARGONOSH" => new Color("b48aaa"),
        "CANTOR" => new Color("d8cd9e"), _ => new Color("e5e5dc")
    };
}
