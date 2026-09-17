using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using game.audio;
using init.paths;
using init.resources;
using init.sprite;
using init.sprite.game;
using settlement.path;
using settlement.tilemap.terrain;
using snake2d;
using util.rendering;

public sealed class TDecor : TerrainTile
{
    private readonly LIST<SheetPair> sheets;

    private readonly TerrainClearing clearing = new TerrainClearing
    {
        sound = AUDIO.race("CLEAR"),

        public RESOURCE clear1(int tx, int ty)
        {
            shared.NADA.placeFixed(tx, ty);
            return null;
        },

        public bool can()
        {
            return true;
        },

        public int clearAll(int tx, int ty)
        {
            shared.NADA.placeFixed(tx, ty);
            return 0;
        },

        public SoundRace sound(int tx, int ty)
        {
            return sound;
        },

        public bool isEasilyCleared()
        {
            return true;
        }
    };

    public TDecor(Terrain t, string name, string sKey) : base("DECORD_" + sKey, t, name, SPRITES.icons().m.cancel, null)
    {
        sheets = SPRITES.GAME().sheets(SheetType.s1x1, JsonConvert.DeserializeObject<Json>(File.ReadAllText(PATHS.CONFIG().init.gets("SETT_MAP_DECORATION"))).json(sKey));
    }

    public override TerrainClearing clearing()
    {
        return clearing;
    }

    protected override bool place(int tx, int ty)
    {
        base.placeRaw(tx, ty);
        return false;
    }

    protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
    {
        return false;
    }

    protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator it, int data)
    {
        if (sheets.size() == 0)
            return false;

        int ran = it.ran();
        SheetPair sheet = sheets.getC(ran);
        if (sheet == null)
            return false;
        ran = ran >> 5;
        sheet.d.color(ran).bind();
        ran = ran >> 4;

        int frame = sheet.d.frame(ran, 1.0);
        int tile = SheetType.s1x1.tile(sheet.s, sheet.d, 0, frame, ran & 0b11);

        sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, 0);
        COLOR.unbind();
        if (s != null)
            sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
        return false;
    }

    public override AVAILABILITY getAvailability(int x, int y)
    {
        return null;
    }

    public override bool isPlacable(int tx, int ty)
    {
        return true;
    }
}