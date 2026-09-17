using System;
using System.Collections.Generic;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.service.food.eatery;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.market
{
    [Serializable]
    internal class MarketInstance : RoomInstance, JOBMANAGER_HASER, RoomDistributionIns
    {
        private static readonly long serialVersionUID = -7063521835843676015L;

        public bool autoE = true;
        private readonly JobIterator jobs;
        public readonly RoomServiceInstance service;
        public readonly InstanceData distData;

        public MarketInstance(ROOM_MARKET p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            int maxAmount = (int)blueprintI().constructor.storage.get(this);
            jobs = new JobIterator(this)
            {
                Init = (tx, ty) => blueprintI().dist.job(tx, ty)
            };
            jobs.SetAlwaysNewJob();

            int m = 0;
            foreach (COORDINATE c in body())
            {
                if (Is(c) && blueprintI().constructor.isCrate(c.x(), c.y()))
                {
                    m++;
                }
            }
            service = new RoomServiceInstance(m, blueprintI().service);
            employees().maxSet(m);
            employees().neededSet((int)Math.Ceiling(blueprintI().constructor.workers.get(this)));
            distData = p.dist.makeData(maxAmount);
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            base.render(r, shadowBatch, it);
            it.lit();
            return false;
        }

        protected override void updateAction(double ds, bool day)
        {
            if (day)
                service.updateDay();
            jobs.searchAgain();
            if (!active() || employees().employed() <= 0)
                return;
        }

        protected override void dispose()
        {
            service.dispose(blueprintI().service);
            distributionNlueData().dispose(this);
        }

        public override ROOM_MARKET blueprintI()
        {
            return (ROOM_MARKET)blueprint();
        }

        public override RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return null;
        }

        public override TILE_STORAGE storage(int tx, int ty)
        {
            return null;
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }

        public RoomState makeState(int tx, int ty, bool broken)
        {
            return distributionNlueData().makeState(this, broken);
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