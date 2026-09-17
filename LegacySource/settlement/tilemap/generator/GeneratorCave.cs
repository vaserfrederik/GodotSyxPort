using System;
using System.Collections.Generic;
using snake2d;
using util;

class GeneratorCave
{
    private readonly PathGame.PathFancy p = new PathGame.PathFancy(5000);
    private readonly int caveSize;
    private readonly double tunnels;

    public GeneratorCave(CapitolArea area, GeneratorUtil util, LinkedList<COORDINATE> caves)
    {
        int amount = (int)(util.json.d("CAVE_AMOUNT", 0, 1.0) * 300);
        caveSize = (int)(util.json.d("CAVE_SIZE", 0, 1.0) * 30);
        tunnels = util.json.d("CAVE_TUNNELS", 0, 1);

        for (int i = 0; i < amount; i++)
        {
            int x = RND.rInt(SETT.TWIDTH);
            int y = RND.rInt(SETT.THEIGHT);
            if (Cave(area, util, x, y))
            {
                caves.Add(new Coo(x, y));
            }
        }

        GUTIL.flooder().Init(this);
        for (int y = 0; y < SETT.THEIGHT; y++)
        {
            for (int x = 0; x < SETT.TWIDTH; x++)
            {
                if (TERRAIN().CAVE.is(x, y))
                    GUTIL.flooder().PushSloppy(x, y, 0);
            }
        }

        while (GUTIL.flooder().HasMore())
        {
            PathTile t = GUTIL.flooder().PollSmallest();
            TERRAIN().CAVE.placeRaw(t.x(), t.y());

            foreach (DIR d in DIR.ORTHO)
            {
                if (!IN_BOUNDS(t, d))
                    continue;
                if (TERRAIN().MOUNTAIN.is(t, d))
                {
                    if (RND.oneIn(3))
                    {
                        GUTIL.flooder().PushSmaller(t, d, 1);
                    }
                    else
                    {
                        GUTIL.flooder().Close(t, d, 0);
                    }
                }
            }
        }
        GUTIL.flooder().Done();
    }

    private bool Cave(CapitolArea area, GeneratorUtil util, int x, int y)
    {
        GUTIL.flooder().Init(this);
        LinkedList<Coo> coos = new LinkedList<Coo>();

        util.polly.CheckInit();
        util.polly.Checker.Set(x, y, true);

        GUTIL.flooder().PushSloppy(x, y, 0);

        for (int i = RND.rInt(caveSize); i > 0; i--)
        {
            int x2 = x + RND.rInt0(10 + caveSize);
            int y2 = y + RND.rInt0(10 + caveSize);
            if (SETT.IN_BOUNDS(x2, y2))
            {
                if (!util.polly.Checker.Is(x2, y2))
                {
                    util.polly.Checker.Set(x2, y2, true);
                    GUTIL.flooder().PushSloppy(x2, y2, 0);
                    coos.Add(new Coo(x2, y2));
                }
            }
        }

        while (GUTIL.flooder().HasMore())
        {
            PathTile t = GUTIL.flooder().PollSmallest();
            if (!TERRAIN().MOUNTAIN.is(t))
            {
                GUTIL.flooder().Done();
                return false;
            }

            foreach (DIR d in DIR.ORTHO)
            {
                if (!IN_BOUNDS(t, d))
                    continue;
                if (!util.polly.Checker.Is(t, d))
                {
                    if (t.GetValue() < 5)
                        GUTIL.flooder().PushSmaller(t, d, t.GetValue() + d.tileDistance());
                }
                else
                {
                    GUTIL.flooder().PushSmaller(t, d, 0);
                }
            }
        }
        GUTIL.flooder().Done();
        GUTIL.flooder().Init(this);
        foreach (Coo c in coos)
        {
            GUTIL.flooder().PushSloppy(c, 0);
        }

        while (GUTIL.flooder().HasMore())
        {
            PathTile t = GUTIL.flooder().PollSmallest();
            if (TERRAIN().MOUNTAIN.is(t))
            {
                TERRAIN().CAVE.placeRaw(t.x(), t.y());
            }

            foreach (DIR d in DIR.ORTHO)
            {
                if (!IN_BOUNDS(t, d))
                    continue;
                if (util.polly.Checker.Is(t, d))
                    GUTIL.flooder().PushSmaller(t, d, 0);
            }
        }
        GUTIL.flooder().Done();

        int a = RND.rInt((int)(1 + coos.Size * tunnels * 10));

        for (int i = 0; i < a; i++)
        {
            Coo c = coos.RemoveFirst();
            Tunnel(area, util, c.x(), c.y());
            coos.Add(c);
        }

        return true;
    }

    private void Tunnel(CapitolArea area, GeneratorUtil util, int startX, int startY)
    {
        PathGame.COST cm = new PathGame.COST()
        {
            getCost = (fromX, fromY, toX, toY) =>
            {
                if (!IN_BOUNDS(toX, toY))
                    return -1;

                if (!util.polly.IsEdge(toX, toY))
                    return 5;

                if (TERRAIN().MOUNTAIN.is(toX, toY))
                    return 1;

                if (TERRAIN().CAVE.is(toX, toY))
                    return 2;

                return 0;
            }
        };

        PathGame.DEST dm = new PathGame.DEST()
        {
            isDest = (x, y) =>
            {
                if (TERRAIN().CAVE.is(x, y))
                {
                    int d = Math.Abs(x - startX);
                    d += Math.Abs(y - startY);
                    return d > 60;
                }
                return !TERRAIN().MOUNTAIN.is(x, y);
            },

            getOptDistance = (x, y) => 0
        };

        if (!GUTIL.astar().GetNearest(p, cm, dm, startX, startY))
        {
            return;
        }

        GUTIL.flooder().Init(this);

        int max = 60 + RND.rInt(60);

        do
        {
            int x = p.x();
            int y = p.y();
            GUTIL.flooder().PushSmaller(x, y, 0);
        } while (p.SetNext() && max-- >= 0);

        while (GUTIL.flooder().HasMore())
        {
            PathTile t = GUTIL.flooder().PollGreatest();
            if (TERRAIN().MOUNTAIN.is(t))
                TERRAIN().CAVE.placeRaw(t.x(), t.y());
            if (TERRAIN().MOUNTAIN.is(t, DIR.E))
                TERRAIN().CAVE.placeRaw(t.x() + 1, t.y());
        }

        GUTIL.flooder().Done();
    }
}