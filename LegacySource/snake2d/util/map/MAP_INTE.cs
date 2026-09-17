using System;

namespace Snake2D.Util.Map
{
    public interface IMapInte : IMapInt
    {
        IMapInte Set(int tile, int value);

        IMapInte Set(int tx, int ty, int value);

        IMapInte Set(int tx, int ty, Dir d, int value);

        IMapInte Set(Coordinate c, int value);

        IMapInte Set(Coordinate c, Dir d, int value);

        IMapInte Increment(int tile, int value);

        IMapInte Increment(int tx, int ty, int value);

        IMapInte Increment(int tx, int ty, Dir d, int value);

        IMapInte Increment(Coordinate c, int value);

        IMapInte Increment(Coordinate c, Dir d, int value);

        abstract class IntMapImp : IMapInte
        {
            private readonly int width;
            private readonly int height;

            public IntMapImp(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            public int Get(int tx, int ty)
            {
                if (tx >= 0 && tx < width && ty >= 0 && ty < height)
                    return Get(tx + ty * width);
                return 0;
            }

            public IMapInte Set(int tx, int ty, int value)
            {
                if (tx >= 0 && tx < width && ty >= 0 && ty < height)
                    Set(tx + ty * width, value);
                return this;
            }
        }
    }
}