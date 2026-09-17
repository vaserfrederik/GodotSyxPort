using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Industry
{
    public class RoomRefiner : RoomBlueprintIns<RefinerInstance>, IIndustryHaser
    {
        public const string Type = "REFINER";
        private readonly Job job;
        private readonly IList<Industry> indus;
        private readonly Constructor constructor;

        public RoomRefiner(RoomInitData init, string key, int index, RoomCategorySub cat) : base(index, init, key, cat)
        {
            constructor = new Constructor(init, this);
            PushBo(init.Data(), Type, true);
            indus = Industry.CreateIndustries(this, init, new RoomBoost[] { constructor.Efficiency }, Bonus());
            job = new Job(this, init.Data().I("STORAGE", 8, 500));
            new RoomExperienceBonus(this, init.Data(), Bonus());
            Employment().CountInputSet();
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        protected override void Update(double ds)
        {
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            IndustryUtil.Save(saveFile, indus);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            IndustryUtil.Load(saveFile, indus);
        }

        protected override void ClearP()
        {
            IndustryUtil.Clear(indus);
        }

        public override bool MakesDudesDirty()
        {
            return true;
        }

        public override void AppendView(IList<UIRoomModule> mm)
        {
        }

        public IList<Industry> Industries()
        {
            return indus;
        }
    }
}