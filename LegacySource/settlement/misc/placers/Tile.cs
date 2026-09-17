using snake2d.util.sprite;

namespace settlement.misc.placers
{
    interface Tile
    {
        bool Placable(int tx, int ty, TileGrid grid, int rx, int ry);
        void Place(int tx, int ty, TileGrid grid, int rx, int ry);
        SPRITE Sprite(TileGrid grid, int rx, int ry, int mask);
    }
}