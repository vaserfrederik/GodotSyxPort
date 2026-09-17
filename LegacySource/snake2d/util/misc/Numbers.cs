using System;
using System.Text;

namespace snake2d.util.misc
{
    public static class Numbers
    {
        public static string GetSuffix(int nr)
        {
            nr = Math.Abs(nr);

            if (nr == 1)
                return "1st";

            if (nr == 2)
                return "2nd";

            if (nr == 3)
                return "3rd";

            return nr.ToString() + "th";
        }

        public static void PrintBits(byte b)
        {
            PrintBits((ulong)b, 8);
        }

        public static void PrintBits(long l)
        {
            PrintBits((ulong)l, 64);
        }

        private static void PrintBits(ulong l, int bits)
        {
            StringBuilder s = new StringBuilder(bits);
            for (; bits > 0; bits--)
            {
                s.Append((l & 0b001) == 0b001 ? '1' : '0');
                l = l >> 1;
            }
            s.Reverse();
        }
    }
}