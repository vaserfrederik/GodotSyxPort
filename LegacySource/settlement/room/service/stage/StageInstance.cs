using System;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.stage
{
    [Serializable]
    internal sealed class StageInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly RoomServiceInstance service;
        private readonly byte off = (byte)RND.rInt(64);
        private readonly Job job = new Job(this);

        private short workers;
        private short services = 0;

        protected StageInstance(ROOM_STAGE b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            service = new RoomServiceInstance((int)b.constructor.spectators.Get(this), blueprintI().Data);

            employees().MaxSet(job.Size());
            employees().NeededSet(job.Size());
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.Lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            if (Active())
            {
                if (employees().Employed() == 0)
                {
                    if (workers > 0)
                    {
                        workers--;
                        if (workers == 0)
                        {
                            SetServices(0);
                        }
                    }
                }
                else
                {
                    if (workers < 10)
                    {
                        workers = 10;
                        SetServices(service.Total());
                    }
                }
            }
            if (day)
                service.UpdateDay();
        }

        protected override void ActivateAction()
        {
            if (workers > 0)
            {
                SetServices(service.Total());
            }
        }

        protected override void DeactivateAction()
        {
            SetServices(0);
            workers = 0;
        }

        internal void IncServices(int s)
        {
            if (workers > 0)
                SetServices(services + s);
        }

        internal bool HasService()
        {
            return workers > 0;
        }

        private void SetServices(int s)
        {
            service.Report(blueprintI().Work.Service(body().CX(), body().CY()), blueprintI().Data, -services, false);
            this.services = (short)CLAMP.I(s, 0, service.Total());
            service.Report(blueprintI().Work.Service(body().CX(), body().CY()), blueprintI().Data, services, true);
        }

        internal int Services()
        {
            return services;
        }

        public JOB_MANAGER GetWork()
        {
            return job;
        }

        protected override void Dispose()
        {
            service.Dispose(blueprintI().Data);
        }

        public ROOM_STAGE BlueprintI()
        {
            return (ROOM_STAGE)Blueprint();
        }

        public RoomServiceInstance Service()
        {
            return service;
        }

        public double Quality()
        {
            return ROOM_SERVICER.DefQuality(this, ((double)employees().Employed() / employees().Max()));
        }

        private class Job : JobPositions<StageInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Job(StageInstance ins) : base(ins)
            {
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return ins.blueprintI().Work.Job(tx, ty) != null;
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.blueprintI().Work.Job(tx, ty);
            }
        }
    }
}