using System;
using System.Collections.Generic;
using System.Linq;
using init.paths;
using init.race;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.keymap;

public static class TRAITS
{
    private static TRAITS s;
    private readonly RMAPS<TRAIT> map;
    private int[] iorder;
    private bool[] idis;
    private readonly ArrayList<TRAIT> tmp;

    private Induvidual sortI;
    private readonly Tree<TRAIT> sort;

    static TRAITS()
    {
        s = new TRAITS();
    }

    private TRAITS()
    {
        ResFolder f = PATHS.RACE().folder("trait");
        string[] keys = f.init.getFiles();
        Json[] js = new Json[keys.Length];

        ArrayList<TRAIT> all = new ArrayList<TRAIT>(keys.Length);

        for (int i = 0; i < keys.Length; i++)
        {
            js[i] = new Json(f.init.gets(keys[i]));
            new TRAIT(all, keys[i], js[i], new Json(f.text.gets(keys[i])));
        }

        map = new RMAPS<TRAIT>("TRAIT", all);

        for (int i = 0; i < keys.Length; i++)
        {
            TRAIT tt = map.all().get(i);
            foreach (TRAIT o in map.readMany("DISABLES_OTHERS", js[i]))
            {
                if (o != tt)
                    tt.disables.add(o);
            }
        }

        iorder = Alloc.ii(map.all().size());
        idis = new bool[map.all().size()];
        tmp = new ArrayList<TRAIT>(map.all().size());
        sort = new Tree<TRAIT>(map.all().size())
        {
            protected override bool isGreaterThan(TRAIT current, TRAIT cmp)
            {
                return current.get(sortI) > cmp.get(sortI);
            }
        };
    }

    public static RMAP<TRAIT> MAP()
    {
        return s.map;
    }

    public static LIST<TRAIT> ALL()
    {
        return s.map.all();
    }

    public static void serRaceData(Race race, Json json)
    {
        s.map.new KJson(json)
        {
            protected override void process(TRAIT s, Json j, string key, bool isWeak)
            {
                s.occRaces[race.index] = j.d(key, 0, 1);
            }
        };
    }

    public static void init(Induvidual invid)
    {
        for (int i = 0; i < s.iorder.Length; i++)
        {
            s.iorder[i] = i;
        }
        for (int i = 0; i < s.iorder.Length; i++)
        {
            int o = s.iorder[i];
            int ii = RND.rInt(s.iorder.Length);
            s.iorder[i] = s.iorder[ii];
            s.iorder[ii] = o;
        }
        Array.Fill(s.idis, false);

        foreach (int i in s.iorder)
        {
            TRAIT t = s.map.all().get(i);
            if (s.idis[i] || RND.rFloat() > t.occRaces[invid.race().index()])
            {
                STATS.TRAITS().stat(t).setD(invid, 0);
                continue;
            }
            double v = RND.rFloat();

            STATS.TRAITS().stat(t).setD(invid, v);
            foreach (TRAIT o in t.disables)
                s.idis[o.index()] = true;
        }
    }

    public static LIST<TRAIT> tmp(Induvidual i, int max)
    {
        s.sort.clear();
        s.sortI = i;
        foreach (TRAIT t in s.map.all())
        {
            if (t.get(i) > 0)
                s.sort.add(t);
        }
        s.tmp.clearSloppy();
        while (s.sort.hasMore())
        {
            s.tmp.add(s.sort.pollGreatest());
        }
        return s.tmp;
    }
}