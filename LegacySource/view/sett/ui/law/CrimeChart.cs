using System;
using System.Collections.Generic;
using game.time;
using init.settings;
using init.sprite;
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
    final class CrimeChart : GuiSection
    {
        final GuiSection pop = new GuiSection();

        public CrimeChart(int sw, HCLASS cl, Selector race)
        {
            foreach (StatCrime t in STATS.LAW().crimes)
            {
                pop.add(new GButt.Glow("commit: " + t.crime.name)
                {
                    protected override void clickA()
                    {
                        t.commit(HCLASS_RACE.clP(race.getRace(), cl), 1);
                    }
                }, pop.body().x1(), pop.body().y2());
                pop.addRightC(20, new GButt.Glow("arrest: " + t.crime.name)
                {
                    protected override void clickA()
                    {
                        t.catchh(race.getRace());
                    }
                });
            }

            GStaples chart = new GStaples(STATS.DAYS_SAVED)
            {
                protected override void renderExtra(SPRITE_RENDERER r, COLOR color, int stapleI, bool hovered, double value, int x1, int x2, int y1, int y2)
                {
                    x1 += 1;
                    x2 -= 1;
                    int ii = STATS.DAYS_SAVED - stapleI - 1;
                    double tot = 0;
                    foreach (CRIME c in CRIMES.all(cl))
                        tot += c.stat().occurence().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii);
                    ColorImp c = ColorImp.TMP;
                    double dy1 = y1;
                    double dy2 = y1;
                    double dy = y2 - y1;
                    if (dy <= 0)
                        return;

                    foreach (CRIME cr in CRIMES.all(cl))
                    {
                        double d = dy * cr.stat().occurence().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii) / tot;
                        dy2 += d;
                        c.set(COLOR.UNIQUE.getC(cr.index()));
                        if (hovered)
                        {
                            c.shadeSelf(1.2);
                        }
                        c.render(r, x1, x2, (int)dy1, (int)dy2);
                        dy1 = dy2;
                    }
                }

                protected override void hover(GBox box, int stapleI)
                {
                    int ii = STATS.DAYS_SAVED - stapleI - 1;

                    box.textL(DicTime.setAgo(box.text(), ii * TIME.secondsPerDay()));
                    box.NL(4);

                    foreach (CRIME c in CRIMES.all(cl))
                    {
                        box.add(SPRITES.icons().s.circle, COLOR.UNIQUE.getC(c.index()));
                        box.text(c.names);
                        box.tab(6);
                        box.add(GFORMAT.i(box.text(), c.stat().occurence().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii)));
                        box.NL();
                    }
                }

                protected override double getValue(int stapleI)
                {
                    int ii = STATS.DAYS_SAVED - stapleI - 1;
                    int am = 0;
                    foreach (CRIME cr in CRIMES.all(cl))
                        am += cr.stat().occurence().history(HCLASS_RACE.clP(race.getRace(), cl)).get(ii);
                    return am;
                }

                protected override void setColor(ColorImp c, int stapleI, double value)
                {
                    c.set(COLOR.YELLOW100).saturateSelf(0.5);
                }
            };
            chart.body().setWidth(STATS.DAYS_SAVED * sw);
            chart.body().setHeight(80);

            add(chart, body().x1() - 40, body().y2() + 4);
        }

        protected override void clickA()
        {
            if (S.get().developer)
                VIEW.inters().popup.show(pop, this);
        }
    }
}