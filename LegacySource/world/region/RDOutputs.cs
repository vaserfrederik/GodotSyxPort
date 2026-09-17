using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.time;
using init.resources;
using init.trade;
using snake2d.util.MATH;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.text;
using world;
using world.entity.caravan;
using world.map.regions;
using world.region.RD;
using world.region.pop;

public static class RDOutputs
{
    private static readonly CharSequence ¤¤taxes = "¤Taxes";
    public static readonly CharSequence ¤¤Squeeze = "¤Squeeze";
    public static readonly CharSequence ¤¤SqueezeD = "¤Squeeze this settlement for what it's got. Instantly delivering goods, but devastating the region, and upsetting the populace.";
    private static readonly CharSequence ¤¤taxD = "¤Taxes are generated from your subjects. Higher tax rate increases taxes, but decreases loyalty.";

    static RDOutputs()
    {
        D.ts(typeof(RDOutputs));
    }

    private INT_OE<Region> squeeze;

    public readonly LIST<RDOutput> ALL;
    public readonly RDOutput MONEY;
    public readonly LIST<RDResource> RES;

    public readonly double sqeezeAmountDays = 4.0;

    public RDOutputs(RDInit init)
    {
        init.ownerChangers.add(new RD.RDOwnerChanger
        {
            change = (reg, oldOwner, newOwner) =>
            {
                foreach (RDOutput r in ALL)
                {
                    r.yearlyAccumulation.set(reg, 0);
                }
            }
        });

        ArrayList<RDResource> rr = new ArrayList<RDResource>(TR.ALL().size);

        foreach (TRADABLE res in TR.ALL())
        {
            rr.add(new RDResource(init, res));
        }
        this.RES = rr;

        squeeze = init.count.new DataShort("TAX_RATE", ¤¤Squeeze, ¤¤SqueezeD, 10);
        Boostable boost = BOOSTING.push("TAX_INCOME", 0, ¤¤taxes, ¤¤taxD, UI.icons().m.coins, BoostableCat.ALL().WORLD);
        MONEY = new RDOutput(boost, init);

        ALL = new ArrayList<RDOutput>(0).join(MONEY).join(rr);

        ACTION a = new ACTION
        {
            exe = () =>
            {
                RBooster b = new RBooster(new BSourceInfo(¤¤Squeeze, UI.icons().s.money), 1, 0.25, true)
                {
                    get = t => squeeze.getD(t)
                };
                foreach (RDRace r in RD.RACES().all)
                {
                    b.add(r.loyalty.target);
                }
            }
        };

        BOOSTING.connecter(a);

        init.upers.add(new RDUpdatable
        {
            update = (reg, time) =>
            {
                time *= TIME.secondsPerDayI();
                int t = (int)time;
                if (RND.rFloat() < time - t)
                    t++;
                squeeze.inc(reg, -t);
            },
            init = reg => { }
        });
    }

    void init()
    {
    }

    public class RDOutput
    {
        public readonly Boostable boost;
        public readonly Boostable boostYearlyPart;
        public readonly INT_OE<Region> yearlyAccumilation;

        public RDOutput(Boostable boost, RDInit init)
        {
            this.boost = boost;
            this.boostYearlyPart = BOOSTING.push(boost.key.Split("WORLD")[1] + "_YEARLY", 0, boost.name, ¤¤taxD, boost.icon, BoostableCat.ALL().WORLD_DUMP);
            yearlyAccumilation = init.count.new DataInt(boost.key + "_" + "ACC");
        }

        public int getDelivery(Region reg)
        {
            return (int)(boost.get(reg) + boostYearlyPart.get(reg));
        }

        public int loot(Region reg)
        {
            double d = 1.0 - RD.DEVASTATION().current.getD(reg);

            return (int)(d * (boost.get(reg) + yearlyAccumilation.get(reg)));
        }

        public int daysUntilDailydelivery()
        {
            int d = 0;
            int now = TIME.days().bitsSinceStart() % (int)TIME.years().bitConversion(TIME.days());
            int remain = (int)MATH.ETA(now, d, (int)TIME.years().bitConversion(TIME.days()));
            return remain;
        }
    }

    public class RDResource : RDOutput
    {
        public readonly TRADABLE res;
        private readonly Growable g;

        public RDResource(RDInit init, TRADABLE res) : base(BOOSTING.push("PRODUCTION_" + res.key(), 0, Dic.¤¤Production + ": " + res.names, res.desc, res.icon(), BoostableCat.ALL().WORLD_PRODUCTION), init)
        {
            this.res = res;

            g = g(res);
        }

        private Growable g(TRADABLE t)
        {
            foreach (Growable g in RESOURCES.growable().all())
            {
                if (TR.get(g.resource) == t)
                    return g;
            }
            return null;
        }

        public override int daysUntilDailydelivery()
        {
            if (g != null)
            {
                int d = (int)(g.seasonalOffset * TIME.years().bitConversion(TIME.days()));
                int now = TIME.days().bitsSinceStart() % (int)TIME.years().bitConversion(TIME.days());
                int remain = (int)MATH.ETA(now, d, (int)TIME.years().bitConversion(TIME.days()));
                return remain;
            }

            return base.daysUntilDailydelivery();
        }
    }

    public RDResource get(TRADABLE res)
    {
        return RES.get(res.index());
    }

    public TRADABLE fromBoost(Boostable bo)
    {
        if (bo.index() >= RES.get(0).boost.index() && bo.index() < RES.get(RES.size() - 1).boostYearlyPart.index())
        {
            return TR.ALL().get((bo.index() - RES.get(0).boost.index()) / 2);
        }
        return null;
    }

    public void squeze(Region reg)
    {
        Faction f = reg.faction();
        if (f == null)
            return;

        f.credits().inc(RD.OUTPUT().MONEY.boost.get(reg) * sqeezeAmountDays, CTYPE.TAX);

        int am = 0;
        foreach (RDResource res in RD.OUTPUT().RES)
        {
            am += res.boostYearlyPart.get(reg) * sqeezeAmountDays / TIME.years().bitConversion(TIME.days());
            am += res.boost.get(reg) * sqeezeAmountDays;
        }

        if (am <= 0)
        {
            RD.DEVASTATION().current.incD(reg, 0.5);
            squeeze.incD(reg, 0.5);
            return;
        }
        Shipment c = WORLD.ENTITIES().caravans.create(reg, f.capitolRegion(), TRADE_TYPE.tax);
        if (c != null)
        {
            foreach (RDResource res in RD.OUTPUT().RES)
            {
                int a = (int)(res.boostYearlyPart.get(reg) * sqeezeAmountDays / TIME.years().bitConversion(TIME.days()));
                a += res.boost.get(reg) * sqeezeAmountDays;
                if (a > 0)
                {
                    c.loadAndReserve(res.res, a);
                }
            }
        }
        RD.DEVASTATION().current.incD(reg, 0.5);
        squeeze.incD(reg, 0.5);
    }
}