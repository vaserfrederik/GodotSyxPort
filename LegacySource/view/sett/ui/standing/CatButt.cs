using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.standing.Cats;

namespace view.sett.ui.standing
{
    class CatButt : GuiSection
    {
        static readonly int width = 220;
        private double last = 0;
        private double now = 0;
        private double max = 0;
        private readonly StatCollection[] cs;
        private readonly Cat cat;
        private readonly HCLASS cl;
        private readonly GETTER<Race> race;

        public CatButt(Cats cats, Cat cat, HCLASS cl, GETTER<Race> race, INTE hov) : base()
        {
            this.cs = cat.cs;
            this.cat = cat;
            this.race = race;
            hoverInfoSet(cs[0].info.desc);

            add(new GHeader(cat.title())
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body, 2);
                    OPACITY.unbind();
                    base.render(r, ds, isHovered);
                }
            }, 0, 0);

            addDown(2, new RENDEROBJ.RenderImp(width - 20, 24)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    double statMax = cats.getBiggest();
                    if (max > 0)
                        GMeter.renderDelta(r, last / max, now / max, body.x1(), (int)(body.x1() + body.width() * max / statMax), body.y1(), body.y2());
                }
            });

            addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.f(text, now);
                    text.add('/');
                    GFORMAT.f(text, max);
                    GFORMAT.colorInterInv(text, now, max);
                }
            });

            pad(4);

            GStaples staples = new Staples(cl, race, hov, cs);
            staples.body().moveX1(width + 8);

            add(staples);
            moveLastToBack();
            pad(4);

            this.cl = cl;
        }

        protected override void clickA()
        {
            if (cl == HCLASSES.CITIZEN())
                VIEW.s().panels.addDontRemove(VIEW.s().ui.standing, cat);
            else
                VIEW.s().panels.addDontRemove(VIEW.s().ui.slaves, cat);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            now = 0;
            max = 0;
            last = 0;

            foreach (StatCollection c in cs)
                foreach (STAT s in c.all())
                {
                    now += s.standing().get(cl, race.get());
                    last += s.standing().get(cl, race.get(), s.data(cl).getPeriodD(race.get(), 8, 0));
                    max += s.standing().max(cl, race.get());
                }
            GCOLOR.UI().border().render(r, body());
            GCOLOR.UI().bg(true, VIEW.s().panels.added(cat), hoveredIs()).render(r, body(), -1);

            base.render(r, ds);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.title(cat.name);
            b.text(cat.desc);
            b.NL();
            base.hoverInfoGet(text);
        }

        static class Staples : GStaples
        {
            private readonly HCLASS cl;
            private readonly INTE hov;
            private readonly StatCollection[] cs;
            private static readonly double div = 100;
            private Race old;
            private readonly GETTER<Race> race;

            private double vmin, vmax, max;
            private int vI = -1;

            public Staples(HCLASS cl, GETTER<Race> race, INTE hov, StatCollection[] cs) : base(STATS.DAYS_SAVED)
            {
                this.cl = cl;
                this.hov = hov;
                this.cs = cs;
                border(false);
                background(true);
                body().setWidth(7 * STATS.DAYS_SAVED);
                body().setHeight(70);
                normalize(false);
                this.race = race;
            }

            protected override void hover(GBox box, int stapleI)
            {
                box.title(cs[0].info.name);

                int fromZero = STATS.DAYS_SAVED - stapleI - 1;
                box.add(box.text().add(-fromZero).s().add(TIME.days().cycleName()));
                box.NL();
                double p = 0;
                double max = 0;
                double pprev = 0;
                foreach (StatCollection c in cs)
                {
                    box.textLL(c.info.name);
                    box.NL();
                    foreach (STAT s in c.all())
                    {
                        double m = s.standing().max(cl, race.get(), fromZero);
                        if (m == 0)
                            continue;

                        double curr = s.standing().getHistoric(cl, race.get(), fromZero);
                        box.textL(s.info().name);
                        box.tab(6);
                        box.add(GFORMAT.fofkInv(box.text(), curr, m));

                        if (fromZero < STATS.DAYS_SAVED - 1)
                        {
                            box.tab(9);
                            box.add("↑");
                            box.add(s.standing().getHistoric(cl, race.get(), fromZero + 1) - curr);
                        }

                        if (fromZero > 0)
                        {
                            box.tab(9);
                            box.add("↓");
                            box.add(curr - s.standing().getHistoric(cl, race.get(), fromZero - 1));
                        }

                        pprev = curr;
                    }
                }
            }

            protected override double getValue(int stapleI)
            {
                if (hov != null)
                {
                    setHovered(hov.get());
                }

                return base.getValue(stapleI);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                if (hov != null)
                {
                    isHovered = true;
                    setHovered(hov.get());
                }

                base.render(r, ds, isHovered);
            }

            protected override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    if (hov != null)
                    {
                        hov.set(hoverI());
                    }
                    return true;
                }
                return false;
            }

            protected override void renderExtra(SPRITE_RENDERER r, COLOR color, int stapleI, bool hovered, double value,
                int x1, int x2, int y1, int y2)
            {
                int v = (int)(getValue(stapleI) * 100);
                int p = (int)((stapleI > 0 ? getValue(stapleI - 1) : value) * 100);
                if (v == p)
                    return;
                SPRITE s = p > v ? SPRITES.icons().s.arrowDown : SPRITES.icons().s.arrowUp;
                color.bind();
                s.renderC(r, x1 + (x2 - x1) / 2, y2 + s.height() / 2);
            }

            protected override void setColor(ColorImp c, int stapleI, double value)
            {
                if (max == 0)
                    return;
                c.interpolate(GCOLOR.UI().BAD.hovered, GCOLOR.UI().GOOD.hovered, value);
            }

            protected override void setColorBg(ColorImp c, int stapleI, double value)
            {
                c.set((stapleI & 1) == 1 ? COLOR.WHITE20 : COLOR.WHITE15);
            }
        }
    }
}