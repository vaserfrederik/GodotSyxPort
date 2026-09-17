using System;
using System.Collections.Generic;
using game.time;
using init.settings;
using init.type;
using settlement.stats;
using settlement.stats.law;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

namespace view.sett.ui.law
{
    internal class LawChart : GuiSection
    {
        private readonly GuiSection pop = new GuiSection();

        private static readonly string ¤¤arrests = "Arrests";

        static LawChart()
        {
            D.ts(typeof(LawChart));
        }

        public LawChart(int sw, HCLASS cl, Selector race)
        {
            if (S.get().developer)
            {
                foreach (var t in STATS.LAW().punishments)
                {
                    pop.add(new GButt.Glow(t.punish.name)
                    {
                        clickA = () =>
                        {
                            STATS.LAW().punish(CRIMES.all(cl).rnd(), cl, race.getRace(), t.punish);
                        }
                    }, pop.body().x1(), pop.body().y2());
                }
            }

            GStaples chart = new GStaples(STATS.DAYS_SAVED)
            {
                renderExtra = (SPRITE_RENDERER r, COLOR color, int stapleI, bool hovered, double value, int x1, int x2, int y1, int y2) =>
                {
                    x1 += 1;
                    x2 -= 1;
                    int ii = STATS.DAYS_SAVED - stapleI - 1;
                    double tot = GetValue(stapleI);
                    ColorImp c = ColorImp.TMP;
                    double dy1 = y1;
                    double dy2 = y1;
                    double dy = y2 - y1;
                    if (dy <= 0)
                        return;
                    {
                        double d = 0;
                        foreach (var cr in STATS.LAW().crimes)
                        {
                            d += cr.caught().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii);
                        }
                        dy2 += d / tot;
                        c.set(COLOR.UNIQUE.getC(0));
                        if (hovered)
                        {
                            c.shadeSelf(1.2);
                        }
                        c.render(r, x1, x2, (int)dy1, (int)dy2);
                        dy1 = dy2;
                    }
                    foreach (var p in STATS.LAW().punishments)
                    {
                        double d = dy * p.success().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii) / tot;
                        dy2 += d;
                        c.set(COLOR.UNIQUE.getC(p.punish.index() + 1));
                        if (hovered)
                        {
                            c.shadeSelf(1.2);
                        }
                        c.render(r, x1, x2, (int)dy1, (int)dy2);
                        dy1 = dy2;
                    }
                },

                hover = (GBox box, int stapleI) =>
                {
                    int i = STATS.DAYS_SAVED - stapleI - 1;
                    box.textLL(DicTime.setAgo(box.text(), i * TIME.secondsPerDay()));
                    box.NL();
                    HCLASS_RACE cc = HCLASS_RACE.clP(race.getRace(), cl);

                    int arrests = 0;
                    foreach (var c in STATS.LAW().crimes)
                    {
                        arrests += c.caught().history(cc).get(i);
                    }
                    box.textLL(¤¤arrests);
                    box.tab(6);
                    box.add(GFORMAT.i(box.text(), arrests));
                    box.NL(8);

                    foreach (var p in STATS.LAW().punishments)
                    {
                        box.textLL(p.punish.names);
                        box.tab(6);
                        box.add(GFORMAT.i(box.text(), p.success().history(cc).get(i)));
                        box.NL();
                    }
                },

                getValue = (int stapleI) =>
                {
                    int i = STATS.DAYS_SAVED - stapleI - 1;
                    HCLASS_RACE cc = HCLASS_RACE.clP(race.getRace(), cl);
                    int arrests = 0;
                    foreach (var c in STATS.LAW().crimes)
                    {
                        arrests += c.caught().history(cc).get(i);
                    }

                    foreach (var p in STATS.LAW().punishments)
                    {
                        arrests += p.success().history(cc).get(i);
                    }

                    return arrests;
                }
            };
            chart.normalize(true);
            chart.body().setDim(STATS.DAYS_SAVED * sw, 80);
            add(chart, body().x1() - 40, body().y2() + 4);
        }

        protected override void clickA()
        {
            if (S.get().developer)
                VIEW.inters().popup.show(pop, this);
        }
    }
}