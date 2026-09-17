using System;
using System.Collections.Generic;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

public sealed class StatStanding
{
    private readonly StandingDef def;
    private readonly STAT stat;
    public readonly double defaultInput;

    public StatStanding(STAT stat, double defaultInput)
    {
        this.stat = stat;
        def = StandingDef.NONE;
        this.defaultInput = defaultInput;
    }

    public StatStanding(STAT stat, double defaultInput, StandingDef def)
    {
        this.stat = stat;
        this.def = def;
        this.defaultInput = defaultInput;
    }

    public StandingDef Base()
    {
        return def;
    }

    public double GetDismiss(HCLASS c, Race r)
    {
        return GetDismiss(c, r, 0);
    }

    public double GetDismiss(HCLASS c, Race r, int daysback)
    {
        if (r != null)
            return Definition(r).Get(c).dismiss ? Definition(r).Get(c).max : 0;
        double m = 0;
        double p = STATS.POP().POP.data(c).Get(null, daysback);
        if (p == 0)
        {
            for (int ri = 0; ri < RACES.All().Size(); ri++)
            {
                Race ra = RACES.All().Get(ri);
                m += (Definition(ra).Get(c).dismiss ? Definition(ra).Get(c).max : 0);
            }
            return m / RACES.All().Size();
        }

        for (int ri = 0; ri < RACES.All().Size(); ri++)
        {
            Race ra = RACES.All().Get(ri);
            m += (Definition(ra).Get(c).dismiss ? Definition(ra).Get(c).max : 0) * STATS.POP().POP.data(c).Get(ra, daysback);
        }
        return m / p;
    }

    public double Max(HCLASS c, Race r)
    {
        return Max(c, r, 0);
    }

    public double Max(HCLASS c, Race r, int daysback)
    {
        if (r != null)
            return Definition(r).Get(c).max;
        double m = 0;
        double p = STATS.POP().POP.data(c).Get(null, daysback);
        if (p == 0)
        {
            for (int ri = 0; ri < RACES.All().Size(); ri++)
            {
                Race ra = RACES.All().Get(ri);
                m += (Definition(ra).Get(c).max);
            }
            return m / RACES.All().Size();
        }
        for (int ri = 0; ri < RACES.All().Size(); ri++)
        {
            Race ra = RACES.All().Get(ri);
            m += Definition(ra).Get(c).max * STATS.POP().POP.data(c).Get(ra, daysback);
        }
        return m / p;
    }

    public double Normalized(HCLASS c, Race r)
    {
        if (r != null)
            return r.Stats().DefNormalized(c, this);
        double m = 0;
        double p = STATS.POP().POP.data(c).Get(null);
        if (p == 0)
            return 0;
        for (int ri = 0; ri < RACES.All().Size(); ri++)
        {
            Race ra = RACES.All().Get(ri);
            m += ra.Stats().DefNormalized(c, this) * STATS.POP().POP.data(c).Get(ra);
        }
        return m / p;
    }

    public double Def(HCLASS c, Race r)
    {
        if (r != null)
            return Get(c, r, defaultInput);
        double m = 0;
        double p = STATS.POP().POP.data(c).Get(null);
        if (p == 0)
            return 0;
        foreach (Race ra in RACES.All())
        {
            m += Get(c, ra, defaultInput) * STATS.POP().POP.data(c).Get(ra);
        }
        return m / p;
    }

    public double Get(HCLASS c, Race r)
    {
        return Get(c, r, stat.data(c).GetD(r));
    }

    public double GetHistoric(HCLASS c, Race race, int daysBack)
    {
        if (race == null)
        {
            double m = 0;
            double p = STATS.POP().POP.data(c).Get(null, daysBack);
            if (p == 0)
            {
                for (int ri = 0; ri < RACES.All().Size(); ri++)
                {
                    Race ra = RACES.All().Get(ri);
                    m += Definition(ra).Get(c).dismiss ? Definition(ra).Get(c).max : 0;
                }
                return m / RACES.All().Size();
            }

            for (int ri = 0; ri < RACES.All().Size(); ri++)
            {
                Race ra = RACES.All().Get(ri);
                m += Definition(ra).Get(c).dismiss ? Definition(ra).Get(c).max : 0 * STATS.POP().POP.data(c).Get(ra, daysBack);
            }
            return m / p;
        }

        StandingDef def = Definition(race);
        double v = stat.data(c).GetD(race);
        double dd = CLAMP.D(v * def.mul, 0, 1);
        if (def.exp != null)
            return def.exp.Pow(dd);
        return dd;
    }

    public double Get(HCLASS c, Race r, double input)
    {
        StandingDef def = Definition(r);
        double dd = CLAMP.D(input * def.mul, 0, 1);
        if (def.exp != null)
            return def.exp.Pow(dd);
        return dd;
    }

    public StandingDef Definition(Race r)
    {
        return r.Stats().GetDef(stat);
    }

    public StandingDef NONE()
    {
        if (def == null)
            def = new StandingDef(null);
        return def;
    }

    public class StandingDef
    {
        public static readonly StandingDef NONE = new StandingDef(null);

        private readonly StandingData[] data = new StandingData[HCLASSES.ALL().Size()];
        public readonly bool inverted;
        public readonly double mul;
        public readonly double expo;
        public readonly int prio;
        public readonly bool child;

        public StandingDef(Json json, StandingDef def)
        {
            Check(json);
            inverted = json.Bool("INVERTED", def.inverted);
            mul = json.DTry("MULTIPLIER", 0, 10000, def.mul);
            expo = json.DTry("EXPONENT", 0.01, 10, def.expo);
            prio = (int)json.DTry("PRIO", 0, 100000, def.prio);
            child = json.Bool("CHILD", def.child);
            bool dismiss = json.Bool("DISMISS", def.data[0].dismiss);
            foreach (HCLASS c in HCLASSES.ALL())
            {
                this.data[c.Index()] = json.Has(c.key) ? new StandingData(json, c) : def.data[c.Index()];
                this.data[c.Index()].dismiss = dismiss;
            }

            if (expo == 1)
                exp = null;
            else
                exp = new MATH.QuickPOW(expo, 64);
        }

        private static void Check(Json json)
        {
            if (oks == null)
            {
                oks = new KeyMap<int>();
                oks.Put("INVERTED", 1);
                oks.Put("MULTIPLIER", 1);
                oks.Put("EXPONENT", 1);
                oks.Put("PRIO", 1);
                oks.Put("DISMISS", 1);
                oks.Put("CHILD", 1);
                foreach (HCLASS c in HCLASSES.ALL())
                {
                    oks.Put(c.key, 1);
                }
            }
            foreach (string k in json.Keys())
            {
                if (!oks.ContainsKey(k))
                {
                    string a = "";
                    foreach (string s in oks.KeysSorted())
                        a += s + ", ";
                    json.Error(k + " is not a valid STANDING field. Valid: " + a, k);
                }
            }
        }

        private double GetMulled(double v)
        {
            double dd = CLAMP.D(v * mul, 0, 1);
            if (exp != null)
                return exp.Pow(dd);
            return dd;
        }

        public StandingDef(Json json)
        {
            bool dismiss = false;
            if (json == null)
            {
                inverted = false;
                mul = 0;
                expo = 1;
                prio = 1;
                child = false;
            }
            else
            {
                if (json.Has(key))
                    json = json.Json(key);
                //check(json);
                mul = json.Has("MULTIPLIER") ? json.D("MULTIPLIER", 0, 100000) : 1;
                expo = json.DTry("EXPONENT", 0.01, 10, 1);
                prio = (int)json.DTry("PRIO", 0, 100000, 1);
                inverted = json.Has("INVERTED") && json.Bool("INVERTED");
                child = json.Bool("CHILD", false);
                dismiss = json.Has("DISMISS") && json.Bool("DISMISS");
            }
            if (json != null)
            {
                foreach (HCLASS c in HCLASSES.ALL())
                {
                    this.data[c.Index()] = new StandingData(json, c);
                    this.data[c.Index()].dismiss = dismiss;
                }
            }
            else
            {
                foreach (HCLASS c in HCLASSES.ALL())
                {
                    this.data[c.Index()] = new StandingData(0);
                    this.data[c.Index()].dismiss = dismiss;
                }
            }

            if (expo == 1)
                exp = null;
            else
                exp = new MATH.QuickPOW(expo, 64);
        }

        public class StandingData
        {
            public readonly double max;
            public readonly double from;
            public readonly double to;
            public bool dismiss;

            public StandingData(Json json, HCLASS clas)
            {
                this((json == null || !json.Has(clas.key)) ? 0 : json.D(clas.key, 0, 1000000));
            }

            public StandingData(double max)
            {
                this.max = max;
                if (inverted)
                {
                    from = this.max;
                    to = 0;
                }
                else
                {
                    from = 0;
                    to = this.max;
                }
            }
        }

        public StandingData Get(HCLASS c)
        {
            return data[c.Index()];
        }
    }
}