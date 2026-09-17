using System;
using settlement.main;
using game.audio;
using init.race;
using init.resources;
using settlement.entity.humanoid;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.food.cannibal
{
    public class Job
    {
        private readonly ROOM_CANNIBAL print;
        public readonly Work WORK = new Work();
        public static readonly Bits gore = new Bits(0b00000000000001110000);
        public static readonly Bits race = new Bits(0b11111111111100000000);

        public Job(ROOM_CANNIBAL print)
        {
            this.print = print;
        }

        public void reset(CannibalInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Set(d, 0));
        }

        public void gore(CannibalInstance ins, COORDINATE c)
        {
            int d = ROOMS().data.Get(c);
            ROOMS().data.Set(ins, c, gore.Inc(d, 1));
        }

        public Race race(int tx, int ty)
        {
            return RACES.All().Get(race.Get(SETT.ROOMS().data.Get(tx, ty)));
        }

        public SETT_JOB Init(int tx, int ty, CannibalInstance ins)
        {
            if (!ins.Is(tx, ty))
                return null;
            if (SETT.ROOMS().fData.tile.Is(tx, ty, ins.blueprintI().constructor.ww))
                return WORK.Init(tx, ty, ins);
            return null;
        }

        public class Work : SETT_JOB
        {
            private const int BITRESERVED = 0b1;

            private readonly Coo coo = new Coo();
            public CannibalInstance ins;
            public int data;

            public Work()
            {
            }

            public override bool JobReserveCanBe()
            {
                return !JobReservedIs(null);
            }

            public Work Init(int tx, int ty, CannibalInstance ins)
            {
                data = ROOMS().data.Get(tx, ty);
                coo.Set(tx, ty);
                this.ins = ins;
                return this;
            }

            public void Save()
            {
                ROOMS().data.Set(ins, coo, data);
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override string JobName()
            {
                return print.employment().verb;
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
                    throw new RuntimeException();
                data |= BITRESERVED;
                Save();
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return (data & BITRESERVED) == BITRESERVED;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                data &= ~BITRESERVED;
                Save();
            }

            public override void JobStartPerforming()
            {
            }

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