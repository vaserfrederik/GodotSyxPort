using System;

namespace snake2d.util.datatypes
{
    public interface COORDINATEE : COORDINATE
    {
        bool Set(double x, double y);
        void XSet(double x);
        void YSet(double y);
        bool Set(COORDINATE other);
        void XIncrement(double amount);
        void YIncrement(double amount);
        void Increment(COORDINATE other);
        void Increment(double x, double y);
        void Increment(COORDINATE other, double factor);
        void XInvert();
        void YInvert();
        void XMakePos();
        void XMakeNeg();
        void YMakePos();
        void YMakeNeg();
        void Scale(double xScale, double yScale);
        void DeScale(double factorX, double factorY);
        bool IsZero();
    }

    public abstract class Abs : COORDINATEE
    {
        public abstract double x();
        public abstract double y();

        public bool Set(double x, double y)
        {
            if (x == x() && y == y())
            {
                XSet(x);
                YSet(y);
                return false;
            }
            XSet(x);
            YSet(y);
            return true;
        }

        public void XIncrement(double amount)
        {
            XSet(x() + amount);
        }

        public void YIncrement(double amount)
        {
            YSet(y() + amount);
        }

        public void Increment(COORDINATE other)
        {
            Increment(other.x(), other.y());
        }

        public void Increment(double x, double y)
        {
            XIncrement(x);
            YIncrement(y);
        }

        public void Increment(COORDINATE other, double factor)
        {
            Increment(other.x() * factor, other.y() * factor);
        }

        public void XInvert()
        {
            XSet(-x());
        }

        public void YInvert()
        {
            YSet(-y());
        }

        public void XMakePos()
        {
            if (x() < 0)
                XInvert();
        }

        public void XMakeNeg()
        {
            if (x() > 0)
                XInvert();
        }

        public void YMakePos()
        {
            if (y() < 0)
                YInvert();
        }

        public void YMakeNeg()
        {
            if (y() > 0)
                YInvert();
        }

        public void Scale(double xScale, double yScale)
        {
            Set(x() * xScale, y() * yScale);
        }

        public void DeScale(double factorX, double factorY)
        {
            Increment(-x() * factorX, -y() * factorY);
        }

        public bool IsZero()
        {
            return x() == 0 && y() == 0;
        }
    }

    [Serializable]
    public class Imp : COORDINATEE
    {
        private static readonly long SerialVersionUID = 1L;
        private double X;
        private double Y;

        public Imp() : this(0, 0)
        {
        }

        public Imp(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Set(double x, double y)
        {
            bool ret = x != x() || y != y();
            X = x;
            Y = y;
            return ret;
        }

        public void XSet(double x)
        {
            Set(x, Y);
        }

        public void YSet(double y)
        {
            Set(X, y);
        }

        public bool Set(COORDINATE other)
        {
            return this.Set(other.x(), other.y());
        }

        public void XIncrement(double amount)
        {
            XSet(X + amount);
        }

        public void YIncrement(double amount)
        {
            YSet(Y + amount);
        }

        public void Increment(COORDINATE other)
        {
            Increment(other.x(), other.y());
        }

        public void Increment(double x, double y)
        {
            XSet(X + x);
            YSet(Y + y);
        }

        public void Increment(COORDINATE other, double factor)
        {
            Increment(other.x() * factor, other.y() * factor);
        }

        public void XInvert()
        {
            XSet(-X);
        }

        public void YInvert()
        {
            YSet(-Y);
        }

        public void XMakePos()
        {
            if (X < 0)
                XSet(-X);
        }

        public void XMakeNeg()
        {
            if (X > 0)
                XSet(-X);
        }

        public void YMakePos()
        {
            if (Y < 0)
                YSet(-Y);
        }

        public void YMakeNeg()
        {
            if (Y > 0)
                YSet(-Y);
        }

        public void Scale(double xScale, double yScale)
        {
            Set(X * xScale, Y * yScale);
        }

        public void DeScale(double factorX, double factorY)
        {
            Set(X - X * factorX, Y - Y * factorY);
        }

        public double x()
        {
            return X;
        }

        public double y()
        {
            return Y;
        }

        public override string ToString()
        {
            return GetType().Name + " x:" + X + " y:" + Y;
        }

        public bool IsZero()
        {
            return x() == 0 && y() == 0;
        }
    }
}