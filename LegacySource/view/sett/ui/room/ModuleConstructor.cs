using System.Collections.Generic;
using settlement.environment;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using snake2d.util.gui;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    public sealed class ModuleConstructor : ModuleMaker
    {
        public ModuleConstructor(Init init)
        {
        }

        public void Make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is RoomBlueprintImp)
            {
                RoomBlueprintImp pp = (RoomBlueprintImp)p;
                if (pp.Constructor() != null)
                    l.Add(new I());
            }
        }

        private sealed class I : UIRoomModule
        {
            public I()
            {
            }

            public void AppendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
            {
            }

            public void Hover(GBox box, Room room, int rx, int ry)
            {
                Furnisher f = room.Constructor();
                if (f != null)
                {
                    bool has = false;
                    foreach (SettEnv e in SETT.ENV().Map.All())
                    {
                        if (f.EnvValue(e))
                        {
                            if (!has)
                            {
                                box.NL(8);
                                box.TextL(Dic.¤¤Emits);
                                box.NL();
                            }
                            box.Text(e.Info.Name);
                        }
                    }
                    if (has)
                        box.NL(8);
                }
            }

            public void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}