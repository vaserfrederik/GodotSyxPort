using settlement.main;
using settlement.room.main;
using settlement.room.main.job;

namespace settlement.room.infra.hauler
{
    internal class Crate : StorageCrate
    {
        public static readonly int size = 80;
        private readonly ROOM_HAULER b;
        private HaulerInstance ins;

        public Crate(ROOM_HAULER b)
        {
            this.b = b;
        }

        protected override bool Is(int tx, int ty)
        {
            if (b.Is(tx, ty))
            {
                ins = b.Getter.Get(tx, ty);
                if (SETT.ROOMS().FData.TileData.Is(tx, ty, 1))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsStorage()
        {
            return true;
        }

        public override bool IsPrio()
        {
            return ins.Prio();
        }

        protected override void Count(int delta)
        {
            b.Tally.Report(this, ins, delta);
            ins.UpdateMasks();
        }

        public override bool IsFindable()
        {
            return !ins.Storing();
        }

        protected override int Max(RoomInstance ins)
        {
            return size;
        }

        protected override double SpoilRate(RoomInstance ins)
        {
            return 1.0;
        }
    }
}