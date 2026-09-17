using System;
using System.Collections.Generic;

namespace Snake2D.Util.Map
{
    public class AbsGrid
    {
        private readonly GridTile[,] quadrants;
        public readonly List<GridTile> all;
        public readonly Rectangle bounds;
        public readonly int qMaxX;
        public readonly int qMaxY;
        public readonly int gridSize;

        public AbsGrid(int mapSizeX, int mapSizeY, int gridSize)
        {
            this.gridSize = gridSize;
            qMaxX = mapSizeX / gridSize;
            qMaxY = mapSizeY / gridSize;
            bounds = new Rectangle(mapSizeX, mapSizeY);
            quadrants = new GridTile[qMaxX, qMaxY];
            List<GridTile> quadrantsI = new List<GridTile>(qMaxX * qMaxY);
            int inIndex = 0;

            for (int y = 0; y < quadrants.GetLength(0); y++)
            {
                for (int x = 0; x < quadrants.GetLength(1); x++)
                {
                    int x1 = x * gridSize;
                    int x2 = Math.Clamp(x1 + gridSize, 0, mapSizeX);
                    int y1 = y * gridSize;
                    int y2 = Math.Clamp(y1 + gridSize, 0, mapSizeY);
                    GridTile t = new GridTile(inIndex, x1, x2, y1, y2);

                    quadrantsI.Add(t);
                    quadrants[y, x] = t;

                    inIndex++;
                }
            }
            this.all = quadrantsI;
        }

        public class GridTile : Rectangle
        {
            public readonly int index;

            public GridTile(int index, int x1, int x2, int y1, int y2) : base(x1, x2, y1, y2)
            {
                this.index = index;
            }
        }

        public GridTile Get(int index)
        {
            return all[index];
        }

        public readonly MAP_OBJECT<GridTile> map = new MAP_OBJECT<AbsGrid.GridTile>()
        {
            public GridTile Get(int tx, int ty)
            {
                if (!bounds.HoldsPoint(tx, ty))
                    return null;
                return quadrants[ty / gridSize, tx / gridSize];
            },

            public GridTile Get(int tile)
            {
                return Get(tile % bounds.Width, tile / bounds.Width);
            }
        };
    }
}