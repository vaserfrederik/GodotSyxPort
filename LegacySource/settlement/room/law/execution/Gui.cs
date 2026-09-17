using System;
using System.Collections.Generic;
using settlement.room.law.execution;
using init.settings;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace Settlement.Room.Law.Execution
{
    class Gui : UIRoomModule
    {
        private static readonly string ¤¤available = "Available Executions";
        private static readonly string ¤¤spectators = "Spectators";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        private readonly ROOM_EXECTUTION b;

        public Gui(ROOM_EXECTUTION s)
        {
            this.b = s;
        }

        private readonly int[] states = Alloc.ii(ExecutionStation.STATE_DEAD + 1);
        private readonly Rec body = new Rec();

        public override void hover(GBox box, Room room, int rx, int ry)
        {
            box.NL();

            body.moveX1Y1(room.x1(rx, ry), room.y1(rx, ry));
            body.setDim(room.width(rx, ry), room.height(rx, ry));

            int available = 0;
            int total = 0;
            Array.Fill(states, 0);
            int specs = 0;
            int specsTot = 0;
            foreach (COORDINATE c in body)
            {
                if (room.isSame(rx, ry, c.x(), c.y()))
                {
                    Client cl = b.stations.client(c.x(), c.y());
                    if (cl != null)
                    {
                        total++;
                        if (!cl.clientReserved())
                            available++;
                        if (b.stations.service(c.x(), c.y()) != null)
                        {
                            specsTot += ExecutionStation.services;
                            specs += b.stations.sevices(c.x(), c.y());
                        }
                        states[b.stations.state(c.x(), c.y())]++;
                    }
                }
            }

            box.textLL(¤¤spectators);
            box.tab(7);
            box.add(GFORMAT.iofk(box.text(), specs, specsTot));
            box.NL();

            box.textLL(¤¤available);
            box.tab(7);
            box.add(GFORMAT.iofk(box.text(), available, total));
            box.NL();

            box.textLL(Dic.¤¤Total);
            box.tab(7);
            box.add(GFORMAT.iofk(box.text(), b.stations.available(), b.stations.total()));
            box.NL();

            if (S.get().developer)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    box.add(box.text().add(i).add(':'));
                    box.add(box.text().add(states[i]));
                    box.NL();
                }
            }
        }
    }
}