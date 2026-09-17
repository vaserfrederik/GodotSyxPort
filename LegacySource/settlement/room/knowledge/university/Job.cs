using System;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.knowledge.university
{
    final class Job
    {
        private readonly ROOM_UNIVERSITY b;
        private int data;
        private readonly Coo coo = new Coo();
        private UniversityInstance ins;

        public Job(ROOM_UNIVERSITY b)
        {
            this.b = b;
        }

        public SETT_JOB Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins != null)
            {
                int i = SETT.ROOMS().fData.tileData.Get(tx, ty);
                if (i > 0)
                {
                    data = SETT.ROOMS().data.Get(tx, ty);
                    coo.Set(tx, ty);
                    return job;
                }
            }
            return null;
        }

        private readonly SETT_JOB job = new SETT_JOB()
        {
            private readonly Bit reserved = new Bit(1),

            public bool JobUseTool()
            {
                return false;
            },

            public void JobStartPerforming()
            {
            },

            public SoundRace JobSound()
            {
                return b.Employment().sound();
            },

            public RBIT JobResourceBitToFetch()
            {
                return null;
            },

            public bool JobReservedIs(RESOURCE r)
            {
                return reserved.Is(data);
            },

            public void JobReserveCancel(RESOURCE r)
            {
                data = reserved.Clear(data);
                SETT.ROOMS().data.Set(ins, coo, data);
            },

            public bool JobReserveCanBe()
            {
                return !reserved.Is(data);
            },

            public void JobReserve(RESOURCE r)
            {
                data = reserved.Set(data);
                SETT.ROOMS().data.Set(ins, coo, data);
            },

            public double JobPerformTime(Humanoid skill)
            {
                return 45;
            },

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                JobReserveCancel(r);
                return null;
            },

            public string JobName()
            {
                return b.Employment().verb;
            },

            public COORDINATE JobCoo()
            {
                return coo;
            }
        };
    }
}