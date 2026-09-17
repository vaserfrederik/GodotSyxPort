using System;
using System.Collections.Generic;
using snake2d;
using util;
using world;
using world.map.road;

namespace settlement.tilemap.generator
{
    final class GeneratorRoads
    {
        private readonly Bitmap2D edge = new Bitmap2D(SETT.TILE_BOUNDS, false);
        private readonly PathGame.PathFancy path = new PathGame.PathFancy(5000);
        private readonly EntryPoints ees = SETT.ENTRY().points;

        final PathGame.COST cm = new PathGame.COST()
        {
            public double getCost(int fromX, int fromY, int toX, int toY)
            {
                if (SETT.TERRAIN().WATER.BRIDGE.is(toX, toY))
                    return 0.2;

                if (!SETT.TERRAIN().get(toX, toY).clearing().isEasilyCleared())
                    return 25;

                if (GUTIL.flooder().getValue2(toX, toY) != 0)
                    return 10;

                if (SETT.TERRAIN().get(toX, toY) != SETT.TERRAIN().NADA)
                    return 2;

                if (!edge.is(toX, toY))
                    return 2;

                if (SETT.FLOOR().getter.get(toX, toY) != null)
                    return 0.2;

                return 1;
            }
        };

        GeneratorRoads(CapitolArea area)
        {
            {
                Polymap polly = new Polymap(SETT.TWIDTH, SETT.THEIGHT, 8, 1);
                foreach (COORDINATE c in SETT.TILE_BOUNDS)
                {
                    edge.set(c, polly.isEdge(c.x(), c.y()));
                    SETT.MAINTENANCE().disabled.set(c, false);
                }
            }

            bool[] isToActivate = new bool[ees.all().size()];
            LinkedList<EntryPoint> toActivate = new LinkedList<EntryPoint>();
            LinkedList<EntryPoint> toFindOther = new LinkedList<EntryPoint>();
            foreach (EntryPoint e in ees.all())
            {
                int wx = area.tiles().x1() + e.wCooD().x();
                int wy = area.tiles().y1() + e.wCooD().y();

                if (WORLD.PATH().map.can(wx, wy, e.dirOut))
                {
                    if (WTRAV.isGoodLandTile(wx, wy))
                    {
                        isToActivate[e.index()] = true;
                        toActivate.add(e);
                    }
                    else
                    {
                        toFindOther.add(e);
                    }
                }
            }

            foreach (EntryPoint notFindable in toFindOther)
            {
                double dist = double.MaxValue;
                EntryPoint best = null;

                int fx = area.tiles().x1() + notFindable.wCooD().x() + notFindable.dirOut.x();
                int fy = area.tiles().y1() + notFindable.wCooD().y() + notFindable.dirOut.y();

                foreach (EntryPoint e in ees.all())
                {
                    if (e == notFindable)
                        continue;

                    int wx = area.tiles().x1() + e.wCooD().x();
                    int wy = area.tiles().y1() + e.wCooD().y();

                    if (WTRAV.isGoodLandTile(wx, wy))
                    {
                        double d = e.distanceValue(fx, fy);
                        if (d < dist)
                        {
                            best = e;
                            dist = d;
                        }
                    }
                }

                if (best != null && !isToActivate[best.index()])
                {
                    isToActivate[best.index()] = true;
                    toActivate.add(best);
                }
            }

            if (!area.isBattle && toActivate.size() == 0)
                GAME.Warn("No active entry points exist");

            foreach (EntryPoint e in toActivate)
            {
                adjust(e);
            }

            Flooder f = GUTIL.flooder();

            {
                f.init(this);
                foreach (COORDINATE c in SETT.TILE_BOUNDS)
                {
                    if (!SETT.TERRAIN().get(c.x(), c.y()).clearing().isEasilyCleared())
                    {
                        f.pushSloppy(c, 0);
                        f.setValue2(c, 1);
                    }
                    else
                        f.setValue2(c, 0);
                }

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    foreach (DIR d in DIR.ALL)
                    {
                        if (SETT.IN_BOUNDS(t, d))
                            f.setValue2(t.x(), t.y(), d, 1);
                    }
                }
                f.done();
            }

            Coo start = new Coo(SETT.TWIDTH / 2, SETT.THEIGHT / 2);
            {
                f.init(this);
                f.pushSloppy(SETT.TWIDTH / 2, SETT.THEIGHT / 2, 0);

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();

                    if (cm.getCost(0, 0, t.x(), t.y()) < 2)
                    {
                        start.set(t);
                        break;
                    }

                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (SETT.IN_BOUNDS(t, d))
                            f.pushSmaller(t, d, t.getValue() + d.tileDistance());
                    }

                }
                f.done();
            }

            if (!area.isBattle && toActivate.size() == 0)
                Console.Error.WriteLine("The city map did not generate any roads, and the city will be unplayable. Please send the save to info@songsofsyx.com");

            foreach (EntryPoint e in toActivate)
            {
                road(start, e.coo());
            }
        }

        private void road(COORDINATE from, COORDINATE to)
        {
            int ox = from.x();
            int oy = to.x();
            if (GUTIL.pathTools().astar.getShortest(path, cm, from.x(), from.y(), to.x(), to.y()))
            {
                while (true)
                {
                    place(path.x(), path.y(), ox, oy);
                    place(ox, oy, path.x(), path.y());
                    ox = path.x();
                    oy = path.y();
                    if (!path.setNext())
                        break;
                }
                place(from.x(), from.y(), from.x(), from.y());
                place(to.x(), to.y(), to.x(), to.y());
            }
        }

        private static void place(int tx, int ty, int ox, int oy)
        {
            if (SETT.TERRAIN().WATER.is.is(tx, ty))
            {
                placeWater(tx, ty, ox, oy);
            }
            else
            {
                placeRoad(tx, ty);
                foreach (DIR d in DIR.ALL)
                {
                    if (!SETT.TERRAIN().WATER.is.is(tx, ty, d))
                    {
                        placeRoad(tx + d.x(), ty + d.y());
                    }
                }
            }
        }

        private static void placeWater(int tx, int ty, int ox, int oy)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                if (TERRAIN().WATER.SHALLOW.is(tx, ty) && RND.oneIn(8))
                    TERRAIN().NADA.placeRaw(tx, ty);
                else if (TERRAIN().WATER.DEEP.is(tx, ty))
                {
                    TERRAIN().WATER.BRIDGE.placeRaw(tx, ty);

                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (RND.oneIn(16) && TERRAIN().WATER.DEEP.is(tx + d.x(), ty + d.y()))
                            TERRAIN().WATER.BRIDGE.placeRaw(tx + d.x(), ty + d.y());
                    }
                }
                if (TERRAIN().WATER.DEEP.is(tx, ty))
                    TERRAIN().WATER.BRIDGE.placeRaw(tx, ty);
                if (TERRAIN().WATER.DEEP.is(tx, ty))
                    TERRAIN().WATER.BRIDGE.placeRaw(tx, ty);
            }
        }

        private static void placeRoad(int tx, int ty)
        {
            if (SETT.TERRAIN().get(tx, ty) != SETT.TERRAIN().NADA)
            {
                SETT.TERRAIN().NADA.placeFixed(tx, ty);
                SETT.FLOOR().mainStartRoad.placeFixed(tx, ty);
                //SETT.FLOOR().degrade.set(tx, ty, 1.0);
            }
        }

        private void adjust(EntryPoint e)
        {
            double bestV = double.MaxValue;
            final int ox = e.body.cX();
            final int oy = e.body.cY();
            final Coo best = new Coo(ox, oy);

            final Coo start = new Coo();

            foreach (COORDINATE c in e.body)
            {
                start.set(c);
                break;
            }

            foreach (COORDINATE c in e.body)
            {
                if (SETT.TERRAIN().get(c.x(), c.y()) != SETT.TERRAIN().NADA)
                {
                    if (start.isSameAs(c))
                    {
                        continue;
                    }

                    int cx = (start.x() + c.x()) / 2;
                    int cy = (start.y() + c.y()) / 2;

                    int size = Math.Abs(start.x() - c.x()) + Math.Abs(start.y() - c.y());
                    size = CLAMP.i(size, 0, 8);
                    int dist = Math.Abs(cx - ox) + Math.Abs(cy - oy);

                    double v = dist;
                    v /= (size + 1);
                    if (v < bestV)
                    {
                        bestV = v;
                        best.set(cx, cy);
                    }
                    start.set(c);
                }
            }

            ees.map.set(best, true);
        }
    }
}