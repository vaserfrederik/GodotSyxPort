using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Infra.Stockpile
{
    public class StockpileTally
    {
        private readonly HistoryResource amountDay = new HistoryResource(STATS.DAYS_SAVED, TIME.Days(), true)
        {
            private readonly INFO info = new INFO(
                MoveDic.¤¤Stored, MoveDic.¤¤Stored);

            public override INFO Info()
            {
                return info;
            }
        };

        private readonly ArrayListGrower<TallyData> datas = new ArrayListGrower<TallyData>();
        public readonly TallyData crates = new TallyData(MoveDic.¤¤crates);
        public readonly TallyData space = new TallyData(MoveDic.¤¤capacity);
        public readonly TallyData spaceReserved = new TallyData(MoveDic.¤¤capacityRes);
        public readonly TallyData amount = new TallyData(MoveDic.¤¤Stored)
        {
            protected override void Set(StockpileInstance ins, int ri, int am)
            {
                base.Set(ins, ri, am);
                amountDay.Set(RESOURCES.ALL().Get(ri), Total(ri));
            }
        };
        public readonly TallyData amountReserved = new TallyData(MoveDic.¤¤StoredRes);

        public readonly DOUBLE_O<RESOURCE> usage = new DOUBLE_O<RESOURCE>()
        {
            public override double GetD(RESOURCE t)
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
            public override int Get(RESOURCE res)
            {
                return amount.Total(res) - amountReserved.Total(res);
            }

            public override int Min(RESOURCE t)
            {
                return 0;
            }

            public override int Max(RESOURCE t)
            {
                return int.MaxValue;
            }
        };

        public void Clear()
        {
            foreach (TallyData d in datas)
                d.Clear();
        }

        public StockpileTally()
        {
            // TODO Auto-generated constructor stub
        }

        public void Init(StockpileInstance ins)
        {
            ins.tdata = Alloc.I2(datas.Size(), RESOURCES.ALL().Size() + 1);
            for (int i = 0; i < ins.crates.Size(); i++)
            {
                ins.crates.Set(i);
                StorageCrate cr = ins.Crate(ins.crates.Get().X, ins.crates.Get().Y);
                Report(cr, ins, 1);
            }
            ins.UpdateMasks();
        }

        public void Report(StorageCrate cr, StockpileInstance ins, int delta)
        {
            if (cr.Resource() != null)
            {
                int ri = cr.Resource().Index();
                crates.Inc(ins, ri, delta);
                space.Inc(ins, ri, delta * ins.CrateSize(cr.Resource()));
                spaceReserved.Inc(ins, ri, delta * cr.ReservedSpace());
                amount.Inc(ins, ri, delta * cr.Amount());
                amountReserved.Inc(ins, ri, delta * cr.Reserved());
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

        private readonly SAVABLE saver = new SAVABLE()
        {
            public void Save(FilePutter file)
            {
                amountDay.Save(file);
            }

            public void Load(FileGetter file) throws IOException
            {
                amountDay.Load(file);
            }

            public void Clear()
            {
                amountDay.Clear();
            }
        };

        public class TallyData
        {
            private readonly int[] ams = Alloc.II(RESOURCES.ALL().Size() + 1);
            private readonly RBITImp bits = new RBITImp();
            public readonly string Name;
            private readonly int index;

            public TallyData(string name)
            {
                this.Name = name;
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

            public int Get(int ri, StockpileInstance ins)
            {
                return ins.tdata[index][ri];
            }

            public int Get(RESOURCE res, StockpileInstance ins)
            {
                if (res == null)
                    return ins.tdata[index][RESOURCES.ALL().Size()];
                return ins.tdata[index][res.Index()];
            }

            public void Inc(StockpileInstance ins, int ri, int am)
            {
                Set(ins, ri, Get(ri, ins) + am);
            }

            public void Set(StockpileInstance ins, int ri, int am)
            {
                int old = ins.tdata[index][ri];
                ins.tdata[index][RESOURCES.ALL().Size()] -= old;
                ins.tdata[index][ri] = am;
                ins.tdata[index][RESOURCES.ALL().Size()] += am;

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