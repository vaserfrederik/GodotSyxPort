using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using world;
using world.map.regions.centre;
using world.map.road;

namespace world.map.terrain
{
    sealed class GeneratorValidator : Bitmap2D
    {
        private readonly Bitmap2D tested = new Bitmap2D(WORLD.TBOUNDS(), false);

        private static readonly CharSequence ¤¤noRoom = "There is no room for a single settlement on the map. Make sure there is at least one 3x3 area where a city can be.";
        private static readonly CharSequence ¤¤notConnected = "This area is isolated by the terrain. Make sure there is an open path to this place.";

        static GeneratorValidator()
        {
            D.ts(typeof(GeneratorValidator));
        }

        public GeneratorValidator(WorldError error) : base(WORLD.TBOUNDS(), false)
        {
            Rec tBound = new Rec(WORLD.TBOUNDS());
            bool hasOne = false;

            foreach (COORDINATE c in tBound)
            {
                if (WorldCentrePlacablity.terrainC(c.x(), c.y()) == null)
                {
                    fill(c.x(), c.y());
                    hasOne = true;
                    break;
                }
            }

            if (!hasOne)
            {
                if (error != null)
                {
                    error.coo.set(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2);
                    error.problem = ¤¤noRoom;
                    return;
                }

                GUTIL.flooder().init(this);
                PathTile t = GUTIL.flooder().pushSloppy(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2, 0);
                hasOne = true;
                t = GUTIL.flooder().pushSmaller(WORLD.TWIDTH() / 2 + 1, WORLD.THEIGHT() / 2 + 1, 0, t);
                GUTIL.flooder().done();

                while (t != null)
                {
                    WORLD.MOUNTAIN().pClear(t.x(), t.y());
                    WORLD.WATER().NOTHING.placeRaw(t.x(), t.y());
                    t = t.getParent();
                }
                fill(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2);

                if (!tested.is(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2))
                {
                    throw new Exception("WTF " + WORLD.MOUNTAIN().is(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2) + " " + WORLD.WATER().isBig.is(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2));
                }
            }

            foreach (COORDINATE c in tBound)
            {
                if (!tested.is(c) && WorldCentrePlacablity.terrainC(c.x(), c.y()) == null)
                {
                    if (error != null)
                    {
                        error.coo.set(c);
                        error.problem = ¤¤notConnected;
                        return;
                    }
                    connect(c);
                    fill(c.x(), c.y());
                }
            }
        }

        private void connect(COORDINATE c)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(c, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                if (tested.is(t))
                {
                    GUTIL.flooder().done();
                    fix(t);
                    GUTIL.flooder().reverse(t);
                    fix(t);
                    return;
                }

                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.IN_BOUNDS(t, d))
                    {
                        if (WTRAV.can(t.x(), t.y(), d, false))
                            GUTIL.flooder().pushSmaller(t, d, t.getValue() + 1, t);
                        else
                            GUTIL.flooder().pushSmaller(t, d, t.getValue() + 15, t);
                    }
                }
            }

            throw new Exception();
        }

        private void fill(int sx, int sy)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(sx, sy, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile c = GUTIL.flooder().pollSmallest();

                tested.set(c, true);

                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.IN_BOUNDS(c, d) && WTRAV.can(c.x(), c.y(), d, false))
                        GUTIL.flooder().pushSmaller(c, d, c.getValue() + 1);
                }
            }
            GUTIL.filler().done();
        }

        private void fix(PathTile t)
        {
            if (t.getParent() == null)
            {
                WORLD.MOUNTAIN().pClear(t.x(), t.y());
                WORLD.WATER().NOTHING.placeRaw(t.x(), t.y());
                return;
            }

            PathTile from = t;
            t = t.getParent();

            while (t != null)
            {
                DIR d = DIR.get(from, t);
                if (!WTRAV.can(from.x(), from.y(), d, false))
                {
                    WORLD.MOUNTAIN().pClear(from.x(), from.y());
                    WORLD.WATER().NOTHING.placeRaw(from.x(), from.y());
                    WORLD.MOUNTAIN().pClear(t.x(), t.y());
                    WORLD.WATER().NOTHING.placeRaw(t.x(), t.y());
                }
                from = t;
                t = t.getParent();
            }
        }
    }
}