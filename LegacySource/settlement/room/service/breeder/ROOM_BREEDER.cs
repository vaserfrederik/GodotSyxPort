using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Service.Breeder
{
    public sealed class RoomBreeder : RoomBlueprintIns<BreederInstance>, IIndustryHaser, IRoomEmployAuto
    {
        public readonly string type = "NURSERY";
        private readonly BreederConstructor constructor;
        private readonly Industry productionData;
        public readonly double PRODUCTION_SPEED_DAY;
        public readonly Race race;
        public int limitTotal = ENTETIES.MAX;
        public int limitSpecies = ENTETIES.MAX;
        private readonly List<Industry> indus;

        private readonly Station station = new Station(this);
        public bool prosecute = false;

        public RoomBreeder(int index, RoomInitData init, RoomCategorySub block, string key) : base(index, init, key, block)
        {
            constructor = new BreederConstructor(this, init);
            PushBo(init.data(), type, true);
            productionData = new Industry(this, init.data(), Bonus());

            productionData.roomBoosts.Add(constructor.coziness);

            if (productionData.ins().Count == 0)
            {
                init.data().Error("Nurseries must have an in-resource (food)", "INDUSTRY");
            }
            PRODUCTION_SPEED_DAY = 1.0 / init.data().i("INCUBATION_DAYS", 0, byte.MaxValue);
            race = RACES.Map().Read("RACE", init.data());

            indus = new List<Industry>(productionData);
            Employment().CountInputSet();
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
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
            productionData.Save(saveFile);
            saveFile.i(limitTotal);
            saveFile.i(limitSpecies);
            saveFile.Bool(prosecute);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            productionData.Load(saveFile);
            limitTotal = saveFile.i();
            limitSpecies = saveFile.i();
            if (!VERSION.VersionIsBefore(71, 15))
                prosecute = saveFile.Bool();
        }

        protected override void ClearP()
        {
            productionData.Clear();
            limitTotal = ENTETIES.MAX;
            limitSpecies = ENTETIES.MAX;
            prosecute = false;
        }

        public bool CanWork()
        {
            return POP.Next(HCLASSES.CITIZEN(), race) < limitSpecies && POP.Next(null, null) < limitTotal;
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public List<Industry> Industries()
        {
            return indus;
        }

        public bool AutoEmploy(Room r)
        {
            BreederInstance i = (BreederInstance)r;
            return i.auto;
        }

        public void AutoEmploy(Room r, bool b)
        {
            BreederInstance i = (BreederInstance)r;
            i.auto = b;
        }
    }
}