using System;
using System.Collections.Generic;
using game.boosting;
using game.time;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using world.map.regions;
using world.region.RDOutputs;

namespace view.world.ui.region
{
    final class PlayOutput : GuiSection
    {
        private static readonly CharSequence ¤¤ship = "This resource is shipped annually in {0} days. Accumulated so far: {1}.";
        static
        {
            D.ts(typeof(PlayOutput));
        }

        private readonly ArrayListGrower<ResButt> butts = new ArrayListGrower<ResButt>();
        private readonly GETTER_IMP<Region> g;
        private readonly List<RENDEROBJ> activeButts = new List<RENDEROBJ>();
        private readonly int width;
        public static readonly int height = 30;
        private readonly int amX;

        public PlayOutput(GETTER_IMP<Region> g, int width)
        {
            {
                GButt b = new GButt.ButtPanel(RDOutputs.¤¤Squeeze)
                {
                    protected override void clickA()
                    {
                        RD.OUTPUT().squeze(g.get());
                        base.clickA();
                    }

                    protected override void renAction()
                    {
                        activeSet(!RD.BUILDINGS().isTmp() && RD.DEVASTATION().current.getD(g.get()) < 0.25);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(RDOutputs.¤¤Squeeze);
                        b.text(RDOutputs.¤¤SqueezeD);
                        b.NL();

                        b.add(UI.icons().s.money);
                        b.textLL(Dic.¤¤Currs);
                        b.tab(6);
                        b.add(GFORMAT.iIncr(b.text(), (long)(RD.OUTPUT().MONEY.boost.get(g.get()) * RD.OUTPUT().sqeezeAmountDays)));
                        b.NL();

                        foreach (RDResource res in RD.OUTPUT().RES)
                        {
                            int am = (int)(res.boostYearlyPart.get(g.get()) * RD.OUTPUT().sqeezeAmountDays / TIME.years().bitConversion(TIME.days()));
                            am += (int)(res.boost.get(g.get()) * RD.OUTPUT().sqeezeAmountDays);
                            if (am > 0)
                            {
                                b.add(res.res.icon());
                                b.textLL(res.res.name);
                                b.tab(6);
                                b.add(GFORMAT.iIncr(b.text(), am));
                                b.NL();
                            }
                        }

                        b.textLL(RD.DEVASTATION().current.info().name);
                        b.tab(6);
                        b.add(GFORMAT.perc(b.text(), -0.5));
                        b.NL();
                        b.textLL(RD.RACES().loyaltyAll.info().name);
                        b.tab(6);
                        b.add(GFORMAT.perc(b.text(), -0.4));
                        b.NL();
                        base.hoverInfoGet(text);
                    }
                };

                b.body().moveX2(body().x2() - 16);
                b.body().moveY1(4);
                add(b);
            }

            this.width = (width - 32) / 5;
            this.g = g;
            for (int i = 0; i < RD.OUTPUT().ALL.size(); i++)
            {
                butts.add(new ResButt(RD.OUTPUT().ALL.get(i)));
            }
            activeButts = new List<RENDEROBJ>(butts.size());

            {
                amX = 5;
                GTableBuilder builder = new GTableBuilder()
                {
                    public override int nrOFEntries()
                    {
                        return (int)Math.Ceiling(activeButts.size() / (double)amX);
                    }
                };

                builder.column(null, amX * this.width, new GRowBuilder()
                {
                    public override RENDEROBJ build(GETTER<Integer> ier)
                    {
                        return new Row(ier);
                    }
                });

                addRelBody(2, DIR.S, builder.createHeight((height) * 2, false));
            }

            pad(6, 6);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            GButt.ButtPanel.renderBG(r, true, false, false, body());
            GButt.ButtPanel.renderFrame(r, body());
            activeButts.Clear();
            foreach (ResButt b in butts)
            {
                if (hasValue(b.bu.boost, g.get()) || hasValue(b.bu.boostYearlyPart, g.get()))
                {
                    activeButts.Add(b);
                }
            }
            base.render(r, ds);
        }

        private bool hasValue(Boostable bo, Region reg)
        {
            foreach (Booster b in bo.all())
            {
                double v = b.get(reg);
                if ((!b.isMul && v > 0))
                {
                    return true;
                }
            }
            return false;
        }

        private class Row : GuiSection
        {
            private readonly GETTER<Integer> ier;

            public Row(GETTER<Integer> ier)
            {
                this.ier = ier;
                body().setHeight(height);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                int x1 = body().x1();
                int y1 = body().y1();
                clear();
                int s = ier.get() * amX;
                for (int i = 0; i < amX && i + s < activeButts.size(); i++)
                {
                    addRightC(0, activeButts.get(i + s));
                }
                body().moveX1(x1);
                body().moveY1(y1);
                base.render(r, ds);
            }
        }

        private class ResButt : ClickableAbs
        {
            private readonly RDOutput bu;
            private readonly GText tt = new GText(UI.FONT().S, 8);

            public ResButt(RDOutput b)
            {
                body.setDim(width, height);
                this.bu = b;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                GCOLOR.UI().border().render(r, body);
                GCOLOR.UI().bg(isActive, isSelected, isHovered).render(r, body, -1);

                bu.boost.icon.medium.renderC(r, body.x1() + 16, body.cY());

                tt.clear();
                GFORMAT.i(tt, (long)(bu.boost.get(g.get()) + bu.boostYearlyPart.get(g.get())));

                tt.renderC(r, body.x1() + 32, body.cY());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (bu.boostYearlyPart.get(g.get()) > 0)
                {
                    bu.boostYearlyPart.hover(text, g.get(), true);
                    GBox b = (GBox)text;
                    b.sep();
                    GText t = b.text();
                    t.add(¤¤ship);
                    t.insert(0, bu.daysUntilDailydelivery());
                    t.insert(1, bu.yearlyAccumulation.get(g.get()));
                    b.add(t);
                    b.sep();

                    if (bu.boost.get(g.get()) > 0)
                    {
                        bu.boost.hover(text, g.get(), true);
                    }
                }
                else
                {
                    bu.boost.hover(text, g.get(), true);
                }
            }
        }
    }
}