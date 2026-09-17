using System;
using System.Collections.Generic;

namespace settlement.path.finders
{
    static class SETT
    {
        public static readonly bool IN_BOUNDS = true;
        public static readonly int THEIGHT = 100;
        public static readonly int TWIDTH = 100;
        public static readonly PATH PATH = new PATH();
    }

    class PATH
    {
        public readonly HURISTICS huristics = new HURISTICS();
    }

    class HURISTICS
    {
        public double getCost(int x1, int y1, int x2, int y2)
        {
            // Placeholder for actual cost calculation
            return 1.0;
        }
    }

    class GAME
    {
        public static void Notify(string message)
        {
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine(message);
        }
    }

    class LOG
    {
        public static void ln(string message)
        {
            Console.WriteLine(message);
        }

        public static void ln(int value)
        {
            Console.WriteLine(value);
        }
    }

    static class DIR
    {
        public static readonly List<DIRECTION> ORTHO = new List<DIRECTION>
        {
            new DIRECTION(1, 0),
            new DIRECTION(-1, 0),
            new DIRECTION(0, 1),
            new DIRECTION(0, -1)
        };
    }

    class DIRECTION
    {
        public readonly int x, y;

        public DIRECTION(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int tileDistance()
        {
            return 1;
        }
    }

    static class COST
    {
        public const double BLOCKED = -1.0;
    }

    interface PathGame
    {
        interface COST
        {
            double BLOCKED { get; }
        }
    }

    interface PathTile
    {
        int x();
        int y();
        double getValue();
        double getValue2();
    }

    interface PathUtilOnline
    {
        Flooder getFlooder();
    }

    interface Flooder
    {
        void init(Type type);
        bool hasMore();
        PathTile pollSmallest();
        void pushSloppy(int x, int y, double value);
        void pushSmaller(int x, int y, double value, PathTile parent);
        PathTile force(short x, short y, double value, PathTile parent);
        void close(int x, int y, double value);
        void done();
    }

    interface SComponent
    {
        bool is(int x, int y);
        int centreX();
        int centreY();
        int index();
    }

    class SComp0Level
    {
        public const int SIZE = 5;
    }

    interface SCompPath
    {
        List<SComponent> path();
        int distance();
    }

    interface SFINDER
    {
        bool isInComponent(SComponent target, int distance);
        bool isTile(int x, int y, int level);
    }

    class SPathFinder
    {
    }

    class SPathFinderDest
    {
        private readonly PathUtilOnline p;

        public SPathFinderDest(PathUtilOnline p)
        {
            this.p = p;
        }

        PathTile findDest(int startX, int startY, SCompPath comp, SFINDER finder)
        {
            SComponent parent = comp.path()[0];
            final SComponent target = parent;

            if (!finder.isInComponent(target, comp.distance()))
            {
                LOG.ln("nay");
                LOG.ln(startX + " " + startY + " " + comp.path().Count);
                LOG.ln(target.centreX() + " " + target.centreY());
                LOG.ln();
                return null;
            }

            Flooder f = p.getFlooder();
            f.init(typeof(SPathFinder));

            if (comp.path().Count >= 2)
            {
                parent = comp.path()[1];
                markNeigh(parent, target);
            }
            else
                f.pushSloppy(startX, startY, 0);

            int tiles = 0;
            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                tiles++;

                if (finder.isTile(t.x(), t.y(), 0))
                {
                    f.done();
                    return t;
                }

                if (!target.is(t.x(), t.y()))
                    continue;

                for (int i = 0; i < DIR.ORTHO.Count; i++)
                {
                    DIRECTION d = DIR.ORTHO[i];
                    int tx = t.x() + d.x;
                    int ty = t.y() + d.y;
                    if (!SETT.IN_BOUNDS)
                        continue;
                    if (finder.isTile(tx, ty, 0))
                    {
                        t = f.force((short)tx, (short)ty, t.getValue2(), t);
                        f.done();
                        return t;
                    }

                    double cost = SETT.PATH().huristics.getCost(t.x(), t.y(), tx, ty);
                    if (cost > 0)
                    {
                        cost *= d.tileDistance();
                        f.pushSmaller(tx, ty, t.getValue() + cost, t);
                    }
                    else if (cost == COST.BLOCKED)
                    {
                        f.close(tx, ty, 0);
                    }
                }
            }

            if (!SETT.PATH().willUpdate())
                GAME.Notify("nono " + startX + " " + startY + " " + parent.centreX() + " " + parent.centreY() + " " + target.centreX() + " " + target.centreY() + " " + tiles + " " + (target.level().get(target.centreX(), target.centreY()) + " " + comp.path().Count + " " + target));

            f.done();
            return null;
        }

        final void markNeigh(SComponent current, SComponent parent)
        {
            int x1 = (current.centreX() & ~(SComp0Level.SIZE - 1));
            int y1 = (current.centreY() & ~(SComp0Level.SIZE - 1));

            if (x1 - 1 >= 0)
            {
                int x = x1 - 1;
                bool hit = false;
                for (int y = -1; y < SComp0Level.SIZE; y++)
                {
                    if (parent.is(x, y + y1))
                    {
                        p.getFlooder().pushSloppy(x, y + y1, 0);
                        hit |= true;
                    }
                }
                if (hit)
                    return;
            }

            if (x1 + SComp0Level.SIZE <= SETT.TWIDTH)
            {
                int x = x1 + SComp0Level.SIZE;
                bool hit = false;
                for (int y = 0; y <= SComp0Level.SIZE; y++)
                {
                    if (parent.is(x, y + y1))
                    {
                        p.getFlooder().pushSloppy(x, y + y1, 0);
                        hit |= true;
                    }
                }
                if (hit)
                    return;
            }

            if (y1 - 1 >= 0)
            {
                int y = y1 - 1;
                bool hit = false;
                for (int x = 0; x <= SComp0Level.SIZE; x++)
                {
                    if (parent.is(x + x1, y))
                    {
                        p.getFlooder().pushSloppy(x + x1, y, 0);
                        hit |= true;
                    }
                }
                if (hit)
                    return;
            }

            if (y1 + SComp0Level.SIZE <= SETT.THEIGHT)
            {
                int y = y1 + SComp0Level.SIZE;
                bool hit = false;
                for (int x = -1; x < SComp0Level.SIZE; x++)
                {
                    if (parent.is(x + x1, y))
                    {
                        p.getFlooder().pushSloppy(x + x1, y, 0);
                        hit |= true;
                    }
                }
                if (hit)
                    return;
            }
            GAME.Error(parent.index() + " " + current.index());
        }
    }
}