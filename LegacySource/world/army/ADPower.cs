using System;
using System.Collections.Generic;
using game;
using game.faction;
using snake2d.util.misc;
using util.data;
using world.entity.army;

namespace world.army
{
    public sealed class ADPower
    {
        private readonly INT_OE<WArmy> carmy;
        private readonly INT_OE<Faction> cfaction;

        private readonly INT_OE<WArmy> army;
        private readonly INT_OE<Faction> faction;

        public ADPower(ADInit init)
        {
            carmy = init.dataA.newDataBit("CPOWER");
            cfaction = init.dataT.newDataBit("CPOWER");

            army = init.dataA.newDataInt("POWER");
            faction = init.dataT.newDataInt("POWER");

            init.countable.Add(new ADInit.Countable()
            {
                public void count(WArmy a, int delta)
                {
                    mor(a);
                }
            });

            init.registers.Add(new ADInit.Register()
            {
                public void register(ADDiv div, int d)
                {
                    mor(div.army());
                }
            });
        }

        private void mor(WArmy a)
        {
            cfaction.set(a.faction(), 0);
            carmy.set(a, 0);
        }

        public int get(WArmy a)
        {
            if (carmy.get(a) == 0)
            {
                army.set(a, CLAMP.i((int)GAME.battle().power.get(a), 0, int.MaxValue));
            }
            carmy.inc(a, 1);
            return (int)Math.Ceiling(army.get(a) * morale(a));
        }

        public int get(Faction f)
        {
            if (cfaction.get(f) == 0)
            {
                int p = 0;
                for (int ai = 0; ai < f.armies().all().Count; ai++)
                {
                    WArmy a = f.armies().all()[ai];
                    p += get(a);
                }
                cfaction.set(f, 1);
                faction.set(f, p);
                if (p < 0)
                {
                    for (int ai = 0; ai < f.armies().all().Count; ai++)
                    {
                        WArmy a = f.armies().all()[ai];
                        Console.Error.WriteLine(get(a) + " " + f);
                    }
                }
            }
            return faction.get(f);
        }

        public double morale(WArmy a)
        {
            return CLAMP.d(AD.supplies().health(a) * (0.5 + 0.5 * AD.supplies().morale(a)), 0, 1);
        }
    }
}