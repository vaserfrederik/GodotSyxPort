using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Citizens;
using GodotSyxPort.Navigation;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.World;

namespace GodotSyxPort.Military;

public enum SettlementInvasionPhase : byte
{
    Warning, Bombardment, Deploying, Fighting, Victory, Defeat, Done, AwaitingPrisoners
}

public sealed record InvasionCohortSnapshot(int Men, int PathIndex, double StepProgress, bool Deployed,
    DivisionBattleTask Task = DivisionBattleTask.Move,
    DivisionFormation Formation = DivisionFormation.Tight, bool Routing = false,
    int Casualties = 0, double Morale = 1);
public sealed record SettlementInvasionSnapshot(
    long BattleId, SettlementInvasionPhase Phase, double PhaseSeconds, int Edge,
    int EnemyDeaths, int PlayerLosses, bool EngagementResolved, int RoutedEnemies,
    IReadOnlyList<InvasionCohortSnapshot> Cohorts, double CombatAccumulator = 0);

/// <summary>
/// Settlement-side Invador/Invasion/SpotMaker adaptation. Enemy divisions enter from
/// the world-facing map edge, wait through warning/bombardment/deployment and then use
/// settlement navigation toward the throne. Tactical damage is supplied by military combat.
/// </summary>
public sealed partial class SettlementInvasionRuntime : Node3D
{
    private sealed class ProjectileTrace
    {
        public Vector3 From, To;
        public double Progress;
    }
    private sealed class Cohort
    {
        public int Men;
        public int PathIndex;
        public double StepProgress;
        public bool Deployed;
        public DivisionBattleTask Task = DivisionBattleTask.Move;
        public DivisionFormation Formation = DivisionFormation.Tight;
        public bool Routing;
        public int Casualties;
        public double Morale = 1;
    }

    private GridWorld _grid = null!;
    private StrategicWorldRuntime _world = null!;
    private WorldBattleRuntime _battles = null!;
    private RoomSystem _rooms = null!;
    private ResourceLedger _resources = null!;
    private CitizenSystem _citizens = null!;
    private JobBoard _jobs = null!;
    private Func<GridCoord?> _throne = null!;
    private readonly List<Cohort> _cohorts = new();
    private List<GridCoord> _path = new();
    private MultiMesh _mesh = null!;
    private MultiMesh _projectileMesh = null!;
    private readonly List<ProjectileTrace> _projectiles = new();
    private readonly Dictionary<int, double> _structureDamage = new();
    private long _battleId = -1;
    private int _edge;
    private int _enemyDeaths;
    private int _playerLosses;
    private double _phaseSeconds;
    private double _deploymentAccumulator;
    private bool _engagementResolved;
    private int _routedEnemies;
    private double _combatAccumulator;
    public SettlementInvasionPhase Phase { get; private set; } = SettlementInvasionPhase.Done;
    public bool Active => Phase is not SettlementInvasionPhase.Done;
    public int EnemyMen => _cohorts.Sum(value => value.Men);
    public int EnemyDeaths => _enemyDeaths;
    public int PlayerLosses => _playerLosses;
    public int RoutedEnemies => _routedEnemies;
    public int PrisonCapacity => Math.Max(0, _rooms.Asylums.PrisonersMaximum - _rooms.Asylums.Prisoners);
    public bool AwaitingPrisonerDecision => Phase == SettlementInvasionPhase.AwaitingPrisoners;

    public int EnemyCohortAt(GridCoord cell, int radius = 3)
    {
        var best = _cohorts.Select((cohort, index) => (Cohort: cohort, Index: index,
                Distance: Manhattan(cell, CohortCell(cohort))))
            .Where(value => value.Cohort.Deployed && value.Cohort.Men > 0 &&
                            value.Distance <= Math.Max(0, radius))
            .OrderBy(value => value.Distance).FirstOrDefault();
        return best.Cohort is null ? -1 : best.Index;
    }

    public void Initialize(GridWorld grid, StrategicWorldRuntime world,
        WorldBattleRuntime battles, RoomSystem rooms, ResourceLedger resources,
        CitizenSystem citizens, JobBoard jobs, Func<GridCoord?> throne)
    {
        _grid = grid;
        _world = world;
        _battles = battles;
        _rooms = rooms;
        _resources = resources;
        _citizens = citizens;
        _jobs = jobs;
        _throne = throne;
        _mesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = new BoxMesh
            {
                Size = new Vector3(0.75f, 0.9f, 0.75f),
                Material = new StandardMaterial3D
                {
                    AlbedoColor = Colors.White,
                    VertexColorUseAsAlbedo = true
                }
            }
        };
        AddChild(new MultiMeshInstance3D { Multimesh = _mesh });
        _projectileMesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = new BoxMesh { Size = new Vector3(0.12f, 0.12f, 0.5f),
                Material = new StandardMaterial3D { AlbedoColor = Colors.White,
                    VertexColorUseAsAlbedo = true } }
        };
        AddChild(new MultiMeshInstance3D { Multimesh = _projectileMesh });
    }

    public void Tick(double seconds, double secondsPerDay)
    {
        if (!Active)
        {
            if (_battles.ActiveSettlementInvasion is { } battle) Begin(battle);
            else return;
        }
        if (seconds <= 0 || _throne() is not { } throne) return;
        _phaseSeconds += seconds;
        UpdateProjectiles(seconds);
        switch (Phase)
        {
            case SettlementInvasionPhase.Warning:
                if (_phaseSeconds >= secondsPerDay / 8.0 && HasBombardmentTarget())
                    ChangePhase(SettlementInvasionPhase.Bombardment);
                else if (_phaseSeconds >= secondsPerDay / 2.0)
                    ChangePhase(SettlementInvasionPhase.Deploying);
                break;
            case SettlementInvasionPhase.Bombardment:
                if (!BombardPath())
                {
                    BuildPath(throne);
                    ChangePhase(SettlementInvasionPhase.Deploying);
                }
                break;
            case SettlementInvasionPhase.Deploying:
                _deploymentAccumulator += seconds;
                while (_deploymentAccumulator >= 1 && _cohorts.Any(value => !value.Deployed))
                {
                    _deploymentAccumulator -= 1;
                    _cohorts.First(value => !value.Deployed).Deployed = true;
                }
                if (_cohorts.All(value => value.Deployed)) ChangePhase(SettlementInvasionPhase.Fighting);
                break;
            case SettlementInvasionPhase.Fighting:
                AdvanceCohorts(seconds, throne);
                ResolveTacticalCombat(seconds);
                if (EnemyMen <= 0) Finish(true);
                else if (_cohorts.Any(value => value.Deployed && value.PathIndex >= _path.Count - 1))
                    Finish(false);
                else if (_phaseSeconds >= secondsPerDay * 5.0) Finish(true);
                break;
            case SettlementInvasionPhase.AwaitingPrisoners:
                break;
        }
        SyncRender();
    }

    public int ApplyEnemyLosses(int requested)
    {
        var remaining = Math.Max(0, requested);
        var applied = 0;
        foreach (var cohort in _cohorts.Where(value => value.Deployed && value.Men > 0)
                     .OrderBy(value => value.Men))
        {
            var loss = Math.Min(cohort.Men, remaining);
            cohort.Men -= loss; remaining -= loss; applied += loss;
            if (remaining == 0) break;
        }
        _enemyDeaths += applied;
        SyncRender();
        return applied;
    }

    public void ReportPlayerLosses(int losses) => _playerLosses += Math.Max(0, losses);

    public SettlementInvasionSnapshot? Capture() => !Active ? null : new SettlementInvasionSnapshot(
        _battleId, Phase, _phaseSeconds, _edge, _enemyDeaths, _playerLosses,
        _engagementResolved, _routedEnemies,
        _cohorts.Select(value => new InvasionCohortSnapshot(
            value.Men, value.PathIndex, value.StepProgress, value.Deployed,
            value.Task, value.Formation, value.Routing, value.Casualties, value.Morale)).ToArray(),
        _combatAccumulator);

    public bool Restore(SettlementInvasionSnapshot snapshot)
    {
        if (_battles.ActiveSettlementInvasion?.Id != snapshot.BattleId || _throne() is not { } throne)
            return false;
        _battleId = snapshot.BattleId; Phase = snapshot.Phase;
        _phaseSeconds = Math.Max(0, snapshot.PhaseSeconds); _edge = Math.Clamp(snapshot.Edge, 0, 3);
        _enemyDeaths = Math.Max(0, snapshot.EnemyDeaths); _playerLosses = Math.Max(0, snapshot.PlayerLosses);
        _engagementResolved = snapshot.EngagementResolved;
        _routedEnemies = Math.Max(0, snapshot.RoutedEnemies);
        _combatAccumulator = Math.Max(0, snapshot.CombatAccumulator);
        _cohorts.Clear();
        _cohorts.AddRange(snapshot.Cohorts.Select(value => new Cohort
        {
            Men = Math.Max(0, value.Men), PathIndex = Math.Max(0, value.PathIndex),
            StepProgress = Math.Clamp(value.StepProgress, 0, 1), Deployed = value.Deployed,
            Task = value.Task, Formation = value.Formation, Routing = value.Routing,
            Casualties = Math.Max(0, value.Casualties), Morale = Math.Clamp(value.Morale, 0, 2)
        }));
        BuildPath(throne); SyncRender(); return true;
    }

    private void Begin(WorldBattleSnapshot battle)
    {
        _battleId = battle.Id; _enemyDeaths = 0; _playerLosses = 0;
        _engagementResolved = false; _routedEnemies = 0; _combatAccumulator = 0;
        _cohorts.Clear();
        var men = battle.Attacker.Men;
        while (men > 0 && _cohorts.Count < WorldArmyRuntime.DivisionsPerArmy)
        {
            var amount = Math.Min(WorldArmyRuntime.MenPerDivision, men);
            _cohorts.Add(new Cohort { Men = amount, Task = DivisionBattleTask.Move });
            men -= amount;
        }
        _edge = SelectEdge(battle.Attacker.FactionId);
        ChangePhase(SettlementInvasionPhase.Warning);
        if (_throne() is { } throne) BuildPath(throne);
        SyncRender();
    }

    private int SelectEdge(int attackerFactionId)
    {
        var attacker = _world.Region(_world.Faction(attackerFactionId)?.CapitalRegionId ?? -1);
        var player = _world.Region(_world.Faction(_world.PlayerFactionId)?.CapitalRegionId ?? -1);
        if (attacker is null || player is null) return (int)(_battleId & 3);
        var dx = attacker.CenterTileX - player.CenterTileX;
        var dz = attacker.CenterTileY - player.CenterTileY;
        return Math.Abs(dx) > Math.Abs(dz) ? dx < 0 ? 0 : 1 : dz < 0 ? 2 : 3;
    }

    private GridCoord SpawnCell(int offset = 0)
    {
        var span = Math.Max(8, Math.Min(_cohorts.Sum(value => value.Men) / 10, GridWorld.Width - 2));
        var across = Math.Clamp(GridWorld.Width / 2 - span / 2 + offset % span, 1, GridWorld.Width - 2);
        return _edge switch
        {
            0 => new GridCoord(1, across), 1 => new GridCoord(GridWorld.Width - 2, across),
            2 => new GridCoord(across, 1), _ => new GridCoord(across, GridWorld.Height - 2)
        };
    }

    private bool BuildPath(GridCoord throne)
    {
        var start = _grid.FindNearestWalkable(SpawnCell());
        _path = HierarchicalGridPathfinder.FindPath(start, throne, GridWorld.Width, GridWorld.Height,
            _grid.Data.IsBlocked, _grid.Data.MovementCost);
        return _path.Count > 0;
    }

    private bool HasBombardmentTarget()
    {
        var (dx, dz) = InwardDirection();
        var start = SpawnCell();
        for (var distance = 0; distance < 12; distance++)
            if (_grid.Data.IsBlocked(new GridCoord(
                    start.X + dx * distance, start.Z + dz * distance))) return true;
        return false;
    }

    private bool BombardPath()
    {
        var start = SpawnCell();
        var (dx, dz) = InwardDirection();
        for (var distance = 0; distance < 12; distance++)
        {
            var cell = new GridCoord(start.X + dx * distance, start.Z + dz * distance);
            if (_grid.RemoveWall(cell)) return true;
        }
        return false;
    }

    private (int X, int Z) InwardDirection() => _edge switch
    {
        0 => (1, 0), 1 => (-1, 0), 2 => (0, 1), _ => (0, -1)
    };

    private void AdvanceCohorts(double seconds, GridCoord throne)
    {
        if (_path.Count == 0 && !BuildPath(throne)) return;
        foreach (var cohort in _cohorts.Where(value => value.Deployed && value.Men > 0))
        {
            if (cohort.Task == DivisionBattleTask.Stop || cohort.Routing) continue;
            cohort.StepProgress += seconds * 1.6;
            while (cohort.StepProgress >= 1 && cohort.PathIndex < _path.Count - 1)
            {
                cohort.StepProgress -= 1;
                cohort.PathIndex++;
            }
        }
    }

    private int DistanceToThrone(GridCoord throne)
    {
        return _cohorts.Where(value => value.Deployed && value.Men > 0)
            .Select(value =>
            {
                var cell = _path.Count == 0 ? SpawnCell() :
                    _path[Math.Min(value.PathIndex, _path.Count - 1)];
                return Math.Abs(cell.X - throne.X) + Math.Abs(cell.Z - throne.Z);
            }).DefaultIfEmpty(int.MaxValue).Min();
    }

    /// <summary>Resolver.autoValue applied when the deployed enemy reaches the city divisions.</summary>
    private void ResolveDefenders()
    {
        _engagementResolved = true;
        var defense = _rooms.Military.DefenseStrength(_citizens.PersonalStats);
        if (defense.Men <= 0 || defense.Power <= 0) return;
        var enemyPower = _battles.ActiveSettlementInvasion?.Attacker.Power ?? EnemyMen;
        var total = Math.Max(1, defense.Power + enemyPower);
        var defenderBalance = defense.Power / total;
        var enemyBalance = enemyPower / total;
        var defenderFraction = AutoValue(defenderBalance);
        var enemyFraction = AutoValue(enemyBalance);
        var enemyBefore = EnemyMen;
        var requestedEnemyLosses = (int)Math.Ceiling(enemyBefore * enemyFraction);
        var routed = enemyFraction >= 1.0 && defenderBalance > enemyBalance
            ? Math.Min(requestedEnemyLosses, Math.Max(0, enemyBefore / 4)) : 0;
        var enemyLosses = ApplyEnemyLosses(requestedEnemyLosses);
        var routedApplied = Math.Min(routed, enemyLosses);
        _routedEnemies += routedApplied;
        _enemyDeaths = Math.Max(0, _enemyDeaths - routedApplied);
        var casualtyCount = Math.Min(defense.Men,
            (int)Math.Ceiling(defense.Men * defenderFraction));
        var casualties = defense.Soldiers.OrderBy(value => value).Take(casualtyCount).ToArray();
        _rooms.Military.RemoveBattleCasualties(casualties);
        var killed = _citizens.KillForEvent(casualties, _resources, _jobs, "BATTLE");
        ReportPlayerLosses(killed);
        var survivors = defense.Soldiers.Except(casualties).ToArray();
        for (var i = 0; i < Math.Min(enemyLosses, survivors.Length); i++)
            _citizens.PersonalStats.RecordKill(survivors[i]);
    }

    private void ResolveTacticalCombat(double seconds)
    {
        _combatAccumulator += seconds;
        if (_combatAccumulator < 2.0) return;
        _combatAccumulator %= 2.0;
        ResolveArtillery();
        ResolveBuildingAttacks();
        var divisions = _rooms.Military.Divisions.Where(value => value.Members.Count > 0 && !value.Routing)
            .Select(value => (Division: value, Cell: _citizens.MilitaryCentroid(value.Members)))
            .Where(value => value.Cell is not null).ToArray();
        foreach (var pair in divisions)
        {
            var division = pair.Division; var cell = pair.Cell!.Value;
            var ranged = division.Order.Task == DivisionBattleTask.AttackRanged && division.HasRangedWeapon;
            // BOW.txt: 35 tile/s at 50 degrees and Trajectory.G = 10 tiles/s².
            // The least-skilled ballistic range is therefore about 122 tiles.
            var range = ranged ? 122 : 2;
            var targets = _cohorts.Select((value, index) => (Cohort: value, Index: index,
                    Distance: Manhattan(cell, CohortCell(value))))
                .Where(value => value.Cohort.Deployed && value.Cohort.Men > 0 && !value.Cohort.Routing);
            if (division.Order.TargetDivisionId >= 0)
                targets = targets.Where(value => value.Index == division.Order.TargetDivisionId);
            var target = targets
                .Where(value => value.Distance <= range).OrderBy(value => value.Distance).FirstOrDefault();
            if (target.Cohort is null)
            {
                division.Engaged = false;
                _rooms.Military.TickBattleCondition(division.Id, 2.0, fighting: false,
                    running: division.Order.Task is DivisionBattleTask.Move or DivisionBattleTask.Charge);
                continue;
            }
            _engagementResolved = true;
            var own = _rooms.Military.CombatProfile(division.Id, _citizens.PersonalStats);
            var enemyPower = Math.Max(1.0, target.Cohort.Men * EnemyPowerPerSoldier());
            var threats = _cohorts.Count(value => value.Deployed && value.Men > 0 &&
                Manhattan(cell, CohortCell(value)) <= 2);
            var flankedMen = Math.Max(0, threats - 1) * own.Men / 2.0;
            var flankValue = Math.Clamp((flankedMen / Math.Max(1, own.Men) - 0.3) * 4.0, 0, 1);
            var flankDefense = 0.2 + 0.8 * (1.0 - flankValue);
            var formationDefense = division.Order.Formation == DivisionFormation.Tight ? 1.25 : 1.0;
            var attackPerSoldier = own.Offence + own.BluntAttack +
                own.PierceAttack / 2.0 + own.SlashAttack / 2.0;
            if (ranged) attackPerSoldier = own.RangedPierce * own.RangedAccuracy;
            var defencePerSoldier = own.Defence + own.DirectedDefence * 0.5 +
                own.BluntDefence + own.PierceDefence / 2.0 + own.SlashDefence / 2.0;
            var enemyLoss = Math.Clamp((int)Math.Ceiling(
                own.Men * attackPerSoldier / enemyPower), 1, 4);
            var playerLoss = ranged ? 0 : Math.Clamp((int)Math.Ceiling(
                enemyPower / Math.Max(1, own.Men * defencePerSoldier *
                    flankDefense * formationDefense)), 1, 4);
            if (ranged)
            {
                var arrows = _rooms.Military.ConsumeAmmunition(division.Id, own.Men);
                if (arrows <= 0) continue;
                enemyLoss = Math.Max(1, enemyLoss * arrows / Math.Max(1, own.Men));
                var destination = CohortCell(target.Cohort);
                if (!TraceProjectile(cell, destination, division.Members, out var impact))
                {
                    SpawnProjectile(cell, impact); DamageStructure(impact, 1);
                    continue;
                }
                SpawnProjectile(cell, impact);
            }
            _rooms.Military.TickBattleCondition(division.Id, 2.0, fighting: true,
                running: division.Order.Task == DivisionBattleTask.Charge);
            enemyLoss = ApplyLoss(target.Cohort, enemyLoss);
            var casualties = own.Soldiers.OrderBy(value => value).Take(Math.Min(playerLoss, own.Men)).ToArray();
            _rooms.Military.RemoveBattleCasualties(casualties);
            var killed = _citizens.KillForEvent(casualties, _resources, _jobs, "BATTLE");
            ReportPlayerLosses(killed);
            foreach (var survivor in own.Soldiers.Except(casualties).Take(enemyLoss))
                _citizens.PersonalStats.RecordKill(survivor);
            var men = Math.Max(0, division.Members.Count);
            var armyMen = Math.Max(0, _rooms.Military.Recruits);
            var armyCasualtyPart = _playerLosses / (double)Math.Max(1, _playerLosses + armyMen);
            var divisionCasualties = division.BattleCasualties + killed;
            var divisionPart = 2.0 * divisionCasualties / Math.Max(1, divisionCasualties + men);
            var morale = Math.Clamp(1.0 - 0.5 * armyCasualtyPart - divisionPart, 0, 1);
            var routing = morale <= 0;
            _rooms.Military.UpdateBattleStatus(division.Id, killed, morale, true, routing);
            if (routing)
            {
                var exit = ClosestExit(cell);
                _citizens.IssueMilitaryFormation(division.Members, exit, exit,
                    DivisionFormation.Loose, _jobs, _resources);
            }
        }
        var enemyArmyMorale = _cohorts.Sum(value => value.Men) == 0 ? 0 :
            _cohorts.Sum(value => value.Morale * value.Men) / Math.Max(1, EnemyMen);
        if (enemyArmyMorale < 0.2)
            foreach (var cohort in _cohorts.Where(value => value.Men > 0)) Route(cohort);
    }

    private int ApplyLoss(Cohort cohort, int requested)
    {
        var loss = Math.Min(cohort.Men, Math.Max(0, requested));
        cohort.Men -= loss; cohort.Casualties += loss; _enemyDeaths += loss;
        var armyPart = _enemyDeaths / (double)Math.Max(1, _enemyDeaths + EnemyMen);
        var divisionPart = 2.0 * cohort.Casualties /
                           Math.Max(1, cohort.Casualties + cohort.Men);
        cohort.Morale = Math.Clamp(1.0 - 0.5 * armyPart - divisionPart, 0, 1);
        if (cohort.Men > 0 && cohort.Morale <= 0) Route(cohort);
        return loss;
    }

    private void Route(Cohort cohort)
    {
        if (cohort.Routing || cohort.Men <= 0) return;
        cohort.Routing = true; _routedEnemies += cohort.Men; cohort.Men = 0;
    }

    private void ResolveArtillery()
    {
        foreach (var artillery in _rooms.Military.Artillery.Where(value => value.Mustered &&
                     value.Crew > 0 && value.Loaded && value.TargetCell is not null).ToArray())
        {
            var targetCell = artillery.TargetCell!.Value;
            var distance = Manhattan(artillery.Anchor, targetCell);
            if (distance > 122) continue; // minimum range from the original 35 tile/s projectile.
            var firingSkill = artillery.AverageSkill;
            if (!_rooms.Military.Fire(artillery.RoomId)) continue;
            if (!TraceProjectile(artillery.Anchor, targetCell, Array.Empty<int>(), out var impact))
            {
                SpawnProjectile(artillery.Anchor, impact);
                DamageStructure(impact, 10 + 20 * firingSkill);
                continue;
            }
            SpawnProjectile(artillery.Anchor, targetCell);
            if (artillery.TargetCohortId >= 0 && artillery.TargetCohortId < _cohorts.Count)
            {
                var cohort = _cohorts[artillery.TargetCohortId];
                if (cohort.Men > 0) ApplyLoss(cohort,
                    Math.Max(1, (int)Math.Round(10 * (1 + 2 * firingSkill))));
            }
            else
            {
                DamageStructure(targetCell, 10 + 20 * firingSkill);
                if (artillery.BombardArea)
                    foreach (var offset in GridCoord.Cardinal)
                        DamageStructure(targetCell + offset, 5 + 10 * firingSkill);
            }
        }
    }

    private void ResolveBuildingAttacks()
    {
        foreach (var division in _rooms.Military.Divisions.Where(value =>
                     value.Members.Count > 0 && !value.Routing &&
                     value.Order.Task == DivisionBattleTask.AttackBuilding))
        {
            if (_citizens.MilitaryCentroid(division.Members) is not { } from) continue;
            var target = division.Order.TargetBuilding;
            var ranged = division.HasRangedWeapon;
            if (Manhattan(from, target) > (ranged ? 122 : 2)) continue;
            var own = _rooms.Military.CombatProfile(division.Id, _citizens.PersonalStats);
            if (ranged)
            {
                if (_rooms.Military.ConsumeAmmunition(division.Id, own.Men) <= 0) continue;
                if (!TraceProjectile(from, target, division.Members, out var impact)) target = impact;
                SpawnProjectile(from, target);
            }
            var structureAttack = own.BluntAttack + own.PierceAttack + own.SlashAttack;
            DamageStructure(target, Math.Max(1, structureAttack));
            _rooms.Military.TickBattleCondition(division.Id, 2.0, fighting: true,
                running: division.Order.Task == DivisionBattleTask.Charge);
        }
    }

    private bool TraceProjectile(GridCoord from, GridCoord to, IEnumerable<int> shooters,
        out GridCoord impact)
    {
        var shooterIds = shooters.ToHashSet();
        var line = GridLine(from, to).ToArray();
        for (var i = 1; i < line.Length - 1; i++)
        {
            var cell = line[i];
            var progress = i / (double)Math.Max(1, line.Length - 1);
            var arc = Math.Sin(progress * Math.PI) * 12.0;
            if (_grid.Data.Has(cell, TileFlags.Mountain) ||
                arc < 2 && _grid.Data.Has(cell, TileFlags.Wall | TileFlags.Furniture))
            { impact = cell; return false; }
            if (arc <= 1.5 && _citizens.InspectAt(cell, 0) is { } friendly &&
                !shooterIds.Contains(friendly.Id))
            {
                _rooms.Military.RemoveBattleCasualties(new[] { friendly.Id });
                ReportPlayerLosses(_citizens.KillForEvent(new[] { friendly.Id },
                    _resources, _jobs, "FRIENDLY_FIRE"));
                impact = cell; return false;
            }
        }
        impact = to; return true;
    }

    private static IEnumerable<GridCoord> GridLine(GridCoord from, GridCoord to)
    {
        var x = from.X; var z = from.Z;
        var dx = Math.Abs(to.X - x); var sx = x < to.X ? 1 : -1;
        var dz = -Math.Abs(to.Z - z); var sz = z < to.Z ? 1 : -1;
        var error = dx + dz;
        while (true)
        {
            yield return new GridCoord(x, z); if (x == to.X && z == to.Z) yield break;
            var twice = 2 * error;
            if (twice >= dz) { error += dz; x += sx; }
            if (twice <= dx) { error += dx; z += sz; }
        }
    }

    private void DamageStructure(GridCoord cell, double damage)
    {
        if (!_grid.IsInside(cell) || (!_grid.Data.IsBlocked(cell) && !_rooms.Contains(cell))) return;
        var index = _grid.CellToIndex(cell);
        _structureDamage[index] = _structureDamage.GetValueOrDefault(index) + Math.Max(0, damage);
        if (_rooms.FindAt(cell) is { } room)
            room.Degradation = Math.Clamp(room.Degradation + damage / 100.0, 0, 1);
        if (_structureDamage[index] < 100) return; // Battle.DAMAGE_REDUCTION = 100.
        _grid.RemoveFurniture(cell); _grid.RemoveDoor(cell); _grid.RemoveWall(cell);
        if (_rooms.FindAt(cell) is { } destroyed && destroyed.Degradation >= 1)
            _rooms.Dismantle(destroyed.Id, _jobs, _resources);
        _structureDamage.Remove(index);
    }

    private double EnemyPowerPerSoldier() => Math.Clamp(
        (_battles.ActiveSettlementInvasion?.Attacker.Power ?? EnemyMen) /
        Math.Max(1.0, EnemyMen), 1.0, 5.0);

    private GridCoord CohortCell(Cohort cohort) => _path.Count == 0 ? SpawnCell() :
        _path[Math.Min(cohort.PathIndex, _path.Count - 1)];

    private static int Manhattan(GridCoord a, GridCoord b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Z - b.Z);

    private static GridCoord ClosestExit(GridCoord cell)
    {
        var distances = new[] { cell.X, GridWorld.Width - 1 - cell.X,
            cell.Z, GridWorld.Height - 1 - cell.Z };
        return Array.IndexOf(distances, distances.Min()) switch
        {
            0 => new GridCoord(1, cell.Z), 1 => new GridCoord(GridWorld.Width - 2, cell.Z),
            2 => new GridCoord(cell.X, 1), _ => new GridCoord(cell.X, GridWorld.Height - 2)
        };
    }

    private void SpawnProjectile(GridCoord from, GridCoord to) => _projectiles.Add(new ProjectileTrace
    {
        From = _grid.CellToWorld(from, 1.1f), To = _grid.CellToWorld(to, 0.7f)
    });

    private void UpdateProjectiles(double seconds)
    {
        foreach (var projectile in _projectiles) projectile.Progress += seconds * 2.5;
        _projectiles.RemoveAll(value => value.Progress >= 1);
        _projectileMesh.InstanceCount = _projectiles.Count;
        for (var i = 0; i < _projectiles.Count; i++)
        {
            var projectile = _projectiles[i];
            var position = projectile.From.Lerp(projectile.To, (float)projectile.Progress);
            position.Y += Mathf.Sin((float)projectile.Progress * Mathf.Pi) * 2f;
            _projectileMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, position));
            _projectileMesh.SetInstanceColor(i, new Color("e8c77a"));
        }
    }

    private static double AutoValue(double powerBalance) => powerBalance < 0.5
        ? 1.0 : Math.Clamp(1.0 - powerBalance, 0, 1);

    private void Finish(bool playerVictory)
    {
        if (playerVictory && _routedEnemies > 0)
        {
            ChangePhase(SettlementInvasionPhase.AwaitingPrisoners);
            return;
        }
        Complete(playerVictory);
    }

    public bool ResolvePrisoners(bool accept)
    {
        if (!AwaitingPrisonerDecision) return false;
        if (accept)
        {
            var admitted = 0;
            while (admitted < _routedEnemies && _rooms.Asylums.TryAdmit() is not null) admitted++;
        }
        _routedEnemies = 0;
        Complete(true);
        return true;
    }

    private void Complete(bool playerVictory)
    {
        Phase = playerVictory ? SettlementInvasionPhase.Victory : SettlementInvasionPhase.Defeat;
        if (!playerVictory)
        {
            var stockLoss = 0.25 + 0.05 * (int)(_battleId % 6);
            _rooms.Trade.Credits -= Math.Max(0, (int)(_rooms.Trade.Credits * 0.75));
            foreach (var resource in Enum.GetValues<ResourceKind>())
                _resources.TryTake(resource,
                    Math.Max(0, (int)Math.Ceiling(_resources.Get(resource) * stockLoss)));
        }
        _battles.CompleteSettlementInvasion(playerVictory, _enemyDeaths, _playerLosses);
        Phase = SettlementInvasionPhase.Done;
        _mesh.InstanceCount = 0;
    }

    private void ChangePhase(SettlementInvasionPhase phase)
    {
        Phase = phase; _phaseSeconds = 0; _deploymentAccumulator = 0;
    }

    private void SyncRender()
    {
        var visible = _cohorts.Where(value => value.Deployed && value.Men > 0).ToArray();
        _mesh.InstanceCount = visible.Length;
        for (var i = 0; i < visible.Length; i++)
        {
            var cohort = visible[i];
            var cell = _path.Count == 0 ? SpawnCell(i) : _path[Math.Min(cohort.PathIndex, _path.Count - 1)];
            var position = _grid.CellToWorld(cell, 0.45f);
            var scale = 0.65f + 0.35f * cohort.Men / WorldArmyRuntime.MenPerDivision;
            _mesh.SetInstanceTransform(i, new Transform3D(Basis.Identity.Scaled(new Vector3(scale, 1, scale)), position));
            var cohortIndex = _cohorts.IndexOf(cohort);
            var targeted = _rooms.Military.Divisions.Any(value =>
                value.Order.TargetDivisionId == cohortIndex && value.Order.Task is
                    DivisionBattleTask.AttackMelee or DivisionBattleTask.AttackRanged);
            _mesh.SetInstanceColor(i, targeted ? new Color("e8c04a") : new Color("b92e2e"));
        }
    }
}
