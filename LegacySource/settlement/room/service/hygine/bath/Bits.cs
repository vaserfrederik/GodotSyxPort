using System;

namespace Settlement.Room.Service.Hygine.Bath
{
    class Bits
    {
        static readonly int CRANK = 0b1000000000000000;
        static readonly int OVEN = 0b0100000000000000;
        static readonly int SERVICE = 0b1100000000000000;
        static readonly int BENCH = 0b0010000000000000;
        static readonly int BENCH_TAIL = 0b0000000001010100;
        static readonly int POOL = 0b1110000000000000;
        static readonly int POOL_FILLED = 0b1110000000000001;
        static readonly int BITS = 0b1110000000000000;
        static readonly int RESERVED = 0b0001000000000000;

        private Bits()
        {
        }
    }
}