using System;
using System.IO;

namespace Settlement.Thing.Pointlight
{
    using Init.Paths;
    using Init.Sprite;
    using Snake2D.Util.Sprite;
    using Util.Spritecomposer;
    using Util.Spritecomposer.ComposerDests;
    using Util.Spritecomposer.ComposerSources;
    using Util.Spritecomposer.ComposerThings;

    internal sealed class Sprites
    {
        public readonly TILE_SHEET FlameSmall;
        public readonly TILE_SHEET FlameMedium;
        public readonly TILE_SHEET FlameBig;
        public readonly TILE_SHEET Candle;

        public readonly TileTexture.TileTextureScroller Displacement = SPRITES.Textures().DisBig.Scroller(4, -3);
        public readonly TileTexture.TileTextureScroller Texture = SPRITES.Textures().Fire.Scroller(-3, 4);

        public Sprites() : base()
        {
            var path = PATHS.SpriteSettlement().GetFolder("thing");

            FlameSmall = new ITileSheet(path.Get("Fire"), 236, 62)
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.S8;
                    var s = ss.Singles;
                    s.Init(0, 0, 1, 1, 8, 4, t);
                    s.SetSkip(0, 8).Paste(true);
                    return t.SaveGame();
                }
            }.Get();

            FlameMedium = new ITileSheet
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.S8;
                    var s = ss.Singles;
                    s.SetSkip(8, 8).Paste(true);
                    return t.SaveGame();
                }
            }.Get();

            FlameBig = new ITileSheet
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.S8;
                    var s = ss.Singles;
                    s.SetSkip(16, 8).Paste(true);
                    return t.SaveGame();
                }
            }.Get();

            Candle = new ITileSheet
            {
                Init = (ComposerUtil c, ComposerSources ss, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.S8;
                    var s = ss.Singles;
                    s.SetSkip(24, 8).Paste(true);
                    return t.SaveGame();
                }
            }.Get();
        }
    }
}