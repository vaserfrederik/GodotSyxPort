using System;
using System.IO;
using init.paths;
using snake2d.util.sprite;
using util.spritecomposer;

namespace world
{
    public class Sprites
    {
        private readonly PATH path = PATHS.SPRITE().getFolder("world").getFolder("map");

        public readonly TILE_SHEET edge = (new ITileSheet(path.get("Edge"), 1176, 28)
        {
            protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                ComposerDests.Tile t = d.s16;
                final ComposerSources.Full f = s.full;
                f.init(0, 0, 1, 1, 36, 1, t);
                f.setVar(0).paste(true);
                return t.saveGame();
            }
        }).get();

        Sprites() { }
    }
}