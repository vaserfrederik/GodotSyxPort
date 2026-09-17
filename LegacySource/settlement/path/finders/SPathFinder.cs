using System;
using System.Collections.Generic;

namespace Settlement.Path.Finders
{
    public static class SETT
    {
        public static bool IN_BOUNDS(int x, int y) => throw new NotImplementedException();
        public static PATH PATH() => throw new NotImplementedException();
    }

    public static class GAME
    {
        public static void Notify(string message) => throw new NotImplementedException();
    }

    public static class C
    {
        public const double SQR2 = 1.41421356237; // Approximation of square root of 2
    }

    public static class snake2d
    {
        public interface PathTile
        {
            PathTile Parent { get; set; }
            int X { get; }
            int Y { get; }
            double GetValue2();
            void SetValue2(double value);
        }

        public interface PathUtilOnline
        {
            Flooder GetFlooder();
        }

        public interface Flooder
        {
            void Init(Type type);
            void PushSloppy(int x, int y, double value);
            void SetValue2(int x, int y, double value);
            bool HasMore();
            PathTile PollSmallest();
            PathTile PushSmaller(int x, int y, double value, PathTile parent);
            void Close(int x, int y, double value);
            void Done();
        }

        public static class COST
        {
            public const double BLOCKED = double.MaxValue;
        }

        public enum DIR
        {
            NORTH, EAST, SOUTH, WEST
        }

        public static class DIR_ALL
        {
            public static readonly DIR[] ALL = { DIR.NORTH, DIR.EAST, DIR.SOUTH, DIR.WEST };
        }

        public static class PathUtilOnline_Flooder
        {
            public static Flooder GetFlooder(PathUtilOnline p) => throw new NotImplementedException();
        }
    }

    public static class settlement
    {
        public static class main
        {
            public static class SETT
            {
                public static PATH PATH() => throw new NotImplementedException();
                public static bool IN_BOUNDS(int x, int y) => throw new NotImplementedException();
            }
        }
    }

    public static class settlement_path_components
    {
        public static class SPathUtilResult
        {
            public int destX, destY;
            public snake2d.PathTile t;
        }

        public static class SCompFinder
        {
            private readonly SPathUtilResult res = new SPathUtilResult();
            private readonly snake2d.Coo coo = new snake2d.Coo();
            private readonly snake2d.PathUtilOnline p;
            public readonly SCompFinder cf;
            private readonly SPathFinderDest fDest;
            private readonly SComponentChecker CHECK;
            private readonly int chunkD;

            public SCompFinder(SCOMPONENTS comps, snake2d.PathUtilOnline p, int chunks)
            {
                this.p = p;
                cf = new SCompFinder(comps, p);
                fDest = new SPathFinderDest(p);
                CHECK = new SComponentChecker(comps.zero);
                chunkD = chunks;
            }

            public snake2d.PathTile Find(int startX, int startY, int destX, int destY, bool full)
            {
                double lastDistance = 0;
                SCompPath comp = cf.FindDest(startX, startY, destX, destY);
                if (comp == null)
                    return null;

                lastDistance = comp.distance();

                if (comp.path().Count > chunkD)
                {
                    return FindComp(comp, startX, startY, comp.path()[comp.path().Count - 1]);
                }

                return Find(comp, startX, startY, destX, destY, full);
            }

            public snake2d.PathTile CDebug(int startX, int startY, int destX, int destY, bool full)
            {
                double lastDistance = 0;
                SCompPath comp = cf.FindDest(startX, startY, destX, destY);
                if (comp == null)
                    return null;

                lastDistance = comp.distance();

                if (comp.path().Count > chunkD)
                {
                    return FindComp(comp, startX, startY, comp.path()[comp.path().Count - 1]);
                }

                return Find(comp, startX, startY, destX, destY, full);
            }

            public snake2d.PathTile Reverse(snake2d.PathTile abs)
            {
                if (abs.Parent != null)
                {
                    snake2d.PathTile p = abs.Parent;
                    abs.Parent = null;
                    abs = Reverse(abs, p);
                }
                return abs;
            }

            private snake2d.PathTile Reverse(snake2d.PathTile newParent, snake2d.PathTile t)
            {
                if (t.Parent == null)
                {
                    t.Parent = newParent;
                    return t;
                }
                snake2d.PathTile res = Reverse(t, t.Parent);
                t.Parent = newParent;
                return res;
            }

            private snake2d.PathTile Find(SCompPath comp, int startX, int startY, int destX, int destY, bool full)
            {
                snake2d.Flooder f = p.GetFlooder();
                f.Init(typeof(SPathFinder));
                f.PushSloppy(startX, startY, 0);
                f.SetValue2(startX, startY, 0);
                OpDist.Init(destX, destY);

                while (f.HasMore())
                {
                    snake2d.PathTile t = f.PollSmallest();

                    if (full && t.X == destX && t.Y == destY)
                    {
                        f.Done();
                        return t;
                    }

                    if (Math.Abs(t.X - destX) + Math.Abs(t.Y - destY) == 1)
                    {
                        if (full)
                        {
                            t = f.Force((short)destX, (short)destY, t.GetValue2(), t);
                        }
                        f.Done();
                        return t;
                    }

                    if (!comp.Is(t))
                        continue;

                    for (int i = 0; i < snake2d.DIR_ALL.ALL.Length; i++)
                    {
                        snake2d.DIR d = snake2d.DIR_ALL.ALL[i];
                        int tx = t.X + (int)d;
                        int ty = t.Y + (int)d;

                        if (!SETT.IN_BOUNDS(tx, ty))
                            continue;

                        double cost = settlement.main.SETT.PATH().huristics.GetCost(t.X, t.Y, tx, ty);
                        if (cost > 0)
                        {
                            cost *= d.tileDistance();

                            cost += t.GetValue2();
                            snake2d.PathTile t2 = f.PushSmaller(tx, ty, cost + OpDist.Get(tx, ty), t);
                            if (t2 != null)
                                t2.SetValue2(cost);
                        }
                        else if (cost == snake2d.COST.BLOCKED)
                        {
                            f.Close(tx, ty, cost);
                        }
                    }
                }

                return null;
            }

            private snake2d.PathTile FindComp(SCompPath comp, int startX, int startY, SComponent target)
            {
                // Implementation of FindComp
                throw new NotImplementedException();
            }

            private static class OpDist
            {
                private static int destX, destY;
                private static double weight = 0.7;

                public static void Init(int dx, int dy)
                {
                    destX = dx;
                    destY = dy;
                }

                private static double Get(int x, int y)
                {
                    x = Math.Abs(x - destX);
                    y = Math.Abs(y - destY);

                    if (x > y)
                    {
                        return weight * (C.SQR2 * y + x - y);
                    }
                    else if (x < y)
                    {
                        return weight * (C.SQR2 * x + y - x);
                    }
                    else
                    {
                        return weight * C.SQR2 * x;
                    }
                }
            }
        }
    }
}