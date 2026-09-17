using System;
using System.Collections.Generic;
using System.Linq;

namespace game.battle.thread.status
{
    public sealed class DivsQuadMap
    {
        public const int Size = 32;
        private readonly QDiv[,] map;
        private readonly Bitsmap1D artillery;
        private readonly QDiv[] free;
        private readonly CircleCooIterator iter;
        private int freeI = 0;
        private readonly int add_scroll;
        private readonly int a_scroll;
        private readonly Tree<QDiv> sort;

        public DivsQuadMap()
        {
            int width = SETT.TWIDTH / Size;
            int height = SETT.THEIGHT / Size;
            map = new QDiv[height, width];
            artillery = new Bitsmap1D(0, 2, width * height);
            free = new QDiv[Config.battle().DIVISIONS_PER_BATTLE];
            for (int i = 0; i < free.Length; i++)
                free[i] = new QDiv();
            iter = new CircleCooIterator((int)Math.Ceiling(Math.Sqrt(width * width + height * height)), GUTIL.flooder());
            add_scroll = BitOperations.TrailingZeros(Size * C.TILE_SIZE);
            a_scroll = BitOperations.TrailingZeros(Size);
            sort = new Tree<QDiv>(free.Length);
            sort.IsGreaterThan = (current, cmp) => current.dist > cmp.dist;
        }

        void Add(Div div, int pcx, int pcy)
        {
            pcx = pcx >> add_scroll;
            if (pcx < 0 || pcx >= map.GetLength(1))
                return;

            pcy = pcy >> add_scroll;
            if (pcy < 0 || pcy >= map.GetLength(0))
                return;

            QDiv old = map[pcy, pcx];
            QDiv n = free[freeI];
            freeI++;
            n.next = old;
            n.divI = (short)div.Index();
            map[pcy, pcx] = n;
        }

        void Clear()
        {
            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                    map[y, x] = null;
            artillery.Clear();
            freeI = 0;
        }

        public void GetNearest(LISTE<Div> res, int px, int py, int pixelDistance, Army target, Div self)
        {
            if (!res.HasRoom())
                return;

            int fx = px;
            int fy = py;

            px = px >> add_scroll;
            py = py >> add_scroll;

            int ra = (int)Math.Ceiling((double)pixelDistance / (C.TILE_SIZE * Size));
            ra = CLAMP.I(ra, 0, iter.Radius(iter.Length() - 1));
            sort.Clear();

            int rac = 0;
            int i = 0;
            while (iter.Radius(i) <= ra)
            {
                if (rac != iter.Radius(i))
                {
                    while (sort.HasMore() && res.HasRoom())
                        res.Add(GAME.ARMIES().Divisions().Get(sort.PollSmallest().divI));
                    if (!res.HasRoom())
                        return;
                    rac = iter.Radius(i);
                }
                int dx = iter.Get(i).X();
                int dy = iter.Get(i).Y();
                i++;
                int pcx = px + dx;
                if (pcx < 0 || pcx >= map.GetLength(1))
                    continue;

                int pcy = py + dy;
                if (pcy < 0 || pcy >= map.GetLength(0))
                    continue;

                QDiv f = map[pcy, pcx];
                while (f != null)
                {
                    Div d = GAME.ARMIES().Divisions().Get(f.divI);
                    if (d != self && d.Army() == target)
                    {
                        int xx = d.Centre().CUnitX() - fx;
                        int yy = d.Centre().CUnitY() - fy;
                        int dist = (int)Math.Sqrt(xx * xx + yy * yy);
                        if (dist < pixelDistance)
                        {
                            f.dist = dist;
                            sort.Add(f);
                        }
                    }
                    f = f.next;
                }
            }

            while (sort.HasMore() && res.HasRoom())
                res.Add(GAME.ARMIES().Divisions().Get(sort.PollSmallest().divI));
        }

        public void GetInQuad(LISTE<Div> res, int tx, int ty, Army target)
        {
            tx = tx >> a_scroll;
            ty = ty >> a_scroll;
            QDiv f = map[ty, tx];
            while (f != null && res.HasRoom())
            {
                Div d = GAME.ARMIES().Divisions().Get(f.divI);
                if (d.Army() == target)
                {
                    res.Add(d);
                }
                f = f.next;
            }
        }

        private sealed class QDiv
        {
            public int dist;
            public QDiv next;
            public short divI;
        }

        public MAP_OBJECT_ISSER<Army> ART => new MAP_OBJECT_ISSER<Army>
        {
            Is = (tile, value) => Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH, value),
            Is = (tx, ty, value) =>
            {
                tx = tx >> a_scroll;
                ty = ty >> a_scroll;
                return (artillery.Get(tx + ty * map.GetLength(1)) & value.bit) != 0;
            }
        };

        void AddArtillery(ArtilleryInstance ins)
        {
            int tx = ins.Body().CX();
            int ty = ins.Body().CY();
            tx = tx >> a_scroll;
            ty = ty >> a_scroll;
            int i = artillery.Get(tx + ty * map.GetLength(1));
            i |= ins.Army().bit;
            artillery.Set(tx + ty * map.GetLength(1), i);
        }
    }
}