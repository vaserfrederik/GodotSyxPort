using System.Collections.Generic;
using game.boosting;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.employment;
using snake2d.util.sets;

namespace view.ui.tech
{
    public class NodeBoosts
    {
        public readonly KeyMap<UpEntry> UpgradeBoost = new KeyMap<UpEntry>();
        public readonly KeyMap<TEntry> Tools = new KeyMap<TEntry>();

        public NodeBoosts()
        {
            foreach (Industry ins in SETT.ROOMS().industries.all)
            {
                for (int i = 1; i <= ins.blue.upgrades().max(); i++)
                {
                    string k = ins.blue.upgrades().reqs.get(i - 1).key;
                    double v = ins.blue.upgrades().boost(i) - ins.blue.upgrades().boost(i - 1);
                    if (!UpgradeBoost.ContainsKey(k))
                    {
                        UpEntry e = new UpEntry();
                        e.blue = ins.blue;
                        e.bo = ins.bonus();
                        e.value = v;
                        UpgradeBoost.Put(k, e);
                    }
                }
            }

            foreach (RoomEmployment emp in SETT.ROOMS().employment.ALL())
            {
                foreach (RoomEquip t in emp.tools())
                {
                    TEntry e = new TEntry();
                    e.blue = emp.blueprint();
                    e.value = t;
                    e.bo = e.blue.bonus();
                    if (!Tools.ContainsKey(t.target(emp).boost().key))
                        Tools.Put(t.target(emp).boost().key, e);
                }
            }
        }

        public class UpEntry
        {
            public double value;
            public RoomBlueprintImp blue;
            public Boostable bo;
        }

        public class TEntry
        {
            public RoomEquip value;
            public RoomBlueprintImp blue;
            public Boostable bo;
        }
    }
}