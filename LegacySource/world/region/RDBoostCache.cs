using System;
using game;
using game.boosting;
using init.sprite.UI;
using snake2d.util.misc;
using util.data;
using world.map.regions;

namespace world.region
{
    public class RDBoostCache
    {
        public readonly Boostable boost;
        public readonly INT_OE<Region> cache;
        public readonly INT_OE<Region> value;

        public RDBoostCache(RDInit init, string key, string name, string desc, Icon icon)
        {
            this(init, BOOSTING.Push(key, 0, name, desc, icon, BoostableCat.ALL().WORLD_DUMP));
        }

        public RDBoostCache(RDInit init, Boostable boost)
        {
            this.boost = boost;
            cache = init.count.new DataBit("CACHE" + boost.key);
            value = init.count.new DataNibble("CACHEL" + boost.key);
        }

        public double Get(Region reg)
        {
            if (reg == null)
                return 0;
            int upI = ((GAME.UpdateI() >> 5) & 1);
            if (cache.Get(reg) != upI)
            {
                cache.Set(reg, upI);

                double v = PGet(reg);
                value.SetD(reg, v);
            }
            return value.GetD(reg);
        }

        protected double PGet(Region reg)
        {
            double v = boost.Get(reg);
            v = CLAMP.d(v, 0, 1);
            return v;
        }
    }
}