using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;
using world;

namespace world.map.terrain
{
    class GeneratorMountains
    {
        private readonly int pen = WORLD.TAREA();

        public GeneratorMountains(HeightMap height, WorldGen spec)
        {
            foreach (COORDINATE c in TBOUNDS())
            {
                if (height.get(c) > 0.75)
                {
                    MOUNTAIN().placeRaw(c.x(), c.y());
                }
                else
                {
                    MOUNTAIN().pClear(c.x(), c.y());
                }
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                GUTIL.flooder().setValue2(c, 0);
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.MOUNTAIN().getHeight(c.x(), c.y()) == 0)
                {
                    fill(c.x(), c.y());
                    break;
                }
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.MOUNTAIN().getHeight(c.x(), c.y()) == 0 && GUTIL.flooder().getValue2(c.x(), c.y()) == 0)
                {
                    connect(c.x(), c.y());
                    fill(c.x(), c.y());
                }
            }
        }

        private void fill(int sx, int sy)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(sx, sy, 0);
            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                t.setValue2(1);
                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (!WORLD.IN_BOUNDS(dx, dy))
                        continue;
                    if (WORLD.MOUNTAIN().getHeight(dx, dy) > 0)
                        continue;
                    if (GUTIL.flooder().getValue2(dx, dy) == 1)
                        continue;
                    GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + d.tileDistance(), t);
                }
            }
            GUTIL.flooder().done();
        }

        private void connect(int sx, int sy)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(sx, sy, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                if (t.getValue2() == 1)
                {
                    GUTIL.flooder().done();
                    while (t != null)
                    {
                        MOUNTAIN().pClear(t.x(), t.y());
                        t = t.getParent();
                    }
                    return;
                }
                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.IN_BOUNDS(t, d))
                    {
                        double v = WORLD.MOUNTAIN().getHeight(t.x() + d.x(), t.y() + d.y()) > 0 ? pen : 1;
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + v * d.tileDistance(), t);
                    }
                }
            }
            GUTIL.flooder().done();
        }
    }
}