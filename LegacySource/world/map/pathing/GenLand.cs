using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using world;
using world.map.regions;
using world.map.road;

namespace world.map.pathing
{
    sealed class GenLand
    {
        private readonly Bitmap2D tmp = new Bitmap2D(WORLD.TBOUNDS(), false);
        private readonly ACTION u;

        public GenLand(ACTION util)
        {
            this.u = util;

            Flooder f = GUTIL.flooder();
            f.Init(this);

            LinkedList<COORDINATE> nodes = new LinkedList<COORDINATE>();

            foreach (Region r in WORLD.REGIONS().all())
            {
                if (r.info.area() > 0)
                {
                    f.PushSloppy(r.cx(), r.cy(), 0, null);
                    f.SetValue2(r.cx(), r.cy(), r.index());
                }
            }

            tmp.Clear();
            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                if (t.GetParent() != null)
                    t.SetValue2(t.GetParent().GetValue2());

                Region rr = WORLD.REGIONS().map.get(t);

                foreach (DIR d in DIR.ALL)
                {
                    if (WTRAV.can(t.x(), t.y(), d, true))
                    {
                        Region other = WORLD.REGIONS().map.get(t, d);
                        if (other != rr)
                        {
                            continue;
                        }
                        f.PushSmaller(t, d, t.GetValue() + d.tileDistance(), t);
                    }
                }
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (f.HasBeenPushed(c.x(), c.y()))
                {
                    int from = (int)f.GetValue2(c.x(), c.y());

                    if (WORLD.ROADS().harbour.is(c))
                    {
                        Gen.connect(f.Get(c.x(), c.y()));
                    }
                    else if (WORLD.WATER().isBig.is(c))
                        continue;
                    else
                    {
                        foreach (DIR d in DIR.ALL)
                        {
                            if (f.HasBeenPushed(c.x(), c.y(), d) && WTRAV.canLand(c.x(), c.y(), d, true))
                            {
                                int regTo = (int)f.GetValue2(c.x(), c.y(), d);
                                if (from != regTo)
                                {
                                    markAdd(f.Get(c.x(), c.y()), nodes);
                                    markAdd(f.Get(c.x() + d.x(), c.y() + d.y()), nodes);

                                }
                            }
                        }
                    }
                }
            }
            u.exe();
            f.Done();
            int i = 0;
            while (!nodes.isEmpty())
            {
                if (i++ % 10 == 0)
                    u.exe();
                f.Init(this);
                Region start = WORLD.REGIONS().map.get(nodes.removeFirst());
                Region end = WORLD.REGIONS().map.get(nodes.removeFirst());
                f.PushSloppy(start.cx(), start.cy(), 0);

                while (f.HasMore())
                {
                    PathTile t = f.PollSmallest();
                    if (t.IsSameAs(end.cx(), end.cy()))
                    {
                        Gen.connect(t);
                        break;
                    }

                    foreach (DIR d in DIR.ALL)
                    {
                        if (tmp.Is(t.x(), t.y(), d) && WTRAV.can(t.x(), t.y(), d, true))
                        {
                            Region other = WORLD.REGIONS().map.get(t, d);
                            if (other != start && other != end)
                            {
                                continue;
                            }
                            double v = WORLD.PATH().map.can(t.x(), t.y(), d) ? 0.5 : 1;
                            f.PushSmaller(t, d, t.GetValue() + v * d.tileDistance(), t);
                        }
                    }
                }
                f.Done();

            }
            util.exe();
        }

        private void markAdd(PathTile t, LinkedList<COORDINATE> li)
        {
            while (t != null)
            {
                tmp.Set(t, true);
                if (t.GetParent() == null)
                    li.Add(t);
                t = t.GetParent();
            }
        }
    }
}