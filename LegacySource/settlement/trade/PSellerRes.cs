using game.faction;
using game.faction.FResources;
using game.faction.npc.stockpile;
using init.resources;
using init.trade;
using settlement.main;
using settlement.thing.halfEntity.caravan;
using snake2d.util.gui;
using util.data.INT;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.trade
{
    class PSellerRes : PSeller
    {
        private static readonly CharSequence ¤¤ExportProblem = "¤You don't have any export depots set to this resource. No exporting can be done.";
        private static readonly CharSequence ¤¤ExportFull = "¤Our export depots are full. We must increase their space if we are to export at full capacity.";
        private static readonly CharSequence ¤¤prio = "¤Priority Limit";
        private static readonly CharSequence ¤¤prioDD = "¤Export workers will only fetch from warehouses when at least {0}% of our warehouse crates are filled (At least {1} items stored). {2} items can currently be fetched.";
        private static readonly CharSequence ¤¤ImportCapacity = "¤Export Capacity";
        private static readonly CharSequence ¤¤ImportCapacityUsed = "¤Export Capacity Used";
        private static readonly CharSequence ¤¤ImportCanBe = "¤Exports available";

        static PSellerRes()
        {
            D.ts(typeof(PSellerRes));
        }

        private readonly TRADABLEO<RESOURCE> res;

        public PSellerRes(TRADABLEO<RESOURCE> type) : base(type, new IntImp(0, 100))
        {
            this.res = type;
        }

        public override void clear()
        {
            limit.set(25);
            base.clear();
        }

        public override int playerOwned()
        {
            return res.t.owned();
        }

        public override double prio()
        {
            double cap = SETT.ROOMS().EXPORT.tally.capacity.get(res.t);
            if (cap <= 0)
                return 0;
            double ava = SETT.ROOMS().EXPORT.tally.amount.get(res.t) - promised.get(null);
            if (ava <= 0)
                return 0;

            return ava / cap;
        }

        public override int removeMax()
        {
            if (SETT.ENTRY().isClosed())
                return 0;
            return SETT.ROOMS().EXPORT.tally.amount.get(res.t) - promised.get(null);
        }

        public override int attempting(TRADE_TYPE t)
        {
            return SETT.HALFENTS().caravans.withdrawals(res.t, t);
        }

        protected override int extract(int amount, TRADE_TYPE t)
        {
            int am = 0;

            while (amount > 0)
            {
                int a = Math.Min(amount, Caravan.MAX_LOAD);
                if (!SETT.HALFENTS().caravans.createFetcher(res.t, a, t))
                    return am;
                am += a;
                amount -= a;
            }
            return am;
        }

        public override void vanish(int amount, RTYPE t)
        {
            res.t.remove(amount, t);
        }

        public override int storedHistorically(int daysBack)
        {
            return SETT.ROOMS().STOCKPILE.tally().amountsDay().get(res.t.index()).get(daysBack);
        }

        public override CharSequence warning()
        {
            if (SETT.ROOMS().EXPORT.tally.amount.get(res.t) >= SETT.ROOMS().EXPORT.tally.capacity.get(res.t))
            {
                return ¤¤ExportFull;
            }

            return base.warning();
        }

        public override CharSequence exporting()
        {
            if (SETT.ROOMS().EXPORT.tally.capacity.get(res.t) > 0)
                return null;
            return ¤¤ExportProblem;
        }

        public override double capacityValue()
        {
            double am = SETT.ROOMS().EXPORT.tally.capacity.get(res.t);
            if (am == 0)
                return 0;

            return SETT.ROOMS().EXPORT.tally.amount.get(res.t) / am;
        }

        public override int tradeCredits(int price)
        {
            double rate = SETT.RECIPES().player.rateTotal(type);
            return SETT.TRADE().tradeCredits(price, rate);
        }

        public override void hoverTradeValue(GUI_BOX box)
        {
            double price = FACTIONS.player().trade.pricesSell.get(type);
            hoverTradeValue(price, box);
        }

        public override double tradeValue(int price)
        {
            double p = tradeCredits(price);
            p /= NPCStockpile.AVERAGE_PRICE;
            return p;
        }

        public override void hoverCapacity(GUI_BOX bob)
        {
            base.hoverCapacity(bob);

            GBox b = (GBox)bob;

            b.textLL(¤¤ImportCapacity);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), SETT.ROOMS().EXPORT.tally.capacity.get(res.t)));
            b.NL();

            b.textLL(¤¤ImportCapacityUsed);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), SETT.ROOMS().EXPORT.tally.amount.get(res.t)));
            b.NL();

            b.textLL(¤¤ImportCanBe);
            b.tab(6);
            b.add(GFORMAT.i(b.text(),
                    SETT.ROOMS().EXPORT.tally.capacity.get(res.t) - SETT.ROOMS().EXPORT.tally.amount.get(res.t)));
            b.NL();

            b.textLL(¤¤prio);
            b.NL();
            GText t = b.text();
            t.add(¤¤prioDD);
            t.insert(0, 100 - limit.get());
            t.insert(1, SETT.ROOMS().EXPORT.storedShouldBeHigherThan(res.t));
            t.insert(2, SETT.ROOMS().EXPORT.toFetchToExport(res.t));
            b.add(t);
            b.NL();

            b.textLL(SETT.ROOMS().STOCKPILE.info.names);
            b.NL();
            b.textL(SETT.ROOMS().STOCKPILE.tally().space.name);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), SETT.ROOMS().STOCKPILE.tally().space.total(res.t)));
            b.NL();
            b.textL(SETT.ROOMS().STOCKPILE.tally().amount.name);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), SETT.ROOMS().STOCKPILE.tally().amount.total(res.t)));
            b.NL();

            base.hoverCapacity(bob);
        }
    }
}