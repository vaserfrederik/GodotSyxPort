using System;

namespace Snake2D.Util.Light
{
    public class Fire : PointLight
    {
        private static readonly long SerialVersionUID = 1L;
        private float offsetX = 0;
        private float offsetY = 0;
        private float offsetHeight = 0;
        private float intensityOffset = 0;
        private float timer = 0.05f;

        private float tmpRed;
        private float tmpGreen;
        private float tmpBlue;

        private float flickerFactor;

        public Fire(double intensity)
            : base((float)intensity, (float)(0.7 * intensity), (float)(0.3 * intensity))
        {
            SetZ(50);
            SetRadius(100);
            flickerFactor = 20;
            timer = -1;
            Flicker(0);
        }

        public void Flicker(float ds)
        {
            timer -= ds;
            if (timer > 0)
                return;

            offsetX = -flickerFactor + Rnd.RFloat(2 * flickerFactor);
            offsetY = -flickerFactor + Rnd.RFloat(2 * flickerFactor);
            offsetHeight = Rnd.RFloat(flickerFactor / 4);
            intensityOffset = (float)(1 + 0.2 * Rnd.RSign() * Rnd.RExpo());
            tmpRed = GetRed() * intensityOffset;
            tmpGreen = GetGreen() * intensityOffset;
            tmpBlue = GetBlue() * intensityOffset;

            timer = 0.025f + Rnd.RFloat(0.05f);
        }

        public override float Cx()
        {
            return base.x() + offsetX;
        }

        public override float Cy()
        {
            return base.y() + offsetY;
        }

        public override float Cz()
        {
            return base.cz() + offsetHeight;
        }

        public override float GetRed()
        {
            return tmpRed;
        }

        public override float GetGreen()
        {
            return tmpGreen;
        }

        public override float GetBlue()
        {
            return tmpBlue;
        }

        public float GetFlickerFactor()
        {
            return flickerFactor;
        }

        public void SetFlickerFactor(float flickerFactor)
        {
            this.flickerFactor = flickerFactor;
        }

        public void SetIntensity(double d)
        {
            SetRed((float)d);
            SetGreen((float)(0.7 * d));
            SetBlue((float)(0.3 * d));
        }
    }
}