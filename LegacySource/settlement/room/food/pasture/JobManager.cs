using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Food.Pasture
{
    public class JobManager : JOB_MANAGER
    {
        private PastureInstance ins;
        private static readonly JobManager i = new JobManager();
        private readonly Job job = new Job();
        private readonly JobBaby jobBaby = new JobBaby();
        public static readonly int workTime = 20;
        private readonly Bit reserved = new Bit(0b010);
        private readonly Bit is = new Bit(0b0100);

        public static JobManager Init(PastureInstance ins)
        {
            i.ins = ins;
            return i;
        }

        private SETT_JOB GetReservableJob()
        {
            if (ins.NeedsWork())
            {
                if (FindAvailable())
                {
                    return job;
                }
                return null;
            }

            if (ins.HasLivestockFetch() && ins.SearchForLivestock)
            {
                if (FindAvailable())
                {
                    return jobBaby.Init(job.coo.x, job.coo.y);
                }
                return null;
            }
            return null;
        }

        private bool FindAvailable()
        {
            int tx = ins.Body().x1 + 1 + RND.rInt(ins.Body().width - 2);
            int ty = ins.Body().y1 + 1 + RND.rInt(ins.Body().height - 2);

            int a = ins.Body().width * ins.Body().height;
            for (int i = 0; i < a; i++)
            {
                if (IsAvailable(tx, ty))
                {
                    return true;
                }
                tx++;
                if (tx >= ins.Body().x2)
                {
                    tx = ins.Body().x1 + 1;
                    ty++;
                    if (ty >= ins.Body().y2)
                        ty = ins.Body().y1 + 1;
                }
            }
            GAME.Notify($"{tx} {ty}");
            return false;
        }

        private bool IsAvailable(int tx, int ty)
        {
            if (!ins.Is(tx, ty))
                return false;
            if (SETT.PATH().Availability.Get(tx, ty).player < 0)
                return false;
            SETT_JOB j = job.Init(tx, ty);
            return j != null && j.JobReserveCanBe();
        }

        public override SETT_JOB ReportResourceMissing(RBIT resourceMask, int jx, int jy)
        {
            ins.MissingLivestock = true;
            ins.SearchForLivestock = false;
            return GetReservableJob();
        }

        public override void ReportResourceFound(RESOURCE res)
        {
            // TODO Auto-generated method stub
        }

        public override bool ResourceReachable(RESOURCE res)
        {
            if (res == RESOURCES.LIVESTOCK())
                return !ins.MissingLivestock;
            return true;
        }

        public override SETT_JOB GetReservableJob(COORDINATE prefered)
        {
            if (prefered == null)
                return GetReservableJob();
            else
                return GetReservableAdjacentJob(prefered);
        }

        public override SETT_JOB GetJob(COORDINATE c)
        {
            int tx = c.x;
            int ty = c.y;
            if (!ins.Is(tx, ty))
                return null;
            if (SETT.PATH().Availability.Get(tx, ty).player < 0)
                return null;
            job.Init(tx, ty);
            if (is.Is(job.data))
                return jobBaby.Init(tx, ty);
            return job;
        }

        public SETT_JOB GetJob(int tx, int ty)
        {
            if (!ins.Is(tx, ty))
                return null;
            if (SETT.PATH().Availability.Get(tx, ty).player < 0)
                return null;
            job.Init(tx, ty);
            if (is.Is(job.data))
                return jobBaby.Init(tx, ty);
            return job;
        }

        private SETT_JOB GetReservableAdjacentJob(COORDINATE c)
        {
            if (!ins.NeedsWork())
                return GetReservableJob();
            CircleCooIterator it = GUTIL.Circle();
            int i = RND.rInt(10);
            while (it.Radius(i++) < 5)
            {
                if (RND.OneIn(4))
                {
                    if (IsAvailable(c.x + it.Get(i).x, c.y + it.Get(i).y))
                        return job;
                }
            }
            return null;
        }

        private class JobBaby : SETT_JOB
        {
            private int data = 0;
            private Coo coo = new Coo();

            public JobBaby Init(int tx, int ty)
            {
                coo.Set(tx, ty);
                data = ROOMS().Data.Get(tx, ty);
                return this;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (JobReservedIs(r) || r != RESOURCES.LIVESTOCK())
                    throw new RuntimeException();
                ins.ConsumeALivestockFetch();
                data = reserved.Set(data);
                data = is.Set(data);
                ROOMS().Data.Set(ins, coo, data);
                ins.MissingLivestock = false;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return reserved.Is(data);
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                data = reserved.Clear(data);
                data = is.Clear(data);
                ROOMS().Data.Set(ins, coo, data);
            }

            public override bool JobReserveCanBe()
            {
                return !JobReservedIs(RESOURCES.LIVESTOCK());
            }

            public override RBIT JobResourceBitToFetch()
            {
                return RESOURCES.LIVESTOCK().bit;
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return 0;
            }

            public override void JobStartPerforming()
            {
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                JobReserveCancel(null);
                ins.Work(skill, r, coo);
                return null;
            }

            public override int JobResourcesNeeded(Humanoid skill)
            {
                return 1;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override string JobName()
            {
                return ins.BlueprintI().Employment().verb.ToString();
            }

            public override bool JobUseTool()
            {
                return true;
            }

            public override SoundRace JobSound()
            {
                return null;
            }

            public override bool LongFetch()
            {
                return true;
            }
        }

        private class Job : SETT_JOB
        {
            private int data = 0;
            private Coo coo = new Coo();

            public Job Init(int tx, int ty)
            {
                coo.Set(tx, ty);
                data = ROOMS().Data.Get(tx, ty);
                return this;
            }

            public override void JobReserve(RESOURCE r)
            {
                if (JobReservedIs(r))
                    throw new RuntimeException();
                data = reserved.Set(data);
                data = is.Clear(data);
                ROOMS().Data.Set(ins, coo, data);
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return reserved.Is(data);
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                data = reserved.Clear(data);
                ROOMS().Data.Set(ins, coo, data);
            }

            public override bool JobReserveCanBe()
            {
                return !JobReservedIs(null);
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override double JobPerformTime(Humanoid skill)
            {
                return workTime;
            }

            public override void JobStartPerforming()
            {
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                JobReserveCancel(r);
                ins.Work(skill, r, coo);
                return null;
            }

            public override int JobResourcesNeeded(Humanoid skill)
            {
                return 0;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override string JobName()
            {
                return ins.BlueprintI().Employment().verb.ToString();
            }

            public override bool JobUseTool()
            {
                return true;
            }

            public override SoundRace JobSound()
            {
                return ins.BlueprintI().Employment().Sound();
            }
        }

        public override void ResetResourceSearch()
        {
            ins.MissingLivestock = false;
            ins.SearchForLivestock = true;
        }

        public override bool ResourceShouldSearch(RESOURCE res)
        {
            return ins.SearchForLivestock;
        }
    }
}