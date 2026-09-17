using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

public sealed class NurseryRuntimeInstance
{
    public int RoomId { get; init; }
    public int Stations { get; set; }
    public int AvailableUses { get; set; }
    public int DailyCapacity => Stations * ChildcareRuntime.ChildrenPerEmployee;
}

public sealed class BreederRuntimeInstance
{
    public int RoomId { get; init; }
    public GridCoord SpawnCell { get; set; }
    public int Workplaces { get; set; }
    public double ChildProgress { get; set; }
    public string Race { get; set; } = "GARTHIMI";
    public int IncubationDays { get; set; } = 8;
}

/// <summary>Consolidates ROOM_NURSERY/Station and ROOM_BREEDER/Station source families.</summary>
public sealed class ChildcareRuntime
{
    public const int ChildrenPerEmployee = 10;
    public const double NurseryPlaySeconds = 120.0;
    public const double BreederWorkSeconds = 30.0;
    private readonly Dictionary<int, NurseryRuntimeInstance> _nurseries = new();
    private readonly Dictionary<int, BreederRuntimeInstance> _breeders = new();
    private double _dayLeft;

    public IReadOnlyCollection<NurseryRuntimeInstance> Nurseries => _nurseries.Values;
    public IReadOnlyCollection<BreederRuntimeInstance> Breeders => _breeders.Values;
    public int NurseryCapacity => _nurseries.Values.Sum(room => room.DailyCapacity);

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var active = rooms.Where(room => room.State == RoomState.Operational).ToArray();
        var ids = active.Select(room => room.Id).ToHashSet();
        foreach (var id in _nurseries.Keys.Where(id => !ids.Contains(id)).ToArray()) _nurseries.Remove(id);
        foreach (var id in _breeders.Keys.Where(id => !ids.Contains(id)).ToArray()) _breeders.Remove(id);
        foreach (var room in active)
        {
            var stations = Math.Max(1, room.Employment.Maximum);
            if (room.DefinitionKey.Equals("NURSERY_NORMAL", StringComparison.OrdinalIgnoreCase))
            {
                if (!_nurseries.TryGetValue(room.Id, out var nursery))
                    _nurseries.Add(room.Id, nursery = new NurseryRuntimeInstance { RoomId = room.Id });
                nursery.Stations = stations;
                nursery.AvailableUses = Math.Min(nursery.AvailableUses, nursery.DailyCapacity);
            }
            else if (room.DefinitionKey.StartsWith("BREEDER_", StringComparison.OrdinalIgnoreCase))
            {
                if (!_breeders.TryGetValue(room.Id, out var breeder))
                    _breeders.Add(room.Id, breeder = new BreederRuntimeInstance { RoomId = room.Id });
                breeder.Workplaces = stations;
                breeder.SpawnCell = fromIndex(room.Cells.First());
                breeder.Race = room.DefinitionKey["BREEDER_".Length..];
                breeder.IncubationDays = room.DefinitionKey.Equals("BREEDER_GARTHIMI", StringComparison.OrdinalIgnoreCase)
                    ? 8 : breeder.IncubationDays;
            }
        }
    }

    public bool TryUseNursery()
    {
        var nursery = _nurseries.Values.FirstOrDefault(room => room.AvailableUses > 0);
        if (nursery is null) return false;
        nursery.AvailableUses--;
        return true;
    }

    public void Tick(double delta, double secondsPerDay, ResourceLedger resources,
        Func<BreederRuntimeInstance, bool> canProduce, Action<BreederRuntimeInstance> produce)
    {
        _dayLeft -= delta;
        if (_dayLeft <= 0)
        {
            _dayLeft += secondsPerDay;
            foreach (var nursery in _nurseries.Values) nursery.AvailableUses = nursery.DailyCapacity;
        }
        foreach (var breeder in _breeders.Values)
        {
            if (breeder.Workplaces <= 0 || !canProduce(breeder)) continue;
            breeder.ChildProgress += breeder.Workplaces * delta /
                                      (secondsPerDay * Math.Max(1, breeder.IncubationDays));
            while (breeder.ChildProgress >= 1.0 && canProduce(breeder))
            {
                // BREEDER_GARTHIMI INDUSTRY.IN consumes two meat for each completed child.
                if (!resources.TryTake(ResourceKind.Meat, 2)) break;
                breeder.ChildProgress -= 1.0;
                produce(breeder);
            }
        }
    }
}
