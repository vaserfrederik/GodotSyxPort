using System;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.rendering;

namespace settlement.room.service.speaker
{
    [Serializable]
    final class SpeakerInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        readonly RoomServiceInstance service;
        readonly byte off = (byte)RND.rInt(64);
        byte workers = 0;
        private short services = 0;

        protected SpeakerInstance(ROOM_SPEAKER b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            service = new RoomServiceInstance((int)b.constructor.spectators.get(this), blueprintI().data);

            employees().maxSet(1);
            employees().neededSet(1);
            activate();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
            if (workers > 0)
            {
                setServices(service.total());
            }
        }

        protected override void deactivateAction()
        {
            setServices(0);
            workers = 0;
        }

        void incServices(int s)
        {
            if (workers > 0)
                setServices(services + s);
        }

        bool hasService()
        {
            return workers > 0;
        }

        private void setServices(int s)
        {
            service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, -services, false);
            this.services = (short)CLAMP.i(s, 0, service.total());
            service.report(blueprintI().work.service(body().cX(), body().cY()), blueprintI().data, services, true);
        }

        int services()
        {
            return services;
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (active())
            {
                if (employees().employed() == 0)
                {
                    if (workers > 0)
                    {
                        workers--;
                        if (workers == 0)
                        {
                            setServices(0);
                        }
                    }
                }
                else
                {
                    if (workers < 10)
                    {
                        workers = 10;
                        setServices(service.total());
                    }
                }
            }
            if (day)
                service.updateDay();
        }

        public JOB_MANAGER getWork()
        {
            return blueprintI().work.manager(this);
        }

        protected override void dispose()
        {
            service.dispose(blueprintI().data);
        }

        public ROOM_SPEAKER blueprintI()
        {
            return (ROOM_SPEAKER)blueprint();
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }
    }
}