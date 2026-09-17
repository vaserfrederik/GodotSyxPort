using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace World.Region
{
    public static class RD
    {
        public static RD self;

        private readonly RDBuildings buildings;
        private readonly RDOutputs resources;
        private readonly RDRandom random;
        private readonly RDRaces races;
        private readonly RDMilitary military;
        private readonly RDHealth health;
        private readonly RDDistance distance;
        private readonly RDReligions religion;
        private readonly RDOwner owner;
        private readonly RDDevastation deva;
        private readonly RDEvent event;
        private readonly RDProspects prospects;
        private readonly RDProblem problem;
        private RDUpdater updater;

        private readonly long[][] regionData;
        private readonly long[][] factionData;
        private readonly int[] factionI;

        private readonly Realm[] drea;
        private readonly RDInit init = new RDInit();

        private static readonly string ¤¤regChange = "{0} changes master from {1} to {2}.";

        static RD()
        {
            D.ts(typeof(RD));
        }

        public RD(WREGIONS regions) : base("region Data", "RD")
        {
            self = this;

            distance = new RDDistance(init);
            random = new RDRandom(init);
            health = new RDHealth(init);
            resources = new RDOutputs(init);
            military = new RDMilitary(init);
            races = new RDRaces(init);
            religion = new RDReligions(init);
            buildings = new RDBuildings(init);
            owner = new RDOwner(init);
            deva = new RDDevastation(init);
            event = new RDEvent(init);
            prospects = new RDProspects(init);
            problem = new RDProblem();
            factionI = new int[WREGIONS.MAX];
            Array.Fill(factionI, -1);

            regionData = new long[WREGIONS.MAX][];
            factionData = new long[FACTIONS.MAX()][];
            for (int i = 0; i < WREGIONS.MAX; i++)
                regionData[i] = new long[init.count.longCount()];
            for (int i = 0; i < FACTIONS.MAX(); i++)
                factionData[i] = new long[init.rCount.longCount()];
            drea = Enumerable.Range(0, FACTIONS.MAX()).Select(i => new Realm(i)).ToArray();

            GAME.addOnInit(() =>
            {
                buildings.init(init);
                updater = new RDUpdater();
                foreach (var region in regions.all())
                {
                    updater.update(region, 0);
                }
            });

            IDebugConsole.AddCommand("setcapitol", (string[] args) =>
            {
                if (args.Length == 1)
                {
                    Region region = WORLD.REGIONS().all().FirstOrDefault(r => r.info.name().Equals(args[0], StringComparison.OrdinalIgnoreCase));
                    if (region != null)
                    {
                        setCapitol(region);
                    }
                    else
                    {
                        Console.WriteLine($"Region '{args[0]}' not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Usage: setcapitol <regionName>");
                }
            });
        }

        private class RDInit
        {
            public DataO<Region> count = new DataO<Region>("RDR")
            {
                data = (t) => regionData[t.index()]
            };

            public DataO<Faction> rCount = new DataO<Faction>("RDF")
            {
                data = (t) => factionData[t.index()]
            };

            public LinkedList<RDUpdatable> upers = new LinkedList<RDUpdatable>();
            public LinkedList<ISavable> savable = new LinkedList<ISavable>();
            public RDBuildPoints points;
        }

        public interface RDUpdatable
        {
            void update(Region reg, double time);
            void init(Region reg);
        }

        public interface RDGeneratable
        {
            void generate(Region r);
        }

        public static RDBuildings BUILDINGS()
        {
            return self.buildings;
        }

        public static RDOutputs OUTPUT()
        {
            return self.resources;
        }

        public static RDRandom RAN()
        {
            return self.random;
        }

        public static RDRaces RACES()
        {
            return self.races;
        }

        public static RDMilitary MILITARY()
        {
            return self.military;
        }

        public static RDHealth HEALTH()
        {
            return self.health;
        }

        public static RDDistance DIST()
        {
            return self.distance;
        }

        public static RDReligions RELIGION()
        {
            return self.religion;
        }

        public static RDOwner OWNER()
        {
            return self.owner;
        }

        public static RDUpdater UPDATER()
        {
            return self.updater;
        }

        public static RDDevastation DEVASTATION()
        {
            return self.deva;
        }

        public static RDProspects PROSPECT()
        {
            return self.prospects;
        }

        public static RDProblem PROBLEM()
        {
            return self.problem;
        }

        public static RDEvent EVENT()
        {
            return self.event;
        }

        public static Realm REALM(Region reg)
        {
            if (self.factionI[reg.index()] != -1)
                return self.drea[self.factionI[reg.index()]];
            return null;
        }

        public static Realm REALM(Faction f)
        {
            return self.drea[f.index()];
        }

        private static void removeFaction(Region region)
        {
            Realm rr = REALM(region);

            if (rr == null)
                return;

            self.factionI[region.index()] = -1;

            rr.regions.removeShort((short)region.index());
            if (rr.capitolI == region.index())
            {
                if (rr.regions.size() > 0)
                    rr.capitolI = (short)rr.regions.get(rr.regions.size() - 1);
                else
                    rr.capitolI = -1;
            }
        }

        public static void setFaction(Region region, Faction f, bool log)
        {
            Realm oldRealm = REALM(region);

            if (f != null && REALM(f) == oldRealm)
                return;

            RD.OWNER().ownerI.set(region, (RD.OWNER().ownerI.get(region) + 1) % RD.OWNER().ownerI.max(region));

            Faction fold = region.faction();

            removeFaction(region);

            if (f != null)
            {
                Realm rr = f.realm();
                if (rr.regions.hasRoom())
                {
                    self.factionI[region.index()] = f.index();

                    rr.regions.add((short)region.index());

                    if (rr.capitolI == -1)
                        rr.capitolI = (short)region.index();
                }
                f.realm().ferArea = 0;
                for (int ri = 0; ri < f.realm().regions(); ri++)
                {
                    Region r = WORLD.REGIONS().all().get(ri);
                    f.realm().ferArea += r.info.area() * r.info.moisture();
                }
            }

            if (fold != null)
            {
                fold.realm().ferArea = 0;
                for (int ri = 0; ri < fold.realm().regions(); ri++)
                {
                    Region r = WORLD.REGIONS().all().get(ri);
                    fold.realm().ferArea += r.info.area() * r.info.moisture();
                }
            }

            WORLD.MINIMAP().updateRegion(region);

            RDOwnerChanger.changeI++;
            foreach (var ch in RDOwnerChanger.ownerChanges)
            {
                ch.change(region, fold, f);
            }

            Str.TMP.clear().add(¤¤regChange);
            Str.TMP.insert(0, region.info.name());
            Str.TMP.insert(1, FACTIONS.name(fold));
            Str.TMP.insert(2, FACTIONS.name(f));
            WORLD.LOG().log(fold, f, UI.icons().s.crown, Str.TMP, region.cx(), region.cy());

            if (f == FACTIONS.player())
            {
                foreach (var bu in RD.BUILDINGS().all)
                {
                    if (bu.level.get(region) > 0 && !bu.levels.get(bu.level.get(region)).reqs.passes(region))
                    {
                        bu.level.set(region, 0);
                    }
                }
            }
        }

        public static void clearFaction(FactionNPC faction)
        {
            while (faction.realm().regions() > 0)
                setFaction(faction.realm().region(0), null, false);
        }

        public static void setCapitol(Region region)
        {
            Realm rr = REALM(region);
            if (rr == null)
                throw new InvalidOperationException("Can't set a rebel region as a capitol");

            Region old = region.faction().capitolRegion();

            rr.capitolI = (short)region.index();

            foreach (var ch in RDOwnerChanger.ownerChanges)
            {
                ch.change(region, region.faction(), region.faction());
            }

            rr.regions.swap(0, rr.regions.indexOf((short)region.index()));
            WORLD.MINIMAP().updateRegion(region);
            if (old != null)
                WORLD.MINIMAP().updateRegion(old);
        }

        public static RDRace RACE(Race r)
        {
            return RACES().get(r);
        }

        public abstract class RDOwnerChanger
        {
            public static int changeI;
            static readonly ArrayListGrower<RDOwnerChanger> ownerChanges = new ArrayListGrower<RDOwnerChanger>();

            static RDOwnerChanger()
            {
                new GameDisposable()
                {
                    dispose = () =>
                    {
                        ownerChanges.clear();
                    }
                };
            }

            public RDOwnerChanger()
            {
                ownerChanges.add(this);
            }

            public abstract void change(Region reg, Faction oldOwner, Faction newOwner);
        }
    }
}