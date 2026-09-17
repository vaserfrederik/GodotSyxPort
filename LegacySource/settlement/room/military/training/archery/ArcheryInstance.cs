using System;
using System.Collections.Generic;
using settlement.misc.job;
using settlement.room.main;
using snake2d;
using util.rendering;

namespace settlement.room.military.training.archery
{
    public class ArcheryInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly JobIterator jobs;

        public ArcheryInstance(ROOM_ARCHERY b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    ArcheryThing t = b.thing.init(c.x(), c.y());
                    if (t != null)
                    {
                        am++;
                    }
                }
            }
            jobs = new Jobs(this);
            employees().maxSet(am);
            employees().neededSet(am);

            activate();
        }

        private class Jobs : JobIterator
        {
            public Jobs(RoomInstance ins) : base(ins)
            {
            }

            private static readonly long serialVersionUID = 1L;

            protected override SETT_JOB init(int tx, int ty)
            {
                return ((ROOM_ARCHERY)ins().blueprintI()).thing.init(tx, ty);
            }
        }

        public override ROOM_ARCHERY blueprintI()
        {
            return (ROOM_ARCHERY)blueprint();
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