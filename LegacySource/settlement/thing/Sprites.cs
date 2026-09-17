using System.IO;
using init.paths;
using snake2d.util.sprite;
using util.spritecomposer;
using util.spritecomposer.ComposerThings;

namespace settlement.thing
{
    public class Sprites
    {
        public readonly TILE_SHEET flesh;
        public readonly TILE_SHEET bloodPool;
        public readonly TILE_SHEET debris;
        public readonly TILE_SHEET caravan;
        public readonly TILE_SHEET rubbish;

        public Sprites()
        {
            var path = PATHS.SPRITE_SETTLEMENT().GetFolder("thing");

            flesh = new ITileSheet(path.Get("Gore"), 236, 62)
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    var t = d.s8;
                    var s = ss.singles;
                    s.Init(0, 0, 1, 1, 8, 4, t);
                    s.Paste(1, true);
                    return t.SaveGame();
                }
            }.Get();

            bloodPool = flesh.Slice(32, 64);

            debris = new ITileSheet(path.Get("Debris"), 292, 34)
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    var t = d.s8;
                    var s = ss.singles;
                    s.Init(0, 0, 1, 1, 10, 2, t);
                    s.SetSkip(0, 20).Paste(true);
                    return t.SaveGame();
                }
            }.Get();

            caravan = new ITileSheet(path.Get("Caravan"), 100, 116)
            {
                Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    s.singles.Init(0, 0, 1, 1, 2, 5, d.s16);
                    for (int i = 0; i < 5; i++)
                    {
                        s.singles.SetSkip(i * 2, 2).Paste(3, true);
                    }
                    return d.s16.SaveGame();
                }
            }.Get();

            rubbish = new ITileSheet(path.Get("Rubbish"), 460, 20)
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    var t = d.s8;
                    var s = ss.singles;
                    s.Init(0, 0, 1, 1, 16, 1, t);
                    s.Paste(true);
                    return t.SaveGame();
                }
            }.Get();
        }
    }
}