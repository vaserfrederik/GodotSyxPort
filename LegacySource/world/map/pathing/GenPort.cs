using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.PathUtilOnline;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util;
using util.rendering;
using world;
using world.map.regions;
using world.map.road;

namespace world.map.pathing
{
    final class GenPort
    {
        private readonly ArrayListResize<Port> ports = new ArrayListResize<Port>(WREGIONS.MAX, WREGIONS.MAX * 10);
        private readonly Bitsmap2D wRegs;

        public GenPort(ACTION util)
        {
            wRegs = new GenPortRegs(util);

            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    if (WORLD.PATH().portArea.is(it.tile()))
                    {
                        COLOR.UNIQUE.getC(wRegs.get(it.tile())).bind();
                        SPRITES.cons().BIG.outline.render(r, 0, it.x(), it.y());
                        COLOR.unbind();
                    }
                    if (WORLD.PATH().map.is.is(it.tile()))
                    {
                        COLOR.ORANGE100.bind();
                        for (int di = 0; di < DIR.ALL.size(); di++)
                        {
                            DIR d = DIR.ALL.get(di);
                            if (WORLD.PATH().map.can(it.tx(), it.ty(), d))
                                SPRITES.cons().ICO.arrows2.get(d.id()).render(r, it.x(), it.y());
                        }
                        COLOR.unbind();
                    }
                }
            };

            Flooder f = GUTIL.flooder();

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (mPort.is(c))
                {
                    f.setValue2(c, ports.size());
                    ports.add(new Port(ports.size(), c));
                }
            }

            setPortAreas(util);
            util.exe();
            connectNeighs(util);
            util.exe();

            LIST<PortGroup> groups = makeGroups();

            Bitmap2D network = network(ports);
            Bitmap1D check = new Bitmap1D(ports.size(), false);
            foreach (PortGroup g in groups)
            {
                final int whome = wRegs.get(g.all.get(0).coo);
                check.clear();
                f.init(this);
                foreach (Port p in g.all)
                    f.pushSloppy(p.coo, 0);
                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();

                    int wnow = wRegs.get(t);

                    if (mPort.is(t))
                    {
                        if (wnow != whome)
                        {
                            Port po = ports.get((int)t.getValue2());
                            if (!check.get(po.group.index))
                            {
                                check.set(wnow, true);
                                Gen.connect(t);
                            }
                        }
                    }
                    foreach (DIR d in DIR.ALL)
                    {
                        if (network.is(t, d) || WORLD.PATH().map.can(t, d))
                        {
                            double v = 1;
                            if (!WORLD.PATH().map.can(t, d))
                                v = 8;
                            f.pushSmaller(t, d, t.getValue() + v * d.tileDistance(), t);
                        }
                    }
                }
                f.done();
                util.exe();
            }

            util.exe();
        }

        private void setPortAreas(ACTION util)
        {
            WORLD.PATH().portArea.clear();
            Flooder f = GUTIL.flooder();
            f.init(this);
            foreach (Port p in ports)
            {
                Region reg = WORLD.REGIONS().map.get(p.coo);
                if (reg != null)
                {
                    f.pushSloppy(p.coo.x(), p.coo.y(), 0, null);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                Region reg = WORLD.REGIONS().map.get(t);
                if (reg != null)
                {
                    WORLD.PATH().portArea.set(t, true);
                }
            }

            f.done();
            util.exe();
        }

        private void connectNeighs(ACTION util)
        {
            foreach (Port p in ports)
            {
                Flooder f = GUTIL.flooder();
                f.init(this);
                f.pushSloppy(p.coo, 0);

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    Region reg = WORLD.REGIONS().map.get(t);
                    if (reg != null && !WORLD.PATH().map.is.is(t))
                    {
                        WORLD.PATH().map.set(t, true);
                    }
                }

                f.done();
            }

            util.exe();
        }

        private LIST<PortGroup> makeGroups()
        {
            LIST<PortGroup> groups = new ArrayListResize<PortGroup>(WREGIONS.MAX);
            Flooder f = GUTIL.flooder();
            int gi = 0;
            foreach (Port p in ports)
            {
                if (p.group != null)
                    continue;

                final int wi = wRegs.get(p.coo);

                PortGroup g = new PortGroup(gi++);
                groups.add(g);
                f.init(this);
                f.pushSloppy(p.coo, 0);

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    if (wRegs.get(t) != wi)
                        continue;

                    if (mPort.is(t))
                    {
                        Port po = ports.get((int)t.getValue2());
                        g.all.add(po);
                        po.group = g;
                    }
                    foreach (DIR d in DIR.ALL)
                    {
                        if (WORLD.PATH().map.can(t, d) && WORLD.WATER().isBig.is(t, d))
                            f.pushSmaller(t, d, t.getValue() + d.tileDistance());
                    }
                }

                f.done();
            }

            groups.shuffle();
            return groups;
        }

        private Bitmap2D coast()
        {
            Bitmap2D coast = new Bitmap2D(WORLD.TWIDTH(), WORLD.THEIGHT(), false);
            Flooder f = GUTIL.flooder();
            f.init(this);

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.WATER().isBig.is(c) && !WORLD.WATER().coversTile.is(c))
                {
                    f.pushSloppy(c.x(), c.y(), 0, null);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                coast.set(t, true);
                if (t.getValue() > 3)
                    continue;

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.WATER().isBig.is(t, d))
                        f.pushSmaller(t, d, t.getValue() + d.tileDistance());
                }
            }

            f.done();
            return coast;
        }

        private Bitmap2D network(LIST<Port> ports)
        {
            Bitmap2D network = new Bitmap2D(WORLD.TBOUNDS(), false);
            Polymap polly = new Polymap(TBOUNDS(), 12, 1);

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (WORLD.WATER().isBig.is(c) && polly.isEdge(c.x(), c.y()))
                {
                    network.set(c, true);
                }
            }

            Flooder f = GUTIL.flooder();
            f.init(this);

            foreach (Port p in ports)
            {
                f.pushSloppy(p.coo, 0);
            }

            Bitmap1D check = new Bitmap1D(ports.size(), false);

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getParent() != null)
                    t.setValue2(t.getParent().getValue2());
                if (check.get((int)t.getValue2()))
                    continue;

                if (network.is(t))
                {
                    check.set((int)t.getValue2(), true);
                    while (t != null)
                    {
                        network.set(t, true);
                        t = t.getParent();
                    }
                    continue;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.WATER().isBig.is(t, d))
                    {
                        double v = 1;
                        if (!WORLD.PATH().map.can(t, d))
                            v = 4;
                        f.pushSmaller(t, d, t.getValue() + v * d.tileDistance());
                    }
                }
            }

            f.done();
            return network;
        }

        public readonly MAP_BOOLEAN mPort = new MAP_BOOLEAN()
        {
            public override bool is(int tx, int ty)
            {
                if (!WORLD.WATER().isBig.is(tx, ty))
                    return false;

                if (WORLD.ROADS().harbour.is(tx, ty) && !WORLD.ROADS().bridge.is(tx, ty))
                {
                    return true;
                }
                else if (WORLD.REGIONS().cTile.get(tx, ty) != null)
                {
                    return true;
                }
                return false;
            }

            public override bool is(int tile)
            {
                return false;
            }
        };

        private class Port
        {
            public readonly Coo coo = new Coo();
            public PortGroup group;

            public Port(int index, COORDINATE c)
            {
                this.coo.set(c);
            }

            public Region region()
            {
                return WORLD.REGIONS().map.get(coo);
            }
        }

        private class PortGroup
        {
            public readonly ArrayListGrower<Port> all = new ArrayListGrower<Port>();
            public readonly int index;

            public PortGroup(int index)
            {
                this.index = index;
            }
        }

        private class Connection : INDEXED
        {
            private double cost = Double.MaxValue;
            private PathTile a;
            private PathTile b;
            private readonly int index;

            public Connection(int ii)
            {
                index = ii;
            }

            public override int index()
            {
                return index;
            }
        }

        public int ii(int a, int b)
        {
            if (a > b)
            {
                int c = a;
                a = b;
                b = c;
            }
            return a * ports.size() + b;
        }
    }
}