using System;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.order;
using init.constant;
using settlement.main;
using settlement.path;
using snake2d;
using snake2d.util.datatypes;

namespace game.battle.thread.order
{
    final class ToolsWalk
    {
        private readonly VectorImp vec = new VectorImp();
        private readonly VectorImp vec2 = new VectorImp();
        private readonly Tools t;

        public static readonly int destMoveStart = 90;
        public static readonly int destMoveResume = 80;

        public ToolsWalk(Tools t)
        {
            this.t = t;
        }

        public bool setStart(int tilesCheckDest)
        {
            PathTile c = getStart();

            if (c == null)
                return false;

            int sx = (c.x() << C.T_SCROLL) + C.TILE_SIZEH;
            int sy = (c.y() << C.T_SCROLL) + C.TILE_SIZEH;

            //if sx is close to prev, and prev is coherent, use prev maybe

            if (c.getParent() != null)
            {
                PathTile p = c.getParent();
                c.parentSet(null);
                c = reverse(c, p);
            }

            if (c.getParent() != null)
                c = c.getParent();

            pp.clear();
            pp.set(c);

            path.clear();
            COORDINATE dest = t.div.getSafeCentrePixel(Plan.dest);
            int destX = dest.x() >> C.T_SCROLL;
            int destY = dest.y() >> C.T_SCROLL;
            path.init(sx, sy, pp, destX, destY, t.pathCost, a, div.race());
            order.path.set(path);

            if (path.isDest())
            {
                return false;
            }
            bool b = setStartPosition(tilesCheckDest);
            return b;
        }

        private PathTile reverse(PathTile newParent, PathTile t)
        {
            if (t.getParent() == null)
            {
                t.parentSet(newParent);
                return t;
            }
            PathTile res = reverse(t, t.getParent());
            t.parentSet(newParent);
            return res;
        }

        private PathTile getStart()
        {
            if (dest.deployed() <= 0 || dest.centreTile() == null || current.deployed() <= 0)
            {
                return null;
            }

            int destX = dest.centreTile().x();
            int destY = dest.centreTile().y();

            if (!SETT.IN_BOUNDS(destX, destY))
                return null;

            COORDINATE pp = t.div.getSafeCentrePixel(prev);

            Flooder f = t.pather.getFlooder();
            f.init(this);

            for (int i = 0; i < current.deployed(); i++)
            {
                if (div.reporter.reachable(i))
                {
                    double dist = 0;
                    if (pp != null)
                    {
                        dist = pp.tileDistanceTo(current.pixel(i));
                        dist *= C.ITILE_SIZE;
                    }
                    f.pushSloppy(current.tile(i), dist);
                    f.setValue2(current.tile(i), dist);
                }
            }

            if (!f.hasMore())
            {
                for (int i = 0; i < current.deployed(); i++)
                {
                    double dist = 0;
                    if (pp != null)
                    {
                        dist = pp.tileDistanceTo(current.pixel(i));
                        dist *= C.ITILE_SIZE;
                    }
                    f.pushSloppy(current.tile(i), dist);
                    f.setValue2(current.tile(i), dist);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.isSameAs(destX, destY))
                {
                    f.done();

                    return startReturn(t);
                }
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        double cost = this.t.pathCost.cost(t.x(), t.y(), d);
                        if (cost < 0)
                            continue;
                        cost *= DIR.ALL.get(i).tileDistance();

                        double di = COORDINATE.tileDistance(dx, dy, destX, destY);
                        if (f.pushSmaller(dx, dy, t.getValue2() + cost + di, t) != null)
                            f.setValue2(dx, dy, t.getValue2() + cost);
                    }
                }
            }

            return null;
        }

        private PathTile startReturn(PathTile t)
        {
            throw new NotImplementedException();
        }

        private bool setStartPosition(int tilesCheckDest)
        {
            throw new NotImplementedException();
        }

        public bool canMoveAllTheWayToDest()
        {
            if (path.length() == 0)
                return true;

            int pi = path.currentI();
            while (!path.isDest())
            {
                if (!checkStep(path.x(), path.y()))
                {
                    path.setCurrentI(pi);
                    return false;
                }
                path.currentIInc(1);
            }

            if (path.isComplete())
            {
                path.setCurrentI(pi);
                return true;
            }

            int sx = path.x();
            int sy = path.y();

            path.setCurrentI(pi);
            return canMoveAllTheWayToDest(sx, sy);
        }

        private bool canMoveAllTheWayToDest(int sx, int sy)
        {
            int dx = dest.centrePixel().x();
            int dy = dest.centrePixel().y();
            double m = vec2.set(sx, sy, dx, dy);
            int steps = (int)Math.Ceiling(m / C.TILE_SIZE);
            for (int i = 0; i < steps; i++)
            {
                int tx = ((int)(sx + vec2.nX() * i * C.TILE_SIZE));
                int ty = ((int)(sy + vec2.nY() * i * C.TILE_SIZE));
                if (!checkStep(tx, ty))
                    return false;
            }
            return true;
        }

        private bool checkStep(int cx, int cy)
        {
            int size = dest.formation().size(div);
            double x1 = cx - dest.dx() * dest.width() / 2;
            double y1 = cy - dest.dy() * dest.width() / 2;
            int am = dest.width() / size;
            for (int i = 0; i < am; i++)
            {
                if (DivPlacability.pixelIsBlocked((int)x1, (int)y1, size, a))
                    return false;
                x1 += dest.dx() * size;
                y1 += dest.dy() * size;
            }
            return true;
        }

        private readonly PathGame.PathFancy pp = new PathGame.PathFancy(1024 * 4);

        public bool hasReachedPrev()
        {
            int am = t.walk.countPosition();
            int lim = (int)Math.Ceiling(0.9 * (Plan.men - div.reporter.unreachable()));
            if (am == 0)
                return false;
            if (am < lim)
                return false;
            return true;
        }

        public int countPosition()
        {
            int dist = div.settings().running ? C.TILE_SIZE : C.TILE_SIZEH;
            return t.div.inPosition(current, prev, dist);
        }
    }
}