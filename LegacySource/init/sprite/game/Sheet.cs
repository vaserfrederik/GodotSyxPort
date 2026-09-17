using System;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.sprite;
using util.rendering;

namespace init.sprite.game
{
    public abstract class Sheet
    {
        public readonly bool HasRotation;
        public readonly bool HasShadow;
        public readonly int Tiles;

        public Sheet(int tiles, bool rots, bool sha)
        {
            this.Tiles = tiles;
            this.HasRotation = rots;
            this.HasShadow = sha;
        }

        public abstract void Render(SheetData da, int x, int y, RenderIterator it, SPRITE_RENDERER sr, int tile, int random, double degrade);
        public abstract void RenderShadow(SheetData da, int x, int y, RenderIterator it, ShadowBatch shadow, int tile, int random);

        public abstract TextureCoords Texture(int tile);

        public class Imp : Sheet
        {
            public readonly TILE_SHEET Sheet;
            private readonly int VarSize;

            public Imp(SheetType type, TILE_SHEET sheet, bool rotates) : base(sheet.Tiles(), rotates & type.DefRotates, Shadow(sheet, type.SizeSize * ((rotates & type.DefRotates) ? 4 : 1)))
            {
                this.Sheet = sheet;
                VarSize = type.SizeSize * ((rotates & type.DefRotates) ? 4 : 1);
            }

            static bool Shadow(TILE_SHEET sheet, int size)
            {
                int i = sheet.Tiles() / (size);
                return i > 1 && (i & 1) == 1;
            }

            public override void Render(SheetData da, int x, int y, RenderIterator it, SPRITE_RENDERER sr, int tile, int random, double degrade)
            {
                Sheet.Render(sr, tile, x, y);
                if (degrade > 0.05)
                {
                    OPACITY.O99.Bind();
                    Sheet.RenderTextured(SETT.ROOMS().Util.Filth.Texture(degrade, it.Ran()), tile, x, y);
                    OPACITY.Unbind();
                }
            }

            public override void RenderShadow(SheetData da, int x, int y, RenderIterator it, ShadowBatch shadow, int tile, int random)
            {
                if (da.ShadowLength > 0 || da.ShadowHeight > 0)
                {
                    shadow.SetHeight(da.ShadowLength).SetDistance2Ground(da.ShadowHeight);
                    int t = tile;
                    if (this.HasShadow)
                    {
                        t = Sheet.Tiles() - (VarSize);
                        t += tile % ((VarSize));
                    }
                    Sheet.Render(shadow, t, x, y);
                }
            }

            public TILE_SHEET Sheet()
            {
                return Sheet;
            }

            public override TextureCoords Texture(int tile)
            {
                return Sheet.GetTexture(tile);
            }
        }

        private class Dummy : Sheet
        {
            public Dummy(int tiles) : base(tiles, true, false)
            {
            }

            public override void Render(SheetData da, int x, int y, RenderIterator it, SPRITE_RENDERER sr, int tile, int random, double degrade)
            {
            }

            public override void RenderShadow(SheetData da, int x, int y, RenderIterator it, ShadowBatch shadow, int tile, int random)
            {
            }

            public override TextureCoords Texture(int tile)
            {
                return COLOR.WHITE100.Texture();
            }
        }
    }
}