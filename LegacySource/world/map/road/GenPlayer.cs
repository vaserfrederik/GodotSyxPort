using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using world;
using world.map.regions;

namespace world.map.road
{
    sealed class GenPlayer
    {
        private readonly RegGroup[] table = new RegGroup[WREGIONS.MAX];

        public GenPlayer(ACTION aa)
        {
            Flooder f = GUTIL.flooder();
            f.init(this);
            foreach (Region reg in WORLD.REGIONS().all())
            {
                if (reg.info.area() > 0)
                {
                    table[reg.index()] = new RegGroup(reg);
                    f.pushSloppy(reg.cx(), reg.cy(), 0);
                    f.setValue2(reg.cx(), reg.cy(), reg.index());
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getParent() != null)
                    t.setValue2(t.getParent().getValue2());
                Region home = WORLD.REGIONS().getByIndex((int)t.getValue2());
                Region now = WORLD.REGIONS().map.get(t);

                if (now == home || now == null)
                {
                    foreach (DIR d in DIR.ALL)
                    {
                        if (WTRAV.can(t.x(), t.y(), d, true))
                        {
                            Region to = WORLD.REGIONS().map.get(t, d);
                            if (home == to || to == null)
                                f.pushSmaller(t, d, t.getValue() + d.tileDistance(), t);
                        }
                    }
                }
            }

            aa.exe();

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (f.hasBeenPushed(c.x(), c.y()))
                {
                    int from = (int)f.getValue2(c.x(), c.y());

                    foreach (DIR d in DIR.ALL)
                    {
                        if (f.hasBeenPushed(c.x(), c.y(), d) && WTRAV.can(c.x(), c.y(), d, true))
                        {
                            int regTo = (int)f.getValue2(c.x(), c.y(), d);
                            if (from != regTo)
                            {
                                join(from, regTo);
                            }
                        }
                    }
                }
            }

            f.done();
            aa.exe();
            for (int i = 0; i < table.Length; i++)
            {
                if (!join())
                    break;
                aa.exe();
            }
        }

        private bool join()
        {
            RegGroup g = null;

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != null && table[i] != g)
                {
                    if (g != null)
                    {
                        path(g, table[i]);
                        join(g, table[i]);
                        return true;
                    }
                    else
                    {
                        g = table[i];
                    }
                }
            }

            return false;
        }

        public void join(int a, int b)
        {
            RegGroup ga = table[a];
            RegGroup gb = table[b];
            join(ga, gb);
        }

        public void join(RegGroup ga, RegGroup gb)
        {
            if (ga == gb)
                return;

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != null && table[i] == gb)
                {
                    table[i] = ga;
                }
            }
            ga.regs.add(gb.regs);
        }

        public void path(RegGroup ga, RegGroup gb)
        {
            Flooder f = GUTIL.flooder();
            f.init(this);

            foreach (Region r in ga.regs)
            {
                f.pushSloppy(r.cx(), r.cy(), 0, null);
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                Region now = WORLD.REGIONS().map.get(t);
                if (now != null && t.isSameAs(now.cx(), now.cy()) && table[now.index()] != ga)
                {
                    join(ga, table[now.index()]);
                    WTRAV.makeRoad(t);
                    f.done();
                    return;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WTRAV.can(t.x(), t.y(), d, false))
                    {
                        if (now == null || table[now.index()] == ga || now == WORLD.REGIONS().map.get(t, d))
                        {
                            double v = d.tileDistance();
                            if (!WTRAV.can(t.x(), t.y(), d, true))
                                v *= 32;
                            f.pushSmaller(t, d, t.getValue() + v, t);
                        }
                    }
                }
            }
            f.done();
        }

        private sealed class RegGroup
        {
            public readonly ArrayListGrower<Region> regs = new ArrayListGrower<>();

            public RegGroup(Region reg)
            {
                regs.add(reg);
            }
        }
    }
}