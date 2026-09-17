using System;

namespace Settlement.Thing.Projectiles
{
    [Serializable]
    public class Trajectory
    {
        private const long serialVersionUID = 1L;
        private static readonly double G = 10 * C.TILE_SIZE;
        public static readonly double FRICTION = C.TILE_SIZE * 0.25;
        private static readonly double ANGLE45 = 45.0 / 360.0;
        private static readonly double ANGLE75 = 75.0 / 360;
        private static readonly double ANGLEMIN60 = -60.0 / 360.0;
        private static readonly double EMARGIN2 = C.TILE_SIZEH * C.TILE_SIZEH;

        public static readonly int HIT_HEIGHT = C.TILE_SIZE * 2;
        public static readonly int RELEASE_HEIGHT = C.TILE_SIZE * 2 + 5;

        private double vz;
        private double vx;
        private double vy;

        private static readonly int TFAR = 1;
        private static readonly int TSHORT = -1;
        private static readonly int THIT = 0;

        public double Vx()
        {
            return vx;
        }

        public double Vy()
        {
            return vy;
        }

        public double Vz()
        {
            return vz;
        }

        public static double Range(int h, double maxAngle, double velocity)
        {
            if (maxAngle > 45)
            {
                maxAngle = ANGLE45;
            }
            else if (maxAngle <= 0)
            {
                return 0;
            }
            else
            {
                maxAngle = maxAngle / 360;
            }

            return Length(h, maxAngle, velocity);
        }

        public void Set(double vx, double vy, double vz)
        {
            this.vx = vx;
            this.vy = vy;
            this.vz = vz;
        }

        public bool CalcLow(int height, int startX, int startY, int destX, int destY, double maxAngle, double velocity)
        {
            double minAngle = ANGLEMIN60;
            if (maxAngle > 45)
            {
                maxAngle = ANGLE45;
            }
            else if (maxAngle <= 0)
            {
                return false;
            }
            else
            {
                maxAngle = maxAngle / 360;
            }

            double dx = destX - startX;
            double dy = destY - startY;
            double L2 = dx * dx + dy * dy;

            int direction = Test(L2, height, maxAngle, velocity);
            switch (direction)
            {
                case THIT:
                    Set(dx, dy, maxAngle, velocity);
                    if (vz < 0 && height < 0)
                        vz = -vz;
                    return true; // direct hit, lets go
                case TFAR:
                    break; // too far, this is good
                case TSHORT:
                    return false; // too short, can't reach
            }

            double delta = maxAngle - minAngle;
            delta /= 2;

            // just adjust them a little for rounding errors
            minAngle -= 0.005;

            // binary search lets go!

            // we are going to aim lower initially
            double angle = minAngle + delta;
            // the result of the shot
            direction = Test(L2, height, angle, velocity);
            // our next delta angle
            delta /= 2;

            // just an emergency fail safe.
            int am = 0;
            while (am++ < 500)
            {
                int newDirection = Test(L2, height, angle, velocity);

                if (newDirection == THIT)
                {
                    Set(dx, dy, angle, velocity);
                    if (vz < 0 && height < 0)
                        vz = -vz;
                    return true;
                }
                else if (newDirection == TFAR)
                {
                    if (angle < minAngle)
                        return false;
                }
                else if (newDirection == TSHORT)
                {
                    if (angle > maxAngle)
                        return false;
                }

                if (newDirection != direction)
                {
                    direction = newDirection;
                    delta /= 2;
                }

                angle -= direction * delta;

                if (angle < minAngle || angle > maxAngle)
                {
                    return false;
                }
            }
            return false;
        }

        public bool CalcHigh(int height, int startX, int startY, int destX, int destY, double maxAngle, double velocity)
        {
            if (maxAngle <= 45)
            {
                return false;
            }
            else if (maxAngle > 75)
            {
                maxAngle = ANGLE75;
            }
            else
            {
                maxAngle = maxAngle / 360;
            }
            double minAngle = ANGLE45;

            double dx = destX - startX;
            double dy = destY - startY;
            double L2 = dx * dx + dy * dy;

            switch (Test(L2, height, ANGLE45, velocity))
            {
                case THIT:
                    Set(dx, dy, ANGLE45, velocity);
                    return true; // direct hit, lets go
                case TFAR:
                    break; // too far, this is good
                case TSHORT:
                    return false; // too short, can't go high
            }

            switch (Test(L2, height, maxAngle, velocity))
            {
                case THIT:
                    Set(dx, dy, maxAngle, velocity);
                    return true; // direct hit, lets go
                case TFAR:
                    return false; // too far, this is bad
                case TSHORT:
                    break; // too short, good
            }

            double delta = maxAngle - minAngle;
            delta /= 2;

            // just adjust them a little for rounding errors
            minAngle -= 0.005;
            maxAngle += 0.005;

            // binary search lets go!

            // we are going to aim higher initially
            double angle = minAngle + delta;
            // the result of the shot
            int direction = Test(L2, height, angle, velocity);
            // our next delta angle
            delta /= 2;

            am = 0;
            while (am++ < 500)
            {
                int newDirection = Test(L2, height, angle, velocity);

                if (newDirection == THIT)
                {
                    Set(dx, dy, angle, velocity);
                    if (vz < 0 && height < 0)
                        vz = -vz;
                    return true;
                }
                else if (newDirection == TFAR)
                {
                    if (angle < minAngle)
                        return false;
                }
                else if (newDirection == TSHORT)
                {
                    if (angle > maxAngle)
                        return false;
                }

                if (newDirection != direction)
                {
                    direction = newDirection;
                    delta /= 2;
                }

                angle += direction * delta;
            }
            return false;
        }

        private int Test(double L2, double height, double angle, double velocity)
        {
            double l = Length(height, angle, velocity);
            double m = L2 - l * l;
            if (m < -EMARGIN2)
            {
                return TFAR;
            }
            else if (m > EMARGIN2)
            {
                return TSHORT;
            }
            else
            {
                return THIT;
            }
        }

        private void Set(double dx, double dy, double angle, double velocity)
        {
            vz = Math.Sin(angle * 2 * Math.PI) * velocity;
            double v = Math.Cos(angle * 2 * Math.PI) * velocity;
            double l = Math.Sqrt(dx * dx + dy * dy);
            vx = v * dx / l;
            vy = v * dy / l;
        }

        public static double GetRange(double v, double time, double FRICTION)
        {
            if (v <= 0) return 0.0;
            return v * time - 0.5 * FRICTION * time * time;
        }

        static double GetLength(double v, double time)
        {
            return time * v - 0.5 * FRICTION * time * time;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("UnusedMember.Global", "IDE0051:Remove unused private members", Justification = "For debugging purposes")]
        private static void Debug(int i, double L2, double height, double angle, double velocity)
        {
            double vz = Math.Sin(angle * 2 * Math.PI) * velocity;
            double v = Math.Cos(angle * 2 * Math.PI) * velocity;
            double t = GetTime(height, vz);
            double l = GetLength(v, t);
            // LOG.ln(i + " " + (int)(angle * 360) + " " + v + " " + t + " " + l + " " + (L2 - l));
        }

        public static double GetTime(double height, double vz)
        {
            double discriminant = vz * vz + 2 * G * height;
            if (discriminant < 0) return double.NaN; // never hits

            double sqrtD = Math.Sqrt(discriminant);

            // two possible roots
            double t1 = (-vz + sqrtD) / G;
            double t2 = (-vz - sqrtD) / G;

            // we want the later (positive) time
            if (t1 >= 0 && t2 >= 0) return Math.Max(t1, t2);
            if (t1 >= 0) return t1;
            if (t2 >= 0) return t2;
            return double.NaN; // both negative → already below ground
        }
    }
}