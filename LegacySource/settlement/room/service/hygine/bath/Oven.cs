using System;
using settlement.main;
using settlement.room.service.hygine.bath;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using snake2d.util.datatypes;

namespace settlement.room.service.hygine.bath
{
    public class Oven : SETT_JOB
    {
        static readonly int BIT = Bits.OVEN;
        private static readonly Oven self = new Oven();
        int data;
        readonly Coo coo = new Coo();
        BathInstance ins;

        public static Oven Init(int tx, int ty, ROOM_BATH b)
        {
            if (!b.Is(tx, ty))
                return null;
            int data = ROOMS().data.Get(tx, ty);
            if ((data & BITS) != BIT)
                return null;
            self.data = data;
            self.coo.Set(tx, ty);
            self.ins = b.Get(tx, ty);
            return self;
        }

        private Oven() { }

        public override bool JobUseTool()
        {
            return false;
        }

        private void Save()
        {
            ROOMS().data.Set(ins, coo.x(), coo.y(), data);
        }

        public override void JobStartPerforming()
        {
        }

        public override SoundRace JobSound()
        {
            return null;
        }

        public override RBIT JobResourceBitToFetch()
        {
            if (ins.BlueprintI().Consumption.Ins().Get(0) == null)
                return null;
            return ins.BlueprintI().Consumption.Ins().Get(0).Resource.bit;
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return (data & RESERVED) != 0;
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            data &= ~RESERVED;
            Save();
        }

        public override bool JobReserveCanBe()
        {
            return ins.Heat < ins.Service().Total() * 2 && !JobReservedIs(null);
        }

        public override void JobReserve(RESOURCE r)
        {
            if (JobReservedIs(null))
                throw new RuntimeException();
            data |= RESERVED;
            Save();
        }

        public override int JobResourcesNeeded(Humanoid skill)
        {
            return SETT.ROOMS().STOCKPILE.CarryCap(skill);
        }

        public override double JobPerformTime(Humanoid skill)
        {
            return 0;
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ri)
        {
            ins.Heat += ri;
            ins.BlueprintI().Consumption.Ins().Get(0).Inc(ins, ri);
            JobReserveCancel(r);
            return null;
        }

        readonly string name = "bringing fuel to furnace";

        public override string JobName()
        {
            return name;
        }

        public override COORDINATE JobCoo()
        {
            return coo;
        }
    }
}