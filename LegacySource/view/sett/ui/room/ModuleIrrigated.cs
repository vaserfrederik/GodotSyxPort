using System;
using System.Collections.Generic;
using System.Linq;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    public class ModuleIrrigated : ModuleMaker
    {
        private readonly string ¤¤Desc = "¤This room depends on the moisture of its ground tiles. All tiles have a default moisture percentage, which can be boosted by sweet water access emanating from natural bodies of water and canals. This room needs an average moisture of {0}% to get a full boost. Current average is {1}%, which will result in a {2}% target value. The current value of {3}% will take some time to move to the target.";

        public ModuleIrrigated(Init init)
        {
            D.t(this);
        }

        public override void Make(RoomBlueprint p, List<UIRoomModule> l)
        {
            if (p is ROOM_IRRIGATED)
            {
                l.Add(new I((ROOM_IRRIGATED)p));
            }
        }

        private class I : UIRoomModule
        {
            private readonly ROOM_IRRIGATED p;

            public I(ROOM_IRRIGATED b)
            {
                this.p = b;
            }

            public override void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                section.AddRelBody(8, DIR.S, new GStat()
                {
                    Update = (GText text) =>
                    {
                        GFORMAT.perc(text, CLAMP.d(p.irrigation().prospectFlat(get.Get()), 0, 1));
                    },
                    HoverInfoGet = (GBox b) =>
                    {
                        b.Title(Ground.¤¤moisture);
                        GText t = b.Text();

                        RoomInstance i = get.Get();

                        double target = p.irrigation().needed(i) / i.area();
                        double current = RoomIrrigated.rawValue(i);
                        double value = p.irrigation().prospectFlat(i);

                        t.Add(¤¤Desc);
                        t.Insert(0, (int)(Math.Round(100 * target)));
                        t.Insert(1, (int)(Math.Round(100 * current)));
                        t.Insert(2, (int)(Math.Round(100 * value)));
                        t.Insert(3, (int)(Math.Round(100 * p.irrigation().current(i))));
                        b.Add(t);
                        SETT.OVERLAY().MOISTURE.Add();
                    }
                }.Hh(SETT.ENV().map.WATER_SWEET.icon));
            }

            public override void Hover(GBox box, Room room, int rx, int ry)
            {
                box.Add(SETT.ENV().map.WATER_SWEET.icon);
                box.TextL(Ground.¤¤moisture);
                box.Tab(6);
                if (room is RoomInstance)
                {
                    box.Add(GFORMAT.perc(box.Text(), CLAMP.d(p.irrigation().prospectFlat((RoomInstance)room), 0, 1)));
                }
                box.NL();
            }
        }
    }
}