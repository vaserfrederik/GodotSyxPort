using settlement.misc.util;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.service.arena.grand
{
    sealed class Service : FSERVICE
    {
        private readonly Coo coo = new Coo();
        private readonly RoomBits bAvailable = new RoomBits(coo, 0b0001_0000_0000);
        private ArenaInstance ins;
        private readonly ROOM_ARENA b;

        public Service(ROOM_ARENA b)
        {
            this.b = b;
        }

        public FSERVICE Get(int tx, int ty)
        {
            if (Init(tx, ty))
                return this;
            return null;
        }

        public bool Init(int tx, int ty)
        {
            ins = b.Getter.Get(tx, ty);
            if (ins != null && b.Constructor.Util.Service(tx, ty))
            {
                coo.Set(tx, ty);
                return true;
            }
            return false;
        }

        public override void Consume()
        {
        }

        public override int X()
        {
            return coo.X();
        }

        public override int Y()
        {
            return coo.Y();
        }

        public override bool FindableReservedCanBe()
        {
            return bAvailable.Get() == 1;
        }

        public override void FindableReserve()
        {
            if (!FindableReservedCanBe())
            {
                throw new RuntimeException();
            }
            ins.Service.Report(this, ins.BlueprintI().Data, -1);
            bAvailable.Set(ins, 0);
        }

        public override bool FindableReservedIs()
        {
            return bAvailable.Get() == 0;
        }

        public override void FindableReserveCancel()
        {
            if (FindableReservedCanBe())
                return;
            bAvailable.Set(ins, 1);
            ins.Service.Report(this, ins.BlueprintI().Data, 1);
        }
    }
}