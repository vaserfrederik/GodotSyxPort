using System;

namespace Game.Time
{
    public class LightThunder
    {
        private readonly AmbientLight flash = new AmbientLight();
        private double flashI;
        private bool flashIs;

        private const float thunderTimer = 5f;
        private const float thunderLength = 0.2f;
        private float timer1 = RND.rFloat() * thunderTimer;
        private float timer2 = RND.rFloat() * thunderLength;

        public LightThunder()
        {
        }

        public void Update(double ds)
        {
            flashIs = false;

            if (timer1 > 0)
            {
                timer1 -= ds;
            }
            else if (timer2 > 0)
            {
                timer2 -= ds;
                flashIs = true;
            }
            else
            {
                timer2 = RND.rFloat() * thunderLength;
                if (RND.rInt(4) == 0)
                {
                    timer1 = RND.rFloat() * thunderTimer;
                    flash.SetDir(RND.rInt(360));
                    flash.SetTilt(-10 + RND.rInt(20));
                    flashI = RND.rFloat() * 5;
                }
                else
                {
                    flashI /= 1.5;
                }
            }
        }

        public void Apply(int x1, int x2, int y1, int y2)
        {
            if (flashIs)
            {
                flash.R(flashI).G(flashI).B(flashI);
                flash.Register(x1, x2, y1, y2);
            }
        }
    }
}