using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Citizens;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class HomeInstanceRuntime
{
    public int RoomId { get; init; }
    public GridCoord ServiceCell { get; set; }
    public int Capacity { get; set; }
    public bool NobleOnly { get; set; }
    public double Isolation { get; set; }
    public HashSet<int> Occupants { get; } = new();
}

public sealed class HomeResidentRuntime
{
    public int CitizenId { get; init; }
    public string Race { get; set; } = "";
    public SocialClass Class { get; set; }
    public Dictionary<ResourceKind, int> Furniture { get; } = new();
    public Dictionary<ResourceKind, double> WearDebt { get; } = new();
}

/// <summary>Common HOME contract for ordinary houses and noble chambers.</summary>
public sealed class HousingRuntime
{
    private readonly Dictionary<int, HomeInstanceRuntime> _homes = new();
    private readonly Dictionary<int, int> _homeByCitizen = new();
    private readonly Dictionary<int, HomeResidentRuntime> _residents = new();
    private readonly Dictionary<(string Race, SocialClass Class, ResourceKind Resource), int> _targets = new();

    public IReadOnlyCollection<HomeInstanceRuntime> Homes => _homes.Values;
    public int Occupants => _homeByCitizen.Count;
    public int Capacity => _homes.Values.Sum(home => home.Capacity);
    public IReadOnlyCollection<HomeResidentRuntime> Residents => _residents.Values;

    public void Synchronize(IEnumerable<RoomRecord> rooms, Func<int, GridCoord> fromIndex)
    {
        var live = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational && IsHome(room.DefinitionKey)))
        {
            live.Add(room.Id);
            if (!_homes.TryGetValue(room.Id, out var home))
            {
                home = new HomeInstanceRuntime { RoomId = room.Id };
                _homes.Add(room.Id, home);
            }
            home.ServiceCell = fromIndex(room.Cells.First());
            home.NobleOnly = room.DefinitionKey.Equals("_HOME_CHAMBER", StringComparison.OrdinalIgnoreCase);
            home.Capacity = home.NobleOnly
                ? (room.Employment.Employed >= 4 ? 1 : 0)
                : HouseCapacity(room);
            home.Isolation = Math.Clamp(room.Isolation, 0, 1);
            while (home.Occupants.Count > home.Capacity)
                Vacate(home.Occupants.Last());
        }
        foreach (var roomId in _homes.Keys.Where(id => !live.Contains(id)).ToArray())
        {
            foreach (var citizenId in _homes[roomId].Occupants.ToArray()) _homeByCitizen.Remove(citizenId);
            _homes.Remove(roomId);
        }
    }

    public HomeInstanceRuntime? TryOccupy(int citizenId, GridCoord origin, bool noble = false)
    {
        if (_homeByCitizen.TryGetValue(citizenId, out var current)) return _homes.GetValueOrDefault(current);
        var home = _homes.Values.Where(value => value.Occupants.Count < value.Capacity &&
                                                (!value.NobleOnly || noble))
            .OrderBy(value => Distance(origin, value.ServiceCell)).FirstOrDefault();
        if (home is null) return null;
        home.Occupants.Add(citizenId);
        _homeByCitizen[citizenId] = home.RoomId;
        return home;
    }

    public void RegisterResident(int citizenId, string race, SocialClass socialClass)
    {
        if (!_residents.TryGetValue(citizenId, out var resident))
        {
            resident = new HomeResidentRuntime { CitizenId = citizenId };
            _residents[citizenId] = resident;
        }
        resident.Race = race;
        resident.Class = socialClass;
    }

    public bool Vacate(int citizenId)
    {
        if (!_homeByCitizen.Remove(citizenId, out var roomId)) return false;
        if (_homes.TryGetValue(roomId, out var home)) home.Occupants.Remove(citizenId);
        return true;
    }

    public IReadOnlyList<int> RemoveRoom(int roomId)
    {
        if (!_homes.Remove(roomId, out var home)) return Array.Empty<int>();
        var occupants = home.Occupants.ToArray();
        foreach (var citizenId in occupants) _homeByCitizen.Remove(citizenId);
        return occupants;
    }

    public IReadOnlyDictionary<ResourceKind, int> RemoveResident(int citizenId)
    {
        Vacate(citizenId);
        if (!_residents.Remove(citizenId, out var resident))
            return new Dictionary<ResourceKind, int>();
        return resident.Furniture.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public HomeInstanceRuntime? Home(int citizenId) =>
        _homeByCitizen.TryGetValue(citizenId, out var roomId) ? _homes.GetValueOrDefault(roomId) : null;

    public bool RestoreOccupancy(int citizenId, int roomId)
    {
        if (!_homes.TryGetValue(roomId, out var home) || home.Occupants.Count >= home.Capacity)
            return false;
        Vacate(citizenId);
        home.Occupants.Add(citizenId);
        _homeByCitizen[citizenId] = roomId;
        return true;
    }

    public int Maximum(int citizenId, ResourceKind resource)
    {
        if (!_residents.TryGetValue(citizenId, out var resident) ||
            !OriginalGameData.Current.Races.TryGetValue(resident.Race, out var race)) return 0;
        var rules = race.HomeFurniture.GetValueOrDefault(resident.Class);
        return rules is null ? 0 : rules.FirstOrDefault(pair =>
            OriginalGameData.TryMapResource(pair.Key, out var mapped) && mapped == resource).Value;
    }

    public int Target(int citizenId, ResourceKind resource)
    {
        if (!_residents.TryGetValue(citizenId, out var resident)) return 0;
        return Math.Clamp(_targets.GetValueOrDefault((resident.Race.ToUpperInvariant(), resident.Class, resource)),
            0, Maximum(citizenId, resource));
    }

    public void SetTarget(string race, SocialClass socialClass, ResourceKind resource, int amount)
    {
        var maximum = 0;
        if (OriginalGameData.Current.Races.TryGetValue(race, out var rule))
        {
            var resources = rule.HomeFurniture.GetValueOrDefault(socialClass);
            maximum = resources is null ? 0 : resources.FirstOrDefault(pair =>
                OriginalGameData.TryMapResource(pair.Key, out var mapped) && mapped == resource).Value;
        }
        _targets[(race.ToUpperInvariant(), socialClass, resource)] = Math.Clamp(amount, 0, maximum);
    }

    public double FurnitureFulfillment(int citizenId)
    {
        if (!_residents.TryGetValue(citizenId, out var resident) ||
            !OriginalGameData.Current.Races.TryGetValue(resident.Race, out var race)) return 0;
        var rules = race.HomeFurniture.GetValueOrDefault(resident.Class);
        if (rules is null) return 1;
        var maximum = rules.Values.Sum();
        if (maximum == 0) return 1;
        return Math.Clamp(resident.Furniture.Sum(pair => pair.Value) / (double)maximum, 0, 1);
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, GridCoord? source)
    {
        if (source is null) return;
        foreach (var resident in _residents.Values)
        {
            var home = Home(resident.CitizenId);
            if (home is null || jobs.All.Any(job => job.Kind == BuildKind.HomeFurnitureSupply &&
                                               job.FacilitySlotId == resident.CitizenId &&
                                               job.State is not (JobState.Completed or JobState.Cancelled))) continue;
            foreach (var resource in Enum.GetValues<ResourceKind>())
            {
                var missing = Target(resident.CitizenId, resource) - resident.Furniture.GetValueOrDefault(resource);
                var amount = Math.Min(BuildJob.MaximumFetchAmount, Math.Min(missing, resources.Get(resource)));
                if (amount <= 0) continue;
                if (jobs.Add(BuildJob.HomeFurnitureSupply(home.ServiceCell, source.Value, home.RoomId,
                        resident.CitizenId, resource, amount))) break;
            }
        }
    }

    public int CompleteSupply(BuildJob job, int amount)
    {
        if (!_residents.TryGetValue(job.FacilitySlotId, out var resident) ||
            Home(job.FacilitySlotId)?.RoomId != job.RoomId) return amount;
        var accepted = Math.Min(amount, Math.Max(0,
            Target(resident.CitizenId, job.Resource) - resident.Furniture.GetValueOrDefault(job.Resource)));
        resident.Furniture[job.Resource] = resident.Furniture.GetValueOrDefault(job.Resource) + accepted;
        return amount - accepted;
    }

    public void TickDay(int citizenId, double furnitureBoost = 1.0)
    {
        if (!_residents.TryGetValue(citizenId, out var resident)) return;
        var home = Home(citizenId);
        var isolationMultiplier = home is null ? 1.0 :
            1.0 + Math.Clamp((1.0 - home.Isolation) * 2.0, 0, 1);
        var yearlyRate = Math.Clamp(0.5 / Math.Max(0.01, furnitureBoost), 0, 1);
        foreach (var resource in resident.Furniture.Keys.ToArray())
        {
            resident.WearDebt[resource] = resident.WearDebt.GetValueOrDefault(resource) +
                                           resident.Furniture[resource] * yearlyRate * isolationMultiplier / 16.0;
            var worn = Math.Min(resident.Furniture[resource], (int)resident.WearDebt[resource]);
            if (worn <= 0) continue;
            resident.Furniture[resource] -= worn;
            resident.WearDebt[resource] -= worn;
        }
    }

    private static int HouseCapacity(RoomRecord room)
    {
        var rule = OriginalGameData.Current.Room(room.DefinitionKey);
        if (rule is null) return 0;
        var capacity = 0.0;
        foreach (var pair in room.ItemGroupAmounts)
        {
            if ((uint)pair.Key >= (uint)rule.FurnisherItems.Count) continue;
            var stats = rule.FurnisherItems[pair.Key].Stats;
            if (stats.Count == 0) continue;
            // HomeContructor.maxOccupants: groups 3/5/10 expand by upgrade
            // to 4/7/14 and finally 5/9/18.
            var baseCapacity = Math.Max(0, stats[0]);
            var upgraded = room.UpgradeLevel switch
            {
                1 when pair.Key == 0 => 4,
                1 when pair.Key == 1 => 7,
                1 when pair.Key == 2 => 14,
                >= 2 when pair.Key == 0 => 5,
                >= 2 when pair.Key == 1 => 9,
                >= 2 when pair.Key == 2 => 18,
                _ => baseCapacity
            };
            capacity += upgraded * pair.Value;
        }
        return Math.Max(0, (int)Math.Round(capacity));
    }

    private static bool IsHome(string key) => key.Equals("_HOME", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("_HOME_CHAMBER", StringComparison.OrdinalIgnoreCase);

    private static int Distance(GridCoord a, GridCoord b) => Math.Abs(a.X - b.X) + Math.Abs(a.Z - b.Z);
}
