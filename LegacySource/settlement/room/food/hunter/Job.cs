using System;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace Settlement.Room.Food.Hunter
{
    class Job
    {
        private readonly ROOM_HUNTER print;
        public readonly Work WORK = new Work();
        static readonly Bits gore = new Bits(0b01110000);

        public Job(ROOM_HUNTER print)
        {
            this.print = print;
        }

        public void Reset(HunterInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Set(d, 0));
        }

        public void Gore(HunterInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Inc(d, 1));
        }

        public SETT_JOB Init(int tx, int ty, HunterInstance ins)
        {
            if (!ins.Is(tx, ty))
                return null;
            if (SETT.ROOMS().fData.Tile.Is(tx, ty, ins.BlueprintI().Constructor.ww))
                return WORK.Init(tx, ty, ins);
            return null;
        }

        public sealed class Work : SETT_JOB
        {
            private const int BITRESERVED = 0b1;

            private readonly Coo coo = new Coo();
            public HunterInstance Ins;
            public int Data;

            public Work() { }

            public override bool JobReserveCanBe()
            {
                return !JobReservedIs(null);
            }

            public Work Init(int tx, int ty, HunterInstance ins)
            {
                Data = ROOMS().data.Get(tx, ty);
                coo.Set(tx, ty);
                Ins = ins;
                return this;
            }

            public void Save()
            {
                ROOMS().data.Set(Ins, coo, Data);
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override string JobName()
            {
                return print.Employment().Verb;
            }

            public override bool JobUseTool()
            {
                return false;
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return 45;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (JobReservedIs(null))
                    throw new Exception();
                Data |= BITRESERVED;
                Save();
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return (Data & BITRESERVED) == BITRESERVED;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                Data &= ~BITRESERVED;
                Save();
            }

            public override void JobStartPerforming() { }

            public override SoundRace JobSound()
            {
                return null;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE res, int ram)
            {
                return null;
            }
        }
    }
}