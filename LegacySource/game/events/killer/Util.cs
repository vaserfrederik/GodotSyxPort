using System;
using System.Collections.Generic;
using game.faction;
using init.race;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.rnd;

class Util
{
    public static int PickKiller()
    {
        Humanoid res = null;

        new EntityIterator.Humans()
        {
            protected override bool ProcessAndShouldBreakH(Humanoid h, int ie)
            {
                if (IsGoodKiller(h) != null)
                {
                    res = h;
                    if (RND.rBoolean())
                        return true;
                }
                return false;
            }
        }.Iterate(RND.rInt() & int.MaxValue);
        if (res == null)
            return -1;
        return res.Id();
    }

    public static Humanoid IsGoodKiller(ENTITY e)
    {
        if (e == null)
            return null;
        if (e is Humanoid)
        {
            Humanoid a = (Humanoid)e;
            if (a.Indu().Clas() == HCLASSES.CITIZEN() && a.Race().Playable)
                return a;
        }
        return null;
    }

    public static int PickRace()
    {
        double pop = 0;
        foreach (Race race in RACES.All())
        {
            if (race.Playable)
            {
                pop += STATS.POP().POP.Data.Get(race);
            }
        }

        pop *= RND.rFloat();

        foreach (Race race in RACES.All())
        {
            if (race.Playable)
            {
                pop -= STATS.POP().POP.Data.Get(race);
                if (pop < 0)
                    return race.Index;
            }
        }

        return FACTIONS.Player().Race().Index;
    }
}