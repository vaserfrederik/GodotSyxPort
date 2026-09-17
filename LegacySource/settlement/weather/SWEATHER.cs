using System;
using System.Collections.Generic;
using System.IO;
using game.debug;
using game.time;
using init.settings;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.rendering;

namespace settlement.weather
{
    public sealed class SWEATHER : SettResource
    {
        private readonly RGBImp lightColor = new RGBImp();
        private readonly WeatherDownfallRenderer rainer = new WeatherDownfallRenderer();

        static SWEATHER()
        {
            WeatherThing.all = new LinkedList<WeatherThing>();
        }

        public readonly WeatherMoisture moisture = new WeatherMoisture();
        public readonly WeatherSnow snow = new WeatherSnow();
        public readonly WeatherIce ice = new WeatherIce();
        public readonly WeatherWind wind = new WeatherWind();
        public readonly WeatherTemp temp = new WeatherTemp();
        public readonly WeatherClouds clouds = new WeatherClouds();
        public readonly WeatherDownfall rain = new WeatherDownfall();
        public readonly WeatherThunder thunder = new WeatherThunder();
        public readonly WeatherGrowth growth = new WeatherGrowth();
        public readonly WeatherGrowthRipe growthRipe = new WeatherGrowthRipe();
        public readonly RainEvent downfall = new RainEvent();

        public SWEATHER() : base("WEATHER", true)
        {
            new UIWeather(this);
        }

        protected override void update(double ds, Profiler profiler)
        {
            lightColor.set(1, 1, 1);
            if (temp.cold() > 0 && TIME.light().dayIs())
            {
                double d = 0.2 * CLAMP.d(temp.cold() * 2, 0, 1);
                lightColor.set(1 - d / 2, 1 - d / 2, 1 + d);
            }
            else if (temp.heat() > 0 && TIME.light().dayIs())
            {
                double d = 0.3 * CLAMP.d(temp.heat() * 2, 0, 1);
                lightColor.set(1 + d / 2, 1 + d / 4, 1 - d / 4);
            }
            lightColor.shade(1.0 - 0.5 * clouds.getD());
            foreach (var t in WeatherThing.all)
                t.update(ds);

            downfall.update(ds);
        }

        protected override void init(bool loaded)
        {
            if (loaded)
                return;
            foreach (var t in WeatherThing.all)
                t.init();
            downfall.saver.clear();
        }

        protected override void save(FilePutter file)
        {
            foreach (var t in WeatherThing.all)
                t.save(file);
            downfall.saver.save(file);
        }

        protected override void load(FileGetter file)
        {
            foreach (var t in WeatherThing.all)
                t.load(file);
            downfall.saver.load(file);
        }

        public LIST<WeatherThing> all()
        {
            return WeatherThing.all;
        }

        public RGBImp lightColor()
        {
            return lightColor;
        }

        public void apply(RECTANGLE rec)
        {
            apply(rec.x1(), rec.x2(), rec.y1(), rec.y2());
            if (S.get().downpour.get() == 1)
                thunder.apply(0, 0, 0, 0);
        }

        public void apply(int x1, int x2, int y1, int y2)
        {
            TIME.light().apply(x1, x2, y1, y2, lightColor);
            if (S.get().downpour.get() == 1)
                thunder.apply(x1, x2, y1, y2);
        }

        public void renderDownfall(Renderer r, float ds, RenderData data, int zoomout)
        {
            if (S.get().downpour.get() == 1)
                rainer.render(r, ds, data, zoomout);
        }
    }
}