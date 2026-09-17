using System;
using System.IO;
using game.events.EVENTS;
using game.time;
using settlement.main;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.disaster
{
    public sealed class EventTemperature : EventResource
    {
        private static readonly string ¤¤ExtremeTemp = "Extreme Temperatures";
        private static readonly string ¤¤ExtremeTempHot = "The temperature is rising to an extreme level today. Make sure you have ways for subjects to cool down!";
        private static readonly string ¤¤ExtremeTempCold = "The temperature has plunged to record lows. Make sure our hearths are stocked with wood.";

        static EventTemperature()
        {
            D.ts(typeof(EventTemperature));
        }

        private int dayLast = -1;

        public EventTemperature() : base("TEMP")
        {
            IDebugPanelSett.add("Event: Temp", new ACTION()
            {
                public void exe()
                {
                    eventMethod(0.20, SETT.WEATHER().temp.average(TIME.years().bitPartOf()));
                }
            });
        }

        protected override void update(double ds)
        {
            if (dayLast != TIME.days().bitsSinceStart())
            {
                dayLast = TIME.days().bitsSinceStart();
                if (TIME.days().bitsSinceStart() > 6)
                {
                    double ave = SETT.WEATHER().temp.average(TIME.years().bitPartOf());
                    double ran = RND.rFloat();
                    for (int i = 0; i < 5; i++)
                    {
                        ran *= ran;
                    }
                    ran *= 0.25;

                    if (ran > 0.15)
                    {
                        eventMethod(ran, ave);
                    }
                }
            }
        }

        private void eventMethod(double ran, double ave)
        {
            if (ave < 0.5)
            {
                SETT.WEATHER().temp.setTarget(CLAMP.d(ave - ran, 0, 1));
                new MessageText(¤¤ExtremeTemp, ¤¤ExtremeTempCold).send();
            }
            else
            {
                SETT.WEATHER().temp.setTarget(CLAMP.d(ave + ran, 0, 1));
                new MessageText(¤¤ExtremeTemp, ¤¤ExtremeTempHot).send();
            }
        }

        protected override void save(FilePutter file)
        {
            file.i(dayLast);
        }

        protected override void load(FileGetter file)
        {
            dayLast = file.i();
        }

        protected override void clear()
        {
            dayLast = -1;
        }
    }
}