using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;

namespace World.Map.Regions
{
    static class GenName
    {
        private static readonly Json jland = new Json(PATHS.NAMES().gets("WorldAreas"));
        private static readonly LandCounter cMisc = new LandCounter(jland.texts("MISC"), 0.2)
        {
            Count = (tx, ty) => true
        };

        private static readonly string[] sIslands = jland.texts("ISLAND_ADDONS");

        private static readonly LandCounter[] counts = new LandCounter[]
        {
            new LandCounter(jland.texts("MOUNTAIN"), 0.2)
            {
                Count = (tx, ty) => MOUNTAIN().is(tx, ty)
            },
            new LandCounter(jland.texts("FOREST"), 0.4)
            {
                Count = (tx, ty) => FOREST().is.is(tx, ty)
            },
            new LandCounter(jland.texts("RIVER"), 0.1)
            {
                Count = (tx, ty) => WATER().isRivery.is(tx, ty)
            },
            new LandCounter(jland.texts("OCEAN"), 0.15)
            {
                Count = (tx, ty) => WATER().OCEAN.normal.is(tx, ty)
            },
            new LandCounter(jland.texts("LAKE"), 0.15)
            {
                Count = (tx, ty) => WATER().LAKE.normal.is(tx, ty)
            },
            new LandCounter(jland.texts("STEPPE"), 0.6)
            {
                Count = (tx, ty) => ty < WORLD.THEIGHT() / 2 && GROUND().getter.get(tx, ty).moisture() < 0.2
            },
            new LandCounter(jland.texts("DESERT"), 0.6)
            {
                Count = (tx, ty) => ty > WORLD.THEIGHT() / 2 && GROUND().getter.get(tx, ty).moisture() < 0.2
            },
        };

        public GenName()
        {
            for (int ri = 1; ri < WREGIONS.MAX; ri++)
            {
                Region reg = WORLD.REGIONS().getByIndex(ri);
                Init(reg);
            }
            WORLD.REGIONS().player.info.name().clear().add(FACTIONS.player().name);
        }

        void Init(Region r)
        {
            if (r == null || r.info.area() == 0)
                return;

            foreach (LandCounter c in counts)
            {
                c.count = 0;
                c.value = 0;
            }

            foreach (COORDINATE coo in r.info.bounds())
            {
                if (!REGIONS().map.is(coo, r))
                    continue;
                foreach (LandCounter c in counts)
                {
                    if (c.Count(coo.x(), coo.y()))
                        c.count++;
                }
            }

            LandCounter bestFit = cMisc;

            foreach (LandCounter c in counts)
            {
                c.value = c.count / (double)r.info.area();
                c.value /= c.treshold;
                if (c.value > 1.0 && c.value > bestFit.value)
                    bestFit = c;
            }

            if (IsIsland(r))
            {
                r.info.name().clear().add(sIslands[RND.rInt(sIslands.Length)]);
                r.info.name().insert(0, bestFit.GetName());
            }
            else
            {
                r.info.name().clear().add(bestFit.GetName());
            }
        }

        private static bool IsIsland(Region r)
        {
            foreach (COORDINATE c in r.info.bounds())
            {
                if (!REGIONS().map.is(c, r))
                    continue;
                foreach (DIR d in DIR.ORTHO)
                    if (!REGIONS().map.is(c, d, r) && !WATER().has.is(c))
                    {
                        return false;
                    }
            }
            return true;
        }

        private abstract class LandCounter
        {
            private readonly string[] names;
            private int nameI = 0;
            private double treshold;
            private double value;

            public LandCounter(string[] names, double treshold)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    string old = names[i];
                    int k = RND.rInt(names.Length);
                    names[i] = names[k];
                    names[k] = old;
                }

                this.names = names;
                this.treshold = treshold;
            }

            public string GetName()
            {
                int i = nameI;
                nameI++;
                if (nameI >= names.Length)
                    nameI = 0;
                return names[i];
            }

            public int count;
            public abstract bool Count(int tx, int ty);
        }
    }
}