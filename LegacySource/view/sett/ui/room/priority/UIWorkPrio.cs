using System;
using System.Collections.Generic;
using Init.Race;
using Init.Sprite;
using Init.Type;
using Settlement.Main;
using Settlement.Room.Main;
using Settlement.Room.Main.Category;
using Settlement.Room.Main.Employment;
using Settlement.Stats;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Sets;
using Util.Gui.Misc;
using Util.Text;

namespace View.Sett.Ui.Room.Priority
{
    public class UIWorkPrio : ISidePanel
    {
        static readonly CharSequence ¤¤title = "Work Priorities";
        static readonly CharSequence ¤¤Adjust = "¤Adjust all by 1.";

        static readonly CharSequence ¤¤FilterW = "¤Filter Workplaces";
        static readonly CharSequence ¤¤FilterG = "¤Filter Work Groups";

        static readonly CharSequence ¤¤setWork = "Set all priorities based on work skill.";
        static readonly CharSequence ¤¤setFull = "Set all priorities based on fulfillment.";

        static UIWorkPrio()
        {
            D.ts(typeof(UIWorkPrio));
        }

        private readonly Filter<WGROUP> filterGroup;

        public UIWorkPrio()
        {
            D.t(this);
            titleSet(¤¤title);

            var rooms = new ArrayListGrower<FilterEntry<RoomEmployment>>();
            Filter<RoomEmployment> fRoom;

            {
                var rr = new ArrayListGrower<FilterEntry<WGROUP>>();

                foreach (WGROUP g in WGROUP.all())
                {
                    var ee = new FilterEntry<WGROUP>(g.name, g.icon, g)
                    {
                        isRelavant = () => STATS.POP().POP.type().get(HTYPE_RACE.get(g.race, g.type)) > 0
                    };
                    rr.add(ee);
                }

                var cc = new ArrayListGrower<FilterCombined<WGROUP>>();
                filterGroup = new Filter<WGROUP>(UI.icons().s.human, ¤¤FilterG, rr, cc);
                section.addRightC(0, filterGroup);
            }

            {
                var cc = new ArrayListGrower<FilterCombined<RoomEmployment>>();

                foreach (RoomCategoryMain catMain in SETT.ROOMS().CATS.MAINS)
                {
                    var c = new FilterCombined<RoomEmployment>(catMain.name, catMain.icon);
                    cc.add(c);
                    foreach (RoomBlueprint blue in catMain.all())
                    {
                        if (blue.employment() is RoomEmployment)
                        {
                            RoomEmployment e = (RoomEmployment)blue.employment();
                            var ee = new FilterEntry<RoomEmployment>(e.blueprint().info.names, e.blueprint().iconBig(), e)
                            {
                                isRelavant = () => e.neededWorkers() > 0
                            };
                            rooms.add(ee);
                            c.all.add(ee);
                        }
                    }
                }
                fRoom = new Filter<RoomEmployment>(UI.icons().s.hammer, ¤¤FilterW, rooms, cc);
                section.addRightC(0, fRoom);
            }

            {
                var butts = new GuiSection();

                butts.addRelBody(24, DIR.E, new GButt.ButtPanel(SPRITES.icons().m.repair)
                {
                    clickA = () =>
                    {
                        foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                        {
                            if (fRoom.active(e))
                            {
                                foreach (FilterEntry<WGROUP> g in filterGroup.all)
                                {
                                    if (filterGroup.active(g))
                                    {
                                        e.o.setPrioOnSkill(g.o);
                                    }
                                }
                            }
                        }
                    }
                }.pad(4, 4).hoverInfoSet(¤¤setWork));

                butts.addRightC(2, new GButt.ButtPanel(SPRITES.icons().m.heart)
                {
                    clickA = () =>
                    {
                        foreach (FilterEntry<RoomEmployment> e in fRoom.all)
                        {
                            if (fRoom.active(e))
                            {
                                foreach (FilterEntry<WGROUP> g in filterGroup.all)
                                {
                                    if (filterGroup.active(g))
                                    {
                                        e.o.setPrioOnFullfillment(g.o);
                                    }
                                }
                            }
                        }
                    }
                }.pad(4, 4).hoverInfoSet(¤¤setFull));

                section.addRightC(32, butts);
            }

            UIRoomRaceAssign a = new UIRoomRaceAssign();

            GButt.ButtPanel butt = new GButt.ButtPanel(UI.icons().m.descrimination)
            {
                clickA = () => VIEW.s().tools.place(a)
            };
            butt.hoverTitleSet(a.name());
            butt.hoverInfoSet(a.desc);
            butt.pad(4, 4);

            section.addRelBody(16, DIR.W, butt);

            section.addRelBody(8, DIR.S, new Table(fRoom, filterGroup, HEIGHT - section.body().height() - 8));
        }

        public void set(Race r, HTYPE slave)
        {
            filterGroup.all.get(WGROUP.get(slave, r).index);
        }
    }
}