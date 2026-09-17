using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using settlement.main;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.employment;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using view.sett.ui.room;

namespace settlement.room.infra.elderly
{
    public sealed class ROOM_RESTHOME : RoomBlueprintIns<ResthomeInstance>
    {
        private readonly ResthomeConstructor constructor;
        private readonly Job job = new Job(this);
        public readonly EmployerSimple emp = new EmployerSimple(employment());

        public ROOM_RESTHOME(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            constructor = new ResthomeConstructor(this, init);
            ClearP();
        }

        protected override void Update(double ds)
        {
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FilePutter f)
        {
        }

        protected override void LoadP(FileGetter f)
        {
        }

        protected override void ClearP()
        {
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(constructor.quality.applier(this));
        }

        public DIR SitDir(COORDINATE c)
        {
            if (SETT.ROOMS().fData.tileData.Get(c) == ResthomeConstructor.ICHAIR)
                return DIR.ORTHO.Get(SETT.ROOMS().fData.spriteData.Get(c) & 0b011);
            return null;
        }

        public bool Dance(COORDINATE c)
        {
            if (SETT.ROOMS().fData.tileData.Get(c) == ResthomeConstructor.ISTAGE)
                return true;
            return false;
        }

        public bool Cards(COORDINATE c)
        {
            if (SETT.ROOMS().fData.tileData.Get(c) == ResthomeConstructor.ITABLE)
                return true;
            return false;
        }

        public double Quality(RoomInstance t)
        {
            return constructor.quality.Get(t) * (1.0 - t.GetDegrade());
        }

        public double Quality()
        {
            return GetStat(constructor.quality.index()) * (1.0 - DegradeAverage());
        }
    }
}