using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.World;

namespace GodotSyxPort.Trade;

public enum WorldTradeOrderState : byte
{
    Planned,
    ShipmentCreated,
    Delivered,
    Cancelled
}

public sealed class WorldTradeOrder
{
    public long Id { get; init; }
    public ShipmentDirection Direction { get; init; }
    public int PartnerFactionId { get; init; }
    public ResourceKind Resource { get; init; }
    public int RequestedAmount { get; init; }
    public int UnitPrice { get; init; }
    public int Toll { get; init; }
    public int CreatedDay { get; init; }
    public WorldTradeOrderState State { get; internal set; }
    public long ShipmentId { get; internal set; }
    public int DeliveredAmount { get; internal set; }
}

public sealed record WorldTradeLedgerEntry(
    long OrderId,
    long ShipmentId,
    ShipmentDirection Direction,
    int PartnerFactionId,
    ResourceKind Resource,
    int Amount,
    int UnitPrice,
    int Toll,
    double CreditsAfter,
    int Day);

public sealed record WorldTradeOrderSnapshot(
    long Id,
    ShipmentDirection Direction,
    int PartnerFactionId,
    ResourceKind Resource,
    int RequestedAmount,
    int UnitPrice,
    int Toll,
    int CreatedDay,
    WorldTradeOrderState State,
    long ShipmentId,
    int DeliveredAmount);

public sealed record WorldTradeDiagnostics(
    int Planned,
    int InTransit,
    int Delivered,
    int Cancelled,
    int Imports,
    int Exports,
    int ImportedUnits,
    int ExportedUnits,
    double ImportCost,
    double ExportRevenue,
    double Credits,
    IReadOnlyDictionary<ResourceKind, int> NetUnits,
    IReadOnlyList<string> Problems);

/// <summary>
/// Settlement-facing controller for physical world trade. The older trade runtime still
/// owns policies and prices; this layer replaces instant transfers with routed shipments.
/// </summary>
public sealed class WorldTradeRuntime
{
    public const int MaximumOrderAmount = 400;
    public const int MaximumOrdersPerTick = 4;
    public const int LedgerLimit = 256;

    private readonly SettlementTradeRuntime _trade;
    private readonly SettlementLogisticsRuntime _logistics;
    private readonly WorldTradeShipmentRuntime _shipments;
    private readonly WorldFactionRuntime _factions;
    private readonly WorldSettlementBridge _bridge;
    private readonly Dictionary<long, WorldTradeOrder> _orders = new();
    private readonly List<WorldTradeLedgerEntry> _ledger = new();
    private long _nextId = 1;
    private double _timer;

    public WorldTradeRuntime(
        SettlementTradeRuntime trade,
        SettlementLogisticsRuntime logistics,
        WorldTradeShipmentRuntime shipments,
        WorldFactionRuntime factions,
        WorldSettlementBridge bridge)
    {
        _trade = trade;
        _logistics = logistics;
        _shipments = shipments;
        _factions = factions;
        _bridge = bridge;
    }

    public IReadOnlyCollection<WorldTradeOrder> Orders => _orders.Values;
    public IReadOnlyList<WorldTradeLedgerEntry> Ledger => _ledger;
    public double Credits
    {
        get => _trade.Credits;
        set => _trade.Credits = value;
    }

    public void PublishQuotes()
    {
        _trade.ClearQuotes();
        foreach (var quote in _factions.QuotesForPlayer(0)) _trade.SetQuote(quote);
    }

    public void Tick(double days, int day)
    {
        if (days <= 0 || !_bridge.Connected) return;
        ReconcileArrivals(day);
        _timer += days;
        if (_timer < SettlementTradeRuntime.TradeInterval) return;
        _timer %= SettlementTradeRuntime.TradeInterval;
        PublishQuotes();
        var created = 0;
        foreach (var resource in Enum.GetValues<ResourceKind>())
        {
            if (created >= MaximumOrdersPerTick) break;
            if (TryCreateImport(resource, day)) created++;
            if (created >= MaximumOrdersPerTick) break;
            if (TryCreateExport(resource, day)) created++;
        }
        TrimOrders(day);
    }

    public bool TryCreateImport(ResourceKind resource, int day)
    {
        var capital = _bridge.Profile;
        if (capital is null) return false;
        var policy = _trade.Policy(resource);
        if (policy.ImportLevel <= 0) return false;
        var capacity = _logistics.ImportCapacity(resource);
        if (capacity <= 0) return false;
        var desired = Math.Min(MaximumOrderAmount,
            Math.Max(1, (int)Math.Ceiling(capacity * policy.ImportLevel)));
        var candidate = BestImportQuote(resource, policy.MaximumImportPrice);
        if (candidate is null) return false;
        var amount = Math.Min(desired, candidate.SellerAmounts.GetValueOrDefault(resource));
        if (amount <= 0) return false;
        var partner = PartnerFaction(candidate.PartnerId);
        if (partner < 0) return false;
        var unitPrice = candidate.SellerPrices.GetValueOrDefault(resource, _trade.ReferencePrice(resource));
        var toll = _trade.Toll(candidate.Distance, candidate.ProximityTollBoost);
        var maximumAffordable = unitPrice <= 0 ? amount :
            (int)Math.Floor(Math.Max(0, Credits - toll) / unitPrice);
        amount = Math.Min(amount, maximumAffordable);
        if (amount <= 0) return false;
        var order = AddOrder(ShipmentDirection.Import, partner, resource, amount, unitPrice, toll, day);
        var shipment = _shipments.CreateImport(
            capital.CapitalRegionId,
            partner,
            resource,
            amount,
            unitPrice,
            toll,
            day);
        if (shipment is null)
        {
            order.State = WorldTradeOrderState.Cancelled;
            return false;
        }
        order.ShipmentId = shipment.Id;
        order.State = WorldTradeOrderState.ShipmentCreated;
        return true;
    }

    public bool TryCreateExport(ResourceKind resource, int day)
    {
        var capital = _bridge.Profile;
        if (capital is null) return false;
        var policy = _trade.Policy(resource);
        var available = _logistics.AvailableForExport(resource);
        var retained = (int)Math.Ceiling(available * policy.RetainedExportFraction);
        var amount = Math.Min(MaximumOrderAmount, Math.Max(0, available - retained));
        if (amount <= 0) return false;
        var candidate = BestExportQuote(resource, policy.MinimumExportPrice);
        if (candidate is null) return false;
        amount = Math.Min(amount, candidate.BuyerAmounts.GetValueOrDefault(resource));
        if (amount <= 0) return false;
        var partner = PartnerFaction(candidate.PartnerId);
        if (partner < 0) return false;
        var unitPrice = candidate.BuyerPrices.GetValueOrDefault(resource, _trade.ReferencePrice(resource));
        var toll = _trade.Toll(candidate.Distance, candidate.ProximityTollBoost);
        var extracted = _logistics.ExtractExport(resource, amount);
        if (extracted <= 0) return false;
        _bridge.DepositRegionalResource(resource, extracted);
        var order = AddOrder(ShipmentDirection.Export, partner, resource, extracted, unitPrice, toll, day);
        var shipment = _shipments.CreateExport(
            capital.CapitalRegionId,
            partner,
            resource,
            extracted,
            unitPrice,
            toll,
            day);
        if (shipment is null)
        {
            _bridge.WithdrawRegionalResource(resource, extracted);
            _logistics.DeliverImport(resource, extracted);
            order.State = WorldTradeOrderState.Cancelled;
            return false;
        }
        order.ShipmentId = shipment.Id;
        order.State = WorldTradeOrderState.ShipmentCreated;
        return true;
    }

    public bool Cancel(long orderId)
    {
        if (!_orders.TryGetValue(orderId, out var order) ||
            order.State is WorldTradeOrderState.Delivered or WorldTradeOrderState.Cancelled) return false;
        if (order.ShipmentId != 0) _shipments.Cancel(order.ShipmentId);
        order.State = WorldTradeOrderState.Cancelled;
        return true;
    }

    public IReadOnlyList<WorldTradeOrderSnapshot> CaptureOrders()
    {
        return _orders.Values.Select(order => new WorldTradeOrderSnapshot(
            order.Id,
            order.Direction,
            order.PartnerFactionId,
            order.Resource,
            order.RequestedAmount,
            order.UnitPrice,
            order.Toll,
            order.CreatedDay,
            order.State,
            order.ShipmentId,
            order.DeliveredAmount)).ToArray();
    }

    public void RestoreOrders(IEnumerable<WorldTradeOrderSnapshot> snapshots)
    {
        _orders.Clear();
        _nextId = 1;
        foreach (var snapshot in snapshots.OrderBy(value => value.Id))
        {
            var order = new WorldTradeOrder
            {
                Id = snapshot.Id,
                Direction = snapshot.Direction,
                PartnerFactionId = snapshot.PartnerFactionId,
                Resource = snapshot.Resource,
                RequestedAmount = Math.Max(0, snapshot.RequestedAmount),
                UnitPrice = Math.Max(0, snapshot.UnitPrice),
                Toll = Math.Max(0, snapshot.Toll),
                CreatedDay = Math.Max(0, snapshot.CreatedDay),
                State = snapshot.State,
                ShipmentId = snapshot.ShipmentId,
                DeliveredAmount = Math.Max(0, snapshot.DeliveredAmount)
            };
            _orders.Add(order.Id, order);
            _nextId = Math.Max(_nextId, order.Id + 1);
        }
    }

    public WorldTradeDiagnostics Diagnose()
    {
        var problems = new List<string>();
        var net = new Dictionary<ResourceKind, int>();
        foreach (var order in _orders.Values)
        {
            if (order.RequestedAmount <= 0)
                problems.Add($"Order {order.Id} has no requested cargo.");
            if (order.UnitPrice < 0)
                problems.Add($"Order {order.Id} has a negative price.");
            if (order.State == WorldTradeOrderState.ShipmentCreated &&
                _shipments.Get(order.ShipmentId) is null)
                problems.Add($"Order {order.Id} references missing shipment {order.ShipmentId}.");
            if (order.DeliveredAmount > order.RequestedAmount)
                problems.Add($"Order {order.Id} delivered more than requested.");
        }
        foreach (var entry in _ledger)
        {
            var direction = entry.Direction == ShipmentDirection.Import ? 1 : -1;
            net[entry.Resource] = net.GetValueOrDefault(entry.Resource) + entry.Amount * direction;
        }
        var imports = _ledger.Where(entry => entry.Direction == ShipmentDirection.Import).ToArray();
        var exports = _ledger.Where(entry => entry.Direction == ShipmentDirection.Export).ToArray();
        return new WorldTradeDiagnostics(
            _orders.Values.Count(value => value.State == WorldTradeOrderState.Planned),
            _orders.Values.Count(value => value.State == WorldTradeOrderState.ShipmentCreated),
            _orders.Values.Count(value => value.State == WorldTradeOrderState.Delivered),
            _orders.Values.Count(value => value.State == WorldTradeOrderState.Cancelled),
            imports.Length,
            exports.Length,
            imports.Sum(entry => entry.Amount),
            exports.Sum(entry => entry.Amount),
            imports.Sum(entry => entry.Amount * entry.UnitPrice + entry.Toll),
            exports.Sum(entry => entry.Amount * entry.UnitPrice - entry.Toll),
            Credits,
            net,
            problems);
    }

    public IReadOnlyList<string> DescribeMarket()
    {
        var diagnostics = Diagnose();
        var lines = new List<string>
        {
            $"Credits {diagnostics.Credits:0.##}",
            $"Orders: {diagnostics.Planned} planned, {diagnostics.InTransit} in transit, {diagnostics.Delivered} delivered, {diagnostics.Cancelled} cancelled",
            $"Imports: {diagnostics.Imports} shipments, {diagnostics.ImportedUnits} units, cost {diagnostics.ImportCost:0.##}",
            $"Exports: {diagnostics.Exports} shipments, {diagnostics.ExportedUnits} units, revenue {diagnostics.ExportRevenue:0.##}"
        };
        lines.AddRange(diagnostics.NetUnits.OrderBy(pair => pair.Key)
            .Select(pair => $"{pair.Key}: net {pair.Value:+#;-#;0}"));
        lines.AddRange(diagnostics.Problems.Select(problem => "Problem: " + problem));
        return lines;
    }

    private void ReconcileArrivals(int day)
    {
        foreach (var arrival in _shipments.Arrivals)
        {
            var order = _orders.Values.FirstOrDefault(value => value.ShipmentId == arrival.ShipmentId);
            if (order is null || order.State != WorldTradeOrderState.ShipmentCreated) continue;
            if (arrival.Direction == ShipmentDirection.Import)
            {
                var withdrawn = _bridge.WithdrawRegionalResource(arrival.Resource, arrival.Amount);
                var delivered = _logistics.DeliverImport(arrival.Resource, withdrawn);
                if (delivered < withdrawn) _bridge.DepositRegionalResource(arrival.Resource, withdrawn - delivered);
                order.DeliveredAmount = delivered;
                Credits -= arrival.Amount * order.UnitPrice + order.Toll;
            }
            else
            {
                order.DeliveredAmount = arrival.Amount;
                Credits += arrival.Amount * order.UnitPrice - order.Toll;
            }
            order.State = WorldTradeOrderState.Delivered;
            _ledger.Add(new WorldTradeLedgerEntry(
                order.Id,
                order.ShipmentId,
                order.Direction,
                order.PartnerFactionId,
                order.Resource,
                order.DeliveredAmount,
                order.UnitPrice,
                order.Toll,
                Credits,
                day));
        }
        if (_ledger.Count > LedgerLimit) _ledger.RemoveRange(0, _ledger.Count - LedgerLimit);
    }

    private TradePartnerQuote? BestImportQuote(ResourceKind resource, int maximumPrice)
    {
        return _trade.Quotes.Where(quote =>
                quote.SellerAmounts.GetValueOrDefault(resource) > 0 &&
                quote.SellerPrices.GetValueOrDefault(resource, int.MaxValue) <= maximumPrice)
            .OrderBy(quote => quote.SellerPrices.GetValueOrDefault(resource, int.MaxValue) +
                              _trade.Toll(quote.Distance, quote.ProximityTollBoost))
            .FirstOrDefault();
    }

    private TradePartnerQuote? BestExportQuote(ResourceKind resource, int minimumPrice)
    {
        return _trade.Quotes.Where(quote =>
                quote.BuyerAmounts.GetValueOrDefault(resource) > 0 &&
                quote.BuyerPrices.GetValueOrDefault(resource) >= minimumPrice)
            .OrderByDescending(quote => quote.BuyerPrices.GetValueOrDefault(resource) -
                                        _trade.Toll(quote.Distance, quote.ProximityTollBoost))
            .FirstOrDefault();
    }

    private WorldTradeOrder AddOrder(
        ShipmentDirection direction,
        int partnerFactionId,
        ResourceKind resource,
        int amount,
        int unitPrice,
        int toll,
        int day)
    {
        var order = new WorldTradeOrder
        {
            Id = _nextId++,
            Direction = direction,
            PartnerFactionId = partnerFactionId,
            Resource = resource,
            RequestedAmount = amount,
            UnitPrice = unitPrice,
            Toll = toll,
            CreatedDay = day,
            State = WorldTradeOrderState.Planned
        };
        _orders.Add(order.Id, order);
        return order;
    }

    private int PartnerFaction(string partnerId)
    {
        return _factions.States.Values.FirstOrDefault(value =>
            value.Key.Equals(partnerId, StringComparison.OrdinalIgnoreCase))?.FactionId ?? -1;
    }

    private void TrimOrders(int day)
    {
        foreach (var order in _orders.Values.Where(value =>
                     value.State is WorldTradeOrderState.Delivered or WorldTradeOrderState.Cancelled &&
                     day - value.CreatedDay > 32).ToArray())
            _orders.Remove(order.Id);
    }
}
