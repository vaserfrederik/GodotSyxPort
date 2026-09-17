using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class JanitorInstanceRuntime
{
    public int RoomId { get; init; }
    public GridCoord Table { get; set; }
    public int Employed { get; set; }
    public Dictionary<ResourceKind, int> Stock { get; } = new();
    public HashSet<ResourceKind> Fetching { get; } = new();
}

/// <summary>
/// Shared resource cache from janitor BITS/JanitorInstance. Five-bit counters
/// cap at 31; source refill targets clamp to 4..28 and scale with employed
/// janitors and settlement maintenance demand.
/// </summary>
public sealed class JanitorRuntime
{
    public const int Radius = 150;
    private const int CounterMaximum = 31;
    private const int TargetMaximum = CounterMaximum - 3;
    private readonly Dictionary<int, JanitorInstanceRuntime> _instances = new();

    public IReadOnlyCollection<JanitorInstanceRuntime> Instances => _instances.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational &&
                     room.DefinitionKey.Equals("_JANITOR", StringComparison.OrdinalIgnoreCase)))
        {
            live.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
            {
                instance = new JanitorInstanceRuntime { RoomId = room.Id };
                _instances.Add(room.Id, instance);
            }
            instance.Table = fromIndex(room.Cells.First());
            instance.Employed = room.Employment.Employed;
        }
        foreach (var roomId in _instances.Keys.Where(id => !live.Contains(id)).ToArray())
            _instances.Remove(roomId);
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var instance in _instances.Values) instance.Fetching.Clear();
        foreach (var job in jobs.Where(job => job.Kind == BuildKind.JanitorSupply &&
                     job.State is not (JobState.Completed or JobState.Cancelled)))
            if (_instances.TryGetValue(job.RoomId, out var instance)) instance.Fetching.Add(job.Resource);
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source,
        IReadOnlyDictionary<ResourceKind, double> dailyDemand)
    {
        if (source is null) return;
        foreach (var instance in _instances.Values)
        foreach (var pair in dailyDemand.Where(pair => pair.Value > 0))
        {
            var target = Target(instance.Employed, pair.Value);
            var stored = instance.Stock.GetValueOrDefault(pair.Key);
            if (stored >= target || instance.Fetching.Contains(pair.Key)) continue;
            var amount = Math.Min(BuildJob.MaximumFetchAmount,
                Math.Min(target - stored, resources.Get(pair.Key)));
            if (amount > 0 && jobs.Add(BuildJob.JanitorSupply(
                    instance.Table, source.Value, instance.RoomId, pair.Key, amount)))
                instance.Fetching.Add(pair.Key);
        }
    }

    public int CompleteSupply(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var instance)) return amount;
        var stored = instance.Stock.GetValueOrDefault(job.Resource);
        var accepted = Math.Min(amount, CounterMaximum - stored);
        instance.Stock[job.Resource] = stored + accepted;
        instance.Fetching.Remove(job.Resource);
        return amount - accepted;
    }

    public bool TryConsume(GridCoord target, ResourceKind resource)
    {
        var instance = _instances.Values
            .Where(value => Distance(value.Table, target) <= Radius &&
                            value.Stock.GetValueOrDefault(resource) > 0)
            .OrderBy(value => Distance(value.Table, target)).FirstOrDefault();
        if (instance is null) return false;
        instance.Stock[resource]--;
        return true;
    }

    public static int Target(int employed, double globalEstimate) =>
        Math.Clamp((int)(Math.Max(0, employed) * Math.Max(0, globalEstimate)), 4, TargetMaximum);

    private static int Distance(GridCoord a, GridCoord b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Z - b.Z);
}
