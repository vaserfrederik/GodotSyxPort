using System;
using settlement.room.law.stockade;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.law.stockade
{
    public sealed class Job
    {
        public const int ISTAND = 1;
        public const int IFOOD = 2;
        public const int ISHIT = 3;

        private readonly ROOM_STOCKADE b;
        private readonly Coo coo = new Coo();
        private StockInstance ins;
        private readonly RoomBits type = new RoomBits(coo, new Bits(0b0111));
        private readonly RoomBits reserved = new RoomBits(coo, new Bits(0b1000));
        private readonly RoomBits sreserved = new RoomBits(coo, new Bits(0b10000));
        private readonly RoomBits data = new RoomBits(coo, new Bits(~0b11111));

        public Job(ROOM_STOCKADE b)
        {
            this.b = b;
        }

        public SETT_JOB Job(int tx, int ty)
        {
            ins = b.getter.get(tx, ty);
            if (ins != null)
            {
                coo.set(tx, ty);
                if (type.get() != 0)
                    return job;
            }
            return null;
        }

        public int Food(int tx, int ty)
        {
            if (Job(tx, ty) != null && type.get() == IFOOD)
                return data.get();
            return 0;
        }

        public int Shit(int tx, int ty)
        {
            if (Job(tx, ty) != null && type.get() == ISHIT)
                return data.get();
            return 0;
        }

        public int Type(int tx, int ty)
        {
            if (Job(tx, ty) != null)
                return type.get();
            return 0;
        }

        public bool Reserve(int tx, int ty, int type, bool reserve, bool use)
        {
            if (Job(tx, ty) == null)
                return false;
            if (this.type.get() == type)
            {
                if (type == IFOOD && data.get() <= 0)
                    return false;

                if (use)
                {
                    if (type == IFOOD)
                        data.inc(ins, -1);
                    else if (type == ISHIT)
                        data.inc(ins, 1);
                }

                if (reserve)
                {
                    if (sreserved.get() == 0)
                    {
                        sreserved.set(ins, 1);
                        return true;
                    }
                    return false;
                }
                else
                {
                    sreserved.set(ins, 0);
                    return true;
                }
            }
            return false;
        }

        private readonly SETT_JOB job = new SETT_JOB()
        {
            public bool JobUseTool() => false,

            public bool JobUseHands() => type.get() == ISHIT,

            public void JobStartPerforming() { }

            public SoundRace JobSound() => b.employment().sound(),

            public RBIT JobResourceBitToFetch()
            {
                if (type.get() == IFOOD)
                {
                    return ins.fetch;
                }
                return null;
            }

            public bool JobReservedIs(RESOURCE r) => reserved.get() == 1,

            public void JobReserveCancel(RESOURCE r) => reserved.set(ins, 0),

            public bool JobReserveCanBe()
            {
                if (type.get() == IFOOD && data.get() > 8)
                    return false;
                return reserved.get() == 0;
            },

            public void JobReserve(RESOURCE r)
            {
                if (r != null)
                {
                    ins.jobs.resetResourceSearch();
                }
                reserved.set(ins, 1);
            },

            public double JobPerformTime(Humanoid a) => type.get() == ISTAND ? 60 : 20,

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                if (type.get() == IFOOD && rAm > 0)
                {
                    data.inc(ins, rAm);
                    foreach (IndustryResource ii in b.indu.ins())
                    {
                        if (ii.resource == r)
                            ii.inc(ins, rAm);
                    }
                }
                else
                    data.set(ins, 0);
                reserved.set(ins, 0);
                return null;
            },

            public CharSequence JobName() => b.employment().title,

            public COORDINATE JobCoo() => coo
        };
    }
}