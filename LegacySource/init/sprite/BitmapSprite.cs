using System;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.sprite
{
    public class BitmapSprite : Bitmap2D
    {
        public const int WIDTH = 12;
        public const int HEIGHT = 12;
        public const int AREA = WIDTH * HEIGHT;

        public BitmapSprite() : base(12, 12, false)
        {
        }

        public void Paint(Bitmap2D data)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Set(x, y, data.Is(x, y));
                }
            }
        }

        public void Scaled(SPRITE_RENDERER r, int sx, int sy, int scale, COLOR foreground, COLOR borderN, COLOR borderS)
        {
            OPACITY.O99.Bind();
            int hs = scale / 2;

            for (int y = -1; y <= HEIGHT; y++)
            {
                for (int x = -1; x <= WIDTH; x++)
                {
                    if (Is(x, y))
                    {
                        foreground.Render(r, sx + x * scale, sx + x * scale + scale, sy + y * scale, sy + y * scale + scale);
                    }
                    else
                    {
                        foreach (DIR d in DIR.ORTHO)
                        {
                            if (Is(x, y, d))
                            {
                                int dx = hs * ((1 + d.X()) / 2);
                                int dy = hs * ((1 + d.Y()) / 2);
                                COLOR c = d.X() < 0 || d.Y() < 0 ? borderN : borderS;
                                c.Render(r, sx + x * scale + dx, sx + x * scale + dx + hs * (1 + Math.Abs(d.Y())), sy + y * scale + dy, sy + y * scale + dy + hs * (1 + Math.Abs(d.X())));
                            }
                        }

                        foreach (DIR d in DIR.NORTHO)
                        {
                            if (Is(x, y, d))
                            {
                                int dx = hs * ((1 + d.X()) / 2);
                                int dy = hs * ((1 + d.Y()) / 2);
                                COLOR c = d.X() < 0 || d.Y() < 0 ? borderN : borderS;
                                c.Render(r, sx + x * scale + dx, sx + x * scale + dx + hs, sy + y * scale + dy, sy + y * scale + dy + hs);
                            }
                        }
                    }
                }
            }
            OPACITY.Unbind();
        }

        public static Bitmap2D[] Read(Path path)
        {
            SnakeImage im = new SnakeImage(path);
            int w = (im.width - 2) / (WIDTH + 2);
            int h = (im.height - 2) / (HEIGHT + 2);

            Bitmap2D[] datas = new Bitmap2D[w * h];
            for (int i = 0; i < datas.Length; i++)
                datas[i] = new Bitmap2D(WIDTH, HEIGHT, false);

            int di = 0;
            for (int fy = 0; fy < h; fy++)
            {
                for (int fx = 0; fx < w; fx++)
                {
                    int sx = 2 + fx * (WIDTH + 2);
                    int sy = 2 + fy * (HEIGHT + 2);
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        for (int x = 0; x < WIDTH; x++)
                        {
                            int px = sx + x;
                            int py = sy + y;
                            if (((im.rgb.Get(px, py) >> 8) & 0x00FFFFFF) == 0)
                            {
                                datas[di].Set(x, y, true);
                            }
                        }
                    }
                    di++;
                }
            }
            im.Dispose();

            return datas;
        }
    }
}