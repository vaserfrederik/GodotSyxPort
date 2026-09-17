using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.text;
using world.army;

namespace settlement.room.military.supply
{
    public sealed class SupplyTally
    {
        private readonly ArrayListGrower<TallyData> datas = new ArrayListGrower<TallyData>();
        private int unusedCrate = 0;
        public readonly TallyData crates = new TallyData(MoveDic.¤¤crates);
        public readonly TallyData spaceReserved = new TallyData(Dic.¤¤Inbound);
        public readonly TallyData amount = new TallyData(MoveDic.¤¤Stored);
        private readonly ROOM_SUPPLY b;

        public SupplyTally(ROOM_SUPPLY b)
        {
            this.b = b;
        }

        void Clear()
        {
            foreach (TallyData d in datas)
                d.Clear();
        }

        void Init(SupplyInstance ins)
        {
            ins.tdata = new short[datas.Size * (RESOURCES.ALL().Size + 1) + 1];
            for (int ji = 0; ji < ins.jobs.Count; ji++)
            {
                COORDINATE c = ins.jobs[ji];
                if (ins.Is(c))
                {
                    Crate cr = b.crate.Get(c.x(), c.y());
                    if (cr != null)
                        Report(cr, ins, 1);
                }
            }
            ins.Reset();
        }

        void Report(Crate crate, SupplyInstance ins, int delta)
        {
            if (crate.Storage() != null)
            {
                RESOURCE a = crate.RealResource();
                if (a != null)
                {
                    crates.Inc(ins, a, delta);
                    spaceReserved.Inc(ins, a, delta * crate.Storage().StorageReserved());
                    amount.Inc(ins, a, delta * crate.ResAmount());
                }
                else
                {
                    ins.tdata[datas.Size * (RESOURCES.ALL().Size + 1)] += delta;
                    unusedCrate += delta;
                }
            }
        }

        public int AmountTotal(RESOURCE res)
        {
            return amount.Total(res);
        }

        public int UnusedCrates()
        {
            return unusedCrate;
        }

        public int UnusedCrates(SupplyInstance ins)
        {
            return ins.tdata[datas.Size * (RESOURCES.ALL().Size + 1)];
        }

        public class TallyData
        {
            private readonly int insStride;
            private readonly int[] ams = Alloc.Ii(RESOURCES.ALL().Size + 1);
            public readonly string name;
            private readonly int index;

            public TallyData(string name)
            {
                this.name = name;
                index = datas.Add(this);
                insStride = index * (RESOURCES.ALL().Size + 1);
            }

            public int Total(RESOURCE a)
            {
                if (a == null)
                    return ams[RESOURCES.ALL().Size];
                return ams[a.Index()];
            }

            public int Get(SupplyInstance ins, RESOURCE a)
            {
                if (a == null)
                    return ins.tdata[insStride + RESOURCES.ALL().Size];
                return ins.tdata[insStride + a.Index()];
            }

            private void Inc(SupplyInstance ins, RESOURCE a, int am)
            {
                Set(ins, a, Get(ins, a) + am);
            }

            private void Set(SupplyInstance ins, RESOURCE a, int am)
            {
                int old = ins.tdata[insStride + a.Index()];
                ams[a.Index()] -= old;
                ams[RESOURCES.ALL().Size] -= old;
                ins.tdata[insStride + a.Index()] = (short)am;
                ins.tdata[insStride + RESOURCES.ALL().Size] = (short)am;
                ams[a.Index()] += am;
                ams[RESOURCES.ALL().Size] -= am;

                if (ams[a.Index()] < 0)
                    throw new RuntimeException($"{a} {name}");
            }

            private void Clear()
            {
                Array.Fill(ams, 0);
            }
        }

        RESOURCE GetNewCrate(int ai, RBIT allowed)
        {
            for (int i = 0; i < AD.Supplies().Reses().Count; i++)
            {
                ai %= AD.Supplies().Reses().Count;
                RESOURCE res = AD.Supplies().Reses()[ai];
                if (allowed.Has(res))
                {
                    int am = b.cache.Needed(res);
                    if (crates.Total(res) * ROOM_SUPPLY.STORAGE < am)
                        return res;
                }
                ai++;
            }

            return null;
        }

        int FetchAmount(RESOURCE a)
        {
            if (a == null)
                return 0;

            int am = b.cache.Needed(a);
            am -= spaceReserved.Total(a);
            am -= amount.Total(a);
            return am;
        }

        private readonly RBITImp tmp = new RBITImp();

        RBIT FetchBit(SupplyInstance ins, RBIT allowed)
        {
            tmp.Clear();

            foreach (RESOURCE res in AD.Supplies().Reses())
            {
                if (!allowed.Has(res))
                    continue;

                if (Capacity(ins, res) > 0)
                {
                    int am = b.cache.Needed(res);
                    am -= spaceReserved.Total(res);
                    am -= amount.Total(res);
                    if (am > 0)
                    {
                        tmp.Or(res);
                    }
                }
                else if (OtherCapacity(ins, res) > 0)
                {
                    int am = b.cache.Needed(res);
                    am -= crates.Total(res) * ROOM_SUPPLY.STORAGE;
                    if (am > 0)
                    {
                        tmp.Or(res);
                    }
                }
            }

            return tmp;
        }

        private int Capacity(SupplyInstance ins, RESOURCE res)
        {
            return crates.Get(ins, res) * ROOM_SUPPLY.STORAGE - spaceReserved.Get(ins, res) - amount.Get(ins, res);
        }

        int OtherCapacity(SupplyInstance ins, RESOURCE res)
        {
            return UnusedCrates(ins) * ROOM_SUPPLY.STORAGE;
        }

        int Capacity(SupplyInstance ins, RESOURCE res, RBIT allowed)
        {
            if (!allowed.Has(res))
                return 0;
            return Capacity(ins, res) + OtherCapacity(ins, res);
        }
    }
}