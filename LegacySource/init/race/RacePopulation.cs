using System;
using snake2d.util.file;

namespace init.race
{
    public sealed class RacePopulation
    {
        public readonly double growth;
        public readonly double max;
        private readonly double[] climates;
        private readonly double maxClimate;
        private readonly double[] terrains;
        private readonly double maxTerrain;

        public RacePopulation(Json json)
        {
            if (!json.Has("POPULATION"))
            {
                max = 1;
                climates = new double[CLIMATES.ALL().Count()];
                terrains = new double[TERRAINS.ALL().Count()];
                maxClimate = 0;
                maxTerrain = 0;
                growth = 0.0001;
            }
            else
            {
                json = json.Json("POPULATION");
                max = json.D("MAX", 0, 1);
                climates = CLIMATES.MAP().ReadFill(json, 1);
                double m = 0;
                foreach (double c in climates)
                    m = Math.Max(c, m);
                maxClimate = m;
                terrains = TERRAINS.MAP().ReadFill(json, 100);
                m = 0;
                foreach (double c in terrains)
                    m = Math.Max(c, m);
                maxTerrain = m;
                growth = json.D("GROWTH", 0.0001, 1);
            }
        }

        public double Climate(CLIMATE c)
        {
            return climates[c.Index()];
        }

        public double Terrain(TERRAIN c)
        {
            return terrains[c.Index()];
        }

        public double MaxClimate()
        {
            return maxClimate;
        }

        public double MaxTerrain()
        {
            return maxTerrain;
        }
    }
}