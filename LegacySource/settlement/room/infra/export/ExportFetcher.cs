using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.resources;
using init.trade;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;

namespace settlement.room.infra.export
{
    public class ExportFetcher
    {
        private readonly ROOM_EXPORT b;

        public ExportFetcher(ROOM_EXPORT b, ExportTally tally)
        {
            this.b = b;
        }

        private COORDINATE getReservableSpot(ExportInstance i, int sx, int sy, RESOURCE res)
        {
            if (!i.is(sx, sy))
            {
                sx = i.mX();
                sy = i.mY();
            }
            GUTIL.filler().init(this);
            GUTIL.filler().filler.set(sx, sy);
            DIR dir = DIR.ORTHO.rnd();
            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                if (reservable(res, c) > 0)
                {
                    GUTIL.filler().done();
                    return c;
                }
                DIR d = dir;
                for (int k = 0; k < DIR.ORTHO.size(); k++)
                {
                    if (i.is(c, d))
                        GUTIL.filler().fill(c, d);
                    d = d.next(2);
                }
            }
            GUTIL.filler().done();
            //GAME.Notify("oh no" + " " + q + " " + i.size());
            return null;
        }

        public COORDINATE getReservableSpot(int sx, int sy, RESOURCE res)
        {
            ExportInstance ins = ROOMS().EXPORT.get(sx, sy);

            if (ins == null || reservable(ins, res) <= 0)
            {
                ins = null;
                ROOM_EXPORT room = ROOMS().EXPORT;
                if (room.all().size() == 0)
                    return null;
                int r = RND.rInt(ROOMS().EXPORT.all().size());
                for (int i = 0; i < room.all().size(); i++)
                {
                    ExportInstance ins2 = room.all().get((i + r) % room.all().size());
                    if (reservable(ins2, res) > 0)
                    {
                        ins = ins2;
                        break;
                    }
                }
            }

            if (ins != null)
            {
                COORDINATE c = getReservableSpot(ins, sx, sy, res);
                if (c == null)
                {
                    GAME.Notify(ins.mX() + " " + ins.mY() + " " + reservable(ins, res));
                }
                return c;
            }

            return null;
        }

        private int reservable(ExportInstance ins, RESOURCE res)
        {
            if (res == ins.resource())
                return ins.amount - ins.amountReserved;
            return 0;
        }

        public int reserved(RESOURCE res, COORDINATE c)
        {
            Crate crate = b.crate(c.x(), c.y());
            if (crate == null || crate.resource() != res)
                return 0;
            return crate.reserved();
        }

        public int reservable(RESOURCE res, COORDINATE c)
        {
            Crate crate = b.crate(c.x(), c.y());
            if (crate == null || crate.resource() != res)
                return 0;
            return crate.amount() - crate.reserved();
        }

        public void reserve(RESOURCE res, COORDINATE c, int amount)
        {
            if (reservable(res, c) < amount)
                throw new RuntimeException();
            Crate crate = b.crate(c.x(), c.y());
            crate.reservedSet(crate.reserved() + amount);
        }

        public void finish(RESOURCE res, COORDINATE c, int amount, TRADE_TYPE type)
        {
            if (amount > reserved(res, c))
                throw new RuntimeException();
            if (amount > 0)
            {
                Crate crate = b.crate(c.x(), c.y());
                crate.reservedSet(crate.reserved() - amount);
                crate.amountSet(crate.amount() - amount);
                FACTIONS.player().res().inc(res, type.rtype, -amount);
            }
        }

        void vacate(int tx, int ty, RESOURCE res, int amount)
        {
            SETT.THINGS().resources.create(tx, ty, res, amount);
        }
    }
}