using System;

namespace Settlement.Room.Knowledge.Laboratory
{
    using Settlement.Misc.Job;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Job;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Util.Rendering;

    public sealed class LaboratoryInstance : RoomInstance, IJOBMANAGER_HASER, IROOM_IDATA_INSTANCE
    {
        private const long serialVersionUID = 1L;
        public Jobs jobs;
        private long[] pdata;

        protected LaboratoryInstance(ROOM_LABORATORY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            pdata = blueprint.consumption().makeData();
            jobs = new Jobs(this);

            employees().neededSet((int)Math.Ceiling(blueprint.constructor.workers.get(this)));
            employees().maxSet(jobs.size());

            jobs.randomize();

            activate();
            blueprintI().data.incStations(jobs.size());
        }

        protected override void loadFix()
        {
            pdata = blueprintI().consumption().makeDataFix(pdata);
        }

        public override long[] productionData()
        {
            return pdata;
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.lit();
            return base.render(r, shadowBatch, i);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            jobs.searchAgain();
            blueprintI().consumption().updateRoom(this);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void dispose()
        {
            blueprintI().consumption().releaseResources(this, this);
            blueprintI().data.incStations(-jobs.size());
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        public override ROOM_LABORATORY blueprintI()
        {
            return (ROOM_LABORATORY)blueprint();
        }

        public class Jobs : JobPositions<LaboratoryInstance>
        {
            private const long serialVersionUID = 1L;

            public Jobs(LaboratoryInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().job.get(tx, ty);
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().job.get(tx, ty) != null;
            }
        }
    }
}