using System;
using System.IO;
using init.constant;
using init.paths;
using snake2d;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.sprite.UI
{
    public class UIImage : SPRITE
    {
        private const int TILE_SIZE = 32 * C.SCALE_NORMAL;
        private readonly int tilesX;
        private readonly int tilesY;
        private readonly int width;
        private readonly int height;

        private readonly TILE_SHEET sheet;

        public UIImage(TILE_SHEET sheet, int tilesX, int tilesY) : this(sheet, tilesX, tilesY, null) { }

        public UIImage(TILE_SHEET sheet, int tilesX, int tilesY, IInit init)
        {
            this.sheet = sheet;
            this.tilesX = tilesX;
            this.tilesY = tilesY;
            width = tilesX * TILE_SIZE;
            height = tilesY * TILE_SIZE;

            if (init == null)
            {
                init = new IInit(PATHS.SPRITE_UI().get("LoadScreen"), 1368, 268);
            }

            sheet = new ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, 0, 1, 1, tilesX, tilesY, d.s32);
                    s.full.paste(true);
                    return d.s32.saveNormal();
                }
            }.get();
        }

        public override int width()
        {
            return width;
        }

        public override int height()
        {
            return height;
        }

        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            int startX = X1;
            int tile = 0;
            for (int ty = 0; ty < tilesY; ty++)
            {
                X1 = startX;
                for (int tx = 0; tx < tilesX; tx++)
                {
                    sheet.render(r, tile, X1, Y1);
                    X1 += TILE_SIZE;
                    tile++;
                }
                Y1 += TILE_SIZE;
            }
        }

        public override void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
        {
            throw new NotImplementedException();
        }
    }
}