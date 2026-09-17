using System;
using settlement.main;
using settlement.misc.util;
using snake2d.util.datatypes;

namespace settlement.room.service.hearth
{
    internal class Hearth : FSERVICE
    {
        private const int AVAILABLE = 1;
        private const int RESERVED = 2;
        private const int USED = 3;
        private int data;
        private readonly Coo coo = new Coo();
        private HearthInstance ins;
        private readonly ROOM_HEARTH blue;

        public Hearth(ROOM_HEARTH blue)
        {
            this.blue = blue;
        }

        public Hearth Get(int tx, int ty)
        {
            if (blue.Is(tx, ty))
            {
                if (ROOMS().fData.tileData.Get(tx, ty) == Constructor.CodeService)
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
                if (State() == USED)
                    ins.used--;
                if (FindableReservedCanBe())
                    ins.service.Report(this, ins.BlueprintI().data, -1);
                data = current;
                if (State() == USED)
                    ins.used++;
                if (FindableReservedCanBe())
                    ins.service.Report(this, ins.BlueprintI().data, 1);
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

        public override void Consume()
        {
            if (State() != USED)
                throw new Exception();
            StateSet(AVAILABLE);
        }

        public override void StartUsing()
        {
            StateSet(USED);
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
            return State() == AVAILABLE;
        }

        public override void FindableReserve()
        {
            if (State() != AVAILABLE)
                throw new Exception();
            StateSet(RESERVED);
        }

        public override bool FindableReservedIs()
        {
            return State() == RESERVED || State() == USED;
        }

        public override void FindableReserveCancel()
        {
            if (State() == RESERVED || State() == USED)
                StateSet(AVAILABLE);
        }

        internal void Dispose()
        {
            StateSet(RESERVED);
        }

        internal void Init()
        {
            StateSet(AVAILABLE);
        }
    }
}