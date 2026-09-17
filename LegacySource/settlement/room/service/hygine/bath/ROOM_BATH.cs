using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using view.sett.ui.room;

namespace settlement.room.service.hygine.bath
{
    public sealed class ROOM_BATH : RoomBlueprintIns<BathInstance>, ROOM_SERVICE_NEED_HASER, INDUSTRY_HASER, ROOM_EMPLOY_AUTO
    {
        private readonly RoomServiceNeed data;
        private readonly Constructor constructor;
        private readonly CharSequence sHeating;
        private readonly CharSequence sHeatingDesc;
        private readonly CharSequence sHeatingProblem;
        private readonly CharSequence sWaterProblem;
        private readonly Industry consumption;
        private readonly LIST<Industry> indus;

        public ROOM_BATH(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            data = new RoomServiceNeed(this, init)
            {
                service = (tx, ty) => Bath.init(tx, ty, this)
            };

            constructor = new Constructor(this, init);
            sHeating = init.text().text("HEATING");
            sHeatingDesc = init.text().text("HEATING_DESC");
            sHeatingProblem = init.text().text("HEATING_PROBLEM");
            sWaterProblem = init.text().text("WATER_PROBLEM");
            consumption = new Industry(this, init.data(), null)
            {
                consumptionRate = (ins, h, oo) =>
                {
                    BathInstance i = (BathInstance)ins;
                    if (ins.employees().employed() == 0)
                        return 0;
                    return oo.rate * i.service.total() / ins.employees().employed();
                }
            };

            indus = new ArrayList<Industry>(consumption);
            employment().countInputSet();
        }

        public Bath bath(int tx, int ty)
        {
            return Bath.init(tx, ty, this);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return data.finder;
        }

        public RoomServiceNeed service()
        {
            return data;
        }

        public static bool isPool(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r is BathInstance)
            {
                int d = ROOMS().data.get(tx, ty);
                return (d & Bits.BITS) == Bits.POOL && (d & 1) == 1;
            }
            return false;
        }

        public bool isBench(int tx, int ty)
        {
            if (is(tx, ty))
            {
                int d = ROOMS().data.get(tx, ty);
                return (d & Bits.BITS) == Bits.BENCH;
            }
            return false;
        }

        public DIR getBenchDir(int tx, int ty)
        {
            if (!isBench(tx, ty))
                throw new RuntimeException();
            foreach (DIR d in DIR.ORTHO)
            {
                int da = ROOMS().data.get(tx, ty, d);
                if (da == Bits.BENCH_TAIL)
                    return d;
            }
            throw new RuntimeException();
        }

        protected override void saveP(FilePutter file)
        {
            data.saver.save(file);
            consumption.save(file);
        }

        protected override void loadP(FileGetter saveFile)
        {
            data.saver.load(saveFile);
            consumption.load(saveFile);
        }

        protected override void clearP()
        {
            this.data.saver.clear();
            consumption.clear();
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public LIST<Industry> industries()
        {
            return indus;
        }

        public bool autoEmploy(Room r)
        {
            return ((BathInstance)r).auto;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((BathInstance)r).auto = b;
        }

        public double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins)
        {
            BathInstance uu = (BathInstance)ins;
            double n = i.rate * uu.service.total();
            GFORMAT.f0(text, -n);
            return n;
        }

        public void industryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins)
        {
        }
    }
}