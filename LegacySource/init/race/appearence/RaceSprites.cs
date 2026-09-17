using System;
using System.IO;
using init.paths;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.race.appearence
{
    public sealed class RaceSprites
    {
        private static int from = 0;
        public readonly TILE_SHEET blood;
        public readonly TILE_SHEET grit;
        public readonly TILE_SHEET Lblood;
        public readonly TILE_SHEET Lgrit;
        public readonly TILE_SHEET gore_stencil;
        public readonly TILE_SHEET gore_overlay;

        public RaceSprites() throws IOException
        {
            blood = new ITileSheet(PATHS.SPRITE().GetFolder("race").GetFolder("misc").Get("Overlays"), 460, 486)
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.Init(0, 0, 1, 1, 2, 24, d.s24);
                    from = 0;
                    return Gets(8, d.s24, s);
                }
            }.Get();

            grit = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    return Gets(8, d.s24, s);
                }
            }.Get();

            Lblood = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.Init(s.singles.body().x2(), 0, 1, 1, 2, 24, d.s32);
                    from = 0;
                    return Gets(8, d.s32, s);
                }
            }.Get();

            Lgrit = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.Init(s.singles.body().x2(), 0, 1, 1, 2, 24, d.s32);
                    from = 0;
                    return Gets(8, d.s32, s);
                }
            }.Get();

            gore_stencil = new ITileSheet(PATHS.SPRITE().GetFolder("race").GetFolder("misc").Get("Gore"), 316, 158)
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.Init(0, 0, 1, 1, 4, 4, d.s32);
                    s.singles.SetSkip(0, 8).Paste(true);
                    return d.s32.SaveGame();
                }
            }.Get();

            gore_overlay = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.SetSkip(8, 8).Paste(true);
                    return d.s32.SaveGame();
                }
            }.Get();
        }

        private TILE_SHEET Gets(int nr, Tile d, ComposerSources s)
        {
            for (int i = 0; i < nr; i++)
            {
                s.singles.SetSkip((from + i) * 2, 2).Paste(3, true);
            }
            from += nr;
            return d.SaveGame();
        }
    }
}