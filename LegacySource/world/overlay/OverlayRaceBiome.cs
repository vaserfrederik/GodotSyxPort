using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Overlay
{
    using Init.Constant;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Util.Colors;
    using Util.Rendering;
    using Util.Text;
    using World;
    using World.Map.Regions;
    using World.Region;
    using World.Region.Pop;

    class OverlayRaceBiome : WorldOverlays.OverlayTileNormal
    {
        private static readonly string ¤¤name = "¤Species Biome";
        private static readonly string ¤¤desc = "¤Shows each region's base capacity for species.";

        static OverlayRaceBiome()
        {
            D.ts(typeof(OverlayRaceBiome));
        }

        public OverlayRaceBiome() : base(¤¤name, ¤¤desc, true, true)
        {
        }

        private Region compReg;
        private readonly IComparer<RDRace> comp = new RaceComparer();
        private RDRace[] all = new RDRace[RD.RACES().All.Size];

        public override void RenderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            base.RenderAbove(r, s, data);

            int scale = 1;
            int size = C.TILE_SIZE * scale;

            foreach (var reg in WORLD.REGIONS().Active)
            {
                foreach (var dir in DIR.NORTHO)
                {
                    if (data.TBounds().HoldsPoint(reg.Cx() + dir.X() * 5, reg.Cy() + dir.Y() * 5))
                    {
                        int am = RD.RACES().All.Size;
                        double tot = 0;

                        foreach (var rr in RD.RACES().All)
                        {
                            tot += rr.Pop.Biome.Get(reg);
                            all[rr.Index()] = rr;
                        }

                        compReg = reg;
                        Array.Sort(all, comp);

                        int x1 = data.TransformGX((reg.Cx() * C.TILE_SIZE + C.TILE_SIZEH) - am * size / 2);
                        int y1 = data.TransformGY((reg.Cy() + 2) * C.TILE_SIZE);

                        foreach (var rr in all)
                        {
                            double sc = am * scale * rr.Pop.Biome.Get(reg) / tot;
                            int ss = (int)Math.Ceiling(C.TILE_SIZE * sc);
                            int d = (size - ss) / 2;
                            COLOR.BLACK.Bind();
                            rr.Race.Appearance().Icon.Render(r, x1 + d + 8, x1 + d + ss + 8, y1 + d + 8, y1 + d + ss + 8);
                            COLOR.Unbind();
                            rr.Race.Appearance().Icon.Render(r, x1 + d, x1 + d + ss, y1 + d, y1 + d + ss);
                            x1 += ss;
                        }

                        break;
                    }
                }
            }
        }

        protected override void RenderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            int m = 0x0F;
            Region reg = WORLD.REGIONS().Map.Get(it.Tile());
            if (WORLD.REGIONS().Border().Is(it.Tile()))
            {
                m = 0;
                foreach (var d in DIR.ORTHO)
                {
                    if (!WORLD.IN_BOUNDS(it.Tx(), it.Ty(), d) || reg == WORLD.REGIONS().Map.Get(it.Tx(), it.Ty(), d))
                    {
                        m |= d.Mask();
                    }
                }
            }

            COLOR c = (reg == null || reg.Faction() == null) ? GCOLOR.MAP().F_REBEL : reg.Faction().Banner().ColorBG();
            c.Bind();
            RenderUnder(m, r, it);
        }

        public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            //Minable m = WORLD.MINERALS().Get(it.Tile());
            //if (m != null)
            //{
            //    render(r, COLOR.WHITE100, it.X(), it.Y(), 8);
            //    render(r, COLOR.WHITE10, it.X() + 2, it.Y() + 2, 6);
            //    render(r, COLOR.WHITE30, it.X(), it.Y(), 4);
            //    m.Resource.Icon().Render(r, it.X(), it.X() + C.TILE_SIZE, it.Y(), it.Y() + C.TILE_SIZE);
            //}
        }

        private class RaceComparer : IComparer<RDRace>
        {
            public int Compare(RDRace o1, RDRace o2)
            {
                return o2.Pop.Biome.Get(OverlayRaceBiome.this.compReg).CompareTo(o1.Pop.Biome.Get(OverlayRaceBiome.this.compReg));
            }
        }
    }
}