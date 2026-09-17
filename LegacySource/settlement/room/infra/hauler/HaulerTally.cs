using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Infra.Hauler
{
    public class HaulerTally
    {
        private readonly HistoryResource amounts = new HistoryResource(64, TIME.Seasons(), true)
        {
            Info = new INFO(MoveDic.¤¤Stored, MoveDic.¤¤StoredD)
        };

        private readonly HistoryResource amountDay = new HistoryResource(STATS.DAYS_SAVED, TIME.Days(), true)
        {
            Info = new INFO(MoveDic.¤¤Stored, MoveDic.¤¤StoredD)
        };

        public readonly ArrayListGrower<TallyData> datas = new ArrayListGrower<>();
        public readonly TallyData crates = new TallyData(MoveDic.¤¤crates);
        public readonly TallyData space = new TallyData(MoveDic.¤¤capacity);
        public readonly TallyData spaceReserved = new TallyData(MoveDic.¤¤capacityRes);
        public readonly TallyData amount = new TallyData(MoveDic.¤¤Stored)
        {
            protected override void Set(HaulerInstance ins, int am)
            {
                base.Set(ins, am);
                if (ins.Resource != null)
                    amountDay.Set(ins.Resource, Total(ins.Resource));
            }
        };

        public readonly TallyData amountReserved = new TallyData(MoveDic.¤¤StoredRes);

        public readonly DOUBLE_O<RESOURCE> usage = new DOUBLE_O<RESOURCE>()
        {
            public double GetD(RESOURCE t)
            {
                int sp = (int)space.Total(t);
                if (sp == 0)
                    return 1.0;
                double used = (int)amount.Total(t);
                return used / sp;
            }
        };

        public readonly INT_O<RESOURCE> amountReservable = new INT_O<RESOURCE>()
        {
            public int Get(RESOURCE res)
            {
                return amount.Total(res) - amountReserved.Total(res);
            }

            public int Min(RESOURCE t)
            {
                return 0;
            }

            public int Max(RESOURCE t)
            {
                return int.MaxValue;
            }
        };

        public void Clear()
        {
            foreach (TallyData d in datas)
                d.Clear();
        }

        public HaulerTally()
        {
        }

        public void Init(HaulerInstance ins)
        {
            ins.tdata = Alloc.II(datas.Size());
            foreach (COORDINATE c in ins.Body())
            {
                StorageCrate cr = ins.Storage(c.x, c.y);
                if (cr != null)
                    Report(cr, ins, 1);
            }
            ins.UpdateMasks();
        }

        public void Report(StorageCrate cr, HaulerInstance ins, int delta)
        {
            if (cr.Resource != null)
            {
                crates.Inc(ins, delta);
                space.Inc(ins, delta * Crate.size);
                spaceReserved.Inc(ins, delta * cr.ReservedSpace());
                amount.Inc(ins, delta * cr.Amount());
                amountReserved.Inc(ins, delta * cr.Reserved());
            }
        }

        public HistoryResource AmountsDay()
        {
            return amountDay;
        }

        public double Load(RESOURCE res)
        {
            if (space.Total(res) == 0)
                return 1;
            return (double)amount.Total(res) / space.Total(res);
        }

        public int AmountTotal(RESOURCE res)
        {
            return amount.Total(res);
        }

        public class TallyData
        {
            private readonly int[] ams = Alloc.II(RESOURCES.ALL().Size() + 1);
            private readonly RBITImp bits = new RBITImp();
            public readonly CharSequence Name;
            private readonly int index;

            public TallyData(CharSequence name)
            {
                Name = name;
                index = datas.Add(this);
            }

            public int Total(int ri)
            {
                return ams[ri];
            }

            public int Total(RESOURCE res)
            {
                if (res == null)
                    return ams[RESOURCES.ALL().Size()];
                return ams[res.Index()];
            }

            public int Get(HaulerInstance ins)
            {
                return ins.tdata[index];
            }

            public void Inc(HaulerInstance ins, int am)
            {
                Set(ins, Get(ins) + am);
            }

            public void Set(HaulerInstance ins, int am)
            {
                int old = ins.tdata[index];
                ins.tdata[index] = am;
                if (ins.Resource == null)
                    return;
                int ri = ins.Resource.Index();
                ams[ri] += am - old;
                ams[RESOURCES.ALL().Size()] += am - old;

                if (ams[ri] < 0)
                    throw new RuntimeException("" + RESOURCES.ALL().Get(ri) + " " + Name);
                if (ams[ri] > 0)
                    bits.Or(RESOURCES.ALL().Get(ri));
                else
                    bits.Clear(RESOURCES.ALL().Get(ri));
            }

            public void Clear()
            {
                Array.Fill(ams, 0);
                bits.Clear();
            }

            public RBIT Bits()
            {
                return bits;
            }
        }
    }
}