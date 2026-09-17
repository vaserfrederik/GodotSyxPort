using System;
using System.Collections.Generic;

namespace Settlement.Path.Thread
{
    using static Settlement.Main.SETT.IN_BOUNDS;
    using static Settlement.Main.SETT.PATH;

    using GAME = Game.GAME;
    using C = Init.Constant.C;
    using SETT = Settlement.Main.SETT;
    using SCOMPONENTS = Settlement.Path.Components.SCOMPONENTS;
    using SComponent = Settlement.Path.Components.SComponent;
    using SComponentChecker = Settlement.Path.Components.SComponentChecker;
    using SComponentEdge = Settlement.Path.Components.SComponentEdge;
    using SCompFinder = Settlement.Path.Components.Finder.SCompFinder;
    using SCompPath = Settlement.Path.Components.Finder.SCompFinder.SCompPath;
    using COST = Snake2D.PathGame.COST;
    using PathTile = Snake2D.PathTile;
    using PathUtilOnline = Snake2D.PathUtilOnline;
    using Flooder = Snake2D.PathUtilOnline.Flooder;
    using DIR = Snake2D.Util.Datatypes.DIR;

    internal sealed class SPathFinderThread
    {
        private double lastDistance;
        private readonly int chunkD;
        private readonly PathUtilOnline p;
        private readonly SCompFinder cf;
        private readonly SComponentChecker CHECK;

        public SPathFinderThread(SCOMPONENTS comps, PathUtilOnline p, int chunks)
        {
            this.p = p;
            cf = new SCompFinder(comps, p);
            CHECK = new SComponentChecker(comps.zero);
            chunkD = chunks;
        }

        public PathTile Find(int startX, int startY, int destX, int destY, bool full)
        {
            lastDistance = 0;
            SCompPath comp = cf.FindDest(startX, startY, destX, destY);
            if (comp == null)
                return null;

            lastDistance = comp.Distance();

            if (comp.Path().Count > chunkD)
            {
                return FindComp(comp, startX, startY, comp.Path()[comp.Path().Count - chunkD]);
            }
            return Find(comp, startX, startY, destX, destY, full);
        }

        private PathTile Find(SCompPath comp, int startX, int startY, int destX, int destY, bool full)
        {
            Flooder f = p.GetFlooder();
            f.Init(typeof(SPathFinderThread));

            f.PushSloppy(startX, startY, 0);
            f.SetValue2(startX, startY, 0);
            OpDist.Init(destX, destY);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                if (full && t.X() == destX && t.Y() == destY)
                {
                    f.Done();
                    return t;
                }

                if (Math.Abs(t.X() - destX) + Math.Abs(t.Y() - destY) == 1)
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

                for (int i = 0; i < DIR.ALL.Count; i++)
                {
                    DIR d = DIR.ALL[i];
                    int tx = t.X() + d.X();
                    int ty = t.Y() + d.Y();

                    if (!IN_BOUNDS(tx, ty))
                        continue;

                    double cost = SETT.PATH().huristics.GetCost(t.X(), t.Y(), tx, ty);
                    if (cost > 0)
                    {
                        cost *= d.TileDistance();

                        cost += t.GetValue2();
                        PathTile t2 = f.PushSmaller(tx, ty, cost + OpDist.Get(tx, ty), t);
                        if (t2 != null)
                            t2.SetValue2(cost);
                    }
                    else if (cost == COST.BLOCKED)
                    {
                        f.Close(tx, ty, 0);
                    }
                }
            }

            if (!PATH().WillUpdate())
            {
                GAME.Notify($"{startX} {startY} -> {destX} {destY} {full}");
            }

            f.Done();
            return null;
        }

        private PathTile FindComp(SCompPath comp, int startX, int startY, SComponent dest)
        {
            Flooder f = p.GetFlooder();
            f.Init(this);
            SComponentChecker check = CHECK;
            check.Init();

            check.IsSetAndSet(dest);
            SComponentEdge e = dest.EdgeFirst();
            while (e != null)
            {
                check.IsSetAndSet(e.To());
                e = e.Next();
            }

            OpDist.Init(dest.CentreX(), dest.CentreY());
            f.PushSloppy(startX, startY, 0);
            f.SetValue2(startX, startY, 0);
            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                SComponent c = SETT.PATH().comps.zero.Get(t);
                if (check.Is(c))
                {
                    f.Done();
                    return t;
                }

                for (int i = 0; i < DIR.ALL.Count; i++)
                {
                    DIR d = DIR.ALL[i];
                    int tx = t.X() + d.X();
                    int ty = t.Y() + d.Y();
                    if (!comp.Is(tx, ty))
                        continue;
                    double cost = SETT.PATH().huristics.GetCost(t.X(), t.Y(), tx, ty);
                    if (cost > 0)
                    {
                        cost *= d.TileDistance();
                        cost += t.GetValue2();
                        PathTile t2 = f.PushSmaller(tx, ty, cost + OpDist.Get(tx, ty), t);
                        if (t2 != null)
                            t2.SetValue2(cost);
                    }
                    else if (cost == COST.BLOCKED)
                    {
                        f.Close(tx, ty, 0);
                    }
                }
            }

            if (!PATH().WillUpdate())
            {
                GAME.Notify($"{startX} {startY} -> {dest.CentreX()} {dest.CentreY()}");
            }

            f.Done();
            return null;
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