using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.Resources;
using GodotSyxPort.Hauling;
using GodotSyxPort.Data;
using GodotSyxPort.Industry;
using GodotSyxPort.Maintenance;
using GodotSyxPort.Trade;
using GodotSyxPort.Technology;
using GodotSyxPort.Law;
using GodotSyxPort.Governance;
using GodotSyxPort.Military;
using GodotSyxPort.Citizens;
using GodotSyxPort.Bootstrap;

namespace GodotSyxPort.Rooms;

public enum RoomType : byte
{
    Storage,
    Workshop,
    Bakery
}

public enum RoomState : byte
{
    Planned,
    Building,
    Operational
}

public sealed class RoomRecord
{
    public int Id { get; init; }
    public RoomType Type { get; init; }
    public string DefinitionKey { get; set; } = "";
    public HashSet<int> Cells { get; init; } = new();
    public HashSet<int> RequiredWalls { get; init; } = new();
    public HashSet<int> RequiredDoors { get; init; } = new();
    public RoomState State { get; set; }
    public int RequiredFurniture { get; set; }
    public Dictionary<int, double> ItemGroupAmounts { get; init; } = new();
    public Dictionary<ResourceKind, int> MaintenanceResourceAmounts { get; init; } = new();
    public Dictionary<(int Cell, ResourceKind Resource), int> FurnitureRepairAmounts { get; init; } = new();
    public List<RoomFurnitureFootprint> FurnitureFootprints { get; init; } = new();
    public int WorkerLimit { get; set; } = -1;
    public int RecipeIndex { get; set; }
    public int UpgradeLevel { get; set; }
    public double Isolation { get; set; } = 1.0;
    public Dictionary<(int Recipe, ResourceKind Resource), double> IndustryInputProgress { get; init; } = new();
    public Dictionary<int, double> IndustryOutputProgress { get; init; } = new();
    public Dictionary<(int Recipe, ResourceKind Resource), double> AdditionalOutputProgress { get; init; } = new();
    public Dictionary<(int Cell, ResourceKind Resource), int> InputStorage { get; init; } = new();
    public RoomEmploymentRuntime Employment { get; init; } = new();
    public double Degradation { get; set; }
    public double MaintenanceDebt { get; set; }
    public bool MaintenancePending { get; set; }
    public int ToolTargetPerWorker { get; set; }
    public int ToolUnits { get; set; }
    public double ToolWearProgress { get; set; }
    public bool EquipmentSupplyPending { get; set; }
}

public sealed class RoomFurnitureFootprint
{
    public int Anchor { get; init; }
    public int Group { get; init; }
    public int Variant { get; init; }
    public int Rotation { get; init; }
    public HashSet<int> Cells { get; init; } = new();
    public bool Broken { get; set; }
}

public sealed class RoomSystem
{
    private readonly GridWorld _world;
    private readonly SettlementWeatherRuntime? _weather;
    private readonly List<RoomRecord> _rooms = new();
    private int _nextId = 1;
    private readonly Dictionary<int, int> _pendingProduction = new();
    private readonly Dictionary<int, RoomInstanceRuntime> _instances = new();
    private const int InputStorageMaximum = 31;
    private const int InputFetchMaximum = 15;
    private const int HospitalBedResourceMaximum = 7;

    public IReadOnlyList<RoomRecord> All => _rooms;
    public RoomBlueprintCatalog Blueprints { get; }
    public RoomConstructionRuntime Construction { get; } = new();
    public RoomStorageRuntime InternalStorage { get; } = new();
    public RoomServiceRuntime Services { get; } = new();
    public BurialRuntime Burials { get; } = new();
    public SanitationRuntime Sanitation { get; } = new();
    public TempleRuntime Temples { get; } = new();
    public BathRuntime Baths { get; } = new();
    public WaterInfrastructureRuntime Water { get; }
    public GateRuntime Gates { get; }
    public HospitalityRuntime Hospitality { get; } = new();
    public BuilderInfrastructureRuntime Builders { get; }
    public ActivityServiceRuntime Activities { get; } = new();
    public PreparedServiceRuntime PreparedServices { get; } = new();
    public FoodVenueRuntime FoodVenues { get; } = new();
    public AsylumRuntime Asylums { get; } = new();
    public CannibalRuntime Cannibals { get; } = new();
    public JanitorRuntime Janitors { get; } = new();
    public HousingRuntime Housing { get; } = new();
    public ChildcareRuntime Childcare { get; } = new();
    public SchoolRuntime Schools { get; } = new();
    public SpecialProductionRuntime SpecialProduction { get; }
    public SettlementLogisticsRuntime Logistics { get; }
    public SettlementTradeRuntime Trade { get; }
    public KnowledgeRuntime Knowledge { get; }
    public TechnologyRuntime Technologies { get; }
    public SettlementLawRuntime Law { get; } = new();
    public SettlementGovernanceRuntime Governance { get; }
    public SettlementMilitaryRuntime Military { get; } = new();
    private double _serviceDayFraction;
    public IReadOnlyDictionary<int, RoomInstanceRuntime> Instances => _instances;
    public int OperationalCount => _rooms.Count(room => room.State == RoomState.Operational);
    private readonly Dictionary<ResourceKind, GridCoord> _landingSupplies = new();

    public RoomSystem(GridWorld world, SettlementWeatherRuntime? weather = null)
    {
        _world = world;
        _weather = weather;
        Blueprints = new RoomBlueprintCatalog(OriginalGameData.Current.Rooms.Values);
        Water = new WaterInfrastructureRuntime(world);
        Gates = new GateRuntime(world);
        Builders = new BuilderInfrastructureRuntime(world);
        SpecialProduction = new SpecialProductionRuntime(world);
        Logistics = new SettlementLogisticsRuntime(world);
        Trade = new SettlementTradeRuntime(Logistics);
        Knowledge = new KnowledgeRuntime(world);
        Technologies = new TechnologyRuntime(Knowledge);
        Governance = new SettlementGovernanceRuntime(Knowledge, Trade);
    }

    public void ConfigureLandingSupply(ResourceKind resource, GridCoord cell) =>
        _landingSupplies[resource] = cell;

    public void RemoveLandingSupply(ResourceKind resource, GridCoord cell)
    {
        if (_landingSupplies.TryGetValue(resource, out var existing) && existing == cell)
            _landingSupplies.Remove(resource);
    }

    public RoomRecord PlaceLandingThrone(IEnumerable<GridCoord> cells)
    {
        var area = cells.Distinct().ToArray();
        var room = new RoomRecord
        {
            Id = _nextId++, Type = RoomType.Workshop, DefinitionKey = "_THRONE",
            Cells = area.Select(_world.CellToIndex).ToHashSet(),
            RequiredFurniture = 0, State = RoomState.Operational
        };
        _rooms.Add(room);
        foreach (var cell in area) _world.SetZone(cell);
        RegisterInstance(room);
        Governance.Synchronize(_rooms, _world.FromIndex);
        return room;
    }

    public RoomRecord Create(
        RoomType type,
        IEnumerable<GridCoord> area,
        IEnumerable<GridCoord> perimeter,
        ISet<GridCoord> doors,
        bool autoWalls,
        JobBoard jobs)
    {
        var room = new RoomRecord
        {
            Id = _nextId++,
            Type = type,
            DefinitionKey = LegacyRoomKey(type) ?? "",
            Cells = area.Select(_world.CellToIndex).ToHashSet(),
            RequiredWalls = autoWalls
                ? perimeter.Where(cell => !doors.Contains(cell)).Select(_world.CellToIndex).ToHashSet()
                : new HashSet<int>(),
            RequiredDoors = doors.Select(_world.CellToIndex).ToHashSet(),
            RequiredFurniture = type == RoomType.Storage ? 0 : 1,
            State = RoomState.Planned
        };
        room.ToolTargetPerWorker = DefaultToolTarget(room.DefinitionKey);
        _rooms.Add(room);
        RegisterInstance(room);

        foreach (var index in room.RequiredWalls)
        {
            var cell = _world.FromIndex(index);
            if (!_world.CanPlanWall(cell)) continue;
            _world.ReserveWall(cell);
            jobs.Add(BuildJob.RoomWall(cell, room.Id));
        }
        foreach (var index in room.RequiredDoors)
            jobs.Add(BuildJob.RoomDoor(_world.FromIndex(index), room.Id));
        foreach (var cell in room.Cells.Concat(room.RequiredWalls).Concat(room.RequiredDoors)
                     .Distinct().Select(_world.FromIndex))
            jobs.Add(BuildJob.RoomClear(cell, room.Id));
        room.State = RoomState.Building;
        return room;
    }

    public RoomRecord CreateFromDefinition(
        string definitionKey,
        IEnumerable<GridCoord> area,
        IEnumerable<GridCoord> perimeter,
        ISet<GridCoord> doors,
        bool autoWalls,
        IReadOnlyDictionary<int, double> itemGroupAmounts,
        IReadOnlyDictionary<int, double> itemGroupCosts,
        int upgradeLevel,
        JobBoard jobs,
        IReadOnlyDictionary<GridCoord, int>? furnitureCells = null,
        IReadOnlyDictionary<GridCoord, FurniturePlacement>? furniturePlacements = null)
    {
        if (!CanCreateRoom(definitionKey))
            throw new InvalidOperationException($"Technology has not unlocked room: {definitionKey}");
        var blueprint = Blueprints.Get(definitionKey) ??
            throw new InvalidOperationException($"Unknown room definition: {definitionKey}");
        var compatibilityType = definitionKey.Equals("REFINER_BAKERY", StringComparison.OrdinalIgnoreCase)
            ? RoomType.Bakery
            : definitionKey.Equals("_STOCKPILE", StringComparison.OrdinalIgnoreCase)
                ? RoomType.Storage
                : RoomType.Workshop;
        var room = Create(compatibilityType, area, perimeter, doors, autoWalls, jobs);
        _instances.Remove(room.Id);
        room.DefinitionKey = blueprint.Key;
        room.ToolTargetPerWorker = DefaultToolTarget(room.DefinitionKey);
        room.UpgradeLevel = Math.Clamp(upgradeLevel, 0, blueprint.MaximumUpgrade);
        if (!CanSetDefinitionUpgrade(room.DefinitionKey, room.UpgradeLevel))
            throw new InvalidOperationException($"Technology has not unlocked room upgrade: {definitionKey}/{room.UpgradeLevel}");
        room.RequiredFurniture = furnitureCells is { Count: > 0 }
            ? furnitureCells.Count
            : (int)Math.Ceiling(itemGroupAmounts.Values.Sum());
        foreach (var pair in itemGroupAmounts) room.ItemGroupAmounts[pair.Key] = pair.Value;
        if (furniturePlacements is not null)
            foreach (var placement in furniturePlacements.Values)
                for (var resourceIndex = 0;
                     resourceIndex < blueprint.Rule.Construction.Resources.Count; resourceIndex++)
                {
                    if (!OriginalGameData.TryMapResource(
                            blueprint.Rule.Construction.Resources[resourceIndex], out var resource)) continue;
                    var amount = blueprint.Furnisher.BrokenResourceAmount(
                        placement.Group, resourceIndex, placement.CostMultiplier,
                        placement.Cells.Count);
                    if (amount <= 0) continue;
                    foreach (var cell in placement.Cells)
                        room.FurnitureRepairAmounts[(_world.CellToIndex(cell), resource)] = amount;
                }
        if (furniturePlacements is not null)
            foreach (var placement in furniturePlacements)
                room.FurnitureFootprints.Add(new RoomFurnitureFootprint
                {
                    Anchor = _world.CellToIndex(placement.Key),
                    Group = placement.Value.Group,
                    Variant = placement.Value.Variant,
                    Rotation = placement.Value.Rotation,
                    Cells = placement.Value.Cells.Select(_world.CellToIndex).ToHashSet()
                });
        var plannedFurniture = furniturePlacements is { Count: > 0 }
            ? furniturePlacements.Select(pair => (Anchor: pair.Key, Cells: pair.Value.Cells,
                Blockers: pair.Value.BlockerCells, Reachable: pair.Value.ReachableCells,
                Work: pair.Value.WorkCells, Storage: pair.Value.StorageCells))
            : (furnitureCells is { Count: > 0 }
                ? furnitureCells.Keys
                : area.Distinct().OrderBy(value => value.Z).ThenBy(value => value.X)
                    .Take(room.RequiredFurniture))
                .Select(cell => (Anchor: cell,
                    Cells: (IReadOnlyList<GridCoord>)new[] { cell },
                    Blockers: (IReadOnlyList<GridCoord>)new[] { cell },
                    Reachable: (IReadOnlyList<GridCoord>)Array.Empty<GridCoord>(),
                    Work: (IReadOnlyList<GridCoord>)Array.Empty<GridCoord>(),
                    Storage: (IReadOnlyList<GridCoord>)Array.Empty<GridCoord>()));
        foreach (var placement in plannedFurniture)
        {
            var cells = placement.Cells.Distinct().ToArray();
            if (cells.Length == 0 || cells.Any(cell => !_world.CanPlanFurniture(cell))) continue;
            foreach (var cell in cells) _world.ReserveFurniture(cell);
            jobs.Add(BuildJob.RoomFurniture(
                placement.Anchor, room.Id, cells, placement.Blockers, placement.Reachable,
                placement.Work, placement.Storage));
        }
        var floorKey = blueprint.Furnisher.Floor(room.UpgradeLevel);
        if (floorKey is not null)
            foreach (var cell in room.Cells.Select(_world.FromIndex))
                jobs.Add(BuildJob.RoomFloor(cell, room.Id, floorKey));
        if (blueprint.Rule.Construction.Indoors)
            foreach (var cell in room.Cells.Select(_world.FromIndex)
                         .Where(cell => !_world.Data.Has(cell, TileFlags.Cave)))
                jobs.Add(BuildJob.RoomRoof(cell, room.Id));
        RegisterInstance(room);
        var construction = Construction.Begin(_instances[room.Id], itemGroupCosts);
        foreach (var pair in construction.Required)
            room.MaintenanceResourceAmounts[pair.Key] = pair.Value;
        return room;
    }

    public bool CanCreateRoom(string definitionKey) =>
        !(definitionKey.Equals("_THRONE", StringComparison.OrdinalIgnoreCase) &&
          _rooms.Any(room => room.DefinitionKey.Equals("_THRONE", StringComparison.OrdinalIgnoreCase))) &&
        Technologies.IsContentUnlocked("ROOM_" + definitionKey);

    public void CompleteFurnitureVisual(BuildJob job)
    {
        var room = _rooms.FirstOrDefault(value => value.Id == job.RoomId);
        if (room is null) return;
        var footprint = room.FurnitureFootprints.FirstOrDefault(value =>
            value.Anchor == _world.CellToIndex(job.Cell));
        if (footprint is null || footprint.Cells.Any(index =>
                !_world.Data.Has(_world.FromIndex(index), TileFlags.Furniture))) return;
        _world.AddFurnitureVisual(new FurnitureVisualPlacement(room.DefinitionKey,
            footprint.Group, footprint.Variant, footprint.Rotation,
            _world.FromIndex(footprint.Anchor)));
    }

    public bool CanSetUpgrade(RoomRecord room, int level) => level <= 0 ||
        CanSetDefinitionUpgrade(room.DefinitionKey, level);

    public bool CanSetDefinitionUpgrade(string definitionKey, int level) => level <= 0 ||
        Technologies.IsContentUnlocked($"ROOM_{definitionKey}_UPGRADE_{level}");

    public bool SetUpgrade(RoomRecord room, int level)
    {
        var blueprint = Blueprints.Get(room.DefinitionKey);
        if (blueprint is null) return false;
        var clamped = Math.Clamp(level, 0, blueprint.MaximumUpgrade);
        if (!CanSetUpgrade(room, clamped)) return false;
        room.UpgradeLevel = clamped;
        return true;
    }

    public void UpdateStates()
    {
        foreach (var room in _rooms)
        {
            var wallsReady = room.RequiredWalls.All(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Wall));
            var doorsReady = room.RequiredDoors.All(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Door));
            var furniture = room.Cells.Count(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture) &&
                !_world.Data.FurnitureBroken(_world.FromIndex(index)));
            var workstations = WorkstationCount(room);
            var floorKey = Blueprints.Get(room.DefinitionKey)?.Furnisher.Floor(room.UpgradeLevel);
            var floorsReady = floorKey is null || room.Cells.All(index =>
                _world.RoomFloorKeys.ContainsKey(index));
            var indoors = OriginalGameData.Current.Room(room.DefinitionKey)?.Construction.Indoors == true;
            var roofsReady = !indoors || room.Cells.Select(_world.FromIndex)
                .Where(cell => !_world.Data.Has(cell, TileFlags.Cave))
                .All(_world.Data.HasRoof);
            var materialsReady = !Construction.Orders.TryGetValue(room.Id, out var order) || order.Complete;
            room.State = wallsReady && doorsReady && roofsReady && floorsReady &&
                         furniture >= room.RequiredFurniture && materialsReady
                ? RoomState.Operational
                : RoomState.Building;
            var rule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
            var militaryRule = OriginalGameData.Current.MilitaryRooms.GetValueOrDefault(RoomKey(room) ?? "");
            var maximum = room.State != RoomState.Operational
                ? 0
                : IsLawRoom(RoomKey(room))
                    ? LawMaximum(room, furniture)
                : militaryRule?.Kind == "ARTILLERY" ? SettlementMilitaryRuntime.ArtilleryCrew
                : RoomKey(room) == "_STATION" ? 15
                : RoomKey(room) == "_BUILDER" ? BuilderInfrastructureInstance.MaximumWorkers
                : RoomKey(room) == "_INN" ? Math.Max(1, furniture / 4)
                : RoomKey(room) == "RESTHOME_NORMAL" ? Math.Max(1, furniture)
                : rule?.Logistics is not null
                    ? LogisticsMaximum(room, rule.Logistics, furniture)
                : rule?.SpecialProduction?.Kind switch
                {
                    // PastureInstance/JOB_MANAGER: one worker per 64 pasture tiles.
                    "PASTURE" => Math.Max(1, (int)Math.Ceiling(room.Cells.Count / 64.0)) * 2,
                    // HunterInstance hard-caps employment independently of furniture count.
                    "HUNTER" => Math.Min(rule.SpecialProduction.MaximumEmployed,
                        Math.Max(1, (int)Math.Ceiling(room.ItemGroupAmounts.GetValueOrDefault(0)))),
                    _ => rule?.Archetype switch
                {
                    // ROOM_FARM: WORKERPERTILEI = (Tile.WORK_TIME 4 + walk-next 3) /
                    // TIME.workSeconds (12 source hours * 48 seconds).
                    RoomArchetype.Agriculture => Math.Max(1,
                        (int)Math.Ceiling(room.Cells.Count * 7.0 /
                                          (OriginalGameData.Current.SecondsPerHour * 12.0))),
                    // Mine Constructor: floor(eligible mineral cells / 1.5). Until the
                    // mineral tile layer arrives, valid placed mine area is the candidate set.
                    RoomArchetype.Extraction => Math.Max(1,
                        (int)Math.Floor(room.Cells.Count / 1.5)),
                    _ when RoomKey(room) is "GRAVEYARD_NORMAL" or "TOMB_NORMAL" =>
                        Math.Max(1, (int)Math.Ceiling(furniture * 0.1)),
                    _ when RoomKey(room) == "SPEAKER_NORMAL" => 1,
                    _ when RoomKey(room) is "STAGE_NORMAL" or "FIGHTPIT_NORMAL" =>
                        Math.Max(1, (int)Math.Round(rule?.FurnisherItems.Sum(item =>
                            item.Stats.Count == 0 ? 0 : item.Stats[0]) ?? 0)),
                    _ when RoomKey(room) == "ARENAG_NORMAL" => Math.Max(1, furniture / 6),
                    _ when RoomKey(room) == "_JANITOR" => furniture,
                    _ when RoomKey(room) == "_HOME_CHAMBER" => 4,
                    // SchoolInstance: maxSet(ceil(jobs.size / 3)).
                    _ when RoomKey(room) == "SCHOOL_NORMAL" =>
                        Math.Max(1, (int)Math.Ceiling(furniture / 3.0)),
                    _ when rule?.HasIndustry == true => workstations,
                    _ => furniture
                }
                };
            var automaticNeeded = RoomKey(room) switch
            {
                _ when IsLawRoom(RoomKey(room)) => LawNeeded(room, maximum),
                "_STATION" => maximum,
                "_BUILDER" => 1,
                "_INN" => Math.Max(1, (int)Math.Ceiling(furniture * HospitalityRuntime.InnWorkersPerBed)),
                "RESTHOME_NORMAL" => maximum,
                _ when rule?.Logistics is not null => LogisticsNeeded(room, rule.Logistics, maximum),
                _ when rule?.SpecialProduction?.Kind == "PASTURE" =>
                    Math.Max(1, (int)Math.Ceiling(room.Cells.Count / 64.0)),
                "_JANITOR" => Math.Max(1, (int)Math.Ceiling(maximum / 5.0)),
                // SchoolInstance: neededSet(ceil(jobs.size / 8)). The proxy has
                // three station cells per maximum employee after the formula above.
                "SCHOOL_NORMAL" => Math.Max(1, (int)Math.Ceiling(furniture / 8.0)),
                _ => maximum
            };
            var needed = room.WorkerLimit < 0 ? automaticNeeded : System.Math.Min(room.WorkerLimit, maximum);
            room.Employment.Configure(maximum, needed);
            _instances[room.Id].SetOperational(room.State == RoomState.Operational);
            if (room.State == RoomState.Operational && !InternalStorage.HasRoom(room.Id))
            {
                var roleStorageCells = room.Cells.Where(index =>
                    _world.Data.FurnitureStorage(_world.FromIndex(index))).ToHashSet();
                var storageCells = room.Cells
                    .Where(index => room.Type == RoomType.Storage ||
                                    roleStorageCells.Contains(index) ||
                                    roleStorageCells.Count == 0 &&
                                    _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture))
                    .Select(_world.FromIndex).ToArray();
                var produces = rule is not null &&
                    (rule.HasIndustry || (!string.IsNullOrWhiteSpace(rule.Minable) &&
                                          rule.YieldWorkerDaily > 0));
                if (storageCells.Length == 0 && produces)
                    storageCells = room.Cells.Select(_world.FromIndex).ToArray();
                var sourceCapacity = rule?.Storage ?? 0;
                if (storageCells.Length > 0 &&
                    (room.Type == RoomType.Storage || sourceCapacity > 0 || produces))
                    InternalStorage.Create(
                        _instances[room.Id], storageCells,
                        room.Type == RoomType.Storage ? 10 : Math.Max(1, sourceCapacity));
            }
        }
        Blueprints.Synchronize(_rooms);
        Services.Synchronize(_rooms, Blueprints, FurnisherStatRuntime.Value);
        Burials.Synchronize(_rooms, _world.FromIndex,
            room => room.Cells.Count(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture)), Services);
        Sanitation.Synchronize(_rooms, _world.FromIndex, Services);
        Temples.Synchronize(_rooms, _world.FromIndex);
        Baths.Synchronize(_rooms, _world.FromIndex, Services);
        Water.Synchronize(_rooms);
        Gates.Synchronize(_rooms);
        Hospitality.Synchronize(_rooms, FurnisherStatRuntime.Value);
        Builders.Synchronize(_rooms);
        Activities.Synchronize(_rooms, _world.FromIndex);
        PreparedServices.Synchronize(_rooms, _world.FromIndex, Services);
        FoodVenues.Synchronize(_rooms, _world.FromIndex, Services);
        Asylums.Synchronize(_rooms, _world.FromIndex);
        Cannibals.Synchronize(_rooms, _world.FromIndex);
        Law.TryReserveHarvest = race => Cannibals.Reserve(race, out var roomId) ? roomId : null;
        Janitors.Synchronize(_rooms, _world.FromIndex);
        Housing.Synchronize(_rooms, _world.FromIndex);
        Childcare.Synchronize(_rooms, _world.FromIndex);
        Schools.Synchronize(_rooms, FurnisherStatRuntime.Value);
        SpecialProduction.Synchronize(_rooms);
        Logistics.Synchronize(_rooms);
        Knowledge.Synchronize(_rooms);
        Law.Synchronize(_rooms, _world.FromIndex, FurnisherStatRuntime.Value);
        Governance.Synchronize(_rooms, _world.FromIndex);
        Military.Synchronize(_rooms, _world.FromIndex);
    }

    public void ScheduleProduction(JobBoard jobs, ResourceLedger resources, double dayFraction)
    {
        Logistics.Reconcile(resources);
        foreach (var room in _rooms)
        {
            if (!IsProductionReady(room, jobs)) continue;
            if (!IsWorkTime(room, dayFraction)) continue;
            var pending = _pendingProduction.GetValueOrDefault(room.Id);
            var limit = EffectiveWorkerLimit(room);
            if (pending >= limit) continue;
            var sourceRule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
            var minableIndex = string.IsNullOrWhiteSpace(sourceRule?.Minable)
                ? -1
                : OriginalGameData.Current.MinableIndex(sourceRule!.Minable);
            foreach (var workCellIndex in ProductionWorkCells(room, minableIndex))
            {
                if (pending >= limit) break;
                var workCell = _world.FromIndex(workCellIndex);
                var job = CreateProductionJob(room, workCell, resources);
                if (job is null) continue;
                if (!jobs.Add(job))
                {
                    CancelProductionSupplyReservation(job);
                    job.ReleaseOutputReservations();
                    continue;
                }
                pending++;
                _pendingProduction[room.Id] = pending;
            }
        }
    }

    public void SynchronizeProductionReadiness(JobBoard jobs, ResourceLedger resources)
    {
        foreach (var room in _rooms)
        {
            var keep = IsProductionReady(room, jobs) ? EffectiveWorkerLimit(room) : 0;
            var retained = jobs.TrimProductionForRoom(room.Id, keep, resources);
            if (retained == 0) _pendingProduction.Remove(room.Id);
            else _pendingProduction[room.Id] = retained;
        }
    }

    private bool IsProductionReady(RoomRecord room, JobBoard jobs)
    {
        if (room.State != RoomState.Operational || room.Type == RoomType.Storage ||
            !_instances.TryGetValue(room.Id, out var instance) || !instance.Active ||
            jobs.HasPendingRoomConstruction(room.Id) || EffectiveWorkerLimit(room) <= 0 ||
            RuntimeRecipe(room) is null || !InternalStorage.HasRoom(room.Id))
            return false;
        var rule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
        var minableIndex = string.IsNullOrWhiteSpace(rule?.Minable)
            ? -1
            : OriginalGameData.Current.MinableIndex(rule!.Minable);
        return ProductionWorkCells(room, minableIndex).Any();
    }

    private IEnumerable<int> ProductionWorkCells(RoomRecord room, int minableIndex) =>
        room.Cells.Where(index =>
        {
            var cell = _world.FromIndex(index);
            var workCell = _world.Data.FurnitureWorkstation(cell) ||
                           minableIndex >= 0 && _world.Data.MineralType(cell) == minableIndex ||
                           SpecialProduction.IsWorkCell(room, cell);
            return workCell && GridCoord.Cardinal.Any(offset =>
                _world.CanStandForConstruction(cell + offset));
        });

    public void TickServices(double delta, double dayFraction)
    {
        _serviceDayFraction = dayFraction;
        Services.Tick(delta, OriginalGameData.Current.SecondsPerDay);
        Burials.Tick(delta, OriginalGameData.Current.SecondsPerDay);
        Temples.Tick(delta, OriginalGameData.Current.SecondsPerDay);
        Baths.Tick(delta, OriginalGameData.Current.SecondsPerDay, Services);
        Water.Tick(delta);
        Asylums.Tick(delta);
        Cannibals.Tick(delta, OriginalGameData.Current.SecondsPerDay * 16,
            OriginalGameData.Current.Races.Count);
        SpecialProduction.Tick(delta, OriginalGameData.Current.SecondsPerDay);
    }

    public void TickTrade(double delta, ResourceLedger resources, bool tradeOpen) =>
        Trade.Tick(delta, resources, tradeOpen);

    public void TickTechnologies(double delta) => Technologies.Tick(delta);

    public void BeginKnowledgeDay(ResourceLedger resources)
    {
        Knowledge.BeginDay();
        Governance.BeginDay();
        Military.BeginDay(resources, Logistics);
    }

    public void RecordServiceUse(int roomId, string need, ResourceLedger? resources = null)
    {
        if (need.Equals("CONSTIPATION", StringComparison.OrdinalIgnoreCase))
            Sanitation.RecordUse(roomId);
        if (need.Equals("BATH", StringComparison.OrdinalIgnoreCase))
        {
            var service = Services.Instances.FirstOrDefault(candidate => candidate.RoomId == roomId);
            if (service is not null) Baths.RecordUse(service);
        }
        if (need is "GROOMING" or "DOCTOR" or "MASSAGE")
        {
            var service = Services.Instances.FirstOrDefault(candidate => candidate.RoomId == roomId);
            if (service is not null) PreparedServices.RecordUse(service);
        }
        if (need is "HUNGER" or "THIRST")
        {
            var service = Services.Instances.FirstOrDefault(candidate => candidate.RoomId == roomId);
            if (service is not null) FoodVenues.Consume(service);
        }
        if (need.Equals("SHOPPING", StringComparison.OrdinalIgnoreCase) && resources is not null)
            Logistics.ConsumeMarket(resources);
    }

    public void ScheduleFacilityWork(JobBoard jobs, ResourceLedger resources)
    {
        Sanitation.Schedule(jobs);
        Temples.Schedule(jobs, resources, FindSupplyCell());
        Baths.Schedule(jobs, resources, FindSupplyCell(), Services);
        Activities.Schedule(jobs, _rooms);
        PreparedServices.Schedule(jobs, _rooms);
        FoodVenues.Schedule(jobs, resources, FindSupplyCell());
        Asylums.Schedule(jobs, resources, FindSupplyCell());
        var maintenanceDemand = new Dictionary<ResourceKind, double>();
        AccumulateExpectedDailyMaintenanceUse(maintenanceDemand);
        Janitors.Schedule(jobs, resources, FindSupplyCell(), maintenanceDemand);
        Housing.Schedule(jobs, resources, FindSupplyCell());
        SpecialProduction.Schedule(jobs, resources, FindSupplyCell());
        Logistics.Schedule(jobs, resources);
        Knowledge.Schedule(jobs, resources, _rooms, FindSupplyCell());
        Law.Schedule(jobs);
        Military.Schedule(jobs, resources);
    }

    public void CompleteSanitation(BuildJob job) => Sanitation.Complete(job.FacilitySlotId);

    public int CompleteTempleSupply(BuildJob job, int amount) => Temples.CompleteSupply(job, amount);

    public int CompleteBathFuel(BuildJob job, int amount) => Baths.CompleteFuel(job, amount);

    public void CompleteBathPump(BuildJob job) => Baths.CompletePump(job, Services);

    public void CompleteServicePreparation(BuildJob job) => PreparedServices.Complete(job, Services);

    public int CompleteVenueSupply(BuildJob job, int amount) =>
        FoodVenues.CompleteSupply(job, amount, Services);

    public int CompleteAsylumFoodSupply(BuildJob job, int amount) =>
        Asylums.CompleteSupply(job, amount);

    public int CompleteJanitorSupply(BuildJob job, int amount) =>
        Janitors.CompleteSupply(job, amount);

    public int CompleteHomeFurnitureSupply(BuildJob job, int amount) =>
        Housing.CompleteSupply(job, amount);

    public int CompletePastureLivestockSupply(BuildJob job, int amount) =>
        SpecialProduction.CompleteLivestockSupply(job, amount);

    public void CompleteTransportPreparation(BuildJob job) =>
        Logistics.CompleteTransportPreparation(job);

    public int CompleteKnowledgeSupply(BuildJob job, int amount) =>
        Knowledge.CompleteSupply(job, amount);

    public void CompleteKnowledgeWork(BuildJob job) =>
        Knowledge.CompleteWork(job, _rooms.FirstOrDefault(room => room.Id == job.RoomId));

    public bool CompleteLawProcess(BuildJob job) =>
        Law.CompleteProcess(job.FacilitySlotId, job.LawProcessType);

    public void CompleteMilitaryTraining(
        int citizenId, BuildJob job, CitizenPersonalStatsRuntime stats) =>
        Military.CompleteTraining(citizenId, job, stats);

    public bool CompleteArtilleryLoad(BuildJob job, ResourceLedger resources, double skill) =>
        Military.CompleteArtilleryLoad(job, resources, Logistics, skill);

    public bool VandalizeNearest(GridCoord cell, int radius = 100)
    {
        var room = _rooms.Where(value => value.State == RoomState.Operational && value.Cells.Count > 0)
            .Select(value => (Room: value, Distance: value.Cells.Min(index =>
            {
                var target = _world.FromIndex(index);
                return Math.Abs(target.X - cell.X) + Math.Abs(target.Z - cell.Z);
            })))
            .Where(value => value.Distance <= radius)
            .OrderBy(value => value.Distance)
            .Select(value => value.Room)
            .FirstOrDefault();
        if (room is null) return false;
        room.MaintenanceDebt += 1.0;
        room.Degradation = MaintenanceRuntime.VisibleDegradation(room.MaintenanceDebt);
        return true;
    }

    public bool BreakFurnitureAt(GridCoord cell)
    {
        var index = _world.CellToIndex(cell);
        var room = _rooms.FirstOrDefault(candidate => candidate.Cells.Contains(index));
        if (room is null) return false;
        var footprint = room.FurnitureFootprints.FirstOrDefault(item =>
            !item.Broken && item.Cells.Contains(index));
        if (footprint is null) return false;
        footprint.Broken = true;
        foreach (var furnitureIndex in footprint.Cells)
            _world.Data.SetFurnitureBroken(_world.FromIndex(furnitureIndex), true);
        room.State = RoomState.Building;
        return true;
    }

    public bool ServiceMatchesReligion(int roomId, string religion)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == roomId);
        var required = room is null ? "" : OriginalGameData.Current.Room(room.DefinitionKey)?.Religion ?? "";
        return string.IsNullOrWhiteSpace(required) || required.Equals(
            religion, StringComparison.OrdinalIgnoreCase);
    }

    public double ServiceQuality(RoomServiceInstanceRuntime service)
    {
        if (service.Rule.Need.Equals("TEMPLE", StringComparison.OrdinalIgnoreCase))
            return Temples.ServiceQuality(service.RoomId);
        if (service.Rule.Need.Equals("BATH", StringComparison.OrdinalIgnoreCase))
            return Baths.Quality(service.RoomId, service.Total);
        if (service.Rule.Need is "ARENA" or "ARENAG" or "SPEAKER" or "STAGE")
        {
            var room = _rooms.FirstOrDefault(candidate => candidate.Id == service.RoomId);
            return Activities.Quality(service.RoomId, room?.Employment.Maximum ?? 0);
        }
        if (service.Rule.Need.Equals("SHOPPING", StringComparison.OrdinalIgnoreCase))
            return Logistics.MarketQuality(service.RoomId);
        return service.Rule.DefaultValue;
    }

    public void ScheduleHospitalSupplies(JobBoard jobs, ResourceLedger resources)
    {
        var source = FindSupplyCell();
        if (source is null) return;
        foreach (var room in _rooms.Where(room => room.State == RoomState.Operational &&
                     RoomKey(room)?.Equals("_HOSPITAL", StringComparison.OrdinalIgnoreCase) == true))
        {
            var inputs = OriginalGameData.Current.Room("_HOSPITAL")?.Recipes
                .FirstOrDefault()?.Inputs ?? Array.Empty<IndustryAmount>();
            foreach (var cellIndex in room.Cells.Where(index =>
                         _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture)))
            {
                foreach (var input in inputs)
                {
                    if (!OriginalGameData.TryMapResource(input.Resource, out var resource)) continue;
                    var key = (cellIndex, resource);
                    var stored = room.InputStorage.GetValueOrDefault(key);
                    if (stored >= HospitalBedResourceMaximum) continue;
                    var amount = Math.Min(BuildJob.MaximumFetchAmount,
                        Math.Min(HospitalBedResourceMaximum - stored, resources.Get(resource)));
                    if (amount <= 0) continue;
                    jobs.Add(BuildJob.HospitalSupply(
                        _world.FromIndex(cellIndex), source.Value, room.Id, resource, amount));
                    break;
                }
            }
        }
    }

    public void ReconcileProductionJobs(IEnumerable<BuildJob> jobs)
    {
        var activeJobs = jobs.ToArray();
        Sanitation.Reconcile(activeJobs);
        Temples.Reconcile(activeJobs);
        Baths.Reconcile(activeJobs);
        _pendingProduction.Clear();
        foreach (var job in activeJobs)
        {
            if (job.Kind is not (BuildKind.Production or BuildKind.ProductionSupply) ||
                job.State is JobState.Completed or JobState.Cancelled) continue;
            RegisterPendingProduction(job.RoomId);
        }
        foreach (var room in _rooms)
        {
            room.Employment.SetEmployed(System.Math.Min(
                room.Employment.Needed, _pendingProduction.GetValueOrDefault(room.Id)));
            room.MaintenancePending = jobs.Any(job => job.Kind == BuildKind.Maintenance &&
                job.RoomId == room.Id && job.State is not (JobState.Completed or JobState.Cancelled));
            room.EquipmentSupplyPending = jobs.Any(job => job.Kind == BuildKind.EquipmentSupply &&
                job.RoomId == room.Id && job.State is not (JobState.Completed or JobState.Cancelled));
        }
        Activities.Reconcile(activeJobs, _rooms);
        PreparedServices.Reconcile(activeJobs, _rooms);
        FoodVenues.Reconcile(activeJobs);
        Asylums.Reconcile(activeJobs);
        Janitors.Reconcile(activeJobs);
        SpecialProduction.Reconcile(activeJobs);
    }

    public void TickMaintenance(double delta, JobBoard jobs, ResourceLedger resources)
    {
        var dayPart = delta / OriginalGameData.Current.SecondsPerDay;
        foreach (var room in _rooms)
        {
            var broken = room.FurnitureFootprints.FirstOrDefault(item => item.Broken);
            if (broken is not null)
            {
                if (!room.MaintenancePending)
                {
                    var repairAmounts = room.FurnitureRepairAmounts
                        .Where(pair => broken.Cells.Contains(pair.Key.Cell))
                        .GroupBy(pair => pair.Key.Resource)
                        .ToDictionary(group => group.Key, group => group.Max(pair => pair.Value));
                    var resource = repairAmounts.Count == 0
                        ? (ResourceKind?)null
                        : repairAmounts.OrderByDescending(pair => pair.Value)
                            .ThenBy(pair => pair.Key).First().Key;
                    if (jobs.Add(BuildJob.Maintenance(
                            _world.FromIndex(broken.Anchor), room.Id, resource)))
                        room.MaintenancePending = true;
                }
                continue;
            }
            if (room.State != RoomState.Operational) continue;
            var constructedResources = room.MaintenanceResourceAmounts.Values.Sum();
            var baseRate = OriginalGameData.Current.Room(RoomKey(room) ?? "")?.DegradeRate ?? 0.75;
            var rate = MaintenanceRuntime.RoomRate(
                1.0, baseRate, room.Isolation, constructedResources, room.Cells.Count);
            room.MaintenanceDebt += rate * dayPart;
            room.Degradation = MaintenanceRuntime.VisibleDegradation(room.MaintenanceDebt);
            TickEquipment(room, dayPart, jobs, resources);
            if (room.MaintenanceDebt < 1.0 || room.MaintenancePending) continue;
            foreach (var index in room.Cells)
            {
                var itemAmounts = room.FurnitureRepairAmounts
                    .Where(pair => pair.Key.Cell == index)
                    .ToDictionary(pair => pair.Key.Resource, pair => pair.Value);
                var resource = MaintenanceRuntime.SelectResource(
                    itemAmounts.Count == 0 ? room.MaintenanceResourceAmounts : itemAmounts,
                    itemAmounts.Count == 0 ? room.Cells.Count : 1, Random.Shared);
                if (!jobs.Add(BuildJob.Maintenance(
                        _world.FromIndex(index), room.Id, resource))) continue;
                room.MaintenancePending = true;
                break;
            }
        }
    }

    private void TickEquipment(
        RoomRecord room,
        double dayPart,
        JobBoard jobs,
        ResourceLedger resources)
    {
        var equipment = OriginalGameData.Current.WorkEquipment;
        var roomKey = RoomKey(room);
        if (equipment is null || roomKey is null) return;
        var maximum = OriginalGameData.Current.MaximumToolsPerWorker(roomKey);
        if (maximum <= 0) return;
        room.ToolTargetPerWorker = System.Math.Clamp(room.ToolTargetPerWorker, 0, maximum);
        var before = (int)room.ToolWearProgress;
        room.ToolWearProgress += room.ToolUnits * equipment.WearPerDay * dayPart;
        var expired = (int)room.ToolWearProgress - before;
        if (expired > 0) room.ToolUnits = System.Math.Max(0, room.ToolUnits - expired);
        var target = room.ToolTargetPerWorker * room.Employment.Employed;
        var missing = System.Math.Max(0, target - room.ToolUnits);
        if (missing == 0 || room.EquipmentSupplyPending || resources.Get(ResourceKind.Tools) == 0) return;
        var source = FindSupplyCell(ResourceKind.Tools);
        if (source is null) return;
        var amount = System.Math.Min(BuildJob.MaximumFetchAmount,
            System.Math.Min(missing, resources.Get(ResourceKind.Tools)));
        foreach (var index in room.Cells)
        {
            if (!jobs.Add(BuildJob.EquipmentSupply(
                    _world.FromIndex(index), source.Value, room.Id, ResourceKind.Tools, amount))) continue;
            room.EquipmentSupplyPending = true;
            break;
        }
    }

    public int CompleteEquipmentSupply(BuildJob job, int amount)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        if (room is null) return amount;
        var target = room.ToolTargetPerWorker * room.Employment.Employed;
        var accepted = System.Math.Min(amount, System.Math.Max(0, target - room.ToolUnits));
        room.ToolUnits += accepted;
        room.EquipmentSupplyPending = false;
        return amount - accepted;
    }

    public void AdjustToolTarget(RoomRecord room, int delta)
    {
        var key = RoomKey(room);
        if (key is null) return;
        var maximum = OriginalGameData.Current.MaximumToolsPerWorker(key);
        room.ToolTargetPerWorker = System.Math.Clamp(room.ToolTargetPerWorker + delta, 0, maximum);
    }

    private static int DefaultToolTarget(string roomKey)
    {
        var equipment = OriginalGameData.Current.WorkEquipment;
        if (equipment is null) return 0;
        return System.Math.Clamp(
            equipment.DefaultTarget, 0, OriginalGameData.Current.MaximumToolsPerWorker(roomKey));
    }

    private static double EquipmentMultiplier(RoomRecord room)
    {
        var equipment = OriginalGameData.Current.WorkEquipment;
        var target = room.ToolTargetPerWorker * room.Employment.Employed;
        var key = RoomKey(room);
        var targetMaximum = key is null
            ? 0
            : OriginalGameData.Current.MaximumToolsPerWorker(key) * room.Employment.Employed;
        if (equipment is null || target <= 0 || targetMaximum <= 0) return 1.0;
        var equipped = System.Math.Min(room.ToolUnits, target);
        var fillOfMaximum = System.Math.Clamp((double)equipped / targetMaximum, 0.0, 1.0);
        return equipment.Multiplicative
            ? System.Math.Max(0.0, 1.0 + (equipment.MaximumBoost - 1.0) * fillOfMaximum)
            : 1.0 + equipment.MaximumBoost * fillOfMaximum;
    }

    public void CompleteMaintenance(BuildJob job)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        if (room is null) return;
        var broken = room.FurnitureFootprints.FirstOrDefault(item =>
            item.Broken && item.Anchor == _world.CellToIndex(job.Cell));
        if (broken is not null)
        {
            broken.Broken = false;
            foreach (var index in broken.Cells)
                _world.Data.SetFurnitureBroken(_world.FromIndex(index), false);
            room.MaintenancePending = false;
            return;
        }
        room.MaintenanceDebt = System.Math.Max(0.0, room.MaintenanceDebt - 1.0);
        room.Degradation = MaintenanceRuntime.VisibleDegradation(room.MaintenanceDebt);
        room.MaintenancePending = false;
    }

    public void AccumulateExpectedDailyMaintenanceUse(IDictionary<ResourceKind, double> totals)
    {
        foreach (var room in _rooms)
        {
            if (room.State != RoomState.Operational) continue;
            var baseRate = OriginalGameData.Current.Room(RoomKey(room) ?? "")?.DegradeRate ?? 0.75;
            var isolationBoost = 1.0 + (1.0 - room.Isolation) * 2.0;
            foreach (var pair in room.MaintenanceResourceAmounts)
                AddMaintenanceUse(totals, pair.Key, pair.Value, baseRate, isolationBoost);
        }
    }

    private static void AddMaintenanceUse(
        IDictionary<ResourceKind, double> totals,
        ResourceKind resource,
        int constructedAmount,
        double baseRate,
        double isolationBoost)
    {
        if (constructedAmount <= 0) return;
        var rate = baseRate * isolationBoost * MaintenanceRuntime.ResourceRate * constructedAmount;
        totals.TryGetValue(resource, out var current);
        totals[resource] = current + rate;
    }

    public void CompleteProduction(
        BuildJob job,
        ResourceLedger resources,
        HaulingSystem hauling,
        JobBoard jobs,
        double travelSeconds)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        var recipe = room is null ? null : RuntimeRecipe(room);
        if (room is not null && recipe is not null)
        {
            room.Employment.RecordCompletedCycle(recipe.SourceWorkSeconds, travelSeconds);
            var industryMultiplier = IndustryRuntime.RoomBonus(
                room.Degradation, room.Employment.TotalEfficiency) * EquipmentMultiplier(room) *
                FurnisherStatRuntime.Efficiency(room) *
                TerrainProductionMultiplier(room, job.Cell) *
                SpecialProduction.ProductionMultiplier(room, job.Cell);
            industryMultiplier = GameSession.TitleBonuses.Apply($"ROOM_{RoomKey(room)}", industryMultiplier);
            var cellIndex = _world.CellToIndex(job.Cell);
            var requiredInputs = recipe.Inputs.ToDictionary(input => input.Key, input =>
                IndustryRuntime.PreviewAdvance(
                    room.IndustryInputProgress.GetValueOrDefault((room.RecipeIndex, input.Key)),
                    input.Value, recipe.WorkFactor, industryMultiplier));
            if (requiredInputs.Any(input => room.InputStorage.GetValueOrDefault(
                    (cellIndex, input.Key)) < input.Value))
            {
                job.ReleaseOutputReservations();
                CompletePending(job.RoomId);
                return;
            }
            var consumed = new Dictionary<ResourceKind, int>();
            foreach (var input in recipe.Inputs)
            {
                var progressKey = (room.RecipeIndex, input.Key);
                var progress = room.IndustryInputProgress.GetValueOrDefault(progressKey);
                consumed[input.Key] = IndustryRuntime.Advance(
                    ref progress, input.Value, recipe.WorkFactor, industryMultiplier);
                room.IndustryInputProgress[progressKey] = progress;
            }
            if (job.ProductionInputs.Count > 0)
                resources.FinalizeProduction(job, consumed); // v9-v11 in-flight compatibility
            else
                job.ResourceReserved = false;
            foreach (var input in consumed.Where(pair => pair.Value > 0))
            {
                var storageKey = (cellIndex, input.Key);
                room.InputStorage[storageKey] = System.Math.Max(
                    0, room.InputStorage.GetValueOrDefault(storageKey) - input.Value);
            }

            foreach (var outputRule in recipe.Outputs)
            {
                int output;
                if (outputRule.Key == recipe.PrimaryOutput)
                {
                    var progress = room.IndustryOutputProgress.GetValueOrDefault(room.RecipeIndex);
                    output = IndustryRuntime.Advance(
                        ref progress, outputRule.Value, recipe.WorkFactor, industryMultiplier);
                    room.IndustryOutputProgress[room.RecipeIndex] = progress;
                }
                else
                {
                    var progressKey = (room.RecipeIndex, outputRule.Key);
                    var progress = room.AdditionalOutputProgress.GetValueOrDefault(progressKey);
                    output = IndustryRuntime.Advance(
                        ref progress, outputRule.Value, recipe.WorkFactor, industryMultiplier);
                    room.AdditionalOutputProgress[progressKey] = progress;
                }
                var reservations = job.OutputReservations.Where(reservation =>
                    reservation.Slot.Resource == outputRule.Key).ToArray();
                var remainder = InternalStorage.Deposit(reservations, output);
                if (remainder > 0) hauling.Spawn(job.Cell, outputRule.Key, remainder, jobs);
            }
            job.ClearOutputReservations();
            SpecialProduction.RecordCompletedCycle(room, job.Cell);
            hauling.ScheduleRoomExports(room.Id, jobs);
        }
        else
        {
            // A removed or unknown room must not leak reserved inputs.
            resources.Refund(job);
            job.ReleaseOutputReservations();
        }
        var pending = _pendingProduction.GetValueOrDefault(job.RoomId);
        if (pending <= 1) _pendingProduction.Remove(job.RoomId);
        else _pendingProduction[job.RoomId] = pending - 1;
    }

    private double TerrainProductionMultiplier(RoomRecord room, GridCoord workCell)
    {
        var rule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
        if (rule is null) return 1.0;
        if (!string.IsNullOrWhiteSpace(rule.Minable))
        {
            var type = OriginalGameData.Current.MinableIndex(rule.Minable);
            if (type < 0 || _world.Data.MineralType(workCell) != type) return 0.0;
            return Math.Max(1.0 / 63.0, _world.Data.MineralAmount(workCell) / 63.0);
        }
        if (rule.Archetype != RoomArchetype.Agriculture || room.Cells.Count == 0) return 1.0;
        var fertility = room.Cells.Average(index => _world.Data.FertilityD(_world.FromIndex(index)));
        // RoomIrrigated receives its value from the shared pump/canal/drain graph.
        // Baseline terrain moisture remains useful, while supplied irrigation can lift
        // dry fields rather than replacing fertility or seasonal growth outright.
        var moisture = room.Cells.Average(index => _world.Data.Moisture(_world.FromIndex(index)) / 15.0);
        var irrigation = room.Cells.Average(index => Water.Irrigation(_world.FromIndex(index)));
        return fertility * (_weather?.CropGrowthMultiplier ?? 1.0) *
               (0.5 + 0.5 * Math.Max(moisture, irrigation));
    }

    public int CompleteProductionSupply(BuildJob job, int amount, double fetchSeconds)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        if (room is null) return amount;
        var key = (_world.CellToIndex(job.Cell), job.Resource);
        var stored = room.InputStorage.GetValueOrDefault(key);
        var accepted = System.Math.Min(amount, InputStorageMaximum - stored);
        room.InputStorage[key] = stored + accepted;
        var recipe = RuntimeRecipe(room);
        if (accepted > 0 && recipe is not null)
            room.Employment.RecordFetchCycle(recipe.SourceWorkSeconds, fetchSeconds);
        CompletePending(job.RoomId);
        return amount - accepted;
    }

    public int PickupProductionSupply(BuildJob job, ResourceLedger resources)
    {
        if (!Logistics.PickupProductionSupply(job)) return 0;
        if (resources.TryTake(job.Resource, job.ResourceAmount))
        {
            job.PickedUp = true;
            return job.ResourceAmount;
        }
        Logistics.ReturnProductionSupply(job);
        return 0;
    }

    public void RestoreProductionOutputReservations(BuildJob job)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        var recipe = room is null ? null : RuntimeRecipe(room);
        if (room is null || recipe is null) return;
        job.SetOutputReservations(ReserveProductionOutputs(room, recipe));
    }

    public void CancelProductionSupplyReservation(BuildJob job) =>
        Logistics.CancelProductionSupply(job);

    public void RestorePhysicalLogisticsReservations(
        IEnumerable<BuildJob> jobs, ResourceLedger resources)
    {
        Logistics.Reconcile(resources);
        foreach (var job in jobs.Where(job =>
                     (job.Kind is BuildKind.ProductionSupply or BuildKind.Haul or
                         BuildKind.RoomOutputHaul) &&
                     job.State is not (JobState.Completed or JobState.Cancelled)))
        {
            var restored = job.Kind == BuildKind.ProductionSupply
                ? Logistics.RestoreProductionSupplyReservation(job)
                : Logistics.RestoreStockpileReservation(job);
            if (restored) continue;
            if (job.Kind == BuildKind.ProductionSupply) resources.Refund(job);
            else if (job.Kind == BuildKind.RoomOutputHaul && !job.PickedUp)
                InternalStorage.At(job.RoomId, job.Cell)?.CancelPickup(job.OutputAmount);
            job.State = JobState.Cancelled;
        }
    }

    private void CompletePending(int roomId)
    {
        var pending = _pendingProduction.GetValueOrDefault(roomId);
        if (pending <= 1) _pendingProduction.Remove(roomId);
        else _pendingProduction[roomId] = pending - 1;
    }

    public void RegisterPendingProduction(int roomId) =>
        _pendingProduction[roomId] = _pendingProduction.GetValueOrDefault(roomId) + 1;

    public GridCoord? FindOperationalStorageCell()
    {
        var storage = _rooms.FirstOrDefault(room =>
            room.Type == RoomType.Storage && room.State == RoomState.Operational && room.Cells.Count > 0);
        return storage is null ? null : _world.FromIndex(storage.Cells.First());
    }

    public GridCoord? FindSupplyCell(ResourceKind? resource = null)
    {
        var storage = FindOperationalStorageCell();
        if (storage is not null) return storage;
        if (resource is { } kind)
            return _landingSupplies.TryGetValue(kind, out var exact) ? exact : null;
        return _landingSupplies.Values.Cast<GridCoord?>().FirstOrDefault();
    }

    public int StorageCapacity => Logistics.StockpileCapacity > 0
        ? Logistics.StockpileCapacity
        : _rooms.Where(room => room.Type == RoomType.Storage && room.State == RoomState.Operational)
            .Sum(room => room.Cells.Count * 10);

    public LogisticsRoomInstanceRuntime? LogisticsInstance(RoomRecord room) =>
        Logistics.Instance(room.Id);

    public bool AdjustStockpileCrates(RoomRecord room, ResourceKind resource, int delta) =>
        Logistics.AdjustStockpileCrates(room.Id, resource, delta);

    public bool AdjustStockpileCrateLimit(RoomRecord room, ResourceKind resource, int delta)
    {
        var instance = Logistics.Instance(room.Id);
        if (instance is null) return false;
        var current = instance.CrateLimit(resource) == 0
            ? instance.CrateCapacity
            : instance.CrateLimit(resource);
        var target = Math.Clamp(current + delta, 1, Math.Max(1, instance.CrateCapacity));
        return Logistics.SetStockpileCrateLimit(
            room.Id, resource, target == instance.CrateCapacity ? 0 : target);
    }

    public bool ToggleStockpileFetching(RoomRecord room)
    {
        var instance = Logistics.Instance(room.Id);
        return instance is not null && Logistics.SetPolicy(
            room.Id, !instance.Fetching, instance.Priority, instance.TargetFraction);
    }

    public bool AdjustStockpilePriority(RoomRecord room, int delta)
    {
        var instance = Logistics.Instance(room.Id);
        return instance is not null && Logistics.SetPolicy(
            room.Id, instance.Fetching, Math.Clamp(instance.Priority + delta, 0, 10),
            instance.TargetFraction);
    }

    public bool CanExportInternalStorage(int roomId) => _rooms.Any(room =>
        room.Id == roomId && room.Type != RoomType.Storage && room.State == RoomState.Operational);

    /// <summary>Returns the data-driven blueprint key for an operational room instance.</summary>
    public string DefinitionKey(int roomId) => _rooms.FirstOrDefault(room => room.Id == roomId)?.DefinitionKey ?? "";

    public IEnumerable<(RoomServiceInstanceRuntime Service, GridCoord Cell, int Distance)>
        ServiceCandidates(GridCoord origin, bool ignoreRadius = false)
    {
        foreach (var service in Services.AvailableInstances)
        {
            if (!_instances.TryGetValue(service.RoomId, out var instance) || !instance.Active) continue;
            if (!Activities.IsOpen(service.RoomId, _serviceDayFraction)) continue;
            var cell = GridCoord.Cardinal.Select(offset => instance.Anchor + offset)
                .Where(candidate => _world.IsInside(candidate) && !_world.Data.IsBlocked(candidate))
                .Select(candidate => (GridCoord?)candidate).FirstOrDefault();
            if (cell is null) continue;
            var distance = Math.Abs(cell.Value.X - origin.X) + Math.Abs(cell.Value.Z - origin.Z);
            if (ignoreRadius || distance <= service.Rule.Radius)
                yield return (service, cell.Value, distance);
        }
    }

    public int RequiredWorkers(WorkProfession profession) => profession switch
    {
        WorkProfession.Baker => _rooms.Where(room =>
            room.Type == RoomType.Bakery && room.State == RoomState.Operational)
            .Sum(EffectiveWorkerLimit),
        WorkProfession.Carpenter => _rooms.Where(room =>
            room.State == RoomState.Operational &&
            Blueprints.Get(RoomKey(room) ?? "")?.Rule.Archetype == RoomArchetype.Industry)
            .Sum(EffectiveWorkerLimit),
        WorkProfession.Farmer => _rooms.Where(room =>
            room.State == RoomState.Operational &&
            Blueprints.Get(RoomKey(room) ?? "")?.Rule.Archetype == RoomArchetype.Agriculture)
            .Sum(EffectiveWorkerLimit),
        WorkProfession.Miner => _rooms.Where(room =>
            room.State == RoomState.Operational &&
            Blueprints.Get(RoomKey(room) ?? "")?.Rule.Archetype == RoomArchetype.Extraction)
            .Sum(EffectiveWorkerLimit),
        WorkProfession.Undertaker => _rooms.Where(room =>
            room.State == RoomState.Operational &&
            RoomKey(room) is "GRAVEYARD_NORMAL" or "TOMB_NORMAL")
            .Sum(room => room.Employment.Needed),
        WorkProfession.Scholar => _rooms.Where(room => room.State == RoomState.Operational &&
                OriginalGameData.Current.Room(RoomKey(room) ?? "")?.Knowledge?.Kind is
                    "LABORATORY" or "LIBRARY")
            .Sum(room => room.Employment.Needed),
        WorkProfession.Guard => _rooms.Where(room => room.State == RoomState.Operational &&
                (IsLawRoom(RoomKey(room)) ||
                 OriginalGameData.Current.MilitaryRooms.GetValueOrDefault(RoomKey(room) ?? "")?.Kind == "ARTILLERY"))
            .Sum(room => room.Employment.Needed),
        WorkProfession.Administrator => _rooms.Where(room => room.State == RoomState.Operational &&
                OriginalGameData.Current.Room(RoomKey(room) ?? "")?.Knowledge?.Kind is
                    "ADMINISTRATION" or "DIPLOMACY")
            .Sum(room => room.Employment.Needed),
        _ => 0
    };

    public IEnumerable<(string Key, int Employed, double AccidentsPerYear,
        double ShiftOffset, int RoomId, GridCoord Cell)> AccidentCandidates()
    {
        foreach (var group in _rooms.Where(room => room.State == RoomState.Operational)
                     .GroupBy(room => RoomKey(room) ?? ""))
        {
            var rule = OriginalGameData.Current.Room(group.Key);
            if (rule is null || rule.Work.AccidentsPerYear <= 0) continue;
            var rooms = group.Where(room => room.Employment.Employed > 0).ToArray();
            if (rooms.Length == 0) continue;
            var selected = rooms[0];
            yield return (group.Key, rooms.Sum(room => room.Employment.Employed),
                rule.Work.AccidentsPerYear, rule.Work.ShiftOffset, selected.Id,
                _instances[selected.Id].Anchor);
        }
    }

    public RoomRecord? FindAt(GridCoord cell)
    {
        var index = _world.CellToIndex(cell);
        return _rooms.FirstOrDefault(room => room.Cells.Contains(index));
    }

    public int EffectiveWorkerLimit(RoomRecord room)
    {
        var rule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
        if (room.State != RoomState.Operational || room.Type == RoomType.Storage &&
            rule?.Logistics is null) return 0;
        if (rule?.SpecialProduction is not null || rule?.Logistics is not null ||
            rule?.Knowledge is not null || IsLawRoom(RoomKey(room)))
            return room.WorkerLimit < 0
                ? room.Employment.Maximum
                : System.Math.Min(room.WorkerLimit, room.Employment.Maximum);
        var stations = rule?.HasIndustry == true
            ? WorkstationCount(room)
            : room.Cells.Count(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture));
        return room.WorkerLimit < 0 ? stations : System.Math.Min(room.WorkerLimit, stations);
    }

    public int WorkstationCount(RoomRecord room) => room.Cells.Count(index =>
        _world.Data.FurnitureWorkstation(_world.FromIndex(index)));

    public int CompletedWallCount(RoomRecord room) => room.RequiredWalls.Count(index =>
        _world.Data.Has(_world.FromIndex(index), TileFlags.Wall));

    public int CompletedDoorCount(RoomRecord room) => room.RequiredDoors.Count(index =>
        _world.Data.Has(_world.FromIndex(index), TileFlags.Door));

    public string RecipeDescription(RoomRecord room)
    {
        var recipe = RuntimeRecipe(room);
        if (recipe is null) return "Исходный рецепт недоступен";
        var inputs = string.Join(" + ", recipe.Inputs.Select(pair => $"{pair.Value:0.###} {pair.Key}"));
        var rule = OriginalGameData.Current.Room(RoomKey(room) ?? "");
        var shift = rule?.Work.ShiftOffset ?? 0;
        var outputs = string.Join(" + ", recipe.Outputs.Select(pair => $"{pair.Value:0.###} {pair.Key}"));
        return $"{inputs} → {outputs} / day-rate; " +
               $"cycle {recipe.SourceWorkSeconds:0}s; shift {shift * 24:0.#}h; " +
               $"eff {room.Employment.TotalEfficiency:0.##}; degrade {room.Degradation:P0}";
    }

    public string InputStorageDescription(RoomRecord room)
    {
        var recipe = RuntimeRecipe(room);
        if (recipe is null) return "";
        return string.Join(", ", recipe.Inputs.Keys.Select(resource =>
            $"{resource} {room.InputStorage.Where(pair => pair.Key.Resource == resource).Sum(pair => pair.Value)}"));
    }

    public int RecipeCount(RoomRecord room)
    {
        var key = RoomKey(room);
        return key is null ? 0 : OriginalGameData.Current.Room(key)?.Recipes.Count ?? 0;
    }

    public void CycleRecipe(RoomRecord room, JobBoard jobs, ResourceLedger resources)
    {
        var count = RecipeCount(room);
        if (count <= 1) return;
        jobs.TrimProductionForRoom(room.Id, 0, resources);
        _pendingProduction.Remove(room.Id);
        room.RecipeIndex = (room.RecipeIndex + 1) % count;
    }

    public void AdjustWorkerLimit(RoomRecord room, int delta, JobBoard jobs, ResourceLedger resources)
    {
        if (room.Type == RoomType.Storage) return;
        var current = room.WorkerLimit < 0 ? EffectiveWorkerLimit(room) : room.WorkerLimit;
        var capacity = OriginalGameData.Current.Room(RoomKey(room) ?? "")?.SpecialProduction is not null
            ? room.Employment.Maximum
            : room.Cells.Count(index =>
                _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture));
        room.WorkerLimit = System.Math.Clamp(current + delta, 0, capacity);
        var retained = jobs.TrimProductionForRoom(room.Id, room.WorkerLimit, resources);
        if (retained == 0) _pendingProduction.Remove(room.Id);
        else _pendingProduction[room.Id] = retained;
    }

    public bool Contains(GridCoord cell) =>
        _rooms.Any(room => room.Cells.Contains(_world.CellToIndex(cell)));

    public bool Dismantle(
        int roomId,
        JobBoard jobs,
        ResourceLedger resources,
        Action<ResourceKind, int, GridCoord>? spill = null)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == roomId);
        if (room is null) return false;
        foreach (var cell in room.RequiredWalls.Select(_world.FromIndex).Concat(
                     room.Cells.Select(_world.FromIndex)))
            jobs.Cancel(cell, resources);
        jobs.CancelRoom(roomId, resources);
        foreach (var item in InternalStorage.ClearRoom(roomId))
            spill?.Invoke(item.Resource, item.Amount, item.Cell);
        foreach (var item in Logistics.ClearRoom(roomId))
            spill?.Invoke(item.Resource, item.Amount, item.Cell);
        Housing.RemoveRoom(roomId);
        Hospitality.RemoveRoom(roomId);
        Construction.Remove(roomId);
        foreach (var index in room.Cells)
        {
            var cell = _world.FromIndex(index);
            _world.RemoveFurniture(cell);
            _world.RemoveRoomFloor(cell);
            if (!_world.Data.Has(cell, TileFlags.Cave)) _world.Data.SetRoof(cell, false);
            _world.ClearZone(cell);
        }
        foreach (var index in room.RequiredWalls)
        {
            var wall = _world.FromIndex(index);
            if (!_rooms.Any(other => other.Id != roomId && other.RequiredWalls.Contains(index)))
                _world.RemoveWall(wall);
            if (!_rooms.Any(other => other.Id != roomId && other.Cells.Contains(index)))
                _world.RemoveDoor(wall);
        }
        foreach (var index in room.RequiredDoors)
            if (!_rooms.Any(other => other.Id != roomId && other.RequiredDoors.Contains(index)))
                _world.RemoveDoor(_world.FromIndex(index));
        foreach (var cell in room.Cells.Select(_world.FromIndex))
            foreach (var edge in GridCoord.Cardinal.Select(offset => cell + offset))
                if (_world.IsInside(edge) && _world.Data.Has(edge, TileFlags.Door) &&
                    !_rooms.Any(other => other.Id != roomId &&
                        (other.Cells.Contains(_world.CellToIndex(edge)) ||
                         other.RequiredDoors.Contains(_world.CellToIndex(edge)))))
                    _world.RemoveDoor(edge);
        _instances.Remove(roomId);
        _rooms.Remove(room);
        UpdateStates();
        return true;
    }

    public RoomRecord Restore(
        RoomType type,
        IEnumerable<int> cells,
        IEnumerable<int> requiredWalls,
        int workerLimit = -1,
        int recipeIndex = 0,
        IReadOnlyDictionary<(int Recipe, ResourceKind Resource), double>? inputProgress = null,
        IReadOnlyDictionary<int, double>? outputProgress = null,
        IReadOnlyDictionary<(int Cell, ResourceKind Resource), int>? inputStorage = null,
        IReadOnlyDictionary<(int Recipe, ResourceKind Resource), double>? additionalOutputProgress = null,
        int roomId = 0,
        string definitionKey = "",
        int requiredFurniture = -1,
        int upgradeLevel = 0,
        double isolation = 1,
        double degradation = 0,
        double maintenanceDebt = 0,
        int toolTargetPerWorker = 0,
        int toolUnits = 0,
        double toolWearProgress = 0,
        IReadOnlyDictionary<int, double>? itemGroupAmounts = null,
        IReadOnlyDictionary<ResourceKind, int>? maintenanceResourceAmounts = null,
        IReadOnlyDictionary<(int Cell, ResourceKind Resource), int>? furnitureRepairAmounts = null,
        IReadOnlyList<RoomFurnitureFootprint>? furnitureFootprints = null,
        IEnumerable<int>? requiredDoors = null)
    {
        var restoredDefinition = string.IsNullOrWhiteSpace(definitionKey)
            ? LegacyRoomKey(type) ?? ""
            : definitionKey;
        var availableRecipes = OriginalGameData.Current.Room(restoredDefinition)?.Recipes.Count ?? RecipeCount(type);
        var room = new RoomRecord
        {
            Id = roomId > 0 ? roomId : _nextId,
            Type = type,
            DefinitionKey = restoredDefinition,
            Cells = cells.ToHashSet(),
            RequiredWalls = requiredWalls.ToHashSet(),
            RequiredDoors = requiredDoors?.ToHashSet() ?? new HashSet<int>(),
            RequiredFurniture = requiredFurniture >= 0
                ? requiredFurniture
                : type == RoomType.Storage ? 0 : 1,
            WorkerLimit = workerLimit,
            RecipeIndex = availableRecipes > 0
                ? System.Math.Clamp(recipeIndex, 0, availableRecipes - 1)
                : 0,
            IndustryInputProgress = inputProgress is null
                ? new Dictionary<(int Recipe, ResourceKind Resource), double>()
                : new Dictionary<(int Recipe, ResourceKind Resource), double>(inputProgress),
            IndustryOutputProgress = outputProgress is null
                ? new Dictionary<int, double>()
                : new Dictionary<int, double>(outputProgress),
            AdditionalOutputProgress = additionalOutputProgress is null
                ? new Dictionary<(int Recipe, ResourceKind Resource), double>()
                : new Dictionary<(int Recipe, ResourceKind Resource), double>(additionalOutputProgress),
            InputStorage = inputStorage is null
                ? new Dictionary<(int Cell, ResourceKind Resource), int>()
                : new Dictionary<(int Cell, ResourceKind Resource), int>(inputStorage),
            UpgradeLevel = Math.Max(0, upgradeLevel),
            Isolation = Math.Clamp(isolation, 0, 1),
            Degradation = Math.Clamp(degradation, 0, 1),
            MaintenanceDebt = Math.Max(0, maintenanceDebt),
            ToolTargetPerWorker = Math.Max(0, toolTargetPerWorker),
            ToolUnits = Math.Max(0, toolUnits),
            ToolWearProgress = Math.Max(0, toolWearProgress),
            ItemGroupAmounts = itemGroupAmounts is null
                ? new Dictionary<int, double>()
                : new Dictionary<int, double>(itemGroupAmounts),
            MaintenanceResourceAmounts = maintenanceResourceAmounts is null
                ? new Dictionary<ResourceKind, int>()
                : new Dictionary<ResourceKind, int>(maintenanceResourceAmounts),
            FurnitureRepairAmounts = furnitureRepairAmounts is null
                ? new Dictionary<(int Cell, ResourceKind Resource), int>()
                : new Dictionary<(int Cell, ResourceKind Resource), int>(furnitureRepairAmounts),
            FurnitureFootprints = furnitureFootprints is null
                ? new List<RoomFurnitureFootprint>()
                : furnitureFootprints.Select(item => new RoomFurnitureFootprint
                {
                    Anchor = item.Anchor,
                    Group = item.Group,
                    Variant = item.Variant,
                    Rotation = item.Rotation,
                    Cells = item.Cells.ToHashSet(),
                    Broken = item.Broken
                }).ToList(),
            State = RoomState.Building
        };
        _nextId = Math.Max(_nextId, room.Id + 1);
        _rooms.Add(room);
        foreach (var footprint in room.FurnitureFootprints.Where(item => item.Broken))
            foreach (var index in footprint.Cells)
                _world.Data.SetFurnitureBroken(_world.FromIndex(index), true);
        foreach (var footprint in room.FurnitureFootprints.Where(item =>
                     item.Cells.All(index => _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture))))
            _world.AddFurnitureVisual(new FurnitureVisualPlacement(room.DefinitionKey,
                footprint.Group, footprint.Variant, footprint.Rotation,
                _world.FromIndex(footprint.Anchor)));
        RegisterInstance(room);
        return room;
    }

    public RoomInstanceRuntime? RuntimeInstance(int roomId) => _instances.GetValueOrDefault(roomId);

    public GridCoord? ConstructionDeliveryCell(RoomRecord room, JobBoard jobs)
    {
        foreach (var index in room.Cells.OrderBy(index => index))
        {
            var cell = _world.FromIndex(index);
            if (jobs.GetAt(cell) is null) return cell;
        }
        var area = room.Cells.Select(_world.FromIndex).ToHashSet();
        foreach (var cell in area.OrderBy(cell => cell.Z).ThenBy(cell => cell.X))
        foreach (var direction in GridCoord.Cardinal)
        {
            var outside = cell + direction;
            if (!_world.IsInside(outside) || area.Contains(outside) ||
                _world.Data.IsBlocked(outside) || jobs.GetAt(outside) is not null) continue;
            return outside;
        }
        return null;
    }

    public int CompleteRoomConstructionSupply(BuildJob job, int amount) =>
        Construction.Deliver(job, amount);

    public int CompleteHospitalSupply(BuildJob job, int amount)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == job.RoomId);
        if (room is null) return amount;
        var key = (_world.CellToIndex(job.Cell), job.Resource);
        var stored = room.InputStorage.GetValueOrDefault(key);
        var accepted = Math.Min(amount, HospitalBedResourceMaximum - stored);
        room.InputStorage[key] = stored + accepted;
        return amount - accepted;
    }

    public double ConsumeHospitalSupplies(int roomId)
    {
        var room = _rooms.FirstOrDefault(candidate => candidate.Id == roomId);
        if (room is null) return 0;
        var quality = 1.0;
        var inputs = OriginalGameData.Current.Room("_HOSPITAL")?.Recipes
            .FirstOrDefault()?.Inputs ?? Array.Empty<IndustryAmount>();
        foreach (var input in inputs)
        {
            if (!OriginalGameData.TryMapResource(input.Resource, out var resource)) continue;
            var key = room.InputStorage.Keys.FirstOrDefault(candidate =>
                candidate.Resource == resource && room.InputStorage.GetValueOrDefault(candidate) > 0);
            if (!room.InputStorage.TryGetValue(key, out var stored) || stored <= 0) continue;
            room.InputStorage[key] = stored - 1;
            quality += 1;
        }
        return Math.Clamp(1.0 - 0.8 / quality, 0, 1);
    }

    private void RegisterInstance(RoomRecord room)
    {
        var blueprint = Blueprints.Get(room.DefinitionKey);
        if (blueprint is null)
            throw new InvalidOperationException($"Unknown room definition: {room.DefinitionKey}");
        _instances.Add(room.Id, new RoomInstanceRuntime(_world, room, blueprint));
    }

    private BuildJob? CreateProductionJob(RoomRecord room, GridCoord cell, ResourceLedger resources)
    {
        var recipe = RuntimeRecipe(room);
        if (recipe is null) return null;
        var archetype = Blueprints.Get(RoomKey(room) ?? "")?.Rule.Archetype;
        var profession = room.Type == RoomType.Bakery
            ? WorkProfession.Baker
            : archetype switch
            {
                RoomArchetype.Agriculture => WorkProfession.Farmer,
                RoomArchetype.Extraction => WorkProfession.Miner,
                _ => WorkProfession.Carpenter
            };
        var cellIndex = _world.CellToIndex(cell);
        if (recipe.Inputs.Count > 0)
        {
            foreach (var input in recipe.Inputs)
            {
                var stored = room.InputStorage.GetValueOrDefault((cellIndex, input.Key));
                var required = recipe.ReservationInputs.GetValueOrDefault(input.Key, 1);
                if (stored >= required) continue;
                var amount = System.Math.Min(
                    InputFetchMaximum,
                    System.Math.Min(InputStorageMaximum - stored, resources.Get(input.Key)));
                if (amount <= 0) continue;
                var source = Logistics.ReserveProductionSupply(input.Key, cell, amount);
                if (source is null) continue;
                return BuildJob.ProductionSupply(
                    cell, source.Value.Cell, source.Value.RoomId, room.Id, profession,
                    input.Key, source.Value.Amount);
            }
        }
        if (recipe.ReservationInputs.Any(input =>
                room.InputStorage.GetValueOrDefault((cellIndex, input.Key)) < input.Value))
            return null;
        var outputReservations = ReserveProductionOutputs(room, recipe);
        if (outputReservations.Count == 0) return null;
        var job = BuildJob.Production(
            cell,
            room.Id,
            profession,
            new Dictionary<ResourceKind, int>(),
            recipe.PrimaryOutput,
            recipe.MaximumOutput(recipe.PrimaryOutput),
            (float)recipe.SourceWorkSeconds);
        job.SetOutputReservations(outputReservations);
        return job;
    }

    private IReadOnlyList<(RoomStorageSlot Slot, int Amount)> ReserveProductionOutputs(
        RoomRecord room, RuntimeRecipe recipe)
    {
        var reservations = new List<(RoomStorageSlot Slot, int Amount)>();
        foreach (var output in recipe.Outputs)
        {
            var reserved = InternalStorage.ReserveSpace(
                room.Id, output.Key, recipe.MaximumOutput(output.Key));
            if (reserved.Count > 0)
            {
                reservations.AddRange(reserved);
                continue;
            }
            InternalStorage.CancelSpace(reservations);
            return Array.Empty<(RoomStorageSlot Slot, int Amount)>();
        }
        return reservations;
    }

    private static string? LegacyRoomKey(RoomType type) => type switch
    {
        RoomType.Bakery => "REFINER_BAKERY",
        RoomType.Workshop => "WORKSHOP_CARPENTER",
        RoomType.Storage => "_STOCKPILE",
        _ => null
    };

    private static string? RoomKey(RoomRecord room) =>
        string.IsNullOrWhiteSpace(room.DefinitionKey) ? LegacyRoomKey(room.Type) : room.DefinitionKey;

    private int LogisticsMaximum(RoomRecord room, LogisticsRule rule, int furniture)
    {
        var crates = Math.Max(1, (int)Math.Ceiling(room.ItemGroupAmounts.Values.Sum()));
        return rule.Kind switch
        {
            "STOCKPILE" => crates * rule.WorkersPerCrate,
            "HAULER" => Math.Max(1, RoomSpan(room)) * rule.WorkersPerCrate,
            "TRANSPORT" => 16,
            "MILITARY_SUPPLY" => crates * rule.WorkersPerCrate,
            "EXPORT" or "MARKET" => crates * rule.WorkersPerCrate,
            _ => 0
        };
    }

    private static int LogisticsNeeded(RoomRecord room, LogisticsRule rule, int maximum)
    {
        if (maximum <= 0) return 0;
        var crates = Math.Max(1, (int)Math.Ceiling(room.ItemGroupAmounts.Values.Sum()));
        return rule.Kind switch
        {
            "HAULER" => Math.Max(1, crates),
            "TRANSPORT" => 1,
            "MILITARY_SUPPLY" => Math.Max(1, crates),
            "EXPORT" or "MARKET" => Math.Max(1,
                (int)Math.Ceiling(crates * rule.NeededWorkersPerCrate)),
            _ => maximum
        };
    }

    private int RoomSpan(RoomRecord room)
    {
        if (room.Cells.Count == 0) return 0;
        var cells = room.Cells.Select(_world.FromIndex).ToArray();
        return Math.Max(cells.Max(cell => cell.X) - cells.Min(cell => cell.X) + 1,
            cells.Max(cell => cell.Z) - cells.Min(cell => cell.Z) + 1);
    }

    private static bool IsLawRoom(string? key) => key is
        "_COURT" or "_GUARD" or "_POLICE" or "_PRISON" or
        "_STOCKADE" or "_STOCKS" or "_EXECUTION";

    private int LawMaximum(RoomRecord room, int furniture)
    {
        var key = RoomKey(room);
        return key switch
        {
            "_POLICE" => Math.Max(1, furniture * 3),
            "_PRISON" => Math.Max(1, (int)Math.Ceiling(furniture *
                SettlementLawRuntime.PrisonWorkersPerPrisoner)),
            "_STOCKS" or "_EXECUTION" => 0,
            _ => Math.Max(1, furniture)
        };
    }

    private int LawNeeded(RoomRecord room, int maximum) => RoomKey(room) switch
    {
        "_POLICE" => Math.Max(1, maximum / 3),
        _ => maximum
    };

    private static int RecipeCount(RoomType type)
    {
        var key = LegacyRoomKey(type);
        return key is null ? 0 : OriginalGameData.Current.Room(key)?.Recipes.Count ?? 0;
    }

    private static RuntimeRecipe? RuntimeRecipe(RoomRecord room)
    {
        var key = RoomKey(room);
        return key is null ? null : OriginalGameData.Current.RuntimeRecipe(key, room.RecipeIndex);
    }

    private static bool IsWorkTime(RoomRecord room, double dayFraction)
    {
        var key = RoomKey(room);
        var rule = key is null ? null : OriginalGameData.Current.Room(key);
        if (rule is null) return true;
        var start = rule.Work.ShiftOffset;
        const double duration = 0.5; // Humanoid.WORK_PER_DAY: 8 of 16 work ticks.
        var relative = (dayFraction - start + 1.0) % 1.0;
        return relative < duration;
    }
}
