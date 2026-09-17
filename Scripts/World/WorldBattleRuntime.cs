using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.World;

public enum WorldBattleKind : byte { Field, Garrison, Siege }
public enum WorldBattleDecision : byte { AutoResolve, Retreat, DefendSettlement }

public sealed record WorldBattleSideSnapshot(
    IReadOnlyList<long> ArmyIds, int RegionId, int FactionId, int Men, double Power);

public sealed record WorldBattleSnapshot(
    long Id, WorldBattleKind Kind, WorldBattleSideSnapshot Attacker,
    WorldBattleSideSnapshot Defender, bool AwaitingPlayer, bool Resolved,
    int WinnerFactionId, int AttackerLosses, int DefenderLosses,
    bool SettlementInvasion);

/// <summary>
/// Strategic conflict queue adapted from PFieldBattle, PRegAttack, PSiege, Resolver and
/// ResolverSide. Battles involving the player remain pending for the battle UI; NPC-only
/// conflicts use Resolver's power balance, retreat and extraction fractions.
/// </summary>
public sealed class WorldBattleRuntime
{
    private readonly StrategicWorldRuntime _world;
    private readonly RegionalEconomyRuntime _economy;
    private readonly WorldEntityRuntime _entities;
    private readonly WorldArmyRuntime _armies;
    private readonly List<WorldBattleSnapshot> _history = new();
    private readonly HashSet<string> _registered = new(StringComparer.Ordinal);
    private long _nextId = 1;

    public WorldBattleRuntime(StrategicWorldRuntime world, RegionalEconomyRuntime economy,
        WorldEntityRuntime entities, WorldArmyRuntime armies)
    {
        _world = world;
        _economy = economy;
        _entities = entities;
        _armies = armies;
    }

    public WorldBattleSnapshot? Pending { get; private set; }
    public WorldBattleSnapshot? ActiveSettlementInvasion { get; private set; }
    public IReadOnlyList<WorldBattleSnapshot> History => _history;

    public void Tick()
    {
        SynchronizeSieges();
        if (Pending is not null) return;
        if (PollFieldBattle()) return;
        PollSiege();
    }

    public bool ResolvePending(bool engage)
        => ResolvePending(engage ? WorldBattleDecision.AutoResolve : WorldBattleDecision.Retreat);

    public bool ResolvePending(WorldBattleDecision decision)
    {
        if (Pending is not { } battle) return false;
        if (decision == WorldBattleDecision.DefendSettlement)
        {
            if (!battle.SettlementInvasion) return false;
            ActiveSettlementInvasion = battle with { AwaitingPlayer = false };
            foreach (var armyId in battle.Attacker.ArmyIds) _armies.Disband(armyId);
            Pending = null;
            return true;
        }
        if (decision == WorldBattleDecision.Retreat)
        {
            var player = PlayerSide(battle);
            var totalPower = battle.Attacker.Power + battle.Defender.Power;
            var playerBalance = totalPower <= 0 ? 0.5 : player.Power / totalPower;
            foreach (var armyId in PlayerSide(battle).ArmyIds)
                if (_armies.Get(armyId) is { } army)
                {
                    var retreat = FindRetreatRegion(army);
                    if (retreat >= 0)
                    {
                        _armies.ApplyLosses(army, Math.Clamp(1 - playerBalance, 0, 1));
                        if (army.Men > 0) _armies.RelocateAndFortify(army, retreat);
                    }
                    else _armies.ApplyLosses(army, 1.0);
                }
            Pending = battle with { Resolved = true, AwaitingPlayer = false,
                WinnerFactionId = OtherSide(battle).FactionId };
            _history.Add(Pending);
            Pending = null;
            return true;
        }
        Resolve(battle);
        if (Pending?.Id != battle.Id) Pending = null;
        return true;
    }

    public IReadOnlyList<WorldBattleSnapshot> Capture() =>
        _history.Concat(Pending is null ? Array.Empty<WorldBattleSnapshot>() : new[] { Pending })
            .Concat(ActiveSettlementInvasion is null
                ? Array.Empty<WorldBattleSnapshot>() : new[] { ActiveSettlementInvasion }).ToArray();

    public bool CompleteSettlementInvasion(bool playerVictory, int attackerLosses, int defenderLosses)
    {
        if (ActiveSettlementInvasion is not { } battle) return false;
        var winner = playerVictory ? battle.Defender.FactionId : battle.Attacker.FactionId;
        _history.Add(battle with
        {
            AwaitingPlayer = false, Resolved = true, WinnerFactionId = winner,
            AttackerLosses = Math.Max(0, attackerLosses),
            DefenderLosses = Math.Max(0, defenderLosses)
        });
        if (!playerVictory)
            _world.SetStance(battle.Attacker.FactionId, _world.PlayerFactionId,
                DiplomacyStance.Vassal);
        _economy.SetBesieged(battle.Defender.RegionId, false);
        ActiveSettlementInvasion = null;
        return true;
    }

    public void Restore(IEnumerable<WorldBattleSnapshot> snapshots)
    {
        _history.Clear(); Pending = null; ActiveSettlementInvasion = null; _registered.Clear();
        foreach (var battle in snapshots.OrderBy(value => value.Id))
        {
            _nextId = Math.Max(_nextId, battle.Id + 1);
            if (battle.SettlementInvasion && !battle.Resolved && !battle.AwaitingPlayer)
                ActiveSettlementInvasion = battle;
            else if (battle.AwaitingPlayer && !battle.Resolved) Pending = battle;
            else _history.Add(battle);
        }
    }

    private bool PollFieldBattle()
    {
        foreach (var army in _armies.Armies.OrderBy(value => value.Id))
        {
            var entity = _entities.Get(army.EntityId);
            if (entity is null) continue;
            var enemy = NearbyArmies(entity.CurrentRegionId).FirstOrDefault(other =>
                other.Id != army.Id && Enemies(army.FactionId, other.FactionId));
            if (enemy is null) continue;
            var key = PairKey("F", army.Id, enemy.Id);
            if (!_registered.Add(key)) continue;
            Begin(WorldBattleKind.Field, army, enemy, -1);
            return true;
        }
        return false;
    }

    private bool PollSiege()
    {
        foreach (var attacker in _armies.Armies.Where(value =>
                     value.State == WorldArmyState.Besieging && value.TargetRegionId >= 0))
        {
            var region = _world.Region(attacker.TargetRegionId);
            var entity = _entities.Get(attacker.EntityId);
            var state = _economy.State(attacker.TargetRegionId);
            if (region is null || entity?.State != WorldEntityState.Arrived || state is null ||
                region.OwnerFactionId < 0 || !Enemies(attacker.FactionId, region.OwnerFactionId)) continue;
            var key = $"S:{attacker.Id}:{region.Id}";
            if (!_registered.Add(key)) continue;
            var defenders = AlliedArmies(region.Id, region.OwnerFactionId).ToArray();
            var attack = Side(new[] { attacker }, -1, attacker.FactionId);
            var defend = Side(defenders, region.Id, region.OwnerFactionId);
            // Resolver.besige adds regional power multiplied by fortification.
            defend = defend with { Men = defend.Men + (int)Math.Ceiling(state.Garrison),
                Power = defend.Power + state.Garrison * (1 + Math.Max(0, state.Fortification)) };
            // PSiege leaves an outmatched NPC besieger in place instead of forcing a battle.
            if (attacker.FactionId != _world.PlayerFactionId && defend.Power > attack.Power) continue;
            Begin(new WorldBattleSnapshot(_nextId++, WorldBattleKind.Siege, attack, defend,
                PlayerInvolved(attack, defend), false, -1, 0, 0, false));
            return true;
        }
        return false;
    }

    private void Begin(WorldBattleKind kind, WorldArmyRecord first,
        WorldArmyRecord second, int regionId)
    {
        var a = AlliedArmiesAround(first, second.FactionId).ToArray();
        var b = AlliedArmiesAround(second, first.FactionId).ToArray();
        Begin(new WorldBattleSnapshot(_nextId++, kind,
            Side(a, regionId, first.FactionId), Side(b, regionId, second.FactionId),
            PlayerInvolved(Side(a, regionId, first.FactionId),
                Side(b, regionId, second.FactionId)), false, -1, 0, 0, false));
    }

    private void Begin(WorldBattleSnapshot battle)
    {
        if (battle.AwaitingPlayer) Pending = battle;
        else Resolve(battle);
    }

    private void Resolve(WorldBattleSnapshot battle)
    {
        var total = battle.Attacker.Power + battle.Defender.Power;
        var attackerBalance = total <= 0 ? 0.5 : battle.Attacker.Power / total;
        var defenderBalance = 1 - attackerBalance;
        var attackerWins = attackerBalance >= defenderBalance;
        var winner = attackerWins ? battle.Attacker : battle.Defender;
        var loser = attackerWins ? battle.Defender : battle.Attacker;
        var loserBalance = attackerWins ? defenderBalance : attackerBalance;
        var defendedRegion = battle.Kind == WorldBattleKind.Siege
            ? _world.Region(battle.Defender.RegionId) : null;
        if (attackerWins && defendedRegion is { Capital: true } &&
            battle.Defender.FactionId == _world.PlayerFactionId)
        {
            Pending = battle with
            {
                AwaitingPlayer = true, Resolved = false,
                WinnerFactionId = battle.Attacker.FactionId, SettlementInvasion = true
            };
            return;
        }
        if (battle.Kind == WorldBattleKind.Field && !PlayerInvolved(battle.Attacker, battle.Defender) &&
            loser.ArmyIds.FirstOrDefault(-1) is var retreatingId && retreatingId >= 0 &&
            _armies.Get(retreatingId) is { } retreatingArmy &&
            FindRetreatRegion(retreatingArmy) is var retreatRegion && retreatRegion >= 0)
        {
            var retreatLosses = Extract(loser, Math.Clamp(1 - loserBalance, 0, 1));
            foreach (var armyId in loser.ArmyIds)
                if (_armies.Get(armyId) is { Men: > 0 } army)
                    _armies.RelocateAndFortify(army, retreatRegion);
            _history.Add(battle with
            {
                AwaitingPlayer = false, Resolved = true, WinnerFactionId = winner.FactionId,
                AttackerLosses = attackerWins ? 0 : retreatLosses,
                DefenderLosses = attackerWins ? retreatLosses : 0
            });
            return;
        }
        var loserFraction = AutoValue(attackerWins ? defenderBalance : attackerBalance);
        var winnerFraction = 1 - (attackerWins ? attackerBalance : defenderBalance);
        var loserLosses = Extract(loser, loserFraction);
        var winnerLosses = Extract(winner, winnerFraction);
        if (battle.Kind == WorldBattleKind.Siege && battle.Defender.RegionId >= 0 && attackerWins)
        {
            var region = _world.Region(battle.Defender.RegionId);
            if (region is not null)
            {
                var attackerFactionPower = _armies.Armies.Where(value =>
                    value.FactionId == battle.Attacker.FactionId).Sum(value => (double)value.Men);
                var defenderFactionPower = _armies.Armies.Where(value =>
                    value.FactionId == battle.Defender.FactionId).Sum(value => (double)value.Men);
                var conquest = _world.ConquerRegion(region.Id, battle.Attacker.FactionId,
                    attackerFactionPower, defenderFactionPower);
                if (conquest is not null)
                {
                    _economy.ApplyConquest(region.Id, 1 - attackerBalance, 1 - attackerBalance);
                    if (conquest.FactionDefeated) _armies.DisbandFaction(conquest.PreviousFactionId);
                }
            }
        }
        var result = battle with
        {
            AwaitingPlayer = false, Resolved = true, WinnerFactionId = winner.FactionId,
            AttackerLosses = attackerWins ? winnerLosses : loserLosses,
            DefenderLosses = attackerWins ? loserLosses : winnerLosses
        };
        _history.Add(result);
    }

    private int Extract(WorldBattleSideSnapshot side, double fraction)
    {
        var garrisonBefore = side.RegionId >= 0
            ? (int)Math.Ceiling(_economy.State(side.RegionId)?.Garrison ?? 0) : 0;
        var before = side.ArmyIds.Select(_armies.Get).Where(value => value is not null)
            .Sum(value => value!.Men) + garrisonBefore;
        foreach (var armyId in side.ArmyIds)
            if (_armies.Get(armyId) is { } army) _armies.ApplyLosses(army, fraction);
        if (side.RegionId >= 0 && _economy.State(side.RegionId) is { } state)
            state.Garrison = Math.Max(0, state.Garrison - Math.Ceiling(state.Garrison * fraction));
        var garrisonAfter = side.RegionId >= 0
            ? (int)Math.Ceiling(_economy.State(side.RegionId)?.Garrison ?? 0) : 0;
        var after = side.ArmyIds.Select(_armies.Get).Where(value => value is not null)
            .Sum(value => value!.Men) + garrisonAfter;
        return Math.Max(0, before - after);
    }

    private void SynchronizeSieges()
    {
        foreach (var region in _world.Regions)
        {
            var besieged = _armies.Armies.Any(army => army.State == WorldArmyState.Besieging &&
                army.TargetRegionId == region.Id && _entities.Get(army.EntityId)?.State == WorldEntityState.Arrived &&
                Enemies(army.FactionId, region.OwnerFactionId));
            _economy.SetBesieged(region.Id, besieged);
        }
    }

    /// <summary>Util.retTile: breadth-first retreat, limited to eight regional steps.</summary>
    private int FindRetreatRegion(WorldArmyRecord army)
    {
        var entity = _entities.Get(army.EntityId);
        if (entity is null) return -1;
        var startPower = EnemyPower(army.FactionId, entity.CurrentRegionId);
        var queue = new Queue<(int RegionId, int Distance)>();
        var visited = new HashSet<int> { entity.CurrentRegionId };
        queue.Enqueue((entity.CurrentRegionId, 0));
        while (queue.Count > 0)
        {
            var (regionId, distance) = queue.Dequeue();
            var enemyPower = EnemyPower(army.FactionId, regionId);
            if (distance > 0 && enemyPower <= startPower && enemyPower < army.Men &&
                MayRetreatInto(army.FactionId, regionId)) return regionId;
            if (distance >= 8) continue;
            foreach (var next in _world.Region(regionId)?.Neighbours ?? new List<int>())
                if (visited.Add(next)) queue.Enqueue((next, distance + 1));
        }
        return -1;
    }

    private double EnemyPower(int factionId, int regionId)
    {
        var power = 0.0;
        var region = _world.Region(regionId);
        if (region is not null && Enemies(factionId, region.OwnerFactionId))
            power += _economy.State(regionId)?.Garrison ?? 0;
        power += NearbyArmies(regionId).Where(army => Enemies(factionId, army.FactionId))
            .Sum(army => (double)army.Men);
        return power;
    }

    private bool MayRetreatInto(int factionId, int regionId)
    {
        var owner = _world.Region(regionId)?.OwnerFactionId ?? -1;
        return owner < 0 || owner == factionId || _world.Stance(factionId, owner) is
            DiplomacyStance.Allied or DiplomacyStance.Vassal or DiplomacyStance.Overlord;
    }

    private IEnumerable<WorldArmyRecord> NearbyArmies(int regionId)
    {
        var regions = new HashSet<int> { regionId };
        foreach (var neighbour in (IEnumerable<int>?)_world.Region(regionId)?.Neighbours ??
                                  Enumerable.Empty<int>())
            regions.Add(neighbour);
        return _armies.Armies.Where(army => _entities.Get(army.EntityId) is { } entity &&
            regions.Contains(entity.CurrentRegionId));
    }

    private IEnumerable<WorldArmyRecord> AlliedArmiesAround(WorldArmyRecord anchor, int enemyFactionId)
    {
        var entity = _entities.Get(anchor.EntityId);
        if (entity is null) return new[] { anchor };
        return NearbyArmies(entity.CurrentRegionId).Where(army =>
            army.Id == anchor.Id || !Enemies(anchor.FactionId, army.FactionId) &&
            Enemies(army.FactionId, enemyFactionId)).Take(WorldArmyRuntime.DivisionsPerArmy);
    }

    private IEnumerable<WorldArmyRecord> AlliedArmies(int regionId, int factionId) =>
        _armies.Armies.Where(army => _entities.Get(army.EntityId)?.CurrentRegionId == regionId &&
            !Enemies(factionId, army.FactionId));

    private WorldBattleSideSnapshot Side(IEnumerable<WorldArmyRecord> armies,
        int regionId, int factionId)
    {
        var selected = armies.DistinctBy(value => value.Id).ToArray();
        var men = selected.Sum(value => value.Men);
        var playerPenalty = factionId == _world.PlayerFactionId ? 0.8 : 1.0;
        return new WorldBattleSideSnapshot(selected.Select(value => value.Id).ToArray(),
            regionId, factionId, men, men * playerPenalty);
    }

    private bool Enemies(int first, int second) => first >= 0 && second >= 0 &&
        first != second && _world.Stance(first, second) == DiplomacyStance.War;
    private bool PlayerInvolved(WorldBattleSideSnapshot a, WorldBattleSideSnapshot b) =>
        a.FactionId == _world.PlayerFactionId || b.FactionId == _world.PlayerFactionId;
    private WorldBattleSideSnapshot PlayerSide(WorldBattleSnapshot battle) =>
        battle.Attacker.FactionId == _world.PlayerFactionId ? battle.Attacker : battle.Defender;
    private WorldBattleSideSnapshot OtherSide(WorldBattleSnapshot battle) =>
        battle.Attacker.FactionId == _world.PlayerFactionId ? battle.Defender : battle.Attacker;
    private static double AutoValue(double powerBalance) => powerBalance < 0.5
        ? 1.0 : Math.Clamp(1.0 - powerBalance, 0, 1);
    private static string PairKey(string prefix, long first, long second) =>
        first < second ? $"{prefix}:{first}:{second}" : $"{prefix}:{second}:{first}";
}
