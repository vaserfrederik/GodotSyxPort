using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using settlement.main;
using game.faction;
using game.time;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.info;
using util.text;
using view.main;
using view.ui.message;

namespace settlement.weather
{
    public sealed class WeatherMoisture : WeatherThing
    {
        private static readonly CharSequence ¤¤name = "Precipitation";
        private static readonly CharSequence ¤¤desc = "How often it has rained. Lack of downfall will decline Precipitation. Values below 25% will cause a drought that can be detrimental to growth.";

        private static readonly CharSequence ¤¤drought = "¤Drought";
        private static readonly CharSequence ¤¤droughtD = "¤Lack of rain has caused a drought. Our irrigation is working, but it's not enough. All our rooms dependent on water will be affected";
        private static readonly INFO binfo = new INFO(¤¤drought, ¤¤droughtD);
        private static readonly CharSequence ¤¤mTitle = "¤Drought!";
        private static readonly CharSequence ¤¤mBody = "¤The gods have forsaken {0}, and the rains have stopped. If this keeps up, it will devastate our crops! Everyone must now pray.";

        private static readonly double rainspeed = 2.0 / (TIME.secondsPerHour());
        private static readonly double dry = 1.0 / (8 * TIME.secondsPerDay());
        private double lastSnow = 0;
        private double sendTimer;

        static
        {
            D.ts(typeof(WeatherMoisture));
        }

        public WeatherMoisture() : base(¤¤name, ¤¤desc)
        {
        }

        public override void update(double ds)
        {
            double d = getD();
            if (!SETT.WEATHER().snow.rainIsSnow())
            {
                d += ds * rainspeed * SETT.WEATHER().rain.getD();
            }

            double snow = SETT.WEATHER().snow.getD();
            double thawed = lastSnow - snow;
            lastSnow = snow;

            if (thawed > 0)
                d += thawed;
            lastSnow = SETT.WEATHER().snow.getD();

            if (SETT.WEATHER().temp.heat() > 0)
                d -= dry * ds;

            sendTimer -= ds;

            setD(d);
        }

        public static RoomBoost makeBoost()
        {
            return new RoomBoost
            {
                info = () => binfo,
                get = r => CLAMP.d(SETT.WEATHER().moisture.growthValue(), 0, 1)
            };
        }

        public override DOUBLE_MUTABLE setD(double d)
        {
            if (d < 0.25 && getD() >= 0.25)
            {
                if (sendTimer < 0 && !VIEW.b().isActive())
                {
                    Str.TMP.clear().add(¤¤mBody).insert(0, FACTIONS.player().name);
                    new MessageText(¤¤mTitle).paragraph(Str.TMP).send();
                    sendTimer = 10;
                }
            }
            return base.setD(d);
        }

        public double growthValue()
        {
            return CLAMP.d(getD() * 4.0, 0, 1);
        }

        protected override void init()
        {
            setD(0.75);
        }
    }
}