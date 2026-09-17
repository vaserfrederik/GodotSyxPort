using System;
using game.time;
using snake2d.util.misc;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherClouds : WeatherThing
    {
        private static readonly string ¤¤name = "Clouds";
        private static readonly string ¤¤desc = "The amount of clouds.";
        private static readonly double speed = 1.0 / (1.5 * TIME.secondsPerHour());
        private double target;

        static WeatherClouds()
        {
            D.ts(typeof(WeatherClouds));
        }

        public WeatherClouds() : base(¤¤name, ¤¤desc)
        {
        }

        protected override void update(double ds)
        {
            setD(adjustTowards(getD(), ds * speed, target));
            target = 0;
        }

        public void setTarget(double target)
        {
            this.target = CLAMP.d(target, 0, 1);
        }

        protected override void init()
        {
            setD(0);
        }
    }
}