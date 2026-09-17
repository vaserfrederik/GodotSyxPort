using System.IO;
using System.Numerics;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.race.appearence
{
    public sealed class RExtras
    {
        private static int from = 0;
        public readonly TILE_SHEET tool;
        public readonly TILE_SHEET water;
        public readonly TILE_SHEET trolly;
        public readonly TILE_SHEET Lwater;

        public RExtras(Path path)
        {
            tool = new ITileSheet(path, 428, 310)
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.init(0, 0, 1, 1, 2, 24, d.s24);
                    return gets(6, d.s24, s);
                }
            }.get();

            water = new ITileSheet
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    from = 0;
                    s.singles.init(s.singles.body().x2(), 0, 1, 1, 2, 24, d.s24);
                    return gets(4, d.s24, s);
                }
            }.get();

            trolly = new ITileSheet
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    from = 4;
                    return gets(4, d.s24, s);
                }
            }.get();

            Lwater = new ITileSheet
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.init(s.singles.body().x2(), 0, 1, 1, 2, 24, d.s32);
                    from = 0;
                    return gets(4, d.s32, s);
                }
            }.get();
        }

        private TILE_SHEET gets(int nr, Tile d, ComposerSources s)
        {
            for (int i = 0; i < nr; i++)
            {
                s.singles.setSkip((from + i) * 2, 2).paste(3, true);
            }
            from += nr;
            return d.saveGame();
        }
    }
}