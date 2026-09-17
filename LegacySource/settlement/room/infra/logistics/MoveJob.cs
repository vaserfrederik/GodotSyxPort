using System;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.util;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.misc;

namespace settlement.room.infra.logistics
{
    public static class MoveJob
    {
        public static readonly MoveJob TMP = new MoveJob();
        private static readonly RBITImp tmp = new RBITImp();
        public readonly Coo source = new Coo();
        public readonly Coo dest = new Coo();
        public RESOURCE res;
        public int maxAm;
        public bool stored;
        public bool prio;

        private MoveJob()
        {
        }

        public interface ROOM_MOVEJOBBER
        {
            MoveJob moveJob(Humanoid skill);
        }

        public interface ROOM_MOVE_DEST
        {
            TILE_STORAGE destCrate(RBIT okMask, int minAm, int ox, int oy);
            default TILE_STORAGE fetchToCrate(RESOURCE res, int desiredAm)
            {
                return destCrate(res.bit, 1, -1, -1);
            }

            RBIT destSpaceMask();
            double storedD(RESOURCE res);
            RBIT moveCapacity();
        }

        public interface ROOM_MOVE_SOURCE
        {
            RESOURCE_TILE sourceCrate(RBIT okMask, int minAm, int ox, int oy, double lim);
            RBIT sourceAmountMask();
            RBIT moveCapacity();
            int moveCapacityAm(RESOURCE res);
            double storedD(RESOURCE res);
        }

        public void Cancel()
        {
        }

        public static MoveJob Fetch(RoomInstance ins, ROOM_MOVE_DEST accepter, int am, int radius, int ox, int oy, RBIT scattered, RBIT stored)
        {
            if (scattered.IsClear() && stored.IsClear())
                return null;

            am = CLAMP.i(am, 1, 100);

            RESOURCE_TILE t = RESOURCE_TILE.GETTER.Reservable(scattered, stored, RBIT.NONE, ox, oy);

            if (t == null)
                t = SETT.PATH().Finders.Resource.Find(scattered, stored, RBIT.NONE, ins, radius);

            if (t == null)
                return null;

            TMP.source.Set(t.x(), t.y());

            RESOURCE r = t.Resource();
            TMP.stored = t.IsStorage();
            TMP.prio = t.IsPrio();
            TMP.res = r;

            tmp.ClearSet(r.bit);

            TILE_STORAGE c = accepter.FetchToCrate(r, am);

            if (c == null)
                return null;

            am = Math.Min(c.StorageReservable(), am);
            if (am <= 0)
                throw new RuntimeException();

            TMP.maxAm = am;
            TMP.dest.Set(c.x(), c.y());
            return TMP;
        }
    }
}