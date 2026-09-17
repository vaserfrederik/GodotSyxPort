using System;
using settlement.room.service.food.canteen;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.infra.stockpile;
using settlement.room.main.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

class SWork : SETT_JOB
{
    public const int I = 1;

    private CanteenInstance ins;
    private readonly Coo coo = new Coo();
    private readonly ROOM_CANTEEN b;
    private const int wt = 60;

    private readonly RoomBits bReserved = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
    private readonly RoomBits bFreeFetc = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010));
    private readonly RoomBits bResource = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));
    private readonly RoomBits bResAmoun = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_1111_0000_0000_0000));
    private readonly RoomBits bCoalAmou = new RoomBits(coo, new Bits(0b0000_0000_0000_1111_0000_0000_0000_0000));

    public SWork(ROOM_CANTEEN b)
    {
        this.b = b;
        if (false)
        {
            //make a separate storage tile that everyone can fetch to, and have the cooks cook whatever they want.
        }
    }

    public SWork Get(int tx, int ty)
    {
        if (b.Is(tx, ty) && SETT.ROOMS().fData.tileData.Get(tx, ty) == I)
        {
            ins = b.getter.Get(tx, ty);
            coo.Set(tx, ty);
            return this;
        }
        return null;
    }

    public void Dispose(int x, int y)
    {
        if (Get(x, y) != null)
        {
            if (Res() != null)
                SETT.THINGS().resources.Create(coo, Res().resource, bResAmoun.Get());
            if (bCoalAmou.Get() > 0)
                SETT.THINGS().resources.Create(coo, b.industryFuel.ins().Get(0).resource, bCoalAmou.Get());
        }
    }

    public ResG Res()
    {
        if (bResAmoun.Get() > 0)
            return RESOURCES.EDI().All().Get(bResource.Get());
        return null;
    }

    public int ResAm()
    {
        return bResAmoun.Get();
    }

    public bool HasCoal()
    {
        return bCoalAmou.Get() > 0;
    }

    public void JobReserve(RESOURCE r)
    {
        bReserved.Set(ins, 1);
        if (r != null)
        {
            ResG g = RESOURCES.EDI().Get(r);
            if (g != null)
            {
                ins.Tally(g, 0, JobResourcesNeeded(null));
            }
        }
    }

    public bool JobReservedIs(RESOURCE r)
    {
        return bReserved.Get() == 1;
    }

    public void JobReserveCancel(RESOURCE r)
    {
        bReserved.Set(ins, 0);
        if (r != null)
        {
            ResG g = RESOURCES.EDI().Get(r);
            if (g != null)
            {
                ins.Tally(g, 0, -JobResourcesNeeded(null));
            }
        }
    }

    public bool JobReserveCanBe()
    {
        if (bReserved.Get() == 1)
            return false;
        if (bCoalAmou.Get() == 0)
            return true;
        if (Res() == null)
            return !ins.FetchMask().IsClear();
        return true;
    }

    public RBIT JobResourceBitToFetch()
    {
        if (bCoalAmou.Get() == 0)
            return b.industryFuel.ins().Get(0).resource.bit;
        if (Res() == null)
            return ins.FetchMask();
        return null;
    }

    public int JobResourcesNeeded(Humanoid skill)
    {
        return ROOM_STOCKPILE.MIN_CARRY;
    }

    public double JobPerformTime(Humanoid skill)
    {
        if (bFreeFetc.Get() == 1)
            return 0;
        return wt;
    }

    public void JobStartPerforming()
    {
        // TODO Auto-generated method stub
    }

    public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
    {
        JobReserveCancel(r);
        if (r == b.industryFuel.ins().Get(0).resource)
        {
            ram = SETT.ROOMS().resourceUnderflow.Deposit(r, ram);
            bCoalAmou.Inc(ins, ram);
        }
        else if (r != null)
        {
            ResG g = RESOURCES.EDI().Get(r);
            if (g == null)
                return null;
            bResource.Set(ins, g.index());
            ram = SETT.ROOMS().resourceUnderflow.Deposit(r, ram);
            bResAmoun.Inc(ins, ram);
            ins.Tally(g, 0, bResAmoun.Get());
        }
        else
        {
            ResG g = Res();
            if (g == null)
                return null;
            ins.Tally(g, 1, -bResAmoun.Get());
            bResAmoun.Inc(ins, -1);
            ins.Tally(g, 0, bResAmoun.Get());
            bFreeFetc.Set(ins, 0);

            int am = ins.industry().ins().Get(0).Work(skill, ins, wt);
            am = SETT.ROOMS().resourceUnderflow.Withdraw(ins.industry().ins().Get(0).resource, am, bCoalAmou.Get());
            bCoalAmou.Inc(ins, -am);

            for (int di = 0; di < DIR.ORTHO.size(); di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                if (b.food.Get(coo.X() + d.X(), coo.Y() + d.Y()) != null)
                {
                    b.food.Check();
                }
            }
        }

        if (bFreeFetc.Get() == 0 && ins.Employees().FetchBonus() >= wt)
        {
            bFreeFetc.Set(ins, 1);
            ins.Employees().FetchBonusConsume(wt);
        }

        return null;
    }

    public COORDINATE JobCoo()
    {
        return coo;
    }

    public CharSequence JobName()
    {
        return b.employment().verb;
    }

    public bool JobUseTool()
    {
        return false;
    }

    public SoundRace JobSound()
    {
        return b.employment().sound();
    }
}