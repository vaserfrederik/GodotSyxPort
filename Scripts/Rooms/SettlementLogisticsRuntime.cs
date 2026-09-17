using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public enum LogisticsKind : byte
{
    Stockpile,
    Hauler,
    Transport,
    Export,
    Import,
    Market,
    MilitarySupply,
    Station
}

public sealed record LogisticsPolicySnapshot(
    int RoomId,
    bool Fetching,
    int Priority,
    double TargetFraction,
    byte[] Resources,
    int[] Crates,
    int[] Limits);

/// <summary>
/// Shared semantic runtime for ROOM_STOCKPILE, ROOM_HAULER, ROOM_TRANSPORT,
/// ROOM_EXPORT/IMPORT and ROOM_MARKET. The Java rooms use different furniture
/// classes, but share resource selection, capacity, pull orders and reservations.
/// </summary>
public sealed class LogisticsRoomInstanceRuntime
{
    private readonly Dictionary<ResourceKind, int> _amounts = new();
    private readonly Dictionary<ResourceKind, int> _reservedPickup = new();
    private readonly Dictionary<ResourceKind, int> _reservedSpace = new();
    private readonly Dictionary<ResourceKind, int> _crateAllocations = new();
    private readonly Dictionary<ResourceKind, int> _crateLimits = new();

    public int RoomId { get; }
    public LogisticsKind Kind { get; }
    public GridCoord Anchor { get; private set; }
    public int Capacity { get; private set; }
    public int TotalCrates { get; private set; }
    public int CrateCapacity { get; private set; }
    public int AllocatedCrates => _crateAllocations.Values.Sum();
    public int Radius { get; private set; }
    public int Priority { get; set; }
    public bool Fetching { get; set; } = true;
    public ResourceKind? SelectedResource { get; private set; }
    public double TargetFraction { get; set; } = 1.0;
    public double TransportPreparation { get; private set; }
    public bool TransportDelivering { get; private set; }
    public int Total => _amounts.Values.Sum();
    public int ReservedSpaceTotal => _reservedSpace.Values.Sum();
    public int Free => Math.Max(0, Capacity - Total - _reservedSpace.Values.Sum());
    public IReadOnlyDictionary<ResourceKind, int> Amounts => _amounts;
    public IReadOnlyDictionary<ResourceKind, int> CrateAllocations => _crateAllocations;
    public IReadOnlyDictionary<ResourceKind, int> CrateLimits => _crateLimits;

    public LogisticsRoomInstanceRuntime(
        int roomId, LogisticsKind kind, GridCoord anchor, int capacity, int radius,
        int totalCrates = 0, int crateCapacity = 0)
    {
        RoomId = roomId;
        Kind = kind;
        Update(anchor, capacity, radius, totalCrates, crateCapacity);
    }

    public void Update(GridCoord anchor, int capacity, int radius,
        int totalCrates = 0, int crateCapacity = 0)
    {
        Anchor = anchor;
        Capacity = Math.Max(0, capacity);
        TotalCrates = Math.Max(0, totalCrates);
        CrateCapacity = Math.Max(0, crateCapacity);
        Radius = Math.Max(0, radius);
        TrimToCapacity();
    }

    public bool Select(ResourceKind? resource)
    {
        if (Kind is LogisticsKind.Stockpile or LogisticsKind.Market or LogisticsKind.MilitarySupply)
            return resource is null;
        if (Total > 0 && SelectedResource != resource) return false;
        SelectedResource = resource;
        return true;
    }

    public int Amount(ResourceKind resource) => _amounts.GetValueOrDefault(resource);
    public int Reservable(ResourceKind resource) =>
        Math.Max(0, Amount(resource) - _reservedPickup.GetValueOrDefault(resource));

    public int AllocatedCratesFor(ResourceKind resource) => _crateAllocations.GetValueOrDefault(resource);
    public int CrateLimit(ResourceKind resource) => _crateLimits.GetValueOrDefault(resource);
    public int ResourceCapacity(ResourceKind resource)
    {
        if (Kind != LogisticsKind.Stockpile) return Accepts(resource) ? Capacity : 0;
        var crates = AllocatedCratesFor(resource);
        var perCrate = CrateLimit(resource) > 0 ? CrateLimit(resource) : CrateCapacity;
        return crates * perCrate;
    }

    public int FreeFor(ResourceKind resource) => Math.Max(0, ResourceCapacity(resource) -
        Amount(resource) - _reservedSpace.GetValueOrDefault(resource));

    public bool Accepts(ResourceKind resource) => Kind switch
    {
        LogisticsKind.Stockpile => AllocatedCratesFor(resource) > 0,
        LogisticsKind.Market => SettlementLogisticsRuntime.MarketResources.Contains(resource),
        LogisticsKind.MilitarySupply => true,
        LogisticsKind.Station => SelectedResource == resource,
        _ => SelectedResource == resource
    };

    public bool ReservePickup(ResourceKind resource, int amount)
    {
        if (amount <= 0 || Reservable(resource) < amount) return false;
        _reservedPickup[resource] = _reservedPickup.GetValueOrDefault(resource) + amount;
        return true;
    }

    public bool ReserveSpace(ResourceKind resource, int amount)
    {
        if (amount <= 0 || !Accepts(resource) || FreeFor(resource) < amount) return false;
        _reservedSpace[resource] = _reservedSpace.GetValueOrDefault(resource) + amount;
        return true;
    }

    public bool Pickup(ResourceKind resource, int amount)
    {
        if (_reservedPickup.GetValueOrDefault(resource) < amount || Amount(resource) < amount)
            return false;
        SetAmount(resource, Amount(resource) - amount);
        SetReservation(_reservedPickup, resource,
            _reservedPickup.GetValueOrDefault(resource) - amount);
        return true;
    }

    public bool Deposit(ResourceKind resource, int amount)
    {
        if (_reservedSpace.GetValueOrDefault(resource) < amount || Total + amount > Capacity)
            return false;
        SetReservation(_reservedSpace, resource,
            _reservedSpace.GetValueOrDefault(resource) - amount);
        SetAmount(resource, Amount(resource) + amount);
        return true;
    }

    public int DepositUnreserved(ResourceKind resource, int amount)
    {
        if (!Accepts(resource)) return 0;
        var accepted = Math.Min(Math.Max(0, amount), FreeFor(resource));
        SetAmount(resource, Amount(resource) + accepted);
        return accepted;
    }

    public int RemoveUnreserved(ResourceKind resource, int amount)
    {
        var removed = Math.Min(Math.Max(0, amount), Reservable(resource));
        SetAmount(resource, Amount(resource) - removed);
        return removed;
    }

    public void CancelPickup(ResourceKind resource, int amount) =>
        SetReservation(_reservedPickup, resource,
            _reservedPickup.GetValueOrDefault(resource) - Math.Max(0, amount));

    public void CancelSpace(ResourceKind resource, int amount) =>
        SetReservation(_reservedSpace, resource,
            _reservedSpace.GetValueOrDefault(resource) - Math.Max(0, amount));

    public bool AllocateCrates(ResourceKind resource, int amount)
    {
        if (Kind != LogisticsKind.Stockpile) return false;
        var target = Math.Clamp(amount, 0,
            TotalCrates - AllocatedCrates + AllocatedCratesFor(resource));
        var minimum = CrateCapacity <= 0 ? 0 :
            (int)Math.Ceiling((Amount(resource) + _reservedSpace.GetValueOrDefault(resource)) /
                              (double)Math.Max(1, CrateLimit(resource) > 0
                                  ? CrateLimit(resource) : CrateCapacity));
        if (target < minimum) return false;
        if (target == 0) _crateAllocations.Remove(resource);
        else _crateAllocations[resource] = target;
        return true;
    }

    public bool SetCrateLimit(ResourceKind resource, int amount)
    {
        if (Kind != LogisticsKind.Stockpile || AllocatedCratesFor(resource) == 0) return false;
        var limit = Math.Clamp(amount, 0, CrateCapacity);
        var effective = limit == 0 ? CrateCapacity : limit;
        if (Amount(resource) + _reservedSpace.GetValueOrDefault(resource) >
            AllocatedCratesFor(resource) * effective) return false;
        if (limit == 0) _crateLimits.Remove(resource);
        else _crateLimits[resource] = limit;
        return true;
    }

    public void CompleteTransportPreparation(double seconds)
    {
        if (Kind != LogisticsKind.Transport || Total <= 0 || TransportDelivering) return;
        TransportPreparation = Math.Min(576.0, TransportPreparation + Math.Max(0, seconds));
    }

    public bool BeginTransportDelivery()
    {
        if (Kind != LogisticsKind.Transport || Total < 400 || TransportPreparation < 576) return false;
        TransportDelivering = true;
        return true;
    }

    public void EndTransportDelivery()
    {
        TransportDelivering = false;
        TransportPreparation = 0;
    }

    private void TrimToCapacity()
    {
        var excess = Total - Capacity;
        foreach (var resource in _amounts.Keys.ToArray())
        {
            if (excess <= 0) break;
            var remove = Math.Min(excess, Reservable(resource));
            SetAmount(resource, Amount(resource) - remove);
            excess -= remove;
        }
    }

    private void SetAmount(ResourceKind resource, int amount)
    {
        if (amount <= 0) _amounts.Remove(resource);
        else _amounts[resource] = amount;
    }

    private static void SetReservation(
        IDictionary<ResourceKind, int> reservations, ResourceKind resource, int amount)
    {
        if (amount <= 0) reservations.Remove(resource);
        else reservations[resource] = amount;
    }
}

public sealed class SettlementLogisticsRuntime
{
    private readonly GridWorld _world;
    private readonly Dictionary<int, LogisticsRoomInstanceRuntime> _instances = new();
    public IReadOnlyCollection<LogisticsRoomInstanceRuntime> Instances => _instances.Values;
    public int StockpileCapacity => _instances.Values
        .Where(room => room.Kind == LogisticsKind.Stockpile).Sum(room => room.Capacity);
    public int StockpileStored => _instances.Values
        .Where(room => room.Kind == LogisticsKind.Stockpile).Sum(room => room.Total);
    public int StockpileReservedSpace => _instances.Values
        .Where(room => room.Kind == LogisticsKind.Stockpile).Sum(room => room.ReservedSpaceTotal);
    public Func<ResourceKind, int>? ExcludedFromStockpile { get; set; }

    public readonly record struct StockpileReservation(int RoomId, GridCoord Cell, int Amount);

    public static readonly HashSet<ResourceKind> MarketResources = BuildMarketResources();
    public static readonly HashSet<ResourceKind> MilitaryResources = new()
    {
        ResourceKind.Ration, ResourceKind.Clothes, ResourceKind.ArmourLeather,
        ResourceKind.ArmourPlate, ResourceKind.Bow, ResourceKind.WeaponHammer,
        ResourceKind.WeaponMount, ResourceKind.WeaponShield, ResourceKind.WeaponShort,
        ResourceKind.WeaponSlash, ResourceKind.WeaponSpear, ResourceKind.Stone
    };

    public SettlementLogisticsRuntime(GridWorld world) => _world = world;

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            var rule = OriginalGameData.Current.Room(room.DefinitionKey);
            var station = room.DefinitionKey.Equals("_STATION", StringComparison.OrdinalIgnoreCase);
            var parsed = default(LogisticsKind);
            if (!station && (rule?.Logistics is null || !Enum.TryParse<LogisticsKind>(
                    rule.Logistics.Kind, true, out parsed))) continue;
            var kind = station ? LogisticsKind.Station : parsed;
            active.Add(room.Id);
            var anchor = room.Cells.Count == 0 ? default : _world.FromIndex(room.Cells.First());
            var crates = Math.Max(1, (int)Math.Ceiling(room.ItemGroupAmounts.Values.Sum()));
            var capacity = Capacity(room, rule, kind, crates);
            var crateCapacity = kind == LogisticsKind.Stockpile ? Math.Max(1, capacity / crates) : 0;
            if (!_instances.TryGetValue(room.Id, out var instance))
            {
                instance = new LogisticsRoomInstanceRuntime(
                    room.Id, kind, anchor, capacity, station ? 0 : rule!.Logistics!.DefaultRadius,
                    kind == LogisticsKind.Stockpile ? crates : 0, crateCapacity);
                if (kind == LogisticsKind.Export) instance.TargetFraction = 0.75;
                _instances.Add(room.Id, instance);
            }
            else instance.Update(anchor, capacity, station ? 0 : rule!.Logistics!.DefaultRadius,
                kind == LogisticsKind.Stockpile ? crates : 0, crateCapacity);
        }
        foreach (var roomId in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
            _instances.Remove(roomId);
    }

    public bool SetResource(int roomId, ResourceKind? resource) =>
        _instances.TryGetValue(roomId, out var room) && room.Select(resource);

    public IReadOnlyList<(ResourceKind Resource, int Amount, GridCoord Cell)> ClearRoom(int roomId)
    {
        var result = new List<(ResourceKind Resource, int Amount, GridCoord Cell)>();
        if (!_instances.Remove(roomId, out var room)) return result;
        foreach (var pair in room.Amounts)
            if (pair.Value > 0) result.Add((pair.Key, pair.Value, room.Anchor));
        return result;
    }

    public int IssueMilitarySupply(ResourceKind resource, int amount)
    {
        var remaining = Math.Max(0, amount);
        var issued = 0;
        foreach (var depot in Ordered(LogisticsKind.MilitarySupply))
        {
            var take = depot.RemoveUnreserved(resource, remaining);
            issued += take;
            remaining -= take;
            if (remaining <= 0) break;
        }
        return issued;
    }

    public bool SetPolicy(int roomId, bool fetching, int priority, double targetFraction)
    {
        if (!_instances.TryGetValue(roomId, out var room)) return false;
        room.Fetching = fetching;
        room.Priority = Math.Clamp(priority, 0, 10);
        room.TargetFraction = Math.Clamp(targetFraction, 0, 1);
        return true;
    }

    public bool AdjustStockpileCrates(int roomId, ResourceKind resource, int delta) =>
        _instances.TryGetValue(roomId, out var room) && room.AllocateCrates(
            resource, room.AllocatedCratesFor(resource) + delta);

    public bool SetStockpileCrateLimit(int roomId, ResourceKind resource, int amount) =>
        _instances.TryGetValue(roomId, out var room) && room.SetCrateLimit(resource, amount);

    public LogisticsRoomInstanceRuntime? Instance(int roomId) => _instances.GetValueOrDefault(roomId);

    public LogisticsPolicySnapshot[] CapturePolicies() => _instances.Values.Select(room =>
    {
        var resources = room.CrateAllocations.Keys.ToArray();
        return new LogisticsPolicySnapshot(
            room.RoomId, room.Fetching, room.Priority, room.TargetFraction,
            resources.Select(resource => (byte)resource).ToArray(),
            resources.Select(room.AllocatedCratesFor).ToArray(),
            resources.Select(room.CrateLimit).ToArray());
    }).ToArray();

    public void RestorePolicies(IEnumerable<LogisticsPolicySnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            if (!_instances.TryGetValue(snapshot.RoomId, out var room)) continue;
            room.Fetching = snapshot.Fetching;
            room.Priority = Math.Clamp(snapshot.Priority, 0, 10);
            room.TargetFraction = Math.Clamp(snapshot.TargetFraction, 0, 1);
            var count = Math.Min(snapshot.Resources.Length,
                Math.Min(snapshot.Crates.Length, snapshot.Limits.Length));
            for (var index = 0; index < count; index++)
            {
                if (snapshot.Resources[index] >= ResourceLedger.KindCount) continue;
                var resource = (ResourceKind)snapshot.Resources[index];
                room.AllocateCrates(resource, snapshot.Crates[index]);
                room.SetCrateLimit(resource, snapshot.Limits[index]);
            }
        }
    }

    public void Reconcile(ResourceLedger resources)
    {
        foreach (var resource in Enum.GetValues<ResourceKind>())
        {
            var allocated = _instances.Values.Sum(room => room.Amount(resource));
            var actual = Math.Max(0, resources.Get(resource) -
                                     (ExcludedFromStockpile?.Invoke(resource) ?? 0));
            if (allocated < actual)
            {
                var remaining = actual - allocated;
                foreach (var stockpile in Ordered(LogisticsKind.Stockpile))
                {
                    remaining -= stockpile.DepositUnreserved(resource, remaining);
                    if (remaining <= 0) break;
                }
            }
            else if (allocated > actual)
            {
                var excess = allocated - actual;
                foreach (var room in _instances.Values.OrderBy(room => room.Kind == LogisticsKind.Stockpile ? 0 : 1))
                {
                    excess -= room.RemoveUnreserved(resource, excess);
                    if (excess <= 0) break;
                }
            }
        }
    }

    public StockpileReservation? ReserveProductionSupply(
        ResourceKind resource, GridCoord destination, int maximum)
    {
        var source = Ordered(LogisticsKind.Stockpile)
            .Where(room => room.Reservable(resource) > 0)
            .OrderBy(room => Math.Abs(room.Anchor.X - destination.X) +
                             Math.Abs(room.Anchor.Z - destination.Z))
            .FirstOrDefault();
        if (source is null) return null;
        var amount = Math.Min(Math.Max(0, maximum), source.Reservable(resource));
        return amount > 0 && source.ReservePickup(resource, amount)
            ? new StockpileReservation(source.RoomId, source.Anchor, amount)
            : null;
    }

    public StockpileReservation? ReserveStockpileSpace(
        ResourceKind resource, GridCoord source, int maximum, int excludedRoomId,
        bool requireFetching = false)
    {
        var destination = Ordered(LogisticsKind.Stockpile)
            .Where(room => room.RoomId != excludedRoomId && room.Accepts(resource) &&
                           room.FreeFor(resource) > 0 && (!requireFetching || room.Fetching))
            .OrderByDescending(room => room.Priority)
            .ThenBy(room => Math.Abs(room.Anchor.X - source.X) + Math.Abs(room.Anchor.Z - source.Z))
            .FirstOrDefault();
        if (destination is null) return null;
        var amount = Math.Min(Math.Max(0, maximum), destination.FreeFor(resource));
        return amount > 0 && destination.ReserveSpace(resource, amount)
            ? new StockpileReservation(destination.RoomId, destination.Anchor, amount)
            : null;
    }

    public bool PickupProductionSupply(BuildJob job) =>
        job.Kind == BuildKind.ProductionSupply &&
        _instances.TryGetValue(job.DestinationRoomId, out var source) &&
        source.Kind == LogisticsKind.Stockpile &&
        source.Pickup(job.Resource, job.ResourceAmount);

    public void ReturnProductionSupply(BuildJob job)
    {
        if (job.Kind != BuildKind.ProductionSupply ||
            !_instances.TryGetValue(job.DestinationRoomId, out var source)) return;
        source.DepositUnreserved(job.Resource, job.ResourceAmount);
    }

    public bool DepositStockpileHaul(BuildJob job) =>
        job.Kind is BuildKind.Haul or BuildKind.RoomOutputHaul &&
        _instances.TryGetValue(job.DestinationRoomId, out var destination) &&
        destination.Kind == LogisticsKind.Stockpile &&
        destination.Deposit(job.OutputResource, job.OutputAmount);

    public void CancelProductionSupply(BuildJob job)
    {
        if (job.Kind != BuildKind.ProductionSupply || job.PickedUp ||
            !_instances.TryGetValue(job.DestinationRoomId, out var source)) return;
        source.CancelPickup(job.Resource, job.ResourceAmount);
    }

    public void CancelStockpileSpace(BuildJob job)
    {
        if (job.Kind is not (BuildKind.Haul or BuildKind.RoomOutputHaul) ||
            !_instances.TryGetValue(job.DestinationRoomId, out var destination)) return;
        destination.CancelSpace(job.OutputResource, job.OutputAmount);
    }

    public bool RestoreProductionSupplyReservation(BuildJob job) =>
        job.Kind == BuildKind.ProductionSupply && (job.PickedUp ||
        _instances.TryGetValue(job.DestinationRoomId, out var source) &&
        source.ReservePickup(job.Resource, job.ResourceAmount));

    public bool RestoreStockpileReservation(BuildJob job) =>
        job.Kind is BuildKind.Haul or BuildKind.RoomOutputHaul &&
        _instances.TryGetValue(job.DestinationRoomId, out var destination) &&
        destination.ReserveSpace(job.OutputResource, job.OutputAmount);

    public void Schedule(JobBoard jobs, ResourceLedger resources)
    {
        Reconcile(resources);
        foreach (var destination in _instances.Values
                     .Where(room => room.Fetching && room.Free > 0 &&
                                    room.Kind is LogisticsKind.Hauler or LogisticsKind.Transport or
                                        LogisticsKind.Export or LogisticsKind.Market or
                                        LogisticsKind.MilitarySupply)
                     .OrderByDescending(room => room.Priority))
        {
            var resourcesToPull = destination.Kind == LogisticsKind.Market
                ? MarketResources
                : destination.Kind == LogisticsKind.MilitarySupply
                    ? MilitaryResources
                : destination.SelectedResource is { } selected
                    ? new HashSet<ResourceKind> { selected }
                    : new HashSet<ResourceKind>();
            foreach (var resource in resourcesToPull)
            {
                if (HasPendingTransfer(jobs, destination.RoomId, resource)) continue;
                var target = Math.Max(1, (int)Math.Ceiling(destination.Capacity * destination.TargetFraction));
                var wanted = Math.Min(BuildJob.MaximumFetchAmount,
                    Math.Min(destination.Free, target - destination.Total));
                if (wanted <= 0) continue;
                var source = Ordered(LogisticsKind.Stockpile)
                    .FirstOrDefault(room => room.RoomId != destination.RoomId &&
                                            InRange(room, destination) && room.Reservable(resource) > 0);
                if (source is null) continue;
                var amount = Math.Min(wanted, source.Reservable(resource));
                if (!source.ReservePickup(resource, amount) ||
                    !destination.ReserveSpace(resource, amount)) continue;
                var job = BuildJob.LogisticsTransfer(
                    source.Anchor, destination.Anchor, source.RoomId, destination.RoomId, resource, amount);
                if (!jobs.Add(job))
                {
                    source.CancelPickup(resource, amount);
                    destination.CancelSpace(resource, amount);
                }
            }
        }

        foreach (var transport in Ordered(LogisticsKind.Transport)
                     .Where(room => room.Total > 0 && room.TransportPreparation < 576 &&
                                    !jobs.All.Any(job => job.Kind == BuildKind.TransportPreparation &&
                                                        job.RoomId == room.RoomId &&
                                                        job.State is not (JobState.Cancelled or JobState.Completed))))
            jobs.Add(BuildJob.TransportPreparation(transport.Anchor, transport.RoomId));
    }

    public bool Pickup(BuildJob job)
    {
        if (job.Kind != BuildKind.LogisticsTransfer ||
            !_instances.TryGetValue(job.RoomId, out var source)) return false;
        return source.Pickup(job.OutputResource, job.OutputAmount);
    }

    public bool Deliver(BuildJob job)
    {
        if (job.Kind != BuildKind.LogisticsTransfer ||
            !_instances.TryGetValue(job.DestinationRoomId, out var destination)) return false;
        return destination.Deposit(job.OutputResource, job.OutputAmount);
    }

    public void Cancel(BuildJob job)
    {
        if (job.Kind != BuildKind.LogisticsTransfer) return;
        if (_instances.TryGetValue(job.RoomId, out var source) && !job.PickedUp)
            source.CancelPickup(job.OutputResource, job.OutputAmount);
        if (_instances.TryGetValue(job.DestinationRoomId, out var destination))
            destination.CancelSpace(job.OutputResource, job.OutputAmount);
    }

    public void ReturnToSource(BuildJob job)
    {
        if (job.Kind != BuildKind.LogisticsTransfer || !job.PickedUp) return;
        if (_instances.TryGetValue(job.RoomId, out var source))
            source.DepositUnreserved(job.OutputResource, job.OutputAmount);
        job.PickedUp = false;
    }

    public void CompleteTransportPreparation(BuildJob job) =>
        _instances.GetValueOrDefault(job.RoomId)?.CompleteTransportPreparation(16);

    public int AvailableForExport(ResourceKind resource) => Ordered(LogisticsKind.Export)
        .Where(room => room.SelectedResource == resource).Sum(room => room.Reservable(resource));

    public int ExtractExport(ResourceKind resource, int amount)
    {
        var remaining = Math.Max(0, amount);
        foreach (var depot in Ordered(LogisticsKind.Export).Where(room => room.SelectedResource == resource))
            remaining -= depot.RemoveUnreserved(resource, remaining);
        return amount - remaining;
    }

    public int ImportCapacity(ResourceKind resource) => Ordered(LogisticsKind.Import)
        .Where(room => room.SelectedResource == resource).Sum(room => room.Free);

    public int ImportStored(ResourceKind resource) => Ordered(LogisticsKind.Import)
        .Where(room => room.SelectedResource == resource).Sum(room => room.Amount(resource));

    public int ImportTotalCapacity(ResourceKind resource) => Ordered(LogisticsKind.Import)
        .Where(room => room.SelectedResource == resource).Sum(room => room.Capacity);

    public int DeliverImport(ResourceKind resource, int amount)
    {
        var remaining = Math.Max(0, amount);
        foreach (var depot in Ordered(LogisticsKind.Import).Where(room => room.SelectedResource == resource))
            remaining -= depot.DepositUnreserved(resource, remaining);
        return amount - remaining;
    }

    public bool ConsumeMarket(ResourceLedger resources)
    {
        var market = Ordered(LogisticsKind.Market)
            .FirstOrDefault(room => room.Amounts.Any(pair => pair.Value > 0));
        if (market is null) return false;
        var resource = market.Amounts.First(pair => pair.Value > 0).Key;
        if (!resources.TryTake(resource, 1)) return false;
        return market.RemoveUnreserved(resource, 1) == 1;
    }

    public double MarketQuality(int roomId)
    {
        if (!_instances.TryGetValue(roomId, out var room) || room.Kind != LogisticsKind.Market)
            return 0;
        return room.Capacity <= 0 ? 0 : Math.Clamp(room.Total / (double)room.Capacity, 0, 1);
    }

    private IEnumerable<LogisticsRoomInstanceRuntime> Ordered(LogisticsKind kind) =>
        _instances.Values.Where(room => room.Kind == kind).OrderByDescending(room => room.Priority);

    private static bool HasPendingTransfer(JobBoard jobs, int destinationRoomId, ResourceKind resource) =>
        jobs.All.Any(job => job.Kind == BuildKind.LogisticsTransfer &&
                            job.DestinationRoomId == destinationRoomId &&
                            job.OutputResource == resource &&
                            job.State is not (JobState.Cancelled or JobState.Completed));

    private static bool InRange(
        LogisticsRoomInstanceRuntime source, LogisticsRoomInstanceRuntime destination) =>
        destination.Radius <= 0 || Math.Abs(source.Anchor.X - destination.Anchor.X) +
        Math.Abs(source.Anchor.Z - destination.Anchor.Z) <= destination.Radius;

    private static int Capacity(RoomRecord room, RoomRule? rule, LogisticsKind kind, int crates)
    {
        if (kind == LogisticsKind.Station) return crates * 400;
        if (rule is null) return 0;
        if (kind == LogisticsKind.Stockpile)
        {
            var boost = rule.Upgrades.Count == 0 ? 81 :
                rule.Upgrades[Math.Clamp(room.UpgradeLevel, 0, rule.Upgrades.Count - 1)].Boost;
            return crates * Math.Max(1, (int)Math.Round(boost) - 1);
        }
        if (kind == LogisticsKind.Transport) return Math.Max(1, rule.Logistics!.MaximumLoad);
        if (kind == LogisticsKind.Market) return crates * 2;
        return crates * Math.Max(1, rule.Logistics!.CrateCapacity);
    }

    private static HashSet<ResourceKind> BuildMarketResources()
    {
        var result = new HashSet<ResourceKind>();
        foreach (var race in OriginalGameData.Current.Races.Values)
            foreach (var classRules in race.HomeFurniture.Values)
                foreach (var resourceKey in classRules.Keys)
                    if (OriginalGameData.TryMapResource(resourceKey, out var resource)) result.Add(resource);
        if (result.Count == 0) result.Add(ResourceKind.Furniture);
        return result;
    }
}
