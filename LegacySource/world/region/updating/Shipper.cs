using System;
using System.Collections.Generic;
using game.faction;
using game.time;
using init.trade;
using snake2d.util.rnd;
using world;
using world.entity.caravan;
using world.map.regions;
using world.region;
using world.region.RDOutputs;

namespace world.region.updating
{
    internal sealed class Shipper
    {
        public Shipper()
        {
        }

        public void Ship(Region r, double seconds)
        {
            Faction f = r.Faction();

            if (f == null)
                return;

            if (r.Besieged())
                return;

            if (f.CapitolRegion() == null)
                return;

            double days = seconds * TIME.SecondsPerDayI();
            int am = 0;

            if (f == FACTIONS.Player())
            {
                if (r.Capitol())
                    return;
            }

            f.Credits().Inc(RD.OUTPUT().MONEY.Boost.Get(r) * days, CTYPE.TAX);

            foreach (RDResource res in RD.OUTPUT().RES)
            {
                Count(res, r, seconds);
                am += Amount(res, r, seconds);
            }

            if (am <= 0)
                return;

            Shipment c = WORLD.ENTITIES().Caravans.Create(r, f.CapitolRegion(), TRADE_TYPE.Tax);
            if (c != null)
            {
                foreach (RDResource res in RD.OUTPUT().RES)
                {
                    int a = Amount(res, r, seconds);
                    if (a > 0)
                    {
                        c.LoadAndReserve(res.Res, a);
                        Clear(res, r);
                    }
                }
            }
        }

        private void Count(RDResource res, Region r, double seconds)
        {
            double am = res.BoostYearlyPart.Get(r) * seconds * TIME.SecondsPerDayI();
            int a = (int)am;
            if (am - a > RND.rFloat())
                a++;

            res.YearlyAccumulation.Inc(r, a);
        }

        private void Clear(RDResource res, Region r)
        {
            if (res.DaysUntilDailydelivery() == 0)
            {
                res.YearlyAccumulation.Set(r, 0);
            }
        }

        private int Amount(RDResource res, Region r, double seconds)
        {
            int am = (int)Math.Ceiling(res.Boost.Get(r) * seconds * TIME.SecondsPerDayI());

            if (res.DaysUntilDailydelivery() == 0)
            {
                am += res.YearlyAccumulation.Get(r);
            }
            return am;
        }

        public void ShipAll(Faction f, double days)
        {
            for (int ri = 0; ri < f.Realm().Regions(); ri++)
            {
                Region reg = f.Realm().Region(ri);
                foreach (RDResource res in RD.OUTPUT().RES)
                {
                    int a = (int)Math.Ceiling(res.Boost.Get(reg) * days);
                    if (a > 0)
                    {
                        f.Buyer(res.Res).AddReserveAndDeliver(a, TRADE_TYPE.Tax);
                    }
                }
            }
        }
    }
}