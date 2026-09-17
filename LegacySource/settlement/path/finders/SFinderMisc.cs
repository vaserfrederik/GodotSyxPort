using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using static settlement.main.SETT;

namespace settlement.path.finders
{
    public abstract class SFinderMisc
    {
        private readonly double max;

        protected SFinderMisc(int max)
        {
            this.max = max;
        }

        protected virtual bool Has()
        {
            return true;
        }

        public abstract bool IsTile(int tx, int ty);

        public bool Find(COORDINATE start, SPath path)
        {
            if (!Has())
                return false;

            if (IsTile(start.x(), start.y()))
                return false;

            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy(start.x(), start.y(), 0, null);

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();

                if (IsTile(t.x(), t.y()))
                {
                    path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                    GUTIL.Flooder().Done();
                    return true;
                }

                foreach (DIR d in DIR.ALL)
                {
                    int dx = d.x() + t.x();
                    int dy = d.y() + t.y();
                    if (!IN_BOUNDS(dx, dy))
                        continue;
                    if (!PATH().connectivity.Is(dx, dy))
                        continue;

                    double v = PATH().huristics.GetCost(t.x(), t.y(), dx, dy);
                    if (v < 0)
                        continue;
                    v *= d.TileDistance();
                    v += t.GetValue();
                    if (v <= max)
                        GUTIL.Flooder().PushSmaller(dx, dy, v, t);
                }
            }
            GUTIL.Flooder().Done();

            return false;
        }

        public static class FinderIdle
        {
            private readonly List<DIR> dirs = new List<DIR>(DIR.ALL);

            public FinderIdle()
            {
            }

            public bool Find(COORDINATE start, SPath path, ENTITY e)
            {
                if (IsGoodTileToStandOn(start.x(), start.y(), e))
                    return false;

                GUTIL.Flooder().Init(null);
                PathTile t = GUTIL.Flooder().PushSloppy(start.x(), start.y(), 0, null);

                while (GUTIL.Flooder().HasMore())
                {
                    t = GUTIL.Flooder().PollSmallest();

                    if (IsGoodTileToStandOn(t.x(), t.y(), e))
                    {
                        path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                        GUTIL.Flooder().Done();
                        return true;
                    }

                    foreach (DIR d in dirs)
                    {
                        int dx = d.x() + t.x();
                        int dy = d.y() + t.y();
                        if (!IN_BOUNDS(dx, dy))
                            continue;
                        if (!PATH().connectivity.Is(dx, dy))
                            continue;

                        double v = PATH().huristics.GetCost(t.x(), t.y(), dx, dy);
                        if (v < 0)
                            continue;
                        v *= d.TileDistance();
                        v += t.GetValue();
                        if (v <= 40)
                            GUTIL.Flooder().PushSmaller(dx, dy, v, t);
                    }
                }
                GUTIL.Flooder().Done();

                return false;
            }

            private bool IsGoodTileToStandOn(int tx, int ty, ENTITY e)
            {
                if (PATH().availability.Get(tx, ty).player < 0)
                    return false;
                if (PATH().availability.Get(tx, ty).player >= 2)
                    return false;
                if (JOBS().getter.Is(tx, ty))
                    return false;
                if (THINGS().GetFirst(tx, ty) != null)
                    return false;
                if (ENTITIES().HasAtTile(e, tx, ty))
                    return false;
                if (PATH().huristics.Getter.Get(tx, ty) > 0.1)
                    return false;
                if (ROOMS().map.Is(tx, ty))
                    return false;
                return true;
            }
        }

        public abstract class FinderMiscWithoutDest
        {
            private readonly double max;

            protected FinderMiscWithoutDest(int max)
            {
                this.max = max;
            }

            protected virtual bool Has()
            {
                return true;
            }

            public abstract bool IsTile(int tx, int ty);

            public bool Find(COORDINATE start, SPath path)
            {
                if (!Has())
                    return false;

                int sx = start.x();
                int sy = start.y();

                if (IsTile(sx, sy))
                {
                    if (path != null)
                    {
                        path.Request(sx, sy, sx, sy, false);
                        return path.IsSuccess();
                    }
                    return true;
                }

                foreach (DIR d in DIR.ORTHO)
                {
                    if (IsTile(sx + d.x(), sy + d.y()))
                    {
                        if (path != null)
                        {
                            path.Request(sx, sy, sx + d.x(), sy + d.y(), false);
                            return path.IsSuccess();
                        }
                        return true;
                    }
                }

                GUTIL.Flooder().Init(this);

                PathTile t = GUTIL.Flooder().PushSloppy(sx, sy, 0, null);

                while (GUTIL.Flooder().HasMore())
                {
                    t = GUTIL.Flooder().PollSmallest();

                    foreach (DIR d in DIR.ALL)
                    {
                        int dx = d.x() + t.x();
                        int dy = d.y() + t.y();
                        if (!IN_BOUNDS(dx, dy))
                            continue;
                        if (d.IsOrtho())
                        {
                            if (IsTile(dx, dy))
                            {
                                GUTIL.Flooder().Done();
                                if (path != null)
                                    path.SetDirect(sx, sy, dx, dy, t, false);
                                return true;
                            }
                        }

                        double v = PATH().huristics.GetCost(t.x(), t.y(), dx, dy);
                        if (v < 0)
                            continue;
                        v *= d.TileDistance();
                        v += t.GetValue();
                        if (v <= max)
                            GUTIL.Flooder().PushSmaller(dx, dy, v, t);
                    }
                }
                GUTIL.Flooder().Done();

                return false;
            }
        }

        public static class FinderArround
        {
            private Coo result = new Coo();

            public FinderArround()
            {
            }

            public COORDINATE Find(int sx, int sy, double distance)
            {
                SCompPath p = PATH().comps.pather.Fill(sx, sy, distance);

                if (p.Path().Size() == 0)
                    return null;

                SComponent s = p.Path().Rnd();

                GUTIL.Coos().Set(0);

                int r = s.Level().Size();

                int x1 = s.CentreX() & ~(r - 1);
                int y1 = s.CentreY() & ~(r - 1);

                for (int dy = 0; dy < r; dy++)
                {
                    for (int dx = 0; dx < r; dx++)
                    {
                        int x = x1 + dx;
                        int y = y1 + dy;
                        if (s.Is(x, y))
                        {
                            GUTIL.Coos().Get().Set(x, y);
                            GUTIL.Coos().Inc();
                        }
                    }
                }

                if (GUTIL.Coos().GetI() == 0)
                {
                    result.Set(s.CentreX(), s.CentreY());
                    return result;
                }

                result.Set(GUTIL.Coos().Set(RND.rInt(GUTIL.Coos().GetI())));
                return result;
            }
        }

        public static class Rnd
        {
            private Coo result = new Coo();

            public Rnd()
            {
            }

            public COORDINATE Find(int sx, int sy, int r)
            {
                SCompPath p = PATH().comps.pather.Fill(sx, sy, r);

                if (p.Path().Size() == 0)
                    return null;

                SComponent s = p.Path().Rnd();

                GUTIL.Coos().Set(0);

                r = s.Level().Size();

                int x1 = s.CentreX() & ~(r - 1);
                int y1 = s.CentreY() & ~(r - 1);

                for (int dy = 0; dy < r; dy++)
                {
                    for (int dx = 0; dx < r; dx++)
                    {
                        int x = x1 + dx;
                        int y = y1 + dy;
                        if (s.Is(x, y))
                        {
                            GUTIL.Coos().Get().Set(x, y);
                            GUTIL.Coos().Inc();
                        }
                    }
                }

                if (GUTIL.Coos().GetI() == 0)
                {
                    result.Set(s.CentreX(), s.CentreY());
                    return result;
                }

                result.Set(GUTIL.Coos().Set(RND.rInt(GUTIL.Coos().GetI())));
                return result;
            }
        }
    }
}