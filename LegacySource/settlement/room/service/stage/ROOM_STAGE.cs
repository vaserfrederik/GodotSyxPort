using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Stage
{
    public class RoomStage : RoomBlueprintIns<StageInstance>, IRoomServiceNeedHaser, IRoomSpectatorHaser
    {
        private readonly RoomServiceNeed data;
        private readonly StageConstructor constructor;
        private readonly Centre work;

        public RoomStage(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            work = new Centre(this);
            data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) => work.Service(tx, ty)
            };
            constructor = new StageConstructor(this, init);
            Employment().SetShiftStart(RoomSpectator.WorkStarts, false);
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return data.Finder;
        }

        public SFinderRoomService Finder()
        {
            return data.Finder;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            data.Saver.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            data.Saver.Load(saveFile);
        }

        protected override void ClearP()
        {
            data.Saver.Clear();
        }

        public RoomServiceNeed Service()
        {
            return data;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
        }

        private readonly RoomSpectator spec = new RoomSpectator()
        {
            Coo = new Coo(),
            Acts = Alloc.Bb(64),

            Spec = () => RoomStage.this.Service,
            LookAt = (sx, sy) =>
            {
                Coo.Set(sx * C.TileSize + C.TileSizeH, sy * C.TileSize + C.TileSizeH);
                return Coo;
            },
            Is = (sx, sy) =>
            {
                StageInstance ins = Getter.Get(sx, sy);
                return ins != null;
            },
            Activity = (sx, sy) =>
            {
                StageInstance ins = Getter.Get(sx, sy);
                if (ins == null)
                    return 0;
                int s = ins.Off;

                s += (int)(Acts.Length * TIME.CurrentSecond() / TIME.SecondsPerDay());
                s %= Acts.Length;
                return Acts[s];
            },
            ShouldCheer = (sx, sy) => Activity(sx, sy) == 1,
            ShouldBoo = (sx, sy) => Activity(sx, sy) == 2,
            IsActive = (sx, sy) => true
        };

        public RoomSpectator Spec()
        {
            return spec;
        }
    }
}