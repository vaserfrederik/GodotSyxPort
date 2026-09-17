using System;
using System.Collections.Generic;

namespace Settlement.Thing.HalfEntity
{
    class _WMap
    {
        private readonly _Quadrant[][] quadrants;
        private readonly int qMaxX;
        private readonly int qMaxY;
        private readonly int gridSize = C.TILE_SIZE * 32;

        public _WMap(int mapSizeX, int mapSizeY)
        {
            qMaxX = mapSizeX / gridSize;
            qMaxY = mapSizeY / gridSize;

            quadrants = new _Quadrant[qMaxX][];
            for (int y = 0; y < qMaxX; y++)
            {
                quadrants[y] = new _Quadrant[qMaxY];
                for (int x = 0; x < qMaxY; x++)
                {
                    quadrants[y][x] = new _Quadrant();
                }
            }
        }

        public void Add(HalfEntity e)
        {
            if (!IsOut(e.gridX, e.gridY))
                throw new RuntimeException();

            e.gridX = (short)(e.body().cX() / gridSize);
            e.gridY = (short)(e.body().cY() / gridSize);

            if (IsOut(e.gridX, e.gridY))
                return;

            quadrants[e.gridY][e.gridX].Add(e);
        }

        public void Remove(HalfEntity e)
        {
            if (IsOut(e.gridX, e.gridY))
                return;

            quadrants[e.gridY][e.gridX].Remove(e);
            e.gridY = -1;
        }

        public void Move(HalfEntity e)
        {
            short gridX = (short)(e.body().cX() / gridSize);
            short gridY = (short)(e.body().cY() / gridSize);

            if (e.gridX != gridX || e.gridY != gridY)
            {
                Remove(e);
                Add(e);
            }
        }

        private bool IsOut(int qx1, int qy1)
        {
            return qx1 >= qMaxX || qy1 >= qMaxY || qx1 < 0 || qy1 < 0;
        }

        void Fill(int x1, int x2, int y1, int y2, Tree<HalfEntity> result)
        {
            int min = 0;

            int qx1 = (x1 - min) / gridSize;
            if (qx1 < 0)
                qx1 = 0;
            int qy1 = (y1 - min) / gridSize;
            if (qy1 < 0)
                qy1 = 0;
            int qx2 = (x2 + min) / gridSize;
            if (qx2 >= qMaxX)
                qx2 = qMaxX - 1;
            int qy2 = (y2 + min) / gridSize;
            if (qy2 >= qMaxY)
                qy2 = qMaxY - 1;

            for (int y = qy1; y <= qy2; y++)
            {
                for (int x = qx1; x <= qx2; x++)
                {
                    foreach (HalfEntity e in quadrants[y][x])
                    {
                        if (e.body().touches(x1, x2, y1, y2))
                        {
                            result.Add(e);
                            if (!result.HasRoom())
                                return;
                        }
                    }
                }
            }
        }

        void Fill(int x1, int x2, int y1, int y2, LISTE<HalfEntity> result)
        {
            int min = 3 * C.TILE_SIZE;

            int qx1 = (x1 - min) / gridSize;
            if (qx1 < 0)
                qx1 = 0;
            int qy1 = (y1 - min) / gridSize;
            if (qy1 < 0)
                qy1 = 0;
            int qx2 = (x2 + min) / gridSize;
            if (qx2 >= qMaxX)
                qx2 = qMaxX - 1;
            int qy2 = (y2 + min) / gridSize;
            if (qy2 >= qMaxY)
                qy2 = qMaxY - 1;

            for (int y = qy1; y <= qy2; y++)
            {
                for (int x = qx1; x <= qx2; x++)
                {
                    foreach (HalfEntity e in quadrants[y][x])
                    {
                        if (e.body().touches(x1, x2, y1, y2))
                            result.Add(e);
                    }
                }
            }
        }

        void Clear()
        {
            for (int y = 0; y < quadrants.Length; y++)
            {
                for (int x = 0; x < quadrants[0].Length; x++)
                {
                    quadrants[y][x].Clear();
                }
            }
        }
    }
}