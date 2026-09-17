using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Job
{
    [Serializable]
    public abstract class JobPositions<T> : JOB_MANAGER where T : RoomInstance
    {
        protected readonly T ins;
        private bool hasSearchedAll;
        private readonly ArrayCooShort coos;
        private int searchI = 0;
        public readonly RBITImp resNotFound = new RBITImp();
        private readonly RBITImp resSearchmask = new RBITImp();
        private bool alwaysNew;
        private const long serialVersionUID = 2358456270243399328L;

        public JobPositions(T ins)
        {
            this.ins = ins;

            int amount = 0;

            foreach (COORDINATE c in ins.body())
            {
                if (ins.is(c) && isAndInit(c.x(), c.y()))
                    amount++;
            }

            coos = new ArrayCooShort(amount);
            int i = 0;

            foreach (COORDINATE c in ins.body())
            {
                if (ins.is(c) && initIs(c.x(), c.y()))
                {
                    coos.set(i).set(c);
                    i++;
                }
            }

            if (i != amount)
                throw new Exception(i + " " + amount);
        }

        protected abstract bool isAndInit(int tx, int ty);

        protected bool initIs(int tx, int ty)
        {
            return get(tx, ty) != null;
        }

        protected abstract SETT_JOB get(int tx, int ty);

        public override SETT_JOB reportResourceMissing(RBIT resMask, int jx, int jy)
        {
            resNotFound.or(resMask);
            resSearchmask.or(resMask);
            return getReservableJob();
        }

        public override void reportResourceFound(RESOURCE res)
        {
            resNotFound.clear(res);
            resSearchmask.clear(res);
        }

        public override bool resourceShouldSearch(RESOURCE res)
        {
            return !resSearchmask.has(res);
        }

        public override bool resourceReachable(RESOURCE res)
        {
            return !resNotFound.has(res);
        }

        private bool resourceCheck(SETT_JOB j)
        {
            RBIT f = j.jobResourceBitToFetch();
            if (f == null)
            {
                return true;
            }
            // if (!PATH().finders.resource.normal.has(f))
            //     return false;
            return !resSearchmask.hasAll(f);
        }

        private SETT_JOB getReservableJob()
        {
            if (hasSearchedAll)
                return null;

            if (alwaysNew)
                searchI++;

            for (int i = 0; i < coos.size(); i++)
            {
                if (searchI >= coos.size())
                    searchI = 0;
                coos.set(searchI);

                SETT_JOB j = reservable(coos.get().x(), coos.get().y());
                if (j != null && resourceCheck(j))
                {
                    return j;
                }
                searchI++;
            }

            hasSearchedAll = true;
            return null;
        }

        private SETT_JOB reservable(int tx, int ty)
        {
            if (!ins.is(tx, ty))
                return null;
            SETT_JOB j = get(tx, ty);
            if (j == null)
                return null;
            if (!j.jobReserveCanBe())
                return null;
            return j;
        }

        public override SETT_JOB getReservableJob(COORDINATE pref)
        {
            if (pref == null || alwaysNew)
                return getReservableJob();

            SETT_JOB j = reservable(pref.x(), pref.y());

            if (j == null)
                return getReservableJob();

            if (!resourceCheck(j))
            {
                return getReservableJob();
            }
            return j;
        }

        public override SETT_JOB getJob(COORDINATE c)
        {
            if (ins.is(c))
            {
                return get(c.x(), c.y());
            }
            return null;
        }

        public void searchAgain()
        {
            resSearchmask.clear();
            this.hasSearchedAll = false;
        }

        public override void resetResourceSearch()
        {
            resSearchmask.clear();
            resNotFound.clear();
        }

        public void searchAgainButDontReset()
        {
            this.hasSearchedAll = false;
        }

        public void stopSearching()
        {
            this.hasSearchedAll = true;
        }

        public bool isSearching()
        {
            return !hasSearchedAll;
        }

        public int size()
        {
            return coos.size();
        }

        public COORDINATE get(int i)
        {
            return coos.set(i);
        }

        public void setAlwaysNew()
        {
            alwaysNew = true;
        }

        public void randomize()
        {
            for (int i = 0; i < coos.size(); i++)
            {
                coos.set(i);
                int x = coos.get().x();
                int y = coos.get().y();
                int d = RND.rInt(coos.size());
                coos.set(d);
                int x2 = coos.get().x();
                int y2 = coos.get().y();

                coos.set(i);
                coos.get().set(x2, y2);

                coos.set(d);
                coos.get().set(x, y);
            }
        }
    }
}