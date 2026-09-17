using System;
using System.Collections.Generic;
using init.constant;
using init.race;
using init.sprite;
using init.type;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using util;
using util.colors;
using util.gui.misc;
using util.rendering;
using util.text;

namespace view.sett.ui.room.priority
{
    final class UIRoomRaceAssign : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "Assign Work Groups";
        private static readonly CharSequence ¤¤desc = "Lets you assign Work Groups (species + class) to individual rooms. If the set priorities allows for it, the rooms will be employed according to these settings.";
        private static readonly CharSequence ¤¤prob = "Must be placed on a workplaces.";
        private static readonly CharSequence ¤¤prob2 = "Some or all of the selected groups are not employed in the current room type, and the setting will have no effect.";
        private static readonly CharSequence ¤¤everyone = "Set priority for everyone";
        private static readonly CharSequence ¤¤none = "Set priority for none";
        private static readonly CharSequence ¤¤permission = "Set priority for:";
        private static readonly CharSequence ¤¤permissionAll = "Set priority for All:";

        static
        {
            D.ts(typeof(UIRoomRaceAssign));
        }

        private readonly HTypeBitsImp data = new HTypeBitsImp(false);

        private readonly LIST<CLICKABLE> butts;

        public UIRoomRaceAssign() : base(¤¤name, ¤¤desc, SPRITES.icons().m.citizen)
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
                        data.set(WGROUP.get(HTYPES.SUBJECT(), r));
                }
            }.hoverInfoSet(¤¤permissionAll + " " + HCLASSES.CITIZEN().names));
            sec.addRightC(0, new GButt.ButtPanel(HCLASSES.SLAVE().icon())
            {
                protected override void clickA()
                {
                    data.clear();
                    foreach (Race r in RACES.all())
                        data.set(WGROUP.get(HTYPES.SLAVE(), r));
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
            foreach (WGROUP t in WGROUP.all())
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
            RoomInstance ins = SETT.ROOMS().map.instance.get(tx, ty);
            if (ins != null && ins.blueprintI().employment() != null)
                return null;
            return ¤¤prob;
        }

        public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            RoomInstance ins = SETT.ROOMS().map.instance.get(tx, ty);
            if (ins != null && ins.blueprintI().employment() != null)
            {
                ins.employees().prefferedSet(data);
            }
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            RoomInstance ins = SETT.ROOMS().map.instance.get(fromX, fromY);
            return ins != null && ins.is(toX, toY);
        }

        public override void placeInfo(GBox b, int oktiles, AREA a)
        {
            base.placeInfo(b, oktiles, a);
            b.NL();

            foreach (COORDINATE c in a.body())
            {
                if (a.is(c))
                {
                    RoomInstance ins = SETT.ROOMS().map.instance.get(c.x(), c.y());
                    if (ins == null || !c.isSameAs(ins.mX(), ins.mY()) || ins.blueprintI().employmentExtra() == null)
                        continue;

                    foreach (WGROUP g in WGROUP.all())
                    {
                        if (data.is(g) && ins.blueprintI().employment().employed(g) <= 0)
                        {
                            GText t = b.text();
                            t.warnify().add(¤¤prob2);
                            b.add(t);
                            return;
                        }
                    }
                }
            }
        }

        public override LIST<CLICKABLE> getClickables()
        {
            return butts;
        }

        private readonly ON<RENDER> ren = new ON<RENDER>()
        {
            Run = () =>
            {
                render();
            }
        };

        private void render()
        {
            ren.run();
        }

        private void renderSingle(Renderer r, int cx, int cy, SPRITE icon)
        {
            int w = icon.width() * C.SCALE;
            int h = icon.height() * C.SCALE;
            int x1 = cx - w / 2;
            int y1 = cy - h / 2;
            icon.render(r, x1, x1 + w, y1, y1 + h);
        }

        private void renderSingle(Renderer r, int cx, int cy, RoomInstance house, WGROUP g)
        {
            SPRITE icon = g.icon;
            int w = icon.width() * C.SCALE;
            int h = icon.height() * C.SCALE;
            int x1 = cx - w / 2;
            int y1 = cy - h / 2;
            icon.render(r, x1, x1 + w, y1, y1 + h);
            if (house.blueprintI().employment().employed(g) <= 0)
            {
                GCOLOR.UI().BAD.hovered.bind();
                int w2 = 16 * C.SCALE;
                UI.icons().s.alert.render(r, x1, x1 + w2, y1 - w2 / 2, y1 + w2 / 2);
                COLOR.unbind();
            }
        }

        private void renderMany(Renderer r, int cx, int cy, RoomInstance house, bool anti)
        {
            int width = house.body().width() * C.TILE_SIZE;
            int w = 24 * C.SCALE / 2;
            int h = rens.get(0).icon.height() * C.SCALE / 2;

            int dx = (width / rens.size());
            dx = CLAMP.i(dx, 1, w);

            int x1 = cx - dx * rens.size() / 2;
            int y1 = cy - h;

            foreach (WGROUP t in rens)
            {
                t.icon.render(r, x1, x1 + w, y1, y1 + h);

                if (anti)
                    UI.icons().m.anti.render(r, x1, x1 + w, y1, y1 + h);
                if (house.blueprintI().employment().employed(t) <= 0)
                {
                    GCOLOR.UI().BAD.hovered.bind();
                    int w2 = 16 * C.SCALE / 2;
                    UI.icons().s.alert.render(r, x1, x1 + w2, y1 - w2 / 2, y1 + w2 / 2);
                    COLOR.unbind();
                }

                x1 += dx;
            }
        }
    }
}