using System;
using init.sprite;
using settlement.job;
using snake2d;
using snake2d.util.datatypes;
using util.colors;
using util.gui.misc;
using view.tool;

namespace view.sett.ui.room.copy
{
    final class Second : PlacableFixed
    {
        private readonly Dest dest;
        private readonly RoomChecker room;

        public Second(Dest dest)
        {
            this.dest = dest;
            room = new RoomChecker(dest);
        }

        public override void init(int cx, int cy)
        {
            dest.init(cx, cy, rot());
            room.init();
        }

        public override int width()
        {
            return dest.body().width();
        }

        public override int height()
        {
            return dest.body().height();
        }

        public override CharSequence placable(int tx, int ty, int rx, int ry)
        {
            return null;
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            if (!dest.is(tx, ty))
                return;
            if (dest.sourceIs(tx, ty))
                return;

            if (room.place(tx, ty))
            {
            }
            else
            {
                COORDINATE s = dest.transform(tx, ty);
                Job j = Jobs.get(s.x(), s.y());
                if (j != null && j is JobBuildRoad && JobBuildRoad.problem(tx, ty) == null)
                    j.placer().place(tx, ty, null, null);
                else if (j != null && j.placer().isPlacable(tx, ty, null, null) == null)
                {
                    j.placer().place(tx, ty, null, null);
                }
            }
        }

        public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, bool isPlacable, bool areaIsPlacable)
        {
            if (blocked(tx, ty))
            {
                blockedD(tx, ty);
            }

            if (blocked(tx, ty))
                GCOLOR.MAP().BAD.bind();
            else if (room.isPartOfBlocked(tx, ty))
                GCOLOR.MAP().SOSO.bind();
            else
                GCOLOR.MAP().BEST.bind();

            if (!dest.is(tx, ty))
                return;
            if (dest.blocking(tx, ty))
                SPRITES.cons().BIG.filled.render(r, 0, x, y);
            else
                SPRITES.cons().BIG.dashed.render(r, 0x0F, x, y);

            mask = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (dest.is(tx, ty, d))
                    mask |= d.mask();
            }
            if (mask != 0)
                SPRITES.cons().BIG.outline.render(r, mask, x, y);
        }

        public override SPRITE getIcon()
        {
            return null;
        }

        public override CharSequence name()
        {
            return E;
        }

        public override PLACABLE getUndo()
        {
            return null;
        }

        public override int rotations()
        {
            return 4;
        }

        public override int sizes()
        {
            return 1;
        }

        public override CharSequence placableWhole(int tx1, int ty1)
        {
            // TODO Auto-generated method stub
            return null;
        }

        private bool blocked(int tx, int ty)
        {
            if (dest.sourceIs(tx, ty))
                return true;

            if (room.isBlocked(tx, ty))
                return true;
            COORDINATE s = dest.transform(tx, ty);
            Job j = Jobs.get(s.x(), s.y());
            if (j != null && j.placer() == null)
                LOG.ln(j);
            if (j != null && j.placer().isPlacable(tx, ty, null, null) != null)
                return true;

            return false;
        }

        private bool blockedD(int tx, int ty)
        {
            if (room.isBlocked(tx, ty))
                return true;
            COORDINATE s = dest.transform(tx, ty);
            Job j = Jobs.get(s.x(), s.y());
            if (j != null && j.placer() == null)
                LOG.ln(j);
            if (j != null && j.placer().isPlacable(tx, ty, null, null) != null)
                return true;

            return false;
        }

        public override void placeInfo(GBox b, int x1, int y1)
        {
            b.add(b.text().add(width() - 1).add('x').add(height() - 1));
        }
    }
}