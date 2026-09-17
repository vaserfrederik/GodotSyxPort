using System;
using System.Collections.Generic;
using init.race;
using init.type;
using settlement.main;
using settlement.room.infra.gate;
using settlement.room.infra.monument;
using settlement.room.main;
using snake2d;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;

namespace view.sett.ui.room
{
    public class UIRooms : ISidePanel
    {
        private readonly UIRoom[] rooms;
        private UIPanelMain main;

        private readonly GuiSection pop2 = new GuiSection();
        private static readonly CharSequence ¤¤reconstructPrompt = "¤Do you wish to refurnish this room? Some progress of the current construction will be lost.";

        static UIRooms()
        {
            D.ts(UIRooms);
        }

        public UIRooms()
        {
            Init init = new Init();
            Modules mm = new Modules(init);

            D.t(UIRoom);

            foreach (RoomBlueprint p in ROOMS().all())
            {
                rooms[p.index()] = new UIRoom(p, mm.get(p));
            }
            main = new UIPanelMain(rooms);

            SETT.addGeneratorHook(new ACTION
            {
                exe = () => main = new UIPanelMain(rooms)
            });

            for (int i = 0; i < RACES.all().size(); i++)
            {
                Race r = RACES.all()[i];

                CLICKABLE c = new GButt.ButtPanel(r.appearance().iconBig.big)
                {
                    clickA = () =>
                    {
                        VIEW.inters().popup.close();
                        main.work.set(r, HTYPES.SLAVE());
                        VIEW.s().panels.add(main.work, true);
                    }
                };
                c.hoverInfoSet(r.info.names);
                c.body().moveX1Y1((i % 5) * c.body().width(), (i / 5) * c.body().height());
                pop2.add(c);
            }
        }

        public ISidePanel main()
        {
            return main;
        }

        public void hover(GBox box, Room r, int rx, int ry)
        {
            rooms[r.blueprint().index()].hover(box, r, rx, ry);
        }

        public bool problem(Room r, int rx, int ry)
        {
            return rooms[r.blueprint().index()].problem(r, rx, ry);
        }

        public bool warning(Room r, int rx, int ry)
        {
            return rooms[r.blueprint().index()].warning(r, rx, ry);
        }

        public void open(RoomEquip w)
        {
            main.open(w);
        }

        private readonly Coo rRoom = new Coo();

        private readonly ACTION recon = new ACTION
        {
            exe = () => VIEW.s().ui.placer.reconstruct(rRoom.x(), rRoom.y())
        };

        public void click(Room room, int tx, int ty)
        {
            if (room.blueprint() is ROOM_GATE)
            {
                ((ROOM_GATE)room.blueprint()).lock(tx, ty, !((ROOM_GATE)room.blueprint()).locked(tx, ty));
            }
            else if (room.blueprint() == ROOMS().HOME)
            {
                VIEW.s().panels.add(VIEW.s().ui.home, true);
            }
            else if (room.blueprint() == ROOMS().THRONE)
            {
                VIEW.UI().level.activate();
            }
            else if (room is RoomInstance)
                open((RoomInstance)room);
            else if (SETT.ROOMS().placement.canReconstruct(tx, ty))
            {
                rRoom.set(tx, ty);
                VIEW.inters().yesNo.activate(¤¤reconstructPrompt, recon, ACTION.NOP, true);
            }
            else if (rooms[room.blueprint().index()].table == null)
                inter.show(room, tx, ty);
            else if (room is RoomInstance)
                open((RoomInstance)room);
        }

        public void open(RoomInstance r)
        {
            if (rooms[r.blueprint().index()].table == null)
                return;

            VIEW.s().panels.add(rooms[r.blueprint().index()].table.get(), true);
            VIEW.s().panels.add(rooms[r.blueprint().index()].detail(r), false);
        }

        public ISidePanel open(RoomBlueprint r)
        {
            if (rooms[r.index()].table == null)
                return null;
            return rooms[r.index()].table.get();
        }

        public bool openIs(RoomBlueprint r)
        {
            if (rooms[r.index()].table == null)
                return false;
            return VIEW.s().panels.added(rooms[r.index()].table) && VIEW.s().panels.added(rooms[r.index()].detail);
        }

        public void prio(HCLASS c, Race r, CLICKABLE trigger)
        {
            if (c == HCLASSES.SLAVE())
            {
                VIEW.inters().popup.show(pop2, trigger);
            }
            else if (c == HCLASSES.CITIZEN() && r != null)
            {
                main.work.set(r, HTYPES.SUBJECT());
                VIEW.s().panels.add(main.work, true);
            }
            else
            {
                VIEW.s().panels.add(main.work, true);
            }
        }

        private readonly Inter inter = new Inter();
        private class Inter : Interrupter
        {
            public Room room;
            public int tx;
            public int ty;

            protected override bool update(float ds)
            {
                return true;
            }

            protected override bool render(Renderer r, float ds)
            {
                if (room.blueprint() is ROOM_MONUMENT)
                {
                    SETT.OVERLAY().monument((ROOM_MONUMENT)room.blueprint());
                }
                else if (room.blueprint() is RoomFinderHaser)
                {
                    SETT.OVERLAY().service((RoomFinderHaser)room.blueprint());
                }
                else
                {
                    foreach (SettEnv e in SETT.ENV().map.all())
                    {
                        if (room.constructor().envValue(e))
                        {
                            SETT.OVERLAY().envThing(e).add();
                            break;
                        }
                    }
                }
                SETT.OVERLAY().add(tx, ty);

                return true;
            }

            public void show(Room room, int tx, int ty)
            {
                if (!has(room))
                    return;
                this.room = room;
                this.tx = tx;
                this.ty = ty;

                show(VIEW.s().uiManager);
            }

            private bool has(Room room)
            {
                if (room.blueprint() is ROOM_MONUMENT)
                {
                    return true;
                }
                if (room.blueprint() is RoomFinderHaser)
                {
                    return true;
                }
                else
                {
                    foreach (SettEnv e in SETT.ENV().map.all())
                    {
                        if (room.constructor().envValue(e))
                            return true;
                    }
                }
                return false;
            }

            protected override void mouseClick(MButt button)
            {
                if (button == MButt.RIGHT)
                    hide();
            }

            protected override bool otherClick(MButt button)
            {
                if (button != MButt.WHEEL_SPIN)
                    hide();
                return false;
            }

            protected override void hide()
            {
                base.hide();
            }

            protected override void hoverTimer(GBox text)
            {
                // TODO Auto-generated method stub
            }

            protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                return false;
            }
        }
    }
}