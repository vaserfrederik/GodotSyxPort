using System;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering.RenderData;
using world;
using world.overlay;

class PlacerOverlay : OverlayTile
{
    WorldLandmark hovered = null;

    public PlacerOverlay() : base(true, false)
    {
        // TODO Auto-generated constructor stub
    }

    protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        WorldLandmark l = WORLD.LANDMARKS().setter.get(it.tile());
        if (l != null)
        {
            COLOR c = l == hovered ? GCOLOR.MAP().BEST : COLOR.WHITE35;
            c.bind();
            int m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (WORLD.LANDMARKS().setter.get(it.tx(), it.ty(), d) == l)
                    m |= d.mask();
            }
            SPRITES.cons().BIG.dashed.render(r, m, it.x(), it.y());
        }
    }
}