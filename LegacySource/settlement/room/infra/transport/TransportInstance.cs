using System;
using System.Collections.Generic;
using settlement.room.infra.transport;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.infra.logistics;
using settlement.room.infra.stockpile;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;
using snake2d;
using util.rendering;

namespace settlement.room.infra.transport
{
    [Serializable]
    final class TransportInstance : RoomInstance, JOBMANAGER_HASER, ROOM_RADIUS_INSTANCE, ROOM_MOVEJOBBER, ROOM_MOVE_DEST, MoveOrderPullInstance
    {
        private static readonly long serialVersionUID = 1L;

        public static readonly int ORDERS = 4;

        private readonly Jobs jobs;

        public bool auto;

        private bool fetching = true;
        public bool prio = false;
        public byte coolFetch = 0;

        public readonly Cart data;

        public readonly MoveOrderPull[] pullOrders = new MoveOrderPull[ORDERS];
        private byte orderIP = 0;

        private short lastSourceX, lastSourceY;
        public byte radius = 20;

        private readonly short sx, sy;

        public float fetchTime;
        public float distance;
        public float stationWorkers;
        public bool stationProblem = false;

        public TransportInstance(ROOM_TRANSPORT p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            data = new Cart();
            int sx = -1;
            int sy = -1;
            foreach (COORDINATE c in body())
            {
                if (is(c) && SETT.ROOMS().fData.tile.get(c) == p.constructor.an)
                {
                    sx = c.x();
                    sy = c.y();
                }
            }
            if (sx == -1)
                throw new RuntimeException();
            this.sx = (short)sx;
            this.sy = (short)sy;
            jobs = new Jobs(this);

            employees().maxSet((int)jobs.size());
            employees().neededSet(jobs.size());

            activate();
        }

        protected override void loadFix()
        {
            data.loadFix();
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void dispose()
        {
            data.resourceSet(null, this);
        }

        protected override void updateAction(double ds, bool day)
        {
            stationProblem = false;
            go();

            if (day)
            {
                double fetch = employees().fetchBonus();
                employees().fetchBonusConsume((int)fetch);
                fetch *= TIME.secondsPerDayI();
                fetch = CLAMP.d(fetch / employees().employed(), 0, 1);
                if (fetchTime == 0)
                    fetchTime = (float)fetch;
                else
                    fetchTime = (float)CLAMP.d(4.0 * fetchTime / 5.0 + fetch / 5.0, 0, 1);
            }

            if (coolFetch > 0)
            {
                coolFetch--;
            }
            if (day)
            {
                lastSourceX = -1;
            }
            foreach (MoveOrderPull o in pullOrders)
            {
                if (o != null && o.cooldown > 0)
                    o.cooldown--;
            }

            jobs.searchAgain();
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        public override ROOM_TRANSPORT blueprintI()
        {
            return (ROOM_TRANSPORT)blueprint();
        }

        public void reportMoved(int dist)
        {
            if (distance == 0)
                distance = dist;
            else
                distance = (float)(distance * 4 / 5 + dist / 5.0);
        }

        public override JOB_MANAGER getWork()
        {
            return jobs;
        }

        void go()
        {
            if (!data.canGo())
                return;

            if (stationProblem)
                return;

            COORDINATE c = SETT.ROOMS().STATION.reserve(resource());

            if (c == null)
            {
                stationProblem = true;
                return;
            }
            byte ran = (byte)SETT.tileRan(sx, sy);
            DIR d = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(sx, sy).rotation);
            if (SETT.HALFENTS().transports.loader(sx + d.x(), sy + d.y(), ran, resource(), ROOM_TRANSPORT.MAX_LOAD, d, c))
            {
                double w = SETT.ROOMS().STATION.workersPerload(c.x(), c.y());
                if (stationWorkers == 0)
                    stationWorkers = (float)w;
                else
                    stationWorkers = (float)(stationWorkers * 4 / 5 + w / 5.0);

                data.go();

            }
            else
            {
                SETT.ROOMS().STATION.reserveCancel(resource(), c.x(), c.y());
                stationProblem = true;
            }
        }

        public void finishDeliveryJob(int am)
        {
            data.deliver(am);
        }

        public override bool searching()
        {
            return true;
        }

        public override TILE_STORAGE storage(int tx, int ty)
        {
            return blueprintI().job.storage(tx, ty);
        }

        private static class Jobs : JOB_MANAGER
        {
            private readonly TransportInstance instance;

            public Jobs(TransportInstance instance)
            {
                this.instance = instance;
            }

            public override IEnumerable<JOB> getJobs()
            {
                // Implement job logic here
                yield break;
            }
        }

        public override MoveJob moveJob(Humanoid h)
        {
            return new MoveJob(h, this);
        }

        public override int moveMaxRadius()
        {
            return 400;
        }

        public override int radius()
        {
            return (this.radius + 5) * 8;
        }

        public override byte radiusRaw()
        {
            return radius;
        }

        public override void radiusRawSet(byte r)
        {
            this.radius = r;
        }

        public RESOURCE resource()
        {
            return data.resource();
        }

        public double efficiency()
        {
            double d = CLAMP.d((double)employees().employed() / jobs.size(), 0, 1);
            d *= d;
            d += 8.0 / ROOM_TRANSPORT.MAX_LOAD;
            return d * (1 - getDegrade());
        }

        public override void copyFrom(MoveOrderPullInstance same)
        {
            TransportInstance ins = (TransportInstance)same;
            data.resourceSet(ins.resource(), this);
            fetchingSet(ins.fetching());
            prio = ins.prio;
            coolFetch = 0;
            radius = ins.radius;
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
            int ri = -1;
            private MoveOrderPull[] orders;

            public State(TransportInstance ins, bool broken) : base(ins)
            {
                this.broken = broken;
                this.fetching = ins.fetching;
                this.prio = ins.prio;
                if (broken)
                {
                    this.orders = ins.pullOrders;
                }
                ri = ins.resource() == null ? -1 : ins.resource().index();
            }

            public override void applyIns(RoomInstance ins)
            {
                if (ins is TransportInstance)
                {
                    TransportInstance s = (TransportInstance)ins;
                    if (ri >= 0)
                        s.data.resourceSet(RESOURCES.ALL().getC(ri), s);

                    if (broken)
                    {
                        for (int i = 0; i < orders.Length; i++)
                        {
                            if (orders[i] != null)
                            {
                                MoveOrderPull p = new MoveOrderPull(orders[i].destCoo(), orders[i].resbits);
                                s.pullOrders[i] = p;
                            }
                        }
                    }
                    s.fetchingSet(fetching);
                    if (prio != s.prio)
                        prio = s.prio;
                }
            }
        }
    }
}