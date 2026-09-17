using System;
using System.Collections.Generic;
using game.battle.div;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.Induvidual;
using settlement.stats.STATS;
using settlement.stats.StatsInit;
using util.data;
using util.statistics;

public abstract class STATFakeData : STAT, StatUpdatable
{
    private readonly SettStatisticsClRace stats;
    private readonly INT_OE<Induvidual> indu;

    public STATFakeData(string key, StatsInit init) : this(key, init.coll.key + "_" + key, init, null)
    {
        if (key == null)
            throw new Exception();
    }

    public STATFakeData(string key, string dkey, StatsInit init, StatInfo info) : base(key, init, info)
    {
        stats = new SettStatisticsClRace(dkey, init, info)
        {
            DataDivider = () => DataDivider(),
            GetDCurrent = (c, cl, r) =>
            {
                if (r == null)
                {
                    double tot = 0;
                    double pop = 0;
                    double ave = 0;
                    for (int ri = 0; ri < RACES.all().Count; ri++)
                    {
                        Race rr = RACES.all()[ri];
                        double p = STATS.POP().POP.data(cl).get(rr);
                        pop += p;
                        double v = GetDCurrent(c, cl, rr);
                        ave += v;
                        tot += v * p;
                    }
                    if (pop > 0)
                        return tot / pop;
                    return ave / RACES.all().Count;
                }
                return GetDD(cl, r);
            }
        };

        indu = new INT_OE<Induvidual>()
        {
            Get = t => (int)(indu(t) * 16),
            Min = t => 0,
            Max = t => DataDivider() * 16,
            Set = (t, i) => { }
        };

        init.upers.Add(this);
    }

    protected double indu(Induvidual t)
    {
        return (int)data(t.clas()).getD(t.race());
    }

    public override int DataDivider()
    {
        return 1;
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

    public override INT_O<HTYPE_RACE> type()
    {
        return stats.type();
    }

    protected abstract double GetDD(HCLASS cl, Race race);

    public override void update(double ds)
    {
        for (int ci = 0; ci < HCLASSES.ALL().Count; ci++)
        {
            HCLASS cl = HCLASSES.ALL()[ci];
            for (int ri = 0; ri < RACES.all().Count; ri++)
            {
                Race r = RACES.all()[ri];
                int am = (int)(GetDD(cl, r) * stats.popDivider(cl, r, 0) * DataDivider());
                stats.inc(cl, r, am - stats.data(cl).get(r));
            }
        }
    }
}