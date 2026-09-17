using System;
using System.Collections.Generic;
using settlement.main;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.path.components;
using settlement.stats;

public sealed class SFinderHumanoid
{
    private Humanoid asker;
    private FindableDataSingle ti;
    private Humanoid res;
    private bool hostile;

    private readonly SFINDER fin = new SFINDER
    {
        IsInComponent = (c, distance) =>
        {
            int am = ti.Get(c);
            // for (DIR d: DIR.ALLC)
            //     if (c.level().get(asker.tc(), d) == c) {
            //         am --;
            //         break;
            //     }
            return am > 0;
        },
        IsTile = (tx, ty, tileNr) =>
        {
            foreach (ENTITY e in SETT.ENTITIES().GetAtTile(tx, ty))
            {
                if (e is Humanoid && e != asker)
                {
                    res = (Humanoid)e;
                    if (res.Indu().HType().IsHostile() == hostile)
                        return true;
                }
            }
            return false;
        }
    };

    // {
    //     new TestPath("other people", fin) {
    //         
    //         protected void place(int sx, int sy, SPath p) {
    //             asker = null;
    //             ti = PATH().comps.data.people(true);
    //             base.place(sx, sy, p);
    //         }
    //         
    //     };
    // }

    public bool EnemiesAreNear(Humanoid client)
    {
        if (SETT.INVADOR().Invading() || STATS.POP().Pop(HTYPES.ENEMY()) > STATS.POP().POP.data().Get(null) * 0.1)
        {
            SComponent ss = SETT.PATH().comps.levels.Get(0).Get(client.Tc());
            if (ss == null)
                return false;
            if (PATH().comps.data.people(client.Indu().Hostile()).Get(ss) > 0)
                return true;
            SComponentEdge e = ss.EdgeFirst();
            while (e != null)
            {
                if (PATH().comps.data.people(client.Indu().Hostile()).Get(e.To()) > 0)
                    return true;
                e = e.Next();
            }
        }

        return false;
    }

    public Humanoid Find(Humanoid client, int radius)
    {
        asker = client;
        ti = PATH().comps.data.people(!client.Indu().Hostile());

        if (STATS.POP().POP.data(null).Get(null, 0) < 2)
            return null;

        ti.ReportAbsence(client.Ssx(), client.Ssy());

        hostile = client.Indu().Hostile();

        if (SETT.PATH().finders.finder().Find(client.Tc().X(), client.Tc().Y(), fin, radius) != null)
        {
            ti.ReportPresence(client.Ssx(), client.Ssy());
            return res;
        }

        ti.ReportPresence(client.Ssx(), client.Ssy());

        return null;
    }

    public Humanoid Enemy(Humanoid client, int radius)
    {
        asker = client;
        ti = PATH().comps.data.people(client.Indu().Hostile());
        hostile = !client.Indu().Hostile();
        if (SETT.PATH().finders.finder().Find(client.Tc().X(), client.Tc().Y(), fin, radius) != null)
        {
            return res;
        }

        return null;
    }

    public SComponent FindComp(Humanoid client, int radius)
    {
        asker = client;
        ti = PATH().comps.data.people(!client.Indu().Hostile());
        ti.ReportAbsence(client.Ssx(), client.Ssy());
        SComponent s = SETT.PATH().comps.pather.Get(client.Ssx(), client.Ssy(), fin, radius);
        ti.ReportPresence(client.Ssx(), client.Ssy());
        return s;
    }
}