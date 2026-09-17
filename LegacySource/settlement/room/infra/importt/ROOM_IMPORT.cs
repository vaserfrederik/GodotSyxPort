using System;
using System.Collections.Generic;
using System.IO;
using Init.Resources;
using Settlement.Path.Finders;
using Settlement.Room.Main;
using Settlement.Room.Main.Category;
using Settlement.Room.Main.Furnisher;
using Settlement.Room.Main.Util;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using View.Sett.UI.Room;

namespace Settlement.Room.Infra.Importt
{
    public sealed class ROOM_IMPORT : RoomBlueprintIns<ImportInstance>
    {
        public readonly ImportTally tally = new ImportTally();
        private readonly Crate crate = new Crate(this);

        private readonly Constructor constructor;

        public ROOM_IMPORT(RoomInitData init, RoomCategorySub cat) : base(0, init, "_IMPORT", cat)
        {
            constructor = new Constructor(this, init);
        }

        protected override void Update(double ds)
        {
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            tally.saver.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            tally.saver.Load(saveFile);
            for (int i = 0; i < InstancesSize(); i++)
            {
                tally.Count(GetInstance(i).Resource(), GetInstance(i).Amount(), GetInstance(i).Capacity());
            }
        }

        protected override void ClearP()
        {
            tally.saver.Clear();
        }

        public readonly ImportThingy UNLOADER = new ImportThingy(this, tally);

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public int GetBestPrice(RESOURCE res)
        {
            return 0;
        }
    }
}