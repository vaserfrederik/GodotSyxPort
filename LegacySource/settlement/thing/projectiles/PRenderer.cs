using System;
using System.Collections.Generic;
using init.constant;
using settlement.main;
using settlement.thing.projectiles;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.rendering;

namespace Settlement.Thing.Projectiles
{
    internal class PRenderer
    {
        private readonly SProjectiles p;

        public PRenderer(SProjectiles p)
        {
            this.p = p;
        }

        private readonly ArrayListInt tmp = new ArrayListInt(2 * 2056);
        private readonly Rec rec = new Rec();

        public void RenderAbove(Renderer r, ShadowBatch s, float ds, int zoomout, RenderData renData)
        {
            r.NewLayer(false, zoomout);

            SETT.WEATHER().Apply(renData.AbsBounds());
            tmp.Clear();
            int min = C.TILE_SIZE;
            rec.Set(renData.GBounds().x1() - min, renData.GBounds().x2() + min, renData.GBounds().y1() - min, renData.GBounds().y2() + min);
            p.Map.Fill(rec, tmp);

            int offX = renData.OffX1();
            int offY = renData.OffY1();

            for (int i = 0; i < p.Data.Last(); i++)
            {
                Data d = p.Data.Data(i);
                double x = d.X() - offX;
                double y = d.Y() - offY;
                int h = (int)d.Z();
                Projectile pr = p.Data.Type(i);
                double @ref = p.Data.Ref(i);
                pr.Sprite().RenderProj(pr, @ref, r, s, x, y, h, i, d.SpeedX(), d.SpeedY(), d.Dz(), ds, zoomout);
            }
            COLOR.Unbind();
        }
    }
}