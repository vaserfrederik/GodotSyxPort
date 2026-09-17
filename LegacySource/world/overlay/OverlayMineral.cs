using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.rendering;
using util.text;
using world;
using world.map.regions;
using world.region;
using world.region.building;

namespace world.overlay
{
    class OverlayMineral : WorldOverlays.OverlayTileNormal
    {
        private static CharSequence ¤¤name = "¤Minerals";
        private static CharSequence ¤¤desc = "¤Shows the location of minerals.";

        static
        {
            D.ts(typeof(OverlayMineral));
        }

        public OverlayMineral() : base(¤¤name, ¤¤desc, true, true)
        {
        }

        public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            base.renderAbove(r, s, data);

            int size = C.TILE_SIZE;

            foreach (Region reg in WORLD.REGIONS().active())
            {
                foreach (DIR dir in DIR.NORTHO)
                {
                    if (data.tBounds().holdsPoint(reg.cx() + dir.x() * 5, reg.cy() + dir.y() * 5))
                    {
                        int am = 0;

                        foreach (RDBuilding b in RD.BUILDINGS().all)
                        {
                            if (BUtil.value(b.baseFactors, reg) > 1)
                            {
                                am++;
                            }
                        }

                        int x1 = data.transformGX((reg.cx() * C.TILE_SIZE + C.TILE_SIZEH) - am * size / 2);
                        int y1 = data.transformGY((reg.cy() + 2) * C.TILE_SIZE);

                        foreach (RDBuilding b in RD.BUILDINGS().all)
                        {
                            if (BUtil.value(b.baseFactors, reg) > 1)
                            {
                                int ss = (int)(C.TILE_SIZE);
                                int d = (size - ss) / 2;
                                COLOR.BLACK.bind();
                                b.icon().render(r, x1 + d + 8, x1 + d + ss + 8, y1 + d + 8, y1 + d + ss + 8);
                                COLOR.unbind();
                                b.icon().render(r, x1 + d, x1 + d + ss, y1 + d, y1 + d + ss);

                                x1 += size;
                            }
                        }

                        break;
                    }
                }
            }
        }

        public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            //Minable m = WORLD.MINERALS().get(it.tile());
            //if (m != null)
            //{
            //    render(r, COLOR.WHITE100, it.x(), it.y(), 8);
            //    render(r, COLOR.WHITE10, it.x() + 2, it.y() + 2, 6);
            //    render(r, COLOR.WHITE30, it.x(), it.y(), 4);
            //    m.resource.icon().render(r, it.x(), it.x() + C.TILE_SIZE, it.y(), it.y() + C.TILE_SIZE);
            //}
        }
    }
}