using System;
using System.Collections.Generic;
using snake2d.util.datatypes;

namespace settlement.room.infra.importt
{
    public sealed class ImportThingy
    {
        private readonly ROOM_IMPORT b;
        private readonly ImportTally tally;

        public ImportThingy(ROOM_IMPORT imp, ImportTally tally)
        {
            this.b = imp;
            this.tally = tally;
        }

        public COORDINATE GetReservableSpot(int sx, int sy, RESOURCE res)
        {
            if (tally.capacity[res] - (tally.amount[res]) < 0)
            {
                return null;
            }

            ImportInstance ins = b.Get(sx, sy);
            if (ins == null || ins.Resource != res || Reservable(ins) <= 0)
            {
                ins = null;
                if (b.All().Count == 0)
                    return null;
                int r = RND.rInt(b.All().Count);
                for (int i = 0; i < b.All().Count; i++)
                {
                    ImportInstance ins2 = b.All()[(i + r) % b.All().Count];
                    if (ins2.Resource == res && Reservable(ins2) > 0)
                    {
                        ins = ins2;
                        break;
                    }
                }
            }

            if (ins != null)
            {
                return GetReservableSpot(ins, sx, sy, res);
            }
            return null;
        }

        private COORDINATE GetReservableSpot(ImportInstance i, int sx, int sy, RESOURCE res)
        {
            if (!i.Is(sx, sy))
            {
                sx = i.MX;
                sy = i.MY;
            }
            GUTIL.Filler().Init(this);
            GUTIL.Filler().Filler.Set(sx, sy);
            DIR dir = DIR.ORTHO.rnd();
            int q = 0;
            while (GUTIL.Filler().HasMore())
            {
                COORDINATE c = GUTIL.Filler().Poll();
                if (Reservable(res, c) > 0)
                {
                    GUTIL.Filler().Done();
                    return c;
                }
                q++;
                DIR d = dir;
                for (int k = 0; k < DIR.ORTHO.Size; k++)
                {
                    if (i.Is(c, d))
                        GUTIL.Filler().Fill(c, d);
                    d = d.Next(2);
                }
            }
            GUTIL.Filler().Done();
            GAME.Notify("oh no " + res + " " + i.Resource + " " + q + " " + i.Area + " " + i.MX + " " + i.MY + " " + Reservable(i));
            return null;
        }

        private int Reservable(ImportInstance ins)
        {
            return ins.Capacity - ins.Amount - ins.SpaceReserved;
        }

        private StorageCrate Get(RESOURCE res, COORDINATE c)
        {
            ImportInstance ins = b.Get(c.X, c.Y);
            if (ins == null)
                return null;

            if (ins.Resource != res)
                return null;

            return b.Crate.Get(c.X, c.Y, ins, ins.SData);
        }

        public int Reservable(RESOURCE r, COORDINATE c)
        {
            StorageCrate cr = Get(r, c);
            if (cr == null)
                return 0;
            return cr.StorageReservable;
        }

        public void Reserve(RESOURCE r, COORDINATE c, int amount)
        {
            StorageCrate cr = Get(r, c);
            if (cr == null)
                throw new Exception();
            cr.StorageReserve(amount);
        }

        public int Reserved(RESOURCE r, COORDINATE c)
        {
            StorageCrate cr = Get(r, c);
            if (cr == null)
                return 0;
            return cr.StorageReserved;
        }

        public void Finish(RESOURCE r, COORDINATE c, int amount, TRADE_TYPE type)
        {
            StorageCrate cr = Get(r, c);
            if (cr == null)
                throw new Exception();
            cr.StorageDeposit(amount);
            FACTIONS.Player().Res.Inc(r, type.RType, amount);
        }
    }
}