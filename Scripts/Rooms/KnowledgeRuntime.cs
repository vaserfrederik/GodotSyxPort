using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

public sealed class KnowledgeRoomInstanceRuntime
{
    public int RoomId { get; init; }
    public KnowledgeRoomRule Rule { get; init; } = null!;
    public GridCoord[] Workstations { get; set; } = Array.Empty<GridCoord>();
    public Dictionary<ResourceKind, int> Inputs { get; } = new();
    public Dictionary<ResourceKind, int> ReservedInputs { get; } = new();
    public Dictionary<ResourceKind, double> ConsumptionProgress { get; } = new();
    public double Value { get; set; }
    public double DayGain { get; set; }
    public double AverageSkill { get; set; } = 1;
    public int SkillSamples { get; set; }
    public double WorkProgress { get; set; } = 1;
    public int StudentsToday { get; set; }
    public int StudentCapacity => Workstations.Length;
}

/// <summary>
/// Shared AdminData/RoomConsumption runtime for laboratories and libraries plus
/// adult RoomEducationHelper behavior for universities.
/// </summary>
public sealed class KnowledgeRuntime
{
    public const double WorkSeconds = 45;
    public const double AdultLearningStep = 1.0 / 16.0;
    public const int AdultLearningUpdatesPerDay = 16;
    public const int DaysPerYear = 16;
    private readonly GridWorld _world;
    private readonly Dictionary<int, KnowledgeRoomInstanceRuntime> _instances = new();
    private readonly Dictionary<string, double> _currencies = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<KnowledgeRoomInstanceRuntime> Instances => _instances.Values;
    public IReadOnlyDictionary<string, double> Currencies => _currencies;

    public KnowledgeRuntime(GridWorld world) => _world = world;

    public void Synchronize(IEnumerable<RoomRecord> rooms)
    {
        var active = new HashSet<int>();
        foreach (var room in rooms.Where(room => room.State == RoomState.Operational))
        {
            var rule = OriginalGameData.Current.Room(room.DefinitionKey)?.Knowledge;
            if (rule is null) continue;
            active.Add(room.Id);
            if (!_instances.TryGetValue(room.Id, out var instance))
                _instances.Add(room.Id, instance = new KnowledgeRoomInstanceRuntime
                    { RoomId = room.Id, Rule = rule });
            instance.Workstations = room.Cells
                .Where(index => _world.Data.Has(_world.FromIndex(index), TileFlags.Furniture))
                .Select(_world.FromIndex).ToArray();
        }
        foreach (var id in _instances.Keys.Where(id => !active.Contains(id)).ToArray())
            _instances.Remove(id);
        RecalculateCurrencies();
    }

    public void BeginDay()
    {
        foreach (var instance in _instances.Values)
        {
            instance.StudentsToday = 0;
            if (instance.Rule.Kind == "UNIVERSITY") continue;
            var degradation = Math.Clamp(instance.Rule.DegradePerYear / DaysPerYear, 0, 1);
            instance.Value = (instance.Value + instance.DayGain) * (1.0 - degradation);
            instance.DayGain = 0;
            SetWorkProgress(instance);
        }
        RecalculateCurrencies();
    }

    public void Schedule(JobBoard jobs, ResourceLedger resources, IEnumerable<RoomRecord> rooms,
        GridCoord? stockpile)
    {
        foreach (var instance in _instances.Values) instance.ReservedInputs.Clear();
        foreach (var job in jobs.All.Where(job => job.Kind == BuildKind.KnowledgeSupply &&
                     job.State is not (JobState.Cancelled or JobState.Completed)))
            if (_instances.TryGetValue(job.RoomId, out var destination))
                destination.ReservedInputs[job.Resource] =
                    destination.ReservedInputs.GetValueOrDefault(job.Resource) + job.ResourceAmount;
        foreach (var instance in _instances.Values.Where(value => value.Rule.Kind != "UNIVERSITY"))
        {
            if (instance.Workstations.Length == 0) continue;
            var room = rooms.FirstOrDefault(candidate => candidate.Id == instance.RoomId);
            if (room is null) continue;
            foreach (var input in instance.Rule.ConsumptionRates)
            {
                if (!OriginalGameData.TryMapResource(input.Key, out var resource)) continue;
                var target = (int)Math.Ceiling(room.Employment.Employed * input.Value);
                var stored = instance.Inputs.GetValueOrDefault(resource);
                var reserved = instance.ReservedInputs.GetValueOrDefault(resource);
                if (stockpile is null || stored + reserved >= target || resources.Get(resource) <= 0) continue;
                var amount = Math.Min(BuildJob.MaximumFetchAmount,
                    Math.Min(target - stored - reserved, resources.Get(resource)));
                var station = instance.Workstations.FirstOrDefault();
                if (amount <= 0 || jobs.All.Any(job => job.Kind == BuildKind.KnowledgeSupply &&
                        job.RoomId == instance.RoomId && job.Resource == resource &&
                        job.State is not (JobState.Cancelled or JobState.Completed))) continue;
                if (jobs.Add(BuildJob.KnowledgeSupply(
                        station, stockpile.Value, instance.RoomId, resource, amount)))
                    instance.ReservedInputs[resource] = reserved + amount;
            }

            foreach (var station in instance.Workstations)
            {
                if (jobs.GetAt(station) is not null) continue;
                if (!HasRequiredInput(instance)) break;
                jobs.Add(BuildJob.KnowledgeWork(station, instance.RoomId,
                    instance.Rule.Kind is "ADMINISTRATION" or "DIPLOMACY"
                        ? WorkProfession.Administrator : WorkProfession.Scholar));
            }
        }
    }

    public int CompleteSupply(BuildJob job, int amount)
    {
        if (!_instances.TryGetValue(job.RoomId, out var instance)) return amount;
        var accepted = Math.Max(0, amount);
        instance.Inputs[job.Resource] = instance.Inputs.GetValueOrDefault(job.Resource) + accepted;
        instance.ReservedInputs[job.Resource] = Math.Max(0,
            instance.ReservedInputs.GetValueOrDefault(job.Resource) - job.ResourceAmount);
        return amount - accepted;
    }

    public void CompleteWork(BuildJob job, RoomRecord? room)
    {
        if (room is null || !_instances.TryGetValue(job.RoomId, out var instance) ||
            instance.Rule.Kind == "UNIVERSITY") return;
        var consumptionBoost = 1.0;
        foreach (var input in instance.Rule.ConsumptionRates)
        {
            if (!OriginalGameData.TryMapResource(input.Key, out var resource)) continue;
            var progress = instance.ConsumptionProgress.GetValueOrDefault(resource) +
                2.0 * input.Value * WorkSeconds / OriginalGameData.Current.SecondsPerDay;
            var whole = (int)progress;
            if (whole > 0)
            {
                var consumed = Math.Min(whole, instance.Inputs.GetValueOrDefault(resource));
                instance.Inputs[resource] = instance.Inputs.GetValueOrDefault(resource) - consumed;
                progress -= consumed;
            }
            instance.ConsumptionProgress[resource] = progress;
            if (instance.Inputs.GetValueOrDefault(resource) > 0)
                consumptionBoost += instance.Rule.ConsumptionBoosts.GetValueOrDefault(input.Key);
        }
        var annualSeconds = OriginalGameData.Current.SecondsPerDay * DaysPerYear;
        var workValue = instance.Rule.ValuePerWorker * instance.Rule.DegradePerYear /
                        Math.Max(1, annualSeconds) * 2.0;
        var skill = Math.Clamp(room.Employment.TotalEfficiency, 0, 2);
        instance.DayGain += WorkSeconds * skill * instance.WorkProgress *
                            instance.Rule.WorkSpeed * workValue * consumptionBoost;
        instance.SkillSamples++;
        instance.AverageSkill += (skill - instance.AverageSkill) / instance.SkillSamples;
        SetWorkProgress(instance);
        RecalculateCurrencies();
    }

    public bool CanStudyAdult(int citizenId, CitizenPersonalStatsRuntime stats,
        EducationTrack track, int limit = CitizenPersonalStatsRuntime.EducationMaximum) =>
        _instances.Values.Any(instance => instance.Rule.Kind == "UNIVERSITY" &&
            instance.StudentsToday < instance.StudentCapacity) &&
        stats.EducationValue(citizenId, track, EducationAge.Adulthood) < Math.Clamp(limit, 0, 100);

    public bool TryStudyAdult(int citizenId, CitizenPersonalStatsRuntime stats,
        EducationTrack track, double bonus = 1.0)
    {
        var university = _instances.Values.FirstOrDefault(instance =>
            instance.Rule.Kind == "UNIVERSITY" && instance.StudentsToday < instance.StudentCapacity);
        if (university is null || !CanStudyAdult(citizenId, stats, track)) return false;
        university.StudentsToday++;
        stats.Educate(citizenId, track, EducationAge.Adulthood,
            Math.Max(0, university.Rule.LearningSpeed * bonus * AdultLearningStep *
                        AdultLearningUpdatesPerDay));
        return true;
    }

    public double Currency(string key) => _currencies.GetValueOrDefault(key);

    private static bool HasRequiredInput(KnowledgeRoomInstanceRuntime instance) =>
        instance.Rule.ConsumptionRates.All(input =>
            !OriginalGameData.TryMapResource(input.Key, out var resource) ||
            instance.Inputs.GetValueOrDefault(resource) > 0);

    private static void SetWorkProgress(KnowledgeRoomInstanceRuntime instance)
    {
        var denominator = Math.Max(1,
            instance.AverageSkill * instance.Workstations.Length * instance.Rule.ValuePerWorker);
        var progress = Math.Clamp((instance.Value + instance.DayGain) / denominator, 0, 1);
        instance.WorkProgress = 1.0 - progress * progress;
    }

    private void RecalculateCurrencies()
    {
        _currencies.Clear();
        foreach (var group in _instances.Values.Where(instance =>
                     instance.Rule.Kind != "UNIVERSITY" && instance.Rule.Currency.Length > 0)
                     .GroupBy(instance => instance.Rule.Currency, StringComparer.OrdinalIgnoreCase))
            _currencies[group.Key] = group.Sum(instance => instance.Value + instance.DayGain);
    }
}
