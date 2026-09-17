using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Water
{
    class Updater
    {
        private readonly ROOM_WATER w;
        private Grid grid1 = new Grid();
        private readonly List<PumpInstance> pumps = new List<PumpInstance>(128);
        private readonly List<DIR> dirs = new List<DIR>(DIR.ORTHO) { DIR.C };
        private readonly List<DIR> ortho = new List<DIR>(DIR.ORTHO);
        private readonly double tilesPerSecond = 1;
        private double timer;

        public int ops = 0;

        public readonly SAVABLE saver = new SAVABLE()
        {
            Save = (FilePutter file) =>
            {
                file.d(timer);
                grid1.Save(file);
            },
            Load = (FileGetter file) =>
            {
                timer = file.d();
                grid1.Load(file);
            },
            Clear = () =>
            {
                timer = 0;
                grid1.Clear();
            }
        };

        public Updater(ROOM_WATER water)
        {
            this.w = water;
        }

        public void ReportChange(int tx, int ty, int radius)
        {
            int i = 0;
            while (GUTIL.Circle().Radius(i) < radius)
            {
                ops++;
                int dx = GUTIL.Circle().Get(i).x() + tx;
                int dy = GUTIL.Circle().Get(i).y() + ty;
                if (w.Pumpable.Get(dx, dy) != null)
                {
                    grid1.Mark(dx, dy);
                }

                i++;
            }

            foreach (DIR d in dirs)
            {
                if (SETT.InBounds(tx, ty, d))
                {
                    grid1.Mark(tx + d.x(), ty + d.y());
                }
            }
        }

        void Update(double ds)
        {
            timer += ds * tilesPerSecond;
            if (timer < 1)
                return;

            timer -= (int)timer;

            ops = 0;

            RECTANGLE bb = grid1.PollNext();
            while (bb != null)
            {
                foreach (COORDINATE c in bb)
                {
                    if (grid1.Mark.Is(c))
                    {
                        grid1.Mark.Set(c, false);
                        if (w.Pumpable.Get(c.x(), c.y()) != null)
                        {
                            pumps.Clear();
                            ops++;
                            Drain(c.x(), c.y(), pumps);
                            if (pumps.Count > 0)
                                Fill(pumps);
                            else
                                Fail(c.x(), c.y());
                        }
                    }
                }

                bb = grid1.PollNext();
            }
        }

        private void Drain(int tx, int ty, List<PumpInstance> pumps)
        {
            Flooder f = GUTIL.Flooder();
            f.Init(this);
            f.PushSloppy(tx, ty, 0);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                RoomPumpable p = w.Pumpable.Get(t.x(), t.y());

                t.SetValue2(p.Dirmask(t.x(), t.y()));

                p.Drain(t.x(), t.y());
                grid1.Mark.Set(t, false);
                ops++;

                if (p.Radius() > 0)
                {
                    int rr = p.Radius();
                    int i = 0;
                    while (GUTIL.Circle().Radius(i) < rr)
                    {
                        ops++;
                        int dx = GUTIL.Circle().Get(i).x() + t.x();
                        int dy = GUTIL.Circle().Get(i).y() + t.y();
                        if (w.Pumpable.Get(dx, dy) == p)
                        {
                            f.PushSmaller(dx, dy, t.GetValue() + GUTIL.Circle().Radius(i), t);
                        }

                        i++;
                    }
                }

                foreach (DIR d in ortho)
                {
                    if (w.Pumpable.Get(t, d) != null)
                        f.PushSmaller(t, d, t.GetValue() + 1, t);
                    else
                    {
                        PumpInstance ins = w.Pump.Get(t.x() + d.x(), t.y() + d.y());
                        if (ins != null && ins.Ox() == t.x() + d.x() && ins.Oy() == t.y() + d.y())
                        {
                            pumps.Add(ins);
                        }
                    }
                }
            }
            f.Done();
        }

        private void Fail(int tx, int ty)
        {
            Flooder f = GUTIL.Flooder();
            f.Init(this);
            f.PushSloppy(tx, ty, 0);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                RoomPumpable p = w.Pumpable.Get(t.x(), t.y());
                p.PumpFail(t.x(), t.y(), (int)t.GetValue2());

                if (p.Radius() > 0)
                {
                    int rr = p.Radius();
                    int i = 0;
                    while (GUTIL.Circle().Radius(i) < rr)
                    {
                        ops++;
                        int dx = GUTIL.Circle().Get(i).x() + t.x();
                        int dy = GUTIL.Circle().Get(i).y() + t.y();
                        if (w.Pumpable.Get(dx, dy) == p)
                        {
                            f.PushSmaller(dx, dy, t.GetValue() + GUTIL.Circle().Radius(i), t);
                        }

                        i++;
                    }
                }

                foreach (DIR d in ortho)
                {
                    if (w.Pumpable.Get(t, d) != null)
                        f.PushSmaller(t, d, t.GetValue() + 1, t);
                }
            }
            f.Done();
        }

        private void Fill(List<PumpInstance> pumps)
        {
            if (pumps.Count == 0)
                return;

            Flooder f = GUTIL.Flooder();
            f.Init(this);
            double am = 0;
            foreach (PumpInstance ins in pumps)
            {
                int a = ins.Output();
                am += a;
                if (a > 0)
                    f.PushSloppy(ins.Ox(), ins.Oy(), 0);
            }

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                ops++;
                RoomPumpable p = w.Pumpable.Get(t);
                if (p != null)
                {
                    if (am <= 0)
                    {
                        p.PumpFail(t.x(), t.y(), (int)t.GetValue2());
                    }
                    else
                    {
                        if (t.Parent != null)
                            p.Pump(t.x(), t.y(), DIR.Get(t.Parent, t), (int)t.GetValue2());
                        am -= p.SuckAmount(t.x(), t.y());

                        if (p.Radius() > 0)
                        {
                            int rr = p.Radius();
                            int i = 0;
                            while (GUTIL.Circle().Radius(i) < rr - 2)
                            {
                                ops++;
                                int dx = GUTIL.Circle().Get(i).x() + t.x();
                                int dy = GUTIL.Circle().Get(i).y() + t.y();

                                if (w.Pumpable.Get(dx, dy) == p)
                                {
                                    f.PushSmaller(dx, dy, t.GetValue() + GUTIL.Circle().Radius(i), t);
                                }

                                i++;
                            }
                        }
                    }
                }

                foreach (DIR d in ortho)
                {
                    if (w.Pumpable.Get(t, d) != null)
                        f.PushSmaller(t, d, t.GetValue() + 1, t);
                }
            }
            f.Done();
        }

        private class Grid
        {
            public readonly List<List<GridTile>> grid = new List<List<GridTile>>();
            private readonly Bitmap mark;

            public Grid()
            {
                for (int y = 0; y < Height; y++)
                {
                    grid.Add(new List<GridTile>());
                    for (int x = 0; x < Width; x++)
                    {
                        grid[y].Add(new GridTile(x, y));
                    }
                }
                mark = new Bitmap(SETT.TILE_BOUNDS.Width, SETT.TILE_BOUNDS.Height);
            }

            public void Save(FilePutter file)
            {
                foreach (var tt in grid)
                {
                    foreach (var t in tt)
                    {
                        file.Bool(t.marked);
                    }
                }
                mark.Save(file);
            }

            public void Load(FileGetter file) throws IOException
            {
                active.Clear();
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var t = grid[y][x];
                        t.marked = file.Bool();
                        if (t.marked)
                        {
                            t.marked = false;
                            Mark(t.body.cX(), t.body.cY());
                        }
                    }
                }
                mark.Load(file);
            }

            public void Clear()
            {
                mark.Clear();
                active.Clear();
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        grid[y][x].marked = false;
                    }
                }
            }

            public RECTANGLE PollNext()
            {
                if (active.Count == 0)
                    return null;
                GridTile t = active.Dequeue();
                t.marked = false;
                return t.body;
            }

            public void Mark(int tx, int ty)
            {
                mark.Set(tx, ty, true);
                tx /= GridTile.size;
                ty /= GridTile.size;
                GridTile t = grid[ty][tx];
                if (!t.marked)
                {
                    t.marked = true;
                    active.Enqueue(t);
                }
            }

            private class GridTile
            {
                public static readonly int size = 16;
                private readonly RECTANGLE body;
                public bool marked = false;

                public GridTile(int gx, int gy)
                {
                    int x2 = Math.Min(gx * size + size, SETT.TWIDTH);
                    int y2 = Math.Min(gy * size + size, SETT.THEIGHT);

                    body = new Rec(gx * size, x2, gy * size, y2);
                }
            }
        }
    }
}