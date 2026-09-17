using System;
using System.Collections.Generic;
using init.sprite;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;
using util.rendering;
using world;
using world.map.landmark;

namespace world.overlay
{
    public sealed class OverlayExplore : WorldOverlays.OverlayTile
    {
        private WorldLandmark hovered;

        public OverlayExplore() : base(true, false)
        {
        }

        public void Hover(WorldLandmark m)
        {
            hovered = m;
        }

        public override void Add()
        {
            base.Add();
        }

        protected override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            WorldLandmark a = WORLD.LANDMARKS().setter.Get(it.Tile());

            if (a == null)
                return;

            if (a == hovered)
            {
                COLOR.WHITE2WHITE.Bind();

                int m = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if (WORLD.LANDMARKS().setter.Get(it.Tx(), it.Ty(), d) == a)
                        m |= d.Mask();
                }

                if (m != 0x0F)
                {
                    SPRITES.cons().BIG.dashed_hollow.Render(r, m, it.X(), it.Y());
                }
            }
            else if (a != null && a.textSize != 0 && it.Tx() == a.cx && it.Ty() == a.cy)
            {
                COLOR.WHITE85.Bind();
                Font f = UI.FONT().H2;
                int scale = 2 + CORE.renderer().GetZoomout();
                int w = f.Width(a.name, 0, a.name.Length, scale);
                s.SetHeight(0).SetDistance2Ground(0);
                COLOR.WHITE100.Render(s, it.X() - w / 2 - 8, it.X() + w / 2 + 8, it.Y() - 8, it.Y() + 8 + f.Height() * scale);

                f.RenderCX(r, it.X(), it.Y(), a.name, scale);
                // f.RenderCX(s, it.X(), it.Y(), a.name, scale);
            }
        }

        public override void RenderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            WORLD.OVERLAY().minerals.RenderAbove(r, s, data);
            base.RenderAbove(r, s, data);
            hovered = null;
        }
    }
}