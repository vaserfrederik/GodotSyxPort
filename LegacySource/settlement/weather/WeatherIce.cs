using System;
using game.time;
using settlement.main;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherIce : WeatherThing
    {
        private static readonly string ¤¤name = "Ice";
        private static readonly string ¤¤desc = "Amount of ice on the water.";
        private static readonly double thawspeed = 1.0 / (8 * TIME.secondsPerHour());

        static WeatherIce()
        {
            D.ts(typeof(WeatherIce));
        }

        public WeatherIce() : base(¤¤name, ¤¤desc)
        {
        }

        public override void update(double ds)
        {
            double c = getD();
            double d = -(SETT.WEATHER().temp.getD() - 0.5) * 2 * thawspeed * ds;
            setD(c + d);
        }

        protected override void init()
        {
            double snow = 0;
            double p = TIME.years().bitPartOf();
            double tmp1 = SETT.WEATHER().temp.average(p - 0.2);
            double tmp2 = SETT.WEATHER().temp.average(p);
            if (tmp1 < 0.5 && tmp2 < 0.5)
            {
                snow = 1;
            }
            else if (tmp1 < 0.5)
            {
                snow = 1 - (tmp2 - 0.5) * 16;
            }
            setD(snow);
        }

        public bool canBatheOutside()
        {
            return getD() < 0.1;
        }
    }
}