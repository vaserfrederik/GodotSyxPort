using settlement.main;
using settlement.misc.util;
using snake2d.util.datatypes;

namespace settlement.room.service.hygiene.well
{
    public sealed class Wash : FSERVICE
    {
        private const int AVAILABLE = 0;
        private const int RESERVED = 1;
        private const int USED = 2;
        private int data;
        private readonly Coo coo = new Coo();
        private WellInstance ins;
        private readonly ROOM_WELL blue;

        public Wash(ROOM_WELL blue)
        {
            this.blue = blue;
        }

        public Wash Get(int tx, int ty)
        {
            if (blue.Is(tx, ty))
            {
                if (ROOMS.FData.TileData.Get(tx, ty) == Constructor.codeService)
                {
                    this.data = ROOMS.Data.Get(tx, ty);
                    this.coo.Set(tx, ty);
                    this.ins = blue.Get(tx, ty);
                    return this;
                }
            }
            return null;
        }

        private void Save()
        {
            int old = ROOMS.Data.Get(coo);

            if (old != data)
            {
                int current = data;
                data = old;
                ins.Service.Report(this, ins.BlueprintI().Data, -1);
                data = current;
                ROOMS.Data.Set(ins, coo, data);
                ins.Service.Report(this, ins.BlueprintI().Data, 1);
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
            StateSet(AVAILABLE);
        }

        public void StartUsing()
        {
            StateSet(USED);
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
            if (State() != AVAILABLE)
                throw new System.InvalidOperationException();
            StateSet(RESERVED);
        }

        public bool FindableReservedIs()
        {
            return State() == RESERVED || State() == USED;
        }

        public void FindableReserveCancel()
        {
            if (State() == RESERVED || State() == USED)
                StateSet(AVAILABLE);
        }

        public void Dispose()
        {
            if (FindableReservedCanBe())
                FindableReserve();
        }

        public void Init(int tx, int ty)
        {
            if (Get(tx, ty) != null)
                ins.Service.Report(this, ins.BlueprintI().Data, 1);
        }

        public bool IsUsed(int tile)
        {
            data = SETT.ROOMS.Data.Get(tile);
            return State() == USED;
        }
    }
}