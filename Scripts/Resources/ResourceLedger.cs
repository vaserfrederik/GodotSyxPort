using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Resources;

public enum ResourceKind : byte
{
    Wood,
    Stone,
    Grain,
    Food,
    Tools,
    Furniture,
    Beer,
    Wine,
    ArmourLeather,
    ArmourPlate,
    Bow,
    Clay,
    Clothes,
    Coal,
    Cotton,
    Egg,
    Fabric,
    Fish,
    Fruit,
    Gem,
    Herb,
    Jewelry,
    Leather,
    Machinery,
    Meat,
    Metal,
    Mushroom,
    Opiates,
    Ore,
    Paper,
    Pottery,
    Ration,
    Sithilon,
    CutStone,
    Vegetable,
    WeaponHammer,
    WeaponMount,
    WeaponShield,
    WeaponShort,
    WeaponSlash,
    WeaponSpear,
    Livestock
}

public sealed class ResourceLedger
{
    private readonly Dictionary<ResourceKind, int> _amounts =
        System.Enum.GetValues<ResourceKind>().ToDictionary(kind => kind, _ => 0);

    public ResourceLedger() { }

    public void InitializeLandingParty(IEnumerable<LandingResourceRule> resources)
    {
        foreach (var kind in System.Enum.GetValues<ResourceKind>()) _amounts[kind] = 0;
        foreach (var resource in resources)
            _amounts[resource.Resource] += System.Math.Max(0, resource.Amount);
    }
    public static int KindCount => System.Enum.GetValues<ResourceKind>().Length;
    private readonly int[] _totalProduced = new int[KindCount];
    private readonly int[] _totalConsumed = new int[KindCount];
    private readonly int[] _periodProduced = new int[KindCount];
    private readonly int[] _periodConsumed = new int[KindCount];

    public int Get(ResourceKind kind) => _amounts.GetValueOrDefault(kind);

    public bool TryReserve(BuildJob job, int maximumAmount = int.MaxValue)
    {
        if (job.ResourceReserved) return true;
        if (job.IsConstruction && job.IsPreparationPhase) return true;
        if (job.Kind == BuildKind.Production)
        {
            foreach (var input in job.ProductionInputs)
                if (Get(input.Key) < input.Value) return false;
            foreach (var input in job.ProductionInputs)
            {
                _amounts[input.Key] = Get(input.Key) - input.Value;
            }
            job.ResourceReserved = true;
            return true;
        }
        if (job.IsSupplyJob)
        {
            var availableSupply = Get(job.Resource);
            if (availableSupply < job.ResourceAmount) return false;
            _amounts[job.Resource] = availableSupply - job.ResourceAmount;
            job.ResourceReserved = true;
            return true;
        }
        var available = Get(job.Resource);
        var amount = job.IsConstruction
            ? System.Math.Min(
                System.Math.Min(System.Math.Min(job.MaterialsNeeded, BuildJob.MaximumFetchAmount), maximumAmount),
                available)
            : job.ResourceAmount;
        if (amount == 0)
            return job.IsConstruction ? job.MaterialsNeeded == 0 : job.ResourceAmount == 0;
        if (available < amount) return false;
        _amounts[job.Resource] = available - amount;
        RecordConsumed(job.Resource, amount);
        if (job.IsConstruction) job.ReserveMaterials(amount);
        else job.ResourceReserved = true;
        return true;
    }

    public void Refund(BuildJob job)
    {
        if (!job.ResourceReserved) return;
        if (job.Kind == BuildKind.Production)
        {
            foreach (var input in job.ProductionInputs)
            {
                _amounts[input.Key] = Get(input.Key) + input.Value;
            }
            job.ResourceReserved = false;
            return;
        }
        if (job.IsSupplyJob)
        {
            _amounts[job.Resource] = Get(job.Resource) + job.ResourceAmount;
            job.ResourceReserved = false;
            return;
        }
        var amount = job.IsConstruction ? job.ReservedAmount : job.ResourceAmount;
        _amounts[job.Resource] = Get(job.Resource) + amount;
        RollbackConsumed(job.Resource, amount);
        if (job.IsConstruction) job.TakeReservedMaterials();
        else job.ResourceReserved = false;
    }

    public void FinalizeProduction(
        BuildJob job,
        IReadOnlyDictionary<ResourceKind, int> actuallyConsumed)
    {
        if (!job.ResourceReserved) return;
        foreach (var reserved in job.ProductionInputs)
        {
            var consumed = actuallyConsumed.GetValueOrDefault(reserved.Key);
            var refund = System.Math.Max(0, reserved.Value - consumed);
            if (refund > 0) _amounts[reserved.Key] = Get(reserved.Key) + refund;
            if (consumed > 0) RecordConsumed(reserved.Key, consumed);
        }
        job.ResourceReserved = false;
    }

    public int PickupProductionSupply(BuildJob job)
    {
        if (!job.IsSupplyJob || !job.ResourceReserved) return 0;
        var amount = job.TakeReservedResource();
        if (amount > 0) RecordConsumed(job.Resource, amount);
        return amount;
    }

    public void Restore(int wood, int stone)
    {
        _amounts[ResourceKind.Wood] = wood;
        _amounts[ResourceKind.Stone] = stone;
    }

    public void Add(ResourceKind kind, int amount)
    {
        _amounts[kind] = Get(kind) + amount;
        _totalProduced[(int)kind] += amount;
        _periodProduced[(int)kind] += amount;
    }

    /// <summary>Returns previously issued stock without counting it as new production.</summary>
    public void ReturnToStock(ResourceKind kind, int amount)
    {
        if (amount > 0) _amounts[kind] = Get(kind) + amount;
    }

    public void RestoreInTransit(ResourceKind kind, int amount)
    {
        if (amount <= 0) return;
        _amounts[kind] = Get(kind) + amount;
        RollbackConsumed(kind, amount);
    }

    public bool TryTake(ResourceKind kind, int amount)
    {
        var current = Get(kind);
        if (current < amount) return false;
        _amounts[kind] = current - amount;
        RecordConsumed(kind, amount);
        return true;
    }

    public void Restore(ResourceKind kind, int amount) => _amounts[kind] = amount;

    public (int[] Produced, int[] Consumed) TakePeriodFlow()
    {
        var produced = (int[])_periodProduced.Clone();
        var consumed = (int[])_periodConsumed.Clone();
        System.Array.Clear(_periodProduced);
        System.Array.Clear(_periodConsumed);
        return (produced, consumed);
    }

    public int[] CaptureTotalProduced() => (int[])_totalProduced.Clone();
    public int[] CaptureTotalConsumed() => (int[])_totalConsumed.Clone();
    public int[] CaptureAmounts() => System.Enum.GetValues<ResourceKind>()
        .Select(Get).ToArray();

    public void RestoreAmounts(IReadOnlyList<int> amounts)
    {
        foreach (var kind in System.Enum.GetValues<ResourceKind>())
            _amounts[kind] = (int)kind < amounts.Count ? amounts[(int)kind] : 0;
    }

    public void RestoreTelemetry(int[] produced, int[] consumed)
    {
        System.Array.Copy(produced, _totalProduced, System.Math.Min(produced.Length, _totalProduced.Length));
        System.Array.Copy(consumed, _totalConsumed, System.Math.Min(consumed.Length, _totalConsumed.Length));
        System.Array.Clear(_periodProduced);
        System.Array.Clear(_periodConsumed);
    }

    private void RecordConsumed(ResourceKind kind, int amount)
    {
        _totalConsumed[(int)kind] += amount;
        _periodConsumed[(int)kind] += amount;
    }

    private void RollbackConsumed(ResourceKind kind, int amount)
    {
        var index = (int)kind;
        _totalConsumed[index] = System.Math.Max(0, _totalConsumed[index] - amount);
        _periodConsumed[index] = System.Math.Max(0, _periodConsumed[index] - amount);
    }
}
