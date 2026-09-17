using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace view.sett.ui.room
{
    final class ModulePumpable : ModuleMaker
    {
        private readonly string ¤¤Name = "¤Water Supply";
        private readonly string ¤¤Desc = "¤Water Supply is gained from adjacent canals and drains that are connected to pumps.";
        private readonly string ¤¤Preasure = "¤Pressure required per tile:";
        private readonly SPRITE icon = UI.icons().s.drop.createColored(COLOR.BLUEISH);

        public ModulePumpable(Init init)
        {
            D.t(this);
        }

        public override void make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is ROOM_PUMPABLE)
            {
                l.add(new I((ROOM_PUMPABLE)p));
            }
        }

        private class I : UIRoomModule
        {
            private readonly ROOM_PUMPABLE p;

            I(ROOM_PUMPABLE b)
            {
                this.p = b;
            }

            public override void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                section.addRelBody(8, DIR.S, new GStat()
                {
                    public override void update(GText text)
                    {
                        int x = get.get().mX();
                        int y = get.get().mY();
                        GFORMAT.perc(text, p.pumpable(x, y).irrigation(x, y));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.title(¤¤Name);
                        b.text(¤¤Desc);

                        int x = get.get().mX();
                        int y = get.get().mY();
                        double d = p.pumpable(x, y).suckAmount(x, y);
                        b.NL();
                        b.textLL(Dic.¤¤Current);
                        b.tab(6);
                        b.add(GFORMAT.iofkInv(b.text(), (long)Math.Ceiling(d * p.pumpable(x, y).irrigation(x, y) * get.get().area()), (long)Math.Ceiling(d * get.get().area())));
                        b.NL();
                        b.textLL(¤¤Preasure);
                        b.NL();
                        b.add(GFORMAT.f(b.text(), d));
                    }
                }.hh(UI.icons().s.drop.createColored(COLOR.BLUEISH)));
            }

            public override void hover(GBox box, Room room, int rx, int ry)
            {
                box.add(icon);
                box.textL(¤¤Name);
                box.tab(6);
                box.add(GFORMAT.perc(box.text(), p.pumpable(rx, ry).irrigation(rx, ry)));
                box.NL();
            }
        }
    }
}