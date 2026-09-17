using System;
using System.Collections.Generic;
using System.Linq;
using game;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.maintenance;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.infra.logistics;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.military.supply
{
    internal sealed class SupplyInstance : RoomInstance, JOBMANAGER_HASER, ROOM_RADIUS_INSTANCE, ROOM_MOVE_DEST, ROOM_MOVEJOBBER, MoveOrderPullInstance
    {
        private static readonly long serialVersionUID = 1L;
        public const int ORDERS = 2;

        public byte coolFetch = 0;
        public readonly MoveOrderPull[] orders = new MoveOrderPull[ORDERS];
        public bool fetch = true;
        public bool auto = true;
        public short[] tdata;
        private short ox, oy;
        private byte orderI = 0;
        private readonly RBITImp allowed = new RBITImp();
        public readonly JobPositions<SupplyInstance> jobs;
        public byte liveCount;
        public byte goCount;
        private bool prio = true;

        public SupplyInstance(ROOM_SUPPLY blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            jobs = new Jobs(this);
            int w = (int)blueprint.constructor.workers.get(this);
            employees().maxSet(w * 4);
            employees().neededSet(w);
            blueprint.tally.init(this);
            allowed.setAll();
            activate();
        }

        void reset()
        {
            coolFetch = 0;
            foreach (MoveOrderPull o in orders)
            {
                if (o != null)
                    o.cooldown = 0;
            }
            verifyCrates();
        }

        private void verifyCrates()
        {
            for (int i = 0; i < jobs.size(); i++)
            {
                int ox = jobs.get(i).x();
                int oy = jobs.get(i).y();
                Crate cr = blueprintI().crate.get(ox, oy);
                if (cr.storage() != null && cr.realResource() != null && !allowed.has(cr.storage().resource()))
                {
                    cr.clear();
                }
            }
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        public void allowedToggle(RESOURCE res)
        {
            foreach (MoveOrderPull p in orders)
            {
                if (p != null)
                    p.resbits.set(res, !allowed.has(res));
            }
            allowed.toggle(res);
        }

        public RBIT allowed()
        {
            return allowed;
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            if (!active() || employees().employed() <= 0)
            {
                for (int i = 0; i < jobs.size(); i++)
                {
                    int ox = jobs.get(i).x();
                    int oy = jobs.get(i).y();
                    Crate cr = blueprintI().crate.get(ox, oy);
                    if (cr.storage() != null && cr.realResource() != null)
                    {
                        cr.clear();
                    }
                }
            }
            jobs.searchAgain();
            if (coolFetch > 0)
            {
                coolFetch--;
            }
            foreach (MoveOrderPull o in orders)
            {
                if (o != null && o.cooldown > 0)
                    o.cooldown--;
            }
        }

        protected override void dispose()
        {
            foreach (COORDINATE c in body())
            {
                if (is(c) && blueprintI().crate.get(c.x(), c.y()) != null)
                {
                    blueprintI().crate.dispose();
                }
            }
        }

        public override ROOM_SUPPLY blueprintI()
        {
            return ROOMS().SUPPLY;
        }

        public override bool destroyTileCan(int tx, int ty)
        {
            return true;
        }

        public override ROOM_DEGRADER degrader(int tx, int ty)
        {
            return null;
        }

        public override MoveOrderPull[] moveOrdersPull()
        {
            return orders;
        }

        public override RBIT moveCapacity()
        {
            return allowed;
        }

        public override RBIT moveOrderPullAvailable()
        {
            return blueprintI().tally.fetchBit(this, allowed);
        }

        public override RBIT moveOrderPullAccepted()
        {
            return moveCapacity();
        }

        public override int moveMinAmount()
        {
            return 1;
        }

        public override int moveMaxRadius()
        {
            return 300;
        }

        public void prioritizeToggle()
        {
            prio = !prio;
            reset();
        }

        public bool prioritizing()
        {
            return prio;
        }

        public void fetchingToggle()
        {
            fetch = !fetch;
            reset();
        }

        public bool fetching()
        {
            return fetch;
        }

        public override MoveJob moveJob(Humanoid skill)
        {
            RBIT bb = blueprintI().tally.fetchBit(this, allowed);
            if (bb.isClear())
                return null;

            int am = SETT.ROOMS().STOCKPILE.carryCap(skill);

            if ((fetch || prio) && coolFetch <= 0)
            {
                MoveJob j = MoveJob.fetch(this, this, am, radius(), ox, oy, fetch ? bb : RBIT.NONE, prio ? bb : RBIT.NONE);

                if (j != null)
                {
                    ox = (short)j.source.x();
                    oy = (short)j.source.y();
                    return j;
                }

                coolFetch = 4;
            }

            for (int ooi = 0; ooi < orders.Length; ooi++)
            {
                orderI++;
                if (orderI >= orders.Length)
                    orderI = 0;
                MoveOrderPull p = orders[orderI];
                if (p != null && p.cooldown <= 0)
                    return p.job();
            }

            return null;
        }

        public override TILE_STORAGE storage(int tx, int ty)
        {
            return blueprintI().crate.storage(tx, ty);
        }

        private static class Jobs : JobPositions<SupplyInstance>
        {
            private static readonly long serialVersionUID = 1L;

            public Jobs(SupplyInstance ins) : base(ins)
            {
            }

            protected override bool isAndInit(int tx, int ty)
            {
                return ins.blueprintI().crate.get(tx, ty) != null;
            }

            protected override SETT_JOB get(int tx, int ty)
            {
                Crate c = ins.blueprintI().crate.get(tx, ty);
                if (c != null)
                    return c.job;
                return null;
            }
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        public override void copyFrom(MoveOrderPullInstance same)
        {
            SupplyInstance ins = (SupplyInstance)same;
            foreach (RESOURCE res in AD.supplies().resses())
            {
                if (allowed().has(res) != ins.allowed().has(res))
                {
                    allowedToggle(res);
                    reset();
                }
            }
            fetch = ins.fetch;
            coolFetch = 0;
            auto = ins.auto;
            employees().neededSet(ins.employees().target());
        }

        public override RoomState makeState(int tx, int ty, bool broken)
        {
            return new State(this, broken);
        }

        private static class State : RoomStateInstance
        {
            private static readonly long serialVersionUID = 1L;

            private bool fetching;
            private bool prio;
            private readonly bool broken;
            private MoveOrderPull[] orders;
            private readonly RBITImp useMask = new RBITImp();

            public State(SupplyInstance ins, bool broken) : base(ins)
            {
                this.broken = broken;
                this.fetching = ins.fetch;
                this.prio = ins.prio;
                useMask.clearSet(ins.allowed);
                if (broken)
                {
                    this.orders = ins.orders;
                }
            }

            public override void applyIns(RoomInstance ins)
            {
                if (ins is SupplyInstance)
                {
                    SupplyInstance s = (SupplyInstance)ins;
                    if (broken)
                    {
                        for (int i = 0; i < orders.Length; i++)
                        {
                            if (orders[i] != null)
                            {
                                MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
                                s.orders[i] = p;
                            }
                        }
                    }
                    if (fetching != s.fetching())
                        s.fetchingToggle();
                    if (prio != s.prioritizing())
                        s.prioritizeToggle();

                    foreach (RESOURCE res in RESOURCES.ALL())
                    {
                        if (s.allowed().has(res.bit) != useMask.has(res.bit))
                        {
                            s.allowedToggle(res);
                        }
                    }
                }
            }
        }
    }
}