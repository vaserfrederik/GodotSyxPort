using System;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace world.map.buildings
{
    final class WorldGeneratorBuildings
    {
        public WorldGeneratorBuildings()
        {
            final HeightMap map = new HeightMap(WORLD.TWIDTH(), WORLD.THEIGHT(), 16, 4);

            final double max = 4 + 2.8;

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                WORLD.BUILDINGS().village.set(c, false);

                if (WORLD.REGIONS().isCentre.is(c))
                    continue;
                if (WORLD.WATER().coversTile.is(c))
                    continue;

                double connected = 0;
                double freshWater = 0;

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.ROADS().is(c, d))
                        connected += 1.0 / d.tileDistance();
                    if (WORLD.WATER().fertile.is(c, d))
                        freshWater += 1.0 / d.tileDistance();
                }

                connected /= max;
                connected = Math.Pow(connected, 0.5);

                freshWater /= max;
                freshWater = Math.Pow(freshWater, 0.5);

                if (farm(c, connected, freshWater))
                {
                    WORLD.BUILDINGS().village.set(c.x(), c.y(), true);
                }
                else if (village(c, connected, map))
                {
                    WORLD.BUILDINGS().village.set(c.x(), c.y(), true);
                }
            }
        }

        public void clear()
        {
            WORLD.BUILDINGS().saver().clear();
        }

        public bool village(COORDINATE c, double connectivity, HeightMap map)
        {
            if (WORLD.MOUNTAIN().coversTile(c.x(), c.y()))
                return false;

            double mul = 0;

            double chance = 0.1 + 0.9 * connectivity;
            chance *= 0.25 + 2.5 * (WORLD.GROUND().getter.get(c).moisture() - 0.25) / 0.75;
            chance *= map.get(c);

            chance += mul;

            return RND.rFloat() < chance;
        }

        public bool farm(COORDINATE c, double connectivity, double freshWater)
        {
            if (WORLD.MOUNTAIN().coversTile(c.x(), c.y()))
                return false;

            double ch = 1.0 * (WORLD.GROUND().getter.get(c).moisture() - 0.25) / 0.75;
            ch *= 0.1 + 0.9 * (1.0 - WORLD.CLIMATE().getter.get(c).seasonChange);
            ch += freshWater * 0.5 + freshWater * RND.rFloat();

            if (Math.Pow(RND.rFloat(), 2) < ch)
                return true;

            return false;
        }
    }
}