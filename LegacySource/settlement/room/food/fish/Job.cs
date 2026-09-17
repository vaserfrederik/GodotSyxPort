using System;
using settlement.main;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.room.main.job;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.food.fish
{
    class Job
    {
        static readonly Bit isWork = new Bit(0b0001);
        static readonly Bit isShip = new Bit(0b0010);
        static readonly Bit reserved = new Bit(0b0100);
        static readonly Bit used = new Bit(0b1000);
        static readonly Bits shipDir = new Bits(0b1111_0000);

        private readonly ROOM_FISHERY print;
        private readonly Work WorkHands = new Work(false);
        public readonly RoomResStorage storage = new RoomResStorage(0b011111)
        {
            @override
            public RESOURCE resource()
            {
                return print.productionData.outs().get(0).resource;
            }

            @override
            protected bool is(int tx, int ty)
            {
                return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
            }

            @override
            protected void changed(int tx, int ty)
            {
                if (hasRoom())
                {
                    FishInstance m = print.get(tx, ty);
                    m.hasStorage = true;
                }
            }
        };

        public Job(ROOM_FISHERY print)
        {
            this.print = print;
        }

        public SETT_JOB init(int tx, int ty, FishInstance ins)
        {
            if (!ins.is(tx, ty))
                return null;
            int d = ROOMS().data.get(tx, ty);

            if (SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE)
                return null;

            if (isWork.is(d) || ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK)
            {
                return WorkHands.init(tx, ty, ins);
            }
            return null;
        }

        static public bool working(int data)
        {
            return used.is(data);
        }

        public class Work : SETT_JOB
        {
            private readonly bool tools;

            private readonly Coo coo = new Coo();
            public FishInstance ins;
            public int data;
            public static readonly string name = "working";
            private readonly double wv = 60;

            public Work(bool tools)
            {
                this.tools = tools;
            }

            @override
            public bool jobReserveCanBe()
            {
                if (jobReservedIs(null))
                    return false;
                if (!ins.hasStorage)
                    return false;
                return true;
            }

            public Work init(int tx, int ty, FishInstance ins)
            {
                data = ROOMS().data.get(tx, ty);
                coo.set(tx, ty);
                this.ins = ins;
                return this;
            }

            void save()
            {
                ROOMS().data.set(ins, coo, data);
            }

            @override
            public COORDINATE jobCoo()
            {
                return coo;
            }

            @override
            public string jobName()
            {
                return name;
            }

            @override
            public bool jobUseTool()
            {
                return tools;
            }

            @override
            public RBIT jobResourceBitToFetch()
            {
                return null;
            }

            @override
            public double jobPerformTime(Humanoid skill)
            {
                return 60;
            }

            @override
            public void jobReserve(RESOURCE r)
            {
                if (jobReservedIs(null))
                    throw new RuntimeException();
                data = reserved.set(data);
                save();
            }

            @override
            public bool jobReservedIs(RESOURCE r)
            {
                return (reserved.is(data));
            }

            @override
            public void jobReserveCancel(RESOURCE r)
            {
                data = reserved.clear(data);
                data = used.clear(data);
                save();
            }

            long now;

            @override
            public void jobStartPerforming()
            {
                now = System.currentTimeMillis();
                data = used.set(data);
                save();
            }

            @override
            public SoundRace jobSound()
            {
                return ins.blueprintI().employment().sound();
            }

            @override
            public RESOURCE jobPerform(Humanoid s, RESOURCE res, int ram)
            {
                secretPerform(s, wv);
                return null;
            }
        }

        public void secretPerform(Humanoid s, double time)
        {
            WorkHands.jobReserveCancel(null);

            int am = print.productionData.outs().get(0).work(s, WorkHands.ins, time);

            if (am == 0)
                return;

            if (!WorkHands.ins.hasStorage)
                return;

            int x1 = WorkHands.ins.sx;
            int y1 = WorkHands.ins.sy;
            RoomResStorage ss = storage.get(x1, y1, WorkHands.ins);

            while (ss != null)
            {
                if (am == 0 && ss.hasRoom())
                    return;
                if (ss.hasRoom())
                {
                    ss.deposit();
                    am--;
                    continue;
                }

                RoomResStorage sss = storage.get(ss.x() + 1, ss.y(), WorkHands.ins);
                if (sss == null)
                    sss = storage.get(x1, ss.y() + 1, WorkHands.ins);
                ss = sss;
            }
            print.productionData.outs().get(0).inc(WorkHands.ins, -am);
            WorkHands.ins.hasStorage = false;
        }
    }
}