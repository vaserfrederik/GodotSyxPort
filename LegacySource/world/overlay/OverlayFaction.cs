using System;
using snake2d;
using snake2d.Renderer;
using snake2d.SPRITE_RENDERER;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;
using util.text;
using world;
using world.map.regions;

namespace world.overlay
{
    class OverlayFaction : WorldOverlays.OverlayTileNormal
    {
        private static readonly CharSequence ¤¤name = "¤Factions";
        private static readonly CharSequence ¤¤desc = "¤Shows a clear view of factions.";

        static OverlayFaction()
        {
            D.ts(typeof(OverlayFaction));
        }

        public OverlayFaction() : base(¤¤name, ¤¤desc, true, true)
        {
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

            COLOR c = (reg == null || reg.faction() == null) ? GCOLOR.MAP().F_REBEL : reg.faction().banner().colorBG();
            c.bind();
            renderUnder(m, r, it);
        }

        public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            WORLD.OVERLAY().regNames.renderAbove(r, s, data);
        }
    }
}