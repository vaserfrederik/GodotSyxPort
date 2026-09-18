using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.Citizens;

/// <summary>
/// Non-rendering port of AIModules.Sorter2. Each available module reports the
/// same integer priority used by the Java AI; plans are attempted from highest
/// priority to lowest, with a per-citizen/day rotation for equal priorities.
/// </summary>
public enum CitizenAiModule : byte
{
    Food,
    Health,
    Home,
    Work,
    Service,
    Subject,
    Idle
}

public readonly record struct CitizenAiCandidate(CitizenAiModule Module, int Priority);

public static class CitizenAiModuleRuntime
{
    public const int CriticalPriority = 10;
    public const int HealthPriority = 7;
    public const int HighNeedPriority = 6;
    public const int WorkPriority = 5;
    public const int LowNeedPriority = 4;
    public const int ServicePriority = 3;
    public const int SubjectActivityPriority = 2;
    public const int HomePriority = 1;

    public static int FoodPriority(int hunger, int needChunk)
    {
        if (hunger >= needChunk * 3) return CriticalPriority;
        if (hunger >= needChunk * 2) return HighNeedPriority;
        if (hunger >= needChunk) return LowNeedPriority;
        return 0;
    }

    public static IReadOnlyList<CitizenAiModule> Order(
        int citizenId,
        int day,
        int foodPriority,
        bool healthDanger,
        int homePriority,
        bool canWork,
        bool hasServices,
        bool hasSubjectActivity)
    {
        var candidates = new List<CitizenAiCandidate>(6);
        Add(CitizenAiModule.Food, foodPriority);
        Add(CitizenAiModule.Health, healthDanger ? HealthPriority : 0);
        Add(CitizenAiModule.Home, homePriority);
        Add(CitizenAiModule.Work, canWork ? WorkPriority : 0);
        Add(CitizenAiModule.Service, hasServices ? ServicePriority : 0);
        Add(CitizenAiModule.Subject,
            hasSubjectActivity ? SubjectActivityPriority : 0);

        // Sorter2 begins at a random module offset. A stable hash gives each
        // citizen the same tie dispersion without depending on render frames.
        var rotation = StableHash(citizenId, day);
        return candidates.OrderByDescending(candidate => candidate.Priority)
            .ThenBy(candidate => TieOrder(candidate.Module, rotation))
            .Select(candidate => candidate.Module)
            .Append(CitizenAiModule.Idle)
            .ToArray();

        void Add(CitizenAiModule module, int priority)
        {
            if (priority > 0) candidates.Add(new CitizenAiCandidate(module, priority));
        }
    }

    private static int TieOrder(CitizenAiModule module, int rotation)
    {
        const int count = 6;
        return ((int)module - rotation + count) % count;
    }

    private static int StableHash(int citizenId, int day)
    {
        unchecked
        {
            var value = citizenId * 1103515245 + day * 12345;
            value ^= value >> 16;
            return Math.Abs(value % 6);
        }
    }
}
