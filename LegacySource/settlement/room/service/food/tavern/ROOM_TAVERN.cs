using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Food.Tavern
{
    public class ROOM_TAVERN : RoomBlueprintIns<TavernInstance>, ROOM_SERVICE_ACCESS_HASER, ROOM_EMPLOY_AUTO
    {
        private static readonly string ¤¤food = "drinks";

        static ROOM_TAVERN()
        {
            D.ts(typeof(ROOM_TAVERN));
        }

        public readonly RoomServiceAccess serviceData;
        readonly RoomDistribution dist;
        readonly Constructor constructor;

        public ROOM_TAVERN(string key, int index, RoomInitData data, RoomCategorySub cat) : base(index, data, key, cat)
        {
            constructor = new Constructor(this, data);
            serviceData = new RoomServiceAccess(this, data, NEEDS.TYPES().THIRST)
            {
                Service = (tx, ty) => dist.Service(tx, ty)
            };

            RBITImp bits = new RBITImp();
            foreach (ResGDrink g in RESOURCES.DRINKS().All())
            {
                if (g.Serve)
                    bits.Or(g.Resource);
            }

            dist = new RoomDistribution(this, this, RESOURCES.DRINKS().Res(), bits, StatsFood.MAX_RATIONS)
            {
                IsPref = (r, race) => race.Pref().DrinkMask.Has(r),
                IsDeposit = (tx, ty) => SETT.ROOMS().fData.TileData.Get(tx, ty) == Constructor.ITABLE,
                IsCrate = (tx, ty) => SETT.ROOMS().fData.TileData.Get(tx, ty) == Constructor.ISTORAGE
            };
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            dist.AppendView(mm, ¤¤food);
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return serviceData.Finder;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            serviceData.Saver.Save(saveFile);
            dist.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            serviceData.Saver.Load(saveFile);
            dist.Load(saveFile);
        }

        protected override void ClearP()
        {
            serviceData.Saver.Clear();
            dist.Clear();
        }

        public RoomServiceAccess Service()
        {
            return serviceData;
        }

        public bool AutoEmploy(Room r)
        {
            return ((TavernInstance)r).Auto;
        }

        public void AutoEmploy(Room r, bool b)
        {
            ((TavernInstance)r).Auto = b;
        }

        public int Consume(List<ResG> prefs, int amount, int tx, int ty)
        {
            return dist.Consume(prefs, amount, tx, ty);
        }
    }
}