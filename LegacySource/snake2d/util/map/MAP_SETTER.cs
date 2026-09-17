using System;

namespace snake2d.util.map
{
    public interface MAP_SETTER
    {
        MAP_SETTER set(int tile);

        MAP_SETTER set(int tx, int ty);

        default MAP_SETTER set(int tx, int ty, DIR d)
        {
            return set(tx + d.x(), ty + d.y());
        }

        default MAP_SETTER set(COORDINATE c)
        {
            return set(c.x(), c.y());
        }

        default MAP_SETTER set(COORDINATE c, DIR d)
        {
            return set(c.x() + d.x(), c.y() + d.y());
        }
    }
}