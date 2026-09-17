using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;

namespace settlement.tilemap.generator
{
    class GeneratorFertilityInit
    {
        private readonly double aI = 1.0 / SettlementGrid.QUAD_AREA;

        public GeneratorFertilityInit(CapitolArea area, GeneratorUtil util)
        {
            HeightMap ferMap = new HeightMap(SETT.TWIDTH, SETT.THEIGHT, 32, 2);

            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                double baseValue = GetBase(area, c);
                double f = ferMap.Get(c);
                double h = 1.0 - util.height.Get(c);
                double res = Get(baseValue, f, h);
                util.fer.Set(c, res);
            }
        }

        private double GetBase(CapitolArea area, COORDINATE c)
        {
            double v = 0;
            int wx = area.Tiles().x1() + c.x() / SettlementGrid.QUAD_SIZE;
            int wy = area.Tiles().y1() + c.y() / SettlementGrid.QUAD_SIZE;

            int dx = (c.x() % SettlementGrid.QUAD_SIZE) - SettlementGrid.QUAD_HALF;
            int dy = (c.y() % SettlementGrid.QUAD_SIZE) - SettlementGrid.QUAD_HALF;

            double ax = Math.Abs(dx) * (SettlementGrid.QUAD_SIZE - Math.Abs(dy));
            double ay = Math.Abs(dy) * (SettlementGrid.QUAD_SIZE - Math.Abs(dx));
            double axy = Math.Abs(dx) * Math.Abs(dy);
            double a = SettlementGrid.QUAD_AREA - ax - ay - axy;

            v += WORLD.GROUND().Moisture.Get(wx, wy) * a;

            foreach (DIR d in DIR.ALL)
            {
                if (d.x() * dx < 0)
                    continue;
                if (d.y() * dy < 0)
                    continue;

                if (d.x() != 0 && d.y() != 0)
                {
                    v += WORLD.MOISTURE().Get(wx + d.x(), wy + d.y()) * axy;
                }
                else if (d.x() != 0)
                {
                    v += WORLD.MOISTURE().Get(wx + d.x(), wy) * ax;
                }
                else if (d.y() != 0)
                {
                    v += WORLD.MOISTURE().Get(wx, wy + d.y()) * ay;
                }
            }

            return v * aI;
        }

        public static double Get(double baseValue, double fe, double hi)
        {
            baseValue = CLAMP.D(baseValue, 0, 1) * 0.7;

            double f = hi;
            f = Math.Pow(f, 1 + 8 * (1 - baseValue));
            f = CLAMP.D(baseValue + f, 0, 1);
            f -= 0.2 * fe;

            return CLAMP.D(f, 0, 1);
        }
    }
}