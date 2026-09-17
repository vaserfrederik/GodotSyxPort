using System;
using System.Collections.Generic;
using init.race;
using init.resources;
using init.type;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.gui.misc;
using util.gui.table;

namespace view.sett.ui.home
{
    final class UIHomesFurniture : GuiSection
    {
        private static readonly string ¤¤Yearly = "¤{0} per item per year, estimation: -{1} in total per year.";

        private static readonly string ¤¤CurrentlyUsed = "¤Currently Used";
        private static readonly string ¤¤CurrentTarget = "¤Current Target";
        private static readonly string ¤¤CurrentMax = "¤Current Maximum";
        private static readonly string ¤¤manage = "¤Limits regarding what resources that can be used, is set in the subject panels respectively.";

        static
        {
            D.ts(typeof(UIHomesFurniture));
        }

        public UIHomesFurniture(int height)
        {
            LinkedList<RES_AMOUNT> all = new LinkedList<RES_AMOUNT>(RACES.res().homeResMax(null));
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            while (!all.isEmpty())
            {
                int am = 0;
                RES_AMOUNT r = all.removeFirst();
                GuiSection s = new GuiSection();
                rows.add(s);
                s.add(new REN(r.resource()));

                while (!all.isEmpty() && am++ < 4)
                {
                    r = all.removeFirst();
                    s.addRightC(8, new REN(r.resource()));
                }
            }

            GScrollRows r = new GScrollRows(rows, height);

            add(r.view());

            addRelBody(8, DIR.N, new GHeader(STATS.HOME().materials.info().name));
        }

        private static class REN : HOVERABLE.HoverableAbs
        {
            private readonly RESOURCE res;
            private readonly int[,] ti = Alloc.i2(HCLASSES.ALL().size(), RACES.all().size());
            private readonly GStat s;

            REN(RESOURCE res)
            {
                this.res = res;
                body.setDim(100, 32);
                for (HCLASS c : HCLASSES.ALL())
                {
                    for (Race r : RACES.all())
                    {
                        ti[c.index()][r.index] = -1;
                        for (int i = 0; i < r.home().clas(c).resources().size(); i++)
                        {
                            if (r.home().clas(c).resources().get(i).resource() == res)
                            {
                                ti[c.index()][r.index] = i;
                                break;
                            }
                        }
                    }
                }

                s = new GStat()
                {
                    public override void update(GText text)
                    {
                        int i = getCurrent();
                        GFORMAT.i(text, i);
                    }

                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        OPACITY.O50.bind();
                        COLOR.BLACK.render(r, X1 - 4, X2 + 4, Y1 - 2, Y2 + 2);
                        OPACITY.unbind();
                        base.render(r, X1, X2, Y1, Y2);
                    }
                };
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                double c = getCurrent();
                double max = max();

                if (max == 0)
                {
                    GMeter.render(r, GMeter.C_GRAY, 1.0, body());
                }
                else
                {
                    double d = c / max();
                    GMeter.render(r, GMeter.C_REDGREEN, d, body());
                }

                res.icon().renderCY(r, body().x1() + 8, body().cY());

                s.adjust();

                s.renderCY(r, body().x1() + 40, body().cY());
            }

            private int getTarget()
            {
                int am = 0;
                for (HCLASS c : HCLASSES.ALL())
                {
                    for (Race r : RACES.all())
                    {
                        if (ti[c.index()][r.index] == -1)
                            continue;
                        am += STATS.HOME().target(c, r, res) * STATS.HOME().GETTER.stat().data(c).get(r);
                    }
                }
                return am;
            }

            private int max()
            {
                int am = 0;
                for (HCLASS c : HCLASSES.ALL())
                {
                    for (Race r : RACES.all())
                    {
                        if (ti[c.index()][r.index] == -1)
                            continue;
                        am += STATS.HOME().max(c, r, res) * STATS.HOME().GETTER.stat().data(c).get(r);
                    }
                }
                return am;
            }

            private int getCurrent()
            {
                int am = 0;
                for (HCLASS c : HCLASSES.ALL())
                {
                    for (Race r : RACES.all())
                    {
                        if (ti[c.index()][r.index] == -1)
                            continue;
                        am += STATS.HOME().current(c, r, ti[c.index()][r.index]);
                    }
                }
                return am;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(res.name);
                b.text(STATS.HOME().materials.info().desc);
                b.NL(4);
                b.text(¤¤manage);

                b.NL(8);

                int current = getCurrent();

                b.textL(¤¤CurrentlyUsed);
                b.tab(5);
                b.add(GFORMAT.i(b.text(), current));
                b.NL();

                b.textL(¤¤CurrentTarget);
                b.tab(5);
                b.add(GFORMAT.i(b.text(), getTarget()));
                b.NL();

                b.textL(¤¤CurrentMax);
                b.tab(5);
                b.add(GFORMAT.i(b.text(), max()));
                b.NL();

                b.textL(Dic.¤¤ConsumptionRate);
                b.NL();
                GText t = b.text();
                t.add(¤¤Yearly);
                t.insert(0, STATS.HOME().rate(null, null), 2);
                t.insert(1, (int)(STATS.HOME().rate(null, null) * current));
                b.add(t);
                base.hoverInfoGet(text);
            }
        }
    }
}