using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.misc.job;
using settlement.room.main.util;
using snake2d.util.datatypes;

class PumpJob : SETT_JOB
{
    private readonly ROOM_PUMP print;
    private readonly Coo coo = new Coo();
    private PumpInstance ins;

    private readonly RoomBits bReserved = new RoomBits(coo, 0b0_0001);
    private readonly RoomBits bWorked = new RoomBits(coo, 0b0_0010);

    public PumpJob(ROOM_PUMP print)
    {
        this.print = print;
    }

    public SETT_JOB Init(int tx, int ty, PumpInstance ins)
    {
        if (!ins.Is(tx, ty))
            return null;
        if (!print.constructor.IsJob(tx, ty))
            return null;
        this.ins = ins;
        coo.Set(tx, ty);
        return this;
    }

    public override bool JobReserveCanBe()
    {
        if (JobReservedIs(null))
            return false;
        return true;
    }

    public override COORDINATE JobCoo()
    {
        return coo;
    }

    public override string JobName()
    {
        return print.employment().verb;
    }

    public override bool JobUseTool()
    {
        return false;
    }

    public bool Working(int data)
    {
        return bWorked.Get(data) != 0;
    }

    public override RBIT JobResourceBitToFetch()
    {
        return null;
    }

    public override double JobPerformTime(Humanoid skill)
    {
        return wv;
    }

    public override void JobReserve(RESOURCE r)
    {
        if (JobReservedIs(null))
            throw new RuntimeException();
        bReserved.Set(ins, 1);
    }

    public override bool JobReservedIs(RESOURCE r)
    {
        return bReserved.Get() == 1;
    }

    public override void JobReserveCancel(RESOURCE r)
    {
        bReserved.Set(ins, 0);
        bWorked.Set(ins, 0);
    }

    public override void JobStartPerforming()
    {
        bWorked.Set(ins, 1);
    }

    public override SoundRace JobSound()
    {
        return ins.blueprintI().employment().sound();
    }

    private readonly double wv = 45;

    public override RESOURCE JobPerform(Humanoid s, RESOURCE res, int ram)
    {
        JobReserveCancel(res);
        return null;
    }
}