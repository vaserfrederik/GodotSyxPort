using System;
using System.Drawing;
using System.Linq;

namespace World.Map.Regions.Centre
{
    class Sparks
    {
        private static Sparks self = new Sparks();
        private const int AMOUNT = 256;
        private const int length = 128;
        private const double duration = 10;
        private static readonly double ticksPerTime = length / duration;
        private static readonly Color[] colors = new Color[AMOUNT];
        private static readonly byte[][] xs = new byte[length][];
        private static readonly byte[][] ys = new byte[length][];
        private static readonly byte[][] op = new byte[length][];
        private static readonly Random RND = new Random();

        static Sparks()
        {
            for (int i = 0; i < AMOUNT; i++)
            {
                colors[i] = Color.FromArgb(60 + RND.Next(20), 90 + RND.Next(20), 20 + RND.Next(10)).AdjustBrightness(1.2 - (float)RND.NextDouble());
            }

            const int aniLength = 25;

            for (int a = 0; a < AMOUNT; a++)
            {
                xs[a] = new byte[length];
                ys[a] = new byte[length];
                op[a] = new byte[length];

                int current = RND.Next(length);
                int tickCount = aniLength;
                double y = RND.Next(4 * C.SCALE);
                double x = RND.Next(4 * C.SCALE);

                double dvx = -0.05 * (1.25 * C.TILE_SIZE + RND.NextDouble() * C.TILE_SIZE / 4);
                double dvy = -0.05 * (1.25 * C.TILE_SIZE + RND.NextDouble() * C.TILE_SIZE / 4);

                double xsin = RND.NextDouble();
                double ysin = RND.NextDouble();
                double dsin = RND.NextDouble() / length;

                for (int t = 0; t < length; t++)
                {
                    tickCount--;
                    if (tickCount < 0)
                    {
                        tickCount = aniLength;
                        y = RND.Next(4 * C.SCALE);
                        x = RND.Next(4 * C.SCALE);
                    }

                    current = current % length;

                    op[current][a] = (byte)(255.0 * (1.0 - t / (double)length));
                    xs[current][a] = (byte)x;
                    ys[current][a] = (byte)y;
                    x += dvx * Math.Sin(xsin);
                    y += dvy * Math.Sin(ysin);
                    xsin += dsin;
                    ysin += dsin;
                    current++;
                }
            }
        }

        public static void Render(int x, int y, int amount, int ran)
        {
            int t = ((int)((VIEW.RenderSecond() + ran / duration) * ticksPerTime)) & (length - 1);

            for (int i = 0; i < amount; i++)
            {
                colors[(ran + i) & (AMOUNT - 1)].Bind();
                CORE.Renderer().RenderParticle(x + (int)xs[t][i], y + (int)ys[t][i]);
            }
            Color.Unbind();
            OPACITY.Unbind();
        }
    }
}