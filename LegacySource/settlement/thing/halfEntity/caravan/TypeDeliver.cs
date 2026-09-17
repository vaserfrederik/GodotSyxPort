using System;
using settlement.main;
using game.faction;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.gui.misc;
using util.text;

namespace settlement.thing.halfEntity.caravan
{
    class TypeDeliver : Type
    {
        private static short STATE_EXPORT = 0;
        private static short STATE_THRONE = 1;
        private static short STATE_RETURN = 2;

        static readonly CharSequence ¤¤name = "¤delivering";
        static TypeDeliver()
        {
            D.ts(typeof(TypeDeliver));
        }

        public TypeDeliver() : base(¤¤name) { }

        public override bool init(Caravan c, int amount)
        {
            c.amountCarried = (short)amount;
            c.reservedGlobally = (short)amount;
            c.state = STATE_EXPORT;
            c.reserved = 0;
            c.path.Clear();
            SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), amount);

            if (findImport(c))
            {
                c.state = STATE_EXPORT;
                return true;
            }

            if (findDump(c))
            {
                c.state = STATE_THRONE;
                return true;
            }

            SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), -amount);
            return false;
        }

        private bool findImport(Caravan c)
        {
            ImportThingy f = ROOMS().IMPORT.UNLOADER;
            COORDINATE coo = f.getReservableSpot(c.ctx(), c.cty(), c.res);
            if (coo != null)
            {
                int am = f.reservable(c.res, coo);
                if (am > c.amountCarried)
                    am = c.amountCarried;
                c.reserved = (short)am;
                f.reserve(c.res, coo, am);
                c.path.request(c.ctx(), c.cty(), coo.x(), coo.y(), false);
                if (c.path.isSuccessful())
                {
                    c.move();
                    return true;
                }
                else
                {
                    f.reserve(c.res, coo, -am);
                    c.reserved = 0;
                }
            }
            return false;
        }

        private bool findDump(Caravan c)
        {
            COORDINATE coo = SETT.PATH().finders.rndCoo.find(THRONE.coo().x(), THRONE.coo().y(), 8);
            c.path.request(c.ctx(), c.cty(), coo.x(), coo.y(), false);

            if (c.path.isSuccessful())
            {
                c.move();
                return true;
            }
            return false;
        }

        private bool findReturn(Caravan c)
        {
            if (PATH().finders.entryPoints.find(c.ctx(), c.cty(), c.path, int.MaxValue))
            {
                c.move();
                return true;
            }
            return false;
        }

        private bool deliverExport(Caravan c)
        {
            if (c.reserved <= 0)
                return false;

            ImportThingy f = ROOMS().IMPORT.UNLOADER;
            coo.set(c.path.destX(), c.path.destY());
            int am = f.reserved(c.res, coo);
            if (am <= 0)
            {
                c.reserved = 0;
                return false;
            }

            f.finish(c.res, coo, 1, c.tType());
            c.amountCarried -= 1;
            c.reserved -= 1;
            SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), -1);
            FACTIONS.player().res().inc(c.res(), c.tType().rtype, 1);
            return true;
        }

        public override bool update(Caravan c, double ds)
        {
            if (c.state == STATE_RETURN)
                return false;

            if (c.amountCarried <= 0)
            {
                c.state = STATE_RETURN;
                if (findReturn(c))
                    return true;
                return false;
            }

            if (c.state == STATE_EXPORT)
            {
                if (deliverExport(c))
                    return true;

                if (c.amountCarried > 0)
                {
                    if (findImport(c))
                        return true;
                    c.state = STATE_THRONE;
                    if (findDump(c))
                        return true;
                }

                c.state = STATE_RETURN;
                if (findReturn(c))
                    return true;
                return false;
            }
            else if (c.state == STATE_THRONE)
            {
                SETT.THINGS().resources.create(c.ctx(), c.cty(), c.res, 1);
                FACTIONS.player().res().inc(c.res(), c.tType().rtype, 1);
                c.amountCarried -= 1;
                SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), -1);
                return true;
            }
            throw new RuntimeException("state " + c.state);
        }

        public override void cancel(Caravan c, bool dump)
        {
            if (c.state == STATE_EXPORT && c.reserved > 0)
            {
                ImportThingy f = ROOMS().IMPORT.UNLOADER;
                int am = f.reserved(c.res, coo);
                am = CLAMP.i(am, 0, c.reserved);
                if (am > 0)
                {
                    f.finish(c.res, coo, am, c.tType());
                    c.amountCarried -= am;
                }
                c.reserved = 0;
            }

            if (dump && c.amountCarried > 0)
            {
                SETT.THINGS().resources.createPrecise(c.ctx(), c.cty(), c.res, c.amountCarried);
                FACTIONS.player().res().inc(c.res(), c.tType().rtype, c.amountCarried);
                c.amountCarried = 0;
            }
            SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), -c.reservedGlobally);
            c.reservedGlobally = 0;
        }

        public override void hoverInfo(GBox box, Caravan c)
        {
            box.text(name);
            if (c.amountCarried > 0)
            {
                box.setResource(c.res, c.amountCarried);
                box.text(c.tType().name);
            }
        }

        protected override void load(Caravan c)
        {
            SETT.HALFENTS().caravans.deliveries[c.res.index()].inc(c.tType(), c.reservedGlobally);
        }
    }
}