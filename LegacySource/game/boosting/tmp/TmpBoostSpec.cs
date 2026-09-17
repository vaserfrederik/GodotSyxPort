using System;
using System.Collections.Generic;
using snake2d.util.sprite;

namespace game.boosting.tmp
{
    public class TmpBoostSpec
    {
        public readonly string name;
        public readonly string desc;
        public readonly SPRITE icon;
        public readonly int index;
        public BoostSpecs spec;
        public readonly string key;

        public TmpBoostSpec(string key, string name, string desc, SPRITE icon)
        {
            index = TmpBoosting.allTmp.Add(this);
            int i = 1;
            string k = key;
            while (TmpBoosting.allMap.ContainsKey(k))
            {
                k = key + i;
                i++;
            }

            TmpBoosting.allMap.Add(k, this);
            this.key = key;
            this.name = name;
            this.desc = desc;
            this.icon = icon;
            spec = new BoostSpecs(new BSourceInfo(name, icon), false);
        }
    }
}