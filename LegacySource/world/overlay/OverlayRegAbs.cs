using System.Collections.Generic;
using snake2d;
using util.colors;
using util.rendering;
using world;
using world.map.regions;

abstract class OverlayRegAbs : WorldOverlays.OverlayTileNormal
{
    private bool inv;

    OverlayRegAbs(CharSequence name, CharSequence desc, bool inv) : base(name, desc, true, true)
    {
        this.inv = inv;
    }

    public override void renderAbove(Renderer ren, ShadowBatch s, RenderData data)
    {
        base.renderAbove(ren, s, data);

        foreach (Region reg in WORLD.REGIONS().active())
        {
            if (is(reg))
            {
                double v = value(reg) * 100.0;

                int x = data.transformGX((reg.cx()) * C.TILE_SIZE + C.TILE_SIZEH);
                int y = data.transformGY((reg.cy()) * C.TILE_SIZE + C.TILE_SIZEH);
                Str.TMP.clear().add(v, 1);
                Str.TMP.add('%');

                int w = C.TILE_SIZE * 2;
                int h = C.TILE_SIZE;

                GCOLOR.UI().panBG.render(ren, x - w, x + w, y - h, y + h);
                GCOLOR.UI().border().renderFrame(ren, x - w, x + w, y - h, y + h, C.SCALE, C.SCALE);
                UI.FONT().S.renderC(ren, x, y, Str.TMP, C.SCALE);
            }
        }

        COLOR.WHITE2WHITE.bind();

        foreach (RaidEntryPoint c in GAME.raiders().entry.entrySpots())
        {
            int x = data.transformGX(c.c().x() * C.TILE_SIZE);
            int y = data.transformGY(c.c().y() * C.TILE_SIZE);

            UI.icons().s.alert.renderScaled(ren, x, y, C.SCALE);
        }

        COLOR.unbind();
    }

    public abstract bool is(Region reg);

    public abstract double value(Region reg);

    protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        COLOR.ORANGE100.bind();

        if (WORLD.PATH().map.is.is(it.tile()))
        {
            foreach (DIR d in DIR.ALL)
            {
                if (WORLD.PATH().map.can(it.tile(), d))
                {
                    SPRITES.cons().ICO.arrows2.get(d.id()).render(r, it.x(), it.y());
                }
            }
        }

        COLOR.unbind();
    }

    protected override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        int m = 0x0F;
        Region reg = WORLD.REGIONS().map.get(it.tile());
        if (WORLD.REGIONS().border().is(it.tile()))
        {
            m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (!WORLD.IN_BOUNDS(it.tx(), it.ty(), d) || reg == WORLD.REGIONS().map.get(it.tx(), it.ty(), d))
                {
                    m |= d.mask();
                }
            }
        }

        if (reg != null && is(reg))
        {
            double v = value(reg);
            if (inv)
                v = 1 - v;
            if (v > 0)
                ColorImp.TMP.interpolate(GCOLOR.MAP().F_NEUTRAL, GCOLOR.MAP().F_ENEMY, v).bind();
            else
                GCOLOR.MAP().F_ALLY.bind();
        }
        else
        {
            GCOLOR.MAP().F_REBEL.bind();
        }

        renderUnder(m, r, it);
    }
}