using System;
using game.battle.div;
using game.battle.formation;
using settlement.main;
using snake2d.util.map;
using snake2d.util.sets;

namespace game.battle.thread.status
{
    public sealed class DivsSpaceMap
    {
        public const int radius = 4;
        private const double radiusI = 1.0 / radius;
        private Bitsmap1D map = new Bitsmap1D(0, 3, SETT.TAREA);

        public DivsSpaceMap(DivStatus[] statuses)
        {
        }

        public void Add(Div div, DivPositionImp next)
        {
            for (int i = 0; i < next.Deployed(); i++)
            {
                int x = next.Tile(i).x();
                int y = next.Tile(i).y();
                Add(x, y, i);
            }
        }

        private void Add(int tx, int ty, int currentI)
        {
            for (int y = -radius + 1; y < radius; y++)
            {
                for (int x = -radius + 1; x < radius; x++)
                {
                    int dx = tx + x;
                    int dy = ty + y;
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        int dist = radius - Math.Abs(x) + Math.Abs(y);
                        if (dist > 0)
                        {
                            int t = dx + dy * SETT.TWIDTH;
                            if (map.Get(t) < dist)
                                map.Set(t, dist);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            map.Clear();
        }

        public MAP_DOUBLE Cost = new MAP_DOUBLE
        {
            public double Get(int tx, int ty)
            {
                return Get(tx + ty * SETT.TWIDTH);
            }

            public double Get(int tile)
            {
                return 32 * map.Get(tile) * radiusI;
            }
        };
    }
}