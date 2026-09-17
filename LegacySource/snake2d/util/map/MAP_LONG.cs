using System;

namespace snake2d.util.map
{
    public interface MAP_LONG
    {
        /**
         * 
         * @param tile
         * @return
         */
        bool is(int tile, long value)
        {
            return get(tile) == value;
        }
        
        /**
         * 
         * @param tx
         * @param ty
         * @return
         */
        bool is(int tx, int ty, long value)
        {
            return get(tx, ty) == value;
        }
        
        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        bool is(int tx, int ty, DIR d, long value)
        {
            return get(tx, ty, d) == value;
        }
        
        /**
         * 
         * @param c
         * @return
         */
        bool is(COORDINATE c, long value)
        {
            return get(c) == value;
        }
        
        /**
         * 
         * @param c
         * @param d
         * @return
         */
        bool is(COORDINATE c, DIR d, long value)
        {
            return get(c, d) == value;
        }
        
        /**
         * 
         * @param tile
         * @return
         */
        long get(int tile);
        
        /**
         * 
         * @param tx - tileX
         * @param ty - tileY
         * @return
         */
        long get(int tx, int ty);
        
        /**
         * 
         * @param tx
         * @param ty
         * @param d
         * @return
         */
        long get(int tx, int ty, DIR d)
        {
            return get(tx + d.x(), ty + d.y());
        }
        
        /**
         * 
         * @param c
         * @return
         */
        long get(COORDINATE c)
        {
            return get(c.x(), c.y());
        }
        
        /**
         * 
         * @param c
         * @param d
         * @return
         */
        long get(COORDINATE c, DIR d)
        {
            return get(c.x() + d.x(), c.y() + d.y());
        }
        
        long max()
        {
            return long.MaxValue;
        }
    }
}