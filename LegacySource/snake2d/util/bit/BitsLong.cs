using System;

namespace Snake2D.Util.Bit
{
    public class BitsLong
    {
        public readonly int Scroll;
        public readonly long Mask;

        public BitsLong(long mask)
        {
            this.Scroll = (int)BitOperations.TrailingZeroCount(mask);
            long m = mask;
            m = m >> this.Scroll;

            this.Mask = m;
        }

        public long Set(long data, long value)
        {
            if (value < 0 || value > Mask)
                throw new RuntimeException(value.ToString());

            value = value << Scroll;
            data &= ~(Mask << Scroll);
            data |= value;
            return data;
        }

        public int Get(long data)
        {
            return (int)((data >> Scroll) & Mask);
        }

        public long Inc(long data, long inc)
        {
            long a = Get(data) + inc;
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
    }
}