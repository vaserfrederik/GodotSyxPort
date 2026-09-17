using System;
using System.Collections.Generic;
using Util.DataTypes;
using Util.File;
using Util.Map;
using Util.Misc;
using Util.Sets;
using Util.Sprite;
using Util.Info;
using Util.Keymap;
using Util.Text;
using World;

namespace Init.Type
{
    public static class TERRAINS
    {
        private static TERRAINS self;

        private readonly TERRAIN OCEAN;
        private readonly TERRAIN WET;
        private readonly TERRAIN MOUNTAIN;
        private readonly TERRAIN FOREST;
        private readonly TERRAIN NONE;
        private readonly INFO info;
        private readonly LIST<TERRAIN> all;
        private readonly RMAP<TERRAIN> map;

        private TERRAINS()
        {
            self = this;
            D.t(typeof(TERRAINS));
            ArrayList<TERRAIN> ts = new ArrayList<TERRAIN>(40);
            string key = "TERRAIN";
            Json json = new Json(PATHS.CONFIG().init.gets(key));
            info = new INFO(D.g("Terrain"), "");

            OCEAN = new TERRAIN(ts, "OCEAN", json,
                    D.g("Ocean"),
                    D.g("OceanD", "Salt water such as oceans. Fish is plentiful."), true)
            {
                public override SPRITE icon()
                {
                    return WORLD.WATER().OCEAN.icon;
                }

                public override double value(int wx, int wy)
                {
                    double res = 0;
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        if (WORLD.WATER().OCEAN.is.is(wx, wy, DIR.ORTHO.get(di)))
                            res += 0.25;
                    }
                    return CLAMP.d(res, 0, 1);
                }
            };

            WET = new TERRAIN(ts, "WET", json,
                    D.g("Sweet", "Fresh water"),
                    D.g("SweetD", "Fresh water such as river beds or lakes. Offers natural irrigation, clay deposits and some fish."), true)
            {
                public override SPRITE icon()
                {
                    return WORLD.WATER().LAKE.icon;
                }

                public override double value(int wx, int wy)
                {
                    double res = 0;
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        if (WORLD.WATER().RIVER_SMALL.is(wx, wy, DIR.ORTHO.get(di)))
                            res += 0.2;
                        if (WORLD.WATER().LAKE.is.is(wx, wy, DIR.ORTHO.get(di)) || WORLD.WATER().isRivery.is(wx, wy, DIR.ORTHO.get(di)))
                            res += 0.25;
                    }
                    return CLAMP.d(res, 0, 1);
                }
            };

            MOUNTAIN = new TERRAIN(ts, "MOUNTAIN", json,
                    D.g("Mountain"),
                    D.g("MountainD", "Rich in caverns and mineral deposits."), true)
            {
                public override SPRITE icon()
                {
                    return SETT.TERRAIN().MOUNTAIN.getIcon();
                }

                public override double value(int wx, int wy)
                {
                    double res = 0;
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        if (WORLD.MOUNTAIN().haser.is(wx, wy, DIR.ORTHO.get(di)))
                            res += 0.25;
                    }
                    return CLAMP.d(res, 0, 1);
                }
            };

            FOREST = new TERRAIN(ts, "FOREST", json,
                    D.g("Forest"),
                    D.g("ForestD", "Forested areas. Good for lumber."), true)
            {
                public override SPRITE icon()
                {
                    return WORLD.FOREST().icon;
                }

                public override double value(int wx, int wy)
                {
                    double res = 0;
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        if (WORLD.MOUNTAIN().haser.is(wx, wy, DIR.ORTHO.get(di)))
                            res += 0.25 * WORLD.FOREST().amount.get(wx, wy);
                    }
                    return res;
                }
            };

            NONE = new TERRAIN(ts, "NONE", json,
                    D.g("OpenLand", "Open Land"),
                    D.g("Open Land", "Open land to roam about on."), true)
            {
                public override SPRITE icon()
                {
                    return WORLD.GROUND().icon;
                }

                public override double value(int wx, int wy)
                {
                    double res = 1;
                    for (int ti = 0; ti < ALL().size; ti++)
                    {
                        if (ti == index())
                            continue;
                        res -= ALL().get(ti).value(wx, wy);
                    }

                    return CLAMP.d(res, 0, 1);
                }
            };

            all = new ArrayList<TERRAIN>(ts);
            KeyMap<TERRAIN> m = new KeyMap<TERRAIN>();
            foreach (TERRAIN t in all)
                m.put(t.key, t);
            map = new RMAP<TERRAIN>(key, all);
        }

        public static MAP_OBJECT<TERRAIN> sett = new MAP_OBJECT<TERRAIN>()
        {
            public TERRAIN get(int tile)
            {
                throw new RuntimeException();
            }

            public TERRAIN get(int tx, int ty)
            {
                if (!IN_BOUNDS(tx, ty))
                    return NONE();
                if (TERRAIN().TREES.isTree(tx, ty))
                    return FOREST();
                if (TERRAIN().MOUNTAIN.isMountain(tx, ty))
                    return MOUNTAIN();
                if (TERRAIN().WATER.SHALLOW.is(tx, ty) && GROUND().types.SAND.is(tx, ty))
                    return OCEAN();
                if (TERRAIN().WATER.SHALLOW.is(tx, ty))
                    return WET();
                return NONE();
            }
        };

        public static MAP_OBJECT<TERRAIN> world = new MAP_OBJECT<TERRAIN>()
        {
            public TERRAIN get(int tile)
            {
                throw new RuntimeException();
            }

            public TERRAIN get(int tx, int ty)
            {
                if (!WORLD.IN_BOUNDS(tx, ty))
                    return NONE();
                if (WORLD.MOUNTAIN().is(tx, ty))
                    return MOUNTAIN();
                if (WORLD.WATER().OCEAN.is.is(tx, ty))
                    return OCEAN();
                if (WORLD.WATER().fertile.is(tx, ty))
                    return WET();
                if (WORLD.FOREST().is.is(tx, ty))
                    return FOREST();
                return NONE();
            }
        };

        public static LIST<TERRAIN> ALL()
        {
            return self.all;
        }

        public static RMAP<TERRAIN> MAP()
        {
            return self.map;
        }

        public static TERRAIN OCEAN()
        {
            return self.OCEAN;
        }

        public static TERRAIN WET()
        {
            return self.WET;
        }

        public static TERRAIN MOUNTAIN()
        {
            return self.MOUNTAIN;
        }

        public static TERRAIN FOREST()
        {
            return self.FOREST;
        }

        public static TERRAIN NONE()
        {
            return self.NONE;
        }

        public static INFO INFO()
        {
            return self.info;
        }
    }
}