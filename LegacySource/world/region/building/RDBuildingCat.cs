using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using world.map.regions;
using world.region.RD;

namespace world.region.building
{
    public class RDBuildingCat
    {
        private ArrayListGrower<RDBuilding> all = new ArrayListGrower<RDBuilding>();
        public readonly COLOR color;
        public readonly string key;
        public readonly int order;

        public RDBuildingCat(Creator creator, LISTE<RDBuilding> all, RDInit init, string folder, ResFolder p) 
        {
            this.key = folder.ToUpper(CultureInfo.InvariantCulture);
            Json json = new Json(p.init.gets("_CAT"));
            this.color = new ColorImp(json);
            order = json.i("ORDER", 0, 10000000, 0);
            addJsons(creator, all, init, p);
            creator.generate(all, init, this, p);

            RDBuilding[] bus = new RDBuilding[this.all.size()];
            for (int i = 0; i < this.all.size(); i++)
                bus[i] = this.all.get(i);
            this.all.clear();
            Array.Sort(bus, (o1, o2) => o1.order.CompareTo(o2.order));
            this.all.add(bus);
        }

        public LIST<RDBuilding> all()
        {
            return all;
        }

        private void addJsons(Creator creator, LISTE<RDBuilding> all, RDInit init, ResFolder p)
        {
            foreach (string f in p.init.getFiles())
            {
                creator.read(all, init, this, f, p);
            }
        }

        static readonly BValue lValue = new BValue.BValueNone()
        {
            public override double vGet(Region reg) => 1.0,
            public override double vGet(Faction f) => 0
        };

        static readonly BValue lGlobal = new BValue.BValueNone()
        {
            public override double vGet(Region reg) => 1.0,
            public override double vGet(Faction f) => 1.0
        };
    }
}