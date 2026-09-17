using settlement.room.main;
using settlement.room.infra.stockpile;

namespace Settlement.Room.Infra.Stockpile
{
    final class Crate : StorageCrate
    {
        protected readonly ROOM_STOCKPILE b;
        StockpileInstance ins;

        Crate(ROOM_STOCKPILE b)
        {
            this.b = b;
        }

        protected override bool Is(int tx, int ty)
        {
            if (b.Is(tx, ty))
            {
                ins = b.Getter.Get(tx, ty);
                if (b.Constructor.IsCrate(tx, ty))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void Count(int delta)
        {
            b.Tally().Report(this, ins, delta);
            ins.UpdateMasks(Resource());
        }

        public override bool IsPrio()
        {
            return ins.Prioritizing();
        }

        public override bool IsStorage()
        {
            return true;
        }

        public override bool IsFindable()
        {
            return !ins.Storing();
        }

        protected override int Max(RoomInstance ins)
        {
            return ((StockpileInstance)ins).CrateSize(Resource());
        }

        protected override double SpoilRate(RoomInstance ins)
        {
            return 0.5 + 0.5 * (ins.GetDegrade());
        }
    }
}