using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Infra.Inn
{
    public class ROOM_INN : RoomBlueprintIns<InnInstance>, ROOM_EMPLOY_AUTO, ROOM_SERVICE_HASER
    {
        private Constructor constructor;
        private ABed bed;
        private RoomService service;

        public ROOM_INN(RoomInitData init, RoomCategorySub block) : base(0, init, "_INN", block)
        {
            bed = new ABed(this);
            constructor = new Constructor(this, init);
            service = new RoomService(this, init, null)
            {
                Service = (tx, ty) =>
                {
                    if (bed.Init(tx, ty) != null)
                        return bed.service;
                    return null;
                }
            };
        }

        protected override void Update(double ds)
        {
            // Implementation of update method
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            service.Saver.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            service.Saver.Load(saveFile);
        }

        protected override void ClearP()
        {
            service.Saver.Clear();
        }

        public bool AutoEmploy(Room r)
        {
            return ((InnInstance)r).auto;
        }

        public void AutoEmploy(Room r, bool b)
        {
            ((InnInstance)r).auto = b;
        }

        public SFinderFindable Service(int tx, int ty)
        {
            if (bed.Init(tx, ty) != null)
                return service.Finder;
            return null;
        }

        public DIR SleepDir(int tx, int ty)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.ROOMS().fData.tileData.Is(tx, ty, d, Constructor.IHEAD))
                    return d;
            }
            return DIR.C;
        }

        public void SetReview(int tx, int ty, Review rev)
        {
            InnInstance ins = Get(tx, ty);
            Review f = ins.reviews[ins.reviews.Length - 1];
            for (int i = ins.reviews.Length - 1; i > 0; i--)
                ins.reviews[i] = ins.reviews[i - 1];
            ins.reviews[0] = f;
            f.CopyOther(rev);
            ins.earnings += rev.credits;
        }

        public RoomService Service()
        {
            return service;
        }
    }
}