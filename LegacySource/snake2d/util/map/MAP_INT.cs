using System;

namespace Snake2D.Util.Map
{
    public interface MapInt
    {
        /**
         * 
         * @param tile
         * @return
         */
        bool Is(int tile, int value);

        /**
         * 
         * @param tx
         * @param ty
         * @return
         */
        bool Is(int tx, int ty, int value);

        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        bool Is(int tx, int ty, Dir d, int value);

        /**
         * 
         * @param c
         * @return
         */
        bool Is(Coordinate c, int value);

        /**
         * 
         * @param c
         * @param d
         * @return
         */
        bool Is(Coordinate c, Dir d, int value);

        /**
         * 
         * @param tile
         * @return
         */
        int Get(int tile);

        /**
         * 
         * @param tx - tileX
         * @param ty - tileY
         * @return
         */
        int Get(int tx, int ty);

        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        int Get(int tx, int ty, Dir d);

        /**
         * 
         * @param c
         * @return
         */
        int Get(Coordinate c);

        /**
         * 
         * @param c
         * @param d
         * @return
         */
        int Get(Coordinate c, Dir d);

        int Max();
    }

    public class Coordinate
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public enum Dir
    {
        North, South, East, West
    }
}