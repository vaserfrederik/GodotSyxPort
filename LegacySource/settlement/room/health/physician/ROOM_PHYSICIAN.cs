using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Health.Physician
{
    public sealed class RoomPhysician : RoomBlueprintIns<Instance>, IRoomServiceNeedHaser, IRoomEmployAuto
    {
        private readonly Constructor constructor;
        private readonly RoomServiceNeed data;
        private readonly Service s;

        public RoomPhysician(string key, int typeI, RoomInitData init, RoomCategorySub block) : base(typeI, init, key, block)
        {
            s = new Service(this);
            data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) => s.Get(tx, ty)
            };

            constructor = new Constructor(this, init);
        }

        protected override void Update(double ds)
        {
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
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

        public DIR GetLayDir(int sx, int sy)
        {
            return DIR.ORTHO.Get((SETT.ROOMS().fData.SpriteData.Get(sx, sy) & 0b11));
        }

        public RoomServiceNeed Service()
        {
            return data;
        }

        public bool AutoEmploy(Room r)
        {
            return ((Instance)r).Auto;
        }

        public void AutoEmploy(Room r, bool b)
        {
            ((Instance)r).Auto = b;
        }
    }
}