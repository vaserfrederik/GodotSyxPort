using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Law.Court
{
    public sealed class RoomCourt : RoomBlueprintIns<CourtInstance>, ROOM_SERVICE_NEED_HASER, PUNISHMENT_SERVICE
    {
        public static readonly double FreeRate = 0.2;

        private readonly RoomServiceNeed data;
        private readonly Constructor constructor;
        private int executions;
        private int total;

        public RoomCourt(RoomInitData init, RoomCategorySub block) : base(0, init, "_COURT", block)
        {
            data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) => Service.Init(tx, ty)
            };

            constructor = new Constructor(this, init);
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public int PunishUsed()
        {
            return executions;
        }

        public int PunishTotal()
        {
            return total;
        }

        private void IncPrisoners(int current, int total)
        {
            this.executions += current;
            this.total += total;
        }

        protected override void SaveP(FilePutter f)
        {
            f.Write(executions);
            f.Write(total);
        }

        protected override void LoadP(FileGetter f)
        {
            executions = f.ReadInt();
            total = f.ReadInt();
        }

        protected override void ClearP()
        {
            executions = 0;
            total = 0;
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return data.Finder;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
        }

        public CourtStation ExecutionReserve()
        {
            if (executions == total)
                return null;
            int i = RND.RInt(instancesSize());
            for (int k = 0; k < instancesSize(); k++)
            {
                CourtInstance ins = GetInstance((k + i) % instancesSize());
                if (ins.Active() && ins.Executions() < ins.Total())
                {
                    return ins.ReserveSpot();
                }
            }
            throw new RuntimeException();
        }

        public CourtStation ExecutionSpot(COORDINATE c)
        {
            if (Is(c))
            {
                return CourtStation.Init(c.X, c.Y);
            }
            return null;
        }

        public CourtStation WorkReserve(Room r)
        {
            CourtInstance ins = (CourtInstance)r;
            return ins.Work();
        }

        public bool ShouldCheer(int tx, int ty)
        {
            CourtInstance ins = Getter.Get(tx, ty);
            return ins != null && ins.Executions() > 0;
        }

        public RoomServiceNeed Service()
        {
            return data;
        }
    }
}