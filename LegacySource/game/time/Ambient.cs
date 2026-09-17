using System;
using init.settings;
using snake2d;
using snake2d.util.color;
using snake2d.util.light;

namespace game.time
{
    sealed class Ambient
    {
        private readonly RGBImp moon2 = new RGBImp().r(0.65f).g(0.65f).b(1.7f);
        private readonly RGBImp dawn = new RGBImp().r(1.2f).g(1.0f).b(0.8f);
        private readonly RGBImp dusk = new RGBImp().r(1.6f).g(0.8f).b(0.6f);
        private readonly RGBImp day = new RGBImp().r(1.0f).g(1.0f).b(1.0f);

        private readonly RGBImp w = new RGBImp();
        private readonly RGBImp moon = new RGBImp();

        private readonly AmbientLight work = new AmbientLight();
        private readonly AmbientLight work2 = new AmbientLight();

        public Ambient()
        {
        }

        public void Apply(int x1, int x2, int y1, int y2, RGB tint)
        {
            double t = TIME.light().shadow.dtilt();

            moon.Copy(moon2);
            // moon.shade(0.65 + S.get().nightGamma.getD() * 0.7);

            double dd = 0.10;
            if (t < dd)
            {
                double d = t / dd;
                if (TIME.light().shadow.rising)
                {
                    if (TIME.light().shadow.isNight)
                    {
                        w.Interpolate(dusk, moon, d);
                    }
                    else
                    {
                        w.Interpolate(dawn, day, d);
                    }
                }
                else
                {
                    d = 1.0 - d;
                    if (TIME.light().shadow.isNight)
                    {
                        w.Interpolate(moon, dawn, d);
                    }
                    else
                    {
                        w.Interpolate(day, dusk, d);
                    }
                }
            }
            else
            {
                if (TIME.light().shadow.isNight)
                {
                    w.Copy(moon);
                }
                else
                {
                    w.Copy(day);
                }
            }

            double tilt = TIME.light().shadow.dtilt();
            double dir = TIME.light().shadow.dir();
            if (S.get().lightCycle.get() == 0)
            {
                w.Copy(day);
            }

            double strength = 0.55 + 0.75 * Math.Pow((1.0 - tilt), 2);
            strength *= 4.5;
            strength *= (0.95 + S.get().brightness.getD() * 0.5);

            double reflect = 0.2 + 0.3 * (Math.Sqrt(tilt));
            double oo = (1 - reflect) / 2;

            work.SetTilt(TIME.light().shadow.tilt()).SetDir(dir).Copy(w);
            work.Shade(strength);
            work.Multiply(tint);

            CORE.Renderer().lightDepthSet((byte)0);
            work2.Set(work, oo);
            work2.Register(x1, x2, y1, y2);

            CORE.Renderer().lightDepthSet((byte)127);
            work2.Set(work, oo);
            work2.Register(x1, x2, y1, y2);

            double dTilt = TIME.light().shadow.tilt();

            CORE.Renderer().shadeLight(false);
            work.SetTilt(90 - dTilt).SetDir(dir + 180).Copy(w);
            work2.Set(work, reflect);
            work2.Register(x1, x2, y1, y2);
        }
    }
}