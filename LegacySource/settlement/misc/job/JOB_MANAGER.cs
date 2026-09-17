using System;

namespace settlement.misc.job
{
    public interface JOB_MANAGER
    {
        public SETT_JOB GetReservableJob(COORDINATE prefered);

        public SETT_JOB ReportResourceMissing(RBIT resourceMask, int jx, int jy);
        public void ReportResourceFound(RESOURCE res);
        public bool ResourceReachable(RESOURCE res);
        public bool ResourceShouldSearch(RESOURCE res);
        public void ResetResourceSearch();
        /**
         * Get a job that might be reserved
         * @param c
         * @return
         */
        public SETT_JOB GetJob(COORDINATE c);

        public interface JOB_GETTER
        {

            public SETT_JOB Init(int tx, int ty);

        }

    }

}