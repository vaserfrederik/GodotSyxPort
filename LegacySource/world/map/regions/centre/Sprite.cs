using System;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util;
using world;
using world.map.regions;

namespace world.map.regions.centre
{
    public sealed class Sprite
    {
        private readonly CSprite sprite = new CSprite();

        private readonly ArrayCooShort centres = new ArrayCooShort(128);
        private readonly Rec tmp = new Rec();
        private readonly int from = -WCentre.TILE_DIM / 2;
        private readonly int to = WCentre.TILE_DIM + from;

        public Sprite()
        {
        }

        public void RenderGround(WRenContext data)
        {
            int last = centres.GetI();

            for (int i = 0; i < last; i++)
            {
                COORDINATE cen = centres.Set(i);
                Region reg = WORLD.REGIONS().Map.Get(cen);

                for (int dty = from; dty <= to; dty++)
                {
                    for (int dtx = from; dtx <= to; dtx++)
                    {
                        int tx = cen.X + dtx;
                        int ty = cen.Y + dty;

                        int x = data.Data.TransformGX(tx * C.TILE_SIZE);
                        int y = data.Data.TransformGY(ty * C.TILE_SIZE);
                        sprite.RenderOnGround(data, dtx - from, dty - from, reg, GUTIL.Ran1().Get(tx, ty), x, y);
                    }
                }
            }

            centres.Set(last);
        }

        public void RenderAbove(WRenContext data)
        {
            int last = centres.GetI();

            for (int i = 0; i < last; i++)
            {
                COORDINATE cen = centres.Set(i);
                Region reg = WORLD.REGIONS().Map.Get(cen);
                for (int dty = from; dty <= to; dty++)
                {
                    for (int dtx = from; dtx <= to; dtx++)
                    {
                        int tx = cen.X + dtx;
                        int ty = cen.Y + dty;

                        int x = data.Data.TransformGX(tx * C.TILE_SIZE);
                        int y = data.Data.TransformGY(ty * C.TILE_SIZE);

                        sprite.RenderAboveB(data, dtx - from, dty - from, reg, GUTIL.Ran1().Get(tx, ty), x, y);
                    }
                }
            }

            CORE.Renderer().NewLayer(false, CORE.Renderer().GetZoomout());

            for (int i = 0; i < last; i++)
            {
                COORDINATE cen = centres.Set(i);
                Region reg = WORLD.REGIONS().Map.Get(cen);
                for (int dty = from; dty <= to; dty++)
                {
                    for (int dtx = from; dtx <= to; dtx++)
                    {
                        int tx = cen.X + dtx;
                        int ty = cen.Y + dty;

                        int x = data.Data.TransformGX(tx * C.TILE_SIZE);
                        int y = data.Data.TransformGY(ty * C.TILE_SIZE);

                        sprite.RenderAboveA(data, dtx - from, dty - from, reg, GUTIL.Ran1().Get(tx, ty), x, y);
                    }
                }
            }

            centres.Set(last);
        }

        public void RenderAboveTerrain(WRenContext data)
        {
            tmp.SetDim(data.Data.TBounds().Width + WCentre.TILE_DIM * 2 + 4, data.Data.TBounds().Height + WCentre.TILE_DIM * 2 + 4);
            tmp.MoveC(data.Data.TBounds().CX(), data.Data.TBounds().CY());
            centres.Set(0);
            foreach (Region reg in WORLD.REGIONS().Active())
            {
                if (reg.CX >= 0 && tmp.HoldsPoint(reg.CX, reg.CY))
                {
                    centres.Get().Set(reg.CX, reg.CY);
                    if (centres.GetI() >= centres.Size - 1)
                        continue;
                    centres.Inc();
                }
            }

            int last = centres.GetI();

            for (int i = 0; i < last; i++)
            {
                COORDINATE cen = centres.Set(i);
                Region reg = WORLD.REGIONS().Map.Get(cen);
                for (int dty = from; dty <= to; dty++)
                {
                    for (int dtx = from; dtx <= to; dtx++)
                    {
                        int tx = cen.X + dtx;
                        int ty = cen.Y + dty;

                        int x = data.Data.TransformGX(tx * C.TILE_SIZE);
                        int y = data.Data.TransformGY(ty * C.TILE_SIZE);

                        sprite.RenderAboveTerrain(data, dtx - from, dty - from, reg, GUTIL.Ran1().Get(tx, ty), x, y);
                    }
                }
            }

            centres.Set(last);
        }
    }
}