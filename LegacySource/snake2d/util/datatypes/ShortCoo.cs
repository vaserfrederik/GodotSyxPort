using System;
using System.IO;

namespace snake2d.util.datatypes
{
    [Serializable]
    public class ShortCoo : COORDINATEE, SAVABLE
    {
        private short X;
        private short Y;

        public ShortCoo()
        {
            this(0, 0);
        }

        public ShortCoo(double x, double y)
        {
            X = (short)x;
            Y = (short)y;
        }

        public bool Set(double x, double y)
        {
            bool ret = x != X || y != Y;
            this.X = (short)x;
            this.Y = (short)y;
            return ret;
        }

        public void XSet(double x) { X = (short)x; }
        public void YSet(double y) { Y = (short)y; }
        public bool Set(COORDINATE other) { return this.Set(other.X(), other.Y()); }
        public void XIncrement(double amount) { X += amount; }
        public void YIncrement(double amount) { Y += amount; }
        public void Increment(COORDINATE other)
        {
            X += other.X();
            Y += other.Y();
        }
        public void Increment(double x, double y)
        {
            X += x;
            Y += y;
        }
        public void Increment(COORDINATE other, double factor)
        {
            X += other.X() * factor;
            Y += other.Y() * factor;
        }

        public void XInvert() { X *= -1; }
        public void YInvert() { Y *= -1; }
        public void XMakePos() { if (X < 0) X = (short)-X; }
        public void XMakeNeg() { if (X > 0) X = (short)-X; }
        public void YMakePos() { if (Y < 0) Y = (short)-Y; }
        public void YMakeNeg() { if (Y > 0) Y = (short)-Y; }

        public void Scale(double xScale, double yScale)
        {
            X *= xScale;
            Y *= yScale;
        }

        /**
         * the coordinates will be reduced by the factors
         * @param factorX
         * @param factorY
         */
        public void DeScale(double factorX, double factorY)
        {
            X -= X * factorX;
            Y -= Y * factorY;
        }

        public int X() { return (int)X; }
        public int Y() { return (int)Y; }

        public override string ToString()
        {
            return this.GetType().Name + " x:" + X + " y:" + Y;
        }

        public void Decrease(double amountX, double amountY)
        {
            if (X < -amountX)
            {
                X += amountX;
            }
            else if (X > amountX)
            {
                X -= amountX;
            }
            else
            {
                X = 0;
            }
            if (Y < -amountY)
            {
                Y += amountY;
            }
            else if (Y > amountY)
            {
                Y -= amountY;
            }
            else
            {
                Y = 0;
            }
        }

        public bool IsZero()
        {
            return X() == 0 && Y() == 0;
        }

        public void Save(FilePutter file)
        {
            file.S(X);
            file.S(Y);
        }

        public void Load(FileGetter file)
        {
            X = file.S();
            Y = file.S();
        }

        public void Clear()
        {
            X = 0;
            Y = 0;
        }
    }
}