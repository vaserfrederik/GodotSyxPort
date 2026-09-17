using System;
using game.audio;
using game.time;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main.util;
using settlement.stats;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.service.breeder
{
    class Station
    {
        private Coo coo = new Coo();
        private Coo masterCoo = new Coo();
        private BreederInstance ins;
        private readonly ROOM_BREEDER b;

        private readonly RoomBits masterRes = new RoomBits(masterCoo, new Bits(0x00000FFF));
        private readonly RoomBits masterActivity = new RoomBits(masterCoo, new Bits(0x0001F000));
        private readonly RoomBits masterActivityCount = new RoomBits(masterCoo, new Bits(0x00010000));
        private readonly RoomBits masterWorkPlaces = new RoomBits(masterCoo, new Bits(0x0FF00000));

        private readonly RoomBits reserved = new RoomBits(coo, new Bits(0x0000000F));

        public Station(ROOM_BREEDER b)
        {
            this.b = b;
        }

        public SETT_JOB Get(int tx, int ty)
        {
            if (Ini(tx, ty) && SETT.ROOMS().fData.tileData.Get(tx, ty) == BreederConstructor.WORK)
            {
                this.coo.Set(tx, ty);
                return job;
            }
            return null;
        }

        private bool Ini(int tx, int ty)
        {
            if (b.Is(tx, ty) && SETT.ROOMS().fData.Item.Is(tx, ty))
            {
                SETT.ROOMS().fData.ItemX1Y1(tx, ty, masterCoo);
                this.ins = b.Get(tx, ty);
                return true;
            }
            return false;
        }

        public bool Init(int tx, int ty)
        {
            SETT_JOB j = Get(tx, ty);
            if (j != null)
                masterWorkPlaces.Inc(ins, 1);
            return j != null;
        }

        public void Dispose(int x, int y)
        {
            if (!Ini(x, y))
                return;
            int am = masterRes.Get();
            masterRes.Set(ins, 0);
            if (am > 0)
            {
                SETT.THINGS().resources.Create(x, y, b.indus.Get(0).Ins().Get(0).resource, am);
            }
        }

        public int Resources(int tx, int ty, int ran)
        {
            if (!Ini(tx, ty))
                return 0;

            double rr = (double)masterRes.Get() / masterWorkPlaces.Get();
            rr = (int)rr + ((ran & 0x0F) / (double)0x0F) * rr;

            return CLAMP.i((int)rr, 0, 8);
        }

        public bool Worm(int tx, int ty, int ran)
        {
            if (!Ini(tx, ty))
                return false;

            double a = masterActivity.GetD() * 2.0;

            return a > ((ran & 0x0F) / (double)0x0F);
        }

        public double ASpeed(int tx, int ty)
        {
            if (!Ini(tx, ty))
                return 0;

            double a = masterActivity.GetD() * 2.0;
            return CLAMP.d(a, 0, 1);
        }

        public void Update(int tx, int ty)
        {
            if (Get(tx, ty) != null)
                masterActivity.Inc(ins, -1);
        }

        private int Resources()
        {
            return masterRes.Get() / masterWorkPlaces.Get();
        }

        private SETT_JOB job = new SETT_JOB()
        {
            private int wt = 30,

            public bool JobUseTool()
            {
                return false;
            }

            public void JobStartPerforming()
            {
            }

            public SoundRace JobSound()
            {
                return b.Employment().Sound();
            }

            public RBIT JobResourceBitToFetch()
            {
                if (Resources() < 1)
                    return b.indus.Get(0).Ins().Get(0).resource.bit;
                return null;
            }

            public bool JobReservedIs(RESOURCE r)
            {
                return reserved.Get() == 1;
            }

            public void JobReserveCancel(RESOURCE r)
            {
                reserved.Set(ins, 0);
            }

            public bool JobReserveCanBe()
            {
                if (reserved.Get() == 1)
                    return false;
                if (!b.CanWork())
                    return false;
                return true;
            }

            public void JobReserve(RESOURCE r)
            {
                reserved.Set(ins, 1);
            }

            public double JobPerformTime(Humanoid skill)
            {
                return wt;
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE res, int am)
            {
                JobReserveCancel(res);
                if (res != null)
                {
                    am = SETT.ROOMS().resourceUnderflow.Deposit(res, am);
                    if (am > 0)
                    {
                        masterRes.Inc(ins, am);
                    }
                    return null;
                }

                if (masterActivityCount.Get() == 0)
                    masterActivity.Inc(ins, 1);

                int t = ins.Employees().FetchBonus(wt);

                foreach (IndustryResource r in ins.Industry().Ins())
                {
                    int a = r.Work(skill, ins, t);
                    if (a > 0)
                    {
                        int max = masterRes.Get();
                        a = SETT.ROOMS().resourceUnderflow.Withdraw(r.resource, a, max);
                        masterRes.Inc(ins, -a);
                    }
                }

                double w = IndustryUtil.CalcProductionRate(t * b.PRODUCTION_SPEED_DAY / TIME.workSeconds(), skill, b.productionData, ins);
                ins.kidsProduction += w;

                while (ins.kidsProduction > 1 && b.CanWork())
                {
                    ins.kidsProduction--;
                    HTYPE ty = HTYPES.CHILD();
                    if (b.prosecute)
                    {
                        ty = HTYPES.CHILD_SLAVE();
                    }
                    Humanoid h = SETT.HUMANOIDS().Create(b.race, skill.Tc().X(), skill.Tc().Y(), ty, CAUSE_ARRIVES.BORN());
                    if (h != null)
                    {
                        STATS.POP().age.DAYS.Set(h.Indu(), 0);
                        STATS.POP().TYPE.NATIVE.Set(h.Indu());
                        STATS.REL().SetParent(h.Indu(), skill.Indu());
                    }
                }

                return null;
            }

            public CharSequence JobName()
            {
                return b.Employment().verb;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }
        };
    }
}