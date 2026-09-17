using System;
using Snake2D.Util.Bit;
using Snake2D.Util.DataTypes;
using Settlement.Main;
using Settlement.Entity.Humanoid;
using Settlement.Room.Main.Job;
using Settlement.Room.Industry.Mine;

namespace Settlement.Room.Industry.Mine
{
    class Job
    {
        static readonly Bit isWork = new Bit(0b01);
        private static readonly Bit reserved = new Bit(0b010);
        private static readonly Bit used = new Bit(0b0100);

        private readonly ROOM_MINE print;
        private readonly Work WorkHands = new Work(false);
        private readonly Work WorkTools = new Work(true);
        public readonly RoomResStorage storage;

        public Job(ROOM_MINE print, int stor)
        {
            this.print = print;
            storage = new RoomResStorage(stor)
            {
                Resource = () => print.minable.resource,
                Is = (tx, ty) => SETT.ROOMS().fData.tileData.Get(tx, ty) == Constructor.B_STORAGE,
                Changed = (tx, ty) =>
                {
                    if (HasRoom())
                    {
                        MineInstance m = print.Get(tx, ty);
                        m.hasStorage = true;
                    }
                }
            };
        }

        public SETT_JOB Init(int tx, int ty, MineInstance ins)
        {
            if (!ins.Is(tx, ty))
                return null;
            int d = SETT.ROOMS().data.Get(tx, ty);
            if (isWork.Is(d) && SETT.MINERALS().getter.Is(tx, ty, print.minable))
            {
                return WorkTools.Init(tx, ty, ins);
            }
            else if (SETT.ROOMS().fData.tileData.Get(tx, ty) == Constructor.B_WORK)
            {
                return WorkHands.Init(tx, ty, ins);
            }
            return null;
        }

        public static bool Working(int data)
        {
            return used.Is(data);
        }

        private final class Work : SETT_JOB
        {
            private readonly bool tools;
            private readonly Coo coo = new Coo();
            public MineInstance ins;
            public int data;
            public static readonly string name = "working";
            private readonly double wv = 45;

            public Work(bool tools)
            {
                this.tools = tools;
            }

            public override bool JobReserveCanBe()
            {
                if (JobReservedIs(null))
                    return false;
                if (!ins.hasStorage)
                    return false;
                return true;
            }

            public Work Init(int tx, int ty, MineInstance ins)
            {
                data = SETT.ROOMS().data.Get(tx, ty);
                coo.Set(tx, ty);
                this.ins = ins;
                return this;
            }

            public void Save()
            {
                int d = SETT.ROOMS().data.Get(coo);
                if (used.Is(d))
                {
                    ins.workage--;
                }
                SETT.ROOMS().data.Set(ins, coo, data);
                if (used.Is(data))
                {
                    ins.workage++;
                }
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override string JobName()
            {
                return name;
            }

            public override bool JobUseTool()
            {
                return tools;
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return 45;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (JobReservedIs(null))
                    throw new Exception();
                data = reserved.Set(data);
                Save();
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return reserved.Is(data);
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                data = reserved.Clear(data);
                data = used.Clear(data);
                Save();
            }

            public override void JobStartPerforming()
            {
                data = used.Set(data);
                Save();
            }

            public override SoundRace JobSound()
            {
                return ins.blueprintI().employment().sound();
            }

            public override RESOURCE JobPerform(Humanoid s, RESOURCE res, int ram)
            {
                JobReserveCancel(null);
                if (!ins.hasStorage)
                    return null;
                int am = print.productionData.outs()[0].Work(s, ins, wv);

                if (am == 0)
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

                    RoomResStorage sss = storage.Get(ss.X() + 1, ss.Y(), ins);
                    if (sss == null)
                        sss = storage.Get(x1, ss.Y() + 1, ins);
                    ss = sss;
                }
                print.productionData.outs()[0].Inc(ins, -am);
                ins.hasStorage = false;

                return null;
            }
        }
    }
}