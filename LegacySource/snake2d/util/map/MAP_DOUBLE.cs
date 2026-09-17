using System;

namespace snake2d.util.map
{
    public interface MAP_DOUBLE
    {
        /**
         * 
         * @param tile
         * @return
         */
        bool Is(int tile, double value);

        /**
         * 
         * @param tx
         * @param ty
         * @return
         */
        bool Is(int tx, int ty, double value);

        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        bool Is(int tx, int ty, DIR d, double value);

        /**
         * 
         * @param c
         * @return
         */
        bool Is(COORDINATE c, double value);

        /**
         * 
         * @param c
         * @param d
         * @return
         */
        bool Is(COORDINATE c, DIR d, double value);

        /**
         * 
         * @param tile
         * @return
         */
        double Get(int tile);

        /**
         * 
         * @param tx - tileX
         * @param ty - tileY
         * @return
         */
        double Get(int tx, int ty);

        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        double Get(int tx, int ty, DIR d);

        /**
         * 
         * @param c
         * @return
         */
        double Get(COORDINATE c);

        /**
         * 
         * @param c
         * @param d
         * @return
         */
        double Get(COORDINATE c, DIR d);
    }
}