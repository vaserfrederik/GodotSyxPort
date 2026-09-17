using System;

namespace snake2d.util.map
{
    public interface MAP_DOUBLEE : MAP_DOUBLE
    {
        MAP_DOUBLEE Set(int tile, double value);

        MAP_DOUBLEE Set(int tx, int ty, double value);

        default MAP_DOUBLEE Set(int tx, int ty, DIR d, double value)
        {
            return Set(tx + d.x(), ty + d.y(), value);
        }

        default MAP_DOUBLEE Set(COORDINATE c, double value)
        {
            return Set(c.x(), c.y(), value);
        }

        default MAP_DOUBLEE Set(COORDINATE c, DIR d, double value)
        {
            return Set(c.x() + d.x(), c.y() + d.y(), value);
        }

        default MAP_DOUBLEE Increment(int tile, double value)
        {
            return Set(tile, Get(tile) + value);
        }

        default MAP_DOUBLEE Increment(int tx, int ty, double value)
        {
            return Set(tx, ty, Get(tx, ty) + value);
        }

        default MAP_DOUBLEE Increment(int tx, int ty, DIR d, double value)
        {
            return Set(tx + d.x(), ty + d.y(), Get(tx, ty, d) + value);
        }

        default MAP_DOUBLEE Increment(COORDINATE c, double value)
        {
            return Set(c.x(), c.y(), Get(c) + value);
        }

        default MAP_DOUBLEE Increment(COORDINATE c, DIR d, double value)
        {
            return Set(c.x() + d.x(), c.y() + d.y(), Get(c, d) + value);
        }

        public abstract class DoubleMapImp : MAP_DOUBLEE
        {
            private readonly int width;
            private readonly int height;

            public DoubleMapImp(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            public override double Get(int tx, int ty)
            {
                if (tx < 0 || tx >= width || ty < 0 || ty >= height)
                    return 0;
                return Get(tx + ty * width);
            }

            public override MAP_DOUBLEE Set(int tx, int ty, double value)
            {
                if (tx < 0 || tx >= width || ty < 0 || ty >= height)
                    return this;
                Set(tx + ty * width, value);
                return this;
            }
        }
    }
}