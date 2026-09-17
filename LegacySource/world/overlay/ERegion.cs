using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.MATH;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.GUTIL;
using util.rendering;
using view.main;
using world.map.pathing;
using world.map.regions;

namespace world.overlay
{
    public sealed class ERegion
    {
        private Region hovered;
        private readonly COLOR cNone = new ColorImp(100, 100, 100);
        private readonly ColorImp col = new ColorImp();
        private double shade;

        private readonly WRegFinder fin = new WRegFinder();

        public void Add(Region r)
        {
            if (r != null)
            {
                hovered = r;
            }
        }

        public void RenderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            if (hovered == null)
                return;
            RenderAbove(hovered, r, s, data);
            RenderPath(hovered, r, s, data);
            hovered = null;
            WORLD.OVERLAY().things.Render(r, s, data);
        }

        public void RenderAbove(Region hovered, Renderer r, ShadowBatch s, RenderData data)
        {
            s.SetHeightUI(6);
            s.SetDistance2GroundUI(10);
            s.SetHard();
            shade = VIEW.RenderSecond();
            shade = MATH.Mod(shade, 2);
            shade = MATH.DistanceC(shade, 1, 2);
            if (hovered.Realm() == null)
                col.Set(cNone);
            else
                col.Set(hovered.Faction().Banner().ColorBG());
            col.ShadeSelf(0.5 + shade);
            col.Bind();
            foreach (COORDINATE c in hovered.Info.Bounds())
            {
                if (hovered.Is(c))
                {
                    int m = 0;
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (hovered.Is(c, d) || !WORLD.IN_BOUNDS(c, d))
                        {
                            m |= d.Mask();
                        }
                    }
                    if (m != 0x0F)
                    {
                        int x = data.TransformGX(c.X() * C.TILE_SIZE);
                        int y = data.TransformGY(c.Y() * C.TILE_SIZE);
                        SPRITES.cons().BIG.dashed_hollow.Render(r, m, x, y);
                        SPRITES.cons().BIG.dashed_hollow.Render(s, m, x, y);
                    }
                }
            }
            s.SetPrev();
        }

        private Bitmap1D cc = new Bitmap1D(WREGIONS.MAX, false);

        public void RenderPath(Region reg, Renderer r, ShadowBatch s, RenderData data)
        {
            List<RegDist> regs = fin.All(reg, Treaty.REG_NEIGHS, WRegSel.DUMMY());

            cc.Clear();
            foreach (RegDist d in regs)
            {
                cc.Set(d.Reg.Index(), true);
            }
            cc.Set(reg.Index(), true);
            Flooder f = GUTIL.Flooder();
            f.Init(this);
            f.PushSloppy(reg.Cx(), reg.Cy(), 0);
            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                Region rr = WORLD.PATH().RegMap.Get(t);

                if (rr != null && !cc.Get(rr.Index()))
                    continue;
                if (rr != reg && rr != null && t.IsSameAs(rr.Cx(), rr.Cy()))
                {
                    cc.Set(rr.Index(), false);
                    Render(r, s, data, t);
                    continue;
                }

                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.PATH().Map.Can(t, d) && (rr == null || rr == reg || rr == WORLD.PATH().RegMap.Get(t, d)))
                    {
                        GUTIL.Flooder().PushSmaller(t, d, t.GetValue() + WPATHING.Cost(t.X(), t.Y(), d) * d.TileDistance(), t);
                    }
                }
            }

            GUTIL.Flooder().Done();
            COLOR.Unbind();
        }

        private void Render(Renderer r, ShadowBatch s, RenderData data, PathTile t)
        {
            Region reg = WORLD.REGIONS().Map.Get(t);

            if (reg != null)
                WORLD.OVERLAY().HoverBox(reg);
            if (t.Parent == null)
                return;
            PathTile prev = t;
            t = t.Parent;
            while (t.Parent != null)
            {
                DIR d = DIR.Get(t, prev);
                int dd = C.TILE_SIZE / 2;
                int x = data.TransformGX(t.X() * C.TILE_SIZE);
                int y = data.TransformGY(t.Y() * C.TILE_SIZE);

                for (int i = 0; i < 2; i++)
                {
                    SPRITES.cons().ICO.arrows2.Get(d.Id()).Render(r, x + d.X() * dd * i, y + d.Y() * dd * i);
                    SPRITES.cons().ICO.arrows2.Get(d.Id()).Render(s, x + d.X() * dd * i, y + d.Y() * dd * i);
                }

                prev = t;
                t = t.Parent;
            }
        }
    }
}