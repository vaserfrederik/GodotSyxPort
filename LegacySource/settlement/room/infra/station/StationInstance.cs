using System;
using System.Collections.Generic;
using settlement.room.infra.station;
using game.time;
using init.resources;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.infra.station
{
    [Serializable]
    internal class StationInstance : RoomInstance, JOBMANAGER_HASER, ROOM_MOVE_SOURCE
    {
        private static readonly long serialVersionUID = 1L;

        private readonly Jobs jobs;
        private bool auto;
        private readonly ArrayCooShort crates;
        private transient StationTally[] tally;
        private int[] incoming;
        private double prepared;

        private RBITImp bamount = new RBITImp();
        private RBITImp bcapacity = new RBITImp();

        public StationInstance(ROOM_STATION p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            tally = new StationTally[RESOURCES.ALL().Size()];
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                tally[res.index()] = new StationTally();
            }

            int cr = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_CRATE) != 0)
                {
                    cr++;
                }
            }
            crates = new ArrayCooShort(cr);
            foreach (COORDINATE c in body())
            {
                if (is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_CRATE) != 0)
                {
                    crates.get().set(c);
                    crates.inc();
                }
            }

            jobs = new Jobs(this);

            employees().maxSet(ROOM_STATION.MAX_EMPLOYEES);
            employees().neededSet(ROOM_STATION.MAX_EMPLOYEES);

            activate();
        }

        protected override void loadFix()
        {
            incoming = RESOURCES.map().loader().fix(incoming, 0);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void dispose()
        {
            for (int i = 0; i < crates.size(); i++)
            {
                blueprintI().crate.get(crates.get().x(), crates.get().y());
                blueprintI().crate.resourceSet(null);
            }
            for (int ri = 0; ri < RESOURCES.ALL().Size(); ri++)
            {
                RESOURCE r = RESOURCES.ALL().get(ri);
                blueprintI().tally(r).remove(tally(r), this);
                incoming[ri] = 0;
            }
            prepared = 0;
            for (int ri = 0; ri < RESOURCES.ALL().Size(); ri++)
            {
                RESOURCE r = RESOURCES.ALL().get(ri);
                blueprintI().tally(r).add(tally(r), this);
            }
        }

        protected override void updateAction(double ds, bool day)
        {
            jobs.searchAgain();
        }

        public void allocate(RESOURCE res, int am)
        {
            am = am - tally(res).crates();

            if (am == 0)
                return;

            if (am > 0)
            {
                for (int i = 0; i < crates.size(); i++)
                {
                    if (blueprintI().crate.get(crates.get().x(), crates.get().y()).resource() == null)
                    {
                        blueprintI().crate.resourceSet(res);
                        am--;
                        if (am <= 0)
                            return;
                    }
                    crates.inc();
                }
                am--;
            }
            if (am < 0)
            {
                for (int i = 0; i < crates.size(); i++)
                {
                    if (blueprintI().crate.get(crates.get().x(), crates.get().y()).resource() == res)
                    {
                        blueprintI().crate.resourceSet(null);
                        am++;
                        if (am >= 0)
                            return;
                    }
                    crates.inc();
                }
            }
        }

        public void deliver(RESOURCE res, int am)
        {
            unreserve(res);

            for (int i = 0; i < crates.size(); i++)
            {
                blueprintI().crate.get(crates.get().x(), crates.get().y());
                if (blueprintI().crate.resource() == res)
                {
                    int a = blueprintI().crate.MAX_AM - blueprintI().crate.stored.get();
                    a = CLAMP.i(a, 0, am);
                    blueprintI().crate.deliver(a);
                    am -= a;
                    if (am <= 0)
                        return;
                }
                crates.inc();
            }

            foreach (COORDINATE c in body())
            {
                if (!SETT.PATH().solidity.is(c))
                {
                    SETT.THINGS().resources.create(c, res, am);
                    return;
                }
            }
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        public override ROOM_STATION blueprintI()
        {
            return (ROOM_STATION)blueprint();
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        public override TILE_STORAGE storage(int tx, int ty)
        {
            return null;
        }

        private class Jobs : JobPositions<StationInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(StationInstance ins) : base(ins)
            {
                randomize();
                setAlwaysNew();
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return base.isAndInit(tx, ty);
            }

            protected override void initJob(JOB job)
            {
                base.initJob(job);
            }
        }

        private static readonly RBITImp tmp = new RBITImp();

        public override RESOURCE_TILE sourceCrate(RBIT okMask, int minAm, int ox, int oy, double limit)
        {
            tmp.clearSet(okMask);
            tmp.and(bamount);

            if (tmp.isClear())
                return null;

            foreach (RESOURCE r in RESOURCES.ALL())
            {
                if (tmp.has(r))
                {
                    StationTally t = tally(r);
                    double st = t.space();
                    double am = t.stored() - t.reserved() - minAm;
                    if (am <= 0 || limit > am / st)
                    {
                        tmp.clear(r);
                    }
                }
            }

            if (tmp.isClear())
                return null;

            if (is(ox, oy))
            {
                RESOURCE_TILE s = blueprintI().crate.get(ox, oy);
                if (s != null && s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm)
                {
                    return s;
                }
            }

            for (int i = 0; i < crates.size(); i++)
            {
                crates.inc();
                RESOURCE_TILE s = blueprintI().crate.get(crates.get().x(), crates.get().y());
                if (s.resource() != null && tmp.has(s.resource()) && s.reservable() >= minAm)
                {
                    return s;
                }
            }

            if (minAm == 1)
                LOG.ln("Weird indeed");

            return null;
        }

        public override RBIT sourceAmountMask()
        {
            return bamount;
        }

        public override RBIT moveCapacity()
        {
            return bcapacity;
        }

        public override int moveCapacityAm(RESOURCE res)
        {
            return tally(res).stored() - tally(res).reserved();
        }

        public override double storedD(RESOURCE res)
        {
            double sp = tally(res).space();
            if (sp == 0)
                return 1;
            return tally(res).stored() / sp;
        }

        public override RoomState makeState(int tx, int ty, bool broken)
        {
            return new State(this, broken);
        }

        private class State : RoomStateInstance
        {
            private static readonly long serialVersionUID = 1L;
            private readonly short[] crates = new short[RESOURCES.ALL().Size()];

            public State(StationInstance ins, bool broken) : base(ins)
            {
                foreach (RESOURCE r in RESOURCES.ALL())
                {
                    crates[r.index()] = (short)ins.tally[r.index()].crates();
                }
            }

            public override void applyIns(RoomInstance ins)
            {
                if (ins is StationInstance s)
                {
                    for (int ri = 0; ri < RESOURCES.ALL().Size() && ri < RESOURCES.ALL().Size(); ri++)
                    {
                        s.allocate(RESOURCES.ALL().get(ri), crates[ri]);
                    }
                }
            }
        }
    }
}