using System;
using System.Collections.Generic;
using System.IO;

using init.constant;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using view.sett.ui.room;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.room.home.chamber
{
    public sealed class ROOM_CHAMBER : RoomBlueprintIns<ChamberInstance>
    {
        private readonly Constructor constructor;
        private readonly Work work;

        public ROOM_CHAMBER(RoomInitData init, RoomCategorySub block) : base(0, init, "_HOME_CHAMBER", block)
        {
            work = new Work(this);
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

        protected override void saveP(FilePutter saveFile)
        {
        }

        protected override void loadP(FileGetter saveFile)
        {
            throw new NotImplementedException();
        }

        protected override void clearP()
        {
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
        }

        public int getSleepPixelX(int tx, int ty)
        {
            DIR d = DIR.ALL.get(get(tx, ty).sleepDir).next(-1);
            int x = tx * C.TILE_SIZE + C.TILE_SIZEH;
            x += d.x() * C.TILE_SIZEH;
            return x;
        }

        public int getSleepPixelY(int tx, int ty)
        {
            DIR d = DIR.ALL.get(get(tx, ty).sleepDir).next(-1);
            int y = ty * C.TILE_SIZE + C.TILE_SIZEH;
            y += d.y() * C.TILE_SIZEH;
            return y;
        }

        public DIR getSleepDir(int tx, int ty)
        {
            DIR d = DIR.ALL.get(get(tx, ty).sleepDir).perpendicular();
            return d;
        }

        public override SFinderFindable service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public override bool degrades()
        {
            return false;
        }
    }
}