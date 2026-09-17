using System;
using System.Collections.Generic;
using System.Linq;
using settlement.trade;
using game.faction.npc.stockpile;
using init.resources;
using init.settings;
using init.trade;
using settlement.main;
using settlement.thing.halfEntity.caravan;
using snake2d.util.gui;
using snake2d.util.misc;
using util.data;
using util.gui.misc;
using util.info;
using util.text;

class PBuyerRes : PBuyer
{
    private static string ¤¤ImportProblem = "¤You don't have any import depots set to this resource. No automated importing can be done.";
    private static string ¤¤ImportFull = "¤Our import depots are full. We must increase their space, or improve logistics, if we are to import more.";
    private static string ¤¤LevelEverything = "¤100% Imports maximum to fill both warehouses and import depots.";
    private static string ¤¤LevelNothing = "¤Never import.";
    private static string ¤¤LevelCurrent = "¤Import to maintain warehouse stock at {0}% of total capacity ({1} items). You will currently import {2} additional items.";
    private static string ¤¤LevelNothingW = "¤Your import level is set to 0%, meaning you will never import anything.";
    private static string ¤¤Profit = "¤Profit";
    private static string ¤¤ProfitD = "¤Your workers can currently produce this ware at the speed of {0} items per day per worker. If you buy such an amount at the price of {1} each, it will cost you {2} denarii.";
    private static string ¤¤probCapacityC = "¤You currently don't have import depot capacity to import this goods.";

    static PBuyerRes()
    {
        D.ts(typeof(PBuyerRes));
    }

    public readonly TRADABLEO<RESOURCE> res;

    public PBuyerRes(TRADABLEO<RESOURCE> res) : base(res, new IntImp(0, 100))
    {
        this.res = res;
        limit.setD(1.0);
    }

    public override int attempting(TRADE_TYPE t)
    {
        return SETT.HALFENTS().caravans.deliveries(res.t, t);
    }

    public override void clear()
    {
        limit.setD(1.0);
        base.clear();
    }

    public int capacityTotal()
    {
        return SETT.ROOMS().IMPORT.tally.capacity.get(res.t);
    }

    public int capacityUsed()
    {
        return SETT.ROOMS().IMPORT.tally.capacity.get(res.t) - SETT.ROOMS().IMPORT.tally.spaceForTribute(res.t);
    }

    public int capacityAvailable()
    {
        return capacityTotal() - capacityUsed();
    }

    protected override int deliver(TRADE_TYPE tt, int amount)
    {
        int am = 0;
        while (amount > 0)
        {
            int a = Math.Min(amount, Caravan.MAX_LOAD);

            if (!SETT.HALFENTS().caravans.createDelivery(res.t, a, tt))
                return am;
            am += a;
            amount -= a;
        }
        return am;
    }

    public int owned()
    {
        return (int)SETT.ROOMS().STOCKPILE.tally().spaceReserved.total(res.t) + SETT.ROOMS().STOCKPILE.tally().amount.total(res.t) + capacityUsed() + incoming.get(null);
    }

    public override double buyPriority(int amount, double price)
    {
        if (base.buyPriority(amount, price) <= 0)
            return 0;

        if (capacityTotal() <= 0)
            return 0;

        if (capacityAvailable() <= 0)
            return 0;

        double p = 1.0 - (double)incoming.get(null) / capacityTotal();

        double d = limit.getD();
        if (d == 1.0)
            return base.buyPriority(amount, price) * p;
        if (d == 0)
            return -1;

        if (capacityTotal() <= 0)
            return -1;

        int sspace = (int)SETT.ROOMS().STOCKPILE.tally().space.total(res.t);
        int samount = owned();

        double dd = (double)(samount + amount) / sspace;

        if (dd >= d)
            return -1;

        return base.buyPriority(amount, price);
    }

    public override string problem()
    {
        if (capacityTotal() <= 0)
        {
            return ¤¤ImportProblem;
        }
        return base.problem();
    }

    public override string warning()
    {
        if (capacityTotal() > 0)
        {
            if (limit.getD() == 0)
            {
                return ¤¤LevelNothingW;
            }
            if (capacityAvailable() <= 0)
                return ¤¤ImportFull;

            if (capacityAvailable() < 1)
            {
                return ¤¤probCapacityC;
            }
        }
        return base.warning();
    }

    public override void hoverCapacity(GBox b)
    {
        int space = (int)SETT.ROOMS().STOCKPILE.tally().space.total(res.t);
        int amount = owned();

        if (limit.getD() == 1)
        {
            b.text(¤¤LevelEverything);
        }
        else if (limit.getD() == 0)
        {
            b.text(¤¤LevelNothing);
        }
        else
        {
            double lim = limit.get() / (limit.max() - 1.0);

            int imp = (int)CLAMP.d(lim * space - amount, 0, space);

            GText t = b.text();
            t.add(¤¤LevelCurrent);

            t.insert(0, (int)(Math.Round(100 * lim)));
            t.insert(1, (int)(lim * space));
            t.insert(2, imp);
            b.text(t);
        }
        b.NL();
        b.textLL(¤¤Owned);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), tradable.ps().playerOwned()));
        b.NL();

        b.textLL(¤¤Inbound);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), incoming.get(null)));
        b.NL();
        foreach (TRADE_TYPE t in TRADE_TYPE.all)
        {
            int am = toBeAdded().get(t) + toBeStored().get(t);
            if (am > 0)
            {
                b.tab(1);
                b.textL(t.name);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), am));
                if (S.get().developer)
                {
                    b.text(toBeAdded().get(t) + " " + toBeStored().get(t) + " " + attempting(t));
                }
            }
            b.NL();
        }

        b.textLL(¤¤ImportCapacity);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), capacityTotal()));
        b.NL();

        b.textLL(¤¤ImportCapacityUsed);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), capacityUsed()));
        b.NL();

        b.textLL(¤¤ImportCanBe);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), capacityAvailable()));
        b.NL();
    }

    public override bool importing()
    {
        return capacityTotal() > 0;
    }

    public override int tradeCredits(int price)
    {
        double rate = SETT.RECIPES().player.rateTotal(tradable);
        return SETT.TRADE().tradeCredits(price, rate);
    }

    public override double tradeValue(int price)
    {
        double p = tradeCredits(price);
        p /= NPCStockpile.AVERAGE_PRICE;
        return p;
    }

    public override void hoverTradeValue(double price, GUI_BOX box)
    {
        GBox b = (GBox)box;
        b.NL();
        double rate = SETT.RECIPES().player.rateTotal(tradable);
        double d = price / rate;
        b.textLL(¤¤Profit);
        b.tab(6);
        b.add(GFORMAT.iIncr(b.text(), -(int)d));
        b.NL();
        GText t = b.text();
        t.add(¤¤ProfitD);
        t.insert(0, 1.0 / rate, 1);
        t.insert(1, (int)price);
        t.insert(2, (int)d);

        b.add(t);

        b.NL();
    }

    public override double capacityValue()
    {
        double cap = capacityTotal();
        if (cap <= 0)
            return 0;
        double c = capacityUsed();
        double n = c + incoming.get(null);
        n /= cap;
        return n;
    }
}