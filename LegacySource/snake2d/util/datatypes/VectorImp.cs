using System;
using snake2d.util.file;
using snake2d.util.rnd;

namespace snake2d.util.datatypes
{
    public class VectorImp : SAVABLE, VECTOR
    {
        private double x = 0;
        private double y = -1;
        private double magnitude = 0;
        private DIR dir = DIR.N;

        public VectorImp() { }

        public VectorImp(double x, double y)
        {
            Set(x, y);
        }

        public double Set(double x, double y)
        {
            this.x = x;
            this.y = y;
            double d = Normalize();
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
            return d;
        }

        public VectorImp Set(VECTOR v)
        {
            this.x = v.nX();
            this.y = v.nY();
            magnitude = v.Magnitude();
            dir = v.Dir();
            return this;
        }

        public double Set(double aX, double aY, double bX, double bY)
        {
            return Set(bX - aX, bY - aY);
        }

        public double Set(double aX, double aY, COORDINATE b)
        {
            return Set(b.X() - aX, b.Y() - aY);
        }

        public double Set(COORDINATE a, COORDINATE b)
        {
            return Set(b.X() - a.X(), b.Y() - a.Y());
        }

        public double Set(RECTANGLE a, RECTANGLE b)
        {
            return Set(a.CX(), a.CY(), b.CX(), b.CY());
        }

        public double Set(RECTANGLE a, double bX, double bY)
        {
            return Set(a.CX(), a.CY(), bX, bY);
        }

        public void Randomize()
        {
            SetAngle(RND.rFloat() * 2);
        }

        public void SetAngle(double radians)
        {
            radians *= Math.PI;
            x = Math.Sin(radians);
            y = Math.Cos(radians);
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
        }

        public override double Magnitude()
        {
            return magnitude;
        }

        public void SetMagnitude(double m)
        {
            magnitude = m;
        }

        public override double nX()
        {
            return x;
        }

        public override double nY()
        {
            return y;
        }

        public override DIR Dir()
        {
            return dir;
        }

        public override double X()
        {
            return x * magnitude;
        }

        public override double Y()
        {
            return y * magnitude;
        }

        public void ReverseX()
        {
            x = -x;
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
        }

        public void ReverseY()
        {
            y = -y;
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
        }

        private double Normalize()
        {
            if (x == 0 && y == 0)
                return 0;

            double length = Math.Sqrt(x * x + y * y);
            x /= length;
            y /= length;
            return length;
        }

        public void Rotate(double degrees)
        {
            double radians = Math.ToRadians(degrees);
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            double newX = x * cos - y * sin;
            double newY = x * sin + y * cos;
            x = newX;
            y = newY;
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
        }

        public void RotateRad(double radians)
        {
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            double newX = x * cos - y * sin;
            double newY = x * sin + y * cos;
            x = newX;
            y = newY;
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
        }

        public VectorImp Rotate90()
        {
            double newX = -y;
            double newY = x;
            x = newX;
            y = newY;
            dir = DIR.ALL.Get(GetDirNr(this.x, this.y));
            return this;
        }

        private static int GetDirNr(double x, double y)
        {
            double abs = Math.Abs(x);

            if (abs < 0.38)
            {
                if (y > 0)
                    return 4;
                return 0;
            }
            else if (abs > 0.92)
            {
                if (x > 0)
                    return 2;
                return 6;
            }
            else if (y > 0)
            {
                if (x > 0)
                    return 3;
                return 5;
            }
            else
            {
                if (x > 0)
                    return 1;
                return 7;
            }
        }

        public override string ToString()
        {
            return "vector x: " + x + ", y:" + y + ", m:" + magnitude;
        }

        public void Save(FilePutter file)
        {
            file.D(x);
            file.D(y);
            file.D(magnitude);
            file.B((byte)dir.Id());
        }

        public void Load(FileGetter file)
        {
            x = file.D();
            y = file.D();
            magnitude = file.D();
            dir = DIR.ALL.Get(file.B());
        }

        public void Clear()
        {
            x = 0;
            y = 0;
            magnitude = 0;
            dir = DIR.C;
        }
    }
}