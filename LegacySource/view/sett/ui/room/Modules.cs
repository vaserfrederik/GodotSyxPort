using System;
using System.Collections.Generic;

namespace view.sett.ui.room
{
    final class Modules
    {
        private readonly ModuleMaker[] makers;

        Modules(Init init)
        {
            makers = new ModuleMaker[]
            {
                new ModuleInstance(init),
                new ModuleEmployment(init),
                new ModuleRadius(init),
                new ModuleDegrade(init),
                new ModuleUpgradable(init),
                new ModulePumpable(init),
                new ModuleIrrigated(init),

                new ModuleService(init),
                new ModuleIndustry(init),
                new ModuleGrave(init),
                new ModuleConstructor(init),
                new ModulePunish(init),
            };
        }

        UIRoomModule[] Get(RoomBlueprint p)
        {
            List<UIRoomModule> apps = new List<UIRoomModule>(32);

            foreach (ModuleMaker m in makers)
            {
                m.Make(p, apps);
            }

            p.AppendView(apps);
            UIRoomModule[] asArray = new UIRoomModule[apps.Count];
            for (int i = 0; i < apps.Count; i++)
                asArray[i] = apps[i];

            return asArray;
        }

        interface ModuleMaker
        {
            void Make(RoomBlueprint p, LISTE<UIRoomModule> l);
        }
    }
}