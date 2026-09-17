using System;
using System.Collections.Generic;
using settlement.main;
using game;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using util.rendering;
using util.datatypes;

namespace settlement.room.industry.workshop
{
    public class WorkshopInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        public JobPositions<WorkshopInstance> jobs;
        private static readonly long serialVersionUID = -3170637142258642320L;
        private long[] pData;
        public bool hasStorage = true;
        public readonly short sx, sy;
        public short WI = 0;
        private short industry = -1;

        public WorkshopInstance(ROOM_WORKSHOP b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            SetIndustry(0);
            int x = -1;
            int y = -1;

            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    if (SETT.ROOMS().fData.tileData.get(c) == Constructor.B_STORAGE)
                    {
                        if (x == -1)
                        {
                            x = c.x();
                            y = c.y();
                        }
                    }
                }
            }

            if (x == -1 || y == -1)
                GAME.Error(x + " " + y);
            sx = (short)x;
            sy = (short)y;

            jobs = new Jobs(this);

            employees().maxSet(jobs.size());
            employees().neededSet(jobs.size());
            activate();
        }

        public override Industry Industry()
        {
            return blueprintI().indus.get(industry);
        }

        public override void SetIndustry(int i)
        {
            if (i == industry)
                return;

            Industry inIndustry = blueprintI().industries().get(i);
            if (inIndustry == null)
                return;
            pData = inIndustry.makeData();

            if (industry != -1)
            {
                foreach (COORDINATE c in body())
                {
                    if (!is(c))
                        continue;
                    if (SETT.ROOMS().fData.tileData.is(c.x(), c.y(), Constructor.B_WORK))
                    {
                        if (blueprintI().job.FETCH.get(c.x(), c.y(), this) != null)
                            blueprintI().job.FETCH.dispose();
                    }
                }
                if (blueprintI().indus.get(industry).outs().get(0).resource != blueprintI().indus.get(i).outs().get(0).resource)
                {
                    hasStorage = true;
                    foreach (COORDINATE c in body())
                    {
                        if (!is(c))
                            continue;
                        if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
                            blueprintI().job.storage.dispose();
                    }
                }
                jobs.searchAgain();
            }
            WI = 0;
            industry = (short)i;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
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
            Industry().updateRoom(this);

            if (!active())
                return;
            jobs.searchAgain();
            updateIndustryLocks();
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (blueprintI().job.storage.get(c.x(), c.y(), this) != null)
                    blueprintI().job.storage.dispose();
                else if (blueprintI().job.FETCH.get(c.x(), c.y(), this) != null)
                    blueprintI().job.FETCH.dispose();
            }
        }

        public override JOB_MANAGER GetWork()
        {
            return jobs;
        }

        public override ROOM_WORKSHOP blueprintI()
        {
            return (ROOM_WORKSHOP)blueprint();
        }

        public override RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return blueprintI().job.storage.get(tx, ty, this);
        }

        public override long[] ProductionData()
        {
            return pData;
        }

        public class Jobs : JobPositions<WorkshopInstance>
        {
            public Jobs(WorkshopInstance ins) : base(ins)
            {
            }

            private static readonly long serialVersionUID = 8423260307910904017L;

            protected override bool IsAndInit(int tx, int ty)
            {
                SETT.ROOMS().data.set(ins, tx, ty, 0);
                return get(tx, ty) != null;
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.blueprintI().job.init(tx, ty, ins);
            }
        }

        public override int IndustryI()
        {
            return industry;
        }
    }
}