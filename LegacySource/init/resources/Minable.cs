using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.keymap;
using util.spritecomposer;
using util.text;

namespace init.resources
{
    public sealed class Minable : MAPPED
    {
        private static readonly CharSequence ¤¤minable = "¤{0} Deposits";
        static
        {
            D.ts(typeof(Minable));
        }

        public readonly RESOURCE resource;
        public readonly CharSequence name;
        public readonly TILE_SHEET sheet;
        public readonly bool onEverymap;
        public readonly COLOR tint;
        public readonly COLOR miniColor;
        public readonly int index;
        private readonly double[] terrainPref;
        public readonly double occurence;
        public double fertilityIncrease;
        private readonly string key;

        Minable(string key, int index, TILE_SHEET sheet, Json json)
        {
            onEverymap = json.bool("ON_EVERY_MAP");
            tint = new ColorImp(json);
            miniColor = new ColorImp(json, "MINIMAP_COLOR");
            fertilityIncrease = json.d("FERTILITY_INCREASE", -1, 1);
            this.sheet = sheet;
            this.index = index;
            this.resource = RESOURCES.map().read(json);
            name = new Str(¤¤minable).insert(0, resource.name).trim();
            terrainPref = TERRAINS.MAP().readFill(json, 1.0);
            double mm = 0;
            foreach (double d in terrainPref)
            {
                mm += d;
            }
            for (int i = 0; i < terrainPref.Length; i++)
            {
                terrainPref[i] /= mm;
            }

            occurence = json.dTry("OCCURENCE", 0, 1000, 1);
            this.key = key;
        }

        public static RMAP<Minable> make(PATH pathData, PATH pathSprites)
        {
            string folder = "minable";

            var pd = pathData.getFolder(folder);
            var ps = pathSprites.getFolder(folder);
            var spriteMap = new Dictionary<string, TILE_SHEET>();

            Util util = new Util();
            string[] files = pd.getFiles(1, 31);
            var res = new ArrayList<Minable>(files.Length);

            foreach (string p in files)
            {
                Json j = new Json(pd.gets(p));
                string sprite = j.value("SPRITE");
                if (!spriteMap.ContainsKey(sprite))
                {
                    if (!ps.exists(sprite))
                    {
                        string er = "Could not find texture file named: " + sprite + " Found only: " + Environment.NewLine;
                        foreach (var e in spriteMap)
                        {
                            er += Environment.NewLine + e.Key;
                        }
                        j.error(er, sprite);
                    }
                    spriteMap[sprite] = util.sprite(ps.get(sprite));

                }
                Minable g = new Minable(p, res.size(), spriteMap[sprite], j);
                res.add(g);
            }

            return new RMAP<Minable>("MINABLE", res);
        }

        private sealed class Util
        {
            public Util()
            {
            }

            private TILE_SHEET sprite(Path path)
            {
                return new ITileSheet(path, 364, 94)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.init(0, 0, 1, 1, 8, 2, d.s16);
                        s.singles.paste(1, true);
                        return d.s16.saveGame();
                    }
                }.get();
            }
        }

        public int index()
        {
            return index;
        }

        public double terrain(TERRAIN t)
        {
            return terrainPref[t.index()];
        }

        public override string key()
        {
            return key;
        }
    }
}