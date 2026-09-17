using System;
using System.Collections.Generic;
using settlement.tilemap.ground;
using init.constant;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.info;

public class GroundType : INFO, MAP_BOOLEAN
{
    public readonly int index;
    public readonly TILE_SHEET sheet;

    public readonly ColorImp miniC = new ColorImp();
    private bool special;
    private readonly ColorImp[] tmps;
    public readonly SPRITE icon;

    public readonly double vegitation;
    public readonly double farm;

    protected GroundType(int index, TILE_SHEET sheet, string name, string desc, double vegitation, double farm)
        : base(name, desc)
    {
        this.index = index;
        this.sheet = sheet;
        tmps = new ColorImp[Ground.MOISTURE_MAX + 1];
        for (int i = 0; i < tmps.Length; i++)
        {
            tmps[i] = new ColorImp();
        }

        icon = new SPRITE.Imp(Icon.L)
        {
            Render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
            {
                tmps[8].bind();
                sheet.render(r, 0, X1, X1 + C.T_PIXELS, Y1, Y1 + C.T_PIXELS);
                sheet.render(r, 1, X1 + C.T_PIXELS, X2, Y1, Y1 + C.T_PIXELS);
                sheet.render(r, 2, X1, X1 + C.T_PIXELS, Y1 + C.T_PIXELS, Y2);
                sheet.render(r, 3, X1 + C.T_PIXELS, X2, Y1 + C.T_PIXELS, Y2);
                COLOR.unbind();
            }
        };

        this.vegitation = vegitation;
        this.farm = farm;
    }

    public void SetColors(COLOR dry, COLOR wet, double add)
    {
        if (special)
            return;

        for (int i = 0; i < tmps.Length; i++)
        {
            double d = i * Ground.MOISTURE_MAXI;
            tmps[i].Interpolate(dry, wet, CLAMP.d(d + add, 0, 1));
        }

        miniC.Interpolate(dry, wet, 0.5 - index / 8.0);
    }

    public GroundType SetColors(Json json)
    {
        COLOR dry = new ColorImp(json, "DRY");
        COLOR wet = new ColorImp(json, "WET");
        SetColors(dry, wet, 0);
        special = true;
        return this;
    }

    public void PlaceFixed(int x, int y)
    {
        SETT.GROUND().MAP.Set(x, y, this);
    }

    public bool Is(int tile)
    {
        return SETT.GROUND().MAP.Get(tile) == this;
    }

    public bool Is(int tx, int ty)
    {
        return SETT.GROUND().MAP.Get(tx, ty) == this;
    }

    public COLOR Col(int tile)
    {
        return tmps[SETT.GROUND().mapMoistureCurrent.Get(tile)];
    }
}