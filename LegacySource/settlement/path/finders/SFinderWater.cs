using System;
using settlement.main;
using settlement.misc.util;
using settlement.path.path;
using settlement.room.main;
using settlement.room.water.pool;
using snake2d;
using util;

namespace settlement.path.finders
{
    public sealed class SFinderWater : SFinderFindable
    {
        public SFinderWater() : base("water")
        {
            new TestPath(name, this);
        }

        public bool FindLand(COORDINATE start, SPath path, int maxDistance)
        {
            int tx = start.x();
            int ty = start.y();
            if (IsLand(tx, ty))
            {
                path.Request(tx, ty, tx, ty, false);
                return path.IsSuccessful();
            }

            GUTIL.Flooder().Init(this);
            PathTile t = GUTIL.Flooder().PushSloppy(tx, ty, 0);

            while (GUTIL.Flooder().HasMore())
            {
                t = GUTIL.Flooder().PollSmallest();
                if (IsLand(t.x(), t.y()))
                {
                    GUTIL.Flooder().Done();
                    path.SetDirect(start.x(), start.y(), t.x(), t.y(), t, true);
                    return true;
                }
                if (t.Value > maxDistance)
                    continue;

                foreach (DIR d in DIR.ALL)
                {
                    double v = t.Value;
                    tx = t.x() + d.x();
                    ty = t.y() + d.y();

                    if (IsLand(tx, ty) && PATH().Coster.Player.GetCost(t.x(), t.y(), tx, ty) > 0)
                    {
                        if (PATH().Connectivity.Is(tx, ty))
                        {
                            GUTIL.Flooder().PushSloppy(tx, ty, v + 1, t);
                        }
                        else
                        {
                            GUTIL.Flooder().PushSloppy(tx, ty, v + 100, t);
                        }
                    }
                    else if (IsWater(tx, ty))
                    {
                        GUTIL.Flooder().PushSloppy(tx, ty, v + 1, t);
                    }
                }
            }

            GUTIL.Flooder().Done();
            return false;
        }

        private bool IsLand(int tx, int ty)
        {
            return !IsWater(tx, ty) && !PATH().Solidity.Is(tx, ty);
        }

        private bool IsWater(int tx, int ty)
        {
            return SETT.ENTITIES().Submerged.Is(tx, ty);
        }

        public FINDABLE Get(int tx, int ty)
        {
            FINDABLE s = TERRAIN().WATER.Service.Get(tx, ty);
            if (s != null)
                return s;
            Room r = SETT.ROOMS().Map.Get(tx, ty);
            if (r != null && r.Blueprint() is ROOM_POOL)
            {
                FINDABLE f = ((ROOM_POOL)r.Blueprint()).FService(tx, ty);
                if (f != null)
                    return f;
            }
            return null;
        }

        public override FINDABLE GetReservable(int tx, int ty)
        {
            FINDABLE f = Get(tx, ty);
            if (f != null && f.FindableReservedCanBe())
                return f;
            return null;
        }

        public override FINDABLE GetReserved(int tx, int ty)
        {
            FINDABLE f = Get(tx, ty);
            if (f != null && f.FindableReservedIs())
                return f;
            return null;
        }
    }
}