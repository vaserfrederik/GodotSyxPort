using System;
using System.IO;

namespace World.Battle
{
    public class Rnd : SAVABLE
    {
        private static readonly int[] rnds = new int[2024];
        private static int ri = 0;

        public static double F()
        {
            double d = Inc() & int.MaxValue;
            return d / int.MaxValue;
        }

        public static int I()
        {
            return Inc();
        }

        public static int I(int max)
        {
            return MATH.Mod(Inc(), max);
        }

        public static bool OneIn(int am)
        {
            int d = MATH.Mod(Inc(), am);
            return d == 0;
        }

        private static int Inc()
        {
            int r = rnds[ri];
            rnds[ri] = RND.RInt();
            ri++;
            if (ri >= rnds.Length)
                ri = 0;
            return r;
        }

        public override void Save(FilePutter file)
        {
            file.Is(rnds);
        }

        public override void Load(FileGetter file)
        {
            file.Is(rnds);
        }

        public override void Clear()
        {
            for (int i = 0; i < rnds.Length; i++)
                rnds[i] = RND.RInt();
            ri = 0;
        }
    }
}