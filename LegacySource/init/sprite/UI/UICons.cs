using System;

namespace Init.Sprite.UI
{
    public sealed class UICons
    {
        private readonly TILE_SHEET sprite;
        private readonly SPRITE[] sprites = new SPRITE[16];
        private readonly UICons tiny;

        public UICons(TILE_SHEET sprite)
        {
            if (sprite.Tiles() < 16)
                throw new RuntimeException("array must be longer than 15! " + sprite.Tiles());
            this.sprite = sprite;
            for (int i = 0; i < 16; i++)
            {
                sprites[i] = new SPRITE.SpriteFromSheet(sprite, i);
            }
            tiny = this;
        }

        private UICons(TILE_SHEET sprite, UICons tiny)
        {
            if (sprite.Tiles() < 16)
                throw new RuntimeException("array must be longer than 15! " + sprite.Tiles());
            this.sprite = sprite;
            for (int i = 0; i < 16; i++)
            {
                sprites[i] = new SPRITE.SpriteFromSheet(sprite, i);
            }
            this.tiny = tiny;
        }

        public SPRITE Get(int i)
        {
            return sprites[i];
        }

        public SPRITE Get(DIR d1, DIR d2, DIR d3, DIR d4)
        {
            int m = 0;
            if (d1 != null)
                m |= d1.Mask();
            if (d2 != null)
                m |= d2.Mask();
            if (d3 != null)
                m |= d3.Mask();
            if (d4 != null)
                m |= d4.Mask();
            return Get(m);
        }

        public SPRITE Get(DIR d1, DIR d2, DIR d3)
        {
            return Get(d1, d2, d3, null);
        }

        public SPRITE Get(DIR d1, DIR d2)
        {
            return Get(d1, d2, null, null);
        }

        public SPRITE Get(DIR d1)
        {
            return Get(d1, null, null, null);
        }

        public static int GetIndex(bool N, bool E, bool S, bool W)
        {
            int nr = 0;

            if (N)
            {
                nr |= 0b0001;
            }

            if (E)
            {
                nr |= 0b0010;
            }

            if (S)
            {
                nr |= 0b0100;
            }

            if (W)
            {
                nr |= 0b1000;
            }

            return nr;
        }

        public void RenderBox(SPRITE_RENDERER r, int x1, int y1, int width, int height)
        {
            int M = sprite.Size() / 4;
            int size = sprite.Size();
            if (width <= size - 2 * M && height <= size - 2 * M)
            {
                RenderCentered(r, 0, x1 + width / 2, y1 + height / 2);
                return;
            }

            int X1 = x1;
            int Y1 = y1;
            int X2 = x1 + width;
            int Y2 = y1 + height;

            int w = (X2 - X1) / size - 1;
            int h = (Y2 - Y1) / size - 1;

            Render(r, DIR.S.Mask() | DIR.E.Mask(), X1 - M, Y1 - M);

            Render(r, DIR.S.Mask() | DIR.W.Mask(), X2 + M - size, Y1 - M);
            Render(r, DIR.N.Mask() | DIR.E.Mask(), X1 - M, Y2 + M - size);
            Render(r, DIR.N.Mask() | DIR.W.Mask(), X2 + M - size, Y2 + M - size);
            for (int i = 0; i < w; i++)
            {
                Render(r, DIR.S.Mask() | DIR.E.Mask() | DIR.W.Mask(), X1 - M + size + i * size, Y1 - M);
                Render(r, DIR.N.Mask() | DIR.E.Mask() | DIR.W.Mask(), X1 - M + size + i * size, Y2 + M - size);
            }
            for (int i = 0; i < h; i++)
            {
                Render(r, DIR.E.Mask() | DIR.N.Mask() | DIR.S.Mask(), X1 - M, Y1 - M + size + i * size);
                Render(r, DIR.W.Mask() | DIR.N.Mask() | DIR.S.Mask(), X2 + M - size, Y1 - M + size + i * size);
            }
        }

        public void Render(bool N, bool E, bool S, bool W, SPRITE_RENDERER r, int x, int y)
        {
            Render(r, GetIndex(N, E, S, W), x, y);
        }

        public void Render(SPRITE_RENDERER r, int s, int x, int y)
        {
            if (CORE.Renderer().GetZoomout() >= 3)
                tiny.sprite.Render(r, s, x, y);
            else
                sprite.Render(r, s, x, y);
        }

        public void Render(SPRITE_RENDERER r, int s, int x1, int x2, int y1, int y2)
        {
            if (CORE.Renderer().GetZoomout() >= 3)
                tiny.sprite.Render(r, s, x1, x2, y1, y2);
            else
                sprite.Render(r, s, x1, x2, y1, y2);
        }

        public void RenderCentered(SPRITE_RENDERER r, int s, int x, int y)
        {
            if (CORE.Renderer().GetZoomout() >= 3)
                tiny.sprite.Render(r, s, x - tiny.sprite.Size() / 2, y - tiny.sprite.Size() / 2);
            else
                sprite.Render(r, s, x - sprite.Size() / 2, y - sprite.Size() / 2);
        }

        public void Render(SPRITE_RENDERER r, int s, int corner, int x, int y)
        {
            Render(r, s, x, y);
            if (corner == 0)
                return;
            if (CORE.Renderer().GetZoomout() >= 3)
                tiny.sprite.Render(r, 16 + corner, x, y);
            else
                sprite.Render(r, 16 + corner, x, y);
        }

        public int Dim()
        {
            return sprite.Size();
        }
    }
}