using System;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.rendering;
using view.main;
using world.map.pathing;

namespace world.overlay
{
    public sealed class EPath
    {
        private readonly double dist = 4;

        private Treaty treaty;

        private WPath path = new WPath
        {
            Treaty = () => treaty
        };
        private Coo start = new Coo();
        private Coo end = new Coo();
        private bool newPath = true;
        private readonly Rec rr = new Rec(C.TILE_SIZE);
        private bool added = false;

        public EPath()
        {
        }

        public void Render(Renderer r, ShadowBatch s, RenderData data)
        {
            if (!added)
                return;

            added = false;

            path.Find(start.x(), start.y(), end.x(), end.y());
            if (!path.IsValid())
                return;

            if (newPath)
            {
                newPath = false;
            }

            COLOR.WHITE100.Bind();

            double move = VIEW.renderSecond() % dist;

            int prevx = path.X() * C.TILE_SIZE;
            int prevy = path.Y() * C.TILE_SIZE;
            if (!path.SetNext())
                return;

            do
            {
                int x = path.X() * C.TILE_SIZE;
                int y = path.Y() * C.TILE_SIZE;

                double md = 0.5 / (path.Dir().TileDistance() * WPATHING.MovementSpeed(path.X(), path.Y()));

                move -= md;

                if (move < 0)
                {
                    double dd = move / md;
                    move += dist;
                    int dx = (int)((x - prevx) * dd);
                    int dy = (int)((y - prevy) * dd);

                    DIR d = DIR.Get(prevx, prevy, x, y);
                    COLOR.YELLOW100.Bind();
                    SPRITES.cons().ICO.arrows2.Get(d.Id()).Render(r, data.transformGX(x + dx), data.transformGY(y + dy));
                    // SPRITES.cons().BIG.outline.Render(r, 0, data.transformGX(x + dx), data.transformGY(y + dy));
                }

                prevx = x;
                prevy = y;

                COLOR.WHITE100.Bind();
                x = data.transformGX(x);
                y = data.transformGY(y);
                SPRITES.cons().BIG.line.Render(r, 0, x, y);
            } while (path.SetNext());
        }

        public void Add(int sx, int sy, int dx, int dy, Treaty treaty)
        {
            newPath |= start.Set(sx, sy);
            newPath |= end.Set(dx, dy);
            this.treaty = treaty;
            if (newPath)
            {
                path.Find(start.x(), start.y(), end.x(), end.y());
                rr.MoveC(sx * C.TILE_SIZE + C.TILE_SIZEH, sy * C.TILE_SIZE + C.TILE_SIZEH);
            }
            added = true;
        }
    }
}