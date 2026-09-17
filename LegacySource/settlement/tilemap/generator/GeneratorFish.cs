using System;
using settlement.main;
using snake2d;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using util;

class GeneratorFish
{
    private CapitolArea area;
    private GeneratorUtil util;

    public GeneratorFish(CapitolArea area, GeneratorUtil util)
    {
        this.area = area;
        this.util = util;

        smoothDepth();
        smooth();
        makeFish();
    }

    private void smooth()
    {
        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            GUTIL.flooder().setValue2(c, 0);
        }

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (SETT.TERRAIN().WATER.SHALLOW.is(c))
            {
                smooth(c);
            }
        }
    }

    private void smooth(COORDINATE start)
    {
        if (GUTIL.flooder().getValue(start) != 0)
            return;

        GUTIL.flooder().init(this);
        GUTIL.flooder().pushSloppy(start, 0);

        int am = 0;

        while (GUTIL.flooder().hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            if (!SETT.TERRAIN().WATER.is.is(t))
            {
                GUTIL.flooder().done();
                return;
            }
            GUTIL.flooder().setValue2(t, 1);
            if (SETT.TERRAIN().WATER.SHALLOW.is(t))
            {
                am++;

                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + 1);
                    }
                }
            }
        }

        GUTIL.flooder().done();

        if (am > 100)
        {
            return;
        }

        GUTIL.flooder().init(this);
        GUTIL.flooder().pushSloppy(start, 0);

        while (GUTIL.flooder().hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            if (SETT.TERRAIN().WATER.SHALLOW.is(t) && SETT.TERRAIN().WATER.groundWaterSalt.is(t))
            {
                SETT.TERRAIN().WATER.DEEP.placeRaw(t.x(), t.y());

                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + 1);
                    }
                }
            }
        }

        GUTIL.flooder().done();
    }

    private void smoothDepth()
    {
        Flooder f = GUTIL.flooder();

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
            f.setValue2(c, 0);
        for (int y = 0; y < SETT.THEIGHT; y++)
        {
            for (int x = 0; x < SETT.TWIDTH; x++)
            {
                smooth(x, y);
            }
        }
    }

    private void smooth(int sx, int sy)
    {
        if (!is.is(sx, sy))
            return;

        Flooder f = GUTIL.flooder();

        if (f.getValue2(sx, sy) != 0)
            return;

        f.init(this);

        f.pushSloppy(sx, sy, 0);

        double tiles = 0;

        double edgeValue = SETT.TAREA * 2;

        while (f.hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            f.setValue2(t, 1);
            if (t.getValue() >= edgeValue)
            {
                break;
            }

            tiles++;

            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.IN_BOUNDS(t, d))
                {
                    f.pushSmaller(t, d, t.getValue() + 1 + (is.is(t, d) ? 0 : edgeValue));
                }
            }
        }

        if (tiles < 50)
        {
            f.done();
            f.init(this);
            f.pushSloppy(sx, sy, 0);

            while (f.hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                SETT.TERRAIN().WATER.SHALLOW.placeRaw(t.x(), t.y());

                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.IN_BOUNDS(t, d) && is.is(t, d))
                    {
                        f.pushSmaller(t, d, t.getValue() + 1);
                    }
                }
            }
        }

        while (f.hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            int am = 0;
            if (is.is(t))
            {
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.TERRAIN().WATER.SHALLOW.is(t, d))
                        am++;
                }
                if (am >= 2)
                    SETT.TERRAIN().WATER.SHALLOW.placeRaw(t.x(), t.y());
            }
        }

        f.done();
    }

    private void makeFish()
    {
        Flooder f = GUTIL.flooder();

        SETT.TERRAIN().WATER.deepSeaFishSpot.clear();
        SETT.TERRAIN().WATER.fishAmount.clear();

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
            f.setValue2(c, 0);

        for (int y = 0; y < SETT.THEIGHT; y++)
        {
            for (int x = 0; x < SETT.TWIDTH; x++)
            {
                make(x, y);
            }
        }
    }

    private void make(int sx, int sy)
    {
        if (!is.is(sx, sy))
            return;

        Flooder f = GUTIL.flooder();

        if (f.getValue2(sx, sy) != 0)
            return;

        f.init(this);

        f.pushSloppy(sx, sy, 0);

        double tiles = 0;

        double edgeValue = SETT.TAREA * 2;

        while (f.hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            f.setValue2(t, 1);
            if (t.getValue() >= edgeValue)
            {
                break;
            }

            tiles += TERRAIN().WATER.groundWaterSalt.is(sx, sy) ? 1 : 0.5;

            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.IN_BOUNDS(t, d))
                {
                    f.pushSmaller(t, d, t.getValue() + 1 + (is.is(t, d) ? 0 : edgeValue));
                }
            }
        }

        double am = tiles / 200.0;
        am = Math.Min(am, f.pushed());

        double delta = f.pushed() / am;
        double de = RND.rFloat() * delta;

        double amount = 2.0 * SETT.TERRAIN().WATER.fishAmount.max() / delta;

        while (f.hasMore())
        {
            PathTile t = GUTIL.flooder().pollSmallest();
            f.setValue2(t, 1);
            de -= 1;
            {
                int vv = (int)(amount);
                if (RND.rFloat() < amount - vv)
                    vv++;
                vv = CLAMP.i(vv, 0, SETT.TERRAIN().WATER.fishAmount.max());
                SETT.TERRAIN().WATER.fishAmount.set(t, vv);
            }

            if (de <= 0)
            {
                TERRAIN().WATER.deepSeaFishSpot.set(t, true);
                de += RND.rFloat() * delta;
            }
        }

        f.done();
    }

    private MAP_BOOLEAN is = new MAP_BOOLEAN()
    {
        public bool is(int tx, int ty)
        {
            return TERRAIN().WATER.DEEP.is(tx, ty) || TERRAIN().WATER.BRIDGE.is(tx, ty);
        }

        public bool is(int tile)
        {
            return false;
        }
    };
}