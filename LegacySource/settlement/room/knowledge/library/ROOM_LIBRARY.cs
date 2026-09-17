using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Knowledge.Library
{
    using static Settlement.Main.SETT.ROOMS;

    using Settlement.Main;
    using Settlement.Path.Finders;
    using Settlement.Room.Industry.Module.Consumption;
    using Settlement.Room.Infra.Admin;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Category;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Snake2D.Util.Datatypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Util.Data;
    using View.Sett.Ui.Room;

    public sealed class ROOM_LIBRARY : RoomBlueprintIns<LibraryInstance>, ROOM_ADMIN_HOLDER, ROOM_CONSUMPTION_HASER
    {
        public static readonly string Type = "LIBRARY";
        public readonly AdminData Data;
        private readonly RoomConsumption Consumption;
        public readonly ConsumptionJob Job;
        public readonly Constructor Constructor;
        private readonly BOOLEANCoo IsJob;

        public ROOM_LIBRARY(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            pushBo(init.data(), Type, true);
            Consumption = new RoomConsumption(this, init.data(), bonus);

            Data = new AdminData(employmentExtra(), init.data(), bonus());
            Job = new ConsumptionJob(this, Consumption, 45, isJob)
            {
                protected override void Perform(double time, double skill)
                {
                    Data.Perform(time, skill);
                }

                public override DIR JobStandDir()
                {
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (SETT.ROOMS().fData.sprite.Get(coo, d) == Constructor.SStool)
                            return d;
                    }
                    return null;
                }

                public override bool JobUseTool()
                {
                    return false;
                }

                public override bool JobUseHands()
                {
                    return false;
                }
            };

            Constructor = new Constructor(this, init);
            Consumption.RoomBoosts.Add(Constructor.Efficiency);
            new RoomExperienceBonus(this, init.data(), bonus);
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
            mm.Add(new ConsumptionGui<LibraryInstance, ROOM_LIBRARY>(this, Consumption).Make());
            mm.Add(new AdminData.Gui<LibraryInstance, ROOM_LIBRARY>(this, Data, Consumption).Make());
        }

        public override AdminData Admin()
        {
            return Data;
        }

        public override RoomConsumption Consumption()
        {
            return Consumption;
        }
    }
}