using System;

namespace GodotSyxPort.Citizens;

public enum HomeActivity : byte
{
    None,
    WalkingHome,
    SleepingAtHome,
    SleepingOutside,
    StayingHome
}

/// <summary>
/// Timing and state selection shared by AIModule_Home's house, chamber and ground plans.
/// Rendering poses and curfew overrides remain consumers of the same state.
/// </summary>
public static class HomeBehaviorRuntime
{
    public const int GroundSearchDistance = 64;
    public const int CurfewGroundSearchDistance = 256;
    public const double SleepActionSeconds = 8.0;

    public static bool IsDay(double dayPart) => dayPart >= 0.25 && dayPart < 0.75;

    public static bool ShouldVisitHome(
        bool sleeps, bool hasHome, bool hasSleptToday, int ageDays,
        int citizenId, int day, double dayPart)
    {
        if (!sleeps) return hasHome && !hasSleptToday;
        if (!hasHome) return !hasSleptToday;
        var homeSitter = IsDay(dayPart) ? (ageDays & 3) != 0 : (ageDays & 1) == 0;
        if (homeSitter) return true;
        var staggeredDay = ((citizenId * 31 + day) & 1) == 1;
        var hour = dayPart * 24.0;
        var distanceFromDayBoundary = Math.Min(hour, 24.0 - hour);
        return staggeredDay && distanceFromDayBoundary > 3.0;
    }

    public static double StaySeconds(int citizenId, int day, bool noble) =>
        SleepActionSeconds * (noble ? 8 : 5 + Math.Abs(citizenId * 17 + day * 13) % 5);
}
