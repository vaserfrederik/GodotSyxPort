using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;
using util.text;

public static class RESOURCES
{
    private static Data data;

    public static readonly string KEY = "RESOURCE";
    public static readonly string KEYS = "RESOURCES";

    private static readonly string helpStone = "Stone can be obtained by manually clearing rocks on the ground.";
    private static readonly string helpWood = "Wood can be obtained by manually clearing trees.";
    private static readonly string helpGrow = "This crop might be growing in the wild. Have a look, maybe you can harvest some manually.";

    static RESOURCES()
    {
        D.ts(typeof(RESOURCES));
    }

    private sealed class Data
    {
        private readonly LIST<RESOURCE> all;
        private readonly RMAPS<RESOURCE> map;
        private readonly RMAP<Minable> minable;
        private readonly ResGroup<ResGDrink> drinks;
        private readonly GrowableGroup growable;
        private readonly ResGroup<ResGEat> edibles;
        private readonly RESOURCE STONE, WOOD, LIFESTOCK;
        private readonly int catAmount;
        private readonly ResSupplies supplies;

        public Data() throws IOException
        {
            data = this;

            PATH gInit = PATHS.INIT().getFolder("resource");
            PATH gText = PATHS.TEXT().getFolder("resource");
            PATH gSprite = PATHS.SPRITE().getFolder("resource");
            PATH gDebris = gSprite.getFolder("debris");

            string[] files;

            {
                string[] fixed = new string[] {
                    "_STONE",
                    "_WOOD",
                    "_LIVESTOCK",
                };
                string[] mod = gInit.getFiles();
                files = new string[fixed.Length + mod.Length];
                for (int i = 0; i < fixed.Length; i++)
                    files[i] = fixed[i];
                for (int i = 0; i < mod.Length; i++)
                {
                    files[i + fixed.Length] = mod[i];
                }

                string[][] resources = new string[10][];
                for (int i = 0; i < resources.Length; i++)
                {
                    resources[i] = new string[64];
                }
                int[] catI = Alloc.ii(10);
                bool[] categories = new bool[10];
                int cats = 0;

                foreach (string s in files)
                {
                    Json inJson = new Json(gInit.gets(s));
                    int c = inJson.i("CATEGORY_DEFAULT", 0, 10);
                    resources[c][catI[c]] = s;
                    catI[c]++;
                    if (!categories[c])
                    {
                        cats++;
                        categories[c] = true;
                    }
                }

                catAmount = cats;

                int q = 0;
                for (int i = 0; i < resources.Length; i++)
                {
                    for (int k = 0; k < catI[i]; k++)
                    {
                        files[q++] = resources[i][k];
                    }
                }
            }

            {
                ArrayList<RESOURCE> all = new ArrayList<RESOURCE>(128);
                KeyMap<Sprite> spriteMap = new KeyMap<Sprite>();
                KeyMap<TILE_SHEET> debrisMap = new KeyMap<TILE_SHEET>();

                foreach (string key in files)
                {
                    new RESOURCE(all, key, gInit, gText, gSprite, gDebris, spriteMap, debrisMap);
                }

                this.all = new ArrayList<RESOURCE>(all);

                map = new RMAPS<RESOURCE>(KEY, this.all);

                STONE = map.get("_STONE", null);
                WOOD = map.get("_WOOD", null);
                LIFESTOCK = map.get("_LIVESTOCK", null);
            }

            {
                minable = Minable.make(gInit, gSprite);
                growable = Growable.make(gInit, gSprite);
                drinks = ResGDrink.make(gInit);
                edibles = ResGEat.make(gInit);
            }

            {
                supplies = new ResSupplies();
            }

            STONE.specialHelpText = helpStone;
            WOOD.specialHelpText = helpWood;
            foreach (Growable g in growable.all())
            {
                g.resource.specialHelpText = helpGrow;
            }
        }
    }

    public RESOURCES(INIT init) throws IOException
    {
        base(init);
        new Data();
    }

    public static LIST<RESOURCE> ALL()
    {
        return data.all;
    }

    public static RMAP<Minable> minables()
    {
        return data.minable;
    }

    public static GrowableGroup growable()
    {
        return data.growable;
    }

    public static ResGroup<ResGDrink> DRINKS()
    {
        return data.drinks;
    }

    public static ResGroup<ResGEat> EDI()
    {
        return data.edibles;
    }

    public static RESOURCE STONE()
    {
        return data.STONE;
    }

    public static RESOURCE WOOD()
    {
        return data.WOOD;
    }

    public static RESOURCE LIVESTOCK()
    {
        return data.LIFESTOCK;
    }

    public static int CATEGORIES()
    {
        return data.catAmount;
    }

    public static ResSupplies SUP()
    {
        return data.supplies;
    }

    public static RMAPS<RESOURCE> map()
    {
        return data.map;
    }
}