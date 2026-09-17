using System;

namespace GodotSyxPort.Industry;

/// <summary>
/// Compile-safe semantic core of converted IndustryResource/IndustryUtil.
/// It keeps fractional room progress and returns only crossed whole units.
/// </summary>
public static class IndustryRuntime
{
    public static double RoomBonus(double degradation, double employmentEfficiency)
    {
        var degrade = Math.Clamp(degradation, 0.0, 1.0);
        var employment = Math.Max(0.0, employmentEfficiency);
        return (1.0 - 0.75 * degrade) * employment;
    }

    public static int Advance(ref double progress, double sourceRate, double workFactor, double multiplier)
    {
        if (!double.IsFinite(progress)) progress = 0;
        if (!double.IsFinite(sourceRate) || !double.IsFinite(workFactor) || !double.IsFinite(multiplier))
            return 0;
        var before = (int)progress;
        progress += Math.Max(0.0, sourceRate * workFactor * multiplier);
        return (int)progress - before;
    }

    public static int PreviewAdvance(
        double progress, double sourceRate, double workFactor, double multiplier)
    {
        if (!double.IsFinite(progress) || !double.IsFinite(sourceRate) ||
            !double.IsFinite(workFactor) || !double.IsFinite(multiplier)) return 0;
        return (int)(progress + Math.Max(0.0, sourceRate * workFactor * multiplier)) -
               (int)progress;
    }
}
