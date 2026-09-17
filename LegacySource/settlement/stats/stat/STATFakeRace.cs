using System;
using System.Collections.Generic;
using util.data;
using util.statistics;

namespace settlement.stats.stat
{
    public abstract class STATFakeRace : STAT, StatUpdatable
    {
        private readonly DataRaces history;
        private readonly INT_OE<Induvidual> indu;
        private readonly INT_OE<Div> div;
        private readonly INT_OE<HTYPE_RACE> type;

        public STATFakeRace(string key, StatsInit init) : this(key, init, null)
        {
            if (key == null)
                throw new RuntimeException();
        }

        public STATFakeRace(string key, StatsInit init, StatInfo info) : base(key, init, info)
        {
            history = new DataRaces(key, init, false)
            {
                public double GetD(Race t, int fromZero)
                {
                    double d = (double)dataDivider();
                    if (d == 0)
                        return 0;
                    return Get(t, fromZero) / (double)(d);
                }

                public int Min(Race t)
                {
                    return 0;
                }

                public int Max(Race t)
                {
                    return int.MaxValue;
                }
            };

            indu = new II<Induvidual>()
            {
                public int Get(Induvidual t)
                {
                    return history.Get(t.race());
                }

                public double GetD(Induvidual t)
                {
                    return history.GetD(t.race());
                }
            };

            div = new II<Div>()
            {
                public int Get(Div t)
                {
                    return history.Get(t.race());
                }

                public double GetD(Div t)
                {
                    return history.GetD(t.race());
                }
            };

            type = new II<HTYPE_RACE>()
            {
                public int Get(HTYPE_RACE t)
                {
                    return history.Get(t.race);
                }

                public double GetD(HTYPE_RACE t)
                {
                    return history.GetD(t.race);
                }
            };

            init.upers.Add(this);
        }

        public override int DataDivider()
        {
            return 1024;
        }

        public override INT_OE<Induvidual> Indu()
        {
            return indu;
        }

        public override HISTORY_INT_OBJECT<Race> Data()
        {
            return Data(null);
        }

        public override HISTORY_INT_OBJECT<Race> Data(HCLASS c)
        {
            return history;
        }

        public override INT_O<Div> Div()
        {
            return div;
        }

        public override INT_O<HTYPE_RACE> Type()
        {
            return type;
        }

        protected abstract double GetDD(Race r);

        public override void Update(double ds)
        {
            int tot = 0;
            for (int ri = 0; ri < RACES.all().Count; ri++)
            {
                Race r = RACES.all()[ri];
                int am = (int)(dataDivider() * CLAMP.d(GetDD(r), 0, 1));
                tot += am;
                history.Set(r, am);
            }
            history.Set(null, tot / RACES.all().Count);
        }

        private abstract class II<T> : INT_OE<T>
        {
            public int Min(T t)
            {
                return 0;
            }

            public int Max(T t)
            {
                return int.MaxValue;
            }

            public void Set(T t, int i)
            {
            }
        }
    }
}