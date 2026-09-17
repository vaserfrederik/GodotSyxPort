using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    using static Settlement.Main.SETT.PATH;

    using Settlement.Main;
    using Settlement.Path;
    using Snake2D.PathTile;
    using Snake2D.Util.Datatypes;
    using Util;

    internal sealed class SComp0Updater
    {
        private readonly SComp0Map map;
        private readonly SComp0Factory factory;
        private readonly Rec bounds = new Rec();
        private readonly SComponentChecker checker;
        private bool[][] assigned = new bool[SComp0Level.SIZE][];

        public SComp0Updater(SComp0Map map, SComp0Factory f, SComp0Level c)
        {
            this.map = map;
            this.factory = f;
            checker = new SComponentChecker(c);
            for (int i = 0; i < SComp0Level.SIZE; i++)
            {
                assigned[i] = new bool[SComp0Level.SIZE];
            }
        }

        public void Remove(RECTANGLE r, SComp0Quads sComp0Quads)
        {
            for (int y = r.y1(); y < r.y2(); y++)
            {
                for (int x = r.x1(); x < r.x2(); x++)
                {
                    SComp0 c = map.Get(x, y);
                    if (c != null && !c.Retired())
                    {
                        factory.Retire(c);
                    }
                    map.Set(x, y, null);
                }
            }
        }

        public void RemoveSuperComp(RECTANGLE r, SComp0Quads sComp0Quads)
        {
            for (int y = r.y1(); y < r.y2(); y++)
            {
                for (int x = r.x1(); x < r.x2(); x++)
                {
                    SComp0 c = map.Get(x, y);
                    if (c != null && c.SuperComp() != null)
                    {
                        SETT.PATH().Comps.Levels.Get(0).Remove(c.SuperComp());
                    }
                }
            }
        }

        public void Assign(RECTANGLE r, SComp0Quads sComp0Quads)
        {
            for (int y = r.y1(); y < r.y2(); y++)
            {
                for (int x = r.x1(); x < r.x2(); x++)
                {
                    assigned[y - r.y1()][x - r.x1()] = false;
                }
            }

            for (int y = r.y1(); y < r.y2(); y++)
            {
                for (int x = r.x1(); x < r.x2(); x++)
                {
                    if (!assigned[y - r.y1()][x - r.x1()])
                        Assign(r, x, y);
                }
            }
        }

        private void Assign(RECTANGLE r, int tx, int ty)
        {
            if (map.Get(tx, ty) != null)
                return;

            AVAILABILITY a = PATH().Availability.Get(tx, ty);
            if (a.Player < 0)
                return;

            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy(tx, ty, 0);
            SComp0 c = factory.Create();
            bounds.Clear();
            int size = 0;

            bool other = false;

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                map.Set(t, c);
                assigned[t.Y() - r.y1()][t.X() - r.x1()] = true;
                size++;
                bounds.Unify(t.X(), t.Y());
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    int dx = t.X() + DIR.ALL.Get(di).X();
                    int dy = t.Y() + DIR.ALL.Get(di).Y();
                    if (PATH().Coster.Player.GetCost(t.X(), t.Y(), dx, dy) > 0)
                    {
                        if (r.HoldsPoint(dx, dy))
                        {
                            if (map.Get(dx, dy) == null)
                            {
                                double v = t.GetValue() + DIR.ALL.Get(di).TileDistance();
                                GUTIL.Flooder().PushSmaller(dx, dy, v);
                            }
                        }
                        else if (!other)
                            other = true;
                    }
                }
            }

            GUTIL.Flooder().Done();

            if (!other)
            {
                for (int y = bounds.y1(); y < bounds.y2(); y++)
                {
                    for (int x = bounds.x1(); x < bounds.x2(); x++)
                    {
                        if (map.Get(x, y) == c)
                        {
                            map.Set(x, y, null);
                        }
                    }
                }
                factory.Retire(c);
                return;
            }

            c.Init(bounds, size, checker);

            if (c.Edgefirst() != null && c.Edgefirst().Next() == null && size <= r.Width() * r.Height() / 2)
            {
            }

            SETT.PATH().Comps.Data.InitComponent0(c, bounds);
            SETT.PATH().Comps.Levels.Get(0).AddNew(c);
        }

        public void InitData(RECTANGLE r)
        {
            checker.Init();

            for (int y = r.y1(); y < r.y2(); y++)
            {
                for (int x = r.x1(); x < r.x2(); x++)
                {
                    SComp0 c = map.Get(x, y);
                    if (c != null && !checker.IsSetAndSet(c))
                    {
                        SETT.PATH().Comps.Levels.Get(0).Remove(c.SuperComp());
                        SETT.PATH().Comps.Levels.Get(0).AddNew(c);
                        SETT.PATH().Comps.Data.InitComponent0(c, r);
                    }
                }
            }
        }
    }
}