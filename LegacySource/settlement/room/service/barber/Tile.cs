using System;
using snake2d.util.datatypes;
using settlement.room.main.util;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.entity.humanoid;
using init.resources;

namespace settlement.room.service.barber
{
    final class Tile
    {
        private readonly Coo coo = new Coo();
        private Instance ins;
        private readonly ROOM_BARBER blue;

        private readonly int workTime;

        readonly RoomBits bWorked = new RoomBits(coo, 0b0000_0000_1111);
        readonly RoomBits bUses = new RoomBits(coo, 0b0000_0111_0000)
        {
            protected override void remove()
            {
                ins.service.report(service, ins.blueprintI().data, -1);
            }

            protected override void add()
            {
                ins.service.report(service, ins.blueprintI().data, 1);
            }
        };

        readonly RoomBits bWReserved = new RoomBits(coo, 0b0001_0000_0000);
        readonly RoomBits bSReserved = new RoomBits(coo, 0b0010_0000_0000)
        {
            protected override void remove()
            {
                ins.service.report(service, ins.blueprintI().data, -1);
            }

            protected override void add()
            {
                ins.service.report(service, ins.blueprintI().data, 1);
            }
        };

        Tile(ROOM_BARBER blue, int workTime)
        {
            int t = workTime / 16;
            this.workTime = t;
            this.blue = blue;
        }

        public SETT_JOB job(int tx, int ty)
        {
            if (init(tx, ty))
                return job;
            return null;
        }

        public FSERVICE service(int tx, int ty)
        {
            if (init(tx, ty))
                return service;
            return null;
        }

        private bool init(int tx, int ty)
        {
            ins = blue.getter.get(tx, ty);

            if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.IWORK)
            {
                coo.set(tx, ty);
                return true;
            }
            return false;
        }

        readonly SETT_JOB job = new SETT_JOB()
        {
            public void jobReserve(RESOURCE r)
            {
                if (!jobReserveCanBe())
                    throw new RuntimeException();
                bWReserved.set(ins, 1);
            }

            public bool jobReservedIs(RESOURCE r)
            {
                return bWReserved.get() == 1;
            }

            public void jobReserveCancel(RESOURCE r)
            {
                bWReserved.set(ins, 0);
            }

            public bool jobReserveCanBe()
            {
                return bWReserved.get() == 0 && (bUses.get() < bUses.max() || bWorked.get() < bWorked.max());
            }

            public RBIT jobResourceBitToFetch()
            {
                return null;
            }

            public double jobPerformTime(Humanoid skill)
            {
                return workTime;
            }

            public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram)
            {
                if (!jobReservedIs(res))
                    throw new RuntimeException();
                bWReserved.set(ins, 0);
                if (bWorked.get() == bWorked.max())
                {
                    bUses.inc(ins, 1);
                    bWorked.set(ins, 0);
                }
                else
                {
                    bWorked.inc(ins, 1);
                }
                return null;
            }

            public COORDINATE jobCoo()
            {
                return coo;
            }

            private static readonly string name = "setting table";

            public string jobName()
            {
                return name;
            }

            public void jobStartPerforming()
            {
                // TODO Auto-generated method stub
            }

            public bool jobUseTool()
            {
                return false;
            }

            public SoundRace jobSound()
            {
                return blue.employment().sound();
            }

            public bool jobUseHands()
            {
                return true;
            }
        };

        private readonly FSERVICE service = new FSERVICE()
        {
            public int y()
            {
                return coo.y();
            }

            public int x()
            {
                return coo.x();
            }

            public bool findableReservedIs()
            {
                return bSReserved.get() == 1;
            }

            public bool findableReservedCanBe()
            {
                return bSReserved.get() == 0 && bUses.get() > 0;
            }

            public void findableReserveCancel()
            {
                bSReserved.set(ins, 0);
            }

            public void findableReserve()
            {
                bSReserved.set(ins, 1);
            }

            public void consume()
            {
                bSReserved.set(ins, 0);

                //create resource
            }

            public void startUsing()
            {
                bUses.inc(ins, -1);
            }
        };
    }
}