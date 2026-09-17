using System;
using System.IO;
using settlement.main;
using init.resources;
using settlement.misc.job;
using settlement.room.main;
using snake2d.util.datatypes;
using util;

namespace settlement.room.main.job
{
    [Serializable]
    public abstract class JobIterator : JOB_MANAGER, ISerializable
    {
        protected readonly RoomInstance ins;
        private readonly Coo search = new Coo();
        private bool hasSearchedAll;
        private readonly RBITImp resSearch = new RBITImp();
        private readonly RBITImp resNotFound = new RBITImp();
        private bool alwaysNew = false;
        private bool randomize = false;

        private const long serialVersionUID = -6055758404619685045L;

        public JobIterator(RoomInstance ins)
        {
            this.ins = ins;
            search.Set(
                ins.body().x1() + RND.rInt(ins.body().width()),
                ins.body().y1() + RND.rInt(ins.body().height()));
        }

        public void SetAlwaysNewJob()
        {
            alwaysNew = true;
        }

        public void Randomize()
        {
            randomize = true;
        }

        public void RandomizeN()
        {
            randomize = false;
        }

        public override SETT_JOB ReportResourceMissing(RBIT resMask, int jx, int jy)
        {
            resSearch.Or(resMask);
            resNotFound.Or(resMask);
            return GetReservableJob();
        }

        public override void ReportResourceFound(RESOURCE res)
        {
            resSearch.Clear(res);
            resNotFound.Clear(res);
        }

        private bool ResourceCheck(SETT_JOB j)
        {
            if (j.jobResourceBitToFetch() == null)
            {
                return true;
            }
            if (!PATH().finders.resource.normal.Has(j.jobCoo().x(), j.jobCoo().y(), j.jobResourceBitToFetch()))
            {
                resSearch.Or(j.jobResourceBitToFetch());
                resNotFound.Or(j.jobResourceBitToFetch());
                return false;
            }
            return !resSearch.HasAll(j.jobResourceBitToFetch());
        }

        public override bool ResourceReachable(RESOURCE res)
        {
            return !resNotFound.Has(res.bit);
        }

        public override bool ResourceShouldSearch(RESOURCE res)
        {
            return !resSearch.Has(res.bit);
        }

        private SETT_JOB GetReservableJob()
        {
            if (hasSearchedAll)
                return null;
            if (randomize)
                search.Set(
                    ins.body().x1() + RND.rInt(ins.body().width()),
                    ins.body().y1() + RND.rInt(ins.body().height()));

            int tiles = ins().Area();
            for (int i = 0; i < tiles; i++)
            {
                if (ins.Is(search))
                {
                    SETT_JOB j = Reservable(search.x(), search.y());
                    if (j != null && ResourceCheck(j))
                    {
                        if (alwaysNew)
                            IncSearch();
                        return j;
                    }
                }
                IncSearch();
            }
            hasSearchedAll = true;
            return null;
        }

        private SETT_JOB Reservable(int tx, int ty)
        {
            if (!ins.Is(tx, ty))
                return null;
            SETT_JOB j = Init(tx, ty);
            if (j == null)
                return null;
            if (!j.jobReserveCanBe())
                return null;
            if (!ResourceCheck(j))
            {
                return null;
            }
            return j;
        }

        public override SETT_JOB GetReservableJob(COORDINATE prefered)
        {
            if (alwaysNew || prefered == null)
                return GetReservableJob();
            int tx = prefered.x();
            int ty = prefered.y();

            SETT_JOB j = Reservable(tx, ty);
            if (j == null)
                j = GetReservableAdjacentJob(tx, ty);
            if (j == null)
                return GetReservableJob();
            return j;
        }

        private void IncSearch()
        {
            do
            {
                search.Increment(1, 0);
                if (search.x() >= ins.body().x2())
                {
                    search.Increment(0, 1);
                    search.xSet(ins.body().x1());
                    if (search.y() >= ins.body().y2())
                        search.ySet(ins.body().y1());
                }
            } while (!ins.Is(search));
        }

        public bool HasSearchedAll()
        {
            return hasSearchedAll;
        }

        public override SETT_JOB GetJob(COORDINATE c)
        {
            if (ins.Is(c))
            {
                return Init(c.x(), c.y());
            }
            return null;
        }

        protected abstract SETT_JOB Init(int tx, int ty);

        public void SearchAgain()
        {
            resSearch.Clear();
            this.hasSearchedAll = false;
        }

        public override void ResetResourceSearch()
        {
            resSearch.Clear();
            resNotFound.Clear();
        }

        public void SearchAgainWithoutResources()
        {
            this.hasSearchedAll = false;
        }

        private SETT_JOB GetReservableAdjacentJob(int tx, int ty)
        {
            if (hasSearchedAll)
                return null;

            if (alwaysNew)
                return null;

            int i = 1;
            while (GUTIL.circle().Radius(i) < 5)
            {
                SETT_JOB j = Reservable(tx + GUTIL.circle().Get(i).x(), ty + GUTIL.circle().Get(i).y());
                if (j != null)
                    return j;
                i++;
            }
            return null;
        }

        public void DontSearch()
        {
            this.hasSearchedAll = true;
        }

        protected RoomInstance Ins()
        {
            return ins;
        }

        protected JobIterator(SerializationInfo info, StreamingContext context)
        {
            ins = (RoomInstance)info.GetValue("ins", typeof(RoomInstance));
            search = (Coo)info.GetValue("search", typeof(Coo));
            hasSearchedAll = info.GetBoolean("hasSearchedAll");
            resSearch = (RBITImp)info.GetValue("resSearch", typeof(RBITImp));
            resNotFound = (RBITImp)info.GetValue("resNotFound");
            alwaysNew = info.GetBoolean("alwaysNew");
            randomize = info.GetBoolean("randomize");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ins", ins);
            info.AddValue("search", search);
            info.AddValue("hasSearchedAll", hasSearchedAll);
            info.AddValue("resSearch", resSearch);
            info.AddValue("resNotFound", resNotFound);
            info.AddValue("alwaysNew", alwaysNew);
            info.AddValue("randomize", randomize);
        }
    }
}