using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.faction.diplomacy;
using snake2d;
using snake2d.util.sets;
using util;
using world;
using world.map.regions;

namespace world.map.pathing
{
    public class WRegFinder
    {
        private readonly RegDist[] regs = new RegDist[WREGIONS.MAX];
        private readonly ArrayList<RegDist> li = new ArrayList<RegDist>(WREGIONS.MAX);

        private int upI = -1;
        private Treaty lastTreaty = null;
        private BOOLEANO<Region> lastSelector = null;
        private int lx, ly;

        public WRegFinder()
        {
            for (int i = 0; i < regs.Length; i++)
                regs[i] = new RegDist();
        }

        public LIST<RegDist> all(Faction f, Treaty trav, WRegSel selector)
        {
            return all(f.capitolRegion().cx(), f.capitolRegion().cy(), trav, selector);
        }

        public LIST<RegDist> all(Region home, Treaty trav, WRegSel selector)
        {
            return all(home.cx(), home.cy(), trav, selector);
        }

        public LIST<RegDist> all(int tx, int ty, Treaty treaty, WRegSel selector)
        {
            if (upI == GAME.updateI() && tx == lx && ty == ly && lastTreaty == treaty && selector == lastSelector)
                return li;

            upI = GAME.updateI();
            lastTreaty = treaty;
            lastSelector = selector;
            lx = tx;
            ly = ty;

            final Region origin = reg(tx, ty);

            li.clearSloppy();

            LIST<PathTile> ll = WORLD.PATH().comps.finder.getComps(tx, ty);

            Flooder f = GUTIL.flooder();
            f.init(f);
            foreach (PathTile t in ll)
            {
                t = f.pushSloppy(t, t.getValue());
                isWater.set(t, false);
                prevReg.set(t, origin);
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                if (t.getParent() != null)
                {
                    t.setValue2(t.getParent().getValue2());
                    if (WORLD.WATER().isBig.is(t) && WORLD.ROADS().harbour.is(t))
                    {
                        isWater.set(t, true);
                    }
                }

                Region from = reg(t.x(), t.y());

                if (from != null && t.isSameAs(from.cx(), from.cy()) && selector.is(from))
                {
                    RegDist rr = regs[from.index()];
                    rr.reg = from;
                    rr.distance = (int)t.getValue();
                    rr.water = isWater.is(t);
                    li.add(rr);
                }

                if (from != null)
                {
                    prevReg.set(t, from);
                }
                else
                {
                    from = prevReg.get(t);
                }

                WComp c = WORLD.PATH().comps.get(t);
                for (int i = 0; i < c.neighs(); i++)
                {
                    WComp to = c.neigh(i);

                    Region rto = reg(to.x(), to.y());

                    double v = t.getValue() + c.dist(i);
                    if (treaty.can(origin, from, rto, to.x(), to.y(), v))
                        f.pushSmaller(to.x(), to.y(), v, t);
                }
            }
            f.done();
            return li;
        }

        public RegDist single(int tx, int ty, Treaty treaty, WRegSel selector)
        {
            final Region origin = reg(tx, ty);
            upI = -1;

            LIST<PathTile> ll = WORLD.PATH().comps.finder.getComps(tx, ty);

            Flooder f = GUTIL.flooder();
            f.init(f);
            foreach (PathTile t in ll)
            {
                t = f.pushSloppy(t, t.getValue());
                isWater.set(t, false);
                prevReg.set(t, origin);
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                if (t.getParent() != null)
                {
                    t.setValue2(t.getParent().getValue2());
                    if (WORLD.WATER().isBig.is(t) && WORLD.ROADS().harbour.is(t))
                    {
                        isWater.set(t, true);
                    }
                }

                Region from = reg(t.x(), t.y());

                if (from != null && t.isSameAs(from.cx(), from.cy()) && selector.is(from))
                {
                    RegDist rr = regs[from.index()];
                    rr.reg = from;
                    rr.distance = (int)t.getValue();
                    rr.water = isWater.is(t);
                    f.done();
                    return rr;
                }

                if (from != null)
                {
                    prevReg.set(t, from);
                }
                else
                {
                    from = prevReg.get(t);
                }

                WComp c = WORLD.PATH().comps.get(t);
                for (int i = 0; i < c.neighs(); i++)
                {
                    WComp to = c.neigh(i);

                    Region rto = reg(to.x(), to.y());

                    double v = t.getValue() + c.dist(i);
                    if (treaty.can(origin, from, rto, to.x(), to.y(), v))
                        f.pushSmaller(to.x(), to.y(), v, t);
                }
            }
            f.done();
            return null;
        }

        private Region reg(int x, int y)
        {
            // Implementation of reg method
            return null;
        }

        private class RegDist
        {
            public Region reg;
            public int distance;
            public bool water;
        }

        private class BOOLEANO<T>
        {
            public void set(PathTile t, bool value) { }
            public bool is(PathTile t) { return false; }
        }

        private class WRegSel
        {
            public bool is(Region from) { return false; }
        }

        private class Treaty
        {
            public virtual bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return false;
            }

            public static readonly Treaty REG_NEIGHBOURS = new TreatyRegNeighbours();
            public static readonly Treaty REG_NO_WALK = new TreatyRegNoWalk();
            public static readonly Treaty REG_ALLY = new TreatyRegAlly();
            public static readonly Treaty REG_ENEMY = new TreatyRegEnemy();
            public static readonly Treaty REG_ALL = new TreatyRegAll();
        }

        private class TreatyRegNeighbours : Treaty
        {
            public override bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return origin == null || to == null || to.faction() == null || to.faction() == FACTIONS.player();
            }
        }

        private class TreatyRegNoWalk : Treaty
        {
            public override bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return origin != null && from != null && from.faction() != null && from.faction() == origin.faction() && to != null && to.faction() != null && to.faction() == origin.faction();
            }
        }

        private class TreatyRegAlly : Treaty
        {
            public override bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return origin != null && from != null && from.faction() != null && from.faction() == origin.faction() && to != null && to.faction() != null && to.faction() == origin.faction() || from.faction() == FACTIONS.player();
            }
        }

        private class TreatyRegEnemy : Treaty
        {
            public override bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return origin != null && from != null && from.faction() != null && from.faction() == origin.faction() && to != null && to.faction() != null && to.faction() == origin.faction() || DIP.get(origin.faction(), from.faction()).ally;
            }
        }

        private class TreatyRegAll : Treaty
        {
            public override bool can(Region origin, Region from, Region to, int tx, int ty, double dist)
            {
                return true;
            }
        }
    }
}