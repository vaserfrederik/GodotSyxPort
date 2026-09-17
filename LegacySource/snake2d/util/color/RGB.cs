using System;

namespace Snake2D.Util.Color
{
    public interface RGB
    {
        double R();
        double G();
        double B();
    }

    public static class RGBConstants
    {
        public static readonly RGB WHITE = new RGBImp().Set(1, 1, 1);
    }

    public class RGBImp : RGB
    {
        private double red;
        private double green;
        private double blue;

        public double R()
        {
            return red;
        }

        public double G()
        {
            return green;
        }

        public double B()
        {
            return blue;
        }

        public RGBImp R(double r)
        {
            red = r;
            return this;
        }

        public RGBImp G(double g)
        {
            green = g;
            return this;
        }

        public RGBImp B(double b)
        {
            blue = b;
            return this;
        }

        public RGBImp Set(double r, double g, double b)
        {
            return R(r).G(g).B(b);
        }

        public RGBImp Shade(double shade)
        {
            red *= shade;
            green *= shade;
            blue *= shade;
            return this;
        }

        public RGBImp Copy(RGB other)
        {
            this.red = other.R();
            this.green = other.G();
            this.blue = other.B();
            return this;
        }

        public RGBImp Interpolate(RGB a, RGB b, double part)
        {
            R(a.R() + (b.R() - a.R()) * part);
            G(a.G() + (b.G() - a.G()) * part);
            B(a.B() + (b.B() - a.B()) * part);
            return this;
        }

        public RGBImp Multiply(RGB currentDay)
        {
            R(R() * currentDay.R());
            G(G() * currentDay.G());
            B(B() * currentDay.B());
            return this;
        }

        public RGBImp Multiply(RGB currentDay, double part)
        {
            R(R() * currentDay.R());
            G(G() * currentDay.G());
            B(B() * currentDay.B());
            return this;
        }
    }
}