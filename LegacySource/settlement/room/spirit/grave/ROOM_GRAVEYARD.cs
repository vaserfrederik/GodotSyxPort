using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Spirit.Grave
{
    public class ROOM_GRAVEYARD : RoomBlueprintIns<GraveInstance>, GraveData.GRAVE_DATA_HOLDER
    {
        private readonly GraveData data;
        private readonly CGraveyard constructor;
        private readonly SFinderRoomService finder;

        public ROOM_GRAVEYARD(int typeIndex, string key, RoomInitData init, RoomCategorySub block, SFinderRoomService finder) : base(typeIndex, init, key, block)
        {
            data = new GraveData(this, init, 20)
            {
                Respect = grave => constructor.Respekk[grave]
            };

            constructor = new CGraveyard(this, init);
            this.finder = finder;
        }

        protected override void Update(double ds)
        {
            data.Update(ds);
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return finder;
        }

        protected override void SaveP(FilePutter file)
        {
            data.Save(file);
        }

        protected override void LoadP(FileGetter file)
        {
            data.Load(file);
        }

        protected override void ClearP()
        {
            data.Clear();
        }

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            //mm.Add(new Gui(this).Make());
        }

        public override GraveData GraveData()
        {
            return data;
        }

        public bool IsGraveHead(int tx, int ty)
        {
            return Is(tx, ty) && SETT.ROOMS().FData.TileData.Get(tx, ty) == Grave.DIG_MARK;
        }
    }
}