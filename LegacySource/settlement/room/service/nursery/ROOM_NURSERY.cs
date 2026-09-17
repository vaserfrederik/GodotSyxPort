using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Nursery
{
    public class RoomNursery : RoomBlueprintIns<NurseryInstance>, ROOM_SERVICE_HASER
    {
        public readonly string type = "NURSERY";
        private readonly NurseryConstructor constructor;
        private readonly NurseryStation ss;
        private readonly RoomService service;

        public readonly IndustryRate rate;

        public static readonly double playTime = 120;
        public readonly double ChildPerE = 10.0;

        public RoomNursery(int index, RoomInitData init, RoomCategorySub block, string key) : base(index, init, key, block)
        {
            ss = new NurseryStation(this);
            constructor = new NurseryConstructor(this, init);
            PushBo(init.data(), type, true);

            service = new RoomService(this, init, null)
            {
                public FSERVICE Service(int tx, int ty)
                {
                    return ss.Service(tx, ty);
                }

                public double TotalMultiplier()
                {
                    return 1;
                }
            };

            rate = new IndustryRate()
            {
                private readonly List<RoomBoost> boos = new List<RoomBoost>(constructor.coziness);

                public IList<RoomBoost> Boosts()
                {
                    return boos;
                }

                public Boostable Bonus()
                {
                    return bonus;
                }
            };
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
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
        }

        protected override void LoadP(FileGetter saveFile)
        {
            if (VERSION.VersionIsBefore(71, 38))
            {
                service.LoadFix(this);
            }
            else
            {
                service.Saver.Load(saveFile);
            }
        }

        protected override void ClearP()
        {
            service.Saver.Clear();
        }

        public override void AppendView(IList<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public override RoomService Service()
        {
            return service;
        }

        public FSERVICE GetOther(COORDINATE c)
        {
            NurseryInstance ins = Get(c.X, c.Y);
            if (ins != null)
            {
                COORDINATE t = ins.GetWork()[RND.rInt(ins.GetWork().Count)];
                FSERVICE s = ss.Service(t.X, t.Y);
                if (s.FindableReservedCanBe())
                    return s;
            }
            return null;
        }

        public StatServiceChild Stat()
        {
            return STATS.SERVICE().Nurseries[typeIndex()];
        }
    }
}