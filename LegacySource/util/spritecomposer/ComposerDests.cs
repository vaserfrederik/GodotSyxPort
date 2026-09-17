using System;
using System.IO;
using System.Threading;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite;

namespace util.spritecomposer
{
    public sealed class ComposerDests
    {
        public readonly Tile s16;
        public readonly Tile s8;
        public readonly Tile s32;
        public readonly Tile s24;
        private readonly DestChunk chunk;

        public ComposerDests(int WIDTH)
        {
            s16 = new Tile(16, WIDTH);
            s8 = new Tile(8, WIDTH);
            s32 = new Tile(32, WIDTH);
            s24 = new Tile(24, WIDTH);
            chunk = new DestChunk(WIDTH - s24.tilesX * s24.size, 512);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    s16.diffuseSet(x, y, 255, 255, 255, 255);
                    s16.normalSet(x, y, 0x80, 0x80, 0xFF, 0xFF);
                }
            }
            s16.skip(1);
        }

        private volatile bool saved = false;

        public void Save(Path deff, Path nor, FilePutter p, int extraHeight)
        {
            int HEIGHT = height(extraHeight);

            SnakeImage diffuse = new SnakeImage(s16.width() * s16.tilesX, HEIGHT);
            SnakeImage normal = new SnakeImage(diffuse.width, HEIGHT);

            for (COORDINATE c : new Rec(diffuse.width, HEIGHT))
                normal.rgb.Set(c.x(), c.y(), 127, 127, 255, 255);

            Tile[] tiles = new Tile[] { s8, s16, s32, s24 };

            int ly = 0;
            int y1 = 0;
            foreach (Tile t in tiles)
            {
                int h = (int)Math.Ceiling((double)t.lastTile / t.tilesX);
                p.i(h);
                h *= t.size;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < t.destWidth(); x++)
                    {
                        diffuse.rgb.Set(x, y1 + y, t.diffuseGet(x, y));
                        normal.rgb.Set(x, y1 + y, t.normalGet(x, y));
                    }
                }
                y1 += h;
                ly = y1 - h;

                t.Dispose();
            }

            for (int y = 0; y < chunk.diffuse.height; y++)
            {
                for (int x = 0; x < chunk.normal.width; x++)
                {
                    int py = ly + y;
                    diffuse.rgb.Set(x + s24.tilesX * 24, py, chunk.diffuseGet(x, y));
                    normal.rgb.Set(x + s24.tilesX * 24, py, chunk.normalGet(x, y));
                }
            }

            chunk.Dispose();

            saved = false;

            Thread t = new Thread(() =>
            {
                diffuse.Save("" + deff.toAbsolutePath());
                normal.Save("" + nor.toAbsolutePath());
                saved = true;
            });
            t.Name = "composer saver";
            t.Start();

            while (!saved)
            {
                CORE.CheckIn();
                try
                {
                    Thread.Sleep(16);
                }
                catch (InterruptedException e)
                {
                    e.printStackTrace();
                }
            }

            diffuse.Dispose();
            normal.Dispose();
        }

        public int Height(int extra)
        {
            Tile[] tiles = new Tile[] { s8, s16, s32, s24 };

            int h = 0;
            foreach (Tile t in tiles)
            {
                h += (int)Math.Ceiling((double)t.lastTile / t.tilesX) * t.size;
            }

            if ((int)Math.Ceiling((double)s24.lastTile / s24.tilesX) * s24.size < chunk.diffuse.height + extra)
            {
                h -= (int)Math.Ceiling((double)s24.lastTile / s24.tilesX) * s24.size;
                h += chunk.diffuse.height + extra;
            }

            int hh = h / 256;
            if (h % 256 > 0)
                hh++;
            return hh * 256;
        }

        public void Dispose()
        {
            s16.Dispose();
            s8.Dispose();
            s32.Dispose();
            s24.Dispose();
            chunk.Dispose();
        }

        public abstract class Dest
        {
            public abstract int x1();
            public abstract int y1();
            public abstract int width();
            public abstract int height();
            public abstract int destWidth();
            public abstract void Dispose();
            public abstract void diffuseSet(int x, int y, int c);
            public abstract int diffuseGet(int x, int y);
            public abstract void normalSet(int x, int y, int c);
            public abstract int normalGet(int x, int y);
            public abstract void Jump(int i);
        }

        public class Tile : Dest
        {
            private readonly SnakeImage[] diffuses;
            private readonly SnakeImage[] normals;
            public readonly int size;
            public readonly int tilesX;
            public int lastTile = 0;

            public Tile(int size, int width)
            {
                this.size = size;
                tilesX = width / size;
                diffuses = new SnakeImage[0];
                normals = new SnakeImage[0];
            }

            public override int x1()
            {
                return 0;
            }

            public override int y1()
            {
                return 0;
            }

            public override int width()
            {
                return tilesX * size;
            }

            public override int height()
            {
                return diffuses.Length * 32 * size;
            }

            public override int destWidth()
            {
                return tilesX * size;
            }

            public override void Dispose()
            {
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i].Dispose();
                    diffuses[i].Dispose();
                }
            }

            private void SetNewImage(int x, int y)
            {
                int k = y / (size * 32);

                if (k >= diffuses.Length)
                {
                    SnakeImage[] diffs = new SnakeImage[diffuses.Length + 1];
                    SnakeImage[] norms = new SnakeImage[normals.Length + 1];

                    for (int i = 0; i < diffuses.Length; i++)
                    {
                        diffs[i] = diffuses[i];
                        norms[i] = normals[i];
                    }
                    diffs[diffs.Length - 1] = new SnakeImage(tilesX * this.size, 32 * this.size);
                    norms[norms.Length - 1] = new SnakeImage(tilesX * this.size, 32 * this.size);

                    diffuses = diffs;
                    normals = norms;
                }
            }

            public override void diffuseSet(int x, int y, int c)
            {
                SetNewImage(x, y);
                int k = y / (size * 32);
                y -= k * size * 32;
                diffuses[k].rgb.Set(x, y, c);
            }

            public override int diffuseGet(int x, int y)
            {
                SetNewImage(x, y);
                int k = y / (size * 32);
                y -= k * size * 32;
                return diffuses[k].rgb.Get(x, y);
            }

            public override void normalSet(int x, int y, int c)
            {
                SetNewImage(x, y);
                int k = y / (size * 32);
                y -= k * size * 32;
                normals[k].rgb.Set(x, y, c);
            }

            public override int normalGet(int x, int y)
            {
                SetNewImage(x, y);
                int k = y / (size * 32);
                y -= k * size * 32;
                return normals[k].rgb.Get(x, y);
            }

            public override void Jump(int i)
            {
                int newX = (tx + i) % tilesX;
                int newY = ty + (tx + i) / tilesX;
                if (newX < 0)
                {
                    newX += tilesX;
                    newY--;
                }
                tx = newX;
                ty = newY;
            }

            private int tx = 0;
            private int ty = 0;

            public void Skip(int i)
            {
                Jump(i);
                lastTile = tx + ty * tilesX;
            }

            public void SkipNPaint(int i)
            {
                while (i-- > 0)
                {
                    SetNewImage(x1(), y1());
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            diffuseSet(x1() + x, y1() + y, -1);
                        }
                    }
                    Jump(1);
                }
            }
        }

        public class DestChunk : Dest
        {
            private readonly Rec rec;
            private readonly SnakeImage diffuse;
            private readonly SnakeImage normal;
            public readonly int width;

            public DestChunk(int width, int height)
            {
                diffuse = new SnakeImage(width, height);
                normal = new SnakeImage(width, height);
                this.width = width;
                rec = new Rec(0, 0, width, height);
            }

            public override int x1()
            {
                return rec.x1();
            }

            public override int y1()
            {
                return rec.y1();
            }

            public override int width()
            {
                return rec.width();
            }

            public override int height()
            {
                return rec.height();
            }

            public override int destWidth()
            {
                return width;
            }

            public override void Dispose()
            {
                normal.Dispose();
                diffuse.Dispose();
            }

            public override void diffuseSet(int x, int y, int c)
            {
                diffuse.rgb.Set(x, y, c);
            }

            public override int diffuseGet(int x, int y)
            {
                return diffuse.rgb.Get(x, y);
            }

            public override void normalSet(int x, int y, int c)
            {
                normal.rgb.Set(x, y, c);
            }

            public override int normalGet(int x, int y)
            {
                return normal.rgb.Get(x, y);
            }

            public override void Jump(int i)
            {
                // Implement jump logic if needed
            }

            public override int size()
            {
                return rec.width();
            }
        }
    }
}