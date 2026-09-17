using System;
using settlement.main;
using settlement.room.main;
using snake2d.util.bit;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.data;

namespace settlement.room.main.util
{
    public class RoomBits : INT
    {
        protected readonly Bits bits;
        private readonly COORDINATE coo;

        public RoomBits(COORDINATE coo, int mask)
        {
            bits = new Bits(mask);
            this.coo = coo;
        }

        public RoomBits(COORDINATE coo, Bits bits)
        {
            this.bits = bits;
            this.coo = coo;
        }

        public override int Get()
        {
            return bits.Get(SETT.ROOMS().data.Get(coo));
        }

        public int Get(int rawData)
        {
            return bits.Get(rawData);
        }

        public int Get(int tx, int ty)
        {
            return bits.Get(SETT.ROOMS().data.Get(tx, ty));
        }

        public override int Min()
        {
            return 0;
        }

        public override int Max()
        {
            return bits.Mask;
        }

        protected void Remove()
        {
            
        }

        protected void Add()
        {
            
        }

        public void Set(ROOMA r, int t)
        {
            Remove();
            Set(coo.X, coo.Y, r, t);
            Add();
        }

        public void Set(ROOMA r, DIR d, int t)
        {
            Remove();
            Set(coo.X + d.X, coo.Y + d.Y, r, t);
            Add();
        }

        public void Set(int tx, int ty, ROOMA r, int t)
        {
            int d = SETT.ROOMS().data.Get(tx, ty);
            d = bits.Set(d, t);
            SETT.ROOMS().data.Set(r, tx, ty, d);
        }

        public void Inc(ROOMA r, int i)
        {
            Set(r, CLAMP.i(Get() + i, 0, bits.Mask));
        }
    }
}