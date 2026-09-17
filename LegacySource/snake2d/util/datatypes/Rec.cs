using System;
using System.Collections.Generic;
using System.IO;

namespace snake2d.util.datatypes
{
    public class Rec : RecFacade, SAVABLE
    {
        private static readonly long serialVersionUID = 1L;

        /**
         * For temporary use only. May be used by everyone
         */
        public static readonly Rec TEMP = new Rec();

        protected double x;
        protected double y;
        protected double width;
        protected double height;

        public Rec()
        {
        }

        public Rec(FileGetter f) : this()
        {
            Load(f);
        }

        public Rec(double dim)
        {
            Set(0, dim, 0, dim);
        }

        public Rec(double width, double height)
        {
            Set(0, width, 0, height);
        }

        public Rec(double x1, double x2, double y1, double y2)
        {
            Set(x1, x2, y1, y2);
        }

        /**
         * 
         * @param other
         *            get copy of this
         */
        public Rec(RECTANGLE other)
        {
            MoveX1(other.X1());
            MoveY1(other.Y1());
            SetWidth(other.Width());
            SetHeight(other.Height());
        }

        public override Rec MoveX1(double X1)
        {
            x = X1;
            return this;
        }

        public override Rec MoveY1(double Y1)
        {
            y = Y1;
            return this;
        }

        public override Rec Incr(double x, double y)
        {
            IncrX(x);
            IncrY(y);
            return this;
        }

        public override Rec IncrX(double amount)
        {
            MoveX1(x + amount);
            return this;
        }

        public override Rec IncrY(double amount)
        {
            MoveY1(y + amount);
            return this;
        }

        public override Rec Incr(COORDINATE vector, double factor)
        {
            MoveX1(x + vector.X() * factor);
            MoveY1(y + vector.Y() * factor);
            return this;
        }

        public override Rec Incr(COORDINATE vector)
        {
            IncrX(vector.X());
            IncrY(vector.Y());
            return this;
        }

        public override Rec MoveX1Y1(double X, double Y)
        {
            MoveX1(X);
            MoveY1(Y);
            return this;
        }

        public override Rec MoveX2(double X2)
        {
            MoveX1(X2 - width);
            return this;
        }

        public override Rec MoveY2(double Y2)
        {
            MoveY1(Y2 - height);
            return this;
        }

        public override Rec MoveX1Y1(COORDINATE vector)
        {
            MoveX1Y1(vector.X(), vector.Y());
            return this;
        }

        public override Rec SetWidth(double width)
        {
            this.width = width;
            return this;
        }

        public override Rec SetHeight(double height)
        {
            this.height = height;
            return this;
        }

        public override Rec SetDim(double width, double height)
        {
            SetWidth(width);
            SetHeight(height);
            return this;
        }

        public override Rec SetDim(double dim)
        {
            SetWidth(dim);
            SetHeight(dim);
            return this;
        }

        public override Rec SetDim(DIMENSION other)
        {
            SetWidth(other.Width());
            SetHeight(other.Height());
            return this;
        }

        public override Rec Scale(double Xmultiplier, double Ymultiplier)
        {
            SetWidth(width * Xmultiplier);
            SetHeight(height * Ymultiplier);
            return this;
        }

        public override Rec CenterIn(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override Rec CenterIn(double x1, double x2, double y1, double y2)
        {
            CenterX(x1, x2);
            CenterY(y1, y2);
            return this;
        }

        public override Rec CenterX(double x1, double x2)
        {
            MoveX1(x1 + ((x2 - x1) - width) / 2);
            return this;
        }

        public override Rec CenterY(double y1, double y2)
        {
            MoveY1(y1 + ((y2 - y1) - height) / 2);
            return this;
        }

        public override Rec CenterX(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            return this;
        }

        public override Rec CenterY(RECTANGLE other)
        {
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override Rec MoveC(COORDINATE v)
        {
            MoveC(v.X(), v.Y());
            return this;
        }

        public override Rec MoveC(double X, double Y)
        {
            MoveX1(X - width / 2);
            MoveY1(Y - height / 2);
            return this;
        }

        public override Rec MoveCX(double X)
        {
            MoveX1(X - width / 2);
            return this;
        }

        public override Rec MoveCY(double Y)
        {
            MoveY1(Y - height / 2);
            return this;
        }

        public override int X1()
        {
            return (int)x;
        }

        public override int X2()
        {
            return (int)(x + width);
        }

        public override int Y1()
        {
            return (int)y;
        }

        public override int Y2()
        {
            return (int)(y + height);
        }

        public override int Height()
        {
            return (int)height;
        }

        public override int Width()
        {
            return (int)width;
        }

        public override int CX()
        {
            return (int)(x + width / 2);
        }

        public override int CY()
        {
            return (int)(y + height / 2);
        }

        public override Rec Scale(double scale)
        {
            Scale(scale, scale);
            return this;
        }

        public void Unify(RECTANGLE o)
        {
            if (o.X1() < x)
            {
                width += x - o.X1();
                x = o.X1();
            }
            if (o.X2() > X2())
            {
                width = o.X2() - X1();
            }
            if (o.Y1() < y)
            {
                height += y - o.Y1();
                y = o.Y1();
            }
            if (o.Y2() > Y2())
            {
                height = o.Y2() - Y1();
            }
        }

        public void Unify(int xx, int yy)
        {
            if (width <= 0)
            {
                width = 1;
                x = xx;
            }
            if (height <= 0)
            {
                height = 1;
                y = yy;
            }

            if (xx < x)
            {
                width += x - xx;
                x = xx;
            }
            if (xx >= X2())
            {
                width = xx - X1() + 1;
            }
            if (yy < y)
            {
                height += y - yy;
                y = yy;
            }
            if (yy >= Y2())
            {
                height = yy - Y1() + 1;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " x1:" + X1() + " x2:" + X2() + " y1:" + Y1() + " y2:" + Y2();
        }

        public override Rec IncrW(double dWidth)
        {
            SetWidth(width += dWidth);
            return this;
        }

        public override void IncrH(double dHeight)
        {
            SetHeight(height + dHeight);
        }

        public override void Save(FilePutter file)
        {
            file.D(x);
            file.D(y);
            file.D(width);
            file.D(height);
        }

        public override void Load(FileGetter file)
        {
            x = file.D();
            y = file.D();
            width = file.D();
            height = file.D();
        }

        public override void Clear()
        {
            x = -1;
            y = -1;
            width = 0;
            height = 0;
        }

        public double Dx1()
        {
            return x;
        }

        public double Dy1()
        {
            return x;
        }

        public class RecThreadSafe : Rec
        {
            private static readonly long serialVersionUID = 1L;
            private readonly RECIter iter = new RECIter(this);

            public override IEnumerator<COORDINATE> GetEnumerator()
            {
                return iter.Init();
            }
        }

        public void Pad(int w, int h)
        {
            x -= w;
            y -= h;
            width += w * 2;
            height += h * 2;
        }
    }
}