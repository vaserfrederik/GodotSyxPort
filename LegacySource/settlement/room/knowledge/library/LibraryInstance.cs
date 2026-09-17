using System;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.knowledge.library
{
    [Serializable]
    internal class LibraryInstance : RoomInstance, JOBMANAGER_HASER, ROOM_IDATA_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;
        internal readonly Jobs jobs;
        private long[] pdata;

        protected LibraryInstance(ROOM_LIBRARY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            pdata = blueprint.consumption().makeData();
            jobs = new Jobs(this);

            employees().neededSet((int)Math.Ceiling(jobs.size()));
            employees().maxSet(jobs.size());

            jobs.randomize();
            activate();
        }

        protected override void loadFix()
        {
            pdata = blueprintI().consumption().makeDataFix(pdata);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            blueprintI().consumption().updateRoom(this);
            jobs.searchAgain();
        }

        protected override void activateAction()
        {
            blueprintI().data.incStations(jobs.size());
        }

        protected override void deactivateAction()
        {
            blueprintI().data.incStations(-jobs.size());
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        protected override void dispose()
        {
            blueprintI().consumption().releaseResources(this, this);
            blueprintI().data.incStations(-jobs.size());
        }

        public ROOM_LIBRARY blueprintI()
        {
            return (ROOM_LIBRARY)blueprint();
        }

        public long[] productionData()
        {
            return pdata;
        }

        internal class Jobs : JobPositions<LibraryInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(LibraryInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().job.get(tx, ty);
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().job.get(tx, ty) != null;
            }
        }
    }
}