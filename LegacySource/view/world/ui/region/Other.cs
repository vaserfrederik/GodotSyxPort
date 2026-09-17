using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using init.settings;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util.data.GETTER;
using util.data.INT;
using util.gui.misc;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using view.tool;
using world;
using world.map.regions;

namespace view.world.ui.region
{
    class Other : ISidePanel, RV
    {
        private static readonly string ¤¤eCurrent = "Assigned envoys";
        private static readonly string ¤¤eAvailable = "Available envoys";
        static
        {
            D.ts(typeof(Other));
        }

        public static readonly int width = 450;

        private GETTER_IMP<Region> g = new GETTER_IMP<Region>();
        private readonly RENDEROBJ mi = MiscMore.garrison(g, 288);
        public Other(ToolManager m, ISidePanels p)
        {
            titleSet(Dic.¤¤Region);
            section = new GuiSection
            {
                Render = (r, ds) =>
                {
                    if (ff != g.Get().faction())
                        last().Remove(this);
                    base.Render(r, ds);
                }
            };
            section.AddDown(0, banner(g, this, p));
            section.AddDown(8, new Emiss(g));
            section.AddRelBody(8, DIR.S, info(g));
            section.AddRelBody(8, DIR.S, infoMore(g));
            section.AddRelBody(8, DIR.S, buildings(g));
            section.AddRelBody(8, DIR.S, more(g));
        }
        private Faction ff;

        public ISidePanel get(Region reg)
        {
            g.Set(reg);
            ff = reg.faction();
            return this;
        }

        private static RENDEROBJ banner(GETTER_IMP<Region> g, ISidePanel panel, ISidePanels p)
        {
            GuiSection s = new GuiSection();
            s.Add(new ClickableAbs(Icon.HUGE + 16, Icon.HUGE + 16)
            {
                HoverInfoGet = text =>
                {
                    VIEW.world().UI.factions.hover(text, g.Get().faction());
                },
                Render = (r, ds, isActive, isSelected, isHovered) =>
                {
                    Region reg = g.Get();
                    if (reg.faction() == null)
                    {
                        FBanner.rebel.HUGE.renderC(r, body().cX(), body().cY());
                    }
                    else
                    {
                        GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body());
                        reg.faction().banner().HUGE.renderC(r, body().cX(), body().cY());
                        if (DIP.WAR().is(FACTIONS.player(), reg.faction()))
                        {
                            COLOR.REDISH.bind();
                            UI.icons().s.sword.render(r, body().x2() - 16, body().y1());
                        }
                        GButt.ButtPanel.renderFrame(r, body());
                    }
                },
                ClickA = () =>
                {
                    if (g.Get().faction() is FactionNPC)
                    {
                        VIEW.world().UI.factions.open((FactionNPC)g.Get().faction());
                    }
                }
            });

            s.Add(new GStat(UI.FONT().H2)
            {
                Update = text =>
                {
                    text.lablify();
                    text.add(g.Get().info.name());
                }
            }.r(DIR.NW), s.getLastX2() + 16, s.getLastY1() + 8);

            if (S.get().developer)
            {
                s.Add(new GButt.ButtPanel(UI.icons().s.cog)
                {
                    ClickA = () =>
                    {
                        p.Add(VIEW.world().UI.regions.player.get(g.Get()), true);
                    }
                }, s.getLastX1(), s.getLastY2() + 4);
            }

            return s;
        }

        private class Emiss : GuiSection
        {
            private readonly GETTER_IMP<Region> g;

            public Emiss(GETTER_IMP<Region> g)
            {
                this.g = g;
                add(UI.icons().m.flag, 0, 0);

                int wi = 150;

                RENDEROBJ r = new RENDEROBJ.RenderImp(wi, 18)
                {
                    Render = (r, ds) =>
                    {
                        GMeter.render(r, GMeter.C_REDGREEN, RD.OWNER().affiliation.getD(g.Get()), body());
                    }
                };

                addRightC(16, r);

                INTE ii = new INTE
                {
                    min = () => 0,
                    max = () => FACTIONS.player().emissaries.assimilate.max(g.Get()),
                    get = () => FACTIONS.player().emissaries.assimilate.get(g.Get()),
                    set = (t) => FACTIONS.player().emissaries.assimilate.set(g.Get(), t)
                };

                GSliderInt sl = new GSliderInt(ii, wi / 2, true);

                addRightC(16, sl);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(RD.OWNER().affiliation.info().name);
                b.text(RD.OWNER().affiliation.info().desc);
                b.NL(8);
                b.textLL(¤¤eCurrent);
                b.tab(7);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().emissaries.assimilate.get(g.Get())));
                b.NL();
                b.textLL(¤¤eAvailable);
                b.tab(7);
                b.add(GFORMAT.i(b.text(), FACTIONS.player().emissaries.available()));
                b.NL();
            }
        }

        private static RENDEROBJ info(GETTER_IMP<Region> g)
        {
            return MiscBasics.info(g);
        }

        private static RENDEROBJ infoMore(GETTER_IMP<Region> g)
        {
            GuiSection s = new GuiSection();
            s.add(MiscBasics.rel(g));
            s.addDownC(2, MiscBasics.prospect(g));
            return new Mystery(g, s, 0.1);
        }

        private static RENDEROBJ more(GETTER_IMP<Region> g)
        {
            GuiSection ss = new GuiSection();
            ss.body().incrW(64);
            ss.body().incrH(1);

            ss.addRightC(0, new GStat
            {
                Update = text =>
                {
                    GFORMAT.iofkInv(text, RD.MILITARY().garrison.get(g.Get()), (int)RD.MILITARY().garrisonTarget(g.Get()));
                },
                HoverInfoGet = b =>
                {
                    b.title(Dic.¤¤garrison);
                }
            }.hv(UI.icons().m.shield));
            ss.addRightC(64, MiscMore.garrison(g, width - ss.body().width() - 64));

            return new Mystery(g, ss, 0.1);
        }

        private static RENDEROBJ buildings(GETTER_IMP<Region> g)
        {
            GuiSection s = new GuiSection();
            s.Add(MiscBasics.buildings(g));
            return s;
        }

        private class Mystery : RENDEROBJ
        {
            private readonly GETTER_IMP<Region> g;
            private readonly RENDEROBJ obj;
            private readonly double reveal;

            public Mystery(GETTER_IMP<Region> g, RENDEROBJ obj, double reveal)
            {
                this.g = g;
                this.obj = obj;
                this.reveal = reveal;
            }

            protected override RENDEROBJ pget()
            {
                if (RD.OWNER().affiliation.getD(g.Get()) < reveal)
                    return new ClickableAbs(obj.body().width(), obj.body().height())
                    {
                        Render = (r, ds, isActive, isSelected, isHovered) =>
                        {
                            GButt.ButtPanel.renderBG(r, true, false, hoveredIs(), body);
                            SPRITES.icons().m.questionmark.renderC(r, body);
                            GButt.ButtPanel.renderFrame(r, body);
                        },
                        HoverInfoGet = text =>
                        {
                            GBox b = (GBox)text;
                            GText t = b.text();
                            t.add(¤¤reveal);
                            t.insert(0, (int)(100 * reveal));
                            b.text(t);
                        },
                        ClickA = () =>
                        {
                            if (S.get().developer)
                                RD.OWNER().affiliation.set(g.Get(), (int)Math.Ceiling(reveal * RD.OWNER().affiliation.max(g.Get())));
                        }
                    };
                return obj;
            }
        }

        private readonly OtherHov hov = new OtherHov();

        public void hover(GBox box, Region reg)
        {
            hov.hover(reg, box);
        }

        public bool added(ISidePanels pans, Region reg)
        {
            return pans.added(this) && reg == g.Get();
        }

        protected override void update(float ds)
        {
            WORLD.OVERLAY().hover(g.Get());
        }

        public void hoverGarrison(GBox box, Region reg)
        {
            box.title(reg.info.name());
            g.Set(reg);
            box.add(mi);
        }
    }
}