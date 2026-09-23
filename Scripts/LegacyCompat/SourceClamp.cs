namespace GodotSyxPort.LegacyCompat;

/// <summary>
/// Compile-safe semantic port of snake2d.util.misc.CLAMP. The generated
/// LegacySource copy cannot compile because its static class has a constructor.
/// </summary>
public static class SourceClamp
{
    public static int Integer(int value, int minimum, int maximum)
    {
        if (value < minimum) return minimum;
        if (value > maximum) return maximum;
        return value;
    }

    public static sbyte Byte(sbyte value, int minimum, int maximum)
    {
        if (value < minimum) return unchecked((sbyte)minimum);
        if (value > maximum) return unchecked((sbyte)maximum);
        return value;
    }

    public static double Double(double value, double minimum, double maximum)
    {
        if (double.IsNaN(value)) return 0;
        if (double.IsNegativeInfinity(value)) return minimum;
        if (double.IsPositiveInfinity(value)) return maximum;
        if (value < minimum) return minimum;
        if (value > maximum) return maximum;
        return value;
    }

    public static double Cycle(double value, double maximum)
    {
        if (value < maximum) return value;
        if (value <= maximum) return value;
        var remainder = value % maximum;
        var section = (int)(value / maximum);
        return (section & 1) == 1 ? maximum - remainder : remainder;
    }
}
