using System;
using settlement.room.law.prison;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.law.prison
{
    final class Cell : SETT_JOB
    {
        private static readonly Bit reserved = new Bit(0b0000_0000_0001_0000);
        private Coo coo = new Coo();
        private PrisonInstance ins;

        private Cell()
        {
        }

        static readonly Cell self = new Cell();

        static Cell Init(int tx, int ty)
        {
            self.ins = SETT.ROOMS().PRISON.Get(tx, ty);
            if (self.ins == null || SETT.ROOMS().fData.TileData.Get(tx, ty) != Constructor.CODE_ENTRANCE)
                return null;
            self.coo.Set(tx, ty);
            return self;
        }

        public override SoundRace JobSound()
        {
            return ins.BlueprintI().Employment().Sound();
        }

        public override string JobName()
        {
            return SETT.ROOMS().PRISON.Employment().Verb;
        }

        public override void JobReserve(RESOURCE r)
        {
            int d = reserved.Set(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return reserved.Is(SETT.ROOMS().Data.Get(coo));
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            int d = reserved.Clear(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public override bool JobReserveCanBe()
        {
            return !JobReservedIs(null);
        }

        public override RBIT JobResourceBitToFetch()
        {
            return null;
        }

        public override double JobPerformTime(Humanoid skill)
        {
            return 45;
        }

        public override void JobStartPerforming()
        {
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
        {
            JobReserveCancel(null);
            return null;
        }

        public override COORDINATE JobCoo()
        {
            return coo;
        }

        public override bool JobUseTool()
        {
            return false;
        }
    }
}