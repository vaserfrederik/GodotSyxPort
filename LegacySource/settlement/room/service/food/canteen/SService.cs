using settlement.room.service.food.canteen;
using init.resources;
using settlement.main;
using settlement.misc.util;
using snake2d.util.bit;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.data.INT;

class SService : FSERVICE
{
    public const int I = 2;
    public const int MAX = 3;
    private readonly ROOM_CANTEEN e;
    private CanteenInstance ins;
    private readonly Coo coo = new Coo();

    private int data;

    private readonly INTE reserved = new INTE()
    {
        private readonly Bits bits = new Bits(0x000F);

        public int min()
        {
            return 0;
        }

        public int max()
        {
            return CLAMP.i(MAX, 0, RESOURCES.EDI().all().size());
        }

        public int get()
        {
            return bits.get(data);
        }

        public void set(int t)
        {
            data = bits.set(data, t);
        }
    };

    private readonly INTE available = new INTE()
    {
        private readonly Bits bits = new Bits(0x00F0);

        public int min()
        {
            return 0;
        }

        public int max()
        {
            return CLAMP.i(MAX, 0, RESOURCES.EDI().all().size());
        }

        public int get()
        {
            return bits.get(data);
        }

        public void set(int t)
        {
            data = bits.set(data, t);
        }
    };

    private int total()
    {
        return reserved.get() + available.get();
    }

    SService(ROOM_CANTEEN e)
    {
        this.e = e;
    }

    SService get(int tx, int ty)
    {
        if (e.is(tx, ty) && SETT.ROOMS().fData.tileData.get(tx, ty) == I)
        {
            ins = e.getter.get(tx, ty);
            coo.set(tx, ty);
            data = SETT.ROOMS().data.get(tx, ty);
            return this;
        }
        return null;
    }

    //int consume(int am)
    //{
    //    am = CLAMP.i(am, 0, available.get());

    //    if (ins.amountTotal() > ins.serviceReserved() && total() < MAX)
    //    {
    //        available.inc(1);
    //    }

    //    if (ins.amountTotal() <= 0 || (ins.amountTotal() < ins.serviceReserved()))
    //    {
    //        if (available.get() > 0)
    //            available.inc(-1);
    //        else if (reserved.get() > 0)
    //            reserved.inc(-1);
    //    }
    //    save();
    //}

    void check()
    {
        if (ins.amountTotal() > ins.serviceReserved() && total() < MAX)
        {
            available.inc(1);
        }

        if (ins.amountTotal() <= 0 || (ins.amountTotal() < ins.serviceReserved()))
        {
            if (available.get() > 0)
                available.inc(-1);
            else if (reserved.get() > 0)
                reserved.inc(-1);
        }
        save();
    }

    private void save()
    {
        int tmp = data;
        data = SETT.ROOMS().data.get(coo);
        if (tmp == data)
            return;
        ins.serviceTally(-available.get());
        ins.service.report(this, e.service, -1, available.get() - reserved.get(), reserved.get());

        data = tmp;
        SETT.ROOMS().data.set(ins, coo, data);

        ins.service.report(this, e.service, 1, available.get() - reserved.get(), reserved.get());
        ins.serviceTally(available.get());
    }

    void dispose(int tx, int ty)
    {
        if (get(tx, ty) != null)
        {
            available.set(0);
            reserved.set(0);
            save();
        }
    }

    public bool findableReservedCanBe()
    {
        return available.get() > reserved.get();
    }

    public void findableReserve()
    {
        reserved.inc(1);
        save();
    }

    public bool findableReservedIs()
    {
        return reserved.get() > 0;
    }

    public void findableReserveCancel()
    {
        reserved.inc(-1);
        save();
        check();
    }

    public int x()
    {
        return coo.x();
    }

    public int y()
    {
        return coo.y();
    }

    public void consume()
    {
        findableReserveCancel();
    }
}