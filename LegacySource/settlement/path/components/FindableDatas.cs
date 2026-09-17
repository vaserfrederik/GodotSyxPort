using System;
using System.Collections.Generic;
using static Settlement.Main.SETT;
using static Settlement.Main.SETT.Entities;
using static Settlement.Main.SETT.Jobs;
using static Settlement.Main.SETT.Path;
using static Settlement.Main.SETT.Rooms;
using static Settlement.Main.SETT.THeight;
using static Settlement.Main.SETT.TWidth;
using Init.Resources;
using Settlement.Entity;
using Settlement.Entity.Animal;
using Settlement.Entity.Humanoid;
using Settlement.Job;
using Settlement.Main;
using Settlement.Misc.Util;
using Settlement.Path.Finders;
using Settlement.Room.Home;
using Settlement.Room.Main;
using Settlement.Thing;
using Settlement.Thing.ThingsResources;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Sets;
using Util;
using Util.Data;

namespace Settlement.Path.Components
{
    public sealed class FindableDatas
    {
        public readonly FindableDataRes resScattered;
        public readonly FindableDataRes resCrate;
        public readonly FindableDataRes resPriority;
        public readonly FindableDataRes storage;
        public readonly FindableDataRes maintenanceRes;
        public readonly FindableDataSingle maintenance;
        public readonly FindableDataRes jobs;
        public readonly List<FindableDataRes> RESSES;

        private readonly FindableDataSingle[] findable;

        public readonly FindableDataSingle job;
        public readonly FindableDataSingle jobHarvest;
        private readonly FindableDataSingle[] people;
        public readonly FindableDataSingle reservableAnimals;
        public readonly FindableDataHome home;
        public readonly List<FindableDataSingle> SINGLES;

        public FindableDatas()
        {
            FindableData.Datao = new DataOSimple<SComponent>()
            {
                Data = t => t.fdata
            };
            FindableData.All.Clear();
            FindableDataSingle.All.Clear();
            FindableDataRes.All.Clear();

            {
                findable = new FindableDataSingle[SFinderFindable.All().Count];
                for (int i = 0; i < findable.Length; i++)
                    findable[i] = new FindableDataSingle(SFinderFindable.All()[i].Name);
            }

            resScattered = new FindableDataRes("R");
            resCrate = new FindableDataRes("Crate");
            resPriority = new FindableDataRes("Prio");
            storage = new FindableDataRes("Store");

            maintenanceRes = new FindableDataRes("Ma");
            maintenance = new FindableDataSingle("Maintain");
            jobs = new FindableDataRes("jobs");
            job = new FindableDataSingle("Job");

            jobHarvest = new FindableDataSingle("Job Harvest");
            people = new FindableDataSingle[] {
                new FindableDataSingle("Friendlies"),
                new FindableDataSingle("Enemies"),
            };
            reservableAnimals = new FindableDataSingle("Animals");
            home = new FindableDataHome();
            RESSES = new List<FindableDataRes>(FindableDataRes.All);
            SINGLES = new List<FindableDataSingle>(FindableDataSingle.All);
        }

        public FindableDataSingle People(bool friend)
        {
            return people[friend ? 0 : 1];
        }

        FindableDataSingle Service(SFinderFindable f)
        {
            return findable[f.Index];
        }

        void InitComponent0(SComp0 c, Rectangle tiles)
        {
            c.ClearData();

            int tx1 = tiles.X1() - (tiles.X1() > 0 ? 1 : 0);
            int tx2 = tiles.X2() + (tiles.X2() < TWidth ? 1 : 0);
            int ty1 = tiles.Y1() - (tiles.Y1() > 0 ? 1 : 0);
            int ty2 = tiles.Y2() + (tiles.Y2() < THeight ? 1 : 0);

            for (int y = ty1; y < ty2; y++)
            {
                for (int x = tx1; x < tx2; x++)
                {
                    if (!Is(c, x, y))
                        continue;

                    foreach (var t in THINGS().Get(x, y))
                    {
                        if (t is ScatteredResource)
                        {
                            var rw = (ScatteredResource)t;
                            if (!rw.FindableReservedCanBe())
                                continue;
                            resScattered.Add(c, rw.Resource());
                        }
                        else if (t is ThingFindable)
                        {
                            var ti = (ThingFindable)t;
                            if (ti.FindableReservedCanBe())
                                findable[ti.Finder().Index].Add(c);
                        }
                    }

                    foreach (var ent in ENTITIES().GetAtTile(x, y))
                    {
                        if (ent is Humanoid)
                        {
                            var a = (Humanoid)ent;
                            People(!a.Indu().Hostile()).Add(c);
                        }
                        else if (ent is Animal)
                        {
                            if (((Animal)ent).HuntReservable())
                                reservableAnimals.Add(c);
                        }
                    }

                    if (SETT.PATH().Finders().Water.GetReservable(x, y) != null)
                        findable[SETT.PATH().Finders().Water.Index].Add(c);
                    else if (PATH().Finders.Indoor.GetReservable(x, y) != null)
                        findable[SETT.PATH().Finders().Indoor.Index].Add(c);

                    Job j = JOBS().Getter.Get(x, y);
                    if (j != null && j.JobReserveCanBe())
                    {
                        RESOURCE r = j.ResourceCurrentlyNeeded();
                        if (r != null)
                        {
                            jobs.Add(c, j.ResourceCurrentlyNeeded());
                        }
                        else if (j.NeedsRipe())
                            jobHarvest.Add(c);
                        else
                            job.Add(c);
                    }

                    if (SETT.MAINTENANCE().Reservable.Is(x, y))
                    {
                        RESOURCE res = SETT.MAINTENANCE().Resource.Get(x, y);
                        if (res != null)
                        {
                            maintenanceRes.Add(c, res);
                        }
                        else
                        {
                            maintenance.Add(c);
                        }
                    }

                    Room i = ROOMS().Map.Get(x, y);
                    if (i == null)
                        continue;

                    {
                        SFinderFindable se = i.Blueprint().Service(x, y);

                        if (se != null)
                        {
                            FINDABLE t = se.GetReservable(x, y);

                            if (t != null)
                                findable[se.Index].Add(c);
                        }
                    }

                    {
                        RESOURCE_TILE r = i.ResourceTile(x, y);
                        if (r != null && r.FindableReservedCanBe())
                        {
                            if (!r.IsFindable())
                            {

                            }
                            else if (r.IsPrio())
                                resPriority.Add(c, r.Resource());
                            else if (r.IsStorage())
                                resCrate.Add(c, r.Resource());
                            else
                                resScattered.Add(c, r.Resource());
                        }
                    }

                    {
                        TILE_STORAGE s = i.Storage(x, y);
                        if (s != null && s.StorageIsFindable() && s.Resource() != null && s.StorageReservable() > 0)
                            storage.Add(c, s.Resource());
                    }

                    {
                        HomeInstance h = SETT.ROOMS().HOME.Service.Get(x, y);
                        if (h != null)
                        {
                            if (h.Availability() != null)
                            {
                                home.Add(c, h.Availability());
                            }
                        }
                    }

                }
            }
        }

        void InitComponentN(SCompN c)
        {
            c.ClearData();
            c.EdgeMask = 0;

            GUTIL.Filler().Init(this);
            GUTIL.Filler().Fill(c.CentreX(), c.CentreY());

            SComponentLevel l = SETT.PATH().Comps.All.Get(c.Level().Level() - 1);

            while (GUTIL.Filler().HasMore())
            {
                Coordinate coo = GUTIL.Filler().Poll();
                SComponent s = l.Get(coo);
                c.EdgeMask |= s.HasEdge() ? 1 : 0;
                c.EdgeMask |= s.HasEntry() ? 2 : 0;
                foreach (var d in FindableData.All)
                {
                    if (d.Get(s) > 0)
                    {
                        d.Add(c);
                    }
                }

                SComponentEdge e = s.EdgeFirst();
                while (e != null)
                {
                    if (e.To().SuperComp() == c)
                        GUTIL.Filler().Fill(e.To().CentreX(), e.To().CentreY());
                    e = e.Next();
                }

            }

            GUTIL.Filler().Done();
        }

        private bool Is(SComponent c, int tx, int ty)
        {
            foreach (var d in DIR.ORTHO)
                if (c.Is(tx, ty, d))
                    return true;
            return c.Is(tx, ty);
        }
    }
}