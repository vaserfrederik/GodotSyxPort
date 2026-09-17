using System;
using System.Collections.Generic;
using static Settlement.Main.SETT;
using static Settlement.Main.SETT.TERRAIN;
using static Settlement.Main.SETT.THEIGHT;
using static Settlement.Main.SETT.TWIDTH;

namespace Settlement.Tilemap.Generator
{
    public class GeneratorGround
    {
        public GeneratorGround(CapitolArea area, GeneratorUtil util)
        {
            Forest(area, util);

            var h = new HeightMap(TWIDTH, THEIGHT, 128, 4);

            double v = 0;
            int am = 0;
            foreach (var c in area.Tiles())
            {
                v += WORLD.GROUND().getter.get(c).Moisture();
                am++;
            }
            v /= am;
            SETT.GROUND().baseMoisture.setD(v);

            v = 0.5;

            GUTIL.flooder().init(this);

            GroundType worst = area.Climate() == CLIMATES.HOT() ? SETT.GROUND().types.SAND : SETT.GROUND().types.INFERTILE;

            foreach (var c in SETT.TILE_BOUNDS)
            {
                if (SETT.GROUND().types.NORMAL.is(c))
                {
                    if (h.get(c) < v / 2)
                    {
                        worst.placeFixed(c.x(), c.y());
                    }
                    else if (h.get(c) < v)
                    {
                        SETT.GROUND().types.PASTURE.placeFixed(c.x(), c.y());
                    }
                }
                if (worst.is(c))
                {
                    GUTIL.flooder().pushSmaller(c, 0);
                }

            }

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                double vv = (4.0 - t.getValue()) / 6.0;
                if (vv < 0)
                    continue;
                util.fer.increment(t, -vv);
                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + d.tileDistance());
                    }
                }
            }

            GUTIL.flooder().done();

            foreach (var c in new Rec(SETT.TILE_BOUNDS))
            {
                double vv = util.fer.get(c.x(), c.y());
                SETT.GROUND().MOISTURE_BASE.set(c, vv);

            }

            SETT.GROUND().init();

        }

        private void Forest(CapitolArea area, GeneratorUtil util)
        {
            double value = util.json.d("FOREST_AMOUNT", 0, 1);

            var ma = new HeightMap(SETT.TWIDTH, SETT.THEIGHT, 32, 4);

            if (area.isBattle)
            {
                value *= 0.75;
            }

            util.polly.checkInit();

            GUTIL.flooder().init(this);

            for (int i = 0; i < GRID.tiles().size(); i++)
            {

                SettlementGrid.Tile ut = GRID.tile(i);
                double wf = WORLD.FOREST().amount.get(area.ts().get(i));
                int a = (int)Math.Ceiling(value * wf * 10);
                while (a-- > 0)
                {
                    int sx = ut.coo(DIR.W).x() + RND.rInt(SettlementGrid.QUAD_SIZE);
                    int sy = ut.coo(DIR.N).y() + RND.rInt(SettlementGrid.QUAD_SIZE);

                    int mm = (int)(5 + value * RND.rInt(10));
                    while (mm-- > 0)
                    {
                        int x = sx + RND.rInt0(40);
                        x = CLAMP.i(x, 0, SETT.TWIDTH);
                        int y = sy + RND.rInt0(40);
                        y = CLAMP.i(y, 0, SETT.THEIGHT);
                        util.polly.checker.set(x, y, true);
                        GUTIL.flooder().pushSloppy(x, y, 0);
                    }

                }

            }

            while (GUTIL.flooder().hasMore())
            {

                PathTile t = GUTIL.flooder().pollSmallest();
                if (!TERRAIN().NADA.is(t))
                    continue;

                SETT.GROUND().types.FOREST.placeFixed(t.x(), t.y());

                double v = t.getValue();

                if (util.polly.checker.is(t))
                {
                    v = 0;
                }

                if (v / 40.0 + ma.get(t) > 1)
                {
                    continue;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.IN_BOUNDS(t, d))
                        GUTIL.flooder().pushSmaller(t, d, v + d.tileDistance());
                }

            }

            GUTIL.flooder().done();
        }
    }
}