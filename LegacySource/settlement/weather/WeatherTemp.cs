using System;
using System.IO;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.weather
{
    public sealed class WeatherTemp : WeatherThing
    {
        private const double speed = 1.0 / TIME.SecondsPerDay();
        private double target;
        private int dayLast = -1;

        private static readonly CharSequence ¤¤format = "¤~";
        private static readonly CharSequence ¤¤name = "¤Temperature";
        private static readonly CharSequence ¤¤desc = "¤Temperature is determined by climate, season and chance. Extreme temperature on either end causes your subjects to be exposed and can lead to death. Hearths warm subjects. Well usage, or skinny dipping in bodies of water cools subjects. Clothes help greatly with both warmth and cold.";
        public static readonly double div = 4.0 / 60.0;

        static WeatherTemp()
        {
            D.ts(typeof(WeatherTemp));
        }

        public WeatherTemp() : base(¤¤name, ¤¤desc)
        {
        }

        protected override void update(double ds)
        {
            if (dayLast != TIME.Days().BitsSinceStart())
            {
                dayLast = TIME.Days().BitsSinceStart();
                target = average(TIME.Years().BitPartOf()) + RND.rFloat() * div * RND.rSign();
            }

            double t = target * (TIME.Light().NightIs() ? (1.0 - div * TIME.Light().PartOfCircular()) : 1);
            double temperature = AdjustTowards(getD(), ds * speed, t);
            temperature = CLAMP.d(temperature, 0, 1.0);
            setD(temperature);
        }

        public double Average(double partOfYear)
        {
            return Average(SETT.ENV().climate(), partOfYear);
        }

        public void SetTarget(double target)
        {
            this.target = target;
            dayLast = TIME.Days().BitsSinceStart();
        }

        private double Average(CLIMATE c, double partOfYear)
        {
            if (partOfYear < 0)
                partOfYear += 1 - (int)partOfYear;

            double p = partOfYear * TIME.Seasons().ALL.Size;
            int si = (int)p;
            p -= si;

            Season s = TIME.Seasons().ALL.Get(si);
            double startWV = 0;
            double endWV = 0;
            if (p < 0.5)
            {
                p += 0.5;
                startWV = TIME.Seasons().ALL.Get(si - 1).WinterValue;
                endWV = s.WinterValue;
            }
            else
            {
                startWV = s.WinterValue;
                endWV = TIME.Seasons().ALL.Get(si + 1).WinterValue;
                p -= 0.5;
            }

            double wv = startWV + p * (endWV - startWV);
            double t = c.TempCold * wv + c.TempWarm * (1.0 - wv);
            return t;
        }

        public double Heat()
        {
            if (getD() > 0.5)
                return 2 * (getD() - 0.5);
            return 0;
        }

        public double Cold()
        {
            if (getD() < 0.5)
                return (0.5 - getD()) * 2;
            return 0;
        }

        public double Target()
        {
            if (TIME.Light().DayIs())
                return CLAMP.d((target + div * TIME.Light().PartOfCircular()), 0, 1);
            else
                return CLAMP.d((target - div * TIME.Light().PartOfCircular()), 0, 1);
        }

        public void Format(Str srt)
        {
            if (getD() >= 0.5)
            {
                srt.Add('+');
                srt.Add((int)(2 * 60 * (getD() - 0.5)));
            }
            else
            {
                srt.Add('-');
                srt.Add((int)(50 * 2 * (0.5 - getD())));
            }
            srt.Add(¤¤format);
        }

        public int ITmp()
        {
            if (getD() >= 0.5)
                return (int)(2 * 60 * (getD() - 0.5));
            else
                return (int)(50 * 2 * (getD() - 0.5));
        }

        protected override void Save(FilePutter file)
        {
            base.Save(file);
            file.D(target);
            file.I(dayLast);
        }

        protected override void Load(FileGetter file)
        {
            base.Load(file);
            target = file.D();
            dayLast = file.I();
        }

        protected override void Init()
        {
            dayLast = -1;
            Update(0);

            setD(Target());
        }

        private const double min = 0.5 + 0.08;
        private const double max = 0.5 + 0.2;

        public double GetEntityTemp()
        {
            double d = getD();
            if (d < min)
                return (d - min) / min;
            else if (d > max)
                return (d - max) / (1.0 - max);
            return 0;
        }
    }
}