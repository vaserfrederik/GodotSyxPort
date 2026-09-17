using GodotSyxPort.Data;

namespace GodotSyxPort.Citizens;

public enum ChildActivity : byte { None, Sleeping, Playing, Nursery, School, Leaving }

/// <summary>Priority/time rules from AIModule_Child without its sprite animation layer.</summary>
public static class ChildBehaviorRuntime
{
    public const double ChildDayStart = 8.0 / 24.0;
    public const double ChildDayLength = 12.0 / 24.0;
    public const int PlaySearchDistance = 64;

    public static bool IsChild(HumanoidType type) =>
        type is HumanoidType.Child or HumanoidType.ChildSlave;

    public static bool IsNight(double dayPart) => dayPart is < 0.25 or > 0.75;

    public static bool IsChildWorkTime(double dayPart) =>
        dayPart >= ChildDayStart && dayPart < ChildDayStart + ChildDayLength;

    public static ChildActivity Select(
        RaceRule race, int ageDays, double dayPart, bool hungry, bool exposed,
        bool emigrating, bool canAttendSchool, bool nurseryAvailable)
    {
        if (emigrating) return ChildActivity.Leaving;
        if (race.Sleeps && (IsNight(dayPart) || hungry || exposed)) return ChildActivity.Sleeping;
        if (!IsChildWorkTime(dayPart)) return ChildActivity.Playing;
        if (ageDays >= race.BabyDays + race.ChildDays && canAttendSchool)
            return ChildActivity.School;
        if (ageDays < race.BabyDays + race.ChildDays && nurseryAvailable)
            return ChildActivity.Nursery;
        return ChildActivity.Playing;
    }
}
