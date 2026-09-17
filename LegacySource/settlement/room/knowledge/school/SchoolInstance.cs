using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.knowledge.school
{
    [Serializable]
    sealed class SchoolInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        readonly Jobs jobs;
        private long[] pdata;
        private readonly RoomServiceInstance service;

        protected SchoolInstance(ROOM_SCHOOL blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);
            service = new RoomServiceInstance(jobs.size(), blueprint.service);

            employees().neededSet((int)Math.Ceiling(jobs.size() / 8.0));
            employees().maxSet((int)Math.Ceiling(jobs.size() / 3.0));
            jobs.randomize();
            pdata = blueprint.industry.makeData();
            activate();
        }

        protected override void loadFix()
        {
            pdata = industry().makeDataFix(pdata);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            blueprintI().industry.updateRoom(this);
            if (day)
                service.updateDay();
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
                    blueprintI().station.dispose(c.x(), c.y());
                }
            }
            service.dispose(blueprintI().service);
        }

        public ROOM_SCHOOL blueprintI()
        {
            return (ROOM_SCHOOL)blueprint();
        }

        public long[] productionData()
        {
            return pdata;
        }

        public Industry industry()
        {
            return blueprintI().industries().get(0);
        }

        public sealed class Jobs : JobPositions<SchoolInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(SchoolInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().station.job(tx, ty);
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().station.job(tx, ty) != null;
            }
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, blueprintI().constructor.quality.get(this));
        }

        public int industryI()
        {
            return 0; // TODO Auto-generated method stub
        }

        public double productionRate(RoomInstance ins, Humanoid h, Industry in, IndustryResource oo)
        {
            double d = service.load() * service.total();
            return d;
        }
    }
}