using System;
using System.Collections.Generic;
using game.faction.FACTIONS;
using game.faction.Faction;
using game.faction.npc.FactionNPC;
using game.faction.player.Player;
using game.faction.trade.TradeShipper;
using init.trade.TR;
using init.trade.TRADABLE;
using init.trade.TRADE_TYPE;
using settlement.main.SETT;
using snake2d;
using snake2d.util.misc;
using snake2d.util.sets;

namespace game.faction.trade
{
    internal sealed class TradeSorter
    {
        private readonly ResTree[] resTrees;
        private readonly Tree<ResTree> tree;
        private readonly Holder[] holders;

        private bool logPlayerSelling = false;
        private bool logPlayerBuying = false;
        private bool logAll = false;

        public TradeSorter()
        {
            resTrees = new ResTree[TR.ALL().Size()];
            tree = new Tree<ResTree>(TR.ALL().Size())
            {
                IsGreaterThan = (current, cmp) => current.value > cmp.value
            };

            holders = new Holder[FACTIONS.MAX() * TR.ALL().Size()];
            for (int i = 0; i < resTrees.Length; i++)
                resTrees[i] = new ResTree(TR.ALL().Get(i));

            for (int i = 0; i < holders.Length; i++)
                holders[i] = new Holder();
        }

        public void SellPlayer(TradeShipper shipper)
        {
            Player player = FACTIONS.player();
            tree.Clear();
            int hI = 0;

            if (shipper.Partners() <= 0)
                return;

            if (logPlayerSelling)
            {
                LOG.ln("player sell time");
            }

            for (int ri = 0; ri < TR.ALL().Size(); ri++)
            {
                TRADABLE r = TR.ALL().Get(ri);
                ResTree t = resTrees[r.Index()];
                t.traders.Clear();

                if (player.Seller(r).RemoveMax() <= 0)
                    continue;

                if (logPlayerSelling)
                {
                    LOG.ln("player wants to sell " + r + " min: " + player.Seller(r).RemovePrice(1) + " prio: " + player.Seller(r).Prio());
                }

                for (int i = 0; i < shipper.Partners(); i++)
                {
                    Partner buyer = shipper.Partner(i);

                    int price = SellPriceItemPlayer(r, 1, buyer.Faction(), buyer.Distance());
                    if (logPlayerSelling)
                    {
                        LOG.ln(buyer.Faction().Name + " bids " + price);
                    }

                    if (price < 0)
                        continue;

                    if (price < player.Seller(r).RemovePrice(1))
                        continue;

                    Holder h = holders[hI++];

                    h.p = buyer;
                    h.value = price;
                    h.price = price;
                    t.traders.Add(h);
                }

                if (logPlayerSelling)
                {
                    LOG.ln("player has " + resTrees[r.Index()].traders.Size() + " bidders");
                }

                if (resTrees[r.Index()].traders.Size() > 0)
                {
                    t.value = player.Seller(r).Prio();
                    tree.Add(t);
                }
            }

            while (tree.HasMore())
            {
                ResTree t = tree.PollGreatest();
                Holder h = t.traders.PollGreatest();

                int forSale = player.Seller(t.res).RemoveMax();

                if (logPlayerSelling)
                {
                    LOG.ln("player auctions " + t.res + " has for sale: " + forSale + " to " + h.p.Faction().Name);
                }

                if (forSale <= 0)
                    continue;

                int nextPrice = 0;
                if (t.traders.HasMore())
                {
                    Holder h2 = t.traders.Smallest();
                    nextPrice = (int)(h2.price * 0.75);
                }

                int minPrice = player.Seller(t.res).RemovePrice(1);

                if (logPlayerSelling)
                {
                    LOG.ln("bid is " + player.Seller(t.res).RemovePrice(1) + " prio: " + player.Seller(t.res).Prio());
                }

                int low = 0;
                int high = forSale;
                int am = 0;

                while (low <= high)
                {
                    int mid = low + (high - low) / 2; // avoid overflow
                    int sellPrice = h.p.Faction().Seller(t.res).RemovePrice(mid) + TradeManager.TotalFee(h.p.Faction(), player, h.p.Distance(), t.res, mid);
                    double p = player.Buyer(t.res).BuyPriority(mid, sellPrice);

                    if (plog)
                    {
                        LOG.ln("buying " + mid + " pieces will cost " + sellPrice + " " + p);
                    }

                    if (p <= 0 || (h2 != null && p < player.Buyer(t.res).BuyPriority(am, h2.p.Faction().Seller(t.res).RemovePrice(am) + TradeManager.TotalFee(h2.p.Faction(), player, h2.p.Distance(), t.res, am))))
                    {
                        high = mid - 1; // too much, reduce
                    }
                    else
                    {
                        am = mid; // feasible, try more
                        low = mid + 1;
                    }
                }

                if (plog)
                {
                    LOG.ln("player buys " + t.res + " " + am + " for " + sellPrice + " " + sellPrice / am + " from " + h.p.Faction().Name);
                    if (sellPrice > FACTIONS.player().Credits().Credits())
                    {
                        LOG.ln("wtf " + sellPrice + " " + " " + player.Buyer(t.res).BuyPriority(am, sellPrice));
                    }
                }

                h.p.Trade(t.res, am);
                player.Buyer(t.res).AddReserve(am, TRADE_TYPE.trade, sellPrice, h.p.Faction());
                h.p.Faction().Seller(t.res).Remove(am, TRADE_TYPE.trade, sellPrice, player);

                if (plog)
                {
                    LOG.ln("player buying " + am + " for $" + sellPrice);
                }

                am = batch;

                if (h.p.Faction().Seller(t.res).RemoveMax() > batch)
                {
                    int price = h.p.Faction().Seller(t.res).RemovePrice(batch) + TradeManager.TotalFee(h.p.Faction(), player, h.p.Distance(), t.res, batch);
                    double v = player.Buyer(t.res).BuyPriority(batch, price);
                    if (v > 0)
                    {
                        h.price = price;
                        h.value = -price;
                        t.traders.Add(h);
                    }
                }

                if (t.traders.Size() > 0)
                {
                    Holder h = t.traders.Greatest();
                    t.value = player.Buyer(t.res).BuyPriority(batch, h.price);
                    if (t.value > 0)
                        tree.Add(t);
                }
            }
        }

        public void Buy(TradeShipper shipper, Faction buyer)
        {
            tree.Clear();
            int hI = 0;

            if (shipper.Partners() <= 0)
                return;

            if (logAll)
            {
                LOG.ln(buyer.Name + " buy time");
            }

            for (int ri = 0; ri < TR.ALL().Size(); ri++)
            {
                TRADABLE r = TR.ALL().Get(ri);
                ResTree t = resTrees[r.Index()];
                t.traders.Clear();

                if (buyer.Buyer(r).AddPrice(1) <= 0)
                    continue;

                for (int i = 0; i < shipper.Partners(); i++)
                {
                    Partner seller = shipper.Partner(i);

                    int price = seller.Faction().Seller(r).RemovePrice(1) + TradeManager.TotalFee(seller.Faction(), buyer, seller.Distance(), r, 1);
                    if (logAll)
                    {
                        LOG.ln(seller.Faction().Name + " asks " + price);
                    }

                    if (price < 0)
                        continue;

                    if (price > buyer.Buyer(r).AddPrice(1))
                        continue;

                    Holder h = holders[hI++];

                    h.p = seller;
                    h.price = price;
                    h.value = -price;
                    t.traders.Add(h);
                }

                if (logAll)
                {
                    LOG.ln(buyer.Name + " has " + resTrees[r.Index()].traders.Size() + " sellers");
                }

                if (resTrees[r.Index()].traders.Size() > 0)
                {
                    Holder h = resTrees[r.Index()].traders.Greatest();
                    t.value = buyer.Buyer(r).BuyPriority(1, h.price);
                    if (t.value > 0)
                        tree.Add(t);
                }
            }

            while (tree.HasMore())
            {
                ResTree t = tree.PollGreatest();
                Holder seller = t.traders.PollGreatest();

                int am = Math.Min(1, seller.p.Faction().Seller(t.res).RemoveMax());
                int sellPrice = seller.price;
                double v = buyer.Buyer(t.res).BuyPriority(1, sellPrice);

                if (v <= 0 || am <= 0)
                {
                    t.traders.Clear();
                    continue;
                }

                if (logAll)
                {
                    LOG.ln(buyer.Name + " buying " + t.res);
                }

                seller.p.Trade(t.res, am);
                buyer.Buyer(t.res).AddReserve(am, TRADE_TYPE.trade, sellPrice, seller.p.Faction());
                seller.p.Faction().Seller(t.res).Remove(am, TRADE_TYPE.trade, sellPrice, buyer);

                if (logAll)
                {
                    LOG.ln(buyer.Name + " buys " + t.res + " " + am + " for " + sellPrice + " " + sellPrice / am + " from " + seller.p.Faction().Name);
                }

                am = 1;

                if (seller.p.Faction().Seller(t.res).RemoveMax() > 1)
                {
                    int price = seller.p.Faction().Seller(t.res).RemovePrice(1) + TradeManager.TotalFee(seller.p.Faction(), buyer, seller.p.Distance(), t.res, 1);
                    double v = buyer.Buyer(t.res).BuyPriority(1, price);
                    if (v > 0)
                    {
                        seller.price = price;
                        seller.value = -price;
                        t.traders.Add(seller);
                    }
                }

                if (t.traders.Size() > 0)
                {
                    Holder h = t.traders.Greatest();
                    t.value = buyer.Buyer(t.res).BuyPriority(1, h.price);
                    if (t.value > 0)
                        tree.Add(t);
                }
            }
        }

        private static class Holder
        {
            public Partner p;
            public int price;
            private double value;
        }

        private static class ResTree
        {
            public double value;
            public readonly TRADABLE res;

            public ResTree(TRADABLE res)
            {
                this.res = res;
            }

            public readonly Tree<Holder> traders = new Tree<Holder>(FACTIONS.MAX())
            {
                IsGreaterThan = (current, cmp) => current.value > cmp.value
            };
        }
    }
}