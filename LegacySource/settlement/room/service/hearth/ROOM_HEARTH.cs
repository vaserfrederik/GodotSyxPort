using System;
using System.IO;

namespace Settlement.Room.Service.Hearth
{
    public sealed class RoomHearth : RoomBlueprintIns<HearthInstance>, IRoomServiceNeedHaser
    {
        private readonly RoomServiceNeed data;
        private readonly Constructor constructor;
        private readonly Hearth bed;

        public RoomHearth(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            bed = new Hearth(this);
            data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) => bed.Get(tx, ty)
            };
            constructor = new Constructor(this, init);
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public Hearth Bed(int tx, int ty)
        {
            return bed.Get(tx, ty);
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        public SFinderRoomService Service(int tx, int ty)
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
    }
}