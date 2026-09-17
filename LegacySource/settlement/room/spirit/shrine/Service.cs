using settlement.main;
using settlement.misc.util;
using snake2d.util.datatypes;

namespace settlement.room.spirit.shrine
{
    final class Service : FSERVICE
    {
        private const int AVAILABLE = 1;
        private const int RESERVED = 2;
        private int data;
        private readonly Coo coo = new Coo();
        private ShrineInstance ins;
        private readonly ROOM_SHRINE blue;

        Service(ROOM_SHRINE blue)
        {
            this.blue = blue;
        }

        Service Get(int tx, int ty)
        {
            if (blue.Is(tx, ty))
            {
                if (ROOMS().fData.tile.Get(tx, ty).availability.player > 0)
                {
                    this.data = ROOMS().data.Get(tx, ty);
                    this.coo.Set(tx, ty);
                    this.ins = blue.Get(tx, ty);
                    return this;
                }
            }
            return null;
        }

        private void Save()
        {
            int old = ROOMS().data.Get(coo);

            if (old != data)
            {
                int current = data;
                data = old;
                if (FindableReservedCanBe())
                    ins.service().Report(this, ins.blueprintI().data, -1);
                data = current;
                if (FindableReservedCanBe())
                    ins.service().Report(this, ins.blueprintI().data, 1);
                ROOMS().data.Set(ins, coo, data);
            }
        }

        private int State()
        {
            return data & 0b01111;
        }

        private void StateSet(int state)
        {
            data &= ~0b01111;
            data |= state;
            Save();
        }

        public void Consume()
        {
            if (State() != RESERVED)
                throw new RuntimeException();
            StateSet(AVAILABLE);
        }

        public void StartUsing()
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
            return State() == AVAILABLE;
        }

        public void FindableReserve()
        {
            if (State() == AVAILABLE)
                StateSet(RESERVED);
            else
                throw new RuntimeException();
        }

        public bool FindableReservedIs()
        {
            return State() == RESERVED;
        }

        public void FindableReserveCancel()
        {
            if (State() == RESERVED)
                StateSet(AVAILABLE);
        }

        void Dispose()
        {
            StateSet(RESERVED);
        }

        void Init()
        {
            StateSet(AVAILABLE);
        }
    }
}