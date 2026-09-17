using System;
using settlement.room.industry.module.consumption;
using game.audio;
using init.resources;
using init.resources.RBIT;
using init.resources.RESOURCE;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;
using util.data;

public abstract class ConsumptionJob : SETT_JOB
{
    protected readonly Coo coo = new Coo();
    private readonly RoomBits reserved = new RoomBits(coo, new Bits(0b0110_0000_0000_0000_0000_0000_0000_0000));
    private readonly RoomBits used = new RoomBits(coo, new Bits(0b0010_0000_0000_0000_0000_0000_0000_0000));

    protected RoomInstance ins;
    protected ROOM_IDATA_INSTANCE insc;
    protected readonly int time;
    protected readonly RoomBlueprintIns<?> blue;
    protected readonly RoomConsumption bluec;
    private readonly BOOLEANCoo is;

    public ConsumptionJob(RoomBlueprintIns<?> b, RoomConsumption cons, int time, BOOLEANCoo is)
    {
        this.blue = b;
        this.bluec = cons;
        this.time = time;
        this.is = is;
    }

    public SETT_JOB Get(int tx, int ty)
    {
        ins = blue.Get(tx, ty);
        if (ins == null)
            return null;
        if (is.Is(tx, ty))
        {
            coo.Set(tx, ty);
            insc = (ROOM_IDATA_INSTANCE)ins;
            return this;
        }
        return null;
    }

    public bool Used(int tx, int ty)
    {
        return used.Get(SETT.ROOMS().data.Get(tx, ty)) == 1;
    }

    public override void JobReserve(RESOURCE r)
    {
        if (reserved.Get() == 1)
        {
            throw new Exception();
        }
        reserved.Set(ins, 1);

        if (r != null)
        {
            IndustryResource rr = bluec.In(r);
            if (rr == null)
                throw new Exception();
            reserved.Set(ins, 1);
            bluec.Reserved(rr).Inc(insc, 1);
        }
    }

    public override bool JobReservedIs(RESOURCE r)
    {
        return reserved.Get() == 1;
    }

    public override void JobReserveCancel(RESOURCE r)
    {
        reserved.Set(ins, 0);
        used.Set(ins, 0);

        if (r == null)
            return;
        IndustryResource rr = bluec.In(r);
        if (rr == null)
            return;
        bluec.Reserved(rr).Inc(insc, -1);
    }

    public override bool JobReserveCanBe()
    {
        if (JobReservedIs(null))
            return false;

        return true;
    }

    private readonly RBITImp resBit = new RBITImp();
    public override RBIT JobResourceBitToFetch()
    {
        resBit.Clear();
        foreach (IndustryResource in in bluec.Ins())
        {
            if (bluec.ShouldFecth(in, insc, ins))
            {
                resBit.Or(in.Resource.Bit);
            }
        }
        return resBit.IsClear() ? null : resBit;
    }

    public override double JobPerformTime(Humanoid skill)
    {
        return time;
    }

    public override void JobStartPerforming()
    {
        used.Set(ins, 1);
    }

    public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
    {
        JobReserveCancel(r);

        if (r != null)
        {
            IndustryResource rr = bluec.In(r);
            if (rr == null)
                return null;
            if (bluec.Enabled(rr, insc))
            {
                ram = SETT.ROOMS().resourceUnderflow.Deposit(r, ram);
                bluec.Stored(rr).Inc(insc, ram);
            }
            else
                SETT.THINGS().resources.Create(skill.Tc(), r, ram);
            return null;
        }

        int t = ins.Employees().FetchBonus(time);
        double d = IndustryUtil.CalcProductionRate(1, skill, bluec, ins);

        foreach (IndustryResource in bluec.Ins())
        {
            if (bluec.Stored(in).Get(insc) > 0)
            {
                int a = in.Work(skill, insc, t);
                if (a > 0)
                {
                    a = SETT.ROOMS().resourceUnderflow.Withdraw(in.Resource, a, bluec.Stored(in).Get(insc));
                    bluec.Stored(in).Inc(insc, -a);
                }
            }
        }

        Perform(t, d);
        return null;
    }

    protected abstract void Perform(double time, double skill);

    public override COORDINATE JobCoo()
    {
        return coo;
    }

    public override string JobName()
    {
        return blue.Employment().Verb;
    }

    public override SoundRace JobSound()
    {
        return blue.Employment().Sound();
    }
}