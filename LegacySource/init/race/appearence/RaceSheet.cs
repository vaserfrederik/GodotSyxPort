using System.IO;
using snake2d.util.sprite;
using util.spritecomposer;
using util.spritecomposer.ComposerThings;

namespace init.race.appearence
{
    public sealed class RaceSheet
    {
        public readonly TILE_SHEET sheet;
        public readonly TILE_SHEET lay;

        public RaceSheet(string path) : this(new FileInfo(path))
        {
        }

        public RaceSheet(FileInfo path) : base()
        {
            sheet = new ITileSheet(path.FullName, 448, 546)
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    int a = 18;
                    s.singles.init(0, 0, 1, 1, 2, a, d.s24);
                    for (int i = 0; i < a; i++)
                    {
                        s.singles.setSkip(i * 2, 2).paste(3, true);
                    }
                    return d.s24.saveGame();
                }
            }.get();

            lay = new ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    int a = 6;
                    s.singles.init(s.singles.body().x2(), 0, 1, 1, 4, 3, d.s32);
                    for (int i = 0; i < a; i++)
                    {
                        s.singles.setSkip(i * 2, 2).paste(3, true);
                    }
                    return d.s32.saveGame();
                }
            }.get();
        }
    }
}