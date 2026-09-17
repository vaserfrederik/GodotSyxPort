using System;
using System.Collections.Generic;
using init.paths;
using snake2d.PathTile;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using util;
using world;

namespace world.map.landmark
{
    internal sealed class GeneratorLandmark
    {
        private readonly Polymap polly = new Polymap(TWIDTH(), THEIGHT(), (int)(40 * (TWIDTH() / 250.0)), 1.0);
        private readonly WorldLandmark rubbish = LANDMARKS().getByIndex(0);

        private readonly Json json = new Json(PATHS.NAMES().gets("WorldLandmarks"));

        private readonly Type[] types = new Type[]
        {
            new Type(json, "MOUNTAIN", 40, 1000)
            {
                public override bool is(int tx, int ty)
                {
                    return MOUNTAIN().heighter.get(tx, ty) > 0 && !WATER().has.is(tx, ty);
                }
            },
            new Type(json, "LAKE", 20, 10000)
            {
                public override bool is(int tx, int ty)
                {
                    return WATER().LAKE.is.is(tx, ty);
                }
            },
            new Type(json, "RIVER", 25, 100)
            {
                public override bool is(int tx, int ty)
                {
                    return WATER().isRivery.is(tx, ty);
                }
            },
            new Type(json, "OCEAN", 50, 5000)
            {
                public override bool is(int tx, int ty)
                {
                    return WATER().OCEAN.is.is(tx, ty);
                }
            },
        };

        public GeneratorLandmark(ACTION loadprint)
        {
            loadprint.exe();

            WORLD.LANDMARKS().saver().clear();

            int nr = 1;

            foreach (COORDINATE c in TBOUNDS())
            {
                if (nr >= WorldLandmarks.MAX)
                    break;
                if (assignTerrain(c.x(), c.y(), LANDMARKS().getByIndex(nr)))
                    nr++;
            }

            foreach (COORDINATE c in TBOUNDS())
            {
                if (LANDMARKS().setter.get(c) == rubbish)
                {
                    LANDMARKS().setter.set(c, null);
                    continue;
                }
            }
            loadprint.exe();

            new GeneratorLandmarkValidator(null);

            loadprint.exe();
        }

        private bool assignTerrain(int tx, int ty, WorldLandmark ass)
        {
            if (LANDMARKS().setter.get(tx, ty) != null)
            {
                return false;
            }

            Type type = type(tx, ty);
            if (type == null)
                return false;

            polly.checkInit();
            polly.checker.set(tx, ty, true);

            int minSize = type.minSize;
            int maxSize = type.maxSize;

            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(tx, ty, 0);
            int area = 0;

            WorldLandmark neigh = null;

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();

                LANDMARKS().setter.set(t, ass);

                area++;

                if (area > maxSize)
                    break;

                foreach (DIR d in DIR.ORTHO)
                {
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (TBOUNDS().holdsPoint(dx, dy))
                    {
                        if (!type.is(dx, dy))
                            continue;

                        WorldLandmark kuk = LANDMARKS().setter.get(dx, dy);
                        if (kuk != null)
                        {
                            if (kuk != rubbish && kuk != ass)
                                neigh = kuk;
                            continue;
                        }

                        double q = t.getValue() + d.tileDistance();
                        double dd = polly.checker.is(t, d) ? q : q + 100;
                        polly.checker.set(t, true);
                        GUTIL.flooder().pushSmaller(t, d, dd);
                    }
                }
            }

            GUTIL.flooder().done();

            if (area < minSize)
            {
                if (neigh != null)
                {
                    assign(tx, ty, ass, neigh);
                }
                else
                {
                    assign(tx, ty, ass, rubbish);
                }
                return false;
            }

            type.init(ass);
            return true;
        }

        private void assign(int tx, int ty, WorldLandmark old, WorldLandmark newa)
        {
            GUTIL.flooder().init(this);

            GUTIL.flooder().pushSloppy(tx, ty, 0);

            while (GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                if (LANDMARKS().setter.get(t) != old)
                    continue;
                LANDMARKS().setter.set(t, newa);
                foreach (DIR d in DIR.ORTHO)
                {
                    if (TBOUNDS().holdsPoint(t, d))
                        GUTIL.flooder().pushSmaller(t, d, t.getValue() + d.tileDistance());
                }
            }

            GUTIL.flooder().done();
        }

        public Type type(int tx, int ty)
        {
            foreach (Type t in types)
                if (t.is(tx, ty))
                    return t;
            return null;
        }

        private abstract class Type : MAP_BOOLEAN
        {
            private readonly string[] names;
            private readonly string[] addons;
            private int nameI = 0;
            private readonly Json[] specials;
            private int sI = 0;
            private int minSize;
            private int maxSize;

            public Type(Json json, string key, int min, int max)
            {
                json = json.json(key);
                this.minSize = min;
                this.maxSize = max;
                names = json.texts("NAMES");
                for (int i = 0; i < names.Length; i++)
                {
                    int k = RND.rInt(names.Length);
                    string o = names[i];
                    names[i] = names[k];
                    names[k] = o;
                }
                addons = json.texts("ADDONS");
                specials = json.jsons("SPECIAL");

                for (int i = 0; i < specials.Length; i++)
                {
                    int k = RND.rInt(specials.Length);
                    Json o = specials[i];
                    specials[i] = specials[k];
                    specials[k] = o;
                }
            }

            public override bool is(int tile)
            {
                return false;
            }

            public void init(WorldLandmark l)
            {
                if (sI < specials.Length)
                {
                    l.name.clear().add(specials[sI].text("NAME"));
                    l.description.clear().add(specials[sI].text("LORE"));
                    if (l.description.length() > 1024)
                        specials[sI].error("Lore is too long...", "LORE");
                    sI++;
                }
                else
                {
                    if (names.Length == 0)
                    {
                        l.name.clear().add(l.index);
                    }
                    else
                    {
                        if (addons.Length > 0)
                        {
                            l.name.clear().add(addons[RND.rInt(addons.Length)]);
                            l.name.insert(0, names[nameI++]);
                        }
                        else
                            l.name.clear().add(names[nameI++]);
                        if (nameI >= names.Length)
                            nameI = 0;
                    }
                }
            }
        }
    }
}