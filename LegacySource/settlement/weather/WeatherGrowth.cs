using System;
using System.IO;

namespace Settlement.Weather
{
    public sealed class WeatherGrowth : WeatherThing
    {
        private static readonly string ¤¤name = "Growth";
        private static readonly string ¤¤desc = "The growth of plants.";
        private static readonly double speed = 1.0 / (2 * TIME.SecondsPerDay());
        private bool isAutumn;

        static WeatherGrowth()
        {
            D.ts(typeof(WeatherGrowth));
        }

        public WeatherGrowth() : base(¤¤name, ¤¤desc)
        {
        }

        // public bool CropsAreRipe()
        // {
        //     return GetD() == 1;
        // }

        protected override void Update(double ds)
        {
            double g = GetD();
            isAutumn = TIME.Years().BitPartOf() > 0.5;
            if (!isAutumn)
            {
                if (SETT.WEATHER().Temp.Heat() > 0)
                    g += ds * speed * 2;
            }
            else
            {
                if (SETT.WEATHER().Temp.Heat() < 0.25)
                    g -= ds * speed;
            }

            SetD(g);

            base.Update(ds);
        }

        public bool IsAutumn()
        {
            return isAutumn;
        }

        protected override void Init()
        {
            Update(0);
            double d = 0;
            double p = TIME.Years().BitPartOf();
            double tmp1 = SETT.WEATHER().Temp.Average(p - 0.2);
            double tmp2 = SETT.WEATHER().Temp.Average(p);
            if (tmp1 > 0.5 && tmp2 > 0.5)
            {
                d = 1;
            }
            else if (tmp1 > 0.5)
            {
                d = 0.5;
            }
            else
                d = 0;
            SetD(d);
        }

        protected override void Save(BinaryWriter file)
        {
            base.Save(file);
            file.Write(isAutumn);
        }

        protected override void Load(BinaryReader file)
        {
            base.Load(file);
            isAutumn = file.ReadBoolean();
        }
    }
}