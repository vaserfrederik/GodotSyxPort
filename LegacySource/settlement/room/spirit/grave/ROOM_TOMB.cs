using System;
using System.Collections.Generic;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using view.sett.ui.room;

namespace settlement.room.spirit.grave
{
    public sealed class ROOM_TOMB : RoomBlueprintIns<GraveInstance>, GraveData.GRAVE_DATA_HOLDER
    {
        private readonly GraveData data;
        private readonly CTomb constructor;
        private readonly SFinderRoomService finder;

        public ROOM_TOMB(int typeIndex, string key, RoomInitData init, RoomCategorySub block, SFinderRoomService finder) : base(typeIndex, init, key, block)
        {
            data = new GraveData(this, init, 40)
            {
                Respect = grave => constructor.respekk[grave]
            };

            constructor = new CTomb(this, init);

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

        public void AppendView(LISTE<UIRoomModule> mm)
        {
            // mm.Add(new Gui(this).Make());
        }

        public GraveData GraveData()
        {
            return data;
        }
    }
}