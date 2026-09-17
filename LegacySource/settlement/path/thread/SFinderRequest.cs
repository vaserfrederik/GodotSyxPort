using System;
using System.Collections.Generic;
using settlement.main;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.path.path;
using settlement.path.thread;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;

public abstract class SFinderRequest
{
    protected readonly ThreadPathJob job = new ThreadPathJob
    {
        public override bool DoJob(PathUtilOnline p, SPathFinderThread fin, ThreadPath pp)
        {
            PathTile t = Find(pp.sx, pp.sy, p);
            if (t != null)
            {
                pp.path.Set(t);
                pp.destX = (short)t.x();
                pp.destY = (short)t.y();
                return true;
            }
            return false;
        }
    };

    public bool CheckAndSetRequest(int sx, int sy, SPath path)
    {
        if (path.thread.IsProcessed(sx, sy, 0, 0))
        {
            path.Clear();
            if (path.thread.IsSuccess())
            {
                path.Copy(path.thread.path, path.thread.destX, path.thread.destY, true);
            }
            return true;
        }
        else if (path.thread.IsBeingProcessed())
        {
            return false;
        }
        return true;
    }

    protected abstract PathTile Find(int sx, int sy, PathUtilOnline p);

    public sealed class FinderIdle : SFinderRequest
    {
        private readonly List<DIR> dirs = new List<DIR>(DIR.ALL);

        public FinderIdle()
        {
        }

        public bool ShouldFind(ENTITY e)
        {
            if (IsGoodTileToStandOn(e.tc().x(), e.tc().y(), 1))
            {
                return false;
            }
            return true;
        }

        public void Request(Humanoid h, SPath path)
        {
            SETT.PATH().thread.Prep(path, job, h.tc().x(), h.tc().y(), 0, 0, true);
        }

        public override PathTile Find(int sx, int sy, PathUtilOnline p)
        {
            p.GetFlooder().Init(null);
            PathTile t = p.GetFlooder().PushSloppy(sx, sy, 0, null);

            while (p.GetFlooder().HasMore())
            {
                t = p.GetFlooder().PollSmallest();

                if (IsGoodTileToStandOn(t.x(), t.y(), 0))
                {
                    p.GetFlooder().Done();
                    return t;
                }

                foreach (DIR d in dirs)
                {
                    int dx = d.x() + t.x();
                    int dy = d.y() + t.y();
                    if (!IN_BOUNDS(dx, dy))
                        continue;
                    if (!SETT.PATH().connectivity.Is(dx, dy))
                        continue;

                    double v = PATH().huristics.GetCost(t.x(), t.y(), dx, dy);
                    if (v < 0)
                        continue;
                    v *= d.TileDistance();
                    v += t.GetValue();
                    if (v <= 40)
                        p.GetFlooder().PushSmaller(dx, dy, v, t);
                }
            }
            p.GetFlooder().Done();
            return null;
        }

        private bool IsGoodTileToStandOn(int tx, int ty, int max)
        {
            if (PATH().availability.Get(tx, ty).player < 0)
                return false;
            if (PATH().availability.Get(tx, ty).player >= 2)
                return false;
            if (JOBS().getter.Is(tx, ty))
                return false;
            if (THINGS().GetFirst(tx, ty) != null)
                return false;
            if (ENTITIES().AmountAtTile(tx, ty) > max)
                return false;
            if (PATH().huristics.getter.Get(tx, ty) > 0.1)
                return false;
            if (SETT.ROOMS().map.Is(tx, ty))
                return false;
            return true;
        }
    }
}