using game.audio;
using game.faction;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.health.hospital
{
    public class Bed
    {
        private readonly Coo coo = new Coo();
        private HospitalInstance ins;

        private readonly RoomBits wreserved = new RoomBits(coo, 0b0000_0000_0001);
        private readonly RoomBits wres1 = new RoomBits(coo, 0b0000_0000_1110);
        private readonly RoomBits wres2 = new RoomBits(coo, 0b0000_1111_0000);
        private readonly RoomBits freeWork = new RoomBits(coo, 0b0000_1000_0000);
        private readonly RoomBits sstate = new RoomBits(coo, 0b1111_0000_0000)
        {
            public override void Set(ROOMA r, int t)
            {
                ins.service().Report(service, ins.blueprintI().service(), -1);
                base.Set(r, t);
                ins.service().Report(service, ins.blueprintI().service(), 1);
            }
        };

        private const int I_UNAVAILABLE = 0;
        private const int I_AVAILABLE = 1;
        private const int I_RESERVED = 2;
        private const int I_USED = 3;

        private static readonly Bed self = new Bed();

        public static SETT_JOB Job(int tx, int ty)
        {
            if (self.Init(tx, ty))
                return self.job;
            return null;
        }

        public static FSERVICE Service(int tx, int ty)
        {
            if (self.Init(tx, ty))
                return self.service;
            return null;
        }

        public static bool Res1(int tx, int ty)
        {
            if (!self.Init(tx, ty))
                return false;
            return self.wres1.Get() > 0;
        }

        public static bool Res2(int tx, int ty)
        {
            if (!self.Init(tx, ty))
                return false;
            return self.wres2.Get() > 0;
        }

        public static void Consume(int tx, int ty)
        {
        }

        private bool Init(int tx, int ty)
        {
            ins = B().Get(tx, ty);
            if (ins != null && SETT.ROOMS().FData.TileData.Get(tx, ty) == Constructor.CODE_S)
            {
                coo.Set(tx, ty);
                return true;
            }
            return false;
        }

        public static bool Made(int tx, int ty)
        {
            return self.Init(tx, ty) && self.sstate.Get() != self.I_UNAVAILABLE;
        }

        public static bool Resource(int tx, int ty)
        {
            return self.Init(tx, ty) && self.wres1.Get() > 0 && self.sstate.Get() != self.I_USED;
        }

        private static ROOM_HOSPITAL B()
        {
            return SETT.ROOMS().HOSPITAL;
        }

        private readonly SETT_JOB job = new SETT_JOB
        {
            private readonly int wt = 30;
            private readonly RBITImp bits = new RBITImp();

            public override bool JobUseTool()
            {
                return false;
            }

            public override bool JobUseHands()
            {
                return sstate.Get() == I_UNAVAILABLE;
            }

            public override void JobStartPerforming()
            {
            }

            public override SoundRace JobSound()
            {
                return B().Employment().Sound();
            }

            public override RBIT JobResourceBitToFetch()
            {
                bits.Clear();
                if (sstate.Get() == I_UNAVAILABLE)
                    return null;
                if (wres1.Get() == 0 && ins.Fetch[0] && B().ResLocks.Get(0).Passes(FACTIONS.player()))
                    bits.Or(B().Indus.Get(0).Ins().Get(0).Resource);
                if (wres2.Get() == 0 && ins.Fetch[1] && B().ResLocks.Get(1).Passes(FACTIONS.player()))
                    bits.Or(B().Indus.Get(0).Ins().Get(1).Resource);
                return bits.IsClear() ? null : bits;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return wreserved.Get() == 1;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                wreserved.Set(ins, 0);
            }

            public override bool JobReserveCanBe()
            {
                if (wreserved.Get() == 1)
                    return false;
                if (sstate.Get() == I_UNAVAILABLE)
                    return true;
                if (wres1.Get() == 0 && ins.Fetch[0] && B().ResLocks.Get(0).Passes(FACTIONS.player()))
                    return true;
                if (wres2.Get() == 0 && ins.Fetch[1] && B().ResLocks.Get(1).Passes(FACTIONS.player()))
                    return true;
                return false;
            }

            public override void JobReserve(RESOURCE r)
            {
                wreserved.Set(ins, 1);
            }

            public override double JobPerformTime(Humanoid a)
            {
                return freeWork.Get() == 1 ? 1 : wt;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                JobReserveCancel(r);
                if (r == B().Indus.Get(0).Ins().Get(0).Resource)
                {
                    B().Indus.Get(0).Ins().Get(0).Inc(ins, rAm);
                    wres1.Inc(ins, rAm);
                }
                else if (r == B().Indus.Get(0).Ins().Get(1).Resource)
                {
                    B().Indus.Get(0).Ins().Get(1).Inc(ins, rAm);
                    wres2.Inc(ins, rAm);
                }
                else
                {
                    if (sstate.Get() == I_UNAVAILABLE)
                    {
                        sstate.Set(ins, I_AVAILABLE);
                    }
                }
                freeWork.Set(ins, ins.Employees().FetchBonusConsume(wt + 1) ? 1 : 0);
                return null;
            }

            public override CharSequence JobName()
            {
                return B().Employment().Verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }
        };

        public readonly FSERVICE service = new FSERVICE
        {
            public override int Y()
            {
                return coo.Y();
            }

            public override int X()
            {
                return coo.X();
            }

            public override bool FindableReservedIs()
            {
                return sstate.Get() == I_RESERVED || sstate.Get() == I_USED;
            }

            public override bool FindableReservedCanBe()
            {
                return sstate.Get() == I_AVAILABLE;
            }

            public override void FindableReserveCancel()
            {
                sstate.Set(ins, I_AVAILABLE);
            }

            public override void FindableReserve()
            {
                if (FindableReservedCanBe())
                    sstate.Set(ins, I_RESERVED);
            }

            public override void StartUsing()
            {
                sstate.Set(ins, I_USED);
            }

            public override void Consume()
            {
                wres1.Inc(ins, -1);
                wres2.Inc(ins, -1);
                sstate.Set(ins, I_UNAVAILABLE);
                ins.Jobs.SearchAgain();
            }
        };
    }
}