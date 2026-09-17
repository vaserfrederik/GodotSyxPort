using System;
using System.Collections.Generic;
using game.time;
using game.tourism;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.infra.inn
{
    [Serializable]
    public class InnInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        public Jobs jobs;
        public const double WORKER_PER_BED = 1d / 8d;
        public bool auto = false;
        public int earnings;
        public int earningsLast;
        public byte year = (byte)TIME.years().bitsSinceStart();

        public readonly RoomServiceInstance service;

        public readonly Review[] reviews = new Review[] {
            new Review(),
            new Review(),
            new Review(),
            new Review()
        };

        protected InnInstance(ROOM_INN b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new Jobs(this);
            int total = (int)b.constructor.beds.get(this);
            employees().maxSet(2 * (int)Math.Ceiling(b.constructor.workers.get(this)));
            employees().neededSet((int)Math.Ceiling(b.constructor.workers.get(this)));
            service = new RoomServiceInstance(total, blueprintI().service);
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override bool renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            base.renderAbove(r, shadowBatch, i);
            blueprintI().constructor.aboveR(r, shadowBatch, i, getDegrade());
            return false;
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
            if (year != (byte)TIME.years().bitCurrent())
            {
                year = (byte)TIME.years().bitCurrent();
                earningsLast = earnings;
                earnings = 0;
            }
            if (day)
                service.updateDay();
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        protected override void dispose()
        {
            service.dispose(blueprintI().service);
        }

        public override ROOM_INN blueprintI()
        {
            return (ROOM_INN)blueprint();
        }

        public class Jobs : JobPositions<InnInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(InnInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                ABed b = ins.blueprintI().bed.init(tx, ty);
                if (b != null)
                    return b.job;
                return null;
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().bed.init(tx, ty) != null;
            }
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, blueprintI().constructor.coziness.get(this));
        }

        public override RoomServiceInstance service()
        {
            return service;
        }
    }
}