using System;
using System.Collections.Generic;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;
using util.text;
using world;
using world.map.regions;

namespace world.overlay
{
    internal class OverlayPathing : WorldOverlays.OverlayTileNormal
    {
        private static CharSequence ¤¤name = "¤Paths";
        private static CharSequence ¤¤desc = "¤show available paths";

        static OverlayPathing()
        {
            D.ts(typeof(OverlayPathing));
        }

        public OverlayPathing() : base(¤¤name, ¤¤desc, true, true)
        {
        }

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
            GCOLOR.MAP().F_REBEL.bind();
            renderUnder(m, r, it);
        }
    }
}