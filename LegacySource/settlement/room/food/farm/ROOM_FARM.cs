using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Food.Farm
{
    using Game.Time;
    using Init.Resources;
    using Init.Type;
    using Settlement.Misc.Util;
    using Settlement.Path.Finders;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Category;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Settlement.Room.Water;
    using Settlement.Weather;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using View.Sett.Ui.Room;
    using World.Map.Regions;

    public class RoomFarm : RoomBlueprintIns<FarmInstance>, IndustryHaser, RoomIrrigated
    {
        static double WORKERPERTILE = TIME.workSeconds() / (Tile.WORK_TIME + TIME.workSecondsWalkNext());
        static double WORKERPERTILEI = 1.0 / WORKERPERTILE;

        public readonly Growable crop;
        public readonly static string type = "FARM";

        readonly Constructor constructor;

        readonly Industry productionData;

        readonly List<Industry> indus;

        readonly Tile tile;
        readonly Time time;

        readonly double yearMul = TIME.years().bitSeconds() / (16 * TIME.secondsPerDay());
        private readonly RoomIrrigated irri;

        public RoomFarm(RoomInitData data, string key, RoomCategorySub cat, int index) : base(index, data, key, cat)
        {
            crop = RESOURCES.growable().MAP.read(data.data());

            constructor = new Constructor(this, data);
            pushBo(data.data(), type, true);

            RoomBoost mo = WeatherMoisture.makeBoost();

            productionData = new Industry(this, data.data(), bonus());
            productionData.roomBoosts.Add(constructor.fertility);
            productionData.roomBoosts.Add(mo);

            new IndustryRegion(productionData, 1.0)
            {
                public override double occurence(Region reg)
                {
                    if (constructor().mustBeOutdoors())
                        return RegionInfo.vFer().getAi(reg);
                    return reg.info.terrain(TERRAINS.MOUNTAIN());
                }
            };
            indus = new ArrayList<Industry>(productionData);

            time = new Time(this);
            tile = new Tile(this);
            new RoomExperienceBonus(this, data.data(), bonus());

            double ibonus = 1;
            {
                double period = TIME.years().bitConversion(TIME.days()) - 1;
                double degrade = 1 - productionData.outs().get(0).resource.degradeSpeed() / (2 * period);
                double consumption = 1;

                double res = 0;

                for (int i = 0; i < period; i++)
                {
                    res += consumption;
                    res /= degrade;
                }

                ibonus = res / (period * consumption);
                ibonus = Math.Round(ibonus * 100) / 100.0;
                ibonus += 0.05;
            }

            irri = new RoomIrrigated(this, bonus(), 0.05, ibonus)
            {
                public override double needed(AREA area)
                {
                    return area.area();
                }

                protected override double irrigation(RoomInstance ins)
                {
                    return ((FarmInstance)ins).irri;
                }
            };

            degradeRate = 0;
        }

        Tile tile(int tx, int ty)
        {
            return tile.get(tx, ty);
        }

        protected override void update(double ds)
        {
        }

        public override Furnisher constructor()
        {
            return constructor;
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

        public override bool degrades()
        {
            return false;
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public override LIST<Industry> industries()
        {
            return indus;
        }

        public bool isGoodDayForEvent()
        {
            return time.dayI() == time.dayEvent;
        }

        public override bool industryIgnoreUI()
        {
            return true;
        }

        public bool shouldReportWorkFailure()
        {
            return (time.dayI() != time.dayOffWork && time.dayI() != time.dayHarvest);
        }

        public override RoomIrrigated irrigation()
        {
            return irri;
        }

        public double fer()
        {
            return getStat(constructor.fertility.index());
        }

        public RESOURCE_TILE toStore(int tx, int ty)
        {
            FarmInstance ins = get(tx, ty);
            if (ins == null)
                return null;

            return ins.getResTile();
        }

        public TILE_STORAGE toStoreTo(int tx, int ty)
        {
            FarmInstance ins = get(tx, ty);
            if (ins == null)
                return null;
            return ins.getStoreTile();
        }
    }
}