using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Infra.Export
{
    public class ROOM_EXPORT : RoomBlueprintIns<ExportInstance>, ROOM_RADIUSE, ROOM_EMPLOY_AUTO
    {
        public readonly ExportTally tally = new ExportTally();
        public readonly ExportFetcher FETCHER;
        private readonly Constructor constructor;

        private readonly Crate crate = new Crate(this);

        public ROOM_EXPORT(RoomInitData data, RoomCategorySub cat) : base(0, data, "_EXPORT", cat)
        {
            constructor = new Constructor(this, data);
            FETCHER = new ExportFetcher(this, tally);
        }

        protected override void update(double ds)
        {
        }

        public Crate crate(int tx, int ty)
        {
            return crate.get(tx, ty);
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
            tally.saver.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            tally.saver.load(saveFile);
        }

        protected override void clearP()
        {
            tally.saver.clear();
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public bool autoEmploy(Room r)
        {
            return ((ExportInstance)r).auto;
        }

        public void autoEmploy(Room r, bool b)
        {
            if (r is ExportInstance)
            {
                ((ExportInstance)r).auto = b;
            }
        }

        public override ROOM_RADIUS_INSTANCE radiusInstance(Room t)
        {
            return (ExportInstance)t;
        }

        public int storedShouldBeHigherThan(RESOURCE res)
        {
            int cap = SETT.ROOMS().STOCKPILE.tally().space.total(res);
            return (int)(cap * (1.0 - FACTIONS.player().seller(res.tr()).limit.getD()));
        }

        public int toFetchToExport(RESOURCE res)
        {
            return SETT.ROOMS().STOCKPILE.tally().amountReservable.get(res) - storedShouldBeHigherThan(res);
        }
    }
}