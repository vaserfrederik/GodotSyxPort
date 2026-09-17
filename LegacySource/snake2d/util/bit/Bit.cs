using System;

namespace Snake2D.Util.Bit
{
    public class Bit
    {
        public readonly int mask;

        public Bit(int mask)
        {
            this.mask = mask;
            if (mask == 0 || ((mask - 1) & mask) != 0)
                throw new Exception();
        }

        public bool Is(int data)
        {
            return (data & mask) != 0;
        }

        public int Set(int data)
        {
            return data | mask;
        }

        public int Set(int data, bool b)
        {
            if (b)
                return Set(data);
            return Clear(data);
        }

        public int Clear(int data)
        {
            return data & ~mask;
        }
    }
}