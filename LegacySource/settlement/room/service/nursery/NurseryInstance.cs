using System;
using System.Collections.Generic;
using System.Linq;
using settlement.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using util;
using util.datatypes;
using util.rendering;

namespace settlement.room.service.nursery
{
    final class NurseryInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;

        private readonly Jobs jobs;
        private readonly RoomServiceInstance service;

        protected NurseryInstance(ROOM_NURSERY blue, TmpArea area, RoomInit init) : base(blue, area, init)
        {
            GUTIL.coos().set(0);

            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (SETT.ROOMS().fData.tileData.get(c) == NurseryConstructor.TABLE)
                {
                    blue.ss.init(this, c.x(), c.y());
                }
                else if (SETT.ROOMS().fData.tileData.get(c) == NurseryConstructor.CARPET)
                {
                    GUTIL.coos().get().set(c);
                    GUTIL.coos().inc();
                }
            }

            int carps = GUTIL.coos().getI();
            int cc = (int)Math.Round(GUTIL.coos().getI() * 0.25);
            GUTIL.coos().shuffle(carps);
            for (int i = 0; i < cc; i++)
            {
                GUTIL.coos().set(i);
                blue.ss.init(this, GUTIL.coos().get().x(), GUTIL.coos().get().y());
            }

            jobs = new Jobs(this);

            employees().maxSet(jobs.size());
            employees().neededSet((int)Math.Ceiling(blue.constructor.workers.get(this)));
            service = new RoomServiceInstance(jobs.size(), blue.service());
            Console.WriteLine(jobs.size());
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
                service.updateDay();
        }

        public override void updateTileDay(int tx, int ty)
        {
        }

        protected override void dispose()
        {
            service.dispose(blueprintI().service);
        }

        public override ROOM_NURSERY blueprintI()
        {
            return (ROOM_NURSERY)blueprint();
        }

        public override JobPositions<NurseryInstance> getWork()
        {
            return jobs;
        }

        public override RoomServiceInstance service()
        {
            return service;
        }

        public override double quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }

        private static class Jobs : JobPositions<NurseryInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(NurseryInstance ins) : base(ins)
            {
            }

            protected override bool isAndInit(int tx, int ty)
            {
                if (ins.is(tx, ty))
                    return ins.blueprintI().ss.job(tx, ty) != null;
                return false;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                if (ins.is(tx, ty))
                    return ins.blueprintI().ss.job(tx, ty);
                return null;
            }
        }
    }
}