using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using util;
using world;

namespace world.map.terrain
{
    class GeneratorSeasoner
    {
        private double heightMul = 0.5;
        private double climateMul = 0.2;

        public GeneratorSeasoner(WORLD m, double value, float[][] climate)
        {
            HeightMap height = new HeightMap(TWIDTH(), THEIGHT(), TWIDTH() / 8, 4);
            double equator = THEIGHT() * value;
            double dSouth = THEIGHT() - equator;

            foreach (COORDINATE c in TBOUNDS())
            {
                double d = 0;
                if (c.y() <= equator)
                {
                    d = c.y() / equator;
                    d = d * (1.0 - heightMul) + heightMul * (1.0 - height.get(c));

                    double cl = c.y() / equator;
                    cl = cl * (1.0 - climateMul) + climateMul * (1.0 - height.get(c));

                    cl *= RND.rFloat1(0.3);

                    if (cl < 0.4)
                    {
                        CLIMATE().setter.set(c, CLIMATES.COLD());
                    }
                    else
                    {
                        CLIMATE().setter.set(c, CLIMATES.TEMP());
                    }
                }
                else
                {
                    d = 1.0 - (c.y() - equator) / dSouth;
                    d = d * (1.0 - heightMul) + heightMul * (1.0 - height.get(c));

                    double cl = 1.0 - (c.y() - equator) / dSouth;
                    cl = cl * (1.0 - climateMul) + climateMul * (1.0 - height.get(c));
                    cl *= RND.rFloat1(0.3);
                    if (cl < 0.3)
                    {
                        CLIMATE().setter.set(c, CLIMATES.HOT());
                    }
                    else
                    {
                        CLIMATE().setter.set(c, CLIMATES.TEMP());
                    }
                }
                climate[c.y()][c.x()] = (float)d + RND.rFloat0(0.05);
            }

            GUTIL.flooder().init(this);

            double waterBonus = 1.0;
            double waterSpread = 8.0;

            foreach (COORDINATE c in TBOUNDS())
            {
                if (WATER().has.is(c.x(), c.y()))
                {
                    if (WATER().fertile.is(c.x(), c.y()))
                    {
                        GUTIL.flooder().pushSloppy(c, waterBonus * waterSpread);
                    }
                    else
                    {
                        GUTIL.flooder().pushSloppy(c, waterBonus * waterSpread * 0.85);
                    }
                }
            }

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollGreatest();
                double bonus = t.getValue() / waterSpread;
                climate[t.y()][t.x()] += 0.7 * heightMul * bonus * bonus;

                foreach (DIR d in DIR.ALL)
                {
                    double v = t.getValue() - d.tileDistance();
                    if (v > 0 && IN_BOUNDS(t.x(), t.y(), d))
                    {
                        GUTIL.flooder().pushGreater(t, d, v);
                    }
                }
            }

            GUTIL.flooder().done();

            double mValue = 0.6;
            for (int y = 0; y < THEIGHT(); y++)
            {
                double mountain = 0;
                for (int x = 0; x < TWIDTH(); x++)
                {
                    if (MOUNTAIN().is(x, y))
                    {
                        mountain = CLAMP.d(mountain + 2, 0, 10);
                    }
                    if (mountain > 0)
                    {
                        double d = Math.Pow(mountain / 10, 1.5);
                        climate[y][x] -= mValue * d;
                        mountain = CLAMP.d(mountain - 1, 0, 10);
                    }
                }
                mountain = 0;
                for (int x = TWIDTH() - 1; x >= 0; x--)
                {
                    if (MOUNTAIN().is(x, y))
                    {
                        mountain = CLAMP.d(mountain + 2, 0, 10);
                    }
                    if (mountain > 0)
                    {
                        double d = Math.Pow(mountain / 10, 1.5);
                        climate[y][x] += mValue * d;
                        mountain = CLAMP.d(mountain - 1, 0, 10);
                    }
                }
            }

            double highest = 0;

            foreach (COORDINATE c in TBOUNDS())
            {
                double d = climate[c.y()][c.x()];
                if (d > highest)
                    highest = d;
            }

            foreach (COORDINATE c in TBOUNDS())
            {
                double d = climate[c.y()][c.x()] / highest;
                d *= 1.25;
                d = CLAMP.d(d, 0, 1);

                d -= 0.20;
                d /= 0.8;
                d *= 2.5;

                int i = (int)Math.Round((GROUND().all().size() - 1) * (1.0 - d));
                i = CLAMP.i(i, 0, GROUND().all().size() - 1);
                GROUND().all().get(i).placeRaw(c.x(), c.y());
            }

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    GROUND().getter.get(x, y).place(x, y, null, null);
                }
            }

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    GROUND().getter.get(x, y).place(x, y, null, null);
                }
            }

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    GROUND().getter.get(x, y).place(x, y, null, null);
                }
            }

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    GROUND().getter.get(x, y).place(x, y, null, null);
                }
            }
        }
    }
}