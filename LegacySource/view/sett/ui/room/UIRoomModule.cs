using System;
using System.Collections.Generic;

namespace view.sett.ui.room
{
    public abstract class UIRoomModule
    {
        public virtual void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
        {
        }

        public virtual void AppendPanelIcon(List<RENDEROBJ> section, GETTER<RoomInstance> get)
        {
        }

        public virtual void AppendManageScr(GGrid icons, GGrid text, GuiSection extra)
        {
        }

        public virtual void AppendTableFilters(List<GTFilter<RoomInstance>> filters, List<GTSort<RoomInstance>> sorts, List<UIRoomBulkApplier> appliers)
        {
        }

        public virtual void AppendButt(GuiSection butt, GETTER<RoomInstance> get)
        {
        }

        public virtual void Hover(GBox box, Room i, int rx, int ry)
        {
        }

        public virtual void Problem(Stack<Str> free, List<CharSequence> errors, List<CharSequence> warnings, Room r, int rx, int ry)
        {
        }

        public abstract class UIRoomModuleImp<T extends RoomInstance, B extends RoomBlueprintIns<T>> : UIRoomModule
            where T : RoomInstance
            where B : RoomBlueprintIns<T>
        {
            protected readonly B blueprint;

            public UIRoomModuleImp(B blueprint)
            {
                this.blueprint = blueprint;
            }

            protected virtual void AppendPanel(GuiSection section, GGrid g, GETTER<T> getter, int x1, int y1)
            {
            }

            protected virtual void AppendMain(GGrid icons, GGrid text, GuiSection sExtra)
            {
            }

            protected virtual void AppendTableFilters(List<GTFilter<RoomInstance>> filters, List<GTSort<RoomInstance>> sorts, List<UIRoomBulkApplier> appliers)
            {
            }

            protected virtual void AppendTableButt(GuiSection s, GETTER<RoomInstance> ins)
            {
            }

            protected virtual void Hover(GBox box, T i)
            {
            }

            protected virtual void Problem(T i, Stack<Str> free, List<CharSequence> errors, List<CharSequence> warnings)
            {
            }

            public override UIRoomModule Make()
            {
                return new UIRoomModule
                {
                    AppendPanel = (section, get, x1, y1) =>
                    {
                        GETTER<T> getter = new GETTER<T>()
                        {
                            Get = () => (T)get.Get()
                        };
                        GGrid g = new GGrid(section, 2, y1);
                        AppendPanel(section, g, getter, x1, y1);
                    },
                    AppendManageScr = (icons, text, extra) => AppendMain(icons, text, extra),
                    AppendTableFilters = (filters, sorts, appliers) => AppendTableFilters(filters, sorts, appliers),
                    AppendButt = (s, get) => AppendTableButt(s, get),
                    Hover = (box, room, rx, ry) => Hover(box, (T)room),
                    Problem = (free, errors, warnings, room, rx, ry) => Problem((T)room, free, errors, warnings)
                };
            }
        }
    }
}