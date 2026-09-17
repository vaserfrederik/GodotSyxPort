using System;
using System.Collections.Generic;
using System.IO;

namespace World.Map.Pathing
{
    public class WDirMap
    {
        private readonly Bitsmap2D m = new Bitsmap2D(0, 8, WORLD.TBOUNDS());

        public readonly SAVABLE Saver = new SAVABLE
        {
            Save = (file) =>
            {
                m.Save(file);
            },
            Load = (file) =>
            {
                m.Load(file);
            },
            Clear = () =>
            {
                m.Clear();
            }
        };

        public readonly MAP_BOOLEAN Is = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                return m.Get(tx, ty) != 0;
            },
            IsTile = (tile) =>
            {
                return m.Get(tile) != 0;
            }
        };

        public bool Can(int fromX, int fromY, DIR d)
        {
            return (m.Get(fromX, fromY) & d.Bit) != 0;
        }

        public bool Can(int tile, DIR d)
        {
            return (m.Get(tile) & d.Bit) != 0;
        }

        public bool Can(COORDINATE from, DIR d)
        {
            return Can(from.X, from.Y, d);
        }

        public bool Can(int fromX, int fromY, int di)
        {
            return Can(fromX, fromY, DIR.ALL[di]);
        }

        public bool Can(COORDINATE c, int di)
        {
            return Can(c.X, c.Y, DIR.ALL[di]);
        }

        public bool IsOnly(COORDINATE c, DIR d)
        {
            int mm = d.Bit | d.Perpendicular().Bit;
            return m.Get(c) == mm;
        }

        public bool IsOnly(int tx, int ty, DIR d)
        {
            int mm = d.Bit | d.Perpendicular().Bit;
            return m.Get(tx, ty) == mm;
        }

        int Get(PathTile t)
        {
            return m.Get(t);
        }

        void Add(int tx, int ty, DIR d)
        {
            int s = m.Get(tx, ty);
            s |= d.Bit;
            m.Set(tx, ty, s);
            if (WORLD.IN_BOUNDS(tx, ty, d))
            {
                s = m.Get(tx, ty, d);
                s |= d.Perpendicular().Bit;
                m.Set(tx, ty, d, s);
            }
        }

        void Remove(int tx, int ty)
        {
            int s = m.Get(tx, ty);
            for (int di = 0; di < DIR.ALL.Length; di++)
            {
                DIR d = DIR.ALL[di];
                if ((s & d.Bit) != 0)
                {
                    s &= ~d.Bit;

                    if (WORLD.IN_BOUNDS(tx, ty, d))
                    {
                        int sd = m.Get(tx, ty, d);
                        sd &= ~d.Perpendicular().Bit;
                        m.Set(tx, ty, d, sd);
                    }
                }
            }
            m.Set(tx, ty, s);
        }

        void Add(COORDINATE c, DIR d)
        {
            Add(c.X, c.Y, d);
        }

        private static readonly List<DIR> dirs = new List<DIR>(DIR.ALL);

        public void Push(PathTile t, double v)
        {
            int md = m.Get(t);
            foreach (DIR d in dirs)
            {
                if ((md & (d.Bit)) != 0)
                    GUTIL.Flooder().PushSmaller(t, d, v + d.TileDistance() * Cost(t.X, t.Y, d), t);
            }
        }

        public void PushSimple(PathTile t)
        {
            int md = m.Get(t);
            foreach (DIR d in dirs)
            {
                if ((md & (d.Bit)) != 0)
                    GUTIL.Flooder().PushSmaller(t, d, t.Value + d.TileDistance(), t);
            }
        }

        public static int Cost(int fromX, int fromY, DIR d)
        {
            if (WORLD.WATER().IsBig.Is(fromX, fromY))
            {
                return 1;
            }
            int toX = fromX + d.X();
            int toY = fromY + d.Y();
            if (WORLD.WATER().IsBig.Is(toX, toY))
                return WTRAV.PORT_PENALTY;

            if (WORLD.MOUNTAIN().CoversTile(fromX, fromY))
                return 6;
            if (WORLD.FOREST().Amount.Get(fromX, fromY) == 1.0)
                return 4;
            return 3;
        }
    }
}