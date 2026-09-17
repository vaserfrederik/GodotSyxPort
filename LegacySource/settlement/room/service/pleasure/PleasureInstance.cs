using settlement.misc.job;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.pleasure
{
    [Serializable]
    final class PleasureInstance : RoomInstance, IJOBMANAGER_HASER, IROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        private readonly Jobs jobs;
        private bool auto = false;

        private readonly RoomServiceInstance service;

        protected PleasureInstance(ROOM_PLEASURE b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new Jobs(this);
            int total = jobs.Size;
            employees().MaxSet(jobs.Size);
            employees().NeededSet(jobs.Size);
            service = new RoomServiceInstance(total, blueprintI().service);
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.Lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override bool RenderAbove(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            base.RenderAbove(r, shadowBatch, i);
            blueprintI().constructor.aboveR(r, shadowBatch, i, GetDegrade());
            return false;
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            jobs.SearchAgain();
            if (day)
                service.UpdateDay();
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
            service.Dispose(blueprintI().service);
        }

        public ROOM_PLEASURE BlueprintI()
        {
            return (ROOM_PLEASURE)blueprint();
        }

        private class Jobs : JobPositions<PleasureInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(PleasureInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                ABed b = ins.blueprintI().bed.Init(tx, ty);
                if (b != null)
                    return b.job;
                return null;
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return ins.Is(tx, ty) && ins.blueprintI().bed.Init(tx, ty) != null;
            }
        }

        public double Quality()
        {
            return IROOM_SERVICER.DefQuality(this, blueprintI().constructor.coziness.Get(this));
        }

        public RoomServiceInstance Service()
        {
            return service;
        }
    }
}