using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Rooms;
using GodotSyxPort.Simulation;
using Godot;

namespace GodotSyxPort.Citizens;

public sealed class CorpseRecord
{
    public int Id { get; init; }
    public GridCoord Cell { get; set; }
    public string Cause { get; init; } = "";
    public bool Reserved { get; set; }
    public bool PickedUp { get; set; }
    public double Decay { get; set; }
    public bool HasMeat => Decay < 2;
}

/// <summary>ThingsCorpses/PlanBuryCorpse adapter with reservable physical bodies.</summary>
public sealed class CorpseRuntime
{
    public const int MaximumCorpses = 2048 * 4;
    private readonly RoomSystem _rooms;
    private readonly Dictionary<int, CorpseRecord> _corpses = new();
    private readonly Dictionary<int, int> _destinations = new();
    private int _nextId = 1;
    private double _decayAccumulator;
    private double _dayAccumulator;
    private int _createdToday;
    private readonly List<int> _addedHistory = new();
    public IReadOnlyCollection<CorpseRecord> All => _corpses.Values;

    public CorpseRuntime(RoomSystem rooms) => _rooms = rooms;

    public CorpseRecord? Create(GridCoord cell, string cause)
    {
        if (_corpses.Count >= MaximumCorpses) return null;
        var corpse = new CorpseRecord { Id = _nextId++, Cell = cell, Cause = cause };
        _corpses.Add(corpse.Id, corpse);
        _createdToday++;
        return corpse;
    }

    public double UnburiedPressure(int population, int daysBack = 0)
    {
        var amount = daysBack <= 0 ? _createdToday :
            daysBack <= _addedHistory.Count ? _addedHistory[^daysBack] : 0;
        return 40.0 * amount / (1.0 + Math.Max(0, population));
    }

    public void Tick(double delta, double secondsPerDay)
    {
        _decayAccumulator += Math.Max(0, delta);
        while (_decayAccumulator >= 100)
        {
            _decayAccumulator -= 100;
            foreach (var corpse in _corpses.Values.ToArray())
            {
                corpse.Decay += 0.05 * GD.Randf();
                if (corpse.Decay <= 2.5 || corpse.Reserved && corpse.Decay <= 20) continue;
                Cancel(corpse.Id);
                _corpses.Remove(corpse.Id);
            }
        }

        _dayAccumulator += Math.Max(0, delta);
        while (_dayAccumulator >= Math.Max(1, secondsPerDay))
        {
            _dayAccumulator -= Math.Max(1, secondsPerDay);
            _addedHistory.Add(_createdToday);
            if (_addedHistory.Count > 32) _addedHistory.RemoveAt(0);
            _createdToday = 0;
        }
    }

    public void Schedule(JobBoard jobs)
    {
        foreach (var corpse in _corpses.Values.Where(corpse => !corpse.Reserved).ToArray())
        {
            var destination = _rooms.Burials.Reserve(corpse.Cell);
            if (destination is null) continue;
            corpse.Reserved = true;
            _destinations[corpse.Id] = destination.Value.SlotId;
            if (!jobs.Add(BuildJob.CorpseHaul(
                    corpse.Id, corpse.Cell, destination.Value.Cell,
                    destination.Value.SlotId, destination.Value.WorkSeconds)))
                Cancel(corpse.Id);
        }
    }

    public bool Pickup(BuildJob job)
    {
        if (!_corpses.TryGetValue(job.CorpseId, out var corpse) ||
            !corpse.Reserved || corpse.PickedUp) return false;
        corpse.PickedUp = true;
        job.PickedUp = true;
        return true;
    }

    public bool Deliver(BuildJob job)
    {
        if (!_corpses.TryGetValue(job.CorpseId, out var corpse) || !corpse.PickedUp) return false;
        if (!_destinations.Remove(job.CorpseId, out var slotId) ||
            !_rooms.Burials.Complete(slotId, corpse.Cause)) return false;
        _corpses.Remove(job.CorpseId);
        return true;
    }

    public void Drop(BuildJob job, GridCoord cell)
    {
        if (!_corpses.TryGetValue(job.CorpseId, out var corpse)) return;
        corpse.Cell = cell;
        corpse.PickedUp = false;
        Cancel(job.CorpseId);
    }

    public void Cancel(int corpseId)
    {
        if (_corpses.TryGetValue(corpseId, out var corpse))
        {
            corpse.Reserved = false;
            corpse.PickedUp = false;
        }
        if (_destinations.Remove(corpseId, out var slotId)) _rooms.Burials.Cancel(slotId);
    }
}
