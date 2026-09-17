using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Resources;

namespace GodotSyxPort.World;

public enum WorldArmyState : byte
{
    Fortified,
    Fortifying,
    Raiding,
    Moving,
    Intercepting,
    Besieging,
    MovingRaid
}

public sealed record WorldArmyDivisionState(
    int SourceDivisionId,
    string Name,
    string Race,
    int Men,
    int TargetMen,
    IReadOnlyDictionary<string, double> Training,
    IReadOnlyDictionary<ResourceKind, int> Equipment);

public sealed class WorldArmyRecord
{
    public long Id { get; init; }
    public long EntityId { get; init; }
    public int FactionId { get; init; }
    public string Name { get; set; } = "Army";
    public WorldArmyState State { get; internal set; }
    public double StateTimeDays { get; internal set; }
    public int TargetRegionId { get; internal set; } = -1;
    public long TargetArmyId { get; internal set; } = -1;
    public List<WorldArmyDivisionState> Divisions { get; } = new();
    public Dictionary<ResourceKind, int> Supplies { get; } = new();
    public int Men => Divisions.Sum(division => division.Men);
}

public sealed record WorldArmySnapshot(
    long Id,
    long EntityId,
    int FactionId,
    string Name,
    WorldArmyState State,
    double StateTimeDays,
    int TargetRegionId,
    long TargetArmyId,
    IReadOnlyList<WorldArmyDivisionState> Divisions,
    IReadOnlyDictionary<ResourceKind, int> Supplies);

/// <summary>
/// Strategic army collection adapted from ADArmies, ADDivs and WArmy. Movement is
/// carried by the shared world-entity path, while army state remains separate as in
/// WArmyState. Battle resolution is intentionally owned by the following battle port.
/// </summary>
public sealed class WorldArmyRuntime
{
    public const int MenPerDivision = 200;
    public const int DivisionsPerArmy = 120;
    public const int MenPerArmy = MenPerDivision * DivisionsPerArmy;
    private const double FortificationDays = 0.5;
    private readonly StrategicWorldRuntime _world;
    private readonly WorldEntityRuntime _entities;
    private readonly Dictionary<long, WorldArmyRecord> _armies = new();
    private long _nextId = 1;

    public WorldArmyRuntime(StrategicWorldRuntime world, WorldEntityRuntime entities)
    {
        _world = world;
        _entities = entities;
    }

    public IReadOnlyCollection<WorldArmyRecord> Armies => _armies.Values;
    public WorldArmyRecord? Get(long id) => _armies.GetValueOrDefault(id);

    public WorldArmyRecord? Create(
        int factionId,
        int regionId,
        string name,
        IEnumerable<WorldArmyDivisionState> divisions)
    {
        if (_world.Faction(factionId) is null || _world.Region(regionId) is null) return null;
        var selected = divisions.Take(DivisionsPerArmy).Where(value => value.Men > 0).ToArray();
        if (selected.Length == 0 || selected.Sum(value => value.Men) > MenPerArmy) return null;
        var entity = _entities.Create(
            WorldEntityKind.Army, factionId, regionId, regionId, "ARMY", 1.0);
        var army = new WorldArmyRecord
        {
            Id = _nextId++, EntityId = entity.Id, FactionId = factionId,
            Name = string.IsNullOrWhiteSpace(name) ? "Army" : name.Trim(),
            State = WorldArmyState.Fortifying
        };
        army.Divisions.AddRange(selected);
        _armies.Add(army.Id, army);
        return army;
    }

    public bool Move(long armyId, int destinationRegionId)
    {
        if (!_armies.TryGetValue(armyId, out var army) || army.Men <= 0) return false;
        var entity = _entities.Get(army.EntityId);
        if (entity is null || entity.IsTerminal || _world.Region(destinationRegionId) is null) return false;
        var route = RouteFrom(entity.CurrentRegionId, destinationRegionId);
        return route.Count > 0 && route.Skip(1).All(regionId => MayEnter(army.FactionId, regionId)) &&
               ReplaceMovingEntity(army, entity, destinationRegionId, route,
                   WorldArmyState.Moving, -1, -1);
    }

    public bool Besiege(long armyId, int destinationRegionId, bool declareWar)
    {
        if (!_armies.TryGetValue(armyId, out var army) || army.Men <= 0) return false;
        var entity = _entities.Get(army.EntityId);
        var region = _world.Region(destinationRegionId);
        if (entity is null || entity.IsTerminal || region is null ||
            region.OwnerFactionId < 0 || region.OwnerFactionId == army.FactionId) return false;
        if (_world.Stance(army.FactionId, region.OwnerFactionId) != DiplomacyStance.War)
        {
            if (!declareWar || !_world.SetStance(
                    army.FactionId, region.OwnerFactionId, DiplomacyStance.War)) return false;
        }
        var route = RouteFrom(entity.CurrentRegionId, destinationRegionId);
        if (route.Count == 0 || route.Skip(1).SkipLast(1).Any(
                regionId => !MayEnter(army.FactionId, regionId))) return false;
        return ReplaceMovingEntity(army, entity, destinationRegionId, route,
            WorldArmyState.Besieging, destinationRegionId, -1);
    }

    public bool Intercept(long armyId, long targetArmyId)
    {
        if (!_armies.TryGetValue(armyId, out var army) ||
            !_armies.TryGetValue(targetArmyId, out var target) || army.FactionId == target.FactionId ||
            _world.Stance(army.FactionId, target.FactionId) != DiplomacyStance.War) return false;
        var entity = _entities.Get(army.EntityId);
        var targetEntity = _entities.Get(target.EntityId);
        if (entity is null || targetEntity is null || entity.IsTerminal || targetEntity.IsTerminal) return false;
        var route = RouteFrom(entity.CurrentRegionId, targetEntity.CurrentRegionId);
        return route.Count > 0 && ReplaceMovingEntity(army, entity,
            targetEntity.CurrentRegionId, route, WorldArmyState.Intercepting, -1, targetArmyId);
    }

    public void Tick(double days)
    {
        foreach (var army in _armies.Values.ToArray())
        {
            var entity = _entities.Get(army.EntityId);
            if (entity is null || entity.IsTerminal || army.Men <= 0)
            {
                _armies.Remove(army.Id);
                continue;
            }
            if (army.State == WorldArmyState.Intercepting)
            {
                UpdateInterception(army, entity);
            }
            else if (army.State == WorldArmyState.Besieging &&
                     entity.State == WorldEntityState.Arrived)
            {
                army.StateTimeDays += days;
            }
            else if (army.State == WorldArmyState.Moving && entity.State == WorldEntityState.Arrived)
            {
                army.State = WorldArmyState.Fortifying;
                army.StateTimeDays = 0;
            }
            else if (army.State == WorldArmyState.Fortifying)
            {
                army.StateTimeDays += days;
                if (army.StateTimeDays >= FortificationDays)
                    army.State = WorldArmyState.Fortified;
            }
        }
    }

    public IReadOnlyList<WorldArmySnapshot> Capture() => _armies.Values.Select(army =>
        new WorldArmySnapshot(army.Id, army.EntityId, army.FactionId, army.Name,
            army.State, army.StateTimeDays, army.TargetRegionId, army.TargetArmyId,
            army.Divisions.ToArray(),
            new Dictionary<ResourceKind, int>(army.Supplies))).ToArray();

    public void Restore(IEnumerable<WorldArmySnapshot> snapshots)
    {
        _armies.Clear();
        foreach (var snapshot in snapshots)
        {
            var entity = _entities.Get(snapshot.EntityId);
            if (entity is null || entity.Kind != WorldEntityKind.Army || entity.IsTerminal) continue;
            var army = new WorldArmyRecord
            {
                Id = snapshot.Id, EntityId = snapshot.EntityId, FactionId = snapshot.FactionId,
                Name = snapshot.Name, State = snapshot.State,
                StateTimeDays = Math.Max(0, snapshot.StateTimeDays),
                TargetRegionId = snapshot.TargetRegionId,
                TargetArmyId = snapshot.TargetArmyId
            };
            army.Divisions.AddRange(snapshot.Divisions.Take(DivisionsPerArmy));
            foreach (var supply in snapshot.Supplies) army.Supplies[supply.Key] = Math.Max(0, supply.Value);
            _armies[army.Id] = army;
            _nextId = Math.Max(_nextId, army.Id + 1);
        }
    }

    private bool MayEnter(int factionId, int regionId)
    {
        var owner = _world.Region(regionId)?.OwnerFactionId ?? -1;
        return owner == factionId || owner >= 0 &&
            _world.Stance(factionId, owner) is DiplomacyStance.Allied or DiplomacyStance.Vassal or
                DiplomacyStance.Overlord;
    }

    internal void ApplyLosses(WorldArmyRecord army, double lossFraction)
    {
        var fraction = Math.Clamp(lossFraction, 0, 1);
        for (var i = 0; i < army.Divisions.Count; i++)
        {
            var division = army.Divisions[i];
            var losses = (int)Math.Ceiling(division.Men * fraction);
            army.Divisions[i] = division with { Men = Math.Max(0, division.Men - losses) };
        }
        army.Divisions.RemoveAll(division => division.Men <= 0);
        foreach (var resource in army.Supplies.Keys.ToArray())
            army.Supplies[resource] = Math.Max(0,
                army.Supplies[resource] - (int)Math.Ceiling(army.Supplies[resource] * 1.1 * fraction));
    }

    internal void DisbandFaction(int factionId)
    {
        foreach (var army in _armies.Values.Where(value => value.FactionId == factionId).ToArray())
        {
            _entities.Cancel(army.EntityId);
            _armies.Remove(army.Id);
        }
    }

    internal void Disband(long armyId)
    {
        if (!_armies.Remove(armyId, out var army)) return;
        _entities.Cancel(army.EntityId);
    }

    internal void StopAndFortify(WorldArmyRecord army)
    {
        var entity = _entities.Get(army.EntityId);
        if (entity is null) return;
        RelocateAndFortify(army, entity.CurrentRegionId);
    }

    internal void RelocateAndFortify(WorldArmyRecord army, int regionId)
    {
        var entity = _entities.Get(army.EntityId);
        if (entity is null || _world.Region(regionId) is null) return;
        _entities.Cancel(entity.Id);
        var replacement = _entities.Create(WorldEntityKind.Army, army.FactionId,
            regionId, regionId, "ARMY", 1.0);
        ReplaceRecord(army, replacement.Id, WorldArmyState.Fortifying, -1, -1);
    }

    private void UpdateInterception(WorldArmyRecord army, WorldEntityRecord entity)
    {
        if (!_armies.TryGetValue(army.TargetArmyId, out var target))
        {
            StopAndFortify(army);
            return;
        }
        var targetEntity = _entities.Get(target.EntityId);
        if (targetEntity is null || targetEntity.IsTerminal)
        {
            StopAndFortify(army);
            return;
        }
        if (entity.DestinationRegionId == targetEntity.CurrentRegionId) return;
        var route = RouteFrom(entity.CurrentRegionId, targetEntity.CurrentRegionId);
        if (route.Count > 0) ReplaceMovingEntity(army, entity, targetEntity.CurrentRegionId,
            route, WorldArmyState.Intercepting, -1, target.Id);
    }

    private IReadOnlyList<int> RouteFrom(int origin, int destination)
    {
        if (origin == destination) return new[] { origin };
        var found = _world.FindRoute(origin, destination);
        if (found.Count == 0) return Array.Empty<int>();
        return new[] { origin }.Concat(found).ToArray();
    }

    private bool ReplaceMovingEntity(WorldArmyRecord army, WorldEntityRecord entity,
        int destination, IReadOnlyList<int> route, WorldArmyState state,
        int targetRegionId, long targetArmyId)
    {
        _entities.Cancel(entity.Id);
        var replacement = _entities.Create(WorldEntityKind.Army, army.FactionId,
            entity.CurrentRegionId, destination, "ARMY", 1.0, route);
        ReplaceRecord(army, replacement.Id, state, targetRegionId, targetArmyId);
        return true;
    }

    private void ReplaceRecord(WorldArmyRecord army, long entityId, WorldArmyState state,
        int targetRegionId, long targetArmyId)
    {
        var restored = new WorldArmyRecord
        {
            Id = army.Id, EntityId = entityId, FactionId = army.FactionId,
            Name = army.Name, State = state, TargetRegionId = targetRegionId,
            TargetArmyId = targetArmyId
        };
        restored.Divisions.AddRange(army.Divisions);
        foreach (var supply in army.Supplies) restored.Supplies[supply.Key] = supply.Value;
        _armies[army.Id] = restored;
    }
}
