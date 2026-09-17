using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;

namespace view.sett.ui.subject
{
    final class UISubjectStats : GuiSection
    {
        private readonly AInfo a;

        UISubjectStats(AInfo a, int height)
        {
            this.a = a;
            addRelBody(8, DIR.S, makeStats(height - 16));
        }

        private RENDEROBJ makeStats(int height)
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            GText work = new GText(UI.FONT().S, 32);

            foreach (var h in STATS.COLLECTIONS())
            {
                LinkedList<STAT> stats = new LinkedList<STAT>();
                foreach (var s in h.all())
                {
                    if (s.key() == null)
                        continue;
                    if (s.standing() == null)
                        continue;
                    stats.Add(s);

                    // outer:
                    // for (Race r : RACES.all())
                    // {
                    //     for (HCLASS c : HCLASSES.ALL())
                    //     if (s.standing().max(c, r) != 0)
                    //     {
                    //         stats.Add(s);
                    //         break outer;
                    //     }
                    // }
                }

                if (stats.Count == 0)
                    continue;

                rows.Add(new GHeader(h.info.name).hoverInfoSet(h.info.desc));
                foreach (var s in stats)
                {
                    CLICKABLE c = new Row(s, work);
                    rows.Add(c);
                }
            }

            GuiSection s = new GuiSection();

            GInput in = new GInput(new StringInputSprite(32, UI.FONT().S).placeHolder(Dic.¤¤Search));

            s.add(in);

            GScrollRows sc = new GScrollRows(rows, height - s.body().height() - 4 - s.body().height(), 0)
            {
                protected override bool passesFilter(int i, RENDEROBJ o)
                {
                    if (in.text() == null || in.text().Length == 0)
                        return true;
                    if (o is Row)
                    {
                        return Str.containsText(((Row)o).s.info().name, in.text());
                    }
                    return false;
                }
            };

            s.addDown(4, sc.view());

            return s;
        }


        private class Row : CLICKABLE.ClickableAbs
        {
            private readonly GText work;
            private readonly STAT s;
            private readonly SPRITE icon;

            Row(STAT stat, GText text)
            {
                this.work = text;
                this.s = stat;
                body.setDim(480, 32);
                if (stat.info().icon != null)
                {
                    icon = stat.info().icon.resized(IconS.M);
                }
                else
                    icon = null;
            }


            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                if (isHovered)
                {
                    COLOR.BLUEDARK.render(r, body());
                }

                if (icon != null)
                {
                    icon.renderCY(r, body().x1(), body().cY());
                }

                work.setFont(UI.FONT().S);
                work.clear();
                work.add(s.info().name);
                work.setMaxWidth(220);
                work.setMultipleLines(false);
                work.lablifySub();
                work.renderCY(r, body().x1() + 32, body().cY());

                work.setFont(UI.FONT().S);
                work.clear();

                if (s == STATS.POP().age.AGE_DAYS)
                {
                    GFORMAT.f(work, (double)s.indu().get(a.a.indu()) / TIME.years().bitConversion(TIME.days()), 2);
                }
                else if (s.indu().max(a.a.indu()) == 1 && s.info().isInt())
                {
                    GFORMAT.bool(work, s.indu().get(a.a.indu()) == 1);
                }
                else if (s.info().isInt())
                {

                    GFORMAT.i(work, s.indu().get(a.a.indu()));
                }
                else
                {
                    GFORMAT.perc(work, s.indu().getD(a.a.indu()));
                }
                work.normalify();
                work.renderCY(r, body().x1() + 260, body().cY());

                double now = s.standing().get(a.a.indu());
                double max = s.standing().max(a.a.indu().clas(), a.a.race());
                int w = (int)(150 * s.standing().normalized(a.a.indu().clas(), a.a.race()));
                if (w > 0)
                {
                    if (w < 20)
                        w = 20;
                    GMeter.render(r, GMeter.C_REDGREEN, now / max, body().x1() + 330, body().x1() + 330 + w, body().y1() + 8, body().y2() - 8);
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                s.hover(text, a.a.indu());
            }

            protected override void clickA()
            {
                if (s.indu() != null)
                    DebugInput.activate(s.indu(), a.a);
            }
        }
    }
}