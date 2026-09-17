using System;
using System.Collections.Generic;
using System.IO;

using Init.Resources;
using Settlement.Entity.Humanoid;
using Settlement.Main;
using Settlement.Path.Finders;
using Settlement.Room.Main;
using Settlement.Room.Main.Category;
using Settlement.Room.Main.Furnisher;
using Settlement.Room.Main.Job;
using Settlement.Room.Main.Util;
using Snake2D.Util.File;
using Snake2D.Util.Misc;
using Snake2D.Util.Rnd;
using Snake2D.Util.Sets;
using Util.Text;
using View.Sett.UI.Room;

namespace Settlement.Room.Infra.Stockpile
{
    public sealed class RoomStockpile : RoomBlueprintIns<StockpileInstance>, IRoomRadius, IRoomEmployAuto
    {
        public static readonly int MIN_CARRY = 7;

        private readonly StockpileTally tally = new StockpileTally();

        public readonly Constructor constructor;

        public readonly Crate crate = new Crate(this);
        private static readonly CharSequence ¤¤bname = "¤Carry Capacity";
        private static readonly CharSequence ¤¤bdesc = "¤Carry Capacity of all logistics workers.";

        public readonly Organiser org = new Organiser(this);

        static RoomStockpile()
        {
            D.ts(typeof(RoomStockpile));
        }

        public RoomStockpile(RoomInitData init, RoomCategorySub cat) : base(0, init, "_STOCKPILE", cat)
        {
            constructor = new Constructor(this, init);
            pushBo(init.data(), ¤¤bname, ¤¤bdesc, null, false, MIN_CARRY + 3);
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

        public StockpileTally tally()
        {
            return tally;
        }

        protected override void saveP(FilePutter saveFile)
        {
            tally.saver.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            this.tally.clear();
            foreach (var ins in all())
            {
                tally.init(ins);
            }
            tally.saver.load(saveFile);
        }

        protected override void clearP()
        {
            this.tally.clear();
            tally.saver.clear();
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public int crates()
        {
            int am = 0;
            foreach (var ins in all())
                am += ins.crates.size();
            return am;
        }

        public bool autoEmploy(Room r)
        {
            return ((StockpileInstance)r).autoE;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((StockpileInstance)r).autoE = b;
        }

        public int carryCap(Humanoid skill)
        {
            double dam = bonus.get(skill.indu());
            int am = (int)dam;
            am += RND.rFloat() < dam - am ? 1 : 0;
            am = CLAMP.i(am, 1, 100);
            return am;
        }

        public RoomInstance getInstance(int wI, RESOURCE res)
        {
            StockpileInstance ins = getInstance(wI);

            if (ins != null && SETT.ROOMS().STOCKPILE.tally().crates.get(res, ins) > 0)
            {
                return ins;
            }
            return null;
        }

        public override IRoomRadiusInstance radiusInstance(Room t)
        {
            return (StockpileInstance)t;
        }
    }
}