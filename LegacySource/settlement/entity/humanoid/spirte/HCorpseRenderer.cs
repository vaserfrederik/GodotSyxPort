using System;
using System.Collections.Generic;
using Init.Race;
using Init.Race.Appearance;
using Settlement.Entity.Humanoid.Sprite;
using Settlement.Stats;
using Settlement.Stats.Colls;
using Snake2D.Renderer;
using Snake2D.Util.Color;
using Snake2D.Util.Sprite;
using Util.Rendering;

public static class HCorpseRenderer
{
    private HCorpseRenderer()
    {
    }

    private static readonly Color decayC = new ColorImp(48, 24, 12);
    private static readonly ColorImp inter = new ColorImp();

    public static void RenderSkelleton(Race race, bool adult, int direction, bool inWater, Renderer r, ShadowBatch s, int ran, int x, int y)
    {
        x += CLAY.off;
        y += CLAY.off;
        int dir = direction;
        if (inWater)
            return;
        s.SetHeight(2).SetDistance2Ground(0);
        TILE_SHEET sheet = race.Appearance().Skelleton(adult);
        int tile = 8 * (ran & 1) + dir;
        sheet.Render(r, tile, x, y);
        sheet.Render(s, tile, x, y);
        tile = 8 * 2 + 8 * (ran & 3) + dir;
        sheet.Render(r, tile, x, y);
        sheet.Render(s, tile, x, y);
        return;
    }

    public static void RenderCorpse(Induvidual indu, int direction, bool inWater, double decay, Renderer r, ShadowBatch s, int x, int y, int distToground)
    {
        x += CLAY.off;
        y += CLAY.off;
        int dir = direction;

        TILE_SHEET sheet = indu.Race().Appearance().Sheet(indu).Sheet.Lay;

        if (!inWater)
        {
            s.SetHeight(3).SetDistance2Ground(distToground);
            sheet.Render(s, CLAY.SHADOW + dir, x, y);
        }

        StatsAppearance ap = STATS.APPEARANCE();
        inter.Interpolate(ap.ColorLegs(indu), decayC, decay).Bind();
        sheet.Render(r, CLAY.PANTS + dir, x, y);
        inter.Interpolate(ap.ColorSkin(indu), decayC, decay).Bind();
        sheet.Render(r, CLAY.ARMS + dir, x, y);
        foreach (RAddon add in indu.Race().Appearance().Types.Get(ap.Gender.Get(indu)).AddonsBelow)
        {
            add.RenderLaying(r, dir, x, y, indu, false, decayC, decay);
        }
        inter.Bind();
        sheet.Render(r, CLAY.HEAD + dir, x, y);
        inter.Interpolate(ap.ColorClothes(indu), decayC, decay).Bind();
        sheet.Render(r, CLAY.TORSO + dir, x, y);

        foreach (RAddon add in indu.Race().Appearance().Types.Get(ap.Gender.Get(indu)).AddonsAbove)
        {
            add.RenderLaying(r, dir, x, y, indu, false, decayC, decay);
        }
        COLOR.Unbind();

        OPACITY.O99.Bind();
        inter.Interpolate(COLOR.WHITE100, decayC, decay).Bind();

        CLAY.Blood(indu, dir, x, y);
        CLAY.Filth(indu, dir, x, y);

        if (inWater)
        {
            CLAY.Water(indu, dir, x, y);
        }

        OPACITY.Unbind();
    }

    public static void RenderDump(Race race, double decay, int dir, Renderer r, ShadowBatch s, int ran, int x, int y)
    {
        x += CLAY.off;
        y += CLAY.off;

        TILE_SHEET sheet = race.Appearance().Adult().Sheet.Lay;

        s.SetHeight(3).SetDistance2Ground(0);
        sheet.Render(s, CLAY.SHADOW + dir, x, y);

        decayC.Bind();
        sheet.Render(r, CLAY.PANTS + dir, x, y);
        sheet.Render(r, CLAY.ARMS + dir, x, y);
        sheet.Render(r, CLAY.HEAD + dir, x, y);
        sheet.Render(r, CLAY.TORSO + dir, x, y);
        COLOR.Unbind();

        OPACITY.O99.Bind();
        inter.Interpolate(COLOR.WHITE100, decayC, decay).Bind();

        CLAY.Filth(race, true, decay, dir, ran, x, y);

        OPACITY.Unbind();
        COLOR.Unbind();
    }

    public static void RenderGore(Induvidual indu, int direction, bool inWater, double decay, Renderer r, ShadowBatch s, int x, int y)
    {
        int ran = (int)((STATS.RAN().Get(indu, 16)) & 7);

        x += CLAY.off;
        y += CLAY.off;
        int dir = direction;

        TILE_SHEET stencil = RACES.Sprites().GoreStencil;

        Color blood = indu.Race().Appearance().Colors.Blood;

        TILE_SHEET sheet = indu.Race().Appearance().Sheet(indu).Sheet.Lay;

        if (!inWater)
        {
            s.SetHeight(3).SetDistance2Ground(0);
            sheet.Render(s, CLAY.SHADOW + dir, x, y);
        }

        StatsAppearance ap = STATS.APPEARANCE();
        inter.Interpolate(ap.ColorLegs(indu), decayC, decay).Bind();
        stencil.RenderTextured(sheet.GetTexture(CLAY.PANTS + dir), ran, x, y);

        inter.Interpolate(ap.ColorSkin(indu), decayC, decay).Bind();
        stencil.RenderTextured(sheet.GetTexture(CLAY.ARMS + dir), ran, x, y);
        stencil.RenderTextured(sheet.GetTexture(CLAY.HEAD + dir), ran, x, y);

        inter.Interpolate(ap.ColorClothes(indu), decayC, decay).Bind();
        stencil.RenderTextured(sheet.GetTexture(CLAY.TORSO + dir), ran, x, y);

        foreach (RAddon add in indu.Race().Appearance().Types.Get(ap.Gender.Get(indu)).AddonsAbove)
        {
            add.RenderLayingTextured(stencil, ran, r, dir, x, y, indu, false);
        }
        COLOR.Unbind();

        inter.Interpolate(blood, decayC, decay).Bind();
        TextureCoords overlay = RACES.Sprites().GoreOverlay.GetTexture(ran);
        sheet.RenderTextured(overlay, CLAY.SHADOW + dir, x, y);

        CLAY.Filth(indu, dir, x, y);

        if (inWater)
        {
            CLAY.Water(indu, dir, x, y);
        }

        OPACITY.Unbind();
        COLOR.Unbind();
    }
}