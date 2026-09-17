using System;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using view.interrupter;
using view.main;
using view.sett;

namespace settlement.weather
{
    final class UIWeather : ISidePanel
    {
        public UIWeather(SWEATHER w)
        {
            titleSet("weather");

            foreach (WeatherThing ww in w.all())
                add(ww);

            IDebugPanelSett.add("weather", new ACTION
            {
                public void exe()
                {
                    VIEW.s().panels.add(this, true);
                }
            });
        }

        private void add(WeatherThing t)
        {
            section.add(new GHeader(t.info().name), 0, section.body().y2() + 2);
            INTE in = new INTE
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return 100;
                }

                public int get()
                {
                    return (int)Math.Round(t.getD() * 100);
                }

                public void set(int ti)
                {
                    t.setD(ti / 100.0);
                }
            };
            section.add(new GSliderInt(in, 120, 16, false), 220, section.getLastY1());
        }
    }
}