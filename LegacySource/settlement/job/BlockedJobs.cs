using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Job
{
    public class BlockedJobs : SAVABLE
    {
        private readonly BlockedJob[] all = new BlockedJob[16];
        private readonly List<BlockedJob> active = new List<BlockedJob>(all.Length);
        private readonly List<BlockedJob> free = new List<BlockedJob>(all.Length);
        private int upI = -1;
        private readonly IUpdater uper = new IUpdater(SETT.TAREA, TIME.SecondsPerDay())
        {
            Update = (i, timeSinceLast) =>
            {
                if (free.Count == 0)
                    return;

                Job job = SETT.JOBS().Getter.Get(i);
                if (job == null)
                    return;

                if (job is JobClear)
                {
                    return;
                }

                int tx = i % SETT.TWIDTH;
                int ty = i / SETT.THEIGHT;
                if (SETT.PATH().Reachability.Is(tx, ty))
                    return;

                if (SETT.JOBS().State.Get(tx, ty) == State.DORMANT)
                    return;

                if (job.JobResourceBitToFetch() != null && !job.JobResourceBitToFetch().IsClear() && !SETT.PATH().Finders.Resource.Has(THRONE.Coo().X, THRONE.Coo().Y, job.JobResourceBitToFetch()))
                    return;

                foreach (BlockedJob j in active)
                {
                    if (j.Blocked.IsSameAs(tx, ty))
                        return;
                }

                Flooder f = GUTIL.Flooder();

                f.Init(this);
                f.PushSloppy(tx, ty, 0);
                while (f.HasMore())
                {
                    PathTile t = GUTIL.Flooder().PollSmallest();

                    if (t.Value > 16)
                        break;

                    if (SETT.PATH().Reachability.Is(t))
                    {
                        BlockedJob j = free[free.Count - 1];
                        free.RemoveAt(free.Count - 1);
                        j.Blocked.Set(tx, ty);
                        j.Coo.Set(t);
                        active.Add(j);
                        f.Done();
                        return;
                    }

                    foreach (DIR d in DIR.ALL)
                    {
                        if (SETT.IN_BOUNDS(t, d))
                        {
                            f.PushSmaller(t, d, t.Value + d.TileDistance());
                        }
                    }
                }

                f.Done();
            }
        };

        void Render(Renderer r, ShadowBatch shadowBatch, RenderData data)
        {
        }

        public BlockedJobs()
        {
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = new BlockedJob(i);
                free.Add(all[i]);
            }
            while (free.HasRoom())
                free.Add(new BlockedJob(free.Count));

            IDebugPanelSett.Add("BLOCKED_JOB PERFORM", new ACTION()
            {
                Exe = () =>
                {
                    BlockedJob j = Next();
                    LOG.Ln(j);
                    if (j != null)
                    {
                        LOG.Ln(j.Coo + "  " + j.Blocked);

                        RBIT bb = j.JobResourceBitToFetch();
                        RESOURCE res = null;
                        if (bb != null)
                        {
                            foreach (RESOURCE r in RESOURCES.ALL())
                                if (bb.Has(r))
                                {
                                    res = r;
                                    break;
                                }
                        }

                        if (j.JobReserveCanBe())
                            j.JobReserve(res);
                        j.JobPerform(null, res, 1);
                    }
                }
            });
        }

        public BlockedJob Next()
        {
            if (upI != GAME.UpdateI())
            {
                foreach (BlockedJob j in active)
                {
                    if (!IsActive(j, true))
                    {
                        Job job = SETT.JOBS().Getter.Get(j.Blocked);
                        if (job != null)
                            job.JobReserveCancel(null);

                        active.Remove(j);
                        free.Add(j);
                        return null;
                    }
                    else if (j.JobReserveCanBe())
                        return j;
                }
                upI = GAME.UpdateI();
            }
            return null;
        }

        public BlockedJob GetByRef(int id)
        {
            BlockedJob j = all[id];
            if (IsActive(j, false))
                return j;
            return null;
        }

        bool IsActive(BlockedJob j, bool res)
        {
            Job job = SETT.JOBS().Getter.Get(j.Blocked);
            if (job == null)
                return false;
            if (!job.BecomesSolid())
                return false;

            if (SETT.PATH().Reachability.Is(j.Blocked))
                return false;
            if (SETT.JOBS().State.Get(j.Blocked) == State.DORMANT)
                return false;
            if (!SETT.PATH().Reachability.Is(j.Coo))
                return false;
            if (res)
            {
                if (job.JobResourceBitToFetch() != null && !job.JobResourceBitToFetch().IsClear() && !SETT.PATH().Finders.Resource.Has(THRONE.Coo().X, THRONE.Coo().Y, job.JobResourceBitToFetch()))
                    return false;
            }

            return true;
        }

        void Update(double ds)
        {
            if (free.Count == 0)
                return;
            uper.Update(ds);
        }

        public sealed class BlockedJob : SETT_JOB
        {
            private readonly Coo coo = new Coo();
            private readonly Coo blocked = new Coo();
            public readonly int ID;

            private BlockedJob(int id)
            {
                this.ID = id;
            }

            public void JobReserve(RESOURCE r)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    j.JobReserve(r);
            }

            public bool JobReservedIs(RESOURCE r)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    return j.JobReservedIs(r);
                return false;
            }

            public void JobReserveCancel(RESOURCE r)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    j.JobReserveCancel(r);
            }

            public bool JobReserveCanBe()
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    return j.JobReserveCanBe();
                return false;
            }

            public RBIT JobResourceBitToFetch()
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                {
                    return j.JobResourceBitToFetch();
                }
                return null;
            }

            public int JobResourcesNeeded(Humanoid skill)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                {
                    return j.JobResourcesNeeded(skill);
                }
                return 1;
            }

            public double JobPerformTime(Humanoid a)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                {
                    return j.JobPerformTime(a);
                }
                return 1;
            }

            public void JobStartPerforming()
            {
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                {
                    return j.JobPerform(skill, r, rAm);
                }
                return null;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }

            public string JobName()
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    return j.JobName().ToString();
                return Dic.Empty.ToString();
            }

            public bool JobUseTool()
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    return j.JobUseTool();
                return false;
            }

            public SoundRace JobSound()
            {
                Job j = SETT.JOBS().Getter.Get(blocked);
                if (j != null)
                    return j.JobSound();
                return null;
            }
        }

        public void Save(FilePutter file)
        {
            uper.Save(file);
            foreach (BlockedJob j in active)
            {
                file.I(j.ID);
                j.Coo.Save(file);
                j.Blocked.Save(file);
                file.Bool(true);
            }
            foreach (BlockedJob j in free)
            {
                file.I(j.ID);
                j.Coo.Save(file);
                j.Blocked.Save(file);
                file.Bool(false);
            }
        }

        public void Load(FileGetter file)
        {
            uper.Load(file);
            active.ClearSloppy();
            free.ClearSloppy();
            for (int i = 0; i < all.Length; i++)
            {
                BlockedJob j = all[file.I()];
                j.Coo.Load(file);
                j.Blocked.Load(file);
                if (file.Bool())
                    active.Add(j);
                else
                    free.Add(j);
            }
        }

        public void Clear()
        {
            active.ClearSloppy();
            free.ClearSloppy();
            foreach (BlockedJob j in all)
                free.Add(j);
        }
    }
}