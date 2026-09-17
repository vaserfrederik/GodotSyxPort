using System;
using System.Collections.Generic;
using System.Linq;
using settlement.trade;
using game.faction.FResources;
using game.faction;
using init.race;
using init.settings;
using init.trade;
using init.type;
using settlement.entity;
using settlement.main;
using settlement.stats;
using snake2d.util.gui;
using util.data;
using util.gui.misc;
using util.text;

class PSellerSlave : PSeller
{
    private static readonly CharSequence ¤¤limit = "¤Limit: {0}. (Export when you own more than {1} slaves.)";
    private static readonly CharSequence ¤¤none = "¤Never Export slaves";
    private static readonly CharSequence ¤¤all = "¤Limit is set to export all slaves";
    private static readonly CharSequence ¤¤nope = "¤You have set the limit to never export slaves.";

    static PSellerSlave()
    {
        D.ts(typeof(PSellerSlave));
    }

    private readonly TRADABLEO<Race> slave;

    PSellerSlave(TRADABLEO<Race> type) : base(type, new IntImp(0, 0, ENTETIES.MAX))
    {
        this.slave = type;
    }

    public override int attempting(TRADE_TYPE t)
    {
        return 0;
    }

    protected override int extract(int amount, TRADE_TYPE t)
    {
        return 0;
    }

    public override void clear()
    {
        base.clear();
        limit.setD(0);
    }

    public override double prio()
    {
        if (removeMax() <= 0)
            return -1;

        return playerOwned() / (1 + outbound.get(null));
    }

    public override void remove(int amount, TRADE_TYPE type, int price, Faction buyer)
    {
        base.remove(amount, type, price, buyer);
    }

    public override int playerOwned()
    {
        int stocked = STATS.POP().pop(slave.t, HTYPES.SLAVE());
        stocked -= outbound.get(null);
        return stocked;
    }

    public override void vanish(int amount, RTYPE t)
    {
        new EntityIterator.Humans()
        {
            aa = amount,
            processAndShouldBreakH = (h, ie) =>
            {
                if (h.indu().hType() == HTYPES.SLAVE())
                {
                    h.kill(false, CAUSE_LEAVES.SOLD());
                    aa--;
                }
                return aa > 0;
            }
        }.iterate();
    }

    public override int storedHistorically(int daysBack)
    {
        return STATS.POP().pop(slave.t, HTYPES.SLAVE(), daysBack);
    }

    public override CharSequence exporting()
    {
        return limit.get() > 0 ? null : ¤¤nope;
    }

    public override void hoverCapacity(GUI_BOX bob)
    {
        base.hoverCapacity(bob);

        GBox b = (GBox)bob;
        GText t = b.text();
        if (limit.getD() == 1.0)
            t.add(¤¤all);
        else if (limit.getD() == 0)
        {
            t.add(¤¤none);
        }
        else
        {
            t.add(¤¤limit);
            t.insert(0, limit.get());
            t.insert(1, limit.max - limit.get());
        }

        bob.add(t);
        bob.NL();
        if (S.get().developer)
        {
            b.text(b.text().add(prio()));
            b.text(b.text().add("" + exporting()));
            b.text(b.text().add(SETT.TRADE().slavesReserved[slave.t.index]));
        }
    }

    public override int removeMax()
    {
        if (SETT.ENTRY().isClosed())
            return 0;
        int mustHave = ENTETIES.MAX - limit.get();
        int am = playerOwned() - outbound.get(null) - mustHave;
        return Math.Max(am, 0);
    }

    public override double capacityValue()
    {
        double owned = STATS.POP().pop(slave.t, HTYPES.SLAVE());
        if (owned == 0)
            return 0;
        return outbound.get(null) / owned;
    }

    public override int tradeCredits(int price)
    {
        return 0;
    }

    public override double tradeValue(int price)
    {
        return 0;
    }

    public override void hoverTradeValue(GUI_BOX box)
    {
    }
}