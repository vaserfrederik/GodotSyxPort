using System;
using game.battle.formation;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util.rendering;

public sealed class DivRenderer
{
    private DivRenderer()
    {
    }

    public static void Render(SPRITE_RENDERER ren, DivFormation p, RenderData data)
    {
        Render(ren, p, data, 0, 0);
    }

    public static void Render(SPRITE_RENDERER ren, DivFormation p, RenderData data, int offX, int offY)
    {
        if (p == null)
            return;

        int men = p.Deployed();

        if (men == 0)
            return;

        if (!p.Body().Touches(data.GBounds()))
            return;

        int ox = data.OffX1() + C.TILE_SIZEH + offX;
        int oy = data.OffY1() + C.TILE_SIZEH + offY;

        for (int i = 0; i < men; i++)
        {
            int rx = p.Px(i) - ox;
            int ry = p.Py(i) - oy;
            if (CORE.Renderer().GetZoomout() < 3)
            {
                DIR d = p.Dir();
                SPRITE s = SPRITES.Cons().ICO.arrows2.Get(d.Id());
                rx -= C.TILE_SIZEH / 2 * d.X();
                ry -= C.TILE_SIZEH / 2 * d.Y();
                s.Render(ren, rx, ry);
            }
            else
            {
                int m = p.DirMaskOrtho(i);
                SPRITES.Cons().BIG.dots.Render(ren, m, rx, ry);
            }
        }

        ox -= C.TILE_SIZEH;
        oy -= C.TILE_SIZEH;
    }
}