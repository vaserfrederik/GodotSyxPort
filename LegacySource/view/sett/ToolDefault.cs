using System;
using System.Collections.Generic;
using static settlement.main.SETT;
using game.faction.FACTIONS;
using init.constant.C;
using init.settings.S;
using init.sprite.SPRITES;
using settlement.entity.ENTITY;
using settlement.environment.SettEnvMap.SettEnv;
using settlement.job.Job;
using settlement.main.SETT;
using settlement.misc.util.RESOURCE_TILE;
using settlement.room.infra.monument.ROOM_MONUMENT;
using settlement.room.main.Room;
using settlement.room.main.RoomInstance;
using settlement.thing.THINGS.Thing;
using settlement.tilemap.GuiTerrainHoverInfo;
using snake2d;
using snake2d.util.color.COLOR;
using snake2d.util.datatypes.BODY_HOLDERE;
using snake2d.util.datatypes.COORDINATE;
using snake2d.util.datatypes.Coo;
using snake2d.util.datatypes.RECTANGLEE;
using snake2d.util.datatypes.Rec;
using snake2d.util.misc.CLAMP;
using snake2d.util.sets.ArrayList;
using snake2d.util.sets.LIST;
using util.gui.misc.GBox;
using util.gui.misc.GText;
using util.info.GFORMAT;
using util.text.D;
using view.keyboard.KEYS;
using view.main.VIEW;
using view.subview.GameWindow;
using view.tool.Tool;
using view.tool.ToolManager;

namespace view.sett
{
    final class ToolDefault : Tool
    {
        private bool dragging = false;
        private readonly Coo dragCoo = new Coo();
        private static readonly CharSequence ¤¤clickJob = "¤Hold '{0}' and click to place more of job: ";
        private static readonly CharSequence ¤¤clickRoom = "¤Hold '{0}' and click to build another: ";
        private static readonly CharSequence ¤¤clickRoom2 = "¤Hold '{0}' and click to copy this: ";
        private static readonly CharSequence ¤¤reconstruct = "¤click to reconstruct rooms";

        static
        {
            D.ts(typeof(ToolDefault));
        }

        private readonly LIST<SettDebugClick> debugs;

        ToolDefault(ToolManager m) : base(m)
        {
            debugs = new ArrayList<SettDebugClick>(SettDebugClick.all);
        }

        protected override void updateHovered(float ds, GameWindow window)
        {
            update(ds, window);
        }

        protected override void update(float ds, GameWindow window)
        {
            dragging &= MButt.RIGHT.isDown();
        }

        private readonly BODY_HOLDERE body = new BODY_HOLDERE
        {
            private readonly Rec body = new Rec(C.TILE_SIZE),

            public RECTANGLEE body()
            {
                return body;
            }
        };

        protected override void renderHovered(SPRITE_RENDERER r, float ds, GameWindow window, GBox box)
        {
            if (!SETT.IN_BOUNDS(window.tile()))
                return;

            if (MButt.RIGHT.isDown())
            {
                if (!dragging)
                {
                    dragging = true;
                    dragCoo.set(window.tile());
                }

                box.add(SPRITES.icons().s.crossheir);
                box.add(box.text().add(VIEW.s().getWindow().tile().x()).add(',').add(VIEW.s().getWindow().tile().y()));
                box.tab(4);

                if (dragging)
                {
                    int dx = dragCoo.x() - window.tile().x();
                    int dy = dragCoo.y() - window.tile().y();
                    int w = Math.Abs(dx);
                    int h = Math.Abs(dy);
                    if (w + h > 0)
                    {
                        dx = CLAMP.i(dx, -1, 1);
                        dy = CLAMP.i(dy, -1, 1);
                        box.add(box.text().add(w + 1).add('x').add(h + 1));

                        for (int d = 1; d <= w; d++)
                        {
                            int x = window.tile().rel().x() + (d * dx) * C.TILE_SIZE;
                            int y = window.tile().rel().y();
                            SPRITES.cons().BIG.dashed_hollow.render(r, 0, x, y);
                        }

                        for (int d = 1; d <= h; d++)
                        {
                            int x = window.tile().rel().x() + (d * dy) * C.TILE_SIZE;
                            int y = window.tile().rel().y();
                            SPRITES.cons().BIG.dashed_hollow.render(r, 0, x, y);
                        }
                    }
                }
            }
            else
            {
                COORDINATE coo = window.pixel();
                if (!PIXEL_BOUNDS.holdsPoint(coo))
                    return;
                ENTITY e = ENTITIES().getArroundPoint(coo.x(), coo.y());
                SETT_HOVERABLE t = THINGS().getArroundCoo(coo.x(), coo.y());

                if (isEntity(coo, e, t) && e.canBeClicked())
                {
                    e.click();
                }
                else if (isThing(coo, e, t) && t.canBeClicked())
                {
                    t.click();
                }
                else if (ROOMS().map.is(window.tile()))
                {
                    Room room = ROOMS().map.get(window.tile());
                    VIEW.s().ui.rooms.click(room, window.tile().x(), window.tile().y());
                }
            }
        }

        private bool isEntity(COORDINATE coo, ENTITY e, SETT_HOVERABLE t)
        {
            if (e == null)
                return false;
            if (t == null)
                return true;
            double edist = COORDINATE.properDistance(coo.x(), coo.y(), e.body().cX(), e.body().cY());
            double tdist = COORDINATE.properDistance(coo.x(), coo.y(), ((Thing)t).body().cX(), ((Thing)t).body().cY());
            return edist <= tdist;
        }

        private bool isThing(COORDINATE coo, ENTITY e, SETT_HOVERABLE t)
        {
            if (t == null)
                return false;
            if (e == null)
                return true;
            double edist = COORDINATE.properDistance(coo.x(), coo.y(), e.body().cX(), e.body().cY());
            double tdist = COORDINATE.properDistance(coo.x(), coo.y(), ((Thing)t).body().cX(), ((Thing)t).body().cY());
            return edist > tdist;
        }

        protected override bool rightClick()
        {
            return false;
        }

        protected override void click(GameWindow window)
        {
            if (MButt.RIGHT.isDown())
            {
                return;
            }
            else
            {
                int px = VIEW.s().getWindow().pixel().x();
                int py = VIEW.s().getWindow().pixel().y();
                int tx = VIEW.s().getWindow().tile().x();
                int ty = VIEW.s().getWindow().tile().y();
                foreach (SettDebugClick c in debugs)
                {
                    if (c.debug(px, py, tx, ty))
                        return;
                }

                if (KEYS.MAIN().MOD.isPressed())
                {
                    Room room = ROOMS().map.get(window.tile());
                    if (room != null && room.constructor() != null && room.blueprint() != SETT.ROOMS().THRONE && room.constructor().blue().reqs.passes(FACTIONS.player()))
                    {
                        if (room.constructor().blue().cat == SETT.ROOMS().CATS.DECOR)
                        {
                            VIEW.s().ui.placer.init(room.constructor().blue(), window.tile().x(), window.tile().y());
                        }
                        else
                        {
                            SETT.ROOMS().placement.placer.structure.set(window.tile().x(), window.tile().y());
                            VIEW.s().ui.placer.init(room.constructor().blue(), VIEW.s().getWindow().tile().x(), VIEW.s().getWindow().tile().y());
                        }
                        return;
                    }

                    Job j = SETT.JOBS().jobGetter.get(window.tile());
                    if (j != null && j.placer() != null)
                    {
                        VIEW.s().tools.place(j.placer(), j.config());
                        return;
                    }
                }
                if (KEYS.MAIN().UNDO.isPressed())
                {
                    Room room = ROOMS().map.get(VIEW.s().getWindow().tile());
                    if (room != null && room.constructor() != null && room.blueprint() != SETT.ROOMS().THRONE)
                    {
                        SETT.ROOMS().copy.copy(VIEW.s().getWindow().tile().x(), VIEW.s().getWindow().tile().y());
                        return;
                    }
                }

                COORDINATE coo = window.pixel();
                if (!PIXEL_BOUNDS.holdsPoint(coo))
                    return;
                ENTITY e = ENTITIES().getArroundPoint(coo.x(), coo.y());
                SETT_HOVERABLE t = THINGS().getArroundCoo(coo.x(), coo.y());

                if (isEntity(coo, e, t) && e.canBeClicked())
                {
                    e.click();
                }
                else if (isThing(coo, e, t) && t.canBeClicked())
                {
                    t.click();
                }
                else if (ROOMS().map.is(window.tile()))
                {
                    Room room = ROOMS().map.get(window.tile());
                    VIEW.s().ui.rooms.click(room, window.tile().x(), window.tile().y());
                }
            }
        }
    }
}