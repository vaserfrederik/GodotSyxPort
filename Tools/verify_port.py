#!/usr/bin/env python3
"""Static source/data checks that do not require a Godot or .NET installation."""

from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def text(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8-sig")


def number(source: str, key: str) -> float:
    match = re.search(rf"(?m)^\s*{re.escape(key)}\s*:\s*(-?\d+(?:\.\d+)?)", source)
    if not match:
        raise AssertionError(f"missing {key}")
    return float(match.group(1))


def main() -> None:
    sett = text("Data/Original/init/config/Sett.txt")
    seconds_per_hour = int(number(sett, "SECONDS_PER_HOUR"))
    hours_per_day = int(number(sett, "HOURS_PER_DAY"))
    assert (seconds_per_hour, hours_per_day) == (48, 24)
    assert int(number(sett, "DIMENSION")) == 768
    assert number(text("Data/Original/init/stats/need/_HUNGER.txt"), "RATE") == 0.5
    need_files = list((ROOT / "Data/Original/init/stats/need").glob("*.txt"))
    assert len(need_files) == 20
    assert "LoadNeeds" in text("Scripts/Data/OriginalGameData.cs")
    assert "NeedRates" in text("Scripts/Data/OriginalGameData.cs")
    assert "Required source race HUMAN was not loaded" in text("Scripts/Data/OriginalGameData.cs")
    assert "redundant terminator" in text("Scripts/Data/SyxDataParser.cs")
    assert "if (!_initialized) return;" in text("Scripts/Bootstrap/GameBootstrap.cs")
    disease_files = [path for path in (ROOT / "Data/Original/init/disease").glob("*.txt")
                     if path.stem != "_CONFIG"]
    assert len(disease_files) == 6
    assert number(text("Data/Original/init/disease/_CONFIG.txt"),
                  "REGULAR_SICKNESS_DAY_INVERVAL") == 64
    plague = text("Data/Original/init/disease/PLAGUE.txt")
    assert number(plague, "INCUBATION_DAYS") == 3
    assert number(plague, "FATALITY_RATE") == 0.3
    assert number(plague, "SPREAD") == 0.60
    assert number(plague, "INFECTION_DAYS") == 3

    bakery = text("Data/Original/init/room/REFINER_BAKERY.txt")
    first_bakery = bakery.split("INDUSTRIES:", 1)[1].split("\n\t{", 2)[1]
    assert number(first_bakery, "GRAIN") == 6
    assert number(first_bakery, "WOOD") == 1
    assert number(first_bakery, "BREAD") == 6

    carpenter = text("Data/Original/init/room/WORKSHOP_CARPENTER.txt")
    first_carpenter = carpenter.split("INDUSTRIES:", 1)[1].split("\n\t{", 2)[1]
    assert number(first_carpenter, "WOOD") == 2
    assert number(first_carpenter, "FURNITURE") == 0.5

    work_hours = hours_per_day * 8 // 16
    work_factor = (hours_per_day / work_hours) * 45 / (seconds_per_hour * hours_per_day)
    assert work_factor == 0.078125
    assert int(16 * 6 * work_factor) == 7
    assert int(16 * 1 * work_factor) == 1
    assert int(16 * 0.5 * work_factor) == 0
    assert seconds_per_hour * hours_per_day / 16 == 72

    manifest = json.loads(text("Porting/legacy_compile_manifest.json"))
    assert manifest["legacy_file_count"] == 2465
    assert sum(manifest["counts"].values()) == manifest["legacy_file_count"]
    assert len(manifest["compiled_adaptations"]) == \
        manifest["counts"]["adapted_compiled_separately"]
    assert manifest["counts"] == {
        "recovery_required": 159,
        "reference_pending_semantic_port": 1221,
        "adapted_compiled_separately": 900,
        "engine_or_runtime_reference": 185,
    }
    emissary_ports = [item for item in manifest["compiled_adaptations"]
                      if item["legacy"].startswith(("game/faction/player/emmi/",
                                                    "game/faction/royalty/opinion/"))]
    assert len(emissary_ports) == 13
    assert all("Scripts/World/WorldDiplomacyRuntime.cs" in item["compiled"]
               for item in emissary_ports)
    active_files = list((ROOT / "Scripts").rglob("*.cs"))
    assert len(active_files) >= 125
    assert sum(len(path.read_text(encoding="utf-8-sig").splitlines())
               for path in active_files) >= 37107
    main_scene = text("Main.tscn")
    assert "AppBootstrap.cs" in main_scene and "GameBootstrap.cs" not in main_scene
    app_bootstrap = text("Scripts/Bootstrap/AppBootstrap.cs")
    assert "ShowMainMenu" in app_bootstrap and "ShowNewGame" in app_bootstrap
    assert "ContinueGame" in app_bootstrap and "ShowWorldOverview" in app_bootstrap
    assert "ProcessModeEnum.Disabled" in app_bootstrap and "ResumeSettlement" in app_bootstrap
    assert "ShowNewGame(PlayerStartProfile? initial)" in app_bootstrap
    assert "screen.BackRequested += () => ShowNewGame(playerProfile)" in app_bootstrap
    session = text("Scripts/Bootstrap/GameSession.cs")
    assert "SelectedRegionId" in session and "PlayerRace" in session
    assert "WorldCapitalX" in session and "WorldCapitalY" in session
    assert "RestoreFromSave" in session and "ConfigurePlayerStart" in session
    world_setup = text("Scripts/UI/WorldSetupScreen.cs")
    new_game_profile = text("Scripts/UI/NewGameProfileScreen.cs")
    assert "public void Initialize(PlayerStartProfile? profile)" in new_game_profile
    assert "GetViewport().SizeChanged += LayoutPanel" in new_game_profile
    assert "TooltipText = LocalizedRaceName" in new_game_profile and "ExpandIcon = true" in new_game_profile
    assert "RussianRacePresentation" in new_game_profile
    game_language = text("Scripts/Bootstrap/GameLanguage.cs")
    assert 'Russian = "ru"' in game_language and 'English = "en"' in game_language
    assert "GameLanguage.LoadAndApply()" in app_bootstrap and "Язык / Language" in app_bootstrap
    assert "BannerBitmapEditor" in new_game_profile and "PixelSide = 12" in new_game_profile
    assert "FactionBanners.png" in new_game_profile and "BannerPixels" in session
    assert "MaxValue = 7" in new_game_profile and "PlayerTitle(title.IconIndex)" in new_game_profile
    assert "ConfirmationDialog" in new_game_profile and "ConfirmTitles" in new_game_profile
    assert (ROOT / "Data/Original/assets/sprite/ui/FactionBanners.png").is_file()
    assert (ROOT / "Data/Original/assets/sprite/ui/Titles.png").is_file()
    profile_progress = text("Scripts/Bootstrap/PlayerProfileProgressRuntime.cs")
    assert "user://player_titles.json" in profile_progress and "UnlockTitle" in profile_progress
    assert "0.5 + 0.5 * count" in profile_progress
    assert "1 + (value - 1) * titleValue" in text("Scripts/Bootstrap/PlayerTitleBonusRuntime.cs")
    assert "StartRequested" in world_setup and "CanSetPlayerStart" in world_setup
    assert "DrawCapitalFootprint" in world_setup and "FindCapitalSite" in world_setup
    strategic_world = text("Scripts/World/StrategicWorldRuntime.cs")
    assert "CapitalFootprintDimension = 3" in strategic_world
    assert "CapitalFootprintDimension = 10" not in strategic_world
    assert "CanPlaceCapitalOnTerrain" in strategic_world
    assert "StageCapitol.placableWhole" in strategic_world
    assert "return CanPlaceCapitalOnTerrain(centerX, centerY);" in strategic_world
    assert "CapitalSelectionPreviewDimension = 3" in world_setup
    assert "SettlementGenerationProfile.FromWorldSite" in world_setup
    settlement_terrain = text("Scripts/Settlement/SettlementTerrainGenerator.cs")
    assert "WorldHaloDimension = StrategicWorldRuntime.CapitalFootprintDimension + 2" in settlement_terrain
    assert "world.Terrain.Climate(centerX, centerY)" in settlement_terrain
    assert "profile.WorldSample(sx, sz)" in settlement_terrain
    assert "var nx = qx + Math.Sign(dx);" in settlement_terrain
    assert "mappedHeight * 0.72" not in settlement_terrain
    assert "ShowFinishStage" in world_setup and "BackFromWorldStage" in world_setup
    assert "RegenerateCurrentStage" in world_setup and "CenterOnPlayerCapital" in world_setup
    assert "ClearGeneratedCivilizations" in world_setup
    assert "RebuildRealmMapTexture" in world_setup
    profile_screen = text("Scripts/UI/NewGameProfileScreen.cs")
    assert '"AMEVIA" or "Q_AMEVIA" => "Amevia"' in profile_screen
    original_icons = text("Scripts/UI/OriginalUiIcons.cs")
    assert "FileAccess.FileExists(path)" in original_icons
    room_planner = text("Scripts/Rooms/RoomPlanner.cs")
    assert "ShowDoorPreview" in room_planner
    build_palette = text("Scripts/UI/RoomBuildPalette.cs")
    assert "public void OpenConstruction()" in build_palette
    assert "public void OpenJobs()" in build_palette
    assert build_palette.index('"MOVE_THRONE"') < build_palette.index('"FENCES"') < \
        build_palette.index('"ROADS"') < build_palette.index('"STRUCTURES"') < \
        build_palette.index('"FORTIFICATION"')
    assert build_palette.index('"JOB_FORAGE"') < build_palette.index('"JOB_HUNT"') < \
        build_palette.index('"JOB_CLEAR_WOOD"') < build_palette.index('"JOB_CLEAR_STONE"') < \
        build_palette.index('"JOB_CLEAR_ALL"') < build_palette.index('"JOB_CLEAR_WATER"') < \
        build_palette.index('"JOB_CLEAR_MOUNTAIN"')
    assert "GeneratorMapsPath" in world_setup and "LoadTerrainTemplates" in world_setup
    assert "GenerateConfiguredWorld" in world_setup and "ShowTerrainStage" in world_setup
    assert "MinValue = 0, MaxValue = 100" in world_setup
    assert "BuildTemplatePreview" in world_setup and "KeepSeedDigits" in world_setup
    assert "GenerateConfiguredWorld(int seed)" in world_setup and "_seed = seed;" in world_setup
    assert "seed == 0 ? 1 : seed" not in world_setup
    assert "StepMapType" in world_setup and "ConfirmationDialog" in world_setup
    assert "LoadPngFromBuffer" in world_setup and "LoadJpgFromBuffer" in world_setup
    assert "MapViewport" in world_setup and "DrawTextureRectRegion" in world_setup
    assert "BuildTerrainEditor" in world_setup and "PaintTerrain" in world_setup
    assert '"Река", "Малая река"' in world_setup
    assert "private float _mapZoom = 16f" in world_setup
    assert "HandleMapInput" in world_setup and "mapInput.GuiInput" in world_setup
    assert "2f : 0.5f), 16f, 64f" in world_setup
    generator_maps = ROOT / "Data/Original/assets/sprite/world/generatorMaps"
    assert len(list(generator_maps.glob("*.png"))) == 24
    assert (generator_maps / "Isle.png").is_file()
    assert "InitializeOverview" in world_setup and "SelectRace" in world_setup
    assert "GenerateTerrain" in world_setup and "GenerateCivilizations" in world_setup
    assert "MiniMapViewport" in world_setup and "SettlementResources" in world_setup
    assert "SettlementGenerationProfile.FromWorldSite" in world_setup
    assert "var selected = _world?.Region(_selectedRegion)" in world_setup
    assert "DrawRealmBorders" in world_setup and "_world.Cities" in world_setup
    assert "WorldTextureScale = " in world_setup and "ApplyOriginalGroundDetail" in world_setup
    assert "MapViewport = new(0, 30, 1600, 870)" in world_setup
    assert "MapToMini" in world_setup and "FactionColor" in world_setup
    assert "_worldFinalized" in world_setup and '"Начать игру"' in world_setup
    profile_setup = text("Scripts/UI/NewGameProfileScreen.cs")
    assert "RacePresentation" in profile_setup and "Начальная сложность" in profile_setup
    assert "ShowRace" in profile_setup and "ShowFaction" in profile_setup
    assert "ShowTitles" in profile_setup and "BannerPreview" in profile_setup
    assert "PlayerStartProfile" in profile_setup and "_selectedTitles.Count < 5" in profile_setup
    assert "ShowWorldSetup" in app_bootstrap and "playerProfile" in app_bootstrap
    world_entities = text("Scripts/World/WorldEntityRuntime.cs")
    assert "WorldEntityKind" in world_entities and "WorldEntitySnapshot" in world_entities
    world_regions = text("Scripts/World/WorldRegionRuntime.cs")
    assert "PlayerCapitalSnapshot" in world_regions and "TransferPopulation" in world_regions
    world_bridge = text("Scripts/World/WorldSettlementBridge.cs")
    assert "ConnectToPlayerCapital" in world_bridge and "QueueRegionalImmigration" in world_bridge
    havens = text("Scripts/World/WorldHavenRuntime.cs")
    assert "HavenMigrationOffer" in havens and "BaseMigrationPerDay" in havens
    landmarks = text("Scripts/World/WorldLandmarkRuntime.cs")
    assert "LandmarkVisit" in landmarks and "RegionAttraction" in landmarks
    assert "foreach (var source in _world.Landmarks)" in landmarks
    assert "ToSimulationKind(source.Kind)" in landmarks and "source.Name" in landmarks
    tourism = text("Scripts/Tourism/TourismRuntime.cs")
    assert "TouristState" in tourism and "AddVisitSummary" in tourism
    reviews = text("Scripts/Tourism/TourismReviewRuntime.cs")
    assert "PositiveThreshold" in reviews and "AttractionMultiplier" in reviews
    planner = text("Scripts/Tourism/TouristPlannerRuntime.cs")
    assert "MaximumTravelDistance = 550" in planner and "TouristParty" in planner
    shipments = text("Scripts/World/WorldTradeShipmentRuntime.cs")
    assert "MaximumRouteDistance = 550" in shipments
    assert "PartnerReservationId" in shipments and "CurrentRegionId" in shipments
    world_trade = text("Scripts/Trade/WorldTradeRuntime.cs")
    assert "ShipmentCreated" in world_trade and "ReconcileArrivals" in world_trade
    settlement_world = text("Scripts/Settlement/SettlementWorldRuntime.cs")
    assert "SettlementWorldSnapshot" in settlement_world and "Diagnose" in settlement_world
    bootstrap = text("Scripts/Bootstrap/GameBootstrap.cs")
    assert "OpenConstructionMenu" in bootstrap and "OpenJobsMenu" in bootstrap
    assert '"Строительство", ref x,\n            OpenConstructionMenu' in bootstrap
    assert '"Задания", ref x,\n            OpenJobsMenu' in bootstrap
    assert "ToggleWindow(_constructionPalette)" not in bootstrap
    assert "PerformTerrainJob" in text("Scripts/Citizens/CitizenSystem.cs")
    for terrain_job in ("Forage", "ClearWood", "ClearStone", "ClearWater", "DigTunnel"):
        assert f"BuildKind.{terrain_job}" in bootstrap
    assert "new SettlementWorldRuntime(" in bootstrap and "_settlementWorld.Tick(" in bootstrap
    assert "new WorldTradeRuntime(" in bootstrap and "_worldTrade.Tick(" in bootstrap
    strategic_map = text("Scripts/UI/StrategicWorldMap.cs")
    assert "VisibleShipments(RegionVisible)" in strategic_map
    assert "_rooms.TickTrade(" not in bootstrap
    room_system = text("Scripts/Rooms/RoomSystem.cs")
    assert "public bool Dismantle(" in room_system and "jobs.CancelRoom(roomId, resources)" in room_system
    assert "InternalStorage.ClearRoom(roomId)" in room_system and "Logistics.ClearRoom(roomId)" in room_system
    jobs = text("Scripts/Simulation/JobBoard.cs")
    assert "public int CancelRoom(int roomId" in jobs
    assert "public int ActivateRoomConstruction(int roomId)" in jobs
    assert "if (job.State == JobState.Reservable) Enqueue(job)" in jobs
    housing = text("Scripts/Rooms/HousingRuntime.cs")
    assert "public IReadOnlyList<int> RemoveRoom(int roomId)" in housing
    assert "public bool RemoveFurniture(GridCoord cell)" in text("Scripts/Settlement/GridWorld.cs")
    assert len(list((ROOT / "Data/Original").rglob("*.*"))) >= 1012
    assert "public const int Width = 768;" in text("Scripts/Settlement/GridWorld.cs")
    assert "public const int Height = 768;" in text("Scripts/Settlement/GridWorld.cs")
    ledger = text("Scripts/Resources/ResourceLedger.cs")
    enum_body = ledger.split("public enum ResourceKind : byte", 1)[1].split("}", 1)[0]
    assert len([line for line in enum_body.splitlines() if line.strip().rstrip(",").isidentifier()]) == 42
    jobs = text("Scripts/Simulation/BuildJob.cs")
    assert "MaximumFetchAmount = 6" in jobs
    assert "MaximumFetchDistance = 250" in jobs
    assert "job.MaterialsNeeded, BuildJob.MaximumFetchAmount), maximumAmount" in ledger
    board = text("Scripts/Simulation/JobBoard.cs")
    assert "TryClaimAdjacentCompatible" in board
    assert "fetchDistance > BuildJob.MaximumFetchDistance" in board
    assert "new GridCoord(-1, 1)" in board and "new GridCoord(1, 1)" in board
    rooms = text("Scripts/Rooms/RoomSystem.cs")
    assert "InputStorageMaximum = 31" in rooms
    assert "TerrainProductionMultiplier" in rooms
    assert "_world.Data.FertilityD" in rooms
    assert "_world.Data.MineralType(cell) == minableIndex" in rooms
    assert "InputFetchMaximum = 15" in rooms
    save_service = text("Scripts/Save/SaveGameService.cs")
    assert "public int Version { get; set; } = 37;" in save_service
    assert "AdditionalOutputRecipes" in save_service
    assert "public int LandingOrigin" in save_service
    assert "ConstructionResources" in save_service
    assert "ConstructionRequired" in save_service
    assert "ConstructionDelivered" in save_service
    assert "ConstructionTransitAmount" in save_service
    assert "public byte State { get; set; }" in save_service
    assert "public int[] FurnitureCells { get; set; }" in save_service
    assert "public int[] FurnitureBlockerCells { get; set; }" in save_service
    assert "public int[] FurnitureReachableCells { get; set; }" in save_service
    assert "SavedRoomStorage[] InternalStorage" in save_service
    for snapshot, state in (("SettlementWorldSnapshot", "SettlementWorld"),
                            ("TechnologySnapshot", "Technologies"),
                            ("SettlementLawSnapshot", "Law"),
                            ("SettlementGovernanceSnapshot", "Governance"),
                            ("StrategicWorldSnapshot", "StrategicWorld"),
                            ("RegionalEconomySnapshot", "RegionalEconomy")):
        assert f"{snapshot}? {state}" in save_service
    assert "CitizenHealthSnapshot? Health" in save_service
    assert "DestinationRoomId" in save_service and "LawProcessType" in save_service
    diplomacy = text("Scripts/World/WorldDiplomacyRuntime.cs")
    assert "Produced / (double)Spent" in diplomacy
    assert "SupportRegion" in diplomacy and "FlatterFaction" in diplomacy
    assert "SabotageFaction" in diplomacy and "TrySetStance" in diplomacy
    assert "GiftWorkdayCredits = 400" in diplomacy
    assert "RequiredStanding" in diplomacy and "LastWarningDay > 10" in diplomacy
    factions = text("Scripts/World/WorldFactionRuntime.cs")
    assert "MaximumCourtMembers = 4" in factions and "TickCourts(days, day)" in factions
    assert "GiftOpinionMultiplier" in factions and "succession:" in factions
    assert "AddPlayerSupport" in text("Scripts/World/RegionalEconomyRuntime.cs")
    assert "_diplomacy.Tick(days, day)" in settlement_world
    assert "_settlementWorld.Diplomacy.SetProduction(_rooms.Governance.Diplomacy)" in bootstrap
    assert "_settlementWorld.Diplomacy.TrySetStance" in strategic_map
    assert "_world.SetStance" not in strategic_map
    assert "public const int MaximumFetchDistance = 250;" in jobs
    events = text("Scripts/Events/SettlementEventRuntime.cs")
    assert "CitizenBreakPoint = 0.85" in events
    assert "secondsPerDay * 1.5" in events
    assert "secondsPerDay * (0.25 + _random.NextDouble() * 0.75)" in events
    assert "secondsPerDay * 16 * 24" in events
    assert "delta / (secondsPerDay * 8)" in events
    assert "EventActionResult" in events and "Requires world, boost registry" in events
    assert "EventWorkSuspended" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "SettlementEventRuntime" in text("Scripts/Bootstrap/GameBootstrap.cs")
    social = text("Scripts/Citizens/CitizenSocialRuntime.cs")
    assert "SearchDistance = 64" in social
    assert "InteractionMinimumSeconds = 5" in social
    assert "InteractionMaximumSeconds = 15" in social
    assert "BestFriend" in social and "DailyRelationDecay" in social
    assert "TryStartSocialInteraction" in text("Scripts/Citizens/CitizenSystem.cs")
    planner = text("Tools/plan_semantic_batch.py")
    assert "FINAL_STAGE_PREFIXES" in planner and "PlanUprise.cs" in planner
    water = text("Scripts/Rooms/WaterInfrastructureRuntime.cs")
    assert "PumpMaximumOutput = 100" in water
    assert "PumpDegradationPenalty = 0.8" in water
    assert "DrainRadius = 10" in water and "TilesPerSecond = 1" in water
    assert "WaterInfrastructureRuntime" in rooms
    assert "SetMoisture" in text("Scripts/Settlement/WorldGridData.cs")
    assert "GateRuntime" in rooms
    assert "LockedForSubjects" in text("Scripts/Rooms/GateRuntime.cs")
    logistics = text("Scripts/Rooms/SettlementLogisticsRuntime.cs")
    assert "Station" in logistics and "crates * 400" in logistics
    assert 'RoomKey(room) == "_STATION" ? 15' in rooms
    hospitality = text("Scripts/Rooms/HospitalityRuntime.cs")
    assert "InnWorkersPerBed = 1.0 / 8.0" in hospitality
    assert "TryReserve" in hospitality and "RESTHOME_NORMAL" in hospitality
    assert "TryRetireCitizen" in text("Scripts/Citizens/CitizenSystem.cs")
    builders = text("Scripts/Rooms/BuilderInfrastructureRuntime.cs")
    assert "DefaultRadius = 32" in builders and "MaximumWorkers = 20" in builders
    assert "BuilderInfrastructureRuntime" in rooms
    registry = text("Scripts/Stats/SettlementStatRegistry.cs")
    assert "HistoryDays = 48" in registry
    assert "SetEvent" in registry and "DefineDecree" in registry
    assert "SettlementStatRegistry" in text("Scripts/Stats/SettlementStatsRuntime.cs")
    assert "public static BuildJob Road" in jobs
    assert "MovementSpeedMultiplier" in text("Scripts/Settlement/WorldGridData.cs")
    terrain_grid = text("Scripts/Settlement/WorldGridData.cs")
    assert "GroundKind" in terrain_grid
    assert "FertilityD" in terrain_grid and "MineralAmount" in terrain_grid
    assert "FishAmount" in terrain_grid and "GrowableAmount" in terrain_grid
    assert "FoundationD" in terrain_grid and "System.Math.Clamp(value, 0, 3)" in terrain_grid
    assert "System.Math.Clamp(amount, 0, 63)" in terrain_grid
    terrain_generator = text("Scripts/Settlement/SettlementTerrainGenerator.cs")
    assert "HashCode.Combine" not in terrain_generator
    expected_calls = [
        "GenerateBaseAndFertility(world, profile);",
        "GenerateMountains(world, profile, settings, polymap);",
        "GenerateCaves(world, profile, settings);",
        "GenerateWater(world, profile, settings, polymap);",
        "GenerateMinerals(world, profile, settings);",
        "FinishGroundAndFertility(world, profile, settings);"
    ]
    positions = [terrain_generator.index(call) for call in expected_calls]
    assert positions == sorted(positions)
    minable_files = list((ROOT / "Data/Original/init/resource/minable").glob("*.txt"))
    assert len(minable_files) == 6
    assert all("TERRAIN:" in path.read_text(encoding="utf-8-sig") for path in minable_files)
    assert "LoadMinables" in text("Scripts/Data/OriginalGameData.cs")
    growable_files = list((ROOT / "Data/Original/init/resource/growable").glob("*.txt"))
    assert len(growable_files) == 7
    assert all("GROWTH_VALUE:" in path.read_text(encoding="utf-8-sig") for path in growable_files)
    assert "LoadGrowables" in text("Scripts/Data/OriginalGameData.cs")
    assert "FinishWater(world);" in terrain_generator
    assert "GenerateFish(world, profile);" in terrain_generator
    assert "GenerateGrowth(world, profile);" in terrain_generator
    assert "GenerateEdibles(world, profile, settings);" in terrain_generator
    assert "GenerateFoundation(world, profile);" in terrain_generator
    weather = text("Scripts/Simulation/SettlementWeatherRuntime.cs")
    assert "CropGrowthMultiplier" in weather
    assert "Moisture * 4.0" in weather
    assert "1.0 / 8.0" in weather and "5.0 / 8.0" in weather
    assert "LoadClimates" in text("Scripts/Data/OriginalGameData.cs")
    climate = text("Data/Original/init/config/CLIMATE.txt")
    assert number(climate.split("TEMPERATE:", 1)[1], "TEMP_COLD") == -0.15
    race_files = list((ROOT / "Data/Original/init/race").glob("*.txt"))
    assert len(race_files) == 8
    assert all("PROPERTIES:" in path.read_text(encoding="utf-8-sig") for path in race_files)
    race_data = text("Scripts/Data/OriginalGameData.cs")
    assert "RaceRule" in race_data and "LoadRaces" in race_data
    assert "RAID_MERCINARY" in race_data and "RAIDER_NAME_FILE" in race_data
    assert len(re.findall(r'(?m)^\s*".*",?\s*$',
                         text("Data/Original/text/race/raider/name/Normal.txt"))) == 40
    identity = text("Scripts/Citizens/CitizenIdentityRuntime.cs")
    assert "enum SocialClass" in identity and "enum HumanoidType" in identity
    assert "CitizenOrigin" in identity and "ChangeType" in identity
    assert "PopulationByRace" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "agent.Id, raceKey, SocialClass.Citizen" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "HumanoidTypeRules.Works" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "WorkPreference(candidate, profession)" in text("Scripts/Citizens/CitizenSystem.cs")
    entry = text("Scripts/Settlement/SettlementEntryRuntime.cs")
    assert "enum ArrivalCause" in entry and "enum LeaveCause" in entry
    assert "short.MaxValue" in entry and "while (_spawnTime >= 1" in entry
    assert "BeginReachabilityRefresh" in entry and "TryBeginEmigration" in entry
    assert "ReachabilityCellsPerTick = 4096" in entry
    assert "AdvanceReachabilityRefresh" in entry
    immigration = text("Scripts/Settlement/ImmigrationRuntime.cs")
    assert "HappinessThreshold = 0.5" in immigration
    assert "state.Emigrants += -delta * wanted / (2 * secondsPerDay)" in immigration
    assert "amount / (amount + expectedPopulation)" in immigration
    citizens = text("Scripts/Citizens/CitizenSystem.cs")
    assert "SpawnCitizen(" in citizens and "LeaveCause.Emigrated" in citizens
    assert "_multiMesh.InstanceCount = _agents.Count" in citizens
    human = text("Data/Original/init/race/HUMAN.txt")
    assert number(human.split("PROPERTIES:", 1)[1], "BABY_DAYS") == 12
    assert number(human.split("PROPERTIES:", 1)[1], "CHILD_DAYS") == 80
    reproduction = text("Scripts/Citizens/CitizenReproductionRuntime.cs")
    assert "CitizenLifeStage" in reproduction and "SourceYearDays = 16" in reproduction
    assert "race.BabyDays + race.ChildDays" in reproduction
    assert "ChildTypeForAge" in reproduction and "ChecksPerYear = 4" in reproduction
    assert "BaseReproductionSpeed = 0.1" in reproduction and "IsFertile" in reproduction
    assert "currentAndIncomingPopulation;" in reproduction
    assert "CheckNaturalReproduction" in citizens and "ChancePerCheck" in citizens
    child_behavior = text("Scripts/Citizens/ChildBehaviorRuntime.cs")
    assert "ChildDayStart = 8.0 / 24.0" in child_behavior
    assert "ChildDayLength = 12.0 / 24.0" in child_behavior
    assert "PlaySearchDistance = 64" in child_behavior
    assert "TickChildBehavior" in citizens and "SelectChildPlaymate" in citizens
    assert 'var boosts = data.Get("BOOST")' in race_data
    standing_runtime = text("Scripts/Stats/SettlementStandingRuntime.cs")
    assert "EntityMaximum = 40000" in standing_runtime
    assert "MaximumPopulationFactor = 0.7143" in standing_runtime
    assert "FulfillmentExponent = 2.65" in standing_runtime
    assert "LoyaltyChangeDays = 100.0" in standing_runtime
    assert "StandingContribution" in standing_runtime and "MoveLoyalty" in standing_runtime
    assert "RaceStandingRule" in race_data and "ReadStandingRules" in race_data
    assert "FromRaceRule" in standing_runtime and "Normalize(StandingContribution" in standing_runtime
    assert "normalized = defaults <= 0 ? 0 : -current / defaults" in standing_runtime
    settlement_stats = text("Scripts/Stats/SettlementStatsRuntime.cs")
    assert "PopulationByRaceAndClass" in citizens
    assert "UpdateStanding" in settlement_stats and "StandingInput" in settlement_stats
    assert 'key.StartsWith("STORED_"' in settlement_stats
    assert 'key.StartsWith("SERVICE_"' in settlement_stats
    assert 'key.Equals("FOOD_STARVATION"' in settlement_stats
    assert 'key.Equals("ENVIRONMENT_UNBURRIED"' in settlement_stats
    assert "UnburiedPressure(Population)" in settlement_stats
    assert "AverageRoomServiceAccess" in citizens
    assert "NewInfantsAllowed" in reproduction and "ChildrenPerYear" in reproduction
    childcare = text("Scripts/Rooms/ChildcareRuntime.cs")
    assert "ChildrenPerEmployee = 10" in childcare
    assert "NurseryPlaySeconds = 120.0" in childcare
    assert "BreederWorkSeconds = 30.0" in childcare
    assert "ResourceKind.Meat, 2" in childcare and "IncubationDays { get; set; } = 8" in childcare
    assert "AdvancePopulationDay" in citizens and "ReproductionStatus" in citizens
    assert "agent.BabyDays++" in citizens and "birth.Parent, ageDays: birth.Age" in citizens
    assert "TryBeginParenthood" in citizens and "HumanoidType.ParentSlave" in citizens
    school = text("Scripts/Rooms/SchoolRuntime.cs")
    assert "DefaultChildLimit = EducationMaximum / 6" in school
    assert "StationPreparationSteps = 3" in school and "WorkCyclesPerDay = 40" in school
    assert "PaperPerLesson = 0.1" in school and "MissingSchoolDaysBeforeGrowth = 3" in school
    assert "Schools.BeginDay" in citizens and "Schools.TryAttend" in citizens
    assert 'RoomKey(room) == "SCHOOL_NORMAL"' in rooms
    assert "candidate.Identity.ParentId == citizenId" in citizens
    assert "Childcare.Synchronize" in rooms and "Childcare.Tick" in citizens
    floor_root = ROOT / "Data/Original/init/settlement/floor"
    road_definitions = [
        path for path in floor_root.glob("*.txt")
        if re.search(r"(?m)^\s*ROAD\s*:", path.read_text(encoding="utf-8-sig"))
    ]
    assert len(road_definitions) == 12
    room_files = list((ROOT / "Data/Original/init/room").glob("*.txt"))
    assert len(room_files) == 112
    original_data = text("Scripts/Data/OriginalGameData.cs")
    assert "RoomArchetype" in original_data
    assert "RoomConstructionRule" in original_data
    assert "RoomServiceRule" in original_data
    assert "var singleIndustry = data.Get(\"INDUSTRY\")" in original_data
    assert "ReadRate(pair.Value)" in original_data
    assert "YEILD_WORKER_DAILY" in original_data
    assert "room.YieldWorkerDaily" in original_data
    blueprints = text("Scripts/Rooms/RoomBlueprintCatalog.cs")
    assert "RoomBlueprintRuntime" in blueprints
    assert "RoomCategoryRuntime" in blueprints
    assert "rules.OrderBy" in blueprints
    assert "AverageDegradation" in blueprints
    assert "UpgradeResourceMask" in blueprints
    assert "UpgradeCompletion" in blueprints
    assert "Enumerable.Repeat(1.0" in original_data
    furnisher = text("Scripts/Rooms/FurnisherRuntime.cs")
    assert "MaximumResources = 4" in furnisher
    assert "ConstructionCost" in furnisher
    assert "Blueprint.UpgradeResourceMask" in furnisher
    assert "var amount = Math.Max(0, area) * AreaCost" in furnisher
    assert "RelativeStat" in furnisher and "EfficiencyStat" in furnisher
    assert "ReadFurnisherItems" in original_data
    room_instance = text("Scripts/Rooms/RoomInstanceRuntime.cs")
    assert "MaximumArea = 2048" in room_instance
    assert "MaximumAreaPlacementDimension = 55" in room_instance
    assert "MaximumDimension = 150" in room_instance
    assert "Exists && Enabled && Reachable" in room_instance
    assert "RoomRuntimeState" in room_instance
    assert "Blueprint.Furnisher.ConstructionCost" in room_instance
    assert "room.Isolation" in rooms
    room_construction = text("Scripts/Rooms/RoomConstructionRuntime.cs")
    citizens = text("Scripts/Citizens/CitizenSystem.cs")
    assert "ResourceUnderflowRuntime" in room_construction
    assert "NextUnderflow(BuildJob.MaximumFetchAmount)" in room_construction
    assert "BuildJob.RoomConstructionSupply" in room_construction
    assert "rooms.FindSupplyCell(underflow.Value.Key)" in room_construction
    assert "rooms.ConstructionDeliveryCell(room.Record, jobs)" in room_construction
    assert "jobs.ActivateRoomConstruction(order.RoomId)" in room_construction
    assert "public RoomConstructionOrder Restore(" in room_construction
    assert "RoomConstructionSupply" in jobs
    assert "public static BuildJob RoomWall" in jobs
    assert "BuildJob.RoomWall(cell, room.Id)" in rooms
    assert "CancelSupply(BuildJob job)" in room_construction
    assert "CompletedWallCount(room.Record) < room.Record.RequiredWalls.Count" in room_construction
    assert "_rooms.Construction.CancelSupply(job)" in bootstrap
    assert "job is { RoomId: > 0 } && job.IsConstruction" in bootstrap
    assert "Construction.Orders.TryGetValue" in rooms
    assert "CompleteRoomConstructionSupply" in citizens
    assert "agent.JobTravelSeconds)" in citizens
    assert "FindWalkableSpawnCell" in citizens
    assert "destination = _rooms.FindSupplyCell(job.Resource);" in citizens
    assert "_rooms.FindSupplyCell(job.Resource) ?? agent.Cell" not in citizens
    assert "AvailableFood(resources)" in citizens
    assert "MaxAssignmentsPerFrame = 4" in citizens
    assert "_assignmentsRemainingThisFrame > 0" in citizens
    assert "_professionRebalanceLeft -= delta" in citizens
    assert "_professionRebalanceLeft = 1.0" in citizens
    employment = text("Scripts/Rooms/RoomEmploymentRuntime.cs")
    assert "RecordFetchCycle" in employment
    assert "freeFetchSeconds = 22.5" in employment
    assert "Logistics.ReserveProductionSupply(input.Key, cell, amount)" in rooms
    assert "DefaultToolTarget(room.DefinitionKey)" in rooms
    assert "FindSupplyCell(ResourceKind.Tools)" in rooms
    overlay = text("Scripts/UI/GlobalMapOverlay.cs")
    assert "SettlementMapLayer.Unemployment" in overlay
    assert "ForEachUnemployedCitizen" in overlay
    assert "IsDynamicLayer(Layer)" in overlay
    assert "LayerRefreshed" in overlay and "LayerRefreshed" in bootstrap
    grid_world = text("Scripts/Settlement/GridWorld.cs")
    assert "PlaceLandingMarker" in grid_world
    assert "PlaceLandingThrone" in rooms
    assert "ConfigureLandingSupply" in rooms
    placement = text("Scripts/Rooms/RoomPlacementRuntime.cs")
    assert "RoomInstanceRuntime.MaximumArea" in placement
    assert "RoomInstanceRuntime.MaximumDimension" in placement
    assert "Все клетки комнаты должны быть соединены" in placement
    assert "Закрытому помещению требуется дверной проём" in placement
    assert "CreateFromDefinition" in placement
    assert "blueprint.Furnisher.ConstructionCost" in placement
    assert "ExpandArea" in placement and "ShrinkArea" in placement
    assert "PreviewArea" in placement and "RoomPlacementSnapshot" in placement
    assert "public bool Undo()" in placement and "HasHistory" in placement
    storage = text("Scripts/Rooms/RoomStorageRuntime.cs")
    assert "ReservedPickup" in storage and "ReservedSpace" in storage
    assert "ReservableResource" in storage and "ReservableSpace" in storage
    assert "FindResource" in storage and "FindSpace" in storage
    assert "AvailableSpace" in storage and "ReserveSpace" in storage
    assert "DepositUnreserved" in storage and "AllSlots" in storage
    assert "InternalStorage.ReserveSpace" in rooms
    assert "InternalStorage.Deposit(reservations, output)" in rooms
    assert "hauling.ScheduleRoomExports(room.Id, jobs)" in rooms
    hauling = text("Scripts/Hauling/HaulingSystem.cs")
    assert "BuildJob.MaximumFetchAmount" in hauling
    assert "slot.ReservePickup(maximum)" in hauling
    assert "BuildJob.RoomOutputHaul" in hauling
    assert "InternalStorage.AllSlots" in hauling
    assert "CanExportInternalStorage" in hauling and "CanExportInternalStorage" in rooms
    services = text("Scripts/Rooms/RoomServiceRuntime.cs")
    assert "CurrentHigh" in services and "LastHigh" in services
    assert "127.0 * (Total - Available) / Total" in services
    assert "instance.Rule.Radius" in services
    assert "Services.Synchronize(_rooms, Blueprints, FurnisherStatRuntime.Value)" in rooms
    assert "ServiceCandidates" in rooms
    assert "distance <= service.Rule.Radius" in rooms
    assert "TickServices" in rooms
    assert "RoomArchetype.Agriculture => WorkProfession.Farmer" in rooms
    assert "RoomArchetype.Extraction => WorkProfession.Miner" in rooms
    assert "room.Cells.Count * 7.0" in rooms
    assert "room.Cells.Count / 1.5" in rooms
    assert "room.RequiredFurniture = furnitureCells is { Count: > 0 }" in rooms
    assert "room.ItemGroupAmounts.GetValueOrDefault(0)" in rooms
    assert "if (storageCells.Length == 0 && produces)" in rooms
    animal_files = list((ROOT / "Data/Original/init/animal").glob("*.txt"))
    assert len(animal_files) == 9
    assert number(text("Data/Original/init/animal/AUROCH.txt"), "MASS") == 110
    pasture_files = list((ROOT / "Data/Original/init/room").glob("PASTURE_*.txt"))
    assert len(pasture_files) == 6
    assert number(text("Data/Original/init/room/FISHERY_NORMAL.txt"), "FISH") == 1.4
    assert number(text("Data/Original/init/room/HUNTER_NORMAL.txt"), "MAX_EMPLOYED") == 15
    orchard_data = text("Data/Original/init/room/ORCHARD_FRUIT.txt")
    assert number(orchard_data, "DAYS_TILL_GROWTH") == 64
    assert number(orchard_data, "RIPE_AT_PART_OF_YEAR") == 0.60
    special = text("Scripts/Rooms/SpecialProductionRuntime.cs")
    assert "ANIMALS_PER_TILE" not in special  # formula is represented directly below
    assert "2.5 / (rule.AnimalMass + 10.0)" in special
    assert "1.0 / 9.0" in special and "room.Cells.Count / 64.0" in rooms
    assert "_elapsedDays - pair.Key >= 14" in special
    assert "Math.Min(toDie, instance.AnimalsToDie)" in special
    assert "0.1 + 0.9 * adultShare" in special
    assert "HunterLuck" in special and "0.6 +" in special and "* 0.8" in special
    assert "instance.HunterLuck / (1.0 + excess / (maximum * 4.0))" in special
    assert "FishAccess" in special and "TileFlags.FishSpot | TileFlags.Water" in special
    assert "DaysPerYear = 16" in special and "45.0 + 3.0" in special
    assert "ClearVegetationStep(cell)" in special
    assert "PastureLivestockSupply" in jobs and "CompletePastureLivestockSupply" in citizens
    assert "SpecialProduction.ProductionMultiplier" in rooms
    assert "AdditionalOutputProgress" in rooms and "reservation.Slot.Resource == outputRule.Key" in rooms
    assert "ServicesPerDay = 4" in citizens
    assert "MaximumPendingServices = ServicesPerDay * 2" in citizens
    assert "TryStartService" in citizens and "TickServicePlan" in citizens
    assert "selected.Service.Reserve()" in citizens
    assert "agent.Service.Consume()" in citizens
    assert "GD.Randf() * usageTotal" in citizens
    assert "System.Math.Sqrt(proximity)" in citizens
    assert "ClearService(agent, true)" in citizens
    assert "UpdateNeeds(agent)" in citizens
    assert "agent.Needs[rule.Need]" in citizens
    assert "- NeedChunk" in citizens
    assert "agent.ServiceBoosts[boost.Key] = boost.Value" in citizens
    assert "agent.Dirtiness = 0" in citizens
    assert "agent.Exposure - NeedChunk" in citizens
    assert "ServiceNeed(key, service)" in original_data
    religion_files = list((ROOT / "Data/Original/init/religion").glob("*.txt"))
    assert len(religion_files) == 4
    for religion_file in religion_files:
        religion = religion_file.read_text(encoding="utf-8-sig")
        assert number(religion, "DEFAULT_SPREAD") == 1
        assert "OPPOSITION:" in religion and "BOOST:" in religion
    assert "ReligionRule" in original_data and "LoadReligions" in original_data
    assert "TempleSacrificeRule" in original_data and "ReadTempleSacrifice" in original_data
    assert 'resource = "LIVESTOCK"' in original_data
    religion_runtime = text("Scripts/Citizens/ReligionRuntime.cs")
    assert "ChooseAffiliation" in religion_runtime
    assert "DefaultSpread" in religion_runtime and "Opposition" in religion_runtime
    temple_files = list((ROOT / "Data/Original/init/room").glob("TEMPLE_*.txt"))
    shrine_files = list((ROOT / "Data/Original/init/room").glob("SHRINE_*.txt"))
    assert len(temple_files) == 4 and len(shrine_files) == 4
    assert all(number(path.read_text(encoding="utf-8-sig"), "RADIUS") == 750
               for path in temple_files)
    assert all(number(path.read_text(encoding="utf-8-sig"), "RADIUS") == 128
               for path in shrine_files)
    temples = text("Scripts/Rooms/TempleRuntime.cs")
    assert "Math.Ceiling(temple.Sacrifice.Time * 3.0)" in temples
    assert "BuildJob.MaximumFetchAmount" in temples
    assert "temple.Sacrifices = (temple.Sacrifices + 1) / 2" in temples
    assert "temple.SacrificesTotal = (temple.SacrificesTotal + 1) / 2" in temples
    assert "GD.Randf() < temple.Sacrifice.Time - whole" in temples
    assert "BuildKind.TempleSupply" in jobs and "BuildJob.TempleSupply" in temples
    assert "CompleteTempleSupply" in citizens
    assert "ServiceMatchesReligion" in rooms
    assert "candidate.Service.RoomId, agent.Religion" in citizens
    assert "var prayers = 5 + (int)(GD.Randi() % 10)" in citizens
    assert "seconds += 4 + (int)(GD.Randi() % 4)" in citizens
    bath = text("Scripts/Rooms/BathRuntime.cs")
    assert "service.SuspendAvailable()" in bath
    assert "BuildJob.BathPump" in bath and "BuildJob.BathFuel" in bath
    assert "WorkLeft = 20f" in jobs
    assert "bath.Heat >= service.Total * 2" in bath
    assert "bath.CoalPerServiceDay / Math.Max(1, secondsPerDay)" in bath
    assert "ResourceKind.Coal" in bath
    assert "CompleteBathPump" in citizens and "CompleteBathFuel" in citizens
    activity = text("Scripts/Rooms/ActivityServiceRuntime.cs")
    assert "BuildKind.ActivityWork" in activity
    assert "hour > 11 || hour < 6" in activity
    assert "activity.ActiveWorkers <= 0" in activity
    assert "Activities.IsOpen(service.RoomId, _serviceDayFraction)" in rooms
    assert 'RoomKey(room) == "SPEAKER_NORMAL" => 1' in rooms
    assert 'RoomKey(room) == "ARENAG_NORMAL" => Math.Max(1, furniture / 6)' in rooms
    assert 'need is "ARENA" or "ARENAG" or "SPEAKER" or "STAGE"' in citizens
    assert "BuildKind.ActivityWork" in citizens
    asylum = text("Scripts/Rooms/AsylumRuntime.cs")
    assert "FoodMaximum = 15" in asylum
    assert "BuildJob.AsylumFoodSupply" in asylum and "WorkLeft = 25f" in jobs
    assert "0.25 + 0.75 * (1.0 - instance.Degradation) * guards" in asylum
    assert "TryAdmit" in asylum and "TryFeed" in asylum and "Release" in asylum
    assert "TryAdmitCitizen" in asylum and "TryAdmitAsylum" in citizens
    cannibal = text("Scripts/Rooms/CannibalRuntime.cs")
    assert "HarvestResources" in cannibal and "Cannibalism" in cannibal
    assert "WorkLeft = 45f" in jobs and 'lawCase.Punishment == "HARVEST"' in \
        text("Scripts/Law/SettlementLawRuntime.cs")
    world_map = text("Scripts/UI/GlobalMapOverlay.cs")
    inspector = text("Scripts/UI/SettlementInspectorPanel.cs")
    bootstrap = text("Scripts/Bootstrap/GameBootstrap.cs")
    assert "Image.CreateEmpty" in world_map and "ImageTexture.CreateFromImage" in world_map
    assert "SettlementMapLayer.Fertility" in world_map and "SettlementMapLayer.Minerals" in world_map
    assert "SettlementMapLayer.Homeless" in world_map and "SettlementMapLayer.Noise" in world_map
    assert "EnvironmentEmits" in world_map and "emit.Radius * 15" in world_map
    assert "InfrastructureRevision" in world_map and "SetSelectedCell" in world_map
    assert "CitizenInspectionSnapshot" in inspector and "RecipeDescription" in inspector
    assert "SettlementInspectorPanel" in bootstrap and "TimeControlBar" in bootstrap
    right_sidebar = text("Scripts/UI/SettlementRightSidebar.cs")
    assert "UIMiniResources" in right_sidebar and "UIMinimapPanel" in right_sidebar
    assert "PopulationByRace" in right_sidebar and "Enum.GetValues<ResourceKind>()" in right_sidebar
    assert "ForEachMiniMapCitizen" in right_sidebar and "RaceColor" in right_sidebar
    assert "CellSelected" in right_sidebar and "InfrastructureRevision" in right_sidebar
    assert "SettlementRightSidebar" in bootstrap and "_rightSidebar.UpdateState" in bootstrap
    assert "OverlayRequested" in right_sidebar and "Снимок всей карты" in right_sidebar
    assert "HideUiRequested" in right_sidebar and "Кинематографический режим" in right_sidebar
    assert "MaximumHotspots = 32" in right_sidebar and "HotspotPlacementRequested" in right_sidebar
    assert "MoveHotspot" in right_sidebar and "DeleteHotspot" in right_sidebar
    assert "OriginalUiIcons.Small(81)" in right_sidebar and "OriginalUiIcons.Medium(9)" in right_sidebar
    assert "if (!_uiLayer.Visible)" in bootstrap and "_uiLayer.Visible = true" in bootstrap
    assert "_rightSidebar.AddHotspot(hotspotCell)" in bootstrap
    top_bar = text("Scripts/UI/SettlementTopBar.cs")
    assert "UIPanelTopSett" in top_bar and "PopulationByClass" in top_bar
    assert "FoodDays" in top_bar and "OriginalUiIcons.Small" in top_bar
    assert "CitizensRequested" in top_bar and "TechnologyRequested" in top_bar
    assert "OriginalUiIcons.Small(17)" in top_bar
    assert "OriginalUiIcons.Medium(99)" in top_bar and "OriginalUiIcons.Medium(106)" in top_bar
    assert "OriginalUiIcons.MainCategory(12)" in top_bar
    assert "OriginalUiIcons.MainCategory(13)" in top_bar
    assert "OriginalUiIcons.MainCategory(19)" in top_bar
    assert "OriginalUiIcons.MainCategory(11)" in top_bar
    assert "SettlementTopBar" in bootstrap and "_topBar.UpdateState()" in bootstrap
    assert "OpenAdministration(_administration.ShowLaw)" in bootstrap
    civic = text("Scripts/UI/SettlementCivicPanel.cs")
    manager = text("Scripts/UI/SettlementManagerPanel.cs")
    assert "UINobles" in civic and "UIArmy" in civic and "UIHomes" in civic
    assert "OpenNobles" in civic and "OpenArmy" in civic and "OpenHealth" in civic
    assert "AppointNoble" in civic and "EnlistCitizen" in civic and "SetTarget" in civic
    assert "view.ui.manage.IManager" in manager and "OpenGoods" in manager
    raiding = text("Scripts/World/WorldRaidingRuntime.cs")
    assert "Amount = 100" in raiding and "PopulationThreshold = 200" in raiding
    assert "PopulationRaiderWorth = 600" in raiding and "RaidIntervalDays = 6 * 16" in raiding
    assert "Math.Pow(fraction, 2.1)" in raiding and "Math.Pow(fraction, 2.75)" in raiding
    assert "RaidMercenary" in raiding and "StatsVisible" in raiding
    assert "WorldRaidingRuntime" in top_bar and "_raiding.Active" in top_bar
    assert "RaiderVisibility.AtLarge" in manager and "Показывать всех" in manager
    assert "_raiding.Initialize(configuration.WorldSeed" in bootstrap
    assert "SettlementCivicPanel" in bootstrap and "OpenCivic" in bootstrap
    time_controls = text("Scripts/UI/TimeControlBar.cs")
    assert "GetViewportRect().Size.X - Size.X" in time_controls and '"»»»"' in time_controls
    assert "AcceptEvent()" in world_map
    palette = text("Scripts/UI/RoomBuildPalette.cs")
    planner = text("Scripts/Rooms/RoomPlanner.cs")
    assert "RoomBlueprintCatalog" in palette and "_categoryColumn" in palette
    assert "_roomColumn" in palette and '"Переработка"' in palette
    assert 'private string _currentSub = "";' in palette
    assert "_roomScroll.Visible = false" in palette
    assert "button.MouseEntered += () => SelectSubcategory" in palette
    assert 'new[] { "REFINER_" }' in palette
    assert "OriginalUiIcons.Room(room.Key)" in palette
    assert "ScrollContainer" in palette and "VerticalScrollMode" in palette
    assert "RowHeight = 44f" in palette and "ColumnWidth = 350f" in palette
    assert "RoomPlacementRuntime" in planner and "SelectDefinition" in planner
    assert "RoomBuildPalette" in bootstrap and "SelectRoomDefinition" in bootstrap
    assert "SetDefinitionItemAmount" in planner and "DefinitionCost" in planner
    assert "SetDefinitionUpgrade" in planner and "CanSetDefinitionUpgrade" in planner
    assert "occupiedCells.Any(occupied => !_area.Contains(occupied)" in text("Scripts/Rooms/RoomPlacementRuntime.cs")
    assert "RoomFurniture" in jobs and "BuildJob.RoomFurniture" in rooms
    assert "State = JobState.Dormant" in jobs
    assert "IEnumerable<GridCoord>? footprint" in jobs
    assert "RestoreFurnitureCells" in jobs
    assert "foreach (var cell in agent.Job.FurnitureCells)" in citizens
    assert "job.OccupiedCells" in text("Scripts/Simulation/JobBoard.cs")
    assert "State = (byte)job.State" in bootstrap
    assert "job.State = (JobState)savedJob.State" in bootstrap
    assert "ConstructionDeliveryCell" in rooms
    icons = text("Scripts/UI/OriginalUiIcons.cs")
    assert "IconBlock" in icons and "IconLayer" in icons and "Compose(" in icons
    composite_room_icons = [path for path in room_files
                            if re.search(r"(?ms)^\s*ICON\s*:\s*\{.*?^\s*\}\s*,?",
                                         path.read_text(encoding="utf-8-sig"))]
    assert len(composite_room_icons) == 30
    assert "sourceSize = 32" in icons and 'sheet.Contains(\'/\')' in icons
    reference = text("Scripts/UI/SettlementReferencePanel.cs")
    assert "OpenWorldLog" in reference and "OpenAdvice" in reference and "OpenWiki" in reference
    assert "RefreshFurnisherControls" in bootstrap and "FurnisherItemNames" in bootstrap
    assert "PreviewSelection" in bootstrap and "InputEventMouseMotion" in bootstrap
    assert "ToggleFurniture" in bootstrap and "DefinitionFurniture" in planner
    assert "CancelDraft" in planner and "Чертёж комнаты отменён" in bootstrap
    assert "BuildTool.RoomShrink" in bootstrap and "Отменить последнее изменение" in bootstrap
    layout_catalog = text("Scripts/Rooms/FurnisherLayoutCatalog.cs")
    assert 'StartsWith("REFINER_") => "industry/refiner"' in layout_catalog
    assert 'StartsWith("WORKSHOP_") => "industry/workshop"' in layout_catalog
    assert 'StartsWith("MINE_") => "industry/mine"' in layout_catalog
    layouts = text("Data/Original/furnisher_layouts.tsv").splitlines()
    assert len(layouts) == 714
    assert any(line.startswith("industry/refiner\t") for line in layouts)
    assert any(line.startswith("industry/workshop\t") for line in layouts)
    assert sum(line.startswith("industry/workshop\t0\t") for line in layouts) == 18
    assert sum(line.startswith("home/house\t") for line in layouts) == 54
    assert '"_HOME" => "home/house"' in layout_catalog
    assert "UsesFixedItemPlacement" in planner and "PlaceFixedFurniture" in planner
    assert "SetFixedItem" in text("Scripts/Rooms/RoomPlacementRuntime.cs")
    ground = text("Scripts/Settlement/GridWorld.cs")
    terrain_texture = text("Scripts/Rendering/OriginalSettlementTerrainTextureBuilder.cs")
    assert "OriginalSettlementTerrainTextureBuilder.Build" in ground and "Image.CreateFromData" in ground
    assert 'MapRoot + "Ground.png"' in terrain_texture
    assert 'MapRoot + "Mountain.png"' in terrain_texture
    assert 'MapRoot + "Tree.png"' in terrain_texture and "size = 3" in terrain_texture
    assert 'TextureRoot + "Water.png"' in terrain_texture and "SmoothFlag" in terrain_texture
    assert "TextureFilterEnum.Nearest" in ground
    assert "if (ticks > 0)" in bootstrap and "RefreshRuntimeUi" in bootstrap
    assert "_uiRefreshAccumulator >= 0.25" in bootstrap
    janitor = text("Scripts/Rooms/JanitorRuntime.cs")
    assert "Radius = 150" in janitor and "CounterMaximum = 31" in janitor
    assert "TargetMaximum = CounterMaximum - 3" in janitor
    assert "Math.Clamp((int)(Math.Max(0, employed) * Math.Max(0, globalEstimate)), 4, TargetMaximum)" in janitor
    assert "BuildJob.JanitorSupply" in janitor and "CompleteJanitorSupply" in citizens
    assert 'RoomKey(room) == "_JANITOR"' in rooms and "Math.Ceiling(maximum / 5.0)" in rooms
    assert "PunishmentMaximum" in activity and "? 4" in activity and "? 1" in activity
    assert "TryReservePunishment" in activity and "CancelPunishment" in activity
    assert "CompletePunishment" in activity and "asylums.Release(subjectId)" in activity
    religion_runtime = text("Scripts/Citizens/ReligionRuntime.cs")
    assert "SettlementOpposition" in religion_runtime
    assert "other.Value / (double)population * Opposition(religion.Key, other.Key)" in religion_runtime
    settlement_stats = text("Scripts/Stats/SettlementStatsRuntime.cs")
    assert "ReligionFollowers" in settlement_stats and "ReligiousOpposition" in settlement_stats
    housing = text("Scripts/Rooms/HousingRuntime.cs")
    assert 'key.Equals("_HOME"' in housing and '"_HOME_CHAMBER"' in housing
    assert "room.Employment.Employed >= 4 ? 1 : 0" in housing
    assert 'RoomKey(room) == "_HOME_CHAMBER" => 4' in rooms
    assert "HomeContructor.maxOccupants" in housing
    assert "1 when pair.Key == 2 => 14" in housing
    assert ">= 2 when pair.Key == 2 => 18" in housing
    assert "TryOccupy" in housing and "Vacate" in housing
    assert "_rooms.Housing.TryOccupy(agent.Id, agent.Cell," in citizens
    assert "HousingAccess" in settlement_stats and "rooms.Housing.Occupants" in settlement_stats
    assert "HomeFurniture" in race_data and "ReadHomeFurniture" in race_data
    assert "HomeResidentRuntime" in housing and "SetTarget" in housing
    assert "FurnitureFulfillment" in housing and "TickDay" in housing
    assert "BuildJob.HomeFurnitureSupply" in housing
    assert "CompleteHomeFurnitureSupply" in citizens
    assert "ReturnToStock" in text("Scripts/Resources/ResourceLedger.cs")
    home_behavior = text("Scripts/Citizens/HomeBehaviorRuntime.cs")
    assert "enum HomeActivity" in home_behavior
    assert "GroundSearchDistance = 64" in home_behavior
    assert "CurfewGroundSearchDistance = 256" in home_behavior
    assert "ShouldVisitHome" in home_behavior and "StaySeconds" in home_behavior
    assert "TickHomeBehavior" in citizens and "HasSleptToday" in citizens
    burial_runtime = text("Scripts/Rooms/BurialRuntime.cs")
    assert "ReserveMourning" in burial_runtime and "radius = 500" in burial_runtime
    assert "MourningReserved" in burial_runtime and "CompleteMourning" in burial_runtime
    assert "4 + (int)(GD.Randi() % 8)" in citizens
    assert "4 + Count / 1000" in citizens and "TryStartMourning" in citizens
    personal = text("Scripts/Citizens/CitizenPersonalStatsRuntime.cs")
    assert "TrainingMaximum = 15" in personal
    assert "EducationMaximum = 100" in personal and "TraitMaximum = 15" in personal
    assert "profile.EnemyKills + 1" in personal
    assert "profile.CombatExperience + 1 + (int)(GD.Randi() % 4)" in personal
    assert "EducationTrack.Indoctrination" in personal and "_educationRemainder" in personal
    assert "PersonalStats.AverageEducation" in settlement_stats
    prepared = text("Scripts/Rooms/PreparedServiceRuntime.cs")
    assert '"BARBER_NORMAL", StringComparison.OrdinalIgnoreCase) ? 16 : 1' in prepared
    assert '"PHYSICIAN_NORMAL", StringComparison.OrdinalIgnoreCase) ? 7 :' in prepared
    assert '"BARBER_NORMAL", StringComparison.OrdinalIgnoreCase) ? 7.2f :' in prepared
    assert "prepared.Prepared < service.Total" in prepared
    assert "BuildJob.ServicePreparation" in prepared
    assert "CompleteServicePreparation" in citizens
    assert 'need.Equals("DOCTOR"' in citizens and "return 25f" in citizens
    assert 'need.Equals("GROOMING"' in citizens and "return 15f" in citizens
    assert 'need.Equals("MASSAGE"' in citizens and "return 6f" in citizens
    assert "PLEASURE_NORMAL" in prepared and "57.6f" in prepared
    venues = text("Scripts/Rooms/FoodVenueRuntime.cs")
    assert "CANTEEN_NORMAL" in venues and "EATERY_NORMAL" in venues and "TAVERN_NORMAL" in venues
    assert "service.SuspendAvailable()" in venues
    assert "BuildJob.VenueSupply" in venues
    assert "ResourceKind.Beer" in venues and "ResourceKind.Wine" in venues
    assert "FoodVenues.Schedule" in rooms and "CompleteVenueSupply" in rooms
    assert "TryStartMealService" in citizens and "agent.FoodPlan == 4" in citizens
    assert "agent.FoodPlan is 2 or 5" in citizens
    assert "CANTEEN_" in original_data and 'return "HUNGER"' in original_data
    stats = text("Scripts/Stats/SettlementStatsRuntime.cs")
    assert "FoodDays" in stats and "edibleStored / dailyFood" in stats
    assert "EdibleResources" in stats and "FoodVenues.Instances" in stats
    assert "Unburied = corpses.UnburiedPressure(Population)" in stats
    assert "rooms.Burials.AvailableFormal" in stats and "BurialDisturbance" in stats
    assert "StoredPerCapita" in stats and "ServiceAccess" in stats
    assert "_settlementStats.Tick(" in text("Scripts/Bootstrap/GameBootstrap.cs")
    activity = text("Scripts/Rooms/ActivityServiceRuntime.cs")
    assert "IsOpen" in activity and "ActiveWorkers" in activity
    assert "ServiceDuration" in citizens and 'need is "ARENA" or "ARENAG" or "SPEAKER" or "STAGE"' in citizens
    assert "NeedRates" in original_data and "UpdateNeeds(agent)" in citizens
    assert "RequiredWorkers" in rooms and "RebalanceProfessions" in citizens
    assert "LoadDiseases" in original_data and "DiseaseRule" in original_data
    health = text("Scripts/Citizens/CitizenHealthRuntime.cs")
    assert "None, Incubating, Sick, Immune" in health
    assert "InjuryDanger = InjuryMaximum / 2" in health
    assert "InjuryCritical = 3 * (InjuryMaximum / 4)" in health
    assert "Math.Min(InjuryMaximum, Injury + 4)" in health
    assert "Math.Ceiling(1 + 15 * Math.Max(0, health))" in health
    assert "disease.IncubationDays * 16" in health
    assert "RegularSicknessDayInterval * (1 + Math.Max(health, 0))" in health
    assert "TryStartHospital" in citizens
    assert "1.0 - 0.8 / quality" in rooms
    assert "StartEpidemic" in citizens
    assert "disease.IncubationDays + disease.InfectionDays" in citizens
    assert "HospitalBedResourceMaximum = 7" in rooms
    assert "BuildJob.MaximumFetchAmount" in rooms
    assert "CompleteHospitalSupply" in rooms
    assert "ConsumeHospitalSupplies" in rooms
    assert "BuildKind.HospitalSupply" in citizens
    assert "BuildJob.HospitalSupply" in rooms
    corpse = text("Scripts/Citizens/CorpseRuntime.cs")
    assert "PlanBuryCorpse" in corpse
    assert "corpse.Reserved" in corpse and "corpse.PickedUp" in corpse
    assert "BuildJob.CorpseHaul" in corpse
    assert "_rooms.Burials.Reserve(corpse.Cell)" in corpse
    assert "_corpses.Create(agent.Cell" in citizens
    assert "_corpses.Pickup(agent.Job)" in citizens
    assert "_corpses.Deliver(agent.Job)" in citizens
    assert "MaximumCorpses = 2048 * 4" in corpse
    assert "corpse.Decay += 0.05 * GD.Randf()" in corpse
    assert "40.0 * amount / (1.0 + Math.Max(0, population))" in corpse
    burial = text("Scripts/Rooms/BurialRuntime.cs")
    assert "Graveyard => 20" in burial
    assert "Tomb => 40" in burial
    assert "_ => 16" in burial
    assert "slot.Available && slot.Formal" in burial
    assert "slot.Available && !slot.Formal" in burial
    assert "slot.Formal ? 25f : 0.1f" in burial
    assert "secondsPerDay) * 16.0" in burial
    sanitation = text("Scripts/Rooms/SanitationRuntime.cs")
    assert "MaximumUses = 4" in sanitation
    assert "CleaningThreshold = 3" in sanitation
    assert "CleaningSeconds = 45f" in sanitation
    assert "BuildJob.Sanitation" in sanitation
    assert "SuspendAvailable" in services and "RestoreAvailable" in services
    assert "BuildKind.Sanitation" in citizens
    accidents = text("Scripts/Citizens/WorkAccidentRuntime.cs")
    assert "candidate.Employed - 150.0" in accidents
    assert "Math.Pow(employed, 1.2)" in accidents
    assert "SecondsPerDay * 16.0" in accidents
    assert "relative < 0.1 || relative > 0.6" in accidents
    assert "_timers[candidate.Key] >= -10" in accidents
    assert "const double radius = 20.0" in citizens
    assert "falloff * GD.Randf() * 2.0" in citizens
    assert "damage * GD.Randf() > 1" in citizens
    assert 'KillFromStarvation(agent, resources, jobs, "ACCIDENT")' in citizens
    assert "public bool IsHaulJob" in jobs
    assert "ReleaseOutputReservations" in jobs and "job.ReleaseOutputReservations()" in board
    positions = text("Scripts/Rooms/RoomJobPositionsRuntime.cs")
    assert "ReportResourceMissing" in positions
    assert "ResourceShouldSearch" in positions and "ResourceReachable" in positions
    assert "Math.Abs(position.X - cell.X) + Math.Abs(position.Z - cell.Z) < 5" in positions
    assert "InternalStorage.Create" in rooms
    assert "Blueprints.Synchronize(_rooms)" in rooms
    assert "DefinitionKey" in rooms
    bootstrap = text("Scripts/Bootstrap/GameBootstrap.cs")
    assert "new WorkAccidentRuntime(_rooms, _citizens)" in bootstrap
    assert "_workAccidents.Tick(" in bootstrap
    assert "BuildJob.Road(cell, SelectedRoad())" in bootstrap
    assert "save.Version is < 1 or > 37" in bootstrap
    world_armies = text("Scripts/World/WorldArmyRuntime.cs")
    assert "MenPerDivision = 200" in world_armies
    assert "DivisionsPerArmy = 120" in world_armies
    assert "WorldArmyState.Fortifying" in world_armies
    assert "WorldEntityKind.Army" in world_armies
    settlement_world = text("Scripts/Settlement/SettlementWorldRuntime.cs")
    assert "WorldArmyRuntime Armies" in settlement_world
    assert "_armies.Tick(days)" in settlement_world
    assert "_armies.Capture()" in settlement_world and "_armies.Restore" in settlement_world
    world_battles = text("Scripts/World/WorldBattleRuntime.cs")
    assert "PFieldBattle, PRegAttack, PSiege, Resolver" in world_battles
    assert "WorldBattleKind : byte { Field, Garrison, Siege }" in world_battles
    assert "AutoValue" in world_battles and "1.1 * fraction" in world_armies
    assert "FindRetreatRegion" in world_battles and "distance >= 8" in world_battles
    assert "ConquerRegion" in world_battles and "DisbandFaction" in world_armies
    strategic_war = text("Scripts/World/StrategicWorldRuntime.cs")
    assert "StrategicConquestResult" in strategic_war and "CapitalRelocated" in strategic_war
    assert "WorldBattleRuntime Battles" in settlement_world
    assert "_battles.Tick()" in settlement_world
    assert "_battles.Capture()" in settlement_world and "_battles.Restore" in settlement_world
    world_entities = text("Scripts/World/WorldEntityRuntime.cs")
    assert "NormalizeRoute" in world_entities and "normalized[0] = origin" in world_entities
    battle_ui = text("Scripts/UI/WorldBattlePanel.cs")
    assert "WorldBattleDecision.Retreat" in battle_ui
    assert "WorldBattleDecision.AutoResolve" in battle_ui
    assert "WorldBattleDecision.DefendSettlement" in battle_ui
    assert "_battlePanel.Open()" in bootstrap and "UpdateBattlePause" in bootstrap
    assert "SettlementInvasion" in world_battles and "ActiveSettlementInvasion" in world_battles
    invasion = text("Scripts/Military/SettlementInvasionRuntime.cs")
    assert "Invador/Invasion/SpotMaker" in invasion
    assert "secondsPerDay / 8.0" in invasion and "secondsPerDay / 2.0" in invasion
    assert "secondsPerDay * 5.0" in invasion and "distance < 12" in invasion
    assert "WorldArmyRuntime.MenPerDivision" in invasion
    assert "HierarchicalGridPathfinder.FindPath" in invasion
    assert "_invasion.Tick" in bootstrap and "Invasion = _invasion.Capture()" in bootstrap
    assert "_invasion.Restore(save.Invasion)" in bootstrap
    military_battle = text("Scripts/Military/SettlementMilitaryRuntime.cs")
    assert "SettlementDefenseStrength" in military_battle
    assert "Power.HIGH_POWER = 5.0" in military_battle
    assert "DefenseStrength" in invasion and "ResolveDefenders" in invasion
    assert "KillForEvent(casualties" in invasion and '"BATTLE"' in invasion
    assert "PersonalStats.RecordKill" in invasion and "AutoValue" in invasion
    assert "SettlementInvasionPhase.AwaitingPrisoners" in invasion
    assert "ResolvePrisoners" in invasion and "Asylums.TryAdmit()" in invasion
    assert "DivisionBattleTask.Stop" in military_battle
    assert "AttackBuilding, AttackMelee, AttackRanged, Charge" in military_battle
    assert "DivisionFormation : byte { Tight, Loose }" in military_battle
    assert "Принять пленных" in battle_ui and "ResolvePrisoners" in battle_ui
    settlement_battle_ui = text("Scripts/UI/SettlementBattleCommandPanel.cs")
    assert "BattlePanel, DivSelection and UISelection" in settlement_battle_ui
    assert "DivisionFormation.Tight" in settlement_battle_ui
    assert "DivisionFormation.Loose" in settlement_battle_ui
    assert "DivisionBattleTask.AttackMelee" in settlement_battle_ui
    assert "DivisionBattleTask.Charge" in settlement_battle_ui
    assert "IssueMilitaryFormation" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "StopMilitaryOrders" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "_battleCommands.AwaitingMapOrder" in bootstrap
    assert "ResolveTacticalCombat" in invasion and "enemyArmyMorale < 0.2" in invasion
    assert "2.0 * divisionCasualties" in invasion and "0.5 * armyCasualtyPart" in invasion
    assert "ClosestExit" in invasion and "DivisionFormation.Loose" in invasion
    assert "MilitaryCentroid" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "BattleCasualties" in military_battle and "public double Morale" in military_battle
    assert "EnemyCohortAt" in invasion and "TargetDivisionId" in invasion
    assert "var range = ranged ? 122 : 2" in invasion
    assert "flankDefense" in invasion and "formationDefense" in invasion
    assert "ConsumeAmmunition" in military_battle and "AmmunitionMaximum" in military_battle
    assert "AMMUNITION_AMOUNT: 40" in text("Data/Original/init/stats/equip/ranged/BOW.txt")
    assert "SpawnProjectile" in invasion and "_projectileMesh" in invasion
    assert "SetArtilleryTarget" in military_battle and "ClearArtilleryTarget" in military_battle
    assert "MusterArtillery" in settlement_battle_ui and "Орудие #" in settlement_battle_ui
    assert "ResolveArtillery" in invasion and "DamageStructure" in invasion
    assert "Battle.DAMAGE_REDUCTION = 100" in invasion
    assert "division.AmmunitionUsed = 0" in military_battle
    assert "TraceProjectile" in invasion and "FRIENDLY_FIRE" in invasion
    assert "GridLine" in invasion and "TileFlags.Mountain" in invasion
    assert "ResolveBuildingAttacks" in invasion and "AttackBuilding" in invasion
    assert "RemoveFurniture(cell)" in invasion and "Dismantle(destroyed.Id" in invasion
    assert "buildingTarget" in settlement_battle_ui and "buildingTarget" in bootstrap
    assert "DivisionCombatProfile" in military_battle and "RangedAccuracy" in military_battle
    assert "ResourceKind.ArmourPlate" in military_battle and "ResourceKind.WeaponShield" in military_battle
    assert "TickBattleCondition" in invasion and "division.Exhaustion" in military_battle
    assert "MilitarySpeedMultiplier" in text("Scripts/Citizens/CitizenSystem.cs")
    assert "taskSpeed = division.Order.Task == DivisionBattleTask.Charge ? 0.9 : 0.7" in bootstrap
    assert "MutableTerrain = _world.Data.CaptureMutableState()" in bootstrap
    assert "RestoreMutableState(save.MutableTerrain)" in bootstrap
    assert "AlreadyAccounted = item.AlreadyAccounted" in bootstrap
    grid_data = text("Scripts/Settlement/WorldGridData.cs")
    assert "CaptureMutableState" in grid_data and "RestoreMutableState" in grid_data
    assert "public bool HasRoof" in grid_data and "public void SetRoof" in grid_data
    assert "Roof = (byte[])_roof.Clone()" in grid_data
    special_production = text("Scripts/Rooms/SpecialProductionRuntime.cs")
    assert "SpecialProductionSnapshot Capture()" in special_production
    assert "void Restore(SpecialProductionSnapshot snapshot)" in special_production
    assert "SpecialProduction = _rooms.SpecialProduction.Capture()" in bootstrap
    assert "_rooms.SpecialProduction.Restore(save.SpecialProduction)" in bootstrap
    assert "TryPlaceLandingParty" in bootstrap
    assert "InitializeLandingParty(supplies.Select" in bootstrap
    assert "LandingSupplies(origin, landingBoost)" in bootstrap and "R1..RA positions" in bootstrap
    assert "LandingBaseCredits = 5000" in bootstrap
    assert 'TitleBonuses.Apply("CIVIC_LANDING", 0)' in bootstrap
    assert "LandingBaseCredits * landingBoost" in bootstrap
    assert "var subjects = 10 + (int)(10 * landingBoost)" in bootstrap
    assert "rule.Amount + (int)(rule.Amount * landingBoost)" in bootstrap
    assert "SetRoof(cell, true)" in bootstrap and "TileFlags.Cave" in bootstrap
    original_data = text("Scripts/Data/OriginalGameData.cs")
    assert "LoadLandingParty" in original_data and "LandingParty.txt" in original_data
    assert "IReadOnlyList<LandingResourceRule> LandingResources" in original_data
    landing_config = text("Data/Original/init/config/LandingParty.txt")
    assert number(landing_config, "STONE") == 60
    assert number(landing_config, "WOOD") == 100
    assert number(landing_config, "RATION") == 40
    assert number(landing_config, "LIVESTOCK") == 16
    assert number(landing_config, "FRUIT") == 60
    assert number(landing_config, "VEGETABLE") == 60
    assert "SpawnAccounted" in bootstrap
    assert "(1, 8), (7, 8), (1, 9), (2, 9), (6, 9)" in bootstrap
    assert "if (_landingPending) return;" in bootstrap
    assert "HasOceanEdge(world, region" in text("Scripts/Settlement/SettlementTerrainGenerator.cs")
    hauling = text("Scripts/Hauling/HaulingSystem.cs")
    assert "bool AlreadyAccounted" in hauling
    assert "TakeAccounted" in hauling
    assert "FindAccounted" in hauling
    assert "if (!job.OutputAlreadyAccounted)" in hauling
    assert "CaptureConstructionTransit" in bootstrap
    assert "_citizens.BeginFrame()" in bootstrap
    assert "RestoreInTransit" in bootstrap
    assert "RestoreContents" in bootstrap
    assert "ConstructionBatch" in citizens
    assert "ClaimAdjacentMaterialBatch" in board
    assert "ClearingTerrain" in jobs and "ClearingVegetation" in jobs
    assert "RemovingObstacle" in jobs
    assert "ClearVegetationStep" in text("Scripts/Settlement/WorldGridData.cs")
    industry = text("Scripts/Industry/IndustryRuntime.cs")
    assert "1.0 - 0.75 * degrade" in industry
    assert "(int)progress - before" in industry
    employment = text("Scripts/Rooms/RoomEmploymentRuntime.cs")
    assert "0.75 * measuredProximity + 0.25 * ProximityEfficiency" in employment
    assert number(bakery, "SHIFT_OFFSET") == 0.125
    assert number(carpenter, "SHIFT_OFFSET") == 0.325
    assert "IsWorkTime(room, dayFraction)" in rooms
    maintenance = text("Scripts/Maintenance/MaintenanceRuntime.cs")
    assert "TilesPerDay = 1.0 / 48.0" in maintenance
    assert "ResourceRate = 1.0 / 64.0" in maintenance
    assert "MinimumJobs = 4.0" in maintenance
    tools = text("Data/Original/init/resource/work/TOOL.txt")
    assert number(tools, "WEAR_PER_DAY") == 0.04
    assert number(tools, "BOOST_MAX_VALUE") == 2.0
    assert number(tools, "DEFAULT_TARGET") == 0
    assert "BuildKind.Maintenance" in rooms
    assert "BuildKind.EquipmentSupply" in rooms
    road_maintenance = text("Scripts/Maintenance/RoadMaintenanceSystem.cs")
    assert "0.25 * MaintenanceRuntime.ResourceRate * road.ResourceAmount" in road_maintenance
    assert "(1.0 - road.Durability)" in road_maintenance
    assert "1 + (int)(GD.Randi() % 3)" in road_maintenance
    assert "GD.Randf() * total < resourcePart" in road_maintenance
    assert "GrowVegetation(cell, 1 + (int)(GD.Randi() % 2))" in road_maintenance
    assert "3 + (int)(GD.Randi() % 2)" in road_maintenance
    consumption = text("Scripts/Maintenance/MaintenanceConsumption.cs")
    assert "EstimateGlobalRaw" in consumption
    assert "AccumulateExpectedDailyResourceUse" in consumption
    assert "AccumulateExpectedDailyMaintenanceUse" in consumption
    world_data = text("Scripts/Settlement/WorldGridData.cs")
    assert "RoadDegradation(cell) < 15" in world_data
    assert "Math.Clamp(value, 0, 15)" in world_data
    assert "BuildJob.RoadMaintenance" in road_maintenance
    assert "agent.Job.ResetResourcePickup()" in citizens
    assert "agent.Job.DeliverMaintenanceResource()" in citizens
    dirt_wear = (1.0 / 48.0) * (1.0 - 0.2)
    resource_road_wear = (1.0 / 48.0 + 0.25 * (1.0 / 64.0) * 2) * (1.0 - 0.5)
    assert abs(dirt_wear - 1.0 / 60.0) < 1e-12
    assert abs(resource_road_wear - 11.0 / 768.0) < 1e-12
    logistics = text("Scripts/Rooms/SettlementLogisticsRuntime.cs")
    trade = text("Scripts/Trade/SettlementTradeRuntime.cs")
    assert "crates * Math.Max(1, (int)Math.Round(boost) - 1)" in logistics
    assert "LogisticsKind.Transport" in logistics and "TransportPreparation < 576" in logistics
    assert "BuildJob.MaximumFetchAmount" in logistics
    assert "MarketResources" in logistics and "ConsumeMarket" in logistics
    assert "AvailableForExport" in logistics and "DeliverImport" in logistics
    assert "AveragePrice = 400" in trade
    assert "TollPerTile = 100.0 / 400.0" in trade
    assert "MaximumPlayerPrice = 1_000_000" in trade
    assert "if (!tradeOpen || _quotes.Count == 0) return" in trade
    assert "BuildKind.LogisticsTransfer" in citizens
    assert "BuildKind.TransportPreparation" in citizens
    assert "_hauling.CancelStorageReservation(job)" in bootstrap
    tech_files = list((ROOT / "Data/Original/init/tech").glob("*.txt"))
    assert len(tech_files) == 11
    knowledge = text("Scripts/Rooms/KnowledgeRuntime.cs")
    technology = text("Scripts/Technology/TechnologyRuntime.cs")
    assert "LoadTechnologies" in original_data and "TechnologyRule" in original_data
    assert "CIVIC_INNOVATION" in text("Data/Original/init/room/LABORATORY_NORMAL.txt")
    assert "CIVIC_KNOWLEDGE" in text("Data/Original/init/room/LIBRARY_NORMAL.txt")
    assert "WorkSeconds = 45" in knowledge
    assert "AdultLearningStep = 1.0 / 16.0" in knowledge
    assert "DaysPerYear = 16" in knowledge and "1.0 - progress * progress" in knowledge
    assert "ForgetThreshold = 0.8" in technology
    assert "100.0 / secondsPerDay" in technology
    assert "count * (count + 1) / 2" in technology
    assert "ValidateRequirements" in technology and "TransitiveRequirements" in technology
    assert "TryEnrollUniversity" in citizens
    assert "BuildKind.KnowledgeSupply" in citizens and "BuildKind.KnowledgeWork" in citizens
    assert "CanCreateRoom" in rooms and "CanSetUpgrade" in rooms
    law_config = text("Data/Original/init/config/LAW.txt")
    assert len(re.findall(r"(?m)^\s*[A-Z_]+\s*:\s*\{", law_config.split("PUNISHMENTS:", 1)[0])) - 1 == 12
    assert len(re.findall(r"(?m)^\s*[A-Z_]+\s*:\s*\{", law_config.split("PUNISHMENTS:", 1)[1])) == 7
    law = text("Scripts/Law/SettlementLawRuntime.cs")
    assert "GuardRadius = 90" in law and "PatrolMaximumTiles = 120" in law
    assert "CourtFreeRate = 0.2" in law and "PrisonSentenceDays = 32" in law
    assert "PrisonWorkersPerPrisoner = 0.25" in law
    assert "StockadePrisonersPerTile = 0.25" in law
    assert "ExecutionServicesPerStation = 8" in law
    assert "LawProcessKind.Hearing" in law and 'value.Punishment != "PRISON"' in law
    assert "Math.Pow(Math.Clamp(value, 0, 1), 1.5)" in law
    assert "LoadLaw" in original_data and "CrimeRule" in original_data and "PunishmentRule" in original_data
    assert "Law.Synchronize" in rooms and "Law.Schedule" in rooms
    assert "TryCommitDailyCrime" in citizens and "ProcessLawEffects" in citizens
    assert "ApplyCrime(agent, resources)" in citizens
    assert 'crime is "THEFT" or "S_THEFT"' in citizens
    assert 'crime == "VANDALISM"' in citizens and "VandalizeNearest" in rooms
    assert 'crime is not ("MURDER" or "S_MURDER")' in citizens
    assert "rooms.Law.Law(race, socialClass)" in settlement_stats
    assert "rooms.Law.Tyranny(race, socialClass)" in settlement_stats
    governance = text("Scripts/Governance/SettlementGovernanceRuntime.cs")
    assert "AnnualInflation = 0.2" in governance and "HistoryDays = 48" in governance
    assert "MaximumNobles = 256" in governance and "RankAllocationIncrease = 2" in governance
    assert "WorkersPerAllocation = 50" in governance
    assert "GovernorPointsPerAllocation = 20" in governance
    assert "PlayerProgressionRuntime" in governance and "SecondsPerDay" in governance
    assert "LoadPlayerProgression" in original_data and "NobleRankNames" in original_data
    assert len(list((ROOT / "Data/Original/init/player/level").glob("*.txt"))) == 14
    assert len(list((ROOT / "Data/Original/init/player/titles").glob("*.txt"))) == 34
    assert '"ADMIN_NORMAL" => "ADMINISTRATION"' in original_data
    assert '"_EMBASSY" => "DIPLOMACY"' in original_data
    assert "WorkProfession.Administrator" in citizens and "Governance.Synchronize" in rooms
    assert "AppointNoble" in citizens and "PromoteNoble" in citizens
    assert "rooms.Governance.Administration" in settlement_stats
    battle = text("Data/Original/init/config/Battle.txt")
    assert number(battle, "MEN_PER_DIVISION") == 200
    assert number(battle, "DIVISIONS_PER_ARMY") == 120
    military = text("Scripts/Military/SettlementMilitaryRuntime.cs")
    assert "MenPerDivision = 200" in military and "DivisionsPerArmy = 120" in military
    assert "BasicTrainingPerDay = 0.1" in military and "TrainingWorkSeconds = 45" in military
    assert "ArtilleryCrew = 6" in military and "SupplyRadius = 300" in military
    assert "SupplyCrateStorage = 80" in military
    assert "ConfigureEquipment" in military and "IssueMilitarySupply" in logistics
    assert "LoadProgress" in military and "Rule.ReloadSeconds" in military
    assert "MilitaryRoomRule" in original_data and "ReadMilitary" in original_data
    assert '"_MILITARY_SUPPLY" => new LogisticsRule("MILITARY_SUPPLY", 300, 2, 80' in original_data
    assert "BuildKind.MilitaryTraining" in citizens and "BuildKind.ArtilleryLoad" in citizens
    assert "EnlistCitizen" in citizens and "DischargeCitizen" in citizens
    assert "Military.Synchronize" in rooms and "Military.Schedule" in rooms
    assert "rooms.Military.Divisions.Count" in settlement_stats
    notifications = text("Scripts/UI/SettlementNotificationFeed.cs")
    room_policy = text("Scripts/UI/RoomPolicyPanel.cs")
    administration_ui = text("Scripts/UI/AdministrationDashboard.cs")
    palette = text("Scripts/UI/RoomBuildPalette.cs")
    assert "value.Sequence > _lastSequence" in notifications
    assert "SettlementEventKind.SerialKiller" in notifications
    assert "AdjustWorkerLimit" in room_policy and "CycleRecipe" in room_policy
    assert "AdjustToolTarget" in room_policy and "SetUpgrade" in room_policy
    assert "rooms.Law" in administration_ui and "rooms.Technologies" in administration_ui
    assert "rooms.Trade" in administration_ui and "rooms.Governance" in administration_ui
    assert "SetDecree" in administration_ui and "UnlockNext" in administration_ui
    assert "ConfigureImport" in administration_ui and "ConfigureExport" in administration_ui
    assert "PopulationByRaceAndClass" in administration_ui
    assert "InspectCitizens" in citizens and "InspectCitizen" in citizens
    assert "Page.Citizens" in administration_ui and "CitizenFocusRequested" in administration_ui
    assert "law.Cases.OrderByDescending" in administration_ui
    assert "GetSelectedItems" in administration_ui
    strategic = text("Scripts/World/StrategicWorldRuntime.cs")
    assert "AssignRegionTiles" in strategic and "BuildRegionAdjacency" in strategic
    assert "RegionIdAtTile" in strategic and "CenterTileX" in strategic
    strategic_ui = text("Scripts/UI/StrategicWorldMap.cs")
    world_general = text("Data/Original/init/world/config/General.txt")
    assert "TileDimension = 256" in strategic and "AverageRegionArea = 50" in strategic
    assert "MaximumFactions = 64" in strategic and "AverageRealmSize = 8" in strategic
    assert "TargetRegionArea" in strategic and "CapitalFootprintDimension + 2" in strategic
    assert "InitialRealmFill = 0.75" in strategic
    assert "DiplomacyStance.Trade" in strategic and "PublishTradeQuotes" in strategic
    assert "trade.ClearQuotes()" in strategic and "_markets" in strategic
    assert "StrategicWorldRuntime" in strategic_ui and "ApplyStance" in strategic_ui
    assert "DiplomacyStance.War" not in strategic_ui
    assert "case Key.F3: ToggleWindow(_strategicMap)" in bootstrap
    assert number(world_general, "TILE_DIMENSION") == 256
    assert number(world_general, "REGION_SIZE") == 50
    terrain = text("Scripts/World/StrategicTerrainRuntime.cs")
    assert "StrategicTerrainTemplate" in terrain and "template.Sample" in terrain
    assert "void Add(int sx, int sy, double weight)" in terrain
    assert "Heights[sx + sy * Width] / targetWidth" in terrain
    assert "var equator = Math.Clamp(latitude, 0.05, 0.95)" in terrain
    assert "enum StrategicTerrainEdit" in terrain and "ApplyEditCell" in terrain
    assert "FinishTerrainEditing" in strategic
    assert "RebuildRegionGeometry();" in strategic
    assert "RestoreTerrain(StrategicTerrainSnapshot snapshot" in strategic
    settlement_terrain = text("Scripts/Settlement/SettlementTerrainGenerator.cs")
    settlement_generation = text("Data/Original/init/config/GenerationSettlement.txt")
    assert "SettlementGeneratorSettings.Load" in settlement_terrain
    assert "SettlementWorldTileSample" in settlement_terrain
    assert "WorldTileDimension = StrategicWorldRuntime.CapitalFootprintDimension" in settlement_terrain
    assert "WorldTileAtSettlement" in settlement_terrain
    assert "GenerateMappedFreshWater" in settlement_terrain and "GenerateMappedOcean" in settlement_terrain
    assert "mappedMoisture * 0.82" in settlement_terrain
    assert "sample => sample.Mountain ? 1.0 : 0.0" in settlement_terrain
    assert "mappedForest * 1.25" in settlement_terrain
    assert "SampleWorld" in settlement_terrain and "Exact centre-weighted interpolation" in settlement_terrain
    assert "GenerateRoads(world, profile)" in settlement_terrain
    assert "sample.Road" in settlement_terrain and "PaintGeneratedRoadLine" in settlement_terrain
    assert settlement_terrain.count("GenerateMinerals(world, profile, settings);") == 1
    assert "GenerateMountains(world, profile, settings, polymap)" in settlement_terrain
    assert "GenerateCaves(world, profile, settings)" in settlement_terrain
    assert "settings.CaveAmount * 300" in settlement_terrain
    assert "settings.CaveSize * 30" in settlement_terrain
    assert "SettlementWaterSides RiverSides" in settlement_terrain
    assert "SettlementWaterSides SmallRiverSides" in settlement_terrain
    assert "GenerateRiver(world, profile, settings, profile.RiverSides" not in settlement_terrain
    assert "GenerateRiver(world, profile.RiverSides" in settlement_terrain
    assert "GenerateLakeExtra(world, profile)" in settlement_terrain
    assert "distance < 4" in settlement_terrain and "DistancePass" in settlement_terrain
    assert "world.Width, world.Height) / 3.0 / 2.2" in settlement_terrain
    assert "FloodWaterComponent" in settlement_terrain and "weighted / 200.0" in settlement_terrain
    assert "GenerateBeachBand" in settlement_terrain
    assert "GrowMineralDeposit" in settlement_terrain and "PriorityQueue" in settlement_terrain
    assert "38 + 2000 * meanAffinity" in settlement_terrain
    assert "GrowEdiblePatch" in settlement_terrain and "EdibleSuitability" in settlement_terrain
    assert "4000 / profile.Growables.Count" in settlement_terrain
    assert "GridCoord.AllDirections" in settlement_terrain
    assert "Sand, Infertile, Pasture" in terrain_grid
    assert number(settlement_generation, "CAVE_AMOUNT") == 0.16
    assert number(settlement_generation, "CAVE_SIZE") == 0.5
    assert number(settlement_generation, "CAVE_TUNNELS") == 0.2
    assert number(settlement_generation, "MOUNTAIN_SIZE") == 0.2
    assert number(settlement_generation, "RIVER_WIDTH") == 10
    regional = text("Scripts/World/RegionalEconomyRuntime.cs")
    assert "PopulationCapacityPerTile = 100" in regional
    assert "EntityMaximum = 40000" in regional
    assert "InitialPopulationCapacity" in regional and "PrimePopulation" in regional
    assert "BuildNpcRegion" in regional and "AllocateBuildingGroup" in regional
    assert "IsResourceBuilding" in regional and "IsMilitaryBuilding" in regional
    assert "_processedDays % 2 == 0" in regional
    assert "\"WORLD_POPULATION_CAPACITY\", state.BasePopulationCapacity" in regional
    assert "BuildingValue(state, \"WORLD_HEALTH\"" in regional
    assert "BuildingValue(state, \"WORLD_TAX_INCOME\"" in regional
    assert "VisualRoads" in regional and "VisualWalls" in regional and "VisualMines" in regional
    assert "state.Health * (1 - state.Devastation" in text("Scripts/World/WorldRegionRuntime.cs")
    assert "UpdateHealthAndDevastation" in regional and "DiseaseOutbreak" in regional
    assert "120.0 / 255.0" in regional and "128.0 / 255.0" in regional
    assert "state.Devastation - 1.0 / 32.0" in regional
    assert "RegionalCondition" in regional and "AccumulateTaxes" in regional
    assert "DispatchTaxCaravans(_processedDays % 2 == 0)" in regional
    assert "SqueezeRegion" in regional and "state.Devastation + 0.5" in regional
    assert "state.TaxSqueeze + 0.5" in regional and "state.TaxSqueeze - 0.1" in regional
    assert "SetBesieged" in regional and "state.Besieged" in regional
    assert "growth *= RegionalCondition(state)" in regional
    assert "squeezeLoyalty" in regional
    assert "region.Area" in regional and "GenerationSeed" in strategic
    assert "PlayerRegionPopulation = 200" in regional
    assert "TradeRouteShipmentMaximum = 50" in regional
    assert "Directory.EnumerateFiles(root, \"_GEN.txt\"" in regional
    assert "WORLD_PRODUCTION_" in regional and "SourceDaysPerYear" in regional
    assert "DispatchTaxCaravans" in regional and "AdvanceCaravans" in regional
    assert "PublishNpcMarkets" in regional and "_world.PublishTradeQuotes(_trade)" in regional
    assert "SetBuildingLevel" in regional and "RegionalEconomyRuntime" in strategic_ui
    assert len(list((ROOT / "Data/Original/init/world/building").glob("**/_GEN.txt"))) == 4
    assert len([path for path in (ROOT / "Data/Original/init/world/building").glob("**/*.txt")
                if not path.name.startswith("_")]) == 21
    assert "RequirementsLess" in regional and "RequirementsPass" in regional
    assert ".Take(2).ToArray()" in regional and "3 - slot" in regional
    assert "0.75 + 0.5 * prospect" in regional
    assert "-8.0 / 255.0, 8.0 / 255.0" in regional
    assert "CurrentRegionId" in regional and "BuildRoute" in regional
    assert "caravan.Route.RemoveAt(0)" in regional
    assert "foreach (var caravan in _economy.Caravans)" in strategic_ui
    assert "RegionalEdict.Sanction => 0.25" in regional
    assert "RegionalEdict.Exile or RegionalEdict.Massacre => 0" in regional
    assert "RacePopulation" in regional and "RaceTargets" in regional
    assert 'PopulationTerrain.GetValueOrDefault("MOUNTAIN", 1)' in regional
    assert 'PopulationTerrain.GetValueOrDefault("FOREST", 1)' in regional
    assert "MoveExiles" in regional and "Neighbours.Select(State)" in regional
    assert "UpdateRealmEdicts" in regional and "active * 0.5" in regional
    assert "-1.0 / (2 * SourceDaysPerYear)" in regional
    assert "UpdateReligions" in regional and "ReligiousOpposition" in regional
    assert "(target - current) / SourceDaysPerYear" in regional
    assert "ReligionInclination" in text("Scripts/Data/OriginalGameData.cs")
    assert "capital.Population * 0.25 + realmPopulation * 0.15" in regional
    assert "9.0 / ResourceLedger.KindCount" in regional
    assert "Math.Clamp(target / (double)after, 0.1, 10.0)" in regional
    assert "Math.Clamp(multiplier - 0.4, 1, 2)" in regional
    assert "Math.Clamp(multiplier + 0.4, 0.5, 1)" in regional
    assert "WorldLayer.Fertility" in strategic_ui and "WorldLayer.Loyalty" in strategic_ui
    assert "WorldLayer.Population" in strategic_ui and "WorldLayer.Religion" in strategic_ui
    assert "SetEdict" in strategic_ui
    terrain = text("Scripts/World/StrategicTerrainRuntime.cs")
    assert "StrategicTerrainRuntime" in strategic and "AggregateAllTerrain" in strategic
    assert "WaterLine = 0.30" in terrain and "OceanComponentMinimum = 150" in terrain
    assert "ForestCoverage = 0.30" in terrain
    assert "LargeRiverReferenceCount = 10" in terrain and "SmallRiverReferenceCount = 200" in terrain
    assert "BuildWaterDistances" in terrain and "GenerateClimateAndFertility" in terrain
    assert "BuildTerrainTexture" in strategic_ui and "WorldLayer.Mountains" in strategic_ui
    assert "OriginalWorldMapTextureBuilder.Build" in strategic_ui
    assert "PositionDetailsPopup(motionPosition)" in world_setup
    assert "BuildFogTexture" in strategic_ui and "VisibleWorldEntities" in strategic_ui
    terrain_renderer = text("Scripts/Rendering/OriginalWorldMapTextureBuilder.cs")
    assert "TilePixels = 16" in terrain_renderer and "Forest.png" in terrain_renderer
    assert "MountainCornerMask" in terrain_renderer and "MountainOffsetX" in terrain_renderer
    save_service = text("Scripts/Save/SaveGameService.cs")
    assert "WorldCapitalX" in save_service and "WorldCapitalY" in save_service
    assert "PlayerProfile" in save_service and "Version { get; set; } = 37" in save_service
    assert "StrategicRoad" in strategic and "GenerateRoads" in strategic
    assert "FindRoadTilePath" in strategic and "LandComponents" in strategic
    assert "TilePath" in strategic and "RoadTerrainCost" in strategic
    assert "PolishRoads" in strategic and "RemoveUnusedRoadTiles" in strategic
    assert "PolymapEdge" in strategic and "RegionInterior" in strategic
    assert "GenerateRuralSites" in strategic and "StrategicRuralSiteKind" in strategic
    assert "neighbourWeightMaximum = 6.8" in strategic
    assert "farmRoll * farmRoll < farmChance" in strategic
    assert "SettlementPattern" in terrain and "public double Moisture" in terrain
    assert "GenerateRegionNames" in strategic and "WorldAreas.txt" in strategic
    assert "GenerateLandmarks" in strategic and "WorldLandmarks.txt" in strategic
    assert "StrategicLandmarkKind.Mountain => (40, 1000)" in strategic
    assert "StrategicLandmarkKind.Lake => (20, 10000)" in strategic
    assert "StrategicLandmarkKind.River => (25, 100)" in strategic
    assert "_ => (50, 5000)" in strategic and "_landmarks.Count < 255" in strategic
    assert "ISLAND_ADDONS" in strategic and "Sanctified" in strategic
    assert "foreach (var spreadMaximum in new[] { 1, 3, 5 })" in strategic
    assert "RegionsByDistance" in strategic and "3 * candidates.Count / 4" in strategic
    assert "ValidateGeneratedWorld" in strategic and "realm is disconnected" in strategic
    assert "road graph is disconnected" in strategic and "rural site occupies invalid terrain" in strategic
    assert "GenerateRoads();" in strategic[:strategic.index("var raceKeys")]
    assert "RegionalTravelCost" in strategic and "region.Mountain >= 0.08 ? 12" in strategic
    assert "region.Forest >= 0.45 ? 6 : 3" in strategic
    assert "FindRoute" in strategic and "PriorityQueue" in strategic
    assert "_world.FindRoute(first, second)" in regional
    assert "RoadTiles" in strategic_ui and "BuildRoadTexture" in strategic_ui
    assert "BuildLayerTexture" in strategic_ui and "RegionAtTile(x, y)" in strategic_ui
    assert "RuralSites" in strategic_ui
    assert "RebuildRoadMapTexture" in world_setup
    assert "region.Name" in world_setup and "faction.Name" in strategic_ui
    assert "LandmarkAtTile" in world_setup and "_world.Landmarks" in strategic_ui
    assert "_world.ValidateGeneratedWorld(_selectedRegion)" in world_setup
    havens = text("Scripts/World/WorldHavenRuntime.cs")
    assert "MaximumHavens = 64" in havens and "GeneratePlacements" in havens
    assert "init/race/worldcamp" in havens and "Candidates.Count" in havens
    assert "world.Terrain.Forest(x, y) > 0.25" in havens
    assert "_world.HavenPlacements" in havens and "_havens.Generate(seed)" in text(
        "Scripts/Settlement/SettlementWorldRuntime.cs")
    assert "HavenPlacements" in world_setup and "HavenPlacements" in strategic_ui
    assert "public string Decree" in law and "public TradePolicy Policy" in trade
    assert "case Key.F2: ToggleWindow(_administration)" in bootstrap
    assert "private void CloseWindows()" in bootstrap
    assert 'OriginalUiIcons.MainCategory(1), "Работы"' in bootstrap
    assert 'OpenRoomCategory("Работы")' in bootstrap
    assert 'OpenRoomCategory("Работы", "Переработка")' not in bootstrap
    assert "_roomPalette.Visible = false;" in bootstrap
    assert "_notifications.Refresh(_events.Notices" in bootstrap
    original_icons = text("Scripts/UI/OriginalUiIcons.cs")
    assert "AtlasTexture" in original_icons and "ICON\\s*" in original_icons
    assert (ROOT / "Data/Original/assets/sprite/icon/32/_UI.png").is_file()
    assert (ROOT / "Data/Original/assets/sprite/icon/32/REFINER.png").is_file()
    assert (ROOT / "Data/Original/assets/sprite/icon/16/_Icons.png").is_file()
    assert (ROOT / "Data/Original/assets/sprite/icon/24/_Icons.png").is_file()
    assert "MediumRoot" in original_icons and "public static Texture2D? Medium" in original_icons
    assert len(list((ROOT / "Data/Original/assets/sprite/icon/24/resource").glob("*.png"))) == 42
    chunk_tiles = text("Scripts/Rendering/ChunkTileRenderer.cs")
    assert "nextByChunk" in chunk_tiles and "previous.SequenceEqual(pair.Value)" in chunk_tiles
    assert "_lastPreviewEnd == previewEnd" in bootstrap and "Time.GetTicksMsec()" in bootstrap
    assert 'new Color(0.96f, 0.97f, 1f, 0.9f)' in text("Scripts/Settlement/GridWorld.cs")
    assert "UIRoomPlacer/Config: SShape + SMaterial | SItems | SStats" in bootstrap
    assert "OriginalUiIcons.Medium(33)" in bootstrap and "OriginalUiIcons.Medium(78)" in bootstrap
    assert "MediumWithBadge(33, 42)" in bootstrap and "BuildOnExistingStructures" in bootstrap
    assert "ApplyResponsiveLayout" in text("Scripts/UI/SettlementTopBar.cs")
    assert "viewport.X - 242f" in text("Scripts/UI/SettlementRightSidebar.cs")

    # A64: validate every extracted source layout, its Java tile semantics and
    # the complete lifecycle of an atomic multi-cell furniture job.
    layout_rows = text("Data/Original/furnisher_layouts.tsv").splitlines()
    assert layout_rows[0] == ("family\tgroup\twidth\theight\tcost_multiplier\t"
                              "stat_multiplier\tmask\troles\tfunctions")
    assert len(layout_rows) == 714
    for row in layout_rows[1:]:
        family, group, width, height, cost_multiplier, stat_multiplier, mask, roles, functions = row.split("\t")
        width_i, height_i = int(width), int(height)
        assert family and int(group) >= 0 and float(cost_multiplier) >= 0 and float(stat_multiplier) >= 0
        mask_rows = mask.split("/")
        assert len(mask_rows) == height_i
        assert all(len(mask_row) == width_i and set(mask_row) <= {"0", "1"}
                   for mask_row in mask_rows)
        role_rows = roles.split("/")
        assert len(role_rows) == height_i
        assert all(len(role_row) == width_i and set(role_row) <= {"0", "p", "b", "r", "x"}
                   for role_row in role_rows)
        assert all((mask_value == "0") == (role_value == "0")
                   for mask_row, role_row in zip(mask_rows, role_rows)
                   for mask_value, role_value in zip(mask_row, role_row))
        function_rows = functions.split("/")
        assert len(function_rows) == height_i
        assert all(len(function_row) == width_i and
                   set(function_row) <= {"0", "p", "w", "s", "d"}
                   for function_row in function_rows)
        assert all((mask_value == "0") == (function_value == "0")
                   for mask_row, function_row in zip(mask_rows, function_rows)
                   for mask_value, function_value in zip(mask_row, function_row))
        cells = [(x, z) for z, mask_row in enumerate(mask_rows)
                 for x, value in enumerate(mask_row) if value == "1"]
        assert cells
        rotations = [
            cells,
            [(height_i - 1 - z, x) for x, z in cells],
            [(width_i - 1 - x, height_i - 1 - z) for x, z in cells],
            [(z, width_i - 1 - x) for x, z in cells],
        ]
        bounds = [(width_i, height_i), (height_i, width_i),
                  (width_i, height_i), (height_i, width_i)]
        for rotated, (bound_x, bound_z) in zip(rotations, bounds):
            assert len(rotated) == len(set(rotated)) == len(cells)
            assert all(0 <= x < bound_x and 0 <= z < bound_z for x, z in rotated)
    build_job = text("Scripts/Simulation/BuildJob.cs")
    job_board = text("Scripts/Simulation/JobBoard.cs")
    grid_world = text("Scripts/Settlement/GridWorld.cs")
    citizens = text("Scripts/Citizens/CitizenSystem.cs")
    assert "public IEnumerable<GridCoord> OccupiedCells" in build_job
    assert "var occupied = job.OccupiedCells.ToArray()" in job_board
    assert "foreach (var cell in job.OccupiedCells)" in job_board
    assert "job.State is JobState.Completed or JobState.Cancelled" in job_board
    assert "public bool CanStandForConstruction" in grid_world
    assert "!Data.IsBlocked(cell)" in grid_world
    assert "TileFlags.Wall | TileFlags.Furniture | TileFlags.Reserved" in grid_world
    assert "_world.CanStandForConstruction(candidate)" in citizens
    assert "foreach (var cell in job.OccupiedCells) _world.CancelReservation(cell)" in bootstrap
    world_data = text("Scripts/Settlement/WorldGridData.cs")
    placement = text("Scripts/Rooms/RoomPlacementRuntime.cs")
    assert "FurnitureRoles" in world_data and "FurnitureBlocks" in world_data
    assert "FurnitureMustBeReachable" in world_data
    assert "К предмету должен оставаться доступный проход" in placement
    assert "FurnitureBlockerCells" in build_job and "FurnitureReachableCells" in build_job
    assert "save.Version >= 29" in bootstrap

    # A75: source tile data markers survive layout rotation, construction and
    # the current save format; production capacity uses only B_WORK/JOB cells.
    assert "WorkCells" in layout_catalog and "StorageCells" in layout_catalog
    assert "RotatedWorkCells" in layout_catalog and "RotatedStorageCells" in layout_catalog
    assert "FurnitureWorkCells" in build_job and "FurnitureStorageCells" in build_job
    assert "FurnitureWorkstation" in world_data and "FurnitureStorage" in world_data
    assert "pair.Value.WorkCells" in rooms and "pair.Value.StorageCells" in rooms
    assert "_world.Data.FurnitureWorkstation(cell)" in rooms
    assert "_world.Data.FurnitureWorkstation(_world.FromIndex(index))" in rooms
    assert "savedJob.FurnitureWorkCells.Select(_world.FromIndex)" in bootstrap
    assert "savedJob.FurnitureStorageCells.Select(_world.FromIndex)" in bootstrap
    assert "_world.Data.FurnitureWorkstation(furnitureCell)" in bootstrap
    assert "_world.Data.FurnitureStorage(furnitureCell)" in bootstrap
    assert "public int[] FurnitureWorkCells" in save_service
    assert "public int[] FurnitureStorageCells" in save_service

    # A76: FurnisherItem(tile, multiplierCosts, multiplierStats) stays split;
    # fractional values are retained and only cost totals feed construction.
    assert "double CostMultiplier" in layout_catalog
    assert "double StatMultiplier" in layout_catalog
    assert "IReadOnlyDictionary<int, double> ItemCosts" in placement
    assert "placement.StatMultiplier" in placement
    assert "placement.CostMultiplier" in placement
    assert "ConstructionCost(_area.Count, _itemCosts, Upgrade)" in placement
    assert "var amount = Math.Max(0, area) * AreaCost" in text("Scripts/Rooms/FurnisherRuntime.cs")
    assert "_placements.Count == 0" in placement
    assert "public double[] ItemGroupAmounts" in save_service
    stockpile_rows = [row.split("\t") for row in layout_rows[1:]
                      if row.startswith("infra/stockpile\t")]
    assert any(cost != stat for _, _, _, _, cost, stat, *_ in stockpile_rows)
    assert any(row[4] == "1.5" or row[5] == "1.5" for row in
               (value.split("\t") for value in layout_rows[1:]))

    # A77: furniture maintenance uses the source per-tile broken amount and
    # MRoom's resource-component probability instead of Wood x2 placeholders.
    maintenance = text("Scripts/Maintenance/MaintenanceRuntime.cs")
    assert "BrokenResourceAmount" in furnisher
    assert "costs[resourceIndex] * costMultiplier / itemArea" in furnisher
    assert "ResourceJobChance" in maintenance
    assert "resourceRate / totalRate" in maintenance
    assert "SelectResource" in maintenance and "random.NextDouble()" in maintenance
    assert "FurnitureRepairAmounts" in rooms
    assert "placement.CostMultiplier" in rooms
    assert "room.RequiredFurniture * 2" not in rooms
    assert "MaintenanceResourceAmounts" in save_service
    assert "FurnitureRepairCells" in save_service
    assert "FurnitureRepairResources" in save_service
    assert "FurnitureRepairAmounts" in save_service

    # A78: damage applies atomically to a whole furniture footprint. Broken
    # work/storage functions remain disabled until its repair job completes.
    assert "FurnitureBroken" in world_data
    assert "SetFurnitureBroken" in world_data
    assert "(_furnitureRoles[Index(cell)] & 20) == 4" in world_data
    assert "(_furnitureRoles[Index(cell)] & 24) == 8" in world_data
    assert "RoomFurnitureFootprint" in rooms
    assert "BreakFurnitureAt" in rooms
    assert "!item.Broken && item.Cells.Contains(index)" in rooms
    assert "room.State = RoomState.Building" in rooms
    assert "item.Broken && item.Anchor" in rooms
    assert "SetFurnitureBroken(_world.FromIndex(index), false)" in rooms
    assert "SavedFurnitureFootprint" in save_service
    assert "FurnitureFootprints" in save_service

    # A79: raw ITEMS.STATS are accumulated once, then source FurnisherStat
    # dependency transforms feed peaceful capacity, quality and production.
    furnisher_stats = text("Scripts/Rooms/FurnisherStatRuntime.cs")
    assert "Transform.Relative" in furnisher_stats
    assert "Transform.Efficiency" in furnisher_stats
    assert "Transform.EmployeesRelative" in furnisher_stats
    assert "FurnisherRuntime.RelativeStat" in furnisher_stats
    assert "FurnisherRuntime.EfficiencyStat" in furnisher_stats
    assert '"BATH_NORMAL"' in furnisher_stats and "0, 1.5" in furnisher_stats
    assert '"LAVATORY_NORMAL"' in furnisher_stats and "1.0 / 8.0" in furnisher_stats
    assert "FurnisherStatRuntime.Efficiency(room)" in rooms
    assert "Services.Synchronize(_rooms, Blueprints, FurnisherStatRuntime.Value)" in rooms
    assert "Schools.Synchronize(_rooms, FurnisherStatRuntime.Value)" in rooms
    assert "Hospitality.Synchronize(_rooms, FurnisherStatRuntime.Value)" in rooms
    assert "Law.Synchronize(_rooms, _world.FromIndex, FurnisherStatRuntime.Value)" in rooms
    assert "_roomPlanner.DefinitionStats()" in bootstrap

    # A80: placement enforces Java flush group bounds before transformed
    # FurnisherStat minima, exactly matching UtilPlacability's order.
    constraints = text("Data/Original/furnisher_constraints.tsv").splitlines()
    assert constraints[0] == "family\tgroup\tminimum\tmaximum\tstat_minimums"
    assert len(constraints) == 110
    for row in constraints[1:]:
        family, group, minimum, maximum, stat_minimums = row.split("\t")
        assert family and int(group) >= 0
        assert 0 <= int(minimum) <= int(maximum)
        assert all(float(value) >= 0 for value in stat_minimums.split(",") if value)
    constraint_catalog = text("Scripts/Rooms/FurnisherConstraintCatalog.cs")
    assert "FurnisherGroupConstraint" in constraint_catalog
    assert "furnisher_constraints.tsv" in constraint_catalog
    assert "FamilyForRoom" in constraint_catalog
    assert "placed < constraint.Minimum" in placement
    assert "placed > constraint.Maximum" in placement
    assert "FurnisherStatRuntime.EvaluatePlacement(" in placement
    assert "minimums[index] > 0" in placement
    constraint_extractor = text("Tools/extract_furnisher_constraints.py")
    assert "flush_pattern" in constraint_extractor and "stat_pattern" in constraint_extractor
    assert "public GridCoord ConstructionCell" in build_job
    assert "Kind is BuildKind.RoomFloor or BuildKind.RoomRoof or BuildKind.RoomClear" in build_job
    assert "System.Func<GridCoord, bool> hasObstacle" in build_job
    assert "job.ConstructionCell" in citizens
    assert "public int ConstructionCell { get; set; }" in save_service
    assert "save.Version >= 30" in bootstrap
    assert "RoomFloor" in build_job and "FloorKey" in build_job
    assert "ActivateRoomConstruction" in job_board
    assert "BuildRoomFloor" in grid_world and "RemoveRoomFloor" in grid_world
    assert "RoomFloorSpeed" in world_data
    assert "floorsReady" in rooms
    assert "save.Version >= 31" in bootstrap and "save.Version < 31" in bootstrap
    assert "RoomRoof" in build_job and "BuildKind.Furniture or BuildKind.RoomFloor or BuildKind.RoomRoof => 10f" in build_job
    assert "ActivateRoomRoofs" in job_board and "HasPendingRoomRoofs" in job_board
    assert "jobs.HasPendingRoomRoofs(order.RoomId)" in room_construction
    assert "roofsReady" in rooms and "SetRoof(agent.Job.Cell, true)" in citizens
    assert "save.Version < 32" in bootstrap
    assert "? 2f : 20f" in build_job
    assert "RoomClear" in build_job and "BuildKind.RoomClear => 0f" in build_job
    assert "HasPendingRoomClears" in job_board and "ActivateRoomWalls" in job_board
    assert "PrepareConstruction?.Invoke(job)" in job_board
    assert "jobs.HasPendingRoomClears(order.RoomId)" in room_construction
    assert "jobs.Add(BuildJob.RoomClear(cell, room.Id))" in rooms
    assert "RoomDoor" in build_job and "public static BuildJob RoomDoor" in build_job
    assert "RequiredDoors" in rooms and "CompletedDoorCount" in rooms
    assert "BuildJob.RoomDoor" in rooms and "SetDoor(agent.Job.Cell)" in citizens
    assert "public int[] RequiredDoors { get; set; }" in save_service
    assert "save.Version >= 34 ? room.RequiredDoors" in bootstrap
    placement = text("Scripts/Rooms/RoomPlacementRuntime.cs")
    assert "foreach (var cell in _doors) _world.SetDoor(cell);" not in placement
    assert "public bool HasPendingRoomConstruction(int roomId)" in job_board
    assert "public void SynchronizeProductionReadiness" in rooms
    assert "jobs.HasPendingRoomConstruction(room.Id)" in rooms
    assert "RuntimeRecipe(room) is null || !InternalStorage.HasRoom(room.Id)" in rooms
    assert "_world.CanStandForConstruction(cell + offset)" in rooms
    assert "_rooms.SynchronizeProductionReadiness(_jobs, _resources);" in bootstrap
    logistics = text("Scripts/Rooms/SettlementLogisticsRuntime.cs")
    hauling = text("Scripts/Hauling/HaulingSystem.cs")
    assert "ReserveProductionSupply" in logistics and "PickupProductionSupply" in logistics
    assert "ReserveStockpileSpace" in logistics and "DepositStockpileHaul" in logistics
    assert "job.Kind != BuildKind.ProductionSupply && !resources.TryReserve(job)" in job_board
    assert "source.Value.RoomId, room.Id, profession" in rooms
    assert "_rooms.Logistics.DepositStockpileHaul(job)" in hauling
    assert "destination.Value.RoomId" in hauling
    assert "public int Version { get; set; } = 37;" in save_service
    assert "_rooms.RestorePhysicalLogisticsReservations(_jobs.All, _resources);" in bootstrap
    assert "StockpileStored" in logistics and "StockpileReservedSpace" in logistics
    assert "ExcludedFromStockpile" in logistics and "UnstoredAccounted" in hauling
    assert "destination.Value.RoomId" in hauling and "BuildJob.Haul(source" in hauling
    assert "OutputAlreadyAccounted" in save_service
    assert "_storedUnits" not in hauling and "_reservedStorageUnits" not in hauling
    assert "AllocatedCratesFor" in logistics and "SetCrateLimit" in logistics
    assert "FreeFor(ResourceKind resource)" in logistics
    assert "room.Fetching" in logistics and "OrderByDescending(room => room.Priority)" in logistics
    assert "CapturePolicies" in logistics and "RestorePolicies" in logistics
    assert "LogisticsPolicies" in save_service and "LogisticsPolicies" in bootstrap
    room_policy = text("Scripts/UI/RoomPolicyPanel.cs")
    assert "StockCrates" in room_policy and "StockLimit" in room_policy
    assert "ToggleFetching" in room_policy and "StockPriority" in room_policy
    industry = text("Scripts/Industry/IndustryRuntime.cs")
    assert "public static int PreviewAdvance(" in industry
    assert "var requiredInputs = recipe.Inputs.ToDictionary" in rooms
    assert "requiredInputs.Any" in rooms and "CompletePending(job.RoomId);" in rooms
    assert "ReserveProductionOutputs" in rooms
    assert "foreach (var output in recipe.Outputs)" in rooms
    assert "reservation.Slot.Resource == outputRule.Key" in rooms
    assert "InternalStorage.DepositUnreserved(room.Id, outputRule.Key, output)" not in rooms
    assert "_rooms.RestoreProductionOutputReservations(job);" in bootstrap
    print("PORT_STATIC_VERIFICATION_OK")


if __name__ == "__main__":
    main()
