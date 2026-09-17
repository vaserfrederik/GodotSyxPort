using System;
using System.Collections.Generic;
using settlement.main;
using snake2d;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.water
{
    internal class PumpGui : UIRoomModuleImp<PumpInstance, ROOM_PUMP>
    {
        private static readonly CharSequence ¤¤Preasure = "Pressure (pump)";
        private static readonly CharSequence ¤¤PreasureS = "Pressure (connected system)";
        private static readonly CharSequence ¤¤PreasureD = "Pressure is created through pumps. The workers in pumps, and their degrade determine the current pressure.";

        static PumpGui()
        {
            D.ts(typeof(PumpGui));
        }

        public PumpGui(ROOM_PUMP blueprint) : base(blueprint)
        {
            // TODO Auto-generated constructor stub
        }

        protected override void hover(GBox box, PumpInstance ins)
        {
            box.NL();
            box.textLL(¤¤Preasure);
            box.add(GFORMAT.iofkInv(box.text(), ins.value, (int)ins.valueMax));
        }

        protected override void appendPanel(GuiSection section, GGrid g, GETTER<PumpInstance> getter, int x1, int y1)
        {
            section.addRelBody(8, DIR.S, new GStat
            {
                public void update(GText text)
                {
                    PumpInstance ins = getter.get();
                    GFORMAT.iofkInv(text, ins.value, (int)ins.valueMax);
                }
            }.hv(¤¤Preasure, ¤¤PreasureD));

            section.addRelBody(8, DIR.S, new GStat
            {
                public void update(GText text)
                {
                    PumpInstance ins = getter.get();
                    int tx = ins.ox();
                    int ty = ins.oy();
                    hoverSystem(text, tx, ty);
                }
            }.hv(¤¤PreasureS, ¤¤PreasureD));
        }

        public static void hoverSystem(GBox box, int tx, int ty)
        {
            GText t = box.text();
            hoverSystem(t, tx, ty);

            box.textLL(¤¤PreasureS);
            box.NL();
            box.add(t);
        }

        private static void hoverSystem(GText text, int tx, int ty)
        {
            int current = 0;
            double needed = 0;

            Flooder f = GUTIL.flooder();
            f.init(typeof(PumpGui));
            f.pushSloppy(tx, ty, 0);
            ROOM_WATER w = SETT.ROOMS().WATER;
            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                RoomPumpable p = w.pumpable.get(t.x(), t.y());
                if (p != null)
                {
                    needed += p.suckAmount(t.x(), t.y());

                    if (p.radius() > 0)
                    {
                        int rr = p.radius();
                        int i = 0;
                        while (GUTIL.circle().radius(i) < rr)
                        {
                            int dx = GUTIL.circle().get(i).x() + t.x();
                            int dy = GUTIL.circle().get(i).y() + t.y();
                            if (w.pumpable.get(dx, dy) == p)
                            {
                                f.pushSmaller(dx, dy, t.getValue() + GUTIL.circle().radius(i), t);
                            }
                            i++;
                        }
                        needed += p.radius();
                    }
                }
                else
                {
                    PumpInstance ins = w.pump.get(t.x(), t.y());
                    if (ins != null && ins.ox() == t.x() && ins.oy() == t.y())
                    {
                        current += ins.value;
                    }
                    else
                    {
                        continue;
                    }
                }

                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    if (!SETT.IN_BOUNDS(t, d))
                    {
                        continue;
                    }
                    if (p != null && w.pumpable.get(t, d) != null && !p.pumpsTo(t.x(), t.y(), t.x() + d.x(), t.y() + d.y()))
                        continue;

                    f.pushSmaller(t, d, t.getValue() + 1, t);
                }
            }
            f.done();

            GFORMAT.iofkInv(text, current, (int)needed);
        }
    }
}