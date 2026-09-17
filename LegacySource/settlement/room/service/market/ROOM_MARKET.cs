using System;
using System.Collections.Generic;
using System.IO;
using game;
using init.race;
using init.race.RaceResources;
using init.resources;
using init.type;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.food.eatery;
using settlement.room.service.module;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;
using view.sett.ui.room;

namespace settlement.room.service.market
{
    public sealed class ROOM_MARKET : RoomBlueprintIns<MarketInstance>, ROOM_EMPLOY_AUTO, ROOM_SERVICE_ACCESS_HASER
    {
        private static readonly string ¤¤food = "wares";

        static ROOM_MARKET()
        {
            D.ts(typeof(ROOM_MARKET));
        }

        private readonly Constructor constructor;
        private readonly RoomServiceAccess service;
        private RoomDistribution dist;

        public ROOM_MARKET(string key, int index, RoomInitData data, RoomCategorySub cat) : base(index, data, key, cat)
        {
            constructor = new Constructor(this, data);

            service = new RoomServiceAccess(this, data, NEEDS.TYPES().SHOPPING)
            {
                service = (tx, ty) => dist.service(tx, ty)
            };

            GAME.addOnInit(new ACTION
            {
                exe = () =>
                {
                    var ress = new ArrayListGrower<RESOURCE>();
                    RBITImp bits = new RBITImp();

                    foreach (RaceResource r in RACES.res().ALL)
                    {
                        ress.add(r.res);
                        bits.or(r.res.bit);
                    }

                    dist = new RoomDistribution(this, this, ress, bits, 1)
                    {
                        isPref = (r, race) => race.home().clas(HCLASSES.CITIZEN()).amount(r) > 0,
                        isDeposit = (tx, ty) => constructor.isStore(tx, ty),
                        isCrate = (tx, ty) => constructor.isCrate(tx, ty)
                    };
                }
            });
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
            return service.finder;
        }

        protected override void saveP(FilePutter saveFile)
        {
            service.saver.save(saveFile);
            dist.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            service.saver.load(saveFile);
            dist.load(saveFile);
        }

        protected override void clearP()
        {
            service.saver.clear();
            dist.clear();
        }

        public long totalFood()
        {
            return dist.tStored.total.get();
        }

        public long amount(ResG e)
        {
            return dist.stored(e.resource).total.get();
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            dist.appendView(mm, ¤¤food);
        }

        public bool autoEmploy(Room r)
        {
            return ((MarketInstance)r).autoE;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((MarketInstance)r).autoE = b;
        }

        public RoomServiceAccess service()
        {
            return service;
        }

        public int buy(RaceResource res, int amount, int tx, int ty)
        {
            return dist.consume(res.res, amount, tx, ty);
        }

        public override bool registersEnvironment()
        {
            return true;
        }
    }
}