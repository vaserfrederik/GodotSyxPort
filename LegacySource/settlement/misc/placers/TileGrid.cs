using System;

namespace Settlement.Misc.Placers
{
    public class TileGrid : MAP_OBJECT<Tile>, DIMENSION
    {
        private readonly Tile[,] tiles;
        public MAP_BOOLEAN isIn = new MAP_BOOLEAN()
        {
            public bool Is(int tx, int ty)
            {
                if (tx < 0 || tx >= tiles.GetLength(1))
                    return false;
                if (ty < 0 || ty >= tiles.GetLength(0))
                    return false;
                return true;
            }

            public bool Is(int tile)
            {
                throw new RuntimeException();
            }
        };

        public TileGrid(Tile[,] tiles)
        {
            this.tiles = tiles;
        }

        public Tile Get(int tile)
        {
            throw new RuntimeException();
        }

        public Tile Get(int tx, int ty)
        {
            if (tx < 0 || tx >= tiles.GetLength(1))
                return null;
            if (ty < 0 || ty >= tiles.GetLength(0))
                return null;
            return tiles[ty, tx];
        }

        public int Width()
        {
            return tiles.GetLength(1);
        }

        public int Height()
        {
            return tiles.GetLength(0);
        }
    }
}