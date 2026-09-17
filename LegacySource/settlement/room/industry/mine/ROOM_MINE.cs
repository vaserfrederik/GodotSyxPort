using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.sett.ui.room;
using world.map.regions;

namespace settlement.room.industry.mine
{
    public sealed class ROOM_MINE : RoomBlueprintIns<MineInstance>, INDUSTRY_HASER
    {
        public const string Type = "MINE";
        private int rawNeeded = 2;
        private readonly Job job;

        private readonly Industry productionData;
        private readonly Constructor constructor;
        public readonly Minable minable;

        private readonly LIST<Industry> indus;

        public ROOM_MINE(RoomInitData init, string key, int index, RoomCategorySub cat) : base(index, init, key, cat)
        {
            minable = RESOURCES.minables().read(init.data());
            constructor = new Constructor(init, this);
            Boostable skill = pushBo(init.data(), Type, true);
            D.t(this);

            productionData = new Industry(this, minable.resource, init.data().d("YEILD_WORKER_DAILY", 0, 1000), skill);
            productionData.roomBoosts.add(constructor.efficiency);
            productionData.roomBoosts.add(constructor.deposits);

            new IndustryRegion(productionData, minable.occurence)
            {
                public override double occurence(Region reg)
                {
                    double d = 0;
                    for (int ti = 0; ti < TERRAINS.ALL().size(); ti++)
                    {
                        d += minable.terrain(TERRAINS.ALL().get(ti)) * reg.info.terrain(TERRAINS.ALL().getC(index));
                    }
                    return d;
                }
            };

            job = new Job(this, init.data().i("STORAGE", 4, 500));
            indus = new ArrayList<Industry>(productionData);

            new RoomExperienceBonus(this, init.data(), skill);
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
            mm.add(constructor.deposits.applier(this));
            mm.add(constructor.efficiency.applier(this));

            mm.add(new UIRoomModule()
            {
                public override void hover(GBox box, Room i, int rx, int ry)
                {
                    box.NL();
                    box.add(box.text().add(((MineInstance)i).workage));
                }
            });
        }

        public override LIST<Industry> industries()
        {
            return indus;
        }
    }
}