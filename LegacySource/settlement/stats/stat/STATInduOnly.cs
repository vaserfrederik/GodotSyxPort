using game.battle.div;
using game.time;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using util.data;
using util.statistics;

namespace settlement.stats.stat
{
    public class STATInduOnly : STAT
    {
        private readonly INT_OE<Induvidual> indu;
        private static readonly HISTORY_INT_OBJECT<Race> data = new HISTORY_INT_OBJECT<Race>()
        {
            public override int min(Race t)
            {
                return 0;
            }

            public override int max(Race t)
            {
                return 1;
            }

            public override double getD(Race t, int fromZero)
            {
                return 0;
            }

            public override TIMECYCLE time()
            {
                return TIME.days();
            }

            public override int historyRecords()
            {
                return STATS.DAYS_SAVED;
            }

            public override int get(Race t, int fromZero)
            {
                return 0;
            }
        };

        private static readonly INT_O<Div> div = new INT_O<Div>()
        {
            public override int get(Div t)
            {
                return 0;
            }

            public override int min(Div t)
            {
                return 0;
            }

            public override int max(Div t)
            {
                return 1;
            }
        };

        private static readonly INT_O<HTYPE_RACE> type = new INT_O<HTYPE_RACE>()
        {
            public override int get(HTYPE_RACE t)
            {
                return 0;
            }

            public override int min(HTYPE_RACE t)
            {
                return 0;
            }

            public override int max(HTYPE_RACE t)
            {
                return 1;
            }
        };

        public STATInduOnly(string key, StatsInit init, INT_OE<Induvidual> data)
            : base(key, init, null)
        {
            this.indu = data;
        }

        protected int pdivider(HCLASS c, Race r, int daysback)
        {
            return 1;
        }

        public override int dataDivider()
        {
            return indu.max(null);
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
            return data;
        }

        public override INT_O<Div> div()
        {
            return div;
        }

        public override bool hasIndu()
        {
            return key != null && key.Length > 0;
        }

        public override INT_O<HTYPE_RACE> type()
        {
            return type;
        }
    }
}