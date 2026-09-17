using game.battle.div;
using game.boosting;
using game.faction.npc;
using game.faction.player;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.misc;
using world.map.regions;

namespace settlement.stats.util
{
    public abstract class StatBooster : BValue
    {
        //	private final int index;

        public StatBooster()
        {
            //		this.index = index;
        }

        public override double vGet(Player f)
        {
            return 0;
        }

        public override double vGet(FactionNPC f)
        {
            return 0;
        }

        public override double vGet(Region reg)
        {
            return vGet(reg.faction());
        }

        public static StatBooster make(STAT stat)
        {
            return new StatBooster()
            {
                public double vGet(Div div)
                {
                    return CLAMP.d(stat.div().getD(div), 0, 1);
                }

                public double vGet(Induvidual indu)
                {
                    return CLAMP.d(stat.indu().getD(indu), 0, 1);
                }

                public double vGet(HCLASS_RACE t)
                {
                    return CLAMP.d(stat.data(t.cl).getD(t.race), 0, 1);
                }
            };
        }
    }
}