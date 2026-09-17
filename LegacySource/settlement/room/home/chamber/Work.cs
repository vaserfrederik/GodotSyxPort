using System;
using settlement.main;
using game.audio;
using init.race;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.path;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.datatypes;
using snake2d.util.misc;

namespace settlement.room.home.chamber
{
    final class Work : SETT_JOB
    {
        private static readonly int NEEDS = 0;
        private static readonly int RESERVED = 1;
        private int data;
        private readonly Coo coo = new Coo();
        private ChamberInstance ins;
        private readonly ROOM_CHAMBER blue;

        public Work(ROOM_CHAMBER blue)
        {
            this.blue = blue;
        }

        public Work Get(int tx, int ty)
        {
            if (blue.Is(tx, ty))
            {
                int data = ROOMS().Data.Get(tx, ty);
                if (PATH().Availability.Get(tx, ty) == AVAILABILITY.ROOM)
                {
                    this.data = data;
                    this.coo.Set(tx, ty);
                    this.ins = blue.Get(tx, ty);

                    return this;
                }
            }
            return null;
        }

        public void Clear()
        {
            ROOMS().Data.Set(ins, coo, 0);
        }

        private void Save()
        {
            int old = ROOMS().Data.Get(coo);

            if (old != data)
            {
                ROOMS().Data.Set(ins, coo, data);
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

        public override bool JobUseTool()
        {
            return false;
        }

        public override void JobStartPerforming()
        {
        }

        public override SoundRace JobSound()
        {
            return ins.BlueprintI().Employment().Sound();
        }

        int i = 0;

        private readonly RBITImp bit = new RBITImp();

        public override RBIT JobResourceBitToFetch()
        {
            i++;
            if ((i & 1) == 0 && !ins.Fetching && ins.Occupant() != null)
            {
                bit.Clear();
                Induvidual inu = ins.Occupant().Indu();
                foreach (StatFurniture f in STATS.HOME().GetTmp(inu))
                {
                    if (f.Needed(ins.Occupant().Indu()) > 0 && ins.Jobs.ResourceReachable(f.Resource(inu)))
                        bit.Or(f.Resource(inu));
                }

                return bit.IsClear() ? null : bit;
            }
            return null;
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return State() == RESERVED;
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            if (JobReservedIs(r))
            {
                if (r != null)
                    ins.Fetching = false;
                StateSet(NEEDS);
            }
        }

        public override bool JobReserveCanBe()
        {
            return State() == NEEDS || State() == 2;
        }

        public override void JobReserve(RESOURCE r)
        {
            if (!JobReserveCanBe())
                throw new RuntimeException();
            StateSet(RESERVED);
            if (r != null)
                ins.Fetching = true;
        }

        public override double JobPerformTime(Humanoid skill)
        {
            return 30;
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE res, int ram)
        {
            if (!JobReservedIs(res))
                throw new RuntimeException();
            StateSet(NEEDS);

            if (res != null)
            {
                if (ins.Occupant() != null)
                {
                    Induvidual inu = ins.Occupant().Indu();
                    foreach (WearableResource rr in RACES.Res().Get(ins.Occupant().Indu().PopCL(), res))
                    {
                        int nn = rr.Needed(inu);
                        int aa = CLAMP.i(ram, 0, nn);
                        rr.Inc(inu, aa);
                        ram -= aa;
                        if (ram <= 0)
                            break;
                    }

                }
                ins.Fetching = false;
            }

            return null;
        }

        public override CharSequence JobName()
        {
            return ins.BlueprintI().Employment().Verb;
        }

        public override COORDINATE JobCoo()
        {
            return coo;
        }
    }
}