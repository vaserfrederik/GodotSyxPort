using System.Collections.Generic;
using init.paths;
using init.race;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;

namespace init.resources
{
    public sealed class ResSupplies
    {
        public readonly LIST<ResSupply> ALL;
        private readonly ResSupply[] look;

        private readonly RMAP<ResSupply> map;

        public ResSupplies()
        {
            ArrayListGrower<ResSupply> all = new ArrayListGrower<ResSupply>();
            PATH p = PATHS.INIT().getFolder("resource").getFolder("supply");
            string[] keys = p.getFiles();

            foreach (string k in keys)
            {
                Json j = new Json(p.gets(k));
                ResSupply s = new ResSupply(k, j, all);
                if (look[s.resource.index()] != null)
                    j.error("Army supply: " + look[s.resource.index()].resource.key + " refers to the same resource: " + s.resource.key, k);
                look[s.resource.index()] = s;
            }
            map = new RMAP<ResSupply>("ARMY_SUPPLY", all);
            this.ALL = all;
        }

        public ResSupply get(RESOURCE res)
        {
            return look[res.index()];
        }

        public RMAP<ResSupply> MAP()
        {
            return map;
        }

        public void setEfficiency(Race race, Json json)
        {
            map.newKJson("MILITARY_SUPPLY_USE", json)
            {
                protected override void process(ResSupply s, Json j, string key, bool isWeak)
                {
                    s.setRace(race, j.i(key, 0, 10000));
                }
            };
        }
    }
}