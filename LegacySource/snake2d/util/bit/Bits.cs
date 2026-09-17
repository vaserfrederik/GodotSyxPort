using System;

namespace Snake2D.Util.Bit
{
    public class Bits
    {
        public readonly int Scroll;
        public readonly int Mask;

        public Bits(int mask)
        {
            this.Scroll = BitOperations.TrailingZeroCount((uint)mask);
            long m = mask & 0x0FFFFFFFF;
            m = m >> this.Scroll;

            this.Mask = (int)m;
        }

        public int Set(int data, int value)
        {
            if ((value & ~Mask) != 0)
                throw new RuntimeException("" + value);

            value = value << Scroll;
            data &= ~(Mask << Scroll);
            data |= value;
            return data;
        }

        public int Get(int data)
        {
            return (data >> Scroll) & Mask;
        }

        public int Inc(int data, int inc)
        {
            int a = Get(data) + inc;
            if (a < 0)
                a = 0;
            if (a > Mask)
                a = Mask;
            return Set(data, a);
        }

        public bool IsMaximum(int data)
        {
            return Get(data) == Mask;
        }

        public static int GetDistance(int a, int b, int mask)
        {
            a &= mask;
            b &= mask;

            if (b >= a)
                return b - a;
            else
                return mask - a + b;
        }

        public static double GetDistanceD(int a, int b, int mask)
        {
            return (double)GetDistance(a, b, mask) / mask;
        }
    }
}