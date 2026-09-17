using System;
using System.IO;
using snake2d.util.file;
using util.data.INT_O;
using world;
using world.map.regions;

namespace world.region
{
    public class RDEvent
    {
        private int am;
        public readonly INT_OE<Region> ii;
        public readonly INT_OE<Region> mark;

        public RDEvent(RDInit init)
        {
            string key = "EVENT_MARK";
            ii = init.count.new DataBit(key)
            {
                public override void set(Region t, int s)
                {
                    am -= get(t);
                    base.set(t, s);
                    am += get(t);
                }

                public override int get(Region t)
                {
                    if (t == null)
                        return am;
                    return base.get(t);
                }
            };

            mark = init.count.new DataShort("EVENT");

            init.savable.add(new SAVABLE()
            {
                public override void save(FilePutter file)
                {
                    // TODO Auto-generated method stub
                }

                public override void load(FileGetter file)
                {
                    am = 0;
                    foreach (Region reg in WORLD.REGIONS().all())
                        am += ii.get(reg);
                }

                public override void clear()
                {
                    am = 0;
                }
            });
        }

        public int total()
        {
            return am;
        }
    }
}