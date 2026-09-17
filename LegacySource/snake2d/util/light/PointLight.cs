using System;

namespace snake2d.util.light
{
    public class PointLight : Coo, LIGHT_POINT
    {
        private static readonly long serialVersionUID = 1L;
        private float red = 3;
        private float green = 3;
        private float blue = 3;
        private float z = 50;
        private float falloff = 2;
        private int radius;

        private int index;

        public PointLight()
        {
            SetRadius(100);
        }

        public PointLight(double red, double green, double blue)
        {
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
            SetRadius(300);
        }

        public PointLight(double red, double green, double blue, int x, int y, int height)
        {
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
            Set(x, y);
            SetRadius(300);
            SetZ(height);
        }

        public PointLight(double red, double green, double blue, int x, int y, int height, int radius)
        {
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
            Set(x, y);
            SetRadius(radius);
            SetZ(height);
        }

        public static PointLight GetFlashLight()
        {
            PointLight p = new PointLight();
            p.SetRed(3f);
            p.SetGreen(3f);
            p.SetBlue(3f);
            p.SetZ(45);
            p.SetRadius(300);
            return p;
        }

        public static PointLight GetFlashLight2()
        {
            PointLight p = new PointLight();
            p.SetRed(3f);
            p.SetGreen(3f);
            p.SetBlue(3f);
            p.SetZ(145);
            p.SetRadius(300);
            return p;
        }

        public void Register()
        {
            CORE.Renderer().RegisterLight(this, X - radius, X + radius, Y - radius, Y + radius);
        }

        public void Register(byte ne, byte se, byte sw, byte nw)
        {
            CORE.Renderer().RegisterLight(this, X - radius, X + radius, Y - radius, Y + radius, ne, se, sw, nw);
        }

        public float GetRed()
        {
            return red;
        }

        public PointLight SetRed(double red)
        {
            this.red = (float)red;
            return this;
        }

        public float GetGreen()
        {
            return green;
        }

        public PointLight SetGreen(double green)
        {
            this.green = (float)green;
            return this;
        }

        public float GetBlue()
        {
            return blue;
        }

        public PointLight SetBlue(double blue)
        {
            this.blue = (float)blue;
            return this;
        }

        public float Cz()
        {
            return z;
        }

        public void SetZ(int height)
        {
            this.z = height;
        }

        public float GetFalloff()
        {
            return falloff;
        }

        public void SetFalloff(float falloff)
        {
            this.falloff = falloff;
        }

        public int GetIndex()
        {
            return index;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetRadius(int radius)
        {
            this.radius = radius;
        }

        public int GetRadius()
        {
            return radius;
        }

        public bool IsWithinRec(RECTANGLE other)
        {
            // TODO Auto-generated method stub
            return false;
        }

        public float Cx()
        {
            return (float)X;
        }

        public float Cy()
        {
            return (float)Y;
        }
    }
}