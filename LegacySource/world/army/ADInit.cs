using System.Collections.Generic;
using game.faction;
using snake2d.util.misc;
using util.data;
using world.entity.army;

namespace world.army
{
    public class ADInit
    {
        public readonly DataO<WArmy> dataA = new DataO<WArmy>("ADARMY")
        {
            protected override long[] data(WArmy t)
            {
                return t.divs().data;
            }
        };

        public readonly DataO<Faction> dataT = new DataO<Faction>("ADFACTION")
        {
            protected override long[] data(Faction t)
            {
                return AD.army(t).data;
            }
        };

        private readonly LinkedList<Countable> countable = new LinkedList<Countable>();
        private readonly LinkedList<Register> registers = new LinkedList<Register>();
        private readonly LinkedList<Updater> updaters = new LinkedList<Updater>();
        private readonly LinkedList<ACTION_O<Faction>> inits = new LinkedList<ACTION_O<Faction>>();

        public interface Countable
        {
            void count(WArmy a, int delta);
        }

        public interface Register
        {
            void register(ADDiv div, int d);
        }

        public interface Updater
        {
            void update(WArmy a, double timeSinceLast);
            void update(Faction f, double timeSinceLast);
        }

        public ADInit()
        {
        }
    }
}