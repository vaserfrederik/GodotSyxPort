using System;
using System.Collections.Generic;
using System.Linq;

namespace Init.Race
{
    public class RaceBoosts
    {
        private double[][] priorities;
        private double[][] skillRelative;
        private double[][] skill;

        public readonly BoostSpecs boosters = new BoostSpecs(RACES.Name(), UI.Icons().s.Citizen, true);
        private readonly KeyMap<BV> bvMap = new KeyMap<BV>();

        public RaceBoosts()
        {
            Action a = new Action(() =>
            {
                foreach (Race c in RACES.All())
                {
                    foreach (BoostSpec s in c.Boosts.All())
                    {
                        string k = s.Boostable.Key + s.Booster.IsMul;
                        if (!bvMap.ContainsKey(k))
                        {
                            bvMap.Put(k, new BV(boosters, s.Boostable, s.Booster.IsMul));
                        }
                        bvMap.Get(k).Set(c, s.Booster.To());
                    }
                }
                SetPrio();
            });
            BOOSTING.Connector(a);
        }

        public BoostSpec PushIfDoesntExist(Race c, double v, Boostable bo, bool isMul)
        {
            string k = bo.Key + isMul;
            double none = isMul ? 1 : 0;
            if (bvMap.ContainsKey(k) && bvMap.Get(k).dd[c.Index()] == none)
                return null;

            bool ret = false;
            if (!bvMap.ContainsKey(k))
            {
                ret = true;
                bvMap.Put(k, new BV(boosters, bo, isMul));
            }

            BV bv = bvMap.Get(k);
            bv.Set(c, v);
            SetPrio();
            return ret ? bv.Spec : null;
        }

        void SetPrio()
        {
            Dictionary<string, RoomEmploymentSimple> map = new Dictionary<string, RoomEmploymentSimple>();

            foreach (RoomEmploymentSimple p in SETT.ROOMS().Employment.ALLs())
            {
                if (p.Blueprint().Bonus() != null)
                {
                    map[p.Blueprint().Bonus().Key] = p;
                }
            }

            double max = 0;
            double min = 10000;

            priorities = new double[RACES.All().Count()][SETT.ROOMS().Employment.ALLs().Count()];
            foreach (Race r in RACES.All())
            {
                foreach (BoostSpec s in r.Boosts.All())
                {
                    RoomEmploymentSimple e = map.GetValueOrDefault(s.Boostable.Key);
                    if (e == null)
                        continue;
                    double v = s.Booster.IsMul ? (s.Booster.To() - 1) * 5 : s.Booster.To();
                    max = Math.Max(v, max);
                    min = Math.Min(min, v);
                }
            }

            foreach (Race r in RACES.All())
            {
                double[] vv = priorities[r.Index()];
                Array.Fill(vv, 0.5);
                if (min > max)
                {
                    continue;
                }
                foreach (RoomEmploymentSimple p in SETT.ROOMS().Employment.ALLs())
                {
                    if (p.Blueprint().Bonus() != null)
                    {
                        double v = r.BValue(p.Blueprint().Bonus());
                        v -= min;
                        v /= (max - min);
                        vv[p.Eindex()] = v;
                    }
                }
            }

            skillRelative = new double[RACES.All().Count()][SETT.ROOMS().Employment.ALLs().Count()];
            skill = new double[RACES.All().Count()][SETT.ROOMS().Employment.ALLs().Count()];

            foreach (RoomEmploymentSimple e in SETT.ROOMS().Employment.ALLs())
            {
                if (e.Blueprint().Bonus() != null)
                {
                    Boostable bo = e.Blueprint().Bonus();

                    foreach (Race r in RACES.All())
                    {
                        double[] vv = skillRelative[r.Index()];
                        double v = r.BValue(bo);
                        if (v > 1)
                            v = 0.5 + v * 2;
                        else
                            v = Math.Pow(v, 2) * 0.5;
                        v = CLAMP.D(v, 0, 2);

                        vv[e.Eindex()] = v;
                        skill[r.Index()][e.Eindex()] = r.BValue(bo);
                    }
                }
                else
                {
                    foreach (Race r in RACES.All())
                    {
                        double[] vv = skillRelative[r.Index()];
                        vv[e.Eindex()] = 0.5;
                        skill[r.Index()][e.Eindex()] = 1;
                    }
                }
            }
        }

        public void Debug()
        {
            LOG.Ln("RACEBOOST");

            foreach (Race r in RACES.All())
                LOG.Ln(r.Key + " " + r.Boosts.All().Count());

            foreach (BoostSpec b in boosters.All())
            {
                string s = b.Boostable.Key + " " + b.Booster.From() + " " + b.Booster.To();

                LOG.Ln(s);
            }
        }

        public double GetNorSkill(Race race, RoomEmploymentSimple e)
        {
            return skillRelative[race.Index()][e.Eindex()];
        }

        public double Skill(Race race, RoomEmploymentSimple e)
        {
            return skill[race.Index()][e.Eindex()];
        }

        private class BV : Booster
        {
            private double from;
            private double to;
            private readonly double[] dd = new double[RACES.All().Count()];
            private readonly bool isMul;
            public readonly BoostSpec Spec;
            private readonly BValue value;

            public BV(BoostSpecs bos, Boostable target, bool isMul) : base(new BSourceInfo(RACES.Name(), UI.Icons().s.Citizen), isMul)
            {
                this.isMul = isMul;
                if (isMul)
                    Array.Fill(dd, 1.0);
                Set();

                Spec = bos.Push(this, target);

                value = new BValue()
                {
                    VGet = (FactionNPC f) =>
                    {
                        if (f == null || f.CapitolRegion() == null)
                            return 0;
                        return VGet(f.CapitolRegion());
                    },
                    VGet = (Player f) =>
                    {
                        return isMul ? 1.0 : 1;
                    },
                    VGet = (HCLASS_RACE popTime) =>
                    {
                        if (popTime.Race == null)
                        {
                            double acc = 0;
                            double tot = 0;
                            for (int ri = 0; ri < RACES.All().Count(); ri++)
                            {
                                Race r = RACES.All()[ri];
                                double pop = STATS.POP().POP.Data(popTime.Cl).Get(r);

                                acc += dd[r.Index()] * pop;
                                tot += pop;
                            }
                            if (tot == 0)
                                return 0;
                            return acc / tot;
                        }
                        else
                        {
                            return dd[popTime.Race.Index()];
                        }
                    },
                    VGet = (Div div) =>
                    {
                        return dd[div.Info.Race().Index()];
                    },
                    VGet = (Induvidual indu) =>
                    {
                        return dd[indu.Race().Index()];
                    },
                    VGet = (Region reg) =>
                    {
                        double acc = 0;
                        double tot = 0;
                        for (int ri = 0; ri < RD.RACES().All.Count; ri++)
                        {
                            RDRace r = RD.RACES().All[ri];
                            double pop = r.Pop.Get(reg);
                            acc += dd[r.Race.Index()] * pop;
                            tot += pop;
                        }
                        if (tot == 0)
                            return 0;
                        return acc / tot;
                    }
                };
            }

            public void Set(Race c, double value)
            {
                dd[c.Index()] = value;
                Set();
            }

            private void Set()
            {
                if (isMul)
                {
                    from = 1.0;
                    to = 1.0;
                }
                else
                {
                    from = 0;
                    to = 0;
                }

                foreach (double v in dd)
                {
                    from = Math.Min(v, from);
                    to = Math.Max(v, to);
                }
            }

            public override double GetValue(double input)
            {
                return input;
            }

            public override double From()
            {
                return from;
            }

            public override double To()
            {
                return to;
            }

            protected override double PGet(BOOSTABLE_O o)
            {
                return o.BoostableValue(value);
            }
        }
    }
}