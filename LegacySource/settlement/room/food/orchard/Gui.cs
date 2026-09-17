using System;
using System.Collections.Generic;
using System.Linq;
using settlement.room.food.orchard;
using game;
using init.sprite;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room;

class Gui : UIRoomModuleImp<Instance, ROOM_ORCHARD>
{
    static CharSequence ¤¤Trees = "¤Trees";
    static CharSequence ¤¤TreesD = "¤Amount of fully grown trees. Only when trees are fully grown will they start producing. Neglected trees will die.";
    private static CharSequence ¤¤TreeNext = "¤Next tree will be grown in:";

    private static CharSequence ¤¤estimated = "¤Estimated Harvest (year)";
    private static CharSequence ¤¤daysToHarvest = "¤Days until harvest";
    private static CharSequence ¤¤baseValue = "¤Capacity";

    private static CharSequence ¤¤HarvestYear = "¤This Year";
    private static CharSequence ¤¤HarvestPrev = "¤Last Year";

    private static CharSequence ¤¤skill = "¤Worker Skill";
    private static CharSequence ¤¤skillD = "¤The average bonus accumulated during the year. This determines the output of the harvest and the growth rate of the trees.";
    private static CharSequence ¤¤skillCurrent = "¤Bonus (current)";
    private static CharSequence ¤¤skillCurrentD = "¤The bonus that is currently being added to the Orchard.";

    private static CharSequence ¤¤chop = "¤Chop";
    private static CharSequence ¤¤chopD = "¤Reset all progress by chopping down the trees and instantly get {0} {1}.";

    Gui(ROOM_ORCHARD s) : base(s)
    {
        D.t(this);
    }

    final Cache cache = new Cache();

    public override void hover(GBox box, Instance i)
    {
        base.hover(box, i);
        box.NL();
        if (!i.blueprintI().constructor.isIndoors)
        {
            box.text(blueprint.constructor.fertility.name());
            box.add(GFORMAT.perc(box.text(), blueprint.constructor.fertility.get(i)));

            box.NL();
        }
        box.text(¤¤estimated);
        box.add(GFORMAT.i(box.text(), (int)cache.output(i)));

        box.space();
    }

    protected override void appendMain(GGrid icons, GGrid text, GuiSection sExtra)
    {
        IndustryResource res = blueprint.industries().get(0).outs().get(0);

        text.add(new GHeader(Dic.¤¤Production));

        GuiSection s = new GuiSection()
        {
            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(¤¤estimated);
            }
        };

        s.add(res.resource.icon(), 0, 0);

        s.addRightC(4, new GStat()
        {
            public override void update(GText text)
            {
                int am = 0;
                for (int i = 0; i < blueprint.instancesSize(); i++)
                {
                    Instance ins = blueprint.getInstance(i);
                    am += cache.output(ins);
                }
                GFORMAT.i(text, am);
            }
        });

        text.add(s);

        GStaples st = new GStaples(res.history().historyRecords())
        {
            protected override void hover(GBox box, int stapleI)
            {
                int i = res.history().historyRecords() - 1 - stapleI;
                int am = res.history().get(i);
                GText t = box.text();
                DicTime.setDaysAgo(t, i);
                box.add(t);
                box.NL(2);
                box.add(GFORMAT.i(box.text(), am));
            }

            protected override double getValue(int stapleI)
            {
                return res.history().get(res.history().historyRecords() - 1 - stapleI);
            }
        };

        st.body().setWidth(180).setHeight(64);
        text.add(st);
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GuiSection sExtra)
    {
        IndustryResource res = blueprint.industries().get(0).outs().get(0);

        GuiSection s = new GuiSection()
        {
            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                Instance ins = getter.get();

                b.textL(¤¤baseValue);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), ins.baseValue));
                b.NL();

                b.textL(Dic.¤¤ProductionRate);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), blueprint.productionData.outs().get(0).rate));
                b.NL();

                b.textL(¤¤skill);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), ins.skill()));
                b.NL();

                b.textLL(Dic.¤¤Total);
                b.tab(6);
                b.add(GFORMAT.f1(b.text(), ins.baseValue * blueprint.productionData.outs().get(0).rate * ins.skill()));
                b.NL();

                b.textL(¤¤Trees);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), (double)ins.trees / ins.treesTotal));
                b.NL();

                b.textLL(¤¤estimated);
                b.tab(6);
                b.add(GFORMAT.f1(b.text(), cache.output(ins)));
                b.NL();

                b.NL(4);

                b.NL(16);
                b.textL(¤¤HarvestYear);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), (int)ins.blueprintI().indus.get(0).outs().get(0).year.get(ins)));
                b.NL();

                b.NL(2);
                b.textL(¤¤HarvestPrev);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), (int)ins.blueprintI().indus.get(0).outs().get(0).yearPrev.get(ins)));
                b.NL();
            }
        };

        s.add(blueprint.productionData.outs().get(0).resource.icon(), 0, 0);
        GStat stat = new GStat()
        {
            public override void update(GText text)
            {
                double am = cache.output(getter.get()) / TIME.years().bitConversion(TIME.days());
                GFORMAT.f0(text, am);
            }
        };

        s.addRightC(6, stat);

        s.body().incrW(64);
        return s;
    }

    private class Cache
    {
        private int upI = -1;
        private int treesTotal;
        private int trees;
        private int daysTillNextTree;
        private int wood;
        private double output;

        private Instance ins;

        public int treesTotal(Instance ins)
        {
            up(ins);
            return treesTotal;
        }

        public int trees(Instance ins)
        {
            up(ins);
            return trees;
        }

        public int daysTillNextTree(Instance ins)
        {
            up(ins);
            return daysTillNextTree;
        }

        public double output(Instance ins)
        {
            up(ins);
            return output;
        }

        private void up(Instance ins)
        {
            if (upI == GAME.updateI() && this.ins == ins)
                return;
            this.ins = ins;
            upI = GAME.updateI();

            wood = 0;
            treesTotal = 0;
            trees = 0;
            daysTillNextTree = int.MaxValue;

            foreach (Coordinate c in ins.body())
            {
                if (!ins.is(c))
                    continue;

                OTile t = blueprint.tile.getM(c.x(), c.y());
                if (t != null)
                {
                    treesTotal++;
                    if (t.state() == t.IBIG)
                        trees++;
                    else
                    {
                        int d = t.state().daysTillGrown();
                        if (d < daysTillNextTree)
                            daysTillNextTree = d;
                    }
                    if (t.state() == t.ISMALL)
                        wood += blueprint.auxRes.amount() / 2;
                    else if (t.state() == t.IBIG || t.state() == t.IDEAD)
                        wood += blueprint.auxRes.amount();
                }
            }

            output = blueprint.time.days * ins.skill() * ins.baseValue * ins.industry().outs().get(0).rate * ins.trees / ins.treesTotal;
        }
    }
}