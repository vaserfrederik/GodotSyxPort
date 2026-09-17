using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game;
using game.boosting.tmp;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty.opinion;
using init.settings;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.data.GETTER;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using world;
using world.map.pathing;
using world.map.regions;
using world.region;
using world.region.building;

namespace view.world.ui.region
{
    final class PlayInfo : GuiSection
    {
        private static CharSequence ¤¤abandon = "Abandon Region";
        private static CharSequence ¤¤abandonQ = "Do you wish to abandon this region? It will be turned over into the hands of rebels.";

        private static CharSequence ¤¤autonomy = "Give Autonomy";
        private static CharSequence ¤¤autonomyD = "Do you wish to give this region autonomy and bestow upon it self-rule? The new faction will be in your debt.";
        private static CharSequence ¤¤autonomyE = "There simply are no suitable faction rulers to appoint a new king here. Perhaps we can gift this region to an ally or vassal instead?";

        private static CharSequence ¤¤DistanceToCapitol = "Distance to Capitol";

        static
        {
            D.ts(typeof(PlayInfo));
        }

        public PlayInfo(GETTER_IMP<Region> g, int WIDTH)
        {
            int i = 0;
            int cols = 7;
            int width = 78;
            int height = 48;
            DIR align = DIR.C;

            {
                SPRITE s = new SPRITE.Imp(48, 16)
                {
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        double c = get(RD.HEALTH().getD(g.get()));
                        double t = get(RD.HEALTH().boostablee.get(g.get()));

                        GMeter.renderC(r, c, t, X1, X2, Y1, Y2);

                        if (RD.HEALTH().outbreak.get(g.get()) == 1)
                        {
                            Y1 -= 24;
                            OPACITY.O25TO100.bind();
                            UI.icons().m.disease.render(r, X1, Y1);
                            OPACITY.unbind();
                        }
                    }

                    private double get(double d)
                    {
                        return CLAMP.d(d, 0, 1);
                    }
                };

                GHeader.HeaderVertical h = new HeaderVertical(UI.icons().m.heart, s)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        RD.HEALTH().hover(b, g.get());
                    }
                };

                addGridD(h, i++, cols, width, height, align);
            }

            {
                RENDEROBJ h = new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.perc(text, CLAMP.d(RD.DIST().bProximity.get(g.get()), 0, 1));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.textLL(¤¤DistanceToCapitol);
                        b.add(GFORMAT.i(b.text(), RD.DIST().distance().get(g.get())));
                        b.NL();
                        b.textLL(Dic.¤¤Neighbours);
                        b.NL();
                        foreach (RegDist d in WORLD.PATH().regFinder.all(g.get(), Treaty.REG_NEIGHS, WRegSel.DUMMY()))
                        {
                            Region reg = d.reg;
                            if (reg.faction() == null)
                            {
                                b.add(FBanner.rebel.MEDIUM);
                            }
                            else
                            {
                                b.add(reg.faction().banner().MEDIUM);
                            }
                            b.textL(reg.info.name());
                            b.tab(7);
                            b.add(GFORMAT.i(b.text(), d.distance));
                            b.NL();
                        }

                        b.sep();

                        b.title(RD.DIST().bProximity.name);
                        b.text(RD.DIST().bProximity.desc);
                        b.sep();
                        RD.DIST().bProximity.hover(b, g.get(), null, true);
                    }
                }.hv(UI.icons().m.wheel);

                addGridD(h, i++, cols, width, height, align);
            }

            {
                SPRITE s = new SPRITE.Imp(48, 16)
                {
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        GMeter.render(r, GMeter.C_REDGREEN, RD.OWNER().affiliation.getD(g.get()), X1, X2, Y1, Y2);
                    }
                };

                GHeader.HeaderVertical h = new HeaderVertical(UI.icons().m.flag, s)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.textLL(Dic.¤¤Current);
                        b.tab(6);
                        b.add(GFORMAT.perc(b.text(), RD.OWNER().affiliation.getD(g.get())));
                        b.NL(8);
                        b.add(RD.OWNER().affiliation.info());
                    }
                };

                addGridD(h, i++, cols, width, height, align);
            }

            {
                RENDEROBJ o = new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.percInv(text, RD.DEVASTATION().current.getD(g.get()));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.add(RD.DEVASTATION().current.info());
                    }
                }.hv(UI.icons().m.skull);
                addGridD(o, i++, cols, width, height, align);
            }

            {
                RENDEROBJ o = new GStat()
                {
                    public override void update(GText text)
                    {
                        double v = CLAMP.d(GAME.raiders().entry.get(g.get()).security(), -1, 1);
                        GFORMAT.perc(text, v, 0);
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        GAME.raiders().entry.get(g.get()).hoverInfoGet(b);
                    }
                }.hv(UI.icons().m.raider);
                addGridD(o, i++, cols, width, height, align);
            }

            foreach (RDBuildPoint c in RD.BUILDINGS().costs.ALL)
            {
                RENDEROBJ o = new GStat()
                {
                    public override void update(GText text)
                    {
                        // Update logic for GStat
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        // Hover info logic for GStat
                    }
                }.hv(c.icon);

                addGridD(o, i++, cols, width, height, align);
            }

            addRightC(0, actions(g));
            addRightC(0, MiscBasics.info(g));
            addRightC(0, TmpBoostingButt.make(g, GAME.BOOST().regions));

            if (S.get().developer)
            {
                addRightC(0, new GButt.ButtPanel(UI.icons().m.cog)
                {
                    PlayDebug dd = new PlayDebug();

                    protected override void clickA()
                    {
                        dd.reg = g.get();
                        VIEW.inters().popup.show(dd, this);
                    }
                });
            }
        }

        private GuiSection actions(GETTER_IMP<Region> g)
        {
            GuiSection butts = new GuiSection();
            butts.addRightC(0, new GButt.ButtPanel(UI.icons().m.cancel)
            {
                ACTION aa = new ACTION()
                {
                    public override void exe()
                    {
                        RD.setFaction(g.get(), null, true);
                    }
                };

                protected override void clickA()
                {
                    VIEW.inters().yesNo.activate(¤¤abandonQ, aa, ACTION.NOP, true);
                }
            }.hoverTitleSet(¤¤abandon));

            return butts;
        }
    }
}