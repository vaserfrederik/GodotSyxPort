using System;
using System.IO;
using System.Collections.Generic;

namespace snake2d.util.rnd
{
    public class Polymap : BODY_HOLDER
    {
        private readonly int width;
        private readonly int height;
        private readonly RECTANGLE bounds;
        private readonly int[] ids;
        private readonly double r;
        private readonly double ri;
        private int checkI;
        private int[] checkers;
        private readonly RECTANGLE body;
        public readonly MAP_BOOLEANE checker;

        public Polymap(int width, int height) : this(width, height, 1.0)
        {
        }

        public Polymap(int width, int height, double scale)
        {
            this.width = width;
            this.height = height;
            ids = Alloc.ii(height * width);
            bounds = new Rec(width, height);
            float[,] heights = new float[height, width];
            double a = scale * width * height / 163;
            int id = 1;
            r = 64 * scale;
            ri = 1.0 / r;
            for (int i = 0; i < a; i++)
                polly(RND.rInt(width), RND.rInt(height), id++, heights);

            checkers = Alloc.ii(id);
            body = new Rec(width, height);
            checker = new MAP_BOOLEANE.BooleanMapE(width, height)
            {
                set = (tile, value) =>
                {
                    if (value)
                        checkers[ids[tile]] = checkI;
                    else
                        checkers[ids[tile]] = checkI - 1;
                    return this;
                },
                is_ = tile => checkers[ids[tile]] == checkI
            };
        }

        public MAP_INT getter = new MAP_INT
        {
            get = (tx, ty) => get(tx + ty * width),
            get_tile = tile => ids[tile]
        };

        public Polymap(int width, int height, int size, double relaxation)
        {
            this.width = width;
            this.height = height;
            ids = Alloc.ii(height * width);
            bounds = new Rec(width, height);
            float[,] heights = new float[height, width];
            int id = 1;

            r = width / size;
            ri = 1.0 / r;

            double dx = (double)width / size;
            double dy = (double)height / size;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double qx = dx * x;
                    double qy = dy * y;
                    qx += dx / 2;
                    qy += dy / 2;
                    qx += dx * Math.Pow(RND.rFloat(), relaxation) * (RND.rBoolean() ? 0.5 : -0.5);
                    qy += dy * Math.Pow(RND.rFloat(), relaxation) * (RND.rBoolean() ? 0.5 : -0.5);
                    polly((int)qx, (int)qy, id++, heights);
                }
            }

            checkers = Alloc.ii(id);
            body = new Rec(width, height);
            checker = new MAP_BOOLEANE.BooleanMapE(width, height)
            {
                set = (tile, value) =>
                {
                    if (value)
                        checkers[ids[tile]] = checkI;
                    else
                        checkers[ids[tile]] = checkI - 1;
                    return this;
                },
                is_ = tile => checkers[ids[tile]] == checkI
            };
        }

        public Polymap(RECTANGLE bounds, int cellsize, double randomness)
        {
            this.width = bounds.width();
            this.height = bounds.height();
            ids = Alloc.ii(height * width);
            this.bounds = new Rec(width, height);
            float[,] heights = new float[height, width];
            int id = 1;

            r = cellsize;
            ri = 1.0 / r;

            int d = cellsize / 2;

            for (int y = -cellsize; y < height + cellsize; y += cellsize)
            {
                for (int x = -cellsize; x < width + cellsize; x += cellsize)
                {
                    double qx = x;
                    double qy = y;
                    qx += d;
                    qy += d;
                    qx += RND.rSign() * RND.rFloat(d) * randomness;
                    qy += RND.rSign() * RND.rFloat(d) * randomness;
                    polly((int)qx, (int)qy, id++, heights);
                }
            }

            checkers = Alloc.ii(id);
            body = new Rec(width, height);
            checker = new MAP_BOOLEANE.BooleanMapE(width, height)
            {
                set = (tile, value) =>
                {
                    if (value)
                        checkers[ids[tile]] = checkI;
                    else
                        checkers[ids[tile]] = checkI - 1;
                    return this;
                },
                is_ = tile => checkers[ids[tile]] == checkI
            };
        }

        public MAP_BOOLEANE getScaled(double scale)
        {
            return new MAP_BOOLEANE
            {
                is_ = tile =>
                {
                    int x = tile % width;
                    int y = tile / width;
                    return is(x, y);
                },
                is_ = (tx, ty) => checker.is_((int)((tx * scale)) % width, (int)((ty * scale)) % height),
                set = (tile, value) =>
                {
                    int x = tile % width;
                    int y = tile / width;
                    return set(x, y, value);
                },
                set = (tx, ty, value) =>
                {
                    checker.set((int)((tx * scale)) % width, (int)((ty * scale)) % height, value);
                    return this;
                }
            };
        }

        private void polly(int x, int y, int id, float[,] heights)
        {
            if (bounds.holdsPoint(x, y) && heights[y, x] == 1f)
                return;
            for (int y1 = (int)(-r); y1 < r; y1++)
            {
                int ty = y1 + y;
                if (ty < 0 || ty >= height)
                    continue;
                for (int x1 = (int)(-r); x1 < r; x1++)
                {
                    int tx = x + x1;
                    if (tx < 0 || tx >= width)
                        continue;
                    //double d = Math.Abs(x1) + Math.Abs(y1);
                    double d = Math.Sqrt(x1 * x1 + y1 * y1);
                    if (d > r)
                        continue;
                    double v = 1.0 - ri * d;
                    if (v > heights[ty, tx])
                    {
                        heights[ty, tx] = (float)v;
                        ids[tx + width * ty] = id;
                    }
                }
            }
        }

        public MAP_BOOLEAN isEdge = new MAP_BOOLEAN
        {
            is_ = (tx, ty) =>
            {
                if (!bounds.holdsPoint(tx, ty))
                    return false;

                int id = ids[tx + ty * width];
                DIR d = DIR.E;

                for (int i = 0; i < 2; i++)
                {
                    int x = tx + d.x();
                    int y = ty + d.y();
                    if (bounds.holdsPoint(x, y))
                        if (id != ids[x + y * width])
                            return true;

                    d = d.next(2);
                }
                return false;
            },
            is_ = (tx, ty) =>
            {
                int id = ids[tx + ty * width];
                DIR d = DIR.E;

                for (int i = 0; i < 2; i++)
                {
                    int x = tx + d.x();
                    int y = ty + d.y();
                    if (bounds.holdsPoint(x, y))
                        if (id != ids[x + y * width])
                            return true;

                    d = d.next(2);
                }

                return false;
            }
        };

        public void checkInit()
        {
            checkI++;
        }

        public static void Main(string[] args)
        {
            int width = 512;
            int height = 512;

            SnakeImage im = new SnakeImage(width, height);
            Polymap p = new Polymap(width, height, 2.0);

            int cols = 128;
            int colM = cols - 1;
            COLOR[] co = new COLOR[128];

            for (int i = 0; i < cols; i++)
                co[i] = new ColorImp(RND.rInt(255), RND.rInt(255), RND.rInt(255));

            foreach (COORDINATE coo in p.body())
            {
                int id = p.ids[coo.x() + coo.y() * width];
                COLOR c = co[id & colM];

                im.rgb.set(coo.x(), coo.y(), c.red(), c.green(), c.red(), 255);
            }

            string path = new File("PollyTest.png").getAbsolutePath();
            Console.WriteLine(path);
            im.save(path);
        }

        public RECTANGLE body()
        {
            return body;
        }

        public int get(int tx, int ty)
        {
            return ids[tx + ty * width];
        }

        public int polys()
        {
            return checkers.Length;
        }
    }
}