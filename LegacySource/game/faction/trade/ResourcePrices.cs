using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Faction.Trade
{
    public static class ResourcePrices
    {
        private static int[] lastCheck = new int[TR.ALL().Count];
        private static double[] price = new double[TR.ALL().Count];
        private static int echeck = -16;
        private static int edible = NPCStockpile.AVERAGE_PRICE;
        private static int edibleLow = NPCStockpile.AVERAGE_PRICE / 8;

        static ResourcePrices()
        {
            Array.Fill(lastCheck, -16);
        }

        public static void ClearCache()
        {
            Array.Fill(lastCheck, -16);
            echeck = -16;
        }

        public static int Get(TRADABLE res)
        {
            if (FACTIONS.Player() == null || FACTIONS.Player().CapitolRegion() == null)
            {
                return NPCStockpile.AVERAGE_PRICE;
            }

            int ri = res.Index();
            if (Math.Abs(lastCheck[res.Index()] - GAME.UpdateI()) > 16)
            {
                lastCheck[res.Index()] = GAME.UpdateI();
                double a = 0;
                price[ri] = 0;
                for (int fi = 0; fi < FACTIONS.NPCs().Count; fi++)
                {
                    FactionNPC f = FACTIONS.NPCs()[fi];
                    if (f.CapitolRegion() == null)
                        continue;

                    double pop = RD.RACES().Population.Get(f.CapitolRegion());
                    if (f.Seller(res).RemoveMax() > 0)
                    {
                        price[ri] += f.Res(res).PriceBase() * pop;
                        a += pop;
                    }
                }
                if (a == 0)
                    price[ri] = NPCStockpile.AVERAGE_PRICE;
                else
                {
                    price[ri] /= a;
                }
            }
            return (int)Math.Ceiling(price[res.Index()]);
        }

        private static void Ee()
        {
            if (Math.Abs(echeck - GAME.UpdateI()) > 16)
            {
                echeck = GAME.UpdateI();
                edible = 0;
                edibleLow = int.MaxValue;
                for (int ei = 0; ei < RESOURCES.EDI().All().Count; ei++)
                {
                    ResGEat e = RESOURCES.EDI().All()[ei];
                    int p = Get(TR.Get(e.Resource));
                    edibleLow = Math.Min(p, edibleLow);
                    edible += p;
                }
                edible /= RESOURCES.EDI().All().Count;
            }
        }

        public static int Edible()
        {
            Ee();
            return edible;
        }

        public static int EdibleLow()
        {
            Ee();
            return edibleLow;
        }
    }
}