using System;
using System.Collections.Generic;
using System.IO;
using init.resources;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett.ui.room;

namespace settlement.room.infra.transport
{
    public sealed class ROOM_TRANSPORT : RoomBlueprintIns<TransportInstance>, ROOM_RADIUSE, ROOM_EMPLOY_AUTO
    {
        public const int MAX_LOAD = 400;
        public const int MAX_EMPLOYEES = 16;
        private readonly Constructor constructor;
        private readonly Job job;

        public ROOM_TRANSPORT(RoomInitData init, RoomCategorySub cat) : base(0, init, "_TRANSPORT", cat)
        {
            constructor = new Constructor(this, init);
            job = new Job(this);
        }

        protected override void update(double ds)
        {
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
            return ((TransportInstance)r).auto;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((TransportInstance)r).auto = b;
        }

        public ROOM_RADIUS_INSTANCE radiusInstance(Room t)
        {
            return (TransportInstance)t;
        }

        public bool hasActive(RESOURCE res)
        {
            return true;
        }

        public void endDelivery(short startTx, short startTy, RESOURCE res, int amount, int distance)
        {
            TransportInstance ins = get(startTx, startTy);
            if (ins != null)
            {
                ins.finishDeliveryJob(amount);
                ins.reportMoved(distance);
            }
        }
    }
}