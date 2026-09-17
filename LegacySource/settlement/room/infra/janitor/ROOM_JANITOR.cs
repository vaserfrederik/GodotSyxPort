using System;
using System.IO;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett.ui.room;

namespace settlement.room.infra.janitor
{
    public sealed class ROOM_JANITOR : RoomBlueprintIns<JanitorInstance>, ROOM_EMPLOY_AUTO
    {
        private readonly JM jm = new JM(this);
        public static readonly int radius = 150;
        private readonly Constructor constructor;

        public ROOM_JANITOR(RoomInitData init, RoomCategorySub block) : base(0, init, "_JANITOR", block)
        {
            constructor = new Constructor(this, init);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
        }

        protected override void loadP(FileGetter saveFile)
        {
        }

        protected override void clearP()
        {
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public bool autoEmploy(Room r)
        {
            return ((JanitorInstance)r).auto;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((JanitorInstance)r).auto = b;
        }
    }
}