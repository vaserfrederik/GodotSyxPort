using System;
using System.Collections.Generic;
using init.resources;
using settlement.main;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.law;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using view.sett.ui.room;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;

public sealed class ROOM_STOCKADE : RoomBlueprintIns<StockInstance>, PUNISHMENT_SERVICE
{
    private Constructor constructor;
    private Industry indu;
    private static readonly double PRISONER_PER_TILE = 1.0 / 4.0;
    private Job job;

    private int prisoners;
    private int prisonersMax;

    public ROOM_STOCKADE(RoomInitData data, RoomCategorySub cat) : base(0, data, "_STOCKADE", cat)
    {
        this.constructor = new Constructor(this, data);
        indu = new Industry(this, RESOURCES.EDI().makeArray(), new double[RESOURCES.EDI().all().size()], null);
        this.job = new Job(this);
    }

    protected override void update(double ds)
    {
    }

    public override SFinderRoomService service(int tx, int ty)
    {
        return null;
    }

    protected override void saveP(FilePutter saveFile)
    {
        indu.save(saveFile);
    }

    protected override void loadP(FileGetter saveFile)
    {
        indu.load(saveFile);
        prisoners = 0;
        prisonersMax = 0;
        foreach (StockInstance ins in all())
        {
            prisoners += ins.prisonersCurrent;
            if (ins.active())
            {
                prisonersMax += ins.prisonersMax;
            }
        }
    }

    protected override void clearP()
    {
        indu.clear();
        prisoners = 0;
        prisonersMax = 0;
    }

    public override bool degrades()
    {
        return false;
    }

    public override Furnisher constructor()
    {
        return constructor;
    }

    public override void appendView(LISTE<UIRoomModule> mm)
    {
        mm.add(new Gui(this).make());
    }

    public override double degradeRate()
    {
        return 0;
    }

    public RoomInstance registerPrisoner(COORDINATE current)
    {
        if (prisoners >= prisonersMax)
            return null;

        {
            StockInstance ins = getter.get(current);
            if (ins != null && ins.active() && ins.prisonersCurrent < ins.prisonersMax)
            {
                if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY()))
                {
                    ins.prisonersCurrent++;
                    prisoners++;
                    return ins;
                }

            }

        }

        int i = RND.rInt(instancesSize());
        for (int k = 0; k < instancesSize(); k++)
        {
            StockInstance ins = getInstance((k + i) % instancesSize());
            if (ins.active() && ins.prisonersCurrent < ins.prisonersMax)
            {
                if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY()))
                {
                    ins.prisonersCurrent++;
                    prisoners++;
                    return ins;
                }
            }
        }
        LOG.ln("nopes");
        return null;
    }

    public void unregisterPrisoner(COORDINATE c)
    {
        StockInstance ins = getter.get(c);
        if (ins != null && ins.active())
        {
            ins.prisonersCurrent--;
            prisoners--;
        }
    }

    public void unregisterPrisoner(int tx, int ty)
    {
        StockInstance ins = getter.get(tx, ty);
        if (ins != null && ins.active())
        {
            ins.prisonersCurrent--;
            prisoners--;
        }
    }

    public COORDINATE foodReserve(COORDINATE c)
    {
        StockInstance ins = getter.get(c);
        if (ins != null)
        {

            int ri = RND.rInt(ins.jobs.size());

            for (int i = 0; i < ins.jobs.size(); i++)
            {
                COORDINATE coo = ins.jobs.get((ri + i) % ins.jobs.size());
                if (job.reserve(coo.x(), coo.y(), Job.IFOOD, true, false))
                    return coo;
            }
        }
        return null;
    }

    public void foodUse(COORDINATE c, bool use)
    {
        job.reserve(c.x(), c.y(), Job.IFOOD, false, use);
    }

    public COORDINATE latrineReserve(COORDINATE c)
    {
        StockInstance ins = getter.get(c);
        if (ins != null)
        {

            int ri = RND.rInt(ins.jobs.size());

            for (int i = 0; i < ins.jobs.size(); i++)
            {
                COORDINATE coo = ins.jobs.get((ri + i) % ins.jobs.size());
                if (job.reserve(coo.x(), coo.y(), Job.ISHIT, true, false))
                    return coo;
            }
        }
        return null;
    }

    public void latrineUse(COORDINATE c, bool use)
    {
        job.reserve(c.x(), c.y(), Job.ISHIT, false, use);
    }

    public bool isWithin(int nx, int ny, COORDINATE cell)
    {
        StockInstance ins = getter.get(nx, ny);
        if (ins != null && ins.is(cell))
        {
            for (int di = 0; di < DIR.ALL.size(); di++)
            {
                if (!ins.is(cell, DIR.ALL.get(di)))
                    return false;
            }
            return true;
        }
        return false;

    }

    public override int punishTotal()
    {
        return prisonersMax;
    }

    public override int punishUsed()
    {
        return prisoners;
    }
}