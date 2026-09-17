using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;

namespace GodotSyxPort.World;

public enum ShipmentDirection : byte
{
    Import,
    Export
}

public enum ShipmentState : byte
{
    Reserved,
    Loading,
    Travelling,
    Arrived,
    Unloading,
    Returning,
    Completed,
    Cancelled
}

public sealed class WorldTradeShipment
{
    public long Id { get; init; }
    public long EntityId { get; internal set; }
    public ShipmentDirection Direction { get; init; }
    public ShipmentState State { get; internal set; }
    public int PartnerFactionId { get; init; }
    public int OriginRegionId { get; init; }
    public int DestinationRegionId { get; internal set; }
    public int CurrentRegionId { get; internal set; }
    public ResourceKind Resource { get; init; }
    public int Amount { get; init; }
    public int UnitPrice { get; init; }
    public int Toll { get; init; }
    public long PartnerReservationId { get; init; }
    public double LoadingProgress { get; internal set; }
    public double UnloadingProgress { get; internal set; }
    public int CreatedDay { get; init; }
    public int ArrivedDay { get; internal set; } = -1;
    public List<int> Route { get; } = new();

    public int GrossValue => checked(Amount * UnitPrice);
    public int TotalValue => checked(GrossValue + (Direction == ShipmentDirection.Import ? Toll : -Toll));
    public bool IsTerminal => State is ShipmentState.Completed or ShipmentState.Cancelled;
}

public sealed record ShipmentArrival(
    long ShipmentId,
    ShipmentDirection Direction,
    int PartnerFactionId,
    int RegionId,
    ResourceKind Resource,
    int Amount,
    int Value,
    int Day);

public sealed record ShipmentSnapshot(
    long Id,
    long EntityId,
    ShipmentDirection Direction,
    ShipmentState State,
    int PartnerFactionId,
    int OriginRegionId,
    int DestinationRegionId,
    int CurrentRegionId,
    ResourceKind Resource,
    int Amount,
    int UnitPrice,
    int Toll,
    long PartnerReservationId,
    double LoadingProgress,
    double UnloadingProgress,
    int CreatedDay,
    int ArrivedDay,
    IReadOnlyList<int> Route);

public sealed record ShipmentRouteSegment(
    int Index,
    int FromRegionId,
    int ToRegionId,
    double RoadCost,
    bool BridgeOrPort,
    bool Traversed,
    double Progress);

public sealed record ShipmentManifest(
    long ShipmentId,
    ShipmentDirection Direction,
    ShipmentState State,
    int PartnerFactionId,
    string Partner,
    ResourceKind Resource,
    int Amount,
    int UnitPrice,
    int Toll,
    int GrossValue,
    int NetValue,
    int OriginRegionId,
    int DestinationRegionId,
    int CurrentRegionId,
    int RouteLength,
    double RouteProgress,
    IReadOnlyList<ShipmentRouteSegment> Segments);

public sealed record ShipmentDiagnostics(
    int Active,
    int Imports,
    int Exports,
    int Reserved,
    int Loading,
    int Travelling,
    int Unloading,
    int Returning,
    int ImportedUnits,
    int ExportedUnits,
    IReadOnlyDictionary<ResourceKind, int> CargoInTransit,
    IReadOnlyList<string> Problems);

/// <summary>
/// Physical peaceful import/export layer. Shipments reserve NPC stock before they
/// appear, move through actual world routes and expose their current region for F3.
/// </summary>
public sealed class WorldTradeShipmentRuntime
{
    public const int MaximumRouteDistance = 550;
    public const int MaximumActiveShipments = 50;
    public const double LoadingDays = 0.5;
    public const double UnloadingDays = 0.25;

    private readonly StrategicWorldRuntime _world;
    private readonly WorldEntityRuntime _entities;
    private readonly WorldRegionRuntime _regions;
    private readonly WorldFactionRuntime _factions;
    private readonly Dictionary<long, WorldTradeShipment> _shipments = new();
    private readonly Dictionary<long, long> _shipmentByEntity = new();
    private readonly List<ShipmentArrival> _arrivals = new();
    private long _nextId = 1;

    public WorldTradeShipmentRuntime(
        StrategicWorldRuntime world,
        WorldEntityRuntime entities,
        WorldRegionRuntime regions,
        WorldFactionRuntime factions)
    {
        _world = world;
        _entities = entities;
        _regions = regions;
        _factions = factions;
    }

    public IReadOnlyCollection<WorldTradeShipment> Shipments => _shipments.Values;
    public IReadOnlyList<ShipmentArrival> Arrivals => _arrivals;
    public int ActiveCount => _shipments.Values.Count(value => !value.IsTerminal);

    public WorldTradeShipment? Get(long shipmentId) => _shipments.GetValueOrDefault(shipmentId);

    public WorldTradeShipment? CreateImport(
        int playerCapitalRegionId,
        int partnerFactionId,
        ResourceKind resource,
        int amount,
        int unitPrice,
        int toll,
        int day)
    {
        var partner = _factions.State(partnerFactionId);
        if (partner is null || amount <= 0 || unitPrice < 0 || ActiveCount >= MaximumActiveShipments)
            return null;
        var route = ValidRoute(partner.CapitalRegionId, playerCapitalRegionId);
        if (route is null) return null;
        var reservation = _factions.ReserveExport(
            partnerFactionId,
            resource,
            amount,
            day,
            "SETTLEMENT_IMPORT");
        if (reservation == 0) return null;
        var reserved = _factions.Reservation(reservation);
        if (reserved is null || reserved.Amount <= 0)
        {
            _factions.CancelReservation(reservation);
            return null;
        }
        return Create(
            ShipmentDirection.Import,
            partnerFactionId,
            partner.CapitalRegionId,
            playerCapitalRegionId,
            resource,
            reserved.Amount,
            unitPrice,
            toll,
            reservation,
            day,
            route);
    }

    public WorldTradeShipment? CreateExport(
        int playerCapitalRegionId,
        int partnerFactionId,
        ResourceKind resource,
        int amount,
        int unitPrice,
        int toll,
        int day)
    {
        var partner = _factions.State(partnerFactionId);
        if (partner is null || amount <= 0 || unitPrice < 0 || ActiveCount >= MaximumActiveShipments)
            return null;
        var route = ValidRoute(playerCapitalRegionId, partner.CapitalRegionId);
        if (route is null || !_regions.ReserveStock(playerCapitalRegionId, resource, amount)) return null;
        var reservation = _factions.ReserveImport(
            partnerFactionId,
            resource,
            amount,
            day,
            "SETTLEMENT_EXPORT");
        if (reservation == 0) return null;
        return Create(
            ShipmentDirection.Export,
            partnerFactionId,
            playerCapitalRegionId,
            partner.CapitalRegionId,
            resource,
            amount,
            unitPrice,
            toll,
            reservation,
            day,
            route);
    }

    public void Tick(double days, int day)
    {
        if (days <= 0) return;
        _arrivals.Clear();
        _entities.Tick(days);
        foreach (var shipment in _shipments.Values.Where(value => !value.IsTerminal).ToArray())
        {
            switch (shipment.State)
            {
                case ShipmentState.Reserved:
                case ShipmentState.Loading:
                    TickLoading(shipment, days);
                    break;
                case ShipmentState.Travelling:
                case ShipmentState.Returning:
                    TickTravelling(shipment, day);
                    break;
                case ShipmentState.Arrived:
                case ShipmentState.Unloading:
                    TickUnloading(shipment, days, day);
                    break;
            }
        }
        TrimTerminal(day);
    }

    public bool Cancel(long shipmentId)
    {
        if (!_shipments.TryGetValue(shipmentId, out var shipment) || shipment.IsTerminal) return false;
        if (shipment.State is ShipmentState.Reserved or ShipmentState.Loading)
            _factions.CancelReservation(shipment.PartnerReservationId);
        _entities.Cancel(shipment.EntityId);
        _shipmentByEntity.Remove(shipment.EntityId);
        shipment.State = ShipmentState.Cancelled;
        return true;
    }

    public bool Recall(long shipmentId)
    {
        if (!_shipments.TryGetValue(shipmentId, out var shipment) || shipment.IsTerminal)
            return false;
        if (shipment.State is ShipmentState.Reserved or ShipmentState.Loading)
            return Cancel(shipmentId);
        if (shipment.State is ShipmentState.Arrived or ShipmentState.Unloading)
            return false;
        var entity = _entities.Get(shipment.EntityId);
        if (entity is null || !_entities.BeginReturn(entity.Id)) return false;
        shipment.State = ShipmentState.Returning;
        shipment.CurrentRegionId = entity.CurrentRegionId;
        return true;
    }

    public bool Reroute(long shipmentId, int destinationRegionId)
    {
        if (!_shipments.TryGetValue(shipmentId, out var shipment) ||
            shipment.IsTerminal || shipment.State != ShipmentState.Travelling) return false;
        var entity = _entities.Get(shipment.EntityId);
        if (entity is null) return false;
        var route = ValidRoute(entity.CurrentRegionId, destinationRegionId);
        if (route is null) return false;
        _entities.Cancel(entity.Id);
        var replacement = _entities.Create(
            shipment.Direction == ShipmentDirection.Import
                ? WorldEntityKind.ImportShipment
                : WorldEntityKind.ExportShipment,
            entity.OwnerFactionId,
            entity.CurrentRegionId,
            destinationRegionId,
            entity.Purpose,
            entity.SpeedRegionsPerDay,
            route);
        _shipmentByEntity.Remove(shipment.EntityId);
        shipment.EntityId = replacement.Id;
        shipment.CurrentRegionId = replacement.CurrentRegionId; shipment.DestinationRegionId = destinationRegionId;
        shipment.Route.Clear();
        shipment.Route.AddRange(route);
        _shipmentByEntity[replacement.Id] = shipment.Id;
        return true;
    }

    public ShipmentManifest? Manifest(long shipmentId)
    {
        if (!_shipments.TryGetValue(shipmentId, out var shipment)) return null;
        var faction = _factions.State(shipment.PartnerFactionId);
        var entity = shipment.EntityId == 0 ? null : _entities.Get(shipment.EntityId);
        var segments = RouteSegments(shipment, entity).ToArray();
        var progress = segments.Length == 0
            ? shipment.State is ShipmentState.Arrived or ShipmentState.Unloading or ShipmentState.Completed ? 1 : 0
            : Math.Clamp(segments.Count(segment => segment.Traversed) / (double)segments.Length +
                         (segments.FirstOrDefault(segment => !segment.Traversed)?.Progress ?? 0) / segments.Length, 0, 1);
        return new ShipmentManifest(
            shipment.Id,
            shipment.Direction,
            shipment.State,
            shipment.PartnerFactionId,
            faction?.Key ?? $"FACTION_{shipment.PartnerFactionId}",
            shipment.Resource,
            shipment.Amount,
            shipment.UnitPrice,
            shipment.Toll,
            shipment.GrossValue,
            shipment.TotalValue,
            shipment.OriginRegionId,
            shipment.DestinationRegionId,
            shipment.CurrentRegionId,
            Math.Max(0, shipment.Route.Count - 1),
            progress,
            segments);
    }

    public IEnumerable<ShipmentRouteSegment> RouteSegments(
        WorldTradeShipment shipment,
        WorldEntityRecord? entity = null)
    {
        for (var index = 0; index + 1 < shipment.Route.Count; index++)
        {
            var from = shipment.Route[index];
            var to = shipment.Route[index + 1];
            var road = _world.Roads.FirstOrDefault(candidate =>
                candidate.FirstRegionId == from && candidate.SecondRegionId == to ||
                candidate.FirstRegionId == to && candidate.SecondRegionId == from);
            var routeIndex = entity?.RouteIndex ?? 0;
            var traversed = shipment.State is ShipmentState.Arrived or ShipmentState.Unloading or ShipmentState.Completed ||
                            entity is not null && index < routeIndex;
            var progress = entity is not null && index == routeIndex ? entity.EdgeProgress : traversed ? 1 : 0;
            yield return new ShipmentRouteSegment(
                index,
                from,
                to,
                road?.Cost ?? 1,
                road?.BridgeOrPort ?? false,
                traversed,
                progress);
        }
    }

    public ShipmentDiagnostics Diagnose()
    {
        var problems = new List<string>();
        var cargo = new Dictionary<ResourceKind, int>();
        foreach (var shipment in _shipments.Values.Where(value => !value.IsTerminal))
        {
            cargo[shipment.Resource] = cargo.GetValueOrDefault(shipment.Resource) + shipment.Amount;
            if (shipment.Amount <= 0)
                problems.Add($"Shipment {shipment.Id} has an empty cargo manifest.");
            if (shipment.UnitPrice < 0)
                problems.Add($"Shipment {shipment.Id} has a negative unit price.");
            if (shipment.Route.Count == 0)
                problems.Add($"Shipment {shipment.Id} has no world route.");
            else
            {
                if (shipment.Route[0] != shipment.OriginRegionId)
                    problems.Add($"Shipment {shipment.Id} route does not start at its origin.");
                if (shipment.Route[^1] != shipment.DestinationRegionId)
                    problems.Add($"Shipment {shipment.Id} route does not end at its destination.");
                if (shipment.Route.Count - 1 > MaximumRouteDistance)
                    problems.Add($"Shipment {shipment.Id} exceeds maximum route distance.");
            }
            if (shipment.State is ShipmentState.Travelling or ShipmentState.Returning)
            {
                var entity = _entities.Get(shipment.EntityId);
                if (entity is null) problems.Add($"Shipment {shipment.Id} has no moving world entity.");
                else if (entity.CurrentRegionId != shipment.CurrentRegionId)
                    problems.Add($"Shipment {shipment.Id} disagrees with entity {entity.Id} about current region.");
            }
            if (_world.Region(shipment.OriginRegionId) is null)
                problems.Add($"Shipment {shipment.Id} has unknown origin region.");
            if (_world.Region(shipment.DestinationRegionId) is null)
                problems.Add($"Shipment {shipment.Id} has unknown destination region.");
        }
        var active = _shipments.Values.Where(value => !value.IsTerminal).ToArray();
        return new ShipmentDiagnostics(
            active.Length,
            active.Count(value => value.Direction == ShipmentDirection.Import),
            active.Count(value => value.Direction == ShipmentDirection.Export),
            active.Count(value => value.State == ShipmentState.Reserved),
            active.Count(value => value.State == ShipmentState.Loading),
            active.Count(value => value.State == ShipmentState.Travelling),
            active.Count(value => value.State is ShipmentState.Arrived or ShipmentState.Unloading),
            active.Count(value => value.State == ShipmentState.Returning),
            active.Where(value => value.Direction == ShipmentDirection.Import).Sum(value => value.Amount),
            active.Where(value => value.Direction == ShipmentDirection.Export).Sum(value => value.Amount),
            cargo,
            problems);
    }

    public IReadOnlyList<string> DescribeShipments()
    {
        var result = new List<string>();
        foreach (var shipment in _shipments.Values.Where(value => !value.IsTerminal).OrderBy(value => value.Id))
        {
            var manifest = Manifest(shipment.Id);
            if (manifest is null) continue;
            result.Add(
                $"#{manifest.ShipmentId} {manifest.Direction} {manifest.Amount} {manifest.Resource} " +
                $"from {manifest.OriginRegionId} to {manifest.DestinationRegionId}, " +
                $"at {manifest.CurrentRegionId}, {manifest.RouteProgress:P0}, {manifest.State}");
        }
        var diagnostics = Diagnose();
        result.AddRange(diagnostics.Problems.Select(problem => "Problem: " + problem));
        return result;
    }

    public IReadOnlyDictionary<int, int> ShipmentsByRegion()
    {
        return _shipments.Values.Where(value => !value.IsTerminal)
            .GroupBy(value => value.CurrentRegionId)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    public IReadOnlyDictionary<int, int> ShipmentsByPartner()
    {
        return _shipments.Values.Where(value => !value.IsTerminal)
            .GroupBy(value => value.PartnerFactionId)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    public int ReservedPartnerUnits(int factionId, ResourceKind resource)
    {
        return _shipments.Values.Where(value =>
                !value.IsTerminal &&
                value.PartnerFactionId == factionId &&
                value.Resource == resource)
            .Sum(value => value.Amount);
    }

    public IEnumerable<WorldTradeShipment> VisibleShipments(Func<int, bool> regionVisible)
    {
        return _shipments.Values.Where(shipment =>
            !shipment.IsTerminal && regionVisible(shipment.CurrentRegionId));
    }

    public IReadOnlyList<ShipmentSnapshot> Capture() => _shipments.Values
        .Where(value => !value.IsTerminal)
        .Select(value => new ShipmentSnapshot(
            value.Id,
            value.EntityId,
            value.Direction,
            value.State,
            value.PartnerFactionId,
            value.OriginRegionId,
            value.DestinationRegionId,
            value.CurrentRegionId,
            value.Resource,
            value.Amount,
            value.UnitPrice,
            value.Toll,
            value.PartnerReservationId,
            value.LoadingProgress,
            value.UnloadingProgress,
            value.CreatedDay,
            value.ArrivedDay,
            value.Route.ToArray()))
        .ToArray();

    public void Restore(IEnumerable<ShipmentSnapshot> snapshots)
    {
        _shipments.Clear();
        _shipmentByEntity.Clear();
        _nextId = 1;
        foreach (var snapshot in snapshots)
        {
            var shipment = new WorldTradeShipment
            {
                Id = snapshot.Id,
                EntityId = snapshot.EntityId,
                Direction = snapshot.Direction,
                State = snapshot.State,
                PartnerFactionId = snapshot.PartnerFactionId,
                OriginRegionId = snapshot.OriginRegionId,
                DestinationRegionId = snapshot.DestinationRegionId,
                CurrentRegionId = snapshot.CurrentRegionId,
                Resource = snapshot.Resource,
                Amount = snapshot.Amount,
                UnitPrice = snapshot.UnitPrice,
                Toll = snapshot.Toll,
                PartnerReservationId = snapshot.PartnerReservationId,
                LoadingProgress = snapshot.LoadingProgress,
                UnloadingProgress = snapshot.UnloadingProgress,
                CreatedDay = snapshot.CreatedDay,
                ArrivedDay = snapshot.ArrivedDay
            };
            shipment.Route.AddRange(snapshot.Route);
            _shipments.Add(shipment.Id, shipment);
            _shipmentByEntity[shipment.EntityId] = shipment.Id;
            _nextId = Math.Max(_nextId, shipment.Id + 1);
        }
    }

    private WorldTradeShipment Create(
        ShipmentDirection direction,
        int partnerFactionId,
        int originRegionId,
        int destinationRegionId,
        ResourceKind resource,
        int amount,
        int unitPrice,
        int toll,
        long reservation,
        int day,
        IReadOnlyList<int> route)
    {
        var shipment = new WorldTradeShipment
        {
            Id = _nextId++,
            Direction = direction,
            State = ShipmentState.Reserved,
            PartnerFactionId = partnerFactionId,
            OriginRegionId = originRegionId,
            DestinationRegionId = destinationRegionId,
            CurrentRegionId = originRegionId,
            Resource = resource,
            Amount = amount,
            UnitPrice = unitPrice,
            Toll = Math.Max(0, toll),
            PartnerReservationId = reservation,
            CreatedDay = day
        };
        shipment.Route.AddRange(route);
        _shipments.Add(shipment.Id, shipment);
        return shipment;
    }

    private void TickLoading(WorldTradeShipment shipment, double days)
    {
        shipment.State = ShipmentState.Loading;
        shipment.LoadingProgress += days / LoadingDays;
        if (shipment.LoadingProgress < 1) return;
        if (shipment.Direction == ShipmentDirection.Export)
        {
            var withdrawn = _regions.WithdrawStock(shipment.OriginRegionId, shipment.Resource, shipment.Amount);
            if (withdrawn < shipment.Amount)
            {
                if (withdrawn > 0) _regions.DepositStock(shipment.OriginRegionId, shipment.Resource, withdrawn);
                Cancel(shipment.Id);
                return;
            }
        }
        else if (!_factions.CommitReservation(shipment.PartnerReservationId, shipment.GrossValue))
        {
            Cancel(shipment.Id);
            return;
        }
        var entity = _entities.Create(
            shipment.Direction == ShipmentDirection.Import
                ? WorldEntityKind.ImportShipment
                : WorldEntityKind.ExportShipment,
            shipment.Direction == ShipmentDirection.Import
                ? shipment.PartnerFactionId
                : _world.PlayerFactionId,
            shipment.OriginRegionId,
            shipment.DestinationRegionId,
            $"{shipment.Direction}:{shipment.Resource}:{shipment.Amount}",
            1,
            shipment.Route);
        shipment.EntityId = entity.Id;
        _shipmentByEntity[entity.Id] = shipment.Id;
        shipment.State = ShipmentState.Travelling;
    }

    private void TickTravelling(WorldTradeShipment shipment, int day)
    {
        var entity = _entities.Get(shipment.EntityId);
        if (entity is null || entity.State == WorldEntityState.Cancelled)
        {
            shipment.State = ShipmentState.Cancelled;
            return;
        }
        shipment.CurrentRegionId = entity.CurrentRegionId;
        if (shipment.State == ShipmentState.Returning && entity.State == WorldEntityState.Completed)
        {
            if (shipment.Direction == ShipmentDirection.Export)
                _regions.DepositStock(shipment.OriginRegionId, shipment.Resource, shipment.Amount);
            shipment.State = ShipmentState.Cancelled;
            _shipmentByEntity.Remove(shipment.EntityId);
            return;
        }
        if (entity.State != WorldEntityState.Arrived) return;
        shipment.State = ShipmentState.Arrived;
        shipment.ArrivedDay = day;
        _arrivals.Add(new ShipmentArrival(
            shipment.Id,
            shipment.Direction,
            shipment.PartnerFactionId,
            shipment.DestinationRegionId,
            shipment.Resource,
            shipment.Amount,
            shipment.TotalValue,
            day));
    }

    private void TickUnloading(WorldTradeShipment shipment, double days, int day)
    {
        shipment.State = ShipmentState.Unloading;
        shipment.UnloadingProgress += days / UnloadingDays;
        if (shipment.UnloadingProgress < 1) return;
        if (shipment.Direction == ShipmentDirection.Import)
        {
            _regions.DepositStock(shipment.DestinationRegionId, shipment.Resource, shipment.Amount);
        }
        else
        {
            if (!_factions.CommitReservation(shipment.PartnerReservationId, -shipment.GrossValue))
            {
                _regions.DepositStock(shipment.OriginRegionId, shipment.Resource, shipment.Amount);
                shipment.State = ShipmentState.Cancelled;
                return;
            }
        }
        _entities.Complete(shipment.EntityId);
        _shipmentByEntity.Remove(shipment.EntityId);
        shipment.State = ShipmentState.Completed;
        shipment.ArrivedDay = day;
    }

    private IReadOnlyList<int>? ValidRoute(int originRegionId, int destinationRegionId)
    {
        var route = _world.FindRoute(originRegionId, destinationRegionId);
        if (route.Count == 0 || route.Count - 1 > MaximumRouteDistance) return null;
        return route;
    }

    private void TrimTerminal(int day)
    {
        foreach (var shipment in _shipments.Values.Where(value =>
                     value.IsTerminal && day - Math.Max(value.CreatedDay, value.ArrivedDay) > 8).ToArray())
            _shipments.Remove(shipment.Id);
    }
}
