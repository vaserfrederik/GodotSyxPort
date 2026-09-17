using System;
using System.IO;

namespace snake2d.util.sets
{
    /**
     * dear mortal. This is not your ordinary bitmap. This is a bit(s)map. That's right.
     * How many bits you want? Up to you. Took me two days to get this right...
     * @author mail__000
     *
     */
    [Serializable]
    public class Bitsmap1D : SAVABLE
    {
        /**
         * 
         */
        private static readonly long serialVersionUID = 1L;
        private readonly long[] bits;
        public readonly int outof;
        private readonly int max;
        private readonly int stride;
        private readonly long mask;

        /**
         * 
         * @param outof - value to return if index is out of bounds
         * @param bits - the number of bits you want to store a value. Will be positive. Should be < 32 
         * @param amount - how many of these memory chunks do you want, huh?
         */
        public Bitsmap1D(int outof, int bits, int amount)
        {
            this.bits = new long[(int)Math.Ceiling(bits * amount / 64.0)];
            this.outof = outof;
            max = amount;
            stride = bits;
            mask = (1 << bits) - 1;
        }

        public override void save(FilePutter fp)
        {
            fp.ls(bits);
        }

        public override void load(FileGetter fp) => fp.ls(bits);

        /**
         * 
         * @param index - your index
         * @return - your value, or outof value if out of bounds
         */
        public int get(int index)
        {
            if (index < 0 || index >= max)
                return outof;

            int i = index * stride;

            int l1 = i >> 6;
            int ls = 64 - stride - (i & 63);

            if (ls >= 0)
                return (int)((bits[l1] >> (ls)) & mask);

            long v = bits[l1] << -ls;
            ls += 64;
            v |= bits[l1 + 1] >> ls;

            return (int)(v & mask);
        }

        public override void clear()
        {
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = 0;
            }
        }

        /**
         * 
         * @param index
         * @param value
         */
        public void set(int index, int value)
        {
            long v = value;
            v &= mask;
            int i = index * stride;

            int l1 = i >> 6;
            int ls = 64 - stride - (i & 63);

            if (ls >= 0)
            {
                bits[l1] &= ~(mask << ls);
                bits[l1] |= v << ls;
            }
            else
            {
                bits[l1] &= ~(mask >> -ls);
                bits[l1] |= v >> -ls;

                ls += 64;

                bits[l1 + 1] &= ~(mask << ls);
                bits[l1 + 1] |= v << ls;
            }
        }

        public void inc(int index, int delta) => set(index, get(index) + delta);

        public void setAll(int value)
        {
            clear();
            for (int i = 0; i < max; i++)
            {
                set(i, value);
            }
        }

        public int maxIndex() => max;

        public int maxValue() => (int)mask;
    }
}