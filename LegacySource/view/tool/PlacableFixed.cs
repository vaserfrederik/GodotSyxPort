using System;
using util.gui.misc;
using view.subview;

namespace view.tool
{
    public abstract class PlacableFixed : PLACABLE
    {
        private int rot, size;

        public PlacableFixed()
        {
        }

        public abstract int rotations();
        public abstract int sizes();

        public void rotSet(int rot)
        {
            this.rot = CLAMP.i(rot, 0, rotations() - 1);
        }

        public final void sizeSet(int size)
        {
            this.size = CLAMP.i(size, 0, sizes() - 1);
        }

        public int rot()
        {
            return CLAMP.i(rot, 0, rotations() - 1);
        }

        public final int size()
        {
            return CLAMP.i(size, 0, sizes() - 1);
        }

        public abstract int width();
        public abstract int height();

        public abstract string placable(int tx, int ty, int rx, int ry);
        public abstract string placableWhole(int tx1, int ty1);

        public abstract void place(int tx, int ty, int rx, int ry);
        public void afterPlaced(int tx1, int ty1)
        {
        }

        public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, bool isPlacable, bool areaIsPlacable)
        {
            SPRITES.cons().BIG.solid.render(r, mask, x, y);
        }

        public void placeInfo(GBox b, int x1, int y1)
        {
            b.add(b.text().add(width()).add('x').add(height()));
        }

        public void init(int cx, int cy)
        {
        }

        public void updateRegardless(GameWindow window)
        {
        }
    }
}