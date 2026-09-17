using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotSyxPort.Citizens;

public enum EducationTrack : byte { Education, Indoctrination }
public enum EducationAge : byte { Childhood, Adulthood }

public sealed class CitizenPersonalProfileRuntime
{
    public int CitizenId { get; init; }
    public byte EnemyKills { get; set; }
    public byte CombatExperience { get; set; }
    public byte BasicTraining { get; set; }
    public Dictionary<string, byte> MilitaryTraining { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<(EducationTrack Track, EducationAge Age), byte> Education { get; } = new();
    public Dictionary<string, byte> Traits { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Compact individual state shared by StatsBattle, StatsEducation and StatsTraits.
/// Byte/nibble limits and mutually exclusive education transfer follow the source
/// collections without importing their UI/booster graph.
/// </summary>
public sealed class CitizenPersonalStatsRuntime
{
    public const int TrainingMaximum = 15;
    public const int EducationMaximum = 100;
    public const int TraitMaximum = 15;
    private readonly Dictionary<int, CitizenPersonalProfileRuntime> _profiles = new();
    private readonly Dictionary<EducationTrack, double> _educationRemainder = new();

    public IReadOnlyCollection<CitizenPersonalProfileRuntime> Profiles => _profiles.Values;
    public CitizenPersonalProfileRuntime Ensure(int citizenId)
    {
        if (!_profiles.TryGetValue(citizenId, out var profile))
        {
            profile = new CitizenPersonalProfileRuntime { CitizenId = citizenId };
            _profiles.Add(citizenId, profile);
        }
        return profile;
    }

    // StatsBattle.makeAKill: one kill and 1..4 experience, stored as bytes.
    public void RecordKill(int citizenId)
    {
        var profile = Ensure(citizenId);
        profile.EnemyKills = (byte)Math.Min(byte.MaxValue, profile.EnemyKills + 1);
        profile.CombatExperience = (byte)Math.Min(byte.MaxValue,
            profile.CombatExperience + 1 + (int)(GD.Randi() % 4));
    }

    public bool BasicTrainingIsMaximum(int citizenId) => Ensure(citizenId).BasicTraining >= TrainingMaximum;

    public void SetBasicTraining(int citizenId, int value) =>
        Ensure(citizenId).BasicTraining = (byte)Math.Clamp(value, 0, TrainingMaximum);

    public bool ShouldTrain(int citizenId, string trainingKey, double target, bool training)
    {
        var profile = Ensure(citizenId);
        if (profile.BasicTraining < TrainingMaximum) return true;
        var current = profile.MilitaryTraining.GetValueOrDefault(trainingKey) / (double)TrainingMaximum;
        if (target > current) return true;
        if (target < current) return false;
        return target > 0 && training && profile.MilitaryTraining.GetValueOrDefault(trainingKey) < TrainingMaximum;
    }

    public void SetMilitaryTraining(int citizenId, string trainingKey, int value) =>
        Ensure(citizenId).MilitaryTraining[trainingKey] = (byte)Math.Clamp(value, 0, TrainingMaximum);

    public void Educate(int citizenId, EducationTrack track, EducationAge age, double amount)
    {
        if (amount <= 0) return;
        var accumulated = amount + _educationRemainder.GetValueOrDefault(track);
        var whole = (int)accumulated;
        _educationRemainder[track] = accumulated - whole;
        if (whole <= 0) return;
        var profile = Ensure(citizenId);
        var other = track == EducationTrack.Education
            ? EducationTrack.Indoctrination : EducationTrack.Education;
        foreach (var otherAge in Enum.GetValues<EducationAge>())
        {
            var key = (other, otherAge);
            var removed = Math.Min(whole, profile.Education.GetValueOrDefault(key));
            profile.Education[key] = (byte)(profile.Education.GetValueOrDefault(key) - removed);
            whole -= removed;
            if (whole == 0) return;
        }
        var target = (track, age);
        var available = EducationMaximum - profile.Education.GetValueOrDefault(target);
        var accepted = Math.Min(whole, available);
        profile.Education[target] = (byte)(profile.Education.GetValueOrDefault(target) + accepted);
        _educationRemainder[track] += whole - accepted;
    }

    public int EducationValue(int citizenId, EducationTrack track, EducationAge age) =>
        Ensure(citizenId).Education.GetValueOrDefault((track, age));

    public void SetTrait(int citizenId, string traitKey, double value) =>
        Ensure(citizenId).Traits[traitKey] = (byte)Math.Clamp(
            (int)(Math.Clamp(value, 0, 1) * TraitMaximum), 0, TraitMaximum);

    public double AverageEducation => _profiles.Count == 0 ? 0 : _profiles.Values.Average(profile =>
        profile.Education.Values.Sum(value => value) / (4.0 * EducationMaximum));
    public double AverageCombatExperience => _profiles.Count == 0 ? 0 :
        _profiles.Values.Average(profile => profile.CombatExperience / (double)byte.MaxValue);
    public int EnemyKills => _profiles.Values.Sum(profile => profile.EnemyKills);
}
