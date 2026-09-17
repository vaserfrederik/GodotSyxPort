using init.resources;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main.job;
using snake2d.util.datatypes;

namespace settlement.room.industry.refiner
{
    class Job
    {
        readonly RoomResDeposit FETCH;
        readonly RoomResStorage storage;

        public Job(ROOM_REFINER print, int store)
        {
            storage = new RoomResStorage(store)
            {
                Resource = () =>
                {
                    ROOM_PRODUCER_INSTANCE ins = (ROOM_PRODUCER_INSTANCE)SETT.ROOMS().map.get(this);
                    return ins.industry().outs().get(0).resource;
                },
                Is = (tx, ty) =>
                {
                    return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
                },
                Changed = (tx, ty) =>
                {
                    if (hasRoom())
                    {
                        RefinerInstance m = print.get(tx, ty);
                        m.hasStorage = true;
                        m.jobs.searchAgain();
                    }
                }
            };
            FETCH = new RoomResDeposit(print)
            {
                Is = (tx, ty) =>
                {
                    return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK;
                },
                HasCallback = () =>
                {
                    // TODO Auto-generated method stub
                },
                RegularJobCanBeReserved = (coo) =>
                {
                    RefinerInstance ins = print.get(coo.x(), coo.y());
                    return ins.hasStorage;
                },
                RegularJobStore = (coo, am) =>
                {
                    RefinerInstance ins = print.get(coo.x(), coo.y());
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

        public SETT_JOB Init(int tx, int ty, RefinerInstance ins)
        {
            return FETCH.get(tx, ty, ins);
        }
    }
}