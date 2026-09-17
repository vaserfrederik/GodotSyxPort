using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Settlement.Trade
{
    public abstract class PBuyer : FBUYER, SAVABLE
    {
        private static readonly string ¤¤probPriceCap = "The current cheapest buy price exceeds your set price cap. To trade, you must disable or increase the price cap.";
        private static readonly string ¤¤probCredits = "You don't have enough credits to purchase this resource.";
        private static readonly string ¤¤probTreasury = "A purchase of a batch of 32 of this resource is not possible due to your treasury limit.";
        private static readonly string ¤¤probClosed = "The city is unreachable, no trade can be done.";

        static readonly string ¤¤Owned = "Owned";
        static readonly string ¤¤Inbound = "Inbound";
        static readonly string ¤¤ImportCapacity = "Import Capacity";
        static readonly string ¤¤ImportCapacityUsed = "Import Capacity Used";
        static readonly string ¤¤ImportCanBe = "Imports available";

        public static readonly string ¤¤PriceCap = "Price Cap";

        public static readonly string ¤¤TreasuryLim = "Treasury Limit";
        public static readonly string ¤¤TradeQuota = "Trade Quota";

        private static readonly string ¤¤BestPrice = "Best price.";

        static PBuyer()
        {
            D.ts(typeof(PBuyer));
        }

        public readonly TRADABLE tradable;
        public readonly IntImp priceCapsI = new IntImp(0, 1000000);
        public readonly IntImp minMoney = new IntImp(1, 10000000);
        public readonly IntImp limit;
        private TradableData toBeAdded = new TradableData();
        private TradableData toBeStored = new TradableData();

        public readonly INT_O<TRADE_TYPE> incoming = new INT_O<TRADE_TYPE>()
        {
            public int min(TRADE_TYPE t) => 0,
            public int max(TRADE_TYPE t) => int.MaxValue,
            public int get(TRADE_TYPE t) => toBeAdded.get(t) + toBeStored.get(t) + attempting(t)
        };

        protected PBuyer(TRADABLE tradable, IntImp buyLimit)
        {
            this.tradable = tradable;
            this.limit = buyLimit;
        }

        public void save(FilePutter file)
        {
            toBeAdded.save(file);
            toBeStored.save(file);
            priceCapsI.save(file);
            minMoney.save(file);
            limit.save(file);
            file.bool(false);
        }

        public void load(FileGetter file)
        {
            toBeAdded.load(file);
            toBeStored.load(file);
            priceCapsI.load(file);
            minMoney.load(file);
            limit.load(file);
            file.bool();

            if (VERSION.versionIsBefore(71, 12))
                toBeAdded.clear();
        }

        public void clear()
        {
            toBeAdded.clear();
            toBeStored.clear();
            priceCapsI.set(priceCapsI.max);
            minMoney.set(1);
            limit.clear();
        }

        public abstract bool importing();
        public abstract int attempting(TRADE_TYPE t);

        public void addReserve(int amount, TRADE_TYPE type, int price, Faction seller)
        {
            FACTIONS.player().credits().inc(-price, type.ctype, tradable, amount);
            GAME.count().TRADE_PURCHASES.inc(price);
            toBeAdded.inc(type, amount);
        }

        public void addDeliver(int amount, TRADE_TYPE type)
        {
            toBeAdded.inc(type, -amount);
            toBeStored.inc(type, amount);
        }

        protected final void deliver()
        {
            if (SETT.ENTRY().isClosed())
                return;

            if (toBeStored.get(null) <= 0)
                return;

            foreach (TRADE_TYPE tt in TRADE_TYPE.all)
            {
                int am = toBeStored.get(tt);

                if (am <= 0)
                    continue;

                toBeStored.inc(tt, -deliver(tt, am));
            }
        }

        protected abstract int deliver(TRADE_TYPE tt, int amount);

        public double buyPriority(int amount, double price)
        {
            if (SETT.ENTRY().isClosed())
                return 0;

            if (FACTIONS.player().credits().getD() - price < minMoney.get())
                return 0;

            if (price / amount >= priceCapsI.get())
                return 0;
            if (GAME.player().credits().credits() < price)
            {
                return 0;
            }

            return amount / price;
        }

        public int addPrice(int amount)
        {
            return 0;
        }

        public INT_O<TRADE_TYPE> toBeAdded()
        {
            return toBeAdded;
        }

        public INT_O<TRADE_TYPE> toBeStored()
        {
            return toBeStored;
        }

        public CharSequence problem()
        {
            if (SETT.ENTRY().isClosed())
            {
                return ¤¤probClosed;
            }

            if (RD.DIST().neighs().size() == 0)
                return ¤¤noTrade;

            if (DIP.traders().size() == 0)
                return ¤¤noTradePartners;

            if (FACTIONS.player().trade.pricesBuy.get(tradable) == 0)
                return ¤¤noTrade;

            return null;
        }

        public CharSequence warning()
        {
            int pr = FACTIONS.player().trade.pricesBuy.get(tradable);

            if (pr > 0 && pr > FACTIONS.player().credits().getD())
            {
                return Str.TMP.clear().add(¤¤probCredits);
            }

            if (pr != int.MaxValue && pr > priceCapsI.get())
                return ¤¤probPriceCap;

            if (FACTIONS.player().credits().getD() - pr < minMoney.get())
                return ¤¤probTreasury;

            return null;
        }

        public void hover(GUI_BOX box)
        {
            GBox b = (GBox)box;
            b.title(tradable.name);

            CharSequence p = problem();
            if (p != null)
            {
                b.error(p);
            }
            else
            {
                p = warning();
                if (p != null)
                {
                    b.add(b.text().warnify().add(p));
                }
            }

            b.NL(4);

            hoverCapacity(b);
            b.NL(8);

            b.textLL(¤¤PriceCap);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), priceCapsI.get()));
            b.NL();

            b.textLL(¤¤TreasuryLim);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), minMoney.get()));
            b.NL();

            b.textLL(¤¤BestPrice);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), FACTIONS.player().trade.pricesBuy.get(tradable)));
            b.NL();

            hoverTradeValue(box);

        }

        public abstract void hoverCapacity(GBox b);

        public final int tradeCredits()
        {
            return tradeCredits(FACTIONS.player().trade.pricesSell.get(tradable));
        }

        public abstract int tradeCredits(int price);

        public abstract double tradeValue(int price);

        public final double tradeValue()
        {
            double p = tradeCredits();
            p /= NPCStockpile.AVERAGE_PRICE;
            return p;
        }

        public final void hoverTradeValue(GUI_BOX box)
        {
            double price = FACTIONS.player().trade.pricesBuy.get(tradable);
            hoverTradeValue(price, box);
        }

        public abstract void hoverTradeValue(double price, GUI_BOX box);

        public abstract double capacityValue();
    }
}