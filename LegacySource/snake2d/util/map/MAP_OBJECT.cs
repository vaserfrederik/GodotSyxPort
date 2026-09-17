using System;

namespace Snake2D.Util.Map
{
    public interface MapObject<T> : MapObjectIsSer<T>, MapBoolean
    {
        bool Is(int tile)
        {
            return Get(tile) != null;
        }

        bool Is(int tx, int ty)
        {
            return Get(tx, ty) != null;
        }

        bool Is(int tile, T value)
        {
            return Get(tile) == value;
        }

        bool Is(int tx, int ty, T value)
        {
            return Get(tx, ty) == value;
        }

        T Get(int tile);

        T Get(int tx, int ty);

        T Get(int tx, int ty, Dir d)
        {
            return Get(tx + d.X, ty + d.Y);
        }

        T Get(Coordinate c)
        {
            return Get(c.X, c.Y);
        }

        T Get(Coordinate c, Dir d)
        {
            return Get(c.X + d.X, c.Y + d.Y);
        }
    }
}