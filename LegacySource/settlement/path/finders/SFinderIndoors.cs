using settlement.main;
using settlement.misc.util;

namespace settlement.path.finders
{
    public sealed class SFinderIndoors : SFinderFindable
    {
        public SFinderIndoors() : base("indoors")
        {
            new TestPath("indoors", this);
        }

        public override FINDABLE GetReservable(int tx, int ty)
        {
            FINDABLE f = TERRAIN().indoors.findable(tx, ty);
            if (f != null && f.findableReservedCanBe())
                return f;
            return null;
        }

        public override FINDABLE GetReserved(int tx, int ty)
        {
            FINDABLE f = TERRAIN().indoors.findable(tx, ty);
            if (f != null && f.findableReservedIs())
                return f;
            return null;
        }
    }
}