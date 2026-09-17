using System;
using System.Collections.Generic;
using game.battle.div;
using game.battle.thread.general;
using game.battle.thread.general.offence.ContextLines;
using init.constant;
using settlement.main;
using snake2d.PathTile;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    class StepLinesMaker
    {
        private readonly StrategosUtil u;
        private readonly Bitmap2D blob;
        private readonly ContextLines lines;

        public StepLinesMaker(StrategosUtil u, Context c)
        {
            this.u = u;
            this.blob = c.blob;
            this.lines = c.lines;
        }

        public void Make()
        {
            lines.Clear();

            Flooder f = u.Flooder.GetFlooder();
            f.Init(this);

            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    f.SetValue2(tx, ty, 0);
                    if (!blob.Is(tx, ty))
                    {
                        f.PushSloppy(tx, ty, ty * SETT.TWIDTH + tx);
                        f.SetValue2(tx, ty, 0);
                    }
                }
            }

            int ww = SETT.TAREA * 2;

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                if (t.GetValue() >= ww)
                {
                    f.Reopen(t);
                    f.PushSloppy(t.X(), t.Y(), 0);
                    break;
                }

                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR d = DIR.ORTHO.Get(di);
                    if (blob.Is(t, d))
                    {
                        f.PushSmaller(t, d, t.GetValue() + ww);
                        f.SetValue2(t, d, 1);
                    }
                }
            }

            //now, only the edges are pushed... and value2 == 1

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                Make(f, t);
            }
            f.Done();

            //now we'll assign blobID to the lines
            {
                f.Init(this);
                int id = 0;
                for (int ty = 0; ty < SETT.THEIGHT; ty++)
                {
                    for (int tx = 0; tx < SETT.TWIDTH; tx++)
                    {
                        if (!f.HasBeenPushed(tx, ty) && blob.Is(tx, ty))
                        {
                            f.PushSloppy(tx, ty, 0);
                            while (f.HasMore())
                            {
                                PathTile t = f.PollSmallest();
                                t.SetValue2(id);

                                for (int i = 0; i < DIR.ORTHO.Size; i++)
                                {
                                    if (blob.Is(t, DIR.ORTHO.Get(i)))
                                        f.PushSmaller(t, DIR.ORTHO.Get(i), t.GetValue() + 1);
                                }
                            }
                            id++;
                        }
                    }
                }
                f.Done();

                for (int i = 0; i < lines.Lines(); i++)
                {
                    Line n = lines.Get(i);
                    int cx = n.Sx;
                    int cy = n.Sy;
                    cx /= C.TILE_SIZE;
                    cy /= C.TILE_SIZE;

                    n.BlobID = (int)f.GetValue2(cx, cy);
                }
            }

            f.Init(this);
            for (int i = 0; i < lines.Lines(); i++)
            {
                Line n = lines.Get(i);
                int cx = n.Cx();
                int cy = n.Cy();
                cx /= C.TILE_SIZE;
                cy /= C.TILE_SIZE;

                f.SetValue2(cx, cy, -(i + 1));
            }

            //we now have lines, like a convex hull. They are ok, but we'll change the angle so that they face the enemy better.

            for (int di = 0; di < Config.Battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = u.GetArmy().Enemy().Divisions().Get(di);
                for (int i = 0; i < d.Position().Deployed(); i++)
                {
                    int x = d.Position().Tx(i);
                    int y = d.Position().Ty(i);
                    f.PushSloppy(x, y, 0);
                }
            }

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                if (t.GetValue2() < 0)
                {
                    Line n = lines.Get((int)(-t.GetValue2()) - 1);

                    int cx = n.Cx();
                    int cy = n.Cy();

                    PathTile o = t;
                    while (o.Parent != null)
                        o = o.Parent;

                    int ex = o.X() * C.TILE_SIZE + C.TILE_SIZEH;
                    int ey = o.Y() * C.TILE_SIZE + C.TILE_SIZEH;
                    if (vec.Set(cx, cy, ex, ey) <= 0 || (vec.NX() == 0 && vec.NY() == 0))
                    {
                        vec.Set(1, 1);
                    }

                    vec.Rotate90();

                    n.Sx = (int)(cx - vec.NX() * n.Length / 2);
                    n.Sy = (int)(cy - vec.NY() * n.Length / 2);
                    n.Dx = vec.NX();
                    n.Dy = vec.NY();
                }

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    if (SETT.IN_BOUNDS(t, d))
                        f.PushSmaller(t, d, d.TileDistance(), t);
                }
            }
            f.Done();
        }

        private readonly double minDist = Dist(4, 4);
        private readonly double maxDist = Dist(8, 8);
        private readonly VectorImp vec = new VectorImp();

        private void Make(Flooder f, PathTile t)
        {
            if (t.GetValue2() != 1)
                return;

            PathTile res = Trymake(f, t, t, t, maxDist);
            if (res == null)
                res = Trymake(f, t, t, t, minDist);

            if (res != null)
            {
                Line line = lines.MakeNew();

                int sx = t.X() * C.TILE_SIZE + C.TILE_SIZEH;
                int sy = t.Y() * C.TILE_SIZE + C.TILE_SIZEH;
                int ex = res.X() * C.TILE_SIZE + C.TILE_SIZEH;
                int ey = res.Y() * C.TILE_SIZE + C.TILE_SIZEH;
                line.Length = (int)vec.Set(sx, sy, ex, ey);
                line.Sx = sx;
                line.Sy = sy;
                line.Dx = vec.NX();
                line.Dy = vec.NY();
            }
        }

        private PathTile Trymake(Flooder f, PathTile start, PathTile prev, PathTile current, double targetDistPow)
        {
            if (current.GetValue2() != 1)
                return null;

            f.SetValue2(current, 2);

            int dx = start.X() - current.X();
            int dy = start.Y() - current.Y();
            double dist = Dist(dx, dy);
            if (dist >= targetDistPow)
                return current;

            for (int i = 0; i < DIR.ALL.Size; i++)
            {
                DIR d = DIR.ALL.Get(i);
                int x = current.X() + d.X();
                int y = current.Y() + d.Y();
                if (prev.IsSameAs(x, y))
                    continue;
                if (!SETT.IN_BOUNDS(x, y))
                    continue;
                if (f.GetValue2(x, y) != 1)
                    continue;
                PathTile t = Trymake(f, start, current, f.Get(x, y), targetDistPow);
                if (t != null)
                {
                    f.SetValue2(current, 2);
                    return t;
                }
            }

            //f.SetValue2(current, 1);

            return null;
        }

        private double Dist(int dx, int dy)
        {
            return dx * dx + dy * dy;
        }
    }
}