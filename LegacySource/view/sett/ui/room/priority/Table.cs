using System;
using System.Collections.Generic;
using util.gui.misc;
using util.gui.table;
using util.text;

namespace view.sett.ui.room.priority
{
    public sealed class Table : GuiSection
    {
        private static readonly string ¤¤MasterPrioInc = "¤Increase Master priority of all filtered workplaces by 1";
        private static readonly string ¤¤MasterPrioDec = "¤Decrease Master priority of all filtered workplaces by 1";
        private static readonly string ¤¤ColInc = "¤Increase priority of all filtered work groups in the column.";
        private static readonly string ¤¤ColDec = "Decrease priority of all filtered work groups in the column.";

        static Table()
        {
            D.ts(typeof(Table));
        }

        public Table(Filter<RoomEmployment> fRoom, Filter<WGROUP> filterGroup, int HEIGHT)
        {
            GuiSection butts = new GuiSection();
            butts.Add(new GButt.ButtPanel(UI.icons().s.minifier)
            {
                protected override void ClickA()
                {
                    foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                    {
                        if (fRoom.active(e))
                            e.o.priority.inc(-1);
                    }
                }
            }.hoverInfoSet(¤¤MasterPrioDec));

            butts.AddRightC(0, new GButt.ButtPanel(UI.icons().s.magnifier)
            {
                protected override void ClickA()
                {
                    foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                    {
                        if (fRoom.active(e))
                            e.o.priority.inc(1);
                    }
                }
            }.hoverInfoSet(¤¤MasterPrioInc));

            int x1 = butts.body().x2();

            for (int i = 0; i <= fRoom.all[0].o.priorities.max(null); i++)
            {
                int prio = fRoom.all[0].o.priorities.max(null) - i;
                if (prio != fRoom.all[0].o.priorities.max(null))
                {
                    CLICKABLE c = new GButt.ButtPanel(UI.icons().s.arrow_left)
                    {
                        protected override void ClickA()
                        {
                            foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                            {
                                if (fRoom.active(e))
                                {
                                    foreach (FilterEntry<WGROUP> g in filterGroup.all)
                                    {
                                        if (filterGroup.active(g) && e.o.priorities.get(g.o) == prio)
                                            e.o.priorities.inc(g.o, 1);
                                    }
                                }
                            }
                        }
                    }.hoverInfoSet(¤¤ColInc);

                    c.body().moveX2(x1 + Row.EW / 2 + i * Row.EW);
                    butts.Add(c);
                }

                if (prio != 0)
                {
                    CLICKABLE c = new GButt.ButtPanel(UI.icons().s.arrow_right)
                    {
                        protected override void ClickA()
                        {
                            foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                            {
                                if (fRoom.active(e))
                                {
                                    foreach (FilterEntry<WGROUP> g in filterGroup.all)
                                    {
                                        if (filterGroup.active(g) && e.o.priorities.get(g.o) == prio)
                                            e.o.priorities.inc(g.o, -1);
                                    }
                                }
                            }
                        }
                    }.hoverInfoSet(¤¤ColDec);

                    c.body().moveX1(x1 + Row.EW / 2 + i * Row.EW);
                    butts.Add(c);
                }
            }

            Add(butts);

            List<RENDEROBJ> rows = new List<RENDEROBJ>();

            foreach (FilterEntry<RoomEmployment> e in fRoom.all)
            {
                rows.Add(new Row(filterGroup, (RoomEmployment)e.o));
            }

            int hh = HEIGHT - body().height() - 8;
            int s = hh / rows[0].body().height();
            hh = s * rows[0].body().height();

            GScrollRows rr = new GScrollRows(rows, hh)
            {
                protected override bool passesFilter(int i, RENDEROBJ o)
                {
                    return fRoom.active(fRoom.all[i]);
                }
            };

            AddDown(8, rr.view());
        }
    }
}