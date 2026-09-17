using System;
using System.Collections.Generic;
using settlement.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.law.police
{
    [Serializable]
    final class PoliceInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        public readonly Jobs jobs;

        int prisoners = 0;

        protected PoliceInstance(ROOM_POLICE b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            int spots = 0;

            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if ((SETT.ROOMS().fData.tileData.get(c) & PoliceConstructor.bitService) == PoliceConstructor.bitService)
                {
                    spots++;
                }
            }

            jobs = new Jobs(this);

            employees().maxSet(spots * 3);
            employees().neededSet(spots);
            activate();
        }

        public int prisonersMax()
        {
            return (int)Math.Ceiling(employees().employed() / 3.0);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().work.job(c.x(), c.y()) != null)
                {
                    blueprintI().work.dispose(c.x(), c.y());
                }
            }
        }

        public override void updateTileDay(int tx, int ty)
        {
            blueprintI().work.update(tx, ty);
            jobs.searchAgain();
        }

        public override ROOM_POLICE blueprintI()
        {
            return (ROOM_POLICE)blueprint();
        }

        public class Jobs : JobPositions<PoliceInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(PoliceInstance ins) : base(ins)
            {
                setAlwaysNew();
                randomize();
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                return ins.blueprintI().work.job(tx, ty);
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().work.job(tx, ty) != null;
            }
        }

        protected override void dispose()
        {
            // TODO Auto-generated method stub
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }
    }
}