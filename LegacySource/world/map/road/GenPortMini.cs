using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Map.Road
{
    public class GenPortMini
    {
        private readonly Bitmap2D debug;
        private readonly Action util;
        private readonly List<Reg> regs = new List<Reg>(WREGIONS.MAX);
        private readonly ArrayListGrower<Port> ports = new ArrayListGrower<Port>();
        private readonly MAP_BOOLEANE marked;

        public GenPortMini(Action util, MAP_BOOLEANE marked)
        {
            this.util = util;
            this.marked = marked;
            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    if (marked.is(it.tile()))
                    {
                        COLOR.BLUEISH.bind();
                        SPRITES.cons().ICO.crosshair.render(r, it.x(), it.y());
                        COLOR.unbind();
                    }
                    else if (debug.is(it.tile()))
                    {
                        COLOR.ORANGE100.bind();
                        SPRITES.cons().BIG.line.render(r, 0, it.x(), it.y());
                        COLOR.unbind();
                    }
                }
            };

            foreach (Region r in WORLD.REGIONS().all())
            {
                regs.Add(new Reg(r));
            }

            SetLand();
            foreach (Port p in ports)
            {
                p.dists = new double[ports.size()];
                p.dists.Fill(Double.MaxValue);
            }

            util.exe();
            SetPortDists();
            util.exe();

            foreach (Port p in new ArrayList<Port>(ports))
            {
                if (!IsConnected(p))
                {
                    marked.set(p, false);
                }
            }
        }

        private void SetLand()
        {
            Flooder f = GUTIL.flooder();
            f.init(this);

            foreach (Region r in WORLD.REGIONS().all())
            {
                if (r.info.area() > 0)
                {
                    f.pushSloppy(r.cx(), r.cy(), 0, null);
                    f.setValue2(r.cx(), r.cy(), r.index());
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getParent() != null)
                    t.setValue2(t.getParent().getValue2());

                Region rr = WORLD.REGIONS().map.get(t);

                foreach (DIR d in DIR.ALL)
                {
                    if (WTRAV.can(t.x(), t.y(), d, true))
                    {
                        Region other = WORLD.REGIONS().map.get(t, d);
                        if (other != rr)
                            continue;
                        f.pushSmaller(t, d, t.getValue() + d.tileDistance(), t);
                    }
                }
            }

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (marked.is(c))
                {
                    Port p = new Port(c, map.get(c), 0, ports.size());
                    map.get(c).ports.add(p);
                    ports.add(p);
                }

                if (f.hasBeenPushed(c.x(), c.y()))
                {
                    if (!WTRAV.LAND.isPossible(c.x(), c.y(), true))
                        continue;
                    else
                    {
                        int from = (int)f.getValue2(c.x(), c.y());
                        foreach (DIR d in DIR.ALL)
                        {
                            if (f.hasBeenPushed(c.x(), c.y(), d) && WTRAV.canLand(c.x(), c.y(), d, true))
                            {
                                int regTo = (int)f.getValue2(c.x(), c.y(), d);
                                if (from != regTo)
                                {
                                    double dd = f.getValue(c) + f.getValue(c.x() + d.x(), c.y() + d.y());
                                    Connect(f.get(c.x(), c.y()));
                                    regs[from].land.add(new Reg.Connect(regs[regTo], dd));
                                    regs[regTo].land.add(new Reg.Connect(regs[from], dd));
                                }
                            }
                        }
                    }
                }
            }

            f.done();

            foreach (Reg home in regs)
            {
                if (home.land.size() == 0)
                    continue;
                if (home.reg.index() % 10 == 0)
                    util.exe();
                f.init(this);
                f.pushSloppy(home.reg.cx(), home.reg.cy(), 0, null);

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    Reg o = map.get(t);
                    o.dists[home.reg.index()] = t.getValue();
                    home.dists[o.reg.index()] = t.getValue();
                    foreach (Reg.Connect l in o.land)
                    {
                        f.pushSmaller(l.to.reg.cx(), l.to.reg.cy(), t.getValue() + l.cost, t);
                    }
                }
                f.done();
            }
        }

        private void Connect(PathTile t)
        {
            while (t != null)
            {
                debug.set(t, true);
                t = t.getParent();
            }
        }

        private void SetPortDists()
        {
            Flooder f = GUTIL.flooder();
            f.init(this);
            foreach (Port p in ports)
            {
                f.pushSloppy(p.x(), p.y(), 0, null);
                f.setValue2(p, p.index);
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getParent() != null)
                    t.setValue2(t.getParent().getValue2());

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.WATER().isBig.is(t, d) && WTRAV.can(t.x(), t.y(), d, true))
                    {
                        f.pushSmaller(t, d, t.getValue() + d.tileDistance(), t);
                    }
                }
            }

            foreach (Port p in ports)
            {
                p.dists = new double[ports.size()];
                p.dists.Fill(Double.MaxValue);
            }

            foreach (Port p in ports)
            {
                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.WATER().isBig.is(p, d) && WTRAV.can(p.x(), p.y(), d, true))
                    {
                        f.pushSmaller(p, d, p.getValue() + d.tileDistance(), p);
                    }
                }
            }

            foreach (Reg home in regs)
            {
                if (home.ports.size() == 0)
                    continue;
                if (home.reg.index() % 10 == 0)
                    util.exe();
                f.init(this);
                f.pushSloppy(home.reg.cx(), home.reg.cy(), 0, null);

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    Reg o = map.get(t);
                    o.dists[home.reg.index()] = t.getValue();
                    home.dists[o.reg.index()] = t.getValue();
                    foreach (Reg.Connect l in o.ports)
                    {
                        f.pushSmaller(l.to.reg.cx(), l.to.reg.cy(), t.getValue() + l.cost, t);
                    }
                }
                f.done();
            }
        }

        private bool IsConnected(Port p)
        {
            foreach (Port o in ports)
            {
                double w = p.dists[o.index];
                double l = p.reg.dists[o.reg.reg.index()];

                if (w == Double.MaxValue)
                    continue;

                if (l == Double.MaxValue)
                    return true;

                w += o.cost;
                w += p.cost;

                if ((w + 32) < l)
                    return true;
            }

            return false;
        }

        public class Reg
        {
            public readonly Region reg;
            public ArrayListGrower<Connect> land = new ArrayListGrower<>();
            public ArrayListGrower<Port> ports = new ArrayListGrower<>();
            public readonly double[] dists;

            private Reg(Region reg)
            {
                this.reg = reg;
                dists = new double[WREGIONS.MAX];
                dists.Fill(Double.MaxValue);
            }

            public static class Connect
            {
                public readonly Reg to;
                public readonly double cost;

                public Connect(Reg to, double cost)
                {
                    this.to = to;
                    this.cost = cost;
                }
            }
        }

        public MAP_OBJECT<Reg> map = new MAP_OBJECT<Reg>()
        {
            public Reg get(int tx, int ty)
            {
                if (WORLD.IN_BOUNDS(tx, ty))
                    return get(tx + ty * WORLD.TWIDTH());
                return null;
            }

            public Reg get(int tile)
            {
                Region reg = WORLD.REGIONS().map.get(tile);
                if (reg != null)
                {
                    return regs[reg.index()];
                }
                return null;
            }
        };

        private static class Port : Coo
        {
            private readonly int index;
            private readonly Reg reg;
            private readonly double cost;
            private double[] dists;
            private ArrayListGrower<Connect> cons = new ArrayListGrower<>();

            private Port(COORDINATE c, Reg reg, double cost, int index) : base(c)
            {
                this.reg = reg;
                this.cost = cost;
                this.index = index;
            }

            public static class Connect
            {
                public readonly Port to;
                public readonly double cost;

                public Connect(Port to, double cost)
                {
                    this.to = to;
                    this.cost = cost;
                }
            }
        }

        public MAP_OBJECT<Port> port = new MAP_OBJECT<Port>()
        {
            public Port get(int tx, int ty)
            {
                if (WORLD.IN_BOUNDS(tx, ty))
                    return get(tx + ty * WORLD.TWIDTH());
                return null;
            }

            public Port get(int tile)
            {
                Reg reg = map.get(tile);
                if (reg != null)
                {
                    for (int i = 0; i < reg.ports.size(); i++)
                    {
                        if (reg.ports.get(i).x() + reg.ports.get(i).y() * WORLD.TWIDTH() == tile)
                            return reg.ports.get(i);
                    }
                }
                return null;
            }
        };

        public Reg get(Region reg)
        {
            return regs[reg.index()];
        }
    }
}