using System;
using Init.Type;
using Snake2D;
using Util.Rendering;
using World;

namespace World.Overlay
{
    class OverlayClimate : WorldOverlays.OverlayTileNormal
    {
        public OverlayClimate() : base(CLIMATES.INFO().name, "", true, true)
        {
        }

        protected override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            CLIMATE cl = WORLD.TERRAIN().climate.getter.get(it.tile());
            int m = 0x0F;

            COLOR c = cl.color;
            c.bind();
            renderUnder(m, r, it);
        }

        public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            WORLD.OVERLAY().regNames.renderAbove(r, s, data);
        }
    }
}