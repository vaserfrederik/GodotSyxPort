using System;
using snake2d;
using view.tool;
using settlement.main;
using init.sprite;

namespace settlement.misc.placers
{
    class Placer : PlacableFixedImp
    {
        private readonly string name;
        private readonly TileGrid grid;

        public Placer(string name, TileGrid grid) : base(name, 1, 1)
        {
            this.name = name;
            this.grid = grid;
        }

        public override SPRITE getIcon()
        {
            return SPRITES.icons().m.cancel;
        }

        public override string name()
        {
            return name;
        }

        public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, int rx, int ry, bool isPlacable, bool areaIsPlacable)
        {
            grid.get(rx, ry).sprite(grid, rx, ry, mask).render(r, x, y);
        }

        public override int width()
        {
            return grid.width();
        }

        public override int height()
        {
            return grid.height();
        }

        public override string placable(int tx, int ty, int rx, int ry)
        {
            return SETT.IN_BOUNDS(tx, ty) && grid.get(rx, ry).placable(tx, ty, grid, rx, ry) ? null : E;
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            grid.get(rx, ry).place(tx, ty, grid, rx, ry);
        }
    }
}