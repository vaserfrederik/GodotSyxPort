using init.resources;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main.job;
using snake2d.util.datatypes;

namespace settlement.room.industry.workshop
{
    class Job
    {
        private readonly ROOM_WORKSHOP print;
        public readonly RoomResDeposit FETCH;

        public readonly RoomResStorage storage = new RoomResStorage(0b011111)
        {
            public override RESOURCE resource()
            {
                ROOM_PRODUCER_INSTANCE ins = (ROOM_PRODUCER_INSTANCE)SETT.ROOMS().map.get(this);
                return ins.industry().outs().get(0).resource;
            }

            protected override bool is(int tx, int ty)
            {
                return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
            }

            protected override void changed(int tx, int ty)
            {
                if (hasRoom())
                {
                    WorkshopInstance m = print.get(tx, ty);
                    m.hasStorage = true;
                    m.jobs.searchAgain();
                }
            }
        };

        public Job(ROOM_WORKSHOP print)
        {
            this.print = print;
            FETCH = new RoomResDeposit(print)
            {
                protected override bool is(int tx, int ty)
                {
                    return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK;
                }

                protected override void hasCallback()
                {
                    // TODO Auto-generated method stub
                }

                protected override bool regularJobCanBeReserved(COORDINATE coo)
                {
                    WorkshopInstance ins = print.get(coo.x(), coo.y());
                    return ins.hasStorage;
                }

                protected override void regularJobStore(COORDINATE coo, int am)
                {
                    WorkshopInstance ins = print.get(coo.x(), coo.y());
                    int x1 = ins.sx;
                    int y1 = ins.sy;
                    RoomResStorage ss = storage.get(x1, y1, ins);

                    while (ss != null && am > 0)
                    {
                        if (ss.hasRoom())
                        {
                            ss.deposit();
                            am--;
                            continue;
                        }

                        RoomResStorage sss = storage.get(ss.x() + 1, ss.y(), ins);
                        if (sss == null)
                            sss = storage.get(x1, ss.y() + 1, ins);
                        ss = sss;
                    }
                    if (am > 0)
                        ins.hasStorage = false;
                }
            };
        }

        public SETT_JOB init(int tx, int ty, WorkshopInstance ins)
        {
            return FETCH.get(tx, ty, ins);
        }

        // static DIR find(COORDINATE coo, int data, WorkshopInstance ins)
        // {
        //     for (DIR d : DIR.ORTHO)
        //     {
        //         if (ins.is(coo, d) && SETT.ROOMS().fData.tileData.get(coo, d) == data)
        //             return d;
        //     }
        //     throw new RuntimeException();
        // }

        // static DIR find(int tx, int ty, int data, WorkshopInstance ins)
        // {
        //     for (DIR d : DIR.ORTHO)
        //     {
        //         if (ins.is(tx, ty, d) && SETT.ROOMS().fData.tileData.get(tx, ty, d) == data)
        //             return d;
        //     }

        //     throw new RuntimeException();
        // }
    }
}