using System;
using System.Text;

namespace Util.Text
{
    public class DicTime
    {
        public static string ¤¤Day = "¤Day";
        public static string ¤¤Days = "¤Days";
        public static string ¤¤Season = "¤Season";
        public static string ¤¤Seasons = "¤Seasons";
        public static string ¤¤Year = "¤Year";
        public static string ¤¤Years = "¤Years";
        public static string ¤¤Age = "¤Age";
        public static string ¤¤Ages = "¤Ages";
        public static string ¤¤Hour = "¤Hours";
        public static string ¤¤Hours = "¤Hours";
        public static string ¤¤Minute = "¤Minute";
        public static string ¤¤Minutes = "¤Minutes";
        public static string ¤¤Second = "¤Second";
        public static string ¤¤Seconds = "¤Seconds";
        public static string ¤¤Today = "¤Today";
        private static string ¤¤now = "¤now";

        private static string ¤¤1OfSomething = "¤1 {0}";
        private static string ¤¤MoreOfSomething = "¤{0} {1}";

        private static string ¤¤dateFormat = "¤Day {0} of {1}, Year {2} of the {3}";
        private static string ¤¤dateFormatShort = "¤{0}/{1} - {2} {3}";

        private static string ¤¤2SomethingAgo = "{0}, {1} ago";
        private static string ¤¤1SomethingAgo = "{0} ago";

        private static string ¤¤yearsDays = "{0} years, {1} days";

        private static readonly StringBuilder f = new StringBuilder(32);
        private static readonly StringBuilder f2 = new StringBuilder(32);

        static DicTime()
        {
            D.ts(typeof(DicTime));
        }

        public static StringBuilder setDate(StringBuilder text, int second)
        {
            int age = (second % (int)TIME.age().cycleSeconds()) / (int)TIME.age().bitSeconds();
            int year = (second % (int)TIME.years().cycleSeconds()) / (int)TIME.years().bitSeconds();
            int season = (second % (int)TIME.seasons().cycleSeconds()) / (int)TIME.seasons().bitSeconds();
            int day = (second % (int)TIME.days().cycleSeconds()) / (int)TIME.days().bitSeconds();
            text.Clear().AppendFormat(¤¤dateFormat);
            text.Insert(0, day + 1);
            text.Insert(1, TIME.seasons().bitName(season));
            text.Insert(2, year + 1);
            text.Insert(3, TIME.age().bitName(age));
            return text;
        }

        public static StringBuilder setDateShort(StringBuilder text, int second)
        {
            int age = (second % (int)TIME.age().cycleSeconds()) / (int)TIME.age().bitSeconds();
            int year = (second % (int)TIME.years().cycleSeconds()) / (int)TIME.years().bitSeconds();
            int season = (second % (int)TIME.seasons().cycleSeconds()) / (int)TIME.seasons().bitSeconds();
            int day = (second % (int)TIME.days().cycleSeconds()) / (int)TIME.days().bitSeconds();
            text.Clear().AppendFormat(¤¤dateFormatShort);
            text.Insert(0, day + 1);
            text.Insert(1, TIME.seasons().bitName(season));
            text.Insert(2, year + 1);
            string aa = TIME.age().bitName(age);
            f.Clear();
            f.Append(aa[0].ToString().ToUpper()).Append('.');
            for (int i = 1; i < aa.Length; i++)
            {
                if (aa[i - 1] == ' ')
                {
                    f.Append(aa[i].ToString().ToUpper()).Append('.');
                }
            }

            text.Insert(3, f);
            return text;
        }

        public static void setTime(StringBuilder text, double second)
        {
            double s = second % TIME.secondsPerDay();
            int h = (int)(s / TIME.secondsPerHour());
            s = s % TIME.secondsPerHour();

            s = (int)(60.0 * s / TIME.secondsPerHour());
            if (h < 10)
                text.Append('0');
            text.Append(h);
            text.Append(':');
            if (s < 10)
                text.Append('0');
            text.Append((int)s);
        }

        public static StringBuilder setAgo(StringBuilder text, double seconds)
        {
            return setYearDay(text, seconds, ¤¤2SomethingAgo);
        }

        public static StringBuilder setSpanDays(StringBuilder text, double from, double to)
        {
            text.Clear();
            int day = (int)(from / (int)TIME.days().bitSeconds());
            text.Append(day).Append('-');
            day = (int)(to / (int)TIME.days().bitSeconds());
            text.Append(day).Append(¤¤1SomethingAgo);
            text.Insert(0, ¤¤Days);
            return text;
        }

        private static StringBuilder setYearDay(StringBuilder text, double seconds, string F)
        {
            int secondAgo = (int)seconds;
            int year = (secondAgo / (int)TIME.years().bitSeconds());
            secondAgo -= year * TIME.years().bitSeconds();
            int day = (secondAgo / (int)TIME.days().bitSeconds());

            if (year == 0 && day == 0)
            {
                text.Clear().Append(¤¤now);
                return text;
            }

            text.Clear().AppendFormat(F);
            text.Insert(0, setDays(f.Clear(), day));
            text.Insert(1, setYears(f.Clear(), year));
            return text;
        }

        public static StringBuilder setYearDay(StringBuilder text, double seconds)
        {
            int secondAgo = (int)seconds;
            int year = (secondAgo / (int)TIME.years().bitSeconds());
            secondAgo -= year * TIME.years().bitSeconds();
            int day = (secondAgo / (int)TIME.days().bitSeconds());
            text.Clear().AppendFormat(¤¤yearsDays);
            text.Insert(0, year);
            text.Insert(1, day);
            return text;
        }

        public static StringBuilder setYears(StringBuilder text, double years)
        {
            format(text, years, ¤¤Year, ¤¤Years);
            return text;
        }

        public static StringBuilder setYearsAgo(StringBuilder text, double years)
        {
            f.Clear();
            setYears(f, years);
            text.Clear().Append(¤¤1SomethingAgo);
            text.Insert(0, f);
            return text;
        }

        public static StringBuilder setDaysAgo(StringBuilder text, double days)
        {
            f.Clear();
            setDays(f, days);
            text.Clear().Append(¤¤1SomethingAgo);
            text.Insert(0, f);
            return text;
        }

        public static StringBuilder setDays(StringBuilder text, double days)
        {
            format(text, days, ¤¤Day, ¤¤Days);
            return text;
        }

        public static StringBuilder setHours(StringBuilder text, double hours)
        {
            format(text, hours, ¤¤Hour, ¤¤Hours);
            return text;
        }

        public static StringBuilder setMinutes(StringBuilder text, double minutes)
        {
            format(text, minutes, ¤¤Minute, ¤¤Minutes);
            return text;
        }

        public static StringBuilder setSeconds(StringBuilder text, double minutes)
        {
            format(text, minutes, ¤¤Second, ¤¤Seconds);
            return text;
        }

        private static void format(StringBuilder text, double days, string singular, string plural)
        {
            f2.Clear();
            if (days == 1)
            {
                text.AppendFormat(¤¤1OfSomething, singular);
            }
            else if (days == (int)days)
            {
                text.AppendFormat(¤¤MoreOfSomething, (int)days, plural);
            }
            else
            {
                text.AppendFormat(¤¤MoreOfSomething, days, plural);
            }
        }
    }
}