using System;
using System.Collections.Generic;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.industry.workshop;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett.ui.room;

namespace settlement.room.industry.workshop
{
    public class ROOM_WORKSHOP : RoomBlueprintIns<WorkshopInstance>, INDUSTRY_HASER
    {
        public static readonly string type = "WORKSHOP";
        private readonly Job job;
        private readonly Constructor constructor;
        private readonly LIST<Industry> indus;

        public ROOM_WORKSHOP(int index, RoomInitData init, string key, RoomCategorySub cat) : base(index, init, key, cat)
        {
            constructor = new Constructor(this, init);
            pushBo(init.data(), type, true);

            job = new Job(this);

            indus = Industry.CreateIndustries(this, init, new RoomBoost[] { constructor.efficiency }, bonus());
            new RoomExperienceBonus(this, init.data(), bonus());
            employment().countInputSet();
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public SFinderRoomService service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
            IndustryUtil.Save(saveFile, indus);
        }

        protected override void loadP(FileGetter saveFile)
        {
            IndustryUtil.Load(saveFile, indus);
        }

        protected override void clearP()
        {
            IndustryUtil.Clear(indus);
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public LIST<Industry> industries()
        {
            return indus;
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
        }
    }
}