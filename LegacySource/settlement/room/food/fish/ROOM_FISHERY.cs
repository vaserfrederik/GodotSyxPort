using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Food.Fish
{
    class ROOM_FISHERY : RoomBlueprintIns<FishInstance>, INDUSTRY_HASER
    {
        public const string type = "FISHERY";
        private readonly Job job;
        private readonly Industry productionData;
        private readonly Constructor constructor;
        private readonly List<Industry> indus;

        public ROOM_FISHERY(RoomInitData init, string key, int index, RoomCategorySub cat) : base(index, init, key, cat)
        {
            constructor = new Constructor(init, this);
            pushBo(init.data(), type, true);

            productionData = new Industry(
                this, init.data(),
                bonus());
            productionData.roomBoosts.Add(constructor.efficiency);
            new IndustryRegion(productionData, 1.0)
            {
                public override double Occurence(Region reg)
                {
                    return Math.Max(RegionInfo.vTerrain(TERRAINS.OCEAN()).GetAi(reg), RegionInfo.vTerrain(TERRAINS.WET()).GetAi(reg));
                }
            };

            job = new Job(this);
            indus = new List<Industry>(productionData);
            new RoomExperienceBonus(this, init.data(), bonus());
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
            productionData.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            productionData.Load(saveFile);
        }

        protected override void ClearP()
        {
            productionData.Clear();
        }

        public override bool MakesDudesDirty()
        {
            return true;
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
        }

        public override List<Industry> Industries()
        {
            return indus;
        }

        public void PerformFishingTrip(Humanoid h, int tx, int ty, double time)
        {
            if (Is(tx, ty))
            {
                SETT_JOB s = job.Init(tx, ty, Get(tx, ty));
                if (s != null)
                    job.SecretPerform(h, time);
            }
        }

        public bool LaunchFishingExpedition(Humanoid h, int tx, int ty)
        {
            if (Is(tx, ty))
            {
                if (Job.isShip.Is(SETT.ROOMS().Data.Get(tx, ty)))
                {
                    job.Init(tx, ty, Get(tx, ty)).JobStartPerforming();
                    if (SETT.HALFENTS().Dingy.Make(h, tx, ty, Industries()[0].Outs()[0].Resource, Get(tx, ty).Upgrade(), DIR.ALL.Get(Job.shipDir.Get(SETT.ROOMS().Data.Get(tx, ty)))))
                        return true;
                }
            }
            return false;
        }
    }
}