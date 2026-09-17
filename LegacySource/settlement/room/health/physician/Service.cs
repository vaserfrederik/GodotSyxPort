using System;
using settlement.room.health.physician;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.health.physician
{
    internal sealed class Service
    {
        private readonly Coo coo = new Coo();

        private readonly RoomBits s_worked = new RoomBits(coo, 0b0000_0000_0001);
        private readonly Bit s_reservable = new Bit(coo, 0b0000_0000_0010);
        private readonly Bit s_reserved = new Bit(coo, 0b0000_0000_0100);
        private readonly RoomBits s_worked_amount = new RoomBits(coo, 0b0000_1111_0000);
        private readonly ROOM_PHYSICIAN b;
        private Instance ins;

        public Service(ROOM_PHYSICIAN b)
        {
            this.b = b;
        }

        public FSERVICE GetS(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins != null && SETT.ROOMS().fData.tileData.Get(tx, ty) == Constructor.BIT_SERVICE)
            {
                coo.Set(tx, ty);
                return service;
            }
            return null;
        }

        public void Dispose(Instance ins, int tx, int ty)
        {
            if (GetS(tx, ty) != null)
                s_worked.Set(ins, 0);
        }

        public SETT_JOB GetJ(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins != null && SETT.ROOMS().fData.tileData.Get(tx, ty) != 0)
            {
                coo.Set(tx, ty);
                return jo;
            }
            return null;
        }

        private readonly FSERVICE service = new FSERVICE()
        {
            public override bool FindableReservedCanBe()
            {
                return s_reservable.Get() == 1 && s_reserved.Get() == 0;
            }

            public override void FindableReserve()
            {
                ins.Jobs.SearchAgain();
                s_reserved.Set(ins, 1);
            }

            public override bool FindableReservedIs()
            {
                return s_reserved.Get() == 1;
            }

            public override void FindableReserveCancel()
            {
                s_reserved.Set(ins, 0);
            }

            public override int X()
            {
                return coo.X();
            }

            public override int Y()
            {
                return coo.Y();
            }

            public override void StartUsing()
            {
            }

            public override void Consume()
            {
                s_worked_amount.Inc(ins, -1);
                s_reservable.Set(ins, s_worked_amount.Get() > 0 ? 1 : 0);
                s_reserved.Set(ins, 0);
            }
        };

        private readonly SETT_JOB jo = new SETT_JOB()
        {
            public override bool JobUseTool()
            {
                return false;
            }

            public override bool JobUseHands()
            {
                return false;
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
                return null;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return s_worked.Get() == 1;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                s_worked.Set(ins, 0);
            }

            public override bool JobReserveCanBe()
            {
                return (s_worked_amount.Get() < 7 || s_reservable.Get() == 0 || s_reserved.Get() == 1) && !JobReservedIs(null);
            }

            public override void JobReserve(RESOURCE r)
            {
                s_worked.Set(ins, 1);
            }

            public override double JobPerformTime(Humanoid a)
            {
                return 20;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                s_worked.Set(ins, 0);
                s_worked_amount.Inc(ins, 1);
                s_reservable.Set(ins, 1);
                return null;
            }

            public override string JobName()
            {
                return b.Employment().Verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }
        };

        private class Bit : RoomBits
        {
            public Bit(COORDINATE coo, int mask) : base(coo, mask)
            {
            }

            public override void Set(ROOMA r, int t)
            {
                ins.Service.Report(service, ins.BlueprintI().Data, -1);
                base.Set(r, t);
                ins.Service.Report(service, ins.BlueprintI().Data, 1);
            }
        }
    }
}