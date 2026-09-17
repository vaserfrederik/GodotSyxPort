using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Law.Execution
{
    public class ROOM_EXECTUTION : RoomBlueprintImp, PUNISHMENT_SERVICE, ROOM_ACTIVITY_HASER
    {
        private readonly SFinderRoomService data;
        private readonly Constructor constructor;

        public readonly ExecutionStation stations = new ExecutionStation(this);
        private readonly ExecutionSingle instance;

        public ROOM_EXECTUTION(RoomInitData init, RoomCategorySub block) : base(init, 0, "_EXECUTION", block)
        {
            instance = new ExecutionSingle(init.m, this);
            constructor = new Constructor(this, init);
            data = new SFinderRoomService("Execution")
            {
                Get = (tx, ty) => stations.service(tx, ty)
            };
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
            return stations.total() - stations.available();
        }

        public int PunishTotal()
        {
            return stations.total();
        }

        protected override void Save(FilePutter f)
        {
            stations.Save(f);
        }

        protected override void Load(FileGetter f)
        {
            stations.Load(f);
        }

        protected override void Clear()
        {
            stations.Clear();
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return data;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this));
        }

        private readonly ROOM_ACTIVITY spec = new ROOM_ACTIVITY
        {
            Finder = () => ROOM_EXECTUTION.this.data,
            LookAt = (sx, sy) =>
            {
                var coo = new Coo(sx * C.TILE_SIZE + C.TILE_SIZEH, sy * C.TILE_SIZE + C.TILE_SIZEH);
                return coo;
            },
            Is = (sx, sy) => ROOM_EXECTUTION.this.is(sx, sy),
            ShouldCheer = (sx, sy) =>
            {
                var s = stations.client(sx, sy);
                return s != null && s.clientBeingExecuted();
            },
            ShouldBoo = (sx, sy) =>
            {
                var s = stations.guard(sx, sy);
                return s != null && s.shouldExecute();
            },
            IsActive = (sx, sy) =>
            {
                var s = stations.client(sx, sy);
                return s != null && s.clientPresent();
            }
        };

        public ROOM_ACTIVITY Spec()
        {
            return spec;
        }
    }
}