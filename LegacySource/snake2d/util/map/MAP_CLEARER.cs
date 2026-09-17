using System;

namespace Snake2D.Util.Map
{
    public interface IMapClearer
    {
        IMapClearer Clear(int tile);

        IMapClearer Clear(int tx, int ty);

        IMapClearer Clear(int tx, int ty, DIR d);

        IMapClearer Clear(COORDINATE c);

        IMapClearer Clear(COORDINATE c, DIR d);
    }
}