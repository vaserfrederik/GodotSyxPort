using System;
using System.IO;

namespace Settlement.Entity
{
    using Init.Constant;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;

    public interface ESpeed : VECTOR
    {
    }

    public class Imp : ESpeed
    {
        private double x = 0;
        private double y = -1;
        private double magnitude = 0;
        private double targetMagnitude = 0;
        private double acceleration = 10 * C.TILE_SIZE;
        private double topMagnitude = 10 * C.TILE_SIZE;
        // private const int MAX = 31 * C.TILE_SIZE;

        private DIR dir = DIR.N;
        private byte lastDir = (byte)dir.Id();

        public Imp()
        {
        }

        public Imp AccelerationInit(double a)
        {
            acceleration = a;
            return this;
        }

        public Imp MagnitudeMaxInit(double topSpeed)
        {
            this.topMagnitude = topSpeed;
            return this;
        }

        public double MagnitudeMax()
        {
            return topMagnitude;
        }

        public Imp MagnitudeInit(double m)
        {
            this.magnitude = m;
            return this;
        }

        /**
         * Sets direction, and magnitude
         * 
         * @param x
         * @param y
         * @return the length of x,y
         */
        public void SetRaw(double x, double y)
        {
            this.x = x;
            this.y = y;
            magnitude = Normalize();
            SetDir();
        }

        public void SetRawNormalized(double x, double y, double magnitude)
        {
            this.x = x;
            this.y = y;
            this.magnitude = magnitude;
            SetDir();
        }

        public void SetRaw(DIR d, double magnitude)
        {
            this.x = d.XN();
            this.y = d.YN();
            this.magnitude = magnitude;
            SetDirCurrent(d);
        }

        public Imp Set(Imp master)
        {
            this.x = master.x;
            this.y = master.y;
            magnitude = master.magnitude;
            acceleration = master.acceleration;
            topMagnitude = master.topMagnitude;
            MagnitudeTargetSetPrecise(master.targetMagnitude);
            SetDirCurrent(master.dir);
            return this;
        }

        /**
         * Sets direction, adjusts magnitude to simulate a turn.
         * 
         * @param x
         * @param y
         * @return
         */
        public Imp Turn2(double x, double y)
        {
            double ox = this.x;
            double oy = this.y;
            this.x = x;
            this.y = y;
            Normalize();
            SetDir();

            if (magnitude > 0)
            {
                double dot = ox * this.x + oy * this.y;
                dot += 1.0;
                dot /= 2.0;
                magnitude *= dot;
            }
            return this;
        }

        public Imp Turn2(DIR d)
        {
            double ox = this.x;
            double oy = this.y;
            this.x = d.XN();
            this.y = d.YN();
            SetDirCurrent(d);

            if (magnitude > 0)
            {
                double dot = ox * this.x + oy * this.y;
                dot += 1.0;
                dot /= 2.0;
                magnitude *= dot;
            }
            return this;
        }

        public Imp Turn2(BODY_HOLDER h, double x, double y)
        {
            return Turn2(h.Body().CX(), h.Body().CY(), x, y);
        }

        public Imp Turn2(double aX, double aY, double bX, double bY)
        {
            return Turn2(bX - aX, bY - aY);
        }

        public Imp Turn2(COORDINATE a, COORDINATE b)
        {
            return Turn2(b.X() - a.X(), b.Y() - a.Y());
        }

        public Imp Turn2(RECTANGLE a, RECTANGLE b)
        {
            return Turn2(a.CX(), a.CY(), b.CX(), b.CY());
        }

        public Imp TurnRandom()
        {
            Turn2Angle(RND.RFloat() * 2);
            return this;
        }

        public Imp Turn2Angle(double angle)
        {
            double ox = this.x;
            double oy = this.y;
            angle *= Math.PI;
            x = Math.Sin(angle);
            y = Math.Cos(angle);
            SetDir();
            if (magnitude > 0)
            {
                double dot = ox * this.x + oy * this.y;
                dot += 1.0;
                dot /= 2.0;
                magnitude *= dot;
            }
            return this;
        }

        public Imp TurnWithAngel(double degrees)
        {
            double ox = this.x;
            double oy = this.y;
            double radians = Math.ToRadians(degrees);
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            double newX = x * cos - y * sin;
            double newY = x * sin + y * cos;
            x = newX;
            y = newY;
            SetDir();
            if (magnitude > 0)
            {
                double dot = ox * this.x + oy * this.y;
                dot += 1.0;
                dot /= 2.0;
                magnitude *= dot;
            }
            return this;
        }

        public Imp Turn90()
        {
            double newX = y;
            double newY = -x;
            x = newX;
            y = newY;
            if (magnitude > 0)
            {
                magnitude *= 0.5;
            }
            SetDir();
            return this;
        }

        public override double Magnitude()
        {
            return magnitude;
        }

        public bool ImpulseBreak(double power)
        {
            magnitude -= topMagnitude * power;
            if (magnitude < 0)
            {
                magnitude = 0;
                return true;
            }
            return magnitude == 0;
        }

        public Imp Sprint(double power)
        {
            magnitude += topMagnitude * power;
            if (magnitude > topMagnitude)
                magnitude = topMagnitude;
            return this;
        }

        public double MagintudeMax()
        {
            return topMagnitude;
        }

        public double MagnitudeTarget()
        {
            return targetMagnitude;
        }

        public Imp MagnitudeTargetSet(double scale)
        {
            MagnitudeTargetSetPrecise(scale * topMagnitude);
            return this;
        }

        public Imp MagnitudeTargetSetPrecise(double scale)
        {
            targetMagnitude = scale;
            return this;
        }

        public double MagnitudeRelative()
        {
            return magnitude / topMagnitude;
        }

        public override double NX()
        {
            return x;
        }

        public override double NY()
        {
            return y;
        }

        public override double M()
        {
            return magnitude;
        }

        public override DIR Dir()
        {
            return dir;
        }

        public override void SetDir(DIR dir)
        {
            this.dir = dir;
        }

        public override void SetLastDir(byte lastDir)
        {
            this.lastDir = lastDir;
        }

        public override byte LastDir()
        {
            return lastDir;
        }

        public override void SetM(double magnitude)
        {
            this.magnitude = magnitude;
        }

        public override void SetNX(double x)
        {
            this.x = x;
        }

        public override void SetNY(double y)
        {
            this.y = y;
        }

        public override void SetDir(byte dirId)
        {
            dir = DIR.ALL[dirId];
        }

        public override void SetLastDir(DIR dir)
        {
            lastDir = (byte)dir.Id();
        }

        public override string ToString()
        {
            return $"x: {x}, y: {y}, m: {magnitude}";
        }

        public bool MagnitudeAdjust(double ds, double d, double bonus)
        {
            double min = topMagnitude * 0.25;
            double target = targetMagnitude * bonus;
            if (target >= min && magnitude < min)
                magnitude = min;
            if (magnitude < target)
            {
                magnitude += acceleration * d * ds;
                if (magnitude > target)
                {
                    magnitude = target;
                    return true;
                }
                return false;
            }
            else if (magnitude > target)
            {
                magnitude -= acceleration * d * ds;
                if (magnitude < target)
                {
                    magnitude = target;
                    return true;
                }
                return false;
            }
            return true;
        }

        public void Brake(double ds)
        {
            magnitude -= ds * (8 * C.TILE_SIZE + magnitude * 0.1);
            if (magnitude < 0)
                magnitude = 0;
        }

        protected static readonly double AIR_REDUCER = 0.0025;

        public void ApplyAirFriction(float ds)
        {
            double m = magnitude * AIR_REDUCER;
            magnitude -= m * ds;
        }

        public bool IsZero()
        {
            return magnitude == 0;
        }

        public void Check()
        {
            if (!double.IsFinite(magnitude) || !double.IsFinite(x) || !double.IsFinite(y))
                throw new RuntimeException($"{magnitude} {x} {y}");
        }

        public double Dot(double norX, double norY)
        {
            return Math.Abs(x * norX + y * norY);
        }

        public void Save(FilePutter file)
        {
            file.D(x).D(y).D(magnitude).D(targetMagnitude).D(acceleration).D(topMagnitude);
            file.B((byte)dir.Id());
            file.B(lastDir);
        }

        public void Load(FileGetter file)
        {
            x = file.D();
            y = file.D();
            magnitude = file.D();
            targetMagnitude = file.D();
            acceleration = file.D();
            topMagnitude = file.D();
            dir = DIR.ALL[file.B()];
            lastDir = file.B();
        }
    }
}