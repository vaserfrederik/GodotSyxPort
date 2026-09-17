using System;
using System.IO;
using System.Linq;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;

namespace settlement.room.main
{
    public interface MapRoomData : MAP_INT
    {
        static class Data : MapRoomData, SAVABLE
        {
            private readonly int[] dataI = new int[SETT.TAREA];

            public void save(FilePutter file)
            {
                file.is(dataI);
            }

            public void load(FileGetter file)
            {
                file.is(dataI);
            }

            public void clear()
            {
                Array.Fill(dataI, 0);
            }

            public int get(int tx, int ty)
            {
                if (SETT.IN_BOUNDS(tx, ty))
                    return get(tx + ty * SETT.TWIDTH);
                return 0;
            }

            public int get(int tile)
            {
                return dataI[tile];
            }

            public void set(ROOMA r, int tile, int value)
            {
                if (!r.is(tile))
                    throw new RuntimeException(r + " " + tile % SETT.TWIDTH + " " + tile / SETT.TWIDTH + " " + SETT.ROOMS().map.get(tile));
                dataI[tile] = value;
            }

            void set(int tile, int value)
            {
                dataI[tile] = value;
            }
        }

        public void inc(ROOMA r, COORDINATE c, int value)
        {
            inc(r, c.x(), c.y(), value);
        }

        public void inc(ROOMA r, COORDINATE c, DIR d, int value)
        {
            inc(r, c.x() + d.x(), c.y() + d.y(), value);
        }

        public void inc(ROOMA r, int tx, int ty, int value)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                inc(r, tx + ty * SETT.TWIDTH, value);
            }
        }

        public void inc(ROOMA r, int tx, int ty, DIR d, int value)
        {
            inc(r, tx + d.x(), ty + d.y(), value);
        }

        public void inc(ROOMA r, int tile, int value)
        {
            set(r, tile, get(tile) + value);
        }

        public void set(ROOMA r, COORDINATE c, int value)
        {
            set(r, c.x(), c.y(), value);
        }

        public void set(ROOMA r, COORDINATE c, DIR d, int value)
        {
            set(r, c.x() + d.x(), c.y() + d.y(), value);
        }

        public void set(ROOMA r, int tx, int ty, int value)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                set(r, tx + ty * SETT.TWIDTH, value);
            }
        }

        public void set(ROOMA r, int tx, int ty, DIR d, int value)
        {
            set(r, tx + d.x(), ty + d.y(), value);
        }

        void set(ROOMA r, int tile, int value);
    }
}