using System;

namespace Settlement.Entity.Humanoid.AI.Work
{
    using Init.Resources;
    using Settlement.Entity.Humanoid;
    using Settlement.Entity.Humanoid.AI.Main;
    using Settlement.Misc.Job;
    using Settlement.Room.Main;
    using Settlement.Stats;

    abstract class PlanWork : AIPLAN.PLANRES
    {
        public PlanWork(string key) : base(key)
        {
        }

        static RoomInstance Work(Humanoid a)
        {
            return STATS.WORK().EMPLOYED.Get(a);
        }

        bool ShouldWork(Humanoid a, AIManager d)
        {
            return Work(a) != null;
        }

        bool HasEmployment(Humanoid a, AIManager d)
        {
            RoomInstance roomInstance = Work(a);
            return roomInstance != null && roomInstance.Active() && !roomInstance.Employees.IsOverstaffed();
        }

        SETT_JOB JobGet(Humanoid a, AIManager d)
        {
            if (ShouldWork(a, d))
            {
                SETT_JOB j = ((JOBMANAGER_HASER)Work(a)).GetWork().GetJob(d.PlanTile);
                return j;
            }
            return null;
        }

        bool JobIsReservedAndReserve(Humanoid a, AIManager d, RESOURCE r)
        {
            if (ShouldWork(a, d))
            {
                SETT_JOB j = ((JOBMANAGER_HASER)Work(a)).GetWork().GetJob(d.PlanTile);
                if (j != null)
                {
                    if (j.JobReservedIs(r))
                        return true;
                    if (j.JobReserveCanBe())
                    {
                        if (j.JobResourceBitToFetch() == null)
                        {
                            j.JobReserve(null);
                            return JobGet(a, d) != null;
                        }
                        else if (r != null && r.Bit.Has(j.JobResourceBitToFetch()))
                        {
                            j.JobReserve(r);
                            return JobGet(a, d) != null;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        void JobCancel(Humanoid a, AIManager d, RESOURCE r)
        {
            if (ShouldWork(a, d))
            {
                Room room = Work(a);
                if (room == null)
                    return;
                if (room != null && room is JOBMANAGER_HASER)
                {
                    SETT_JOB j = ((JOBMANAGER_HASER)room).GetWork().GetJob(d.PlanTile);
                    if (j != null && j.JobReservedIs(r))
                        j.JobReserveCancel(r);
                }
            }
        }

        protected override string Debug(Humanoid a, AIManager d)
        {
            return base.Debug(a, d) + " w: " + Work(a);
        }
    }
}