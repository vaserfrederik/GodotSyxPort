using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;

class GeneratorLakeExtra
{
    private readonly Polymap polly;

    public GeneratorLakeExtra(CapitolArea area, GeneratorUtil util)
    {
        polly = util.polly;
        polly.CheckInit();

        ExtraLake(area, util);
    }

    private void ExtraLake(CapitolArea area, GeneratorUtil util)
    {
        if (area.IsBattle)
            return;

        int am = 0;
        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (SETT.TERRAIN().WATER.is.is(c) && SETT.TERRAIN().WATER.groundWater.is(c.x(), c.y()))
            {
                am++;
            }
        }
        if (am > 100)
            return;

        util.polly.CheckInit();

        GUTIL.Flooder().Init(this);

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (SETT.TILE_BOUNDS.IsOnEdge(c.x(), c.y()) || TERRAIN().MOUNTAIN.is(c) || ((TERRAIN().WATER.is.is(c))))
            {
                GUTIL.Flooder().PushSloppy(c, 0);
                GUTIL.Flooder().SetValue2(c, 1);
            }
        }

        while (GUTIL.Flooder().HasMore())
        {
            PathTile t = GUTIL.Flooder().PollSmallest();
            if (t.GetValue() > 48)
                break;
            foreach (DIR d in DIR.ALL)
            {
                if (SETT.IN_BOUNDS(t, d))
                    GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.tileDistance());
            }
        }

        double b = 0;
        int tx = SETT.TWIDTH / 2;
        int ty = SETT.THEIGHT / 2;
        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (!GUTIL.Flooder().HasBeenPushed(c.x(), c.y()))
            {
                double d = RND.rFloat();
                if (d >= b)
                {
                    b = d;
                    tx = c.x();
                    ty = c.y();
                }
            }
        }

        GUTIL.Flooder().Done();

        int radius = 12;
        for (int y1 = (int)(-radius); y1 < radius; y1++)
        {
            int y = y1 + ty;
            if (y < 0 || y >= SETT.TWIDTH)
                continue;
            for (int x1 = (int)(-radius); x1 < radius; x1++)
            {
                int x = tx + x1;
                if (x < 0 || x >= SETT.TWIDTH)
                    continue;
                double d = Math.Sqrt(x1 * x1 + y1 * y1);
                if (d < radius)
                {
                    polly.Checker.set(x, y, true);
                }
            }
        }

        GUTIL.Flooder().Init(this);

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            if (polly.Checker.is(c.x(), c.y()))
            {
                if (util.Height.Get(c) < 0.8)
                {
                    TERRAIN().WATER.SHALLOW.PlaceRaw(c.x(), c.y());
                    GUTIL.Flooder().PushSloppy(c, 0);
                }
            }
        }

        while (GUTIL.Flooder().HasMore())
        {
            PathTile t = GUTIL.Flooder().PollSmallest();
            if (t.GetValue() > 8)
                break;
            SETT.TERRAIN().WATER.groundWater.set(t, true);
            foreach (DIR d in DIR.ALL)
            {
                if (SETT.IN_BOUNDS(t, d))
                    GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.tileDistance());
            }
        }

        GUTIL.Flooder().Done();
    }
}