using System;
using System.IO;
using System.Collections.Generic;

namespace Game.Faction.Trade
{
    public class TradeManager : FactionResource
    {
        public static readonly int TRADE_INTERVAL = 1;
        public static double tollPerTile = 100.0 / NPCStockpile.AVERAGE_PRICE;

        private readonly TileUpdater updater;
        private readonly TradeShipper shipper = new TradeShipper();
        private readonly TradeSorter sorter = new TradeSorter();

        public static int TotalFee(Faction seller, Faction buyer, double distance, TRADABLE res, int amount)
        {
            double toll = Toll(seller, buyer, distance);
            double tariff = Tariff(seller, buyer, res, amount);
            return (int)Math.Floor((toll + tariff) * amount);
        }

        public static double Tariff(Faction seller, Faction buyer, TRADABLE res, int amount)
        {
            if (buyer == FACTIONS.Player())
            {
                return 0; // PlayerTariff((FactionNPC)seller, res, amount);
            }

            if (seller == FACTIONS.Player())
            {
                return PlayerTariff((FactionNPC)buyer, res, amount);
            }

            FactionNPC npc = (FactionNPC)buyer;

            double price = Price(npc, res);
            double tt = DIP.ALLY().Tariff;
            return tt * price;
        }

        private static double PlayerTariff(FactionNPC npc, TRADABLE res, int amount)
        {
            double price = Price(npc, res);
            double tt = ROPINION.TradeCost(npc);
            tt += npc.Res(res).PlayerTariff(amount);
            if (tt > 0.9)
                tt = 0.9;
            return tt * price;
        }

        public static double Toll(FactionNPC f)
        {
            return Toll(FACTIONS.Player(), f, RD.DIST().Distance(f));
        }

        private static double Price(FactionNPC npc, TRADABLE res)
        {
            double price = npc.Res(res).PriceAt(0);
            return price;
        }

        public static double Toll(Faction f, Faction f2, double distance)
        {
            distance = (20 + distance) * tollPerTile;
            distance = Math.Max(0, distance);

            if (f == FACTIONS.Player() || f2 == FACTIONS.Player())
            {
                return distance / RD.DIST().BProximityToll.Get(HCLASSES.CITIZEN().Get(null));
            }
            else
            {
                return distance / 4.0;
            }
        }

        public TradeManager(FACTIONS fs)
        {
            IDebugPanel.Add("Trade all", new ACTION
            {
                Exe = () =>
                {
                    Clear();
                    Prime();
                }
            });

            updater = new TileUpdater(FACTIONS.MAX(), FACTIONS.MAX() + 4, TRADE_INTERVAL * TIME.Days().BitSeconds())
            {
                Update = (iteration, factionI, vv, timeSinceLast) =>
                {
                    if (factionI == FACTIONS.MAX() / 2 || factionI == 0)
                    {
                        if (iteration == 0)
                        {
                            SellPlayer();
                        }
                        if (shipper.Partners() > 0)
                        {
                            Partner p = shipper.PopNextPartner();
                            Faction b = p.Faction();
                            Ship(b, FACTIONS.Player(), p, true);
                        }
                        return;
                    }
                    if (factionI == FACTIONS.MAX() / 2 + 1 || factionI == 1)
                    {
                        PBuy(FACTIONS.Player(), iteration);
                        return;
                    }

                    if (factionI < FACTIONS.MAX() / 2)
                        factionI -= 1;
                    else
                        factionI -= 3;

                    if (factionI >= FACTIONS.MAX())
                        return;

                    Faction buyer = FACTIONS.GetByIndex(factionI);
                    PBuy(buyer, iteration);
                }
            };
        }

        protected override void Save(FilePutter file)
        {
            updater.Save(file);
            shipper.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            updater.Load(file);
            shipper.Load(file);
        }

        protected override void Clear()
        {
            updater.Clear();
            shipper.Clear();
        }

        protected override void Update(double ds, Faction f)
        {
            updater.Update(ds);
        }

        private void SellPlayer()
        {
            if (!SETT.Exists() || SETT.ENTRY().IsClosed())
                return;

            shipper.Init(FACTIONS.Player());
            sorter.SellPlayer(shipper);
        }

        void Buy(Faction buyer)
        {
            shipper.Init(buyer);
            sorter.Buy(buyer, shipper);
        }

        private void Ship(Faction buyer, Faction seller, Partner count, bool shipping)
        {
            if (!buyer.IsActive())
                return;

            int am = 0;
            foreach (TRADABLE r in TR.ALL())
            {
                am += count.Traded(r);
            }

            if (am <= 0)
                return;

            Shipment s = null;
            if (shipping && seller.IsActive())
            {
                bool create = buyer == FACTIONS.Player() || seller == FACTIONS.Player();
                if (!create)
                    create = WORLD.ENTITIES().AllFast().Count < 200;

                if (create)
                {
                    s = WORLD.ENTITIES().Caravans.Create(seller.CapitolRegion(), buyer.CapitolRegion(), TRADE_TYPE.Trade);
                    if (s == null)
                        LOG.Ln("here!");
                }
            }

            if (s != null)
            {
                foreach (TRADABLE r in TR.ALL())
                {
                    int a = count.Traded(r);
                    if (a > 0)
                    {
                        s.Load(r, a);
                    }
                }
            }
            else
            {
                foreach (TRADABLE r in TR.ALL())
                {
                    int a = count.Traded(r);
                    if (a > 0)
                    {
                        buyer.Buyer(r).AddDeliver(am, TRADE_TYPE.Trade);
                    }
                }
            }
            //LOG.Ln();
        }

        public void Prime()
        {
            for (int i = 0; i < FACTIONS.NPCs().Size; i++)
            {
                FactionNPC f = FACTIONS.NPCs().Get(i);
                if (!f.IsActive())
                    continue;
                Buy(f);
                while (shipper.HasNextPartner())
                {
                    Partner p = shipper.PopNextPartner();
                    Ship(f, p.Faction(), p, false);
                }
            }
        }
    }
}