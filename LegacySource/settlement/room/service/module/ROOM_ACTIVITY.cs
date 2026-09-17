using System;
using System.Collections.Generic;
using snake2d.util.MATH;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;

namespace settlement.room.service.module
{
    public abstract class ROOM_ACTIVITY
    {
        public static readonly double WORK_ENDSD = (double)2.0 / TIME.hoursPerDay();
        public static readonly double WORK_STARTSD = (double)MATH.mod(3 - TIME.workHours(), TIME.hoursPerDay()) / TIME.hoursPerDay();

        private static readonly ArrayCooShort spots = new ArrayCooShort(20 * 35);

        public bool Is(int sx, int sy)
        {
            return Finder().Get(sx, sy) != null;
        }

        public abstract SFinderRoomService Finder();

        private Coo coo = new Coo();

        public COORDINATE LookAt(int sx, int sy)
        {
            coo.Set(sx * C.TILE_SIZE + C.TILE_SIZEH, sy * C.TILE_SIZE + C.TILE_SIZEH);
            return coo;
        }

        public bool ShouldCheer(int sx, int sy)
        {
            return false;
        }

        public bool ShouldBoo(int sx, int sy)
        {
            return false;
        }

        public abstract bool IsActive(int sx, int sy);

        public interface ROOM_ACTIVITY_HASER
        {
            ROOM_ACTIVITY Spec();
        }

        public COORDINATE GetDestination(COORDINATE roomT)
        {
            ROOMA r = SETT.ROOMS().Map.Rooma.Get(roomT);
            GUTIL.Flooder().Init(this);
            spots.Set(0);
            foreach (COORDINATE c in r.Body())
            {
                if (r.Is(c))
                {
                    GUTIL.Flooder().PushSloppy(c.X, c.Y, 7);
                }
            }

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollGreatest();
                if (IsSpot(t.X, t.Y))
                {
                    spots.Get().Set(t.X, t.Y);
                    spots.Inc();
                }
                if (t.Value <= 0)
                    continue;

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR dd = DIR.ALL.Get(di);
                    int dx = t.X + dd.X;
                    int dy = t.Y + dd.Y;
                    double c = SETT.PATH().Coster.Player.GetCost(t.X, t.Y, dx, dy);
                    if (c > 0)
                    {
                        DIR old = dd;
                        if (t.Parent != null)
                            old = DIR.Get(t.Parent, t);
                        if (old != dd)
                            c *= 2;
                        c *= dd.TileDistance();
                        GUTIL.Flooder().PushGreater(dx, dy, t.Value - c);
                    }
                }
            }

            GUTIL.Flooder().Done();

            int max = spots.GetI();

            if (max == 0)
                return roomT;

            return spots.Set(RND.rInt(max));
        }

        protected bool IsSpot(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return false;
            if (SETT.ROOMS().Map.Is(tx, ty))
                return false;
            AVAILABILITY av = PATH().Availability.Get(tx, ty);
            if (av.Player >= 0 && av.Player < AVAILABILITY.Penalty && av.From == 0)
            {
                return true;
            }
            return false;
        }
    }
}