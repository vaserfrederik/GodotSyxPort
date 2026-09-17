using System;

namespace Snake2D.Util.DataTypes
{
    public interface DIMENSION
    {
        public int Width { get; }
        public int Height { get; }
    }

    public class Imp : DIMENSION
    {
        private double w, h;

        public int Width
        {
            get { return (int)w; }
        }

        public int Height
        {
            get { return (int)h; }
        }

        public Imp WidthSet(double w)
        {
            this.w = w;
            return this;
        }

        public Imp HeightSet(double h)
        {
            this.h = h;
            return this;
        }
    }
}