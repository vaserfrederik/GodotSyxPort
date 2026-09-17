using System;
using System.Collections.Generic;

namespace Game.Faction.NPC.Stockpile
{
    public class NPCStockpileTest
    {
        private const double AISize = 5000;

        public NPCStockpileTest()
        {
            FactionNPC f = FACTIONS.NPCs().Rnd();

            int[] workers = new int[]
            {
                50,
                100,
                150,
                200,
                250,
                300,
            };

            foreach (int w in workers)
            {
                Console.WriteLine("WORKERS: " + w);

                RESOURCE ma = RESOURCES.Map().Get("MACHINERY")[0];
                Sell(f, ma.Tr(), w);

                ma = RESOURCES.Map().Get("GRAIN")[0];
                Sell(f, ma.Tr(), w);

                Console.WriteLine();
            }
        }

        private void Sell(FactionNPC f, TRADABLE ma, double workers)
        {
            NPCStockpile s = f.Stockpile;

            s.Saver().Clear();
            s.Update(f, 0, AISize);
            f.Credits().Set(0);

            double po = f.Buyer(ma).AddPrice(1);

            double credits = 0;
            double items = 0;
            int resBought = 0;
            double tot = 0;

            double cr = 0;
            double am = 0;

            const int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                am += workers * s.Res(ma).Rate();
                items += am;

                while (am > 0)
                {
                    double p = f.Buyer(ma).AddPrice(1);
                    cr += p;
                    credits += p;
                    s.Res(ma).Inc(1);
                    f.Credits().Inc(-p, CTYPE.TAX);
                    am--;
                }

                foreach (TRADABLE r in TR.ALL())
                {
                    tot += Math.Max(f.Seller(r).RemovePrice(1), 1);
                }

                while (cr > 0)
                {
                    TRADABLE rr = TR.ALL().Rnd();
                    if (rr == ma)
                        continue;
                    double c = Math.Max(f.Seller(rr).RemovePrice(1), 1);
                    s.Res(rr).Inc(-1);
                    cr -= c;
                    f.Credits().Inc(c, CTYPE.TAX);
                    resBought++;
                }

                s.Update(f, TIME.SecondsPerDay(), AISize);
                f.Credits().Inc(-f.Credits().Credits() * 0.05, CTYPE.INFLATION);
            }

            tot /= RESOURCES.ALL().Count * iterations;

            Console.WriteLine(ma.Key());
            Console.WriteLine("starprice: " + (int)po);
            Console.WriteLine("aveprice: " + (int)(credits / items));
            Console.WriteLine("items sold: " + (int)items);
            Console.WriteLine("money circulated: " + (int)credits);
            Console.WriteLine("goods purchased: " + (int)resBought);
            Console.WriteLine("average buy price: " + (int)tot);
            double d = s.Res(ma).RateTot() / s.Res(ma).Rate();
            Console.WriteLine("profits: " + d * resBought / workers);
        }
    }
}