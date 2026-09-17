using System;
using System.Collections.Generic;

namespace Settlement.Room.Service.Breeder
{
    using Settlement.Entity;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Misc.Job;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Job;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Snake2D.Util.Datatypes;
    using Util.Rendering;

    public sealed class BreederInstance : RoomInstance, IRoomProducerInstance, IJobManagerHaser
    {
        private static readonly long SerialVersionUID = 1L;

        private readonly long[] pData;
        private readonly Jobs jobs;
        public double KidsProduction { get; private set; } = 0;
        public bool Auto { get; set; } = true;

        protected BreederInstance(ROOM_BREEDER blue, TmpArea area, RoomInit init) : base(blue, area, init)
        {
            pData = blue.ProductionData.MakeData();

            jobs = new Jobs(this);

            Employees().MaxSet(jobs.Size());
            Employees().NeededSet((int)Math.Ceiling(blue.Constructor.Workers.Get(this)));

            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.Lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            BlueprintI().ProductionData.UpdateRoom(this);
            jobs.SearchAgain();
        }

        public void UpdateTileDay(int tx, int ty)
        {
            BlueprintI().Station.Update(tx, ty);
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in Body())
            {
                if (Is(c))
                {
                    BlueprintI().Station.Dispose(c.X, c.Y);
                }
            }
            foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
            {
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    HEvent.Handler.RemoveRoom(a, this);
                }
            }
        }

        public ROOM_BREEDER BlueprintI()
        {
            return (ROOM_BREEDER)Blueprint();
        }

        public long[] ProductionData()
        {
            return pData;
        }

        public JobPositions<BreederInstance> GetWork()
        {
            return jobs;
        }

        public Industry Industry()
        {
            return BlueprintI().Industries().Get(0);
        }

        private class Jobs : JobPositions<BreederInstance>
        {
            private static readonly long SerialVersionUID = 1L;

            public Jobs(BreederInstance ins) : base(ins)
            {
            }

            protected override bool IsAndInit(int tx, int ty)
            {
                return Ins.BlueprintI().Station.Init(tx, ty);
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return Ins.BlueprintI().Station.Get(tx, ty);
            }
        }

        public int IndustryI()
        {
            return 0;
        }
    }
}