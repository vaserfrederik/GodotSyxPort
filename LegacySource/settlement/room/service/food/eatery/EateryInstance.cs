using System;
using System.Collections.Generic;
using System.Linq;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.service.food.eatery.RoomDistribution;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.food.eatery
{
    final class EateryInstance : RoomInstance, JOBMANAGER_HASER, RoomDistributionIns
    {
        private static readonly long serialVersionUID = -7063521835843676015L;
        public bool autoE = true;
        private readonly JobIterator jobs;
        public readonly RoomServiceInstance service;
        public readonly InstanceData distData;

        public EateryInstance(ROOM_EATERY p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            int maxAmount = 2 * (int)blueprintI().constructor.storage.Get(this);
            distData = p.dist.MakeData(maxAmount);
            jobs = new JobIterator(this)
            {
                Init = (tx, ty) => blueprintI().dist.Job(tx, ty)
            };
            //jobs.SetAlwaysNewJob();

            int m = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().constructor.IsCrate(c.x(), c.y()))
                    m++;
            }
            service = new RoomServiceInstance(m, blueprintI().service);
            employees().MaxSet(m);
            employees().NeededSet((int)Math.Ceiling(blueprintI().constructor.Workers.Get(this)));
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            base.Render(r, shadowBatch, it);
            it.Lit();
            return false;
        }

        protected override void LoadFix()
        {
            
        }
        
        protected override void UpdateAction(double ds, bool day)
        {
            if (day)
                service.UpdateDay();
            distData.Update(distributionNlueData());
            jobs.SearchAgain();
            if (!Active() || employees().Employed() <= 0)
                return;
            
        }
        
        public override void UpdateTileDay(int tx, int ty)
        {
            
        }
        
        protected override void Dispose()
        {
            
            distributionNlueData().Dispose(this);
            service.Dispose(blueprintI().service);
            
        }
        
        public override ROOM_EATERY BlueprintI()
        {
            return (ROOM_EATERY)blueprint();
        }
        
        public override RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return null;
        }
        
        public override TILE_STORAGE Storage(int tx, int ty)
        {
            return null;
        }

        protected override void ActivateAction()
        {
            // TODO Auto-generated method stub
            
        }

        protected override void DeactivateAction()
        {
            // TODO Auto-generated method stub
            
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        public RoomServiceInstance Service()
        {
            return service;
        }

        public double Quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }
        
        public RoomState MakeState(int tx, int ty, bool broken)
        {
            return distributionNlueData().MakeState(this, broken);
        }

        public InstanceData DistributionData()
        {
            return distData;
        }

        public RoomDistribution DistributionNlueData()
        {
            return blueprintI().dist;
        }
        
    }
}