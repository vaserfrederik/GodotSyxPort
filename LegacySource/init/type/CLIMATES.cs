using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace init.type
{
    public static class CLIMATES
    {
        public const string KEY = "CLIMATE";
        private static Data d;

        static CLIMATES()
        {
            d = new Data();
        }

        public static CLIMATE COLD()
        {
            return d.COLD;
        }

        public static CLIMATE TEMP()
        {
            return d.TEMPERATE;
        }

        public static CLIMATE HOT()
        {
            return d.HOT;
        }

        public static List<CLIMATE> ALL()
        {
            return d.all;
        }

        public static RMAP<CLIMATE> MAP()
        {
            return d.map;
        }

        public static INFO INFO()
        {
            return d.info;
        }

        public static BoostSpecs BONUS()
        {
            return d.boosters;
        }

        public static void PushBonuses(Json json, Boostable bo)
        {
            if (!json.Has(KEY))
                return;
            double[] vv = new double[ALL().Count];
            vv.Fill(1.0);
            CLIMATES.MAP().ReadFill(vv, json, 0, 2000);
            foreach (CLIMATE c in CLIMATES.ALL())
            {
                if (vv[c.Index()] == 1.0)
                    continue;
                c.boosters.PushPromise(bo, null, vv[c.Index()], true);
            }
        }

        public static BoostSpec PushIfDoesntExist(CLIMATE c, double v, Boostable bo, bool isMul)
        {
            string k = bo.key + isMul;
            double none = isMul ? 1 : 0;

            if (d.bvmap.ContainsKey(k) && d.bvmap[k].values[c.Index()] != none)
                return null;

            bool ret = false;
            if (!d.bvmap.ContainsKey(k))
            {
                ret = true;
                d.bvmap[k] = new BV(d.boosters, bo, isMul);
            }
            BV bv = d.bvmap[k];
            bv.Set(c, v);
            c.boosters.Push(bo, v, isMul);
            return ret ? bv.spec : null;
        }

        private class Data
        {
            private readonly CLIMATE COLD;
            private readonly CLIMATE TEMPERATE;
            private readonly CLIMATE HOT;
            private readonly List<CLIMATE> all = new List<CLIMATE>(3);
            private readonly INFO info;
            private readonly BoostSpecs boosters;
            private readonly Dictionary<string, BV> bvmap = new Dictionary<string, BV>();
            private readonly RMAP<CLIMATE> map;

            public Data() : base()
            {
                D.gInit(typeof(CLIMATES));
                info = new INFO(D.g("Climate"), D.g("desc", "Climate zones have a range of bonuses and drawbacks. They also have different base temperatures, which can lead to exposure and death for your subjects depending on their natural resilience to hot and cold."));
                d = this;
                Json j = new Json(PATHS.CONFIG().init.gets(KEY));
                COLD = new CLIMATE(
                    all, "COLD",
                    D.g("Cold"),
                    D.g("cold_desc", "Very cold winters. Unique crops. Low disease rates."),
                    j);
                TEMPERATE = new CLIMATE(
                    all, "TEMPERATE",
                    D.g("Temperate"),
                    D.g("temp_desc", "Varying temperature."),
                    j);
                HOT = new CLIMATE(
                    all, "HOT",
                    D.g("Warm"),
                    D.g("warm_desc", "Hot summers."),
                    j);

                map = new RMAP<CLIMATE>(KEY, all);

                boosters = new BoostSpecs(info.name, UI.icons().s.heat, true);

                ACTION a = new ACTION()
                {
                    public void Exe()
                    {
                        foreach (CLIMATE c in CLIMATES.ALL())
                        {
                            foreach (BoostSpec s in c.boosters.all())
                            {
                                string k = s.boostable.key + s.booster.isMul;
                                if (!bvmap.ContainsKey(k))
                                {
                                    bvmap[k] = new BV(boosters, s.boostable, s.booster.isMul);
                                }
                                bvmap[k].Set(c, s.booster.to());
                            }
                        }
                    }
                };
                BOOSTING.connecter(a);

                foreach (CLIMATE c in all)
                {
                    GVALUES.FACTION.push("CLIMATE_" + c.key, info.name + ": " + c.name, UI.icons().s.heat, new BOOLEANO<Faction>()
                    {
                        public bool Is(Faction t)
                        {
                            if (t.capitolRegion() != null)
                                return WORLD.CLIMATE().getter.get(t.capitolRegion().cx(), t.capitolRegion().cy()) == c;
                            return false;
                        }
                    });
                }
            }
        }

        private class BV : Booster
        {
            private double from;
            private double to;
            private readonly double[] values = new double[CLIMATES.ALL().Count];
            private readonly bool isMul;
            public readonly BoostSpec spec;
            private readonly BValue value;

            public BV(BoostSpecs bos, Boostable target, bool isMul) : base(new BSourceInfo(CLIMATES.INFO().name, UI.icons().s.heat), isMul)
            {
                this.isMul = isMul;
                if (isMul)
                    values.Fill(1.0);
                Set();
                spec = bos.push(this, target);
                value = new BValue()
                {
                    public double vGet(Region reg)
                    {
                        if (reg == null)
                            return 0;
                        double res = 0;
                        for (int ci = 0; ci < CLIMATES.ALL().Count; ci++)
                        {
                            res += values[ci] * reg.info.climate(CLIMATES.ALL()[ci]);
                        }

                        return res;
                    }

                    public double vGet(Faction f)
                    {
                        if (f == null)
                            return 0;
                        return BValue.super.vGet(f);
                    }

                    public double vGet(Induvidual indu)
                    {
                        return vGet(indu.faction());
                    }

                    public double vGet(Div div)
                    {
                        return vGet(div.faction());
                    }

                    public double vGet(HCLASS_RACE popTime)
                    {
                        if (FACTIONS.player() == null)
                            return 0;
                        return vGet(FACTIONS.player());
                    }

                    public double vGet(Player f)
                    {
                        if (f.capitolRegion() == null)
                            return 0;
                        return values[WORLD.CLIMATE().getter.get(f.capitolRegion().cx(), f.capitolRegion().cy()).index()];
                    }

                    public double vGet(FactionNPC f)
                    {
                        return vGet(f.capitolRegion());
                    }
                };
            }

            public void Set(CLIMATE c, double value)
            {
                values[c.Index()] = value;
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

                foreach (double v in values)
                {
                    from = Math.Min(v, from);
                    to = Math.Max(v, to);
                }
            }

            public override double From()
            {
                return from;
            }

            public override double To()
            {
                return to;
            }

            public override double GetValue(double input)
            {
                return input;
            }

            protected override double pget(BOOSTABLE_O o)
            {
                return o.boostableValue(value);
            }
        }
    }
}