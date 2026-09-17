using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Maintenance
{
    using Init.Resources;
    using Settlement.Main;
    using Settlement.Room.Food.Fish;
    using Settlement.Room.Industry.Mine;
    using Settlement.Room.Industry.Refiner;
    using Settlement.Room.Industry.Workshop;
    using Settlement.Room.Main;
    using Snake2D.Util.Sets;

    class Test
    {
        private double[] acc = new double[RESOURCES.ALL().Count];

        private readonly double rate = 1.0 / 64.0;
        private readonly Bitmap1D check = new Bitmap1D(SETT.ROOMS().AMOUNT_OF_BLUEPRINTS, false);

        public Test()
        {
            acc.Fill(double.MaxValue);
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                double v = SETT.RECIPES().ratesV.bestRecipe(res.tr()).manpowerTotal();
                acc[res.index()] = v;
                Log(res.key + "\t" + dd(acc[res.index()]));
            }

            Log("");

            {
                foreach (ROOM_MINE m in SETT.ROOMS().MINES)
                {
                    double[][] groups = new double[][]
                    {
                        new double[] { 1, (1.0 / m.constructor().groups().get(1).stat(2)) }
                    };

                    print(m, groups);
                }
            }
            Log("");

            {
                foreach (ROOM_REFINER m in SETT.ROOMS().REFINERS)
                {
                    double[][] groups = new double[][]
                    {
                        new double[] { 0, 1.0 / m.constructor().groups().get(0).stat(0) },
                        new double[] { 2, 1.0 / m.constructor().groups().get(2).stat(1) }
                    };

                    print(m, groups);
                }
            }
            Log("");

            {
                foreach (ROOM_WORKSHOP m in SETT.ROOMS().WORKSHOPS)
                {
                    double[][] groups = new double[][]
                    {
                        new double[] { 1, 1.0 / m.constructor().groups().get(1).stat(0) },
                        new double[] { 2, 1.0 / m.constructor().groups().get(2).stat(1) }
                    };

                    print(m, groups);
                }
            }
            Log("");

            {
                foreach (ROOM_FISHERY m in SETT.ROOMS().FISHERIES)
                {
                    double[][] groups = new double[][]
                    {
                        new double[] { 1, 1.0 / m.constructor().groups().get(1).stat(2) }
                    };

                    print(m, groups);
                }
            }
            Log("");

            {
                RoomBlueprintIns<?> m = SETT.ROOMS().WOOD_CUTTER;
                double[][] groups = new double[][]
                {
                    new double[] { 1, 1.0 / m.constructor().groups().get(1).stat(1) }
                };
                print(m, groups);
            }

            foreach (RoomBlueprint bb in SETT.ROOMS().all())
            {
                if (check.get(bb.index()))
                    continue;
                if (bb is RoomBlueprintImp)
                {
                    RoomBlueprintImp b = (RoomBlueprintImp)bb;

                    double max = 0;
                    foreach (FurnisherItemGroup i in b.constructor().groups())
                    {
                        for (int uI = 0; uI <= b.upgrades().max(); uI++)
                        {
                            double m = 0;
                            for (int ri = 0; ri < b.constructor().resources(); ri++)
                            {
                                m += i.cost(ri, ri) * acc[b.constructor().resource(ri).index()];
                            }
                            m /= i.item(0, 0).area;
                            max = Math.Max(max, m);
                        }
                    }
                    for (int uI = 0; uI <= b.upgrades().max(); uI++)
                    {
                        double m = 0;
                        for (int ri = 0; ri < b.constructor().resources(); ri++)
                        {
                            m += b.constructor().areaCost(ri, uI) * acc[b.constructor().resource(ri).index()];
                        }
                        max = Math.Max(max, m);
                    }
                    Log(b.key + " " + max * b.degradeRate());
                }
            }
        }

        private void print(RoomBlueprintIns<?> m, double[][] groups)
        {
            check.set(m.index(), true);
            Log(m.key + " " + m.degradeRate());
            string res = "";
            for (int i = 0; i < m.constructor().resources(); i++)
            {
                res += m.constructor().resource(i) + " "
                        + dd((acc[m.constructor().resource(i).index()])) + " | ";
            }
            Log("    res: " + res);

            res = "";
            foreach (double[] gi in groups)
            {
                FurnisherItemGroup g = m.constructor().groups().get((int)gi[0]);
                res += g.name + " " + gi[1] + " | ";
            }
            Log("    items: : " + res);
            double prev = 0;

            for (int i = 0; i <= m.upgrades().max(); i++)
            {
                double bo = m.upgrades().boost(i);
                if (i > 0)
                    bo -= m.upgrades().boost(i - 1);
                double mm = 0;
                foreach (double[] gi in groups)
                {
                    FurnisherItemGroup g = m.constructor().groups().get((int)gi[0]);

                    for (int ri = 0; ri < m.constructor().resources(); ri++)
                    {
                        mm += g.cost(ri, i) * gi[1] * acc[m.constructor().resource(ri).index()];
                    }
                }

                mm *= m.degradeRate();
                mm -= prev;
                prev += mm;

                if (i == 0)
                    bo = 1;
                double d = mm / bo;

                Log("  #" + i + ": " + dd(100 * d * rate) + "%");
            }
        }

        private static void Log(string s)
        {
            LOG.err(s);
        }

        private static string dd(double d)
        {
            return string.Format("{0:.2f}", d);
        }
    }
}