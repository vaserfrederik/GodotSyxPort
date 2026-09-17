using System;
using game;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d;
using snake2d.util.bit;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.rendering;

public abstract class RoomResDeposit : SETT_JOB
{
    private static readonly Bits[] AMOUNTS = new Bits[] {
        new Bits(0b0000_0000_0000_0000_0000_0000_0001_1111),
        new Bits(0b0000_0000_0000_0000_0000_0011_1110_0000),
        new Bits(0b0000_0000_0000_0000_0111_1100_0000_0000),
        new Bits(0b0000_0000_0000_1111_1000_0000_0000_0000),
    };

    private static readonly Bit[] RESERVED = new Bit[] {
        new Bit(0b0000_0000_0001_0000_0000_0000_0000_0000),
        new Bit(0b0000_0000_0010_0000_0000_0000_0000_0000),
        new Bit(0b0000_0000_0100_0000_0000_0000_0000_0000),
        new Bit(0b0000_0000_1000_0000_0000_0000_0000_0000),
    };

    private static readonly Bit WRESERVED = new Bit(0b0000_0001_0000_0000_0000_0000_0000_0000);
    private static readonly Bit WUSED = new Bit(0b0000_0010_0000_0000_0000_0000_0000_0000);
    private static readonly int ww = 45;
    protected Coo coo = new Coo();
    protected int data;
    protected static readonly string name = "Gettings raw materials";
    private ROOM_PRODUCER_INSTANCE ins;
    private RoomInstance insi;
    private readonly RoomBlueprintImp blue;

    protected RoomResDeposit(RoomBlueprintImp blue)
    {
        this.blue = blue;
    }

    public RoomResDeposit Get(int tx, int ty, RoomInstance i)
    {
        if (i != null && i.Is(tx, ty) && i is ROOM_PRODUCER_INSTANCE && Is(tx, ty))
        {
            coo.Set(tx, ty);
            data = ROOMS().data.Get(tx, ty);
            insi = i;
            ins = (ROOM_PRODUCER_INSTANCE)i;
            return this;
        }
        return null;
    }

    protected abstract bool Is(int tx, int ty);

    public int Amount(int index)
    {
        return AMOUNTS[index].Get(data);
    }

    public bool WithDraw(int ri, int amount)
    {
        AmountSet(Amount(ri) - amount, ri);
        Save();
        return Amount(ri) > 0;
    }

    private void AmountSet(int value, int index)
    {
        data = AMOUNTS[index].Set(data, value);
    }

    private void Save()
    {
        ROOMS().data.Set((RoomInstance)ins, coo, data);
    }

    public override RBIT JobResourceBitToFetch()
    {
        RBITImp bits = new RBITImp();
        bits.Clear();
        for (int i = 0; i < ResAm(); i++)
        {
            if (!RESERVED[i].Is(data))
            {
                if (Amount(i) < 1)
                    bits.Or(Res(i));
            }
        }

        return bits.IsClear() ? null : bits;
    }

    public override int JobResourcesNeeded(Humanoid skill)
    {
        return 0b01111;
    }

    public override double JobPerformTime(Humanoid skill)
    {
        return ww;
    }

    public override void JobStartPerforming()
    {
        data = WUSED.Set(data, true);
        Save();
    }

    public bool Working(int data)
    {
        return WUSED.Is(data);
    }

    public override SoundRace JobSound()
    {
        return null;
    }

    public override RESOURCE JobPerform(Humanoid skill, RESOURCE res, int ram)
    {
        if (res == null)
        {
            data = WRESERVED.Set(data, false);
            data = WUSED.Set(data, false);
            Save();

            if (!RegularJobCanBeReserved(coo))
                return null;

            int ri = 0;
            final double w = insi.Employees().FetchBonus(ww);

            foreach (IndustryResource r in ins.Industry().Ins())
            {
                int a = r.Work(skill, ins, w);
                if (a > 0)
                {
                    int max = Amount(ri);
                    a = SETT.ROOMS().ResourceUnderflow.Withdraw(r.Resource, a, max);
                    WithDraw(ri, a);
                }
                ri++;
            }

            int am = ins.Industry().Outs().Get(0).Work(skill, ins, w);
            if (am > 0)
                RegularJobStore(coo, am);

            return null;
        }

        bool has = true;

        for (int i = 0; i < ResAm(); i++)
        {
            if (Res(i) == res)
            {
                data = RESERVED[i].Clear(data);
                ram = SETT.ROOMS().ResourceUnderflow.Deposit(res, ram);
                if (ram > 0)
                {
                    ram = CLAMP.I(ram, 0, JobResourcesNeeded(skill));
                    AmountSet(Amount(i) + ram, i);
                }
            }
            has &= Amount(i) > 0;
        }

        Save();
        if (has)
            HasCallback();
        return null;
    }

    protected abstract void HasCallback();

    public override void JobReserve(RESOURCE r)
    {
        if (r != null)
        {
            for (int i = 0; i < ResAm(); i++)
            {
                if (Res(i) == r)
                {
                    data = RESERVED[i].Set(data);
                    Save();
                    return;
                }
            }
        }
        else if (!WRESERVED.Is(data))
        {
            data = WRESERVED.Set(data);
            Save();
            return;
        }

        throw new Exception("" + r);
    }

    public override bool JobReservedIs(RESOURCE r)
    {
        if (r == null)
        {
            return WRESERVED.Is(data);
        }

        for (int i = 0; i < ResAm(); i++)
        {
            if (Res(i) == r)
            {
                return RESERVED[i].Is(data);
            }
        }

        return false;
    }

    public override void JobReserveCancel(RESOURCE r)
    {
        if (r == null)
        {
            data = WRESERVED.Set(data, false);
            data = WUSED.Set(data, false);
            Save();
            return;
        }
        else
        {
            for (int i = 0; i < ResAm(); i++)
            {
                if (Res(i) == r)
                {
                    if (RESERVED[i].Is(data))
                    {
                        data = RESERVED[i].Clear(data);
                        Save();
                    }
                    return;
                }
            }
        }

        GAME.Notify("" + r);
    }

    public override COORDINATE JobCoo()
    {
        return coo;
    }

    public override string JobName()
    {
        return WRESERVED.Is(data) ? blue.Employment().Verb : name;
    }

    public override bool JobUseTool()
    {
        return false;
    }

    public RESOURCE Res(int index)
    {
        return ins.Industry().Ins().Get(index).Resource;
    }

    public int ResAm()
    {
        return ins.Industry().Ins().Size();
    }

    public void Dispose()
    {
        for (int i = 0; i < ResAm(); i++)
        {
            if (Amount(i) > 0)
            {
                bool unload = false;
                for (int di = 0; di < DIR.ALL.Size(); di++)
                {
                    int dx = coo.X() + DIR.ALL.Get(di).X();
                    int dy = coo.Y() + DIR.ALL.Get(di).Y();
                    if (SETT.PATH().Connectivity.Is(dx, dy))
                    {
                        unload = true;
                        THINGS().Resources.Create(dx, dy, Res(i), Amount(i));
                        break;
                    }
                }
                if (!unload)
                {
                    THINGS().Resources.Create(coo, Res(i), Amount(i));
                }
            }
        }
        data = 0;
        Save();
    }

    protected abstract bool RegularJobCanBeReserved(Coo coo);
    protected abstract void RegularJobStore(Coo coo, int amount);
}