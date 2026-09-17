using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Region
{
    public class RDMilitary
    {
        private static readonly string ¤¤garrisonD = "Troops that are stationed in a region which will defend it against attacks.";
        private static readonly string ¤¤conscriptD = "Conscripts are candidates that can be trained into soldiers.";

        static
        {
            D.ts(typeof(RDMilitary));
        }

        public readonly RDataE garrison;
        public readonly DOUBLE_O<Region> fort;
        public readonly Boostable bgarrison;
        public readonly Boostable conscriptTarget;
        public readonly Boostable bFortification;
        private readonly RDMilitaryGar gar = new RDMilitaryGar();

        public RDMilitary(RDInit init)
        {
            bgarrison = BOOSTING.push("GARRISON", 0, Dic.¤¤garrison, ¤¤garrisonD, UI.icons().s.shield, BoostableCat.ALL().WORLD);
            bFortification = BOOSTING.push("FORTIFICATION", 8, Dic.¤¤Fort, Dic.¤¤FortD, UI.icons().s.degrade, BoostableCat.ALL().WORLD);

            INT_OE<Region> dd = init.count.new DataShort("GARRISON", null, Config.battle().REGION_MAX_DIVS * Config.battle().MEN_PER_DIVISION)
            {
                public override int get(Region t)
                {
                    if (FACTIONS.player().capitolRegion() == t)
                    {
                        int pow = 0;
                        foreach (WDIV d in gar.player())
                        {
                            pow += d.men();
                        }
                        return pow;
                    }
                    return base.get(t);
                }

                public override void set(Region t, int s)
                {
                    gar.init();
                    base.set(t, s);
                }
            };
            garrison = new RDataE("GARRISON", dd, init, Dic.¤¤garrison);
            fort = init.count.new DataDouble("REG_FORT");

            init.upers.add(new RDUpdatable
            {
                private readonly double dt = 2.0 / (TIME.secondsPerDay());

                public override void update(Region reg, double time)
                {
                    int t = garrisonTarget(reg);

                    if (WORLD.BATTLES().besigedTime(reg) > 0)
                    {
                        int d = (int)(garrisonTarget(reg) * (1.0 - besigeMul(reg)));
                        d = (int)Math.Min(garrison.get(reg) * (1.0 - besigeMul(reg)), d);
                        d = CLAMP.i(d, 0, t);
                        if (d < garrison.get(reg))
                        {
                            garrison.set(reg, d);
                        }
                        return;
                    }

                    int d = garrisonTarget(reg);

                    garrison.moveTo(reg, time * dt * 50, d);
                    double f = bFortification.get(reg) - fort.getD(reg);
                    double nn = fort.getD(reg) + time * dt * f;
                    if (f < 0)
                    {
                        nn += 3 * time * dt * f;
                        nn = Math.Max(nn, 0);
                    }
                    else
                    {
                        nn = Math.Min(nn, bFortification.get(reg));
                    }
                    fort.setD(reg, nn);
                }

                public override void init(Region reg)
                {
                    garrison.set(reg, (int)garrisonTarget(reg));
                    fort.setD(reg, bFortification.get(reg));
                }
            });

            conscriptTarget = BOOSTING.push("CONSCRIPTABLE_TARGET", 0, Dic.¤¤Conscripts, ¤¤conscriptD, UI.icons().s.sword, BoostableCat.ALL().WORLD);

            new RBooster(new BSourceInfo(Dic.¤¤Population, UI.icons().s.human), 0, 20000, false)
            {
                public override double get(Region t)
                {
                    return RD.RACES().population.get(t) * 0.05 / 20000.0;
                }
            }.add(conscriptTarget);

            // Uncomment and implement the following if needed
            // new RBooster(new BSourceInfo(Dic.¤¤Besiege, UI.icons().s.degrade), 1, 0, true)
            // {
            //     public override double get(Region t)
            //     {
            //         return besigeMul(t);
            //     }
            // }.add(bgarrison);
        }

        public double besigeMul(Region t)
        {
            return CLAMP.d((WORLD.BATTLES().besigedTime(t) - TIME.secondsPerDay()) / (TIME.secondsPerDay() * 16.0), 0, 1);
        }

        public int defensePower(Region reg)
        {
            return (int)Math.Ceiling((1 + fort.getD(reg)) * power.getD(reg));
        }

        public int garrisonTarget(Region reg)
        {
            if (reg == null)
                return 0;
            if (reg.faction() == FACTIONS.player())
            {
                return (int)bgarrison.get(reg);
            }
            else
            {
                double dz = RD.RACES().population.get(reg) / (double)ENTETIES.MAX;
                dz *= 1 + 0.25 * (-8 + RD.RAN().get(reg, 9, 4)) / 8.0;
                dz = CLAMP.d(dz, 0, 1);

                dz *= CLAMP.d(POP.tot(null) / 8000.0, 0.1, 1);

                if (reg.faction() is FactionNPC)
                {
                    FactionNPC f = (FactionNPC)reg.faction();
                    dz *= 1.0 + 0.5 * f.court().king().garrison();
                }
                dz = CLAMP.d(dz, 0, 1);
                dz = (100 + dz * (garrison.max(reg) - 100));

                return (int)dz;
            }
        }

        public int conscripts(Race r, Faction f)
        {
            if (f == FACTIONS.player())
            {
                if (RD.RACES().get(r) == null)
                {
                    return 0;
                }

                int am = 0;
                for (int i = 0; i < f.realm().regions(); i++)
                {
                    Region rr = f.realm().region(i);
                    if (!rr.capitol() && RD.RACES().population.get(rr) > 0)
                    {
                        am += conscriptTarget.get(rr) * RD.RACES().get(r).pop.get(rr) / RD.RACES().population.get(rr);
                    }
                }

                return am;
            }
            else
            {
                if (RD.RACES().get(r) == null)
                {
                    double d = (double)(f.realm().all().size() / 8.0);
                    d = CLAMP.d(d, 0, 1);
                    return (int)(WORLD.camps().current(f, r) * d);
                }

                FactionNPC ff = (FactionNPC)f;
                double dist = 1 + CLAMP.d(RD.DIST().distance(ff) / 512.0, 0, 2);

                double p = 1 + f.realm().all().size() * 0.025;
                double d = POP.tot(null) / 20000.0;
                d = CLAMP.d(d, 0.1, 1);
                d *= dist * (RD.RACES().get(r).pop.faction().get(f) * 0.25 / p);
                if (r == ff.court().king().roy().induvidual.race())
                    return (int)(10 + d);
                return (int)d;
            }
        }

        public readonly DOUBLE_O<Region> power = new DOUBLE_O<Region>
        {
            private readonly INFO info = new INFO(Dic.¤¤Garrison,
                Dic.¤¤GarrisonD);

            public override double getD(Region t)
            {
                int p = 0;
                foreach (WDIV d in divisions(t))
                    p += d.provess();
                return p;
            }

            public override INFO info()
            {
                return info;
            }
        };

        public double garrison(Region reg)
        {
            return garrison.getD(reg);
        }

        public void extractSpoils(Region r, int[] equipAmounts)
        {
            if (FACTIONS.player().capitolRegion() == r)
            {
                gar.extractLostEquipment(equipAmounts);
            }
        }

        public LIST<WDIV> divisions(Region r)
        {
            return gar.divisions(r, garrison.get(r), garrisonTarget(r));
        }
    }
}