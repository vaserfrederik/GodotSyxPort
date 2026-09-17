using System;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.info;
using util.gui;
using util.gui.renderable;

namespace view.sett.ui.standing
{
    public class StatRow : GuiSection
    {
        private readonly STAT s;
        private readonly HCLASS cl;
        private readonly GETTER<Race> race;

        static readonly int StatX = 268;
        static readonly int MeterX = 328;
        static readonly int MeterW = 200;
        static readonly int Width = MeterX + MeterW + 4;

        public StatRow(STAT s, HCLASS cl, GETTER<Race> race) : this(s, null, cl, race)
        {
        }

        public StatRow(STAT s, RENDEROBJ ins, HCLASS cl, GETTER<Race> race)
        {
            this.s = s;
            this.cl = cl;
            this.race = race;
            add(new Arrow(s, cl, race));

            if (ins != null)
                addRightC(2, ins);

            SPRITE icon = s.info().icon;
            if (icon != null)
            {
                addRightC(2, icon.resized(Icon.L));
            }
            GText t = new GText(UI.FONT().H2, s.info().name);
            t.setMultipleLines(false);
            t.setMaxWidth(210 - (icon != null ? Icon.L + 4 : 0));
            addRightC(2, t.lablify());
            add(new GStat
            {
                update = (text) => format(text, s, value(cl, 0), cl, race.get())
            }, StatX, 0);
            add(new Meter(s, cl, race), MeterX, 0);
            degree(cl, race);
            pad(2, 4);
        }

        private class Arrow : RENDEROBJ.RenderImp
        {
            private readonly STAT s;
            private readonly HCLASS cl;
            private readonly GETTER<Race> race;

            public Arrow(STAT s, HCLASS cl, GETTER<Race> race) : base(Icon.S)
            {
                this.s = s;
                this.cl = cl;
                this.race = race;
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                int v = (int)(s.standing().getHistoric(cl, race.get(), 1) * 256);
                int n = (int)(s.standing().get(cl, race.get()) * 256);
                if (n > v)
                {
                    GCOLOR.UI().goodFlash().bind();
                    SPRITES.icons().s.arrow_right.render(r, body);
                }
                else if (n < v)
                {
                    GCOLOR.UI().badFlash().bind();
                    SPRITES.icons().s.arrow_left.render(r, body);
                }
                COLOR.unbind();
            }
        }

        private class Meter : RENDEROBJ.RenderImp
        {
            private readonly STAT s;
            private readonly HCLASS cl;
            private readonly GETTER<Race> race;

            public Meter(STAT s, HCLASS cl, GETTER<Race> race)
            {
                this.s = s;
                body().setDim(200, 16);
                this.cl = cl;
                this.race = race;
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                double now = s.standing().get(cl, race.get());
                double max = s.standing().max(cl, race.get());
                double prev = s.standing().getPrev(cl, race.get(), 8);
                int w = (int)(200 * Math.Sqrt(s.standing().normalized(cl, race.get())));
                if (w > 0)
                {
                    GMeter.renderDelta(r, prev / max, now / max, body().x1(), body().x1() + w, body().y1(), body().y2());
                }
            }
        }

        void degree(HCLASS cl, GETTER<Race> race)
        {
            StatDecree c = s.decree();

            if (c == null)
                return;
            INT_OE<Race> rr = c.getI(cl);
            if (rr.max(null) == 1)
            {
                Checkbox b = new GButt.Checkbox((SPRITE)(new GText(UI.FONT().S, c.name).lablifySub()))
                {
                    renAction = () =>
                    {
                        selectedSet(rr.get(race.get()) == 1);
                    },

                    clickA = () =>
                    {
                        rr.set(race.get(), (rr.get(race.get()) + 1) & 1);
                    }
                };
                b.hoverSet(c);
                b.body().moveX1Y1(64, getLastY2() + 8);
                add(b);
            }
            else if (rr.max(null) > 25)
            {
                INTE d = new INTE()
                {
                    min = () => rr.min(race.get()),
                    max = () => rr.max(race.get()),
                    get = () => rr.get(race.get()),
                    set = (t) => rr.set(race.get(), t)
                };

                GSliderInt inSlider = new GSliderInt(d, 100, true, true)
                {
                    hoverInfoGet = (text) =>
                    {
                        GBox b = (GBox)text;
                        b.add(GFORMAT.f(b.text(), c.get(cl, race.get())));
                    }
                };
                add(new GText(UI.FONT().S, c.name).lablifySub(), 64, getLastY2() + 8);
                addRightC(8, inSlider);
            }
            else
            {
                INTE d = new INTE()
                {
                    min = () => rr.min(race.get()),
                    max = () => rr.max(race.get()),
                    get = () => rr.get(race.get()),
                    set = (t) => rr.set(race.get(), t)
                };

                GStat ss = new GStat()
                {
                    update = (text) => GFORMAT.f(text, c.get(cl, race.get()), 1)
                };

                GTarget m = new GTarget(64, false, true, ss, d)
                {
                    hoverTitleSet = c.name,
                    hoverInfoSet = c.desc
                };
                add(new GText(UI.FONT().S, c.name).lablifySub(), 64, getLastY2() + 8);
                addRightC(8, m);
            }
        }

        static GText format(GText t, STAT s, double v, HCLASS cl, Race race)
        {
            if (race == null)
            {
                if (s.info().isInt())
                {
                    return GFORMAT.f(t, v * s.dataDivider());
                }
                else
                {
                    return GFORMAT.perc(t, v).normalify();
                }
            }
            else
            {
                double d = race.stats().def(s.standing()).get(cl).to - race.stats().def(s.standing()).get(cl).from;
                if (s.info().isInt())
                {
                    double m = s.dataDivider();
                    double n = v * s.dataDivider();

                    if (d > 0)
                    {
                        return GFORMAT.f0(t, n, m);
                    }
                    else if (d < 0)
                    {
                        return GFORMAT.f0Inv(t, n, m);
                    }
                    else
                    {
                        return GFORMAT.f(t, n);
                    }
                }
                else
                {
                    if (d > 0)
                        return GFORMAT.perc(t, v);
                    else if (d < 0)
                    {
                        return GFORMAT.percInv(t, v);
                    }
                    else
                    {
                        return GFORMAT.perc(t, v).normalify();
                    }
                }
            }
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
            GCOLOR.UI().border().render(r, body().x1(), body().x2(), body().y2() - 1, body().y2());
        }

        protected double value(HCLASS c, int daysBack)
        {
            return s.data(c).getD(race.get(), daysBack);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (isHoveringAHoverElement())
            {
                base.hoverInfoGet(text);
                return;
            }

            s.hover(text, cl, race.get());
        }

        public class Title : HoverableAbs
        {
            private readonly GText t;

            public Title(INFO info) : this(info.name, info.desc)
            {
            }

            public Title(CharSequence name, CharSequence desc)
            {
                t = new GText(UI.FONT().H2, name).lablify();
                body().setWidth(500);
                body().setHeight(t.height() * 2);
                hoverTitleSet(name);
                hoverInfoSet(desc);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                t.renderC(r, body().cX(), body().cY() + t.height() / 2 - 6);
                COLOR.WHITE30.render(r, body().x1(), body().x2(), body().y2() - 1, body().y2());
            }
        }
    }
}