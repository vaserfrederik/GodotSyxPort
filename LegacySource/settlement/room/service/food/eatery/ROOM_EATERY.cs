using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Food.Eatery
{
    public class ROOM_EATERY : RoomBlueprintIns<EateryInstance>, ROOM_EMPLOY_AUTO, ROOM_SERVICE_ACCESS_HASER
    {
        private static readonly string ¤¤food = "food";

        static ROOM_EATERY()
        {
            D.ts(typeof(ROOM_EATERY));
        }

        readonly Constructor constructor;
        readonly RoomServiceAccess service;
        readonly RoomDistribution dist;

        public ROOM_EATERY(string key, int index, RoomInitData data, RoomCategorySub cat) : base(index, data, key, cat)
        {
            constructor = new Constructor(this, data);

            service = new RoomServiceAccess(this, data, NEEDS.TYPES().HUNGER)
            {
                public FSERVICE Service(int tx, int ty)
                {
                    return dist.Service(tx, ty);
                }
            };

            RBITImp bits = new RBITImp();
            foreach (ResGEat g in RESOURCES.EDI().All())
            {
                if (g.serve)
                    bits.Or(g.resource);
            }

            dist = new RoomDistribution(this, this, RESOURCES.EDI().Res(), bits, StatsFood.MAX_RATIONS)
            {
                protected override bool IsPref(RESOURCE r, Race race)
                {
                    return race.pref().foodMask.Has(r);
                }

                protected override bool IsDeposit(int tx, int ty)
                {
                    return constructor.IsDeposit(tx, ty);
                }

                protected override bool IsCrate(int tx, int ty)
                {
                    return constructor.IsCrate(tx, ty);
                }
            };
        }

        protected override void Update(double ds)
        {
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return service.Finder;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            service.Saver.Save(saveFile);
            dist.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            service.Saver.Load(saveFile);
            dist.Load(saveFile);
        }

        protected override void ClearP()
        {
            service.Saver.Clear();
            dist.Clear();
        }

        public long TotalFood()
        {
            return dist.tStored.Total.Get();
        }

        public long Amount(ResG e)
        {
            return dist.Stored(e.resource).Total.Get();
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            dist.AppendView(mm, ¤¤food);
        }

        public override bool AutoEmploy(Room r)
        {
            return ((EateryInstance)r).autoE;
        }

        public override void AutoEmploy(Room r, bool b)
        {
            ((EateryInstance)r).autoE = b;
        }

        public override RoomServiceAccess Service()
        {
            return service;
        }

        public int Eat(List<ResG> prefs, int amount, int tx, int ty)
        {
            return dist.Consume(prefs, amount, tx, ty);
        }

        public override bool RegistersEnvironment()
        {
            return true;
        }
    }
}