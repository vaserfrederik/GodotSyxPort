using System;
using System.Collections.Generic;

namespace Settlement.Room.Infra.Admin
{
    public class AdminInstance : RoomInstance, JOBMANAGER_HASER, ROOM_IDATA_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;
        public Jobs jobs;
        private long[] pdata;

        protected AdminInstance(ROOM_ADMIN blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);

            Employees.NeededSet(jobs.Size);
            Employees.MaxSet(jobs.Size);

            jobs.Randomize();
            pdata = blueprint.Consumption.MakeData();
            Activate();
            blueprint.Data.IncStations(jobs.Size);
        }

        protected override void LoadFix()
        {
            pdata = blueprintI().Consumption.MakeDataFix(pdata);
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.Lit();
            return base.Render(r, shadowBatch, i);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            blueprintI().Consumption.UpdateRoom(this);
            jobs.SearchAgain();
        }

        protected override void ActivateAction()
        {
            blueprintI().Data.IncStations(jobs.Size);
        }

        protected override void DeactivateAction()
        {
            blueprintI().Data.IncStations(-jobs.Size);
        }

        public JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
            blueprintI().Consumption.ReleaseResources(this, this);
            blueprintI().Data.IncStations(-jobs.Size);
        }

        public ROOM_ADMIN BlueprintI()
        {
            return (ROOM_ADMIN)Blueprint();
        }

        public long[] ProductionData()
        {
            return pdata;
        }

        public class Jobs : JobPositions<AdminInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(AdminInstance ins) : base(ins) { }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.blueprintI().Job.Get(tx, ty);
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return ins.blueprintI().Job.Get(tx, ty) != null;
            }
        }
    }
}