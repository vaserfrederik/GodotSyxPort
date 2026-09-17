using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util;

namespace settlement.tilemap.generator
{
    class GeneratorWaterFin
    {
        private CapitolArea area;
        private GeneratorUtil util;

        public GeneratorWaterFin(CapitolArea area, GeneratorUtil util)
        {
            this.area = area;
            this.util = util;
            AddDepth(area, util);
        }

        private void AddDepth(CapitolArea area, GeneratorUtil util)
        {
            GUTIL.Flooder().Init(this);

            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                if (!TERRAIN().WATER.SHALLOW.Is(c))
                {
                    GUTIL.Flooder().PushSloppy(c, 0);
                    GUTIL.Flooder().SetValue2(c, 1 + RND.rExpo() * 50);
                }
            }

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                double v2 = t.GetValue();

                if (TERRAIN().WATER.SHALLOW.Is(t))
                {
                    if (v2 < 4)
                    {
                        TERRAIN().NADA.PlaceRaw(t.x(), t.y());
                    }
                    else
                    {
                        double v = v2 * (Math.Pow(CLAMP.d(util.height.Get(t), 0, 1), 1.7));
                        if (v > 4)
                            TERRAIN().WATER.DEEP.PlaceRaw(t.x(), t.y());
                    }
                }

                foreach (DIR d in DIR.ALL)
                {
                    int x = t.x() + d.x();
                    int y = t.y() + d.y();
                    if (IN_BOUNDS(x, y))
                    {
                        double v = v2;
                        if (v2 >= 4)
                        {
                            v += d.TileDistance();
                        }
                        else
                        {
                            v += d.TileDistance() * t.GetValue2();
                        }
                        if (v > 0)
                        {
                            if (GUTIL.Flooder().PushSmaller(x, y, v) != null)
                            {
                                GUTIL.Flooder().SetValue2(x, y, t.GetValue2());
                            }
                        }
                    }
                }
            }

            GUTIL.Flooder().Done();

            SETT.TERRAIN().WATER.GroundWater.Clear();
            GUTIL.Flooder().Init(this);
            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                if (TERRAIN().WATER.SHALLOW.Is(c))
                {
                    GUTIL.Flooder().PushSloppy(c, 0);
                }
            }
            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                if (t.GetValue() > 8)
                    break;
                SETT.TERRAIN().WATER.GroundWater.Set(t, true);
                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.IN_BOUNDS(t, d))
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance());
                }
            }

            GUTIL.Flooder().Done();
        }
    }
}