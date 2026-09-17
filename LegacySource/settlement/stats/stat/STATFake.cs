using System.Collections.Generic;
using game.battle.div;
using game.time;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.Induvidual;
using util.data;
using util.statistics;

namespace settlement.stats.stat
{
    public abstract class STATFake : STAT
    {
        private readonly List<HISTORY_INT_OBJECT<Race>> datas = new List<HISTORY_INT_OBJECT<Race>>(HCLASSES.ALL().Count + 1);
        private readonly INT_O<Div> div;
        private readonly INT_O<HTYPE_RACE> type;
        private readonly INT_OE<Induvidual> indu;

        public STATFake(string key, StatsInit init) : this(key, init, null) { }

        public STATFake(string key, StatsInit init, StatInfo info) : base(key, init, info)
        {
            foreach (HCLASS c in HCLASSES.ALL())
            {
                datas.Add(new HISTORY_INT_OBJECT<Race>()
                {
                    Min = (Race t) => 0,
                    Max = (Race t) => dataDivider() * pdivider(c, t, 0),
                    GetD = (Race t, int fromZero) => getDD(c, t, fromZero),
                    Time = () => TIME.days(),
                    HistoryRecords = () => STATS.DAYS_SAVED,
                    Get = (Race t, int fromZero) =>
                    {
                        double d = dataDivider() * pdivider(c, t, fromZero);
                        return (int)(getDD(c, t, fromZero) * d);
                    }
                });
            }

            datas.Add(new HISTORY_INT_OBJECT<Race>()
            {
                Min = (Race t) => 0,
                Max = (Race t) => dataDivider() * pdivider(null, t, 0),
                GetD = (Race t, int fromZero) =>
                {
                    double am = 0;
                    for (int hi = 0; hi < HCLASSES.ALL().Count; hi++)
                    {
                        HCLASS cl = HCLASSES.ALL()[hi];
                        if (cl.player)
                        {
                            am += STATS.POP().POP.data(cl).Get(null, fromZero) * getDD(cl, null, fromZero);
                        }
                    }
                    double pop = STATS.POP().POP.data().Get(null, fromZero);
                    if (pop == 0)
                        return am > 0 ? 1 : 0;
                    return am / pop;
                },
                Time = () => TIME.days(),
                HistoryRecords = () => STATS.DAYS_SAVED,
                Get = (Race t, int fromZero) =>
                {
                    double d = dataDivider() * pdivider(null, t, fromZero);
                    return (int)(getD(t, fromZero) * d);
                }
            });

            div = new INT_O<Div>()
            {
                Get = (Div t) => data(HCLASSES.CITIZEN()).Get(t.race()),
                Min = (Div t) => 0,
                Max = (Div t) => dataDivider() * pdivider(HCLASSES.CITIZEN(), t.race(), 0)
            };

            type = new INT_O<HTYPE_RACE>()
            {
                Get = (HTYPE_RACE t) => data(t.cl.CLASS).Get(t.race),
                Min = (HTYPE_RACE t) => 0,
                Max = (HTYPE_RACE t) => dataDivider() * pdivider(t.cl.CLASS, t.race, 0)
            };

            indu = new INT_OE<Induvidual>()
            {
                Get = (Induvidual t) => (int)(64 * induGet(t)),
                Min = (Induvidual t) => 0,
                Max = (Induvidual t) => 64,
                Set = (Induvidual t, int i) => { }
            };
        }

        protected double induGet(Induvidual t)
        {
            return getDD(t.clas(), t.race(), 0);
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
            if (c == null)
                return datas[datas.Count - 1];
            return datas[c.index()];
        }

        public override INT_O<HTYPE_RACE> type()
        {
            return type;
        }

        public override INT_O<Div> div()
        {
            return div;
        }

        protected abstract double getDD(HCLASS s, Race r, int daysBack);

        protected int pdivider(HCLASS c, Race r, int daysback)
        {
            return STATS.POP().POP.data(c).Get(r, daysback);
        }

        public override int dataDivider()
        {
            return 1;
        }
    }
}