using System;
using System.IO;
using System.Linq;

namespace Settlement.Stats.Stat
{
    public interface ISettStatistics
    {
        INFO Info { get; }

        HISTORY_INT_OBJECT<Race> Data { get; }
        HISTORY_INT_OBJECT<Race> Data(HCLASS c);
        INT_O<Div> Div { get; }
        INT_O<HTYPE_RACE> Type { get; }

        int DataDivider { get; }
    }

    public class SettStatisticsClRace : ISettStatistics
    {
        private readonly DataRaces[] datas = new DataRaces[HCLASSES.ALL().Count];
        private readonly DataRaces total;
        private readonly INFO info;

        public SettStatisticsClRace(string key, StatsInit init, string name, string desc)
            : this(key, init, new StatInfo(name, desc))
        {
        }

        public SettStatisticsClRace(string key, StatsInit init, INFO info)
        {
            for (int c = 0; c < datas.Length; c++)
            {
                int k = c;
                datas[c] = new DataRaces(key + "_" + HCLASSES.ALL()[k].Key, init, false)
                {
                    GetD = (Race t, int fromZero) =>
                    {
                        if (fromZero == 0)
                            return GetDCurrent(this, HCLASSES.ALL()[k], t);
                        double d = (double)(PopDivider(HCLASSES.ALL()[k], t, fromZero) * DataDivider);
                        if (d == 0)
                            return 0;
                        return Get(t, fromZero) / (double)d;
                    },
                    Min = (Race t) => 0,
                    Max = (Race t) => (int)(PopDivider(HCLASSES.ALL()[k], t, 0) * DataDivider)
                };
            }

            this.info = info;

            total = new DataRaces(key + "_ALL", init, false)
            {
                GetD = (Race t, int fromZero) =>
                {
                    double d = (double)(PopDivider(null, t, fromZero) * DataDivider);
                    if (d == 0)
                        return 0;
                    return Get(t, fromZero) / (double)d;
                },
                Min = (Race t) => 0,
                Max = (Race t) => (int)(PopDivider(null, t, 0) * DataDivider)
            };
        }

        private readonly INT_O<Div> div = new INT_O<Div>
        {
            Get = (Div t) => data(HCLASSES.CITIZEN()).Get(t.Race),
            Min = (Div t) => 0,
            Max = (Div t) => DataDivider * PopDivider(HCLASSES.CITIZEN(), t.Race, 0)
        };

        public INFO Info => info;

        public HISTORY_INT_OBJECT<Race> Data => data(null);

        public HISTORY_INT_OBJECT<Race> Data(HCLASS c)
        {
            if (c == null)
                return total;
            return datas[c.Index];
        }

        public INT_O<Div> Div => div;

        public void Inc(HCLASS c, Race r, int am)
        {
            datas[c.Index].Set(r, datas[c.Index].Get(r) + am);
            datas[c.Index].Set(null, datas[c.Index].Get(null) + am);

            if (c.Player)
            {
                total.Set(r, total.Get(r) + am);
                total.Set(null, total.Get(null) + am);
            }
        }

        public void Inc(Induvidual i, int d)
        {
            if (i.Added)
            {
                Inc(i.Clas, i.Race, d);
            }
        }

        protected int PopDivider(HCLASS c, Race r, int daysback)
        {
            return STATS.POP().POP.Data(c).Get(r, daysback);
        }

        public int DataDivider => 1;

        private readonly INT_O<HTYPE_RACE> type = new INT_O<HTYPE_RACE>
        {
            Get = (HTYPE_RACE t) => data(t.Cl.CLASS).Get(t.Race),
            Min = (HTYPE_RACE t) => 0,
            Max = (HTYPE_RACE t) => DataDivider * PopDivider(t.Cl.CLASS, t.Race, 0)
        };

        public INT_O<HTYPE_RACE> Type => type;

        protected double GetDCurrent(DataRaces c, HCLASS cl, Race r)
        {
            double d = (double)(PopDivider(cl, r, 0) * DataDivider);
            if (d == 0)
                return 0;
            return c.Get(r, 0) / (double)d;
        }
    }

    public class SettStatistics : ISettStatistics
    {
        private readonly DataRaces[] datas = new DataRaces[HCLASSES.ALL().Count];
        private readonly DataRaces total;
        private int[] divData = Alloc.ii(Config.Battle().DIVISIONS_PER_ARMY * 2);
        private int[] typeData = Alloc.ii(HTYPE_RACE.ALL().Count);
        private readonly INFO info;

        public SettStatistics(string key, StatsInit init, string name, string desc)
            : this(key, init, new StatInfo(name, desc))
        {
        }

        public SettStatistics(string key, StatsInit init, INFO info)
        {
            for (int c = 0; c < datas.Length; c++)
            {
                int k = c;
                datas[c] = new DataRaces(key + "_" + HCLASSES.ALL()[k].Key, init, true)
                {
                    GetD = (Race t, int fromZero) =>
                    {
                        double d = (double)(PopDivider(HCLASSES.ALL()[k], t, fromZero) * DataDivider);
                        if (d == 0)
                            return 0;
                        return Get(t, fromZero) / (double)d;
                    },
                    Min = (Race t) => 0,
                    Max = (Race t) => (int)(PopDivider(HCLASSES.ALL()[k], t, 0) * DataDivider)
                };
            }

            this.info = info;

            total = new DataRaces(key + "_ALL", init, true)
            {
                GetD = (Race t, int fromZero) =>
                {
                    double d = (double)(PopDivider(null, t, fromZero) * DataDivider);
                    if (d == 0)
                        return 0;
                    return Get(t, fromZero) / (double)d;
                },
                Min = (Race t) => 0,
                Max = (Race t) => (int)(PopDivider(null, t, 0) * DataDivider)
            };

            init.Save(key, () =>
            {
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write(divData.Length);
                        foreach (var item in divData)
                        {
                            bw.Write(item);
                        }

                        bw.Write(typeData.Length);
                        foreach (var item in typeData)
                        {
                            bw.Write(item);
                        }
                    }

                    return ms.ToArray();
                }
            });
        }

        private readonly INT_O<Div> div = new INT_O<Div>
        {
            Get = (Div t) => STATS.BATTLE().DIV.Get(t).Get(t),
            Min = (Div t) => 0,
            GetD = (Div t) =>
            {
                double p = STATS.POP().POP.Div().Get(t);
                if (p == 0)
                    return 0;
                return Get(t) / (DataDivider * p);
            },
            Max = (Div t) => int.MaxValue
        };

        public INFO Info => info;

        public HISTORY_INT_OBJECT<Race> Data => data(null);

        public HISTORY_INT_OBJECT<Race> Data(HCLASS c)
        {
            if (c == null)
                return total;
            return datas[c.Index];
        }

        public INT_O<Div> Div => div;

        public void Inc(HTYPE type, Race r, int am, int divi)
        {
            typeData[HTYPE_RACE.Get(r, type).Index] += am;
            typeData[HTYPE_RACE.Get(null, type).Index] += am;
            typeData[HTYPE_RACE.Get(r, null).Index] += am;

            HCLASS c = type.CLASS;
            datas[c.Index].Set(r, datas[c.Index].Get(r) + am);
            datas[c.Index].Set(null, datas[c.Index].Get(null) + am);

            if (c.Player)
            {
                total.Set(r, total.Get(r) + am);
                total.Set(null, total.Get(null) + am);
            }
            if (divi != -1)
            {
                divData[divi] += am;
            }
        }

        public void Inc(Induvidual i, int d)
        {
            if (i.Added)
            {
                Div div = STATS.BATTLE().DIV.Get(i);
                Inc(i.HType, i.Race, d, div == null ? -1 : div.Index);
            }
        }

        protected int PopDivider(HCLASS c, Race r, int daysback)
        {
            return STATS.POP().POP.Data(c).Get(r, daysback);
        }

        public int DataDivider => 1;

        private readonly INT_O<HTYPE_RACE> type = new INT_O<HTYPE_RACE>
        {
            Get = (HTYPE_RACE t) => typeData[t.Index],
            Min = (HTYPE_RACE t) => 0,
            GetD = (HTYPE_RACE t) =>
            {
                double p = STATS.POP().POP.Type().Get(t);
                if (p == 0)
                    return 0;
                return Get(t) / (DataDivider * p);
            },
            Max = (HTYPE_RACE t) => int.MaxValue
        };

        public INT_O<HTYPE_RACE> Type => type;
    }
}