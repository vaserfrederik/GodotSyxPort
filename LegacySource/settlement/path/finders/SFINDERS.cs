using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Path.Finders
{
    public class SFINDERS
    {
        public readonly SFinderResources resource = new SFinderResources();
        public readonly SFinderResourceStorage storage = new SFinderResourceStorage();
        public readonly SFinderWater water = new SFinderWater();
        public readonly SFinderUnreachable reachable = new SFinderUnreachable();
        public readonly SFinderIndoors indoor = new SFinderIndoors();
        public readonly SFinderJob job = new SFinderJob();
        public readonly SFinderEntry entryPoints = new SFinderEntry();
        public readonly SFinderRND randomDistanceAway = new SFinderRND();
        public readonly FinderArround arround = new FinderArround();
        public readonly SFinderResourceStore jobStore = new SFinderResourceStore();
        public readonly SFinderPrey prey = new SFinderPrey();
        public readonly SFinderHumanoid otherHumanoid = new SFinderHumanoid();
        public readonly SFinderEntity entity = new SFinderEntity();
        public readonly Rnd rndCoo = new Rnd();
        public readonly SFinderHumanTarget target = new SFinderHumanTarget();
        public readonly SFinderHome home = new SFinderHome();
        public readonly SFinderMaintenance maintenance = new SFinderMaintenance();
        private SPathFinder finder;
        private readonly SFindersUpdater uper = new SFindersUpdater();

        private readonly SFinderSoldierManning[] soldierManning = new SFinderSoldierManning[] {
            new SFinderSoldierManning(true),
            new SFinderSoldierManning(false),
        };

        public SFINDERS()
        {
            if (S.Get().Developer)
                new Tests(this);
        }

        public SPathFinder Finder()
        {
            if (finder == null)
                finder = new SPathFinder(SETT.PATH().comps, GUTIL.PathTools(), 2);
            return finder;
        }

        public readonly SFinderMisc resourceDump = new SFinderMisc(25)
        {
            public override bool IsTile(int tx, int ty)
            {
                if (ROOMS().Map.Is(tx, ty))
                    return false;
                if (JOBS().Getter.Is(tx, ty))
                    return false;
                return true;
            }
        };

        public void Update(double ds)
        {
            jobStore.Update(ds);
            job.Update(ds);
            uper.Update(ds);
        }

        public SAVABLE Saver = new SAVABLE()
        {
            public override void Save(FilePutter file)
            {
                uper.Saver.Save(file);
            }

            public override void Load(FileGetter file)
            {
                uper.Saver.Load(file);
            }

            public override void Clear()
            {
                uper.Saver.Clear();
            }
        };

        public SFinderSoldierManning Manning(Army a)
        {
            return soldierManning[a.Index()];
        }

        public readonly SFinderRequest.FinderIdle getOutofWay = new SFinderRequest.FinderIdle();

        public readonly FinderThing<Corpse> corpses = new FinderThing<Corpse>("corpse")
        {
            public override Corpse GetReservable(int tx, int ty)
            {
                foreach (Thing t in SETT.THINGS().Get(tx, ty))
                    if (t is Corpse)
                    {
                        Corpse c = (Corpse)t;
                        if (c.FindableReservedCanBe())
                            return c;
                    }
                return null;
            }

            public override Corpse GetReserved(int tx, int ty)
            {
                foreach (Thing t in SETT.THINGS().Get(tx, ty))
                    if (t is Corpse)
                    {
                        Corpse c = (Corpse)t;
                        if (c.FindableReservedIs())
                            return c;
                    }
                return null;
            }
        };

        public bool IsGoodTileToStandOn(int tx, int ty, ENTITY e)
        {
            if (PATH().Availability.Get(tx, ty).Player <= 0)
                return false;
            if (PATH().Availability.Get(tx, ty).Player >= 2)
                return false;
            if (JOBS().Getter.Is(tx, ty))
                return false;
            if (THINGS().GetFirst(tx, ty) != null)
                return false;
            if (ENTITIES().HasAtTile(e, tx, ty))
                return false;
            if (PATH().Huristics.Getter.Get(tx, ty) > 0.2)
                return false;
            return true;
        }
    }
}