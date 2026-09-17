using System;
using init.race;
using init.type;
using settlement.main;
using world.army;

public class POP
{
    public static int Tot(HCLASS c, Race r)
    {
        return Others(c, r) + Physical(c, r);
    }

    public static int Tot(Race r)
    {
        return Tot(null, r);
    }

    public static int Tot()
    {
        return Tot(null, null);
    }

    public static int Physical(HCLASS c, Race r)
    {
        int pop = Pop(c, r);
        if (c == null || c == HCLASSES.CITIZEN())
        {
            pop += STATS.POP().Pop(r, HTYPES.CHILD());
            pop += STATS.POP().Pop(r, HTYPES.RIOTER());
            pop += STATS.POP().Pop(r, HTYPES.DERANGED());
            pop += STATS.LAW().Criminals(HCLASSES.CITIZEN(), r);
        }
        if (c == null || c == HCLASSES.SLAVE())
        {
            pop += STATS.POP().Pop(r, HTYPES.CHILD_SLAVE());
            pop += STATS.LAW().Criminals(HCLASSES.SLAVE(), r);
        }

        return pop;
    }

    public static int Others(HCLASS c, Race r)
    {
        int pop = 0;
        if (c == null || c == HCLASSES.CITIZEN())
        {
            pop += AD.CityDivs().Total(r);
        }

        return pop;
    }

    public static int Next(HCLASS c, Race r)
    {
        int pop = Tot(c, r);
        if (c == null || c == HCLASSES.CITIZEN())
        {
            pop += STATS.POP().POP.Type().Get(HTYPE_RACE.Get(r, HTYPES.PARENT()));
            pop += SETT.ENTRY().OnTheirWay(r, HTYPES.SUBJECT());
        }
        if (c == null || c == HCLASSES.SLAVE())
        {
            pop += STATS.POP().POP.Type().Get(HTYPE_RACE.Get(r, HTYPES.PARENT_SLAVE()));

            pop += SETT.ENTRY().OnTheirWay(r, HTYPES.SLAVE());
        }
        return pop;
    }

    public static int Incoming(HCLASS c, Race r)
    {
        int pop = 0;
        if (c == null || c == HCLASSES.CITIZEN())
        {
            pop += STATS.POP().POP.Type().Get(HTYPE_RACE.Get(r, HTYPES.PARENT()));
            pop += SETT.ENTRY().OnTheirWay(r, HTYPES.SUBJECT());
        }
        if (c == null || c == HCLASSES.SLAVE())
        {
            pop += STATS.POP().POP.Type().Get(HTYPE_RACE.Get(r, HTYPES.PARENT_SLAVE()));

            pop += SETT.ENTRY().OnTheirWay(r, HTYPES.SLAVE());
        }
        return pop;
    }

    public static int Next(Race r)
    {
        return Next(null, r);
    }

    public static int Next()
    {
        return Next(null, null);
    }

    public static int Pop(HCLASS c, Race r)
    {
        return STATS.POP().POP.Data(c).Get(r);
    }

    public static int Pop(Race r)
    {
        return Tot(null, r);
    }

    public static int Pop()
    {
        return Tot(null, null);
    }
}