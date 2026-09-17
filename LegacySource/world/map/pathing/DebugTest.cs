using System;
using System.Collections.Generic;

namespace World.Map.Pathing
{
    public class DebugTest
    {
        public DebugTest()
        {
            ArrayCooShort coos = new ArrayCooShort(WORLD.TAREA());
            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.PATH().map.is.is(c))
                {
                    coos.Get().Set(c);
                    coos.Inc();
                }
            }

            int max = coos.GetI();
            Test("mixed", coos, max);

            coos.Set(0);
            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                Region r = WORLD.REGIONS().map.Get(c);
                if (r != null && WORLD.PATH().map.is.is(c) && c.IsSameAs(r.Cx(), r.Cy()))
                {
                    coos.Get().Set(c);
                    coos.Inc();
                }
            }

            Test("capitols", coos, max);
        }

        private void Test(string title, ArrayCooShort coos, int max)
        {
            LOG.Ln(title);
            Test("simple", Simple, coos, max, 1000);
            Test("normal", Make, coos, max, 1000);
            Test("fancy", MakeFancy, coos, max, 1000);
        }

        private void Test(string title, TEST test, ArrayCooShort coos, int max, int amount)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int fails = 0;
            int diff1 = 0;
            int diff2 = 0;
            for (int i = 0; i < amount; i++)
            {
                coos.Set(RND.rInt(max));
                int sx = coos.Get().X();
                int sy = coos.Get().Y();

                coos.Set(RND.rInt(max));
                int dx = coos.Get().X();
                int dy = coos.Get().Y();

                PathTile t = test.Make(WORLD.WATER().Is(sx, sy), sx, sy, dx, dy);
                if (t == null)
                {
                    fails++;
                }
                else
                {
                    if (!t.IsSameAs(dx, dy))
                        diff1++;

                    while (t.Parent != null)
                    {
                        t = t.Parent;
                    }

                    if (!t.IsSameAs(sx, sy))
                        diff2++;
                }
            }
            LOG.Ln(title + " " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - now) + " " + fails + " " + diff1 + " " + diff2);
        }

        public interface TEST
        {
            PathTile Make(bool isShip, int fromX, int fromY, int tox, int toy);
        }

        public readonly TEST Simple = new TEST()
        {
            public PathTile Make(bool isShip, int fromX, int fromY, int tox, int toy)
            {
                if (!WORLD.PATH().map.is.is(fromX, fromY) || !WORLD.PATH().map.is.is(tox, toy))
                {
                    return null;
                }

                GUTIL.Flooder().Init(typeof(WPATHING));
                GUTIL.Flooder().PushSloppy(fromX, fromY, 0);
                while (GUTIL.Flooder().HasMore())
                {
                    PathTile t = GUTIL.Flooder().PollSmallest();
                    if (t.IsSameAs(tox, toy))
                    {
                        GUTIL.Flooder().Done();
                        return t;
                    }
                    for (int di = 0; di < DIR.ALL.Size; di++)
                    {
                        DIR d = DIR.ALL.Get(di);
                        int dx = t.X() + d.X();
                        int dy = t.Y() + d.Y();
                        if (WORLD.PATH().map.is.is(dx, dy))
                            GUTIL.Flooder().PushSmaller(dx, dy, t.Value + d.TileDistance() * WPATHING.Cost(t.X(), t.Y(), d), t);
                    }
                }
                GUTIL.Flooder().Done();
                return null;
            }
        };

        public readonly TEST Make = new TEST()
        {
            public PathTile Make(bool isShip, int fromX, int fromY, int tox, int toy)
            {
                if (!WORLD.PATH().map.is.is(fromX, fromY) || !WORLD.PATH().map.is.is(tox, toy))
                    return null;

                GUTIL.Flooder().Init(typeof(WPATHING));
                GUTIL.Flooder().PushSloppy(fromX, fromY, 0);
                while (GUTIL.Flooder().HasMore())
                {
                    PathTile t = GUTIL.Flooder().PollSmallest();
                    if (t.IsSameAs(tox, toy))
                    {
                        GUTIL.Flooder().Done();
                        return t;
                    }
                    Process(t);
                }
                GUTIL.Flooder().Done();
                return null;
            }

            private void Process(PathTile t)
            {
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    int dx = t.X() + d.X();
                    int dy = t.Y() + d.Y();
                    if (WORLD.PATH().map.is.is(dx, dy))
                    {
                        if (WORLD.WATER().IsBig.Is(t))
                        {
                            if (WORLD.WATER().CanTravelToByBoat(t.X(), t.Y(), d) || WORLD.PATH().map.is.is(dx, dy, d))
                                GUTIL.Flooder().PushSmaller(dx, dy, t.Value + d.TileDistance() * WPATHING.Cost(t.X(), t.Y(), d), t);
                        }
                        else
                        {
                            if (WORLD.WATER().IsBig.Is(dx, dy))
                            {
                                if (WORLD.PATH().map.is.is(dx, dy))
                                    GUTIL.Flooder().PushSmaller(dx, dy, t.Value + d.TileDistance() + WTRAV.PORT_PENALTY, t);
                            }
                            else
                            {
                                GUTIL.Flooder().PushSmaller(dx, dy, t.Value + d.TileDistance() * WPATHING.Cost(t.X(), t.Y(), d), t);
                            }
                        }
                    }
                }
            }
        };

        public readonly TEST MakeFancy = new TEST()
        {
            public PathTile Make(bool isShip, int fromX, int fromY, int tox, int toy)
            {
                PathTile t = WORLD.PATH().Path(fromX, fromY, tox, toy);
                if (t == null)
                {
                    LOG.Ln(fromX + " " + fromY + " " + tox + " " + toy);
                }
                return t;
            }
        };
    }
}