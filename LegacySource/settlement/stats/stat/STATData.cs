using game.battle.div;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.StatsInit;
using util.data;
using util.statistics;

namespace settlement.stats.stat
{
    public class STATData : STAT, Addable
    {
        public readonly SettStatistics stats;
        private readonly INT_OE<Induvidual> indu;

        public STATData(string key, string dkey, StatsInit init, INT_OE<Induvidual> data)
            : this(key, dkey, init, data, null)
        {
        }

        public STATData(string key, StatsInit init, DataO<Induvidual>.DataAbs data)
            : this(key, data.key, init, data, null)
        {
        }

        public STATData(string key, StatsInit init, DataO<Induvidual>.DataAbs data, StatInfo info)
            : this(key, data.key, init, data, info)
        {
        }

        public STATData(string key, string dkey, StatsInit init, INT_OE<Induvidual> data, StatInfo info)
            : base(key, init, info)
        {
            stats = new SettStatistics(dkey, init, this.info)
            {
                PopDivider = (HCLASS c, Race r, int daysback) => (int)pdivider(c, r, daysback),
                DataDivider = () => dataDivider()
            };

            indu = new INT_OE<Induvidual>
            {
                Get = t => data.get(t),
                Min = t => data.min(t),
                Max = t => data.max(t),
                GetD = t => data.getD(t),
                Set = (t, i) =>
                {
                    removeH(t);
                    data.set(t, i);
                    addH(t);
                }
            };

            init.addable.add(this);
        }

        protected int pdivider(HCLASS c, Race r, int daysback)
        {
            return STATS.POP().POP.data(c).get(r, daysback);
        }

        public override int dataDivider()
        {
            return indu.max(null);
        }

        public override void addPrivate(Induvidual i)
        {
            stats.inc(i, indu.get(i));
        }

        public override void removePrivate(Induvidual i)
        {
            stats.inc(i, -indu.get(i));
        }

        public override INT_OE<Induvidual> indu()
        {
            return indu;
        }

        public override HISTORY_INT_OBJECT<Race> data()
        {
            return data(null);
        }

        public override HISTORY_INT_OBJECT<Race> data(HCLASS c)
        {
            return stats.data(c);
        }

        public override INT_O<Div> div()
        {
            return stats.div();
        }

        public override bool hasIndu()
        {
            return key != null && key.Length > 0;
        }

        public override INT_O<HTYPE_RACE> type()
        {
            return stats.type();
        }
    }
}