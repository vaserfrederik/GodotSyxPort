using System.Collections.Generic;
using util.data;
using util.misc;

namespace settlement.stats
{
    public sealed class StatsInit
    {
        public readonly PATH pd = PATHS.INIT().getFolder("stats");
        public readonly PATH pt = PATHS.TEXT().getFolder("stats");

        public readonly KeyMap<SAVABLE> savers = new KeyMap<SAVABLE>();

        public readonly LinkedList<StatUpdatableI> updatable = new LinkedList<StatUpdatableI>();

        public readonly LinkedList<StatDisposable> disposable = new LinkedList<StatDisposable>();
        public readonly LinkedList<StatCollection> holders = new LinkedList<StatCollection>();
        public readonly LinkedList<StatUpdatable> upers = new LinkedList<StatUpdatable>();
        public readonly ArrayListGrower<Addable> addable = new ArrayListGrower<Addable>();

        public readonly LinkedList<StatInitable> onArrival = new LinkedList<StatInitable>();
        public readonly LinkedList<STAT> onArrivalStats = new LinkedList<STAT>();
        public readonly LinkedList<ACTION_O<Induvidual>> onArrivalActions = new LinkedList<ACTION_O<Induvidual>>();
        public readonly LinkedList<StatInitable> onConstruct = new LinkedList<StatInitable>();
        public readonly LinkedList<INT_OE<Induvidual>> copier = new LinkedList<INT_OE<Induvidual>>();

        public readonly LinkedList<STAT> stats = new LinkedList<STAT>();

        public readonly LinkedList<DataRaces> datas = new LinkedList<DataRaces>();

        public readonly KeyMap<StatCollection> collMap = new KeyMap<StatCollection>();
        public readonly KeyMap<STAT> statMap = new KeyMap<STAT>();

        public StatCollection coll;

        public readonly DataO<Induvidual> count = new DataO<Induvidual>("STATS")
        {
            protected override long[] data(Induvidual t)
            {
                return t.data;
            }
        };

        public readonly Json dText = new Json(PATHS.TEXT().getFolder("stats").gets("NAMES"));

        public StatsInit()
        {
        }

        public void init(string key, StatCollection collection)
        {
            coll = collection;
            collMap.put(key, collection);
        }

        public interface Addable
        {
            void addH(Induvidual i);
            void removeH(Induvidual i);
            void addPrivate(Induvidual i);
            void removePrivate(Induvidual i);
        }

        public interface StatUpdatableI
        {
            void update16(Humanoid h, int updateR, bool day, int updateI);
        }

        public interface StatInitable
        {
            void init(Induvidual h);
        }

        public interface StatDisposable
        {
            void dispose(Humanoid h);
        }

        public interface StatUpdatable
        {
            void update(double ds);
        }
    }
}