using System;
using System.Collections.Generic;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using util.rendering;
using util.datatypes;

namespace settlement.room.health.physician
{
    [Serializable]
    final class Instance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        readonly JobPositions<Instance> jobs;
        readonly RoomServiceInstance service;
        bool auto = true;

        protected Instance(ROOM_PHYSICIAN b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new Jobs(this);
            jobs.setAlwaysNew();
            service = new RoomServiceInstance(jobs.size(), blueprintI().data);
            employees().maxSet((int)Math.Ceiling(blueprintI().constructor.workers.get(this)));
            employees().neededSet(employees().max());
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
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
            if (day)
            {
                service.updateDay();
            }
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c))
                    blueprintI().s.dispose(this, c.x(), c.y());
            }
            service.dispose(blueprintI().data);
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, blueprintI().constructor.quality.get(this));
        }

        public override ROOM_PHYSICIAN blueprintI()
        {
            return (ROOM_PHYSICIAN)blueprint();
        }

        private class Jobs : JobPositions<Instance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(Instance ins) : base(ins)
            {
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().s.getJ(tx, ty) != null;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().s.getJ(tx, ty);
            }
        }
    }
}