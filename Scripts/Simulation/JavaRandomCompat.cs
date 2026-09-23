using System;

namespace GodotSyxPort.Simulation;

/// <summary>Java's 48-bit java.util.Random sequence for reference scenarios.</summary>
public sealed class JavaRandomCompat
{
    private const ulong Multiplier = 0x5DEECE66DUL;
    private const ulong Mask = (1UL << 48) - 1;
    private ulong _state;

    public JavaRandomCompat(int seed) => SetSeed(seed);

    public void SetSeed(int seed) => _state = (unchecked((ulong)(long)seed) ^ Multiplier) & Mask;

    private int NextBits(int count)
    {
        _state = (_state * Multiplier + 11UL) & Mask;
        return unchecked((int)(_state >> (48 - count)));
    }

    public int NextInt() => NextBits(32);
    public bool NextBoolean() => NextBits(1) != 0;
    public float NextFloat() => NextBits(24) / (float)(1 << 24);
    public long NextLong() => unchecked(((long)NextBits(32) << 32) + NextBits(32));

    public int NextInt(int bound)
    {
        if (bound <= 0) throw new ArgumentOutOfRangeException(nameof(bound));
        if ((bound & -bound) == bound)
            return (int)((bound * (long)NextBits(31)) >> 31);
        int bits, value;
        do
        {
            bits = NextBits(31);
            value = bits % bound;
        } while (unchecked(bits - value + bound - 1) < 0);
        return value;
    }
}
