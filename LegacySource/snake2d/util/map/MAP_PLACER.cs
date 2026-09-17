using System;

namespace snake2d.util.map
{
    public interface MAP_PLACER : MAP_BOOLEAN, MAP_SETTER, MAP_CLEARER
    {
        new MAP_PLACER clear(int tile);

        new MAP_PLACER clear(int tx, int ty);

        default MAP_PLACER clear(int tx, int ty, DIR d)
        {
            return clear(tx + d.x(), ty + d.y());
        }

        default MAP_PLACER clear(COORDINATE c)
        {
            return clear(c.x(), c.y());
        }

        default MAP_PLACER clear(COORDINATE c, DIR d)
        {
            return clear(c.x() + d.x(), c.y() + d.y());
        }

        new MAP_PLACER set(int tile);

        new MAP_PLACER set(int tx, int ty);

        default MAP_PLACER set(int tx, int ty, DIR d)
        {
            return set(tx + d.x(), ty + d.y());
        }

        default MAP_PLACER set(COORDINATE c)
        {
            return set(c.x(), c.y());
        }

        default MAP_PLACER set(COORDINATE c, DIR d)
        {
            return set(c.x() + d.x(), c.y() + d.y());
        }
    }
}