using settlement.room.law.guard;

public class Service : FSERVICE
{
    private GuardInstance ins;
    private int x, y;
    private readonly ROOM_GUARD b;

    public Service(ROOM_GUARD blue)
    {
        this.b = blue;
    }

    public Service Get(GuardInstance ins)
    {
        this.ins = ins;
        x = ins.Body().cX();
        y = ins.Body().cY();
        return this;
    }

    public override bool FindableReservedCanBe()
    {
        return b.reporter.available(ins);
    }

    public override void FindableReserve()
    {
        // TODO Auto-generated method stub
    }

    public override bool FindableReservedIs()
    {
        return true;
    }

    public override void FindableReserveCancel()
    {
        // TODO Auto-generated method stub
    }

    public override int X()
    {
        return x;
    }

    public override int Y()
    {
        return y;
    }

    public override void Consume()
    {
        // TODO Auto-generated method stub
    }
}