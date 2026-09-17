using System;

namespace GodotSyxPort.Rooms;

/// <summary>Adapted numerical core of RoomEmploymentSimple/RoomEmploymentIns.</summary>
public sealed class RoomEmploymentRuntime
{
    public int Maximum { get; private set; }
    public int Needed { get; private set; }
    public int Employed { get; private set; }
    public double WorkloadEfficiency { get; private set; } = 1.0;
    public double ProximityEfficiency { get; private set; } = 1.0;
    public double FetchEfficiency { get; private set; } = 1.0;
    public double TotalEfficiency => WorkloadEfficiency * ProximityEfficiency * FetchEfficiency;

    public void Configure(int maximum, int needed)
    {
        Maximum = Math.Max(0, maximum);
        Needed = Math.Clamp(needed, 0, Maximum);
        Employed = Math.Min(Employed, Maximum);
    }

    public void SetEmployed(int employed) => Employed = Math.Clamp(employed, 0, Maximum);

    public void UpdateEfficiency(double workload, double proximity, double fetch)
    {
        WorkloadEfficiency = Math.Clamp(workload, 0.0, 1.0);
        ProximityEfficiency = Math.Clamp(proximity, 0.0, 1.0);
        FetchEfficiency = Math.Clamp(fetch, 0.0, 1.0);
    }

    public void RecordCompletedCycle(double workSeconds, double travelSeconds)
    {
        if (workSeconds <= 0) return;
        var measuredProximity = Math.Clamp(1.0 - travelSeconds / workSeconds, 0.0, 1.0);
        // RoomEmploymentIns keeps 75% of the new sample and 25% of the last value.
        ProximityEfficiency = 0.75 * measuredProximity + 0.25 * ProximityEfficiency;
        WorkloadEfficiency = 1.0;
    }

    public void RecordFetchCycle(double workSeconds, double fetchSeconds)
    {
        if (workSeconds <= 0) return;
        // RoomEmploymentIns grants short fetches for free and smooths the daily
        // hauling sample with 75% new value and 25% previous value.
        // Original allowance is a 36-tile round trip. At the settlement
        // runtime's 3.2 tiles/second movement rate this is 22.5 seconds.
        const double freeFetchSeconds = 22.5;
        var charged = Math.Max(0.0, fetchSeconds - freeFetchSeconds);
        var measuredFetch = Math.Clamp(1.0 - charged / workSeconds, 0.0, 1.0);
        FetchEfficiency = 0.75 * measuredFetch + 0.25 * FetchEfficiency;
    }
}
