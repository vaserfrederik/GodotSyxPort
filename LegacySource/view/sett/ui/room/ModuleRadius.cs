using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.data.GETTER;
using util.data.INT;
using util.gui.misc;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    final class ModuleRadius : ModuleMaker
    {
        private readonly CharSequence ¤¤NAME = "¤Radius";
        private readonly CharSequence ¤¤PROBLEM = "¤No work is within the radius!";
        private readonly CharSequence ¤¤DESC = "¤Set the work radius of this room. Subjects will look for work within the radius. A big radius can be ineffective and workers will have a hard time getting back to services when work is done.";

        private RoomInstance i;

        public ModuleRadius(Init init)
        {
            D.t(this);
        }

        public void make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is ROOM_RADIUS)
            {
                l.add(new I(p));
            }
        }

        private class I : UIRoomModule
        {
            private readonly RoomBlueprint p;

            public I(RoomBlueprint b)
            {
                this.p = b;
            }

            public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                if (p is ROOM_RADIUSE)
                {
                    GuiSection s = new GuiSection()
                    {
                        hoverInfoGet = (GUI_BOX text) =>
                        {
                            text.title(¤¤NAME);
                            text.text(¤¤DESC);
                        },
                        render = (SPRITE_RENDERER r, float ds) =>
                        {
                            i = get.get();
                            SETT.OVERLAY().roomRadius(get.get(), ((ROOM_RADIUS_INSTANCE)get.get()).radius());
                            base.render(r, ds);
                        }
                    };

                    GHeader h = new GHeader(¤¤NAME)
                    {
                        render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                        {
                            base.render(r, ds, isHovered);
                        }
                    };
                    h.hoverInfoSet(¤¤DESC);
                    s.add(UI.icons().m.place_ellispse, 0, 0);

                    ROOM_RADIUSE r = (ROOM_RADIUSE)p;
                    INTE i = new INTE()
                    {
                        min = () => 0,
                        max = () => 100,
                        get = () => r.radiusInstance(get.get()).radiusRaw(),
                        set = (int t) => r.radiusInstance(get.get()).radiusRawSet((byte)t)
                    };
                    GSliderInt m = new GSliderInt(i, 200, true, false);
                    s.addRightC(8, m);
                    section.addRelBody(2, DIR.S, s);
                }
                else if (p is ROOM_RADIUS)
                {
                    section.add(new RenderImp()
                    {
                        render = (SPRITE_RENDERER r, float ds) =>
                        {
                            i = get.get();
                            SETT.OVERLAY().roomRadius(get.get(), ((ROOM_RADIUS_INSTANCE)get.get()).radius());
                        }
                    });
                }
            }

            public void hover(GBox box, Room room, int rx, int ry)
            {
                ModuleRadius.this.i = (RoomInstance)i;
                //ren.add();
            }

            public void problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry)
            {
                ROOM_RADIUS_INSTANCE i = (ROOM_RADIUS_INSTANCE)room;
                if (!i.searching())
                {
                    errors.add(¤¤PROBLEM);
                }
            }
        }
    }
}