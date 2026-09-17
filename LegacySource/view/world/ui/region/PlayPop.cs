using System;
using System.Collections.Generic;
using game.boosting;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using util.data.GETTER;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using world.map.regions;
using world.region;
using world.region.pop;

namespace view.world.ui.region
{
    sealed class PlayPop : GuiSection
    {
        static readonly CharSequence ¤¤eWarning = "¤Enabling an edict has a global effect in your whole kingdom. The affected race will have their loyalty decreased in all regions.";
        private double massacreAsked = -20;

        static PlayPop()
        {
            D.ts(typeof(PlayPop));
        }

        public PlayPop(GETTER_IMP<Region> g, int width, int height) : base()
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            {
                GuiSection h = new GuiSection();

                {
                    GStat s = new GStat()
                    {
                        public override void update(GText text)
                        {
                            GFORMAT.i(text, RD.RACES().population.get(g.get()));
                        }
                    };

                    SPRITE pop = new SPRITE.Imp(140, 24)
                    {
                        public override void render(SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2)
                        {
                            double n = RD.RACES().population.get(g.get());
                            double nn = RD.RACES().popTarget.getD(g.get());
                            double mm = Math.Max(n, nn);
                            n /= mm;
                            nn /= mm;
                            GMeter.renderDelta(rr, n, nn, X1, X2, Y1, Y2);
                            s.adjust();
                            X1 += 8;
                            Y1 = Y1 + ((Y2 - Y1) - s.height()) / 2;
                            OPACITY.O50.bind();
                            COLOR.BLACK.render(rr, X1 - 1, X1 + s.width() + 2, Y1 + 2, Y1 + s.height() - 2);
                            OPACITY.unbind();
                            s.render(rr, X1, Y1);
                        }
                    };

                    h.add(new GHeader.HeaderHorizontal(UI.icons().m.citizen.resized(24), pop)
                    {
                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            GBox b = (GBox)text;

                            b.title(RD.RACES().population.name);

                            b.textLL(Dic.¤¤Current);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), RD.RACES().population.get(g.get())));
                            b.NL();
                            b.textLL(Dic.¤¤Target);
                            b.tab(6);
                            b.add(GFORMAT.i(b.text(), (int)RD.RACES().popTarget.getD(g.get())));
                            b.sep();


                            RD.RACES().capacity.hover(b, g.get(), RD.RACES().capacity.name, true);

                            b.NL(8);
                            b.tab(1);
                            b.textL(Dic.¤¤Used);

                            double d = RD.RACES().capacity.get(g.get()) * RD.RACES().population.get(g.get()) / (RD.RACES().popTarget.getD(g.get()));

                            b.tab(5);
                            b.add(GFORMAT.f0(b.text(), -d));
                        }
                    });
                }

                h.addCentredY(new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.perc(text, RD.RACES().loyaltyAll.getD(g.get()));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        RD.RACES().loyaltyAll.info().hover(b);
                    }

                }.hh(UI.icons().m.rebellion), 252);

                h.addCentredY(new HOVERABLE.Sprite(UI.icons().m.descrimination)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        RD.RACES().edicts.sanction.info.hover(text);
                    }
                }, 476);

                h.addCentredY(new HOVERABLE.Sprite(UI.icons().m.exit)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        RD.RACES().edicts.exile.info.hover(text);
                    }
                }, 476 + 32);

                h.addCentredY(new HOVERABLE.Sprite(UI.icons().m.skull)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        RD.RACES().edicts.massacre.info.hover(text);
                    }
                }, 476 + 32 * 2);

                add(h);
            }

            foreach (RDRace r in RD.RACES().all)
            {
                GuiSection row = new GuiSection();
                row.addRightC(0, new HOVERABLE.Sprite(r.race.appearance().icon)
                {

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        r.race.info.hover(b);
                        b.NL(8);

                        r.race.boosts.hover(text, 1.0, BoostableCat.TYPE_WORLD);

                        text.NL(8);

                        r.race.pref().hoverOther(b);
                    }

                });

                GuiSection popGrowth = new GuiSection()
                {

                    string mt = Dic.¤¤Modifiers + ": " + Dic.¤¤Target;

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        base.hoverInfoGet(text);
                        text.NL(8);
                        b.sep();
                        r.loyalty.target.hover(b, g.get(), Dic.¤¤Factors, true);
                    }

                };

                RENDEROBJ popRender = new RENDEROBJ.RenderImp(140, 16)
                {
                    public override void render(SPRITE_RENDERER rr, float ds, bool isHovered)
                    {
                        double c = (1 + r.loyalty.getD(g.get()) / 10.0) / 2.0;
                        double t = (1 + r.loyalty.target.get(g.get()) / 10.0) / 2.0;
                        GMeter.renderC(rr, c, t, body);
                    }
                };

                popGrowth.add(popRender);
                popGrowth.addRightC(8, new GStat()
                {
                    public override void update(GText text)
                    {
                        double gg = ((int)((r.loyalty.target.get(g.get())) * 100)) / 100.0;
                        GFORMAT.f0(text, gg);
                    }
                });
                popGrowth.body().incrW(66);
                row.addRightC(8, popGrowth);

                foreach (RDRaceEdict e in RD.RACES().edicts.all)
                {

                    ACTION a = new ACTION()
                    {
                        public override void exe()
                        {
                            int i = (e.toggled(r).get(g.get()) + 1) & 1;
                            if (i == 1)
                                massacreAsked = VIEW.renderSecond();
                            foreach (RDRaceEdict ee in RD.RACES().edicts.all)
                                ee.toggled(r).set(g.get(), 0);
                            e.toggled(r).set(g.get(), i);
                        }
                    };

                    row.addRightC(8, new GButt.Checkbox()
                    {
                        protected override void clickA()
                        {
                            int i = (e.toggled(r).get(g.get()) + 1) & 1;
                            if (i == 1 && VIEW.renderSecond() - massacreAsked > 20)
                            {
                                VIEW.inters().yesNo.activate(¤¤eWarning, a, ACTION.NOP, true);
                            }
                            else
                                a.exe();
                        };

                        protected override void renAction()
                        {
                            selectedSet(e.toggled(r).get(g.get()) == 1);
                        }

                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            e.info.hover(text);
                            text.NL(8);
                            e.boosts.hover(text, 1.0, BoostableCat.TYPE_WORLD);
                            e.boosts.hover(text, 1.0, BoostableCat.TYPE_WORLD);
                        }

                    });
                }

                row.body().setWidth(width - 24);

                row.add(new RENDEROBJ.RenderImp(row.body().width(), 4)
                {
                    public override void render(SPRITE_RENDERER r, float ds)
                    {
                        GCOLOR.UI().border().render(r, body.x1(), body.x2(), body.y1() + 1, body.y1() + 2);
                    }
                }, 0, row.body().y2());

                rows.add(row);
            }

            height -= body().height();
            height = rows.get(0).body().height() * (height / rows.get(0).body().height());

            add(new GScrollRows(rows, height).view(), 0, body().y2());
        }
    }
}