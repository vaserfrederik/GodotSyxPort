using System;
using System.Collections.Generic;
using settlement.main;
using settlement.tilemap;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;

namespace settlement.tilemap.generator
{
    class GeneratorMountain
    {
        private readonly double radius;
        private int PS = 8;
        private readonly TileMap m = TILE_MAP();

        private readonly HeightMap height = new HeightMap(TWIDTH, THEIGHT, 16, 1);

        public GeneratorMountain(CapitolArea area, GeneratorUtil util)
        {
            radius = this.radius * RND.rFloat1(0.10);
            final double radius2 = radius * radius;
            tx += RND.rInt0(20);
            ty += RND.rInt0(20);

            for (int y = (int)-radius; y < radius; y++)
            {
                for (int x = (int)-radius; x < radius; x++)
                {
                    int dx = tx + x;
                    int dy = ty + y;
                    if (!IN_BOUNDS(dx, dy))
                        continue;
                    double r = x * x + y * y;
                    if (r > radius2)
                        continue;
                    util.polly.checker.set(dx / PS, dy / PS, true);
                }
            }
        }

        private void fertilize(GeneratorUtil util)
        {
            GUTIL.flooder().init(this);

            HeightMap hh = new HeightMap(TWIDTH, THEIGHT, 32, 8);

            for (int y = 0; y < SETT.TWIDTH; y++)
            {
                for (int x = 0; x < SETT.TWIDTH; x++)
                {
                    if (m.topology.MOUNTAIN.is(x, y))
                    {
                        util.fer.set(x, y, hh.get(x, y));
                        for (DIR d : DIR.ORTHO)
                            if (!m.topology.MOUNTAIN.is(x, y, d))
                            {
                                GUTIL.flooder().pushSloppy(x, y, 0);
                                break;
                            }
                    }
                }
            }
            {
                double ma = 5;

                while (GUTIL.flooder().hasMore())
                {
                    PathTile t = GUTIL.flooder().pollSmallest();
                    if (t.getValue() >= ma)
                        continue;
                    double v = t.getValue() / ma;
                    util.fer.set(t.x(), t.y(), GeneratorFertilityFin.T_ROCK * (1.0 - v) + util.fer.get(t.x(), t.y()) * v);
                    for (DIR d : DIR.ALL)
                    {
                        int dx = t.x() + d.x();
                        int dy = t.y() + d.y();
                        if (SETT.IN_BOUNDS(dx, dy))
                            GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + d.tileDistance());
                    }
                }
            }

            GUTIL.flooder().done();

            {
                double am = 8;
                int dx = -1;
                int dy = -1;

                double d = 0.1;

                for (int y = 0; y < SETT.TWIDTH; y++)
                {
                    for (int x = 0; x < SETT.TWIDTH; x++)
                    {
                        if (m.topology.MOUNTAIN.is(x, y))
                        {
                            for (int i = 0; i < am; i++)
                            {
                                int tx = x + i * dx;
                                int ty = y + i * dy;
                                if (!SETT.IN_BOUNDS(tx, ty))
                                    break;
                                if (!m.topology.MOUNTAIN.is(tx, ty))
                                {
                                    double dd = d * (1.0 - i / am);
                                    util.fer.increment(tx, ty, dd);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void makeHeight(GeneratorUtil util)
        {
            GUTIL.flooder().init(this);

            for (int y = 0; y < SETT.TWIDTH; y++)
            {
                for (int x = 0; x < SETT.TWIDTH; x++)
                {
                    if (m.topology.MOUNTAIN.is(x, y))
                    {
                        GUTIL.flooder().pushSloppy(x, y, 0);
                        util.height.set(x, y, 1.0);
                        double v2 = 0;
                        if (RND.oneIn(5))
                        {
                            v2 = RND.rFloat(30);
                        }
                        GUTIL.flooder().setValue2(x, y, v2);
                    }
                }
            }

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                if (t.getValue() >= t.getValue2())
                    continue;

                for (DIR d : DIR.ORTHO)
                {
                    int x = t.x() + d.x();
                    int y = t.y() + d.y();
                    if (IN_BOUNDS(x, y))
                    {
                        if (GUTIL.flooder().hasBeenPushed(x, y))
                            continue;

                        double v = t.getValue() + d.tileDistance();
                        double dd = v / t.getValue2();

                        if (t.getValue2() > 20)
                        {
                            double h = util.height.get(x, y);
                            h = h * dd + (1 - dd);
                            h *= RND.rFloat1(0.3);
                            util.height.set(x, y, h);
                        }

                        if (GUTIL.flooder().pushSloppy(x, y, v) != null)
                        {
                            GUTIL.flooder().setValue2(x, y, t.getValue2());
                        }
                    }
                }
            }

            GUTIL.flooder().done();
        }

        private bool isSolo(int tx, int ty)
        {
            for (int i = 0; i < DIR.NORTHO.size(); i++)
            {
                DIR d = DIR.NORTHO.get(i);
                if (is(tx, ty, d) && is(tx, ty, d.next(1)) && is(tx, ty, d.next(-1)))
                    return false;
            }
            return true;
        }

        private bool is(int tx, int ty, DIR d)
        {
            if (!IN_BOUNDS(tx, ty, d))
                return true;
            return TERRAIN().MOUNTAIN.is(tx, ty, d);
        }

        private void clear(DIR d, int startX, int startY, Polymap p)
        {
            for (int i = 0; i < QUAD_HALF; i++)
            {
                p.checker.set(startX, startY, false);
                if (IN_BOUNDS(startX - 1, startY))
                    p.checker.set(startX, startY, false);
                if (IN_BOUNDS(startX - 1, startY - 1))
                    p.checker.set(startX, startY, false);
                if (IN_BOUNDS(startX, startY - 1))
                    p.checker.set(startX, startY, false);
                p.checker.set(startX, startY, false);
                startX += d.x();
                startY += d.y();
            }
        }
    }
}