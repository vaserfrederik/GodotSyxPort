using System;

namespace Settlement.Room.Main.Construction
{
    public class ConstructionData
    {
        public static readonly Map DFloored = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
        public static readonly Map DConstructed = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010));
        public static readonly Map DBroken = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_0100));
        public static readonly Map DExpensive = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0000_1000));
        public static readonly Map DData = new Map(new Bits(0b01111));

        public static readonly Map DBlocked = new Map(new Bits(0b0000_0000_0000_0000_0001_0000_0000_0000));

        public static readonly Map DWall = new Map(new Bits(0b0000_0000_0000_0000_0000_1111_1111_0000));

        private static readonly Map DMarked = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0001_0000));
        private static readonly Map DError = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0010_0000));
        private static readonly Map DTmpInstance = new Map(new Bits(0b0000_0000_0000_0000_0000_0000_0010_0000));
        private static readonly Map[] DResourceNeeded = new Map[] {
            new Map(new Bits(0b0000_0000_0000_0000_0000_0000_1111_0000)),
            new Map(new Bits(0b0000_0000_0000_0000_0000_1111_0000_0000)),
            new Map(new Bits(0b0000_0000_0000_0000_1111_0000_0000_0000)),
            new Map(new Bits(0b0000_0000_0000_1111_0000_0000_0000_0000)),
        };
        private static readonly Map DResourceNeededAll = new Map(new Bits(0b0000_0000_0000_1111_1111_1111_1111_0000));
        private static readonly Map DResAllocated = new Map(new Bits(0b0000_0011_1111_0000_0000_0000_0000_0000));
        private static readonly Map DWorkAmount = new Map(new Bits(0b1111_1100_0000_0000_0000_0000_0000_0000));

        public class Map : MapRoomData
        {
            private readonly Bits bits;
            public readonly int max;

            public Map(Bits bits)
            {
                this.bits = bits;
                this.max = bits.mask;
            }

            public override int Get(int tile)
            {
                return bits.Get(SETT.ROOMS().Data.Get(tile));
            }

            public override int Get(int tx, int ty)
            {
                if (SETT.IN_BOUNDS(tx, ty))
                    return Get(tx + ty * SETT.TWIDTH);
                return 0;
            }

            public override void Set(ROOMA r, int tile, int value)
            {
                if (value < 0 || value > max)
                    throw new RuntimeException("" + value);
                int d = SETT.ROOMS().Data.Get(tile);
                d = bits.Set(d, value);
                SETT.ROOMS().Data.Set(r, tile, d);
            }
        }
    }
}