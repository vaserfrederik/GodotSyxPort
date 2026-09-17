using init.race;
using init.trade;
using init.type;
using settlement.entity;
using settlement.main;
using settlement.stats;
using snake2d.util.gui;
using snake2d.util.misc;
using util.data;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.trade
{
    class PBuyerSlave : PBuyer
    {
        private static CharSequence ¤¤warning = "¤You don't have any capacity to import more captives.";
        private static CharSequence ¤¤LevelNever = "¤Limit is set to never Import {0}.";
        private static CharSequence ¤¤LevelCurrent = "¤Limit is set to import to maintain {0} {1}.";

        private static CharSequence ¤¤punishFree = "¤Your automatic punishment for captives is set to either pardon or exile! Imported captives will instantly leave your city!";
        private static CharSequence ¤¤punishSlave = "¤Your automatic punishment for captives is set to enslavement. Captives that arrive will be instantly turned to slaves, and your limit will refer to your slave population.";
        private static CharSequence ¤¤punishOther = "¤Your automatic punishment for captives is set so that the limit refers to your stockade and prisoners.";

        static PBuyerSlave()
        {
            D.ts(typeof(PBuyerSlave));
        }

        private readonly TRADABLEO<Race> slave;

        public PBuyerSlave(TRADABLEO<Race> tradable) : base(tradable, new IntImp(0, ENTETIES.MAX))
        {
            this.slave = tradable;
        }

        public override int Attempting(TRADE_TYPE t)
        {
            return SETT.ENTRY().onTheirWay(slave.t, HTYPES.PRISONER());
        }

        protected override int Deliver(TRADE_TYPE tt, int amount)
        {
            SETT.ENTRY().add(slave.t, HTYPES.PRISONER(), amount);
            return amount;
        }

        public override bool Importing()
        {
            return limit.get() > 0;
        }

        public override double BuyPriority(int amount, double price)
        {
            Type t = type();
            int owned = t.capacityUsed();
            int lim = t.capacity();
            if (owned + amount > lim)
                return -1;
            return base.BuyPriority(amount, price);
        }

        public override CharSequence Warning()
        {
            Type t = type();
            int owned = t.capacityUsed();
            int lim = t.capacity();

            if (owned >= lim)
                return ¤¤warning;
            return base.Warning();
        }

        public override void HoverCapacity(GBox b)
        {
            Type ty = type();

            type().hover(b);

            b.NL(4);

            {
                GText t = b.text();
                double lim = limit.get();
                if (lim == 0)
                {
                    t.add(¤¤LevelNever);
                    t.insert(0, slave.t.info.names);
                    b.add(t);
                    return;
                }

                t.add(¤¤LevelCurrent);
                t.insert(0, slave.t.info.names);
                t.insert(1, limit.get());
                b.add(t);
            }
            b.NL(4);

            b.textLL(¤¤Owned);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), ty.capacityUsed()));
            b.NL();

            b.textLL(¤¤Inbound);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), incoming.get(null)));
            b.NL();


            b.textLL(¤¤ImportCanBe);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), ty.capacity() - ty.capacityUsed()));
            b.NL();
        }

        public override int TradeCredits(int price)
        {
            return 0;
        }

        public override double TradeValue(int price)
        {
            return 0;
        }

        public override void HoverTradeValue(double price, GUI_BOX box)
        {
        }

        public override double CapacityValue()
        {
            double cap = type().capacity();
            if (cap == 0)
                return 0;
            return CLAMP.d(type().capacityUsed() / cap, 0, 1);
        }

        private Type type()
        {
            PUNISHMENT p = CRIMES.WAR().stat().punishment(HCLASSES.OTHER(), slave.t).punish;
            if (p == CRIME_PUNISHMENTS.BANISH() || p == CRIME_PUNISHMENTS.PARDON())
            {
                return tfree;
            }
            else if (p == CRIME_PUNISHMENTS.ENSLAVE())
            {
                return tslave;
            }
            else
            {
                return tstock;
            }
        }

        private interface Type
        {
            int capacity();
            int capacityUsed();
            void hover(GBox b);
        }

        private Type tslave = new Type()
        {
            public int capacityUsed()
            {
                return POP.next(HCLASSES.SLAVE(), slave.t) + toBeAdded().get(null) + toBeStored().get(null);
            }

            public int capacity()
            {
                return limit.get();
            }

            public void hover(GBox b)
            {
                b.text(¤¤punishSlave);
            }
        };

        private Type tfree = new Type()
        {
            public int capacityUsed()
            {
                return incoming.get(null);
            }

            public int capacity()
            {
                return limit.get();
            }

            public void hover(GBox b)
            {
                b.warn(¤¤punishFree);
            }
        };

        private Type tstock = new Type()
        {
            public int capacityUsed()
            {
                return toBeAdded().get(null) + toBeStored().get(null) + SETT.ROOMS().STOCKADE.punishUsed();
            }

            public int capacity()
            {
                return Math.Min(SETT.ROOMS().STOCKADE.punishTotal() - (Math.Max(CRIMES.WAR().stat().criminals(null), SETT.ROOMS().STOCKADE.punishUsed()) + incoming.get(null)), limit.get());
            }

            public void hover(GBox b)
            {
                b.warn(¤¤punishOther);
            }
        };
    }
}