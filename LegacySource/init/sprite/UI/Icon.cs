using System;
using Snake2D;

namespace Init.Sprite.UI
{
    public class Icon : SPRITE
    {
        public static readonly int S = 16;
        public static readonly int M = 24;
        public static readonly int L = 32;
        public static readonly int HUGE = 64;

        public readonly int size;

        private readonly SPRITE sprite;
        public readonly SPRITE huge;
        public readonly SPRITE big;
        public readonly SPRITE small;
        public readonly SPRITE medium;

        public Icon(int size, SPRITE s)
        {
            this.size = size;
            this.sprite = s;
            big = size == L ? this : new SPRITE.Scaled(this, L, L);
            small = size == S ? this : new SPRITE.Scaled(this, S, S);
            medium = size == M ? this : new SPRITE.Scaled(this, M, M);
            huge = size == HUGE ? this : new SPRITE.Scaled(this, HUGE, HUGE);
        }

        public Icon(SpriteData data)
            : this(data.width, new SPRITE.SpriteImp(data.x1, data.x1 + data.width, data.y1, data.y1 + data.height, data.width, data.height))
        {
        }

        public Icon(SPRITE sprite)
            : this(sprite.width(), sprite)
        {
        }

        public Icon Twin(SPRITE b)
        {
            return new Icon(size, new SPRITE.Twin(this, b));
        }

        public override Icon CreateColored(COLOR color)
        {
            return new Icon(SPRITE.super.CreateColored(color));
        }

        public override Icon Twin(SPRITE b, DIR align, int shadow)
        {
            return new Icon(size, SPRITE.super.Twin(b, align, shadow));
        }

        public override int Width()
        {
            return size;
        }

        public override int Height()
        {
            return size;
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            sprite.Render(r, X1, X2, Y1, Y2);
        }

        public override void RenderTextured(TextureCoords other, int X1, int X2, int Y1, int Y2)
        {
            sprite.RenderTextured(other, X1, X2, Y1, Y2);
        }

        private class IconSheet : Icon
        {
            private readonly TILE_SHEET sheet;
            private readonly int tile;

            public IconSheet(int size, TILE_SHEET sheet, int tile)
                : base(size, new SPRITE.Imp(size)
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        sheet.Render(r, tile, X1, X2, Y1, Y2);
                    }
                })
            {
                this.sheet = sheet;
                this.tile = tile;
            }

            public override TextureCoords Texture()
            {
                return sheet.GetTexture(tile);
            }
        }
    }
}