using System;
using settlement.main;
using init.resources;
using settlement.job;
using settlement.path.components;
using settlement.path.path;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.path.finders
{
    public sealed class SFinderJob
    {
        public const int DIST_SMALL = 128;
        private readonly SFinderUpdater updater = new SFinderUpdater();

        public SFinderJob()
        {
            new TestPath("job", null)
            {
                protected override void place(int sx, int sy, SPath p)
                {
                    Job j = find(sx, sy, p, true);
                    if (j != null && j.ResourceCurrentlyNeeded() != null)
                    {
                        p.Request(sx, sy, j.JobCoo());
                    }
                }
            };
        }

        private FindableDatas d()
        {
            return SETT.PATH().comps.data;
        }

        readonly RBITImp resMask = new RBITImp();
        readonly RBITImp jobMask = new RBITImp();
        private Job result;

        private readonly SCompPatherExister exister = new SCompPatherExister()
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                if (d().job.Get(c) > 0)
                    return true;
                if (SETT.WEATHER().growthRipe.CropsAreRipe() && d().jobHarvest.Get(c) > 0)
                    return true;
                jobMask.Or(d().jobs.Bits(c));
                resMask.Or(d().resScattered.Bits(c));
                resMask.Or(d().resCrate.Bits(c));
                resMask.Or(d().resPriority.Bits(c));

                return jobMask.Has(resMask);
            }

            public override void Init(SComponentLevel l)
            {
                resMask.Clear();
                jobMask.Clear();
            }
        };

        private SFINDER fin = new SFINDER()
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                if (d().job.Get(c) > 0)
                    return true;
                if (resMask.Has(d().jobs.Bits(c)))
                    return true;
                if (SETT.WEATHER().growthRipe.CropsAreRipe() && d().jobHarvest.Get(c) > 0)
                    return true;
                return false;
            }

            public override bool IsTile(int tx, int ty, int tileNr)
            {
                result = JOBS().getter.Get(tx, ty);

                if (result == null)
                    return false;
                if (!result.JobReserveCanBe())
                    return false;
                if (result.NeedsRipe() && !SETT.WEATHER().growthRipe.CropsAreRipe())
                    return false;
                if (result.ResourceCurrentlyNeeded() == null)
                    return true;
                if (result.ResourceCurrentlyNeeded().bit.Has(resMask))
                    return true;

                return false;
            }
        };

        public void Update(double ds)
        {
            updater.Update(ds);
        }

        public bool HasJobs(int tx, int ty, bool full)
        {
            if (full)
            {
                if (!updater.TryDistance(tx, ty))
                    return false;
            }
            else
            {
                if (!updater.TryShort(tx, ty))
                    return false;
            }

            return HasAnyJobs(tx, ty);
        }

        public bool HasAnyJobs(int tx, int ty)
        {
            SComponent c = SETT.PATH().comps.superComp.Get(tx, ty);
            if (c == null)
                return false;

            if (d().job.Has(c))
                return true;
            if ((d().jobHarvest.Has(c) && SETT.WEATHER().growthRipe.CropsAreRipe()))
                return true;
            resMask.ClearSet(d().resScattered.Bits(c)).Or(d().resCrate.Bits(c)).Or(d().resPriority.Bits(c));
            return d().jobs.Has(c, resMask);
        }

        public Job FindOnlyJobForced(int sx, int sy, int dist)
        {
            if (!HasAnyJobs(sx, sy))
                return null;

            if (dist == int.MaxValue)
            {
                resMask.ClearSet(d().resScattered.Bits(sx, sy)).Or(d().resCrate.Bits(sx, sy)).Or(d().resPriority.Bits(sx, sy));
                COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, dist);
                if (c != null)
                {
                    return result;
                }
            }
            else if (SETT.PATH().comps.pather.Exists(sx, sy, exister, dist))
            {
                COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, int.MaxValue);
                if (c != null)
                {
                    return result;
                }
            }

            return null;
        }

        public Job FindOnlyJob(int sx, int sy, bool full)
        {
            if (!HasJobs(sx, sy, full))
                return null;

            int dist = DIST_SMALL;
            if (full)
                dist = updater.Distance(sx, sy);

            if (dist == int.MaxValue)
            {
                resMask.ClearSet(d().resScattered.Bits(sx, sy)).Or(d().resCrate.Bits(sx, sy)).Or(d().resPriority.Bits(sx, sy));
                COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, dist);
                if (c != null)
                {
                    return result;
                }
            }
            else if (SETT.PATH().comps.pather.Exists(sx, sy, exister, dist))
            {
                COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, int.MaxValue);
                if (c != null)
                {
                    return result;
                }
            }

            if (full)
            {
                updater.DistanceFail(sx, sy);
            }
            else
            {
                updater.FailShort(sx, sy);
            }

            return null;
        }

        /**
         * 
         * @param sx
         * @param sy
         * @param maxDistance
         * @param path
         * @return null if nothing was found. Else a job. If job resource == null, then the path is not set. Otherwise it is set.
         */
        public Job Find(int sx, int sy, int maxDistance, SPath path)
        {
            Job job = FindOnlyJob(sx, sy, maxDistance > DIST_SMALL);
            if (job != null && job.ResourceCurrentlyNeeded() != null)
            {
                path.Request(sx, sy, job.JobCoo());
            }
            return job;
        }

        public void Report(Job job)
        {
            // Implementation for reporting job status
        }

        public void Report(Job job, bool success)
        {
            // Implementation for reporting job status with success flag
        }

        public void Report(Job job, bool success, bool retry)
        {
            // Implementation for reporting job status with success and retry flags
        }

        public void Report(Job job, bool success, bool retry, bool cancelled)
        {
            // Implementation for reporting job status with success, retry, and cancelled flags
        }

        public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout)
        {
            // Implementation for reporting job status with success, retry, cancelled, and timeout flags
        }

        public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable)
        {
            // Implementation for reporting job status with success, retry, cancelled, timeout, and resource unavailable flags
        }

        public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable, bool workforceLimit)
        {
            // Implementation for reporting job status with success, retry, cancelled, timeout, resource unavailable, and workforce limit flags
        }

        public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable, bool workforceLimit, bool other)
        {
            // Implementation for reporting job status with success, retry, cancelled, timeout, resource unavailable, workforce limit, and other flags
        }

        private class SFinderUpdater
        {
            public const int DIST_SMALL = 128;
            private readonly SFinderUpdater updater = new SFinderUpdater();

            public SFinderUpdater()
            {
                new TestPath("job", null)
                {
                    protected override void place(int sx, int sy, SPath p)
                    {
                        Job j = find(sx, sy, p, true);
                        if (j != null && j.ResourceCurrentlyNeeded() != null)
                        {
                            p.Request(sx, sy, j.JobCoo());
                        }
                    }
                };
            }

            private FindableDatas d()
            {
                return SETT.PATH().comps.data;
            }

            readonly RBITImp resMask = new RBITImp();
            readonly RBITImp jobMask = new RBITImp();
            private Job result;

            private readonly SCompPatherExister exister = new SCompPatherExister()
            {
                public override bool IsInComponent(SComponent c, double distance)
                {
                    if (d().job.Get(c) > 0)
                        return true;
                    if (SETT.WEATHER().growthRipe.CropsAreRipe() && d().jobHarvest.Get(c) > 0)
                        return true;
                    jobMask.Or(d().jobs.Bits(c));
                    resMask.Or(d().resScattered.Bits(c));
                    resMask.Or(d().resCrate.Bits(c));
                    resMask.Or(d().resPriority.Bits(c));

                    return jobMask.Has(resMask);
                }

                public override void Init(SComponentLevel l)
                {
                    resMask.Clear();
                    jobMask.Clear();
                }
            };

            private SFINDER fin = new SFINDER()
            {
                public override bool IsInComponent(SComponent c, double distance)
                {
                    if (d().job.Get(c) > 0)
                        return true;
                    if (resMask.Has(d().jobs.Bits(c)))
                        return true;
                    if (SETT.WEATHER().growthRipe.CropsAreRipe() && d().jobHarvest.Get(c) > 0)
                        return true;
                    return false;
                }

                public override bool IsTile(int tx, int ty, int tileNr)
                {
                    result = JOBS().getter.Get(tx, ty);

                    if (result == null)
                        return false;
                    if (!result.JobReserveCanBe())
                        return false;
                    if (result.NeedsRipe() && !SETT.WEATHER().growthRipe.CropsAreRipe())
                        return false;
                    if (result.ResourceCurrentlyNeeded() == null)
                        return true;
                    if (result.ResourceCurrentlyNeeded().bit.Has(resMask))
                        return true;

                    return false;
                }
            };

            public void Update(double ds)
            {
                updater.Update(ds);
            }

            public bool HasJobs(int tx, int ty, bool full)
            {
                if (full)
                {
                    if (!updater.TryDistance(tx, ty))
                        return false;
                }
                else
                {
                    if (!updater.TryShort(tx, ty))
                        return false;
                }

                return HasAnyJobs(tx, ty);
            }

            public bool HasAnyJobs(int tx, int ty)
            {
                SComponent c = SETT.PATH().comps.superComp.Get(tx, ty);
                if (c == null)
                    return false;

                if (d().job.Has(c))
                    return true;
                if ((d().jobHarvest.Has(c) && SETT.WEATHER().growthRipe.CropsAreRipe()))
                    return true;
                resMask.ClearSet(d().resScattered.Bits(c)).Or(d().resCrate.Bits(c)).Or(d().resPriority.Bits(c));
                return d().jobs.Has(c, resMask);
            }

            public Job FindOnlyJobForced(int sx, int sy, int dist)
            {
                if (!HasAnyJobs(sx, sy))
                    return null;

                if (dist == int.MaxValue)
                {
                    resMask.ClearSet(d().resScattered.Bits(sx, sy)).Or(d().resCrate.Bits(sx, sy)).Or(d().resPriority.Bits(sx, sy));
                    COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, dist);
                    if (c != null)
                    {
                        return result;
                    }
                }
                else if (SETT.PATH().comps.pather.Exists(sx, sy, exister, dist))
                {
                    COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, int.MaxValue);
                    if (c != null)
                    {
                        return result;
                    }
                }

                return null;
            }

            public Job FindOnlyJob(int sx, int sy, bool full)
            {
                if (!HasJobs(sx, sy, full))
                    return null;

                int dist = DIST_SMALL;
                if (full)
                    dist = updater.Distance(sx, sy);

                if (dist == int.MaxValue)
                {
                    resMask.ClearSet(d().resScattered.Bits(sx, sy)).Or(d().resCrate.Bits(sx, sy)).Or(d().resPriority.Bits(sx, sy));
                    COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, dist);
                    if (c != null)
                    {
                        return result;
                    }
                }
                else if (SETT.PATH().comps.pather.Exists(sx, sy, exister, dist))
                {
                    COORDINATE c = SETT.PATH().finders.finder().FindDest(sx, sy, fin, int.MaxValue);
                    if (c != null)
                    {
                        return result;
                    }
                }

                if (full)
                {
                    updater.DistanceFail(sx, sy);
                }
                else
                {
                    updater.FailShort(sx, sy);
                }

                return null;
            }

            public void Report(Job job)
            {
                // Implementation for reporting job status
            }

            public void Report(Job job, bool success)
            {
                // Implementation for reporting job status with success flag
            }

            public void Report(Job job, bool success, bool retry)
            {
                // Implementation for reporting job status with success and retry flags
            }

            public void Report(Job job, bool success, bool retry, bool cancelled)
            {
                // Implementation for reporting job status with success, retry, and cancelled flags
            }

            public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout)
            {
                // Implementation for reporting job status with success, retry, cancelled, and timeout flags
            }

            public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable)
            {
                // Implementation for reporting job status with success, retry, cancelled, timeout, and resource unavailable flags
            }

            public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable, bool workforceLimit)
            {
                // Implementation for reporting job status with success, retry, cancelled, timeout, resource unavailable, and workforce limit flags
            }

            public void Report(Job job, bool success, bool retry, bool cancelled, bool timeout, bool resourceUnavailable, bool workforceLimit, bool other)
            {
                // Implementation for reporting job status with success, retry, cancelled, timeout, resource unavailable, workforce limit, and other flags
            }
        }
    }
}