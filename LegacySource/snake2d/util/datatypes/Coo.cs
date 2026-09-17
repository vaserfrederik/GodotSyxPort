using System;
using System.IO;

namespace Snake2D.Util.DataTypes
{
    [Serializable]
    public class Coo : COORDINATEE, SAVABLE
    {
        private static readonly long serialVersionUID = 1L;
        public static readonly Coo TMP = new Coo();
        public static readonly Coo TMP2 = new Coo();

        double X;
        double Y;

        public Coo()
        {
            this(0, 0);
        }

        public Coo(COORDINATE c)
        {
            this(c.x(), c.y());
        }

        public Coo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Set(double x, double y)
        {
            bool ret = x != X || y != Y;
            this.X = x;
            this.Y = y;
            return ret;
        }

        public void XSet(double x) { X = x; }
        public void YSet(double y) { Y = y; }

        public bool Set(COORDINATE other)
        {
            return this.Set(other.x(), other.y());
        }

        public void XIncrement(double amount) { X += amount; }
        public void YIncrement(double amount) { Y += amount; }

        public void Increment(COORDINATE other)
        {
            X += other.x();
            Y += other.y();
        }

        public void Increment(double x, double y)
        {
            X += x;
            Y += y;
        }

        public void Increment(COORDINATE other, double factor)
        {
            X += other.x() * factor;
            Y += other.y() * factor;
        }

        public void XInvert() { X *= -1; }
        public void YInvert() { Y *= -1; }

        public void XMakePos() { if (X < 0) X = -X; }
        public void XMakeNeg() { if (X > 0) X = -X; }
        public void YMakePos() { if (Y < 0) Y = -Y; }
        public void YMakeNeg() { if (Y > 0) Y = -Y; }

        public void Scale(double xScale, double yScale)
        {
            X *= xScale;
            Y *= yScale;
        }

        public void DeScale(double factorX, double factorY)
        {
            X -= X * factorX;
            Y -= Y * factorY;
        }

        public int X() { return (int)X; }
        public int Y() { return (int)Y; }

        public override string ToString()
        {
            return this.GetType().Name + " --> (" + X + ", " + Y + ")";
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
            file.D(X);
            file.D(Y);
        }

        public void Load(FileGetter file)
        {
            X = file.D();
            Y = file.D();
        }

        public void Clear()
        {
            X = -1;
            Y = -1;
        }
    }
}