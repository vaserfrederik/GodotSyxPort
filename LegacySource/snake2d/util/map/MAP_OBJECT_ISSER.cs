using System;

namespace snake2d.util.map
{
    public interface IMAP_OBJECT_ISSER<T>
    {
        /**
         * 
         * @param tile
         * @return
         */
        bool Is(int tile, T value);

        /**
         * 
         * @param tx
         * @param ty
         * @return
         */
        bool Is(int tx, int ty, T value);

        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        bool Is(int tx, int ty, DIR d, T value)
        {
            return Is(tx + d.X(), ty + d.Y(), value);
        }

        /**
         * 
         * @param c
         * @return
         */
        bool Is(COORDINATE c, T value)
        {
            return Is(c.X(), c.Y(), value);
        }

        /**
         * 
         * @param c
         * @param d
         * @return
         */
        bool Is(COORDINATE c, DIR d, T value)
        {
            return Is(c.X() + d.X(), c.Y() + d.Y(), value);
        }
    }
}