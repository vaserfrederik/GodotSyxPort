using System;
using init;
using init.paths;
using snake2d;
using snake2d.util.file;

namespace init.constant
{
    public sealed class Config : InitResource
    {
        static Config()
        {
            if (!PATHS.inited())
            {
                throw new RuntimeException("paths must be inited first!");
            }
        }

        private static Json j = null;
        private static ConfigBattle BATTLE;
        private static ConfigSett SETT;
        private static ConfigWorld WORLD;

        public Config(INIT init) : base(init)
        {
            if (!PATHS.inited())
            {
                throw new RuntimeException("paths must be inited first!");
            }
            j = new Json(PATHS.CONFIG().init.gets("Battle"));
            BATTLE = new ConfigBattle();
            j = new Json(PATHS.CONFIG().init.gets("Sett"));
            SETT = new ConfigSett();
            j = new Json(PATHS.WORLD().folder("config").init.gets("General"));
            WORLD = new ConfigWorld();
        }

        public static ConfigBattle battle()
        {
            return BATTLE;
        }

        public static ConfigSett sett()
        {
            return SETT;
        }

        public static ConfigWorld world()
        {
            return WORLD;
        }

        public static sealed class ConfigBattle
        {
            public readonly double MORALE_HOLDOUT = j.d("MORALE_HOLDOUT", 0, 10000);
            public readonly int TRAINING_DEGRADE = j.i("TRAINING_DEGRADE", 0, 50);
            public readonly int MEN_PER_DIVISION = j.i("MEN_PER_DIVISION", 1, 255);
            public readonly int DIVISIONS_PER_ARMY = j.i("DIVISIONS_PER_ARMY", 1, 126);
            public readonly int DIVISIONS_PER_BATTLE = DIVISIONS_PER_ARMY * 2;
            public readonly int MEN_PER_ARMY = MEN_PER_DIVISION * DIVISIONS_PER_ARMY;
            public readonly int REGION_MAX_DIVS = j.i("REGION_MAX_DIVS", 0, 127);
            public readonly int REGION_MAX_MEN = REGION_MAX_DIVS * MEN_PER_DIVISION;
            public readonly double DAMAGE_REDUCTION = j.d("DAMAGE_REDUCTION", 1, 10000);

            private ConfigBattle()
            {
            }
        }

        public static sealed class ConfigSett
        {
            public readonly double HAPPINESS_EXPONENT = j.d("HAPPINESS_EXPONENT");
            public readonly int TOURIST_PER_YEAR_MAX = j.i("TOURIST_PER_YEAR_MAX");
            public readonly double TOURIST_CRETIDS = j.d("TOURIST_CRETIDS");
            public readonly int DIMENSION = j.i("DIMENSION", 256, 16000);

            public readonly double POP_RAIDER_WORTH = j.i("POP_RAIDER_WORTH", 1, 10000);
            public readonly int secondsPerHour = j.i("SECONDS_PER_HOUR");
            public readonly int hoursPerDay = j.i("HOURS_PER_DAY");

            private ConfigSett()
            {
                if (DIMENSION % 64 != 0)
                    throw new Errors.DataError("SETT DIMENSION MUST BE A MULTIPLE OF 64");
            }
        }

        public static sealed class ConfigWorld
        {
            public readonly int POPULATION_CAPACITY_MAX = j.i("POPULATION_CAPACITY_MAX", 1, 100000);
            public readonly int WORLD_SIZE = j.i("TILE_DIMENSION", 128, 512);
            public readonly double FOREST_AMOUNT = j.d("FOREST_AMOUNT", 0, 1);
            public readonly double REGION_SIZE = j.i("REGION_SIZE", 0, 1000);

            public static Json json(string resource)
            {
                return new Json(PATHS.WORLD().folder("config").init.gets(resource));
            }
        }
    }
}