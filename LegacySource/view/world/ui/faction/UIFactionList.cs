using System;
using System.Collections.Generic;
using System.Linq;

namespace View.World.UI.Faction
{
    class UIFactionList : ISidePanel
    {
        private readonly List<FactionNPC> sorted = new List<FactionNPC>(FACTIONS.MAX());

        public static int ROW_HEIGHT = 30;
        private readonly int width = C.SG * 264;

        private readonly StringInputSprite filter = new StringInputSprite(20, UI.FONT().S);
        private readonly GTableBuilder builder;
        private Faction hovered;

        private readonly Tree<FactionNPC> sorter = new Tree<FactionNPC>(FACTIONS.MAX());

        static UIFactionList()
        {
            sorter.IsGreaterThan = (current, cmp) => value(current) > value(cmp);
        }

        private static double value(Faction f)
        {
            double d = 1.0 - 1.0 / RD.DIST().distance(f);
            if (DIP.WAR().is(FACTIONS.player(), f))
                return 0 + d;
            if (DIP.get(FACTIONS.player(), f).trades)
                return FACTIONS.MAX() + d;
            if (RD.DIST().reachable(f))
                return FACTIONS.MAX() * 2 + d;
            if (RD.DIST().factionHasRegionBorderingPlayer(f))
                return FACTIONS.MAX() * 3 + d;
            return FACTIONS.MAX() * 4 + d;
        }

        public UIFactionList(int HEIGHT)
        {
            this.section = new GuiSection
            {
                Render = (r, ds) =>
                {
                    sorted.Clear();
                    foreach (FactionNPC f in FACTIONS.NPCs())
                        sorter.Add(f);
                    while (sorter.HasMore())
                    {
                        FactionNPC f = sorter.PollSmallest();
                        if (filter.Text.Length > 0)
                        {
                            if (f.Name.ContainsText(filter.Text))
                                sorted.Add(f);
                        }
                        else
                            sorted.Add(f);
                    }
                    WORLD.OVERLAY().factions.Add();

                    base.Render(r, ds);
                    hovered = null;
                }
            };

            filter.PlaceHolder(Dic.¤¤Search);

            GInput input = new GInput(filter);

            section.AddDownC(8, input);

            builder = new GTableBuilder
            {
                NrOFEntries = () => sorted.Count
            };

            builder.Column(null, width, new GRowBuilder
            {
                Build = ier => new Button(ier)
            });

            section.AddDownC(8, builder.CreateHeight(HEIGHT - 16 - section.Body().Height, false));
        }

        private class Button : GuiSection
        {
            private readonly GETTER<int> ier;

            public Button(GETTER<int> ier)
            {
                this.ier = ier;

                RENDEROBJ o;

                o = new RENDEROBJ.RenderImp(Icon.L * 2 + 16, Icon.L * 2)
                {
                    Render = (r, ds) =>
                    {
                        FactionNPC f = g();
                        if (f == null)
                            return;
                        f.banner().HUGE.Render(r, body().x1(), body().y1());
                        Royalty ro = f.court().king().roy();
                        int x1 = body().x1() + Icon.L + Icon.L / 2;
                        int y1 = body().y1() + 8;
                        STATS.APPEARANCE().portraitRender(r, ro.induvidual, x1, y1, 1);
                        ro.induvidual.race().appearance().crown.crowns().get(0).renderScaled(r, x1, y1 + 8, 1);
                    }
                };

                add(o);

                o = new GStat
                {
                    Update = text =>
                    {
                        FactionNPC f = g();
                        if (f != null)
                            text.lablifySub().add(f.name);
                    }
                }.r(DIR.NW);
                add(o, getLastX2() + 12, 4);

                GuiSection pp = new GuiSection();
                pp.add(GMeter.sprite(GMeter.C_ORANGE, new DOUBLE
                {
                    GetD = () =>
                    {
                        FactionNPC f = g();
                        if (f == null)
                            return 0;
                        return RD.RACES().population.faction().get(f) / (10 * RD.RACES().maxPopReg());
                    }
                }, 100, 24), 0, 0);

                pp.addCentredY(new GStat
                {
                    Update = text =>
                    {
                        text.darkBG();
                        FactionNPC f = g();
                        if (f == null)
                            return;

                        int am = RD.RACES().population.faction().get(f);
                        GFORMAT.i(text, am);
                    }
                }.hh(SPRITES.icons().s.human), 4);

                pp.add(new SPRITE.Imp(100, 16)
                {
                    Render = (r, X1, X2, Y1, Y2) =>
                    {
                        FactionNPC f = g();
                        if (f == null)
                            return;
                        double c = ROPINION.get(f.court().king().roy());
                        GMeterCol col = c < DIP.get(f).opinionNeeded ? GMeter.C_RED : GMeter.C_BLUE;

                        c /= DIP.ALLY().opinionNeeded + 1;
                        GMeter.render(r, col, c, X1, X2, Y1, Y2);

                        int x1 = (int)(X1 + (X2 - X1) * DIP.get(f).opinionNeeded / (DIP.ALLY().opinionNeeded + 1));
                        GCOLOR.UI().border().render(r, x1 - 2, x1 + 2, Y1, Y2);
                    }
                }, 0, pp.body().y2() + 4);

                pp.addRelBody(8, DIR.E, new SPRITE.Imp(Icon.S * 2)
                {
                    Render = (r, X1, X2, Y1, Y2) =>
                    {
                        FactionNPC f = g();
                        if (DIP.get(f) != DIP.NEUTRAL())
                            DIP.get(f).icon.render(r, X1, X2, Y1, Y2);
                    }
                });

                add(pp, getLastX1(), getLastY2() + 4);

                pad(8, 6);
                body().setWidth(width);
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                bool hovered = hoveredIs();
                FactionNPC f = g();
                bool selected = VIEW.world().UI.factions.openIs(f);
                bool active = f.capitolRegion() != null;

                hovered |= UIFactionList.this.hovered == f;

                if (hovered || selected)
                {
                    WORLD.MINIMAP().hilight(f);
                    WORLD.OVERLAY().hover(f.capitolRegion());
                }

                base.Render(r, ds);

                if (!RD.DIST().reachable(f))
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body(), -4);
                    OPACITY.unbind();
                }

                GButt.ButtPanel.renderFrame(r, body());
            }

            private COLOR cbad = COLOR.RED100.makeSaturated(0.75).shade(0.75);
            private COLOR cSoSo = COLOR.YELLOW100.makeSaturated(0.75).shade(0.75);
            private COLOR cOK = COLOR.BLUE100.makeSaturated(0.75).shade(0.75);
            private COLOR cGood = COLOR.NYAN100.makeSaturated(0.75).shade(0.75);

            private void bg(SPRITE_RENDERER r)
            {
                FactionNPC f = g();

                double v = ROPINION.trust().get(f);

                COLOR c = cbad;
                if (v < 1)
                    c = ColorImp.TMP.interpolate(cbad, cSoSo, v);
                else
                    c = ColorImp.TMP.interpolate(cOK, cGood, (v - 1));

                if (selectedIs())
                    OPACITY.O75.bind();
                else if (hoveredIs())
                    OPACITY.O66.bind();
                else
                    OPACITY.O50.bind();
                c.render(r, body(), -4);
                OPACITY.unbind();
            }

            public override bool click()
            {
                open(g(), false);
                return base.click();
            }

            private FactionNPC g()
            {
                return sorted[ier.get()];
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                base.hoverInfoGet(text);
                if (text.emptyIs())
                    VIEW.world().UI.factions.hover(text, g());
            }
        }

        void open(FactionNPC f, bool shove)
        {
            if (f != null)
            {
                if (shove)
                    builder.set(sorted.IndexOf(f));
                VIEW.world().UI.factions.open(f);
                //VIEW.world().UI.faction.open(f);
                //VIEW.world().window.centererTile.set(f.capitolRegion().cx(), f.capitolRegion().cy());
            }
        }

        void hover(Faction f)
        {
            hovered = f;
        }
    }
}