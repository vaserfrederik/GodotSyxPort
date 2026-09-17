using System;
using System.Numerics;

namespace GodotSyxPort.LegacyCompat;

/// <summary>
/// Compile-safe semantic port of snake2d.util.bit.Bits. It retains the source
/// mask layout and saturating increment behavior.
/// </summary>
public sealed class PackedBits
{
    public int Shift { get; }
    public int Mask { get; }

    public PackedBits(int mask)
    {
        if (mask == 0) throw new ArgumentOutOfRangeException(nameof(mask));
        Shift = BitOperations.TrailingZeroCount((uint)mask);
        Mask = (int)(((long)mask & 0xFFFFFFFFL) >> Shift);
    }

    public int Set(int data, int value)
    {
        if ((value & ~Mask) != 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value does not fit the mask.");
        value <<= Shift;
        data &= ~(Mask << Shift);
        return data | value;
    }

    public int Get(int data) => (data >> Shift) & Mask;

    public int Increment(int data, int increment) =>
        Set(data, SourceClamp.Integer(Get(data) + increment, 0, Mask));

    public bool IsMaximum(int data) => Get(data) == Mask;

    public static int Distance(int first, int second, int mask)
    {
        first &= mask;
        second &= mask;
        return second >= first ? second - first : mask - first + second;
    }

    public static double DistanceFraction(int first, int second, int mask) =>
        mask == 0 ? 0 : (double)Distance(first, second, mask) / mask;
}
