using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace util.rendering
{
    public sealed class Minimap
    {
        private const double grit = 0.075;
        private bool open = true;

        private readonly int TILESIZE = 32;
        private readonly int DIM_PIXEL;
        private readonly int DIM_TILES;
        private readonly TILE_SHEET sheet;

        public Minimap(int dim) : this(dim, null)
        {
        }

        public Minimap(int dim, Allocator allocator) : this(dim, allocator, null)
        {
        }

        public Minimap(int dim, Allocator allocator, ComposerThings.ITileSheetComposer composer)
        {
            DIM_PIXEL = dim;
            DIM_TILES = DIM_PIXEL / TILESIZE;

            sheet = composer?.Get() ?? new ComposerThings.ITileSheet()
            {
                Init = (c, s, d) =>
                {
                    d.S32.SkipNPaint(DIM_TILES * DIM_TILES);
                    return d.S32.Save(1);
                }
            }.Get();
        }

        public void SetOpen(bool o)
        {
            this.open = o;
        }

        public void PutPixel(int x, int y, COLOR col)
        {
            if (open)
            {
                int tile = (x / TILESIZE) + (y / TILESIZE) * DIM_TILES;
                int dx = x % TILESIZE;
                int dy = y % TILESIZE;
                TextureCoords c = sheet.GetTexture(tile);
                GAME.Texture().PutPixel(c.X1 + dx, c.Y1 + dy, GetC(col.Red()), GetC(col.Green()), GetC(col.Blue()));
            }
        }

        public void PutPixels(byte[] pixels)
        {
            int s = TILESIZE * TILESIZE;
            byte[] tmp = Alloc.Bb(s * 4);

            for (int py = 0; py < DIM_PIXEL; py += TILESIZE)
            {
                for (int px = 0; px < DIM_PIXEL; px += TILESIZE)
                {
                    int tile = px / TILESIZE + (py / TILESIZE) * DIM_TILES;
                    TextureCoords c = sheet.GetTexture(tile);
                    for (int y = 0; y < TILESIZE; y++)
                    {
                        for (int x = 0; x < TILESIZE; x++)
                        {
                            int to = (x + y * TILESIZE) * 4;
                            int from = ((py + y) * DIM_PIXEL + px + x) * 4;
                            for (int j = 0; j < 3; j++)
                            {
                                tmp[to + j] = GetC(pixels[from + j]);
                            }
                            tmp[to + 3] = pixels[from + 3];
                        }
                    }
                    GAME.Texture().PutPixelBatch(c.X1, c.Y1, TILESIZE, tmp);
                }
            }
        }

        public static byte GetC(byte c)
        {
            int res = BitConverter.ToInt32(new byte[] { c, 0, 0, 0 }, 0) * 2;
            int gmax = (int)(res * grit);
            int g = (int)(-gmax + RND.RFloat() * 2 * gmax);
            res += g;
            if (res > 255)
                res = 255;
            return (byte)res;
        }

        public void Flush()
        {
        }

        private readonly TextureCoords texture = new TextureCoords();

        public void Render(SPRITE_RENDERER r, double px1d, double py1d, int sx1, int sy1, int swidth, int sheight, double scale)
        {
            if (px1d < 0)
            {
                double d = px1d * scale;
                px1d = 0;
                sx1 -= d;
                swidth += d;
            }

            if (py1d < 0)
            {
                double d = py1d * scale;
                py1d = 0;
                sy1 -= d;
                sheight += d;
            }

            int px1 = (int)px1d;
            int py1 = (int)py1d;
            {
                int dx = (int)Math.Ceiling((px1d - px1) * scale);
                int dy = (int)Math.Ceiling((py1d - py1) * scale);
                sx1 -= dx;
                sy1 -= dy;
                swidth += dx;
                sheight += dy;
            }

            while (sheight > 0)
            {
                int ph = TILESIZE;
                ph -= (py1 % TILESIZE);
                if (py1 + ph > DIM_PIXEL)
                {
                    ph = DIM_PIXEL - py1;
                    if (ph <= 0)
                        return;
                }

                int th = (int)(ph * scale);

                if (th > sheight)
                {
                    ph = (int)Math.Ceiling(sheight / scale);
                    th = (int)(ph * scale);
                }

                int px = px1;
                int x = sx1;
                int w = swidth;
                while (w > 0)
                {
                    int pw = TILESIZE;
                    pw -= (px % TILESIZE);

                    if (px + pw > DIM_PIXEL)
                    {
                        pw = DIM_PIXEL - px;
                        if (pw <= 0)
                            break;
                    }

                    int tw = (int)(pw * scale);

                    if (tw > w)
                    {
                        pw = (int)Math.Ceiling(w / scale);
                        tw = (int)(pw * scale);
                    }

                    TextureCoords c = Get(px, py1, pw, ph);
                    r.RenderSprite(x, x + tw, sy1, sy1 + th, c);
                    w -= tw;
                    x += tw;
                    px += pw;
                }

                sheight -= th;
                sy1 += th;
                py1 += ph;
            }
        }

        private TextureCoords Get(int px1, int py1, int w, int h)
        {
            int tile = (px1 / TILESIZE) + (py1 / TILESIZE) * DIM_TILES;
            int dx = px1 % TILESIZE;
            int dy = py1 % TILESIZE;
            TextureCoords c = sheet.GetTexture(tile);

            texture.Get(c.X1 + dx, c.Y1 + dy, w, h);

            return texture;
        }

        public TextureCoords Texture(int tx, int ty, int w, int h)
        {
            int tile = (tx / TILESIZE) + (ty / TILESIZE) * DIM_TILES;
            int dx = tx % TILESIZE;
            int dy = ty % TILESIZE;
            TextureCoords c = sheet.GetTexture(tile);
            texture.Get(c.X1 + dx, c.Y1 + dy, w, h);

            return texture;
        }

        public void Render(SPRITE_RENDERER r, int x1, int y1, RECTANGLE quad)
        {
            COLOR.WHITE10.Render(r, x1, x1 + quad.Width(), y1, y1 + quad.Height());
            Render(r, quad.X1(), quad.Y1(), x1, y1, quad.Width(), quad.Height(), 1);
        }

        public void Render(SPRITE_RENDERER r, int x1, int y1)
        {
            COLOR.WHITE10.Render(r, x1, x1 + Width(), y1, y1 + Height());
            Render(r, 0, 0, x1, y1, Width(), Height(), 1);
        }

        public int Width()
        {
            return DIM_PIXEL;
        }

        public int Height()
        {
            return DIM_PIXEL;
        }
    }
}