using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Battle.Invasion
{
    public static class SpotMaker
    {
        public static InvasionSpot Get(int men, int wx, int wy)
        {
            EntryPoint p = GetEntry(wx, wy);
            InvasionSpot sp = Make(men, p);

            return sp;
        }

        private static EntryPoint GetEntry(int wx, int wy)
        {
            EntryPoint start = SETT.ENTRY().points.active(wx, wy);
            if (start == null)
                return SETT.ENTRY().points.all(wx, wy);

            wx = start.wx() + start.dirOut.x();
            wy = start.wy() + start.dirOut.y();

            LinkedList<COORDINATE> all = new LinkedList<COORDINATE>();

            var inner = SETT.WORLD_AREA().tiles();
            var outer = new Rec(inner.width() + 2, inner.height() + 2);
            outer.moveX1Y1(inner).incrX(-1).incrY(-1);
            GUTIL.flooder().init(typeof(SpotMaker));
            GUTIL.flooder().pushSloppy(wx, wy, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                all.add(t);

                foreach (DIR d in DIR.ORTHO)
                {
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (!WORLD.IN_BOUNDS(dx, dy))
                        continue;
                    if (!outer.isOnEdge(dx, dy))
                        continue;
                    if (WTRAV.canLand(t.x(), t.y(), d, false))
                    {
                        GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + d.tileDistance());
                    }
                }
            }

            GUTIL.flooder().done();

            COORDINATE cc = all.rnd();

            EntryPoint best = null;
            double bestV = double.MaxValue;
            foreach (EntryPoint p in SETT.ENTRY().points.all())
            {
                if (!WTRAV.isGoodLandTile(p.wx(), p.wy()))
                    continue;
                double v = p.distanceValue(cc.x(), cc.y()) + RND.rFloat();
                if (v < bestV)
                {
                    bestV = v;
                    best = p;
                }
            }
            return best;
        }

        private static InvasionSpot Make(int men, EntryPoint p)
        {
            GUTIL.coos().set(0);
            foreach (COORDINATE c in p.body)
            {
                if (SETT.PATH().connectivity.is(c))
                {
                    GUTIL.coos().get().set(c);
                    GUTIL.coos().inc();
                }
            }

            if (GUTIL.coos().getI() < 8)
            {
                GUTIL.coos().set(0);
                foreach (COORDINATE c in p.body)
                {
                    if (SETT.TERRAIN().get(c).clearing().isStructure())
                    {
                        GUTIL.coos().get().set(c);
                        GUTIL.coos().inc();
                    }
                }
            }

            if (GUTIL.coos().getI() < 8)
            {
                GUTIL.coos().set(0);
                foreach (COORDINATE c in p.body)
                {
                    GUTIL.coos().get().set(c);
                    GUTIL.coos().inc();
                }
            }

            Rec c = new Rec(1);

            GUTIL.coos().set(RND.rInt(GUTIL.coos().getI()));

            int sx = GUTIL.coos().get().x();
            int sy = GUTIL.coos().get().y();

            c.moveX1Y1(sx, sy);
            DIR d = dir(p.body).next(2);

            int w = Math.Max(men / 10, 8);
            int i = 1;

            while (w > 0)
            {
                int x = sx + i * d.x();
                int y = sy + i * d.y();

                if (p.body.holdsPoint(x, y))
                {
                    w--;
                    c.unify(x, y);
                }
                int x2 = sx - i * d.x();
                int y2 = sy - i * d.y();
                if (p.body.holdsPoint(x2, y2))
                {
                    w--;
                    c.unify(x2, y2);
                }
                i++;
                if (!SETT.IN_BOUNDS(x, y) && !SETT.IN_BOUNDS(x2, y2))
                    break;
            }

            if (c.width() * c.height() < 8)
            {
                LOG.ln(p.body + " " + p.dirOut);
                LOG.ln(sx + " " + sy + " " + GUTIL.coos().getI());
                LOG.ln(c + " " + d);
                w = 8;
                i = 1;
                while (w > 0)
                {
                    int x = sx + i * d.x();
                    int y = sy + i * d.y();

                    if (p.body.holdsPoint(x, y))
                    {
                        w--;
                        c.unify(x, y);
                    }
                    int x2 = sx - i * d.x();
                    int y2 = sy - i * d.y();
                    if (p.body.holdsPoint(x2, y2))
                    {
                        w--;
                        c.unify(x2, y2);
                    }
                    LOG.ln(x + " " + y + " | " + x2 + " " + y2);
                    i++;
                    if (!SETT.IN_BOUNDS(x, y) && !SETT.IN_BOUNDS(x2, y2))
                        break;
                }
            }

            return new InvasionSpot(c);
        }

        private static DIR dir(RECTANGLE body)
        {
            if (body.width() == 1)
                return body.x1() == 0 ? DIR.W : DIR.E;
            else if (body.height() == 1)
                return body.y1() == 0 ? DIR.N : DIR.S;
            throw new RuntimeException("" + body);
        }

        public class InvasionSpot
        {
            private static Coo bomb = new Coo();
            public readonly Rec body;
            private readonly ArrayCooShort coos;
            public readonly int size;
            public readonly DIR dir;
            private int lastBombarded;
            private static Trajectory traj = new Trajectory();
            private bool any = false;

            public InvasionSpot(FileGetter f) throws IOException
            {
                this.body = new Rec();
                this.body.load(f);
                size = Math.Max(body.width(), body.height());
                coos = new ArrayCooShort(size);
                coos.load(f);
                DIR.ALL.get(f.i());
                dir = dir(body).perpendicular();
                lastBombarded = f.i();
                any = f.bool();
            }

            public InvasionSpot(Rec rec)
            {
                this.body = rec;
                size = Math.Max(rec.width(), rec.height());
                coos = new ArrayCooShort(size);
                int i = 0;
                foreach (COORDINATE c in rec)
                {
                    coos.set(i).set(c);
                    i++;
                }
                coos.shuffle(i - 1);
                coos.set(0);
                dir = dir(body).perpendicular();
                lastBombarded = 0;
            }

            public void save(FilePutter p)
            {
                this.body.save(p);
                coos.save(p);
                p.i(dir.id());
                p.i(lastBombarded);
                p.bool(any);
            }

            private COORDINATE getNextBombardStart()
            {
                int dist = (int)12;

                while (lastBombarded < dist)
                {
                    if (coos.getI() >= coos.size() - 1)
                    {
                        coos.set(0);
                        if (!any)
                            any = true;
                        bomb.set(sx, sy);

                        return bomb;
                    }
                }
                return null;
            }

            public bool launchProj()
            {
                COORDINATE coo = getNextBombardStart();
                if (coo == null)
                    return false;
                int sx = coo.x() * C.TILE_SIZE + C.TILE_SIZEH;
                int sy = coo.y() * C.TILE_SIZE + C.TILE_SIZEH;
                int x = sx + (lastBombarded) * dir.x() * C.TILE_SIZE;
                int y = sy + (lastBombarded) * dir.y() * C.TILE_SIZE;
                Projectile proj = SETT.INVADOR().proj;

                if (lastBombarded == 0 || !traj.calcLow(16, sx, sy, x, y, proj.maxAngle(1.0), proj.velocity(1.0)))
                    GAME.ARMIES().map.breakIt(x / C.TILE_SIZE, y / C.TILE_SIZE);
                else
                {
                    SETT.PROJS().launch(sx, sy, 16, traj, proj, 0, 1.0, null);
                }
                return true;
            }
        }
    }
}