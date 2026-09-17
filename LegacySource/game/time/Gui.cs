using System;
using init.settings;
using snake2d.util.datatypes;
using snake2d.util.light;
using view.main;

namespace game.time
{
    final class Gui : AmbientLight
    {
        private bool night;
        private Fire one;

        public Gui()
        {
            one = new Fire(0.1);
            one.setRadius(300);
            // one.set(C.WIDTH + 200, C.HEIGHT / 2 + 150);
            one.setFalloff(1f);
            one.setFlickerFactor(1f);

            setDir(220);
            setTilt(35);
            g(1.3f);
            b(1.3f);
            r(1.3f);
        }

        public void update(Light s, double ds)
        {
            if (S.get().uilightCycle.get() == 1 && TIME.light().nightIs())
            {
                night = true;
                one.setIntensity((float)(0.5f * s.partOfCircular()));
                one.flicker((float)(1.0 / 64.0));
                r(1.0 + 0.3 * (1.0 - s.partOfCircular()));
                g(1.0 + 0.3 * (1.0 - s.partOfCircular()));
                b(1.2 + 0.1 * (1.0 - s.partOfCircular()));
            }
            else
            {
                night = false;
                g(1.3f);
                b(1.3f);
                r(1.3f);
            }
        }

        public void register(float ds, RECTANGLE rec)
        {
            if (night)
            {
                one.set(VIEW.mouse());
                one.register();
            }
            base.register(rec);
        }

        public void register(float ds, int x1, int x2, int y1, int y2)
        {
            if (night)
            {
                one.set(VIEW.mouse());
                one.register();
            }
            base.register(x1, x2, y1, y2);
        }
    }
}