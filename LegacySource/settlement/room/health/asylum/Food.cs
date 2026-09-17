using System;
using snake2d.util.bit;
using snake2d.util.datatypes;
using settlement.room.health.asylum;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;

namespace settlement.room.health.asylum
{
    internal sealed class Food : SETT_JOB
    {
        private static readonly Bits amount = new Bits(0b001111);
        private static readonly Bit reserved = new Bit(0b010000);
        private Coo coo = new Coo();
        private AsylumInstance ins;

        private Food()
        {
        }

        private static Food self = new Food();

        public static Food Init(int tx, int ty)
        {
            self.ins = B().Get(tx, ty);
            if (self.ins == null || SETT.ROOMS().fData.tileData.Get(tx, ty) != Constructor.CODE_FOOD)
                return null;
            self.coo.Set(tx, ty);

            return self;
        }

        public static int FoodAmount(int data)
        {
            return amount.Get(data);
        }

        public int Food()
        {
            return FoodAmount(SETT.ROOMS().data.Get(coo));
        }

        public void Consume()
        {
            int d = amount.Inc(SETT.ROOMS().data.Get(coo), -1);
            SETT.ROOMS().data.Set(ins, coo, d);
        }

        public override SoundRace JobSound()
        {
            return null;
        }

        public override CharSequence JobName()
        {
            return SETT.ROOMS().PRISON.Employment().Verb;
        }

        public override void JobReserve(RESOURCE r)
        {
            int d = reserved.Set(SETT.ROOMS().data.Get(coo));
            SETT.ROOMS().data.Set(ins, coo, d);
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return reserved.Is(SETT.ROOMS().data.Get(coo));
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            int d = reserved.Clear(SETT.ROOMS().data.Get(coo));
            SETT.ROOMS().data.Set(ins, coo, d);
        }

        public override bool JobReserveCanBe()
        {
            return !JobReservedIs(null);
        }

        public override RBIT JobResourceBitToFetch()
        {
            if (amount.Get(SETT.ROOMS().data.Get(coo)) < 1 && Ins.Jobs.ResourceShouldSearch(B().Consumption.Ins().Get(0).Resource))
                return B().Consumption.Ins().Get(0).Resource.Bit;
            return null;
        }

        public override double JobPerformTime(Humanoid skill)
        {
            return 25;
        }

        public override void JobStartPerforming()
        {
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ri)
        {
            if (r == B().Consumption.Ins().Get(0).Resource)
            {
                int d = amount.Inc(SETT.ROOMS().data.Get(coo), ri);
                SETT.ROOMS().data.Set(ins, coo, d);
                B().Consumption.Ins().Get(0).Inc(B().Get(coo.X(), coo.Y()), ri);
            }
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

        public override bool JobUseHands()
        {
            return false;
        }

        private static readonly ROOM_ASYLUM B()
        {
            return SETT.ROOMS().ASYLUM;
        }
    }
}