using System;
using settlement.misc.job;
using settlement.room.main;
using snake2d;
using util.rendering;

namespace settlement.room.military.training.barracks
{
    [Serializable]
    final class BarracksInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly JobIterator jobs;

        public BarracksInstance(ROOM_BARRACKS b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    BarracksThing t = b.thing.init(c.x(), c.y());
                    if (t != null)
                        am++;
                }
            }

            employees().maxSet(am);
            employees().neededSet(am);

            jobs = new Jobs(this);

            activate();
        }

        private class Jobs : JobIterator
        {
            public Jobs(RoomInstance ins) : base(ins)
            {
                // TODO Auto-generated constructor stub
            }

            private static readonly long serialVersionUID = 1L;

            protected override SETT_JOB init(int tx, int ty)
            {
                return ((ROOM_BARRACKS)ins().blueprintI()).thing.init(tx, ty);
            }
        }

        public override ROOM_BARRACKS blueprintI()
        {
            return (ROOM_BARRACKS)blueprint();
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            jobs.searchAgain();
        }

        protected override void dispose()
        {
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }
    }
}