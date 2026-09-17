using System;
using System.IO;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using settlement.thing.ThingsCorpses;
using snake2d.util.file;
using util.text;

namespace settlement.room.spirit.dump
{
    public class ROOM_DUMP : RoomBlueprintIns<DumpInstance>, ROOM_SERVICE_HASER
    {
        private Constructor constructor;
        private RoomService service;
        public static string ¤¤RemoveProblem = "¤This resting place still holds the dead and can not be removed. Deactivate the room and allow the corpses to decompose peacefully. Current cadavers: {0}. Days until clear: {1}.";

        static
        {
            D.ts(typeof(ROOM_DUMP));
        }

        public ROOM_DUMP(RoomInitData data, RoomCategorySub cat) : base(0, data, "_DUMP_CORPSE", cat)
        {
            constructor = new Constructor(this, data);
            service = new RoomService(this, data, null)
            {
                public FSERVICE service(int tx, int ty)
                {
                    return Dump.get(tx, ty);
                }
            };
        }

        protected override void saveP(FilePutter saveFile)
        {
            service.saver.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            service.saver.load(saveFile);
        }

        protected override void clearP()
        {
            service.saver.clear();
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return service.finder;
        }

        public RoomService service()
        {
            return service;
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public void burry(Corpse corpse, int tx, int ty)
        {
            Dump.get(tx, ty).burry(corpse);
        }
    }
}