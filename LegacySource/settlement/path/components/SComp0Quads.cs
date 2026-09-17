using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    using static Settlement.Main.SETT;
    using static Settlement.Main.SETT.TWIDTH;

    using Settlement.Main;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Map;
    using Snake2D.Util.Sets;

    internal sealed class SComp0Quads
    {
        private readonly int QuadrantSize;
        private readonly int QScroll;
        private readonly int quadsDim;
        private readonly Bitmap1D up;
        private readonly Bitmap1D change;
        private readonly IntegerStack needsUpdate;
        private readonly Rec grid;

        public SComp0Quads(int size)
        {
            QuadrantSize = size;
            QScroll = BitOperations.TrailingZeros(QuadrantSize);
            quadsDim = TWIDTH / QuadrantSize;
            up = new Bitmap1D(quadsDim * quadsDim, false);
            change = new Bitmap1D(quadsDim * quadsDim, false);
            needsUpdate = new IntegerStack(quadsDim * quadsDim);
            grid = new Rec(QuadrantSize, QuadrantSize);
        }

        public void SetChangedAvailability(int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return;

            for (int i = 0; i < DIR.ALLC.Size; i++)
            {
                int x = (tx + DIR.ALLC.Get(i).X) >> QScroll;
                int y = (ty + DIR.ALLC.Get(i).Y) >> QScroll;

                if (x >= 0 && y >= 0 && x < quadsDim && y < quadsDim)
                {
                    int qi = x + y * quadsDim;
                    if (!change.Get(qi))
                    {
                        needsUpdate.Push(qi);
                        change.Set(qi, true);
                    }
                    up.Set(qi, true);
                }
            }
        }

        public void SetChangedServices(int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return;

            for (int i = 0; i < DIR.ALLC.Size; i++)
            {
                int x = (tx + DIR.ALLC.Get(i).X) >> QScroll;
                int y = (ty + DIR.ALLC.Get(i).Y) >> QScroll;
                int qi = x + y * quadsDim;
                if (x >= 0 && y >= 0 && x < quadsDim && y < quadsDim && !up.Get(qi))
                {
                    if (!change.Get(qi))
                    {
                        needsUpdate.Push(qi);
                        change.Set(qi, true);
                    }
                }
            }
        }

        public void ChangeAll()
        {
            change.SetAll(true);
            up.SetAll(true);
            needsUpdate.Clear();
            for (int i = 0; !needsUpdate.IsFull; i++)
                needsUpdate.Push(i);
        }

        public bool Updating()
        {
            return needsUpdate.Size > 0;
        }

        public void Clear()
        {
            needsUpdate.Clear();
            up.Clear();
            change.Clear();
        }

        internal void Update(SComp0Updater updater)
        {
            if (needsUpdate.Size > 0)
                SETT.ENTRY().points.UpdateAvailability();

            for (int ii = 0; ii < needsUpdate.Size; ii++)
            {
                int i = needsUpdate.Get(ii);
                int tx = i % quadsDim;
                int ty = i / quadsDim;
                grid.MoveX1Y1(tx << QScroll, ty << QScroll);
                if (up.Get(i))
                {
                    updater.RemoveSuperComp(grid, this);
                }
            }

            for (int ii = 0; ii < needsUpdate.Size; ii++)
            {
                int i = needsUpdate.Get(ii);
                int tx = i % quadsDim;
                int ty = i / quadsDim;
                grid.MoveX1Y1(tx << QScroll, ty << QScroll);
                if (up.Get(i))
                {
                    updater.Remove(grid, this);
                }
            }

            while (!needsUpdate.IsEmpty)
            {
                int i = needsUpdate.Pop();
                int tx = i % quadsDim;
                int ty = i / quadsDim;
                grid.MoveX1Y1(tx << QScroll, ty << QScroll);
                bool bup = up.Get(i);
                up.Set(i, false);
                change.Set(i, false);

                if (bup)
                {
                    updater.Assign(grid, this);
                }
                else
                {
                    updater.InitData(grid);
                }
            }
        }

        public RECTANGLE PopNext()
        {
            if (needsUpdate.IsEmpty)
                return null;
            int i = needsUpdate.Pop();
            int tx = i % quadsDim;
            int ty = i / quadsDim;
            up.Set(i, false);
            grid.MoveX1Y1(tx << QScroll, ty << QScroll);

            return grid;
        }

        public RECTANGLE PeekpNext(int ii)
        {
            int i = needsUpdate.Get(ii);
            int tx = i % quadsDim;
            int ty = i / quadsDim;
            grid.MoveX1Y1(tx << QScroll, ty << QScroll);

            return grid;
        }

        public int Updatable()
        {
            return needsUpdate.Size;
        }

        public bool Upping()
        {
            return needsUpdate.Size > 0;
        }

        public MAP_BOOLEAN Updating = new MAP_BOOLEAN()
        {
            public bool Is(int tx, int ty)
            {
                int x = (tx) >> QScroll;
                int y = (ty) >> QScroll;
                int qi = x + y * quadsDim;
                if (x > 0 && y > 0 && x < quadsDim && y < quadsDim && up.Get(qi))
                {
                    return true;
                }
                return false;
            }

            public bool Is(int tile)
            {
                return Is(tile % TWIDTH, tile / TWIDTH);
            }
        };
    }
}