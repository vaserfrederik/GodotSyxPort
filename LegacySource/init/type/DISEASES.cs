using System;
using System.Collections.Generic;
using init.paths;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.keymap;

public class DISEASES
{
    private readonly LIST<DISEASE> all;
    private static DISEASES s;
    private readonly RMAP<DISEASE> map;
    private readonly double regularDays;

    DISEASES()
    {
        s = this;
        PATH pd = PATHS.INIT().getFolder("disease");
        PATH ps = PATHS.TEXT().getFolder("disease");
        regularDays = new Json(pd.gets("_CONFIG")).d("REGULAR_SICKNESS_DAY_INVERVAL", 1, 10000000);
        LinkedList<DISEASE> all = new LinkedList<DISEASE>();

        foreach (string k in pd.getFiles(1, 120))
        {
            new DISEASE(all, k, new Json(pd.gets(k)), new Json(ps.gets(k)));
        }

        this.all = new ArrayList<DISEASE>(all);

        map = new RMAP<DISEASE>("DISEASE", all);
    }

    public static LIST<DISEASE> all()
    {
        return s.all;
    }

    public static RMAP<DISEASE> map()
    {
        return s.map;
    }

    public static double regularDays()
    {
        return s.regularDays;
    }

    public static DISEASE randomEpidemic(double ran)
    {
        double lim = 0;
        for (int i = 0; i < s.all.size(); i++)
        {
            DISEASE dd = s.all.get(i);
            if (!dd.epidemic)
                continue;
            lim += dd.occurence();
        }
        lim *= ran;
        double d = 0;
        for (int i = 0; i < s.all.size(); i++)
        {
            DISEASE dd = s.all.get(i);
            if (!dd.epidemic)
                continue;
            d += dd.occurence();
            if (d >= lim)
                return dd;
        }
        return s.all.get(s.all.size() - 1);
    }

    public static DISEASE randomRegular()
    {
        double lim = 0;
        for (int i = 0; i < s.all.size(); i++)
        {
            DISEASE dd = s.all.get(i);
            if (!dd.regular)
                continue;
            lim += dd.occurence();
        }
        lim *= RND.rFloat();
        double d = 0;
        for (int i = 0; i < s.all.size(); i++)
        {
            DISEASE dd = s.all.get(i);
            if (!dd.regular)
                continue;
            d += dd.occurence();
            if (d >= lim)
                return dd;
        }
        return s.all.get(s.all.size() - 1);
    }
}