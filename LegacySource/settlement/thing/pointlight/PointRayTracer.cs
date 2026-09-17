using System;
using System.Collections.Generic;

namespace Settlement.Thing.PointLight
{
    using static Settlement.Main.SETT.IN_BOUNDS;

    using Init.Constant;
    using Settlement.Main;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Sets;

    class PointRayTracer
    {
        private readonly Bitmap1D lit;
        private readonly Bitsmap1D corners;
        private readonly int tileDiameter;

        public PointRayTracer(int tileDiameter)
        {
            this.tileDiameter = tileDiameter;
            lit = new Bitmap1D(tileDiameter * tileDiameter, false);
            corners = new Bitsmap1D(0, 4, tileDiameter * tileDiameter);
        }

        public bool LitIs(int tx, int ty)
        {
            return (corners.Get(tx + ty * tileDiameter)) != 0;
        }

        private bool Lit(int tx, int ty)
        {
            return lit.Get(tx + ty * tileDiameter);
        }

        private bool Lit(int tx, int ty, DIR d)
        {
            return Lit(tx + d.X(), ty + d.Y());
        }

        protected void Init(int x, int y)
        {
            int ctx = x >> C.T_SCROLL;
            int cty = y >> C.T_SCROLL;

            if (!ShouldBeLit(ctx, cty))
            {
                return;
            }

            lit.Clear();
            lit.Set(tileDiameter / 2 + tileDiameter * tileDiameter / 2, true);
            int tx1 = ctx - tileDiameter / 2;
            int ty1 = cty - tileDiameter / 2;

            for (int gy = 0; gy <= tileDiameter; gy++)
            {
                RayTrace(-tx1, -ty1, ctx, cty, tx1, ty1 + gy);
                RayTrace(-tx1, -ty1, ctx, cty, tx1 + tileDiameter - 1, ty1 + gy);
            }

            for (int gx = 1; gx <= tileDiameter - 1; gx++)
            {
                RayTrace(-tx1, -ty1, ctx, cty, tx1 + gx, ty1);
                RayTrace(-tx1, -ty1, ctx, cty, tx1 + gx, ty1 + tileDiameter - 1);
            }

            corners.Clear();

            for (int ty = 1; ty <= tileDiameter - 1; ty++)
            {
                for (int tx = 1; tx <= tileDiameter - 1; tx++)
                {
                    int t = tx + ty * tileDiameter;
                    if (lit.Get(t))
                    {
                        corners.Set(t, 0x0F);
                    }
                    else
                    {
                        foreach (DIR d in DIR.NORTHO)
                        {
                            if (!CornerCheck(tx, ty, d, tx1, ty1, d))
                                if (!CornerCheck(tx, ty, d.Next(1), tx1, ty1, d))
                                    CornerCheck(tx, ty, d.Next(-1), tx1, ty1, d);
                        }
                    }
                }
            }
        }

        private bool CornerCheck(int tx, int ty, DIR d, int tx1, int ty1, DIR dir)
        {
            if (Lit(tx, ty, d))
            {
                int fx = tx1 + tx;
                int fy = ty1 + ty;
                int tox = tx1 + tx + d.X();
                int toy = ty1 + ty + d.Y();

                LOS from = SETT.LIGHTS().los().Get(fx, fy);
                LOS to = SETT.LIGHTS().los().Get(tox, toy);
                if (from.PassesToOtherFromThis(fx, fy, tox, toy) && to.PassesFromOtherToThis(fx, fy, tox, toy))
                {
                    int t = tx + ty * tileDiameter;
                    int c = corners.Get(t);
                    c |= dir.Mask();
                    corners.Set(t, c);
                    return true;
                }
            }
            return false;
        }

        private void RayTrace(int mx, int my, int fromx, int fromy, int tox, int toy)
        {
            double divider;
            if (Math.Abs(tox - fromx) > Math.Abs(toy - fromy))
            {
                divider = Math.Abs(tox - fromx);
            }
            else if (Math.Abs(tox - fromx) < Math.Abs(toy - fromy))
            {
                divider = Math.Abs(toy - fromy);
            }
            else
            {
                divider = Math.Abs(tox - fromx);
            }

            double dx = (tox - fromx) / divider;
            double dy = (toy - fromy) / divider;

            double x = fromx + 0.5;
            double y = fromy + 0.5;

            for (int i = 0; i < divider; i++)
            {
                int otx = (int)x;
                int oty = (int)y;
                lit.Set(otx + mx + (oty + my) * tileDiameter, true);
                LOS lFrom = SETT.LIGHTS().los().Get(otx, oty);
                x += dx;
                y += dy;
                int tx = (int)x;
                int ty = (int)y;

                LOS lTo = SETT.LIGHTS().los().Get(tx, ty);

                if (!lFrom.PassesToOtherFromThis(otx, oty, tx, ty) || !lTo.PassesFromOtherToThis(otx, oty, tx, ty))
                {
                    return;
                }
            }
        }

        public byte GetSide(int tx, int ty, DIR d)
        {
            return (corners.Get(tx + ty * tileDiameter) & d.Mask()) != 0 ? byte.MaxValue : 0;
        }

        private bool ShouldBeLit(int x, int y)
        {
            return IN_BOUNDS(x, y);
        }
    }
}