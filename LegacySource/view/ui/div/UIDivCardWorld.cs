using System;
using System.Collections.Generic;
using game.battle.util;
using game.faction;
using init.race;
using init.resources;
using init.sprite;
using settlement.main;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.info;
using util.text;
using world.army;
using world.entity.army;

public sealed class UIDivCardWorld : DIMENSION
{
    private static readonly string ¤¤supHealth = "Supplies Health";
    private static readonly string ¤¤supHealthD = "Supplies that are needed for general health. Initial supplies for 8 days of use must be available in warehouses, and will be sent to the army automatically.";
    private static readonly string ¤¤supMorale = "Supplies Morale";
    private static readonly string ¤¤supMoraleD = "Supplies that boosts morale. Will be sent if available automatically, but are not mandatory.";
    private static readonly string ¤¤supWarning = "Warning: you currently do not have any operational army supply depots for the needed resources! Once the supplies are gone, the unit might rout and be lost.";
    private static readonly string ¤¤LowSupplies = "¤Not enough supplies to send out. Fill up your warehouses of essential army supplies.";

    private static readonly string ¤¤NewConscripts = "¤Conscripts are training and will be ready in {0} days.";
    private static readonly string ¤¤NewConscriptsProblem = "¤There are no conscripts to train for this division.";
    private static readonly string ¤¤Training = "¤This division is currently training to reach the desired training level. Days left: {0}.";
    private static readonly string ¤¤NotMustering = "¤This army is currently not mustering, and will not train conscripts.";

    static UIDivCardWorld()
    {
        D.ts(typeof(UIDivCardWorld));
    }

    private readonly GText tmp = new GText(UI.FONT().S, 5);

    private readonly Rec body = new Rec();
    private readonly int WIDTH;
    private readonly int HEIGHT;
    private readonly UIDiv _uidiv;

    private readonly GuiSection sec = new GuiSection();
    private readonly UIDivStat stat = new UIDivStat();

    private WDiv current;

    public UIDivCardWorld(UIDiv uidiv)
    {
        _uidiv = uidiv;
        WIDTH = 300;
        HEIGHT = 300;
    }

    public override int Width => WIDTH;
    public override int Height => HEIGHT;

    public void hover(WDiv d, GUI_BOX box)
    {
        GBox b = (GBox)box;
        b.title(d.Name);

        current = d;
        b.add(sec);
        b.NL();

        b.add(stat.get(d));

        b.NL(8);
        b.sep();

        if (d.CostPerMan > 0)
        {
            b.textL(Dic.¤¤InitialCost);
            b.tab(3);
            b.add(GFORMAT.i(b.text(), 4 * d.CostPerMan * d.MenTarget));
            b.NL();

            b.textL(Dic.¤¤Upkeep);
            b.tab(3);
            b.add(GFORMAT.i(b.text(), d.CostPerMan * d.MenTarget));
            b.NL();
        }

        if (d.NeedConscripts)
        {
            if (d.Army.Recruiting)
            {
                if (d.Men < d.MenTarget)
                {
                    if (!AD.conscripts().canTrain(d.Race, d.Faction))
                    {
                        b.error(¤¤NewConscriptsProblem);
                    }
                    else
                    {
                        GText te = b.text();
                        te.add(¤¤NewConscripts);
                        te.insert(0, d.DaysUntilMenArrives.ToString());
                        te.normalify2();
                        b.add(te);
                    }
                }
                else
                {
                    int tt = trainingTime(d);
                    if (tt > 0)
                    {
                        GText te = b.text();
                        te.add(¤¤Training);
                        te.insert(0, tt.ToString());
                        te.normalify2();
                        b.add(te);
                    }
                }
            }
            else if (d.Men < d.MenTarget || trainingTime(d) > 0)
            {
                b.error(¤¤NotMustering);
            }
        }
    }

    static int trainingTime(WDiv div)
    {
        int m = 0;
        foreach (StatTraining tr in STATS.BATTLE().TRAINING_ALL)
        {
            m += WDivRegional.trainingDays(tr, div.Target.Training[tr] - div.Training[tr], div.Faction);
        }
        return m;
    }

    public static string supplyError(DIV_SIMPLE div)
    {
        foreach (ResSupply s in RESOURCES.SUP().ALL)
        {
            if (s.Health > 0 && s.Amount(div.Race, div.Men) > SETT.ROOMS().STOCKPILE.tally().amountReservable[s.Resource])
                return ¤¤LowSupplies;
        }
        return null;
    }

    public static void hoverSendOut(IList<DIV_SIMPLE> divs, GUI_BOX box)
    {
        GBox b = (GBox)box;

        bool sup = true;

        b.textLL(¤¤supHealth);
        b.NL();
        b.text(¤¤supHealthD);
        b.NL();
        foreach (ResSupply s in RESOURCES.SUP().ALL)
        {
            if (s.Health <= 0)
                continue;
            int need = 0;
            foreach (DIV_SIMPLE div in divs)
            {
                need += s.Amount(div.Race, div.Men);
            }

            int available = SETT.ROOMS().STOCKPILE.tally().amountReservable[s.Resource];

            b.add(s.Resource.Icon);
            GText t = b.text();
            GFORMAT.i(t, -need);
            if (need <= available)
                t.normalify2();
            else
                t.errorify();
            b.add(t);

            b.tab(3);
            b.add(SETT.ROOMS().STOCKPILE.Icon.Small);
            b.add(GFORMAT.i(b.text(), available));

            b.tab(6);

            if (SETT.ROOMS().SUPPLY.Has(s.Resource))
                b.add(SETT.ROOMS().SUPPLY.Icon.Small);
            else
                b.add(UI.icons().s.cancel, GCOLOR.UI().BAD.Hovered);

            b.NL();
            if (need > 0 && !SETT.ROOMS().SUPPLY.Has(s.Resource))
                sup = false;
        }

        b.textLL(¤¤supMorale);
        b.NL();
        b.text(¤¤supMoraleD);
        b.NL();
        foreach (ResSupply s in RESOURCES.SUP().ALL)
        {
            if (s.Morale <= 0)
                continue;
            int need = 0;
            foreach (DIV_SIMPLE div in divs)
            {
                need += s.Amount(div.Race, div.Men);
            }

            int available = SETT.ROOMS().STOCKPILE.tally().amountReservable[s.Resource];

            b.add(s.Resource.Icon);
            GText t = b.text();
            GFORMAT.i(t, -need);
            if (need <= available)
                t.normalify2();
            else
                t.warnify();
            b.add(t);

            b.tab(3);
            b.add(SETT.ROOMS().STOCKPILE.Icon.Small);
            b.add(GFORMAT.i(b.text(), available));

            b.tab(6);

            if (SETT.ROOMS().SUPPLY.Has(s.Resource))
                b.add(SETT.ROOMS().SUPPLY.Icon.Small);
            else
                b.add(UI.icons().s.cancel, GCOLOR.UI().BAD.Hovered);

            b.NL();
            if (need > 0 && !SETT.ROOMS().SUPPLY.Has(s.Resource))
                sup = false;
        }

        if (!sup)
        {
            b.add(SETT.ROOMS().SUPPLY.Icon);
            b.warn(¤¤supWarning);
            b.NL();
        }
    }
}