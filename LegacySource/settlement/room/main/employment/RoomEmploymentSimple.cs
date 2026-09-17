using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Room.Main.Employment
{
    public class RoomEmploymentSimple
    {
        private bool countInput = false;
        private readonly RoomBlueprintIns p;
        protected int workersNeeded = 0;
        private int employedTot = 0;
        private int[] employed;

        private int employedMax = 0;
        private int[] workersNeededPreferred;

        public readonly string title;
        public readonly string verb;
        private double hourStart;
        private readonly SoundRace sound;
        private bool shift;
        public readonly double accidentsPerYear;
        public readonly double defaultFullfillment;
        public readonly double healthFactor;
        private double fill = 1.0;
        public readonly int eindex;
        public readonly int largeWorkforce;
        private int efficiency = 0;
        private int proximity = 0;
        private int fetch = 0;

        static List<RoomEmploymentSimple> WORK_ALL = new List<RoomEmploymentSimple>(10);
        static RoomEmploymentSimple()
        {
            new GameDisposable()
            {
                protected override void Dispose()
                {
                    WORK_ALL.Clear();
                }
            };
        }

        public RoomEmploymentSimple(string key, RoomBlueprintIns p, RoomInitData init)
        {
            this.p = p;
            Json data = init.data().json(key);
            Json text = init.text().json(key);
            this.title = text.text("TITLE");
            this.verb = text.text("VERB");
            this.hourStart = data.d("SHIFT_OFFSET", 0, 0.99);
            shift = data.has("NIGHT_SHIFT") && data.bool("NIGHT_SHIFT");
            this.sound = AUDIO.race("ROOM_WORK_" + p.key);
            if (data.has("FULFILLMENT"))
                defaultFullfillment = data.d("FULFILLMENT", -100000, 100000);
            else
                defaultFullfillment = 0.5;
            accidentsPerYear = data.dTry("ACCIDENTS_PER_YEAR", 0, 10000, 0) / 2.0;
            healthFactor = data.dTry("HEALTH_FACTOR", 0, 1.0, 1);
            largeWorkforce = data.i("LARGE_WORKFORCE", 10, 10000, 1000);
            eindex = WORK_ALL.Add(this);

            GVALUES.FACTION.push("EMPLOYED_" + p.key(), Dic.¤¤Employees + ": " + p.info.names, p.iconBig(), new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return employed();
                }
            }, false);
        }

        public void countInputSet()
        {
            countInput = true;
        }

        public bool countInput()
        {
            return countInput;
        }

        public int eindex()
        {
            return eindex;
        }

        public int employed()
        {
            return employedTot;
        }

        public int employed(WGROUP g)
        {
            if (g == null)
                return employedTot;
            return employed(g.type, g.race);
        }

        public int employed(HTYPE t, Race r)
        {
            return employed[r.index() * HTYPES.ALL().size() + t.index()];
        }

        public int employedMax()
        {
            return employedMax;
        }

        public int neededWorkers()
        {
            return workersNeeded;
        }

        public int neededWorkers(WGROUP t)
        {
            return workersNeededPreferred[t.index];
        }

        public SoundRace sound()
        {
            return sound;
        }

        void register(RoomEmploymentIns ins, int delta)
        {
            workersNeeded += delta * ins.hardTarget();
            for (int hi = 0; hi < WGROUP.all().size(); hi++)
            {
                WGROUP g = WGROUP.all().get(hi);
                workersNeededPreferred[g.index] += ins.preffered().is(g) ? delta * ins.hardTarget() : 0;
            }

            efficiency += delta * ins.efficiency() * 100 * ins.hardTarget();
            efficiency = Math.Max(0, efficiency);
            proximity += delta * ins.proximity() * 100 * ins.hardTarget();
            proximity = Math.Max(0, proximity);
            fetch += delta * ins.fetchProximity() * 100 * ins.hardTarget();
            fetch = Math.Max(0, fetch);
            employedMax += delta * ins.max();
            foreach (RoomEquip t in tools())
            {
                t.count(this, delta * ins.tools(t));
            }

            if (delta > 0 && workersNeeded < 0)
                throw new RuntimeException(blueprint().info.name + " " + workersNeeded + " " + delta);
        }

        void employ(Humanoid h, int delta)
        {
            employedTot += delta;
            employed[h.race().index() * HTYPES.ALL().size() + h.indu().hType().index()] += delta;
        }

        void loadadd(RoomEmploymentIns ins)
        {
            ins.add();
            employedTot += ins.employed();
        }

        public double getFill()
        {
            return fill;
        }

        public List<RoomEquip> tools()
        {
            return SETT.ROOMS().employment.equip.get(this);
        }

        public double efficiency()
        {
            if (p.instancesSize() == 0)
                return 1;
            return efficiency / (100.0 * workersNeeded);
        }

        public double proximity()
        {
            if (p.instancesSize() == 0 || workersNeeded <= 0)
                return 1;
            return proximity / (100.0 * workersNeeded);
        }

        public double fetch()
        {
            if (p.instancesSize() == 0 || workersNeeded <= 0)
                return 1;
            return fetch / (100.0 * workersNeeded);
        }

        public double totEff()
        {
            return efficiency() * proximity() * fetch();
        }

        public RoomBlueprintIns blueprint()
        {
            return p;
        }

        void save(FilePutter file)
        {
            file.isE(employed);
        }

        void load(FileGetter file) throws IOException
        {
            file.isE(employed);
        }

        void clear()
        {
            employed.Fill(0);
            employedTot = 0;
            employedMax = 0;
            workersNeeded = 0;
            efficiency = 0;
            proximity = 0;
            fetch = 0;
            workersNeededPreferred.Fill(0);
        }

        public double getShiftStart()
        {
            return hourStart;
        }

        public void setShiftStart(double start, bool nights)
        {
            this.hourStart = start;
            this.shift = nights;
        }

        public bool worksNights()
        {
            return shift;
        }

        public class EmployerSimple
        {
            private readonly RoomEmploymentSimple si;

            public EmployerSimple(RoomEmploymentSimple si)
            {
                this.si = si;
            }

            public bool employ(Humanoid h)
            {
                RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
                if (ins != null && ins.blueprintI() == si.blueprint())
                {
                    if (ins.blueprintI() == si.blueprint())
                    {
                        if (ins.employees().isOverstaffed())
                        {
                            STATS.WORK().EMPLOYED.set(h, null);
                            ins = null;
                        }
                        else
                            return true;
                    }
                    else
                    {
                        STATS.WORK().EMPLOYED.set(h, null);
                    }
                }

                if (si.neededWorkers() > si.employed())
                {
                    if (si.blueprint().instancesSize() <= 0)
                        throw new RuntimeException(si.blueprint().key + " " + si.neededWorkers() + " " + si.employed());
                    int i = RND.rInt(si.blueprint().instancesSize());
                    for (int k = 0; k < si.blueprint().instancesSize(); k++)
                    {
                        RoomInstance in = si.blueprint().getInstance((i + k) % si.blueprint().instancesSize());
                        if (in.active() && in.employees().employed() < in.employees().target())
                        {
                            STATS.WORK().EMPLOYED.set(h, in);
                            return true;
                        }
                    }
                    LOG.err("no!" + " " + si.neededWorkers() + " " + si.employed() + " " + si.blueprint().instancesSize());
                    for (int k = 0; k < si.blueprint().instancesSize(); k++)
                    {
                        RoomInstance in = si.blueprint().getInstance((i + k) % si.blueprint().instancesSize());
                        LOG.err(k + " " + in.active() + " " + in.employees().employed() + " " + in.employees().target() + " " + in.employees().hardTarget() + " " + si.getFill() + " " + i);
                    }
                }
                return false;
            }

            public int employable()
            {
                return si.neededWorkers() - si.employed();
            }
        }
    }
}