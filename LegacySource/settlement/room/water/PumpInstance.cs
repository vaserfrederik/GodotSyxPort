using System;
using System.Collections.Generic;

namespace Settlement.Room.Water
{
    class PumpInstance : RoomInstance, IJobManagerHolder
    {
        private JobIterator jobs;
        private static readonly long serialVersionUID = -3170637142258642320L;

        private int workersHas;
        private int workersNeed;
        public short value;
        private int dw;
        private static int maxWorkAm = 8;
        public double valueMax = 100;

        private readonly short ox, oy;

        public PumpInstance(ROOM_PUMP b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            jobs = new Jobs(this);
            int px = 0;
            int py = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && SETT.ROOMS().fData.tile.get(c) == blueprintI().constructor.ou)
                {
                    px = c.x();
                    py = c.y();
                    break;
                }
            }

            ox = (short)px;
            oy = (short)py;

            setEmployees();
            employees().neededSet(employees().max());

            valueMax = (int)Math.Ceiling(25 * employees().max());

            dw = maxWorkAm - 2;
            value = 64;
            workersHas = 1;
            workersNeed = 1;
            activate();
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
            SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
        }

        protected override void DeactivateAction()
        {
            SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            if (!active())
                return;

            workersHas += employees().employed();
            workersNeed += employees().max();
            dw++;
            if (dw >= maxWorkAm)
            {
                short nv = (short)Math.Ceiling((1.0 - 0.8 * getDegrade()) * valueMax * workersHas / workersNeed);
                workersHas = 0;
                workersNeed = 0;
                dw = 0;
                if (nv != value)
                {
                    value = nv;

                    SETT.ROOMS().WATER.updater.reportChange(ox, oy, 0);
                }
            }
        }

        protected override void Dispose()
        {
        }

        public IJobManager GetWork()
        {
            return jobs;
        }

        public ROOM_PUMP BlueprintI()
        {
            return (ROOM_PUMP)blueprint();
        }

        private void setEmployees()
        {
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().job.init(c.x(), c.y(), this) != null)
                {
                    am++;
                }
            }
            employees().maxSet(am);
            if (am > employees().max())
                employees().neededSet(employees().max());
            jobs.searchAgain();
        }

        private class Jobs : JobIterator
        {
            public Jobs(PumpInstance ins) : base(ins)
            {
            }

            protected override SETT_JOB Init(int tx, int ty)
            {
                PumpInstance ins = SETT.ROOMS().WATER.pump.get(tx, ty);
                if (ins != null)
                    return SETT.ROOMS().WATER.pump.job.init(tx, ty, ins);
                return null;
            }
        }

        public int Ox()
        {
            return ox;
        }

        public int Oy()
        {
            return oy;
        }

        public int Output()
        {
            if (!active())
                return 0;
            return value;
        }

        public double AniSpeed()
        {
            return (double)employees().employed() / employees().max();
        }

        public override void UpgradeSet(int upgrade)
        {
            base.UpgradeSet(upgrade);
            setEmployees();
        }
    }
}