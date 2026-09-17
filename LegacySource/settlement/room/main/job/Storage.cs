using System;
using System.Collections.Generic;
using System.Linq;
using settlement.main;
using game.boosting;
using init.resources;
using settlement.misc.util;
using settlement.path.path;
using settlement.room.main;
using snake2d.util.datatypes;

namespace settlement.room.main.job
{
    public abstract class Storage : RESOURCE_TILE, TILE_STORAGE
    {
        private const int noRes = 0x0;
        private int tx, ty;
        private readonly int max;
        private StorageData data;

        protected Storage(int max)
        {
            this.max = max;
            if (RESOURCES.ALL().Count + 1 > 0x0FF)
            {
                throw new Errors.GameError("Too many resources are declared: " + RESOURCES.ALL().Count);
            }
        }

        public StorageData[] Make(ROOMA room)
        {
            int am = 0;
            foreach (COORDINATE c in room.body())
            {
                if (room.Is(c) && Is(c.x(), c.y()))
                {
                    SETT.ROOMS().data.Set(room, c, am);
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

        public Storage Get(int tx, int ty, StorageData[] data)
        {
            if (Is(tx, ty))
            {
                int i = SETT.ROOMS().data.Get(tx, ty);
                this.tx = tx;
                this.ty = ty;
                this.data = data[i];
                return this;
            }
            return null;
        }

        protected abstract bool Is(int tx, int ty);

        protected abstract int Max();

        public void StorageDeposit(int amount)
        {
            if (Resource() == null || Amount() + amount > Max)
                throw new Exception(Resource() + " " + Amount() + " " + amount + " " + Max);
            ReservedSpaceSet(ReservedSpace() - amount);
            AmountSet(Amount() + amount);
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
            return max - (Amount() + StorageReserved());
        }

        public void StorageReserve(int amount)
        {
            if (StorageReservable() < amount)
                throw new Exception(StorageReservable() + " " + amount);

            ReservedSpaceSet(ReservedSpace() + amount);
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
            if (i == noRes)
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

        public void Remove()
        {
            RESOURCE r = Resource();
            if (r != null)
            {
                Count(r.BIndex(), -1, -data.bAmount, -(data.bAmount - data.bReserved), -data.bReservedSpace);
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
                Count(r.BIndex(), 1, data.bAmount, data.bAmount - data.bReserved, data.bReservedSpace);
                if (FindableReservedCanBe())
                    PATH().Finders.Resource.ReportPresence(this);
                if (StorageReservable() > 0)
                    PATH().Finders.Storage.ReportPresence(this);
            }
        }

        protected abstract void Count(int res, int crates, int amountTot, int amountUnres, int spaceRes);

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
                    if (!PATH().solidity.Is(this, dd))
                    {
                        SETT.THINGS().resources.Create(x() + dd.x(), y() + dd.y(), Resource(), am);
                        break;
                    }
                }
            }
            data.res = noRes;
        }

        public void Dispose()
        {
            RESOURCE res = Resource();
            if (res == null)
                return;
            int am = Amount();
            Remove();
            if (am > 0)
                SETT.THINGS().resources.Create(tx, ty, res, am);

            data.res = noRes;
        }

        public bool IsFindable()
        {
            return true;
        }

        public interface STORAGE_CRATE_HASSER
        {
            TILE_STORAGE Job(COORDINATE start, SPath path);
            TILE_STORAGE Job(int tx, int ty);

            bool GetsMaximum(RESOURCE res);

            bool FetchesFromEveryone(RESOURCE res);

            Boostable CarryBonus();
        }

        [Serializable]
        public class StorageData
        {
            public short res;
            public short bAmount;
            public short bReserved;
            public short bReservedSpace;

            private StorageData()
            {
            }
        }
    }
}