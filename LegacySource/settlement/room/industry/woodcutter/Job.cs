using System;
using settlement.main;
using game.audio;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d.util.datatypes;
using util;

namespace settlement.room.industry.woodcutter
{
    class Job
    {
        private readonly ROOM_WOODCUTTER print;
        private readonly Coo jobCoo = new Coo();
        private Instance ins;
        private readonly RoomBits is_ = new RoomBits(jobCoo, 0b0000_0001);
        private readonly RoomBits reserved = new RoomBits(jobCoo, 0b0000_0010);
        private readonly RoomBits used = new RoomBits(jobCoo, 0b0000_0100)
        {
            public override void Set(ROOMA r, int t)
            {
                ins.workage -= base.Get();
                base.Set(r, t);
                ins.workage += base.Get();
            }
        };
        private readonly RoomBits chopped = new RoomBits(jobCoo, 0xFFFFFFF0);

        private readonly double wv = 60;
        private readonly int workPerDay = (int)Math.Ceiling(TIME.workSeconds() / wv);

        public readonly RoomResStorage storage;

        public Job(ROOM_WOODCUTTER print, int store)
        {
            this.print = print;
            storage = new RoomResStorage(store)
            {
                public override RESOURCE Resource()
                {
                    return print.productionData.outs()[0].resource;
                }

                protected override bool Is(int tx, int ty)
                {
                    return SETT.ROOMS().fData.tileData.Get(tx, ty) == Constructor.B_STORAGE;
                }

                protected override void Changed(int tx, int ty)
                {
                    if (HasRoom())
                    {
                        Instance m = print.Get(tx, ty);
                        m.hasStorage = true;
                    }
                }
            };
        }

        public SETT_JOB Init(int tx, int ty, Instance ins)
        {
            if (!ins.Is(tx, ty))
                return null;
            jobCoo.Set(tx, ty);
            this.ins = ins;
            if (is_.Get() == 0)
                return null;

            return work;
        }

        public void Mark(int tx, int ty, Instance ins)
        {
            jobCoo.Set(tx, ty);
            this.ins = ins;
            is_.Set(ins, 1);
        }

        public void Update(int tx, int ty, Instance ins)
        {
            jobCoo.Set(tx, ty);
            if (is_.Get() == 0)
                return;
            if (SETT.ROOMS().fData.item.Get(tx, ty) != null)
                return;
            if (SETT.ROOMS().fData.tileData.Get(jobCoo) == Constructor.B_WORK)
                return;
            if (SETT.ROOMS().fData.tileData.Get(jobCoo) == Constructor.B_STORAGE)
                return;
            if (chopped.Get() == 0)
            {
                chopped.Inc(ins, 1);
                return;
            }

            double d = ins.irri - (GUTIL.ran2().Get(tx, ty) % 0x0FF) / (double)0x0FF;
            if (d > 0)
            {
                if (SETT.TERRAIN().TREES.IsTree(jobCoo.x(), jobCoo.y()))
                {
                    TERRAIN().TREES.amount.Increment(tx, ty, 1);
                }
                else if (SETT.TERRAIN().BUSH.Is(jobCoo))
                {
                    TERRAIN().TREES.SMALL.PlaceRaw(tx, ty);
                    TERRAIN().TREES.amount.Set(tx, ty, 1);
                }
                else
                {
                    SETT.TERRAIN().BUSH.PlaceFixed(tx, ty);
                }
            }
            else
            {
                if (SETT.TERRAIN().TREES.IsTree(jobCoo.x(), jobCoo.y()))
                {
                    SETT.TERRAIN().BUSH.PlaceFixed(tx, ty);
                }
                else if (SETT.TERRAIN().BUSH.Is(jobCoo))
                {
                    TERRAIN().NADA.PlaceRaw(tx, ty);
                }
            }
        }

        public static bool IsTreeCurrent(int tx, int ty)
        {
            return SETT.TERRAIN().TREES.IsTree(tx, ty) || SETT.TERRAIN().BUSH.Is(tx, ty);
        }

        public static bool Working(int data)
        {
            return (data & 0b10) != 0;
        }

        private readonly SETT_JOB work = new SETT_JOB()
        {
            public override bool JobReserveCanBe()
            {
                if (JobReservedIs(null))
                    return false;
                if (!ins.hasStorage)
                    return false;
                return true;
            }

            public override COORDINATE JobCoo()
            {
                return jobCoo;
            }

            public override CharSequence JobName()
            {
                return print.employment().verb;
            }

            public override bool JobUseTool()
            {
                return true;
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return wv;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (JobReservedIs(null))
                    throw new RuntimeException();
                reserved.Set(ins, 1);
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return reserved.Get() == 1;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                reserved.Set(ins, 0);
                used.Set(ins, 0);
            }

            public override void JobStartPerforming()
            {
                used.Set(ins, 1);
            }

            public override SoundRace JobSound()
            {
                return ins.blueprintI().employment().sound();
            }

            public override RESOURCE JobPerform(Humanoid s, RESOURCE res, int ram)
            {
                JobReserveCancel(null);

                chopped.Inc(ins, 1);
                if (chopped.Get() > workPerDay && SETT.ROOMS().fData.tileData.Get(jobCoo) != Constructor.B_WORK)
                {
                    TERRAIN().DECOR_WOOD.PlaceFixed(jobCoo.x(), jobCoo.y());
                    chopped.Set(ins, 0);
                }

                int am = print.productionData.outs()[0].Work(s, ins, wv);

                if (am == 0)
                    return null;

                if (!ins.hasStorage)
                    return null;

                int x1 = ins.sx;
                int y1 = ins.sy;
                RoomResStorage ss = storage.Get(x1, y1, ins);

                while (ss != null)
                {
                    if (am == 0 && ss.HasRoom())
                        return null;
                    if (ss.HasRoom())
                    {
                        ss.Deposit();
                        am--;
                        continue;
                    }

                    RoomResStorage sss = storage.Get(ss.x() + 1, ss.y(), ins);
                    if (sss == null)
                        sss = storage.Get(x1, ss.y() + 1, ins);
                    ss = sss;
                }

                print.productionData.outs()[0].Inc(ins, -am);

                ins.hasStorage = false;

                return null;
            }
        };
    }
}