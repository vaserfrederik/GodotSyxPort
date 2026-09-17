using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class TempleInstanceRuntime
{
    public int RoomId { get; init; }
    public GridCoord Cell { get; set; }
    public string Religion { get; set; } = "";
    public TempleSacrificeRule Sacrifice { get; set; } = new("", "", 0);
    public int Altars { get; set; }
    public int Stored { get; set; }
    public int Sacrifices { get; set; }
    public int SacrificesTotal { get; set; }
    public int YearConsumed { get; set; }
    public bool SupplyPending { get; set; }

    public double SacrificeValue => SacrificesTotal <= 0
        ? 0
        : Math.Clamp(Sacrifices / (double)SacrificesTotal, 0, 1);
}

/// <summary>
/// Shared altar/sacrifice runtime for every TEMPLE_* blueprint. It preserves the original
/// target stock, six-unit hauling batches, stochastic daily consumption and history decay.
/// Human sacrifices intentionally remain unavailable until the prisoner runtime exists.
/// </summary>
public sealed class TempleRuntime
{
    private readonly Dictionary<int, TempleInstanceRuntime> _instances = new();
    private double _dayLeft;

    public IReadOnlyDictionary<int, TempleInstanceRuntime> Instances => _instances;

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            var rule = OriginalGameData.Current.Room(room.DefinitionKey);
            if (rule?.Sacrifice is null || !room.DefinitionKey.StartsWith(
                    "TEMPLE_", StringComparison.OrdinalIgnoreCase)) continue;
            active.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var temple))
            {
                temple = new TempleInstanceRuntime { RoomId = room.Id };
                _instances[room.Id] = temple;
            }
            temple.Cell = fromIndex(room.Cells.First());
            temple.Religion = rule.Religion;
            temple.Sacrifice = rule.Sacrifice;
            temple.Altars = Math.Max(1, room.Employment.Maximum);
        }
        foreach (var id in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
            _instances.Remove(id);
    }

    public void Reconcile(IEnumerable<BuildJob> jobs)
    {
        foreach (var temple in _instances.Values) temple.SupplyPending = false;
        foreach (var job in jobs.Where(job => job.Kind == BuildKind.TempleSupply &&
                     job.State is not (JobState.Completed or JobState.Cancelled)))
            if (_instances.TryGetValue(job.RoomId, out var temple)) temple.SupplyPending = true;
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source)
    {
        if (source is null) return;
        foreach (var temple in _instances.Values)
        {
            if (temple.SupplyPending || !UsesResource(temple.Sacrifice)) continue;
            if (!OriginalGameData.TryMapResource(temple.Sacrifice.Resource, out var resource)) continue;
            var perAltar = Math.Clamp((int)Math.Ceiling(temple.Sacrifice.Time * 3.0), 0, 10);
            var missing = Math.Max(0, perAltar * temple.Altars - temple.Stored);
            var amount = Math.Min(BuildJob.MaximumFetchAmount, Math.Min(missing, resources.Get(resource)));
            if (amount <= 0) continue;
            if (jobs.Add(BuildJob.TempleSupply(temple.Cell, source.Value, temple.RoomId, resource, amount)))
                temple.SupplyPending = true;
        }
    }

    public int CompleteSupply(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var temple)) return amount;
        temple.Stored += amount;
        temple.SupplyPending = false;
        return 0;
    }

    public void Tick(double delta, int secondsPerDay)
    {
        if (_dayLeft <= 0) _dayLeft = secondsPerDay;
        _dayLeft -= delta;
        while (_dayLeft <= 0)
        {
            _dayLeft += secondsPerDay;
            foreach (var temple in _instances.Values) ConsumeDay(temple);
        }
    }

    public double ServiceQuality(int roomId) => _instances.TryGetValue(roomId, out var temple)
        ? temple.SacrificeValue
        : 0;

    private static bool UsesResource(TempleSacrificeRule sacrifice) =>
        sacrifice.Type.Equals("RESOURCE", StringComparison.OrdinalIgnoreCase) ||
        sacrifice.Type.Equals("ANIMAL", StringComparison.OrdinalIgnoreCase);

    private static void ConsumeDay(TempleInstanceRuntime temple)
    {
        temple.Sacrifices = (temple.Sacrifices + 1) / 2;
        temple.SacrificesTotal = (temple.SacrificesTotal + 1) / 2;
        var requested = 0;
        for (var altar = 0; altar < temple.Altars; altar++)
        {
            var whole = (int)Math.Floor(temple.Sacrifice.Time);
            requested += whole;
            if (GD.Randf() < temple.Sacrifice.Time - whole) requested++;
        }
        temple.SacrificesTotal += requested;
        if (!UsesResource(temple.Sacrifice)) return;
        var consumed = Math.Min(requested, temple.Stored);
        temple.Stored -= consumed;
        temple.Sacrifices += consumed;
        temple.YearConsumed += consumed;
    }
}
