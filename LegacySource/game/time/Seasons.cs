using System;
using System.Collections.Generic;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using util.text;

namespace game.time
{
    public sealed class Seasons : TIMECYCLE
    {
        public readonly Season SPRING;
        public readonly Season SUMMER;
        public readonly Season AUTUMN;
        public readonly Season WINTER;
        public readonly LIST<Season> ALL;
        public readonly InterPolation currentDay = new InterPolation();
        public readonly InterPolation previousDay = new InterPolation();
        public readonly InterPolation nextDay = new InterPolation();

        public Seasons(double seconds, Json jData, Json jText) : base((int)seconds, 4, DicTime.¤¤Season, DicTime.¤¤Seasons)
        {
            SPRING = new Season(0, 0.25, "SPRING", jData, jText);
            SUMMER = new Season(1, 0, "SUMMER", jData, jText);
            AUTUMN = new Season(2, 0.25, "AUTUMN", jData, jText);
            WINTER = new Season(3, 1.0, "WINTER", jData, jText);
            ALL = new ArrayList<Season>(new List<Season> { SPRING, SUMMER, AUTUMN, WINTER });
        }

        public override string BitName(int bit)
        {
            return ALL.Get(bit).name;
        }

        public double WinterValue()
        {
            double d = (1.0 - BitPartOf());
            double dd = 1.0 - d;
            d = current().winterValue * d + next().winterValue * dd;
            return d;
        }

        public Season current()
        {
            return ALL.Get(BitCurrent());
        }

        public Season next()
        {
            return ALL.Get((BitCurrent() + 1) % ALL.size());
        }

        public Season next(int i)
        {
            i += BitCurrent();
            i &= 3;
            return ALL.Get(i);
        }

        public sealed class Season
        {
            public readonly string name;
            private readonly int index;
            readonly double dayNightRatio;
            public readonly double red;
            public readonly double green;
            public readonly double blue;
            public readonly double winterValue;

            public Season(int index, double winterValue, string key, Json data, Json text)
            {
                name = text.text(key);
                data = data.json(key);
                dayNightRatio = data.d("NIGHTRATIO", 0.1, 0.9);
                red = data.d("RED", 0, 10);
                green = data.d("GREEN", 0, 10);
                blue = data.d("BLUE", 0, 10);
                this.index = index;
                this.winterValue = winterValue;
            }

            public int index()
            {
                return index;
            }
        }

        public Season GetWithOffset(double seconds)
        {
            int s = (int)(SecondOfBit() + seconds);
            if (s < 0)
            {
                s += CycleSeconds() * (int)Math.Ceiling(-s / CycleSeconds());
            }
            s /= BitSeconds();
            s %= ALL.size();
            return ALL.Get(s);
        }

        protected override void Update(double currentSecond)
        {
            base.Update(currentSecond);
            currentDay.update(0);
            previousDay.update(-TIME.secondsPerDay());
            nextDay.update(TIME.secondsPerDay());
        }

        public class InterPolation : RGB
        {
            private double red;
            private double green;
            private double blue;
            private double dayLength;
            private double winterValue;
            private int dayCurrent = -1;

            private InterPolation()
            {
            }

            private void update(int offSeconds)
            {
                double secSeasons = TIME.seasons().BitSeconds();
                int seasons = TIME.seasons().ALL.size();
                double s = TIME.years().BitSeconds() + TIME.currentSecond() + offSeconds;
                double d = TIME.secondsPerDay();

                double dPrev = s - d;
                double dNext = s + d;

                int prevI = (int)(dPrev / secSeasons);
                int nextI = (int)(dNext / secSeasons);
                int currentI = (int)(s / secSeasons);

                if (prevI != nextI)
                {
                    if (prevI != currentI)
                    {
                        dPrev = currentI * secSeasons - dPrev;
                        dNext = d + d - dPrev;
                    }
                    else if (nextI != currentI)
                    {
                        dNext = dNext - nextI * secSeasons;
                        dPrev = d + d - dNext;
                    }
                }
                else
                {
                    dPrev = d;
                    dNext = d;
                }

                dPrev /= d * 2.0;
                dNext /= d * 2.0;

                prevI %= seasons;
                nextI %= seasons;
                currentI %= seasons;
                Season prev = TIME.seasons().ALL.Get(prevI);
                Season next = TIME.seasons().ALL.Get(nextI);

                red = dPrev * prev.red + dNext * next.red;
                green = dPrev * prev.green + dNext * next.green;
                blue = dPrev * prev.blue + dNext * next.blue;
                winterValue = dPrev * prev.winterValue + dNext * next.winterValue;

                if (dayCurrent != TIME.days().BitsSinceStart())
                {
                    dayLength = 1.0 - (dPrev * prev.dayNightRatio + dNext * next.dayNightRatio);
                    dayCurrent = TIME.days().BitsSinceStart();
                }
            }

            public double DayLength()
            {
                return dayLength;
            }

            public double WinterValue()
            {
                return winterValue;
            }

            public double r()
            {
                return red;
            }

            public double g()
            {
                return green;
            }

            public double b()
            {
                return blue;
            }
        }
    }
}