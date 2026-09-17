using System;
using settlement.main;
using snake2d.util.datatypes;

namespace settlement.tilemap.generator
{
    class GeneratorFertilityFin
    {
        static readonly double T_ROCK = 0.30;

        public GeneratorFertilityFin(CapitolArea area, GeneratorUtil util)
        {
            SETT.GROUND().setColors(area.climate().colorGroundDry, area.climate().colorGroundWet, 0);

            for (int y = 0; y < THEIGHT; y++)
            {
                for (int x = 0; x < TWIDTH; x++)
                {
                    if (SETT.TERRAIN().is(x, y))
                    {
                        SETT.TERRAIN().get(x, y).placeFixed(x, y);
                    }
                }
            }

            SETT.ENV().map.initWater();

            foreach (COORDINATE c in new Rec(SETT.TILE_BOUNDS))
            {
                int x = c.x();
                int y = c.y();

                double v = util.fer.get(c.x(), c.y());

                util.fer.set(c, v);
                v = util.fer.get(c.x(), c.y());

                SETT.GRASS().current.set(x, y, v);

                SETT.GRASS().grow(c.x(), c.y(), 16);
            }
        }
    }
}