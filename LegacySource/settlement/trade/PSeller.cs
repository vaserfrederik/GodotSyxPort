using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc.stockpile;
using game.faction.trade;
using init.trade;
using settlement.main;
using snake2d.util.file;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world.region;

namespace settlement.trade
{
    public abstract class PSeller : FSELLER, SAVABLE
    {
        private static readonly CharSequence ¤¤priceCapProblem = "The current price is below your price cap. To trade, you must disable or decrease the price cap.";
        private static readonly CharSequence ¤¤NoPrice = "¤There is no one willing to buy this goods. You must decrease either the tariff or the toll.";
        private static readonly CharSequence ¤¤NoGoods = "¤There are no goods available to sell.";
        private static readonly CharSequence ¤¤Owned = "¤Owned";
        private static readonly CharSequence ¤¤Inbound = "¤Outbound";
        private static readonly CharSequence ¤¤Forsale = "¤For Sale";
        private static readonly CharSequence ¤¤PriceCap = "¤Price Cap";
        private static readonly CharSequence ¤¤BestPrice = "¤Best price.";

        private static readonly CharSequence ¤¤Profit = "¤Profit";
        private static readonly CharSequence ¤¤ProfitD = "¤Your workers can currently produce this ware at the speed of {0} items per day per worker. If you sell such an amount at the price of {1}, you will earn {2} denarii.";

        static PSeller()
        {
            D.ts(typeof(PSeller));
        }

        public readonly TRADABLE type;
        private readonly TradableData promised = new TradableData();
        public IntImp priceCapsI = new IntImp(1, 10000000);
        public readonly IntImp limit;

        public readonly INT_O<TRADE_TYPE> outbound = new INT_O<TRADE_TYPE>
        {
            Min = (t) => 0,
            Max = (t) => int.MaxValue,
            Get = (t) => promised.Get(t) + Attempting(t)
        };

        protected PSeller(TRADABLE type, IntImp limit)
        {
            this.type = type;
            this.limit = limit;
        }

        public void Save(FilePutter file)
        {
            promised.Save(file);
            priceCapsI.Save(file);
            limit.Save(file);
        }

        public void Load(FileGetter file)
        {
            promised.Load(file);
            priceCapsI.Load(file);
            if (!VERSION.versionIsBefore(71, 14))
                limit.Load(file);
        }

        public void Clear()
        {
            promised.Clear();
            priceCapsI.Clear();
            limit.Clear();
        }

        public abstract int PlayerOwned();

        public abstract CharSequence Exporting();

        public abstract int Attempting(TRADE_TYPE t);

        public abstract int StoredHistorically(int daysBack);

        protected void Extract()
        {
            if (SETT.ENTRY().IsClosed())
                return;

            if (promised.Get(null) <= 0)
                return;

            foreach (TRADE_TYPE tt in TRADE_TYPE.all)
            {
                int am = promised.Get(tt);
                promised.Inc(tt, -Extract(am, tt));
            }
        }

        public INT_O<TRADE_TYPE> Promised()
        {
            return promised;
        }

        protected abstract int Extract(int amount, TRADE_TYPE t);

        public abstract double Prio();

        public int RemovePrice(int amount)
        {
            return priceCapsI.Get() * amount;
        }

        public void Remove(int amount, TRADE_TYPE type, int price, Faction buyer)
        {
            FACTIONS.player().credits().Inc(price, type.ctype, this.type, amount);
            promised.Inc(type, amount);
            GAME.count().TRADE_SALES.Inc(price);
        }

        public abstract void Vanish(int amount, RTYPE t);

        public CharSequence Problem()
        {
            if (RD.DIST().neighs().Size() == 0)
                return ¤¤noTrade;

            if (DIP.traders().Size() == 0)
                return ¤¤noTradePartners;

            if (FACTIONS.player().trade.pricesSell.Get(type) <= 0)
                return ¤¤NoPrice;

            return null;
        }

        public CharSequence Warning()
        {
            if (PlayerOwned() <= 0)
                return ¤¤NoGoods;

            if (FACTIONS.player().trade.pricesSell.Get(type) > 0 && FACTIONS.player().trade.pricesSell.Get(type) < priceCapsI.Get())
                return ¤¤priceCapProblem;

            return null;
        }

        public void Hover(GUI_BOX bob)
        {
            GBox b = (GBox)bob;
            b.title(type.names);

            if (Problem() != null)
            {
                b.error(Problem());
                b.NL();
            }
            else if (Warning() != null)
            {
                b.error(Warning());
                b.NL();
            }

            HoverCapacity(b);
            b.NL(8);

            b.textLL(¤¤PriceCap);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), priceCapsI.Get()));
            b.NL();

            b.textLL(¤¤BestPrice);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesSell.Get(type)));
            b.NL();
            HoverTradeValue(bob);
        }

        public abstract double CapacityValue();

        public void HoverCapacity(GUI_BOX bob)
        {
            GBox b = (GBox)bob;
            b.textLL(¤¤Owned);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), PlayerOwned()));
            b.NL();

            b.textLL(¤¤Inbound);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), outbound.Get(null)));
            b.NL();
            foreach (TRADE_TYPE t in TRADE_TYPE.all)
            {
                int am = outbound.Get(t);
                if (am > 0)
                {
                    b.tab(1);
                    b.textL(t.name);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), am));
                }
                b.NL();
            }
            b.textLL(¤¤Forsale);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), RemoveMax()));
            b.NL();
        }

        public int TradeCredits()
        {
            return TradeCredits(FACTIONS.player().trade.pricesBuy.Get(type));
        }

        public double TradeValue()
        {
            double p = TradeCredits();
            p /= NPCStockpile.AVERAGE_PRICE;
            return p;
        }

        public abstract int TradeCredits(int price);

        public abstract double TradeValue(int price);

        public abstract void HoverTradeValue(GUI_BOX box);

        public void HoverTradeValue(double price, GUI_BOX box)
        {
            GBox b = (GBox)box;
            b.NL();
            double rate = SETT.RECIPES().player.rateTotal(type);
            double d = price / rate;
            b.textLL(¤¤Profit);
            b.tab(6);
            b.add(GFORMAT.iIncr(b.text(), (int)d));
            b.NL();
            GText t = b.text();
            t.add(¤¤ProfitD);
            t.insert(0, 1.0 / rate, 1);
            t.insert(1, (int)price);
            t.insert(2, (int)d);
            b.add(t);

            b.NL();
        }
    }
}