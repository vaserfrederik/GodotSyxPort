using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.World;

public enum EmissaryMissionKind : byte { SupportRegion, FlatterFaction, SabotageFaction }

public sealed record EmissaryMissionSnapshot(
    EmissaryMissionKind Kind, int TargetId, int Points);

public sealed record WorldDiplomacySnapshot(
    double ProducedPoints,
    int LastWarningDay,
    IReadOnlyList<EmissaryMissionSnapshot> Missions);

/// <summary>
/// Peaceful Emissaries/OpsEmi/RTrust adapter. Embassy output is a finite pool:
/// over-allocation reduces every mission by the same source-shaped efficiency.
/// </summary>
public sealed class WorldDiplomacyRuntime
{
    public const int SourceDaysPerYear = 16;
    public const int GiftWorkdayCredits = 400;
    public const int MaximumPointsPerMission = 1000;

    private readonly StrategicWorldRuntime _world;
    private readonly RegionalEconomyRuntime _economy;
    private readonly WorldFactionRuntime _factions;
    private readonly Dictionary<(EmissaryMissionKind Kind, int TargetId), int> _missions = new();
    private double _producedPoints;

    public WorldDiplomacyRuntime(
        StrategicWorldRuntime world,
        RegionalEconomyRuntime economy,
        WorldFactionRuntime factions)
    {
        _world = world;
        _economy = economy;
        _factions = factions;
    }

    public int Produced => Math.Max(0, (int)Math.Floor(_producedPoints));
    public int Spent => _missions.Values.Sum();
    public int Available => Produced - Spent;
    public double Efficiency => Spent <= Produced || Spent == 0
        ? 1
        : Math.Clamp(Produced / (double)Spent, 0, 1);
    public int LastWarningDay { get; private set; } = -60;
    public string LastResult { get; private set; } = "";
    public IReadOnlyList<EmissaryMissionSnapshot> Missions => _missions
        .Select(value => new EmissaryMissionSnapshot(value.Key.Kind, value.Key.TargetId, value.Value))
        .OrderBy(value => value.Kind).ThenBy(value => value.TargetId).ToArray();

    public void SetProduction(double diplomacyPoints) =>
        _producedPoints = Math.Max(0, diplomacyPoints);

    public int Allocation(EmissaryMissionKind kind, int targetId) =>
        _missions.GetValueOrDefault((kind, targetId));

    public bool SetAllocation(EmissaryMissionKind kind, int targetId, int points)
    {
        points = Math.Clamp(points, 0, MaximumPointsPerMission);
        if (!ValidTarget(kind, targetId))
            return Fail("Недопустимая цель дипломатической миссии.");
        var key = (kind, targetId);
        if (points == 0) _missions.Remove(key);
        else _missions[key] = points;
        LastResult = points == 0 ? "Миссия отменена." : $"Назначено эмиссаров: {points}.";
        return true;
    }

    public bool TrySetStance(int factionId, DiplomacyStance stance, int day)
    {
        if (factionId == _world.PlayerFactionId || _world.Faction(factionId) is null)
            return Fail("Нельзя изменить отношения с этой фракцией.");
        var state = _factions.State(factionId);
        if (state is null) return Fail("Дипломатический двор фракции недоступен.");
        var (opinion, trust) = RequiredStanding(stance);
        if (state.Opinion + 0.0001 < opinion || state.Trust + 0.0001 < trust)
            return Fail($"Недостаточно отношений: нужно мнение {opinion:P0}, доверие {trust:P0}.");
        if (!_factions.SetStance(_world.PlayerFactionId, factionId, stance, day))
            return Fail("Предложение статуса отклонено.");
        LastResult = $"Установлен статус {stance}.";
        return true;
    }

    public bool ApplyGift(int factionId, double credits, int day)
    {
        if (credits <= 0 || factionId == _world.PlayerFactionId || _factions.State(factionId) is null)
            return Fail("Дар не может быть передан этой фракции.");
        var state = _factions.State(factionId)!;
        var worth = Math.Max(GiftWorkdayCredits * 25.0,
            state.Credits + state.Stock.Values.Sum() * GiftWorkdayCredits);
        var opinion = Math.Clamp(credits * 12.0 / worth *
                                 _factions.GiftOpinionMultiplier(factionId), 0.0025, 0.25);
        _factions.AdjustOpinion(factionId, _world.PlayerFactionId, opinion);
        _factions.ReceiveGift(factionId, credits, day);
        LastResult = $"Дар улучшил мнение на {opinion:P1}.";
        return true;
    }

    public void Tick(double days, int day)
    {
        if (days <= 0) return;
        var efficiency = Efficiency;
        foreach (var mission in Missions)
        {
            var strength = mission.Points * efficiency;
            switch (mission.Kind)
            {
                case EmissaryMissionKind.SupportRegion:
                    _economy.AddPlayerSupport(mission.TargetId,
                        strength * days / (SourceDaysPerYear * 2.0));
                    break;
                case EmissaryMissionKind.FlatterFaction:
                    _factions.AdjustOpinion(mission.TargetId, _world.PlayerFactionId,
                        strength * days / (SourceDaysPerYear * 200.0));
                    break;
                case EmissaryMissionKind.SabotageFaction:
                    _factions.AdjustOpinion(mission.TargetId, _world.PlayerFactionId,
                        -strength * days / (SourceDaysPerYear * 100.0));
                    break;
            }
        }
        RemoveInvalidMissions();
        if (Efficiency < 1 && day - LastWarningDay > 10)
        {
            LastWarningDay = day;
            LastResult = "Эмиссаров не хватает: эффективность всех миссий снижена.";
        }
    }

    public WorldDiplomacySnapshot Capture() => new(
        _producedPoints, LastWarningDay, Missions);

    public void Restore(WorldDiplomacySnapshot snapshot)
    {
        _producedPoints = Math.Max(0, snapshot.ProducedPoints);
        LastWarningDay = snapshot.LastWarningDay;
        _missions.Clear();
        foreach (var mission in snapshot.Missions)
            if (mission.Points > 0 && ValidTarget(mission.Kind, mission.TargetId))
                _missions[(mission.Kind, mission.TargetId)] =
                    Math.Min(MaximumPointsPerMission, mission.Points);
    }

    private bool ValidTarget(EmissaryMissionKind kind, int targetId)
    {
        if (kind == EmissaryMissionKind.SupportRegion)
        {
            var region = _world.Region(targetId);
            return region is not null && region.Habitable &&
                   region.OwnerFactionId != _world.PlayerFactionId;
        }
        return targetId != _world.PlayerFactionId && _world.Faction(targetId) is not null;
    }

    private void RemoveInvalidMissions()
    {
        foreach (var key in _missions.Keys.Where(key => !ValidTarget(key.Kind, key.TargetId)).ToArray())
            _missions.Remove(key);
    }

    private bool Fail(string result)
    {
        LastResult = result;
        return false;
    }

    private static (double Opinion, double Trust) RequiredStanding(DiplomacyStance stance) => stance switch
    {
        DiplomacyStance.Trade => (-0.25, 0.10),
        DiplomacyStance.Pact => (0.25, 0.35),
        DiplomacyStance.Allied => (0.55, 0.60),
        DiplomacyStance.Vassal or DiplomacyStance.Overlord => (0.75, 0.75),
        _ => (-1, 0)
    };
}
