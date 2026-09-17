using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Recipe
{
    public class RecipeRatesPlayer
    {
        private readonly Dictionary<int, int> mapBo = new Dictionary<int, int>();
        private readonly List<Data> boData = new List<Data>();

        private readonly double[] best;
        private readonly double[] bestTotal;
        private readonly Recipe[] bestR;
        private readonly double[] rateTot;
        int tt = 0;

        private int rateI = -1;
        private readonly Recipes recp;

        public RecipeRatesPlayer(Recipes recp)
        {
            rateTot = new double[recp.All().Count];
            this.recp = recp;

            foreach (Industry ins in SETT.ROOMS().Industries.All)
            {
                if (ins.Bonus != null)
                {
                    mapBo[ins.Bonus.Index()] = boData.Count;
                    boData.Add(new Data());
                }

                if (ins.ConBonus != null)
                {
                    mapBo[ins.ConBonus.Index()] = boData.Count;
                    boData.Add(new Data());
                }
            }
        }

        public double RateTotal(TRADABLE res)
        {
            Calc();
            return bestTotal[res.Index()];
        }

        public double Rate(TRADABLE res)
        {
            Calc();
            return best[res.Index()];
        }

        double RateTotal(Recipe res)
        {
            Calc();
            return rateTot[res.Index()];
        }

        public Recipe BestRecipe(TRADABLE res)
        {
            Calc();
            return bestR[res.Index()];
        }

        private readonly SAVABLE saver = new SAVABLE()
        {
            Save = file =>
            {
                file.Write(boData.Count);
                foreach (Data d in boData)
                {
                    file.Write(d.prev);
                }
            },
            Load = file =>
            {
                int am = file.ReadInt();
                for (int i = 0; i < am; i++)
                    boData[i].prev = file.ReadDouble();
                tt = 0;
            },
            Clear = () =>
            {
                foreach (Data d in boData)
                {
                    d.prev = -1;
                    d.am = 0;
                    d.mul = 0;
                }
                tt = 0;
            }
        };

        private void Calc()
        {
            if (rateI == GAME.UpdateI())
                return;
            rateI = GAME.UpdateI();

            {
                ENTITY[] ee = SETT.ENTITIES().GetAllEnts();

                for (int i = 0; i < 256; i++)
                {
                    if (tt >= ee.Length)
                    {
                        foreach (Data d in boData)
                        {
                            d.Calc();
                        }
                        tt = 0;
                    }
                    else
                    {
                        ENTITY e = ee[tt++];
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            RoomInstance ww = STATS.WORK().EMPLOYED.Get(a);
                            if (ww != null && ww is ROOM_PRODUCER_INSTANCE)
                            {
                                Industry ii = ((ROOM_PRODUCER_INSTANCE)ww).Industry();
                                if (ii.Bonus != null)
                                {
                                    boData[mapBo[ii.Bonus.Index()]].Add(a, ww, ii);
                                }

                                if (ii.ConBonus != null)
                                {
                                    boData[mapBo[ii.ConBonus.Index()]].Add(a, ww, ii);
                                }
                            }
                        }
                    }
                }
            }

            Array.Fill(best, double.MaxValue);
            Array.Fill(bestTotal, double.MaxValue);

            for (int iii = 0; iii < recp.All().Count; iii++)
            {
                Recipe ii = recp.All()[iii];
                double r = 1.0 / (ii.Rate * Boost(ii));
                if (r < best[ii.Out.Index()])
                    best[ii.Out.Index()] = r;
            }

            for (int i = 0; i < 10; i++)
            {
                for (int iii = 0; iii < recp.All().Count; iii++)
                {
                    Recipe ii = recp.All()[iii];
                    Set2(ii);
                }
            }
        }

        private void Set2(Recipe ii)
        {
            double mm = 1.0 / (ii.Rate * Boost(ii));

            if (bestR[ii.Out.Index()] == null)
                bestR[ii.Out.Index()] = ii;

            foreach (RecipeInput i in ii.Ins)
            {
                if (bestTotal[i.Res.Index()] == double.MaxValue)
                    return;
                mm += (i.Rate) * bestTotal[i.Res.Index()] / (ii.Rate * Boost(i));
            }

            rateTot[ii.Index] = mm;

            if (mm < bestTotal[ii.Out.Index()])
            {
                bestTotal[ii.Out.Index()] = mm;
                bestR[ii.Out.Index()] = ii;
            }
        }

        private double Boost(Recipe bo)
        {
            return Boost(bo.Bo);
        }

        private double Boost(RecipeInput bo)
        {
            if (bo.Boost != null)
                return Boost(bo.Boost);
            return 1.0;
        }

        public double Boost(Boostable bo)
        {
            Calc();
            if (mapBo.ContainsKey(bo.Index()))
            {
                return boData[mapBo[bo.Index()]].Get(bo);
            }
            else
            {
                return bo.Get(HCLASS_RACE.ClP());
            }
        }

        private class Data
        {
            public double prev = -1;
            public double mul = 1;
            public int am = 1;

            public void Add(Humanoid h, RoomInstance work, Industry i)
            {
                am++;
                double mul = 1;
                if (!(work.BlueprintI() is ROOM_PASTURE))
                {
                    mul *= IndustryUtil.RoomBonus(work, i);
                }
                mul *= work.Employees().TotEfficiency();
                mul *= i.Bonus().Get(h.Indu());
                this.mul += mul;
            }

            public void AddD(Humanoid h, RoomInstance work, Industry i)
            {
                am++;
                double mul = 1;
                Console.WriteLine(work.Blueprint().Key);
                mul *= IndustryUtil.RoomBonus(work, i);
                Console.WriteLine(mul);
                mul *= work.Employees().TotEfficiency();
                Console.WriteLine(mul);
                mul *= i.Bonus().Get(h.Indu());
                Console.WriteLine(mul);
                this.mul += mul;
            }

            public double Get(Boostable bo)
            {
                if (prev < 0)
                    return bo.Get(HCLASS_RACE.ClP());
                return prev;
            }

            public void Calc()
            {
                if (am > 0)
                    prev = mul / am;
                else
                    prev = -1;
                mul = 0;
                am = 0;
            }
        }
    }
}