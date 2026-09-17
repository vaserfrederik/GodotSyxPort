using System;
using System.Collections.Generic;

namespace Settlement.Room.Service.Barber
{
    public class Instance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;
        public Jobs jobs;

        public RoomServiceInstance service;
        public bool auto = true;

        protected Instance(ROOM_BARBER blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);

            service = new RoomServiceInstance(jobs.Size(), blueprintI().Data);

            Employees().MaxSet(jobs.Size());
            Employees().NeededSet((int)Math.Ceiling(blueprint.Constructor.Workers.Get(this)));
            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.Lit();
            return base.Render(r, shadowBatch, i);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            if (day)
                service.UpdateDay();
            jobs.SearchAgain();
        }

        protected override void ActivateAction()
        {

        }

        protected override void DeactivateAction()
        {

        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
            for (int i = 0; i < jobs.Size(); i++)
            {
                COORDINATE c = jobs.Get(i);
                FSERVICE s = blueprintI().Ll.Service(c.X, c.Y);
                if (s.FindableReservedCanBe())
                    s.FindableReserve();
            }

            service.Dispose(blueprintI().Data);
        }

        public ROOM_BARBER BlueprintI()
        {
            return (ROOM_BARBER)Blueprint();
        }

        public RoomServiceInstance Service()
        {
            return service;
        }

        public double Quality()
        {
            return ROOM_SERVICER.DefQuality(this, blueprintI().Constructor.Quality.Get(this));
        }

        public class Jobs : JobPositions<Instance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(Instance ins) : base(ins)
            {
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.BlueprintI().Ll.Job(tx, ty);
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return ins.BlueprintI().Ll.Job(tx, ty) != null;
            }
        }
    }
}