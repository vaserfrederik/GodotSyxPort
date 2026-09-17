using System;
using System.Collections.Generic;

namespace snake2d.util.datatypes
{
    public abstract class RecFacade : RECTANGLEE
    {
        private static readonly long serialVersionUID = 1L;
        private readonly RECIter inter = new RECIter(this);
        public RecFacade()
        {
        }

        public RecFacade Set(double x1, double x2, double y1, double y2)
        {
            MoveX1(x1);
            SetWidth(x2 - x1);
            MoveY1(y1);
            SetHeight(y2 - y1);
            return this;
        }

        public RecFacade MakePositive()
        {
            int x1 = X1();
            int x2 = X2();
            int y1 = Y1();
            int y2 = Y2();
            if (x2 < x1)
            {
                int x = (int)x1;
                x1 = x2;
                x2 = x;
            }

            if (y2 < y1)
            {
                int y = (int)y1;
                y1 = y2;
                y2 = y;
            }
            return Set(x1, x2, y1, y2);
        }

        public RecFacade Set(RECTANGLE other)
        {
            MoveX1(other.X1());
            MoveY1(other.Y1());
            SetWidth(other.Width());
            SetHeight(other.Height());
            return this;
        }

        public RecFacade Set(BODY_HOLDER other)
        {
            Set(other.Body());
            return this;
        }

        public override RecFacade Incr(double x, double y)
        {
            IncrX(x);
            IncrY(y);
            return this;
        }

        public override RecFacade IncrX(double amount)
        {
            MoveX1(X1() + amount);
            return this;
        }

        public override RecFacade IncrY(double amount)
        {
            MoveY1(Y1() + amount);
            return this;
        }

        public override RecFacade Incr(COORDINATE vector, double factor)
        {
            MoveX1(X1() + vector.X() * factor);
            MoveY1(Y1() + vector.Y() * factor);
            return this;
        }

        public override RecFacade Incr(COORDINATE vector)
        {
            IncrX(vector.X());
            IncrY(vector.Y());
            return this;
        }

        public override RecFacade MoveX1Y1(double X, double Y)
        {
            MoveX1(X);
            MoveY1(Y);
            return this;
        }

        public override RecFacade MoveX2(double X2)
        {
            MoveX1(X2 - Width());
            return this;
        }

        public override RecFacade MoveY2(double Y2)
        {
            MoveY1(Y2 - Height());
            return this;
        }

        public override RecFacade MoveX1Y1(COORDINATE vector)
        {
            MoveX1Y1(vector.X(), vector.Y());
            return this;
        }

        public abstract RecFacade SetWidth(double width);
        public abstract RecFacade SetHeight(double height);

        public RecFacade SetDim(double width, double height)
        {
            SetWidth(width);
            SetHeight(height);
            return this;
        }

        public RecFacade SetDim(double dim)
        {
            SetWidth(dim);
            SetHeight(dim);
            return this;
        }

        public RecFacade SetDim(DIMENSION other)
        {
            SetWidth(other.Width());
            SetHeight(other.Height());
            return this;
        }

        public RecFacade Scale(double Xmultiplier, double Ymultiplier)
        {
            SetWidth(Width() * Xmultiplier);
            SetHeight(Height() * Ymultiplier);
            return this;
        }

        public override RecFacade CenterIn(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override RecFacade CenterIn(double x1, double x2, double y1, double y2)
        {
            CenterX(x1, x2);
            CenterY(y1, y2);
            return this;
        }

        public override RecFacade CenterX(double x1, double x2)
        {
            MoveX1(x1 + ((x2 - x1) - Width()) / 2);
            return this;
        }

        public override RecFacade CenterY(double y1, double y2)
        {
            MoveY1(y1 + ((y2 - y1) - Height()) / 2);
            return this;
        }

        public override RecFacade CenterX(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            return this;
        }

        public override RecFacade CenterY(RECTANGLE other)
        {
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override RecFacade MoveC(COORDINATE v)
        {
            MoveC(v.X(), v.Y());
            return this;
        }

        public override RecFacade MoveC(double X, double Y)
        {
            MoveX1(X - Width() / 2);
            MoveY1(Y - Height() / 2);
            return this;
        }

        public override RecFacade MoveCX(double X)
        {
            MoveX1(X - Width() / 2);
            return this;
        }

        public override RecFacade MoveCY(double Y)
        {
            MoveY1(Y - Height() / 2);
            return this;
        }

        public override int X2()
        {
            // TODO Auto-generated method stub
            return X1() + Width();
        }

        public override int Y2()
        {
            // TODO Auto-generated method stub
            return Y1() + Height();
        }

        public override int CX()
        {
            return (int)(X1() + Width() / 2);
        }

        public override int CY()
        {
            return (int)(Y1() + Height() / 2);
        }

        public RecFacade Scale(double scale)
        {
            Scale(scale, scale);
            return this;
        }

        public override string ToString()
        {
            return this.GetType().Name + " x1:" + X1() + " x2:" + X2()
                    + " y1:" + Y1() + " y2:" + Y2();
        }

        public RecFacade IncrW(double dWidth)
        {
            SetWidth(Width() + dWidth);
            return this;
        }

        public void IncrH(double dHeight)
        {
            SetHeight(Height() + dHeight);
        }

        public override IEnumerator<COORDINATE> GetEnumerator()
        {
            return inter.Init();
        }

        // @Override
        // public bool IsSameAs(RECTANGLE other) {
        //     return !(gX1() != other.gX1() || gX2() != other.gX2() ||
        //              gY1() != other.gY1() || gY2() != other.gY2());
        // }

        public abstract class RecFacadePoint : RECTANGLE
        {
            private static readonly long serialVersionUID = 1L;
            private readonly RECIter inter = new RECIter(this);
            public RecFacadePoint()
            {
            }

            public override int X2()
            {
                return X1() + Width();
            }

            public override int Y2()
            {
                return Y1() + Height();
            }

            public override int CX()
            {
                return (int)(X1() + Width() / 2);
            }

            public override int CY()
            {
                return (int)(Y1() + Height() / 2);
            }

            public override string ToString()
            {
                return this.GetType().Name + " x1:" + X1() + " x2:" + X2()
                        + " y1:" + Y1() + " y2:" + Y2();
            }

            public override IEnumerator<COORDINATE> GetEnumerator()
            {
                return inter.Init();
            }

            // @Override
            // public bool IsSameAs(RECTANGLE other) {
            //     return !(gX1() != other.gX1() || gX2() != other.gX2() ||
            //              gY1() != other.gY1() || gY2() != other.gY2());
            // }
        }
    }
}