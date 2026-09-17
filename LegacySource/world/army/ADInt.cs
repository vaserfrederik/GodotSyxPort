using System;
using game.faction;
using util.data;
using world.entity.army;

namespace world.army
{
    public interface ADInt : INT_O<WArmy>
    {
        public default int faction(WArmy a)
        {
            return faction(a.faction());
        }

        public int faction(Faction f);

        public interface ADIntE : ADInt, INT_OE<WArmy>
        {
        }

        public class ADIntImp : ADIntE, ADInit.Countable
        {
            private readonly INT_OE<WArmy> a;
            private readonly INT_OE<Faction> f;

            public ADIntImp(ADInit init, string key, string name, string desc)
            {
                a = init.dataA.new DataInt(key);
                f = init.dataT.new DataInt(key);
                init.countable.Add(this);
            }

            public void set(WArmy t, int i)
            {
                count(t, -1);
                a.set(t, i);
                count(t, 1);
            }

            public int get(WArmy t)
            {
                return a.get(t);
            }

            public int min(WArmy t)
            {
                return 0;
            }

            public int max(WArmy t)
            {
                return int.MaxValue;
            }

            public int faction(Faction f)
            {
                return this.f.get(f);
            }

            public void count(WArmy t, int delta)
            {
                f.inc(t.faction(), a.get(t) * delta);
            }
        }
    }
}