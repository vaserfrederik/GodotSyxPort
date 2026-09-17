using System;
using System.Collections.Generic;
using init.constant;
using init.race;
using init.sprite;
using init.type;
using settlement.main;
using settlement.room.home.house;
using settlement.room.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.rendering;
using util.text;

namespace view.sett.ui.home
{
    final class UIHomeAssign : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "Assign";
        private static readonly CharSequence ¤¤desc = "Lets you assign homes to specific criteria.";
        private static readonly CharSequence ¤¤prob = "Must be placed on a house or a house construction.";

        private static readonly CharSequence ¤¤everyone = "Set permission for everyone";
        private static readonly CharSequence ¤¤none = "Set permission for none";
        private static readonly CharSequence ¤¤permission = "Set permission for:";
        private static readonly CharSequence ¤¤permissionAll = "Set permission for All:";

        static
        {
            D.ts(typeof(UIHomeAssign));
        }

        private readonly HTypeBitsImp data = new HTypeBitsImp(false);

        private readonly LIST<CLICKABLE> butts;

        public UIHomeAssign()
            : base(¤¤name, ¤¤desc, SPRITES.icons().m.citizen)
        {
            GuiSection sec = new GuiSection();
            sec.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.questionmark)
            {
                protected override void clickA()
                {
                    data.setEveryone();
                }
            }.hoverInfoSet(¤¤everyone));
            sec.addRightC(0, new GButt.ButtPanel(HCLASSES.CITIZEN().icon())
            {
                protected override void clickA()
                {
                    data.clear();
                    foreach (Race r in RACES.all())
                        data.set(HGROUP.get(HCLASSES.CITIZEN(), r));
                }
            }.hoverInfoSet(¤¤permissionAll + " " + HCLASSES.CITIZEN().names));
            sec.addRightC(0, new GButt.ButtPanel(HCLASSES.SLAVE().icon())
            {
                protected override void clickA()
                {
                    data.clear();
                    foreach (Race r in RACES.all())
                        data.set(HGROUP.get(HCLASSES.SLAVE(), r));
                }
            }.hoverInfoSet(¤¤permissionAll + " " + HCLASSES.SLAVE().names));
            sec.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.cancel)
            {
                protected override void clickA()
                {
                    data.clear();
                }
            }.hoverInfoSet(¤¤none));

            GuiSection s = new GuiSection();
            foreach (HGROUP t in HGROUP.all())
            {
                CLICKABLE bb = new GButt.ButtPanel(t.icon)
                {
                    protected override void clickA()
                    {
                        if (selectedIs())
                            data.clear(t);
                        else
                            data.set(t);
                    }

                    protected override void renAction()
                    {
                        selectedSet(data.is(t));
                    }
                }.hoverInfoSet(¤¤permission + " " + t.name);

                if (s.getLastX2() > 500)
                {
                    bb.body().moveX1(0).moveY1(s.body().y2());
                    s.add(bb);
                }
                else
                    s.addRightC(0, bb);
            }

            sec.addRelBody(8, DIR.S, s);

            butts = new ArrayList<CLICKABLE>(sec);
        }

        public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            if (room(tx, ty) != null)
                return null;
            return ¤¤prob;
        }

        public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            settingSet(data, tx, ty);
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            return room(fromX, fromY) != null && room(fromX, fromY) == room(toX, toY);
        }

        public override LIST<CLICKABLE> getAdditionalButt()
        {
            ren.add();
            return butts;
        }

        public ROOMA room(int tx, int ty)
        {
            if (availability(tx, ty) != null)
                return (ROOMA)SETT.ROOMS().map.get(tx, ty);
            return null;
        }

        public HomeInstance.State state(int tx, int ty)
        {
            RoomState state = SETT.ROOMS().construction.state(tx, ty);
            if (state is HomeInstance.State)
            {
                return (State)state;
            }
            return null;
        }

        public HTypeBits availability(int tx, int ty)
        {
            HomeInstance h = SETT.ROOMS().HOME.getter.get(tx, ty);
            if (h != null)
                return h.setting();
            State s = state(tx, ty);
            if (s != null)
                return s.egroup;
            return null;
        }

        public void settingSet(HTypeBits bits, int tx, int ty)
        {
            HomeInstance h = SETT.ROOMS().HOME.getter.get(tx, ty);
            if (h != null)
                h.settingSet(bits);
            else
            {
                State s = state(tx, ty);
                if (s != null)
                    s.egroup.copy(bits);
            }
        }

        private readonly ON_TOP_RENDERABLE ren = new ON_TOP_RENDERABLE()
        {
            public void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
            {
                RenderIterator it = data.onScreenTiles();
                while (it.has())
                {
                    ROOMA h = room(it.tx(), it.ty());
                    if (h != null && h.body().cX() == it.tx() && h.body().cY() == it.ty())
                    {
                        render(r, h, availability(it.tx(), it.ty()), it);
                    }
                    it.next();
                }
                remove();
            }

            private readonly ArrayList<HGROUP> rens = new ArrayList<HGROUP>(HGROUP.all().size());

            private void render(Renderer r, ROOMA h, HTypeBits t, RenderIterator it)
            {
                double dx = h.body().x1() + h.body().width() * 0.5;
                dx = dx - h.body().x1();
                int cx = it.x() - C.TILE_SIZEH; // (int) (it.x() + dx*C.TILE_SIZE);

                double dy = h.body().y1() + h.body().height() * 0.5;
                dy = dy - h.body().y1();
                int cy = it.y() - C.TILE_SIZEH; // (int) (it.y() + dy*C.TILE_SIZE);

                int am = 0;
                HGROUP single = null;
                foreach (HGROUP hh in HGROUP.all())
                {
                    if (t.is(hh))
                    {
                        single = hh;
                        am++;
                    }
                }

                if (am == HGROUP.all().size())
                {
                    renderSingle(r, cx, cy, UI.icons().m.questionmark);
                }
                else if (am == 0)
                {
                    renderSingle(r, cx, cy, UI.icons().m.cancel);
                }
                else if (am == 1)
                {
                    renderSingle(r, cx, cy, single.icon);
                }
                else if (am < HGROUP.all().size() / 2)
                {
                    rens.clearSloppy();
                    foreach (HGROUP hh in HGROUP.all())
                    {
                        if (t.is(hh))
                        {
                            rens.add(hh);
                        }
                    }
                    renderMany(r, cx, cy, h, false);
                }
                else
                {
                    rens.clearSloppy();
                    foreach (HGROUP hh in HGROUP.all())
                    {
                        if (!t.is(hh))
                        {
                            rens.add(hh);
                        }
                    }
                    renderMany(r, cx, cy, h, true);
                }
            }

            private void renderSingle(Renderer r, int cx, int cy, SPRITE icon)
            {
                int w = icon.width() * C.SCALE;
                int h = icon.height() * C.SCALE;
                int x1 = cx - w / 2;
                int y1 = cy - h / 2;
                icon.render(r, x1, x1 + w, y1, y1 + h);
            }

            private void renderMany(Renderer r, int cx, int cy, ROOMA house, bool anti)
            {
                int width = house.body().width() * C.TILE_SIZE;
                int w = 24 * C.SCALE / 2;
                int h = rens.get(0).icon.height() * C.SCALE / 2;

                int dx = (width / rens.size());
                dx = CLAMP.i(dx, 1, w);

                int x1 = cx - dx * rens.size() / 2;
                int y1 = cy - h;

                foreach (HGROUP t in rens)
                {
                    t.icon.render(r, x1, x1 + w, y1, y1 + h);
                    if (anti)
                        UI.icons().m.anti.render(r, x1, x1 + w, y1, y1 + h);
                    x1 += dx;
                }
            }
        };
    }
}