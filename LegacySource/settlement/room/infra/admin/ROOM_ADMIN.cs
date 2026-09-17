using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Infra.Admin
{
    using static Settlement.Main.SETT;
    using Settlement.Main;
    using Settlement.Path.Finders;
    using Settlement.Room.Industry.Module.Consumption;
    using Settlement.Room.Infra.Admin;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Category;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Util.Data;
    using View.Sett.Ui.Room;

    public sealed class ROOM_ADMIN : RoomBlueprintIns<AdminInstance>, ROOM_CONSUMPTION_HASER, ROOM_ADMIN_HOLDER
    {
        public static readonly string type = "ADMIN";
        public readonly AdminData data;
        private readonly ConsumptionJob job;
        private readonly RoomConsumption consumption;
        private readonly Constructor constructor;
        private readonly BOOLEANCoo isJob = new BOOLEANCoo
        {
            Is = (tx, ty) => ROOMS().fData.tile.Is(tx, ty, constructor.ww)
        };

        public ROOM_ADMIN(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            constructor = new Constructor(this, init);
            PushBo(init.data(), type, true);
            consumption = new RoomConsumption(this, init.data(), Bonus());
            data = new AdminData(EmploymentExtra(), init.data(), Bonus());

            consumption.roomBoosts.Add(constructor.efficiency);

            job = new ConsumptionJob(this, consumption, 45, isJob)
            {
                Perform = (time, skill) => data.Perform(time, skill),
                JobStandDir = () =>
                {
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (Ins.Is(Coo, d) && SETT.ROOMS().fData.tileData.Get(Coo, d) == Constructor.ICHAIR)
                            return d;
                    }
                    return null;
                },
                JobUseTool = () => false,
                JobUseHands = () => false
            };

            new RoomExperienceBonus(this, init.data(), Bonus());
            Employment().CountInputSet();
        }

        protected override void SaveP(FilePutter f)
        {
            data.Save(f);
            consumption.Save(f);
        }

        protected override void LoadP(FileGetter f)
        {
            data.Load(f);
            consumption.Load(f);
        }

        protected override void ClearP()
        {
            this.data.Clear();
            consumption.Clear();
        }

        protected override void Update(double ds)
        {
            data.Update();
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        public void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new ConsumptionGui<AdminInstance, ROOM_ADMIN>(this, consumption).Make());
            mm.Add(new AdminData.Gui<AdminInstance, ROOM_ADMIN>(this, data, consumption).Make());
        }

        public RoomConsumption Consumption()
        {
            return consumption;
        }

        public AdminData Admin()
        {
            return data;
        }
    }
}