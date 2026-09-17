using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using view.sett.ui.room;

namespace settlement.room.knowledge.laboratory
{
    public sealed class ROOM_LABORATORY : RoomBlueprintIns<LaboratoryInstance>, ROOM_ADMIN_HOLDER, ROOM_CONSUMPTION_HASER
    {
        public static readonly string Type = "LABORATORY";
        public readonly AdminData Data;
        private readonly RoomConsumption Consumption;
        public readonly ConsumptionJob Job;
        public readonly Constructor Constructor;
        public readonly BOOLEANCoo IsJob = new BOOLEANCoo
        {
            Is = (tx, ty) => SETT.ROOMS().FData.TileData.Get(tx, ty) == Constructor.WORK
        };

        public ROOM_LABORATORY(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            PushBo(init.Data(), Type, true);
            Consumption = new RoomConsumption(this, init.Data(), Bonus);

            Data = new AdminData(EmploymentExtra(), init.Data(), Bonus());
            Job = new ConsumptionJob(this, Consumption, 45, IsJob)
            {
                Perform = (time, skill) =>
                {
                    Data.Perform(time, skill);
                },

                JobStandDir = () =>
                {
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        if (Ins.Is(Coo, DIR.ORTHO.Get(di)) && SETT.ROOMS().FData.Sprite.Is(Coo, DIR.ORTHO.Get(di), Constructor.SChair))
                            return DIR.ORTHO.Get(di);
                    }
                    return null;
                },

                JobUseTool = () => false,
                JobUseHands = () => false
            };

            Constructor = new Constructor(this, init);

            new RoomExperienceBonus(this, init.Data(), Bonus);
            Employment().CountInputSet();
        }

        protected override void Update(double ds)
        {
            Data.Update();
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            Data.Save(saveFile);
            Consumption.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            Data.Load(saveFile);
            Consumption.Load(saveFile);
        }

        protected override void ClearP()
        {
            Data.Clear();
            Consumption.Clear();
        }

        public override Furnisher Constructor()
        {
            return Constructor;
        }

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new ConsumptionGui<LaboratoryInstance, ROOM_LABORATORY>(this, Consumption).Make());
            mm.Add(new AdminData.Gui<LaboratoryInstance, ROOM_LABORATORY>(this, Data, Consumption).Make());
        }

        public AdminData Admin()
        {
            return Data;
        }

        public RoomConsumption Consumption()
        {
            return Consumption;
        }
    }
}