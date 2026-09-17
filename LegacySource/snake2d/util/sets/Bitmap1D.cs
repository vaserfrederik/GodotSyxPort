using System;
using System.IO;

namespace snake2d.util.sets
{
    public class Bitmap1D : SAVABLE
    {
        private readonly int[] bits;
        private readonly int max;
        private static readonly int[] masks = new int[32];
        private static readonly int[] imasks = new int[32];
        private readonly bool outof;

        static Bitmap1D()
        {
            int m = 1;
            for (int i = 0; i < 32; i++)
            {
                masks[i] = m;
                imasks[i] = ~m;
                m = m << 1;
            }
        }

        public override void save(FilePutter fp)
        {
            fp.isE(bits);
        }

        public override void load(FileGetter fp)
        {
            fp.isE(bits);
        }

        public Bitmap1D(int size, bool outof)
        {
            max = size;
            int l = size / 32;
            if (size % 32 != 0)
                l++;
            bits = Alloc.ii(l);
            this.outof = outof;
        }

        public bool get(int bit)
        {
            if (bit < 0 || bit >= max)
                return outof;

            int m = bit & 0x0000001F;
            int i = bit >> 5;

            return (bits[i] & masks[m]) == masks[m];
        }

        public void setTrue(int bit)
        {
            if (bit < 0 || bit >= max)
                return;

            int m = bit & 0x0000001F;
            int i = bit >> 5;
            bits[i] |= masks[m];
        }

        public void setFalse(int bit)
        {
            if (bit < 0 || bit >= max)
                return;

            int m = bit & 0x0000001F;
            int i = bit >> 5;
            bits[i] &= imasks[m];
        }

        public void set(int bit, bool boolValue)
        {
            if (boolValue)
                setTrue(bit);
            else
                setFalse(bit);
        }

        public void toggle(int bit)
        {
            set(bit, !get(bit));
        }

        public override void clear()
        {
            for (int i = 0; i < bits.Length; i++)
                bits[i] = 0;
        }

        public int size()
        {
            return max;
        }

        public void setAll(bool b)
        {
            int k = b ? -1 : 0;
            for (int i = 0; i < bits.Length; i++)
                bits[i] = k;
        }
    }
}