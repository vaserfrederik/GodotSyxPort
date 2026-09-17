using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main.Job
{
    public abstract class StorageCrate : RESOURCE_TILE, TILE_STORAGE
    {
        private const int noRes = 0x0;
        private int tx, ty;
        private StorageData data;
        private RoomInstance ins;

        protected StorageCrate()
        {
            if (RESOURCES.ALL().Count + 1 > 0x0FF)
            {
                throw new Exception("Too many resources are declared: " + RESOURCES.ALL().Count);
            }
        }

        public StorageData[] Make(ROOMA room)
        {
            int am = 0;
            foreach (COORDINATE c in room.Body())
            {
                if (room.Is(c) && Is(c.x(), c.y()))
                {
                    SETT.ROOMS().Data.Set(room, c, am);
                    am++;
                }
            }
            StorageData[] res = new StorageData[am];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new StorageData();
            }
            return res;
        }

        public StorageCrate Get(int tx, int ty, RoomInstance ins, StorageData[] data)
        {
            if (Is(tx, ty))
            {
                int i = SETT.ROOMS().Data.Get(tx, ty);
                this.tx = tx;
                this.ty = ty;
                this.data = data[i];
                this.ins = ins;
                return this;
            }
            return null;
        }

        protected abstract bool Is(int tx, int ty);

        protected abstract int Max(RoomInstance ins);

        public void StorageDeposit(int amount)
        {
            if (amount == 0)
                return;
            if (Resource() == null || amount() + amount > Max(ins))
                throw new Exception(Resource() + " " + amount() + " " + amount + " " + Max(ins));
            ReservedSpaceSet(reservedSpace() - amount);
            amountSet(amount() + amount);
        }

        public int StorageReserved()
        {
            if (Resource() == null)
                return 0;
            return data.bReservedSpace;
        }

        public int StorageReservable()
        {
            if (Resource() == null)
                return 0;
            return Max(ins) - (amount() + StorageReserved());
        }

        public void StorageReserve(int amount)
        {
            if (StorageReservable() < amount)
                throw new Exception(StorageReservable() + " " + amount);

            ReservedSpaceSet(reservedSpace() + amount);
        }

        public void StorageUnreserve(int amount)
        {
            if (StorageReserved() < amount)
                amount = StorageReserved();
            ReservedSpaceSet(StorageReserved() - amount);
        }

        public RESOURCE Resource()
        {
            int i = data.res;
            if (i == noRes || i > RESOURCES.ALL().Count)
                return null;
            return RESOURCES.ALL()[i - 1];
        }

        public void ResourceSet(RESOURCE res)
        {
            if (Resource() != null)
            {
                throw new Exception();
            }
            data.res = (short)(res.Index() + 1);
            Add();
        }

        public int Amount(int tx, int ty, RoomInstance ins, StorageData[] data)
        {
            if (Is(tx, ty))
            {
                int i = SETT.ROOMS().Data.Get(tx, ty);
                return data[i].bAmount;
            }
            return 0;
        }

        public RESOURCE Res(int tx, int ty, RoomInstance ins, StorageData[] data)
        {
            if (Is(tx, ty))
            {
                int i = SETT.ROOMS().Data.Get(tx, ty);
                i = data[i].res;
                if (i == noRes || i > RESOURCES.ALL().Count)
                    return null;
                return RESOURCES.ALL()[i - 1];
            }
            return null;
        }

        public void Remove()
        {
            RESOURCE r = Resource();
            if (r != null)
            {
                Count(-1);
                if (FindableReservedCanBe())
                    PATH().Finders.Resource.ReportAbsence(this);
                if (StorageReservable() > 0)
                    PATH().Finders.Storage.ReportAbsence(this);
            }
        }

        public void Add()
        {
            RESOURCE r = Resource();
            if (r != null)
            {
                Count(1);
                if (FindableReservedCanBe())
                {
                    PATH().Finders.Resource.ReportPresence(this);
                }
                if (StorageReservable() > 0)
                {
                    PATH().Finders.Storage.ReportPresence(this);
                }
            }
        }

        protected abstract void Count(int delta);

        public int Amount()
        {
            return data.bAmount;
        }

        public void AmountSet(int am)
        {
            Remove();
            data.bAmount = (short)am;
            Add();
        }

        public int Reserved()
        {
            return data.bReserved;
        }

        public void ReservedSet(int r)
        {
            Remove();
            data.bReserved = (short)r;
            Add();
        }

        public int ReservedSpace()
        {
            return data.bReservedSpace;
        }

        private void ReservedSpaceSet(int r)
        {
            Remove();
            data.bReservedSpace = (short)r;
            Add();
        }

        public int Y()
        {
            return ty;
        }

        public int X()
        {
            return tx;
        }

        public bool FindableReservedIs()
        {
            return data.bReserved > 0;
        }

        public bool FindableReservedCanBe()
        {
            return data.bReserved < data.bAmount;
        }

        public void FindableReserveCancel()
        {
            if (Reserved() > 0)
                ReservedSet(Reserved() - 1);
        }

        public void FindableReserve()
        {
            ReservedSet(Reserved() + 1);
        }

        public void ResourcePickup()
        {
            FindableReserveCancel();
            AmountSet(Amount() - 1);
        }

        public int Reservable()
        {
            return Amount() - Reserved();
        }

        public double SpoilRate()
        {
            return SpoilRate(ins);
        }

        protected abstract double SpoilRate(RoomInstance ins);

        public void Clear()
        {
            if (Resource() == null)
                return;
            int am = Amount();
            Remove();
            if (am > 0)
            {
                foreach (DIR dd in DIR.ORTHO)
                {
                    if (!PATH().Solidity.Is(this, dd))
                    {
                        SETT.THINGS().Resources.Create(x() + dd.x(), y() + dd.y(), Resource(), am);
                        break;
                    }
                }
            }
            data.bAmount = 0;
            data.bReserved = 0;
            data.bReservedSpace = 0;
            data.res = noRes;
        }

        public void Dispose()
        {
            RESOURCE res = Resource();
            if (res == null)
                return;
            int am = Amount();
            Remove();

            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.IN_BOUNDS(tx, ty, d) && !SETT.PATH().Solidity.Is(tx, ty, d))
                {
                    SETT.THINGS().Resources.Create(tx + d.x(), ty + d.y(), res, am);
                    am = 0;
                    break;
                }
            }

            if (am > 0)
                SETT.THINGS().Resources.Create(tx, ty, res, am);
            data.bAmount = 0;
            data.bReserved = 0;
            data.bReservedSpace = 0;
            data.res = noRes;
        }

        public void DisposeSilent()
        {
            data.bAmount = 0;
            data.bReserved = 0;
            data.bReservedSpace = 0;
            data.res = noRes;
        }

        public bool IsFindable()
        {
            return true;
        }

        [Serializable]
        public class StorageData
        {
            private static readonly long serialVersionUID = 1L;
            public short res;
            public short bAmount;
            public short bReserved;
            public short bReservedSpace;

            private StorageData()
            {
            }

            public void Clear()
            {
                res = 0;
                bAmount = 0;
                bReserved = 0;
                bReservedSpace = 0;
            }
        }
    }
}