using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Navigation;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Hauling;
using GodotSyxPort.Data;
using GodotSyxPort.Maintenance;
using GodotSyxPort.Bootstrap;
using GodotSyxPort.Military;

namespace GodotSyxPort.Citizens;

public sealed record CitizenState(
    int Id,
    int Cell,
    float Hunger,
    WorkProfession Profession,
    bool Alive,
    byte FoodPlan,
    float EatTimeLeft,
    int FoodSource,
    bool FoodFromLoose,
    CitizenIdentity Identity,
    int AgeDays,
    int BabyDays,
    string Religion,
    int HomeRoomId,
    double NeedTimeLeft,
    IReadOnlyDictionary<string, int> Needs,
    CitizenHealthSnapshot Health);
public readonly record struct WorkAccidentResult(int Injured, int Deaths);
public readonly record struct EventCitizenSnapshot(
    int Id, string Race, SocialClass Class, HumanoidType Type, WorkProfession Profession);
public sealed record CitizenInspectionSnapshot(
    int Id,
    GridCoord Cell,
    string Name,
    string Race,
    SocialClass Class,
    HumanoidType Type,
    WorkProfession Profession,
    int AgeDays,
    float Hunger,
    int HomeRoomId,
    string Religion,
    string Health,
    int Injury,
    string CurrentJob,
    IReadOnlyList<KeyValuePair<string, int>> HighestNeeds);

/// <summary>
/// Dense citizen simulation plus a single MultiMesh renderer. Citizens are data objects,
/// not Godot Nodes, allowing the same system to scale into the thousands.
/// </summary>
public sealed partial class CitizenSystem : Node3D
{
    private readonly record struct ConstructionLoad(BuildJob Job, int Amount);

    private sealed class Agent
    {
        public int Id;
        public GridCoord Cell;
        public Vector3 Position;
        public int PathHandle;
        public bool MilitaryOrder;
        public float MilitarySpeedMultiplier = 1f;
        public BuildJob? Job;
        public float Hunger;
        public bool Carrying;
        public ResourceKind CarriedResource;
        public int CarriedAmount;
        public WorkProfession Profession;
        public bool Alive = true;
        public byte FoodPlan;
        public float EatTimeLeft;
        public GridCoord FoodSource;
        public bool FoodFromLoose;
        public RoomServiceInstanceRuntime? FoodService;
        public GridCoord FoodServiceCell;
        public double NeedTimeLeft;
        public readonly List<ConstructionLoad> ConstructionBatch = new();
        public int ConstructionBatchIndex;
        public double JobTravelSeconds;
        public int ServiceRemaining;
        public double ServiceDayLeft;
        public byte ServicePlan;
        public RoomServiceInstanceRuntime? Service;
        public GridCoord ServiceCell;
        public int ServiceDistance;
        public float ServiceTimeLeft;
        public readonly Dictionary<string, double> ServiceAccess = new();
        // StatsService is keyed by room blueprint in the original game. Keep this alongside
        // the need-level view: several blueprints can satisfy the same need but have separate
        // standing entries (for example SERVICE_WELL_NORMAL and SERVICE_BATH_NORMAL).
        public readonly Dictionary<string, double> ServiceAccessByRoom =
            new(System.StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, double> ServiceQuality = new();
        public readonly Dictionary<string, double> ServiceProximity = new();
        public readonly Dictionary<string, int> Needs = new(System.StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, double> ServiceBoosts = new(System.StringComparer.OrdinalIgnoreCase);
        public int Dirtiness;
        public int Exposure;
        public CitizenHealthRuntime Health = new();
        public int NeedUpdateIndex;
        public string Religion = "";
        public CitizenIdentity Identity = null!;
        public ArrivalCause ArrivalCause;
        public byte EmigrationPlan;
        public int HomeRoomId;
        public bool SubjectActivityPending;
        public byte MourningPlan;
        public int MourningSlotId;
        public int MourningSteps;
        public float MourningTimeLeft;
        public int AgeDays;
        public int BabyDays;
        public int NurseryDays;
        public int SchoolDays;
        public int MissedSchoolDays;
        public int NurseryTimeoutDays;
        public int SchoolTimeoutDays;
        public ChildActivity ChildActivity;
        public int ChildRoomId;
        public int ChildPlanSteps;
        public int FriendId;
        public byte SocialPlan;
        public int SocialTargetId;
        public double SocialTimeLeft;
        public double ChildPlanTimeLeft;
        public HomeActivity HomeActivity;
        public double HomePlanTimeLeft;
        public bool HasSleptToday;
        public int HomeDayMarker = -1;
    }

    private const float MoveSpeed = 3.2f;
    private const int MaxAssignmentsPerFrame = 4;
    private readonly List<Agent> _agents = new();
    private GridWorld _world = null!;
    private MultiMesh _multiMesh = null!;
    private RoomSystem _rooms = null!;
    private HaulingSystem _hauling = null!;
    private RoadMaintenanceSystem _roadMaintenance = null!;
    private CorpseRuntime _corpses = null!;
    private ReligionRuntime _religions = null!;
    private readonly SharedPathPool _pathPool = new();
    private int _bakerLimit = -1;
    private int _carpenterLimit = -1;
    private double _epidemicTimeLeft;
    private double _populationDayLeft;
    private double _reproductionCheckLeft;
    private double _populationElapsed;
    private int _nextCitizenId = 1;
    private int _assignmentsRemainingThisFrame;
    private double _professionRebalanceLeft;
    private readonly Dictionary<ArrivalCause, int> _arrivalCounts = new();
    private readonly Dictionary<LeaveCause, int> _leaveCounts = new();
    private readonly Dictionary<int, EducationTrack> _adultEducationPolicy = new();
    public string CurrentEpidemic { get; private set; } = "";
    public CitizenPersonalStatsRuntime PersonalStats { get; } = new();
    public CitizenIdentityRuntime Identities { get; private set; } = null!;
    public CitizenReproductionRuntime Reproduction { get; private set; } = null!;
    public CitizenSocialRuntime Social { get; } = new();
    public System.Func<EventCitizenSnapshot, bool>? EventWorkSuspended { get; set; }

    private const int NeedChunk = 16;
    private const int FoodSeekThreshold = NeedChunk * 2;
    private const int StarvationThreshold = NeedChunk * 3;
    private const int HungerMaximum = NeedChunk * 4;
    private const int ServicesPerDay = 4;
    private const int MaximumPendingServices = ServicesPerDay * 2;

    public int Count => _agents.Count(agent => agent.Alive);
    public int StarvingCount { get; private set; }
    public int BakerCount => CountProfession(WorkProfession.Baker);
    public int CarpenterCount => CountProfession(WorkProfession.Carpenter);
    public int BakerTarget => ProfessionTarget(WorkProfession.Baker);
    public int CarpenterTarget => ProfessionTarget(WorkProfession.Carpenter);
    public int BakerLimit => _bakerLimit;
    public int CarpenterLimit => _carpenterLimit;
    public int ActivePathCount => _pathPool.ActiveCount;
    public int CitizensSeekingService => _agents.Count(agent => agent.Alive && agent.ServicePlan != 0);
    public int SickCount => _agents.Count(agent => agent.Alive && agent.Health.ActiveDisease);
    public int InjuredCount => _agents.Count(agent => agent.Alive && agent.Health.Injury > 0);
    public int SleepingChildren => _agents.Count(agent => agent.Alive &&
        agent.ChildActivity == ChildActivity.Sleeping);
    public int PlayingChildren => _agents.Count(agent => agent.Alive &&
        agent.ChildActivity == ChildActivity.Playing);
    public int CitizensSleeping => _agents.Count(agent => agent.Alive &&
        agent.HomeActivity is HomeActivity.SleepingAtHome or HomeActivity.SleepingOutside);
    public double AverageHunger => Count == 0 ? 0 : _agents.Where(agent => agent.Alive)
        .Average(agent => agent.Hunger) / HungerMaximum;

    public double AverageServiceAccess(string need) => AverageServiceValue(
        need, agent => agent.ServiceAccess);

    public double AverageRoomServiceAccess(string roomDefinition) => AverageServiceValue(
        roomDefinition, agent => agent.ServiceAccessByRoom);

    public double AverageRoomServiceAccess(
        string roomDefinition, string race, SocialClass socialClass) => AverageServiceValue(
        roomDefinition, agent => agent.ServiceAccessByRoom,
        agent => agent.Identity.Race.Equals(race, System.StringComparison.OrdinalIgnoreCase) &&
                 agent.Identity.Class == socialClass);

    public double StarvingFraction(string race, SocialClass socialClass)
    {
        var group = _agents.Where(agent => agent.Alive &&
            agent.Identity.Race.Equals(race, System.StringComparison.OrdinalIgnoreCase) &&
            agent.Identity.Class == socialClass).ToArray();
        return group.Length == 0 ? 0 : group.Count(agent => agent.Hunger >= StarvationThreshold) /
            (double)group.Length;
    }

    public double AverageServiceQuality(string need) => AverageServiceValue(
        need, agent => agent.ServiceQuality);

    public double AverageServiceProximity(string need) => AverageServiceValue(
        need, agent => agent.ServiceProximity);

    public double AverageServiceBoost(string boostKey) => AverageServiceValue(
        boostKey, agent => agent.ServiceBoosts);

    public double AverageNeed(string need)
    {
        var alive = _agents.Where(agent => agent.Alive).ToArray();
        if (alive.Length == 0) return 0;
        return alive.Sum(agent => agent.Needs.GetValueOrDefault(need)) /
               (alive.Length * (double)HungerMaximum);
    }

    public double AverageSocialRelation => Count == 0 ? 0 : _agents
        .Where(agent => agent.Alive).Average(agent => Social.AverageRelation(agent.Id));

    public int CriticalNeedCount(string need) => _agents.Count(agent =>
        agent.Alive && agent.Needs.GetValueOrDefault(need) >= StarvationThreshold);

    public IReadOnlyDictionary<string, int> ReligionFollowers() => _agents
        .Where(agent => agent.Alive)
        .GroupBy(agent => agent.Religion, System.StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Count(), System.StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> PopulationByRace() => _agents
        .Where(agent => agent.Alive)
        .GroupBy(agent => agent.Identity.Race, System.StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Count(), System.StringComparer.OrdinalIgnoreCase);

    public void ForEachMiniMapCitizen(System.Action<GridCoord, string> visitor)
    {
        foreach (var agent in _agents)
            if (agent.Alive) visitor(agent.Cell, agent.Identity.Race);
    }

    public void ForEachOverlayCitizen(System.Action<GridCoord, bool> visitor)
    {
        foreach (var agent in _agents)
            if (agent.Alive) visitor(agent.Cell, agent.HomeRoomId == 0);
    }

    public void ForEachUnemployedCitizen(System.Action<GridCoord> visitor)
    {
        foreach (var agent in _agents)
            if (agent.Alive && HumanoidTypeRules.Works(agent.Identity.Type) &&
                agent.Profession == WorkProfession.Laborer && agent.Job is null)
                visitor(agent.Cell);
    }

    public IReadOnlyDictionary<SocialClass, int> PopulationByClass() => _agents
        .Where(agent => agent.Alive).GroupBy(agent => agent.Identity.Class)
        .ToDictionary(group => group.Key, group => group.Count());

    public IReadOnlyDictionary<(string Race, SocialClass Class), int> PopulationByRaceAndClass() => _agents
        .Where(agent => agent.Alive)
        .GroupBy(agent => (agent.Identity.Race, agent.Identity.Class))
        .ToDictionary(group => group.Key, group => group.Count());

    public double HousingAccess(string race, SocialClass socialClass)
    {
        var group = _agents.Where(agent => agent.Alive &&
            agent.Identity.Race.Equals(race, System.StringComparison.OrdinalIgnoreCase) &&
            agent.Identity.Class == socialClass).ToArray();
        return group.Length == 0 ? 1 : group.Count(agent => _rooms.Housing.Home(agent.Id) is not null) /
            (double)group.Length;
    }

    public double AverageHomeFurniture(string race, SocialClass socialClass)
    {
        var group = _agents.Where(agent => agent.Alive &&
            agent.Identity.Race.Equals(race, System.StringComparison.OrdinalIgnoreCase) &&
            agent.Identity.Class == socialClass).ToArray();
        return group.Length == 0 ? 0 : group.Average(agent => _rooms.Housing.FurnitureFulfillment(agent.Id));
    }

    public CitizenIdentity? Identity(int citizenId) => Identities?.Get(citizenId);
    public IReadOnlyDictionary<ArrivalCause, int> ArrivalCounts => _arrivalCounts;
    public IReadOnlyDictionary<LeaveCause, int> LeaveCounts => _leaveCounts;

    public IReadOnlyList<EventCitizenSnapshot> EventPopulation() => _agents
        .Where(agent => agent.Alive)
        .Select(agent => new EventCitizenSnapshot(agent.Id, agent.Identity.Race,
            agent.Identity.Class, agent.Identity.Type, agent.Profession)).ToArray();

    public CitizenInspectionSnapshot? InspectAt(GridCoord cell, int radius = 1)
    {
        var agent = _agents.Where(candidate => candidate.Alive)
            .Select(candidate => (Agent: candidate, Distance:
                System.Math.Abs(candidate.Cell.X - cell.X) + System.Math.Abs(candidate.Cell.Z - cell.Z)))
            .Where(candidate => candidate.Distance <= System.Math.Max(0, radius))
            .OrderBy(candidate => candidate.Distance).ThenBy(candidate => candidate.Agent.Id)
            .Select(candidate => candidate.Agent).FirstOrDefault();
        return agent is null ? null : Inspection(agent);
    }

    public CitizenInspectionSnapshot? InspectCitizen(int citizenId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        return agent is null ? null : Inspection(agent);
    }

    public IReadOnlyList<CitizenInspectionSnapshot> InspectCitizens() => _agents
        .Where(agent => agent.Alive).OrderBy(agent => agent.Id).Select(Inspection).ToArray();

    public int IssueMilitaryFormation(IEnumerable<int> citizenIds, GridCoord start, GridCoord end,
        DivisionFormation formation, JobBoard jobs, ResourceLedger resources,
        float speedMultiplier = 1f)
    {
        var members = citizenIds.Select(id => _agents.FirstOrDefault(value => value.Id == id && value.Alive))
            .Where(value => value is not null).Cast<Agent>().ToArray();
        if (members.Length == 0) return 0;
        var dx = end.X - start.X; var dz = end.Z - start.Z;
        var length = Math.Max(1, Math.Max(Math.Abs(dx), Math.Abs(dz)) + 1);
        var stepX = Math.Sign(dx); var stepZ = Math.Sign(dz);
        var rowX = stepZ == 0 ? 0 : -stepZ; var rowZ = stepX == 0 ? 1 : stepX;
        var spacing = formation == DivisionFormation.Loose ? 2 : 1;
        var issued = 0;
        for (var i = 0; i < members.Length; i++)
        {
            var agent = members[i];
            CancelForMilitary(agent, jobs, resources);
            var column = i % length; var row = i / length;
            var target = new GridCoord(start.X + column * stepX + row * rowX * spacing,
                start.Z + column * stepZ + row * rowZ * spacing);
            target = _world.FindNearestWalkable(target);
            if (!SetPathTo(agent, target)) continue;
            agent.MilitaryOrder = true;
            agent.MilitarySpeedMultiplier = Math.Clamp(speedMultiplier, 0.1f, 4f);
            issued++;
        }
        return issued;
    }

    public int StopMilitaryOrders(IEnumerable<int> citizenIds)
    {
        var stopped = 0;
        foreach (var id in citizenIds.Distinct())
        {
            var agent = _agents.FirstOrDefault(value => value.Id == id && value.Alive);
            if (agent is null || !agent.MilitaryOrder) continue;
            agent.MilitaryOrder = false;
            agent.MilitarySpeedMultiplier = 1f;
            ClearPath(agent); stopped++;
        }
        return stopped;
    }

    public GridCoord? MilitaryCentroid(IEnumerable<int> citizenIds)
    {
        var members = citizenIds.Select(id => _agents.FirstOrDefault(value => value.Id == id && value.Alive))
            .Where(value => value is not null).Cast<Agent>().ToArray();
        return members.Length == 0 ? null : new GridCoord(
            (int)Math.Round(members.Average(value => value.Cell.X)),
            (int)Math.Round(members.Average(value => value.Cell.Z)));
    }

    private void CancelForMilitary(Agent agent, JobBoard jobs, ResourceLedger resources)
    {
        if (agent.Job is not null)
        {
            if (agent.Job.IsHaulJob) _hauling.CancelStorageReservation(agent.Job);
            if (agent.Carrying) DropCarried(agent, jobs);
            ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
            jobs.Release(agent.Job, resources); agent.Job = null;
        }
        ClearPath(agent);
    }

    private static CitizenInspectionSnapshot Inspection(Agent agent)
    {
        var health = agent.Health.ActiveDisease ? $"Болезнь: {agent.Health.DiseaseKey}" :
            agent.Health.Critical ? "Критическое состояние" :
            agent.Health.InDanger ? "Ранен" : "Здоров";
        return new CitizenInspectionSnapshot(
            agent.Id, agent.Cell, agent.Identity.FullName, agent.Identity.Race,
            agent.Identity.Class, agent.Identity.Type, agent.Profession,
            agent.AgeDays, agent.Hunger, agent.HomeRoomId, agent.Religion,
            health, agent.Health.Injury, agent.Job?.Kind.ToString() ?? "Нет",
            agent.Needs.OrderByDescending(pair => pair.Value).Take(4).ToArray());
    }

    public int KillForEvent(IEnumerable<int> citizenIds, ResourceLedger resources, JobBoard jobs,
        string cause = "EVENT")
    {
        var ids = citizenIds.ToHashSet();
        var killed = 0;
        foreach (var agent in _agents.Where(agent => agent.Alive && ids.Contains(agent.Id)).ToArray())
        {
            KillFromStarvation(agent, resources, jobs, cause);
            killed++;
        }
        return killed;
    }

    public int BeginEventEmigration(IEnumerable<int> citizenIds, GridCoord destination)
    {
        var moved = 0;
        foreach (var id in citizenIds.Distinct())
            if (TryBeginEmigration(id, destination)) moved++;
        return moved;
    }

    public double ReligiousOpposition()
    {
        var followers = ReligionFollowers();
        return _religions is null ? 0 : _religions.SettlementOpposition(followers);
    }

    public double ReligionServiceAccess(string religion, string need) =>
        AverageReligionServiceValue(religion, need, agent => agent.ServiceAccess);

    public double ReligionServiceQuality(string religion, string need) =>
        AverageReligionServiceValue(religion, need, agent => agent.ServiceQuality);

    public bool StartEpidemic(string diseaseKey, double? spread = null)
    {
        var disease = OriginalGameData.Current.Diseases.GetValueOrDefault(diseaseKey);
        if (disease is null || !disease.Epidemic) return false;
        var alive = _agents.Where(agent => agent.Alive).ToArray();
        if (alive.Length == 0) return false;
        var averageHealth = alive.Average(HealthMultiplier);
        if (averageHealth <= 0) averageHealth = 1;
        var infected = alive.Where(agent => GD.Randf() <
            (spread ?? disease.Spread) * HealthMultiplier(agent) / averageHealth).ToArray();
        if (infected.Length <= 1) return false;
        foreach (var agent in infected) agent.Health.Infect(disease, true);
        infected[(int)(GD.Randi() % (uint)infected.Length)].Health.Infect(disease, false);
        CurrentEpidemic = disease.Key;
        _epidemicTimeLeft = OriginalGameData.Current.SecondsPerDay *
                            (disease.IncubationDays + disease.InfectionDays);
        return true;
    }

    public bool InjureRandom(int amount)
    {
        var candidates = _agents.Where(agent => agent.Alive).ToArray();
        if (candidates.Length == 0 || amount <= 0) return false;
        candidates[(int)(GD.Randi() % (uint)candidates.Length)].Health.AddInjury(amount);
        return true;
    }

    public bool InfectRandom(string diseaseKey, bool incubating = false)
    {
        var disease = OriginalGameData.Current.Diseases.GetValueOrDefault(diseaseKey);
        var candidates = _agents.Where(agent => agent.Alive).ToArray();
        if (disease is null || candidates.Length == 0) return false;
        candidates[(int)(GD.Randi() % (uint)candidates.Length)].Health.Infect(disease, incubating);
        return true;
    }

    public WorkAccidentResult CreateWorkAccident(
        int roomId,
        GridCoord center,
        ResourceLedger resources,
        JobBoard jobs)
    {
        const double radius = 20.0;
        var affected = _agents.Where(agent => agent.Alive &&
            _rooms.FindAt(agent.Cell)?.Id == roomId)
            .Select(agent => (Agent: agent, Distance: System.Math.Sqrt(
                System.Math.Pow(agent.Cell.X - center.X, 2) +
                System.Math.Pow(agent.Cell.Z - center.Z, 2))))
            .Where(item => item.Distance <= radius).ToArray();
        if (affected.Length == 0) return default;
        var injured = 0;
        var deaths = 0;
        var origin = affected.OrderBy(item => item.Distance).First().Agent;
        ApplyAccidentDamage(origin, 1.0, resources, jobs, ref injured, ref deaths);
        foreach (var item in affected)
        {
            if (ReferenceEquals(item.Agent, origin) || !item.Agent.Alive) continue;
            var falloff = 1.0 - item.Distance / radius;
            ApplyAccidentDamage(item.Agent, falloff * GD.Randf() * 2.0,
                resources, jobs, ref injured, ref deaths);
        }
        return new WorkAccidentResult(injured, deaths);
    }

    private void ApplyAccidentDamage(
        Agent agent,
        double damage,
        ResourceLedger resources,
        JobBoard jobs,
        ref int injured,
        ref int deaths)
    {
        if (damage <= 0) return;
        var immediateDeath = damage * GD.Randf() > 1;
        var exact = damage * CitizenHealthRuntime.InjuryMaximum;
        var amount = (int)exact + (GD.Randf() < exact - (int)exact ? 1 : 0);
        if (immediateDeath || agent.Health.Injury + amount >= CitizenHealthRuntime.InjuryMaximum)
        {
            agent.Health.AddInjury(CitizenHealthRuntime.InjuryMaximum);
            KillFromStarvation(agent, resources, jobs, "ACCIDENT");
            deaths++;
            return;
        }
        agent.Health.AddInjury(amount);
        if (agent.Health.InDanger) injured++;
    }

    public void AdjustProfessionLimit(WorkProfession profession, int delta)
    {
        if (profession == WorkProfession.Baker)
            _bakerLimit = System.Math.Clamp(
                _bakerLimit < 0 ? BakerTarget + delta : _bakerLimit + delta, 0, Count);
        else if (profession == WorkProfession.Carpenter)
            _carpenterLimit = System.Math.Clamp(
                _carpenterLimit < 0 ? CarpenterTarget + delta : _carpenterLimit + delta, 0, Count);
    }

    public void RestoreProfessionLimits(int bakerLimit, int carpenterLimit)
    {
        _bakerLimit = bakerLimit < 0 ? -1 : System.Math.Min(bakerLimit, Count);
        _carpenterLimit = carpenterLimit < 0 ? -1 : System.Math.Min(carpenterLimit, Count);
    }

    public void Initialize(
        GridWorld world,
        RoomSystem rooms,
        HaulingSystem hauling,
        RoadMaintenanceSystem roadMaintenance,
        CorpseRuntime corpses,
        int count,
        GridCoord firstCell,
        string raceKey = "HUMAN")
    {
        _world = world;
        _rooms = rooms;
        _hauling = hauling;
        _roadMaintenance = roadMaintenance;
        _corpses = corpses;
        _religions = new ReligionRuntime(OriginalGameData.Current.Religions);
        Identities = new CitizenIdentityRuntime(OriginalGameData.Current.Races);
        Reproduction = new CitizenReproductionRuntime(OriginalGameData.Current.Races);
        _populationDayLeft = OriginalGameData.Current.SecondsPerDay;
        _reproductionCheckLeft = OriginalGameData.Current.SecondsPerDay *
                                 CitizenReproductionRuntime.SourceYearDays /
                                 CitizenReproductionRuntime.ChecksPerYear;
        var occupiedSpawnCells = new HashSet<GridCoord>();
        for (var i = 0; i < count; i++)
        {
            var requestedCell = new GridCoord(firstCell.X + i % 32, firstCell.Z + i / 32);
            var cell = FindWalkableSpawnCell(requestedCell, occupiedSpawnCells);
            occupiedSpawnCells.Add(cell);
            var agent = new Agent
            {
                Id = i + 1,
                Cell = cell,
                Position = world.CellToWorld(cell, 0.45f),
                Hunger = (i * 17) % (NeedChunk * 2),
                NeedTimeLeft = GD.Randf() * NeedsInterval,
                ServiceDayLeft = GD.Randf() * OriginalGameData.Current.SecondsPerDay,
                ServiceRemaining = (int)(GD.Randi() % ServicesPerDay)
            };
            agent.Identity = Identities.Create(
                agent.Id, raceKey, SocialClass.Citizen, HumanoidType.Subject, CitizenOrigin.Immigrant);
            agent.ArrivalCause = ArrivalCause.Immigrated;
            agent.AgeDays = Reproduction.AdultAgeDays(raceKey) + i % (CitizenReproductionRuntime.SourceYearDays * 20);
            _arrivalCounts[ArrivalCause.Immigrated] =
                _arrivalCounts.GetValueOrDefault(ArrivalCause.Immigrated) + 1;
            agent.Religion = _religions.ChooseAffiliation();
            var personal = PersonalStats.Ensure(agent.Id);
            var race = OriginalGameData.Current.Races[agent.Identity.Race];
            var wildcardTrait = race.TraitChances.GetValueOrDefault("*");
            foreach (var trait in race.TraitChances.Where(pair => pair.Key != "*"))
                if (GD.Randf() < System.Math.Max(wildcardTrait, trait.Value))
                    personal.Traits[trait.Key] = CitizenPersonalStatsRuntime.TraitMaximum;
            foreach (var boost in _religions.Boosts(agent.Religion))
                agent.ServiceBoosts[boost.Key] = boost.Value;
            foreach (var need in OriginalGameData.Current.NeedRates.Keys.Where(key => key != "HUNGER"))
                agent.Needs[need] = (int)(GD.Randf() * NeedChunk * 2);
            _agents.Add(agent);
        }
        _nextCitizenId = count + 1;

        _multiMesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            InstanceCount = count,
            Mesh = new CapsuleMesh
            {
                Radius = 0.25f,
                Height = 0.9f,
                Material = new StandardMaterial3D
                {
                    AlbedoColor = Colors.White,
                    VertexColorUseAsAlbedo = true
                }
            }
        };
        AddChild(new MultiMeshInstance3D { Multimesh = _multiMesh });
        SyncRenderTransforms();
    }

    private GridCoord FindWalkableSpawnCell(GridCoord requested, ISet<GridCoord> occupied)
        => _world.FindNearestWalkable(requested, occupied);

    public CitizenIdentity SpawnCitizen(
        GridCoord cell, string raceKey, SocialClass socialClass, HumanoidType type,
        CitizenOrigin origin, ArrivalCause cause, int parentId = 0, int birthDay = 0,
        int ageDays = -1)
    {
        var id = _nextCitizenId++;
        var identity = Identities.Create(id, raceKey, socialClass, type, origin, parentId, birthDay);
        var agent = new Agent
        {
            Id = id, Identity = identity, ArrivalCause = cause, Cell = cell,
            Position = _world.CellToWorld(cell, 0.45f),
            Hunger = GD.Randf() * NeedChunk * 2,
            NeedTimeLeft = GD.Randf() * NeedsInterval,
            ServiceDayLeft = GD.Randf() * OriginalGameData.Current.SecondsPerDay,
            ServiceRemaining = (int)(GD.Randi() % ServicesPerDay),
            Religion = _religions.ChooseAffiliation()
        };
        agent.AgeDays = ageDays >= 0
            ? ageDays
            : type is HumanoidType.Child or HumanoidType.ChildSlave
                ? OriginalGameData.Current.Races[raceKey].BabyDays
                : Reproduction.AdultAgeDays(raceKey);
        PersonalStats.Ensure(id);
        foreach (var boost in _religions.Boosts(agent.Religion)) agent.ServiceBoosts[boost.Key] = boost.Value;
        foreach (var need in OriginalGameData.Current.NeedRates.Keys.Where(key => key != "HUNGER"))
            agent.Needs[need] = (int)(GD.Randf() * NeedChunk * 2);
        _agents.Add(agent);
        _arrivalCounts[cause] = _arrivalCounts.GetValueOrDefault(cause) + 1;
        _multiMesh.InstanceCount = _agents.Count;
        SyncRenderTransforms();
        return identity;
    }

    public bool TryBeginEmigration(int citizenId, GridCoord destination)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Id == citizenId);
        if (agent is null || !agent.Alive || agent.Identity.Class != SocialClass.Citizen ||
            agent.Job is not null || agent.FoodPlan != 0 || agent.ServicePlan != 0) return false;
        if (!SetPathTo(agent, destination) && agent.Cell != destination) return false;
        ClearMourning(agent);
        agent.EmigrationPlan = 1;
        agent.Profession = WorkProfession.Laborer;
        _rooms.Housing.Vacate(agent.Id);
        agent.HomeRoomId = 0;
        foreach (var child in _agents.Where(candidate => candidate.Alive &&
                     candidate.Identity.ParentId == citizenId &&
                     candidate.Identity.Type is HumanoidType.Child or HumanoidType.ChildSlave).ToArray())
            TryBeginEmigration(child.Id, destination);
        return true;
    }

    public bool TryBeginParenthood(int citizenId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Id == citizenId);
        if (agent is null || !agent.Alive ||
            agent.Identity.Type is not (HumanoidType.Subject or HumanoidType.Slave) ||
            !Reproduction.Propagates(agent.Identity.Race, agent.Identity.Class) ||
            Reproduction.NewInfantsAllowed(agent.Identity.Race, agent.Identity.Class,
                PopulationByRace().GetValueOrDefault(agent.Identity.Race)) <= 0) return false;
        var parent = agent.Identity.Class == SocialClass.Slave
            ? HumanoidType.ParentSlave : HumanoidType.Parent;
        Identities.ChangeType(agent.Id, agent.Identity.Class, parent);
        agent.Identity = Identities.Get(agent.Id)!;
        agent.BabyDays = 0;
        return true;
    }

    public bool AppointNoble(int citizenId, string officeKey = "")
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type != HumanoidType.Subject ||
            !_rooms.Governance.Nobility.Appoint(citizenId, officeKey)) return false;
        Identities.ChangeType(citizenId, SocialClass.Noble, HumanoidType.Nobility);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Laborer;
        _rooms.Housing.Vacate(citizenId);
        return true;
    }

    public bool DismissNoble(int citizenId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || !_rooms.Governance.Nobility.Dismiss(citizenId)) return false;
        Identities.ChangeType(citizenId, SocialClass.Citizen, HumanoidType.Subject);
        agent.Identity = Identities.Get(citizenId)!;
        return true;
    }

    public bool PromoteNoble(int citizenId) => _rooms.Governance.Nobility.Promote(citizenId);

    public bool TryCheckInInn(int citizenId, int innRoomId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type != HumanoidType.Tourist ||
            !_rooms.Hospitality.TryReserve(innRoomId, citizenId)) return false;
        return true;
    }

    public bool TryRetireCitizen(int citizenId, int resthomeRoomId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type is not (HumanoidType.Subject or HumanoidType.Slave) ||
            !_rooms.Hospitality.TryReserve(resthomeRoomId, citizenId)) return false;
        Identities.ChangeType(citizenId, agent.Identity.Class, HumanoidType.Retiree);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Laborer;
        return true;
    }

    /// <summary>
    /// Moves an existing subject into an asylum cell. The room owns its reservation;
    /// the resident becomes DERANGED and is excluded from ordinary employment.
    /// Recovery is deliberately explicit because the damaged source AI module contains
    /// no recover-rate logic beyond ROOM_ASYLUM's treatment factor.
    /// </summary>
    public bool TryAdmitAsylum(int citizenId, int? preferredRoomId = null)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type is HumanoidType.Deranged or HumanoidType.Prisoner ||
            _rooms.Asylums.TryAdmitCitizen(citizenId, preferredRoomId) is null) return false;
        Identities.ChangeType(citizenId, agent.Identity.Class, HumanoidType.Deranged);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Laborer;
        _rooms.Housing.Vacate(citizenId);
        agent.HomeRoomId = 0;
        return true;
    }

    public bool TryReleaseAsylum(int citizenId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type != HumanoidType.Deranged ||
            !_rooms.Asylums.Release(citizenId)) return false;
        var ordinary = agent.Identity.Class == SocialClass.Slave
            ? HumanoidType.Slave : HumanoidType.Subject;
        Identities.ChangeType(citizenId, agent.Identity.Class, ordinary);
        agent.Identity = Identities.Get(citizenId)!;
        return true;
    }

    public double AsylumTreatmentFactor(int citizenId)
    {
        var detention = _rooms.Asylums.Find(citizenId);
        return detention is null ? 0 : _rooms.Asylums.TreatmentFactor(detention.RoomId);
    }

    public int CreateDivision(string race, string? name = null) =>
        _rooms.Military.CreateDivision(race, name)?.Id ?? 0;

    public bool EnlistCitizen(int citizenId, int divisionId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type != HumanoidType.Subject ||
            !_rooms.Military.Enlist(citizenId, divisionId)) return false;
        Identities.ChangeType(citizenId, SocialClass.Citizen, HumanoidType.Recruit);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Recruit;
        _rooms.Housing.Vacate(citizenId);
        return true;
    }

    public bool DischargeCitizen(int citizenId)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || !_rooms.Military.Discharge(citizenId)) return false;
        Identities.ChangeType(citizenId, SocialClass.Citizen, HumanoidType.Subject);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Laborer;
        return true;
    }

    public void BeginFrame() => _assignmentsRemainingThisFrame = MaxAssignmentsPerFrame;

    public void Tick(double delta, JobBoard jobs, ResourceLedger resources)
    {
        _populationElapsed += delta;
        _populationDayLeft -= delta;
        while (_populationDayLeft <= 0)
        {
            _populationDayLeft += OriginalGameData.Current.SecondsPerDay;
            AdvancePopulationDay(resources, jobs);
        }
        _reproductionCheckLeft -= delta;
        while (_reproductionCheckLeft <= 0)
        {
            _reproductionCheckLeft += OriginalGameData.Current.SecondsPerDay *
                                      CitizenReproductionRuntime.SourceYearDays /
                                      CitizenReproductionRuntime.ChecksPerYear;
            CheckNaturalReproduction();
        }
        _rooms.Childcare.Tick(delta, OriginalGameData.Current.SecondsPerDay, resources,
            breeder => Reproduction.NewInfantsAllowed(
                breeder.Race, SocialClass.Citizen, PopulationByRace().GetValueOrDefault(breeder.Race)) > 0,
            breeder => SpawnCitizen(breeder.SpawnCell, breeder.Race, SocialClass.Citizen,
                HumanoidType.Child, CitizenOrigin.Native, ArrivalCause.Born));
        if (_epidemicTimeLeft > 0 && (_epidemicTimeLeft -= delta) <= 0)
            CurrentEpidemic = "";
        _professionRebalanceLeft -= delta;
        if (_professionRebalanceLeft <= 0)
        {
            _professionRebalanceLeft = 1.0;
            RebalanceProfessions();
        }
        foreach (var agent in _agents)
        {
            if (!agent.Alive) continue;
            if (agent.EmigrationPlan != 0)
            {
                if (MoveAlongPath(agent, (float)delta)) continue;
                agent.EmigrationPlan = 0;
                agent.Alive = false;
                ReleaseHome(agent, resources);
                _leaveCounts[LeaveCause.Emigrated] = _leaveCounts.GetValueOrDefault(LeaveCause.Emigrated) + 1;
                continue;
            }
            _rooms.Housing.RegisterResident(agent.Id, agent.Identity.Race, agent.Identity.Class);
            if (agent.HomeRoomId != 0 && _rooms.Housing.Home(agent.Id) is null)
                agent.HomeRoomId = 0;
            if (agent.HomeRoomId == 0)
            {
                agent.HomeRoomId = _rooms.Housing.TryOccupy(agent.Id, agent.Cell,
                    agent.Identity.Class == SocialClass.Noble)?.RoomId ?? 0;
            }
            agent.NeedTimeLeft -= delta;
            while (agent.Alive && agent.NeedTimeLeft <= 0)
            {
                agent.NeedTimeLeft += NeedsInterval;
                UpdateHunger(agent, resources, jobs);
                if (!agent.Alive) break;
                if (!agent.Health.ActiveDisease) UpdateNeeds(agent);
                agent.NeedUpdateIndex = (agent.NeedUpdateIndex + 1) % 16;
                if (!agent.Health.Update16(
                        OriginalGameData.Current,
                        agent.NeedUpdateIndex == 0,
                        HealthMultiplier(agent)))
                    KillFromStarvation(agent, resources, jobs);
            }
            if (!agent.Alive) continue;
            UpdateServiceDay(agent, delta);
            // AIModules updates every module and lets a higher priority plan
            // interrupt ordinary work. Health (7) therefore cannot wait for a
            // worker's current job to finish.
            if ((agent.Health.ActiveDisease || agent.Health.InDanger) &&
                agent.ServicePlan == 0 && agent.Job is not null)
                ReleaseJobForHigherPriority(agent, jobs, resources);
            if (agent.FoodPlan != 0 && TickFoodPlan(agent, (float)delta, jobs, resources)) continue;
            if (agent.ServicePlan != 0 && TickServicePlan(agent, (float)delta, resources)) continue;
            if (agent.MourningPlan != 0 && TickMourningPlan(agent, (float)delta)) continue;
            if (ChildBehaviorRuntime.IsChild(agent.Identity.Type))
            {
                TickChildBehavior(agent, (float)delta, resources);
                continue;
            }
            if (agent.HomeActivity != HomeActivity.None && TickHomeBehavior(agent, (float)delta)) continue;
            var workSuspended = EventWorkSuspended?.Invoke(new EventCitizenSnapshot(
                agent.Id, agent.Identity.Race, agent.Identity.Class,
                agent.Identity.Type, agent.Profession)) == true;
            if (agent.Job is null && TryStartHighestPriorityPlan(
                    agent, (float)delta, jobs, resources, workSuspended)) continue;
            if (TickSocialInteraction(agent, (float)delta)) continue;
            if (TickFoodPlan(agent, (float)delta, jobs, resources)) continue;
            TickAgent(agent, (float)delta, resources, jobs);
        }
        StarvingCount = _agents.Count(agent => agent.Alive && agent.Hunger >= StarvationThreshold);
    }

    private bool TryStartHighestPriorityPlan(
        Agent agent,
        float delta,
        JobBoard jobs,
        ResourceLedger resources,
        bool workSuspended)
    {
        if (agent.Carrying) return false;
        var day = (int)(_populationElapsed / OriginalGameData.Current.SecondsPerDay);
        var order = CitizenAiModuleRuntime.Order(
            agent.Id,
            day,
            CitizenAiModuleRuntime.FoodPriority((int)agent.Hunger, NeedChunk),
            agent.Health.ActiveDisease || agent.Health.InDanger,
            HomeModulePriority(agent, day),
            !workSuspended && HumanoidTypeRules.Works(agent.Identity.Type) &&
                _assignmentsRemainingThisFrame > 0,
            agent.ServiceRemaining > 0,
            agent.SubjectActivityPending);
        foreach (var module in order)
        {
            switch (module)
            {
                case CitizenAiModule.Food:
                    if (TickFoodPlan(agent, delta, jobs, resources)) return true;
                    break;
                case CitizenAiModule.Health:
                    if (TryStartRecovery(agent)) return true;
                    break;
                case CitizenAiModule.Home:
                    if (TryStartHomePlan(agent, false)) return true;
                    break;
                case CitizenAiModule.Work:
                    if (TryStartWorkPlan(agent, jobs, resources)) return true;
                    break;
                case CitizenAiModule.Service:
                    if (TryStartService(agent)) return true;
                    break;
                case CitizenAiModule.Subject:
                    if (TryStartMourning(agent) || TryStartSocialInteraction(agent)) return true;
                    break;
                case CitizenAiModule.Idle:
                    return TryStartSocialInteraction(agent);
            }
        }
        return false;
    }

    private int HomeModulePriority(Agent agent, int day)
    {
        var home = _rooms.Housing.Home(agent.Id);
        var race = OriginalGameData.Current.Races[agent.Identity.Race];
        var dayPart = (_populationElapsed / OriginalGameData.Current.SecondsPerDay) % 1.0;
        if (!HomeBehaviorRuntime.ShouldVisitHome(race.Sleeps, home is not null,
                agent.HasSleptToday, agent.AgeDays, agent.Id, day, dayPart)) return 0;
        // AIModule_Home returns 7 after a failed/no-home search and 1 for the
        // ordinary staggered visit. An owned home uses the normal priority.
        return home is null && !agent.HasSleptToday
            ? CitizenAiModuleRuntime.HealthPriority
            : CitizenAiModuleRuntime.HomePriority;
    }

    private bool TryStartRecovery(Agent agent)
    {
        if (agent.Health.RequiresHospital(OriginalGameData.Current) && TryStartHospital(agent))
            return true;
        return TryStartHomePlan(agent, true);
    }

    private bool TryStartWorkPlan(Agent agent, JobBoard jobs, ResourceLedger resources)
    {
        BuildJob? job = null;
        if (agent.Profession != WorkProfession.Laborer)
            job = jobs.TryClaim(agent.Cell, resources,
                candidate => candidate.RequiredProfession == agent.Profession);
        job ??= jobs.TryClaim(agent.Cell, resources,
            candidate => candidate.RequiredProfession == WorkProfession.Laborer);
        if (job is null) return false;
        _assignmentsRemainingThisFrame--;
        if (Assign(agent, job, jobs, resources)) return true;
        jobs.Release(job, resources);
        return false;
    }

    private void ReleaseJobForHigherPriority(
        Agent agent, JobBoard jobs, ResourceLedger resources)
    {
        if (agent.Job is null) return;
        if (agent.Carrying) DropCarried(agent, jobs);
        jobs.Release(agent.Job, resources);
        ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
        agent.Job = null;
        ClearPath(agent);
    }

    private void AdvancePopulationDay(ResourceLedger resources, JobBoard jobs)
    {
        _rooms.Schools.BeginDay();
        _rooms.BeginKnowledgeDay(resources);
        _rooms.Law.TickDay();
        var population = Count;
        var policeCoverage = _rooms.Law.PoliceCoverage(population);
        var lawDay = (int)(_populationElapsed / OriginalGameData.Current.SecondsPerDay);
        var births = new List<(GridCoord Cell, string Race, SocialClass Class, int Parent, int Age)>();
        Social.BeginDay(_populationElapsed / OriginalGameData.Current.SecondsPerDay,
            _agents.Where(agent => agent.Alive).Select(agent => agent.Id).ToHashSet());
        foreach (var agent in _agents.Where(agent => agent.Alive))
        {
            if ((agent.Identity.Class is SocialClass.Citizen or SocialClass.Slave) &&
                (agent.Identity.Type is HumanoidType.Subject or HumanoidType.Slave or
                    HumanoidType.Parent or HumanoidType.ParentSlave))
            {
                var lawfulness = 1.0 + policeCoverage * 10.0;
                if (_rooms.Law.TryCommitDailyCrime(agent.Id, agent.Identity.Race,
                        agent.Identity.Class, lawfulness, population,
                        _rooms.Law.GuardWorkers, lawDay))
                {
                    ApplyCrime(agent, resources);
                    var detected = _rooms.Law.GuardPowerAt(agent.Cell) > 0 ||
                                   HashChance(agent.Id, lawDay, policeCoverage);
                    if (detected && _rooms.Law.TryCatch(
                            agent.Id, agent.Cell, policeCoverage > 0))
                    {
                        Identities.ChangeType(agent.Id, agent.Identity.Class, HumanoidType.Prisoner);
                        agent.Identity = Identities.Get(agent.Id)!;
                        agent.Profession = WorkProfession.Laborer;
                        _rooms.Housing.Vacate(agent.Id);
                    }
                }
            }
            if (agent.Identity.Type == HumanoidType.Student)
            {
                var track = _adultEducationPolicy.GetValueOrDefault(
                    agent.Id, EducationTrack.Education);
                if (!_rooms.Knowledge.TryStudyAdult(agent.Id, PersonalStats, track))
                {
                    var ordinaryType = agent.Identity.Class == SocialClass.Slave
                        ? HumanoidType.Slave : HumanoidType.Subject;
                    Identities.ChangeType(agent.Id, agent.Identity.Class, ordinaryType);
                    agent.Identity = Identities.Get(agent.Id)!;
                    _adultEducationPolicy.Remove(agent.Id);
                }
            }
            _rooms.Housing.TickDay(agent.Id, HomeFurnitureBoost(agent));
            if (agent.Identity.Type is HumanoidType.Parent or HumanoidType.ParentSlave)
            {
                agent.BabyDays++;
                var race = OriginalGameData.Current.Races[agent.Identity.Race];
                if (agent.BabyDays < race.BabyDays) continue;
                births.Add((agent.Cell, agent.Identity.Race, agent.Identity.Class,
                    agent.Id, race.BabyDays));
                var adult = agent.Identity.Class == SocialClass.Slave
                    ? HumanoidType.Slave : HumanoidType.Subject;
                Identities.ChangeType(agent.Id, agent.Identity.Class, adult);
                agent.Identity = Identities.Get(agent.Id)!;
                agent.BabyDays = 0;
                continue;
            }
            agent.AgeDays++;
            if (agent.NurseryTimeoutDays > 0) agent.NurseryTimeoutDays--;
            if (agent.SchoolTimeoutDays > 0) agent.SchoolTimeoutDays--;
            if (agent.Identity.Type is HumanoidType.Child or HumanoidType.ChildSlave &&
                agent.AgeDays >= Reproduction.AdultAgeDays(agent.Identity.Race) &&
                _rooms.Schools.CanEducate(PersonalStats, agent.Id,
                    agent.Identity.Race, agent.Identity.Class))
            {
                if (_rooms.Schools.HasService &&
                    agent.MissedSchoolDays <= SchoolRuntime.MissingSchoolDaysBeforeGrowth) continue;
            }
            var expected = Reproduction.ChildTypeForAge(
                agent.Identity.Race, agent.Identity.Class, agent.AgeDays);
            if (agent.Identity.Type != expected &&
                agent.Identity.Type is HumanoidType.Child or HumanoidType.ChildSlave)
            {
                Identities.ChangeType(agent.Id, agent.Identity.Class, expected);
                agent.Identity = Identities.Get(agent.Id)!;
            }
        }
        foreach (var birth in births)
            SpawnCitizen(birth.Cell, birth.Race, birth.Class,
                birth.Class == SocialClass.Slave ? HumanoidType.ChildSlave : HumanoidType.Child,
                CitizenOrigin.Native, ArrivalCause.Born, birth.Parent, ageDays: birth.Age);
        ProcessLawEffects(resources, jobs);
    }

    public ReproductionSnapshot ReproductionStatus(string raceKey, SocialClass socialClass) =>
        Reproduction.Snapshot(_agents.Where(agent => agent.Alive).Select(agent =>
            (agent.Identity.Race, agent.Identity.Class, agent.Identity.Type, agent.AgeDays)),
            raceKey, socialClass);

    public bool TryEnrollUniversity(int citizenId, EducationTrack track)
    {
        var agent = _agents.FirstOrDefault(candidate => candidate.Alive && candidate.Id == citizenId);
        if (agent is null || agent.Identity.Type is not (HumanoidType.Subject or HumanoidType.Slave) ||
            !_rooms.Knowledge.CanStudyAdult(citizenId, PersonalStats, track)) return false;
        _adultEducationPolicy[citizenId] = track;
        Identities.ChangeType(citizenId, agent.Identity.Class, HumanoidType.Student);
        agent.Identity = Identities.Get(citizenId)!;
        agent.Profession = WorkProfession.Laborer;
        return true;
    }

    private void CheckNaturalReproduction()
    {
        var citizenPopulation = _agents.Count(agent => agent.Alive &&
            agent.Identity.Class == SocialClass.Citizen);
        foreach (var raceGroup in _agents.Where(agent => agent.Alive &&
                     agent.Identity.Type is HumanoidType.Subject or HumanoidType.Slave &&
                     Reproduction.IsFertile(agent.Identity.Race, agent.AgeDays))
                 .GroupBy(agent => (agent.Identity.Race, agent.Identity.Class)))
        {
            var eligible = raceGroup.ToArray();
            var fertileGroup = _agents.Count(agent => agent.Alive &&
                agent.Identity.Race.Equals(raceGroup.Key.Race, System.StringComparison.OrdinalIgnoreCase) &&
                agent.Identity.Class == raceGroup.Key.Class &&
                Reproduction.IsFertile(agent.Identity.Race, agent.AgeDays));
            var chance = Reproduction.ChancePerCheck(
                raceGroup.Key.Race, citizenPopulation) * (fertileGroup + 1.0) / (eligible.Length + 1.0);
            foreach (var agent in eligible)
            {
                if (Reproduction.NewInfantsAllowed(agent.Identity.Race, agent.Identity.Class,
                        PopulationByRace().GetValueOrDefault(agent.Identity.Race)) <= 0) break;
                if (GD.Randf() <= chance) TryBeginParenthood(agent.Id);
            }
        }
    }

    private void TickChildBehavior(Agent agent, float delta, ResourceLedger resources)
    {
        var dayPart = (_populationElapsed / OriginalGameData.Current.SecondsPerDay) % 1.0;
        var race = OriginalGameData.Current.Races[agent.Identity.Race];

        // A reserved child plan owns its service until its own resumer completes or cancels it.
        // Do not replace it merely because the selection predicate changes at the work-day edge.
        if (agent.ChildActivity == ChildActivity.School && agent.ChildRoomId != 0)
        {
            TickChildSchool(agent, delta, dayPart, resources);
            return;
        }
        if (agent.ChildActivity == ChildActivity.Nursery && agent.ChildRoomId != 0)
        {
            TickChildNursery(agent, delta, dayPart);
            return;
        }
        var activity = ChildBehaviorRuntime.Select(
            race, agent.AgeDays, dayPart,
            agent.Hunger >= FoodSeekThreshold, agent.Exposure > 0,
            agent.EmigrationPlan != 0,
            agent.SchoolTimeoutDays == 0 && _rooms.Schools.HasService && _rooms.Schools.CanEducate(
                PersonalStats, agent.Id, agent.Identity.Race, agent.Identity.Class),
            agent.NurseryTimeoutDays == 0 && _rooms.Childcare.NurseryCapacity > 0);
        if (activity != agent.ChildActivity)
        {
            CancelChildReservation(agent);
            agent.ChildActivity = activity;
            agent.ChildPlanTimeLeft = 0;
            agent.ChildPlanSteps = 0;
        }
        if (activity == ChildActivity.School)
        {
            if (!TryStartChildSchool(agent))
            {
                agent.SchoolTimeoutDays = 2;
                agent.MissedSchoolDays++;
                agent.ChildActivity = ChildActivity.Playing;
            }
            return;
        }
        if (activity == ChildActivity.Nursery)
        {
            if (!TryStartChildNursery(agent))
            {
                agent.NurseryTimeoutDays = 2;
                agent.ChildActivity = ChildActivity.Playing;
            }
            return;
        }
        if (activity == ChildActivity.Sleeping)
        {
            if (agent.PathHandle == 0)
            {
                var parentHome = _rooms.Housing.Home(agent.Identity.ParentId);
                if (parentHome is not null && agent.Cell != parentHome.ServiceCell)
                    SetPathTo(agent, parentHome.ServiceCell);
            }
            MoveAlongPath(agent, delta);
            return;
        }
        if (activity != ChildActivity.Playing) return;
        if (MoveAlongPath(agent, delta)) return;
        agent.ChildPlanTimeLeft -= delta;
        if (agent.ChildPlanTimeLeft > 0) return;
        agent.ChildPlanTimeLeft = 5.0 + GD.Randf() * 35.0;
        var target = SelectChildPlaymate(agent);
        if (target is not null && target.Cell != agent.Cell) SetPathTo(agent, target.Cell);
    }

    private bool TryStartChildSchool(Agent agent)
    {
        if (!_rooms.Schools.TryReserveLesson(PersonalStats, agent.Id, agent.Identity.Race,
                agent.Identity.Class, out var roomId)) return false;
        var destination = ChildRoomDestination(roomId, agent.Cell);
        if (destination is null || !SetPathTo(agent, destination.Value))
        {
            _rooms.Schools.CancelReservedLesson(roomId);
            return false;
        }
        agent.ChildRoomId = roomId;
        agent.ChildPlanTimeLeft = 5.0;
        return true;
    }

    private bool TryStartChildNursery(Agent agent)
    {
        if (!_rooms.Childcare.TryReserveNursery(out var roomId)) return false;
        var destination = ChildRoomDestination(roomId, agent.Cell);
        if (destination is null || !SetPathTo(agent, destination.Value))
        {
            _rooms.Childcare.ReleaseNursery(roomId);
            return false;
        }
        agent.ChildRoomId = roomId;
        agent.ChildPlanTimeLeft = 5.0;
        return true;
    }

    private void TickChildSchool(Agent agent, float delta, double dayPart, ResourceLedger resources)
    {
        if (MoveAlongPath(agent, delta)) return;
        agent.ChildPlanTimeLeft -= delta;
        if (agent.ChildPlanTimeLeft > 0) return;
        agent.ChildPlanTimeLeft += 5.0;
        agent.ChildPlanSteps++;
        if (agent.ChildPlanSteps <= 5 || ChildBehaviorRuntime.IsChildWorkTime(dayPart)) return;
        if (_rooms.Schools.CompleteReservedLesson(agent.ChildRoomId, PersonalStats, agent.Id,
                agent.Identity.Race, agent.Identity.Class, resources))
        {
            agent.SchoolDays++;
            agent.MissedSchoolDays = 0;
        }
        // CompleteReservedLesson consumes the reserved lesson on success and restores it on
        // resource failure, so clearing the fields must not cancel it a second time.
        agent.ChildRoomId = 0;
        agent.ChildActivity = ChildActivity.None;
        agent.ChildPlanSteps = 0;
    }

    private void TickChildNursery(Agent agent, float delta, double dayPart)
    {
        if (MoveAlongPath(agent, delta)) return;
        if (!ChildBehaviorRuntime.IsChildWorkTime(dayPart))
        {
            _rooms.Childcare.ReleaseNursery(agent.ChildRoomId);
            agent.ChildRoomId = 0;
            agent.ChildActivity = ChildActivity.None;
            agent.NurseryDays++;
            return;
        }
        agent.ChildPlanTimeLeft -= delta;
        if (agent.ChildPlanTimeLeft > 0) return;
        agent.ChildPlanTimeLeft += 5.0;
        agent.ChildPlanSteps++;
        if (agent.ChildPlanSteps * 5 >= ChildcareRuntime.NurseryPlaySeconds)
            agent.ChildPlanSteps = 0;
    }

    private GridCoord? ChildRoomDestination(int roomId, GridCoord origin)
    {
        var room = _rooms.All.FirstOrDefault(candidate => candidate.Id == roomId);
        if (room is null) return null;
        return room.Cells.Select(_world.FromIndex)
            .SelectMany(cell => GridCoord.Cardinal.Select(offset => cell + offset))
            .Where(cell => _world.IsInside(cell) && !_world.Data.IsBlocked(cell))
            .OrderBy(cell => Math.Abs(cell.X - origin.X) + Math.Abs(cell.Z - origin.Z))
            .Select(cell => (GridCoord?)cell).FirstOrDefault();
    }

    private void CancelChildReservation(Agent agent)
    {
        ClearPath(agent);
        if (agent.ChildRoomId == 0) return;
        if (agent.ChildActivity == ChildActivity.School)
            _rooms.Schools.CancelReservedLesson(agent.ChildRoomId);
        else if (agent.ChildActivity == ChildActivity.Nursery)
            _rooms.Childcare.ReleaseNursery(agent.ChildRoomId);
        agent.ChildRoomId = 0;
    }

    private Agent? SelectChildPlaymate(Agent child)
    {
        var parent = _agents.FirstOrDefault(agent => agent.Alive &&
            agent.Id == child.Identity.ParentId);
        if (parent is not null && (child.FriendId == 0 || GD.Randi() % 4 != 0)) return parent;
        var preference = OriginalGameData.Current.Races[child.Identity.Race].OtherRacePreferences;
        var friend = _agents.Where(agent => agent.Alive && agent.Id != child.Id &&
                     ChildBehaviorRuntime.IsChild(agent.Identity.Type) &&
                     System.Math.Abs(agent.Cell.X - child.Cell.X) +
                     System.Math.Abs(agent.Cell.Z - child.Cell.Z) <= ChildBehaviorRuntime.PlaySearchDistance)
            .OrderByDescending(agent => preference.GetValueOrDefault(agent.Identity.Race, 1.0))
            .ThenBy(agent => System.Math.Abs(agent.Cell.X - child.Cell.X) +
                             System.Math.Abs(agent.Cell.Z - child.Cell.Z))
            .FirstOrDefault();
        if (friend is not null) child.FriendId = friend.Id;
        return friend ?? parent;
    }

    private bool TryStartSocialInteraction(Agent agent)
    {
        if (agent.SocialPlan != 0 || agent.Job is not null ||
            agent.Identity.Type is HumanoidType.Prisoner or HumanoidType.Recruit or
                HumanoidType.Student or HumanoidType.Parent or HumanoidType.ParentSlave ||
            GD.Randi() % 12 != 0) return false;
        var nearby = _agents.Where(candidate => candidate.Alive && candidate.Id != agent.Id &&
                     candidate.SocialPlan == 0 && candidate.Job is null &&
                     !ChildBehaviorRuntime.IsChild(candidate.Identity.Type) &&
                     System.Math.Abs(candidate.Cell.X - agent.Cell.X) +
                     System.Math.Abs(candidate.Cell.Z - agent.Cell.Z) <= CitizenSocialRuntime.SearchDistance)
        .ToArray();

        if (nearby.Length == 0) return false;
        var friendId = Social.BestFriend(agent.Id, nearby.Select(candidate => candidate.Id));
        var preference = OriginalGameData.Current.Races[agent.Identity.Race].OtherRacePreferences;
        var target = nearby.FirstOrDefault(candidate => candidate.Id == friendId) ?? nearby
            .OrderByDescending(candidate => preference.GetValueOrDefault(candidate.Identity.Race, 1.0))
            .ThenBy(candidate => System.Math.Abs(candidate.Cell.X - agent.Cell.X) +
                                 System.Math.Abs(candidate.Cell.Z - agent.Cell.Z)).First();
        agent.FriendId = target.Id;
        agent.SocialTargetId = target.Id;
        agent.SocialPlan = 1;
        if (target.Cell != agent.Cell && !SetPathTo(agent, target.Cell))
        {
            ClearSocialInteraction(agent);
            return false;
        }
        return true;
    }

    private bool TickSocialInteraction(Agent agent, float delta)
    {
        if (agent.SocialPlan == 0) return false;
        var target = _agents.FirstOrDefault(candidate => candidate.Alive &&
            candidate.Id == agent.SocialTargetId);
        if (target is null || agent.Job is not null || agent.FoodPlan != 0 || agent.ServicePlan != 0)
        {
            ClearSocialInteraction(agent);
            return false;
        }
        if (agent.SocialPlan == 1)
        {
            if (MoveAlongPath(agent, delta)) return true;
            var distance = System.Math.Abs(target.Cell.X - agent.Cell.X) +
                           System.Math.Abs(target.Cell.Z - agent.Cell.Z);
            if (distance > 2)
            {
                ClearSocialInteraction(agent);
                return false;
            }
            agent.SocialPlan = 2;
            agent.SocialTimeLeft = CitizenSocialRuntime.InteractionMinimumSeconds + GD.Randf() *
                (CitizenSocialRuntime.InteractionMaximumSeconds -
                 CitizenSocialRuntime.InteractionMinimumSeconds);
        }
        agent.SocialTimeLeft -= delta;
        if (agent.SocialTimeLeft > 0) return true;
        var preference = OriginalGameData.Current.Races[agent.Identity.Race]
            .OtherRacePreferences.GetValueOrDefault(target.Identity.Race, 1.0);
        Social.Meet(agent.Id, target.Id,
            _populationElapsed / OriginalGameData.Current.SecondsPerDay, preference);
        agent.Needs["SOCIAL"] = System.Math.Max(0,
            agent.Needs.GetValueOrDefault("SOCIAL") - NeedChunk);
        agent.ServiceAccess["SOCIAL"] = 1;
        ClearSocialInteraction(agent);
        return false;
    }

    private void ClearSocialInteraction(Agent agent)
    {
        agent.SocialPlan = 0;
        agent.SocialTargetId = 0;
        agent.SocialTimeLeft = 0;
        ClearPath(agent);
    }

    private static double NeedsInterval => OriginalGameData.Current.SecondsPerDay / 16.0;

    private bool TickHomeBehavior(Agent agent, float delta)
    {
        var day = (int)(_populationElapsed / OriginalGameData.Current.SecondsPerDay);
        if (agent.HomeDayMarker != day)
        {
            agent.HomeDayMarker = day;
            agent.HasSleptToday = false;
        }
        if (agent.HomeActivity == HomeActivity.WalkingHome)
        {
            if (MoveAlongPath(agent, delta)) return true;
            agent.HomeActivity = HomeActivity.SleepingAtHome;
            agent.HomePlanTimeLeft = HomeBehaviorRuntime.StaySeconds(
                agent.Id, day, agent.Identity.Class == SocialClass.Noble);
        }
        if (agent.HomeActivity is HomeActivity.SleepingAtHome or HomeActivity.SleepingOutside or
            HomeActivity.StayingHome)
        {
            agent.HomePlanTimeLeft -= delta;
            if (agent.HomePlanTimeLeft > 0) return true;
            agent.HasSleptToday = true;
            agent.HomeActivity = HomeActivity.None;
            return false;
        }
        return TryStartHomePlan(agent, false);
    }

    private bool TryStartHomePlan(Agent agent, bool forceRecovery)
    {
        var day = (int)(_populationElapsed / OriginalGameData.Current.SecondsPerDay);
        var race = OriginalGameData.Current.Races[agent.Identity.Race];
        var home = _rooms.Housing.Home(agent.Id);
        var dayPart = (_populationElapsed / OriginalGameData.Current.SecondsPerDay) % 1.0;
        if (!forceRecovery && !HomeBehaviorRuntime.ShouldVisitHome(race.Sleeps, home is not null,
                agent.HasSleptToday, agent.AgeDays, agent.Id, day, dayPart)) return false;
        agent.HomePlanTimeLeft = HomeBehaviorRuntime.StaySeconds(
            agent.Id, day, agent.Identity.Class == SocialClass.Noble);
        if (home is null)
        {
            agent.HomeActivity = race.Sleeps ? HomeActivity.SleepingOutside : HomeActivity.StayingHome;
            return true;
        }
        if (agent.Cell == home.ServiceCell)
        {
            agent.HomeActivity = race.Sleeps ? HomeActivity.SleepingAtHome : HomeActivity.StayingHome;
            return true;
        }
        if (SetPathTo(agent, home.ServiceCell))
        {
            agent.HomeActivity = HomeActivity.WalkingHome;
            return true;
        }
        _rooms.Housing.Vacate(agent.Id);
        agent.HomeRoomId = 0;
        agent.HomeActivity = HomeActivity.None;
        return false;
    }

    private void UpdateServiceDay(Agent agent, double delta)
    {
        agent.ServiceDayLeft -= delta;
        if (agent.ServiceDayLeft > 0) return;
        agent.ServiceDayLeft += OriginalGameData.Current.SecondsPerDay;
        agent.ServiceRemaining = System.Math.Min(
            MaximumPendingServices,
            agent.ServiceRemaining + (int)(GD.Randi() % ServicesPerDay));
        if (GD.Randi() % (uint)(4 + Count / 1000) == 0) agent.SubjectActivityPending = true;
    }

    private bool TryStartMourning(Agent agent)
    {
        if (!agent.SubjectActivityPending || agent.Job is not null || agent.Carrying) return false;
        var destination = _rooms.Burials.ReserveMourning(agent.Cell, 500);
        if (destination is null)
        {
            agent.SubjectActivityPending = false;
            return false;
        }
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell, destination.Value.Cell, GridWorld.Width, GridWorld.Height,
            _world.Data.IsBlocked, _world.Data.MovementCost);
        if (agent.Cell != destination.Value.Cell && path.Count == 0)
        {
            _rooms.Burials.CancelMourning(destination.Value.SlotId);
            agent.SubjectActivityPending = false;
            return false;
        }
        agent.MourningSlotId = destination.Value.SlotId;
        agent.MourningPlan = 1;
        agent.SubjectActivityPending = false;
        SetPath(agent, path);
        return true;
    }

    private bool TickMourningPlan(Agent agent, float delta)
    {
        if (agent.MourningPlan == 1)
        {
            if (MoveAlongPath(agent, delta)) return true;
            agent.MourningPlan = 2;
            agent.MourningSteps = 4 + (int)(GD.Randi() % 8);
            agent.MourningTimeLeft = 1f;
            return true;
        }
        agent.MourningTimeLeft -= delta;
        if (agent.MourningTimeLeft > 0) return true;
        if (--agent.MourningSteps > 0)
        {
            agent.MourningTimeLeft = 1f;
            return true;
        }
        _rooms.Burials.CompleteMourning(agent.MourningSlotId);
        agent.MourningPlan = 0;
        agent.MourningSlotId = 0;
        return true;
    }

    private void ClearMourning(Agent agent)
    {
        if (agent.MourningSlotId != 0) _rooms.Burials.CancelMourning(agent.MourningSlotId);
        agent.MourningSlotId = 0;
        agent.MourningPlan = 0;
    }

    private void UpdateHunger(Agent agent, ResourceLedger resources, JobBoard jobs)
    {
        if (GD.Randf() < GameSession.TitleBonuses.Apply("RATES_HUNGER", OriginalGameData.Current.HungerRate))
            agent.Hunger = Mathf.Min(HungerMaximum, agent.Hunger + 1f);
        if (agent.Hunger >= HungerMaximum) KillFromStarvation(agent, resources, jobs);
    }

    private static void UpdateNeeds(Agent agent)
    {
        foreach (var need in OriginalGameData.Current.NeedRates)
        {
            if (need.Key.Equals("HUNGER", System.StringComparison.OrdinalIgnoreCase)) continue;
            if (GD.Randf() < need.Value)
                agent.Needs[need.Key] = System.Math.Min(
                    HungerMaximum, agent.Needs.GetValueOrDefault(need.Key) + 1);
        }
    }

    public void SyncRenderTransforms()
    {
        for (var i = 0; i < _agents.Count; i++)
        {
            var position = _agents[i].Alive ? _agents[i].Position : new Vector3(0f, -100f, 0f);
            _multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, position));
            _multiMesh.SetInstanceColor(i, ProfessionColor(_agents[i]));
        }
    }

    public CitizenState[] CaptureState()
    {
        var result = new CitizenState[_agents.Count];
        for (var i = 0; i < _agents.Count; i++)
            result[i] = new CitizenState(
                _agents[i].Id,
                _world.CellToIndex(_agents[i].Cell),
                _agents[i].Hunger,
                _agents[i].Profession,
                _agents[i].Alive,
                _agents[i].FoodPlan,
                _agents[i].EatTimeLeft,
                _world.CellToIndex(_agents[i].FoodSource),
                _agents[i].FoodFromLoose,
                _agents[i].Identity,
                _agents[i].AgeDays,
                _agents[i].BabyDays,
                _agents[i].Religion,
                _agents[i].HomeRoomId,
                _agents[i].NeedTimeLeft,
                new Dictionary<string, int>(_agents[i].Needs, System.StringComparer.OrdinalIgnoreCase),
                _agents[i].Health.Capture());
        return result;
    }

    public IReadOnlyDictionary<BuildJob, int> CaptureConstructionTransit()
    {
        var result = new Dictionary<BuildJob, int>();
        foreach (var agent in _agents.Where(agent => agent.Alive && agent.Carrying &&
                     agent.CarriedAmount > 0 && agent.Job?.IsConstruction == true))
        {
            var remaining = agent.CarriedAmount;
            if (agent.ConstructionBatch.Count == 0)
            {
                result[agent.Job!] = remaining;
                continue;
            }
            var start = agent.Job!.Phase == BuildPhase.DeliveringMaterials
                ? agent.ConstructionBatchIndex
                : agent.ConstructionBatchIndex + 1;
            for (var index = start; index < agent.ConstructionBatch.Count && remaining > 0; index++)
            {
                var load = agent.ConstructionBatch[index];
                var amount = System.Math.Min(load.Amount, remaining);
                result[load.Job] = result.GetValueOrDefault(load.Job) + amount;
                remaining -= amount;
            }
        }
        return result;
    }

    public void RestoreState(IReadOnlyList<CitizenState> states)
    {
        while (_agents.Count > states.Count)
        {
            ClearPath(_agents[^1]);
            _agents.RemoveAt(_agents.Count - 1);
        }
        while (_agents.Count < states.Count)
            _agents.Add(new Agent());
        Identities.Clear();
        for (var i = 0; i < states.Count; i++)
        {
            var agent = _agents[i];
            agent.Id = states[i].Id;
            agent.Identity = Identities.Restore(states[i].Identity);
            agent.Cell = _world.FromIndex(states[i].Cell);
            agent.Position = _world.CellToWorld(agent.Cell, 0.45f);
            agent.Hunger = states[i].Hunger;
            agent.Profession = states[i].Profession;
            agent.Alive = states[i].Alive;
            agent.FoodPlan = states[i].FoodPlan == 2 ? (byte)2 : (byte)0;
            agent.EatTimeLeft = agent.FoodPlan == 2 ? states[i].EatTimeLeft : 0f;
            agent.FoodSource = _world.FromIndex(states[i].FoodSource);
            agent.FoodFromLoose = false;
            agent.AgeDays = states[i].AgeDays;
            agent.BabyDays = states[i].BabyDays;
            agent.Religion = states[i].Religion;
            agent.HomeRoomId = states[i].HomeRoomId;
            agent.NeedTimeLeft = states[i].NeedTimeLeft;
            agent.Needs.Clear();
            foreach (var pair in states[i].Needs) agent.Needs[pair.Key] = pair.Value;
            agent.Health.Restore(states[i].Health);
            _rooms.Housing.RegisterResident(agent.Id, agent.Identity.Race, agent.Identity.Class);
            if (agent.HomeRoomId != 0 && !_rooms.Housing.RestoreOccupancy(agent.Id, agent.HomeRoomId))
                agent.HomeRoomId = 0;
            ClearPath(agent);
            agent.Job = null;
        }
        _nextCitizenId = states.Count == 0 ? 1 : states.Max(state => state.Id) + 1;
        _multiMesh.InstanceCount = _agents.Count;
        SyncRenderTransforms();
    }

    private void RebalanceProfessions()
    {
        RebalanceProfession(WorkProfession.Baker, BakerTarget);
        RebalanceProfession(WorkProfession.Carpenter, CarpenterTarget);
        RebalanceProfession(WorkProfession.Farmer, ProfessionTarget(WorkProfession.Farmer));
        RebalanceProfession(WorkProfession.Miner, ProfessionTarget(WorkProfession.Miner));
        RebalanceProfession(WorkProfession.Undertaker, ProfessionTarget(WorkProfession.Undertaker));
        RebalanceProfession(WorkProfession.Scholar, ProfessionTarget(WorkProfession.Scholar));
        RebalanceProfession(WorkProfession.Guard, ProfessionTarget(WorkProfession.Guard));
        RebalanceProfession(WorkProfession.Administrator, ProfessionTarget(WorkProfession.Administrator));
    }

    private int ProfessionTarget(WorkProfession profession) => profession switch
    {
        WorkProfession.Baker => _bakerLimit >= 0
            ? _bakerLimit
            : _rooms.RequiredWorkers(profession),
        WorkProfession.Carpenter => _carpenterLimit >= 0
            ? _carpenterLimit
            : _rooms.RequiredWorkers(profession),
        WorkProfession.Farmer or WorkProfession.Miner or WorkProfession.Undertaker or
            WorkProfession.Scholar or WorkProfession.Guard or WorkProfession.Administrator =>
            _rooms.RequiredWorkers(profession),
        _ => Count
    };

    private void RebalanceProfession(WorkProfession profession, int required)
    {
        var assigned = CountProfession(profession);
        if (assigned < required)
        {
            foreach (var agent in _agents.OrderByDescending(candidate => WorkPreference(candidate, profession)))
            {
                if (assigned >= required) break;
                if (!agent.Alive || !HumanoidTypeRules.Works(agent.Identity.Type) ||
                    agent.Job is not null || agent.Profession != WorkProfession.Laborer) continue;
                agent.Profession = profession;
                assigned++;
            }
        }
        else if (assigned > required)
        {
            foreach (var agent in _agents)
            {
                if (assigned <= required) break;
                if (!agent.Alive || agent.Job is not null || agent.Profession != profession) continue;
                agent.Profession = WorkProfession.Laborer;
                assigned--;
            }
        }
    }

    private static double WorkPreference(Agent agent, WorkProfession profession)
    {
        if (!OriginalGameData.Current.Races.TryGetValue(agent.Identity.Race, out var race)) return 0;
        var keys = profession switch
        {
            WorkProfession.Baker => new[] { "REFINER_BAKERY", "REFINER*", "*" },
            WorkProfession.Carpenter => new[] { "WORKSHOP_CARPENTER", "WORKSHOP*", "*" },
            WorkProfession.Farmer => new[] { "FARM*", "ORCHARD*", "PASTURE*", "*" },
            WorkProfession.Miner => new[] { "MINE*", "*" },
            WorkProfession.Undertaker => new[] { "GRAVEYARD_NORMAL", "TOMB_NORMAL", "*" },
            WorkProfession.Scholar => new[] { "LABORATORY*", "LIBRARY*", "UNIVERSITY*", "*" },
            WorkProfession.Guard => new[] { "_GUARD", "_POLICE", "_PRISON", "_COURT", "*" },
            WorkProfession.Administrator => new[] { "ADMIN*", "_EMBASSY", "*" },
            _ => new[] { "*" }
        };
        foreach (var key in keys)
            if (race.WorkPreferences.TryGetValue(key, out var value)) return value;
        return 0;
    }

    private int CountProfession(WorkProfession profession)
    {
        var count = 0;
        foreach (var agent in _agents)
            if (agent.Alive && agent.Profession == profession) count++;
        return count;
    }

    private static Color ProfessionColor(Agent agent)
    {
        if (agent.Hunger >= StarvationThreshold) return new Color("c94b40");
        if (agent.Carrying) return new Color("d8c56a");
        return agent.Profession switch
        {
            WorkProfession.Baker => new Color("f2e8cc"),
            WorkProfession.Carpenter => new Color("b97845"),
            WorkProfession.Farmer => new Color("74a851"),
            WorkProfession.Miner => new Color("808080"),
            WorkProfession.Undertaker => new Color("756a80"),
            WorkProfession.Scholar => new Color("86b8bd"),
            WorkProfession.Guard => new Color("9b4545"),
            WorkProfession.Administrator => new Color("7996b8"),
            _ => new Color("e0b56d")
        };
    }

    private bool Assign(Agent agent, BuildJob job, JobBoard jobs, ResourceLedger resources)
    {
        var resumingHaul = job.IsHaulJob && job.PickedUp;
        var resumingCorpse = job.IsCorpseJob && job.PickedUp;
        var resumingSupply = job.IsSupplyJob && job.PickedUp;
        var fetchingSupply = job.IsSupplyJob && !job.PickedUp;
        var resumingMaintenance = job.Kind == BuildKind.Maintenance &&
            job.ResourceAmount > 0 && job.PickedUp;
        var fetchingMaintenance = job.Kind == BuildKind.Maintenance &&
            job.ResourceAmount > 0 && !job.PickedUp;
        var fetchingConstructionMaterial = job.IsConstruction &&
            job.Phase == BuildPhase.FetchingMaterials && job.MaterialsNeeded > 0;
        GridCoord? destination;
        if (resumingHaul || resumingCorpse) destination = job.Destination;
        else if (job.IsCorpseJob) destination = job.Cell;
        else if (resumingSupply) destination = FindWorkCell(job);
        else if (fetchingSupply) destination = job.Destination;
        else if (resumingMaintenance) destination = FindWorkCell(job);
        else if (fetchingMaintenance)
            destination = _rooms.FindSupplyCell(job.Resource);
        else if (fetchingConstructionMaterial)
            destination = _hauling.FindAccounted(job.Resource, agent.Cell) ??
                          _rooms.FindSupplyCell(job.Resource);
        else destination = FindWorkCell(job);
        if (destination is null) return false;
        if (fetchingConstructionMaterial || fetchingMaintenance)
            job.SetResourceSource(destination.Value);
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell,
            destination.Value,
            GridWorld.Width,
            GridWorld.Height,
            _world.Data.IsBlocked,
            _world.Data.MovementCost);
        if (agent.Cell != destination.Value && path.Count == 0) return false;

        agent.Job = job;
        agent.JobTravelSeconds = 0;
        agent.ConstructionBatch.Clear();
        agent.ConstructionBatchIndex = 0;
        agent.Carrying = resumingHaul || resumingCorpse || resumingSupply || resumingMaintenance;
        if (resumingHaul)
        {
            agent.CarriedResource = job.OutputResource;
            agent.CarriedAmount = job.OutputAmount;
        }
        else if (resumingSupply)
        {
            agent.CarriedResource = job.Resource;
            agent.CarriedAmount = job.ResourceAmount;
        }
        else if (resumingMaintenance)
        {
            agent.CarriedResource = job.Resource;
            agent.CarriedAmount = job.ResourceAmount;
        }
        if (fetchingConstructionMaterial)
        {
            job.Phase = BuildPhase.FetchingMaterials;
            agent.ConstructionBatch.Add(new ConstructionLoad(job, job.ReservedAmount));
            var remaining = BuildJob.MaximumFetchAmount - job.ReservedAmount;
            foreach (var adjacent in jobs.ClaimAdjacentMaterialBatch(job, resources, remaining))
            {
                adjacent.SetResourceSource(destination.Value);
                agent.ConstructionBatch.Add(new ConstructionLoad(adjacent, adjacent.ReservedAmount));
                remaining -= adjacent.ReservedAmount;
            }
        }
        SetPath(agent, path);
        return true;
    }

    private bool TickFoodPlan(Agent agent, float delta, JobBoard jobs, ResourceLedger resources)
    {
        if (agent.FoodPlan is 2 or 5)
        {
            agent.EatTimeLeft -= delta;
            if (agent.EatTimeLeft <= 0f)
            {
                var wasStarving = agent.Hunger >= StarvationThreshold;
                agent.Hunger = Mathf.Max(0f, agent.Hunger - NeedChunk);
                if (wasStarving && agent.Hunger < StarvationThreshold && StarvingCount > 0)
                    StarvingCount--;
                agent.FoodPlan = 0;
                ClearFoodService(agent, false);
            }
            return true;
        }

        if (agent.FoodPlan == 4)
        {
            if (MoveAlongPath(agent, delta)) return true;
            if (agent.FoodService is null || !_rooms.Services.IsRegistered(agent.FoodService) ||
                !agent.FoodService.Consume())
            {
                ClearFoodService(agent, true);
                agent.FoodPlan = 0;
                return false;
            }
            _rooms.RecordServiceUse(agent.FoodService.RoomId, agent.FoodService.Rule.Need, resources);
            agent.FoodPlan = 5;
            agent.EatTimeLeft = EatingDuration();
            return true;
        }

        if (agent.FoodPlan == 1)
        {
            if (MoveAlongPath(agent, delta)) return true;
            var consumed = agent.FoodFromLoose
                ? _hauling.ConsumeReservedLoose(agent.FoodSource, agent.CarriedResource)
                : resources.TryTake(agent.CarriedResource, 1);
            if (consumed && !agent.FoodFromLoose &&
                _hauling.HasAccounted(agent.FoodSource, agent.CarriedResource))
            {
                _hauling.TakeAccounted(agent.FoodSource, agent.CarriedResource, 1);
                if (!_hauling.HasAccounted(agent.FoodSource, agent.CarriedResource))
                    _rooms.RemoveLandingSupply(agent.CarriedResource, agent.FoodSource);
            }
            if (consumed)
            {
                agent.FoodPlan = 2;
                agent.EatTimeLeft = EatingDuration();
            }
            else
            {
                agent.FoodPlan = 0;
            }
            agent.FoodFromLoose = false;
            return true;
        }

        var seekThreshold = agent.Job is null ? NeedChunk : FoodSeekThreshold;
        if (agent.Hunger < seekThreshold || agent.Carrying) return false;
        if (agent.Job is null && TryStartMealService(agent)) return true;
        var foodResource = AvailableFood(resources);
        var foodCell = foodResource is { } available
            ? _rooms.FindSupplyCell(available)
            : null;
        var fromLoose = false;
        if (foodCell is null)
        {
            foodResource = ResourceKind.Food;
            foodCell = _hauling.FindReservable(ResourceKind.Food, agent.Cell);
            if (foodCell is not null)
            {
                if (!_hauling.ReserveLoose(foodCell.Value, ResourceKind.Food)) foodCell = null;
                else fromLoose = true;
            }
        }
        if (foodCell is null)
        {
            if (agent.Hunger < StarvationThreshold) return false;
            ReleaseJobForHigherPriority(agent, jobs, resources);
            // F_PlanStarve enters a desperate state when no reservable food exists.
            // State 3 keeps food search active and suppresses ordinary work.
            agent.FoodPlan = 3;
            ClearService(agent, true);
            return true;
        }
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell,
            foodCell.Value,
            GridWorld.Width,
            GridWorld.Height,
            _world.Data.IsBlocked,
            _world.Data.MovementCost);
        if (agent.Cell != foodCell.Value && path.Count == 0)
        {
            if (fromLoose) _hauling.ReleaseLooseReservation(foodCell.Value, foodResource!.Value);
            return false;
        }
        ReleaseJobForHigherPriority(agent, jobs, resources);
        ClearService(agent, true);
        ClearMourning(agent);
        SetPath(agent, path);
        agent.FoodSource = foodCell.Value;
        agent.FoodFromLoose = fromLoose;
        agent.CarriedResource = foodResource!.Value;
        agent.FoodPlan = 1;
        return true;
    }

    private static ResourceKind? AvailableFood(ResourceLedger resources)
    {
        foreach (var kind in new[]
                 { ResourceKind.Food, ResourceKind.Ration, ResourceKind.Fruit, ResourceKind.Vegetable })
            if (resources.Get(kind) > 0) return kind;
        return null;
    }

    private bool TryStartMealService(Agent agent)
    {
        var selected = _rooms.ServiceCandidates(agent.Cell).FirstOrDefault(candidate =>
            candidate.Service.Rule.Need.Equals("HUNGER", System.StringComparison.OrdinalIgnoreCase) &&
            _rooms.FoodVenues.IsVenue(candidate.Service.RoomId));
        if (selected.Service is null) return false;
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell, selected.Cell, GridWorld.Width, GridWorld.Height,
            _world.Data.IsBlocked, _world.Data.MovementCost);
        if (agent.Cell != selected.Cell && path.Count == 0 || !selected.Service.Reserve()) return false;
        agent.FoodService = selected.Service;
        agent.FoodServiceCell = selected.Cell;
        agent.FoodPlan = 4;
        SetPath(agent, path);
        return true;
    }

    private static void ClearFoodService(Agent agent, bool cancelReservation)
    {
        if (cancelReservation) agent.FoodService?.Cancel();
        agent.FoodService = null;
    }

    private bool TryStartService(Agent agent)
    {
        if (agent.ServiceRemaining <= 0 || agent.Carrying || agent.Job is not null) return false;
        var candidates = _rooms.ServiceCandidates(agent.Cell)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Service.Rule.Need) &&
                                agent.Needs.ContainsKey(candidate.Service.Rule.Need) &&
                                _rooms.ServiceMatchesReligion(
                                    candidate.Service.RoomId, agent.Religion))
            .ToArray();
        if (candidates.Length == 0) return false;
        var missingAccess = candidates.Where(candidate =>
            !agent.ServiceAccess.ContainsKey(candidate.Service.Rule.Need)).ToArray();
        if (missingAccess.Length > 0) candidates = missingAccess;
        var usageByNeed = candidates.GroupBy(candidate => candidate.Service.Rule.Need)
            .ToDictionary(group => group.Key, group => group.Sum(candidate =>
                System.Math.Max(0, candidate.Service.Rule.Usage)));
        double Weight((RoomServiceInstanceRuntime Service, GridCoord Cell, int Distance) candidate)
        {
            var rule = candidate.Service.Rule;
            var needRate = OriginalGameData.Current.NeedRates.GetValueOrDefault(rule.Need, 0);
            var usage = System.Math.Max(0, rule.Usage);
            var total = usageByNeed.GetValueOrDefault(rule.Need);
            return total > 0 ? needRate * usage / total : needRate;
        }
        var usageTotal = candidates.Sum(Weight);
        var choice = usageTotal > 0 ? GD.Randf() * usageTotal : 0;
        var selected = candidates[0];
        foreach (var candidate in candidates)
        {
            choice -= Weight(candidate);
            if (choice <= 0)
            {
                selected = candidate;
                break;
            }
        }
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell,
            selected.Cell,
            GridWorld.Width,
            GridWorld.Height,
            _world.Data.IsBlocked,
            _world.Data.MovementCost);
        if (agent.Cell != selected.Cell && path.Count == 0) return false;
        if (!selected.Service.Reserve()) return false;
        agent.Service = selected.Service;
        agent.ServiceCell = selected.Cell;
        agent.ServiceDistance = selected.Distance;
        agent.ServicePlan = 1;
        SetPath(agent, path);
        return true;
    }

    private bool TryStartHospital(Agent agent)
    {
        var candidates = _rooms.ServiceCandidates(agent.Cell)
            .Where(candidate => candidate.Service.Rule.Need.Equals(
                "HOSPITAL", System.StringComparison.OrdinalIgnoreCase)).ToArray();
        if (candidates.Length == 0) return false;
        var selected = candidates.OrderBy(candidate => candidate.Distance).First();
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell, selected.Cell, GridWorld.Width, GridWorld.Height,
            _world.Data.IsBlocked, _world.Data.MovementCost);
        if (agent.Cell != selected.Cell && path.Count == 0 || !selected.Service.Reserve()) return false;
        agent.Service = selected.Service;
        agent.ServiceCell = selected.Cell;
        agent.ServiceDistance = selected.Distance;
        agent.ServicePlan = 1;
        SetPath(agent, path);
        return true;
    }

    private bool TickServicePlan(Agent agent, float delta, ResourceLedger resources)
    {
        if (agent.Service is null || !_rooms.Services.IsRegistered(agent.Service))
        {
            agent.Service?.Cancel();
            agent.ServicePlan = 0;
            agent.Service = null;
            return false;
        }
        if (agent.ServicePlan == 1)
        {
            if (MoveAlongPath(agent, delta)) return true;
            agent.ServicePlan = 2;
            agent.ServiceTimeLeft = ServiceDuration(agent.Service.Rule.Need);
            return true;
        }
        agent.ServiceTimeLeft -= delta;
        if (agent.ServiceTimeLeft > 0) return true;
        if (agent.Service.Consume())
        {
            var rule = agent.Service.Rule;
            var radius = System.Math.Max(1, rule.Radius);
            var proximity = System.Math.Clamp(
                1.0 - (agent.ServiceDistance - radius / 3.0) / radius, 0, 1);
            agent.ServiceAccess[rule.Need] = 1.0;
            var roomDefinition = _rooms.DefinitionKey(agent.Service.RoomId);
            if (!string.IsNullOrWhiteSpace(roomDefinition))
                agent.ServiceAccessByRoom[roomDefinition] = 1.0;
            agent.ServiceQuality[rule.Need] = _rooms.ServiceQuality(agent.Service);
            agent.ServiceProximity[rule.Need] = System.Math.Sqrt(proximity);
            if (!string.IsNullOrWhiteSpace(rule.Need))
                agent.Needs[rule.Need] = System.Math.Max(
                    0, agent.Needs.GetValueOrDefault(rule.Need) - NeedChunk);
            foreach (var boost in rule.Boosts)
                agent.ServiceBoosts[boost.Key] = boost.Value;
            ApplyServiceEffect(agent, rule.Need);
            _rooms.RecordServiceUse(agent.Service.RoomId, rule.Need, resources);
            agent.ServiceRemaining = System.Math.Max(0, agent.ServiceRemaining - 1);
        }
        ClearService(agent, false);
        return true;
    }

    private static void ClearService(Agent agent, bool cancelReservation)
    {
        if (cancelReservation) agent.Service?.Cancel();
        agent.Service = null;
        agent.ServicePlan = 0;
        agent.ServiceTimeLeft = 0;
    }

    private static float ServiceDuration(string need)
    {
        if (need.Equals("HOSPITAL", System.StringComparison.OrdinalIgnoreCase)) return 60f;
        if (need.Equals("TEMPLE", System.StringComparison.OrdinalIgnoreCase) ||
            need.Equals("SHRINE", System.StringComparison.OrdinalIgnoreCase))
        {
            var seconds = 0f;
            var prayers = 5 + (int)(GD.Randi() % 10);
            for (var prayer = 0; prayer < prayers; prayer++)
                seconds += 4 + (int)(GD.Randi() % 4);
            return seconds;
        }
        if (need is "ARENA" or "ARENAG" or "SPEAKER" or "STAGE") return 10f;
        if (need.Equals("DOCTOR", System.StringComparison.OrdinalIgnoreCase)) return 25f;
        if (need.Equals("GROOMING", System.StringComparison.OrdinalIgnoreCase)) return 15f;
        if (need.Equals("MASSAGE", System.StringComparison.OrdinalIgnoreCase)) return 6f;
        return 1f + GD.Randf() * 5f;
    }

    private static bool HashChance(int citizenId, int day, double chance)
    {
        unchecked
        {
            uint value = (uint)(citizenId * 0x1f123bb5 ^ day * 0x5f356495);
            value ^= value >> 16;
            value *= 0x7feb352d;
            value ^= value >> 15;
            return value / (double)uint.MaxValue < System.Math.Clamp(chance, 0, 1);
        }
    }

    private void ApplyCrime(Agent offender, ResourceLedger resources)
    {
        var crime = _rooms.Law.CrimeOf(offender.Id);
        if (crime is "THEFT" or "S_THEFT")
        {
            var candidates = System.Enum.GetValues<ResourceKind>()
                .Where(resource => resources.Get(resource) > 0).ToArray();
            if (candidates.Length == 0) return;
            var resource = candidates[(int)(GD.Randi() % (uint)candidates.Length)];
            // Theft.res reserves 4 + RND.rInt(10) loose units, then removes them.
            resources.TryTake(resource, System.Math.Min(
                resources.Get(resource), 4 + (int)(GD.Randi() % 10)));
            return;
        }
        if (crime == "VANDALISM")
        {
            _rooms.VandalizeNearest(offender.Cell);
            return;
        }
        if (crime is not ("MURDER" or "S_MURDER")) return;
        var victim = _agents.Where(candidate => candidate.Alive && candidate.Id != offender.Id &&
                System.Math.Abs(candidate.Cell.X - offender.Cell.X) +
                System.Math.Abs(candidate.Cell.Z - offender.Cell.Z) <= 100)
            .OrderBy(candidate => System.Math.Abs(candidate.Cell.X - offender.Cell.X) +
                                  System.Math.Abs(candidate.Cell.Z - offender.Cell.Z))
            .FirstOrDefault();
        if (victim is not null)
            victim.Health.AddInjury((int)(GD.Randf() * 0.99f * CitizenHealthRuntime.InjuryMaximum));
    }

    private void ProcessLawEffects(ResourceLedger resources, JobBoard jobs)
    {
        while (_rooms.Law.TryTakeEffect(out var effect))
        {
            var target = _agents.FirstOrDefault(agent => agent.Alive && agent.Id == effect.CitizenId);
            if (target is null) continue;
            switch (effect.Punishment)
            {
                case "BANISH":
                    target.Alive = false;
                    ReleaseHome(target, resources);
                    _leaveCounts[LeaveCause.Exiled] = _leaveCounts.GetValueOrDefault(LeaveCause.Exiled) + 1;
                    break;
                case "ENSLAVE":
                    Identities.ChangeType(target.Id, SocialClass.Slave, HumanoidType.Slave);
                    target.Identity = Identities.Get(target.Id)!;
                    target.Profession = WorkProfession.Laborer;
                    ReleaseHome(target, resources);
                    break;
                case "EXECUTE":
                    KillFromStarvation(target, resources, jobs, "EXECUTION");
                    _leaveCounts[LeaveCause.Executed] =
                        _leaveCounts.GetValueOrDefault(LeaveCause.Executed) + 1;
                    break;
                case "HARVEST":
                    if (_rooms.Cannibals.Complete(effect.FacilityRoomId, target.Identity.Race,
                            Count, resources))
                    {
                        KillFromStarvation(target, resources, jobs, "HARVEST");
                        _leaveCounts[LeaveCause.Executed] =
                            _leaveCounts.GetValueOrDefault(LeaveCause.Executed) + 1;
                    }
                    break;
                default:
                    var ordinary = target.Identity.Class == SocialClass.Slave
                        ? HumanoidType.Slave : HumanoidType.Subject;
                    Identities.ChangeType(target.Id, target.Identity.Class, ordinary);
                    target.Identity = Identities.Get(target.Id)!;
                    break;
            }
        }
    }

    private double AverageServiceValue(
        string need,
        System.Func<Agent, Dictionary<string, double>> selector,
        System.Func<Agent, bool>? filter = null)
    {
        var alive = _agents.Where(agent => agent.Alive && (filter is null || filter(agent))).ToArray();
        if (alive.Length == 0) return 0;
        return alive.Sum(agent => selector(agent).GetValueOrDefault(need)) / alive.Length;
    }

    private double AverageReligionServiceValue(
        string religion,
        string need,
        System.Func<Agent, Dictionary<string, double>> selector)
    {
        var followers = _agents.Where(agent => agent.Alive && agent.Religion.Equals(
            religion, System.StringComparison.OrdinalIgnoreCase)).ToArray();
        if (followers.Length == 0) return 0;
        return followers.Sum(agent => selector(agent).GetValueOrDefault(need)) / followers.Length;
    }

    private void ApplyServiceEffect(Agent agent, string need)
    {
        if (need.Equals("BATH", System.StringComparison.OrdinalIgnoreCase) ||
            need.Equals("WELL", System.StringComparison.OrdinalIgnoreCase))
        {
            agent.Dirtiness = 0;
            agent.Exposure = System.Math.Max(0, agent.Exposure - NeedChunk);
        }
        else if (need.Equals("HEARTH", System.StringComparison.OrdinalIgnoreCase))
        {
            agent.Exposure = System.Math.Max(0, agent.Exposure - NeedChunk);
        }
        else if (need.Equals("HOSPITAL", System.StringComparison.OrdinalIgnoreCase))
        {
            agent.Health.Treat(_rooms.ConsumeHospitalSupplies(agent.Service?.RoomId ?? 0));
        }
    }

    private static double HealthMultiplier(Agent agent) => agent.ServiceBoosts
        .Where(boost => boost.Key.Equals("PHYSICS_HEALTH>MUL",
            System.StringComparison.OrdinalIgnoreCase))
        .Select(boost => boost.Value).DefaultIfEmpty(1.0).Aggregate(1.0, (a, b) => a * b);

    private static double HomeFurnitureBoost(Agent agent) => agent.ServiceBoosts
        .Where(boost => boost.Key.Equals("CIVIC_FURNITURE>MUL",
            System.StringComparison.OrdinalIgnoreCase) || boost.Key.Equals("FURNITURE>MUL",
            System.StringComparison.OrdinalIgnoreCase))
        .Select(boost => boost.Value).DefaultIfEmpty(1.0).Aggregate(1.0, (a, b) => a * b);

    private void ReleaseHome(Agent agent, ResourceLedger resources)
    {
        foreach (var furniture in _rooms.Housing.RemoveResident(agent.Id))
            resources.ReturnToStock(furniture.Key, furniture.Value);
        agent.HomeRoomId = 0;
        agent.HomeActivity = HomeActivity.None;
    }

    private void KillFromStarvation(
        Agent agent,
        ResourceLedger resources,
        JobBoard jobs,
        string? cause = null)
    {
        ClearService(agent, true);
        ClearMourning(agent);
        if (agent.Job is not null)
        {
            if (agent.Job.IsCorpseJob)
            {
                if (agent.Job.PickedUp) _corpses.Drop(agent.Job, agent.Cell);
                else _corpses.Cancel(agent.Job.CorpseId);
                agent.Job.State = JobState.Cancelled;
            }
            else if (agent.Job.IsHaulJob)
            {
                _hauling.CancelStorageReservation(agent.Job);
                if (agent.Carrying) DropCarried(agent, jobs);
                agent.Job.State = JobState.Cancelled;
            }
            else if (agent.Job.IsSupplyJob && agent.Carrying)
            {
                DropCarried(agent, jobs);
                jobs.Cancel(agent.Job.Cell, resources);
            }
            else
            {
                // The worker dies, not the job. Delivered construction material
                // remains at the site and another odd-jobber can continue it.
                if (agent.Carrying) DropCarried(agent, jobs);
                jobs.Release(agent.Job, resources);
                ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
            }
            agent.Job = null;
        }
        if (agent.FoodPlan == 1 && agent.FoodFromLoose)
            _hauling.ReleaseLooseReservation(agent.FoodSource, agent.CarriedResource);
        agent.Carrying = false;
        ClearPath(agent);
        agent.FoodPlan = 0;
        agent.Alive = false;
        ReleaseHome(agent, resources);
        _corpses.Create(agent.Cell, cause ?? (agent.Health.ActiveDisease ? "DISEASE" :
            agent.Health.Injury >= CitizenHealthRuntime.InjuryMaximum ? "ACCIDENT" : "STARVATION"));
    }

    private void TickAgent(Agent agent, float delta, ResourceLedger resources, JobBoard jobs)
    {
        if (agent.MilitaryOrder)
        {
            if (!MoveAlongPath(agent, delta, agent.MilitarySpeedMultiplier))
            {
                agent.MilitaryOrder = false;
                agent.MilitarySpeedMultiplier = 1f;
            }
            return;
        }
        if (agent.Job is null) return;
        if (agent.Job.State == JobState.Cancelled)
        {
            if (agent.Job.IsCorpseJob)
            {
                if (agent.Job.PickedUp) _corpses.Drop(agent.Job, agent.Cell);
                else _corpses.Cancel(agent.Job.CorpseId);
            }
            if (agent.Job.IsHaulJob) _hauling.CancelStorageReservation(agent.Job);
            if (agent.Carrying) DropCarried(agent, jobs);
            ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
            DropDeliveredConstructionMaterials(agent.Job, jobs);
            ClearPath(agent);
            agent.Job = null;
            return;
        }
        if (MoveAlongPath(agent, delta))
        {
            agent.JobTravelSeconds += delta;
            return;
        }

        if (agent.Job.IsHaulJob && !agent.Carrying)
        {
            if (!_hauling.Pickup(agent.Job))
            {
                _hauling.CancelStorageReservation(agent.Job);
                agent.Job.State = JobState.Cancelled;
                agent.Job = null;
                return;
            }
            agent.Carrying = true;
            agent.CarriedResource = agent.Job.OutputResource;
            agent.CarriedAmount = agent.Job.OutputAmount;
            var deliveryPath = HierarchicalGridPathfinder.FindPath(
                agent.Cell,
                agent.Job.Destination,
                GridWorld.Width,
                GridWorld.Height,
                _world.Data.IsBlocked,
                _world.Data.MovementCost);
            SetPath(agent, deliveryPath);
            if (agent.Cell != agent.Job.Destination && deliveryPath.Count == 0)
            {
                _hauling.CancelStorageReservation(agent.Job);
                if (agent.Job.Kind == BuildKind.LogisticsTransfer)
                    _rooms.Logistics.ReturnToSource(agent.Job);
                else if (agent.Job.OutputAlreadyAccounted)
                    _hauling.SpawnAccounted(
                        agent.Job.Cell, agent.Job.OutputResource, agent.Job.OutputAmount, jobs);
                else
                    _hauling.Spawn(agent.Job.Cell, agent.Job.OutputResource, agent.Job.OutputAmount, jobs);
                agent.Job.PickedUp = false;
                ClearCarried(agent);
                agent.Job.State = JobState.Cancelled;
                agent.Job = null;
            }
            return;
        }

        if (agent.Job.IsCorpseJob && !agent.Job.PickedUp)
        {
            if (!_corpses.Pickup(agent.Job))
            {
                agent.Job.State = JobState.Cancelled;
                agent.Job = null;
                return;
            }
            agent.Carrying = true;
            var deliveryPath = HierarchicalGridPathfinder.FindPath(
                agent.Cell, agent.Job.Destination, GridWorld.Width, GridWorld.Height,
                _world.Data.IsBlocked, _world.Data.MovementCost);
            SetPath(agent, deliveryPath);
            if (agent.Cell != agent.Job.Destination && deliveryPath.Count == 0)
            {
                _corpses.Drop(agent.Job, agent.Cell);
                ClearCarried(agent);
                agent.Job.State = JobState.Cancelled;
                agent.Job = null;
            }
            return;
        }

        if (agent.Job.IsSupplyJob && !agent.Carrying)
        {
            var amount = agent.Job.Kind == BuildKind.ProductionSupply
                ? _rooms.PickupProductionSupply(agent.Job, resources)
                : resources.PickupProductionSupply(agent.Job);
            if (amount <= 0)
            {
                jobs.Release(agent.Job, resources);
                ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
                agent.Job = null;
                return;
            }
            ConsumeLandingSource(agent.Job, amount);
            agent.Carrying = true;
            agent.CarriedResource = agent.Job.Resource;
            agent.CarriedAmount = amount;
            var workCell = FindWorkCell(agent.Job);
            if (workCell is null || !SetPathTo(agent, workCell.Value))
            {
                DropCarried(agent, jobs);
                jobs.Cancel(agent.Job.Cell, resources);
                agent.Job = null;
            }
            return;
        }

        if (agent.Job.IsSupplyJob && agent.Carrying)
        {
            var remainder = agent.Job.Kind switch
            {
                BuildKind.EquipmentSupply => _rooms.CompleteEquipmentSupply(agent.Job, agent.CarriedAmount),
                BuildKind.RoomConstructionSupply =>
                    _rooms.CompleteRoomConstructionSupply(agent.Job, agent.CarriedAmount),
                BuildKind.HospitalSupply =>
                    _rooms.CompleteHospitalSupply(agent.Job, agent.CarriedAmount),
                BuildKind.TempleSupply =>
                    _rooms.CompleteTempleSupply(agent.Job, agent.CarriedAmount),
                BuildKind.BathFuel =>
                    _rooms.CompleteBathFuel(agent.Job, agent.CarriedAmount),
                BuildKind.VenueSupply =>
                    _rooms.CompleteVenueSupply(agent.Job, agent.CarriedAmount),
                BuildKind.AsylumFoodSupply =>
                    _rooms.CompleteAsylumFoodSupply(agent.Job, agent.CarriedAmount),
                BuildKind.JanitorSupply =>
                    _rooms.CompleteJanitorSupply(agent.Job, agent.CarriedAmount),
                BuildKind.HomeFurnitureSupply =>
                    _rooms.CompleteHomeFurnitureSupply(agent.Job, agent.CarriedAmount),
                BuildKind.PastureLivestockSupply =>
                    _rooms.CompletePastureLivestockSupply(agent.Job, agent.CarriedAmount),
                BuildKind.KnowledgeSupply =>
                    _rooms.CompleteKnowledgeSupply(agent.Job, agent.CarriedAmount),
                _ => _rooms.CompleteProductionSupply(
                    agent.Job, agent.CarriedAmount, agent.JobTravelSeconds)
            };
            if (remainder > 0)
                _hauling.Spawn(agent.Job.Cell, agent.CarriedResource, remainder, jobs);
            ClearCarried(agent);
            agent.Job.State = JobState.Completed;
            agent.Job = null;
            return;
        }

        if (agent.Job.Kind == BuildKind.Maintenance &&
            agent.Job.ResourceAmount > 0 && !agent.Job.PickedUp)
        {
            var amount = agent.Job.TakeReservedResource();
            if (amount <= 0)
            {
                jobs.Release(agent.Job, resources);
                agent.Job = null;
                return;
            }
            agent.Carrying = true;
            agent.CarriedResource = agent.Job.Resource;
            agent.CarriedAmount = amount;
            var workCell = FindWorkCell(agent.Job);
            if (workCell is null || !SetPathTo(agent, workCell.Value))
            {
                DropCarried(agent, jobs);
                agent.Job.ResetResourcePickup();
                jobs.Release(agent.Job, resources);
                agent.Job = null;
            }
            return;
        }

        if (agent.Job.Kind == BuildKind.Maintenance && agent.Carrying)
        {
            ClearCarried(agent);
            agent.Job.DeliverMaintenanceResource();
            return;
        }

        if (agent.Job.IsConstruction && agent.Job.Phase == BuildPhase.FetchingMaterials)
        {
            var amount = 0;
            foreach (var load in agent.ConstructionBatch)
            {
                if (load.Job.State == JobState.Cancelled) continue;
                amount += load.Job.TakeReservedMaterials();
                load.Job.Phase = BuildPhase.DeliveringMaterials;
            }
            if (agent.ConstructionBatch.Count == 0)
                amount = agent.Job.TakeReservedMaterials();
            if (amount <= 0)
            {
                jobs.Release(agent.Job, resources);
                ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
                agent.Job = null;
                return;
            }
            ConsumeLandingSource(agent.Job, amount);
            agent.Carrying = true;
            agent.CarriedResource = agent.Job.Resource;
            agent.CarriedAmount = amount;
            agent.Job.Phase = BuildPhase.DeliveringMaterials;
            var workCell = FindWorkCell(agent.Job);
            if (workCell is null || !SetPathTo(agent, workCell.Value))
            {
                DropCarried(agent, jobs);
                jobs.Release(agent.Job, resources);
                ReleaseConstructionBatch(agent, jobs, resources, agent.Job);
                agent.Job = null;
            }
            return;
        }

        if (agent.Job.IsConstruction && agent.Job.Phase == BuildPhase.DeliveringMaterials)
        {
            var delivery = agent.ConstructionBatch.Count > agent.ConstructionBatchIndex
                ? agent.ConstructionBatch[agent.ConstructionBatchIndex].Amount
                : agent.CarriedAmount;
            agent.Job.DeliverMaterials(System.Math.Min(delivery, agent.CarriedAmount));
            if (agent.Job.MaterialsNeeded == 0)
                agent.Job.PrepareConstructionSite(
                    _world.Data, _hauling.HasUnreservedAt);
            agent.CarriedAmount = System.Math.Max(0, agent.CarriedAmount - delivery);
            if (agent.CarriedAmount == 0) ClearCarried(agent);
            if (agent.Job.MaterialsNeeded > 0)
            {
                jobs.Release(agent.Job, resources);
                agent.Job = null;
                agent.ConstructionBatch.Clear();
            }
            return;
        }

        agent.Job.WorkLeft -= delta;
        if (agent.Job.WorkLeft > 0f) return;
        if (agent.Job.IsConstruction && agent.Job.IsPreparationPhase)
        {
            if (!CompleteConstructionPreparation(agent.Job, jobs, resources, agent.Cell,
                    agent.ConstructionBatch.Count > 0))
                agent.Job = null;
            return;
        }
        switch (agent.Job.Kind)
        {
            case BuildKind.Wall: _world.BuildWall(agent.Job.Cell); break;
            case BuildKind.RoomDoor: _world.SetDoor(agent.Job.Cell); break;
            case BuildKind.Road:
                _world.BuildRoad(agent.Job.Cell, OriginalGameData.Current.Floor(agent.Job.RoadKey));
                break;
            case BuildKind.RoomFloor:
                _world.BuildRoomFloor(agent.Job.Cell, OriginalGameData.Current.Floor(agent.Job.FloorKey));
                break;
            case BuildKind.RoomRoof:
                _world.Data.SetRoof(agent.Job.Cell, true);
                break;
            case BuildKind.Furniture:
            {
                var blockers = agent.Job.FurnitureBlockerCells.ToHashSet();
                var reachable = agent.Job.FurnitureReachableCells.ToHashSet();
                var work = agent.Job.FurnitureWorkCells.ToHashSet();
                var storage = agent.Job.FurnitureStorageCells.ToHashSet();
                foreach (var cell in agent.Job.FurnitureCells)
                    _world.BuildFurniture(cell, blockers.Contains(cell), reachable.Contains(cell),
                        work.Contains(cell), storage.Contains(cell));
                _rooms.CompleteFurnitureVisual(agent.Job);
                break;
            }
            case BuildKind.Production:
                _rooms.CompleteProduction(
                    agent.Job, resources, _hauling, jobs, agent.JobTravelSeconds);
                break;
            case BuildKind.Haul:
            case BuildKind.RoomOutputHaul:
            case BuildKind.LogisticsTransfer:
                _hauling.Deliver(agent.Job, resources, jobs);
                jobs.RetryBlocked();
                ClearCarried(agent);
                break;
            case BuildKind.Maintenance:
                if (agent.Job.RoomId == 0) _roadMaintenance.Complete(agent.Job);
                else _rooms.CompleteMaintenance(agent.Job);
                break;
            case BuildKind.CorpseHaul:
                if (!_corpses.Deliver(agent.Job))
                {
                    _corpses.Drop(agent.Job, agent.Cell);
                    agent.Job.State = JobState.Cancelled;
                    ClearCarried(agent);
                    agent.Job = null;
                    return;
                }
                ClearCarried(agent);
                break;
            case BuildKind.Sanitation:
                _rooms.CompleteSanitation(agent.Job);
                break;
            case BuildKind.BathPump:
                _rooms.CompleteBathPump(agent.Job);
                break;
            case BuildKind.ActivityWork:
                break;
            case BuildKind.ServicePreparation:
                _rooms.CompleteServicePreparation(agent.Job);
                break;
            case BuildKind.TransportPreparation:
                _rooms.CompleteTransportPreparation(agent.Job);
                break;
            case BuildKind.KnowledgeWork:
                _rooms.CompleteKnowledgeWork(agent.Job);
                break;
            case BuildKind.LawProcess:
                _rooms.CompleteLawProcess(agent.Job);
                ProcessLawEffects(resources, jobs);
                break;
            case BuildKind.MilitaryTraining:
                _rooms.CompleteMilitaryTraining(agent.Id, agent.Job, PersonalStats);
                break;
            case BuildKind.ArtilleryLoad:
                var profile = PersonalStats.Ensure(agent.Id);
                _rooms.CompleteArtilleryLoad(agent.Job, resources,
                    profile.BasicTraining / (double)CitizenPersonalStatsRuntime.TrainingMaximum);
                break;
            case BuildKind.Forage:
            case BuildKind.ClearWood:
            case BuildKind.ClearStone:
            case BuildKind.ClearWater:
            case BuildKind.DigTunnel:
                // JobClear performs one terrain/resource step, then becomes reservable
                // again while anything remains on the tile.
                if (PerformTerrainJob(agent.Job, resources))
                {
                    agent.Job.WorkLeft = TerrainJobSeconds(agent.Job.Kind);
                    jobs.Release(agent.Job, resources);
                    agent.Job = null;
                    return;
                }
                break;
        }
        var completed = agent.Job;
        completed.State = JobState.Completed;
        agent.Job = null;
        if (completed.IsConstruction)
        {
            if (TryAdvanceConstructionBatch(agent, jobs, resources)) return;
            var adjacent = jobs.TryClaimAdjacentCompatible(completed, resources);
            if (adjacent is not null && !Assign(agent, adjacent, jobs, resources))
                jobs.Release(adjacent, resources);
        }
    }

    private bool PerformTerrainJob(BuildJob job, ResourceLedger resources)
    {
        var data = _world.Data;
        var cell = job.Cell;
        switch (job.Kind)
        {
            case BuildKind.Forage:
            {
                var type = data.GrowableType(cell);
                var amount = data.GrowableAmount(cell);
                if (type < 0 || amount <= 0) return false;
                if (type < OriginalGameData.Current.Growables.Count &&
                    OriginalGameData.TryMapResource(
                        OriginalGameData.Current.Growables[type].Resource, out var resource))
                    resources.Add(resource, 1);
                data.SetGrowable(cell, type, amount - 1);
                return data.GrowableAmount(cell) > 0;
            }
            case BuildKind.ClearWood:
                if (data.VegetationAmount(cell) <= 0 || data.GrowableType(cell) >= 0) return false;
                data.ClearVegetationStep(cell);
                resources.Add(ResourceKind.Wood, 1);
                return data.VegetationAmount(cell) > 0;
            case BuildKind.ClearStone:
                if (!data.Has(cell, TileFlags.ClearableTerrain) ||
                    data.Has(cell, TileFlags.Mountain)) return false;
                data.ClearTerrain(cell);
                resources.Add(ResourceKind.Stone, 1);
                return false;
            case BuildKind.ClearWater:
                if (!data.Has(cell, TileFlags.Water)) return false;
                data.SetDeepWater(cell, false);
                data.Set(cell, TileFlags.SaltWater, false);
                data.SetTerrain(cell, GroundKind.Soil, data.Elevation(cell),
                    data.Fertility(cell), data.Moisture(cell));
                return false;
            case BuildKind.DigTunnel:
                if (!data.Has(cell, TileFlags.Mountain) || data.Has(cell, TileFlags.Cave)) return false;
                data.SetCave(cell, true);
                data.ClearTerrain(cell);
                resources.Add(ResourceKind.Stone, 1);
                return false;
            default:
                return false;
        }
    }

    private static float TerrainJobSeconds(BuildKind kind) => kind switch
    {
        BuildKind.ClearStone => 5f,
        BuildKind.DigTunnel => 60f,
        _ => 30f
    };

    private void ConsumeLandingSource(BuildJob job, int amount)
    {
        if (!_hauling.HasAccounted(job.Destination, job.Resource)) return;
        _hauling.TakeAccounted(job.Destination, job.Resource, amount);
        if (!_hauling.HasAccounted(job.Destination, job.Resource))
            _rooms.RemoveLandingSupply(job.Resource, job.Destination);
    }

    private bool CompleteConstructionPreparation(
        BuildJob job,
        JobBoard jobs,
        ResourceLedger resources,
        GridCoord workerCell,
        bool retainReservation)
    {
        switch (job.Phase)
        {
            case BuildPhase.ClearingTerrain:
                _world.Data.ClearTerrain(job.ConstructionCell);
                break;
            case BuildPhase.ClearingVegetation:
                _world.Data.ClearVegetationStep(job.ConstructionCell);
                break;
            case BuildPhase.RemovingObstacle:
                if (_hauling.RemoveOneUnreservedAt(job.ConstructionCell, out var resource))
                    _hauling.Spawn(workerCell, resource, 1, jobs);
                break;
        }
        job.PrepareConstructionSite(_world.Data, _hauling.HasUnreservedAt);
        if (job.Kind == BuildKind.RoomClear && job.Phase == BuildPhase.Constructing)
            return true;
        if (retainReservation && job.Phase == BuildPhase.Constructing) return true;
        jobs.Release(job, resources);
        return false;
    }

    private bool TryAdvanceConstructionBatch(Agent agent, JobBoard jobs, ResourceLedger resources)
    {
        while (++agent.ConstructionBatchIndex < agent.ConstructionBatch.Count)
        {
            var next = agent.ConstructionBatch[agent.ConstructionBatchIndex].Job;
            if (next.State == JobState.Cancelled)
            {
                var cancelledCargo = System.Math.Min(
                    agent.CarriedAmount,
                    agent.ConstructionBatch[agent.ConstructionBatchIndex].Amount);
                if (cancelledCargo > 0)
                {
                    _hauling.Spawn(agent.Cell, agent.CarriedResource, cancelledCargo, jobs);
                    agent.CarriedAmount -= cancelledCargo;
                }
                continue;
            }
            agent.Job = next;
            var workCell = FindWorkCell(next);
            if (workCell is not null && SetPathTo(agent, workCell.Value)) return true;
            DropCarried(agent, jobs);
            ReleaseConstructionBatch(agent, jobs, resources, null);
            agent.Job = null;
            return false;
        }
        agent.ConstructionBatch.Clear();
        if (agent.CarriedAmount > 0) DropCarried(agent, jobs);
        else ClearCarried(agent);
        return false;
    }

    private static void ReleaseConstructionBatch(
        Agent agent,
        JobBoard jobs,
        ResourceLedger resources,
        BuildJob? except)
    {
        foreach (var load in agent.ConstructionBatch)
            if (!ReferenceEquals(load.Job, except) && load.Job.State == JobState.Reserved)
                jobs.Release(load.Job, resources);
        agent.ConstructionBatch.Clear();
        agent.ConstructionBatchIndex = 0;
    }

    private bool MoveAlongPath(Agent agent, float delta, float speedMultiplier = 1f)
    {
        if (!_pathPool.TryPeek(agent.PathHandle, out var nextCell)) return false;
        var target = _world.CellToWorld(nextCell, 0.45f);
        agent.Position = agent.Position.MoveToward(
            target, MoveSpeed * speedMultiplier * _world.Data.MovementSpeedMultiplier(nextCell) * delta);
        if (agent.Position.DistanceSquaredTo(target) < 0.001f)
        {
            agent.Cell = nextCell;
            agent.Position = target;
            if (!_pathPool.Advance(agent.PathHandle)) agent.PathHandle = 0;
        }
        return true;
    }

    private static float EatingDuration() =>
        8f + GD.Randf() * 4f + GD.Randf() * 2f + GD.Randf() * 4f + GD.Randf() * 2f;

    private GridCoord? FindWorkCell(GridCoord target)
    {
        foreach (var offset in GridCoord.Cardinal)
        {
            var candidate = target + offset;
            if (_world.CanStandForConstruction(candidate))
                return candidate;
        }
        return null;
    }

    private GridCoord? FindWorkCell(BuildJob job)
    {
        if (job.Kind != BuildKind.Furniture) return FindWorkCell(job.Cell);
        var footprint = job.FurnitureCells.ToHashSet();
        var blockers = job.FurnitureBlockerCells.ToHashSet();
        IReadOnlyList<GridCoord> targets = job.IsPreparationPhase
            ? new[] { job.ConstructionCell }
            : job.FurnitureReachableCells.Count > 0
                ? job.FurnitureReachableCells
                : job.FurnitureCells;
        foreach (var tile in targets)
        foreach (var offset in GridCoord.Cardinal)
        {
            var candidate = tile + offset;
            if (footprint.Contains(candidate))
            {
                if ((job.IsPreparationPhase || !blockers.Contains(candidate)) && _world.IsInside(candidate) &&
                    !_world.Data.IsBlocked(candidate) &&
                    !_world.Data.Has(candidate, TileFlags.Wall | TileFlags.Furniture))
                    return candidate;
            }
            else if (_world.CanStandForConstruction(candidate))
                return candidate;
        }
        return null;
    }

    private void SetPath(Agent agent, IReadOnlyList<GridCoord> path)
    {
        ClearPath(agent);
        agent.PathHandle = _pathPool.Acquire(path);
    }

    private bool SetPathTo(Agent agent, GridCoord destination)
    {
        var path = HierarchicalGridPathfinder.FindPath(
            agent.Cell,
            destination,
            GridWorld.Width,
            GridWorld.Height,
            _world.Data.IsBlocked,
            _world.Data.MovementCost);
        if (agent.Cell != destination && path.Count == 0) return false;
        SetPath(agent, path);
        return true;
    }

    private void DropCarried(Agent agent, JobBoard jobs)
    {
        if (agent.Carrying && agent.CarriedAmount > 0)
        {
            if (agent.Job?.Kind == BuildKind.LogisticsTransfer)
                _rooms.Logistics.ReturnToSource(agent.Job);
            else if (agent.Job?.OutputAlreadyAccounted == true)
                _hauling.SpawnAccounted(
                    agent.Cell, agent.CarriedResource, agent.CarriedAmount, jobs);
            else
                _hauling.Spawn(agent.Cell, agent.CarriedResource, agent.CarriedAmount, jobs);
        }
        ClearCarried(agent);
    }

    private void ClearCarried(Agent agent)
    {
        agent.Carrying = false;
        agent.CarriedAmount = 0;
    }

    private void DropDeliveredConstructionMaterials(BuildJob job, JobBoard jobs)
    {
        if (!job.IsConstruction) return;
        var delivered = job.TakeDeliveredMaterials();
        if (delivered > 0) _hauling.Spawn(job.Cell, job.Resource, delivered, jobs);
    }

    private void ClearPath(Agent agent)
    {
        _pathPool.Release(agent.PathHandle);
        agent.PathHandle = 0;
    }
}
