using System;
using System.Collections.Generic;
using System.IO;
using init.paths;
using init.type;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using util.rendering;

public static class Growable : ResG
{
    public readonly double seasonalOffset;
    public readonly double growthValue;
    public readonly COLOR colorMinimap;

    private readonly double[] climate;
    public readonly GrowableSprite sprite;

    private Growable(string key, int index, Json json, KeyMap<TILE_SHEET> sheetMap) : base(index, key, RESOURCES.map().read(json))
    {
        seasonalOffset = json.d("SEASONAL_OFFSET", 0, 1);
        growthValue = json.d("GROWTH_VALUE", 0, 1.0);
        this.colorMinimap = new ColorImp(json, "MINIMAP_COLOR");
        climate = new double[CLIMATES.ALL().size()];
        CLIMATES.MAP().readFill("CLIMATE_BONUS", climate, json, 0, 10000);

        {
            json = json.json("SPRITE");

            double poll = json.d("POLLEN", 0, 10);
            double wind = json.dTry("WIND_SWAY", 0, 10, 1);

            sprite = new GrowableSprite(json.value("SPRITE"), wind, poll, sheetMap);

            sprite.setPollenColor(new ColorImp(json, "COLOR_POLLEN"));

            set(json.json("STEM"), sprite.trunk);
            set(json.json("GROWTH"), sprite.growth);
        }
    }

    private static void set(Json json, GrowableSprite.Part part)
    {
        part.sheightoverGround = json.d("SHADOW_HEIGHT", 0, 32);
        part.sheight = json.d("SHADOW_LENGTH", 0, 32);
        part.setColors(new ColorImp(json, "DEAD"), new ColorImp(json, "LIVE"), new ColorImp(json, "RIPE"));
        if (json.has("WIND_SWAY"))
            part.sway = json.d("WIND_SWAY", 0, 10);
    }

    public void render(SPRITE_RENDERER r, ShadowBatch shadowBatch, RenderData.RenderIterator it, int amount, bool ripe)
    {
    }

    public double availability(CLIMATE c)
    {
        return climate[c.index()];
    }

    public static GrowableGroup make(final PATH pathData, final PATH pathSprites) throws IOException
    {
        string folder = "growable";
        final PATH pd = pathData.getFolder(folder);

        KeyMap<TILE_SHEET> sheetMap = new KeyMap<TILE_SHEET>();
        string[] files = pd.getFiles();
        final ArrayList<Growable> res = new ArrayList<Growable>(files.Length);

        foreach (string p in files)
        {
            Json j = new Json(pd.gets(p));
            Growable g = new Growable(p, res.size(), j, sheetMap);
            res.add(g);
        }

        return new GrowableGroup(sheetMap, res);
    }

    public static class GrowableGroup : ResGroup<Growable>
    {
        private readonly KeyMap<TILE_SHEET> sheetMap;

        public GrowableGroup(KeyMap<TILE_SHEET> sheetMap, ArrayList<Growable> res) : base("GROWABLE", res)
        {
            this.sheetMap = sheetMap;
        }

        public GrowableSprite sprite(string ssheet, double wind, double pollen) throws IOException
        {
            return new GrowableSprite(ssheet, wind, pollen, sheetMap);
        }
    }
}