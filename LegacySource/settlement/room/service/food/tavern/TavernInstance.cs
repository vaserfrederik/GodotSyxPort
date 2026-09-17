using System;
using System.Collections.Generic;
using System.Linq;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.food.eatery;
using settlement.room.service.module;
using snake2d;
using util.rendering;
using util.datatypes;

namespace settlement.room.service.food.tavern
{
    [Serializable]
    final class TavernInstance : RoomInstance, JOBMANAGER_HASER, RoomDistributionIns
    {
        private static readonly long serialVersionUID = 1L;
        readonly RoomServiceInstance service;
        readonly Jobs jobs;
        readonly InstanceData distData;
        bool auto = true;

        protected TavernInstance(ROOM_TAVERN b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new Jobs(this);
            jobs.setAlwaysNew();

            int sers = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && b.service(c.x(), c.y()) != null)
                {
                    sers++;
                }
            }
            distData = b.dist.makeData(sers);

            service = new RoomServiceInstance(sers, blueprintI().serviceData);

            employees().maxSet((int)Math.Ceiling(jobs.size() / 2.0));
            employees().neededSet((int)Math.Ceiling(jobs.size() / 4.0));
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
                service.updateDay();
            jobs.searchAgain();
            distData.update(distributionNlueData());
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
            distributionNlueData().dispose(this);
            service.dispose(blueprintI().serviceData);
        }

        public ROOM_TAVERN blueprintI()
        {
            return (ROOM_TAVERN)blueprint();
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, blueprintI().constructor.coziness.get(this));
        }

        [Serializable]
        static class Jobs : JobPositions<TavernInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(TavernInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().dist.job(tx, ty);
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().dist.job(tx, ty) != null;
            }
        }

        public InstanceData distributionData()
        {
            return distData;
        }

        public RoomDistribution distributionNlueData()
        {
            return blueprintI().dist;
        }
    }
}