using System;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.infra.station
{
    final class Job
    {
        private StationInstance ins;
        private readonly Coo coo = new Coo();

        public readonly RoomBits breserved = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));

        private readonly ROOM_STATION b;

        public Job(ROOM_STATION blue)
        {
            this.b = blue;
        }

        public SETT_JOB Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins == null)
                return null;
            if ((SETT.ROOMS().fData.tileData.Get(tx, ty) & Constructor.BIT_WORK) != 0)
            {
                coo.Set(tx, ty);
                return job;
            }
            return null;
        }

        public readonly SETT_JOB job = new SETT_JOB()
        {
            private int time = 48,

            public bool JobUseTool()
            {
                return false;
            },

            public void JobStartPerforming()
            {
            },

            public SoundRace JobSound()
            {
                return null;
            },

            public RBIT JobResourceBitToFetch()
            {
                return null;
            },

            public bool JobReservedIs(RESOURCE r)
            {
                return breserved.Get() == 1;
            },

            public void JobReserveCancel(RESOURCE r)
            {
                breserved.Set(ins, 0);
            },

            public bool JobReserveCanBe()
            {
                return breserved.Get() == 0 && ins.prepared < ins.MaxPrep();
            },

            public void JobReserve(RESOURCE r)
            {
                breserved.Set(ins, 1);
            },

            public double JobPerformTime(Humanoid skill)
            {
                return time;
            },

            public RESOURCE JobPerform(Humanoid skill, RESOURCE res, int ram)
            {
                JobReserveCancel(res);
                double am = SETT.ROOMS().STOCKPILE.bonus().Get(skill.indu()) / SETT.ROOMS().STOCKPILE.bonus().baseValue * ins.efficiency() * time;
                ins.SetPrepared(ins.prepared + am);
                return null;
            },

            public CharSequence JobName()
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