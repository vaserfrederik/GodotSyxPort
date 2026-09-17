using System;
using System.Collections.Generic;

namespace Settlement.Thing.Projectiles
{
    class Map
    {
        private readonly Quad[,] quadrants;
        private readonly int qMaxX;
        private readonly int qMaxY;
        public const int gridSize = C.TILE_SIZE * 16;
        public const int gridScroll = BitOperations.TrailingZeroCount(gridSize);

        public Map(int mapSizeX, int mapSizeY)
        {
            qMaxX = mapSizeX / gridSize;
            qMaxY = mapSizeY / gridSize;

            quadrants = new Quad[qMaxX, qMaxY];

            for (int y = 0; y < quadrants.GetLength(0); y++)
            {
                for (int x = 0; x < quadrants.GetLength(1); x++)
                {
                    quadrants[y, x] = new Quad(x, y);
                }
            }
        }

        public void Add(int e)
        {
            Data d = SETT.PROJS().data.data(e);

            int gridX = d.qx();
            int gridY = d.qy();
            quadrants[gridY, gridX].Add(e);
        }

        public bool Contains(int e)
        {
            Data d = SETT.PROJS().data.data(e);

            int gridX = d.qx();
            int gridY = d.qy();
            return quadrants[gridY, gridX].Contains(e);
        }

        public void Remove(int e)
        {
            Data d = SETT.PROJS().data.data(e);

            int gridX = d.qx();
            int gridY = d.qy();
            quadrants[gridY, gridX].Remove(e);
        }

        void Fill(RECTANGLE bounds, ArrayListInt result)
        {
            int qx1 = bounds.x1() / gridSize;
            if (qx1 < 0)
                qx1 = 0;
            int qy1 = bounds.y1() / gridSize;
            if (qy1 < 0)
                qy1 = 0;
            int qx2 = bounds.x2() / gridSize;
            if (qx2 >= qMaxX)
                qx2 = qMaxX - 1;
            int qy2 = bounds.y2() / gridSize;
            if (qy2 >= qMaxY)
                qy2 = qMaxY - 1;

            for (int y = qy1; y <= qy2; y++)
            {
                for (int x = qx1; x <= qx2; x++)
                {
                    quadrants[y, x].Fill(bounds, result);
                }
            }
        }

        void Clear()
        {
            for (int y = 0; y < quadrants.GetLength(0); y++)
            {
                for (int x = 0; x < quadrants.GetLength(1); x++)
                {
                    quadrants[y, x].Clear();
                }
            }
        }
    }
}