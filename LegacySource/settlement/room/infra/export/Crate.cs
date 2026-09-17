using System;

namespace Settlement.Room.Infra.Export
{
    public class Crate : ITileStorage
    {
        private readonly Bits bAmount = new Bits(0b0000_0000_0000_0000_0000_0011_1111_1111);
        private readonly Bits bReserved = new Bits(0b0000_0000_0000_1111_1111_1100_0000_0000);
        private readonly Bits bReservedSpace = new Bits(0b0011_1111_1111_0000_0000_0000_0000_0000);
        protected readonly RoomExport b;
        public int tx, ty;
        public ExportInstance ins;

        public Crate(RoomExport b)
        {
            this.b = b;
        }

        public Crate Get(int tx, int ty)
        {
            if (b.Is(tx, ty))
            {
                ins = b.Getter.Get(tx, ty);
                if (b.Constructor.IsCrate(tx, ty))
                {
                    this.tx = tx;
                    this.ty = ty;
                    return this;
                }
            }
            return null;
        }

        public RESOURCE Resource()
        {
            return ins.Resource();
        }

        public int Amount(int data)
        {
            return bAmount.Get(data);
        }

        private void Remove()
        {
            RESOURCE r = Resource();
            if (r != null)
            {
                b.Tally.Inc(r, -ins.Amount, -ExportInstance.CrateMax * (ins.Crates));
                ins.Amount -= Amount();
                ins.AmountReserved -= Reserved();
                ins.SpaceReserved -= StorageReserved();
                b.Tally.Inc(r, ins.Amount, ExportInstance.CrateMax * (ins.Crates));
            }
        }

        private void Add()
        {
            RESOURCE r = Resource();
            if (r != null)
            {
                b.Tally.Inc(r, -ins.Amount, -ExportInstance.CrateMax * (ins.Crates));
                ins.Amount += Amount();
                ins.AmountReserved += Reserved();
                ins.SpaceReserved += StorageReserved();
                b.Tally.Inc(r, ins.Amount, ExportInstance.CrateMax * (ins.Crates));
            }
        }

        public int Amount()
        {
            return bAmount.Get(Data());
        }

        public void AmountSet(int am)
        {
            Remove();
            int d = bAmount.Set(Data(), am);
            Save(d);
            Add();
        }

        public int Reserved()
        {
            return bReserved.Get(Data());
        }

        public void ReservedSet(int r)
        {
            Remove();
            int d = bReserved.Set(Data(), r);
            Save(d);
            Add();
        }

        private int Data()
        {
            return SETT.ROOMS().Data.Get(tx, ty);
        }

        private void Save(int d)
        {
            SETT.ROOMS().Data.Set(ins, tx, ty, d);
        }

        public void Clear()
        {
            if (Resource() == null)
                return;
            Remove();
            Save(0);
        }

        public int X()
        {
            return tx;
        }

        public int Y()
        {
            return ty;
        }

        public void StorageDeposit(int amount)
        {
            Remove();
            int d = bReservedSpace.Inc(Data(), -amount);
            d = bAmount.Inc(d, amount);
            Save(d);
            Add();
        }

        public int StorageReservable()
        {
            return ExportInstance.CrateMax - bAmount.Get(Data()) - bReservedSpace.Get(Data());
        }

        public int StorageReserved()
        {
            return bReservedSpace.Get(Data());
        }

        public void StorageReserve(int amount)
        {
            Remove();
            int d = bReservedSpace.Inc(Data(), amount);
            Save(d);
            Add();
        }

        public void StorageUnreserve(int amount)
        {
            Remove();
            int d = bReservedSpace.Inc(Data(), -amount);
            Save(d);
            Add();
        }

        public bool StorageIsFindable()
        {
            return false;
        }
    }
}