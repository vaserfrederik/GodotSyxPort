using System;
using settlement.main;
using settlement.misc.util;
using snake2d.util.map;

namespace settlement.path.finders
{
    public abstract class SFinderRoomService : SFinderFindable, MAP_OBJECT<FSERVICE>
    {
        public SFinderRoomService(string s) : base(s)
        {
        }

        public override FINDABLE getReservable(int tx, int ty)
        {
            FINDABLE f = get(tx, ty);
            if (f != null && f.findableReservedCanBe())
                return f;
            return null;
        }

        public override FINDABLE getReserved(int tx, int ty)
        {
            FINDABLE f = get(tx, ty);
            if (f != null && f.findableReservedIs())
                return f;
            return null;
        }

        public override FSERVICE get(int tile)
        {
            return get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
        }
    }
}