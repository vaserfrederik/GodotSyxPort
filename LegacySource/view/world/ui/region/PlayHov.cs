using System;
using init.sprite.UI;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;
using util.info;
using util.text;
using world.map.regions;
using world.region.RD;
using world.region.RDOutputs;
using world.region.pop;

namespace view.world.ui.region
{
    final class PlayHov
    {
        static void hover(Region reg, GUI_BOX box)
        {
            box.title(reg.info.name());
            GBox b = (GBox)box;

            if (reg.capitol())
            {
                b.text(Dic.¤¤CapitolYou);
                b.add(UI.icons().m.sword);
                b.add(GFORMAT.iIncr(b.text(), (int)RD.MILITARY().power.getD(reg)));
                b.NL();
                return;
            }

            {
                double ii = 32.0;
                double tot = RD.RACES().maxPop(reg);
                if (tot > 0)
                {
                    foreach (RDRace r in RD.RACES().all)
                    {
                        int am = (int)(ii * r.pop.get(reg) / tot);
                        if (RD.RACES().visuals.cRace(reg) == r.race)
                        {
                            am++;
                        }
                        while (am-- > 0)
                        {
                            b.add(r.race.appearance().icon);
                            b.rewind(16);
                        }
                    }
                }
                b.NL(8);
            }

            b.NL(5);

            b.add(UI.icons().m.heart);
            b.add(GFORMAT.perc(b.text(), RD.HEALTH().getD(reg)));
            b.tab(3);

            b.add(UI.icons().m.rebellion);
            b.add(GFORMAT.perc(b.text(), RD.RACES().loyaltyAll.getD(reg)));
            b.tab(6);

            b.add(UI.icons().m.flag);
            b.add(GFORMAT.perc(b.text(), RD.OWNER().affiliation.getD(reg)));
            b.tab(9);

            b.add(UI.icons().m.sword);
            b.add(GFORMAT.iIncr(b.text(), (int)RD.MILITARY().power.getD(reg)));
            b.NL(2);

            int i = 0;

            foreach (RDOutput o in RD.OUTPUT().ALL)
                i = addTax(b, i, o.boost.icon, o.getDelivery(reg));

            OtherHov.hovSiege(reg, box);
            RD.HEALTH().problem(b, reg);
        }

        private static int addTax(GBox b, int i, SPRITE icon, double amount)
        {
            if (amount > 1)
            {
                if (i > 4)
                {
                    i = 0;
                    b.NL();
                }

                b.tab(i * 3);
                b.add(icon);
                b.add(GFORMAT.i(b.text(), (int)amount));
                i++;
            }
            return i;
        }
    }
}