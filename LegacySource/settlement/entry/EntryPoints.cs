using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace settlement.entry
{
    public sealed class EntryPoints
    {
        private readonly int ww = Math.Max(SETT.TWIDTH, SETT.THEIGHT);
        private readonly Bitmap1D ismap = new Bitmap1D(ww * 4, false);
        private readonly IList<EntryPoint> all;
        private readonly ArrayList<EntryPoint> active;
        private readonly ArrayList<EntryPoint> reachable;
        private bool dirty = true;

        public EntryPoints()
        {
            Rec ww = new Rec(WCentre.TILE_DIM);

            ArrayListGrower<EntryPoint> all = new ArrayListGrower<EntryPoint>();
            int index = 0;
            foreach (COORDINATE c in ww)
            {
                if (c.x() == 0)
                {
                    int x1 = 0;
                    int x2 = 1;
                    int y1 = c.y() * SETT.THEIGHT / WCentre.TILE_DIM;
                    int y2 = (c.y() + 1) * SETT.THEIGHT / WCentre.TILE_DIM;
                    EntryPoint p = new EntryPoint(index++, x1, x2, y1, y2, DIR.NORTH, c.x(), c.y());
                    all.add(p);

                    if (!RND.oneIn(3))
                    {
                        DIR d = p.dirOut.next(-2 + RND.rInt(2) * 4);
                        if (SETT.PATH().connectivity.is(c, d))
                        {
                            c.increment(d.x(), d.y());
                        }
                    }
                    if (SETT.PATH().connectivity.is(c))
                    {
                        p.sCoo.set(c.x(), c.y());
                    }
                    else
                    {
                        p.sCoo.set(body.cX(), body.cY());
                    }
                }
            }

            this.all = all;
            active = new ArrayList<EntryPoint>();
            reachable = new ArrayList<EntryPoint>();
        }

        private int imapi(int tx, int ty)
        {
            if (tx < 0 || tx >= ww || ty < 0 || ty >= ww)
            {
                return -1;
            }
            return tx + ty * ww;
        }

        private void setActive()
        {
            active.clearSloppy();
            foreach (EntryPoint e in all)
            {
                if (e.active)
                {
                    active.add(e);
                }
            }
        }

        public void generate(CapitolArea area)
        {
        }

        public IList<EntryPoint> all()
        {
            return all;
        }

        public IList<EntryPoint> active()
        {
            return active;
        }

        public IList<EntryPoint> reachable()
        {
            return reachable;
        }

        public EntryPoint all(int wx, int wy)
        {
            return find(wx, wy, all);
        }

        public EntryPoint active(int wx, int wy)
        {
            return find(wx, wy, active);
        }

        public EntryPoint reachable(int wx, int wy)
        {
            return find(wx, wy, reachable);
        }

        private Coo ctmp = new Coo();

        public COORDINATE randomReachable()
        {
            return randomReachable(RND.rInt());
        }

        public COORDINATE randomReachable(int rr)
        {
            if (reachable.size() <= 0)
            {
                return null;
            }
            EntryPoint p = reachable.getC(rr);
            ctmp.set(p.coo());
            if (!RND.oneIn(3))
            {
                DIR d = p.dirOut.next(-2 + RND.rInt(2) * 4);
                if (SETT.PATH().connectivity.is(ctmp, d))
                {
                    ctmp.increment(d.x(), d.y());
                }
            }
            if (SETT.PATH().connectivity.is(ctmp))
            {
                return ctmp;
            }
            return null;
        }

        private EntryPoint find(int wx, int wy, IList<EntryPoint> all)
        {
            if (all.size() == 0)
            {
                return null;
            }

            EntryPoint best = all.get(0);
            double bestD = double.MaxValue;

            for (int ei = 1; ei < all.size(); ei++)
            {
                EntryPoint ee = all.get(ei);
                double dist = ee.distanceValue(wx, wy);
                if (dist < bestD)
                {
                    bestD = dist;
                    best = ee;
                }
            }

            return best;
        }

        public bool hasAny()
        {
            return reachable.size() > 0;
        }

        public void update()
        {
            if (!dirty)
            {
                return;
            }

            reachable.clearSloppy();
            foreach (EntryPoint e in active)
            {
                e.reachable = SETT.PATH().connectivity.is(e.sCoo.x(), e.sCoo.y());
                if (e.reachable)
                {
                    reachable.add(e);
                }
            }
            dirty = false;
        }

        public void updateAvailability()
        {
            dirty = true;
        }

        public void render(Renderer r, RenderData renData)
        {
            foreach (EntryPoint b in active)
            {
                if (renData.tBounds().holdsPoint(b.coo()))
                {
                    if (b.reachable)
                    {
                        GCOLOR.MAP().BETTER.bind();
                        SPRITES.cons().ICO.scratch.render(r, b.coo().x() * C.TILE_SIZE - renData.offX1(), b.coo().y() * C.TILE_SIZE - renData.offY1());
                        GCOLOR.MAP().BEST.bind();
                    }
                    else
                    {
                        GCOLOR.MAP().BAD.bind();
                        SPRITES.cons().ICO.scratch.render(r, b.coo().x() * C.TILE_SIZE - renData.offX1(), b.coo().y() * C.TILE_SIZE - renData.offY1());
                        GCOLOR.MAP().SOSO.bind();
                    }
                    if (map.is(b.coo()))
                    {
                        COLOR.BLUE100.render(r, b.coo().x() * C.TILE_SIZE - renData.offX1(), b.coo().y() * C.TILE_SIZE - renData.offY1());
                    }
                    else
                    {
                        COLOR.MEDIUM_BROWN.render(r, b.coo().x() * C.TILE_SIZE - renData.offX1(), b.coo().y() * C.TILE_SIZE - renData.offY1());
                    }

                    foreach (DIR d in DIR.ORTHO)
                    {
                        int dx = b.coo().x() + d.x();
                        int dy = b.coo().y() + d.y();
                        if (SETT.IN_BOUNDS(dx, dy) && renData.tBounds().holdsPoint(dx, dy))
                        {
                            SPRITES.cons().ICO.scratch.render(r, dx * C.TILE_SIZE - renData.offX1(), dy * C.TILE_SIZE - renData.offY1());
                        }
                    }
                }
            }
            COLOR.unbind();
        }

        public class EntryPoint : INDEXED
        {
            public readonly int DIM;
            public readonly RECTANGLE body;
            private readonly Coo sCoo = new Coo();
            private readonly COORDINATE wCoo;
            public readonly DIR dirOut;
            private readonly int index;
            private bool reachable = false;
            private bool active = false;

            public void save(FilePutter file)
            {
                sCoo.save(file);
                file.bool(active);
                file.bool(reachable);
            }

            public void load(FileGetter file)
            {
                sCoo.load(file);
                active = file.bool();
                reachable = file.bool();
            }

            public void clear()
            {
                sCoo.set(body.cX(), body.cY());
                active = false;
                reachable = false;
            }

            public EntryPoint(int index, int x1, int x2, int y1, int y2, DIR dir, int wdx, int wdy)
            {
                this.body = new Rec(x1, x2, y1, y2);
                DIM = Math.Max(body.width(), body.height());
                this.dirOut = dir;
                sCoo.set(body.cX(), body.cY());
                wCoo = new Coo(wdx, wdy);
                this.index = index;
            }

            public int index()
            {
                return index;
            }

            public bool reachable()
            {
                return reachable;
            }

            public bool active()
            {
                return active;
            }

            public COORDINATE coo()
            {
                return sCoo;
            }

            public COORDINATE wCooD()
            {
                return wCoo;
            }

            public int wx()
            {
                return wCoo.x() + SETT.WORLD_AREA().tiles().x1();
            }

            public int wy()
            {
                return wCoo.y() + SETT.WORLD_AREA().tiles().y1();
            }

            public double distanceValue(int wx, int wy)
            {
                double ox = wx + 0.5;
                double oy = wy + 0.5;

                double x = wCoo.x() + SETT.WORLD_AREA().tiles().x1() + 0.5;
                double y = wCoo.y() + SETT.WORLD_AREA().tiles().y1() + 0.5;
                x += dirOut.x() * 0.5;
                y += dirOut.y() * 0.5;

                x -= ox;
                y -= oy;

                double dist = x * x + y * y;

                return dist;
            }
        }
    }
}