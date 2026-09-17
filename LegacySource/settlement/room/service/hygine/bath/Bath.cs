using settlement.main;
using settlement.room.service.hygine.bath;
using snake2d.util.datatypes;
using settlement.room.main.furnisher;
using settlement.misc.util;

namespace settlement.room.service.hygine.bath
{
    public class Bath : FSERVICE
    {
        static readonly int BIT = Bits.SERVICE;
        static readonly Bath self = new Bath();

        int data;
        readonly Coo coo = new Coo();
        BathInstance ins;

        public static Bath Init(int tx, int ty, ROOM_BATH b)
        {
            if (!b.Is(tx, ty))
                return null;
            int data = ROOMS().data.Get(tx, ty);
            if ((data & BITS) != BIT)
                return null;
            self.data = data;
            self.coo.Set(tx, ty);
            self.ins = b.Get(tx, ty);
            return self;
        }

        private Bath()
        {
        }

        public override int X()
        {
            return coo.X();
        }

        public override int Y()
        {
            return coo.Y();
        }

        public override bool FindableReservedCanBe()
        {
            return Reserved() < Available();
        }

        public override void FindableReserve()
        {
            if (Reserved() >= Available())
                throw new System.Exception();
            ReservedSet(Reserved() + 1);
            Save();
        }

        public override bool FindableReservedIs()
        {
            return Reserved() > 0;
        }

        public override void FindableReserveCancel()
        {
            if (Reserved() > 0)
            {
                ReservedSet(Reserved() - 1);
                Save();
            }
        }

        public void Consume()
        {
            if (!FindableReservedIs())
                throw new System.Exception();
            ReservedSet(Reserved() - 1);
            AvailableSet(Available() - 1);
            Save();
        }

        int Total()
        {
            return (data >> 8) & 0x0F;
        }

        int Available()
        {
            return (data >> 4) & 0x0F;
        }

        private void AvailableSet(int a)
        {
            data &= 0xFF0F;
            data |= a << 4;
        }

        public void AvailabilityInc()
        {
            AvailableSet(Available() + 1);
            Save();
        }

        bool AvailabilityNeeds()
        {
            return Available() < Total();
        }

        private int Reserved()
        {
            return data & 0x0F;
        }

        private void ReservedSet(int r)
        {
            data &= 0xFFF0;
            data |= r;
        }

        private void Save()
        {
            int old = data;
            data = ROOMS().data.Get(coo);
            if (old == data)
                return;

            ins.Service().Report(this, ins.BlueprintI().Data, -(Available() - Reserved()));
            int a = Available();
            data = old;
            ins.Service().Report(this, ins.BlueprintI().Data, (Available() - Reserved()));

            if (Available() == 0 && a > 0)
                Blip(coo.X(), coo.Y(), Bits.POOL);
            else if (Available() > 0 && a == 0)
                Blip(coo.X(), coo.Y(), Bits.POOL_FILLED);

            ROOMS().data.Set(ins, coo, data);
        }

        private void Blip(int tx, int ty, int data)
        {
            FurnisherItem it = ROOMS().fData.Item.Get(tx, ty);
            COORDINATE coo = ROOMS().fData.ItemX1Y1(tx, ty, Coo.TMP);
            int sx = coo.X();
            int sy = coo.Y();

            for (int y = 0; y < it.Height(); y++)
            {
                for (int x = 0; x < it.Width(); x++)
                {
                    int dx = sx + x;
                    int dy = sy + y;
                    if (IsPool(dx, dy, ins))
                        ROOMS().data.Set(ins, dx, dy, data);
                }
            }
        }

        public static int InitService(int tx, int ty, BathInstance ins)
        {
            FurnisherItem it = ROOMS().fData.Item.Get(tx, ty);

            COORDINATE coo = ROOMS().fData.ItemX1Y1(tx, ty, Coo.TMP);
            int sx = coo.X();
            int sy = coo.Y();

            int size = 0;
            for (int y = 0; y < it.Height(); y++)
            {
                for (int x = 0; x < it.Width(); x++)
                {
                    int dx = sx + x;
                    int dy = sy + y;
                    if (IsPool(dx, dy, ins))
                        size++;
                }
            }

            size /= 2;
            if (size <= 0 || size > 0x0F)
                throw new System.Exception(tx + " " + ty + " " + sx + " " + sy + " " + size);

            int data = size << 8;
            data |= SERVICE;
            ROOMS().data.Set(ins, tx, ty, data);
            return size;
        }

        private static bool IsPool(int tx, int ty, BathInstance ins)
        {
            return ins.Is(tx, ty) && (ROOMS().data.Get(tx, ty) & Bits.BITS) == Bits.POOL;
        }

        public void Dispose()
        {
            ReservedSet(0);
            AvailableSet(0);
            Blip(coo.X(), coo.Y(), Bits.POOL);
            Save();
        }
    }
}