using System;
using System.Collections.Generic;
using System.IO;
using game;
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

namespace settlement.room.military.supply
{
    public sealed class ROOM_SUPPLY : RoomBlueprintIns<SupplyInstance>, ROOM_RADIUS, ROOM_EMPLOY_AUTO
    {
        public const int STORAGE = 80;
        private readonly Constructor constructor;
        private readonly Crate crate;
        private readonly Cache cache;
        public readonly SupplyTally tally;
        private readonly RESOURCE liveStock;

        public ROOM_SUPPLY(RoomInitData init, RoomCategorySub cat) : base(0, init, "_MILITARY_SUPPLY", cat)
        {
            constructor = new Constructor(this, init);
            liveStock = RESOURCES.map().read("LIVESTOCK", init.data());
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
            cache.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            cache.clear();
            tally.clear();
            foreach (var ins in all())
            {
                tally.init(ins);
            }
            cache.load(saveFile);
        }

        protected override void clearP()
        {
            cache.clear();
            tally.clear();
            cache.clear();
        }

        public override bool degrades()
        {
            return false;
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public override bool autoEmploy(Room r)
        {
            return ((SupplyInstance)r).auto;
        }

        public override void autoEmploy(Room r, bool b)
        {
            ((SupplyInstance)r).auto = b;
        }

        private int upI = -1;
        private RBITImp hh = new RBITImp();

        public bool has(RESOURCE res)
        {
            if (upI == GAME.updateI())
            {
                return hh.has(res);
            }
            upI = GAME.updateI();
            for (int i = 0; i < instancesSize(); i++)
            {
                hh.or(getInstance(i).allowed());
            }
            return hh.has(res);
        }
    }
}