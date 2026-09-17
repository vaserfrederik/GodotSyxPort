using System;
using System.Collections.Generic;
using init.race;
using settlement.room.law;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    final class ModulePunish : ModuleMaker
    {
        private static string ¤¤Punishments = "¤Punishments";
        private static string ¤¤PunishmentsDesc = "¤The current and total amount of punishments in use.";
        private static string ¤¤Set = "¤Toggle usage for {0} prisoners.";

        static
        {
            D.ts(typeof(ModulePunish));
        }

        public ModulePunish(Init init)
        {
        }

        public void make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is PUNISHMENT_SERVICE)
            {
                l.add(new I((PUNISHMENT_SERVICE)p));
            }
        }

        private class I : UIRoomModule
        {
            private readonly PUNISHMENT_SERVICE pun;

            public I(PUNISHMENT_SERVICE blue)
            {
                this.pun = blue;
            }

            public void appendManageScr(GGrid grid, GGrid text, GuiSection sExta)
            {
                GuiSection s = new GuiSection();

                string name = pun.punishEnabled() == null ? ¤¤Punishments : pun.punishEnabled().info().name;
                string desc = pun.punishEnabled() == null ? ¤¤PunishmentsDesc : pun.punishEnabled().info().desc;

                if (pun.punishEnabled() != null)
                {
                    foreach (Race r in RACES.all())
                    {
                        RENDEROBJ c = new GButt.ButtPanel(r.appearance().icon)
                        {
                            protected override void clickA()
                            {
                                pun.punishEnabled().toggle(r);
                            }

                            protected override void renAction()
                            {
                                selectedSet(pun.punishEnabled().is(r));
                            }

                            public override void hoverInfoGet(GUI_BOX text)
                            {
                                text.text(Str.TMP.clear().add(¤¤Set).insert(0, r.info.namePosessive));
                            }
                        };
                        s.addGrid(c, r.index(), 8, 0, 0);
                    }
                }

                RENDEROBJ h = new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.iofkNoColor(text, pun.punishUsed(), pun.punishTotal());
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.title(name);
                        b.text(desc);
                    }
                }.hh(name);
                s.addRelBody(4, DIR.N, h);

                text.add(s);
            }

            public void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
            {
            }

            public void appendButt(GuiSection s, GETTER<RoomInstance> get)
            {
            }

            public void hover(GBox box, Room room, int rx, int ry)
            {
            }

            public void problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry)
            {
            }

            public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
            }
        }
    }
}