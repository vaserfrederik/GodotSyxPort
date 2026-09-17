using System;
using game.audio;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.service.nursery
{
    class NurseryStation
    {
        private readonly Coo coo = new Coo();
        private NurseryInstance ins;
        private readonly ROOM_NURSERY b;

        private readonly RoomBits bMark = new RoomBits(coo, new Bits(0b0000_0000_0001));
        private readonly RoomBits bWorked = new BB(coo, new Bits(0b0000_0000_1110));
        private readonly RoomBits bWorkReserved = new RoomBits(coo, new Bits(0b0000_0001_0000));
        private readonly RoomBits bServiceReserved = new BB(coo, new Bits(0b0000_0010_0000));

        private readonly int wt;

        NurseryStation(ROOM_NURSERY b)
        {
            this.b = b;

            double worksPerDay = b.ChildPErE * (TIME.workSeconds() / ROOM_NURSERY.playTime);
            wt = (int)(TIME.workSeconds() / worksPerDay);
        }

        private bool pinit(int tx, int ty)
        {
            if (b.is(tx, ty))
            {
                this.coo.set(tx, ty);
                this.ins = b.get(tx, ty);
                if (bMark.get() == 1)
                    return true;
            }
            return false;
        }

        public SETT_JOB job(int tx, int ty)
        {
            if (pinit(tx, ty))
                return job;
            return null;
        }

        public FSERVICE service(int tx, int ty)
        {
            if (pinit(tx, ty))
                return service;
            return null;
        }

        public void init(RoomInstance ins, int tx, int ty)
        {
            coo.set(tx, ty);
            bMark.set(ins, 1);
        }

        public int stuff(int tx, int ty)
        {
            if (pinit(tx, ty))
                return 3 - bWorked.get();
            return 0;
        }

        private readonly SETT_JOB job = new SETT_JOB()
        {
            public override bool jobUseTool()
            {
                return false;
            }

            public override void jobStartPerforming()
            {
            }

            public override SoundRace jobSound()
            {
                return b.employment().sound();
            }

            public override RBIT jobResourceBitToFetch()
            {
                return null;
            }

            public override bool jobReservedIs(RESOURCE r)
            {
                return bWorkReserved.get() == 1;
            }

            public override void jobReserveCancel(RESOURCE r)
            {
                bWorkReserved.set(ins, 0);
            }

            public override bool jobReserveCanBe()
            {
                if (bWorkReserved.get() == 1)
                    return false;

                if (bWorked.get() >= 3)
                    return false;
                return true;
            }

            public override void jobReserve(RESOURCE r)
            {
                bWorkReserved.set(ins, 1);
            }

            public override double jobPerformTime(Humanoid skill)
            {
                double d = IndustryUtil.calcProductionRate(1, skill, b.rate, ins);
                if (d == 0)
                    return wt * 5;
                return wt / (d);
            }

            public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int am)
            {
                bWorkReserved.set(ins, 0);
                bWorked.inc(ins, 1);
                return null;
            }

            public override CharSequence jobName()
            {
                return b.employment().verb;
            }

            public override COORDINATE jobCoo()
            {
                return coo;
            }
        };

        private readonly FSERVICE service = new FSERVICE()
        {
            public override int y()
            {
                return coo.y();
            }

            public override int x()
            {
                return coo.x();
            }

            public override bool findableReservedIs()
            {
                return bServiceReserved.get() == 1;
            }

            public override bool findableReservedCanBe()
            {
                return bServiceReserved.get() == 0 && bWorked.get() > 0;
            }

            public override void findableReserveCancel()
            {
                bServiceReserved.set(ins, 0);
            }

            public override void findableReserve()
            {
                if (findableReservedCanBe())
                    bServiceReserved.set(ins, 1);
            }

            public override void startUsing()
            {
                bWorked.inc(ins, -1);
            }

            public override void consume()
            {
                bServiceReserved.set(ins, 0);

                ins.getWork().searchAgain();
            }
        };

        private class BB : RoomBits
        {
            public BB(COORDINATE coo, Bits bits) : base(coo, bits)
            {
            }

            public override void set(int tx, int ty, ROOMA r, int t)
            {
                ins.service().report(service, ins.blueprintI().service(), -1);
                base.set(tx, ty, r, t);
                ins.service().report(service, ins.blueprintI().service(), 1);
            }
        }
    }
}