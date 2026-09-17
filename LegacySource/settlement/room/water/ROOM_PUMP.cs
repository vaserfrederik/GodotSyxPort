using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Water
{
    public sealed class RoomPump : RoomBlueprintIns<PumpInstance>
    {
        private readonly PumpJob _job;
        private readonly PumpConstructor _constructor;

        public RoomPump(RoomInitData init, RoomCategorySub cat) : base(0, init, "_WATERPUMP", cat)
        {
            _constructor = new PumpConstructor(this, init);
            _job = new PumpJob(this);
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new PumpGui(this).Make());
        }

        protected override void Update(double ds)
        {
            SETT.Rooms.Water.Updater.Update(ds);
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            SETT.Rooms.Water.Updater.Saver.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            SETT.Rooms.Water.Updater.Saver.Load(saveFile);
        }

        protected override void ClearP()
        {
            SETT.Rooms.Water.Updater.Saver.Clear();
        }

        public override Furnisher Constructor()
        {
            return _constructor;
        }

        public bool IsCanalConnection(int tx, int ty)
        {
            return Is(tx, ty) && SETT.Rooms.FData.Tile.Get(tx, ty) == _constructor.Ou;
        }

        //double GetOutput(int tx, int ty)
        //{
        //    return Is(tx, ty) && SETT.Rooms.FData.Tile.Get(tx, ty) == _constructor.Ou ? 1 : 0;
        //}
    }
}