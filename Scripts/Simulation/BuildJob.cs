using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.LegacyCompat;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.Simulation;

public enum BuildKind : byte
{
    Wall,
    Road,
    Furniture,
    Production,
    Haul,
    ProductionSupply,
    Maintenance,
    EquipmentSupply,
    RoomConstructionSupply,
    RoomOutputHaul,
    HospitalSupply,
    TempleSupply,
    BathFuel,
    BathPump,
    ActivityWork,
    ServicePreparation,
    VenueSupply,
    AsylumFoodSupply,
    JanitorSupply,
    HomeFurnitureSupply,
    PastureLivestockSupply,
    CorpseHaul,
    Sanitation,
    LogisticsTransfer,
    TransportPreparation,
    KnowledgeSupply,
    KnowledgeWork,
    LawProcess,
    MilitaryTraining,
    ArtilleryLoad,
    RoomFloor,
    RoomRoof,
    RoomClear,
    RoomDoor,
    // Appended to preserve byte values used by existing saves.
    Forage,
    ClearWood,
    ClearStone,
    ClearWater,
    DigTunnel
}

public enum JobState : byte
{
    Dormant,
    Reservable,
    Reserved,
    Blocked,
    Completed,
    Cancelled
}

public enum LawProcessKind : byte
{
    Hearing,
    Punishment
}

public enum BuildPhase : byte
{
    FetchingMaterials,
    DeliveringMaterials,
    Constructing,
    ClearingTerrain,
    ClearingVegetation,
    RemovingObstacle
}

public enum WorkProfession : byte
{
    Laborer,
    Baker,
    Carpenter,
    Farmer,
    Miner,
    Undertaker,
    Scholar,
    Guard,
    Administrator,
    Recruit
}

public enum JobPriority : byte
{
    Road,
    Construction,
    Furniture,
    Hauling,
    Production
}

public sealed class BuildJob
{
    // AIModule_Work.MAX_FETCH_AMOUNT = ROOM_STOCKPILE.MIN_CARRY - 1 = 6.
    public const int MaximumFetchAmount = 6;
    public const int MaximumFetchDistance = 250;
    public GridCoord Cell { get; }
    public BuildKind Kind { get; }
    public JobState State { get; set; } = JobState.Reservable;
    public ResourceKind Resource { get; private set; }
    public int ResourceAmount { get; private set; }
    public bool ResourceReserved { get; set; }
    public int ReservedAmount { get; private set; }
    public int DeliveredAmount { get; private set; }
    public BuildPhase Phase { get; set; }
    public ResourceKind OutputResource { get; private set; }
    public int OutputAmount { get; private set; }
    public bool OutputAlreadyAccounted { get; private set; }
    public IReadOnlyDictionary<ResourceKind, int> ProductionInputs => _productionInputs;
    public int RoomId { get; private set; }
    public int DestinationRoomId { get; private set; }
    public GridCoord Destination { get; private set; }
    public string RoadKey { get; private set; } = "DIRT";
    public string FloorKey { get; private set; } = "DIRT";
    public bool PickedUp { get; set; }
    public WorkProfession RequiredProfession { get; private set; } = WorkProfession.Laborer;
    public JobPriority Priority { get; set; }
    public float WorkLeft { get; set; } = 1.5f;
    public IReadOnlyList<(RoomStorageSlot Slot, int Amount)> OutputReservations =>
        _outputReservations;
    public IReadOnlyList<GridCoord> FurnitureCells => _furnitureCells;
    public IReadOnlyList<GridCoord> FurnitureBlockerCells => _furnitureBlockerCells;
    public IReadOnlyList<GridCoord> FurnitureReachableCells => _furnitureReachableCells;
    public IReadOnlyList<GridCoord> FurnitureWorkCells => _furnitureWorkCells;
    public IReadOnlyList<GridCoord> FurnitureStorageCells => _furnitureStorageCells;
    public IEnumerable<GridCoord> OccupiedCells =>
        Kind == BuildKind.Furniture ? _furnitureCells :
        Kind is BuildKind.RoomFloor or BuildKind.RoomRoof or BuildKind.RoomClear
            ? System.Array.Empty<GridCoord>() : new[] { Cell };
    public bool IsConstruction => Kind is BuildKind.Wall or BuildKind.Road or
        BuildKind.Furniture or BuildKind.RoomFloor or BuildKind.RoomRoof or
        BuildKind.RoomClear or BuildKind.RoomDoor;
    public bool IsHaulJob => Kind is BuildKind.Haul or BuildKind.RoomOutputHaul or
        BuildKind.LogisticsTransfer;
    public bool IsCorpseJob => Kind == BuildKind.CorpseHaul;
    public int CorpseId { get; private set; }
    public int FacilitySlotId { get; private set; }
    public LawProcessKind LawProcessType { get; private set; }
    public int MaterialsNeeded => System.Math.Max(0, ResourceAmount - DeliveredAmount);
    private readonly Dictionary<ResourceKind, int> _productionInputs = new();
    private readonly List<(RoomStorageSlot Slot, int Amount)> _outputReservations = new();
    private readonly List<GridCoord> _furnitureCells = new();
    private readonly List<GridCoord> _furnitureBlockerCells = new();
    private readonly List<GridCoord> _furnitureReachableCells = new();
    private readonly List<GridCoord> _furnitureWorkCells = new();
    private readonly List<GridCoord> _furnitureStorageCells = new();
    private float _constructionWorkTime;
    public bool ConstructionPreparationInitialized { get; private set; }
    public GridCoord ConstructionCell { get; private set; }

    public BuildJob(GridCoord cell, BuildKind kind)
    {
        Cell = cell;
        ConstructionCell = cell;
        Kind = kind;
        if (kind == BuildKind.Furniture) _furnitureCells.Add(cell);
        Priority = kind switch
        {
            BuildKind.Production or BuildKind.ProductionSupply => JobPriority.Production,
            BuildKind.EquipmentSupply or BuildKind.RoomConstructionSupply or
                BuildKind.HospitalSupply or BuildKind.TempleSupply or BuildKind.BathFuel => JobPriority.Hauling,
            BuildKind.Haul or BuildKind.RoomOutputHaul or BuildKind.LogisticsTransfer => JobPriority.Hauling,
            BuildKind.CorpseHaul => JobPriority.Hauling,
            BuildKind.RoomFloor or BuildKind.RoomRoof or BuildKind.RoomClear => JobPriority.Construction,
            BuildKind.Forage or BuildKind.ClearWood or BuildKind.ClearStone or
                BuildKind.ClearWater or BuildKind.DigTunnel => JobPriority.Construction,
            BuildKind.Sanitation => JobPriority.Production,
            BuildKind.BathPump => JobPriority.Production,
            BuildKind.ActivityWork => JobPriority.Production,
            BuildKind.ServicePreparation => JobPriority.Production,
            BuildKind.VenueSupply => JobPriority.Hauling,
            BuildKind.AsylumFoodSupply => JobPriority.Hauling,
            BuildKind.JanitorSupply => JobPriority.Hauling,
            BuildKind.HomeFurnitureSupply => JobPriority.Hauling,
            BuildKind.PastureLivestockSupply => JobPriority.Hauling,
            BuildKind.TransportPreparation => JobPriority.Production,
            BuildKind.KnowledgeSupply => JobPriority.Hauling,
            BuildKind.KnowledgeWork => JobPriority.Production,
            BuildKind.LawProcess => JobPriority.Production,
            BuildKind.MilitaryTraining or BuildKind.ArtilleryLoad => JobPriority.Production,
            BuildKind.Maintenance => JobPriority.Construction,
            BuildKind.Furniture => JobPriority.Furniture,
            BuildKind.Road => JobPriority.Road,
            _ => JobPriority.Construction
        };
        WorkLeft = kind switch
        {
            BuildKind.Wall => (float)OriginalGameData.Current.Structure("STONE").BuildTime,
            BuildKind.RoomDoor => (float)OriginalGameData.Current.Structure("STONE").BuildTime,
            BuildKind.Furniture or BuildKind.RoomFloor or BuildKind.RoomRoof => 10f,
            BuildKind.RoomClear => 0f,
            // JobClear.jobPerformTime() is 30. Rock overrides it with 5;
            // mountain tunnelling is race-dependent in Java and starts at 60.
            BuildKind.Forage or BuildKind.ClearWood or BuildKind.ClearWater => 30f,
            BuildKind.ClearStone => 5f,
            BuildKind.DigTunnel => 60f,
            BuildKind.Production => 3f,
            BuildKind.ProductionSupply => 0.1f,
            BuildKind.Haul or BuildKind.RoomOutputHaul or BuildKind.LogisticsTransfer => 0.1f,
            BuildKind.CorpseHaul => 25f,
            BuildKind.Sanitation => 45f,
            BuildKind.BathPump => 20f,
            BuildKind.ActivityWork => 10f,
            BuildKind.ServicePreparation => 20f,
            BuildKind.VenueSupply => 0.1f,
            BuildKind.AsylumFoodSupply => 25f,
            BuildKind.JanitorSupply => 0.1f,
            BuildKind.HomeFurnitureSupply => 0.1f,
            BuildKind.PastureLivestockSupply => 0.1f,
            BuildKind.TransportPreparation => 16f,
            BuildKind.KnowledgeSupply => 0.1f,
            BuildKind.KnowledgeWork => 45f,
            BuildKind.LawProcess => 20f,
            BuildKind.MilitaryTraining => 45f,
            BuildKind.ArtilleryLoad => 45f,
            BuildKind.Maintenance => 1.5f,
            BuildKind.EquipmentSupply or BuildKind.RoomConstructionSupply or
                BuildKind.HospitalSupply or BuildKind.TempleSupply or BuildKind.BathFuel => 0.1f,
            // JobBuildRoad.constructionTime() returns 25 in the Java source.
            _ => 25f
        };
        _constructionWorkTime = WorkLeft;
        Resource = kind == BuildKind.Furniture ? ResourceKind.Wood : ResourceKind.Stone;
        ResourceAmount = kind switch
        {
            BuildKind.Wall => OriginalGameData.Current.Structure("STONE").ResourceAmount,
            BuildKind.Road => OriginalGameData.Current.DefaultRoad().ResourceAmount,
            BuildKind.Furniture => 2,
            _ => 1
        };
        if (kind == BuildKind.Wall && OriginalGameData.TryMapResource(
                OriginalGameData.Current.Structure("STONE").Resource, out var wallResource))
            Resource = wallResource;
        if (kind == BuildKind.Road && OriginalGameData.TryMapResource(
                OriginalGameData.Current.DefaultRoad().Resource, out var roadResource))
            Resource = roadResource;
        if (kind is BuildKind.Haul or BuildKind.RoomOutputHaul or BuildKind.LogisticsTransfer)
            ResourceAmount = 0;
        if (kind is BuildKind.Maintenance or BuildKind.Forage or BuildKind.ClearWood or
            BuildKind.ClearStone or BuildKind.ClearWater or BuildKind.DigTunnel)
            ResourceAmount = 0;
        Phase = IsConstruction && ResourceAmount > 0
            ? BuildPhase.FetchingMaterials
            : BuildPhase.Constructing;
    }

    public static BuildJob Maintenance(
        GridCoord cell, int roomId, ResourceKind? resource = null) =>
        new(cell, BuildKind.Maintenance)
        {
            RoomId = roomId,
            Resource = resource ?? ResourceKind.Stone,
            ResourceAmount = resource is null ? 0 : 1,
            WorkLeft = 1.5f
        };

    public void SetOutputReservations(
        IEnumerable<(RoomStorageSlot Slot, int Amount)> reservations)
    {
        _outputReservations.Clear();
        _outputReservations.AddRange(reservations);
    }

    public void ClearOutputReservations() => _outputReservations.Clear();

    public void SetResourceSource(GridCoord source) => Destination = source;

    public void RestorePersistentData(
        ResourceKind resource, int resourceAmount,
        ResourceKind outputResource, int outputAmount,
        int roomId, int destinationRoomId, GridCoord destination,
        int corpseId, int facilitySlotId, LawProcessKind lawProcessType,
        WorkProfession requiredProfession, bool pickedUp, bool outputAlreadyAccounted)
    {
        Resource = resource;
        ResourceAmount = System.Math.Max(0, resourceAmount);
        OutputResource = outputResource;
        OutputAmount = System.Math.Max(0, outputAmount);
        RoomId = roomId;
        DestinationRoomId = destinationRoomId;
        Destination = destination;
        CorpseId = corpseId;
        FacilitySlotId = facilitySlotId;
        LawProcessType = lawProcessType;
        RequiredProfession = requiredProfession;
        PickedUp = pickedUp;
        OutputAlreadyAccounted = outputAlreadyAccounted;
    }

    public void ReleaseOutputReservations()
    {
        foreach (var reservation in _outputReservations)
            reservation.Slot.CancelSpace(reservation.Amount);
        _outputReservations.Clear();
    }

    public static BuildJob RoadMaintenance(GridCoord cell, ResourceKind? resource)
    {
        var job = Maintenance(cell, 0);
        if (resource is null) return job;
        job.Resource = resource.Value;
        job.ResourceAmount = 1;
        return job;
    }

    public static BuildJob EquipmentSupply(
        GridCoord roomCell,
        GridCoord source,
        int roomId,
        ResourceKind resource,
        int amount) =>
        new(roomCell, BuildKind.EquipmentSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            WorkLeft = 0.1f
        };

    public static BuildJob RoomConstructionSupply(
        GridCoord roomCell,
        GridCoord source,
        int roomId,
        ResourceKind resource,
        int amount) =>
        new(roomCell, BuildKind.RoomConstructionSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob RoomFurniture(
        GridCoord cell,
        int roomId,
        IEnumerable<GridCoord>? footprint = null,
        IEnumerable<GridCoord>? blockers = null,
        IEnumerable<GridCoord>? reachable = null,
        IEnumerable<GridCoord>? work = null,
        IEnumerable<GridCoord>? storage = null)
    {
        var job = new BuildJob(cell, BuildKind.Furniture)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            Phase = BuildPhase.Constructing,
            State = JobState.Dormant
        };
        job.RestoreFurnitureCells(footprint ?? new[] { cell }, blockers, reachable, work, storage);
        return job;
    }

    public void RestoreFurnitureCells(
        IEnumerable<GridCoord> cells,
        IEnumerable<GridCoord>? blockers = null,
        IEnumerable<GridCoord>? reachable = null,
        IEnumerable<GridCoord>? work = null,
        IEnumerable<GridCoord>? storage = null)
    {
        _furnitureCells.Clear();
        _furnitureCells.AddRange(cells.Distinct());
        if (_furnitureCells.Count == 0) _furnitureCells.Add(Cell);
        var footprint = _furnitureCells.ToHashSet();
        _furnitureBlockerCells.Clear();
        _furnitureBlockerCells.AddRange((blockers ?? _furnitureCells).Where(footprint.Contains).Distinct());
        _furnitureReachableCells.Clear();
        _furnitureReachableCells.AddRange((reachable ?? System.Array.Empty<GridCoord>())
            .Where(footprint.Contains).Distinct());
        _furnitureWorkCells.Clear();
        _furnitureWorkCells.AddRange((work ?? System.Array.Empty<GridCoord>())
            .Where(footprint.Contains).Distinct());
        _furnitureStorageCells.Clear();
        _furnitureStorageCells.AddRange((storage ?? System.Array.Empty<GridCoord>())
            .Where(footprint.Contains).Distinct());
    }

    public static BuildJob RoomWall(GridCoord cell, int roomId, string structureKey = "STONE") =>
        ConfigureStructure(new BuildJob(cell, BuildKind.Wall)
        {
            RoomId = roomId,
            State = JobState.Dormant
        }, structureKey);

    public static BuildJob RoomDoor(GridCoord cell, int roomId, string structureKey = "STONE") =>
        ConfigureStructure(new BuildJob(cell, BuildKind.RoomDoor)
        {
            RoomId = roomId,
            State = JobState.Dormant
        }, structureKey);

    private static BuildJob ConfigureStructure(BuildJob job, string structureKey)
    {
        var structure = OriginalGameData.Current.Structure(structureKey);
        job.ResourceAmount = structure.ResourceAmount;
        if (OriginalGameData.TryMapResource(structure.Resource, out var resource))
            job.Resource = resource;
        job.WorkLeft = (float)structure.BuildTime;
        job._constructionWorkTime = job.WorkLeft;
        job.Phase = job.ResourceAmount > 0 ? BuildPhase.FetchingMaterials : BuildPhase.Constructing;
        return job;
    }

    public static BuildJob RoomFloor(GridCoord cell, int roomId, string floorKey) =>
        new(cell, BuildKind.RoomFloor)
        {
            RoomId = roomId,
            FloorKey = floorKey,
            State = JobState.Dormant
        };

    public static BuildJob RoomRoof(GridCoord cell, int roomId) =>
        new(cell, BuildKind.RoomRoof)
        {
            RoomId = roomId,
            State = JobState.Dormant
        };

    public static BuildJob RoomClear(GridCoord cell, int roomId) =>
        new(cell, BuildKind.RoomClear) { RoomId = roomId };

    public void RestoreFloor(string floorKey)
    {
        if (Kind == BuildKind.RoomFloor) FloorKey = floorKey;
    }

    public static BuildJob HospitalSupply(
        GridCoord bedCell,
        GridCoord source,
        int roomId,
        ResourceKind resource,
        int amount) =>
        new(bedCell, BuildKind.HospitalSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob TempleSupply(
        GridCoord altarCell,
        GridCoord source,
        int roomId,
        ResourceKind resource,
        int amount) =>
        new(altarCell, BuildKind.TempleSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob BathFuel(
        GridCoord ovenCell,
        GridCoord source,
        int roomId,
        int amount) =>
        new(ovenCell, BuildKind.BathFuel)
        {
            Destination = source,
            RoomId = roomId,
            Resource = ResourceKind.Coal,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob BathPump(GridCoord crankCell, int roomId) =>
        new(crankCell, BuildKind.BathPump)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 20f
        };

    public static BuildJob ActivityWork(GridCoord station, int roomId) =>
        new(station, BuildKind.ActivityWork)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 10f
        };

    public static BuildJob ServicePreparation(GridCoord station, int roomId, float workSeconds) =>
        new(station, BuildKind.ServicePreparation)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = workSeconds
        };

    public static BuildJob VenueSupply(
        GridCoord venueCell, GridCoord source, int roomId, ResourceKind resource, int amount) =>
        new(venueCell, BuildKind.VenueSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    // Food.Job.JobPerformTime() is 25 seconds. Source food stations hold a
    // six-bit ration counter; AsylumRuntime owns that counter.
    public static BuildJob AsylumFoodSupply(
        GridCoord station, GridCoord source, int roomId, int amount) =>
        new(station, BuildKind.AsylumFoodSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = ResourceKind.Ration,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 25f
        };

    public static BuildJob JanitorSupply(
        GridCoord table, GridCoord source, int roomId, ResourceKind resource, int amount) =>
        new(table, BuildKind.JanitorSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob HomeFurnitureSupply(
        GridCoord homeCell, GridCoord source, int roomId, int citizenId,
        ResourceKind resource, int amount) =>
        new(homeCell, BuildKind.HomeFurnitureSupply)
        {
            Destination = source,
            RoomId = roomId,
            FacilitySlotId = citizenId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob PastureLivestockSupply(
        GridCoord pastureCell, GridCoord source, int roomId, int amount) =>
        new(pastureCell, BuildKind.PastureLivestockSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = ResourceKind.Livestock,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob RoomOutputHaul(
        GridCoord source,
        GridCoord destination,
        int roomId,
        int destinationRoomId,
        ResourceKind resource,
        int amount) =>
        new(source, BuildKind.RoomOutputHaul)
        {
            Destination = destination,
            RoomId = roomId,
            DestinationRoomId = destinationRoomId,
            OutputResource = resource,
            OutputAmount = amount,
            WorkLeft = 0.1f
        };

    public static BuildJob LogisticsTransfer(
        GridCoord source,
        GridCoord destination,
        int sourceRoomId,
        int destinationRoomId,
        ResourceKind resource,
        int amount) =>
        new(source, BuildKind.LogisticsTransfer)
        {
            Destination = destination,
            RoomId = sourceRoomId,
            DestinationRoomId = destinationRoomId,
            OutputResource = resource,
            OutputAmount = amount,
            WorkLeft = 0.1f
        };

    public static BuildJob TransportPreparation(GridCoord cell, int roomId) =>
        new(cell, BuildKind.TransportPreparation)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 16f
        };

    public static BuildJob KnowledgeSupply(
        GridCoord workCell, GridCoord source, int roomId, ResourceKind resource, int amount) =>
        new(workCell, BuildKind.KnowledgeSupply)
        {
            Destination = source,
            RoomId = roomId,
            Resource = resource,
            ResourceAmount = amount,
            RequiredProfession = WorkProfession.Laborer,
            WorkLeft = 0.1f
        };

    public static BuildJob KnowledgeWork(
        GridCoord workCell, int roomId,
        WorkProfession profession = WorkProfession.Scholar) =>
        new(workCell, BuildKind.KnowledgeWork)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = profession,
            WorkLeft = 45f
        };

    public static BuildJob LawProcess(
        GridCoord cell, int roomId, int citizenId, float seconds,
        LawProcessKind process = LawProcessKind.Punishment) =>
        new(cell, BuildKind.LawProcess)
        {
            RoomId = roomId,
            FacilitySlotId = citizenId,
            LawProcessType = process,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Guard,
            WorkLeft = seconds
        };

    public static BuildJob MilitaryTraining(GridCoord cell, int roomId) =>
        new(cell, BuildKind.MilitaryTraining)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Recruit,
            WorkLeft = 45f
        };

    public static BuildJob ArtilleryLoad(GridCoord cell, int roomId) =>
        new(cell, BuildKind.ArtilleryLoad)
        {
            RoomId = roomId,
            ResourceAmount = 0,
            RequiredProfession = WorkProfession.Guard,
            WorkLeft = 45f
        };

    public static BuildJob CorpseHaul(
        int corpseId,
        GridCoord source,
        GridCoord destination,
        int facilitySlotId,
        float workSeconds) =>
        new(source, BuildKind.CorpseHaul)
        {
            CorpseId = corpseId,
            Destination = destination,
            FacilitySlotId = facilitySlotId,
            RequiredProfession = workSeconds >= 25f
                ? WorkProfession.Undertaker
                : WorkProfession.Laborer,
            ResourceAmount = 0,
            WorkLeft = workSeconds
        };

    public static BuildJob Sanitation(int slotId, int roomId, GridCoord cell) =>
        new(cell, BuildKind.Sanitation)
        {
            FacilitySlotId = slotId,
            RoomId = roomId,
            RequiredProfession = WorkProfession.Laborer,
            ResourceAmount = 0,
            WorkLeft = 45f
        };

    public bool IsSupplyJob => Kind is BuildKind.ProductionSupply or BuildKind.EquipmentSupply or
        BuildKind.RoomConstructionSupply or BuildKind.HospitalSupply or BuildKind.TempleSupply or
        BuildKind.BathFuel or BuildKind.VenueSupply or BuildKind.AsylumFoodSupply or
        BuildKind.JanitorSupply or BuildKind.HomeFurnitureSupply or
        BuildKind.PastureLivestockSupply or BuildKind.KnowledgeSupply;

    public bool IsPreparationPhase => Phase is BuildPhase.ClearingTerrain or
        BuildPhase.ClearingVegetation or BuildPhase.RemovingObstacle;

    public void PrepareConstructionSite(WorldGridData world, System.Func<GridCoord, bool> hasObstacle)
    {
        if (!IsConstruction) return;
        ConstructionPreparationInitialized = true;
        var occupied = Kind is BuildKind.RoomFloor or BuildKind.RoomRoof or BuildKind.RoomClear
            ? new[] { Cell } : OccupiedCells.ToArray();
        var terrain = occupied.FirstOrDefault(cell => world.Has(cell, TileFlags.ClearableTerrain));
        var vegetation = occupied.FirstOrDefault(cell => world.Has(cell, TileFlags.Vegetation));
        var obstacle = occupied.FirstOrDefault(hasObstacle);
        if (occupied.Any(cell => world.Has(cell, TileFlags.ClearableTerrain)))
        {
            ConstructionCell = terrain;
            Phase = BuildPhase.ClearingTerrain;
            WorkLeft = world.Has(terrain, TileFlags.EasilyClearableTerrain) ? 2f : 20f;
        }
        else if (occupied.Any(cell => world.Has(cell, TileFlags.Vegetation)))
        {
            ConstructionCell = vegetation;
            Phase = BuildPhase.ClearingVegetation;
            WorkLeft = 2f;
        }
        else if (MaterialsNeeded > 0)
        {
            Phase = BuildPhase.FetchingMaterials;
            WorkLeft = 0f;
        }
        else if ((Kind is BuildKind.Wall or BuildKind.Furniture) && occupied.Any(hasObstacle))
        {
            ConstructionCell = obstacle;
            Phase = BuildPhase.RemovingObstacle;
            WorkLeft = 0f;
        }
        else
        {
            ConstructionCell = Cell;
            Phase = BuildPhase.Constructing;
            WorkLeft = _constructionWorkTime;
        }
    }

    public static BuildJob Road(GridCoord cell, FloorRule road)
    {
        var job = new BuildJob(cell, BuildKind.Road)
        {
            RoadKey = road.Key,
            ResourceAmount = road.ResourceAmount
        };
        if (road.ResourceAmount > 0 && OriginalGameData.TryMapResource(road.Resource, out var resource))
            job.Resource = resource;
        job.Phase = job.ResourceAmount > 0
            ? BuildPhase.FetchingMaterials
            : BuildPhase.Constructing;
        return job;
    }

    public void RestoreRoad(string roadKey)
    {
        if (Kind != BuildKind.Road) return;
        var road = OriginalGameData.Current.Floor(roadKey);
        RoadKey = road.Key;
        ResourceAmount = road.ResourceAmount;
        if (road.ResourceAmount > 0 && OriginalGameData.TryMapResource(road.Resource, out var resource))
            Resource = resource;
    }

    public static BuildJob Haul(
        GridCoord source, GridCoord destination, int destinationRoomId,
        ResourceKind resource, int amount,
        bool alreadyAccounted = false)
    {
        return new BuildJob(source, BuildKind.Haul)
        {
            Destination = destination,
            DestinationRoomId = destinationRoomId,
            OutputResource = resource,
            OutputAmount = amount,
            OutputAlreadyAccounted = alreadyAccounted,
            ResourceAmount = 0
        };
    }

    public static BuildJob Production(
        GridCoord cell,
        int roomId,
        WorkProfession profession,
        IReadOnlyDictionary<ResourceKind, int> inputs,
        ResourceKind output,
        int outputAmount,
        float workSeconds)
    {
        var job = new BuildJob(cell, BuildKind.Production)
        {
            RoomId = roomId,
            RequiredProfession = profession,
            OutputResource = output,
            OutputAmount = outputAmount,
            WorkLeft = workSeconds
        };
        job.SetProductionInputs(inputs);
        return job;
    }

    public static BuildJob ProductionSupply(
        GridCoord workCell,
        GridCoord source,
        int sourceRoomId,
        int roomId,
        WorkProfession profession,
        ResourceKind resource,
        int amount)
    {
        return new BuildJob(workCell, BuildKind.ProductionSupply)
        {
            Destination = source,
            DestinationRoomId = sourceRoomId,
            RoomId = roomId,
            RequiredProfession = profession,
            Resource = resource,
            ResourceAmount = amount,
            WorkLeft = 0.1f
        };
    }

    public void RestoreProductionData(
        int roomId,
        WorkProfession profession,
        IReadOnlyDictionary<ResourceKind, int> inputs,
        ResourceKind output,
        int outputAmount,
        float workSeconds)
    {
        RoomId = roomId;
        RequiredProfession = profession;
        OutputResource = output;
        OutputAmount = outputAmount;
        WorkLeft = workSeconds;
        SetProductionInputs(inputs);
    }

    private void SetProductionInputs(IReadOnlyDictionary<ResourceKind, int> inputs)
    {
        _productionInputs.Clear();
        foreach (var pair in inputs.Where(pair => pair.Value > 0))
            _productionInputs[pair.Key] = pair.Value;
        if (_productionInputs.Count == 0)
        {
            ResourceAmount = 0;
            return;
        }
        var primary = _productionInputs.First();
        Resource = primary.Key;
        ResourceAmount = primary.Value;
    }

    public void RestoreHaulData(GridCoord destination, ResourceKind resource, int amount, bool pickedUp)
    {
        Destination = destination;
        OutputResource = resource;
        OutputAmount = amount;
        ResourceAmount = 0;
        PickedUp = pickedUp;
    }

    public void RestoreProductionSupplyData(
        GridCoord source,
        int roomId,
        WorkProfession profession,
        ResourceKind resource,
        int amount,
        bool pickedUp)
    {
        Destination = source;
        RoomId = roomId;
        RequiredProfession = profession;
        Resource = resource;
        ResourceAmount = amount;
        PickedUp = pickedUp;
    }

    public int TakeReservedResource()
    {
        if (!ResourceReserved) return 0;
        ResourceReserved = false;
        PickedUp = true;
        return ResourceAmount;
    }

    public void ResetResourcePickup() => PickedUp = false;

    public void DeliverMaintenanceResource()
    {
        if (Kind != BuildKind.Maintenance) return;
        PickedUp = false;
        ResourceAmount = 0;
    }

    public void ReserveMaterials(int amount)
    {
        ReservedAmount = amount;
        ResourceReserved = amount > 0;
    }

    public int TakeReservedMaterials()
    {
        var amount = ReservedAmount;
        ReservedAmount = 0;
        ResourceReserved = false;
        return amount;
    }

    public void DeliverMaterials(int amount)
    {
        DeliveredAmount = System.Math.Min(ResourceAmount, DeliveredAmount + amount);
        Phase = MaterialsNeeded == 0 ? BuildPhase.Constructing : BuildPhase.FetchingMaterials;
    }

    public int TakeDeliveredMaterials()
    {
        var amount = DeliveredAmount;
        DeliveredAmount = 0;
        if (IsConstruction && ResourceAmount > 0) Phase = BuildPhase.FetchingMaterials;
        return amount;
    }

    public void RestoreConstructionData(
        int deliveredAmount,
        int reservedAmount,
        BuildPhase phase,
        GridCoord? constructionCell = null)
    {
        ConstructionPreparationInitialized = true;
        DeliveredAmount = SourceClamp.Integer(deliveredAmount, 0, ResourceAmount);
        ReservedAmount = SourceClamp.Integer(reservedAmount, 0, MaterialsNeeded);
        ResourceReserved = ReservedAmount > 0;
        Phase = MaterialsNeeded == 0 && phase is BuildPhase.FetchingMaterials or BuildPhase.DeliveringMaterials
            ? BuildPhase.Constructing
            : phase;
        ConstructionCell = constructionCell ?? Cell;
    }
}
