using System;
using settlement.main;
using settlement.room.service.hygine.bath;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.room.main.util;
using snake2d.util.datatypes;
using snake2d.util.bit;

namespace settlement.room.service.hygine.bath
{
    public class Crank : SETT_JOB
    {
        private static readonly int wt = 20;
        private static readonly int BIT = Bits.CRANK;
        private static readonly Crank self = new Crank();
        private Bath bath;
        private readonly Coo coo = new Coo();
        private BathInstance ins;
        private readonly RoomBits reserved;
        private readonly RoomBits working;
        private readonly RoomBits free;

        static Crank()
        {
            reserved = new RoomBits(self.coo, new snake2d.util.bit.Bits(RESERVED));
            working = new RoomBits(self.coo, new snake2d.util.bit.Bits(RESERVED >> 1));
            free = new RoomBits(self.coo, new snake2d.util.bit.Bits(RESERVED >> 2));
        }

        public static Crank Init(int tx, int ty, ROOM_BATH b)
        {
            if (!b.Is(tx, ty))
                return null;

            BathInstance ins = b.Getter.Get(tx, ty);

            int data = ROOMS().Data.Get(tx, ty);
            if ((data & BITS) != BIT)
                return null;

            foreach (DIR d in DIR.ORTHO)
            {
                if (ins.Is(tx, ty, d) && (ROOMS().Data.Get(tx, ty, d) & BITS) == SERVICE)
                {
                    self.bath = b.Bath(tx + d.X(), ty + d.Y());
                    self.coo.Set(tx, ty);
                    self.ins = b.Get(tx, ty);
                    return self;
                }
            }
            throw new Exception();
        }

        private Crank()
        {
        }

        public static bool Working(int data)
        {
            return self.working.Get(data) == 1;
        }

        public override bool JobUseTool()
        {
            return false;
        }

        public override void JobStartPerforming()
        {
            working.Set(ins, 1);
        }

        public override SoundRace JobSound()
        {
            return ins.BlueprintI().Employment().Sound();
        }

        public override RBIT JobResourceBitToFetch()
        {
            return null;
        }

        public override bool JobReservedIs(RESOURCE r)
        {
            return reserved.Get() != 0;
        }

        public override void JobReserveCancel(RESOURCE r)
        {
            working.Set(ins, 0);
            reserved.Set(ins, 0);
        }

        public override bool JobReserveCanBe()
        {
            return bath.AvailbilityNeeds() && !JobReservedIs(null);
        }

        public override void JobReserve(RESOURCE r)
        {
            if (JobReservedIs(null))
                throw new Exception();
            reserved.Set(ins, 1);
        }

        public override double JobPerformTime(Humanoid skill)
        {
            return free.Get() == 1 ? 1 : wt;
        }

        public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
        {
            bath.AvailabilityInc();
            JobReserveCancel(null);
            if (ins.Employees().FetchBonusConsume(wt + 1))
            {
                free.Set(ins, 1);
            }
            else
            {
                free.Set(ins, 0);
            }
            return null;
        }

        private readonly string name = "pumping water";

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