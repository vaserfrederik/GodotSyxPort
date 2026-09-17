using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Food
{
    public sealed class RoomCanteen : RoomBlueprintIns<CanteenInstance>, IROOM_EMPLOY_AUTO, IROOM_SERVICE_ACCESS_HASER, IINDUSTRY_HASER
    {
        private readonly Constructor constructor;
        public readonly Industry industryFuel;
        private readonly RoomServiceAccess service;
        private readonly long[] amounts;
        private long total;
        private readonly SService food;
        private readonly SWork job;
        private readonly SChair chair;
        private readonly List<Industry> indus;

        public RoomCanteen(string key, int index, RoomInitData data, RoomCategorySub cat) : base(index, data, key, cat)
        {
            constructor = new Constructor(this, data);
            industryFuel = new Industry(this, data.data(), null);
            service = new RoomServiceAccess(this, data, NEEDS.TYPES().HUNGER)
            {
                public FSERVICE service(int tx, int ty)
                {
                    return food.get(tx, ty);
                }
            };
            employment().countInputSet();
            indus = new List<Industry> { industryFuel };
        }

        protected override void update(double ds)
        {
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return service.finder;
        }

        protected override void saveP(FilePutter saveFile)
        {
            industryFuel.save(saveFile);
            service.saver.save(saveFile);

            saveFile.l(total);
            saveFile.lsE(amounts);
        }

        protected override void loadP(FileGetter saveFile)
        {
            industryFuel.load(saveFile);
            service.saver.load(saveFile);
            total = saveFile.l();
            saveFile.lsE(amounts);
        }

        protected override void clearP()
        {
            industryFuel.clear();
            service.saver.clear();
            total = 0;
            Array.Fill(amounts, 0L);
        }

        public long totalFood()
        {
            return total;
        }

        public long amount(ResG e)
        {
            return amounts[e.index()];
        }

        public override void appendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).make());
        }

        public bool autoEmploy(Room r)
        {
            return ((CanteenInstance)r).autoE;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((CanteenInstance)r).autoE = b;
        }

        public RoomServiceAccess service()
        {
            return service;
        }

        public int grab(List<ResG> prefs, int amount, int tx, int ty)
        {
            ResG pref = prefs.rnd();
            CanteenInstance ins = getter.get(tx, ty);
            if (ins == null)
                return Meal.make(pref, 0, 0);
            FSERVICE f = food.get(tx, ty);
            if (f == null)
                return Meal.make(pref, 0, 0);
            f.consume();

            if (amount > ins.amountTotal() - ins.serviceReserved() + 1)
            {
                amount = ins.amountTotal() - ins.serviceReserved() + 1;
            }
            int am = amount;

            int ipref = 0;
            int iopref = 0;

            if (am < 0)
            {
                GAME.Notify("here! " + am + " " + tx + " " + ty);
                return Meal.make(pref, 0, 0);
            }

            ResG ee = null;

            if (ins.amount(pref) > 0)
            {
                ee = pref;
                int a = Math.Min(ins.amount(pref), am);
                am -= a;
                ipref += a;
                ins.consume(pref, a, tx, ty);
                GAME.player().res().inc(pref.resource, RTYPE.CONSUMED, -a);
            }

            if (am > 0)
            {
                int ri = RND.rInt(prefs.size());
                for (int i = 0; i < prefs.size() && am > 0; i++)
                {
                    ResG g = prefs.getC(i + ri);
                    if (ins.amount(g) > 0)
                    {
                        if (ee == null)
                            ee = g;
                        int a = Math.Min(ins.amount(g), am);
                        am -= a;
                        iopref += a;
                        ins.consume(g, a, tx, ty);
                        GAME.player().res().inc(g.resource, RTYPE.CONSUMED, -a);
                    }
                }
            }

            if (am > 0)
            {
                int ri = RND.rInt(RESOURCES.EDI().all().size());
                for (int i = 0; i < RESOURCES.EDI().all().size() && am > 0; i++)
                {
                    ResG g = RESOURCES.EDI().all().getC(i + ri);
                    if (ins.amount(g) > 0)
                    {
                        if (ee == null)
                            ee = g;
                        int a = Math.Min(ins.amount(g), am);
                        am -= a;
                        ins.consume(g, a, tx, ty);
                        GAME.player().res().inc(g.resource, RTYPE.CONSUMED, -a);
                    }
                }
            }

            if (ee == null)
                ee = RESOURCES.EDI().all().rnd();

            amount -= am;
            int pt = ipref + iopref;
            double pv = 0;
            if (pt > 0)
                pv = (ipref + 0.25 * iopref) / pt;
            return Meal.make(ee, amount, pv);
        }

        public COORDINATE getChair(int tx, int ty)
        {
            return chair.get(tx, ty);
        }

        public DIR setChair(int tx, int ty, int mealData)
        {
            return chair.set(tx, ty, mealData);
        }

        public void returnChair(int tx, int ty)
        {
            chair.returnTable(tx, ty);
        }

        public List<Industry> industries()
        {
            return indus;
        }
    }
}