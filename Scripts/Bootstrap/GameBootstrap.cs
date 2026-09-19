using System;
using System.Collections.Generic;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.Save;
using GodotSyxPort.Rooms;
using GodotSyxPort.Resources;
using GodotSyxPort.Hauling;
using GodotSyxPort.Data;
using GodotSyxPort.Maintenance;
using GodotSyxPort.Stats;
using GodotSyxPort.Events;
using GodotSyxPort.UI;
using GodotSyxPort.World;
using GodotSyxPort.Trade;
using GodotSyxPort.Military;
using System.Linq;
using System.Threading.Tasks;

namespace GodotSyxPort.Bootstrap;

public sealed partial class GameBootstrap : Node3D
{
    private const int LandingBaseCredits = 5000;
    private enum BuildTool
    {
        Wall, Road, RoomArea, RoomShrink, Door, Furniture, RoomInspect, Cancel,
        Forage, ClearWood, ClearStone, ClearAll, ClearWater, DigTunnel
    }

    private readonly JobBoard _jobs = new();
    private readonly SimulationClock _clock = new();
    private GridWorld _world = null!;
    private Camera3D _camera = null!;
    private Vector3 _cameraVelocity;
    private bool _cameraDragging;
    private Vector2 _cameraDragMouse;
    private Vector3 _cameraDragOrigin;
    private Label _status = null!;
    private Label _toolLabel = null!;
    private Label _economyInfo = null!;
    private ColorRect _economyPanel = null!;
    private GlobalMapOverlay _globalMap = null!;
    private SettlementInspectorPanel _inspector = null!;
    private SettlementTopBar _topBar = null!;
    private TimeControlBar _timeControls = null!;
    private SettlementRightSidebar _rightSidebar = null!;
    private RoomBuildPalette _roomPalette = null!;
    private ColorRect _constructionPalette = null!;
    private Label _constructionTitle = null!;
    private Button _autoWallsButton = null!;
    private Button _fixedAutoWallsButton = null!;
    private Button _fixedStructureButton = null!;
    private PopupPanel _fixedStructurePopup = null!;
    private VBoxContainer _constructionShapeControls = null!;
    private VBoxContainer _fixedStructureControls = null!;
    private ColorRect _constructionShapeSeparator = null!;
    private ColorRect _constructionStatsSeparator = null!;
    private HBoxContainer _constructionFrameActions = null!;
    private ColorRect _bottomToolbar = null!;
    private VBoxContainer _furnisherControls = null!;
    private Label _furnisherCost = null!;
    private RoomPolicyPanel _roomPolicy = null!;
    private SettlementNotificationFeed _notifications = null!;
    private SettlementReferencePanel _referencePanel = null!;
    private AdministrationDashboard _administration = null!;
    private SettlementManagerPanel _manager = null!;
    private SettlementCivicPanel _civic = null!;
    private StrategicWorldRuntime _strategicWorld = null!;
    private RegionalEconomyRuntime _regionalEconomy = null!;
    private SettlementWorldRuntime _settlementWorld = null!;
    private WorldTradeRuntime _worldTrade = null!;
    private WorldRaidingRuntime _raiding = null!;
    private StrategicWorldMap _strategicMap = null!;
    private WorldBattlePanel _battlePanel = null!;
    private SettlementBattleCommandPanel _battleCommands = null!;
    private SettlementInvasionRuntime _invasion = null!;
    private CanvasLayer _uiLayer = null!;
    private BuildTool _tool = BuildTool.Wall;
    private CitizenSystem _citizens = null!;
    private GridCoord? _dragStart;
    private GridCoord? _lastPreviewEnd;
    private ulong _lastPreviewAt;
    private RoomPlanner _roomPlanner = null!;
    private readonly ResourceLedger _resources = new();
    private RoomSystem _rooms = null!;
    private HaulingSystem _hauling = null!;
    private RoadMaintenanceSystem _roadMaintenance = null!;
    private MaintenanceConsumption _maintenanceConsumption = null!;
    private CorpseRuntime _corpses = null!;
    private WorkAccidentRuntime _workAccidents = null!;
    private readonly SettlementStatsRuntime _settlementStats = new();
    private RoomRecord? _selectedRoom;
    private GridCoord? _selectedCell;
    private readonly EconomyTracker _economy = new();
    private SettlementWeatherRuntime _weather = null!;
    private SettlementEntryRuntime _entry = null!;
    private readonly SettlementEventRuntime _events = new();
    private IReadOnlyList<FloorRule> _roadTypes = Array.Empty<FloorRule>();
    private int _roadTypeIndex;
    private int _selectedFurnisherGroup;
    private int _selectedFurnisherVariant;
    private int _selectedFurnisherRotation;
    private GridCoord? _lastFurniturePreviewCell;
    private string _lastFurniturePreviewDefinition = "";
    private int _lastFurniturePreviewGroup = -1;
    private int _lastFurniturePreviewVariant = -1;
    private int _lastFurniturePreviewRotation = -1;
    private double _autoSaveAccumulator;
    private double _uiRefreshAccumulator;
    private double _worldTickAccumulator;
    private bool _initialized;
    private bool _firstCityFrameReported;
    private bool _landingPending;
    private bool _placingHotspot;
    private int _battleResumeSpeed = -1;
    private GridCoord? _landingOrigin;
    private const double AutoSaveSeconds = 300.0;
    public bool LoadExisting { get; set; }
    public bool Initialized => _initialized;
    public string? InitializationError { get; private set; }
    public event Action? WorldRequested;
    public event Action? MainMenuRequested;

    public override void _Ready() => _ = InitializeAsync();

    private async Task InitializeAsync()
    {
        try
        {
        GD.Print("[LAUNCH] settlement_ready_begin");
        OriginalGameData.Load();
        GameSession.EnsureDefault();
        var configuration = GameSession.Current!;
        _strategicWorld = GameSession.World!;
        var selectedRegion = _strategicWorld.Region(configuration.SelectedRegionId) ??
            throw new InvalidOperationException("Selected settlement region does not exist.");
        _roadTypes = OriginalGameData.Current.Roads;
        var defaultRoad = OriginalGameData.Current.DefaultRoad().Key;
        _roadTypeIndex = System.Math.Max(0,
            _roadTypes.ToList().FindIndex(road => road.Key == defaultRoad));
        var generationProfile = SettlementGenerationProfile.FromWorldSite(
            configuration.WorldSeed,
            _strategicWorld,
            selectedRegion,
            configuration.WorldCapitalX,
            configuration.WorldCapitalY,
            OriginalGameData.Current.Minables,
            OriginalGameData.Current.Growables);
        _world = new GridWorld
        {
            Name = "Settlement", GenerationProfile = generationProfile, GenerateOnReady = false
        };
        AddChild(_world);
        var terrainSettings = SettlementGeneratorSettings.Load();
        await Task.Run(() => _world.GenerateTerrainData(terrainSettings));
        GD.Print("[LAUNCH] settlement_terrain_data_ready");
        _world.InitializeRendering();
        GD.Print("[LAUNCH] settlement_renderers_ready");
        var climate = OriginalGameData.Current.Climates.GetValueOrDefault(generationProfile.Climate) ??
            new ClimateRule("TEMPERATE", 0.5, -0.15, 0.5, 0.45,
                new Color(193 / 255f, 181 / 255f, 135 / 255f),
                new Color(85 / 255f, 52 / 255f, 52 / 255f));
        _weather = new SettlementWeatherRuntime(
            climate, OriginalGameData.Current.SecondsPerHour, OriginalGameData.Current.SecondsPerDay);
        _rooms = new RoomSystem(_world, _weather);
        _regionalEconomy = new RegionalEconomyRuntime(_strategicWorld, _rooms.Trade);
        if (LoadExisting)
        {
            var pendingSave = SaveGameService.Load();
            if (pendingSave?.Version >= 14 && pendingSave.RegionalEconomy is not null)
                _regionalEconomy.Restore(pendingSave.RegionalEconomy);
        }
        _entry = new SettlementEntryRuntime(_world.Data);
        _settlementWorld = new SettlementWorldRuntime(
            _strategicWorld, _regionalEconomy, _entry, _rooms.Hospitality);
        if (!_settlementWorld.Initialize(configuration.WorldSeed))
            GD.PushWarning("Не удалось связать поселение со столицей игрока.");
        _worldTrade = new WorldTradeRuntime(
            _rooms.Trade, _rooms.Logistics, _settlementWorld.Shipments,
            _settlementWorld.Factions, _settlementWorld.Bridge);
        _world.NavigationChanged += _entry.InvalidateReachability;
        _hauling = new HaulingSystem(_world, _rooms);
        _roadMaintenance = new RoadMaintenanceSystem(_world);
        _maintenanceConsumption = new MaintenanceConsumption(_roadMaintenance, _rooms);
        _corpses = new CorpseRuntime(_rooms);
        _jobs.PrepareConstruction = job => job.PrepareConstructionSite(
            _world.Data, _hauling.HasUnreservedAt);
        _jobs.JobCancelled = job =>
        {
            foreach (var cell in job.OccupiedCells) _world.CancelReservation(cell);
            _hauling.CancelStorageReservation(job);
            _rooms.Construction.CancelSupply(job);
        };
        _roomPlanner = new RoomPlanner(_world, _rooms);

        _camera = new Camera3D
        {
            Name = "Camera",
            Projection = Camera3D.ProjectionType.Orthogonal,
            Size = 20f,
            Position = new Vector3(0, 24, 0),
            RotationDegrees = new Vector3(-90, 0, 0),
            Current = true
        };
        AddChild(_camera);

        var light = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-55, -35, 0),
            ShadowEnabled = true,
            LightEnergy = 1.1f
        };
        AddChild(light);

        _citizens = new CitizenSystem { Name = "Citizens" };
        AddChild(_citizens);
        _citizens.Initialize(
            _world, _rooms, _hauling, _roadMaintenance, _corpses,
            0, new GridCoord(GridWorld.Width / 2, GridWorld.Height / 2), configuration.PlayerRace);
        _workAccidents = new WorkAccidentRuntime(_rooms, _citizens);
        _citizens.EventWorkSuspended = _events.IsStriking;
        _raiding = new WorldRaidingRuntime();
        _raiding.Initialize(configuration.WorldSeed, _rooms, _citizens);
        _invasion = new SettlementInvasionRuntime { Name = "SettlementInvasion" };
        AddChild(_invasion);
        _invasion.Initialize(_world, _strategicWorld, _settlementWorld.Battles,
            _rooms, _resources, _citizens, _jobs, () => _landingOrigin is { } origin
                ? new GridCoord(origin.X + 4, origin.Z + 2) : null);

        CreateUi();
        if (LoadExisting) TryLoad(false);
        else
        {
            _landingPending = true;
            _status.Text = "Выберите на карте место для высадки и трона";
        }
        _initialized = true;
        GD.Print("[LAUNCH] settlement_ready_complete");
        }
        catch (Exception exception)
        {
            InitializationError = exception.ToString();
            GD.PushError("[LAUNCH] settlement_ready_failed\n" + InitializationError);
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (!_initialized) return;

        if (inputEvent is InputEventMouseButton pan &&
            pan.ButtonIndex == MouseButton.Middle)
        {
            _cameraDragging = pan.Pressed;
            if (pan.Pressed)
            {
                _cameraDragMouse = pan.Position;
                _cameraDragOrigin = _camera.Position;
                _cameraVelocity = Vector3.Zero;
            }
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_cameraDragging && inputEvent is InputEventMouseMotion panMotion)
        {
            var viewportHeight = Math.Max(1f, GetViewport().GetVisibleRect().Size.Y);
            var unitsPerPixel = _camera.Size / viewportHeight;
            var delta = panMotion.Position - _cameraDragMouse;
            _camera.Position = _cameraDragOrigin +
                new Vector3(-delta.X * unitsPerPixel, 0, -delta.Y * unitsPerPixel);
            ClampCamera();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_uiLayer.Visible)
        {
            if ((inputEvent is InputEventKey restoreKey && restoreKey.Pressed &&
                 restoreKey.Keycode == Key.Escape) ||
                (inputEvent is InputEventMouseButton restoreMouse && restoreMouse.Pressed &&
                 restoreMouse.ButtonIndex == MouseButton.Right))
            {
                _uiLayer.Visible = true;
                GetViewport().SetInputAsHandled();
            }
            return;
        }

        if (_placingHotspot && inputEvent is InputEventMouseButton cancelHotspot &&
            cancelHotspot.Pressed && cancelHotspot.ButtonIndex == MouseButton.Right)
        {
            _placingHotspot = false;
            _status.Text = "Постановка метки отменена";
            GetViewport().SetInputAsHandled();
            return;
        }

        if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            var cell = ScreenToCell(mouse.Position);
            if (_landingPending)
            {
                if (!mouse.Pressed && cell is { } landingCell) TryPlaceLandingParty(landingCell);
                return;
            }
            if (_placingHotspot)
            {
                if (!mouse.Pressed && cell is { } hotspotCell && _rightSidebar.AddHotspot(hotspotCell))
                {
                    _placingHotspot = false;
                    _status.Text = $"Метка поставлена: {hotspotCell.X}:{hotspotCell.Z}";
                }
                return;
            }
            if (_battleCommands.Visible && _battleCommands.AwaitingMapOrder)
            {
                if (mouse.Pressed) _dragStart = cell;
                else if (_dragStart is { } battleStart && cell is { } battleEnd)
                {
                    var division = _rooms.Military.Divisions.FirstOrDefault(value =>
                        value.Id == _battleCommands.SelectedDivisionId);
                    var enemyTarget = _invasion.EnemyCohortAt(battleEnd);
                    var buildingTarget = enemyTarget < 0 &&
                        (_world.Data.IsBlocked(battleEnd) || _rooms.Contains(battleEnd));
                    if (division is not null && _battleCommands.CommitMapOrder(
                            battleStart, battleEnd, enemyTarget, buildingTarget))
                    {
                        var combat = _rooms.Military.CombatProfile(division.Id, _citizens.PersonalStats);
                        var taskSpeed = division.Order.Task == DivisionBattleTask.Charge ? 0.9 : 0.7;
                        if (division.Engaged) taskSpeed *= 0.75;
                        var moved = _citizens.IssueMilitaryFormation(division.Members,
                            battleStart, battleEnd, division.Order.Formation, _jobs, _resources,
                            (float)(taskSpeed * combat.SpeedMultiplier));
                        _rooms.Military.TickBattleCondition(division.Id, 2.0, fighting: false,
                            running: division.Order.Task is DivisionBattleTask.Move or DivisionBattleTask.Charge);
                        _status.Text = $"{division.Name}: приказ получили {moved} бойцов";
                    }
                    else if (division is null)
                        _battleCommands.CommitMapOrder(battleStart, battleEnd, enemyTarget, buildingTarget);
                    _world.ClearRoomPreview(); _dragStart = null;
                }
                return;
            }
            if (mouse.Pressed)
            {
                _dragStart = cell;
                _lastPreviewEnd = null;
                if (cell is { } pressedCell) PreviewSelection(pressedCell, pressedCell);
            }
            else if (_dragStart is { } start && cell is { } end) ApplySelection(start, end);
            if (!mouse.Pressed)
            {
                _dragStart = null;
                _lastPreviewEnd = null;
            }
        }

        if (inputEvent is InputEventMouseMotion motion && _dragStart is { } previewStart &&
            ScreenToCell(motion.Position) is { } previewEnd)
        {
            var now = Time.GetTicksMsec();
            if (_lastPreviewEnd == previewEnd || now - _lastPreviewAt < 16) return;
            _lastPreviewEnd = previewEnd;
            _lastPreviewAt = now;
            if (_battleCommands.Visible && _battleCommands.AwaitingMapOrder)
            {
                _world.ShowRoomPreview(Line(previewStart, previewEnd),
                    Array.Empty<GridCoord>(), Array.Empty<GridCoord>());
                return;
            }
            PreviewSelection(previewStart, previewEnd);
            return;
        }

        if (inputEvent is InputEventMouseButton wheel && wheel.Pressed)
        {
            if (wheel.ButtonIndex == MouseButton.WheelUp)
                _camera.Size = Mathf.Max(7f, _camera.Size - 1.5f);
            else if (wheel.ButtonIndex == MouseButton.WheelDown)
                _camera.Size = Mathf.Min(80f, _camera.Size + 2f);
        }

        if (inputEvent is not InputEventKey key || !key.Pressed || key.Echo) return;
        switch (key.Keycode)
        {
            case Key.Key1: SelectTool(BuildTool.Wall); break;
            case Key.Key2: SelectTool(BuildTool.Road); break;
            case Key.Key3: SelectTool(BuildTool.RoomArea); break;
            case Key.E when _tool == BuildTool.Furniture:
                ChangeFurnisherVariant(1);
                break;
            case Key.Q when _tool == BuildTool.Furniture:
                ChangeFurnisherVariant(-1);
                break;
            case Key.R when _tool == BuildTool.Furniture:
                RotateFurnisher();
                break;
            case Key.R: SelectTool(BuildTool.Door); break;
            case Key.F: SelectTool(BuildTool.Furniture); break;
            case Key.I:
                SelectTool(BuildTool.RoomInspect);
                ToggleWindow(_inspector);
                break;
            case Key.Tab: CycleRoomType(); break;
            case Key.X: SelectTool(BuildTool.Cancel); break;
            case Key.Delete when _selectedRoom is not null:
                _rooms.Dismantle(_selectedRoom.Id, _jobs, _resources,
                    (resource, amount, cell) => _hauling.Spawn(cell, resource, amount, _jobs));
                _selectedRoom = null;
                break;
            case Key.B: _roomPlanner.AutoWalls = !_roomPlanner.AutoWalls; break;
            case Key.Enter: CommitRoomDraft(); break;
            case Key.Space: _clock.TogglePause(); break;
            case Key.M: ToggleWindow(_globalMap); break;
            case Key.G: _globalMap.CycleLayer(); break;
            case Key.P: ToggleWindow(_roomPalette); break;
            case Key.F1: OpenManager(_manager.OpenEconomy); break;
            case Key.F2: ToggleWindow(_administration); break;
            case Key.F3: ToggleWindow(_strategicMap); break;
            case Key.F4: ToggleWindow(_notifications); break;
            case Key.Escape: CloseWindows(); break;
            case Key.Key4: _clock.SetSpeedLevel(2); break;
            case Key.Key5: _clock.SetSpeedLevel(3); break;
            case Key.Key6: _clock.SetSpeedLevel(4); break;
            case Key.F7: _citizens.AdjustProfessionLimit(WorkProfession.Baker, -1); break;
            case Key.F8: _citizens.AdjustProfessionLimit(WorkProfession.Baker, 1); break;
            case Key.F9: _citizens.AdjustProfessionLimit(WorkProfession.Carpenter, -1); break;
            case Key.F10: _citizens.AdjustProfessionLimit(WorkProfession.Carpenter, 1); break;
            case Key.F11 when _selectedRoom is not null:
                _rooms.AdjustWorkerLimit(_selectedRoom, -1, _jobs, _resources);
                break;
            case Key.F12 when _selectedRoom is not null:
                _rooms.AdjustWorkerLimit(_selectedRoom, 1, _jobs, _resources);
                break;
            case Key.V when _selectedRoom is not null:
                _rooms.CycleRecipe(_selectedRoom, _jobs, _resources);
                break;
            case Key.C when _tool == BuildTool.Road:
                if (_roadTypes.Count > 0) _roadTypeIndex = (_roadTypeIndex + 1) % _roadTypes.Count;
                break;
            case Key.T when _selectedRoom is not null:
                _rooms.AdjustToolTarget(_selectedRoom, key.ShiftPressed ? -1 : 1);
                break;
            case Key.S when key.CtrlPressed: Save(); break;
            case Key.L when key.CtrlPressed: TryLoad(true); break;
        }
    }

    public override void _Process(double delta)
    {
        if (!_initialized) return;
        if (!_firstCityFrameReported)
        {
            _firstCityFrameReported = true;
            GD.Print("[LAUNCH] first_city_frame");
        }

        MoveCamera((float)delta);
        // Camera movement is visual state, so the minimap viewport must follow
        // every rendered frame.  The heavier resource/sidebar refresh remains
        // on its 0.25 second cadence below.
        _rightSidebar.UpdateCameraView(_world.WorldToCell(_camera.Position), _camera.Size);
        RefreshFurnitureCursorPreview();
        if (_landingPending) return;
        var ticks = _clock.ConsumeTicks(delta);
        _citizens.BeginFrame();
        for (var tick = 0; tick < ticks; tick++)
        {
            _weather.Tick(SimulationClock.FixedStep, _clock.PlayedSeconds);
            _entry.Tick(SimulationClock.FixedStep, _citizens);
            _citizens.Tick(SimulationClock.FixedStep, _jobs, _resources);
            _workAccidents.Tick(
                SimulationClock.FixedStep, _clock.PlayedSeconds, _resources, _jobs);
            _economy.Tick(SimulationClock.FixedStep, _resources);
            _rooms.TickMaintenance(SimulationClock.FixedStep, _jobs, _resources);
            _rooms.TickServices(
                SimulationClock.FixedStep,
                (_clock.PlayedSeconds / OriginalGameData.Current.SecondsPerDay) % 1.0);
            // WorldTradeRuntime now settles trade only after physical shipment arrival.
            _rooms.TickTechnologies(SimulationClock.FixedStep);
            // The Java world uses distributed updaters; advancing every realm and
            // every resource on each 20 Hz settlement step caused the city view stalls.
            _worldTickAccumulator += SimulationClock.FixedStep;
            if (_worldTickAccumulator >= 0.25)
            {
                var worldDelta = _worldTickAccumulator;
                _worldTickAccumulator = 0;
                _settlementWorld.Diplomacy.SetProduction(_rooms.Governance.Diplomacy);
                var worldTick = _settlementWorld.Tick(
                    worldDelta, OriginalGameData.Current.SecondsPerDay,
                    _citizens.PopulationByRace(),
                    0.25 + _settlementStats.MonumentEnvironment * 0.75,
                    Math.Clamp(1.0 - _settlementStats.Unburied / 4.0, 0, 1),
                    Math.Clamp((_settlementStats.HousingAccess + 1.0 - _settlementStats.Hunger / 64.0) / 2.0, 0, 1));
                _worldTrade.Tick(worldDelta /
                    OriginalGameData.Current.SecondsPerDay, worldTick.Day);
            }
            _invasion.Tick(SimulationClock.FixedStep, OriginalGameData.Current.SecondsPerDay);
            _corpses.Tick(SimulationClock.FixedStep, OriginalGameData.Current.SecondsPerDay);
            _settlementStats.Tick(
                SimulationClock.FixedStep, OriginalGameData.Current.SecondsPerDay,
                _citizens, _resources, _rooms, _corpses);
            var eventExit = _entry.Reachable.FirstOrDefault()?.Cell ?? new GridCoord(GridWorld.Width / 2, 0);
            var yearPart = (_clock.PlayedSeconds /
                (OriginalGameData.Current.SecondsPerDay * 16.0)) % 1.0;
            _events.Tick(SimulationClock.FixedStep, _clock.PlayedSeconds,
                OriginalGameData.Current.SecondsPerDay, _citizens, _resources, _jobs,
                _settlementStats, _rooms, eventExit,
                _weather.Temperature - _weather.AverageTemperature(yearPart));
            _roadMaintenance.Tick(
                SimulationClock.FixedStep, _jobs, cell => _rooms.Contains(cell));
            _maintenanceConsumption.Tick(SimulationClock.FixedStep);
        }
        _world.UpdateWeatherVisuals(_weather.Ice, _weather.Moisture);
        _rightSidebar.UpdateWeather(_weather.Ice);
        _citizens.SyncRenderTransforms();

        if (ticks > 0)
        {
            _jobs.RemoveCompleted();
            _rooms.UpdateStates();
            _hauling.RetryUnassigned(_jobs);
            _rooms.SynchronizeProductionReadiness(_jobs, _resources);
            _rooms.ReconcileProductionJobs(_jobs.All);
            _rooms.Construction.Reconcile(_jobs.All);
            _rooms.Construction.Schedule(_rooms, _jobs, _resources);
            _roadMaintenance.Reconcile(_jobs.All);
            var dayFraction = (_clock.PlayedSeconds / OriginalGameData.Current.SecondsPerDay) % 1.0;
            _rooms.ScheduleProduction(_jobs, _resources, dayFraction);
            _rooms.ScheduleHospitalSupplies(_jobs, _resources);
            _rooms.ScheduleFacilityWork(_jobs, _resources);
            _corpses.Schedule(_jobs);
        }
        _uiRefreshAccumulator += delta;
        if (_uiRefreshAccumulator >= 0.25)
        {
            _uiRefreshAccumulator %= 0.25;
            RefreshRuntimeUi();
        }
        if (ticks > 0)
        {
            _autoSaveAccumulator += delta;
            if (_autoSaveAccumulator >= AutoSaveSeconds)
            {
                _autoSaveAccumulator %= AutoSaveSeconds;
                SaveGameService.SaveAuto(CaptureSnapshot());
            }
        }
    }

    private void RefreshRuntimeUi()
    {
        var roadInfo = _tool == BuildTool.Road
            ? $" · дорога {SelectedRoad().Key} (C — сменить)"
            : "";
        _toolLabel.Text = $"{ToolTitle(_tool)}{roadInfo} · {_roomPlanner.SelectedDefinition} · " +
                          $"площадь {_roomPlanner.Area}, проёмы {_roomPlanner.Doors}, " +
                          $"автостены {(_roomPlanner.AutoWalls ? "да" : "нет")}";
        if (_invasion.Active)
            _status.Text = $"Вторжение: {_invasion.Phase} · врагов {_invasion.EnemyMen} · " +
                           $"потери {_invasion.PlayerLosses}/{_invasion.EnemyDeaths}";
        _topBar.UpdateState();
        _roomPolicy.Refresh();
        UpdateEconomyInfo();
        _notifications.Refresh(_events.Notices, OriginalGameData.Current.SecondsPerDay);
        _administration.Refresh(_rooms, _settlementStats);
        _manager.Refresh();
        _civic.Refresh();
        _battlePanel.Refresh();
        UpdateBattlePause();
        _globalMap.SetCameraCell(_world.WorldToCell(_camera.Position));
        _rightSidebar.UpdateState(_world.WorldToCell(_camera.Position), _camera.Size);
        _timeControls.UpdateState(_clock.PlayedSeconds,
            OriginalGameData.Current.SecondsPerDay, _clock.SpeedLevel);
        RefreshInspector();
        if (_roomPalette.Visible) _roomPalette.RefreshDraft();
        RefreshFurnisherSummary();
    }

    private GridCoord? ScreenToCell(Vector2 mousePosition)
    {
        var from = _camera.ProjectRayOrigin(mousePosition);
        var direction = _camera.ProjectRayNormal(mousePosition);
        if (Mathf.Abs(direction.Y) < 0.0001f) return null;
        var distance = -from.Y / direction.Y;
        if (distance < 0f) return null;
        var cell = _world.WorldToCell(from + direction * distance);
        return _world.IsInside(cell) ? cell : null;
    }

    private void RefreshFurnitureCursorPreview(bool force = false)
    {
        if (_tool != BuildTool.Furniture || !_roomPlanner.UsesDefinition ||
            (!_roomPlanner.HasDraft && !_roomPlanner.UsesFixedItemPlacement) ||
            _dragStart is not null) return;
        if (ScreenToCell(GetViewport().GetMousePosition()) is { } cell)
        {
            if (!force && _lastFurniturePreviewCell == cell &&
                _lastFurniturePreviewDefinition == _roomPlanner.SelectedDefinition &&
                _lastFurniturePreviewGroup == _selectedFurnisherGroup &&
                _lastFurniturePreviewVariant == _selectedFurnisherVariant &&
                _lastFurniturePreviewRotation == _selectedFurnisherRotation)
                return;
            _lastFurniturePreviewCell = cell;
            _lastFurniturePreviewDefinition = _roomPlanner.SelectedDefinition;
            _lastFurniturePreviewGroup = _selectedFurnisherGroup;
            _lastFurniturePreviewVariant = _selectedFurnisherVariant;
            _lastFurniturePreviewRotation = _selectedFurnisherRotation;
            if (_roomPlanner.UsesFixedItemPlacement)
                _roomPlanner.PreviewFixedFurniture(cell, _selectedFurnisherGroup,
                    _selectedFurnisherVariant, _selectedFurnisherRotation);
            else
                _roomPlanner.PreviewFurniture(cell, _selectedFurnisherGroup,
                    _selectedFurnisherVariant, _selectedFurnisherRotation);
        }
    }

    private void ChangeFurnisherVariant(int delta)
    {
        if (!_roomPlanner.UsesDefinition) return;
        var variants = FurnisherLayoutCatalog.Variants(
            _roomPlanner.SelectedDefinition, _selectedFurnisherGroup);
        _selectedFurnisherVariant = Math.Clamp(
            _selectedFurnisherVariant + delta, 0, Math.Max(0, variants.Count - 1));
        RefreshFurnisherControls();
        RefreshFurnitureCursorPreview(true);
    }

    private void RotateFurnisher()
    {
        _selectedFurnisherRotation = (_selectedFurnisherRotation + 1) % 4;
        RefreshFurnisherControls();
        RefreshFurnitureCursorPreview(true);
    }

    private void TryPlaceLandingParty(GridCoord throneCenter)
    {
        var origin = new GridCoord(throneCenter.X - 4, throneCenter.Z - 2);
        var footprint = Rectangle(origin, new GridCoord(origin.X + 8, origin.Z + 11)).ToArray();
        if (footprint.Any(cell => !_world.IsInside(cell)))
        {
            _status.Text = "Высадка не помещается на карте";
            return;
        }
        var throne = Rectangle(
            new GridCoord(origin.X + 2, origin.Z + 1),
            new GridCoord(origin.X + 6, origin.Z + 3)).ToArray();
        var subjectOffsets = new (int X, int Z)[]
        {
            (1, 8), (7, 8), (1, 9), (2, 9), (6, 9),
            (7, 9), (1, 10), (2, 10), (6, 10), (7, 10)
        };
        if (throne.Concat(subjectOffsets.Select(offset =>
                new GridCoord(origin.X + offset.X, origin.Z + offset.Z)))
            .Any(cell => _world.Data.IsBlocked(cell)))
        {
            _status.Text = "Здесь нельзя разместить трон; выберите свободное место";
            return;
        }

        void PlaceLandingWall(GridCoord cell)
        {
            if (_world.Data.Has(cell, TileFlags.Cave))
            {
                _world.Data.SetCave(cell, false);
                _world.Data.SetTerrain(cell, GroundKind.Mountain,
                    _world.Data.Elevation(cell), 0, _world.Data.Moisture(cell));
                return;
            }
            _world.BuildWall(cell);
        }

        for (var x = 0; x < 9; x++)
        {
            PlaceLandingWall(new GridCoord(origin.X + x, origin.Z));
            if (x is < 3 or > 5)
                PlaceLandingWall(new GridCoord(origin.X + x, origin.Z + 7));
        }
        for (var z = 1; z <= 6; z++)
        {
            PlaceLandingWall(new GridCoord(origin.X, origin.Z + z));
            PlaceLandingWall(new GridCoord(origin.X + 8, origin.Z + z));
        }
        // PlacerLanding rr/th/to/resource compounds all include the mud roof.
        for (var z = 1; z <= 6; z++)
        for (var x = 1; x <= 7; x++)
        {
            var cell = new GridCoord(origin.X + x, origin.Z + z);
            if (!_world.Data.Has(cell, TileFlags.Cave)) _world.Data.SetRoof(cell, true);
        }
        for (var x = 3; x <= 5; x++)
        {
            var cell = new GridCoord(origin.X + x, origin.Z + 7);
            if (!_world.Data.Has(cell, TileFlags.Cave)) _world.Data.SetRoof(cell, true);
        }
        _rooms.PlaceLandingThrone(throne);
        // BOOSTABLES.CIVICS.LANDING has source baseValue 0.0.
        var landingBoost = Math.Max(0, GameSession.TitleBonuses.Apply("CIVIC_LANDING", 0));
        _rooms.Trade.Credits += (int)(LandingBaseCredits * landingBoost);
        _world.PlaceLandingMarker(throneCenter, new Vector3(1.4f, 1.4f, 1.4f),
            new Color("d1aa52"), "Throne");

        var supplies = LandingSupplies(origin, landingBoost).ToArray();
        _resources.InitializeLandingParty(supplies.Select(supply =>
            new LandingResourceRule(supply.Kind, supply.Amount)));
        foreach (var supply in supplies)
        {
            var cell = supply.Cell;
            _rooms.ConfigureLandingSupply(supply.Kind, cell);
            _hauling.SpawnAccounted(cell, supply.Kind, supply.Amount, _jobs);
            _world.PlaceLandingMarker(cell, new Vector3(0.72f, 0.55f, 0.72f),
                supply.Color, $"Landing_{supply.Kind}_{supply.Amount}");
        }
        var subjects = 10 + (int)(10 * landingBoost);
        var perTile = (int)Math.Ceiling(subjects / 10.0);
        var remainingSubjects = subjects;
        foreach (var offset in subjectOffsets)
        for (var index = 0; index < perTile && remainingSubjects > 0; index++)
        {
            _citizens.SpawnCitizen(
                new GridCoord(origin.X + offset.X, origin.Z + offset.Z),
                GameSession.Current?.PlayerRace ?? "HUMAN", SocialClass.Citizen,
                HumanoidType.Subject, CitizenOrigin.Immigrant, ArrivalCause.Immigrated);
            remainingSubjects--;
        }
        _landingOrigin = origin;
        _landingPending = false;
        FocusCamera(throneCenter);
        _status.Text = $"Высадка завершена: трон, {subjects} подданных и стартовые припасы";
    }

    private void ApplySelection(GridCoord start, GridCoord end)
    {
        if (_tool == BuildTool.RoomInspect)
        {
            SelectCell(end);
            return;
        }
        if (_tool == BuildTool.RoomArea)
        {
            if (_roomPlanner.HasDraft) _roomPlanner.ExpandArea(Rectangle(start, end));
            else _roomPlanner.SetArea(Rectangle(start, end));
            _constructionPalette.Visible = true;
            return;
        }
        if (_tool == BuildTool.RoomShrink)
        {
            _roomPlanner.ShrinkArea(Rectangle(start, end));
            _constructionPalette.Visible = true;
            return;
        }
        if (_tool == BuildTool.Furniture && _roomPlanner.UsesFixedItemPlacement)
        {
            if (_roomPlanner.PlaceFixedFurniture(end, _selectedFurnisherGroup,
                    _selectedFurnisherVariant, _selectedFurnisherRotation, _jobs))
                _status.Text = "Дом запланирован: жители доставят материалы и построят его";
            else
                _status.Text = _roomPlanner.PlacementStatus;
            _lastFurniturePreviewCell = null;
            RefreshFurnisherControls();
            return;
        }
        if (_tool == BuildTool.Furniture && _roomPlanner.HasDraft && _roomPlanner.UsesDefinition)
        {
            // Mouse release commits the currently previewed furnisher.  The old
            // branch only redrew a ghost, so selecting another group replaced it
            // and no FurniturePlacement ever reached the room draft.
            if (!_roomPlanner.ToggleFurniture(end, _selectedFurnisherGroup,
                    _selectedFurnisherVariant, _selectedFurnisherRotation))
                _status.Text = _roomPlanner.PlacementStatus;
            _lastFurniturePreviewCell = null;
            RefreshFurnisherControls();
            return;
        }
        if (_tool == BuildTool.Door)
        {
            _roomPlanner.ToggleDoor(end);
            return;
        }
        var cells = Line(start, end);
        foreach (var cell in cells)
        {
            if (_tool == BuildTool.Cancel)
            {
                var job = _jobs.GetAt(cell);
                if (job is { RoomId: > 0 } && job.IsConstruction)
                {
                    _rooms.Dismantle(job.RoomId, _jobs, _resources,
                        (resource, amount, dropCell) => _hauling.Spawn(dropCell, resource, amount, _jobs));
                    _status.Text = "Строительный проект комнаты отменён";
                    break;
                }
                if (job is not null && job.IsConstruction)
                {
                    var delivered = job.TakeDeliveredMaterials();
                    if (delivered > 0) _hauling.Spawn(job.Cell, job.Resource, delivered, _jobs);
                }
                _jobs.Cancel(cell, _resources);
                continue;
            }
            switch (_tool)
            {
                case BuildTool.Wall when _roomPlanner.HasDraft:
                    _status.Text = "Стены комнаты остаются чертежом до нажатия «Готово»";
                    break;
                case BuildTool.Wall when _world.CanPlanWall(cell):
                    _world.ReserveWall(cell);
                    _jobs.Add(new BuildJob(cell, BuildKind.Wall));
                    break;
                case BuildTool.Road when _world.CanPlanRoad(cell):
                    _world.ReserveRoad(cell);
                    _jobs.Add(BuildJob.Road(cell, SelectedRoad()));
                    break;
                case BuildTool.Furniture when _roomPlanner.HasDraft && _roomPlanner.UsesDefinition:
                    _roomPlanner.ToggleFurniture(cell, _selectedFurnisherGroup,
                        _selectedFurnisherVariant, _selectedFurnisherRotation);
                    break;
                case BuildTool.Furniture when _world.CanPlanFurniture(cell) && _rooms.Contains(cell):
                    _world.ReserveFurniture(cell);
                    _jobs.Add(new BuildJob(cell, BuildKind.Furniture));
                    break;
                case BuildTool.Forage when _world.Data.GrowableType(cell) >= 0:
                    _jobs.Add(new BuildJob(cell, BuildKind.Forage));
                    break;
                case BuildTool.ClearWood when _world.Data.VegetationAmount(cell) > 0 &&
                                              _world.Data.GrowableType(cell) < 0:
                    _jobs.Add(new BuildJob(cell, BuildKind.ClearWood));
                    break;
                case BuildTool.ClearStone when _world.Data.Has(cell, TileFlags.ClearableTerrain) &&
                                               !_world.Data.Has(cell, TileFlags.Mountain):
                    _jobs.Add(new BuildJob(cell, BuildKind.ClearStone));
                    break;
                case BuildTool.ClearAll when _world.Data.VegetationAmount(cell) > 0 &&
                                             _world.Data.GrowableType(cell) < 0:
                    _jobs.Add(new BuildJob(cell, BuildKind.ClearWood));
                    break;
                case BuildTool.ClearAll when _world.Data.Has(cell, TileFlags.ClearableTerrain) &&
                                             !_world.Data.Has(cell, TileFlags.Mountain):
                    _jobs.Add(new BuildJob(cell, BuildKind.ClearStone));
                    break;
                case BuildTool.ClearWater when _world.Data.Has(cell, TileFlags.Water):
                    _jobs.Add(new BuildJob(cell, BuildKind.ClearWater));
                    break;
                case BuildTool.DigTunnel when _world.Data.Has(cell, TileFlags.Mountain) &&
                                              !_world.Data.Has(cell, TileFlags.Cave):
                    _jobs.Add(new BuildJob(cell, BuildKind.DigTunnel));
                    break;
            }
        }
        if (_tool == BuildTool.Road || (_tool == BuildTool.Wall && !_roomPlanner.HasDraft) ||
            _tool is BuildTool.Forage or BuildTool.ClearWood or BuildTool.ClearStone or
                BuildTool.ClearAll or BuildTool.ClearWater or BuildTool.DigTunnel)
            _world.ClearRoomPreview();
        if (_tool == BuildTool.Furniture) RefreshFurnisherControls();
    }

    private void PreviewSelection(GridCoord start, GridCoord end)
    {
        if (_tool is BuildTool.RoomArea or BuildTool.RoomShrink)
        {
            _roomPlanner.PreviewArea(Rectangle(start, end), _tool == BuildTool.RoomShrink);
            return;
        }
        if (_tool == BuildTool.Furniture && _roomPlanner.UsesFixedItemPlacement)
        {
            _roomPlanner.PreviewFixedFurniture(end, _selectedFurnisherGroup,
                _selectedFurnisherVariant, _selectedFurnisherRotation);
            return;
        }
        if (_tool == BuildTool.Furniture && _roomPlanner.HasDraft && _roomPlanner.UsesDefinition)
        {
            _roomPlanner.PreviewFurniture(end, _selectedFurnisherGroup,
                _selectedFurnisherVariant, _selectedFurnisherRotation);
            return;
        }
        if (_tool == BuildTool.Road || (_tool == BuildTool.Wall && !_roomPlanner.HasDraft) ||
            _tool is BuildTool.Forage or BuildTool.ClearWood or BuildTool.ClearStone or
                BuildTool.ClearAll or BuildTool.ClearWater or BuildTool.DigTunnel)
            _world.ShowRoomPreview(Line(start, end), Array.Empty<GridCoord>(), Array.Empty<GridCoord>());
    }

    private FloorRule SelectedRoad() => _roadTypes.Count > 0
        ? _roadTypes[_roadTypeIndex]
        : OriginalGameData.Current.DefaultRoad();

    private static IEnumerable<GridCoord> Rectangle(GridCoord a, GridCoord b)
    {
        for (var z = Math.Min(a.Z, b.Z); z <= Math.Max(a.Z, b.Z); z++)
            for (var x = Math.Min(a.X, b.X); x <= Math.Max(a.X, b.X); x++)
                yield return new GridCoord(x, z);
    }

    private static IEnumerable<GridCoord> Line(GridCoord a, GridCoord b)
    {
        var x = a.X;
        var z = a.Z;
        var dx = Math.Abs(b.X - a.X);
        var dz = Math.Abs(b.Z - a.Z);
        var sx = a.X < b.X ? 1 : -1;
        var sz = a.Z < b.Z ? 1 : -1;
        var error = dx - dz;
        while (true)
        {
            yield return new GridCoord(x, z);
            if (x == b.X && z == b.Z) yield break;
            var doubled = error * 2;
            if (doubled > -dz) { error -= dz; x += sx; }
            if (doubled < dx) { error += dx; z += sz; }
        }
    }

    private void MoveCamera(float delta)
    {
        var direction = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) direction.Z -= 1;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) direction.Z += 1;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) direction.X -= 1;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) direction.X += 1;
        // GameWindow.java: maxSpeed=32*TILE_SIZE and acceleration=maxSpeed*1.5;
        // settlement coordinates are tiles, so divide the source pixel values by TILE_SIZE.
        var zoomScale = Math.Max(1f, _camera.Size / 20f);
        var maximum = 32f * zoomScale;
        var acceleration = 48f * zoomScale;
        if (direction != Vector3.Zero)
            _cameraVelocity += direction.Normalized() * acceleration * delta;
        else
        {
            var braking = acceleration * 4f * delta;
            _cameraVelocity.X = Mathf.MoveToward(_cameraVelocity.X, 0, braking);
            _cameraVelocity.Z = Mathf.MoveToward(_cameraVelocity.Z, 0, braking);
        }
        var planar = new Vector2(_cameraVelocity.X, _cameraVelocity.Z);
        if (planar.Length() > maximum)
        {
            planar = planar.Normalized() * maximum;
            _cameraVelocity.X = planar.X;
            _cameraVelocity.Z = planar.Y;
        }
        _camera.Position += _cameraVelocity * delta;
        ClampCamera();
    }

    private void ClampCamera()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        var aspect = viewport.Y <= 0 ? 16f / 9f : viewport.X / viewport.Y;
        var halfX = Math.Min(GridWorld.Width / 2f, _camera.Size * aspect * 0.5f);
        var halfZ = Math.Min(GridWorld.Height / 2f, _camera.Size * 0.5f);
        _camera.Position = new Vector3(
            Mathf.Clamp(_camera.Position.X, -GridWorld.Width / 2f + halfX,
                GridWorld.Width / 2f - halfX),
            _camera.Position.Y,
            Mathf.Clamp(_camera.Position.Z, -GridWorld.Height / 2f + halfZ,
                GridWorld.Height / 2f - halfZ));
    }

    private void CreateUi()
    {
        _uiLayer = new CanvasLayer();
        AddChild(_uiLayer);
        var layer = _uiLayer;

        _topBar = new SettlementTopBar { Name = "SettlementTopBar" };
        layer.AddChild(_topBar);
        _topBar.Initialize(_citizens, _rooms, _settlementStats, _hauling,
            _settlementWorld, _raiding);
        _topBar.CitizensRequested += () => OpenAdministration(_administration.ShowCitizens);
        _topBar.SlavesRequested += () => OpenAdministration(_administration.ShowSlaves);
        _topBar.NoblesRequested += () => OpenCivic(_civic.OpenNobles);
        _topBar.RoomsRequested += () => OpenCivic(_civic.OpenRooms);
        _topBar.SubjectsRequested += () => OpenAdministration(_administration.ShowSubjects);
        _topBar.HousingRequested += () => OpenCivic(_civic.OpenHousing);
        _topBar.LawRequested += () => OpenAdministration(_administration.ShowLaw);
        _topBar.ArmyRequested += () => OpenCivic(_civic.OpenArmy);
        _topBar.HealthRequested += () => OpenCivic(_civic.OpenHealth);
        _topBar.FoodRequested += () => OpenCivic(_civic.OpenFood);
        _topBar.GoodsRequested += () => OpenManager(_manager.OpenGoods);
        _topBar.TourismRequested += () => OpenManager(_manager.OpenTourism);
        _topBar.RaidersRequested += () => OpenManager(_manager.OpenRaiders);
        _topBar.LevelRequested += () => OpenManager(_manager.OpenLevel);
        _topBar.ProfileRequested += () => OpenManager(_manager.OpenProfile);
        _topBar.TechnologyRequested += () => OpenAdministration(_administration.ShowTechnology);
        _topBar.EconomyRequested += () => OpenManager(_manager.OpenEconomy);
        _topBar.NotificationsRequested += () => ToggleWindow(_notifications);
        _topBar.WorldLogRequested += () =>
        {
            CloseWindows();
            _referencePanel.OpenWorldLog(_events.Notices, OriginalGameData.Current.SecondsPerDay);
        };
        _topBar.AdviceRequested += () =>
        {
            CloseWindows();
            _referencePanel.OpenAdvice();
        };
        _topBar.WikiRequested += () =>
        {
            CloseWindows();
            _referencePanel.OpenWiki();
        };
        _topBar.WorldRequested += () => WorldRequested?.Invoke();
        _topBar.BattleRequested += () =>
        {
            CloseWindows();
            if (_settlementWorld.Battles.Pending is not null) _battlePanel.Open();
            else { _battleCommands.Refresh(); _battleCommands.Visible = true; }
        };
        _topBar.MainMenuRequested += () => MainMenuRequested?.Invoke();

        _status = new Label
        {
            Position = new Vector2(8, 54), Size = new Vector2(1008, 24),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _status.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        layer.AddChild(_status);
        _toolLabel = new Label
        {
            Position = new Vector2(8, 628), Size = new Vector2(1018, 24),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _toolLabel.AddThemeColorOverride("font_color", new Color("d8d4bf"));
        layer.AddChild(_toolLabel);

        _roomPolicy = new RoomPolicyPanel { Name = "RoomPolicy" };
        layer.AddChild(_roomPolicy);
        _roomPolicy.Initialize(_rooms, _jobs, _resources, () => _selectedRoom);
        _roomPolicy.Visible = false;

        _economyPanel = new ColorRect
        {
            Color = new Color(0.03f, 0.04f, 0.05f, 0.84f),
            Position = new Vector2(444, 116),
            Size = new Vector2(548, 180),
            MouseFilter = Control.MouseFilterEnum.Stop,
            Visible = false
        };
        layer.AddChild(_economyPanel);
        _economyInfo = new Label { Position = new Vector2(12, 10) };
        _economyPanel.AddChild(_economyInfo);

        _inspector = new SettlementInspectorPanel { Name = "SettlementInspector" };
        layer.AddChild(_inspector);
        _inspector.Initialize();
        _inspector.Visible = false;

        _roomPalette = new RoomBuildPalette { Name = "RoomBuildPalette" };
        layer.AddChild(_roomPalette);
        _roomPalette.Initialize(_rooms.Blueprints, _roomPlanner, _rooms.CanCreateRoom);
        _roomPalette.RoomSelected += SelectRoomDefinition;
        _roomPalette.BuildActionSelected += SelectBuildAction;
        _roomPalette.Visible = false;

        _constructionPalette = CreateConstructionPalette(layer);
        _constructionPalette.Visible = false;

        _timeControls = new TimeControlBar { Name = "TimeControls" };
        layer.AddChild(_timeControls);
        _timeControls.Initialize();
        _timeControls.SpeedSelected += _clock.SetSpeedLevel;

        _globalMap = new GlobalMapOverlay { Name = "GlobalMap" };
        layer.AddChild(_globalMap);
        _globalMap.Initialize(_world.Data, _rooms, _citizens);
        _globalMap.CellSelected += FocusCamera;
        _globalMap.LayerChanged += layer =>
        {
            if (layer == SettlementMapLayer.Terrain) _world.ApplyGroundOverlay(null);
            else _world.ApplyGroundOverlay(_globalMap.ColorAt);
        };
        _globalMap.LayerRefreshed += layer =>
        {
            if (layer != SettlementMapLayer.Terrain)
                _world.ApplyGroundOverlay(_globalMap.ColorAt);
        };
        _globalMap.Visible = false;

        _rightSidebar = new SettlementRightSidebar { Name = "SettlementRightSidebar" };
        layer.AddChild(_rightSidebar);
        _rightSidebar.Initialize(_world.Data, _resources, _citizens, _rooms);
        _rightSidebar.CellSelected += FocusCamera;
        _rightSidebar.ZoomRequested += amount =>
            _camera.Size = Mathf.Clamp(_camera.Size + amount, 7f, 80f);
        _rightSidebar.MapRequested += OpenSettlementMap;
        _rightSidebar.OverlayRequested += _globalMap.SetLayer;
        _rightSidebar.EconomyRequested += () => OpenManager(_manager.OpenGoods);
        _rightSidebar.HotspotPlacementRequested += () =>
        {
            CloseWindows();
            _placingHotspot = true;
            _status.Text = "ЛКМ — поставить метку, ПКМ — отменить";
        };
        _rightSidebar.HideUiRequested += () =>
        {
            CloseWindows();
            _uiLayer.Visible = false;
        };

        _notifications = new SettlementNotificationFeed { Name = "Notifications" };
        layer.AddChild(_notifications);
        _notifications.Initialize();
        _notifications.Visible = false;

        _referencePanel = new SettlementReferencePanel { Name = "SettlementReference" };
        layer.AddChild(_referencePanel);
        _referencePanel.Initialize();

        _administration = new AdministrationDashboard { Name = "Administration" };
        layer.AddChild(_administration);
        _administration.Initialize(_rooms, _citizens);
        _administration.CitizenFocusRequested += FocusCamera;

        _civic = new SettlementCivicPanel { Name = "SettlementCivic" };
        layer.AddChild(_civic);
        _civic.Initialize(_world, _rooms, _citizens, _resources, _economy, _settlementStats);
        _civic.FocusRequested += FocusCamera;

        _manager = new SettlementManagerPanel { Name = "SettlementManager" };
        layer.AddChild(_manager);
        _manager.Initialize(_resources, _economy, _rooms, _hauling,
            _settlementWorld, _raiding, _citizens);
        _manager.TechnologyRequested += () => OpenAdministration(_administration.ShowTechnology);
        _manager.TradeRequested += () => OpenAdministration(_administration.ShowTrade);

        _strategicMap = new StrategicWorldMap { Name = "StrategicWorldMap" };
        layer.AddChild(_strategicMap);
        _strategicMap.Initialize(
            _strategicWorld, _regionalEconomy, _rooms.Trade, _settlementWorld,
            _rooms.Governance);

        _battlePanel = new WorldBattlePanel { Name = "WorldBattlePanel" };
        layer.AddChild(_battlePanel);
        _battlePanel.Initialize(_settlementWorld, _invasion);
        _battlePanel.SettlementDefenseRequested += () =>
            _status.Text = "Враг входит на карту столицы — подготовка городской обороны";

        _battleCommands = new SettlementBattleCommandPanel { Name = "SettlementBattleCommands" };
        layer.AddChild(_battleCommands);
        _battleCommands.Initialize(_rooms.Military);
        _battleCommands.Visible = false;
        _battleCommands.CloseRequested += () => { _battleCommands.Visible = false; _world.ClearRoomPreview(); };
        _battleCommands.ImmediateOrderIssued += (divisionId, task) =>
        {
            if (task != DivisionBattleTask.Stop) return;
            var division = _rooms.Military.Divisions.FirstOrDefault(value => value.Id == divisionId);
            if (division is not null) _citizens.StopMilitaryOrders(division.Members);
        };

        _bottomToolbar = new ColorRect
        {
            Color = new Color(0.03f, 0.04f, 0.05f, 0.92f),
            Size = new Vector2(568, 56),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        layer.AddChild(_bottomToolbar);
        var x = 8f;
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(0), "Сельское хозяйство", ref x,
            () => OpenRoomCategory("Сельское хозяйство"));
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(1), "Работы", ref x,
            () => OpenRoomCategory("Работы"));
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(2), "Службы", ref x,
            () => OpenRoomCategory("Службы"));
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(4), "Управление", ref x,
            () => OpenRoomCategory("Управление"));
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(19), "Строительство", ref x,
            OpenConstructionMenu);
        AddIconHudButton(_bottomToolbar, OriginalUiIcons.MainCategory(3), "Задания", ref x,
            OpenJobsMenu);
        _bottomToolbar.Size = new Vector2(x + 4f, 56f);

        ApplyResponsiveLayout();
        GetViewport().SizeChanged += ApplyResponsiveLayout;

        SelectTool(BuildTool.Wall);
    }

    private void ApplyResponsiveLayout()
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        var playWidth = Mathf.Max(320f, viewport.X - 242f);
        _status.Position = new Vector2(8f, 54f);
        _status.Size = new Vector2(Mathf.Max(1f, playWidth - 16f), 24f);
        _toolLabel.Position = new Vector2(8f, Mathf.Max(80f, viewport.Y - 92f));
        _toolLabel.Size = new Vector2(Mathf.Max(1f, playWidth - 16f), 24f);
        _bottomToolbar.Position = new Vector2(
            Mathf.Max(8f, (viewport.X - _bottomToolbar.Size.X) * 0.5f),
            Mathf.Max(84f, viewport.Y - _bottomToolbar.Size.Y - 8f));
        _constructionPalette.Position = new Vector2(
            Mathf.Max(8f, (viewport.X - _constructionPalette.Size.X) * 0.5f), 80f);
        _economyPanel.Position = new Vector2(
            Mathf.Max(8f, (playWidth - _economyPanel.Size.X) * 0.5f),
            Mathf.Max(84f, (viewport.Y - _economyPanel.Size.Y) * 0.25f));
    }

    private static void AddIconHudButton(
        Control parent, Texture2D? icon, string tooltip, ref float x, Action pressed)
    {
        var button = new Button
        {
            Icon = icon,
            TooltipText = tooltip,
            Position = new Vector2(x, 4),
            Size = new Vector2(48, 48),
            FocusMode = Control.FocusModeEnum.None
        };
        button.AddThemeStyleboxOverride("normal", HudButtonStyle(new Color("1d1f1b"), new Color("55584d")));
        button.AddThemeStyleboxOverride("hover", HudButtonStyle(new Color("34382e"), new Color("b3a56f")));
        button.AddThemeStyleboxOverride("pressed", HudButtonStyle(new Color("444838"), new Color("e1cc82")));
        button.Pressed += pressed;
        parent.AddChild(button);
        x += 52;
    }

    private ColorRect CreateConstructionPalette(CanvasLayer layer)
    {
        var menu = new ColorRect
        {
            Color = new Color(0.055f, 0.058f, 0.055f, 0.98f),
            // UIRoomPlacer/Config: SShape + SMaterial | SItems | SStats,
            // wrapped by SFrame with the destructive/confirm controls at the bottom.
            Size = new Vector2(736, 250),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        layer.AddChild(menu);
        _constructionTitle = new Label
        {
            Text = "СТРОИТЕЛЬСТВО",
            Position = new Vector2(8, 4), Size = new Vector2(720, 28),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _constructionTitle.AddThemeFontSizeOverride("font_size", 18);
        _constructionTitle.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        menu.AddChild(_constructionTitle);
        _constructionShapeControls = new VBoxContainer
        {
            Position = new Vector2(8, 36),
            Size = new Vector2(176, 168)
        };
        var column = _constructionShapeControls;
        menu.AddChild(column);
        column.AddChild(ConstructionSectionTitle("ФОРМА"));
        var shapeRow = new HBoxContainer();
        shapeRow.AddThemeConstantOverride("separation", 2);
        column.AddChild(shapeRow);
        AddConstructionIconAction(shapeRow, OriginalUiIcons.Medium(33), "Расширить форму комнаты",
            () =>
            {
                _roomPlanner.BuildOnExistingStructures = false;
                SelectConstructionTool(BuildTool.RoomArea);
            });
        AddConstructionIconAction(shapeRow, OriginalUiIcons.MediumWithBadge(33, 42),
            "Расширить форму поверх существующих конструкций",
            () =>
            {
                _roomPlanner.BuildOnExistingStructures = true;
                SelectConstructionTool(BuildTool.RoomArea);
            });
        AddConstructionIconAction(shapeRow, OriginalUiIcons.Medium(34), "Уменьшить форму комнаты",
            () => SelectConstructionTool(BuildTool.RoomShrink));
        column.AddChild(ConstructionSectionTitle("МАТЕРИАЛ И ПРОЁМЫ"));
        var materialRow = new HBoxContainer();
        materialRow.AddThemeConstantOverride("separation", 2);
        column.AddChild(materialRow);
        _autoWallsButton = AddConstructionIconAction(materialRow, OriginalUiIcons.Medium(11),
            "Автоматически построить стены по периметру", () =>
            {
                _roomPlanner.AutoWalls = !_roomPlanner.AutoWalls;
                _autoWallsButton.ButtonPressed = _roomPlanner.AutoWalls;
                RefreshFurnisherSummary();
            }, true);
        _autoWallsButton.ButtonPressed = _roomPlanner.AutoWalls;
        AddConstructionIconAction(materialRow, OriginalUiIcons.Medium(104), "Установить или убрать дверной проём",
            () => SelectConstructionTool(BuildTool.Door));
        AddConstructionIconAction(materialRow, OriginalUiIcons.Medium(6), "Убрать дверной проём",
            () => SelectConstructionTool(BuildTool.Door));
        AddConstructionIconAction(materialRow, OriginalUiIcons.Medium(96), "Выбрать материал конструкции", () =>
        {
            SelectTool(BuildTool.RoomInspect);
            ToggleWindow(_inspector);
        });
        _constructionShapeSeparator = new ColorRect
        {
            Position = new Vector2(190, 36), Size = new Vector2(2, 168),
            Color = new Color("575950"), MouseFilter = Control.MouseFilterEnum.Ignore
        };
        menu.AddChild(_constructionShapeSeparator);
        _fixedStructureControls = new VBoxContainer
        {
            Position = new Vector2(8, 36),
            Size = new Vector2(176, 168),
            Visible = false
        };
        _fixedStructureControls.AddThemeConstantOverride("separation", 4);
        menu.AddChild(_fixedStructureControls);
        _fixedStructureControls.AddChild(ConstructionSectionTitle("ИЗОЛЯЦИЯ 100%"));
        _fixedStructureControls.AddChild(ConstructionSectionTitle("МАТЕРИАЛ"));
        var fixedMaterialRow = new HBoxContainer();
        fixedMaterialRow.AddThemeConstantOverride("separation", 2);
        _fixedStructureControls.AddChild(fixedMaterialRow);
        _fixedAutoWallsButton = AddConstructionIconAction(
            fixedMaterialRow, OriginalUiIcons.Medium(11),
            "Автоматически построить внешние стены дома", () =>
            {
                _roomPlanner.AutoWalls = !_roomPlanner.AutoWalls;
                _fixedAutoWallsButton.ButtonPressed = _roomPlanner.AutoWalls;
            }, true);
        _fixedStructureButton = AddConstructionIconAction(
            fixedMaterialRow, OriginalUiIcons.Medium(96),
            "Выбрать материал конструкции", OpenFixedStructureMenu);
        _fixedStructurePopup = new PopupPanel();
        menu.AddChild(_fixedStructurePopup);
        var structureList = new VBoxContainer { CustomMinimumSize = new Vector2(250, 0) };
        _fixedStructurePopup.AddChild(structureList);
        foreach (var structure in OriginalGameData.Current.Structures.Values
                     .Where(value => !value.Key.StartsWith('_'))
                     .OrderBy(value => value.Key))
        {
            var structureKey = structure.Key;
            var selectStructure = new Button
            {
                Text = $"{RussianStructureName(structure.Key)}  ·  {structure.Resource} ×{structure.ResourceAmount}",
                Alignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(250, 34)
            };
            selectStructure.Pressed += () =>
            {
                _roomPlanner.StructureKey = structureKey;
                _fixedStructurePopup.Hide();
                RefreshFurnisherControls();
                _status.Text = $"Материал стен: {RussianStructureName(structureKey)}";
            };
            structureList.AddChild(selectStructure);
        }
        _furnisherControls = new VBoxContainer
        {
            Position = new Vector2(202, 36), Size = new Vector2(248, 168)
        };
        _furnisherControls.AddThemeConstantOverride("separation", 4);
        menu.AddChild(_furnisherControls);
        _constructionStatsSeparator = new ColorRect
        {
            Position = new Vector2(458, 36), Size = new Vector2(2, 168),
            Color = new Color("575950"), MouseFilter = Control.MouseFilterEnum.Ignore
        };
        menu.AddChild(_constructionStatsSeparator);
        _furnisherCost = new Label
        {
            Position = new Vector2(470, 36), Size = new Vector2(258, 168),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _furnisherCost.AddThemeColorOverride("font_color", new Color("d5c79d"));
        menu.AddChild(_furnisherCost);

        _constructionFrameActions = new HBoxContainer
        {
            Position = new Vector2(580, 206), Size = new Vector2(148, 40),
            Alignment = BoxContainer.AlignmentMode.End
        };
        var frameActions = _constructionFrameActions;
        frameActions.AddThemeConstantOverride("separation", 8);
        menu.AddChild(frameActions);
        AddConstructionIconAction(frameActions, OriginalUiIcons.Medium(78), "Удалить весь чертёж", () =>
        {
            if (_roomPlanner.HasDraft)
            {
                _roomPlanner.CancelDraft();
                RefreshFurnisherControls();
                _status.Text = "Чертёж комнаты отменён";
            }
            else SelectConstructionTool(BuildTool.Cancel);
        });
        AddConstructionIconAction(frameActions, OriginalUiIcons.Medium(45), "Отменить последнее изменение", () =>
        {
            if (_roomPlanner.Undo())
            {
                RefreshFurnisherControls();
                _status.Text = "Последнее изменение чертежа отменено";
            }
        });
        AddConstructionIconAction(frameActions, OriginalUiIcons.Medium(26), "Подтвердить строительство", () =>
        {
            CommitRoomDraft();
            _constructionPalette.Visible = _roomPlanner.HasDraft;
        });
        RefreshFurnisherControls();
        return menu;
    }

    private static Label ConstructionSectionTitle(string text)
    {
        var label = new Label { Text = text, CustomMinimumSize = new Vector2(180, 24) };
        label.AddThemeColorOverride("font_color", new Color("d5c79d"));
        label.AddThemeFontSizeOverride("font_size", 14);
        return label;
    }

    private static Button AddConstructionIconAction(
        Control parent, Texture2D? icon, string tooltip, Action pressed, bool toggle = false)
    {
        var button = new Button
        {
            Icon = icon, TooltipText = tooltip, CustomMinimumSize = new Vector2(44, 44),
            FocusMode = Control.FocusModeEnum.None, ToggleMode = toggle
        };
        button.AddThemeStyleboxOverride("normal", HudButtonStyle(new Color("202120"), new Color("575950")));
        button.AddThemeStyleboxOverride("hover", HudButtonStyle(new Color("363732"), new Color("a39a73")));
        button.AddThemeStyleboxOverride("pressed", HudButtonStyle(new Color("41423b"), new Color("c6b879")));
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }

    private static void AddConstructionAction(
        Control parent, string text, Texture2D? icon, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            Icon = icon,
            Alignment = HorizontalAlignment.Left,
            IconAlignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(210, 28),
            FocusMode = Control.FocusModeEnum.None
        };
        button.AddThemeFontSizeOverride("font_size", 16);
        button.AddThemeColorOverride("font_color", new Color("d5c79d"));
        button.AddThemeStyleboxOverride("normal", HudButtonStyle(new Color("202120"), new Color("575950")));
        button.AddThemeStyleboxOverride("hover", HudButtonStyle(new Color("363732"), new Color("a39a73")));
        button.AddThemeStyleboxOverride("pressed", HudButtonStyle(new Color("41423b"), new Color("c6b879")));
        button.Pressed += pressed;
        parent.AddChild(button);
    }

    private static StyleBoxFlat HudButtonStyle(Color background, Color border) => new()
    {
        BgColor = background,
        BorderColor = border,
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2
    };

    private void SelectConstructionTool(BuildTool tool)
    {
        SelectTool(tool);
        _constructionPalette.Visible = true;
    }

    private void OpenRoomCategory(string main)
    {
        // Switching BuildMain branches deactivates the current Java placement tool.
        // Keeping the previous room draft active made a click on Housing reopen the
        // Woodcutter editor and kept its hundreds of preview cells alive.
        if (_roomPlanner.HasDraft) _roomPlanner.CancelDraft();
        _lastFurniturePreviewCell = null;
        _world.ClearRoomPreview();
        CloseWindows();
        _roomPalette.OpenCategory(main);
    }

    private void OpenConstructionMenu()
    {
        CloseWindows();
        _roomPalette.OpenConstruction();
    }

    private void OpenJobsMenu()
    {
        CloseWindows();
        _roomPalette.OpenJobs();
    }

    private void SelectBuildAction(string action)
    {
        _roomPalette.Visible = false;
        switch (action)
        {
            case "ROADS": SelectTool(BuildTool.Road); return;
            case "JOB_FORAGE": SelectTool(BuildTool.Forage); return;
            case "JOB_CLEAR_WOOD": SelectTool(BuildTool.ClearWood); return;
            case "JOB_CLEAR_STONE": SelectTool(BuildTool.ClearStone); return;
            case "JOB_CLEAR_ALL": SelectTool(BuildTool.ClearAll); return;
            case "JOB_CLEAR_WATER": SelectTool(BuildTool.ClearWater); return;
            case "JOB_CLEAR_MOUNTAIN": SelectTool(BuildTool.DigTunnel); return;
            case "MOVE_THRONE":
                _status.Text = "Перенос существующего трона ещё не подключён: обычное строительство трона не подставляется";
                return;
            case "FENCES":
                _status.Text = "Заборы требуют отдельного JobBuildFence; инструмент стены не подставляется";
                return;
            case "STRUCTURES":
                _status.Text = "Конструкции требуют JobBuildStructure с выбором материала, стен и крыш; каменная стена не подставляется";
                return;
            case "FORTIFICATION":
                _status.Text = "Укрепления требуют JobBuildFort и лестницы; инструмент стены не подставляется";
                return;
            case "JOB_HUNT":
                _status.Text = "Ручная охота ожидает перенос диких животных и huntMark; охотничий лагерь не подставляется";
                return;
        }
    }

    private void OpenSettlementMap()
    {
        CloseWindows();
        _globalMap.Visible = true;
        if (!_globalMap.Expanded) _globalMap.ToggleExpanded();
    }

    private void OpenAdministration(Action selectPage)
    {
        CloseWindows();
        selectPage();
        _administration.Visible = true;
    }

    private void OpenManager(Action openPage)
    {
        CloseWindows();
        openPage();
    }

    private void OpenCivic(Action openPage)
    {
        CloseWindows();
        openPage();
    }

    private void ToggleWindow(Control target)
    {
        var show = !target.Visible;
        CloseWindows();
        if (show && ReferenceEquals(target, _strategicMap))
            _strategicMap.Toggle();
        else
            target.Visible = show;
    }

    private void CloseWindows()
    {
        _roomPalette.Visible = false;
        _constructionPalette.Visible = false;
        _roomPolicy.Visible = false;
        _economyPanel.Visible = false;
        _inspector.Visible = false;
        _notifications.Visible = false;
        _referencePanel.Visible = false;
        _globalMap.Visible = false;
        _administration.Visible = false;
        _manager.Visible = false;
        _civic.Visible = false;
        _strategicMap.Visible = false;
        _battlePanel.Visible = false;
        _battleCommands.Visible = false;
    }

    private void UpdateBattlePause()
    {
        var blocked = _settlementWorld.Battles.Pending is not null;
        if (blocked && _battleResumeSpeed < 0)
        {
            _battleResumeSpeed = _clock.SpeedLevel;
            _clock.SetSpeedLevel(0);
        }
        else if (!blocked && _battleResumeSpeed >= 0)
        {
            _clock.SetSpeedLevel(_battleResumeSpeed);
            _battleResumeSpeed = -1;
        }
    }

    private static string ToolTitle(BuildTool tool) => tool switch
    {
        BuildTool.Wall => "Стены",
        BuildTool.Road => "Дороги",
        BuildTool.RoomArea => "Площадь комнаты",
        BuildTool.Door => "Проёмы",
        BuildTool.Furniture => "Мебель",
        BuildTool.RoomShrink => "Уменьшение формы комнаты",
        BuildTool.RoomInspect => "Выбор",
        BuildTool.Cancel => "Отмена",
        BuildTool.Forage => "Сбор съедобных растений",
        BuildTool.ClearWood => "Рубка деревьев",
        BuildTool.ClearStone => "Уборка камней",
        BuildTool.ClearAll => "Уборка деревьев и камней",
        BuildTool.ClearWater => "Осушение воды",
        BuildTool.DigTunnel => "Прокладка тоннеля",
        _ => tool.ToString()
    };

    private void FocusCamera(GridCoord cell)
    {
        var world = _world.CellToWorld(cell, _camera.Position.Y);
        _camera.Position = new Vector3(world.X, _camera.Position.Y, world.Z);
        SelectCell(cell);
    }

    private void SelectCell(GridCoord cell)
    {
        if (!_world.IsInside(cell)) return;
        _selectedCell = cell;
        _selectedRoom = _rooms.FindAt(cell);
        _globalMap.SetSelectedCell(cell);
        RefreshInspector();
    }

    private void SelectRoomDefinition(string definitionKey)
    {
        if (_roomPlanner.HasDraft) _roomPlanner.CancelDraft();
        _roomPlanner.SelectDefinition(definitionKey);
        _selectedFurnisherGroup = 0;
        _selectedFurnisherVariant = 0;
        _selectedFurnisherRotation = 0;
        _lastFurniturePreviewCell = null;
        _lastFurniturePreviewDefinition = "";
        RefreshFurnisherControls();
        SelectTool(_roomPlanner.UsesFixedItemPlacement ? BuildTool.Furniture : BuildTool.RoomArea);
        _roomPalette.Visible = false;
        _constructionPalette.Visible = true;
        if (_roomPlanner.UsesFixedItemPlacement)
            _status.Text = "Выберите тип и размер дома; ЛКМ размещает готовый чертёж, E/Q меняют размер, R поворачивает";
    }

    private void CommitRoomDraft()
    {
        var created = _roomPlanner.Commit(_jobs);
        var message = string.IsNullOrWhiteSpace(_roomPlanner.PlacementStatus)
            ? "Планировка не задана" : _roomPlanner.PlacementStatus;
        _roomPalette.SetStatus(message);
        _status.Text = created > 0 || message == "Комната запланирована"
            ? "Комната запланирована: жители доставят материалы и построят стены и обстановку"
            : message;
    }

    private void RefreshFurnisherControls()
    {
        if (_furnisherControls is null) return;
        foreach (var child in _furnisherControls.GetChildren())
        {
            _furnisherControls.RemoveChild(child);
            child.QueueFree();
        }
        if (!_roomPlanner.UsesDefinition)
        {
            _furnisherControls.AddChild(new Label { Text = "Выберите помещение в меню строительства." });
            if (_furnisherCost is not null) _furnisherCost.Text = "";
            return;
        }
        var blueprint = _rooms.Blueprints.Get(_roomPlanner.SelectedDefinition);
        if (blueprint is null) return;
        var fixedPlacement = _roomPlanner.UsesFixedItemPlacement;
        _constructionShapeControls.Visible = !fixedPlacement;
        _fixedStructureControls.Visible = fixedPlacement;
        _constructionShapeSeparator.Visible = true;
        _constructionStatsSeparator.Visible = !fixedPlacement;
        _constructionFrameActions.Visible = !fixedPlacement;
        _furnisherCost.Visible = !fixedPlacement;
        _constructionPalette.Size = new Vector2(fixedPlacement ? 458 : 736, 250);
        _constructionTitle.Size = new Vector2(fixedPlacement ? 442 : 720, 28);
        if (_bottomToolbar is not null) ApplyResponsiveLayout();
        _furnisherControls.Position = new Vector2(202, 36);
        _furnisherControls.Size = new Vector2(248, 168);
        _constructionTitle.Text = fixedPlacement
            ? $"СТРОИТЕЛЬСТВО {RussianRoomName(blueprint.Key, blueprint.Rule.Name)}"
            : $"{RussianRoomName(blueprint.Key, blueprint.Rule.Name).ToUpperInvariant()} — СТРОИТЕЛЬСТВО";
        _autoWallsButton.Disabled = !blueprint.Rule.Construction.Indoors;
        _autoWallsButton.ButtonPressed = _roomPlanner.AutoWalls;
        _fixedAutoWallsButton.ButtonPressed = _roomPlanner.AutoWalls;
        var structure = OriginalGameData.Current.Structure(_roomPlanner.StructureKey);
        _fixedStructureButton.TooltipText =
            $"Материал: {RussianStructureName(structure.Key)}; {structure.Resource} ×{structure.ResourceAmount}";
        _fixedStructureButton.Text = RussianStructureName(structure.Key);
        _furnisherControls.AddChild(ConstructionSectionTitle("ОБЪЕКТЫ"));
        var names = FurnisherItemNames(blueprint.Key, blueprint.Rule.FurnisherItems.Count);
        if (blueprint.Rule.FurnisherItems.Count > 0)
            _selectedFurnisherGroup = Math.Clamp(_selectedFurnisherGroup, 0, blueprint.Rule.FurnisherItems.Count - 1);
        var variants = FurnisherLayoutCatalog.Variants(blueprint.Key, _selectedFurnisherGroup);
        _selectedFurnisherVariant = Math.Clamp(_selectedFurnisherVariant, 0, variants.Count - 1);
        var variationBar = new HBoxContainer();
        var smaller = new Button
        {
            Icon = OriginalUiIcons.Medium(43), TooltipText = "Предыдущий размер",
            CustomMinimumSize = new Vector2(38, 34)
        };
        var size = new Label
        {
            Text = $"Размер {_selectedFurnisherVariant + 1}/{variants.Count}",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(130, 30)
        };
        var larger = new Button
        {
            Icon = OriginalUiIcons.Medium(42), TooltipText = "Следующий размер",
            CustomMinimumSize = new Vector2(38, 34)
        };
        var rotate = new Button
        {
            Icon = OriginalUiIcons.Medium(44), TooltipText = "Повернуть объект",
            CustomMinimumSize = new Vector2(38, 34)
        };
        smaller.Pressed += () => ChangeFurnisherVariant(-1);
        larger.Pressed += () => ChangeFurnisherVariant(1);
        rotate.Pressed += RotateFurnisher;
        variationBar.AddChild(smaller);
        variationBar.AddChild(size);
        variationBar.AddChild(larger);
        variationBar.AddChild(rotate);
        _furnisherControls.AddChild(variationBar);
        for (var group = 0; group < blueprint.Rule.FurnisherItems.Count; group++)
        {
            var groupIndex = group;
            var select = new Button
            {
                Text = fixedPlacement ? names[group] :
                    $"{names[group]} ×{_roomPlanner.DefinitionItems.GetValueOrDefault(group):0.##}",
                CustomMinimumSize = new Vector2(250, 34),
                Alignment = HorizontalAlignment.Left,
                ButtonPressed = group == _selectedFurnisherGroup,
                ToggleMode = true
            };
            select.Pressed += () =>
            {
                _selectedFurnisherGroup = groupIndex;
                _selectedFurnisherVariant = 0;
                _lastFurniturePreviewCell = null;
                SelectTool(BuildTool.Furniture);
                RefreshFurnisherControls();
            };
            _furnisherControls.AddChild(select);
        }
        if (blueprint.Rule.FurnisherItems.Count == 0)
            _furnisherControls.AddChild(new Label { Text = "Обстановка для этого помещения не требуется." });
        RefreshFurnisherSummary();
    }

    private void OpenFixedStructureMenu()
    {
        _fixedStructurePopup.PopupCentered(new Vector2I(270, 260));
    }

    private static string RussianStructureName(string key) => key.ToUpperInvariant() switch
    {
        "WOOD" => "Дерево",
        "STONE" => "Камень",
        "GRAND" => "Монументальный камень",
        "MUD" => "Глина",
        _ => key
    };

    private void RefreshFurnisherSummary()
    {
        if (_furnisherCost is null || !_roomPlanner.UsesDefinition) return;
        var items = _roomPlanner.DefinitionItemCount;
        var cost = _roomPlanner.DefinitionCost();
        var stats = _roomPlanner.DefinitionStats();
        var blueprint = _rooms.Blueprints.Get(_roomPlanner.SelectedDefinition);
        var statLines = stats.Select((value, index) =>
        {
            var sourceName = blueprint is not null && index < blueprint.Rule.FurnisherStats.Count
                ? blueprint.Rule.FurnisherStats[index].Name : $"Показатель {index + 1}";
            return $"{RussianStatName(_roomPlanner.SelectedDefinition, index, sourceName)}: " +
                   FormatFurnisherStat(sourceName, value);
        });
        var variants = FurnisherLayoutCatalog.Variants(
            _roomPlanner.SelectedDefinition, _selectedFurnisherGroup);
        var selected = variants[Math.Clamp(_selectedFurnisherVariant, 0, variants.Count - 1)];
        var projectedStats = blueprint is null || (uint)_selectedFurnisherGroup >=
            (uint)blueprint.Rule.FurnisherItems.Count ? stats :
            _roomPlanner.DefinitionStatsWithAdditionalItem(
                _selectedFurnisherGroup, selected.StatMultiplier);
        var itemLines = Enumerable.Range(0, Math.Max(stats.Length, projectedStats.Length))
            .Select(index => (index,
                before: index < stats.Length ? stats[index] : 0,
                after: index < projectedStats.Length ? projectedStats[index] : 0))
            .Where(entry => Math.Abs(entry.after - entry.before) > 0.000001)
            .Select(entry =>
        {
            var sourceName = entry.index < blueprint!.Rule.FurnisherStats.Count
                ? blueprint.Rule.FurnisherStats[entry.index].Name : $"Показатель {entry.index + 1}";
            return $"{RussianStatName(_roomPlanner.SelectedDefinition, entry.index, sourceName)}: " +
                   $"{FormatFurnisherDelta(sourceName, entry.after - entry.before)} → " +
                   FormatFurnisherStat(sourceName, entry.after);
        }).ToArray();
        _furnisherCost.Text = $"ПОКАЗАТЕЛИ\nРазмещено предметов: {items}\n" +
            (stats.Length == 0 ? "" : string.Join("\n", statLines) + "\n") +
            (itemLines.Length == 0 ? "" : "Если разместить выбранный объект:\n" +
             string.Join("\n", itemLines) + "\n") +
            (cost.Count == 0 ? "Материалы не требуются" : "Материалы: " +
             string.Join(", ", cost.Select(pair => $"{RussianResourceName(pair.Key)} ×{pair.Value}"))) +
            (string.IsNullOrWhiteSpace(_roomPlanner.PlacementStatus)
                ? "" : $"\n{_roomPlanner.PlacementStatus}");
    }

    private static string RussianStatName(string roomKey, int index, string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return roomKey.Equals("FISHERY_NORMAL", StringComparison.OrdinalIgnoreCase) && index == 3
                ? "Глубоководный доступ" : $"Показатель {index + 1}";
        return source.Trim().ToUpperInvariant() switch
        {
            "EFFICIENCY" => "Эффективность",
            "EMPLOYEES" or "WORKERS" or "HUNTERS" or "FISHERMEN" => "Рабочие места",
            "OUTPUT" or "PRODUCTION" => "Производство",
            "SERVICES" or "SERVICE" => "Обслуживание",
            "STORAGE" => "Хранилище",
            "CAPACITY" => "Вместимость",
            "TABLES" => "Столы",
            "COZINESS" => "Уют",
            "MOISTURE" => "Влажность",
            "DEEP SEA" or "DEEP SEA ACCESS" => "Глубоководный доступ",
            _ => source
        };
    }

    private static string RussianRoomName(string key, string fallback) => key.ToUpperInvariant() switch
    {
        "_HOME" => "Дом",
        "_HOME_CHAMBER" => "Покои знати",
        "HUNTER_NORMAL" => "Охотничий лагерь",
        "FISHERY_NORMAL" => "Рыболовня",
        _ => fallback
    };

    private static string FormatFurnisherStat(string name, double value) =>
        name.Contains("Efficiency", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Эффектив", StringComparison.OrdinalIgnoreCase)
            ? $"{value * 100:0.#}%" : value.ToString("0.##");

    private static string FormatFurnisherDelta(string name, double value)
    {
        var sign = value >= 0 ? "+" : "";
        return name.Contains("Efficiency", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Эффектив", StringComparison.OrdinalIgnoreCase)
            ? $"{sign}{value * 100:0.#} п.п."
            : $"{sign}{value:0.##}";
    }

    private static string RussianResourceName(ResourceKind resource) => resource switch
    {
        ResourceKind.Wood => "Дерево",
        ResourceKind.Stone => "Камень",
        ResourceKind.Furniture => "Мебель",
        ResourceKind.Fabric => "Ткань",
        ResourceKind.Tools => "Инструменты",
        ResourceKind.Food => "Еда",
        ResourceKind.Fish => "Рыба",
        ResourceKind.Meat => "Мясо",
        ResourceKind.Leather => "Кожа",
        _ => resource.ToString()
    };

    private static IReadOnlyList<string> FurnisherItemNames(string roomKey, int count)
    {
        try
        {
            var source = FileAccess.GetFileAsString($"res://Data/Original/text/room/{roomKey}.txt");
            var items = SyxDataParser.Parse(source).Get("ITEMS")?.Items;
            if (items is not null)
                return Enumerable.Range(0, count).Select(index => RussianItemName(roomKey, index,
                    index < items.Count
                        ? items[index].Get("NAME")?.Text($"Предмет {index + 1}") ?? $"Предмет {index + 1}"
                        : $"Предмет {index + 1}")).ToArray();
        }
        catch (Exception)
        {
            // A missing localization entry must not hide the source data group.
        }
        return Enumerable.Range(1, count).Select(index => $"Предмет {index}").ToArray();
    }

    private static string RussianItemName(string roomKey, int index, string fallback) =>
        (roomKey.ToUpperInvariant(), index) switch
        {
            ("_HOME", 0) => "Квартира",
            ("_HOME", 1) => "Дом",
            ("_HOME", 2) => "Длинный дом",
            ("HUNTER_NORMAL", 0) => "Разделочный стол",
            ("HUNTER_NORMAL", 1) => "Оснащение",
            ("FISHERY_NORMAL", 0) => "Хранилище",
            ("FISHERY_NORMAL", 1) => "Вспомогательное оборудование",
            _ => fallback
        };

    private void RefreshInspector()
    {
        if (_selectedCell is not { } cell) return;
        _selectedRoom = _rooms.FindAt(cell);
        _inspector.ShowSelection(cell, _world.Data, _selectedRoom, _rooms,
            _citizens.InspectAt(cell, 2));
    }

    private void SelectTool(BuildTool tool)
    {
        _tool = tool;
        if (_toolLabel is not null) _toolLabel.Text = $"Инструмент: {tool}";
    }

    private void CycleRoomType()
    {
        _roomPlanner.SelectLegacy((RoomType)(((int)_roomPlanner.Type + 1) % 3));
    }

    private void UpdateEconomyInfo()
    {
        var latest = _economy.Latest;
        var produced = latest?.Produced ?? new int[ResourceLedger.KindCount];
        var consumed = latest?.Consumed ?? new int[ResourceLedger.KindCount];
        _economyInfo.Text = "Экономика — остаток | +производство / -расход за 60 сек\n" +
            $"Wood: {_resources.Get(ResourceKind.Wood)} | +{produced[(int)ResourceKind.Wood]} / -{consumed[(int)ResourceKind.Wood]}\n" +
            $"Stone: {_resources.Get(ResourceKind.Stone)} | +{produced[(int)ResourceKind.Stone]} / -{consumed[(int)ResourceKind.Stone]}\n" +
            $"Grain: {_resources.Get(ResourceKind.Grain)} | +{produced[(int)ResourceKind.Grain]} / -{consumed[(int)ResourceKind.Grain]}\n" +
            $"Food: {_resources.Get(ResourceKind.Food)} | +{produced[(int)ResourceKind.Food]} / -{consumed[(int)ResourceKind.Food]}\n" +
            $"Tools: {_resources.Get(ResourceKind.Tools)} | +{produced[(int)ResourceKind.Tools]} / -{consumed[(int)ResourceKind.Tools]}\n" +
            $"Furniture: {_resources.Get(ResourceKind.Furniture)} | +{produced[(int)ResourceKind.Furniture]} / -{consumed[(int)ResourceKind.Furniture]}\n" +
            $"История: {_economy.History.Count}/{EconomyTracker.MaxSamples} минут";
    }

    private void Save() => SaveGameService.Save(CaptureSnapshot());
    public void SaveNow() => Save();

    private SaveSnapshot CaptureSnapshot()
    {
        var constructionTransit = _citizens.CaptureConstructionTransit();
        return new SaveSnapshot
        {
            WorldSeed = GameSession.Current?.WorldSeed ?? GameSession.DefaultSeed,
            SelectedRegionId = GameSession.Current?.SelectedRegionId ?? -1,
            WorldCapitalX = GameSession.Current?.WorldCapitalX ?? -1,
            WorldCapitalY = GameSession.Current?.WorldCapitalY ?? -1,
            PlayerProfile = GameSession.Current?.Profile,
            LandingOrigin = _landingOrigin is { } landingOrigin
                ? _world.CellToIndex(landingOrigin) : -1,
            PlayerRace = GameSession.Current?.PlayerRace ?? "HUMAN",
            SettlementWorld = _settlementWorld.Capture(),
            Technologies = _rooms.Technologies.Capture(),
            Law = _rooms.Law.Capture(),
            Governance = _rooms.Governance.Capture(),
            StrategicWorld = _strategicWorld.Capture(),
            RegionalEconomy = _regionalEconomy.Capture(),
            SpecialProduction = _rooms.SpecialProduction.Capture(),
            LogisticsPolicies = _rooms.Logistics.CapturePolicies(),
            Invasion = _invasion.Capture(),
            MapWidth = GridWorld.Width,
            MapHeight = GridWorld.Height,
            MutableTerrain = _world.Data.CaptureMutableState(),
            Tick = _clock.Tick,
            PlayedSeconds = _clock.PlayedSeconds,
            Walls = _world.WallCells.ToArray(),
            Roads = _world.RoadCells.ToArray(),
            RoadTypes = _world.RoadCells
                .Select(index => _world.RoadKeys.GetValueOrDefault(index, "DIRT")).ToArray(),
            RoomFloors = _world.RoomFloorCells.ToArray(),
            RoomFloorTypes = _world.RoomFloorCells
                .Select(index => _world.RoomFloorKeys.GetValueOrDefault(index, "DIRT")).ToArray(),
            Zones = _world.ZoneCells.ToArray(),
            Doors = _world.DoorCells.ToArray(),
            Furniture = _world.FurnitureCells.ToArray(),
            Jobs = _jobs.All.Select(job => new SavedJob
            {
                Cell = _world.CellToIndex(job.Cell),
                Kind = (byte)job.Kind,
                WorkLeft = job.WorkLeft,
                ResourceReserved = job.ResourceReserved,
                Resource = (byte)job.Resource,
                ResourceAmount = job.ResourceAmount,
                OutputResource = (byte)job.OutputResource,
                OutputAmount = job.OutputAmount,
                OutputAlreadyAccounted = job.OutputAlreadyAccounted,
                RoomId = job.RoomId,
                DestinationRoomId = job.DestinationRoomId,
                Destination = _world.CellToIndex(job.Destination),
                CorpseId = job.CorpseId,
                FacilitySlotId = job.FacilitySlotId,
                LawProcessType = (byte)job.LawProcessType,
                PickedUp = job.PickedUp,
                RequiredProfession = (byte)job.RequiredProfession,
                Priority = (byte)job.Priority,
                ReservedAmount = job.ReservedAmount,
                DeliveredAmount = job.DeliveredAmount,
                BuildPhase = (byte)job.Phase,
                State = (byte)job.State,
                FurnitureCells = job.FurnitureCells.Select(_world.CellToIndex).ToArray(),
                FurnitureBlockerCells = job.FurnitureBlockerCells.Select(_world.CellToIndex).ToArray(),
                FurnitureReachableCells = job.FurnitureReachableCells.Select(_world.CellToIndex).ToArray(),
                FurnitureWorkCells = job.FurnitureWorkCells.Select(_world.CellToIndex).ToArray(),
                FurnitureStorageCells = job.FurnitureStorageCells.Select(_world.CellToIndex).ToArray(),
                RoadKey = job.RoadKey,
                FloorKey = job.FloorKey,
                InputResources = job.ProductionInputs.Keys.Select(kind => (byte)kind).ToArray(),
                InputAmounts = job.ProductionInputs.Values.ToArray(),
                ConstructionTransitAmount = constructionTransit.GetValueOrDefault(job),
                ConstructionCell = _world.CellToIndex(job.ConstructionCell)
            }).ToArray(),
            Wood = _resources.Get(ResourceKind.Wood),
            Stone = _resources.Get(ResourceKind.Stone),
            Grain = _resources.Get(ResourceKind.Grain),
            Food = _resources.Get(ResourceKind.Food),
            Tools = _resources.Get(ResourceKind.Tools),
            FurnitureStock = _resources.Get(ResourceKind.Furniture),
            ResourceKinds = System.Enum.GetValues<ResourceKind>().Select(kind => (byte)kind).ToArray(),
            ResourceAmounts = _resources.CaptureAmounts(),
            Rooms = _rooms.All.Select(room =>
            {
                _rooms.Construction.Orders.TryGetValue(room.Id, out var construction);
                var constructionResources = construction?.Required.Keys.ToArray() ?? Array.Empty<ResourceKind>();
                var constructionRequired = constructionResources
                    .Select(kind => construction!.Required[kind]).ToArray();
                var constructionDelivered = constructionResources
                    .Select(kind => construction!.Delivered.GetValueOrDefault(kind)).ToArray();
                return new SavedRoom
                {
                    Id = room.Id,
                    Type = (byte)room.Type,
                    DefinitionKey = room.DefinitionKey,
                    Cells = room.Cells.ToArray(),
                    RequiredWalls = room.RequiredWalls.ToArray(),
                    RequiredDoors = room.RequiredDoors.ToArray(),
                    WorkerLimit = room.WorkerLimit,
                    RecipeIndex = room.RecipeIndex,
                    RequiredFurniture = room.RequiredFurniture,
                    UpgradeLevel = room.UpgradeLevel,
                    Isolation = room.Isolation,
                    Degradation = room.Degradation,
                    MaintenanceDebt = room.MaintenanceDebt,
                    ToolTargetPerWorker = room.ToolTargetPerWorker,
                    ToolUnits = room.ToolUnits,
                    ToolWearProgress = room.ToolWearProgress,
                    ItemGroupKeys = room.ItemGroupAmounts.Keys.ToArray(),
                    ItemGroupAmounts = room.ItemGroupAmounts.Values.ToArray(),
                    MaintenanceResources = room.MaintenanceResourceAmounts.Keys
                        .Select(kind => (byte)kind).ToArray(),
                    MaintenanceResourceAmounts = room.MaintenanceResourceAmounts.Values.ToArray(),
                    FurnitureRepairCells = room.FurnitureRepairAmounts.Keys
                        .Select(key => key.Cell).ToArray(),
                    FurnitureRepairResources = room.FurnitureRepairAmounts.Keys
                        .Select(key => (byte)key.Resource).ToArray(),
                    FurnitureRepairAmounts = room.FurnitureRepairAmounts.Values.ToArray(),
                    FurnitureFootprints = room.FurnitureFootprints.Select(item =>
                        new SavedFurnitureFootprint
                        {
                            Anchor = item.Anchor,
                            Group = item.Group,
                            Variant = item.Variant,
                            Rotation = item.Rotation,
                            Cells = item.Cells.ToArray(),
                            Broken = item.Broken
                        }).ToArray(),
                    ProductionInputRecipes = room.IndustryInputProgress.Keys
                        .Select(key => key.Recipe).ToArray(),
                    ProductionInputResources = room.IndustryInputProgress.Keys
                        .Select(key => (byte)key.Resource).ToArray(),
                    ProductionInputProgress = room.IndustryInputProgress.Values.ToArray(),
                    ProductionOutputRecipes = room.IndustryOutputProgress.Keys.ToArray(),
                    ProductionOutputProgressByRecipe = room.IndustryOutputProgress.Values.ToArray(),
                    AdditionalOutputRecipes = room.AdditionalOutputProgress.Keys
                        .Select(key => key.Recipe).ToArray(),
                    AdditionalOutputResources = room.AdditionalOutputProgress.Keys
                        .Select(key => (byte)key.Resource).ToArray(),
                    AdditionalOutputProgress = room.AdditionalOutputProgress.Values.ToArray(),
                    InputStorageCells = room.InputStorage.Keys.Select(key => key.Cell).ToArray(),
                    InputStorageResources = room.InputStorage.Keys
                        .Select(key => (byte)key.Resource).ToArray(),
                    InputStorageAmounts = room.InputStorage.Values.ToArray(),
                    ConstructionResources = constructionResources.Select(kind => (byte)kind).ToArray(),
                    ConstructionRequired = constructionRequired,
                    ConstructionDelivered = constructionDelivered
                };
            }).ToArray(),
            InternalStorage = _rooms.InternalStorage.AllSlots.Select(item => new SavedRoomStorage
            {
                RoomId = item.RoomId,
                Cell = _world.CellToIndex(item.Slot.Cell),
                Resource = item.Slot.Resource is { } resource ? (int)resource : -1,
                Amount = item.Slot.Amount
            }).ToArray(),
            Citizens = _citizens.CaptureState().Select(citizen => new SavedCitizen
            {
                Id = citizen.Id,
                Cell = citizen.Cell,
                Hunger = citizen.Hunger,
                Profession = (byte)citizen.Profession,
                Alive = citizen.Alive,
                FoodPlan = citizen.FoodPlan,
                EatTimeLeft = citizen.EatTimeLeft,
                FoodSource = citizen.FoodSource,
                FoodFromLoose = citizen.FoodFromLoose,
                Race = citizen.Identity.Race,
                Class = (byte)citizen.Identity.Class,
                Type = (byte)citizen.Identity.Type,
                FirstName = citizen.Identity.FirstName,
                Surname = citizen.Identity.Surname,
                Gender = citizen.Identity.Gender,
                Origin = (byte)citizen.Identity.Origin,
                ParentId = citizen.Identity.ParentId,
                BirthDay = citizen.Identity.BirthDay,
                AgeDays = citizen.AgeDays,
                BabyDays = citizen.BabyDays,
                Religion = citizen.Religion,
                HomeRoomId = citizen.HomeRoomId,
                NeedTimeLeft = citizen.NeedTimeLeft,
                Needs = new Dictionary<string, int>(citizen.Needs),
                Health = citizen.Health
            }).ToArray(),
            LooseResources = _hauling.Capture().Select(item => new SavedLooseResource
            {
                Cell = _world.CellToIndex(item.Cell),
                Resource = (byte)item.Kind,
                Amount = item.Amount,
                AlreadyAccounted = item.AlreadyAccounted
            }).ToArray(),
            BakerLimit = _citizens.BakerLimit,
            CarpenterLimit = _citizens.CarpenterLimit,
            TotalProduced = _resources.CaptureTotalProduced(),
            TotalConsumed = _resources.CaptureTotalConsumed(),
            EconomyHistory = _economy.Capture().Select(sample => new SavedEconomySample
            {
                Time = sample.Time,
                Produced = sample.Produced,
                Consumed = sample.Consumed
            }).ToArray()
        };
    }

    private void TryLoad(bool userRequested)
    {
        var save = SaveGameService.Load();
        if (save is null) return;
        if (save.Version is < 1 or > 37 || save.MapWidth != GridWorld.Width || save.MapHeight != GridWorld.Height)
        {
            if (userRequested) GD.PushWarning("Формат сохранения или размер карты не совпадает.");
            return;
        }
        if (save.Version >= 19 && save.MutableTerrain is not null &&
            !_world.Data.RestoreMutableState(save.MutableTerrain))
        {
            if (userRequested) GD.PushWarning("Размер данных местности в сохранении не совпадает с картой.");
            return;
        }
        foreach (var index in save.Walls) _world.BuildWall(_world.FromIndex(index));
        for (var roadIndex = 0; roadIndex < save.Roads.Length; roadIndex++)
        {
            var key = save.Version >= 13 && roadIndex < save.RoadTypes.Length
                ? save.RoadTypes[roadIndex]
                : OriginalGameData.Current.DefaultRoad().Key;
            _world.BuildRoad(
                _world.FromIndex(save.Roads[roadIndex]), OriginalGameData.Current.Floor(key));
        }
        if (save.Version >= 31)
            for (var floorIndex = 0; floorIndex < save.RoomFloors.Length; floorIndex++)
            {
                var key = floorIndex < save.RoomFloorTypes.Length
                    ? save.RoomFloorTypes[floorIndex]
                    : "DIRT";
                _world.BuildRoomFloor(
                    _world.FromIndex(save.RoomFloors[floorIndex]), OriginalGameData.Current.Floor(key));
            }
        foreach (var index in save.Zones) _world.SetZone(_world.FromIndex(index));
        foreach (var index in save.Doors) _world.SetDoor(_world.FromIndex(index));
        foreach (var index in save.Furniture)
        {
            var furnitureCell = _world.FromIndex(index);
            _world.BuildFurniture(
                furnitureCell,
                _world.Data.FurnitureBlocks(furnitureCell),
                _world.Data.FurnitureMustBeReachable(furnitureCell),
                _world.Data.FurnitureWorkstation(furnitureCell),
                _world.Data.FurnitureStorage(furnitureCell));
        }
        foreach (var room in save.Rooms)
        {
            var inputProgress = new Dictionary<(int Recipe, ResourceKind Resource), double>();
            var inputCount = System.Math.Min(room.ProductionInputResources.Length, room.ProductionInputProgress.Length);
            for (var index = 0; index < inputCount; index++)
            {
                var recipeIndex = save.Version >= 11 && index < room.ProductionInputRecipes.Length
                    ? room.ProductionInputRecipes[index]
                    : 0;
                inputProgress[(recipeIndex, (ResourceKind)room.ProductionInputResources[index])] =
                    room.ProductionInputProgress[index];
            }
            var outputProgress = new Dictionary<int, double>();
            if (save.Version >= 11)
            {
                var outputCount = System.Math.Min(
                    room.ProductionOutputRecipes.Length,
                    room.ProductionOutputProgressByRecipe.Length);
                for (var index = 0; index < outputCount; index++)
                    outputProgress[room.ProductionOutputRecipes[index]] =
                        room.ProductionOutputProgressByRecipe[index];
            }
            else if (save.Version >= 10)
            {
                outputProgress[0] = room.ProductionOutputProgress;
            }
            var inputStorage = new Dictionary<(int Cell, ResourceKind Resource), int>();
            if (save.Version >= 12)
            {
                var storageCount = System.Math.Min(
                    room.InputStorageCells.Length,
                    System.Math.Min(room.InputStorageResources.Length, room.InputStorageAmounts.Length));
                for (var index = 0; index < storageCount; index++)
                    inputStorage[(room.InputStorageCells[index],
                        (ResourceKind)room.InputStorageResources[index])] = room.InputStorageAmounts[index];
            }
            var additionalOutputProgress = new Dictionary<(int Recipe, ResourceKind Resource), double>();
            var additionalCount = System.Math.Min(room.AdditionalOutputRecipes.Length,
                System.Math.Min(room.AdditionalOutputResources.Length, room.AdditionalOutputProgress.Length));
            for (var index = 0; index < additionalCount; index++)
                additionalOutputProgress[(room.AdditionalOutputRecipes[index],
                    (ResourceKind)room.AdditionalOutputResources[index])] = room.AdditionalOutputProgress[index];
            var restoredRoom = _rooms.Restore(
                (RoomType)room.Type,
                room.Cells,
                room.RequiredWalls,
                save.Version >= 4 ? room.WorkerLimit : -1,
                save.Version >= 11 ? room.RecipeIndex : 0,
                inputProgress,
                outputProgress,
                inputStorage,
                additionalOutputProgress,
                save.Version >= 14 ? room.Id : 0,
                save.Version >= 14 ? room.DefinitionKey : "",
                save.Version >= 14 ? room.RequiredFurniture : -1,
                save.Version >= 14 ? room.UpgradeLevel : 0,
                save.Version >= 14 ? room.Isolation : 1,
                save.Version >= 14 ? room.Degradation : 0,
                save.Version >= 14 ? room.MaintenanceDebt : 0,
                save.Version >= 14 ? room.ToolTargetPerWorker : 0,
                save.Version >= 14 ? room.ToolUnits : 0,
                save.Version >= 14 ? room.ToolWearProgress : 0,
                save.Version >= 14
                    ? room.ItemGroupKeys.Zip(room.ItemGroupAmounts).ToDictionary(pair => pair.First, pair => pair.Second)
                    : null,
                room.MaintenanceResources.Zip(room.MaintenanceResourceAmounts)
                    .ToDictionary(pair => (ResourceKind)pair.First, pair => pair.Second),
                room.FurnitureRepairCells.Zip(room.FurnitureRepairResources, room.FurnitureRepairAmounts)
                    .ToDictionary(pair => (pair.First, (ResourceKind)pair.Second), pair => pair.Third),
                room.FurnitureFootprints.Select(item => new RoomFurnitureFootprint
                {
                    Anchor = item.Anchor,
                    Group = item.Group,
                    Variant = item.Variant,
                    Rotation = item.Rotation,
                    Cells = item.Cells.ToHashSet(),
                    Broken = item.Broken
                }).ToArray(),
                save.Version >= 34 ? room.RequiredDoors : null);
            if (save.Version < 31)
            {
                var legacyFloor = _rooms.Blueprints.Get(restoredRoom.DefinitionKey)?
                    .Furnisher.Floor(restoredRoom.UpgradeLevel);
                if (legacyFloor is not null)
                    foreach (var index in restoredRoom.Cells)
                        _world.BuildRoomFloor(_world.FromIndex(index), OriginalGameData.Current.Floor(legacyFloor));
            }
            if (save.Version < 32 &&
                OriginalGameData.Current.Room(restoredRoom.DefinitionKey)?.Construction.Indoors == true)
                foreach (var cell in restoredRoom.Cells.Select(_world.FromIndex)
                             .Where(cell => !_world.Data.Has(cell, TileFlags.Cave)))
                    _world.Data.SetRoof(cell, true);
            if (save.Version >= 16 && room.ConstructionResources.Length > 0)
            {
                var constructionCount = System.Math.Min(
                    room.ConstructionResources.Length, room.ConstructionRequired.Length);
                var deliveredCount = System.Math.Min(constructionCount, room.ConstructionDelivered.Length);
                var required = new Dictionary<ResourceKind, int>();
                var delivered = new Dictionary<ResourceKind, int>();
                for (var index = 0; index < constructionCount; index++)
                    required[(ResourceKind)room.ConstructionResources[index]] =
                        room.ConstructionRequired[index];
                for (var index = 0; index < deliveredCount; index++)
                    delivered[(ResourceKind)room.ConstructionResources[index]] =
                        room.ConstructionDelivered[index];
                var instance = _rooms.RuntimeInstance(restoredRoom.Id);
                if (instance is not null)
                    _rooms.Construction.Restore(instance, required, delivered);
            }
        }
        _rooms.UpdateStates();
        _rooms.Logistics.RestorePolicies(save.LogisticsPolicies);
        if (save.Version >= 19 && save.SpecialProduction is not null)
            _rooms.SpecialProduction.Restore(save.SpecialProduction);
        if (save.Version >= 18 && save.LandingOrigin >= 0)
        {
            _landingOrigin = _world.FromIndex(save.LandingOrigin);
            RestoreLandingSupplyLocations(_landingOrigin.Value);
        }
        if (save.Version >= 17)
            foreach (var stored in save.InternalStorage)
                _rooms.InternalStorage.At(stored.RoomId, _world.FromIndex(stored.Cell))?
                    .RestoreContents(
                        stored.Resource >= 0 ? (ResourceKind)stored.Resource : null,
                        stored.Amount);
        _citizens.RestoreState(save.Citizens.Select((citizen, index) =>
            new CitizenState(
                save.Version >= 14 && citizen.Id > 0 ? citizen.Id : index + 1,
                citizen.Cell,
                save.Version < 6
                    ? Mathf.Clamp(citizen.Hunger * 0.64f, 0f, 64f)
                    : citizen.Hunger,
                (WorkProfession)citizen.Profession,
                save.Version < 6 || citizen.Alive,
                save.Version >= 6 ? citizen.FoodPlan : (byte)0,
                save.Version >= 6 ? citizen.EatTimeLeft : 0f,
                save.Version >= 6 ? citizen.FoodSource : 0,
                save.Version >= 6 && citizen.FoodFromLoose,
                new CitizenIdentity(
                    save.Version >= 14 && citizen.Id > 0 ? citizen.Id : index + 1,
                    save.Version >= 14 ? citizen.Race : GameSession.Current?.PlayerRace ?? "HUMAN",
                    save.Version >= 14 ? (SocialClass)citizen.Class : SocialClass.Citizen,
                    save.Version >= 14 ? (HumanoidType)citizen.Type : HumanoidType.Subject,
                    save.Version >= 14 ? citizen.FirstName : $"Citizen {index + 1}",
                    save.Version >= 14 ? citizen.Surname : "",
                    save.Version >= 14 ? citizen.Gender : (byte)0,
                    save.Version >= 14 ? (CitizenOrigin)citizen.Origin : CitizenOrigin.Immigrant,
                    save.Version >= 14 ? citizen.ParentId : 0,
                    save.Version >= 14 ? citizen.BirthDay : 0),
                save.Version >= 14 ? citizen.AgeDays : 0,
                save.Version >= 14 ? citizen.BabyDays : 0,
                save.Version >= 14 ? citizen.Religion : "",
                save.Version >= 14 ? citizen.HomeRoomId : 0,
                save.Version >= 14 ? citizen.NeedTimeLeft : 0,
                save.Version >= 14 ? citizen.Needs : new Dictionary<string, int>(),
                save.Version >= 14 && citizen.Health is not null
                    ? citizen.Health
                    : new CitizenHealthSnapshot("", DiseaseState.None, 0, false, 0))).ToArray());
        if (save.Version >= 3)
            _citizens.RestoreProfessionLimits(save.BakerLimit, save.CarpenterLimit);
        _hauling.Restore(save.LooseResources.Select(item => new LooseResource(
            _world.FromIndex(item.Cell), (ResourceKind)item.Resource, item.Amount,
            item.AlreadyAccounted)));
        foreach (var savedJob in save.Jobs)
        {
            var cell = _world.FromIndex(savedJob.Cell);
            var kind = (BuildKind)savedJob.Kind;
            switch (kind)
            {
                case BuildKind.Wall: _world.ReserveWall(cell); break;
                case BuildKind.RoomDoor: _world.ReserveDoor(cell); break;
                case BuildKind.Road: _world.ReserveRoad(cell); break;
                case BuildKind.Furniture: _world.ReserveFurniture(cell); break;
            }
            var job = new BuildJob(cell, kind)
            {
                WorkLeft = savedJob.WorkLeft
            };
            if (kind == BuildKind.Furniture)
            {
                job.RestoreFurnitureCells(
                    save.Version >= 28 && savedJob.FurnitureCells.Length > 0
                        ? savedJob.FurnitureCells.Select(_world.FromIndex)
                        : new[] { cell },
                    save.Version >= 29
                        ? savedJob.FurnitureBlockerCells.Select(_world.FromIndex)
                        : null,
                    save.Version >= 29
                        ? savedJob.FurnitureReachableCells.Select(_world.FromIndex)
                        : null,
                    savedJob.FurnitureWorkCells.Select(_world.FromIndex),
                    savedJob.FurnitureStorageCells.Select(_world.FromIndex));
                foreach (var furnitureCell in job.FurnitureCells)
                    _world.ReserveFurniture(furnitureCell);
            }
            if (kind == BuildKind.Road)
                job.RestoreRoad(save.Version >= 13 ? savedJob.RoadKey :
                    OriginalGameData.Current.DefaultRoad().Key);
            if (kind == BuildKind.RoomFloor)
                job.RestoreFloor(save.Version >= 31 ? savedJob.FloorKey : "DIRT");
            if (job.IsConstruction)
            {
                var reserved = save.Version >= 8
                    ? savedJob.ReservedAmount
                    : savedJob.ResourceReserved ? savedJob.ResourceAmount : 0;
                job.RestoreConstructionData(
                    save.Version >= 8 ? savedJob.DeliveredAmount : 0,
                    reserved,
                    save.Version >= 17 && savedJob.ConstructionTransitAmount > 0
                        ? BuildPhase.FetchingMaterials
                        : save.Version >= 8
                            ? (BuildPhase)savedJob.BuildPhase
                            : BuildPhase.FetchingMaterials,
                    save.Version >= 30 ? _world.FromIndex(savedJob.ConstructionCell) : cell);
            }
            else
            {
                job.ResourceReserved = savedJob.ResourceReserved;
            }
            if (save.Version >= 3) job.Priority = (JobPriority)savedJob.Priority;
            if (kind == BuildKind.Production)
            {
                var inputs = save.Version >= 9
                    ? savedJob.InputResources.Zip(savedJob.InputAmounts)
                        .ToDictionary(pair => (ResourceKind)pair.First, pair => pair.Second)
                    : new Dictionary<ResourceKind, int>
                    {
                        [(ResourceKind)savedJob.Resource] = savedJob.ResourceAmount
                    };
                job.RestoreProductionData(
                    savedJob.RoomId,
                    savedJob.RequiredProfession == 0
                        ? (ResourceKind)savedJob.OutputResource == ResourceKind.Food
                            ? WorkProfession.Baker
                            : WorkProfession.Carpenter
                        : (WorkProfession)savedJob.RequiredProfession,
                    inputs,
                    (ResourceKind)savedJob.OutputResource,
                    savedJob.OutputAmount,
                    savedJob.WorkLeft);
                _rooms.RestoreProductionOutputReservations(job);
                _rooms.RegisterPendingProduction(savedJob.RoomId);
            }
            else if (kind == BuildKind.ProductionSupply)
            {
                job.RestoreProductionSupplyData(
                    _world.FromIndex(savedJob.Destination),
                    savedJob.RoomId,
                    (WorkProfession)savedJob.RequiredProfession,
                    (ResourceKind)savedJob.Resource,
                    savedJob.ResourceAmount,
                    savedJob.PickedUp);
                _rooms.RegisterPendingProduction(savedJob.RoomId);
            }
            else if (kind == BuildKind.Haul)
            {
                job.RestoreHaulData(
                    _world.FromIndex(savedJob.Destination),
                    (ResourceKind)savedJob.OutputResource,
                    savedJob.OutputAmount,
                    savedJob.PickedUp);
            }
            if (save.Version >= 14)
                job.RestorePersistentData(
                    (ResourceKind)savedJob.Resource,
                    savedJob.ResourceAmount,
                    (ResourceKind)savedJob.OutputResource,
                    savedJob.OutputAmount,
                    savedJob.RoomId,
                    savedJob.DestinationRoomId,
                    _world.FromIndex(savedJob.Destination),
                    savedJob.CorpseId,
                    savedJob.FacilitySlotId,
                    (LawProcessKind)savedJob.LawProcessType,
                    (WorkProfession)savedJob.RequiredProfession,
                    savedJob.PickedUp,
                    savedJob.OutputAlreadyAccounted);
            if (save.Version >= 17 && kind == BuildKind.RoomOutputHaul && !savedJob.PickedUp)
                _rooms.InternalStorage.At(savedJob.RoomId, cell)?
                    .ReservePickup(savedJob.OutputAmount);
            if (save.Version >= 27)
                job.State = (JobState)savedJob.State;
            _jobs.Add(job);
        }
        _hauling.RetryUnassigned(_jobs);
        if (save.Version >= 11)
        {
            var amounts = new int[ResourceLedger.KindCount];
            var count = System.Math.Min(save.ResourceKinds.Length, save.ResourceAmounts.Length);
            for (var index = 0; index < count; index++)
                if (save.ResourceKinds[index] < amounts.Length)
                    amounts[save.ResourceKinds[index]] = save.ResourceAmounts[index];
            _resources.RestoreAmounts(amounts);
        }
        else
        {
            _resources.Restore(save.Wood, save.Stone);
            _resources.Restore(ResourceKind.Grain, save.Grain);
            _resources.Restore(ResourceKind.Food, save.Food);
            _resources.Restore(ResourceKind.Tools, save.Tools);
            _resources.Restore(ResourceKind.Furniture, save.Version >= 9 ? save.FurnitureStock : 0);
        }
        if (save.Version >= 5)
        {
            _resources.RestoreTelemetry(save.TotalProduced, save.TotalConsumed);
            _economy.Restore(save.EconomyHistory.Select(sample =>
                new EconomySample(sample.Time, sample.Produced, sample.Consumed)));
        }
        if (save.Version >= 17)
            foreach (var savedJob in save.Jobs.Where(job => job.ConstructionTransitAmount > 0))
                _resources.RestoreInTransit(
                    (ResourceKind)savedJob.Resource, savedJob.ConstructionTransitAmount);
        _hauling.RestoreAccountedInTransit(_jobs.All);
        _rooms.RestorePhysicalLogisticsReservations(_jobs.All, _resources);
        _clock.Restore(save.Tick, save.PlayedSeconds);
        if (save.Version >= 14 && save.SettlementWorld is not null)
            _settlementWorld.Restore(save.SettlementWorld);
        if (save.Version >= 23 && save.Invasion is not null)
            _invasion.Restore(save.Invasion);
        if (save.Version >= 14 && save.Technologies is not null)
            _rooms.Technologies.Restore(save.Technologies);
        if (save.Version >= 14 && save.Law is not null)
            _rooms.Law.Restore(save.Law);
        if (save.Version >= 14 && save.Governance is not null)
            _rooms.Governance.Restore(save.Governance);
    }

    private void RestoreLandingSupplyLocations(GridCoord origin)
    {
        var throneCenter = new GridCoord(origin.X + 4, origin.Z + 2);
        for (var z = 1; z <= 6; z++)
        for (var x = 1; x <= 7; x++)
            _world.Data.SetRoof(new GridCoord(origin.X + x, origin.Z + z), true);
        for (var x = 3; x <= 5; x++)
            _world.Data.SetRoof(new GridCoord(origin.X + x, origin.Z + 7), true);
        _world.PlaceLandingMarker(throneCenter, new Vector3(1.4f, 1.4f, 1.4f),
            new Color("d1aa52"), "Throne");
        var landingBoost = Math.Max(0, GameSession.TitleBonuses.Apply("CIVIC_LANDING", 0));
        foreach (var supply in LandingSupplies(origin, landingBoost))
        {
            _rooms.ConfigureLandingSupply(supply.Kind, supply.Cell);
            _world.PlaceLandingMarker(supply.Cell, new Vector3(0.72f, 0.55f, 0.72f),
                supply.Color, $"Landing_{supply.Kind}");
        }
    }

    private static IEnumerable<(ResourceKind Kind, int Amount, GridCoord Cell, Color Color)>
        LandingSupplies(GridCoord origin, double landingBoost)
    {
        // R1..RA positions from settlement.misc.placers.PlacerLanding.
        var slots = new (int X, int Z)[]
        {
            (4, 4), (5, 4), (3, 4), (4, 5), (3, 5),
            (5, 5), (2, 4), (6, 4), (2, 5), (6, 5)
        };
        var configured = OriginalGameData.Current.LandingResources;
        for (var index = 0; index < configured.Count && index < slots.Length; index++)
        {
            var rule = configured[index];
            var slot = slots[index];
            var amount = rule.Amount + (int)(rule.Amount * landingBoost);
            yield return (rule.Resource, amount,
                new GridCoord(origin.X + slot.X, origin.Z + slot.Z), LandingResourceColor(rule.Resource));
        }
    }

    private static Color LandingResourceColor(ResourceKind resource) => resource switch
    {
        ResourceKind.Stone => new Color("8f949b"),
        ResourceKind.Wood => new Color("8a5b32"),
        ResourceKind.Ration => new Color("c5a467"),
        ResourceKind.Livestock => new Color("ded1bd"),
        ResourceKind.Fruit => new Color("c94b40"),
        ResourceKind.Vegetable => new Color("6f9b45"),
        _ => new Color("b8aa82")
    };
}
