using System;
using System.Collections.Generic;
using System.IO;
using init.type;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.water;
using settlement.weather;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett.ui.room;
using world.map.regions;

namespace settlement.room.industry.woodcutter
{
    public class ROOM_WOODCUTTER : RoomBlueprintIns<Instance>, INDUSTRY_HASER, ROOM_IRRIGATED
    {
        private readonly Job job;
        private readonly Industry productionData;
        private readonly Constructor constructor;
        private readonly LIST<Industry> indus;
        private readonly RoomIrrigated irrigation;

        public ROOM_WOODCUTTER(RoomInitData init, RoomCategorySub cat) : base(0, init, "_WOODCUTTER", cat)
        {
            constructor = new Constructor(init, this);
            pushBo(init.data(), null, true);

            productionData = new Industry(this, init.data(), bonus());
            productionData.roomBoosts.add(constructor.efficiency);

            productionData.roomBoosts.add(WeatherMoisture.makeBoost());
            new IndustryRegion(productionData, 1.0)
            {
                public override double occurence(Region reg)
                {
                    return reg.info.terrain(TERRAINS.FOREST());
                }
            };

            job = new Job(this, init.data().i("STORAGE", 8, 500));
            indus = new ArrayList<Industry>(productionData);

            new RoomExperienceBonus(this, init.data(), bonus());

            irrigation = new RoomIrrigated(this, bonus, 0.75, 1.05)
            {
                public override double needed(AREA area)
                {
                    return area.area();
                }

                protected override double irrigation(RoomInstance ins)
                {
                    return ((Instance)ins).irri;
                }
            };
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        protected override void update(double ds)
        {
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
            productionData.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile) throws IOException
        {
            productionData.load(saveFile);
        }

        protected override void clearP()
        {
            productionData.clear();
        }

        public override bool makesDudesDirty()
        {
            return true;
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
        }

        public override LIST<Industry> industries()
        {
            return indus;
        }

        public override RoomIrrigated irrigation()
        {
            return irrigation;
        }
    }
}