using System;
using static settlement.main.SETT;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;
using world;

namespace settlement.tilemap.generator
{
    class GeneratorLake
    {
        private readonly int PS = 4;
        private readonly Polymap polly;
        private readonly double radius;

        public GeneratorLake(CapitolArea area, GeneratorUtil util)
        {
            radius = util.json.d("LAKE_SIZE", 0.1, 1.0) * 200;

            polly = util.polly;
            polly.checkInit();

            for (int i = 0; i < GRID.tiles().size(); i++)
            {
                SettlementGrid.Tile t = GRID.tile(i);
                COORDINATE wt = area.ts().get(i);

                if (!WORLD.WATER().LAKE.is.is(wt))
                    continue;
                foreach (DIR d in t.getDirs())
                {
                    if (WORLD.WATER().LAKE.is.is(wt, d))
                    {
                        sink(t.cooInner(d));
                    }
                }
            }

            GUTIL.filler().init(this);
            for (int y = 0; y < SETT.TWIDTH; y++)
            {
                for (int x = 0; x < SETT.TWIDTH; x++)
                {
                    if (util.polly.checker.is(x / PS, y / PS))
                    {
                        GUTIL.filler().fill(x, y);
                    }
                }
            }

            util.polly.checkInit();
            while (GUTIL.filler().hasMore())
            {
                COORDINATE t = GUTIL.filler().poll();
                util.polly.checker.set(t, true);
                if (RND.oneIn(500))
                {
                    int x = (int)(t.x() + (RND.rBoolean() ? RND.rExpo() * 20 : -RND.rExpo() * 20));
                    int y = (int)(t.y() + (RND.rBoolean() ? RND.rExpo() * 20 : -RND.rExpo() * 20));
                    util.polly.checker.set(x, y, true);
                }
            }
            GUTIL.filler().done();

            int islands = (int)(util.json.d("LAKE_ISLANDS", 0.0, 1.0) * 100);

            if (islands != 0)
            {
                for (int i = RND.rInt(islands); i > 0; i--)
                {
                    int x = RND.rInt(TWIDTH);
                    int y = RND.rInt(THEIGHT);
                    util.polly.checker.set(x, y, false);
                    for (int l = RND.rInt(20); l > 0; l--)
                    {
                        int x2 = x + RND.rInt0(30);
                        int y2 = y + RND.rInt0(30);
                        util.polly.checker.set(x2, y2, false);
                    }
                }
            }

            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                if (polly.checker.is(c.x(), c.y()))
                {
                    if (util.height.get(c) < 0.8)
                    {
                        TERRAIN().WATER.SHALLOW.placeRaw(c.x(), c.y());
                    }
                }
            }
        }

        public void sink(int cx, int cy)
        {
            for (int y1 = (int)(-radius); y1 < radius; y1++)
            {
                int ty = y1 + cy;
                if (ty < 0 || ty >= SETT.TWIDTH)
                    continue;
                for (int x1 = (int)(-radius); x1 < radius; x1++)
                {
                    int tx = cx + x1;
                    if (tx < 0 || tx >= SETT.TWIDTH)
                        continue;
                    double d = Math.Sqrt(x1 * x1 + y1 * y1);
                    if (d < radius)
                    {
                        polly.checker.set(tx / PS, ty / PS, true);
                    }
                }
            }
        }

        public void sink(COORDINATE c)
        {
            sink(c.x(), c.y());
        }
    }
}