using System;
using snake2d;
using util;

namespace settlement.path.finders
{
    public sealed class SFinderUnreachable
    {
        SFinderUnreachable()
        {
        }

        public bool Find(COORDINATE start, SPath path, int maxDistance)
        {
            if (TERRAIN().WATER.open.Is(start.x(), start.y()))
            {
                return FindWater(start, path, maxDistance);
            }
            else if (PATH().connectivity.Is(start))
            {
                return false;
            }

            int tx = start.x();
            int ty = start.y();

            GUTIL.flooder().Init(this);
            PathTile t = GUTIL.flooder().PushSloppy(tx, ty, 0);

            while (GUTIL.flooder().HasMore())
            {
                t = GUTIL.flooder().PollSmallest();

                if (PATH().connectivity.Is(t))
                {
                    GUTIL.flooder().Done();
                    path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                    return true;
                }
                if (t.GetValue() > maxDistance)
                    continue;

                for (int i = 0; i < DIR.ALL.Length; i++)
                {
                    DIR d = DIR.ALL[i];
                    double v = t.GetValue();
                    tx = t.x() + d.x();
                    ty = t.y() + d.y();

                    if (IsPassable(t.x(), t.y(), d))
                    {
                        GUTIL.flooder().PushSloppy(tx, ty, v + d.tileDistance(), t);
                    }
                }
            }

            GUTIL.flooder().Done();
            return false;
        }

        public bool FindWater(COORDINATE start, SPath path, int maxDistance)
        {
            int tx = start.x();
            int ty = start.y();
            if (IsLand(tx, ty))
            {
                path.Request(tx, ty, tx, ty, false);
                return path.IsSuccess();
            }

            GUTIL.flooder().Init(this);
            PathTile t = GUTIL.flooder().PushSloppy(tx, ty, 0);

            int bx = -1;
            int by = -1;

            PathTile backup = null;

            while (GUTIL.flooder().HasMore())
            {
                t = GUTIL.flooder().PollSmallest();
                if (IsLand(t.x(), t.y()))
                {
                    if (PATH().connectivity.Is(t))
                    {
                        GUTIL.flooder().Done();
                        path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                        return true;
                    }
                    else
                    {
                        bx = t.x();
                        by = t.y();
                    }
                }
                else if (!PATH().solidity.Is(t) && backup == null)
                    backup = t;
                if (t.GetValue() > maxDistance)
                    break;

                for (int i = 0; i < DIR.ALL.Length; i++)
                {
                    DIR d = DIR.ALL[i];
                    double v = t.GetValue();
                    tx = t.x() + d.x();
                    ty = t.y() + d.y();

                    if (IsPassable(t.x(), t.y(), d))
                    {
                        GUTIL.flooder().PushSloppy(tx, ty, v + d.tileDistance(), t);
                    }
                }
            }

            if (bx != -1)
            {
                t = GUTIL.flooder().Get(bx, by);
                GUTIL.flooder().Done();
                path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                return true;
            }
            GUTIL.flooder().Done();
            if (backup != null)
            {
                path.SetDirect(start.x(), start.y(), backup.x(), backup.y(), backup, true);
                return true;
            }

            return false;
        }

        private bool IsPassable(int tx, int ty, DIR d)
        {
            if (!IN_BOUNDS(tx + d.x(), ty) || SETT.PATH().availability.Get(tx + d.x(), ty).tileCollide)
                return false;
            if (!IN_BOUNDS(tx, ty + d.y()) || SETT.PATH().availability.Get(tx, ty + d.y()).tileCollide)
                return false;
            if (!IN_BOUNDS(tx + d.x(), ty + d.y()) || SETT.PATH().availability.Get(tx + d.x(), ty + d.y()).tileCollide)
                return false;
            return true;
        }

        private bool IsLand(int tx, int ty)
        {
            return !IsWater(tx, ty) && !PATH().solidity.Is(tx, ty);
        }

        private bool IsWater(int tx, int ty)
        {
            return SETT.ENTITIES().submerged.Is(tx, ty);
        }
    }
}