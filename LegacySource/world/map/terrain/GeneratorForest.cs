using System;
using static World.WORLD;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace world.map.terrain
{
    class GeneratorForest
    {
        private readonly HeightMap noise = new HeightMap(TWIDTH(), THEIGHT(), 16, 4);
        private readonly HeightMap bigNoise = new HeightMap(TWIDTH(), THEIGHT(), 64, 64);

        private double tres = 1.0 - Config.world().FOREST_AMOUNT;

        public GeneratorForest(float[][] climate)
        {
            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    FOREST().amount.set(x, y, 0);
                    if (FOREST().placable.is(x, y))
                        if (WORLD.MOISTURE().get(x, y) >= 0.15)
                            place(x, y, climate[y][x]);
                }
            }
        }

        private void place(int tx, int ty, double c)
        {
            double n = noise.get(tx, ty) * (0.35 + 0.65 * bigNoise.get(tx, ty));
            if (n > 0.8)
                n = 0.8;

            if (WATER().RIVER.is(tx, ty))
            {
                n += n * 0.050;
            }
            double f = 0;
            for (int i = 0; i < DIR.ALL.size(); i++)
            {
                DIR d = DIR.ALL.get(i);
                if (WATER().RIVER.is(tx, ty, d) || WATER().LAKE.normal.is(tx, ty, d))
                    n += 0.010;
                f += WORLD.MOISTURE().get(tx + d.x(), ty + d.y());
            }
            f /= DIR.ALL.size();
            n += 0.2 * f;

            n *= 0.5 + c * 1.2;
            n = Math.Pow(n, 1.5);

            if (MOUNTAIN().is(tx, ty))
            {
                double h = MOUNTAIN().heighter.get(tx, ty);
                if (h > 3)
                    return;
                h = 1.0 - h / 3;
                n -= 0.3 * h;
            }

            if (n > tres)
            {
                n -= tres;
                n *= 1.0 / (1.0 - tres);
                float r = RND.rFloat();
                n += RND.rBoolean() ? 0.1 * r : -0.1 * r;
                if (n > 1)
                    n = 1;
                if (MOUNTAIN().is(tx, ty))
                {
                    double h = MOUNTAIN().heighter.get(tx, ty);
                    if (h > 3)
                        return;
                    n *= 1.0 - h / 3;
                }

                FOREST().amount.set(tx, ty, n);
            }
            else
            {
                float r = RND.rFloat();
                r *= r;
                n += RND.rBoolean() ? 0.3 * r : -0.3 * r;
                if (n > 0.6)
                {
                    n -= 0.6;
                    n *= 1.66;
                    n -= 0.75 - WORLD.MOISTURE().get(tx, ty);
                    n *= RND.rFloat() * RND.rFloat();

                    FOREST().amount.set(tx, ty, n);
                }
            }
        }
    }
}