using System;
using System.Collections.Generic;
using settlement.room.service.food.canteen;
using init.resources;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using util.rendering;

namespace settlement.room.service.food.canteen
{
    [Serializable]
    internal sealed class CanteenInstance : RoomInstance, JOBMANAGER_HASER, ROOM_PRODUCER_INSTANCE, ROOM_SERVICER
    {
        private const long serialVersionUID = -7063521835843676015L;

        public bool autoE = true;
        private long[] pdata;
        private int[] amounts;
        private int[] amountIncoming;
        private int amountTotal = 0;
        public readonly int maxAmount;
        private int serviceReserved = 0;

        private readonly JobIterator jobs;
        private readonly RBITImp fetchMask = new RBITImp().or(RESOURCES.EDI().mask);
        private readonly RBITImp useMask = new RBITImp();
        public readonly RoomServiceInstance service;
        public short tableX = -1;
        public short tableY = -1;

        public CanteenInstance(ROOM_CANTEEN p, TmpArea area, RoomInit init) : base(p, area, init)
        {
            jobs = new JobIterator(this)
            {
                init = (tx, ty) => blueprintI().job.get(tx, ty)
            };

            int m = 0;
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                if (tableX == -1 && is(c))
                {
                    tableX = (short)c.x();
                    tableY = (short)c.y();
                }
                if (is(c) && p.food.get(c.x(), c.y()) != null)
                    m++;
            }
            pdata = blueprintI().industryFuel.makeData();
            service = new RoomServiceInstance(m * SService.MAX, blueprintI().service);
            maxAmount = (int)2 * m;

            employees().maxSet((int)Math.Ceiling(blueprintI().constructor.workers.get(this) * 2));
            employees().neededSet((int)Math.Ceiling(blueprintI().constructor.workers.get(this)));
            activate();

            foreach (ResGEat e in RESOURCES.EDI().all())
            {
                if (e.serve)
                {
                    useMask.or(e.resource);
                }
            }

            fetchMask.and(useMask);
        }

        protected override void loadFix()
        {
            pdata = industry().makeDataFix(pdata);
            if (amounts.Length != RESOURCES.EDI().all().Length)
            {
                int[] ams = Alloc.ii(RESOURCES.EDI().all().Length);
                int[] amsI = Alloc.ii(RESOURCES.EDI().all().Length);
                amountTotal = 0;
                fetchMask.clear();
                fetchMask.or(RESOURCES.EDI().mask);
                useMask.clear();
                for (int i = 0; i < ams.Length; i++)
                {
                    ams[i] = amounts[i % amounts.Length];
                    amsI[i] = amountIncoming[i % amountIncoming.Length];
                    amountTotal += ams[i];
                }
                this.amounts = ams;
                this.amountIncoming = amsI;
            }
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            base.render(r, shadowBatch, it);
            it.lit();
            return false;
        }

        protected override void updateAction(double ds, bool day)
        {
            if (day)
                service.updateDay();
            blueprintI().industryFuel.updateRoom(this);
            jobs.searchAgain();
            if (tableX == -1)
            {
                tableX = (short)body().x1();
                tableY = (short)body().y1();
            }
        }

        public int amount(ResG e)
        {
            return amounts[e.index()];
        }

        public int amountReserved(ResG e)
        {
            return amountIncoming[e.index()];
        }

        public int amountTotal()
        {
            return amountTotal;
        }

        public int serviceReserved()
        {
            return serviceReserved;
        }

        public RBIT fetchMask()
        {
            return fetchMask;
        }

        public bool uses(ResG e)
        {
            return useMask.has(e.resource.bit);
        }

        public void usesToggle(ResG e)
        {
            useMask.toggle(e.resource);
            setMask(e);
        }

        public void tally(ResG e, int dAmount, int amountReserved)
        {
            amounts[e.index()] += dAmount;
            this.amountIncoming[e.index()] += amountReserved;
            amountTotal += dAmount;
            blueprintI().total += dAmount;
            blueprintI().amounts[e.index()] += dAmount;
            setMask(e);
        }

        public void serviceTally(int dReserved)
        {
            serviceReserved += dReserved;
        }

        public void consume(ResG e, int amount, int tx, int ty)
        {
            tally(e, -amount, 0);
            blueprintI().food.get(tx, ty).check();
        }

        private void setMask(ResG e)
        {
            if (amounts[e.index()] + amountIncoming[e.index()] < maxAmount)
            {
                fetchMask.or(e.resource);
            }
            else
            {
                fetchMask.clear(e.resource);
            }
            fetchMask.and(useMask);
            if (fetchMask.isClear())
            {
                jobs.dontSearch();
            }
            else
            {
                jobs.searchAgainWithoutResources();
            }
            jobs.resetResourceSearch();
        }

        protected override void dispose()
        {
            int amI = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    int a = amounts[amI];
                    if (a > 0)
                        SETT.THINGS().resources.create(c, RESOURCES.EDI().all().get(amI).resource, a);
                    amI++;
                    if (amI == amounts.Length)
                        return;
                }
            }

            foreach (ResG e in RESOURCES.EDI().all())
                tally(e, -amounts[amI], -amountIncoming[amI]);

            foreach (COORDINATE c in body())
            {
                if (is(c))
                {
                    blueprintI().food.dispose(c.x(), c.y());
                    blueprintI().job.dispose(c.x(), c.y());
                }
            }
            service.dispose(blueprintI().service);
        }

        public override ROOM_CANTEEN blueprintI()
        {
            return (ROOM_CANTEEN)blueprint();
        }

        public override RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return null;
        }

        public override TILE_STORAGE storage(int tx, int ty)
        {
            return null;
        }

        protected override void activateAction()
        {
            // TODO Auto-generated method stub
        }

        protected override void deactivateAction()
        {
            // TODO Auto-generated method stub
        }

        public JOB_MANAGER getWork()
        {
            return jobs;
        }

        public long[] productionData()
        {
            return pdata;
        }

        public Industry industry()
        {
            return blueprintI().industries().get(0);
        }

        public RoomServiceInstance service()
        {
            return service;
        }

        public double quality()
        {
            return ROOM_SERVICER.defQuality(this, 0.2 + 0.8 * blueprintI().constructor.tables.get(this));
        }

        public int industryI()
        {
            // TODO Auto-generated method stub
            return 0;
        }

        public RoomState makeState(int rx, int ry, bool broken)
        {
            return new State(this);
        }

        private sealed class State : RoomState.RoomStateInstance
        {
            private readonly RBITImp useMask = new RBITImp();

            private const long serialVersionUID = 1L;

            public State(CanteenInstance ins) : base(ins)
            {
                useMask.clear(ins.useMask);
            }

            protected override void applyIns(RoomInstance ins)
            {
                if (ins is CanteenInstance i)
                {
                    i.useMask.clear(useMask);
                    foreach (ResG g in RESOURCES.EDI().all())
                    {
                        i.setMask(g);
                    }
                }
                base.applyIns(ins);
            }
        }
    }
}