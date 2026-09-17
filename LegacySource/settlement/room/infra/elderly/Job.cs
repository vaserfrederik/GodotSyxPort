using System;
using settlement.room.infra.elderly;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.infra.elderly
{
    internal class Job
    {
        private readonly Bit isBit = new Bit(0b0001);
        private readonly Bit reservedBit = new Bit(0b0010);
        private readonly Bit usingBit = new Bit(0b0100);
        private readonly ROOM_RESTHOME b;
        private int data;
        private readonly Coo coo = new Coo();
        private ResthomeInstance ins;

        public Job(ROOM_RESTHOME b)
        {
            this.b = b;
        }

        public SETT_JOB Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins != null)
            {
                if (isBit.Is(SETT.ROOMS().data.Get(tx, ty)))
                {
                    data = SETT.ROOMS().data.Get(tx, ty);
                    coo.Set(tx, ty);
                    return job;
                }
            }
            return null;
        }

        public bool Used(int tx, int ty)
        {
            if (Get(tx, ty) != null)
                return usingBit.Is(data);
            return false;
        }

        public void Set(ResthomeInstance ins, int tx, int ty)
        {
            SETT.ROOMS().data.Set(ins, tx, ty, isBit.Set(0));
        }

        private readonly SETT_JOB job = new SETT_JOB()
        {
            public override bool JobUseTool()
            {
                return false;
            }

            public override void JobStartPerforming()
            {
                data = usingBit.Set(data);
                SETT.ROOMS().data.Set(ins, coo, data);
            }

            public override SoundRace JobSound()
            {
                return b.Employment().Sound();
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return reservedBit.Is(data);
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                data = reservedBit.Clear(data);
                data = usingBit.Clear(data);
                SETT.ROOMS().data.Set(ins, coo, data);
            }

            public override bool JobReserveCanBe()
            {
                return !reservedBit.Is(data);
            }

            public override void JobReserve(RESOURCE r)
            {
                data = reservedBit.Set(data);
                data = usingBit.Clear(data);
                SETT.ROOMS().data.Set(ins, coo, data);
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return 45;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                JobReserveCancel(r);
                return null;
            }

            public override CharSequence JobName()
            {
                return b.Employment().Verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }
        };
    }
}