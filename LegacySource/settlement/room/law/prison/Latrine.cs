using System;
using snake2d;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;

namespace settlement.room.law.prison
{
    internal sealed class Latrine : SETT_JOB, FSERVICE
    {
        private static readonly Bit latrine_reserved = new Bit(0b0000_0000_0000_0001);
        private static readonly Bit latrine_used = new Bit(0b0000_0000_0000_0010);
        private static readonly Bit latrine_jobreserved = new Bit(0b0000_0000_0000_0100);
        private Coo coo = new Coo();
        private PrisonInstance ins;

        private Latrine()
        {
        }

        static readonly Latrine self = new Latrine();

        static Latrine init(int tx, int ty)
        {
            self.ins = SETT.ROOMS().PRISON.get(tx, ty);
            if (self.ins == null || SETT.ROOMS().fData.tileData.get(tx, ty) != Constructor.CODE_LATRINE)
                return null;
            self.coo.set(tx, ty);
            return self;
        }

        static bool latrineUsed(int data)
        {
            return latrine_used.is(data);
        }

        public SoundRace jobSound()
        {
            if (SETT.ROOMS().LAVATORIES.size() > 0)
                return SETT.ROOMS().LAVATORIES.get(0).employment().sound();
            return null;
        }

        public CharSequence jobName()
        {
            return SETT.ROOMS().PRISON.employment().verb;
        }

        public void jobReserve(RESOURCE r)
        {
            int d = latrine_jobreserved.set(SETT.ROOMS().data.get(coo));
            SETT.ROOMS().data.set(ins, coo, d);
        }

        public bool jobReservedIs(RESOURCE r)
        {
            return latrine_jobreserved.is(SETT.ROOMS().data.get(coo));
        }

        public void jobReserveCancel(RESOURCE r)
        {
            int d = latrine_jobreserved.clear(SETT.ROOMS().data.get(coo));
            SETT.ROOMS().data.set(ins, coo, d);
        }

        public bool jobReserveCanBe()
        {
            return !jobReservedIs(null);
        }

        public RBIT jobResourceBitToFetch()
        {
            return null;
        }

        public double jobPerformTime(Humanoid skill)
        {
            return 45;
        }

        public void jobStartPerforming()
        {
        }

        public RESOURCE jobPerform(Humanoid skill, RESOURCE r, int ram)
        {
            jobReserveCancel(null);
            int d = latrine_used.clear(SETT.ROOMS().data.get(coo));
            SETT.ROOMS().data.set(ins, coo, d);
            return null;
        }

        public COORDINATE jobCoo()
        {
            return coo;
        }

        public bool jobUseTool()
        {
            return false;
        }

        public bool findableReservedCanBe()
        {
            return !latrine_reserved.is(SETT.ROOMS().data.get(coo));
        }

        public void findableReserve()
        {
            int d = latrine_reserved.set(SETT.ROOMS().data.get(coo));
            SETT.ROOMS().data.set(ins, coo, d);
        }

        public bool findableReservedIs()
        {
            return latrine_reserved.is(SETT.ROOMS().data.get(coo));
        }

        public void findableReserveCancel()
        {
            int d = latrine_reserved.clear(SETT.ROOMS().data.get(coo));
            SETT.ROOMS().data.set(ins, coo, d);
        }

        public int x()
        {
            return coo.x();
        }

        public int y()
        {
            return coo.y();
        }

        public void consume()
        {
            int d = latrine_used.set(SETT.ROOMS().data.get(coo));
            d = latrine_reserved.clear(d);
            SETT.ROOMS().data.set(ins, coo, d);
        }
    }
}