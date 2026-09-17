using System.IO;
using snake2d;
using util.spritecomposer;

namespace settlement.thing.halfEntity.dingy
{
    final class Sprite
    {
        private readonly TILE_SHEET sheetCart;

        Sprite() : this(init.paths.PATHS.SETT().sprite.getFolder("thing").get("BOAT_DINGY"), 164, 158)
        {
        }

        private Sprite(string path, int width, int height) : this(new ITileSheet(path, width, height))
        {
        }

        private Sprite(ITileSheet tileSheet)
        {
            sheetCart = tileSheet.get();
        }

        public void render(SPRITE_RENDERER r, ShadowBatch s, int rot, int x, int y, int frame, int upgrade)
        {
            int i = rot;
            i += (frame & 1) * 8;
            i += 16 * (upgrade & 1);

            sheetCart.render(r, i, x, y);
            s.setHeight(4).setDistance2Ground(0);
            sheetCart.render(s, i, x, y);
        }
    }
}