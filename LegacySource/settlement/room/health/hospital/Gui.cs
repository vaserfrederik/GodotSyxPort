using System;
using System.Collections.Generic;
using game.faction;
using init.resources;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.text;
using view.sett.ui.room;

namespace settlement.room.health.hospital
{
    class Gui : UIRoomModuleImp<HospitalInstance, ROOM_HOSPITAL>
    {
        private static readonly string ¤¤nn = "Fetch:";
        private static readonly string ¤¤hov = "Allowing this resource use increases recovery rate by 75%";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_HOSPITAL s) : base(s)
        {
        }

        protected override void AppendPanel(GuiSection section, GGrid grid, GETTER<HospitalInstance> getter, int x1, int y1)
        {
            GuiSection s = new GuiSection();

            for (int i = 0; i < blueprint.consumtion.ins().Size(); i++)
            {
                RESOURCE res = blueprint.consumtion.ins().Get(i).resource;
                int k = i;
                GButt.ButtPanel b = new GButt.ButtPanel(res.icon())
                {
                    RenAction = () =>
                    {
                        selectedSet(getter.Get().fetch[k]);
                        activeSet(blueprint.resLocks.Get(k).Passes(FACTIONS.player()));
                    },

                    ClickA = () =>
                    {
                        getter.Get().fetch[k] = !getter.Get().fetch[k];
                    },

                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        base.HoverInfoGet(text);
                        text.NL();
                        blueprint.resLocks.Get(k).Hover(text, FACTIONS.player());
                    }
                };

                b.hoverTitleSet("" + ¤¤nn + " " + res.names);
                b.hoverInfoSet(¤¤hov);
                b.pad(16, 4);
                s.addRightC(0, b);
            }

            section.addRelBody(8, DIR.S, s);
        }

        protected override void Problem(HospitalInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            base.Problem(i, free, errors, warnings);
        }
    }
}