using System;

namespace Util.Data
{
    public sealed class RanMap : MAP_INT
    {
        private const int SIZE = 128;
        private const int yScroll = BitOperations.TrailingZeroCount(SIZE);
        private const int tMaskX = SIZE - 1;
        private const int tMask = SIZE * SIZE - 1;

        private readonly int[] ran = new int[SIZE * SIZE];

        public RanMap()
        {
            for (int i = 0; i < ran.Length; i++)
                ran[i] = RND.rInt() & 0x7FFFFFFF;
        }

        public override int Get(int tile)
        {
            return ran[tile & tMask];
        }

        public override int Get(int tx, int ty)
        {
            tx &= tMaskX;
            ty &= tMaskX;
            return ran[tx + (ty << yScroll)];
        }
    }
}