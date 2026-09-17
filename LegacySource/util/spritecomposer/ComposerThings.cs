using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace util.spritecomposer
{
    using static util.spritecomposer.Resources.c;
    using static util.spritecomposer.Resources.dests;
    using static util.spritecomposer.Resources.fonter;
    using static util.spritecomposer.Resources.g;
    using static util.spritecomposer.Resources.immi;
    using static util.spritecomposer.Resources.p;
    using static util.spritecomposer.Resources.sources;

    using init.constant.C;
    using snake2d.SPRITE_RENDERER;
    using snake2d.util.color.COLOR;
    using snake2d.util.color.ColorImp;
    using snake2d.util.file.FileGetter;
    using snake2d.util.sets.ArrayList;
    using snake2d.util.sets.LIST;
    using snake2d.util.sprite.SPRITE;
    using snake2d.util.sprite.TILE_SHEET;
    using snake2d.util.sprite.TextureCoords;
    using snake2d.util.sprite.TileTexture;
    using snake2d.util.sprite.text.Font;

    public sealed class ComposerThings
    {
        private ComposerThings()
        {
        }

        public class IInit
        {
            public IInit(Path path, int width, int height) => throw new NotImplementedException();

            public IInit() => throw new NotImplementedException();

            protected void init(ComposerUtil c, ComposerSources s, ComposerDests d) => throw new NotImplementedException();
        }

        public abstract class ITileSheet
        {
            protected ITileSheet() { }

            protected ITileSheet(Path path, int width, int height) => throw new NotImplementedException();

            static TILE_SHEET save(int scale, int tileSize, int startTile, int tiles, int tilesX) => throw new NotImplementedException();

            private static TILE_SHEET read(FileGetter g) => throw new NotImplementedException();

            public TILE_SHEET get() => throw new NotImplementedException();

            protected abstract TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d);
        }

        public abstract class ITileSprite : SPRITE
        {
            private readonly int width, height;
            private readonly TILE_SHEET sheet;

            protected ITileSprite(int width, int height, int scale) => throw new NotImplementedException();

            protected ITileSprite(int width, int height, int scale, Path path, int w, int h) => throw new NotImplementedException();

            public TILE_SHEET get() => throw new NotImplementedException();

            protected abstract TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d);

            public void render(SPRITE_RENDERER renderer) => throw new NotImplementedException();

            public int getWidth() => width;

            public int getHeight() => height;

            public int getTextureID() => throw new NotImplementedException();

            public TextureCoords getTextureCoords() => throw new NotImplementedException();
        }

        public abstract class ITileSheetList
        {
            protected ITileSheetList() { }

            protected ITileSheetList(Path path, int width, int height) => throw new NotImplementedException();

            public TILE_SHEET[] get() => throw new NotImplementedException();

            protected abstract int init(ComposerUtil c, ComposerSources s, ComposerDests d);

            protected abstract TILE_SHEET next(int i, ComposerUtil c, ComposerSources s, ComposerDests d);
        }

        public abstract class IColorSampler
        {
            protected IColorSampler() { }

            protected IColorSampler(Path path, int width, int height) => throw new NotImplementedException();

            static COLOR save(int c) => throw new NotImplementedException();

            public LIST<COLOR> get() => throw new NotImplementedException();

            public LIST<COLOR> getHalf() => throw new NotImplementedException();

            protected abstract int init(ComposerUtil c, ComposerSources s, ComposerDests d);

            protected abstract COLOR next(int i, ComposerUtil c, ComposerSources s, ComposerDests d);
        }

        public abstract class IColorSamplerSingle
        {
            protected IColorSamplerSingle() { }

            public COLOR get() => throw new NotImplementedException();

            public COLOR getHalf() => throw new NotImplementedException();

            protected abstract COLOR init(ComposerUtil c, ComposerSources s, ComposerDests d);
        }

        public abstract class IFont
        {
            protected IFont() { }

            protected IFont(Path path) => throw new NotImplementedException();

            public Font get(int trail) => throw new NotImplementedException();

            protected abstract Font init(ComposerUtil c, ComposerFonter f);
        }

        public static class ISprite
        {
            private static SPRITE.SpriteImp getSprite(int scale, SpriteData d) => throw new NotImplementedException();

            public static SPRITE game(SpriteData d) => throw new NotImplementedException();

            public static SPRITE gui(SpriteData d) => throw new NotImplementedException();

            public static SPRITE normal(SpriteData d) => throw new NotImplementedException();

            public static SPRITE scaled(SpriteData d, int scale) => throw new NotImplementedException();

            public static LIST<SPRITE> game(SpriteData[] data) => throw new NotImplementedException();

            public static LIST<SPRITE> gui(SpriteData[] data) => throw new NotImplementedException();

            public static LIST<SPRITE> normal(SpriteData[] data) => throw new NotImplementedException();
        }
    }
}