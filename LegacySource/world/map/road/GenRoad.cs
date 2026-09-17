using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Map.Road
{
    using static World.WORLD;
    using Snake2D.PathTile;
    using Snake2D.PathUtilOnline;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Map;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util;
    using World.Map.Regions;

    internal sealed class GenRoad
    {
        private readonly IntChecker dests = new IntChecker(WREGIONS.MAX);
        private readonly MAP_DOUBLE u;
        public readonly RReg[] all = new RReg[WREGIONS.MAX];
        private double[] dists = new double[WREGIONS.MAX];
        public readonly Polymap polly = new Polymap(TBOUNDS(), 6, 1);

        public GenRoad(ACTION util, MAP_DOUBLE infra)
        {
            this.u = infra;
            foreach (Region r in WORLD.REGIONS().all())
            {
                if (r.info.area() > 0)
                {
                    all[r.index()] = new RReg(r);
                }
            }

            Tree<RReg> sort = new Tree<GenRoad.RReg>(WREGIONS.MAX)
            {
                IsGreaterThan = (current, cmp) => current.lastValue > cmp.lastValue
            };

            foreach (RReg r in all)
            {
                if (r != null)
                {
                    r.init(dests);

                    sort.add(r);
                }
            }

            util.exe();
            int a = 0;
            while (sort.hasMore())
            {
                RReg r = sort.pollSmallest();
                if (++a > 10)
                {
                    a = 0;
                    util.exe();
                }

                if (r.changed)
                {
                    r.changed = false;
                    sort.add(r);
                }
                else if (findNextConnection(r))
                {
                    sort.add(r);
                }
            }
        }

        private bool findNextConnection(RReg r)
        {
            dests.init();

            double longest = 0;
            foreach (RDistance d in r.neighs)
            {
                dests.isSetAndSet(d.to.reg.index());
                dists[d.to.reg.index()] = d.dist;
                longest = Math.Max(longest, d.dist);
            }

            RReg nn = tryNeighs(r, dests, longest);

            if (nn != null)
            {
                nn.remove(r);
                r.remove(nn);
                return true;
            }

            PathTile t = r.findNext(dests);

            if (t == null)
                return false;

            connect(t);
            RReg to = map.get(t);
            to.remove(r);
            r.remove(to);

            to.dists.add(new RDistance(r, t.getValue()));
            r.dists.add(new RDistance(to, t.getValue()));

            return true;
        }

        private RReg tryNeighs(RReg home, IntChecker check, double longest)
        {
            if (home.dists.size() == 0)
                return null;

            Flooder f = GUTIL.flooder();
            f.init(this);

            foreach (RDistance d in home.dists)
            {
                if (!check.isSet(d.to.reg.index()))
                    f.pushSloppy(d.to.reg.cx(), d.to.reg.cy(), 0);
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getValue() > longest)
                    break;

                RReg current = map.get(t);

                if (check.isSet(current.reg.index()))
                {
                    if (t.getValue() < dists[current.reg.index()])
                    {
                        f.done();
                        return current;
                    }
                }

                foreach (RDistance d in current.dists)
                {
                    if (d.to != home)
                        f.pushSmaller(d.to.reg.cx(), d.to.reg.cy(), t.getValue() + d.dist, t);
                }
            }
            f.done();
            return null;
        }

        void connect(PathTile t)
        {
            WTRAV.makeRoad(t);
        }

        private class RReg
        {
            public readonly Region reg;
            private double lastValue = double.MaxValue;
            private readonly ArrayListGrower<RDistance> neighs = new ArrayListGrower<RDistance>();
            public readonly ArrayListGrower<RDistance> dists = new ArrayListGrower<RDistance>();
            internal bool changed = false;

            RReg(Region home)
            {
                this.reg = home;
            }

            void remove(RReg to)
            {
                foreach (RDistance d in neighs)
                {
                    if (d.to == to)
                    {
                        neighs.remove(d);
                        return;
                    }
                }
                changed = true;
                lastValue = double.MaxValue;
                foreach (RDistance d in neighs)
                {
                    if (d.dist < lastValue)
                        lastValue = d.dist;
                }
            }

            void init(IntChecker check)
            {
                check.init();

                foreach (RDistance d in neighs)
                {
                    check.isSetAndSet(d.to.reg.index());
                }

                Flooder f = GUTIL.flooder();
                f.init(this);
                f.pushSloppy(reg.info.cx(), reg.info.cy(), 0);
                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    RReg current = map.get(t);
                    if (current == null)
                        continue;

                    if (current != this)
                    {
                        if (!check.isSet(current.reg.index()))
                        {
                            if (t.isSameAs(current.reg.cx(), current.reg.cy()))
                            {
                                check.isSetAndSet(current.reg.index());
                                neighs.add(new RDistance(current, t.getValue()));
                                current.neighs.add(new RDistance(this, t.getValue()));
                                if (t.getValue() < lastValue)
                                    lastValue = t.getValue();
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    push(t);
                }
                f.done();
            }

            PathTile findNext(IntChecker check)
            {
                if (neighs.size() == 0)
                    return null;

                check.init();

                foreach (RDistance d in neighs)
                {
                    check.isSetAndSet(d.to.reg.index());
                }

                Flooder f = GUTIL.flooder();
                f.init(this);
                f.pushSloppy(reg.info.cx(), reg.info.cy(), 0);
                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    RReg current = map.get(t);
                    if (current == null)
                        continue;

                    if (current != this)
                    {
                        if (check.isSet(current.reg.index()))
                        {
                            if (t.isSameAs(current.reg.cx(), current.reg.cy()))
                            {
                                f.done();
                                return t;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    push(t);
                }
                f.done();
                neighs.clear();
                return null;
            }

            private void push(PathTile t)
            {
                RReg current = map.get(t);
                foreach (DIR d in DIR.ALL)
                {
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (WTRAV.canLand(t.x(), t.y(), d, false))
                    {
                        double v = u.get(dx, dy) + WTRAV.extracost(dx, dy, d);
                        RReg to = map.get(dx, dy);
                        if (to != current && !d.isOrtho())
                            continue;
                        if (current != this && current != to)
                            continue;
                        if (WTRAV.canLand(t.x(), t.y(), d, true))
                            v *= 0.5;
                        else if (WORLD.WATER().isBig.is(dx, dy))
                            v += 32;

                        GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + v * d.tileDistance(), t);
                    }
                }
            }
        }

        private static class RDistance
        {
            internal readonly RReg to;
            internal readonly double dist;

            RDistance(RReg to, double dist)
            {
                this.to = to;
                this.dist = dist;
            }
        }

        private readonly MAP_OBJECT<RReg> map = new MAP_OBJECT<RReg>()
        {
            Get = tile =>
            {
                Region r = WORLD.REGIONS().map.get(tile);
                if (r != null)
                    return all[r.index()];
                return null;
            },
            Get = (tx, ty) =>
            {
                Region r = WORLD.REGIONS().map.get(tx, ty);
                if (r != null)
                    return all[r.index()];
                return null;
            }
        };
    }
}