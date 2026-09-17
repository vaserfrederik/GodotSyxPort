using System;
using System.Collections.Generic;
using settlement.room.food.orchard;
using game.time;
using init.resources;
using init.type;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.water;
using settlement.weather;
using snake2d.util;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using view.sett.ui.room;
using world.map.regions;

namespace settlement.room.food.orchard
{
    public class ROOM_ORCHARD : RoomBlueprintIns<Instance>, INDUSTRY_HASER, ROOM_IRRIGATED
    {
        public static readonly string type = "ORCHARD";

        private static readonly double TILES_PER_WORKER = TIME.workSeconds() / (OTile.WORK_TIME + TIME.workSecondsWalkNext());

        private readonly Constructor constructor;
        private readonly Industry productionData;
        private readonly LIST<Industry> indus;
        private readonly OTile tile;
        public readonly RES_AMOUNT auxRes;
        private byte year = -1;
        public readonly Time time;
        public readonly double AmountPerTile = TIME.years().bitConversion(TIME.days()) / TILES_PER_WORKER;
        private readonly RoomIrrigated irri;

        public ROOM_ORCHARD(RoomInitData data, string key, RoomCategorySub cat, int index) : base(index, data, key, cat)
        {
            constructor = new Constructor(this, data);
            pushBo(data.data(), type, true);

            RoomBoost mo = WeatherMoisture.makeBoost();

            productionData = new Industry(this, data.data(), bonus());
            productionData.roomBoosts.add(constructor.fertility);
            productionData.roomBoosts.add(mo);
            new IndustryRegion(productionData, 0.5)
            {
                public override double occurence(Region reg)
                {
                    return reg.info.terrain(TERRAINS.FOREST()) + RegionInfo.vFer().getAi(reg);
                }
            };

            indus = new ArrayList<Industry>(productionData);
            this.time = new Time(data);
            auxRes = new RES_AMOUNT.Abs(RESOURCES.map().read("EXTRA_RESOURCE", data.data()), data.data().i("EXTRA_RESOURCE_AMOUNT"));
            tile = new OTile(this);
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
                ibonus = ((int)(ibonus * 100) / 100.0);
                ibonus += 0.025;
            }

            irri = new RoomIrrigated(this, bonus, 0.75, ibonus)
            {
                public override double needed(AREA area)
                {
                    return area.area();
                }

                protected override double irrigation(RoomInstance ins)
                {
                    return ((Instance)ins).irri;
                }
            };
        }

        private OTile tile(int tx, int ty)
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
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
            productionData.save(saveFile);
            saveFile.b(year);
        }

        protected override void loadP(FileGetter saveFile) throws IOException
        {
            productionData.load(saveFile);
            year = saveFile.b();
        }

        protected override void clearP()
        {
            productionData.clear();
            year = -1;
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

        public override bool industryIgnoreUI()
        {
            return true;
        }

        public bool event(int tx, int ty, double severity)
        {
            Instance ins = getter.get(tx, ty);
            if (ins != null)
            {
                return ins.event();
            }
            return false;
        }

        public class Time
        {
            public readonly int DAYS_TILL_GROWTH;
            public readonly int ripeDay;
            public readonly int deadDay;
            public readonly int days = (int)TIME.years().bitConversion(TIME.days());

            public Time(RoomInitData data)
            {
                DAYS_TILL_GROWTH = data.data().i("DAYS_TILL_GROWTH", 8, 1024);
                ripeDay = (int)(days * data.data().d("RIPE_AT_PART_OF_YEAR", 0, 1));
                deadDay = MATH.mod(ripeDay + 3, days);
            }

            public bool isRipe()
            {
                return isRipe(dayI());
            }

            public bool isRipe(double day)
            {
                return MATH.isWithin(day, ripeDay, deadDay);
            }

            public bool isDeadDay()
            {
                return dayI() == deadDay;
            }

            public double fruit()
            {
                return fruit(day());
            }

            private double fruit(double day)
            {
                double rd = (int)(ripeDay + days - 2);
                double dd = (int)(deadDay + days - 1);
                if (deadDay < ripeDay)
                {
                    dd += TIME.years().bitConversion(TIME.days());
                }

                double di = day + days;

                if (di > rd)
                {
                    if (di < dd)
                        return CLAMP.d(0.5 * (di - rd), 0, 1);
                    else if (di > dd)
                        return CLAMP.d(1.0 - (di - dd), 0, 1);
                    return 1.0;
                }
                return 0;
            }

            public double day()
            {
                return TIME.years().bitPartOf() * TIME.years().bitConversion(TIME.days());
            }

            public int dayI()
            {
                return (int)day();
            }

            public int daysTillHarvest()
            {
                return MATH.distance(dayI(), ripeDay, days);
            }

            public int daysTillHarvest(int nextTreeDays)
            {
                int day = TIME.days().bitCurrent();
                day += nextTreeDays;
                int dy = (int)TIME.years().bitConversion(TIME.days());
                int dday = day % dy;
                return day + MATH.distance(dday, ripeDay, days);
            }
        }

        public override RoomIrrigated irrigation()
        {
            return irri;
        }
    }
}