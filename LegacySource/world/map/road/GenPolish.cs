using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using util;
using world;
using world.map.regions;

namespace world.map.road
{
    internal sealed class GenPolish
    {
        public readonly Polymap polly = new Polymap(WORLD.TBOUNDS(), 6, 1);

        public GenPolish(ACTION util, MAP_DOUBLE infra)
        {
            RemoveUnusedRoads();

            foreach (Region r in WORLD.REGIONS().all())
            {
                if (r.info.area() > 0)
                {
                    RandomRoad(r);
                }
            }
        }

        private void RemoveUnusedRoads()
        {
            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.REGIONS().cTile.is(c))
                {
                    continue;
                }
                bool needed = false;
                bool canBeRemoved = false;

                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.ROADS().is(c, d) && WORLD.ROADS().is(c, d.next(2)))
                    {
                        if (!WORLD.ROADS().is(c, d.next(1)) || (WORLD.REGIONS().map.get(c) != null && WORLD.REGIONS().map.get(c) != WORLD.REGIONS().map.get(c, d)))
                        {
                            needed = true;
                            break;
                        }
                        else
                        {
                            canBeRemoved = true;
                        }
                    }
                }
                if (!needed && canBeRemoved)
                {
                    WORLD.ROADS().set(c, false);
                }
            }
        }

        private void RandomRoad(Region r)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(r.info.cx(), r.info.cy(), 0);

            int amount = (int)(r.info.area() * r.info.moisture());

            while (GUTIL.flooder().hasMore() && amount > 0)
            {
                PathTile t = GUTIL.flooder().pollSmallest();

                if (r != WORLD.REGIONS().map.get(t))
                {
                    continue;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.REGIONS().map.get(t, d) != r)
                    {
                        continue;
                    }
                }

                if (!WORLD.ROADS().is(t))
                {
                    WORLD.ROADS().set(t, true);
                    WORLD.ROADS().minified.set(t, true);
                    amount--;
                }

                foreach (DIR d in DIR.ORTHO)
                {
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (WORLD.IN_BOUNDS(dx, dy))
                    {
                        if (WORLD.ROADS().is(dx, dy))
                        {
                            GUTIL.flooder().pushSmaller(dx, dy, 0);
                        }
                        double v = 1;
                        if (v >= 0 && polly.isEdge(dx, dy) && (!WORLD.WATER().isBig.is(dx, dy)))
                        {
                            GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + v * d.tileDistance(), t);
                        }
                    }
                }
            }

            GUTIL.flooder().done();
        }
    }
}