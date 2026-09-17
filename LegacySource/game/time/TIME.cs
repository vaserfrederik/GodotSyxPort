using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using game.GAME;
using game.debug;
using game.faction;
using game.time.Seasons;
using game.time.TIMECYCLE;
using init.constant;
using init.paths;
using init.sprite.UI;
using init.value;
using snake2d.util.file;
using util.data;

namespace game.time
{
    [Serializable]
    public class TIME : GameResource
    {
        private static readonly long serialVersionUID = 1L;

        private readonly int secondsPerHour;
        private readonly int hoursPerDay;
        private readonly int secondsPerDay;
        private readonly double secondsPerDayI;
        private readonly int workHours;
        private readonly double workValue;

        private readonly double workSecondsWalkNext = 3;
        private readonly double workSeconds;

        private readonly int SERVICE_PER_DAY = 4;

        private double currentSecond;
        private double playedGame;
        private double offsetSecond = 0;

        private readonly Hours hours;
        private readonly Days days;
        private readonly Seasons seasons;
        private readonly Years years;
        private readonly Ages ages;
        private readonly Light light;

        private static TIME t;

        public TIME() : base("TIME", true)
        {
            TIME.t = this;

            Json jData = new Json(PATHS.CONFIG().init.gets("Time"));
            Json jText = new Json(PATHS.TEXT_MISC().gets("Time"));

            secondsPerHour = Config.sett().secondsPerHour;
            hoursPerDay = Config.sett().hoursPerDay;

            secondsPerDay = secondsPerHour * hoursPerDay;
            secondsPerDayI = 1.0 / secondsPerDay;
            workHours = hoursPerDay * 8 / 16;
            workValue = (double)hoursPerDay / workHours;
            workSeconds = workHours * secondsPerHour;

            hours = new Hours(secondsPerHour(), hoursPerDay());

            days = new Days((int)hours.cycleSeconds(), jData.i("DAYS_PER_SEASON", 2, 8));

            seasons = new Seasons(days.cycleSeconds(), jData, jText);

            years = new Years((int)seasons.cycleSeconds(), jData.i("YEARS_PER_AGE"));

            ages = new Ages((int)years.cycleSeconds(), jData, jText);

            currentSecond += days.bitSeconds() * 0.5;
            light = new Light();
            update(0, Profiler.DUMMY);

            GVALUES.FACTION.push("TIME_YEAR_PART", "time of year", UI.icons().s.clock, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return years().bitPartOf();
                }
            });

            GVALUES.FACTION.push("TIME_YEARS_SINCE_START", "time of year", UI.icons().s.clock, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    return years().bitsSinceStart();
                }
            }, false);
        }

        public static void set(double currentSecond)
        {
            t.currentSecond = currentSecond;
            t.update(0, Profiler.DUMMY);
        }

        public static double currentSecond()
        {
            return t.currentSecond;
        }

        public static Hours hours()
        {
            return t.hours;
        }

        public static Days days()
        {
            return t.days;
        }

        public static Season season()
        {
            return t.seasons.current();
        }

        public static Seasons seasons()
        {
            return t.seasons;
        }

        public static Years years()
        {
            return t.years;
        }

        public static Ages age()
        {
            return t.ages;
        }

        public static Light light()
        {
            return t.light;
        }

        static double getIncrementedTime(double time)
        {
            double currentSecond = t.currentSecond;
            currentSecond += time;
            if (currentSecond >= t.ages.cycleSeconds())
                currentSecond -= t.ages.cycleSeconds();
            else if (currentSecond < 0)
                currentSecond += t.ages.cycleSeconds();
            return currentSecond;
        }

        public double getFertility()
        {
            return 1.0;
        }

        public static int getWorkPerDay(double workSeconds)
        {
            double walkSpeed = 1.5;
            double toFrom = 150 / walkSpeed;
            double workNet = TIME.workSeconds() - toFrom;

            return (int)Math.Ceiling(workNet / (workSeconds + 3));
        }

        protected override void save(FilePutter file)
        {
            file.d(currentSecond);
            file.d(offsetSecond);
            file.d(playedGame);
        }

        protected override void load(FileGetter file)
        {
            currentSecond = file.d();
            offsetSecond = file.d();
            playedGame = file.d();
            update(0, Profiler.DUMMY);
        }

        public static double playedGame()
        {
            return t.playedGame;
        }

        protected override void update(double ds, Profiler prof)
        {
            currentSecond += ds;
            playedGame += ds;
            double current = currentSecond + offsetSecond;

            while (currentSecond >= ages.cycleSeconds())
                currentSecond -= ages.cycleSeconds();
            while (current >= ages.cycleSeconds())
                current -= ages.cycleSeconds();
            hours.update(current);
            days.update(current);
            seasons.update(current);
            years.update(current);
            ages.update(current);
            light.update(ds);
        }

        public static int servicePerDay()
        {
            return t.SERVICE_PER_DAY;
        }

        public static double workSeconds()
        {
            return t.workSeconds;
        }

        public static double workSecondsWalkNext()
        {
            return t.workSecondsWalkNext;
        }

        public static double workValue()
        {
            return t.workValue;
        }

        public static int workHours()
        {
            return t.workHours;
        }

        public static double secondsPerDayI()
        {
            return t.secondsPerDayI;
        }

        public static int secondsPerDay()
        {
            return t.secondsPerDay;
        }

        public static int hoursPerDay()
        {
            return t.hoursPerDay;
        }

        public static int secondsPerHour()
        {
            return t.secondsPerHour;
        }
    }
}