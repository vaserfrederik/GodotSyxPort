using System;
using System.Collections.Generic;
using snake2d;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.service.module;
using util.rendering;

public sealed class BathInstance : RoomInstance, JOBMANAGER_HASER, ROOM_SERVICER, ROOM_PRODUCER_INSTANCE
{
    private static readonly long serialVersionUID = 1L;

    private readonly Jobs jobs;

    private double heat = 0;

    private readonly RoomServiceInstance service;

    private readonly ArrayCooShort benches;
    private int benchI;
    private long[] pData;
    private bool auto = true;

    private float water;
    private short waterTiles = 0;
    private short waterCount = 0;

    protected BathInstance(ROOM_BATH blue, TmpArea area, RoomInit init) : base(blue, area, init)
    {
        int s = 0;
        int b = 0;

        foreach (COORDINATE c in body())
        {
            if (!is(c))
                continue;
            if (ROOMS().fData.tile.is(c))
            {
                int d = ROOMS().fData.tile.get(c).data();
                ROOMS().data.set(this, c, d);
                if ((d & Bits.BITS) == Bits.BENCH)
                    b++;
            }
            if (SETT.ENV().map.WATER_SWEET.get(c) > 0)
                water++;
        }

        water *= 1.5f;
        water /= area();
        water = (float)CLAMP.d(water, 0, 1);

        jobs = new Jobs(this);

        benches = new ArrayCooShort(b);

        foreach (COORDINATE c in body())
        {
            if (!is(c))
                continue;
            int d = ROOMS().data.get(c);
            if ((d & Bits.BITS) == Bits.SERVICE)
                s += Bath.initService(c.x(), c.y(), this);
            if ((d & Bits.BITS) == Bits.BENCH)
                benches.set(--b).set(c);
        }

        service = new RoomServiceInstance(s, blueprintI().data);

        employees().maxSet(jobs.size());
        employees().neededSet((int)Math.Ceiling(jobs.size() / 1.5f));
        pData = blue.consumtion.makeData();
        activate();
    }

    protected override void loadFix()
    {
        pData = industry().makeDataFix(pData);
    }

    protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
    {
        it.lit();
        return base.render(r, shadowBatch, it);
    }

    protected override void updateAction(double updateInterval, bool day)
    {
        if (day)
            service.updateDay();
        blueprintI().consumtion.updateRoom(this);
        heat -= updateInterval * coalPerDay() * TIME.secondsPerDayI();
        if (heat < 0)
            heat = 0;

        jobs.searchAgain();
    }

    public override void updateTileDay(int tx, int ty)
    {
        waterCount++;
        waterTiles += SETT.ENV().map.WATER_SWEET.get(tx, ty) > 0 ? (short)1 : (short)0;

        if (waterCount >= area())
        {
            water = (float)CLAMP.d((1.5f * waterTiles) / area(), 0, 1);
            waterCount = 0;
            waterTiles = 0;
        }

        base.updateTileDay(tx, ty);
    }

    private double coalPerDay()
    {
        return service.total() * blueprintI().industries().get(0).ins().get(0).rate;
    }

    public override JOB_MANAGER getWork()
    {
        return jobs;
    }

    public COORDINATE getBench()
    {
        if (benchI == benches.size())
            return null;
        return benches.set(benchI++);
    }

    public void returnBench(int tx, int ty)
    {
        if (!is(tx, ty))
            return;
        int data = ROOMS().data.get(tx, ty);
        if ((data & Bits.BITS) != Bits.BENCH)
            return;
        if (benchI == 0)
            return;
        benchI--;
        benches.set(benchI).set(tx, ty);
    }

    protected override void dispose()
    {
        foreach (COORDINATE c in body())
        {
            Bath b = blueprintI().bath(c.x(), c.y());
            if (b != null)
                b.dispose();
        }
        service.dispose(blueprintI().data);
    }

    public double getHeat()
    {
        double d = heat / service.total();
        if (d > 1)
            d = 1;
        return d;
    }

    public override ROOM_BATH blueprintI()
    {
        return (ROOM_BATH)blueprint();
    }

    public override RoomServiceInstance service()
    {
        return service;
    }

    protected override void activateAction()
    {
        // TODO Auto-generated method stub
    }

    protected override void deactivateAction()
    {
        // TODO Auto-generated method stub
    }

    public override double quality()
    {
        double h = 0.5 + 0.5 * getHeat();
        double w = 1; //0.5 + 0.5 * water;
        double b = 0.5 + 0.5 * blueprintI().constructor.relaxation.get(this);
        return ROOM_SERVICER.defQuality(this, b * h * w);
    }

    public override long[] productionData()
    {
        return pData;
    }

    public override Industry industry()
    {
        return blueprintI().industries().get(0);
    }

    public override int industryI()
    {
        // TODO Auto-generated method stub
        return 0;
    }

    private class Jobs : JobPositions<BathInstance>
    {
        private static readonly long serialVersionUID = 1L;

        public Jobs(BathInstance ins) : base(ins)
        {
        }

        protected override SETT_JOB get(int tx, int ty)
        {
            SETT_JOB j = Crank.init(tx, ty, ins.blueprintI());
            if (j == null)
                return Oven.init(tx, ty, ins.blueprintI());
            return j;
        }

        protected override bool isAndInit(int tx, int ty)
        {
            return Crank.init(tx, ty, ins.blueprintI()) != null || Oven.init(tx, ty, ins.blueprintI()) != null;
        }
    }
}