using System;
using System.Collections.Generic;
using settlement.room.health.hospital;
using game.faction;
using init.value;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.health
{
    [Serializable]
    internal sealed class HospitalInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER
    {
        private static readonly long serialVersionUID = 1L;

        internal Jobs jobs;
        private long[] pData;
        private readonly RoomServiceInstance service;
        internal bool[] fetch;

        protected HospitalInstance(ROOM_HOSPITAL blue, TmpArea area, RoomInit init)
            : base(blue, area, init)
        {
            jobs = new Jobs(this);

            int j = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && Bed.job(c.x(), c.y()) != null)
                    j++;
            }

            fetch = new bool[blue.resLocks.size()];
            service = new RoomServiceInstance(j, blue.service());

            employees().maxSet((int)Math.Ceiling(blue.constructor.workers.get(this)));
            employees().neededSet((int)Math.Ceiling(blue.constructor.workers.get(this)));
            pData = blue.consumtion.makeData();

            int ii = 0;
            foreach (Lockable<Faction> l in blue.resLocks)
            {
                fetch[ii++] = l.passes(FACTIONS.player());
            }
            activate();
        }

        protected override void loadFix()
        {
            pData = blueprintI().consumtion.makeDataFix(pData);
            if (fetch == null || fetch.Length != blueprintI().resLocks.size())
                fetch = new bool[blueprintI().resLocks.size()];
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (day)
            {
                service.updateDay();
                jobs.searchAgain();
            }
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c) && Bed.service(c.x(), c.y()) != null)
                    Bed.service(c.x(), c.y()).findableReserve();
            }
            service.dispose(blueprintI().service);
        }

        public ROOM_HOSPITAL blueprintI()
        {
            return (ROOM_HOSPITAL)blueprint();
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        public long[] productionData()
        {
            return pData;
        }

        public Industry industry()
        {
            return blueprintI().industries().get(0);
        }

        public int industryI()
        {
            return 0;
        }

        internal sealed class Jobs : JobIterator
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(HospitalInstance ins)
                : base(ins)
            {
                setAlwaysNewJob();
                randomize();
            }

            protected override SETT_JOB init(int tx, int ty)
            {
                return Bed.job(tx, ty);
            }
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, 1);
        }

        public RoomState makeState(int tx, int ty, bool broken)
        {
            return new State(this);
        }

        private sealed class State : RoomStateInstance
        {
            private static readonly long serialVersionUID = 1L;
            private bool[] opium;

            public State(HospitalInstance ins)
                : base(ins)
            {
                this.opium = ins.fetch;
            }

            protected override void applyIns(RoomInstance ins)
            {
                if (ins is HospitalInstance)
                {
                    ((HospitalInstance)ins).fetch = opium;
                }
                base.applyIns(ins);
            }
        }
    }
}