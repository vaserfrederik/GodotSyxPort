using System;
using System.Collections.Generic;

namespace Settlement.Room.Infra.Janitor
{
    [Serializable]
    public class BITS
    {
        private static readonly long serialVersionUID = 2L;
        private Bitsmap1D resAms = new Bitsmap1D(0, 5, RESOURCES.ALL().Count);
        private readonly RBITImp bitsAvailable = new RBITImp();
        private readonly RBITImp bitsHaveEnough = new RBITImp();
        private readonly RBITImp bitsHaveAny = new RBITImp();
        private readonly RBITImp bitsTimedOut = new RBITImp();
        private readonly RBITImp bitsIsFetching = new RBITImp();
        private readonly RBITImp bitsMissing = new RBITImp();
        Bitsmap1D fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().Count);

        public int ResAm(RESOURCE res)
        {
            return resAms.Get(res.Index());
        }

        public RBITImp ResHave()
        {
            return bitsAvailable;
        }

        public void ResSetMissing(RBIT resourceMask)
        {
            bitsMissing.Or(resourceMask);
            bitsTimedOut.Or(resourceMask);
        }

        public bool ResMissing(RESOURCE res)
        {
            return bitsMissing.Has(res) && !bitsIsFetching.Has(res);
        }

        public void ResInc(JanitorInstance ins, RESOURCE res, int am)
        {
            am = Math.Clamp(resAms.Get(res.Index()) + am, 0, resAms.MaxValue());
            resAms.Set(res.Index(), am);

            if (resAms.Get(res.Index()) > 0)
            {
                bitsAvailable.Or(res);
                bitsMissing.Clear(res);
                bitsTimedOut.Clear(res);
            }
            else
            {
                bitsAvailable.Clear(res);
            }

            double ma = MaxAm(ins, res);

            bitsHaveEnough.Set(res, resAms.Get(res.Index()) > ma);
            bitsHaveAny.Set(res, resAms.Get(res.Index()) > 0 && resAms.Get(res.Index()) + fetchAms.Get(res.Index()) * 4 > ma / 2);
            bitsIsFetching.Set(res, resAms.Get(res.Index()) + fetchAms.Get(res.Index()) * 4 >= ma);
        }

        public int MaxAm(JanitorInstance ins, RESOURCE res)
        {
            int max = ins.Employees().Employed();
            max = (int)(max * SETT.MAINTENANCE().EstimateGlobal(res));
            return Math.Clamp(max, 4, resAms.MaxValue() - 3);
        }

        private static readonly RBITImp tmp = new RBITImp();

        public RBITImp ResMaskFetcher(RoomInstance ins)
        {
            tmp.ClearSet(SETT.PATH().Finders.Maintenance.Mask(ins.mX(), ins.mY()));
            tmp.Xor(bitsIsFetching);
            tmp.Xor(bitsHaveEnough);
            tmp.Xor(bitsTimedOut);
            return tmp;
        }

        public RBITImp ResMaskFetcherMust(RoomInstance ins)
        {
            tmp.ClearSet(SETT.PATH().Finders.Maintenance.Mask(ins.mX(), ins.mY()));
            tmp.Xor(bitsIsFetching);
            tmp.Xor(bitsHaveEnough);
            tmp.Xor(bitsTimedOut);
            tmp.Xor(bitsHaveAny);
            return tmp;
        }

        public RBITImp ResMaskWorker(RoomInstance ins)
        {
            tmp.ClearSet(SETT.PATH().Finders.Maintenance.Mask(ins.mX(), ins.mY()));
            tmp.Xor(bitsIsFetching);
            tmp.Xor(bitsHaveAny);
            tmp.Xor(bitsTimedOut);
            return tmp;
        }

        public void Update()
        {
            bitsTimedOut.Clear();
        }

        public bool ResReserved(RESOURCE res)
        {
            return fetchAms.Get(res.Index()) > 0;
        }

        public void ResReserve(JanitorInstance ins, RESOURCE res, bool yes)
        {
            if (yes)
            {
                fetchAms.Inc(res.Index(), 1);
            }
            else
                fetchAms.Inc(res.Index(), -1);
            bitsIsFetching.Set(res, resAms.Get(res.Index()) + fetchAms.Get(res.Index()) * 4 >= MaxAm(ins, res));
        }

        public void Hover(GBox b, RESOURCE res, RoomInstance ins)
        {
            b.Text("avai");
            b.Add(b.Text().Add(bitsAvailable.Has(res)));
            b.NL();
            b.Text("max");
            b.Add(b.Text().Add(MaxAm((JanitorInstance)ins, res)));
            b.NL();

            b.Text("enough");
            b.Add(b.Text().Add(bitsHaveEnough.Has(res)));
            b.NL();
            b.Text("timed");
            b.Add(b.Text().Add(bitsTimedOut.Has(res)));
            b.NL();
            b.Text("fetchReserved");
            b.Add(b.Text().Add(fetchAms.Get(res.Index())));
            b.NL();
            b.Text("isFetching");
            b.Add(b.Text().Add(bitsIsFetching.Has(res)));
            b.NL();
            b.Text("missing");
            b.Add(b.Text().Add(bitsMissing.Has(res)));
            b.NL();
            b.Text("globalHas");
            b.Add(b.Text().Add(SETT.PATH().Finders.Maintenance.Mask(ins.mX(), ins.mY()).Has(res)));
            b.NL();
            b.Text("fetch");
            b.Add(b.Text().Add(ResMaskFetcher(ins).Has(res)));
            b.NL();
            b.Text("fetch work");
            b.Add(b.Text().Add(ResMaskWorker(ins).Has(res)));
            b.NL();
        }
    }
}