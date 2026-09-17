using System;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.knowledge.university
{
    [Serializable]
    public class UniversityInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        public readonly Jobs jobs;

        protected UniversityInstance(ROOM_UNIVERSITY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);
            employees().neededSet(jobs.size());
            employees().maxSet(jobs.size());
            jobs.randomize();
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
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                }
            }
        }

        public ROOM_UNIVERSITY blueprintI()
        {
            return (ROOM_UNIVERSITY) blueprint();
        }

        public class Jobs : JobPositions<UniversityInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(UniversityInstance ins) : base(ins)
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