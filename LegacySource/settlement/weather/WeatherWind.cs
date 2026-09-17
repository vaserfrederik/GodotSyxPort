using System;
using System.IO;
using util.data;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherWind : WeatherThing
    {
        private static readonly string ¤¤name = "Wind";
        private static readonly string ¤¤desc = "Wind Strength";

        static WeatherWind()
        {
            D.ts(typeof(WeatherWind));
        }

        public WeatherWind() : base(¤¤name, ¤¤desc)
        {
            target = RND.rFloat();
            setD(RND.rFloat());
            dayM = -0.5 + RND.rFloat();
        }

        private const double MAX = 1.0;
        private double target;
        private double dayM;
        private double speed = 0.1;
        private int day = -1;
        private double t;

        protected override void update(double ds)
        {
            double next = adjustTowards(getD(), ds * speed, target);
            if (next == getD())
                reset();
            setD(CLAMP.d(next, 0, MAX));
            t += ds * getD();
            if (t > int.MaxValue)
                t -= int.MaxValue;
            //setD(0.5);
        }

        private void reset()
        {
            if (day != TIME.days().bitsSinceStart())
            {
                day = TIME.days().bitsSinceStart();
                dayM = -0.5 + RND.rFloat();
            }
            target = CLAMP.d(dayM + RND.rFloat(), 0, 1);
            speed = 0.1 * (0.25 + RND.rFloat() * 0.75);
        }

        public void setDayTarget(double target)
        {
            dayM = target;
        }

        protected override void save(FilePutter file)
        {
            file.d(target);
            file.d(dayM);
            file.d(t);
            file.i(day);
            base.save(file);
        }

        protected override void load(FileGetter file)
        {
            target = file.d();
            dayM = file.d();
            t = file.d();
            day = file.i();
            base.load(file);
        }

        public double x()
        {
            return -getD();
        }

        public double y()
        {
            return -getD();
        }

        public double dirX()
        {
            return -1;
        }

        public double dirY()
        {
            return 1;
        }

        protected override void init()
        {
            reset();
            setD(target);
        }

        public readonly DOUBLE time = new DOUBLE
        {
            public override double getD()
            {
                return t;
            }
        };
    }
}