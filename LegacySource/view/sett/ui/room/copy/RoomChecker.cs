using System;
using System.Collections.Generic;
using settlement.main;
using settlement.room.main;
using settlement.room.main.util;
using snake2d.util.datatypes;
using snake2d.util.iterators;
using snake2d.util.sets;

namespace view.sett.ui.room.copy
{
    internal sealed class RoomChecker
    {
        private readonly Dest dest;
        private readonly Bitmap1D placableBits;
        private Rec box;
        private readonly RECIter iter;
        private static readonly RoomAreaWrapper wrap = new RoomAreaWrapper();

        public RoomChecker(Dest dest)
        {
            this.dest = dest;
            placableBits = new Bitmap1D(SETT.TAREA, false);
            box = new Rec();
            iter = new RECIter(box);
        }

        public void Init()
        {
            placableBits.Clear();
            foreach (COORDINATE c in dest.Body())
            {
                if (!placableBits.Get(c.X + c.Y * SETT.TWIDTH) && IsBlocked(c.X, c.Y))
                {
                    MarkRoom(c);
                }
            }
        }

        private void MarkRoom(COORDINATE coo)
        {
            SetBox(coo.X, coo.Y);
            COORDINATE source = dest.Transform(coo.X, coo.Y);
            Room room = SETT.ROOMS().Map.Get(source);
            ROOMA r = wrap.Init(room, source.X, source.Y);
            foreach (COORDINATE c in iter)
            {
                COORDINATE s = dest.Transform(c.X, c.Y);
                if (r.Is(s))
                {
                    placableBits.Set(c.X + c.Y * SETT.TWIDTH, true);
                }
            }
            wrap.Done();
        }

        private void SetBox(int newX, int newY)
        {
            COORDINATE s = dest.Transform(newX, newY);

            Room room = SETT.ROOMS().Map.Get(s.X, s.Y);
            ROOMA r = wrap.Init(room, s.X, s.Y);

            int dx = s.X - r.Body().CX();
            int dy = s.Y - r.Body().CY();
            int w = r.Body().Width();
            int h = r.Body().Height();
            for (int i = 0; i < 4 - dest.Rot(); i++)
            {
                int k = dx;
                dx = dy;
                dy = -k;
                k = w;
                w = h;
                h = k;
            }

            int x1 = newX - dx - w / 2;
            int y1 = newY - dy - h / 2;
            box.SetDim(w, h);
            box.MoveX1Y1(x1, y1);
            wrap.Done();
        }

        public bool Place(int tx, int ty)
        {
            COORDINATE s = dest.Transform(tx, ty);

            Room room = SETT.ROOMS().Map.Get(s.X, s.Y);
            if (room == null)
                return false;

            if (!IsBlocked(tx, ty) && !IsPartOfBlocked(tx, ty))
            {
                ROOMA r = wrap.Init(room, s.X, s.Y);
                wrap.Done();
                if (r == null)
                {
                    return true;
                }
                if (r.MX() == s.X && r.MY() == s.Y)
                {
                    foreach (COORDINATE c in r.Body())
                    {
                        if (r.Is(c))
                        {
                            int x = c.X - r.MX() + tx;
                            int y = c.Y - r.MY() + ty;

                            if (dest.SourceIs(x, y))
                            {
                                return true;
                            }
                        }
                    }
                    s = dest.Transform(tx, ty);

                    int nx = s.X - r.Body().CX();
                    int ny = s.Y - r.Body().CY();
                    for (int i = 0; i < 4 - dest.Rot(); i++)
                    {
                        int k = nx;
                        nx = ny;
                        ny = -k;
                    }
                    tx -= nx;
                    ty -= ny;
                    SETT.ROOMS().Copy.Copier.Copy(r.MX(), r.MY(), tx, ty, dest.Rot());
                }

                return true;
            }

            return true;
        }

        public bool IsBlocked(int dx, int dy)
        {
            COORDINATE s = dest.Transform(dx, dy);
            return SETT.ROOMS().Copy.Copier.CanCopy(s.X, s.Y) && !SETT.ROOMS().Copy.Copier.IsPlacable(s.X, s.Y, dx, dy);
        }

        public bool IsPartOfBlocked(int dx, int dy)
        {
            return placableBits.Get(dx + dy * SETT.TWIDTH);
        }
    }
}