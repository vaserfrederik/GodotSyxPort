using game.time;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherDownfall : WeatherThing
    {
        private static readonly string ¤¤name = "Downfall";
        private static readonly string ¤¤desc = "The amount of downfall.";
        private static readonly double speed = 0.5 / (TIME.secondsPerHour());
        private double target = 0;

        static WeatherDownfall()
        {
            D.ts(typeof(WeatherDownfall));
        }

        public WeatherDownfall() : base(¤¤name, ¤¤desc)
        {
        }

        protected override void Update(double ds)
        {
            SetD(AdjustTowards(GetD(), ds * speed, target));
            target = 0;
        }

        public void SetTarget(double target)
        {
            this.target = target;
        }

        protected override void Init()
        {
            SetD(0);
        }
    }
}