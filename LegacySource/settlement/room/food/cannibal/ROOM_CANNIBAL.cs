using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Food.Cannibal
{
    public class ROOM_CANNIBAL : RoomBlueprintIns<CannibalInstance>, PUNISHMENT_SERVICE
    {
        private static readonly string ¤¤eat = "Slaughter";
        private static readonly string ¤¤eatD = "Prisoners convicted to be executed can be put to good use in the Cannibal.";

        static
        {
            D.ts(typeof(ROOM_CANNIBAL));
        }

        private readonly Job job;
        private readonly int[] produced;
        private int year = -1;
        private readonly double[] cannibalism;
        private readonly Constructor constructor;

        private Cage cage = new Cage(this);
        private RESOURCE[] resources;

        private int prisoners;

        private readonly BooleanOEImp<Race> permission;

        public ROOM_CANNIBAL(RoomInitData init, RoomCategorySub cat) : base(0, init, "_CANNIBAL", cat)
        {
            constructor = new Constructor(this, init);
            job = new Job(this);

            permission = new BooleanOEImp<Race>(RACES.all().Count, true);
            permission.info = new INFO(¤¤eat, ¤¤eatD);
        }

        private RESOURCE[] resources()
        {
            RBITImp m = new RBITImp();

            if (resources == null)
            {
                int am = 0;
                foreach (Race race in RACES.all())
                {
                    foreach (RES_AMOUNT r in race.resources())
                    {
                        if (!m.has(r.resource()))
                        {
                            am++;
                            m.or(r.resource());
                        }
                    }
                }
                RESOURCE[] res = new RESOURCE[am];
                m.clear();
                am = 0;
                foreach (Race race in RACES.all())
                {
                    foreach (RES_AMOUNT r in race.resources())
                    {
                        if (!m.has(r.resource()))
                        {
                            res[am++] = r.resource();
                            m.or(r.resource());
                        }
                    }
                }
                resources = res;
            }

            return resources;
        }

        protected override void update(double ds)
        {
            if (year != TIME.years().bitsSinceStart())
            {
                produced.Fill(0);
                year = TIME.years().bitsSinceStart();
            }

            for (int ri = 0; ri < RACES.all().Count; ri++)
            {
                double d = cannibalism[ri];
                if (d < 1)
                    d = 1;

                cannibalism[ri] -= d * ds / (TIME.years().bitSeconds() * 2 * cannibalism.Length);
                cannibalism[ri] = CLAMP.d(cannibalism[ri], 0, 1.5);
            }
        }

        public SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        protected override void saveP(FilePutter f)
        {
            f.isE(produced);
            f.i(year);
            f.dsE(cannibalism);
            f.i(prisoners);
            permission.save(f);
        }

        protected override void loadP(FileGetter f)
        {
            f.isE(produced);
            year = f.i();
            f.dsE(cannibalism);
            prisoners = f.i();
            permission.load(f);
        }

        protected override void clearP()
        {
            year = -1;
            produced.Fill(0);
            cannibalism.Fill(0);
            permission.clear();
            prisoners = 0;
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
        }

        public void reportCannibal(Race race)
        {
            cannibalism[race.index] += 100.0 / STATS.POP().POP.data().get(null);
        }

        public double cannibalism(Race race)
        {
            return CLAMP.d(cannibalism[race.index], 0, 1);
        }

        public BOOLEAN_OE<Race> punishEnabled()
        {
            return permission;
        }

        public void setRace(int tx, int ty, Race race)
        {
            CannibalInstance ins = get(tx, ty);
            if (ins != null)
            {
                int d = SETT.ROOMS().data.get(tx, ty);
                d = Job.race.set(d, race.index());
                SETT.ROOMS().data.set(ins, tx, ty, d);
            }
        }

        public int punishTotal()
        {
            return employment().employedMax();
        }

        public int punishUsed()
        {
            return prisoners;
        }

        public Cage getPrisonerCage()
        {
            if (prisoners >= punishTotal())
            {
                return null;
            }

            if (instancesSize() == 0)
                return null;

            int ri = RND.rInt(instancesSize());

            for (int i = 0; i < instancesSize(); i++)
            {
                CannibalInstance ins = getInstance((i + ri) % instancesSize());
                if (ins.prisoners < ins.employees().max())
                {
                    for (int ci = 0; ci < ins.cages.size(); ci++)
                    {
                        Cage ca = cage(ins.cages.get().x(), ins.cages.get().y());
                        if (ca.available())
                            return ca;
                        ins.cages.inc();
                    }
                    LOG.err("nono");
                    return null;
                }
            }

            LOG.err("nono2");
            return null;
        }

        public Cage getWorkCage(RoomInstance work)
        {
            CannibalInstance ins = (CannibalInstance)work;

            if (ins.reservable <= 0)
                return null;

            for (int ci = 0; ci < ins.cages.size(); ci++)
            {
                Cage ca = cage(ins.cages.get().x(), ins.cages.get().y());
                if (ca.canGrab())
                    return ca;
                ins.cages.inc();
            }
            LOG.err("nono");
            return null;
        }

        public Cage cage(int tx, int ty)
        {
            return cage.get(tx, ty);
        }
    }
}