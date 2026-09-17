using System;
using System.IO;

namespace Settlement.Weather
{
    public class RainEvent
    {
        private double downfall;
        private double time;
        private double thunder;
        private double cloud;
        private double timeToNext;

        private double droughtNext;
        private double droughtLength;

        public RainEvent()
        {
            Saver.Clear();
            IDebugPanelSett.Add("WEATHER DROUGHT", new Action()
            {
                public void Exe()
                {
                    droughtNext = 0;
                }
            });
        }

        public void Rain(double size)
        {
            downfall = Math.Clamp(size + RND.rFloat(), 0.1, 1);
            time = TIME.SecondsPerHour() * (0.5 + RND.rFloat() * 4);
            if (time * downfall < TIME.SecondsPerHour() * 0.7)
                time = 0.7 * TIME.SecondsPerHour() / downfall;
            cloud = 0.5 + RND.rFloat() * 0.5;
            thunder = 0;
            double wind = RND.rFloat();

            if (SETT.WEATHER().Temp.Heat() > 0 && RND.rBoolean())
            {
                wind += RND.rFloat();
                thunder = RND.rFloat();
                cloud += 0.5;
            }
            SETT.WEATHER().Wind.SetDayTarget(wind);
        }

        public void Update(double ds)
        {
            SWEATHER w = SETT.WEATHER();

            droughtNext -= ds * (SETT.ENV().Climate().TempCold + SETT.ENV().Climate().TempWarm) * 0.5;
            if (time > 0)
            {
                time -= ds;

                w.Rain.SetTarget(downfall);
                w.Clouds.SetTarget(cloud);
                w.Thunder.SetTarget(thunder);
            }
            else if (droughtNext < 0)
            {
                if (w.Moisture.GrowthValue() < 1)
                {
                    droughtLength -= ds;
                    if (droughtLength < 0)
                    {
                        SetNextDrought(1);
                        SetNextRain();
                        Rain(RND.rFloat());
                    }
                }
            }
            else
            {
                timeToNext -= ds;
                if (timeToNext < 0)
                {
                    SetNextRain();
                    Rain(RND.rFloat());
                }
            }
        }

        private void SetNextRain()
        {
            timeToNext = (0.25 + RND.rFloat() * 2.7) * TIME.SecondsPerDay();
        }

        private void SetNextDrought(int cooloffYears)
        {
            droughtNext = (cooloffYears + RND.rFloat() * 10) * TIME.SecondsPerDay() * TIME.Years().BitConversion(TIME.Days());
            droughtLength = (0.25 + 4 * RND.rFloat()) * TIME.SecondsPerDay();
        }

        public SAVABLE Saver = new SAVABLE()
        {
            public void Save(FilePutter file)
            {
                file.D(downfall);
                file.D(time);
                file.D(thunder);
                file.D(cloud);
                file.D(timeToNext);
                file.D(droughtNext);
                file.D(droughtLength);
            }

            public void Load(FileGetter file)
            {
                downfall = file.D();
                time = file.D();
                thunder = file.D();
                cloud = file.D();
                timeToNext = file.D();
                droughtNext = file.D();
                droughtLength = file.D();
            }

            public void Clear()
            {
                time = 0;
                SetNextRain();
                SetNextDrought(5);
                timeToNext /= 2;
            }
        };
    }
}