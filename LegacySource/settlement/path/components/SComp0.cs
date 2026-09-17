using System;
using System.Collections.Generic;
using static settlement.main.SETT;

namespace settlement.path.components
{
    final class SComp0 : SComponent
    {
        private readonly int index;
        private short cx, cy;
        private byte edgeMask;
        private bool checked;

        SComp0(int index)
        {
            this.index = index;
        }

        public override int Index()
        {
            return index;
        }

        public override int CentreX()
        {
            return cx;
        }

        public override int CentreY()
        {
            return cy;
        }

        public override bool HasEdge()
        {
            return (edgeMask & 1) != 0;
        }

        public override bool HasEntry()
        {
            return (edgeMask & 2) != 0;
        }

        protected override void Retire()
        {
            Retire(true);
            cx = -1;
            cy = -1;
            base.Retire();
        }

        public override bool Retired()
        {
            return (edgeMask & 0b0001_0000) != 0;
        }

        void Retire(bool b)
        {
            if (b)
                edgeMask |= 0b0001_0000;
            else
                edgeMask &= ~0b0001_0000;
        }

        bool Checked()
        {
            return (edgeMask & 0b0010_0000) != 0;
        }

        void Checked(bool b)
        {
            if (b)
                edgeMask |= 0b0010_0000;
            else
                edgeMask &= ~0b0010_0000;
        }

        void Init(RECTANGLE bounds, int size, SComponentChecker neighbours)
        {
            edgeMask = 0;
            if (bounds.X1() == 0)
            {
                edgeMask |= 1;
            }
            else if (bounds.X2() == TWIDTH)
            {
                edgeMask |= 1;
            }
            if (bounds.Y1() == 0)
            {
                edgeMask |= 1;
            }
            else if (bounds.Y2() == THEIGHT)
            {
                edgeMask |= 1;
            }

            foreach (EntryPoint p in SETT.ENTRY().Points.Active())
            {
                if (Is(p.Coo()))
                    edgeMask |= 2;
            }

            int smallest = -1;
            double smallestValue = double.MaxValue;
            foreach (COORDINATE c in bounds)
            {
                int x = c.X();
                int y = c.Y();
                if (Is(x, y))
                {
                    AVAILABILITY a = PATH().Availability.Get(x, y);
                    double rx = (bounds.CX() - x);
                    double ry = (bounds.CY() - y);
                    double r = rx * rx + ry * ry + 1;
                    double v = (a.Player + a.From);
                    v = v * v * r;
                    if (v < smallestValue)
                    {
                        smallest = 1;
                        smallestValue = v;
                        this.cx = (short)x;
                        this.cy = (short)y;
                    }
                }
            }

            if (smallest == -1)
                throw new Exception("shitty component");

            SetEdges(neighbours);
        }

        void SetEdges(SComponentChecker neighbours)
        {
            base.Retire();
            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy(CentreX(), CentreY(), 0);
            GUTIL.Flooder().SetValue2(CentreX(), CentreY(), 0);

            neighbours.Init();

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                int x = t.X();
                int y = t.Y();

                SComp0 n = PATH().Comps.Zero.Get(x, y);
                if (neighbours.Is(n))
                    continue;

                if (n != this)
                {
                    if (SETT.PATH().Comps.Zero.Updating().Is(x, y))
                    {
                        neighbours.IsSetAndSet(n);
                        continue;
                    }

                    if ((n.CentreX() == x && n.CentreY() == y))
                    {
                        PushEdge(n, t.GetValue2(), t.GetValue());
                        n.PushEdge(this, t.GetValue2(), t.GetValue());
                        neighbours.IsSetAndSet(n);
                    }
                }

                for (int i = 0; i < DIR.ALL.Size(); i++)
                {
                    DIR d = DIR.ALL.Get(i);
                    double v2 = PATH().Coster.Player.GetCost(x, y, x + d.X(), y + d.Y()) * d.TileDistance();
                    if (v2 <= 0)
                        continue;
                    SComponent next = PATH().Comps.Zero.Get(x, y, d);
                    if (next == null)
                        continue;
                    if (next != this && n != this && next != n)
                        continue;
                    if (neighbours.Is(next))
                        continue;
                    double v = PATH().Availability.Get(x + d.X(), y + d.Y()).MovementSpeedI;
                    if (GUTIL.Flooder().PushSloppy(x, y, d, t.GetValue() + v * d.TileDistance(), t) != null)
                    {
                        GUTIL.Flooder().SetValue2(x, y, d, v2 + t.GetValue2());
                    }
                }
            }

            GUTIL.Flooder().Done();
            PruneEdges();
        }

        public override SComponentLevel Level()
        {
            return PATH().Comps.Zero;
        }
    }
}