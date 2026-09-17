using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.Trade;

public sealed record TradePartnerQuote(
    string PartnerId,
    int Distance,
    double ProximityTollBoost,
    double ImportTariff,
    double ExportTariff,
    IReadOnlyDictionary<ResourceKind, int> SellerPrices,
    IReadOnlyDictionary<ResourceKind, int> SellerAmounts,
    IReadOnlyDictionary<ResourceKind, int> BuyerPrices,
    IReadOnlyDictionary<ResourceKind, int> BuyerAmounts);

public sealed record TradeTransaction(
    string PartnerId, ResourceKind Resource, int Amount, int UnitPrice,
    int Fee, bool Import, double CreditsAfter);
public sealed record TradePolicy(
    ResourceKind Resource, double ImportLevel, int MaximumImportPrice,
    double RetainedExportFraction, int MinimumExportPrice);

/// <summary>
/// Settlement side of FBUYER/FSELLER, PBuyer/PSeller and TradeManager. Strategic
/// factions inject quotes; this runtime deliberately performs no fabricated trade
/// while the world/faction layer is absent.
/// </summary>
public sealed class SettlementTradeRuntime
{
    public const int AveragePrice = 400;
    public const int MaximumPlayerPrice = 1_000_000;
    public const double TollPerTile = 100.0 / 400.0;
    public const double TradeInterval = 1.0;

    private readonly SettlementLogisticsRuntime _logistics;
    private readonly Dictionary<string, TradePartnerQuote> _quotes =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ResourceKind, int> _buyerPriceCaps = new();
    private readonly Dictionary<ResourceKind, int> _sellerMinimumPrices = new();
    private readonly Dictionary<ResourceKind, double> _importLevels = new();
    private readonly Dictionary<ResourceKind, double> _exportLimits = new();
    private readonly Dictionary<(string Partner, ResourceKind Resource), int> _partnerSold = new();
    private readonly Dictionary<(string Partner, ResourceKind Resource), int> _partnerBought = new();
    private double _timer;

    public double Credits { get; set; }
    public IReadOnlyCollection<TradePartnerQuote> Quotes => _quotes.Values;
    public List<TradeTransaction> RecentTransactions { get; } = new();

    public SettlementTradeRuntime(SettlementLogisticsRuntime logistics) => _logistics = logistics;

    public void SetQuote(TradePartnerQuote quote)
    {
        _quotes[quote.PartnerId] = quote;
        foreach (var key in _partnerSold.Keys.Where(key =>
                     key.Partner.Equals(quote.PartnerId, StringComparison.OrdinalIgnoreCase)).ToArray())
            _partnerSold.Remove(key);
        foreach (var key in _partnerBought.Keys.Where(key =>
                     key.Partner.Equals(quote.PartnerId, StringComparison.OrdinalIgnoreCase)).ToArray())
            _partnerBought.Remove(key);
    }
    public bool RemoveQuote(string partnerId) => _quotes.Remove(partnerId);
    public void ClearQuotes() => _quotes.Clear();

    public void ConfigureImport(ResourceKind resource, double level, int maximumUnitPrice)
    {
        _importLevels[resource] = Math.Clamp(level, 0, 1);
        _buyerPriceCaps[resource] = Math.Clamp(maximumUnitPrice, 0, MaximumPlayerPrice);
    }

    public void ConfigureExport(ResourceKind resource, double retainedFraction, int minimumUnitPrice)
    {
        _exportLimits[resource] = Math.Clamp(retainedFraction, 0, 1);
        _sellerMinimumPrices[resource] = Math.Max(1, minimumUnitPrice);
    }

    public TradePolicy Policy(ResourceKind resource) => new(
        resource,
        _importLevels.GetValueOrDefault(resource, 1.0),
        _buyerPriceCaps.GetValueOrDefault(resource, MaximumPlayerPrice),
        _exportLimits.GetValueOrDefault(resource, 0.25),
        _sellerMinimumPrices.GetValueOrDefault(resource, 1));

    public int ReferencePrice(ResourceKind resource)
    {
        var offers = _quotes.Values
            .Where(quote => quote.SellerPrices.ContainsKey(resource) &&
                            quote.SellerAmounts.GetValueOrDefault(resource) > 0)
            .Select(quote => quote.SellerPrices[resource]).ToArray();
        if (offers.Length > 0) return (int)Math.Round(offers.Average());
        var source = OriginalGameData.Current.TradeResource(resource.ToString());
        return Math.Max(1, (int)Math.Round(AveragePrice * source.PriceMultiplier));
    }

    public void Tick(double delta, ResourceLedger resources, bool tradeOpen)
    {
        if (!tradeOpen || _quotes.Count == 0) return;
        _timer += Math.Max(0, delta);
        while (_timer >= TradeInterval)
        {
            _timer -= TradeInterval;
            ExecuteOneImport(resources);
            ExecuteOneExport(resources);
        }
        if (RecentTransactions.Count > 128)
            RecentTransactions.RemoveRange(0, RecentTransactions.Count - 128);
    }

    public int Toll(int distance, double proximityTollBoost, bool playerInvolved = true)
    {
        var toll = (20 + Math.Max(0, distance)) * TollPerTile;
        toll /= playerInvolved ? Math.Max(0.01, proximityTollBoost) : 4.0;
        return Math.Max(0, (int)Math.Floor(toll));
    }

    public int Fee(TradePartnerQuote quote, int amount, bool importing)
    {
        var tariff = importing ? quote.ImportTariff : quote.ExportTariff;
        return Math.Max(0, (int)Math.Floor((Toll(
            quote.Distance, quote.ProximityTollBoost) + Math.Max(0, tariff)) * amount));
    }

    private void ExecuteOneImport(ResourceLedger resources)
    {
        foreach (var resource in Enum.GetValues<ResourceKind>())
        {
            var capacity = _logistics.ImportCapacity(resource);
            if (capacity <= 0) continue;
            var target = _importLevels.GetValueOrDefault(resource, 1.0);
            if (target <= 0) continue;
            var wantedByPolicy = (int)Math.Ceiling(
                _logistics.ImportTotalCapacity(resource) * target) -
                _logistics.ImportStored(resource);
            if (wantedByPolicy <= 0) continue;
            var cap = _buyerPriceCaps.GetValueOrDefault(resource, MaximumPlayerPrice);
            var quote = _quotes.Values
                .Where(item => SellerRemaining(item, resource) > 0 &&
                               item.SellerPrices.GetValueOrDefault(resource, int.MaxValue) <= cap)
                .OrderBy(item => item.SellerPrices[resource]).FirstOrDefault();
            if (quote is null) continue;
            var amount = Math.Min(BuildJobMaximumCarry,
                Math.Min(wantedByPolicy, Math.Min(capacity, SellerRemaining(quote, resource))));
            var unitPrice = quote.SellerPrices[resource];
            var affordable = amount;
            while (affordable > 0 && affordable * unitPrice + Fee(quote, affordable, true) > Credits)
                affordable--;
            if (affordable <= 0) continue;
            var fee = Fee(quote, affordable, true);
            var delivered = _logistics.DeliverImport(resource, affordable);
            if (delivered <= 0) continue;
            Credits -= delivered * unitPrice + fee;
            resources.Add(resource, delivered);
            var inventoryKey = (quote.PartnerId, resource);
            _partnerSold[inventoryKey] = _partnerSold.GetValueOrDefault(inventoryKey) + delivered;
            RecentTransactions.Add(new TradeTransaction(
                quote.PartnerId, resource, delivered, unitPrice, fee, true, Credits));
            return;
        }
    }

    private void ExecuteOneExport(ResourceLedger resources)
    {
        foreach (var resource in Enum.GetValues<ResourceKind>())
        {
            var available = _logistics.AvailableForExport(resource);
            if (available <= 0) continue;
            var minimum = _sellerMinimumPrices.GetValueOrDefault(resource, 1);
            var quote = _quotes.Values
                .Where(item => BuyerRemaining(item, resource) > 0 &&
                               item.BuyerPrices.GetValueOrDefault(resource) >= minimum)
                .OrderByDescending(item => item.BuyerPrices[resource]).FirstOrDefault();
            if (quote is null) continue;
            var amount = Math.Min(BuildJobMaximumCarry,
                Math.Min(available, BuyerRemaining(quote, resource)));
            var retained = _exportLimits.GetValueOrDefault(resource, 0.25);
            var allowedByStock = Math.Max(0, resources.Get(resource) -
                (int)Math.Ceiling(resources.Get(resource) * retained));
            amount = Math.Min(amount, allowedByStock);
            if (amount <= 0) continue;
            var extracted = _logistics.ExtractExport(resource, amount);
            if (extracted <= 0 || !resources.TryTake(resource, extracted)) continue;
            var unitPrice = quote.BuyerPrices[resource];
            var fee = Fee(quote, extracted, false);
            Credits += extracted * unitPrice - fee;
            var inventoryKey = (quote.PartnerId, resource);
            _partnerBought[inventoryKey] = _partnerBought.GetValueOrDefault(inventoryKey) + extracted;
            RecentTransactions.Add(new TradeTransaction(
                quote.PartnerId, resource, extracted, unitPrice, fee, false, Credits));
            return;
        }
    }

    // TradeShipper operates in larger world shipments; settlement depots expose
    // hand-sized batches to keep citizen hauling bounded by AIModule_Work.MAX_FETCH_AMOUNT.
    private const int BuildJobMaximumCarry = 6;

    private int SellerRemaining(TradePartnerQuote quote, ResourceKind resource) =>
        Math.Max(0, quote.SellerAmounts.GetValueOrDefault(resource) -
                    _partnerSold.GetValueOrDefault((quote.PartnerId, resource)));

    private int BuyerRemaining(TradePartnerQuote quote, ResourceKind resource) =>
        Math.Max(0, quote.BuyerAmounts.GetValueOrDefault(resource) -
                    _partnerBought.GetValueOrDefault((quote.PartnerId, resource)));
}
