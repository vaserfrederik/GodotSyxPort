using System;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.food.cannibal
{
    public class Cage
    {
        private readonly ROOM_CANNIBAL b;
        private CannibalInstance ins;
        private Coo coo = new Coo();

        private readonly int sNone = 0;
        private readonly int sReserved = 1;
        private readonly int sInside = 2;
        private readonly int sFetching = 3;

        private readonly RoomBits state = new RoomBits(coo, new Bits(0b0111))
        {
            protected override void Remove()
            {
                if (Get() > sNone)
                {
                    ins.prisoners--;
                    b.prisoners--;
                }
                if (Get() == sInside)
                {
                    ins.reservable--;
                }
            }

            protected override void Add()
            {
                if (Get() > sNone)
                {
                    ins.prisoners++;
                    b.prisoners++;
                }
                if (Get() == sInside)
                {
                    ins.reservable++;
                }
            }
        };

        public Cage(ROOM_CANNIBAL print)
        {
            this.b = print;
        }

        public Cage Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins != null && SETT.ROOMS().fData.tile.Is(tx, ty, ins.BlueprintI().constructor.cc))
            {
                coo.Set(tx, ty);
                return this;
            }
            return null;
        }

        public bool Available()
        {
            return state.Get() == sNone;
        }

        public void PrisonerReserve()
        {
            state.Set(ins, sReserved);
        }

        public void PrisonerArrive()
        {
            state.Set(ins, sInside);
        }

        public bool PrisonerOk()
        {
            return state.Get() != sNone;
        }

        public void PrisonerCancel()
        {
            state.Set(ins, sNone);
        }

        public bool CanGrab()
        {
            return state.Get() == sInside;
        }

        public void Grab()
        {
            state.Set(ins, sFetching);
        }

        public void GrabCancel()
        {
            if (state.Get() == sFetching)
                state.Set(ins, sInside);
        }

        public COORDINATE Coo()
        {
            return coo;
        }
    }
}