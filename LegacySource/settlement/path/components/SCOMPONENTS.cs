using System;
using System.Collections.Generic;
using settlement.main;
using settlement.path.components.finder;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util;
using view.sett;

public sealed class SCOMPONENTS
{
    public readonly FindableDatas data = new FindableDatas();
    public readonly SComp0Level zero = new SComp0Level();
    public readonly LIST<SCompNLevel> levels;
    public readonly SCompNLevel last;
    public readonly LIST<SComponentLevel> all;
    public readonly SCompFinder pather;

    private bool debug = false;

    public SCOMPONENTS()
    {
        const int increase = 8;

        int ls = 0;
        int ss = SComp0Level.SIZE * increase;
        while (ss <= SETT.TWIDTH)
        {
            ss *= increase;
            ls++;
        }
        ls++;
        ArrayList<SComponentLevel> all = new ArrayList<SComponentLevel>(ls + 1);
        all.Add(zero);
        ArrayList<SCompNLevel> levels = new ArrayList<SCompNLevel>(ls);
        ss = SComp0Level.SIZE * increase;
        ls = 1;
        while (levels.HasRoom())
        {
            SCompNLevel l = new SCompNLevel(all.Get(all.Size() - 1), ls, ss);
            levels.Add(l);
            all.Add(l);
            ss *= increase;
            if (ss > SETT.TWIDTH)
                ss = SETT.TWIDTH;
            ls++;
        }
        this.levels = levels;
        last = levels.Get(levels.Size() - 1);
        this.all = all;

        pather = new SCompFinder(this, GUTIL.pathTools());

        new SCompTests(this);
        new SCompUI(this);

        IDebugPanelSett.Add("comp debug", new ACTION()
        {
            public void Exe()
            {
                debug = !debug;
            }
        });
    }

    public void Clear()
    {
        foreach (SComponentLevel l in all)
            l.Init();
    }

    public void Init()
    {
        foreach (SComponentLevel l in all)
            l.Init();
        Update();

        //check();
    }

    public void Update()
    {
        if (debug)
        {
            for (int k = 0; k < 10; k++)
            {
                int x = RND.rInt(SETT.TWIDTH);
                int y = RND.rInt(SETT.THEIGHT);
                DIR d = DIR.ALL.rnd();
                TerrainTile t = RND.rBoolean() ? SETT.TERRAIN().NADA : SETT.TERRAIN().BUILDINGS.all().Get(0).wall;

                for (int i = 0; i < 8; i++)
                {
                    int tx = x + d.x() * i;
                    int ty = y + d.y() * i;
                    if (SETT.IN_BOUNDS(tx, ty))
                        t.placeFixed(tx, ty);
                }
            }

            for (int k = 0; k < 10; k++)
            {
                int x = RND.rInt(SETT.TWIDTH);
                int y = RND.rInt(SETT.THEIGHT);
                DIR d = DIR.ALL.rnd();

                for (int i = 0; i < 8; i++)
                {
                    int tx = x + d.x() * i;
                    int ty = y + d.y() * i;
                    if (SETT.IN_BOUNDS(tx, ty))
                        UpdateService(tx, ty);
                }
            }

            foreach (SComponentLevel l in all)
                l.Update();

            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                SComponent co = zero.Get(c);
                while (co != null)
                {
                    SComponentEdge e = co.EdgeFirst();
                    while (e != null)
                    {
                        if (e.To() != e.To().Level().Get(e.To().CentreX(), e.To().CentreY()))
                            LOG.ln("eye!");
                        e = e.Next();
                    }
                    co = co.SuperComp();
                }
            }
        }

        foreach (SComponentLevel l in all)
            l.Update();
    }

    //public void check()
    //{
    //    SComponentChecker check = new SComponentChecker(zero);
    //    check.Init();
    //    foreach (COORDINATE c in SETT.TILE_BOUNDS)
    //    {
    //        SComponent cc = zero.Get(c);
    //        if (cc != null && !check.IsSetAndSet(cc))
    //        {
    //            while (cc != null)
    //            {
    //                if (cc.Level().Get(cc.CentreX(), cc.CentreY()) != cc)
    //                    System.err.println(cc.CentreX() + " " + cc.CentreY() + " " + cc.Level().Level());
    //                cc = cc.SuperComp();
    //            }
    //        }
    //    }
    //}

    public void UpdateAvailability(int tx, int ty)
    {
        zero.Update(tx, ty);
    }

    public void UpdateService(int tx, int ty)
    {
        zero.ChangeSerives(tx, ty);
    }

    public readonly MAP_OBJECT<SComponent> SuperComp = new MAP_OBJECT<SComponent>()
    {
        public SComponent Get(int tile)
        {
            SComponent s = zero.Get(tile);
            while (s != null && s.SuperComp() != null)
                s = s.SuperComp();
            return s;
        }

        public SComponent Get(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
                return Get(tx + ty * SETT.TWIDTH);
            return null;
        }
    };
}