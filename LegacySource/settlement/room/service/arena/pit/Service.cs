using settlement.main;
using settlement.misc.util;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.service.arena.pit
{
    internal class Service : FSERVICE
    {
        private readonly Coo coo = new Coo();
        private readonly RoomBits bAvailable = new RoomBits(coo, 0b0001);
        private ArenaInstance ins;
        private readonly ROOM_FIGHTPIT b;

        public Service(ROOM_FIGHTPIT b)
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
            if (ins != null && SETT.ROOMS().fData.TileData.Get(tx, ty) == ArenaConstructor.STATION)
            {
                coo.Set(tx, ty);
                return true;
            }
            return false;
        }

        public void Consume()
        {
        }

        public int X()
        {
            return coo.X();
        }

        public int Y()
        {
            return coo.Y();
        }

        public bool FindableReservedCanBe()
        {
            return bAvailable.Get() == 1;
        }

        public void FindableReserve()
        {
            if (!FindableReservedCanBe())
            {
                throw new RuntimeException();
            }
            ins.Service.Report(this, ins.BlueprintI().Data, -1);
            bAvailable.Set(ins, 0);
        }

        public bool FindableReservedIs()
        {
            return bAvailable.Get() == 0;
        }

        public void FindableReserveCancel()
        {
            if (FindableReservedCanBe())
                return;
            bAvailable.Set(ins, 1);
            ins.Service.Report(this, ins.BlueprintI().Data, 1);
        }
    }
}