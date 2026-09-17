using System;
using System.Collections.Generic;
using game.boosting;
using game.faction.npc;
using game.faction.player;
using game.time;
using init.religion;
using init.sprite.UI;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.text;
using world.map.regions;
using world.region.RD;
using world.region.pop;

public class RDReligions
{
    private readonly ArrayListGrower<RDReligion> all = new ArrayListGrower<RDReligion>();
    public readonly INT_OE<Region> opposition;
    private static readonly CharSequence ¤¤Opposition = "Religious differences";

    static
    {
        D.ts(typeof(RDReligions));
    }

    public RDReligions(RDInit init)
    {
        opposition = init.count.new DataByte("REL_OPPOSITION");

        foreach (Religion r in RELIGIONS.ALL())
        {
            all.add(new RDReligion(init, r));
        }

        BOOSTING.connecter(new ACTION
        {
            public void exe()
            {
                RBooster lo = new RBooster(new BSourceInfo(¤¤Opposition, UI.icons().s.heat), 1.0, 0.75, true)
                {
                    public double get(Region t)
                    {
                        return opposition.getD(t);
                    }
                };

                foreach (RDRace race in RD.RACES().all)
                {
                    lo.add(race.loyalty.target);
                }
            }
        });

        init.upers.add(new RDUpdatable
        {
            private readonly double dt = 1.0 / (TIME.secondsPerDay() * 16);

            public void update(Region reg, double time)
            {
                double min = min(reg);
                double tot = tot(reg) - min;

                foreach (RDReligion r in all)
                {
                    double target = Math.Round(0xFF * (r.boost.get(reg) - min) / tot);
                    double now = target + dt * (target - r.current.get(reg));
                    now = CLAMP.d(now, Math.Min(target, r.current.get(reg)), Math.Max(target, r.current.get(reg)));
                    r.current.set(reg, (int)now);
                }

                setop(reg);
            }

            public void init(Region reg)
            {
                double min = min(reg);
                double tot = tot(reg) - min;
                foreach (RDReligion r in all)
                {
                    double target = Math.Round(0xFF * (r.boost.get(reg) - min) / tot);
                    r.current.set(reg, (int)target);
                }
                setop(reg);
            }

            private void setop(Region reg)
            {
                double op = 0;

                for (int ri = 0; ri < all.size(); ri++)
                {
                    double vv = 0;
                    RDReligion r = all.get(ri);
                    for (int ri2 = 0; ri2 < all.size(); ri2++)
                    {
                        RDReligion r2 = all.get(ri2);
                        double am = r2.current.getD(reg);
                        am *= r.religion.opposition(r2.religion);
                        vv += am;
                    }
                    op += vv * r.current.getD(reg);
                }

                op = CLAMP.d(op, 0, 1);
                opposition.setD(reg, op);
            }
        });
    }

    private double min(Region reg)
    {
        double mi = 0;
        foreach (RDReligion r in all)
        {
            mi = Math.Min(mi, r.boost.get(reg));
        }
        return mi;
    }

    private double tot(Region reg)
    {
        double tot = 0;
        foreach (RDReligion r in all)
        {
            tot += Math.Max(r.boost.get(reg), 0);
        }
        return tot;
    }

    public LIST<RDReligion> all()
    {
        return all;
    }

    public RDReligion get(Religion t)
    {
        return all.get(t.index());
    }

    public class RDReligion
    {
        public readonly Boostable boost;
        public readonly BoostSpecs boosts;
        public readonly Religion religion;
        public readonly INT_OE<Region> current;

        private RDReligion(RDInit init, Religion reg)
        {
            boosts = new BoostSpecs(reg.info.name, reg.icon, true);
            religion = reg;
            current = init.count.new DataByte("REL" + reg.key);
            boost = BOOSTING.push("CONVERSION_" + reg.key, reg.inclination, reg.info.name, reg.info.desc, reg.icon, BoostableCat.ALL().WORLD_CIVICS);
            BOOSTING.connecter(new ACTION
            {
                public void exe()
                {
                    foreach (BoostSpec s in reg.boosts.all())
                    {
                        if ((s.boostable.cat.typeMask & BoostableCat.TYPE_WORLD) != 0)
                        {
                            BoosterValue b = new BoosterValue(new value(s.boostable), s.booster.info, s.booster.to(), s.booster.isMul);
                            boosts.push(b, s.boostable);
                        }
                    }
                }
            });
        }

        public double target(Region reg)
        {
            double tot = 0;
            foreach (RDReligion r in all)
            {
                tot += r.boost.get(reg);
            }
            return boost.get(reg) / tot;
        }

        public class value : BValue.BValueFaction
        {
            value(Boostable bo) : base(bo) { }

            public override double vGet(Region reg)
            {
                return current.getD(reg);
            }

            public override double vGet(Player f)
            {
                double d = 0;
                for (int i = 0; i < f.realm().regions(); i++)
                {
                    Region reg = f.realm().region(i);
                    d += current.getD(reg) * RD.RACES().population.get(reg);
                }
                d /= RD.RACES().population.faction().get(f);
                return d;
            }

            public override double vGet(FactionNPC f)
            {
                return 0;
            }
        }
    }
}