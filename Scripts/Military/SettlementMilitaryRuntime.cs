using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Military;

public enum DivisionFormation : byte { Tight, Loose }
public enum DivisionBattleTask : byte
{
    Stop, Move, AttackBuilding, AttackMelee, AttackRanged, Charge
}

public sealed record DivisionBattleOrder(
    DivisionBattleTask Task, DivisionFormation Formation, GridCoord Destination,
    int TargetDivisionId, GridCoord TargetBuilding, bool OrderedWhenFighting);

public sealed class MilitaryTrainingRoomRuntime
{
    public int RoomId { get; init; }
    public MilitaryRoomRule Rule { get; init; } = null!;
    public GridCoord[] Stations { get; set; } = Array.Empty<GridCoord>();
}

public sealed class ArtilleryRuntime
{
    public int RoomId { get; init; }
    public GridCoord Anchor { get; set; }
    public MilitaryRoomRule Rule { get; init; } = null!;
    public int Crew { get; set; }
    public double LoadProgress { get; set; }
    public bool Loaded { get; set; }
    public double AverageSkill { get; set; }
    public int SkillSamples { get; set; }
    public int Ammunition { get; set; }
    public bool Mustered { get; set; }
    public int TargetCohortId { get; set; } = -1;
    public GridCoord? TargetCell { get; set; }
    public bool BombardArea { get; set; }
    public int CrewMaximum => 6;
}

public sealed class DivisionRuntime
{
    public int Id { get; init; }
    public string Name { get; set; } = "";
    public string Race { get; set; } = "HUMAN";
    public int TargetMen { get; set; }
    public HashSet<int> Members { get; } = new();
    public Dictionary<string, double> TrainingTargets { get; } =
        new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<ResourceKind, int> EquipmentPerSoldier { get; } = new();
    public Dictionary<ResourceKind, int> IssuedEquipment { get; } = new();
    public DivisionBattleOrder Order { get; set; } = new(
        DivisionBattleTask.Stop, DivisionFormation.Tight, default, -1, default, false);
    public int BattleCasualties { get; set; }
    public double Morale { get; set; } = 1;
    public bool Routing { get; set; }
    public bool Engaged { get; set; }
    public int AmmunitionUsed { get; set; }
    public double Exhaustion { get; set; }
    public int AmmunitionMaximum => IssuedEquipment.GetValueOrDefault(ResourceKind.Bow) * 40;
    public int AmmunitionRemaining => Math.Max(0, AmmunitionMaximum - AmmunitionUsed);
    public bool HasRangedWeapon => IssuedEquipment.GetValueOrDefault(ResourceKind.Bow) > 0 &&
                                   AmmunitionRemaining > 0;
    public int FreeSpots => Math.Max(0, SettlementMilitaryRuntime.MenPerDivision - Members.Count);
}

public sealed record SettlementDefenseStrength(int Men, double Power, IReadOnlyList<int> Soldiers);
public sealed record DivisionCombatProfile(
    int Men, double Offence, double Defence, double DirectedDefence,
    double PierceAttack, double SlashAttack, double BluntAttack,
    double PierceDefence, double SlashDefence, double BluntDefence,
    double RangedAccuracy, double RangedPierce, double SpeedMultiplier,
    IReadOnlyList<int> Soldiers);

/// <summary>
/// Shared runtime for ROOM_M_TRAINER, barracks/range recruit plans, local Div/DivInfo/DivMen,
/// artillery manning/loading and military supply consumption.
/// </summary>
public sealed class SettlementMilitaryRuntime
{
    public const int MenPerDivision = 200;
    public const int DivisionsPerArmy = 120;
    public const double BasicTrainingPerDay = 0.1;
    public const double TrainingWorkSeconds = 45;
    public const int ArtilleryCrew = 6;
    public const int SupplyRadius = 300;
    public const int SupplyCrateStorage = 80;
    private readonly Dictionary<int, MilitaryTrainingRoomRuntime> _trainingRooms = new();
    private readonly Dictionary<int, ArtilleryRuntime> _artillery = new();
    private readonly Dictionary<int, DivisionRuntime> _divisions = new();
    private readonly Dictionary<int, int> _citizenDivision = new();
    private readonly Dictionary<(int Citizen, string Training), double> _trainingRemainder = new();
    private int _nextDivisionId = 1;

    public IReadOnlyCollection<MilitaryTrainingRoomRuntime> TrainingRooms => _trainingRooms.Values;
    public IReadOnlyCollection<ArtilleryRuntime> Artillery => _artillery.Values;
    public IReadOnlyCollection<DivisionRuntime> Divisions => _divisions.Values;
    public int Recruits => _citizenDivision.Count;

    public bool IssueOrder(int divisionId, DivisionBattleTask task, GridCoord destination = default,
        int targetDivisionId = -1, GridCoord targetBuilding = default, bool fighting = false)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return false;
        division.Order = division.Order with
        {
            Task = task, Destination = destination, TargetDivisionId = targetDivisionId,
            TargetBuilding = targetBuilding, OrderedWhenFighting = fighting
        };
        return true;
    }

    public bool SetFormation(int divisionId, DivisionFormation formation)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return false;
        division.Order = division.Order with { Formation = formation };
        return true;
    }

    public SettlementDefenseStrength DivisionStrength(int divisionId, CitizenPersonalStatsRuntime stats)
    {
        if (!_divisions.TryGetValue(divisionId, out var division) || division.Members.Count == 0)
            return new SettlementDefenseStrength(0, 0, Array.Empty<int>());
        var soldiers = division.Members.ToArray();
        var profiles = soldiers.Select(stats.Ensure).ToArray();
        var basic = profiles.Average(value => value.BasicTraining /
            (double)CitizenPersonalStatsRuntime.TrainingMaximum);
        var specialized = profiles.Average(value => value.MilitaryTraining.Count == 0 ? 0 :
            value.MilitaryTraining.Values.Max() / (double)CitizenPersonalStatsRuntime.TrainingMaximum);
        var experience = profiles.Average(value => value.CombatExperience / (double)byte.MaxValue);
        var required = division.EquipmentPerSoldier.Sum(value => value.Value) * soldiers.Length;
        var equipment = required <= 0 ? 0 : Math.Clamp(
            division.IssuedEquipment.Sum(value => value.Value) / (double)required, 0, 1);
        var quality = Math.Clamp(0.40 * basic + 0.25 * specialized +
                                 0.15 * experience + 0.20 * equipment, 0, 1);
        return new SettlementDefenseStrength(soldiers.Length,
            soldiers.Length * (1 + quality * 4.0), soldiers);
    }

    public DivisionCombatProfile CombatProfile(int divisionId, CitizenPersonalStatsRuntime stats)
    {
        if (!_divisions.TryGetValue(divisionId, out var division) || division.Members.Count == 0)
            return new DivisionCombatProfile(0, 0, 1, 1, 0, 0, 1, 0, 0, 1,
                0.8, 4, 0.4, Array.Empty<int>());
        var soldiers = division.Members.ToArray();
        var profiles = soldiers.Select(stats.Ensure).ToArray();
        var basic = profiles.Average(value => value.BasicTraining /
            (double)CitizenPersonalStatsRuntime.TrainingMaximum);
        var melee = profiles.Average(value => value.MilitaryTraining.GetValueOrDefault("BARRACKS") /
            (double)CitizenPersonalStatsRuntime.TrainingMaximum);
        var archery = profiles.Average(value => value.MilitaryTraining.GetValueOrDefault("ARCHERY") /
            (double)CitizenPersonalStatsRuntime.TrainingMaximum);
        var experience = profiles.Average(value => value.CombatExperience / (double)byte.MaxValue);
        double Fraction(ResourceKind kind)
        {
            var requested = division.EquipmentPerSoldier.GetValueOrDefault(kind) * soldiers.Length;
            if (requested <= 0) return 0;
            return Math.Clamp(division.IssuedEquipment.GetValueOrDefault(kind) / (double)requested, 0, 1);
        }
        var leather = Fraction(ResourceKind.ArmourLeather);
        var plate = Fraction(ResourceKind.ArmourPlate);
        var hammer = Fraction(ResourceKind.WeaponHammer);
        var shield = Fraction(ResourceKind.WeaponShield);
        var sword = Fraction(ResourceKind.WeaponShort);
        var slash = Fraction(ResourceKind.WeaponSlash);
        var spear = Fraction(ResourceKind.WeaponSpear);
        var bow = Fraction(ResourceKind.Bow);
        var mount = Fraction(ResourceKind.WeaponMount);
        var readiness = Math.Clamp(1.0 - division.Exhaustion * 0.65, 0.25, 1);

        // Original equipment BOOST blocks: ADD scales by issued share; MUL is
        // interpolated from the neutral multiplier to the original full value.
        var offenceSkill = (1 + basic + melee + experience) *
            (1 - 0.2 * plate) * (1 + 0.25 * sword) * (1 + slash) * (1 + 0.25 * mount);
        var defenceSkill = (1 + basic + melee * 0.5 + 5 * spear) * (1 - 0.5 * mount);
        var bluntAttack = 1 + 0.5 * hammer;
        var pierceAttack = hammer * 3 + sword + spear;
        var slashAttack = sword + slash * 6;
        var bluntDefence = (1 + 0.25 * leather) * (1 + 0.5 * plate);
        var directed = bluntDefence * (1 + 0.5 * plate) * (1 + 3 * shield);
        var speed = (1 - 0.05 * leather) * (1 - 0.25 * plate) *
                    (1 - 0.1 * bow) * (1 + 2.5 * mount) * readiness;
        return new DivisionCombatProfile(soldiers.Length, offenceSkill * readiness,
            defenceSkill * readiness, directed * readiness,
            pierceAttack, slashAttack, bluntAttack,
            leather * 1.25 + plate * 2, leather * 3.5 + plate * 8, bluntDefence,
            0.8 + 0.18 * archery, 4 + 8 * archery, speed, soldiers);
    }

    public void TickBattleCondition(int divisionId, double seconds, bool fighting, bool running)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return;
        var change = fighting ? seconds / 600.0 : running ? seconds / 1200.0 : -seconds / 900.0;
        division.Exhaustion = Math.Clamp(division.Exhaustion + change, 0, 1);
    }

    public void UpdateBattleStatus(int divisionId, int casualties, double morale,
        bool engaged, bool routing)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return;
        division.BattleCasualties += Math.Max(0, casualties);
        division.Morale = Math.Clamp(morale, 0, 2);
        division.Engaged = engaged; division.Routing = routing;
        if (routing) division.Order = division.Order with { Task = DivisionBattleTask.Move };
    }

    public int ConsumeAmmunition(int divisionId, int requested)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return 0;
        var used = Math.Min(Math.Max(0, requested), division.AmmunitionRemaining);
        division.AmmunitionUsed += used;
        return used;
    }

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var activeTraining = new HashSet<int>();
        var activeArtillery = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            var rule = OriginalGameData.Current.MilitaryRooms.GetValueOrDefault(room.DefinitionKey);
            if (rule is null) continue;
            var roomRule = OriginalGameData.Current.Room(room.DefinitionKey);
            var stationCount = Math.Max(1, (int)Math.Ceiling(room.ItemGroupAmounts.Sum(pair =>
                pair.Key >= 0 && roomRule is not null && pair.Key < roomRule.FurnisherItems.Count &&
                roomRule.FurnisherItems[pair.Key].Stats.Count > 0
                    ? pair.Value * roomRule.FurnisherItems[pair.Key].Stats[0] : 0)));
            var stations = room.Cells.Take(stationCount).Select(fromIndex).ToArray();
            if (rule.Kind is "BARRACKS" or "ARCHERY")
            {
                activeTraining.Add(room.Id);
                if (!_trainingRooms.TryGetValue(room.Id, out var training))
                    _trainingRooms[room.Id] = training = new MilitaryTrainingRoomRuntime
                        { RoomId = room.Id, Rule = rule };
                training.Stations = stations;
            }
            else if (rule.Kind == "ARTILLERY")
            {
                activeArtillery.Add(room.Id);
                if (!_artillery.TryGetValue(room.Id, out var artillery))
                    _artillery[room.Id] = artillery = new ArtilleryRuntime
                        { RoomId = room.Id, Rule = rule };
                artillery.Anchor = room.Cells.Count == 0 ? default : fromIndex(room.Cells.First());
                artillery.Crew = Math.Min(ArtilleryCrew, room.Employment.Employed);
            }
        }
        foreach (var id in _trainingRooms.Keys.Where(id => !activeTraining.Contains(id)).ToArray())
            _trainingRooms.Remove(id);
        foreach (var id in _artillery.Keys.Where(id => !activeArtillery.Contains(id)).ToArray())
            _artillery.Remove(id);
    }

    public DivisionRuntime? CreateDivision(string race, string? name = null)
    {
        if (_divisions.Count >= DivisionsPerArmy) return null;
        var id = _nextDivisionId++;
        var division = new DivisionRuntime
        {
            Id = id, Race = race, Name = name ?? $"Division #{id}", TargetMen = MenPerDivision
        };
        foreach (var rule in OriginalGameData.Current.MilitaryRooms.Values.Where(value =>
                     value.Kind is "BARRACKS" or "ARCHERY"))
            division.TrainingTargets[rule.Kind] = 1;
        _divisions[id] = division;
        return division;
    }

    public bool Enlist(int citizenId, int divisionId)
    {
        if (!_divisions.TryGetValue(divisionId, out var division) ||
            division.Members.Count >= Math.Min(MenPerDivision, division.TargetMen) ||
            _citizenDivision.ContainsKey(citizenId)) return false;
        division.Members.Add(citizenId);
        _citizenDivision[citizenId] = divisionId;
        return true;
    }

    public bool Discharge(int citizenId)
    {
        if (!_citizenDivision.Remove(citizenId, out var divisionId)) return false;
        if (_divisions.TryGetValue(divisionId, out var division)) division.Members.Remove(citizenId);
        return true;
    }

    public bool RemoveDivision(int divisionId)
    {
        if (!_divisions.Remove(divisionId, out var division)) return false;
        foreach (var citizenId in division.Members.ToArray()) _citizenDivision.Remove(citizenId);
        return true;
    }

    public int DivisionOf(int citizenId) => _citizenDivision.GetValueOrDefault(citizenId);

    public bool ConfigureDivision(int divisionId, string name, int targetMen)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return false;
        division.Name = string.IsNullOrWhiteSpace(name) ? $"Division #{division.Id}" : name.Trim();
        division.TargetMen = Math.Clamp(targetMen, division.Members.Count, MenPerDivision);
        return true;
    }

    public bool ConfigureTraining(int divisionId, string training, double target)
    {
        if (!_divisions.TryGetValue(divisionId, out var division) ||
            !division.TrainingTargets.ContainsKey(training)) return false;
        division.TrainingTargets[training] = Math.Clamp(target, 0, 1);
        return true;
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources)
    {
        foreach (var room in _trainingRooms.Values)
        foreach (var station in room.Stations)
            if (jobs.GetAt(station) is null) jobs.Add(BuildJob.MilitaryTraining(station, room.RoomId));
        foreach (var artillery in _artillery.Values.Where(value => value.Mustered && !value.Loaded &&
                     value.Crew > 0 && value.Ammunition < value.Rule.ProjectileStorage))
            if (!jobs.All.Any(job => job.Kind == BuildKind.ArtilleryLoad &&
                    job.RoomId == artillery.RoomId &&
                    job.State is not (JobState.Cancelled or JobState.Completed)))
                jobs.Add(BuildJob.ArtilleryLoad(artillery.Anchor, artillery.RoomId));
    }

    public void BeginDay(ResourceLedger resources, SettlementLogisticsRuntime logistics)
    {
        foreach (var division in _divisions.Values.Where(value => !value.Engaged))
            division.AmmunitionUsed = 0;
        foreach (var division in _divisions.Values)
        foreach (var target in division.EquipmentPerSoldier)
        {
            var required = target.Value * division.Members.Count;
            var missing = Math.Max(0, required - division.IssuedEquipment.GetValueOrDefault(target.Key));
            if (missing <= 0) continue;
            var issued = logistics.IssueMilitarySupply(target.Key,
                Math.Min(missing, resources.Get(target.Key)));
            if (issued <= 0 || !resources.TryTake(target.Key, issued)) continue;
            division.IssuedEquipment[target.Key] =
                division.IssuedEquipment.GetValueOrDefault(target.Key) + issued;
        }
    }

    public bool ConfigureEquipment(int divisionId, ResourceKind resource, int perSoldier)
    {
        if (!_divisions.TryGetValue(divisionId, out var division)) return false;
        division.EquipmentPerSoldier[resource] = Math.Max(0, perSoldier);
        return true;
    }

    public void CompleteTraining(int citizenId, BuildJob job, CitizenPersonalStatsRuntime stats)
    {
        if (!_trainingRooms.TryGetValue(job.RoomId, out var room) ||
            !_citizenDivision.TryGetValue(citizenId, out var divisionId) ||
            !_divisions.TryGetValue(divisionId, out var division)) return;
        var profile = stats.Ensure(citizenId);
        var workDay = OriginalGameData.Current.SecondsPerHour * 12.0;
        if (profile.BasicTraining < CitizenPersonalStatsRuntime.TrainingMaximum)
        {
            AddTraining(citizenId, "BASIC", CitizenPersonalStatsRuntime.TrainingMaximum *
                BasicTrainingPerDay * TrainingWorkSeconds / workDay, value =>
                    stats.SetBasicTraining(citizenId, profile.BasicTraining + value));
            return;
        }
        var target = division.TrainingTargets.GetValueOrDefault(room.Rule.Kind);
        var current = profile.MilitaryTraining.GetValueOrDefault(room.Rule.Kind);
        if (current / (double)CitizenPersonalStatsRuntime.TrainingMaximum >= target) return;
        AddTraining(citizenId, room.Rule.Kind,
            CitizenPersonalStatsRuntime.TrainingMaximum * TrainingWorkSeconds /
            (workDay * Math.Max(1, room.Rule.FullTrainingDays)), value =>
                stats.SetMilitaryTraining(citizenId, room.Rule.Kind, current + value));
    }

    public bool CompleteArtilleryLoad(
        BuildJob job, ResourceLedger resources, SettlementLogisticsRuntime logistics, double skill)
    {
        if (!_artillery.TryGetValue(job.RoomId, out var artillery) || artillery.Loaded ||
            !OriginalGameData.TryMapResource(artillery.Rule.ProjectileResource, out var resource)) return false;
        if (artillery.Ammunition == 0)
        {
            if (resources.Get(resource) <= 0 || logistics.IssueMilitarySupply(resource, 1) <= 0 ||
                !resources.TryTake(resource, 1)) return false;
            artillery.Ammunition = 1;
        }
        var reload = Math.Max(1, artillery.Rule.ReloadSeconds);
        artillery.LoadProgress += TrainingWorkSeconds / (ArtilleryCrew * reload);
        artillery.AverageSkill = (artillery.AverageSkill * artillery.SkillSamples +
            Math.Clamp(skill, 0, 1)) / ++artillery.SkillSamples;
        if (artillery.LoadProgress < 1) return true;
        artillery.LoadProgress -= 1;
        artillery.Ammunition--;
        artillery.Loaded = true;
        return true;
    }

    public bool Fire(int roomId)
    {
        if (!_artillery.TryGetValue(roomId, out var artillery) || !artillery.Loaded) return false;
        artillery.Loaded = false;
        artillery.SkillSamples = 0;
        artillery.AverageSkill = 0;
        return true;
    }

    public bool SetArtilleryTarget(int roomId, GridCoord cell, int cohortId = -1, bool bombard = false)
    {
        if (!_artillery.TryGetValue(roomId, out var artillery) || !artillery.Mustered ||
            artillery.Crew <= 0) return false;
        artillery.TargetCell = cell; artillery.TargetCohortId = cohortId;
        artillery.BombardArea = bombard; return true;
    }

    public void ClearArtilleryTarget(int roomId)
    {
        if (!_artillery.TryGetValue(roomId, out var artillery)) return;
        artillery.TargetCell = null; artillery.TargetCohortId = -1; artillery.BombardArea = false;
    }

    public bool MusterArtillery(int roomId, bool mustered)
    {
        if (!_artillery.TryGetValue(roomId, out var artillery)) return false;
        artillery.Mustered = mustered;
        return true;
    }

    /// <summary>
    /// Power.get adapter over the combat data currently present in the port. The source
    /// normalizes a division to 1..HIGH_POWER per soldier; training, experience and issued
    /// equipment supply that normalized quality here until the full racial boost graph is ported.
    /// </summary>
    public SettlementDefenseStrength DefenseStrength(CitizenPersonalStatsRuntime stats)
    {
        var soldiers = _divisions.Values.SelectMany(value => value.Members).Distinct().ToArray();
        var power = 0.0;
        foreach (var division in _divisions.Values.Where(value => value.Members.Count > 0))
        {
            power += DivisionStrength(division.Id, stats).Power; // Power.HIGH_POWER = 5.0
        }
        return new SettlementDefenseStrength(soldiers.Length, power, soldiers);
    }

    public void RemoveBattleCasualties(IEnumerable<int> citizenIds)
    {
        foreach (var citizenId in citizenIds.ToArray()) Discharge(citizenId);
    }

    private void AddTraining(int citizen, string key, double amount, Action<int> apply)
    {
        var state = (citizen, key);
        var total = _trainingRemainder.GetValueOrDefault(state) + amount;
        var whole = (int)total;
        _trainingRemainder[state] = total - whole;
        if (whole > 0) apply(whole);
    }
}
