using System;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.infra.embassy
{
    [Serializable]
    public class EmbassyInstance : RoomInstance, JOBMANAGER_HASER, ROOM_IDATA_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;
        public Jobs jobs;

        private long[] pdata;

        protected EmbassyInstance(ROOM_EMBASSY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);

            employees().neededSet((int)Math.Ceiling(jobs.size()));
            employees().maxSet(jobs.size());

            blueprint.data.incStations(jobs.size());
            pdata = blueprintI().consumption().makeData();
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            jobs.searchAgain();
            blueprintI().consumption().updateRoom(this);
        }

        protected override void loadFix()
        {
            pdata = blueprintI().consumption().makeDataFix(pdata);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        protected override void dispose()
        {
            blueprintI().data.incStations(-jobs.size());
            blueprintI().consumption().releaseResources(this, this);
        }

        public ROOM_EMBASSY blueprintI()
        {
            return (ROOM_EMBASSY)blueprint();
        }

        public class Jobs : JobPositions<EmbassyInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(EmbassyInstance ins) : base(ins)
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

        public long[] productionData()
        {
            return pdata;
        }

        [Serializable]
        public class Res
        {
            private static readonly long serialVersionUID = 1L;
            public int reserved;
            public int current;
            public bool unreachable = false;
            public bool disabled = true;
        }
    }
}