using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using world;

namespace world.map.landmark
{
    final class GeneratorLandmarkValidator
    {
        private static string ¤¤error = "¤This landmark does not have a connected body. Landmarks must be connected. Landmark:";

        static
        {
            D.ts(typeof(GeneratorLandmarkValidator));
        }

        private readonly Rec work = new Rec();

        public GeneratorLandmarkValidator(WorldError error)
        {
            Bitmap2D check = new Bitmap2D(WORLD.TBOUNDS(), false);
            Bitmap1D inited = new Bitmap1D(WorldLandmarks.MAX, false);

            bool hasOne = false;

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (check.is(c))
                    continue;

                WorldLandmark m = WORLD.LANDMARKS().setter.get(c);

                if (m == null)
                    continue;

                if (inited.get(m.index()))
                {
                    string prob = ¤¤error + " ID: " + m.index() + " , name: " + m.name;
                    if (error != null)
                    {
                        error.coo.set(c);
                        error.problem = prob;
                        return;
                    }
                    else
                    {
                        LOG.err(inited);
                    }
                }

                fill(c, check, m);
                inited.set(m.index(), true);
                hasOne = true;
            }

            if (!hasOne && error != null)
                error.warning = "No landmarks are on the current map.";
        }


        private void fill(COORDINATE start, Bitmap2D check, WorldLandmark a)
        {
            GUTIL.filler().init(this);
            GUTIL.filler().filler.set(start);

            int x1 = WORLD.TWIDTH();
            int x2 = -1;
            int y1 = WORLD.THEIGHT();
            int y2 = -1;
            int area = 0;

            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                if (!LANDMARKS().setter.is(c, a))
                {
                    if (c.x() < x1)
                        x1 = c.x();
                    if (c.x() > x2)
                        x2 = c.x();
                    if (c.y() < y1)
                        y1 = c.y();
                    if (c.y() > y2)
                        y2 = c.y();
                }
                else
                {
                    area++;
                    check.set(c.x(), c.y(), true);
                    foreach (DIR d in DIR.ORTHO)
                        if (WORLD.TBOUNDS().holdsPoint(c, d))
                            GUTIL.filler().filler.set(c, d);
                }
            }

            GUTIL.filler().done();


            GUTIL.flooder().init(this);
            work.set(x1, x2, y1, y2);
            int cx = work.cX();
            int cy = work.cY();
            int dist = int.MaxValue;
            for (int y = y1; y <= y2; y++)
            {
                for (int x = x1; x <= x2; x++)
                {
                    if (!LANDMARKS().setter.is(x, y, a) || work.isOnEdge(x, y))
                        GUTIL.flooder().pushSloppy(x, y, 0);
                }
            }

            PathTile t = null;
            while (GUTIL.flooder().hasMore())
            {
                t = GUTIL.flooder().pollSmallest();

                if (LANDMARKS().setter.is(t, a))
                {
                    if (COORDINATE.tileDistance(t.x(), t.y(), work.cX(), work.cY()) < dist)
                    {
                        cx = t.x();
                        cy = t.y();
                        dist = (int)COORDINATE.tileDistance(t.x(), t.y(), work.cX(), work.cY());
                    }
                }

                foreach (DIR d in DIR.ORTHO)
                {
                    if (work.holdsPoint(t, d))
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + d.tileDistance());
                }
            }
            GUTIL.flooder().done();

            if (t.getValue() > 14)
                a.init(t.x(), t.y(), area, 3);
            else if (t.getValue() > 8)
                a.init(t.x(), t.y(), area, 2);
            else if (t.getValue() > 5)
                a.init(t.x(), t.y(), area, 1);
            else
            {
                a.init(cx, cy, area, 0);
            }
        }
    }
}