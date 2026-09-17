using System;

namespace snake2d.util.light
{
    public class AmbientLight : RGBImp, LIGHT_AMBIENT
    {
        public static readonly AmbientLight Strongmoonlight = new AmbientLight(1.0f, 1.0f, 1.3f, 135, 35);
        public static readonly AmbientLight none = new AmbientLight(0, 0, 0, 90, 90);
        public static readonly AmbientLight full = new AmbientLight(1, 1, 1, 0, 90);

        private double tilt;
        private double direction;

        private float dirX;
        private float dirY;
        private float dirZ;

        public AmbientLight()
        {
            //base.set(0, CORE.getDisplay().nativeWidth, 0, CORE.getDisplay().nativeHeight);
            SetDir(0);
            SetTilt(90);
        }

        public AmbientLight(double d, double e, double f, float direction, float tilt)
        {
            //base.set(0, CORE.getDisplay().nativeWidth, 0, CORE.getDisplay().nativeHeight);
            this.r((float)d);
            this.g((float)e);
            this.b((float)f);
            this.SetDir(direction);
            this.SetTilt(tilt);
        }

        public AmbientLight Set(AmbientLight other, double i)
        {
            copy(other).shade(i);

            tilt = other.tilt;
            direction = other.direction;
            dirX = other.dirX;
            dirY = other.dirY;
            dirZ = other.dirZ;
            return this;
        }

        public void SetFullLight()
        {
            set(1, 1, 1);
        }

        public override AmbientLight r(double red)
        {
            base.r(red);
            return this;
        }

        public override AmbientLight g(double green)
        {
            base.g(green);
            return this;
        }

        public override AmbientLight b(double blue)
        {
            base.b(blue);
            return this;
        }

        public AmbientLight SetDir(double deg)
        {
            direction = deg;
            calc();
            return this;
        }

        public double GetTilt()
        {
            return tilt;
        }

        public AmbientLight SetTilt(double tilt2)
        {
            if (tilt2 < -90)
                tilt2 = -90;
            else if (tilt2 > 90)
                tilt2 = 90;
            tilt = tilt2;
            calc();
            return this;
        }

        public double GetDir()
        {
            return direction;
        }

        private void calc()
        {
            dirZ = (float)(Math.Sin(Math.PI * tilt / 180));

            float q = (float)(Math.Cos(Math.PI * tilt / 180));

            dirX = q * (float)(Math.Cos(Math.PI * direction / 180));
            dirY = q * (float)(Math.Sin(Math.PI * direction / 180));

            //float tmp = (float)Math.PI * direction / 180;

            //dirX = (float)(Math.Sin(Math.PI * tilt / 180) * Math.Cos(tmp));
            //dirY = (float)(Math.Sin(Math.PI * tilt / 180) * Math.Sin(tmp));
            //dirZ = -(float)(Math.Cos(Math.PI * tilt / 180));
        }

        public void Register(RECTANGLE r)
        {
            Register(r.x1(), r.x2(), r.y1(), r.y2());
        }

        public void Register(int x1, int x2, int y1, int y2)
        {
            CORE.renderer().RegisterAmbient(this, x1, x2, y1, y2);
        }

        public override float x()
        {
            return dirX;
        }

        public override float y()
        {
            return dirY;
        }

        public override float z()
        {
            return dirZ;
        }

        public void Interpolate(AmbientLight from, AmbientLight to, double part)
        {
            base.Interpolate(from, to, part);
            this.tilt = from.tilt + (to.tilt - from.tilt) * part;
            this.direction = from.direction + (to.direction - from.direction) * part;
            this.calc();
        }
    }
}