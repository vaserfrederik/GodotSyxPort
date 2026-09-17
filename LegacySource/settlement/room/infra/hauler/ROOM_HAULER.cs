using System;
using System.Collections.Generic;
using game.faction;
using init.resources;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using view.sett.ui.room;

namespace settlement.room.infra.hauler
{
    public sealed class ROOM_HAULER : RoomBlueprintIns<HaulerInstance>, ROOM_RADIUSE, ROOM_EMPLOY_AUTO
    {
        private readonly Furnisher constructor;
        public readonly Crate crate = new Crate(this);
        public readonly HaulerTally tally = new HaulerTally();

        public ROOM_HAULER(RoomInitData init, RoomCategorySub cat) : base(0, init, "_HAULER", cat)
        {
            constructor = new Constructor(init);
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
        }

        protected override void loadP(FileGetter saveFile)
        {
            this.tally.Clear();
            foreach (HaulerInstance ins in all())
            {
                tally.Init(ins);
            }
        }

        protected override void clearP()
        {
            this.tally.Clear();
        }

        public override bool degrades()
        {
            return false;
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).make());
        }

        public override bool autoEmploy(Room r)
        {
            return ((HaulerInstance)r).auto;
        }

        public override void autoEmploy(Room r, bool b)
        {
            ((HaulerInstance)r).auto = b;
        }

        public override ROOM_RADIUS_INSTANCE radiusInstance(Room t)
        {
            return ((HaulerInstance)t);
        }

        public void removeFromEverywhere(STOCKPILE.StockpileImp stock, RTYPE record)
        {
            double[] res = new double[RESOURCES.ALL().Count];
            foreach (RESOURCE r in RESOURCES.ALL())
            {
                double d = stock.Get(r) / (1.0 + tally.amountReservable.Get(r));
                d = CLAMP.d(d, 0, 1);
                res[r.Index()] = d;
            }

            foreach (COORDINATE c in TILE_BOUNDS)
            {
                Room r = ROOMS().STOCKPILE.Get(c.x(), c.y());
                if (r == null)
                    continue;
                RESOURCE_TILE cr = (RESOURCE_TILE)r.storage(c.x(), c.y());
                if (cr != null && cr.resource() != null && stock.Get(cr.resource()) > 0 && res[cr.resource().Index()] > 0)
                {
                    int a = (int)Math.Ceiling(res[cr.resource().Index()] * cr.reservable());
                    stock.inc(cr.resource(), -a);
                    for (int i = 0; i < a; i++)
                    {
                        cr.findableReserve();
                        cr.resourcePickup();
                    }
                    FACTIONS.player().res().inc(cr.resource(), record, -a);
                }
            }
        }
    }
}