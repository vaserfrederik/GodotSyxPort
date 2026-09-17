using System;
using System.Drawing;

namespace Settlement.Thing.Pointlight
{
    public class FireSparks
    {
        private static FireSparks self = new FireSparks();
        private const int AMOUNT = 256;
        private const int length = 128;
        private const double duration = 5;
        private static readonly double ticksPerTime = length / duration;
        private readonly Color[] colors = new Color[AMOUNT];
        private readonly byte[][] xs = new byte[length][];
        private readonly byte[][] ys = new byte[length][];
        private double ani = Random.Shared.NextDouble();

        private FireSparks()
        {
            for (int i = 0; i < AMOUNT; i++)
            {
                colors[i] = Color.FromArgb(
                    110 + Random.Shared.Next(20),
                    80 + Random.Shared.Next(10),
                    20 + Random.Shared.Next(10)
                ).Lighter(1.2 - Random.Shared.NextDouble());
            }

            const int aniLength = 25;

            for (int a = 0; a < AMOUNT; a++)
            {
                int current = Random.Shared.Next(length);
                int tickCount = aniLength;
                double y = Random.Shared.Next(4 * C.SCALE);
                double x = Random.Shared.Next(4 * C.SCALE);

                double dvx = -0.05 * (1.25 * C.TILE_SIZE + Random.Shared.NextDouble() * C.TILE_SIZE / 4);
                double dvy = 0.05 * (1.25 * C.TILE_SIZE + Random.Shared.NextDouble() * C.TILE_SIZE / 4);

                double xsin = Random.Shared.NextDouble();
                double ysin = Random.Shared.NextDouble();
                double dsin = Random.Shared.NextDouble() / length;

                for (int t = 0; t < length; t++)
                {
                    tickCount--;
                    if (tickCount < 0)
                    {
                        tickCount = aniLength;
                        y = Random.Shared.Next(4 * C.SCALE);
                        x = Random.Shared.Next(4 * C.SCALE);
                    }

                    current = current % length;

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

        public static void Update(float ds)
        {
            self.ani += ds * ticksPerTime;
        }

        public static void Render(int x, int y, int amount, int ran, double wind)
        {
            Render(self.ani, x, y, amount, ran, wind);
        }

        public static void Render(double ani, int x, int y, int amount, int ran, double wind)
        {
            int t = ((int)ani) & (length - 1);

            double d = 0.5 + 0.5 * wind;

            for (int i = 0; i < amount; i++)
            {
                Color color = self.colors[(ran + i) & (AMOUNT - 1)];
                CORE.Renderer().RenderParticle(x + (int)(self.xs[t][i] * d), y + (int)(self.ys[t][i] * d), color);
            }
        }
    }
}