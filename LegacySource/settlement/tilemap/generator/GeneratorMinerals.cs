using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Tilemap.Generator
{
    internal class GeneratorMinerals
    {
        private readonly HeightMap height2;
        private readonly HeightMap height3;
        private readonly Polymap map;
        private readonly Poly[] pps;

        public GeneratorMinerals(CapitolArea area, GeneratorUtil util)
        {
            height2 = new HeightMap(TWIDTH, THEIGHT, 16, 4);
            height3 = new HeightMap(TWIDTH, THEIGHT, 16, 4);
            map = new Polymap(TWIDTH, THEIGHT);
            pps = new Poly[map.Polys()];

            Log(pps.Length);

            int valid = 0;
            foreach (var c in SETT.TILE_BOUNDS)
            {
                SETT.MINERALS().Getter.Set(c, null);
                int id = map.Getter.Get(c);
                if (pps[id] == null)
                {
                    pps[id] = new Poly();
                    valid++;
                }

                if (placable.Is(c))
                {
                    pps[id].X = c.X();
                    pps[id].Y = c.Y();
                }

                if (SETT.TILE_BOUNDS.IsOnEdge(c.X(), c.Y()) && pps[id].Valid)
                {
                    pps[id].Valid = false;
                    valid--;
                }

                foreach (var m in RESOURCES.Minables().All())
                {
                    pps[id].Occ[m.Index] += m.Terrain(TERRAINS.Sett.Get(c));
                }
            }

            if (area.IsBattle)
                return;

            Region reg = FACTIONS.Player().CapitolRegion();

            Log(valid);

            foreach (var m in RESOURCES.Minables().All())
            {
                double am = 0;

                foreach (var t in TERRAINS.ALL())
                {
                    am += m.Terrain(t) * reg.Info.Terrain(t);
                }

                am = (int)(38 + 2000 * am * m.Occurence);
                Log($"{m.Key()} {am}");

                Generate2(util, m, am, 1.0);
                Generate2(util, m, 1000, 0.0);
            }

            foreach (var m in RESOURCES.Minables().All())
            {
                Log($"{m.Resource.Name} {SETT.MINERALS().Totals.Get(m)}");
            }

            BlurEdges();
        }

        private void Generate2(GeneratorUtil util, Minable m, double size, double quality)
        {
            Array.Sort(pps, (o1, o2) =>
            {
                if (o1 == null)
                    return o2 == null ? 0 : -1;
                if (o2 == null)
                    return 1;
                double v = o2.Occ[m.Index] - o1.Occ[m.Index];
                return v > 0 ? 1 : (v < 0 ? -1 : 0);
            });

            double s = size;
            foreach (var p in pps)
            {
                if (p == null || !p.Valid())
                    continue;
                double rich = RND.rFloat1(0.1);
                double ai = size;
                ai /= rich;
                if (size > 50)
                {
                    ai = size * (0.5 + RND.rFloat() * 0.5);
                }
                ai = CLAMP.D(ai, 0, s);

                s -= Mineralize(util, p.X, p.Y, m, ai, quality);
                if (s <= 0)
                    return;
            }
        }

        private void Log(object o)
        {
            // LOG.ln(o);
        }

        private readonly MAP_BOOLEAN placable = new MAP_BOOLEAN()
        {
            Is = (tile) => !MINERALS().Getter.Is(tile) && !TERRAIN().WATER.DEEP.Is(tile),
            Is = (tx, ty) => IN_BOUNDS(tx, ty) && Is(tx + ty * TWIDTH)
        };

        private int Mineralize(GeneratorUtil util, int x, int y, Minable t, double size, double quality)
        {
            if (!placable.Is(x, y))
                return 0;

            Log($"making {t.Resource.Name} {size} {quality} ({x}:{y})");

            GUTIL.Flooder().Init(this);

            GUTIL.Flooder().PushSloppy(x, y, 0);
            GUTIL.Flooder().SetValue2(x, y, 0);
            double nor = height3.Get(x, y);
            double oldSize = size;
            double vv = 0;

            while (GUTIL.Flooder().HasMore())
            {
                PathTile c = GUTIL.Flooder().PollSmallest();
                if (pps[map.Get(c.X(), c.Y())] == null)
                    continue;
                pps[map.Get(c.X(), c.Y())].Valid = false;
                if (!placable.Is(c))
                {
                    continue;
                }
                vv = Math.Max(vv, c.GetValue());
                double dh = 0.5 + height2.Get(c) * 0.75;
                size -= 1;
                if (size < 0)
                    break;
                double dist = c.GetValue2();

                MINERALS().Getter.Set(c, t);
                MINERALS().AmountD.Set(c, dh);
                MINERALS().Value.Set(c, quality);

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    if (IN_BOUNDS(c, dir))
                    {
                        if (pps[map.Get(c.X() + dir.X(), c.Y() + dir.Y())] != null && (pps[map.Get(c.X(), c.Y())] == pps[map.Get(c.X() + dir.X(), c.Y() + dir.Y())] || pps[map.Get(c.X() + dir.X(), c.Y() + dir.Y())].Valid()))
                        {
                            double v = height3.Get(c, dir);
                            v = Math.Abs(v - nor);
                            double ddsist = dist + dir.TileDistance();
                            v += ddsist / 64.0;
                            if (GUTIL.Flooder().PushSmaller(c, dir, v) != null)
                            {
                                GUTIL.Flooder().SetValue2(c, dir, ddsist);
                            }
                        }
                    }
                }
            }

            GUTIL.Flooder().Done();

            return (int)(oldSize - size);
        }

        private void BlurEdges()
        {
            double e = 4.0;
            GUTIL.Flooder().Init(this);
            foreach (var c in SETT.TILE_BOUNDS)
            {
                if (SETT.MINERALS().AmountInt.Get(c) == 0)
                    GUTIL.Flooder().PushSloppy(c, 0);
            }

            while (GUTIL.Flooder().HasMore())
            {
                PathTile c = GUTIL.Flooder().PollSmallest();
                if (c.GetValue() > e)
                    break;

                if (SETT.MINERALS().AmountInt.Get(c) > 0)
                {
                    double am = (c.GetValue()) / (e);
                    SETT.MINERALS().AmountD.Set(c, SETT.MINERALS().AmountD.Get(c) * am);
                }
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    if (IN_BOUNDS(c, dir))
                    {
                        double v = c.GetValue() + dir.TileDistance();
                        GUTIL.Flooder().PushSmaller(c, dir, v);
                    }
                }
            }

            GUTIL.Flooder().Done();
        }

        private class Poly
        {
            public int X = -1;
            public int Y = -1;
            public double[] Occ;
            public bool Valid = true;

            public Poly()
            {
                Occ = new double[RESOURCES.Minables().All().Count()];
            }

            private bool Valid()
            {
                return Valid && X != -1;
            }
        }
    }
}