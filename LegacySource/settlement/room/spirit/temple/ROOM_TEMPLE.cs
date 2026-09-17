using System;
using System.Collections.Generic;
using System.IO;
using game;
using init.race;
using init.religion;
using init.resources;
using init.type;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using settlement.room.spirit.temple;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett.ui.room;

namespace settlement.room.spirit.temple
{
    public class ROOM_TEMPLE : RoomBlueprintIns<TempleInstance>, ROOM_SERVICE_HASER
    {
        int consumed = 0;
        private int year = TIME.years().bitsSinceStart();
        readonly TempleConstructor constructor;
        readonly RoomService service;
        readonly Service serviceTile;
        readonly TempleJob job;
        readonly TempleAltar altar;
        public readonly RESOURCE resource;
        public readonly double STIME;
        private double searchCooloff = 0;
        public const string TYPE = "TEMPLE";

        public readonly Religion religion;

        public ROOM_TEMPLE(int typeIndex, RoomInitData data, string key, RoomCategorySub cat) : base(typeIndex, data, key, cat)
        {
            constructor = new TempleConstructor(this, data);
            serviceTile = new Service(this);
            service = new RoomService(this, data, NEEDS.TYPES().TEMPLE)
            {
                public FSERVICE service(int tx, int ty) => serviceTile.get(tx, ty);
            };
            religion = RELIGIONS.MAP().read(data.data());
            switch (data.data().value("SACRIFICE_TYPE"))
            {
                case "RESOURCE":
                    resource = RESOURCES.map().read("SACRIFICE_RESOURCE", data.data());
                    job = new TempleJob.Resources(this, resource);
                    altar = new TempleAltar.Resource(this, resource);
                    break;
                case "ANIMAL":
                    resource = RESOURCES.LIVESTOCK();
                    job = new TempleJob.Resources(this, resource);
                    altar = new TempleAltar.Animal(this);
                    break;
                case "HUMAN":
                    resource = null;
                    job = new TempleJob.None(this);
                    altar = new TempleAltar.Prisoner(this);
                    break;
                default:
                    resource = null;
                    job = null;
                    altar = null;
                    data.data().error(data.data().value("SACRIFICE_TYPE") + " is not a sacrifice type. Pick from RESOURCES,", "SACRIFICE_TYPE");
                    break;
            }

            STIME = data.data().d("SACRIFICE_TIME", 0, 10);
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        protected override void saveP(FilePutter f)
        {
            service.saver.save(f);
            f.i(consumed);
        }

        protected override void loadP(FileGetter f) throws IOException
        {
            service.saver.load(f);
            consumed = f.i();
        }

        protected override void clearP()
        {
            service.saver.clear();
            consumed = 0;
            year = TIME.years().bitsSinceStart();
        }

        protected override void update(double ds)
        {
            if (year != TIME.years().bitsSinceStart())
            {
                consumed = 0;
                year = TIME.years().bitsSinceStart();
            }
            searchCooloff -= ds;
            if (searchCooloff < 0)
                searchCooloff = 0;
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return service.finder;
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public RoomService service()
        {
            return service;
        }

        int si = 0;

        public COORDINATE sacrificeReserve(Race race)
        {
            if (!(altar is TempleAltar.Prisoner))
            {
                return null;
            }
            if (searchCooloff > 0)
                return null;

            for (int i = 0; i < instancesSize(); i++)
            {
                si %= instancesSize();
                TempleInstance ins = getInstance(si);
                si++;
                if (ins.sacrificesRequired > 0)
                {
                    int old = ins.jobs.getI();
                    for (int j = 0; j < ins.jobs.size(); j++)
                    {
                        if (job.get(ins.jobs.set(j).x(), ins.jobs.get().y()) != null)
                        {
                            TempleAltar.Prisoner p = (Prisoner)altar.get(job.faceCoo().x(), job.faceCoo().y());
                            if (p.sacrificeReservable())
                            {
                                ins.jobs.set(old);
                                p.sacrificeReserve(race);
                                return p.coo();
                            }
                        }
                    }
                    ins.jobs.set(old);
                    GAME.Notify("Weird!");
                }
            }

            searchCooloff = 60;
            return null;
        }

        public bool sacrifices()
        {
            if (!(altar is TempleAltar.Prisoner))
            {
                return false;
            }
            return true;
        }

        public bool sacrificeReserved(COORDINATE coo)
        {
            if (altar.get(coo.x(), coo.y()) == null)
                return false;
            if (!(altar is TempleAltar.Prisoner))
            {
                return false;
            }
            TempleAltar.Prisoner p = (Prisoner)altar;
            return p.sacrificeReserved();
        }

        public void sacrificeUnreserve(COORDINATE coo)
        {
            if (altar.get(coo.x(), coo.y()) == null)
                return;
            if (!(altar is TempleAltar.Prisoner))
            {
                return;
            }
            TempleAltar.Prisoner p = (Prisoner)altar;
            p.sacrificeUnreserve();
        }

        public void sacrificeSetReady(COORDINATE coo)
        {
            if (altar.get(coo.x(), coo.y()) == null)
                return;
            if (!(altar is TempleAltar.Prisoner))
            {
                return;
            }
            TempleAltar.Prisoner p = (Prisoner)altar;
            p.sacrificeReady();
        }

        public double sacrificeKillAmount(COORDINATE coo)
        {
            if (altar.get(coo.x(), coo.y()) == null)
                return 0;
            if (!(altar is TempleAltar.Prisoner))
            {
                return 0;
            }
            TempleAltar.Prisoner p = (Prisoner)altar;
            return p.sacrificeKillAmount();
        }
    }
}