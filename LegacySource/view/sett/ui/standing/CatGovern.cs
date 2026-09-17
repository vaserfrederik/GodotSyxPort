using System;
using System.Collections.Generic;
using init.race;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.main;
using view.sett.ui.standing.Cats;

namespace view.sett.ui.standing
{
    final class CatGovern : Cat
    {
        private static readonly CharSequence ¤¤name = "¤Government";
        private static readonly CharSequence ¤¤manageLaw = "¤Manage Law";
        static
        {
            D.ts(typeof(CatGovern));
        }

        CatGovern(HCLASS cl, GETTER<Race> race) : base(new[] { STATS.LAW(), STATS.GOVERN() })
        {
            titleSet(¤¤name);

            LinkedList<RENDEROBJ> rens = new LinkedList<RENDEROBJ>();

            rens.add(new StatRow.Title(STATS.GOVERN().info));
            foreach (STAT s in STATS.GOVERN().all())
            {
                GuiSection ss = new StatRow(s, cl, race);
                rens.add(ss);
            }

            rens.add(new StatRow.Title(STATS.LAW().info));

            foreach (STAT s in STATS.LAW().all())
            {
                GuiSection ss = new StatRow(s, cl, race);
                rens.add(ss);
            }

            rens.add(new GButt.ButtPanel(¤¤manageLaw)
            {
                protected override void clickA()
                {
                    VIEW.s().panels.add(VIEW.s().ui.law, true);
                }
            });

            section.addDown(4, new GScrollRows(rens, HEIGHT, 0).view());
        }
    }
}