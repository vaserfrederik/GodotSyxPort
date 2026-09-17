using System;
using System.Collections.Generic;

namespace Game.Battle.Thread.Status
{
    public sealed class DivArmyMap
    {
        public static readonly int Radius = 7;
        private readonly Bitmap2D[] maps = new Bitmap2D[] { new Bitmap2D(SETT.TILE_BOUNDS, false), new Bitmap2D(SETT.TILE_BOUNDS, false) };

        public DivArmyMap(DivStatus[] statuses)
        {
        }

        public void Add(Div div, DivPositionImp next)
        {
            for (int i = 0; i < next.Deployed; i++)
            {
                int x = next.Tile(i).X;
                int y = next.Tile(i).Y;
                Bitmap2D m = maps[div.Army.Index];
                Add(x, y, m);
            }
        }

        private void Add(int tx, int ty, Bitmap2D map)
        {
            for (int y = -Radius + 1; y < Radius; y++)
            {
                for (int x = -Radius + 1; x < Radius; x++)
                {
                    int dx = tx + x;
                    int dy = ty + y;
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        map.Set(dx, dy, true);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (Bitmap2D m in maps)
            {
                m.Clear();
            }
        }

        public MAP_BOOLEAN Enemy(Div div)
        {
            return maps[(div.Army.Index + 1) & 1];
        }
    }
}