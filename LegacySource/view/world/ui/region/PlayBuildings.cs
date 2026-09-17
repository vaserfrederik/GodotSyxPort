using System;
using System.Collections.Generic;
using snake2d;
using util.datatypes;
using util.gui;
using util.gui.misc;
using util.gui.table;
using util.text;
using world.map.regions;
using world.region;
using world.region.building;

namespace view.world.ui.region
{
    class PlayBuildings : GuiSection
    {
        private readonly List<RENDEROBJ> activeButts = new List<RENDEROBJ>(RD.BUILDINGS().all.size);
        private readonly RENDEROBJ[] butts = new RENDEROBJ[RD.BUILDINGS().all.size + 1];

        private static readonly CharSequence ¤¤Click = "Click to construct buildings.";

        static PlayBuildings()
        {
            D.ts(typeof(PlayBuildings));
        }

        public int width = 64 + 8;
        public static readonly int height = 64 + 24;
        private readonly int amX;

        private readonly PlayBuildingsPop build;

        public PlayBuildings(GETTER_IMP<Region> g, int width, int height)
        {
            build = new PlayBuildingsPop(null, g);
            this.width = ((width - 7 * 4) / 7) & ~0b01;
            for (int i = 0; i < RD.BUILDINGS().sorted.size; i++)
            {
                var bu = RD.BUILDINGS().sorted.get(i);

                butts[i] = new HOVERABLE.HoverableAbs(PlayBuildingsPop.width, PlayBuildingsPop.height)
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                    {
                        build.render(bu, g.get(), body, r, true, false, isHovered);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        build.hover(bu, g.get(), text);
                    }
                };
            }

            amX = (width - 24) / (this.width);

            var builder = new GTableBuilder
            {
                nrOFEntries = () => (int)Math.Ceiling(activeButts.Count / (double)amX)
            };

            builder.column(null, amX * this.width, new GRowBuilder
            {
                public RENDEROBJ build(GETTER<Integer> ier)
                {
                    return new Row(ier);
                }
            });

            int hi = height - body().height() - 16;
            int h = hi / (PlayBuildings.height + 16);
            RENDEROBJ cc = h < 1 ? builder.createHeight(PlayBuildings.height, false) : builder.createHeight((PlayBuildings.height + 16) * h, false);

            var sec = new GuiSection
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    bool hov = hoveredIs();
                    GButt.ButtPanel.renderBG(r, true, false, hov, body());
                    GButt.ButtPanel.renderFrame(r, body());

                    activeButts.ClearSloppy();
                    for (int i = 0; i < RD.BUILDINGS().sorted.size; i++)
                    {
                        var b = RD.BUILDINGS().sorted.get(i);
                        if (RD.BUILDINGS().tmp().level(b, g.get()) != 0)
                            activeButts.add(butts[i]);
                    }

                    if (activeButts.Count == 0)
                        UI.icons().m.building.renderC(r, body());

                    base.render(r, ds);
                }

                public override bool click()
                {
                    build.pop(PlayBuildings.this.body());
                    return true;
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                    if (text.emptyIs())
                    {
                        text.title(Dic.¤¤Buildings);
                        text.text(¤¤Click);
                    }
                }
            };
            sec.add(cc);
            sec.pad(6);
            addRelBody(12, DIR.S, sec);
        }

        private class Row : GuiSection
        {
            private readonly GETTER<Integer> ier;

            public Row(GETTER<Integer> ier)
            {
                this.ier = ier;
                body().setHeight(height + 16);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                int x1 = body().x1();
                int y1 = body().y1();
                clear();
                int s = ier.get() * amX;
                for (int i = 0; i < amX && i + s < activeButts.Count; i++)
                {
                    addRightC(0, activeButts.get(i + s));
                }
                body().setHeight(height + 16);
                body().moveX1(x1);
                body().moveY1(y1);
                base.render(r, ds);
            }
        }
    }
}