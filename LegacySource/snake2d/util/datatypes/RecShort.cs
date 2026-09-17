using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.iterators;

namespace snake2d.util.datatypes
{
    public class RecShort : RecFacade, SAVABLE
    {
        private static readonly long serialVersionUID = 1L;

        /**
         * For temporary use only. May be used by everyone
         */
        public static readonly RecShort TEMP = new RecShort();

        protected short x;
        protected short y;
        protected short width;
        protected short height;

        public RecShort()
        {
        }

        public RecShort(FileGetter f) : this()
        {
            Load(f);
        }

        public RecShort(double dim) : this()
        {
            Set(0, dim, 0, dim);
        }

        public RecShort(double width, double height) : this()
        {
            Set(0, width, 0, height);
        }

        public RecShort(double x1, double x2, double y1, double y2) : this()
        {
            Set(x1, x2, y1, y2);
        }

        /**
         * 
         * @param other
         *            get copy of this
         */
        public RecShort(RECTANGLE other) : this()
        {
            MoveX1(other.X1());
            MoveY1(other.Y1());
            SetWidth(other.Width());
            SetHeight(other.Height());
        }

        public override RecShort MoveX1(double X1)
        {
            x = (short)X1;
            return this;
        }

        public override RecShort MoveY1(double Y1)
        {
            y = (short)Y1;
            return this;
        }

        public override RecShort Incr(double x, double y)
        {
            IncrX(x);
            IncrY(y);
            return this;
        }

        public override RecShort IncrX(double amount)
        {
            MoveX1(this.x + amount);
            return this;
        }

        public override RecShort IncrY(double amount)
        {
            MoveY1(this.y + amount);
            return this;
        }

        public override RecShort Incr(COORDINATE vector, double factor)
        {
            MoveX1(this.x + vector.X() * factor);
            MoveY1(this.y + vector.Y() * factor);
            return this;
        }

        public override RecShort Incr(COORDINATE vector)
        {
            IncrX(vector.X());
            IncrY(vector.Y());
            return this;
        }

        public override RecShort MoveX1Y1(double X, double Y)
        {
            MoveX1(X);
            MoveY1(Y);
            return this;
        }

        public override RecShort MoveX2(double X2)
        {
            MoveX1(X2 - width);
            return this;
        }

        public override RecShort MoveY2(double Y2)
        {
            MoveY1(Y2 - height);
            return this;
        }

        public override RecShort MoveX1Y1(COORDINATE vector)
        {
            MoveX1Y1(vector.X(), vector.Y());
            return this;
        }

        public override RecShort SetWidth(double width)
        {
            this.width = (short)width;
            return this;
        }

        public override RecShort SetHeight(double height)
        {
            this.height = (short)height;
            return this;
        }

        public override RecShort SetDim(double width, double height)
        {
            SetWidth(width);
            SetHeight(height);
            return this;
        }

        public override RecShort SetDim(double dim)
        {
            SetWidth(dim);
            SetHeight(dim);
            return this;
        }

        public override RecShort SetDim(DIMENSION other)
        {
            SetWidth(other.Width());
            SetHeight(other.Height());
            return this;
        }

        public override RecShort Scale(double Xmultiplier, double Ymultiplier)
        {
            SetWidth(this.width * Xmultiplier);
            SetHeight(this.height * Ymultiplier);
            return this;
        }

        public override RecShort CenterIn(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override RecShort CenterIn(double x1, double x2, double y1, double y2)
        {
            CenterX(x1, x2);
            CenterY(y1, y2);
            return this;
        }

        public override RecShort CenterX(double x1, double x2)
        {
            MoveX1(x1 + ((x2 - x1) - width) / 2);
            return this;
        }

        public override RecShort CenterY(double y1, double y2)
        {
            MoveY1(y1 + ((y2 - y1) - height) / 2);
            return this;
        }

        public override RecShort CenterX(RECTANGLE other)
        {
            CenterX(other.X1(), other.X2());
            return this;
        }

        public override RecShort CenterY(RECTANGLE other)
        {
            CenterY(other.Y1(), other.Y2());
            return this;
        }

        public override RecShort MoveC(COORDINATE v)
        {
            MoveC(v.X(), v.Y());
            return this;
        }

        public override RecShort MoveC(double X, double Y)
        {
            MoveX1(X - width / 2);
            MoveY1(Y - height / 2);
            return this;
        }

        public override RecShort MoveCX(double X)
        {
            MoveX1(X - width / 2);
            return this;
        }

        public override RecShort MoveCY(double Y)
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

        public override RecShort Scale(double scale)
        {
            Scale(scale, scale);
            return this;
        }

        public override string ToString()
        {
            return this.GetType().Name + " x1:" + X1() + " x2:" + X2()
                   + " y1:" + Y1() + " y2:" + Y2();
        }

        public override RecShort IncrW(double dWidth)
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
            file.S(x);
            file.S(y);
            file.S(width);
            file.S(height);
        }

        public override void Load(FileGetter file)
        {
            x = file.S();
            y = file.S();
            width = file.S();
            height = file.S();
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

        public class RecThreadSafe : RecShort
        {
            private static readonly long serialVersionUID = 1L;
            private readonly RECIter iter = new RECIter(this);

            public override IEnumerator<COORDINATE> GetEnumerator()
            {
                return iter.Init();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}