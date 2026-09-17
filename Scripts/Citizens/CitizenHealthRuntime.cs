using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Data;

namespace GodotSyxPort.Citizens;

public enum DiseaseState : byte { None, Incubating, Sick, Immune }
public sealed record CitizenHealthSnapshot(
    string DiseaseKey, DiseaseState Status, int DiseaseDays, bool FatalOutcome, int Injury);

/// <summary>Source-shaped StatsDisease/Data/Updater and StatsNeeds.StatDanger state.</summary>
public sealed class CitizenHealthRuntime
{
    public const int InjuryMaximum = 255;
    public const int InjuryDanger = InjuryMaximum / 2;
    public const int InjuryCritical = 3 * (InjuryMaximum / 4);
    public string DiseaseKey { get; private set; } = "";
    public DiseaseState Status { get; private set; }
    public int DiseaseDays { get; private set; }
    public bool FatalOutcome { get; private set; }
    public int Injury { get; private set; }
    public bool InDanger => Injury >= InjuryDanger;
    public bool Critical => Injury >= InjuryCritical;
    public bool ActiveDisease => Status == DiseaseState.Sick;
    public bool RequiresHospital(OriginalGameData data)
    {
        var disease = data.Diseases.GetValueOrDefault(DiseaseKey);
        return Critical || (ActiveDisease &&
            (FatalOutcome || disease is not null && disease.InfectionDays - DiseaseDays > 1));
    }

    public CitizenHealthSnapshot Capture() => new(
        DiseaseKey, Status, DiseaseDays, FatalOutcome, Injury);

    public void Restore(CitizenHealthSnapshot snapshot)
    {
        DiseaseKey = snapshot.DiseaseKey ?? "";
        Status = snapshot.Status;
        DiseaseDays = Math.Max(0, snapshot.DiseaseDays);
        FatalOutcome = snapshot.FatalOutcome;
        Injury = Math.Clamp(snapshot.Injury, 0, InjuryMaximum);
    }

    public void AddInjury(int amount) => Injury = Math.Clamp(Injury + amount, 0, InjuryMaximum);

    public void Infect(DiseaseRule disease, bool incubating)
    {
        DiseaseKey = disease.Key;
        Status = incubating ? DiseaseState.Incubating : DiseaseState.Sick;
        DiseaseDays = 0;
        FatalOutcome = GD.Randf() < disease.FatalityRate;
    }

    public bool Update16(OriginalGameData data, bool newDay, double health)
    {
        if (Injury > 0)
        {
            if (Injury >= InjuryMaximum) return false;
            Injury = InDanger
                ? Math.Min(InjuryMaximum, Injury + 4)
                : Math.Max(0, Injury - (int)Math.Ceiling(1 + 15 * Math.Max(0, health)));
        }
        var disease = data.Diseases.GetValueOrDefault(DiseaseKey);
        switch (Status)
        {
            case DiseaseState.Incubating:
                if (disease is null) ClearDisease();
                else if (GD.Randi() % (uint)Math.Max(1, disease.IncubationDays * 16) == 0)
                    Status = DiseaseState.Sick;
                break;
            case DiseaseState.Sick when newDay:
                DiseaseDays++;
                if (disease is not null && DiseaseDays >= disease.InfectionDays)
                {
                    if (FatalOutcome) return false;
                    Cure();
                }
                break;
            case DiseaseState.Immune when newDay:
                if (++DiseaseDays >= 15) ClearDisease();
                TryRegularDisease(data, health);
                break;
            case DiseaseState.None when newDay:
                TryRegularDisease(data, health);
                break;
        }
        return Injury < InjuryMaximum;
    }

    public void Treat(double recoveryRate)
    {
        var treatment = Math.Clamp(recoveryRate, 0, 1);
        if (InDanger && GD.Randf() <= treatment) Injury = InjuryDanger - 2;
        if (ActiveDisease)
        {
            FatalOutcome &= GD.Randf() > treatment;
            var disease = OriginalGameData.Current.Diseases.GetValueOrDefault(DiseaseKey);
            if (disease is not null)
                DiseaseDays = Math.Max(DiseaseDays,
                    (int)Math.Ceiling(disease.InfectionDays * (1 - treatment)));
            if (!FatalOutcome && (disease is null || DiseaseDays >= disease.InfectionDays * (1 - treatment)))
                Cure();
        }
    }

    public void Cure()
    {
        Status = DiseaseState.Immune;
        DiseaseDays = 0;
        FatalOutcome = false;
    }

    private void TryRegularDisease(OriginalGameData data, double health)
    {
        var chanceInverse = Math.Max(1,
            (int)Math.Ceiling(data.RegularSicknessDayInterval * (1 + Math.Max(health, 0))));
        if (GD.Randi() % (uint)chanceInverse != 0) return;
        var regular = data.Diseases.Values.Where(disease => disease.Regular &&
            !(Status == DiseaseState.Immune && disease.Key.Equals(
                DiseaseKey, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (regular.Length > 0) Infect(regular[(int)(GD.Randi() % (uint)regular.Length)], false);
    }

    private void ClearDisease()
    {
        DiseaseKey = "";
        Status = DiseaseState.None;
        DiseaseDays = 0;
        FatalOutcome = false;
    }
}
