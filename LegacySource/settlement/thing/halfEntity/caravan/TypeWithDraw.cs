using settlement.thing.halfEntity.caravan;
using game.faction;
using settlement.main;
using settlement.room.infra.export;
using snake2d.util.datatypes;
using util.gui.misc;
using util.text;

class TypeWithDraw : Type
{
    private static short STATE_EXPORT = 0;
    private static short STATE_OTHER = 1;
    private static short STATE_RETURN = 2;

    private static CharSequence ¤¤verb = "¤fetching";

    static
    {
        D.ts(TypeWithDraw.class);
    }

    public TypeWithDraw() : base(¤¤verb)
    {
    }

    public override bool Init(Caravan c, int amount)
    {
        c.reservedGlobally = (short)amount;
        c.amountCarried = 0;
        SETT.HALFENTS().caravans.withdrawals[c.res.index()].Inc(c.tType(), c.reservedGlobally);
        if (fetch(c))
            return true;
        SETT.HALFENTS().caravans.withdrawals[c.res.index()].Inc(c.tType(), -c.reservedGlobally);
        c.reservedGlobally = 0;
        return false;
    }

    private bool fetch(Caravan c)
    {
        c.path.Clear();

        c.tmp = 1;

        c.reserved = 0;
        int target = c.reservedGlobally - c.amountCarried;

        ExportFetcher f = ROOMS().EXPORT.FETCHER;
        COORDINATE coo = f.GetReservableSpot(c.ctx(), c.cty(), c.res);
        if (coo != null)
        {
            int am = f.Reservable(c.res, coo);
            if (am > target)
                am = target;

            if (c.path.Request(c.ctx(), c.cty(), coo.x(), coo.y(), false))
            {
                c.reserved = (short)am;
                f.Reserve(c.res, coo, am);
                c.Move();
                c.state = STATE_EXPORT;
                return true;
            }
        }

        if (SETT.PATH().Finders.resource.Find(c.res().bit, c.res().bit, c.res().bit, c.ctx(), c.cty(), c.path, int.MaxValue) != null)
        {
            c.reserved = 1;
            if (target - 1 > 0)
            {
                c.reserved += SETT.PATH().Finders.resource.ReserveExtra(true, true, c.res, c.path.destX(), c.path.destY(), target - 1);
            }
            c.Move();
            c.state = STATE_OTHER;
            return true;
        }

        return false;
    }

    private bool pickup(Caravan c)
    {
        if (c.reserved <= 0)
            return false;

        ExportFetcher f = ROOMS().EXPORT.FETCHER;
        if (c.state == STATE_EXPORT)
        {
            coo.Set(c.path.destX(), c.path.destY());
            int am = f.Reserved(c.res, coo);
            if (am > 0)
            {
                int max = 1;
                if (max > am)
                    max = am;
                if (max > c.reserved)
                    max = c.reserved;
                f.Finish(c.res, coo, max, c.tType());
                c.amountCarried += max;
                c.reserved -= max;
                return true;
            }
        }
        else if (c.state == STATE_OTHER)
        {
            if (SETT.PATH().Finders.resource.Pickup(c.res, c.path.destX(), c.path.destY(), 1) == 1)
            {
                c.reserved--;
                c.amountCarried++;
                FACTIONS.player().res().Inc(c.res, c.tType().rtype, -1);
                return true;
            }
        }
        else
        {
            throw new RuntimeException("" + c.state);
        }
        c.reserved = 0;
        return false;
    }

    public override bool Update(Caravan c, double ds)
    {
        if (c.state == STATE_RETURN)
            return false;

        if (pickup(c))
            return true;

        if (c.amountCarried < c.reservedGlobally)
        {
            if (fetch(c))
                return true;
        }

        if (PATH().Finders.entryPoints.Find(c.ctx(), c.cty(), c.path, int.MaxValue))
        {
            c.Move();
            c.state = STATE_RETURN;
            return true;
        }

        return false;
    }

    public override void Cancel(Caravan c, bool dump)
    {
        ExportFetcher f = ROOMS().EXPORT.FETCHER;
        if (c.reserved > 0)
        {
            if (c.state == STATE_EXPORT)
            {
                coo.Set(c.path.destX(), c.path.destY());
                int am = f.Reserved(c.res, coo);
                if (am > 0)
                {
                    if (am > c.reserved)
                        am = c.reserved;
                    FACTIONS.player().res().Inc(c.res, c.tType().rtype, -am);
                    f.Finish(c.res, coo, am, c.tType());
                    c.amountCarried += am;
                    c.reserved -= am;
                    c.reservedGlobally -= am;
                }
            }
            else if (c.state == STATE_OTHER)
            {
                int am = SETT.PATH().Finders.resource.Pickup(c.res, c.path.destX(), c.path.destY(), c.reserved);
                FACTIONS.player().res().Inc(c.res, c.tType().rtype, -am);
                c.amountCarried += am;
                c.reserved -= am;
                c.reservedGlobally -= am;
            }
            c.reserved = 0;
        }
        SETT.HALFENTS().caravans.withdrawals[c.res.index()].Inc(c.tType(), -c.reservedGlobally);
        c.reservedGlobally = 0;
    }

    protected override void Load(Caravan c)
    {
        SETT.HALFENTS().caravans.withdrawals[c.res.index()].Inc(c.tType(), c.reservedGlobally);
    }

    public override void HoverInfo(GBox box, Caravan c)
    {
        box.Text(name);
        if (c.reservedGlobally - c.amountCarried > 0)
        {
            box.SetResource(c.res, c.reservedGlobally - c.amountCarried);
            box.Text(c.tType().name);
        }
    }
}