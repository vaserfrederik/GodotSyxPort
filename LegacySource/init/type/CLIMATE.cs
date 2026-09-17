using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Type
{
    public sealed class CLIMATE : INFO, MAPPED
    {
        public readonly string key;
        private readonly int index;
        public readonly double seasonChange;
        public readonly double tempCold;
        public readonly double tempWarm;
        public readonly double fertility;
        public readonly COLOR color;
        public readonly COLOR colorGroundDry;
        public readonly COLOR colorGroundWet;
        public readonly SPRITE icon;
        public readonly BoostSpecs boosters;

        public CLIMATE(List<CLIMATE> all, string key, string name, string desc, Json json) : base(name, desc)
        {
            index = all.Add(this);
            this.key = key;
            json = json.Json(key);
            this.seasonChange = json.d("SEASONAL_CHANGE", 0, 1);
            double t = json.d("TEMP_COLD", -1, 1);
            icon = UI.icons().Get(json);
            t = t < 0 ? 1.0 + t : 1 + t;
            tempCold = t / 2.0;
            t = json.d("TEMP_WARM", -1, 1);
            t = t < 0 ? 1.0 + t : 1 + t;
            tempWarm = t / 2.0;
            color = new ColorImp(json);
            boosters = new BoostSpecs($"{CLIMATES.INFO().Name}: {name}", icon, false);
            boosters.Read(json, null);
            fertility = json.d("FERTILITY");
            json = json.Json("GROUND");
            colorGroundDry = new ColorImp(json, "DRY");
            colorGroundWet = new ColorImp(json, "WET");
        }

        public int Index()
        {
            return index;
        }

        public double GetPartOfYear()
        {
            int pow = (int)(seasonChange * 2);
            if (pow == 0)
                return 0.5;
            double d = (TIME.Years().BitPartOf() + 0.125) % 1.0;
            if (d < 0.5)
            {
                d *= 2;
                d = Math.Pow(d, pow);
                return d * 0.5;
            }
            else
            {
                d -= 0.5;
                d *= 2;
                d = 1.0 - d;
                d = Math.Pow(d, pow);
                d = 1.0 - d;
                return 0.5 + d * 0.5;
            }
        }

        public string Key()
        {
            return key;
        }
    }
}