using System;
using System.IO;

namespace Settlement.Weather
{
    public sealed class WeatherGrowthRipe : WeatherThing
    {
        private static readonly double ripeStart = 1 / 8.0;
        private static readonly double ripeEnd = 5 / 8.0;
        private double d = 0;

        public WeatherGrowthRipe() : base("Ripness", "")
        {
        }

        private bool ripening;
        private bool ripe;

        public bool CropsAreRipe()
        {
            return ripe;
        }

        public override void Update(double ds)
        {
            ripening = false;
            double part = TIME.Years().BitPartOf();
            if (part > ripeEnd)
            {
                d = Math.Clamp(1.0 - (part - ripeEnd) * 4.0, 0, 1);
            }
            else if (part > ripeStart)
            {
                ripening = true;
                d = Math.Clamp((part - ripeStart) * 4.0, 0, 1);
            }
            else
            {
                d = 0;
            }

            ripening |= GetD() > 0;
            ripe = GetD() == 1 && SETT.WEATHER().Moisture.GetD() > 0.25;
            SetD(d);
            base.Update(ds);
        }

        public bool Ripe()
        {
            return ripening;
        }

        protected override void Init()
        {
            Update(0);
        }

        protected override void Save(BinaryWriter file)
        {
            base.Save(file);
        }

        protected override void Load(BinaryReader file)
        {
            base.Load(file);
            Update(0);
        }
    }
}