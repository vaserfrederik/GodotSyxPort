using System;
using System.Collections.Generic;
using static SettlementMain.SETT;

namespace Settlement.Tilemap.Generator
{
    class GeneratorGrowth
    {
        private readonly GG[] map = new GG[128];

        public GeneratorGrowth()
        {
            HeightMap ferMap = new HeightMap(TWIDTH, THEIGHT, 8, 2);

            Add(SETT.TILE_MAP().growth.bush, 0, 0.1);
            Add(SETT.TILE_MAP().growth.tree, 0.1, 0.25);
            Add(SETT.TILE_MAP().growth.bush, 0.25, 0.35);
            Fill(ferMap, SETT.GROUND().types.FOREST, 0.175, 1.0);

            foreach (var c in SETT.TILE_BOUNDS)
            {
                double f = ferMap.Get(c);
                int i = (int)(f * (map.Length - 1));
                GG g = map[i];
                if (g != null)
                {
                    double a = (f - g.from) / (g.to - g.from);
                    g.g.Set(c.x(), c.y(), a);
                }
            }

            ferMap = new HeightMap(TWIDTH, THEIGHT, 32, 2);
            Fill(ferMap, SETT.GROUND().types.NORMAL, 1.0, 0.2);
            foreach (var c in SETT.TILE_BOUNDS)
            {
                double f = ferMap.Get(c);

                if (f > 0.65)
                {
                    double a = (f - 0.65) / 0.2;
                    a = CLAMP.d(a, 0, 1);
                    Grower g = SETT.TILE_MAP().growth.flower;
                    g.Set(c.x(), c.y(), a);
                }
            }

            ferMap = new HeightMap(TWIDTH, THEIGHT, 16, 1);
            Fill(ferMap, SETT.GROUND().types.PASTURE, 1.0, 0.2);
            foreach (var c in SETT.TILE_BOUNDS)
            {
                double f = ferMap.Get(c);

                if (f > 0.7)
                {
                    double a = (f - 0.7) / 0.2;
                    a = CLAMP.d(a, 0, 1);
                    Grower g = SETT.TILE_MAP().growth.bush;
                    g.Set(c.x(), c.y(), a);

                }
                else
                {
                    f = ferMap.Get((c.x() + 64) % SETT.TWIDTH, (c.y() + 64) % SETT.THEIGHT);
                    if (f > 0.8)
                    {
                        double a = (f - 0.8) / 0.15;
                        a = CLAMP.d(a, 0, 1);
                        Grower g = SETT.TILE_MAP().growth.mushroom;
                        g.Set(c.x(), c.y(), a);
                    }
                }
            }
        }

        void Add(Grower[] gs, Grower g, double amount)
        {
            int am = (int)Math.Ceiling(gs.Length * amount);
            for (int i = 0; i < gs.Length; i++)
            {
                if (gs[i] == null)
                {
                    gs[i] = g;
                    am--;
                    if (am <= 0)
                        return;
                }
            }
        }

        private void Fill(HeightMap ferMap, GroundType type, double to, double strength)
        {
            GUTIL.Flooder().Init(this);
            foreach (var c in SETT.TILE_BOUNDS)
            {
                if (type.Is(c))
                {
                    GUTIL.Flooder().PushSloppy(c, 0);
                }
            }

            while (GUTIL.Flooder().HasMore())
            {
                PathTile t = GUTIL.Flooder().PollSmallest();

                double v = 1.0 - t.GetValue() / 10.0;
                if (v < 0)
                    break;

                double inc = to - ferMap.Get(t);
                inc *= strength * v;
                ferMap.Increment(t, inc);

                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.IN_BOUNDS(t, d))
                    {
                        GUTIL.Flooder().PushSloppy(t, d, t.GetValue() + d.TileDistance());
                    }
                }
            }
            GUTIL.Flooder().Done();
        }

        private void Add(Grower g, double from, double to)
        {
            GG gg = new GG(g, from, to);
            int i = (int)(from * (map.Length - 1));
            int t = (int)(to * (map.Length - 1));
            for (; i < t; i++)
            {
                map[i] = gg;
            }
        }


        private class GG
        {
            public readonly Grower g;
            public readonly double from;
            public readonly double to;

            public GG(Grower g, double from, double to)
            {
                this.g = g;
                this.from = from;
                this.to = to;
            }
        }
    }
}