using System;

namespace Settlement.Room.Home.House
{
    using Settlement.Room.Main.Furnisher;
    using Snake2D.Util.DataTypes;

    class SpriteCarpet
    {
        private readonly int[][][][] data = new int[4][4][][];

        public SpriteCarpet()
        {
            data[0] = Make(
                new int[][]
                {
                    new int[] { 0x00, 0x00, 0x00 },
                    new int[] { 0x00, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10 },
                }
            );

            data[1] = Make(
                new int[][]
                {
                    new int[] { 0x00, 0x00, 0x00 },
                    new int[] { 0x00, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10 },
                    new int[] { 0x00, 0x00, 0x00 },
                }
            );

            data[2] = Make(
                new int[][]
                {
                    new int[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
                    new int[] { 0x00, 0x10, 0x10, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10, 0x10, 0x10 },
                    new int[] { 0x00, 0x10, 0x10, 0x10, 0x10 },
                    new int[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
                }
            );
        }

        private int[][][] Make(int[][] o)
        {
            int[][][] r = new int[4][][];
            
            for (int i = 0; i < 4; i++)
            {
                r[i] = o;
                o = Rotate(o);
            }

            for (int[][] is in r)
            {
                int oi = 0;
                int co = 0;
                for (int y = 0; y < is.Length; y++)
                {
                    for (int x = 0; x < is[y].Length; x++)
                    {
                        if (is[y][x] == 0)
                            continue;
                        else if (is[y][x] != oi)
                        {
                            co = 0;
                            oi = is[y][x];
                            is[y][x] |= co;
                            co++;
                        }
                    }
                }
            }

            return r;
        }

        private int[][] Rotate(int[][] l)
        {
            int M = l.Length;
            int N = l[0].Length;
            int[][] ret = new int[N][M];
            for (int r = 0; r < M; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    ret[c][M - 1 - r] = l[r][c];
                }
            }
            return ret;
        }

        public int Get(int rx, int ry, FurnisherItem it)
        {
            int[][] m = data[it.Group().Index()][it.Rotation & 1];
            if (ry < 0 || ry >= m.Length)
                return 0;
            if (rx < 0 || rx >= m[0].Length)
                return 0;
            return (m[ry][rx] >> 4) & 0x0F;
        }

        public int Get(int rx, int ry, DIR d, FurnisherItem it)
        {
            return Get(rx + d.X(), ry + d.Y(), it);
        }
    }
}