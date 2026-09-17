using System;

namespace Snake2D.Util.Map
{
    public interface IMapObject<T> : IMapObject<T>
    {
        void Set(int tile, T obj);
        void Set(int tx, int ty, T obj);
        default void Set(int tx, int ty, Dir d, T obj)
        {
            Set(tx + d.X, ty + d.Y, obj);
        }
        default void Set(Coordinate c, T obj)
        {
            Set(c.X, c.Y, obj);
        }

        default void Set(Coordinate c, Dir d, T obj)
        {
            Set(c.X + d.X, c.Y + d.Y, obj);
        }
    }
}