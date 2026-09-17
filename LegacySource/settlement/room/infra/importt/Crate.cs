using settlement.room.main;
using settlement.room.main.job;

namespace settlement.room.infra.importt
{
    internal sealed class Crate : StorageCrate
    {
        private readonly ROOM_IMPORT b;
        private ImportInstance ins;

        public Crate(ROOM_IMPORT b)
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
            ins.Count(this, delta);
        }

        protected override int Max(RoomInstance ins)
        {
            return ImportInstance.CrateMax;
        }

        protected override double SpoilRate(RoomInstance ins)
        {
            return 1.0;
        }

        public override bool IsStorage()
        {
            return false;
        }

        public override bool IsPrio()
        {
            return false;
        }

        public override bool StorageIsFindable()
        {
            return false;
        }
    }
}