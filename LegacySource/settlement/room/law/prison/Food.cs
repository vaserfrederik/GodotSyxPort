using System;
using game.audio;
using game.faction;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.law.prison
{
    internal sealed class Food : SETT_JOB, FSERVICE
    {
        private static readonly Bits food_amount = new Bits(0b0000_0000_0000_1111_1111);
        private static readonly Bit food_reserved = new Bit(0b0000_0000_0001_0000_0000);
        private static readonly Bit job_reserved = new Bit(0b0000_0000_0010_0000_0000);
        private Coo coo = new Coo();
        private PrisonInstance ins;

        private Food()
        {
        }

        private static Food self = new Food();

        public static Food Init(int tx, int ty)
        {
            self.ins = SETT.ROOMS().PRISON.Get(tx, ty);
            if (self.ins == null || SETT.ROOMS().fData.tileData.Get(tx, ty) != Constructor.CODE_FOOD)
                return null;

            self.coo.Set(tx, ty);
            return self;
        }

        public static int FoodAmount(int data)
        {
            return food_amount.Get(data);
        }

        public SoundRace JobSound()
        {
            return null;
        }

        public CharSequence JobName()
        {
            return SETT.ROOMS().PRISON.Employment().Verb;
        }

        public void JobReserve(RESOURCE r)
        {
            int d = job_reserved.Set(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public bool JobReservedIs(RESOURCE r)
        {
            return job_reserved.Is(SETT.ROOMS().Data.Get(coo));
        }

        public void JobReserveCancel(RESOURCE r)
        {
            int d = job_reserved.Clear(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public bool JobReserveCanBe()
        {
            return !JobReservedIs(null) && food_amount.Get(SETT.ROOMS().Data.Get(coo)) < 8;
        }

        public RBIT JobResourceBitToFetch()
        {
            return ins.Fetch;
        }

        public int JobResourcesNeeded(Humanoid skill)
        {
            return food_amount.Mask - food_amount.Get(SETT.ROOMS().Data.Get(coo));
        }

        public double JobPerformTime(Humanoid skill)
        {
            return 0;
        }

        public void JobStartPerforming()
        {
        }

        public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ri)
        {
            int d = food_amount.Inc(SETT.ROOMS().Data.Get(coo), ri);
            SETT.ROOMS().Data.Set(ins, coo, d);
            if (RESOURCES.EDI().Get(r) != null)
            {
                ins.BlueprintI().Indu.Ins().Get(RESOURCES.EDI().Get(r).Index()).Inc(ins, ri);
            }
            else
                FACTIONS.Player().Res().Inc(r, RTYPE.CONSUMED, -ri);
            JobReserveCancel(null);
            return null;
        }

        public COORDINATE JobCoo()
        {
            return coo;
        }

        public bool JobUseTool()
        {
            return false;
        }

        public bool FindableReservedCanBe()
        {
            return food_amount.Get(SETT.ROOMS().Data.Get(coo)) > 0 && !food_reserved.Is(SETT.ROOMS().Data.Get(coo));
        }

        public void FindableReserve()
        {
            int d = food_reserved.Set(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public bool FindableReservedIs()
        {
            return food_reserved.Is(SETT.ROOMS().Data.Get(coo));
        }

        public void FindableReserveCancel()
        {
            int d = food_reserved.Clear(SETT.ROOMS().Data.Get(coo));
            SETT.ROOMS().Data.Set(ins, coo, d);
        }

        public int X()
        {
            return coo.X();
        }

        public int Y()
        {
            return coo.Y();
        }

        public void Consume()
        {
            int d = food_amount.Inc(SETT.ROOMS().Data.Get(coo), -1);
            d = food_reserved.Clear(d);
            SETT.ROOMS().Data.Set(ins, coo, d);
        }
    }
}