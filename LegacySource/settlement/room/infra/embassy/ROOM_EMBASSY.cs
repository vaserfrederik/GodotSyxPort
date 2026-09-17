using System;
using System.Collections.Generic;
using System.IO;
using game.boosting;
using game.faction.player;
using init.type;
using settlement.path.finders;
using settlement.room.industry.module.consumption;
using settlement.room.infra.admin;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.text;
using view.sett.ui.room;

namespace settlement.room.infra.embassy
{
    public sealed class ROOM_EMBASSY : RoomBlueprintIns<EmbassyInstance>, ROOM_ADMIN_HOLDER, ROOM_CONSUMPTION_HASER
    {
        private readonly ConsumptionJob job;
        private readonly Constructor constructor;
        private readonly RoomConsumption consumption;
        private readonly AdminData data;

        private readonly BOOLEANCoo isJob = new BOOLEANCoo
        {
            public bool is(int tx, int ty)
            {
                return ROOMS().fData.tileData.get(tx, ty) == Constructor.IWORK;
            }
        };

        public ROOM_EMBASSY(RoomInitData init, RoomCategorySub block) : base(0, init, "_EMBASSY", block)
        {
            pushBo(init.data(), type, true);
            constructor = new Constructor(this, init);

            consumption = new RoomConsumption(this, init.data(), bonus);
            consumption.roomBoosts.add(constructor.efficiency);

            job = new ConsumptionJob(this, consumption, 45, isJob)
            {
                protected override void perform(double time, double skill)
                {
                    data.perform(time, skill);
                }

                public override bool jobUseTool()
                {
                    return false;
                }

                public override bool jobUseHands()
                {
                    return RND.rBoolean();
                }
            };

            data = new AdminData(employmentExtra(), init.data(), bonus());

            const double max = 10000000;
            const double maxI = 1.0 / max;
            new BoosterImp(new BSourceInfo(info.names, iconBig().small), 0, 10000000, false)
            {
                public double vGet(Player f)
                {
                    return data.value() * maxI;
                }

                public double vGet(HCLASS_RACE reg)
                {
                    return data.value() * maxI;
                }
            }.add(BOOSTABLES.CIVICS().DIPLOMACY);

            employment().countInputSet();
        }

        protected override void update(double ds)
        {
            data.update();
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
            data.save(saveFile);
            consumption.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            data.load(saveFile);
            consumption.load(saveFile);
        }

        protected override void clearP()
        {
            data.clear();
            consumption.clear();
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        private static readonly string ¤¤target = "¤The estimated amount of diplomacy points that will be produced.";

        static
        {
            D.ts(typeof(ROOM_EMBASSY));
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new ConsumptionGui<EmbassyInstance, ROOM_EMBASSY>(this, consumption).make());
            mm.add(new AdminData.Gui<EmbassyInstance, ROOM_EMBASSY>(this, data, consumption, BOOSTABLES.CIVICS().DIPLOMACY.name, ¤¤target).make());
        }

        public RoomConsumption consumption()
        {
            return consumption;
        }

        public AdminData admin()
        {
            return data;
        }
    }
}