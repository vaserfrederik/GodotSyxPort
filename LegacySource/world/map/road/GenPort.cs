using System;
using System.Collections.Generic;
using System.Linq;
using static world.WORLD;

namespace world.map.road
{
    internal class GenPort
    {
        private readonly IntChecker wCheck;
        public readonly LinkedList<Port> allports = new LinkedList<Port>();

        public readonly Bitsmap2D wmap;
        private readonly MAP_DOUBLE u;

        public Bitmap2D port = new Bitmap2D(WORLD.TBOUNDS(), false);
        public Bitmap2D oldPort = new Bitmap2D(WORLD.TBOUNDS(), false);

        public GenPort(ACTION u, MAP_DOUBLE infra)
        {
            this.u = infra;

            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    if (port.is(it.tile()))
                    {
                        if (oldPort.is(it.tile()))
                            COLOR.REDISH.bind();
                        else
                            COLOR.ORANGE100.bind();
                        SPRITES.cons().ICO.crosshair.render(r, it.x(), it.y());
                    }
                }
            };

            int wi = 0;
            {
                foreach (COORDINATE c in WORLD.TBOUNDS())
                {
                    GUTIL.flooder().setValue2(c, 0);
                }

                wi = 1;

                foreach (COORDINATE c in WORLD.TBOUNDS())
                {
                    if (WATER().isBig.is(c) && GUTIL.flooder().getValue2(c.x(), c.y()) == 0)
                        if (assignWater(wi, c.x(), c.y()))
                        {
                            wi++;
                        }
                }
                wi--;

                int kk = wi == 0 ? 32 : 32 - Math.Abs((wi & -wi).bitCount());

                wmap = new Bitsmap2D(0, kk, WORLD.TBOUNDS());

                foreach (COORDINATE c in WORLD.TBOUNDS())
                {
                    wmap.set(c, (int)CLAMP.d(GUTIL.flooder().getValue2(c.x(), c.y()) - 1, 0, wi));
                }
            }

            u.exe();
            wCheck = new IntChecker(wi + 1);

            int a = 0;

            foreach (Region r in REGIONS().all())
            {
                if (r.info.area() == 0)
                    continue;
                if (a++ > 10)
                {
                    a = 0;
                    u.exe();
                }
                createPorts(r);
            }

            new GenPortMini(u, port);
            new GenPortBuild(u, infra, port, oldPort);
        }

        private bool assignWater(int i, int x, int y)
        {
            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(x, y, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                t.setValue2(i);
                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR d = DIR.ALL.get(di);
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (WORLD.WATER().isBig.is(dx, dy) && WTRAV.can(t.x(), t.y(), d, false))
                    {
                        GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + d.tileDistance(), t);
                    }
                }
            }
            GUTIL.flooder().done();
            return true;
        }

        private void createPorts(Region home)
        {
            wCheck.init();
            int am = 0;

            foreach (COORDINATE c in home.info.bounds())
            {
                if (home.is(c) && WTRAV.HARBOUR.isPossible(c.x(), c.y(), false) && !wCheck.isSetAndSet(wmap.get(c)))
                {
                    am++;
                }
            }

            wCheck.init();

            foreach (COORDINATE c in home.info.bounds())
            {
                if (home.is(c) && WORLD.WATER().isBig.is(c) && WORLD.ROADS().is(c))
                {
                    if (!wCheck.isSetAndSet(wmap.get(c)))
                    {
                        port.set(c, true);
                        am--;
                    }
                    oldPort.set(c, true);
                }
            }

            while (createPort(home) && am > 0)
            {
                am--;
            }
        }

        private bool createPort(Region home)
        {
            Flooder f = GUTIL.flooder();
            f.init(this);

            PathTile hh = f.close(home.info.cx(), home.info.cy(), 0, null);

            for (int di = 0; di < DIR.ORTHO.size(); di++)
            {
                DIR d = DIR.ORTHO.get(di);
                int dx = home.info.cx() + d.x();
                int dy = home.info.cy() + d.y();
                if (WTRAV.isGoodLandTile(dx, dy) && WTRAV.canLand(home.info.cx(), home.info.cy(), d, false))
                {
                    f.pushSloppy(dx, dy, 0, hh);
                    f.setValue2(dx, dy, 0);
                }
            }

            PathTile backup = null;

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                Region current = WORLD.REGIONS().map.get(t);
                if (current == null || current != home)
                    continue;

                if (WORLD.WATER().isBig.is(t) && !wCheck.isSet(wmap.get(t)))
                {
                    if (WTRAV.isHarbour(t.x(), t.y()) && !WORLD.REGIONS().cTile.is(t))
                    {
                        wCheck.isSetAndSet(wmap.get(t));
                        f.done();
                        port.set(t, true);
                        return true;
                    }
                    else
                    {
                        backup = t;
                    }
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WTRAV.can(t.x(), t.y(), d, false))
                    {
                        int dx = t.x() + d.x();
                        int dy = t.y() + d.y();
                        double v = u.get(dx, dy) + WTRAV.cost(t.x(), t.y(), d);
                        if (WORLD.WATER().isBig.is(t))
                            v += 100;
                        f.pushSmaller(dx, dy, t.getValue() + v * d.tileDistance(), t);
                    }
                }
            }
            f.done();
            if (backup != null)
            {
                wCheck.isSetAndSet(wmap.get(backup));
                port.set(backup, true);
                return true;
            }
            return false;
        }
    }
}