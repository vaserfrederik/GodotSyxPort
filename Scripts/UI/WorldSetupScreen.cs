using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.World;
using GodotSyxPort.Bootstrap;
using GodotSyxPort.Settlement;
using GodotSyxPort.Rendering;

namespace GodotSyxPort.UI;

/// <summary>C# new-game world generator, race picker and settlement-site selector.</summary>
public sealed partial class WorldSetupScreen : Control
{
    private static readonly int[] HouseOffsetX =
        { 52, 52, 0, 0, 52, 52, 0, 0, 32, 32, 16, 16, 32, 32, 16, 16 };
    private static readonly int[] HouseOffsetY =
        { 52, 32, 52, 32, 0, 16, 0, 16, 52, 32, 52, 32, 0, 16, 0, 16 };
    private enum SetupStage : byte { Terrain, Capital, Finish, Launching }
    private static readonly Rect2 MapViewport = new(0, 30, 1600, 870);
    private const string GeneratorMapsPath = "res://Data/Original/assets/sprite/world/generatorMaps";
    // Java renders the strategic world with 16x16 source tiles. Keeping their
    // native resolution avoids the 8x8 downsample/nearest-neighbour blocks.
    private const int WorldTextureScale = 16;
    private const string WorldGroundAtlasPath = "res://Data/Original/assets/sprite/world/map/Ground.png";
    private const string WorldWaterAtlasPath = "res://Data/Original/assets/sprite/world/map/Water.png";
    private const string WorldForestAtlasPath = "res://Data/Original/assets/sprite/world/map/Forest.png";
    private const string WorldMountainAtlasPath = "res://Data/Original/assets/sprite/world/map/Mountain.png";
    private const string WorldRiverAtlasPath = "res://Data/Original/assets/sprite/world/map/RiverBig.png";
    private const string WorldRiverSmallAtlasPath = "res://Data/Original/assets/sprite/world/map/RiverSmall.png";
    private const string WorldRoadAtlasPath = "res://Data/Original/assets/sprite/world/map/buildings/Roads.png";
    private const string OriginalIconsPath = "res://Data/Original/assets/sprite/icon/32/_ICONS.png";
    private const string OriginalUiIconsPath = "res://Data/Original/assets/sprite/icon/32/_UI.png";
    private const int CapitalSelectionPreviewDimension = 3;
    private StrategicWorldRuntime _world = null!;
    private int _seed;
    private int _selectedRegion = -1;
    private Vector2I _selectedCapitalTile = new(-1, -1);
    private OptionButton _race = null!;
    private Label _details = null!;
    private ColorRect _detailsBackground = null!;
    private TextureRect _detailsIcon = null!;
    private readonly List<TextureRect> _detailStatIcons = new();
    private Label _title = null!;
    private Button _confirm = null!;
    private Control _generationPanel = null!;
    private Control _selectionPanel = null!;
    private OptionButton _mapType = null!;
    private TextureRect _mapPreview = null!;
    private HSlider _latitude = null!;
    private LineEdit _seedInput = null!;
    private Label _generationStatus = null!;
    private OptionButton _terrainTool = null!;
    private SpinBox _brushSize = null!;
    private Label _brushLabel = null!;
    private Button _editTerrain = null!;
    private Button _regenerate = null!;
    private Button _back = null!;
    private Button _home = null!;
    private Label _editorHint = null!;
    private ColorRect _busyOverlay = null!;
    private Label _busyLabel = null!;
    private ImageTexture? _worldMapTexture;
    private Image? _worldGroundAtlas;
    private Image? _worldWaterAtlas;
    private Image? _worldForestAtlas;
    private Image? _worldMountainAtlas;
    private Image? _worldRiverAtlas;
    private Image? _worldRiverSmallAtlas;
    private Image? _worldRoadAtlas;
    private ImageTexture? _realmMapTexture;
    private ImageTexture? _roadMapTexture;
    private readonly Dictionary<string, Texture2D?> _townTextures =
        new(StringComparer.OrdinalIgnoreCase);
    private Vector2 _mapCenter = new(StrategicWorldRuntime.TileDimension / 2f,
        StrategicWorldRuntime.TileDimension / 2f);
    private Rect2 _mapSource;
    // The original world is 256x256 logical tiles and 32 pixels per tile in the
    // world view: the complete map is 8192x8192 pixels and must be camera-scrolled.
    // The Java world view starts at one native 16 px world tile per screen tile.
    private float _mapZoom = 16f;
    private bool _editMode;
    private bool _painting;
    private bool _panning;
    private bool _terrainDirty;
    private ulong _lastTerrainTextureBuildMsec;
    private Vector2 _lastMouse;
    private readonly List<StrategicTerrainTemplate?> _templates = new();
    private readonly List<Texture2D?> _templateTextures = new();
    private bool _overviewOnly;
    private PlayerStartProfile? _profile;
    private bool _worldFinalized;
    private bool _generatingCivilizations;
    private SetupStage _stage = SetupStage.Terrain;
    private IReadOnlyList<string> _generationProblems = Array.Empty<string>();
    private int _hoveredRegion = -1;
    private Vector2I _hoveredTile = new(-1, -1);
    private static readonly Rect2 MiniMapViewport = new(1344, 64, 240, 240);

    public event Action<StrategicWorldRuntime, int, int, int, int, string>? StartRequested;
    public event Action? BackRequested;
    public event Action? MainMenuRequested;

    public StrategicWorldRuntime World => _world;

    public void ShowLaunchError(Exception exception)
    {
        _stage = SetupStage.Finish;
        SetBusy(false);
        _worldFinalized = true;
        _confirm.Disabled = false;
        _confirm.Text = "";
        _confirm.TooltipText = "Повторить запуск";
        _title.Text = "Ошибка запуска поселения";
        _details.Text = exception.ToString();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_world is null || !_selectionPanel.Visible || _generatingCivilizations ||
            GetViewport().GuiGetFocusOwner() is LineEdit) return;
        var direction = Vector2.Zero;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) direction.X -= 1;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) direction.X += 1;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) direction.Y -= 1;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) direction.Y += 1;
        var mouse = GetViewport().GetMousePosition();
        const float edge = 10f;
        if (MapViewport.HasPoint(mouse))
        {
            if (mouse.X <= MapViewport.Position.X + edge) direction.X -= 1;
            if (mouse.X >= MapViewport.Position.X + MapViewport.Size.X - edge) direction.X += 1;
            if (mouse.Y <= MapViewport.Position.Y + edge) direction.Y -= 1;
            if (mouse.Y >= MapViewport.Position.Y + MapViewport.Size.Y - edge) direction.Y += 1;
        }
        if (direction == Vector2.Zero) return;
        _mapCenter += direction.Normalized() * 28f * (float)delta;
        ClampMapCenter();
        QueueRedraw();
    }

    public void InitializeNew(int seed, PlayerStartProfile? profile = null)
    {
        _overviewOnly = false;
        _profile = profile;
        _seed = seed;
        BuildUi();
        if (profile is not null)
        {
            SelectRace(profile.Race);
            _race.Disabled = true;
        }
        ShowTerrainStage();
    }

    public void InitializeOverview(
        StrategicWorldRuntime world, int seed, int regionId, string playerRace)
    {
        _overviewOnly = true;
        BuildUi();
        _stage = SetupStage.Finish;
        _world = world;
        _seed = seed;
        _selectedRegion = regionId;
        SelectRace(playerRace);
        _race.Disabled = true;
        _title.Text = "Глобальная карта";
        _generationPanel.Visible = false;
        _selectionPanel.Visible = true;
        _confirm.Text = "";
        _confirm.TooltipText = "Вернуться в поселение";
        CenterMapOnRegion(regionId);
        RebuildWorldMapTexture();
        RebuildRealmMapTexture();
        RebuildRoadMapTexture();
        RefreshDetails();
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
        => HandleMapInput(inputEvent, Vector2.Zero);

    private void HandleMapInput(InputEvent inputEvent, Vector2 positionOffset)
    {
        if (_world is null || !_selectionPanel.Visible) return;
        if (inputEvent is InputEventMouseButton mouse)
        {
            var mapPosition = mouse.Position + positionOffset;
            if (mouse.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown && mouse.Pressed &&
                MapViewport.HasPoint(mapPosition))
            {
                _mapZoom = Math.Clamp(_mapZoom *
                    (mouse.ButtonIndex == MouseButton.WheelUp ? 2f : 0.5f), 16f, 64f);
                ClampMapCenter(); QueueRedraw(); AcceptEvent();
                return;
            }
            if (mouse.ButtonIndex is MouseButton.Middle or MouseButton.Right)
            {
                _panning = mouse.Pressed && MapViewport.HasPoint(mapPosition);
                _lastMouse = mapPosition;
                AcceptEvent();
                return;
            }
            if (mouse.ButtonIndex == MouseButton.Left && !mouse.Pressed && _painting)
            {
                FinishPaintStroke(); AcceptEvent();
                return;
            }
            if (mouse.ButtonIndex != MouseButton.Left || !MapViewport.HasPoint(mapPosition)) return;
            if (_editMode && !_overviewOnly)
            {
                _painting = mouse.Pressed;
                if (mouse.Pressed) PaintTerrain(mapPosition);
                else FinishPaintStroke();
                AcceptEvent();
                return;
            }
            if (mouse.Pressed && !_overviewOnly && !_worldFinalized) SelectSettlementRegion(mapPosition);
            return;
        }
        if (inputEvent is not InputEventMouseMotion motion) return;
        var motionPosition = motion.Position + positionOffset;
        if (_panning)
        {
            _mapCenter -= (motionPosition - _lastMouse) / _mapZoom;
            _lastMouse = motionPosition;
            ClampMapCenter(); QueueRedraw(); AcceptEvent();
        }
        else if (_painting && _editMode && MapViewport.HasPoint(motionPosition))
        {
            PaintTerrain(motionPosition);
            AcceptEvent();
        }
        else if (MapViewport.HasPoint(motionPosition))
        {
            var tile = ScreenToMap(motionPosition);
            _hoveredTile = new Vector2I(Math.Clamp((int)tile.X, 0, StrategicWorldRuntime.TileDimension - 1),
                Math.Clamp((int)tile.Y, 0, StrategicWorldRuntime.TileDimension - 1));
            _hoveredRegion = _world.RegionAtTile(_hoveredTile.X, _hoveredTile.Y)?.Id ?? -1;
            PositionDetailsPopup(motionPosition);
            RefreshDetails();
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_world is null || !_selectionPanel.Visible) return;
        if (_generatingCivilizations)
        {
            DrawRect(MapViewport, new Color("070b0f"));
            return;
        }
        if (_worldMapTexture is null) RebuildWorldMapTexture();
        UpdateMapSource();
        DrawRect(MapViewport, new Color("070b0f"));
        DrawTextureRectRegion(_worldMapTexture!, MapViewport,
            new Rect2(_mapSource.Position * WorldTextureScale, _mapSource.Size * WorldTextureScale));
        DrawRect(MapViewport, new Color("88939a"), false, 2f);
        if (_mapZoom >= 32f)
        {
            var font = ThemeDB.FallbackFont;
            foreach (var landmark in _world.Landmarks)
            {
                var position = MapToScreen(new Vector2(landmark.CenterTileX + 0.5f,
                    landmark.CenterTileY + 0.5f));
                if (!MapViewport.HasPoint(position)) continue;
                var width = font.GetStringSize(landmark.Name, HorizontalAlignment.Left, -1, 11).X;
                DrawString(font, position + new Vector2(-width / 2f, 4), landmark.Name,
                    HorizontalAlignment.Left, -1, 11, new Color("c7d7df"));
            }
        }
        if (_worldFinalized || _overviewOnly)
        {
            if (_realmMapTexture is null) RebuildRealmMapTexture();
            DrawTextureRectRegion(_realmMapTexture!, MapViewport,
                new Rect2(_mapSource.Position * WorldTextureScale,
                    _mapSource.Size * WorldTextureScale));
            if (_roadMapTexture is null) RebuildRoadMapTexture();
            DrawTextureRectRegion(_roadMapTexture!, MapViewport,
                new Rect2(_mapSource.Position * WorldTextureScale,
                    _mapSource.Size * WorldTextureScale));
        }
        foreach (var region in _world.Regions)
        {
            var center = MapToScreen(new Vector2(region.CenterTileX + 0.5f, region.CenterTileY + 0.5f));
            if (region.Capital && region.OwnerFactionId != _world.PlayerFactionId &&
                MapViewport.HasPoint(center))
                DrawCircle(center, Math.Max(2f, _mapZoom * 0.18f), new Color("d6a45d"));
        }
        DrawCapitalFootprint();
        if (_worldFinalized || _overviewOnly)
            foreach (var city in _world.Cities)
            {
                var region = _world.Region(city.RegionId);
                if (region is null) continue;
                var position = MapToScreen(RegionTile(region, null));
                if (!MapViewport.HasPoint(position)) continue;
                DrawOriginalTown(city, position);
            }
        if ((_worldFinalized || _overviewOnly) && _mapZoom >= 24f)
        {
            var font = ThemeDB.FallbackFont;
            foreach (var region in _world.Regions)
            {
                var position = MapToScreen(RegionTile(region, null));
                if (!MapViewport.HasPoint(position)) continue;
                var width = font.GetStringSize(region.Name, HorizontalAlignment.Left, -1, 12).X;
                DrawString(font, position + new Vector2(-width / 2f, -8), region.Name,
                    HorizontalAlignment.Left, -1, 12, new Color("ead9aa"));
            }
        }
        DrawTextureRect(_worldMapTexture!, MiniMapViewport, false);
        if ((_worldFinalized || _overviewOnly) && _realmMapTexture is not null)
            DrawTextureRect(_realmMapTexture, MiniMapViewport, false);
        if ((_worldFinalized || _overviewOnly) && _roadMapTexture is not null)
            DrawTextureRect(_roadMapTexture, MiniMapViewport, false);
        var miniScale = MiniMapViewport.Size / StrategicWorldRuntime.TileDimension;
        DrawRect(new Rect2(MiniMapViewport.Position + _mapSource.Position * miniScale,
            _mapSource.Size * miniScale), Colors.White, false, 2f);
        DrawRect(MiniMapViewport, new Color("b8ad87"), false, 2f);
    }

    private void BuildUi()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        var background = new ColorRect
        {
            Color = new Color("101419"),
            MouseFilter = MouseFilterEnum.Ignore,
            ShowBehindParent = true
        };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(background);
        _title = new Label
        {
            Text = "Разместите столицу",
            Position = new Vector2(686, 96), Size = new Vector2(264, 38),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _title.AddThemeFontSizeOverride("font_size", 20);
        AddChild(_title);
        BuildGenerationPanel();
        _selectionPanel = new Control();
        _selectionPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_selectionPanel);
        var mapInput = new Control
        {
            Position = MapViewport.Position,
            Size = MapViewport.Size,
            MouseFilter = MouseFilterEnum.Stop
        };
        mapInput.GuiInput += inputEvent => HandleMapInput(inputEvent, MapViewport.Position);
        _selectionPanel.AddChild(mapInput);
        _race = new OptionButton { Visible = false };
        foreach (var race in OriginalGameData.Current.Races.Values
                     .Where(candidate => candidate.Playable).OrderBy(candidate => candidate.Key))
        {
            _race.AddItem(LocalizeRace(race.Key));
            _race.SetItemMetadata(_race.ItemCount - 1, race.Key);
        }
        _race.ItemSelected += _ => RefreshDetails();
        _selectionPanel.AddChild(_race);
        _detailsBackground = new ColorRect
        {
            Position = new Vector2(720, 450), Size = new Vector2(560, 328),
            Color = new Color(0.025f, 0.025f, 0.03f, 0.94f), MouseFilter = MouseFilterEnum.Ignore
        };
        _selectionPanel.AddChild(_detailsBackground);
        _detailsIcon = new TextureRect
        {
            Position = new Vector2(734, 462), Size = new Vector2(24, 24),
            Texture = OriginalIcon(OriginalIconsPath, 1, 0),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _selectionPanel.AddChild(_detailsIcon);
        _details = new Label
        {
            Position = new Vector2(738, 462),
            Size = new Vector2(524, 304),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _details.AddThemeFontSizeOverride("font_size", 13);
        _details.AddThemeConstantOverride("line_spacing", 1);
        _selectionPanel.AddChild(_details);
        AddDetailStatIcon("res://Data/Original/assets/sprite/icon/24/resource/Grain.png", 738, 742);
        AddDetailStatIcon("res://Data/Original/assets/sprite/icon/24/resource/Wood.png", 770, 742);
        AddDetailStatIcon("res://Data/Original/assets/sprite/icon/32/WATER.png", 802, 738, 32);
        BuildTerrainEditor();
        _confirm = new Button
        {
            Text = "", TooltipText = "Продолжить / основать поселение",
            Icon = OriginalIcon(OriginalUiIconsPath, 2, 2),
            Position = new Vector2(846, 140), Size = new Vector2(44, 44)
        };
        _confirm.Pressed += Confirm;
        _selectionPanel.AddChild(_confirm);
        _regenerate = new Button
        {
            Text = "", TooltipText = "Создать другой мир",
            Icon = OriginalIcon(OriginalIconsPath, 1, 0),
            Position = new Vector2(750, 140), Size = new Vector2(44, 44)
        };
        _regenerate.Pressed += RegenerateCurrentStage;
        _regenerate.Visible = !_overviewOnly;
        _selectionPanel.AddChild(_regenerate);
        _back = new Button
        {
            Text = "←", TooltipText = "Назад",
            Position = new Vector2(702, 140), Size = new Vector2(44, 44)
        };
        _back.Pressed += BackFromWorldStage;
        _selectionPanel.AddChild(_back);
        _home = new Button
        {
            Text = "", TooltipText = "К столице",
            Icon = OriginalIcon(OriginalUiIconsPath, 4, 3),
            Position = new Vector2(798, 140), Size = new Vector2(44, 44),
            Visible = false
        };
        _home.Pressed += CenterOnPlayerCapital;
        _selectionPanel.AddChild(_home);
        var menu = new Button
        {
            Text = "×", TooltipText = "Главное меню",
            Position = new Vector2(894, 140), Size = new Vector2(44, 44),
            Visible = _overviewOnly
        };
        menu.Pressed += () => MainMenuRequested?.Invoke();
        _selectionPanel.AddChild(menu);
        BuildMiniMapToolbar();
        BuildBusyOverlay();
    }

    private void BuildMiniMapToolbar()
    {
        AddMiniMapButton(1344, OriginalUiIconsPath, 0, 0, "Население", () => { });
        AddMiniMapButton(1384, OriginalIconsPath, 2, 2, "Города", () => { });
        AddMiniMapButton(1424, OriginalUiIconsPath, 2, 2, "К выбранной области", () =>
        {
            if (_selectedRegion >= 0) CenterMapOnRegion(_selectedRegion);
        });
        AddMiniMapButton(1464, OriginalIconsPath, 1, 0, "Рельеф", () => { });
        AddMiniMapButton(1504, OriginalIconsPath, 3, 3, "Приблизить", () =>
        {
            _mapZoom = Math.Clamp(_mapZoom * 2f, 16f, 64f); ClampMapCenter(); QueueRedraw();
        });
        AddMiniMapButton(1544, OriginalIconsPath, 4, 3, "Отдалить", () =>
        {
            _mapZoom = Math.Clamp(_mapZoom * 0.5f, 16f, 64f); ClampMapCenter(); QueueRedraw();
        });
    }

    private void AddMiniMapButton(float x, string atlasPath, int column, int row,
        string tooltip, Action pressed)
    {
        var button = new Button
        {
            Position = new Vector2(x, 28), Size = new Vector2(38, 34), Text = "",
            Icon = OriginalIcon(atlasPath, column, row), TooltipText = tooltip
        };
        button.Pressed += pressed;
        _selectionPanel.AddChild(button);
    }

    private static Texture2D? OriginalIcon(string atlasPath, int column, int row)
    {
        var atlas = GD.Load<Texture2D>(atlasPath);
        if (atlas is null) return null;
        return new AtlasTexture
        {
            Atlas = atlas,
            Region = new Rect2(6 + column * 38, 6 + row * 38, 32, 32),
            FilterClip = true
        };
    }

    private static Texture2D? OriginalSingleIcon(string atlasPath, int size)
    {
        var atlas = GD.Load<Texture2D>(atlasPath);
        if (atlas is null) return null;
        return new AtlasTexture
        {
            Atlas = atlas,
            Region = new Rect2(6, 6, size, size),
            FilterClip = true
        };
    }

    private void BuildBusyOverlay()
    {
        _busyOverlay = new ColorRect
        {
            Color = new Color(0.015f, 0.018f, 0.022f, 0.9f),
            MouseFilter = MouseFilterEnum.Stop,
            Visible = false
        };
        _busyOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_busyOverlay);
        _busyLabel = new Label
        {
            Position = new Vector2(500, 380), Size = new Vector2(600, 140),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _busyLabel.AddThemeFontSizeOverride("font_size", 24);
        _busyOverlay.AddChild(_busyLabel);
    }

    private void SetBusy(bool busy, string text = "")
    {
        if (_busyOverlay is null) return;
        _busyLabel.Text = text;
        _busyOverlay.Visible = busy;
    }

    private void BuildTerrainEditor()
    {
        _editTerrain = new Button
        {
            Text = "", TooltipText = "Редактор карты",
            Icon = OriginalIcon(OriginalIconsPath, 3, 3),
            Position = new Vector2(798, 140),
            Size = new Vector2(44, 44), ToggleMode = true, Visible = !_overviewOnly
        };
        _editTerrain.Toggled += enabled =>
        {
            _editMode = enabled;
            _terrainTool.Disabled = !enabled;
            _brushSize.Editable = enabled;
            _terrainTool.Visible = enabled;
            _brushLabel.Visible = enabled;
            _brushSize.Visible = enabled;
            _editorHint.Visible = enabled;
            _editorHint.Text = enabled
                ? "ЛКМ — рисовать · ПКМ/WASD/края — двигать · колесо — масштаб"
                : "ЛКМ — выбрать · ПКМ/WASD/края — двигать · колесо — масштаб";
        };
        _selectionPanel.AddChild(_editTerrain);
        _terrainTool = new OptionButton
        {
            Position = new Vector2(1344, 300), Size = new Vector2(240, 36),
            Disabled = true, Visible = false
        };
        foreach (var name in new[]
                 {
                     "Грунт", "Гора", "Понизить гору", "Озеро", "Глубокое озеро",
                     "Океан", "Глубокий океан", "Река", "Малая река", "Убрать воду",
                     "Лес", "Убрать лес", "Холодный климат", "Умеренный климат", "Жаркий климат"
                 })
            _terrainTool.AddItem(name);
        _selectionPanel.AddChild(_terrainTool);
        _brushLabel = new Label
        {
            Text = "Размер кисти", Position = new Vector2(1344, 344), Visible = false
        };
        _selectionPanel.AddChild(_brushLabel);
        _brushSize = new SpinBox
        {
            Position = new Vector2(1344, 370), Size = new Vector2(116, 34),
            MinValue = 1, MaxValue = 12, Step = 1, Value = 2,
            Editable = false, Visible = false
        };
        _selectionPanel.AddChild(_brushSize);
        _editorHint = new Label
        {
            Text = "ЛКМ — выбрать · ПКМ/WASD/края — двигать · колесо — масштаб",
            Position = new Vector2(1344, 418), Size = new Vector2(240, 110),
            AutowrapMode = TextServer.AutowrapMode.WordSmart, Visible = false
        };
        _selectionPanel.AddChild(_editorHint);
    }

    private void AddDetailStatIcon(string path, float x, float y, float size = 24)
    {
        var icon = new TextureRect
        {
            Position = new Vector2(x, y), Size = new Vector2(size, size),
            Texture = OriginalSingleIcon(path, (int)size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _selectionPanel.AddChild(icon);
        _detailStatIcons.Add(icon);
    }

    private void PositionDetailsPopup(Vector2 cursor)
    {
        const float margin = 18f;
        var position = cursor + new Vector2(24, 24);
        if (position.X + _detailsBackground.Size.X > Size.X - margin)
            position.X = cursor.X - _detailsBackground.Size.X - 24;
        if (position.Y + _detailsBackground.Size.Y > Size.Y - margin)
            position.Y = cursor.Y - _detailsBackground.Size.Y - 24;
        position.X = Math.Clamp(position.X, margin, Math.Max(margin, Size.X - _detailsBackground.Size.X - margin));
        position.Y = Math.Clamp(position.Y, 64f, Math.Max(64f, Size.Y - _detailsBackground.Size.Y - margin));
        var delta = position - _detailsBackground.Position;
        _detailsBackground.Position += delta;
        _details.Position += delta;
        _detailsIcon.Position += delta;
        foreach (var icon in _detailStatIcons) icon.Position += delta;
    }

    private void BuildGenerationPanel()
    {
        _generationPanel = new Control();
        _generationPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_generationPanel);
        var heading = new Label { Text = "Выберите тип глобальной карты", Position = new Vector2(420, 82) };
        heading.AddThemeFontSizeOverride("font_size", 18);
        _generationPanel.AddChild(heading);
        _mapPreview = new TextureRect
        {
            Position = new Vector2(420, 122), Size = new Vector2(260, 260),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        _generationPanel.AddChild(_mapPreview);
        _mapType = new OptionButton { Position = new Vector2(710, 122), Size = new Vector2(330, 38) };
        _mapType.ItemSelected += _ => SelectMapType();
        _generationPanel.AddChild(_mapType);
        var previousTemplate = new Button
        {
            Text = "<", Position = new Vector2(374, 224), Size = new Vector2(38, 58)
        };
        previousTemplate.Pressed += () => StepMapType(-1);
        _generationPanel.AddChild(previousTemplate);
        var nextTemplate = new Button
        {
            Text = ">", Position = new Vector2(688, 224), Size = new Vector2(38, 58)
        };
        nextTemplate.Pressed += () => StepMapType(1);
        _generationPanel.AddChild(nextTemplate);
        _generationPanel.AddChild(new Label { Text = "Широта экватора", Position = new Vector2(710, 188) });
        _latitude = new HSlider
        {
            Position = new Vector2(710, 216), Size = new Vector2(330, 28),
            MinValue = 0, MaxValue = 100, Step = 1, Value = 50
        };
        _generationPanel.AddChild(_latitude);
        _generationPanel.AddChild(new Label { Text = "Север", Position = new Vector2(710, 246) });
        _generationPanel.AddChild(new Label { Text = "Юг", Position = new Vector2(1008, 246) });
        _generationPanel.AddChild(new Label { Text = "Случайное зерно", Position = new Vector2(710, 286) });
        _seedInput = new LineEdit
        {
            Position = new Vector2(710, 314), Size = new Vector2(330, 38),
            Text = _seed.ToString(), MaxLength = 10
        };
        _seedInput.TextChanged += KeepSeedDigits;
        _generationPanel.AddChild(_seedInput);
        var generate = new Button
        {
            Text = "Создать глобальную карту",
            Position = new Vector2(710, 382), Size = new Vector2(250, 44)
        };
        generate.Pressed += GenerateConfiguredWorld;
        _generationPanel.AddChild(generate);
        var back = new Button { Text = "Назад", Position = new Vector2(970, 382), Size = new Vector2(140, 44) };
        back.Pressed += () => BackRequested?.Invoke();
        _generationPanel.AddChild(back);
        _generationStatus = new Label
        {
            Position = new Vector2(420, 454), Size = new Vector2(690, 80),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _generationPanel.AddChild(_generationStatus);
        LoadTerrainTemplates();
    }

    private void LoadTerrainTemplates()
    {
        _templates.Clear();
        _templateTextures.Clear();
        _mapType.Clear();
        _templates.Add(null);
        _templateTextures.Add(null);
        _mapType.AddItem("Случайная");
        foreach (var file in DirAccess.GetFilesAt(GeneratorMapsPath)
                     .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
        {
            var image = LoadGeneratorImage($"{GeneratorMapsPath}/{file}");
            if (image is null) continue;
            if (image.GetWidth() != image.GetHeight())
                throw new System.IO.InvalidDataException(
                    $"Generator map must be square: {file} ({image.GetWidth()}x{image.GetHeight()})");
            var heights = new byte[image.GetWidth() * image.GetHeight()];
            for (var y = 0; y < image.GetHeight(); y++)
            for (var x = 0; x < image.GetWidth(); x++)
                heights[x + y * image.GetWidth()] = (byte)image.GetPixel(x, y).G8;
            var name = System.IO.Path.GetFileNameWithoutExtension(file);
            _templates.Add(new StrategicTerrainTemplate(name, image.GetWidth(), image.GetHeight(), heights));
            _templateTextures.Add(ImageTexture.CreateFromImage(BuildTemplatePreview(
                _templates[^1]!)));
            _mapType.AddItem(name);
        }
        SelectMapType();
    }

    private static Image? LoadGeneratorImage(string path)
    {
        var bytes = FileAccess.GetFileAsBytes(path);
        if (bytes.Length < 4) return null;
        var image = new Image();
        var jpeg = bytes[0] == 0xff && bytes[1] == 0xd8;
        var error = jpeg ? image.LoadJpgFromBuffer(bytes) : image.LoadPngFromBuffer(bytes);
        return error == Error.Ok ? image : null;
    }

    private static Image BuildTemplatePreview(StrategicTerrainTemplate template)
    {
        var preview = Image.CreateEmpty(template.Width * 2, template.Height * 2,
            false, Image.Format.Rgb8);
        var colors = new[]
        {
            new Color(25 / 255f, 25 / 255f, 50 / 255f),
            new Color(50 / 255f, 60 / 255f, 20 / 255f),
            new Color(30 / 255f, 25 / 255f, 25 / 255f)
        };
        for (var y = 0; y < template.Height; y++)
        for (var x = 0; x < template.Width; x++)
        {
            var source = template.Heights[x + y * template.Width];
            var color = colors[Math.Clamp((int)(source / (double)StrategicWorldRuntime.TileDimension *
                colors.Length), 0, colors.Length - 1)];
            preview.FillRect(new Rect2I(x * 2, y * 2, 2, 2), color);
        }
        return preview;
    }

    private void KeepSeedDigits(string value)
    {
        var digits = new string(value.Where(char.IsDigit).Take(10).ToArray());
        if (digits == value) return;
        _seedInput.Text = digits.Length == 0 ? "1" : digits;
        _seedInput.CaretColumn = _seedInput.Text.Length;
    }

    private void SelectMapType()
    {
        var index = Math.Clamp(_mapType.Selected, 0, _templateTextures.Count - 1);
        _mapPreview.Texture = _templateTextures[index];
        _generationStatus.Text = index == 0
            ? $"Случайная генерация · доступно шаблонов: {_templates.Count - 1}"
            : $"Шаблон: {_templates[index]!.Name} · {_templates[index]!.Width}×{_templates[index]!.Height}";
    }

    private void StepMapType(int direction)
    {
        if (_templates.Count == 0) return;
        var index = (_mapType.Selected + direction) % _templates.Count;
        if (index < 0) index += _templates.Count;
        _mapType.Select(index);
        SelectMapType();
    }

    private void ShowTerrainStage()
    {
        _stage = SetupStage.Terrain;
        SetBusy(false);
        _painting = false;
        _panning = false;
        _editMode = false;
        if (_editTerrain is not null) _editTerrain.ButtonPressed = false;
        _generationPanel.Visible = true;
        _selectionPanel.Visible = false;
        _title.Text = "Новая игра — генерация мира";
        QueueRedraw();
    }

    private void GenerateConfiguredWorld()
    {
        if (!int.TryParse(_seedInput.Text, out var seed) || seed < 0)
        {
            _generationStatus.Text = "Зерно должно быть целым числом от 0 до 2147483647.";
            return;
        }
        if (_world is not null && (_selectedCapitalTile.X >= 0 || _worldFinalized))
        {
            var dialog = new ConfirmationDialog
            {
                Title = "Перегенерировать мир",
                DialogText = "Перегенерация территории сбросит выбранное место и текущий мир. Продолжить?"
            };
            dialog.Confirmed += () => GenerateConfiguredWorld(seed);
            dialog.Canceled += dialog.QueueFree;
            dialog.Confirmed += dialog.QueueFree;
            AddChild(dialog);
            dialog.PopupCentered(new Vector2I(560, 180));
            return;
        }
        GenerateConfiguredWorld(seed);
    }

    private async void GenerateConfiguredWorld(int seed)
    {
        SetBusy(true, "Создание рельефа, климата, рек и регионов…");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _seed = seed;
        _worldFinalized = false;
        var templateIndex = Math.Clamp(_mapType.Selected, 0, _templates.Count - 1);
        try
        {
            _world = new StrategicWorldRuntime();
            _world.GenerateTerrain(_seed,
                _templates[templateIndex], _latitude.Value / 100.0);
            RebuildWorldMapTexture();
        }
        catch (Exception exception)
        {
            SetBusy(false);
            _generationStatus.Text = "Ошибка генерации: " + exception.Message;
            GD.PushError(exception.ToString());
            return;
        }
        GD.Print($"[LAUNCH] terrain_ready seed={_seed} regions={_world.Regions.Count}");
        _selectedRegion = -1;
        _selectedCapitalTile = new Vector2I(-1, -1);
        _mapCenter = new Vector2(StrategicWorldRuntime.TileDimension / 2f,
            StrategicWorldRuntime.TileDimension / 2f);
        ClampMapCenter();
        _stage = SetupStage.Capital;
        SetBusy(false);
        _generationPanel.Visible = false;
        _selectionPanel.Visible = true;
        _editTerrain.Disabled = false;
        _confirm.Text = "";
        _confirm.TooltipText = "Создать мир";
        _regenerate.Text = "";
        _regenerate.TooltipText = "Новый рельеф";
        _home.Visible = false;
        _title.Text = "Разместите столицу";
        RefreshDetails();
        QueueRedraw();
    }

    private void SelectSettlementRegion(Vector2 screenPosition)
    {
        var tile = ScreenToMap(screenPosition);
        var center = new Vector2I(Math.Clamp((int)tile.X, 0, StrategicWorldRuntime.TileDimension - 1),
            Math.Clamp((int)tile.Y, 0, StrategicWorldRuntime.TileDimension - 1));
        var region = _world.RegionAtTile(center.X, center.Y);
        if (region is null || !_world.CanSetPlayerStart(region.Id) ||
            !_world.CanPlaceCapitalAt(center.X, center.Y))
        {
            _details.Text = "Здесь нельзя разместить столицу: площадка должна помещаться на карте, а её центр — иметь свободный выход.";
            return;
        }
        _selectedRegion = region.Id;
        _selectedCapitalTile = center;
        RefreshDetails();
        QueueRedraw();
        AcceptEvent();
    }

    private void PaintTerrain(Vector2 screenPosition)
    {
        var tile = ScreenToMap(screenPosition);
        var edit = (StrategicTerrainEdit)Math.Clamp(_terrainTool.Selected, 0,
            Enum.GetValues<StrategicTerrainEdit>().Length - 1);
        if (!_world.ApplyTerrainEdit(edit, (int)tile.X, (int)tile.Y, (int)_brushSize.Value)) return;
        _terrainDirty = true;
        var now = Time.GetTicksMsec();
        if (now - _lastTerrainTextureBuildMsec >= 80)
        {
            RebuildWorldMapTexture();
            _lastTerrainTextureBuildMsec = now;
        }
        QueueRedraw();
    }

    private void FinishPaintStroke()
    {
        _painting = false;
        if (!_terrainDirty) return;
        _terrainDirty = false;
        RebuildWorldMapTexture();
        _lastTerrainTextureBuildMsec = Time.GetTicksMsec();
        _world.FinishTerrainEditing();
        if (_selectedCapitalTile.X >= 0)
            _selectedRegion = _world.RegionAtTile(
                _selectedCapitalTile.X, _selectedCapitalTile.Y)?.Id ?? -1;
        if (_selectedRegion >= 0 && (!_world.CanSetPlayerStart(_selectedRegion) ||
            !_world.CanPlaceCapitalAt(_selectedCapitalTile.X, _selectedCapitalTile.Y)))
        {
            _selectedRegion = -1;
            _selectedCapitalTile = new Vector2I(-1, -1);
        }
        RefreshDetails();
        QueueRedraw();
    }

    private void RebuildWorldMapTexture()
    {
        if (_world is null) return;
        _worldMapTexture = OriginalWorldMapTextureBuilder.Build(_world);
    }

    private Color ApplyOriginalGroundDetail(Color tint, int tileX, int tileY, int pixelX, int pixelY)
    {
        if (_worldGroundAtlas is null) return tint;
        var water = _world.Terrain.Water(tileX, tileY);
        var origin = water != StrategicWaterKind.None ? new Vector2I(146, 216) :
            _world.Terrain.Mountain(tileX, tileY) ? new Vector2I(6, 216) :
            _world.Terrain.Forest(tileX, tileY) > 0.55 ? new Vector2I(6, 356) :
            new Vector2I(6, 76);
        var sourceX = origin.X + (tileX & 7) * 16 + pixelX;
        var sourceY = origin.Y + (tileY & 7) * 16 + pixelY;
        if ((uint)sourceX >= (uint)_worldGroundAtlas.GetWidth() ||
            (uint)sourceY >= (uint)_worldGroundAtlas.GetHeight()) return tint;
        var sample = AtlasPixel(_worldGroundAtlas, sourceX, sourceY);
        if (sample.A <= 0) return tint;
        var luminance = sample.R * 0.299f + sample.G * 0.587f + sample.B * 0.114f;
        var detailFactor = 0.58f + luminance * 0.58f;
        var factor = Mathf.Lerp(1f, detailFactor, sample.A);
        return new Color(Math.Clamp(tint.R * factor, 0, 1),
            Math.Clamp(tint.G * factor, 0, 1),
            Math.Clamp(tint.B * factor, 0, 1), 1);
    }

    private Color ApplyOriginalTerrainLayers(Color tint, int tileX, int tileY, int pixelX, int pixelY,
        StrategicWaterKind water, int waterMask, int riverMask)
    {
        if ((water is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake or
             StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean) && _worldWaterAtlas is not null)
        {
            if (waterMask != 15)
            {
                var shore = SampleHouseTile(_worldWaterAtlas, 2, tileX, tileY,
                    waterMask, pixelX, pixelY);
                tint = ModulateFromMask(tint, shore, new Color("9a9b82"), 0.52f);
            }
            var foregroundRow = water is StrategicWaterKind.DeepOcean or StrategicWaterKind.DeepLake
                ? 210 : 100;
            var foreground = SampleHouseTile(_worldWaterAtlas, foregroundRow, tileX, tileY,
                waterMask, pixelX, pixelY);
            tint = ModulateFromMask(tint, foreground, water is StrategicWaterKind.DeepOcean or
                StrategicWaterKind.DeepLake ? new Color("344f6d") : new Color("587895"), 0.68f);
        }

        if (water is StrategicWaterKind.River or StrategicWaterKind.Delta && _worldRiverAtlas is not null)
        {
            var bank = SampleHouseTile(_worldRiverAtlas, 2, tileX, tileY,
                riverMask, pixelX, pixelY);
            tint = ModulateFromMask(tint, bank, new Color("929178"), 0.48f);
            var stream = SampleHouseTile(_worldRiverAtlas, 74, tileX, tileY,
                riverMask, pixelX, pixelY);
            tint = ModulateFromMask(tint, stream, new Color("668b9d"), 0.76f);
        }
        else if (water == StrategicWaterKind.SmallRiver && _worldRiverSmallAtlas is not null)
        {
            var bank = SampleHouseTile(_worldRiverSmallAtlas, 2, tileX, tileY,
                riverMask, pixelX, pixelY);
            tint = ModulateFromMask(tint, bank, new Color("929178"), 0.42f);
            var stream = SampleHouseTile(_worldRiverSmallAtlas, 74, tileX, tileY,
                riverMask, pixelX, pixelY);
            tint = ModulateFromMask(tint, stream, new Color("7395a2"), 0.72f);
        }

        if (_world.Terrain.Mountain(tileX, tileY) && _worldMountainAtlas is not null)
        {
            var variant = StableTileVariant(tileX, tileY, 16);
            var sample = AtlasPixel(_worldMountainAtlas,
                6 + variant * 16 + pixelX, 200 + pixelY);
            tint = ModulateFromMask(tint, sample, new Color("c2b9a5"), 0.68f);
        }
        return tint;
    }

    private void ApplyForestCanopies(byte[] pixels, int textureSize)
    {
        if (_worldForestAtlas is null) return;
        for (var tileY = 0; tileY < StrategicWorldRuntime.TileDimension; tileY++)
        for (var tileX = 0; tileX < StrategicWorldRuntime.TileDimension; tileX++)
        {
            var forest = _world.Terrain.Forest(tileX, tileY);
            if (forest <= 0 || _world.Terrain.IsWater(tileX, tileY)) continue;
            var level = Math.Clamp((int)Math.Ceiling(forest * 3), 1, 3);
            var variant = StableTileVariant(tileX, tileY, 16);
            var sourceOriginX = 4 + variant * 30;
            var sourceOriginY = 4 + (level + 2) * 31;
            var destinationOriginX = tileX * WorldTextureScale - 4;
            var destinationOriginY = tileY * WorldTextureScale - 4;
            var climate = _world.Terrain.Climate(tileX, tileY);
            var foliage = climate == "COLD" ? new Color("53614d") :
                climate == "HOT" ? new Color("59633a") : new Color("315425");
            for (var sourceY = 0; sourceY < 24; sourceY++)
            for (var sourceX = 0; sourceX < 24; sourceX++)
            {
                var destinationX = destinationOriginX + sourceX;
                var destinationY = destinationOriginY + sourceY;
                if ((uint)destinationX >= (uint)textureSize ||
                    (uint)destinationY >= (uint)textureSize) continue;
                var sample = AtlasPixel(_worldForestAtlas,
                    sourceOriginX + sourceX, sourceOriginY + sourceY);
                if (sample.A <= 0) continue;
                var luminance = Math.Clamp(sample.R * 0.299f + sample.G * 0.587f +
                    sample.B * 0.114f, 0.12f, 1f);
                var source = new Color(foliage.R * luminance, foliage.G * luminance,
                    foliage.B * luminance, sample.A);
                BlendRgb(pixels, textureSize, destinationX, destinationY, source,
                    Math.Clamp(0.62f + (float)forest * 0.34f, 0, 0.96f));
            }
        }
    }

    private static void BlendRgb(byte[] pixels, int size, int x, int y, Color source, float strength)
    {
        var offset = (x + y * size) * 3;
        var alpha = Math.Clamp(source.A * strength, 0, 1);
        pixels[offset] = (byte)Math.Clamp(pixels[offset] * (1 - alpha) + source.R8 * alpha, 0, 255);
        pixels[offset + 1] = (byte)Math.Clamp(pixels[offset + 1] * (1 - alpha) + source.G8 * alpha, 0, 255);
        pixels[offset + 2] = (byte)Math.Clamp(pixels[offset + 2] * (1 - alpha) + source.B8 * alpha, 0, 255);
    }

    private Color SampleHouseTile(Image atlas, int rowOrigin, int tileX, int tileY,
        int mask, int pixelX, int pixelY)
    {
        // SpriteComposer's house.paste(1, true) creates eight runtime variants
        // from four source houses: unrotated, then a 90-degree duplicate.
        // The right half of the PNG is a normal map, not diffuse artwork.
        var runtimeVariant = StableTileVariant(tileX, tileY, 8);
        var sourceVariant = runtimeVariant >> 1;
        var rotated = (runtimeVariant & 1) != 0;
        var sourceMask = rotated ? RotateMaskRight(mask) : mask;
        var sourcePixelX = rotated ? pixelY : pixelX;
        var sourcePixelY = rotated ? WorldTextureScale - 1 - pixelX : pixelY;
        var sourceX = 2 + sourceVariant * 72 + HouseOffsetX[sourceMask] + sourcePixelX;
        var sourceY = rowOrigin + HouseOffsetY[sourceMask] + sourcePixelY;
        return AtlasPixel(atlas, sourceX, sourceY);
    }

    private static int RotateMaskRight(int mask) => (mask >> 1) | ((mask & 1) << 3);

    private void DrawOriginalTown(StrategicCity city, Vector2 center)
    {
        var faction = _world.Faction(city.FactionId);
        var race = faction?.Race ?? "Normal";
        var sheetName = race.Equals("HUMAN", StringComparison.OrdinalIgnoreCase) ? "Human" :
            race.Equals("CRETONIAN", StringComparison.OrdinalIgnoreCase) ? "Cretonian" :
            race.Equals("GARTHIMI", StringComparison.OrdinalIgnoreCase) ? "Garthimi" : "Normal";
        if (!_townTextures.TryGetValue(sheetName, out var texture))
        {
            texture = GD.Load<Texture2D>(
                $"res://Data/Original/assets/sprite/world/centre/town/{sheetName}.png");
            _townTextures[sheetName] = texture;
        }
        if (texture is null) return;
        var dimension = city.Capital ? 4 : 2;
        var spriteSize = Math.Max(4f, _mapZoom * 0.5f);
        for (var y = 0; y < dimension; y++)
        for (var x = 0; x < dimension; x++)
        {
            var variant = StableTileVariant(city.Id * 7 + x, city.RegionId * 11 + y, 16);
            var level = city.Capital ? 3 : 1;
            var source = new Rect2(6 + variant * 14, 6 + level * 14, 8, 8);
            var destination = new Rect2(
                center.X + (x - dimension / 2f) * spriteSize,
                center.Y + (y - dimension / 2f) * spriteSize,
                spriteSize, spriteSize);
            DrawTextureRectRegion(texture, destination, source);
        }
    }

    private static Color AtlasPixel(Image atlas, int x, int y)
    {
        if ((uint)x >= (uint)atlas.GetWidth() || (uint)y >= (uint)atlas.GetHeight())
            return Colors.Transparent;
        var sample = atlas.GetPixel(x, y);
        // Sprite-composer sources use saturated guide/chroma colours which the
        // Java composer removes. They must not become visible world pixels.
        var chromaBlue = sample.B > 0.82f && sample.B > sample.R * 1.12f &&
            sample.B > sample.G * 1.12f;
        var guideBlue = sample.R < 0.08f && sample.G < 0.16f && sample.B > 0.34f;
        var guideRed = sample.R > 0.88f && sample.G < 0.18f && sample.B < 0.18f;
        var guideGreen = sample.G > 0.88f && sample.R < 0.18f && sample.B < 0.18f;
        if (sample.A < 0.05f || chromaBlue || guideBlue || guideRed || guideGreen)
            return Colors.Transparent;
        return sample;
    }

    private static Color ModulateFromMask(Color current, Color mask, Color layer, float strength)
    {
        if (mask.A <= 0) return current;
        var luminance = Math.Clamp(mask.R * 0.299f + mask.G * 0.587f + mask.B * 0.114f, 0, 1);
        return current.Lerp(layer, luminance * strength * mask.A);
    }

    private int NeighbourMask(int x, int y, Func<int, int, bool> same)
    {
        var mask = 0;
        var size = StrategicWorldRuntime.TileDimension;
        if (y > 0 && same(x, y - 1)) mask |= 1;
        if (x + 1 < size && same(x + 1, y)) mask |= 2;
        if (y + 1 < size && same(x, y + 1)) mask |= 4;
        if (x > 0 && same(x - 1, y)) mask |= 8;
        return mask;
    }

    private static int StableTileVariant(int x, int y, int count) =>
        (int)((uint)(x * 73856093 ^ y * 19349663) % (uint)count);

    private void RebuildRealmMapTexture()
    {
        if (_world is null) return;
        var mapSize = StrategicWorldRuntime.TileDimension;
        var size = mapSize * WorldTextureScale;
        var pixels = new byte[size * size * 4];
        for (var y = 0; y < mapSize; y++)
        for (var x = 0; x < mapSize; x++)
        {
            var region = _world.RegionAtTile(x, y);
            if (region is null || region.OwnerFactionId < 0) continue;
            var color = FactionColor(region.OwnerFactionId);
            var left = _world.RegionAtTile(x - 1, y)?.OwnerFactionId != region.OwnerFactionId;
            var right = _world.RegionAtTile(x + 1, y)?.OwnerFactionId != region.OwnerFactionId;
            var up = _world.RegionAtTile(x, y - 1)?.OwnerFactionId != region.OwnerFactionId;
            var down = _world.RegionAtTile(x, y + 1)?.OwnerFactionId != region.OwnerFactionId;
            if (left) PaintRealmBorder(pixels, size, x * WorldTextureScale,
                y * WorldTextureScale, 2, WorldTextureScale, color);
            if (right) PaintRealmBorder(pixels, size, (x + 1) * WorldTextureScale - 2,
                y * WorldTextureScale, 2, WorldTextureScale, color);
            if (up) PaintRealmBorder(pixels, size, x * WorldTextureScale,
                y * WorldTextureScale, WorldTextureScale, 2, color);
            if (down) PaintRealmBorder(pixels, size, x * WorldTextureScale,
                (y + 1) * WorldTextureScale - 2, WorldTextureScale, 2, color);
        }
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, pixels);
        if (_realmMapTexture is null) _realmMapTexture = ImageTexture.CreateFromImage(image);
        else _realmMapTexture.Update(image);
    }

    private static void PaintRealmBorder(byte[] pixels, int size, int x, int y,
        int width, int height, Color color)
    {
        for (var py = y; py < y + height; py++)
        for (var px = x; px < x + width; px++)
        {
            if ((uint)px >= (uint)size || (uint)py >= (uint)size) continue;
            var offset = (px + py * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = byte.MaxValue;
        }
    }

    private void RebuildRoadMapTexture()
    {
        if (_world is null) return;
        _worldRoadAtlas ??= GD.Load<Texture2D>(WorldRoadAtlasPath)?.GetImage();
        var mapSize = StrategicWorldRuntime.TileDimension;
        var size = mapSize * WorldTextureScale;
        var pixels = new byte[size * size * 4];
        foreach (var tile in _world.RoadTiles)
        {
            var x = tile % mapSize;
            var y = tile / mapSize;
            if ((uint)x >= (uint)mapSize || (uint)y >= (uint)mapSize) continue;
            var water = _world.Terrain.Water(x, y);
            var roadColor = water is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                StrategicWaterKind.Lake or StrategicWaterKind.DeepLake
                ? new Color("c7ad72") : new Color("756346");
            var mask = 0;
            if (HasRoadTile(x, y - 1)) mask |= 1;
            if (HasRoadTile(x + 1, y)) mask |= 2;
            if (HasRoadTile(x, y + 1)) mask |= 4;
            if (HasRoadTile(x - 1, y)) mask |= 8;
            for (var py = 0; py < WorldTextureScale; py++)
            for (var px = 0; px < WorldTextureScale; px++)
            {
                var sample = _worldRoadAtlas is null ? Colors.White :
                    SampleHouseTile(_worldRoadAtlas, 2, x, y, mask, px, py);
                if (sample.A <= 0) continue;
                var luminance = Math.Clamp(sample.R * 0.299f + sample.G * 0.587f +
                    sample.B * 0.114f, 0.15f, 1f);
                var destinationX = x * WorldTextureScale + px;
                var destinationY = y * WorldTextureScale + py;
                var offset = (destinationX + destinationY * size) * 4;
                pixels[offset] = (byte)Math.Clamp(roadColor.R8 * luminance, 0, 255);
                pixels[offset + 1] = (byte)Math.Clamp(roadColor.G8 * luminance, 0, 255);
                pixels[offset + 2] = (byte)Math.Clamp(roadColor.B8 * luminance, 0, 255);
                pixels[offset + 3] = (byte)sample.A8;
            }
        }
        foreach (var site in _world.RuralSites)
        {
            var color = site.Kind == StrategicRuralSiteKind.Farm
                ? new Color("b69a55") : new Color("d4c28a");
            var centerX = site.TileX * WorldTextureScale + WorldTextureScale / 2;
            var centerY = site.TileY * WorldTextureScale + WorldTextureScale / 2;
            var offset = (centerX + centerY * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = 255;
        }
        foreach (var haven in _world.HavenPlacements)
        {
            var color = new Color("b989d6");
            var centerX = haven.TileX * WorldTextureScale + WorldTextureScale / 2;
            var centerY = haven.TileY * WorldTextureScale + WorldTextureScale / 2;
            var offset = (centerX + centerY * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = 255;
        }
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, pixels);
        if (_roadMapTexture is null) _roadMapTexture = ImageTexture.CreateFromImage(image);
        else _roadMapTexture.Update(image);
    }

    private bool HasRoadTile(int x, int y) => (uint)x < StrategicWorldRuntime.TileDimension &&
        (uint)y < StrategicWorldRuntime.TileDimension &&
        _world.RoadTiles.Contains(x + y * StrategicWorldRuntime.TileDimension);

    private Color WorldTileColor(int x, int y)
    {
        var terrain = _world.Terrain;
        var water = terrain.Water(x, y);
        if (water == StrategicWaterKind.DeepOcean) return new Color("334e6c");
        if (water == StrategicWaterKind.Ocean) return new Color("506d8b");
        if (water == StrategicWaterKind.DeepLake) return new Color("405f79");
        if (water == StrategicWaterKind.Lake) return new Color("617f91");
        if (water == StrategicWaterKind.River) return new Color("668e9a");
        if (water == StrategicWaterKind.SmallRiver) return new Color("789da4");
        if (water == StrategicWaterKind.Delta) return new Color("698b8e");
        if (terrain.Mountain(x, y))
        {
            var height = Math.Clamp(terrain.MountainHeight(x, y) / 15f, 0, 1);
            return new Color("77766f").Lerp(new Color("ddd8cc"), height);
        }
        return WorldGroundTileColor(x, y);
    }

    private Color WorldGroundTileColor(int x, int y)
    {
        var terrain = _world.Terrain;
        var climate = terrain.Climate(x, y);
        var ground = climate == "COLD" ? new Color("a7a9a3") :
            climate == "HOT" ? new Color("aaa083") : new Color("858d70");
        ground = ground.Lerp(new Color("929c73"), (float)terrain.Fertility(x, y) * 0.30f);
        return ground.Lerp(new Color("53694b"), (float)terrain.Forest(x, y) * 0.34f);
    }

    private void UpdateMapSource()
    {
        var size = StrategicWorldRuntime.TileDimension;
        var visible = new Vector2(
            Math.Min(size, MapViewport.Size.X / _mapZoom),
            Math.Min(size, MapViewport.Size.Y / _mapZoom));
        _mapCenter.X = Math.Clamp(_mapCenter.X, visible.X / 2f, size - visible.X / 2f);
        _mapCenter.Y = Math.Clamp(_mapCenter.Y, visible.Y / 2f, size - visible.Y / 2f);
        _mapSource = new Rect2(_mapCenter - visible / 2f, visible);
    }

    private void ClampMapCenter()
    {
        UpdateMapSource();
    }

    private Vector2 ScreenToMap(Vector2 screen) => _mapSource.Position +
        (screen - MapViewport.Position) / MapViewport.Size * _mapSource.Size;

    private Vector2 MapToScreen(Vector2 tile) => MapViewport.Position +
        (tile - _mapSource.Position) / _mapSource.Size * MapViewport.Size;

    private static Vector2 MapToMini(Vector2 tile) => MiniMapViewport.Position +
        tile / StrategicWorldRuntime.TileDimension * MiniMapViewport.Size;

    private void CenterMapOnRegion(int regionId)
    {
        var region = _world.Region(regionId);
        if (region is null) return;
        _mapCenter = new Vector2(
            region.CenterTileX, region.CenterTileY);
        ClampMapCenter();
    }

    private async void Confirm()
    {
        if (_generatingCivilizations || _stage == SetupStage.Launching) return;
        if (_overviewOnly)
        {
            BackRequested?.Invoke();
            return;
        }
        if (_selectedRegion < 0 || !_world.CanSetPlayerStart(_selectedRegion) ||
            !_world.CanPlaceCapitalAt(_selectedCapitalTile.X, _selectedCapitalTile.Y)) return;
        var race = SelectedRaceKey();
        if (_stage == SetupStage.Capital)
        {
            _generatingCivilizations = true;
            SetBusy(true, "Создание государств, дорог, городов и региональной экономики…");
            _confirm.Disabled = true;
            _confirm.Text = "";
            _confirm.TooltipText = "Генерация мира…";
            _details.Text = "Создаются государства, дороги, города и региональная экономика. " +
                            "Интерфейс остаётся активным; дождитесь завершения.";
            QueueRedraw();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            try
            {
                await Task.Run(() => _world.GenerateCivilizations(
                    OriginalGameData.Current.Races, _seed, _selectedRegion, race,
                    _profile?.FactionName));
            }
            catch (Exception exception)
            {
                _generatingCivilizations = false;
                SetBusy(false);
                _confirm.Disabled = false;
                _confirm.Text = "";
                _confirm.TooltipText = "Повторить генерацию";
                _title.Text = "Ошибка генерации мира";
                _details.Text = exception.ToString();
                GD.PushError(exception.ToString());
                QueueRedraw();
                return;
            }
            _generatingCivilizations = false;
            SetBusy(false);
            GD.Print($"[LAUNCH] civilizations_ready factions={_world.Factions.Count} cities={_world.Cities.Count}");
            _generationProblems = _world.ValidateGeneratedWorld(_selectedRegion);
            if (_generationProblems.Count > 0)
            {
                _title.Text = "Ошибка проверки созданного мира";
                _generationStatus.Text = string.Join("\n", _generationProblems);
                _generationStatus.Visible = true;
                _details.Text = "Генерация не прошла проверку:\n" +
                    string.Join("\n", _generationProblems);
                _confirm.Disabled = true;
                GD.PushError(string.Join("; ", _generationProblems));
                return;
            }
            _confirm.Disabled = false;
            ShowFinishStage();
            return;
        }
        if (_stage != SetupStage.Finish) return;
        _stage = SetupStage.Launching;
        _confirm.Disabled = true;
        _confirm.Text = "";
        _confirm.TooltipText = "Запуск города…";
        SetBusy(true, "Создание городской карты и запуск поселения…");
        StartRequested?.Invoke(_world, _seed, _selectedRegion,
            _selectedCapitalTile.X, _selectedCapitalTile.Y, race);
    }

    private void ShowFinishStage()
    {
        _stage = SetupStage.Finish;
        _worldFinalized = true;
        RebuildRealmMapTexture();
        RebuildRoadMapTexture();
        _editTerrain.ButtonPressed = false;
        _editTerrain.Disabled = true;
        _editTerrain.Visible = false;
        _terrainTool.Visible = false;
        _brushLabel.Visible = false;
        _brushSize.Visible = false;
        _editorHint.Visible = false;
        _regenerate.Text = "";
        _regenerate.TooltipText = "Пересоздать государства";
        _home.Visible = true;
        _confirm.Text = "";
        _confirm.TooltipText = "Начать игру";
        _title.Text = "Осмотрите созданный мир";
        RefreshDetails();
        QueueRedraw();
    }

    private void BackFromWorldStage()
    {
        if (_overviewOnly)
        {
            BackRequested?.Invoke();
            return;
        }
        if (!_worldFinalized)
        {
            ShowTerrainStage();
            return;
        }
        _world.ClearGeneratedCivilizations();
        _stage = SetupStage.Capital;
        _worldFinalized = false;
        _selectedRegion = -1;
        _selectedCapitalTile = new Vector2I(-1, -1);
        _realmMapTexture = null;
        _roadMapTexture = null;
        _editTerrain.Disabled = false;
        _editTerrain.Visible = true;
        _terrainTool.Visible = false;
        _brushLabel.Visible = false;
        _brushSize.Visible = false;
        _editorHint.Visible = false;
        _regenerate.Text = "";
        _regenerate.TooltipText = "Новый рельеф";
        _home.Visible = false;
        _confirm.Text = "";
        _confirm.TooltipText = "Создать мир";
        _title.Text = "Разместите столицу";
        RefreshDetails();
        QueueRedraw();
    }

    private async void RegenerateCurrentStage()
    {
        if (_overviewOnly) return;
        if (!_worldFinalized)
        {
            var seed = Random.Shared.Next(0, int.MaxValue);
            _seedInput.Text = seed.ToString();
            GenerateConfiguredWorld(seed);
            return;
        }
        var race = SelectedRaceKey();
        _generatingCivilizations = true;
        SetBusy(true, "Пересоздание государств, дорог и городов…");
        _confirm.Disabled = true;
        _regenerate.Disabled = true;
        _title.Text = "Пересоздание государств…";
        QueueRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        try
        {
            await Task.Run(() => _world.GenerateCivilizations(
                OriginalGameData.Current.Races, _seed, _selectedRegion, race,
                _profile?.FactionName));
        }
        catch (Exception exception)
        {
            _generatingCivilizations = false;
            SetBusy(false);
            _confirm.Disabled = false;
            _regenerate.Disabled = false;
            _title.Text = "Ошибка генерации мира";
            _details.Text = exception.ToString();
            GD.PushError(exception.ToString());
            QueueRedraw();
            return;
        }
        _generatingCivilizations = false;
        SetBusy(false);
        _confirm.Disabled = false;
        _regenerate.Disabled = false;
        _generationProblems = _world.ValidateGeneratedWorld(_selectedRegion);
        if (_generationProblems.Count > 0)
        {
            _details.Text = "Генерация не прошла проверку:\n" + string.Join("\n", _generationProblems);
            _confirm.Disabled = true;
            return;
        }
        ShowFinishStage();
    }

    private void CenterOnPlayerCapital()
    {
        if (_selectedCapitalTile.X < 0) return;
        _mapCenter = new Vector2(_selectedCapitalTile.X + 0.5f, _selectedCapitalTile.Y + 0.5f);
        ClampMapCenter();
        QueueRedraw();
    }

    private void RefreshDetails()
    {
        var region = _world?.Region(_hoveredRegion >= 0 ? _hoveredRegion : _selectedRegion);
        var landmark = _world is not null && _hoveredTile.X >= 0
            ? _world.LandmarkAtTile(_hoveredTile.X, _hoveredTile.Y) : null;
        var haven = _world?.HavenPlacements.FirstOrDefault(value =>
            value.TileX == _hoveredTile.X && value.TileY == _hoveredTile.Y);
        var race = _race.Selected >= 0 ? _race.GetItemText(_race.Selected) : "—";
        var selected = _world?.Region(_selectedRegion);
        var detailsVisible = region is not null;
        _detailsBackground.Visible = detailsVisible;
        _details.Visible = detailsVisible;
        // The Java GBox begins with a text climate row. The former oversized
        // leading icon shifted the complete layout and made the box look like a
        // custom replacement rather than UIWorldToolCapitolPlaceInfo.
        _detailsIcon.Visible = false;
        foreach (var icon in _detailStatIcons) icon.Visible = detailsVisible;
        _confirm.Disabled = !_overviewOnly && (selected is null ||
            !_world!.CanSetPlayerStart(selected.Id) ||
            !_world.CanPlaceCapitalAt(_selectedCapitalTile.X, _selectedCapitalTile.Y));
        var site = LocalSiteInfo(_hoveredTile.X, _hoveredTile.Y, region);
        _details.Text = region is null
            ? ""
            : $"Климат                         {LocalizeClimate(site.Climate)}\n" +
              $"Пригодность для расы {race}     {SettlementSuitability(site):P0}\n" +
              "────────────────────────────────────────\n" +
              $"Плодородие {site.Fertility:P0}     Горы {site.Mountain:P0}     Реки {site.River:P0}\n\n" +
              $"Влажность                      {site.Moisture:P0}\n" +
              $"{MoistureDescription(site.Moisture)}\n\n" +
              $"Пресная вода                   {site.FreshWater:P0}\n" +
              $"{FreshWaterDescription(site.FreshWater)}\n\n" +
              $"Лес                            {site.Forest:P0}\n" +
              $"{ForestDescription(site.Forest)}\n\n" +
              (landmark is null ? "" : $"Ориентир: {landmark.Name}, площадь {landmark.Area}\n") +
              (haven is null ? "" : $"Поблизости: {haven.Name}, {LocalizeRace(haven.Race)}, население {haven.Population}\n") +
              $"Ресурсы: {SettlementResources(_world!, region, _seed, _hoveredTile)}";
    }

    private readonly record struct CapitalSiteInfo(string Climate, double Fertility,
        double Moisture, double Forest, double Mountain, double River, double Water,
        double Ocean, double Elevation, double FreshWater);

    private CapitalSiteInfo LocalSiteInfo(int centerX, int centerY, StrategicRegion? fallback)
    {
        if (_world is null || centerX < 0 || centerY < 0)
            return new CapitalSiteInfo(fallback?.Climate ?? "TEMPERATE", fallback?.Fertility ?? 0,
                fallback?.Moisture ?? 0, fallback?.Forest ?? 0, fallback?.Mountain ?? 0,
                fallback?.River ?? 0, fallback?.Water ?? 0, fallback?.Ocean ?? 0,
                fallback?.Elevation ?? 0, fallback is null ? 0 : FreshWater(fallback));
        double fertility = 0, moisture = 0, forest = 0, mountain = 0, river = 0;
        double water = 0, ocean = 0, elevation = 0;
        var cold = 0; var temperate = 0; var hot = 0; var count = 0;
        var half = CapitalSelectionPreviewDimension / 2;
        for (var oy = -half; oy <= half; oy++)
        for (var ox = -half; ox <= half; ox++)
        {
            var x = centerX + ox; var y = centerY + oy;
            if ((uint)x >= StrategicWorldRuntime.TileDimension ||
                (uint)y >= StrategicWorldRuntime.TileDimension) continue;
            count++;
            fertility += _world.Terrain.Fertility(x, y);
            moisture += _world.Terrain.Moisture(x, y);
            forest += _world.Terrain.Forest(x, y);
            mountain += _world.Terrain.Mountain(x, y) ? 1 : 0;
            elevation += _world.Terrain.Height(x, y) / 255.0;
            var kind = _world.Terrain.Water(x, y);
            if (kind != StrategicWaterKind.None) water++;
            if (kind is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean) ocean++;
            if (kind is StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta) river++;
            switch (_world.Terrain.Climate(x, y))
            {
                case "COLD": cold++; break;
                case "HOT": hot++; break;
                default: temperate++; break;
            }
        }
        var divisor = Math.Max(1, count);
        fertility /= divisor; moisture /= divisor; forest /= divisor; mountain /= divisor;
        river /= divisor; water /= divisor; ocean /= divisor; elevation /= divisor;
        var climate = cold > temperate && cold > hot ? "COLD" : hot > temperate ? "HOT" : "TEMPERATE";
        var freshWater = Math.Clamp(river * 3.0 + Math.Max(0, water - ocean), 0, 1);
        return new CapitalSiteInfo(climate, fertility, moisture, forest, mountain, river,
            water, ocean, elevation, freshWater);
    }

    private static double SettlementSuitability(CapitalSiteInfo site) => Math.Clamp(
        site.Fertility * 0.40 + site.Moisture * 0.20 + site.Forest * 0.15 +
        site.FreshWater * 0.25 - site.Mountain * 0.25 - site.Ocean * 0.50, 0, 1);

    private static double FreshWater(StrategicRegion region) =>
        Math.Clamp(region.River * 3.0 + Math.Max(0, region.Water - region.Ocean), 0, 1);

    private static double SettlementSuitability(StrategicRegion region) => Math.Clamp(
        region.Fertility * 0.40 + region.Moisture * 0.20 + region.Forest * 0.15 +
        FreshWater(region) * 0.25 - region.Mountain * 0.25 - region.Ocean * 0.50, 0, 1);

    private static string MoistureDescription(double value) => value switch
    {
        < 0.20 => "засушливая местность",
        < 0.45 => "умеренно сухая местность",
        < 0.70 => "влажная местность",
        _ => "очень влажная местность"
    };

    private static string FreshWaterDescription(double value) => value switch
    {
        < 0.10 => "почти отсутствует",
        < 0.35 => "небольшие запасы",
        < 0.70 => "достаточно",
        _ => "в изобилии"
    };

    private static string ForestDescription(double value) => value switch
    {
        < 0.10 => "открытая местность",
        < 0.35 => "редколесье",
        < 0.70 => "густой лес",
        _ => "очень густой лес"
    };

    private StrategicRegion? RegionAtScreen(Vector2 screenPosition)
    {
        var tile = ScreenToMap(screenPosition);
        return _world.RegionAtTile((int)tile.X, (int)tile.Y);
    }

    private void DrawCapitalFootprint()
    {
        if (_overviewOnly || _selectedCapitalTile.X < 0) return;
        var valid = _world.CanPlaceCapitalAt(_selectedCapitalTile.X, _selectedCapitalTile.Y);
        var color = valid ? new Color("2a77ca") : new Color("c34f45");
        var half = CapitalSelectionPreviewDimension / 2;
        for (var y = -half; y < CapitalSelectionPreviewDimension - half; y++)
        for (var x = -half; x < CapitalSelectionPreviewDimension - half; x++)
        {
            var topLeft = MapToScreen(new Vector2(_selectedCapitalTile.X + x,
                _selectedCapitalTile.Y + y));
            var bottomRight = MapToScreen(new Vector2(_selectedCapitalTile.X + x + 1,
                _selectedCapitalTile.Y + y + 1));
            var rect = new Rect2(topLeft, bottomRight - topLeft);
            if (rect.Intersects(MapViewport)) DrawRect(rect.Intersection(MapViewport), color, false, 2f);
        }
    }

    private Vector2I FindCapitalSite(StrategicRegion region)
    {
        var center = new Vector2I(region.CenterTileX, region.CenterTileY);
        for (var radius = 0; radius < 16; radius++)
        for (var y = center.Y - radius; y <= center.Y + radius; y++)
        for (var x = center.X - radius; x <= center.X + radius; x++)
            if (_world.RegionAtTile(x, y)?.Id == region.Id && _world.CanPlaceCapitalAt(x, y))
                return new Vector2I(x, y);
        return center;
    }

    private static Vector2 RegionTile(StrategicRegion region, bool? farCorner)
    {
        return new Vector2(region.CenterTileX + 0.5f, region.CenterTileY + 0.5f);
    }

    private static Color FactionColor(int id)
        => OriginalWorldPalette.Faction(id);

    private static string SettlementResources(StrategicWorldRuntime world, StrategicRegion region, int seed,
        Vector2I hoveredTile)
    {
        var site = world.CanPlaceCapitalAt(hoveredTile.X, hoveredTile.Y)
            ? hoveredTile : new Vector2I(region.CenterTileX, region.CenterTileY);
        var profile = SettlementGenerationProfile.FromWorldSite(seed, world, region, site.X, site.Y,
            OriginalGameData.Current.Minables, OriginalGameData.Current.Growables);
        var resources = profile.Minables.Where(value => value.OnEveryMap || profile.IncludeRareMinables)
            .Select(value => value.Resource).Distinct().ToArray();
        return resources.Length == 0 ? "нет данных" : string.Join(", ", resources.Select(LocalizeResource));
    }

    private void DrawRealmBorders(StrategicRegion region, Color color)
    {
        var x0 = region.X * StrategicWorldRuntime.TileDimension / (float)StrategicWorldRuntime.RegionsAcross;
        var y0 = region.Y * StrategicWorldRuntime.TileDimension / (float)StrategicWorldRuntime.RegionsAcross;
        var x1 = (region.X + 1) * StrategicWorldRuntime.TileDimension / (float)StrategicWorldRuntime.RegionsAcross;
        var y1 = (region.Y + 1) * StrategicWorldRuntime.TileDimension / (float)StrategicWorldRuntime.RegionsAcross;
        bool Foreign(int x, int y) => _world.At(x, y)?.OwnerFactionId != region.OwnerFactionId;
        if (Foreign(region.X, region.Y - 1)) DrawLine(MapToScreen(new Vector2(x0, y0)), MapToScreen(new Vector2(x1, y0)), color, 2f);
        if (Foreign(region.X + 1, region.Y)) DrawLine(MapToScreen(new Vector2(x1, y0)), MapToScreen(new Vector2(x1, y1)), color, 2f);
        if (Foreign(region.X, region.Y + 1)) DrawLine(MapToScreen(new Vector2(x1, y1)), MapToScreen(new Vector2(x0, y1)), color, 2f);
        if (Foreign(region.X - 1, region.Y)) DrawLine(MapToScreen(new Vector2(x0, y1)), MapToScreen(new Vector2(x0, y0)), color, 2f);
    }

    private void SelectRace(string race)
    {
        for (var index = 0; index < _race.ItemCount; index++)
            if (_race.GetItemMetadata(index).AsString()
                .Equals(race, StringComparison.OrdinalIgnoreCase))
            {
                _race.Select(index);
                return;
            }
    }

    private string SelectedRaceKey() => _race.Selected >= 0
        ? _race.GetItemMetadata(_race.Selected).AsString() : "HUMAN";

    private static string LocalizeClimate(string key) => key.ToUpperInvariant() switch
    {
        "COLD" => "Холодный климат",
        "HOT" => "Тёплый климат",
        _ => "Умеренный климат"
    };

    private static string LocalizeRace(string key) => key.ToUpperInvariant() switch
    {
        "HUMAN" => "Люди", "CRETONIAN" => "Кретонианцы", "DONDORIAN" => "Дондорианцы",
        "GARTHIMI" => "Гартими", "TILAPI" => "Тилапи", "ARGONOSH" => "Аргоноши",
        "Q_AMEVIA" or "AMEVIA" => "Амевии", "CANTOR" => "Канторы", _ => key
    };

    private static string LocalizeResource(string key) => key.ToUpperInvariant() switch
    {
        "CLAY" => "Глина", "COAL" => "Уголь", "GEM" => "Самоцветы", "ORE" => "Руда",
        "SITHILON" => "Ситилон", "STONE" => "Камень", "WOOD" => "Древесина",
        "GRAIN" => "Зерно", "FRUIT" => "Фрукты", "VEGETABLE" => "Овощи", _ => key
    };
}
