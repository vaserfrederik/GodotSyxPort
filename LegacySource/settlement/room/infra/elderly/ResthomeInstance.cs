using System;
using System.Collections.Generic;

namespace Settlement.Room.Infra.Elderly
{
    using Settlement.Main;
    using Settlement.Misc.Job;
    using Settlement.Path;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Util.Rendering;

    internal sealed class ResthomeInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        internal readonly Jobs jobs;

        protected ResthomeInstance(ROOM_RESTHOME blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            int work = 0;
            int open = 0;

            GUTIL.Coos().Set(0);

            foreach (COORDINATE c in Body())
            {
                if (Is(c))
                {
                    if (SETT.ROOMS().fData.tileData.Get(c) != 0)
                    {
                        blueprint.job.Set(this, c.X(), c.Y());
                        work++;
                    }
                    else if (SETT.ROOMS().fData.availability.Get(c) == AVAILABILITY.ROOM)
                    {
                        GUTIL.Coos().Set(open).Set(c);
                        open++;
                    }
                }
            }

            int am = (int)blueprint.constructor.stations.Get(this);
            am -= work;
            if (am > open)
                am = open;

            GUTIL.Coos().Shuffle(am);

            for (int i = 0; i < am; i++)
            {
                blueprint.job.Set(this, GUTIL.Coos().Set(i).X(), GUTIL.Coos().Set(i).Y());
            }

            jobs = new Jobs(this);
            jobs.Randomize();
            jobs.SetAlwaysNewJob();
            employees().NeededSet(am + work);
            employees().MaxSet(am + work);

            Activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            i.Lit();
            return base.Render(r, shadowBatch, i);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            jobs.SearchAgain();
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        public override JOB_MANAGER GetWork()
        {
            return jobs;
        }

        protected override void Dispose()
        {
        }

        public override ROOM_RESTHOME BlueprintI()
        {
            return (ROOM_RESTHOME)Blueprint();
        }

        internal class Jobs : JobIterator
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(ResthomeInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB Init(int tx, int ty)
            {
                return ((ROOM_RESTHOME)Ins().BlueprintI()).job.Get(tx, ty);
            }
        }
    }
}