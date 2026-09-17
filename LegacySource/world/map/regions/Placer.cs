using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.rendering;
using util.text;
using view.main;
using view.subview;
using view.tool;
using view.world.generator.tools;
using world;
using world.map.regions.centre;
using world.overlay;

namespace world.map.regions
{
    class Placer : ArrayListGrower<PLACABLE>
    {
        private static readonly long serialVersionUID = 1L;
        private readonly INT.IntImp ii = new IntImp(1, WREGIONS.MAX - 1);
        private static readonly CharSequence ¤¤name = "Place Player";
        private static readonly CharSequence ¤¤locate = "locate";
        private static readonly CharSequence ¤¤removeC = "Remove Completely";
        private static readonly CharSequence ¤¤remove = "Remove";
        private static readonly CharSequence ¤¤namem = "Name";
        private static readonly CharSequence ¤¤centre = "Centre";

        static Placer()
        {
            D.ts(Placer.class);
        }

        public Placer()
        {
            LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();
            GSliderInt sl = new GSliderInt(ii, 100, true, true)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.add(b.text().add(ii.get()).add(':').s().add(get().info.name()));
                }
            };
            HovOverlay hov = new HovOverlay();

            butts.add(sl);
            butts.add(new GButt.ButtPanel(Dic.¤¤name)
            {
                protected override void clickA()
                {
                    VIEW.inters().input.requestInput(new STRING_RECIEVER()
                    {
                        public override void acceptString(CharSequence string)
                        {
                            if (string != null)
                                get().info.name().clear().add(string);
                        }
                    }, Dic.¤¤name);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(get().info.name());
                }
            });

            butts.add(new GButt.ButtPanel(UI.icons().m.crossair)
            {
                protected override void clickA()
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                        if (WORLD.REGIONS().map.get(c) == get())
                        {
                            VIEW.world().window.centererTile.set(c);
                            return;
                        }
                }
            }.hoverTitleSet(¤¤locate));

            butts.add(new GButt.ButtPanel(UI.icons().m.skull)
            {
                protected override void clickA()
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                    {
                        Region r = WORLD.REGIONS().pmap.get(c);
                        if (r != null && r.index() == ii.get())
                            WORLD.REGIONS().pmap.set(c.x(), c.y(), null);
                    }
                }
            }.hoverInfoSet(¤¤removeC));

            PLACABLE undo = new PlacableMulti(¤¤remove + ": " + Dic.¤¤Region, "", UI.icons().m.place_ellispse.resized(IconS.L).twin(UI.icons().m.anti, DIR.C, 0))
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    WORLD.REGIONS().pmap.set(tx, ty, null);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }

                public override void updateRegardless(GameWindow window, AREA selected)
                {
                    hov.hovered = null;
                    hov.add();
                }
            };

            PLACABLE p = new PlacableMulti(Dic.¤¤Region, "", UI.icons().m.place_ellispse.resized(IconS.L))
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    WORLD.REGIONS().pmap.set(tx, ty, get());
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }

                public override PLACABLE getUndo()
                {
                    return undo;
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return butts;
                }

                public override void updateRegardless(GameWindow window, AREA selected)
                {
                    hov.hovered = get();
                    hov.add();
                }

                public override void placeInfo(GBox b, int oktiles, AREA a)
                {
                    if (a.area() == 1)
                        hover(b, a.body().x1(), a.body().y1());
                    base.placeInfo(b, oktiles, a);
                }
            };
            add(p);
            add(undo);
            p = new PlacableSimpleTile(¤¤namem + ": " + Dic.¤¤Region)
            {
                public override void place(int tx, int ty)
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    VIEW.inters().input.requestInput(new STRING_RECIEVER()
                    {
                        public override void acceptString(CharSequence string)
                        {
                            if (string != null)
                                reg.info.name().clear().add(string);
                        }
                    }, Dic.¤¤name);
                }

                public override CharSequence isPlacable(int tx, int ty)
                {
                    return WORLD.REGIONS().map.get(tx, ty) != null ? null : "Not placable";
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                }

                public override void updateRegardless(GameWindow window)
                {
                    base.updateRegardless(window);
                }
            };
            add(p);
            p = new PlacableSimpleTile(¤¤centre + ": " + Dic.¤¤Region)
            {
                public override void place(int tx, int ty)
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    reg.info.centreSet(tx, ty);
                }

                public override CharSequence isPlacable(int tx, int ty)
                {
                    return WORLD.REGIONS().map.get(tx, ty) != null ? null : "Not placable";
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                }

                public override void updateRegardless(GameWindow window)
                {
                    base.updateRegardless(window);
                }
            };
            add(p);
            p = new PlacableFixed(WCentre.TILE_DIM, WCentre.TILE_DIM)
            {
                public override void place(int tx, int ty, int rx, int ry)
                {
                    if (rx == 0 && ry == 0)
                        clear();
                    WORLD.REGIONS().pmap.set(tx, ty, WORLD.REGIONS().getByIndex(0));
                    if (rx == WCentre.TILE_DIM / 2 && ry == WCentre.TILE_DIM / 2)
                        WORLD.REGIONS().getByIndex(0).info.centreSet(tx, ty);
                }

                public override CharSequence placable(int tx, int ty, int rx, int ry)
                {
                    if (rx == 0 && ry == 0)
                        return WorldCentrePlacablity.terrain(tx, ty);
                    return null;
                }

                public override void placeInfo(GBox b, int x1, int y1)
                {
                    info.placeInfo(b, x1, y1, FACTIONS.player().race());
                }

                public void clear()
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                    {
                        if (WORLD.REGIONS().map.get(c) == WORLD.REGIONS().getByIndex(0))
                            WORLD.REGIONS().pmap.set(c, null);
                    }
                }

                public override void updateRegardless(GameWindow window)
                {
                    hov.hovered = null;
                    hov.add();
                }
            };
            add(p);
        }

        private class HovOverlay : OverlayTile
        {
            public HovOverlay() : base(true, false) { }

            private Region hovered;

            protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
            {
                Region reg = WORLD.REGIONS().map.get(it.tile());
                if (reg != null)
                {
                    COLOR c = reg == hovered ? COLOR.WHITE100 : COLOR.WHITE30;
                    c.bind();
                    if (it.tx() == reg.cx() && it.ty() == reg.cy())
                    {
                        foreach (DIR d in DIR.ALL)
                        {
                            int m = d.mask();
                            if (!d.isOrtho())
                                m = d.next(1).mask() | d.next(-1).mask();
                            m = ~m;
                            m &= 0x0F;
                            SPRITES.cons().BIG.outline.render(r, m, it.x() + d.x() * C.TILE_SIZE, it.y() + d.y() * C.TILE_SIZE);
                        }
                    }
                    else
                    {
                        int m = 0;
                        foreach (DIR d in DIR.ORTHO)
                        {
                            if (WORLD.REGIONS().map.get(it.tx(), it.ty(), d) == reg)
                                m |= d.mask();
                        }
                        SPRITES.cons().BIG.dashed.render(r, m, it.x(), it.y());
                    }
                }
            }
        }

        private void hover(GBox b, int tx, int ty)
        {
            Region rr = WORLD.REGIONS().map.get(tx, ty);
            if (rr != null)
            {
                b.NL();
                b.add(b.text().add(Dic.¤¤Current).add(':').s().add(rr.index()).add(rr.info.name()));
                b.NL();
            }
        }

        private Region get()
        {
            return WORLD.REGIONS().getByIndex(ii.get());
        }
    }
}