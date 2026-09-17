using System;
using System.Collections.Generic;

namespace Settlement.Room.Industry.Refiner
{
    using static Settlement.Main.SETT.ROOMS;

    using Game;
    using Settlement.Main;
    using Settlement.Misc.Job;
    using Settlement.Misc.Util;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Job;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Util.Rendering;

    [Serializable]
    internal sealed class RefinerInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE
    {
        private JobPositions<RefinerInstance> jobs;
        private const long serialVersionUID = -3170637142258642320L;
        private long[] pData;
        public bool hasStorage = true;
        private readonly short sx, sy;
        private short WI = 0;
        private short industry = -1;

        public RefinerInstance(ROOM_REFINER b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            SetIndustry(0);
            int x = -1;
            int y = -1;

            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) == Constructor.B_STORAGE)
                    {
                        if (x == -1)
                        {
                            x = c.X();
                            y = c.Y();
                        }
                    }
                }
            }

            if (x == -1 || y == -1)
                GAME.Error(x + " " + y);
            sx = (short)x;
            sy = (short)y;

            jobs = new Jobs(this);

            employees().MaxSet(jobs.Size());
            employees().NeededSet(jobs.Size());
            Activate();
        }

        protected override void LoadFix()
        {
            pData = Industry().MakeDataFix(pData);
        }

        public override Industry Industry()
        {
            return blueprintI().indus.Get(industry);
        }

        public override void SetIndustry(int i)
        {
            if (i == industry)
                return;

            Industry inIndustry = blueprintI().industries().Get(i);
            pData = inIndustry.MakeData();

            if (industry != -1)
            {
                foreach (COORDINATE c in body())
                {
                    if (!Is(c))
                        continue;
                    if (blueprintI().job.FETCH.Get(c.X(), c.Y(), this) != null)
                        blueprintI().job.FETCH.Dispose();
                }
                if (blueprintI().indus.Get(industry).Outs().Get(0).resource != blueprintI().indus.Get(i).Outs().Get(0).resource)
                {
                    hasStorage = true;
                    foreach (COORDINATE c in body())
                    {
                        if (!Is(c))
                            continue;
                        if (blueprintI().job.storage.Get(c.X(), c.Y(), this) != null)
                            blueprintI().job.storage.Dispose();
                    }
                }
                jobs.SearchAgain();
            }
            industry = (short)i;
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
            Industry().UpdateRoom(this);

            if (!Active())
                return;
            jobs.SearchAgain();
            UpdateIndustryLocks();
        }

        protected override void Dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (!Is(c))
                    continue;
                if (blueprintI().job.storage.Get(c.X(), c.Y(), this) != null)
                    blueprintI().job.storage.Dispose();
                else if (blueprintI().job.FETCH.Get(c.X(), c.Y(), this) != null)
                    blueprintI().job.FETCH.Dispose();
            }
        }

        public override JOB_MANAGER GetWork()
        {
            return jobs;
        }

        public override ROOM_REFINER blueprintI()
        {
            return (ROOM_REFINER)blueprint();
        }

        public override RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return blueprintI().job.storage.Get(tx, ty, this);
        }

        public override long[] ProductionData()
        {
            return pData;
        }

        private class Jobs : JobPositions<RefinerInstance>
        {
            public Jobs(RefinerInstance ins) : base(ins)
            {
            }

            private const long serialVersionUID = 8423260307910904017L;

            protected override bool IsAndInit(int tx, int ty)
            {
                ROOMS().data.Set(ins, tx, ty, 0);
                return Get(tx, ty) != null;
            }

            protected override SETT_JOB Get(int tx, int ty)
            {
                return ins.blueprintI().job.Init(tx, ty, ins);
            }
        }

        public override int IndustryI()
        {
            return industry;
        }
    }
}