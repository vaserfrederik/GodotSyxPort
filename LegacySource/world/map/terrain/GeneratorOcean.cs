using System;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;

namespace world.map.terrain
{
    class GeneratorOcean
    {
        private double limit = 0.3;
        private readonly bool[,] check = new bool[WORLD.THEIGHT(), WORLD.TWIDTH()];

        public GeneratorOcean(HeightMap height)
        {
            for (int y = 0; y < WORLD.THEIGHT(); y++)
            {
                for (int x = 0; x < WORLD.TWIDTH(); x++)
                {
                    double v = height.get(x, y);
                    if (v < limit && !WORLD.MOUNTAIN().coversTile(x, y))
                    {
                        if (v < limit * 0.75)
                            WORLD.WATER().LAKE.deep.placeRaw(x, y);
                        else
                            WORLD.WATER().LAKE.normal.placeRaw(x, y);
                    }
                    else
                    {
                        WORLD.WATER().NOTHING.placeRaw(x, y);
                    }
                }
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.WATER().LAKE.is.is(c) & !check[c.y(), c.x()])
                {
                    int size = check(c.x(), c.y());
                    if (size > 150)
                    {
                        size -= 150;
                        double ch = size / 2500.0;
                        if (RND.rFloat() < ch)
                            makeOcean(c.x(), c.y());
                    }
                }
            }
        }

        private int check(int tx, int ty)
        {
            GUTIL.filler().init(this);
            GUTIL.filler().fill(tx, ty);
            int size = 0;
            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                if (WORLD.WATER().LAKE.is.is(c))
                {
                    check[c.y(), c.x()] = true;
                    size++;
                }
                else
                {
                    continue;
                }
                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.IN_BOUNDS(c, d))
                        GUTIL.filler().fill(c, d);
                }
            }
            GUTIL.filler().done();
            return size;
        }

        private int makeOcean(int tx, int ty)
        {
            GUTIL.filler().init(this);
            GUTIL.filler().fill(tx, ty);
            int size = 0;
            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                if (WORLD.WATER().LAKE.is.is(c))
                {
                    if (WORLD.WATER().LAKE.deep.is(c))
                        WORLD.WATER().OCEAN.deep.placeRaw(c.x(), c.y());
                    else
                        WORLD.WATER().OCEAN.normal.placeRaw(c.x(), c.y());
                    size++;
                }
                else
                {
                    continue;
                }
                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.IN_BOUNDS(c, d))
                        GUTIL.filler().fill(c, d);
                }
            }
            GUTIL.filler().done();
            return size;
        }
    }
}