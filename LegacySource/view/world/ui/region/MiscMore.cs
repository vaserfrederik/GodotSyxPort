using System;
using System.Collections.Generic;
using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data.GETTER;
using util.data.GETTER.GETTER_IMP;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using world.army;
using world.map.regions;
using world.region;
using world.region.building;

namespace view.world.ui.region
{
    final class MiscMore : GuiSection
    {
        public static RENDEROBJ garrison(GETTER_IMP<Region> g, int width)
        {
            GuiSection ss = new GuiSection();
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
            int row = (width / Icon.M);
            if (Config.battle().REGION_MAX_DIVS / row >= 3)
                row = ((width - 24) / Icon.M);

            for (int i = 0; i < Config.battle().REGION_MAX_DIVS; i++)
            {
                GuiSection s = new GuiSection();
                rows.Add(s);
                for (int k = 0; k < row && i < Config.battle().REGION_MAX_DIVS; k++)
                {
                    s.addRightC(0, new DivCard(i, g));
                    i++;
                }
            }

            if (rows.Count < 3)
            {
                GuiSection s = new GuiSection();
                foreach (RENDEROBJ o in rows)
                    s.addDown(0, o);
                ss.addRightCAbs(48, s);
            }
            else
            {
                ss.addRightCAbs(48, new GScrollRows(rows, rows[0].body().height() * 3).view());
            }
            return ss;
        }

        public static RENDEROBJ buildings(GETTER_IMP<Region> g)
        {
            int cols = 11;
            ArrayList<RDBuilding> buildings = new ArrayList<RDBuilding>(RD.BUILDINGS().all.Count);

            GuiSection ss = new GuiSection()
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    buildings.clear();
                    foreach (RDBuilding bu in RD.BUILDINGS().all)
                    {
                        if (bu.level.get(g.get()) > 0)
                            buildings.add(bu);
                    }
                    base.render(r, ds);
                }
            };

            GTableBuilder bu = new GTableBuilder()
            {
                public override int nrOFEntries()
                {
                    return (int)Math.Ceiling((double)buildings.Count / cols);
                }
            };

            bu.column(null, cols * 32, new GRowBuilder()
            {
                public override RENDEROBJ build(GETTER<int> ier)
                {
                    GuiSection s = new GuiSection();
                    for (int i = 0; i < cols; i++)
                    {
                        final int k = i;
                        s.addRightC(0, new HOVERABLE.HoverableAbs(32, 32)
                        {
                            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                            {
                                RDBuilding bu = buildings.get(k + ier.get() * cols);
                                if (bu != null)
                                {
                                    bu.levels.get(bu.level.get(g.get())).icon.render(r, body);
                                }
                            }

                            public override void hoverInfoGet(GUI_BOX text)
                            {
                                RDBuilding bu = buildings.get(k + ier.get() * cols);
                                if (bu != null)
                                {
                                    RDBuildingLevel l = bu.levels.get(bu.level.get(g.get()));
                                    text.title(l.name);
                                    text.text(bu.info.desc);
                                }
                            }
                        });
                    }
                    return s;
                }
            });

            ss.add(bu.create(3, false));

            ss.addRelBody(2, DIR.N, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, RD.DEVASTATION().raidCredits(g.get()));
                }
            }.hh(UI.icons().s.money).hoverTitleSet(Dic.¤¤Spoils));

            return ss;
        }

        public static class DivCard : HoverableAbs
        {
            private readonly int di;
            private readonly GETTER_IMP<Region> g;

            DivCard(int di, GETTER_IMP<Region> g) : base(Icon.M, Icon.M)
            {
                this.g = g;
                this.di = di;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                LIST<WDIV> divs = RD.MILITARY().divisions(g.get());

                if (di < divs.size())
                {
                    WDIV d = divs.get(di);
                    d.race().appearance().icon.render(r, body);
                    int width = (int)((body().width() * d.menTarget()) / (double)Config.battle().MEN_PER_DIVISION);
                    double dd = (double)d.men() / d.menTarget();
                    GMeter.render(r, GMeter.C_REDGREEN, dd, body.x1(), body.x1() + width, body.y2() - 8, body.y2());
                }
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                LIST<WDIV> divs = RD.MILITARY().divisions(g.get());

                if (di < divs.size())
                {
                    WDIV d = divs.get(di);
                    VIEW.UI().div.world.hover(d, b);
                }

                base.hoverInfoGet(text);
            }
        }
    }
}