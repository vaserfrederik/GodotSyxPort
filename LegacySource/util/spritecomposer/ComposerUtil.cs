using System;
using System.IO;
using snake2d;
using util.spritecomposer;
using static util.spritecomposer.Resources;

namespace util.spritecomposer
{
    public class ComposerUtil
    {
        private Path sourcePath;
        private SnakeImage TexSource;
        private int sourceHalf;
        private readonly int[][] buffer = Alloc.i2(1024, 1024);
        private readonly int[][] buffern = Alloc.i2(1024, 1024);

        public ComposerUtil()
        {
        }

        public void drawNormal(ComposerDests.Dest dest, int x, int y, int width, int height)
        {
            for (int y1 = 0; y1 < height; y1++)
            {
                for (int x1 = 0; x1 < width; x1++)
                {
                    dest.normalSet(x + x1, y + y1, 0x80, 0x80, 0xFF, 0xFF);
                }
            }
        }

        public void setSource(Path path, int width, int minHeight) throws IOException
        {
            setSource(path);
            if (p != null)
            {
                if (TexSource.width != width)
                {
                    throw new Errors.DataError("Image has the wrong width of " + TexSource.width + "\n" +
                            "resize the image's width to " + width, "" + path);
                }
                if (TexSource.height < minHeight)
                {
                    throw new Errors.DataError("Image has the wrong height of " + TexSource.height + "\n" +
                            "resize the image's height to at least " + minHeight, "" + path);
                }
            }
        }

        public void setSource(Path path) throws IOException
        {
            if (p != null)
            {
                sourcePath = path;
                if (TexSource != null)
                {
                    TexSource.dispose();
                    TexSource = null;
                }
                CORE.checkIn();
                TexSource = new SnakeImage(path);
                if (TexSource.width % 2 != 0)
                    throw new RuntimeException(path + " has the wrong dimension. Width must be divisible by 2");
                sourceHalf = TexSource.width / 2;
                saveFile(path);
            }
            else
            {
                validateFile(path);
            }
        }

        public void saveFile(Path path)
        {
            p.chars(PATHS.getSavePath(path));
            try
            {
                p.l(Files.getLastModifiedTime(path).toMillis());
            }
            catch (IOException e)
            {
                e.printStackTrace();
                throw new RuntimeException();
            }
        }

        public void validateFile(Path path) throws IOException
        {
            string path2 = g.chars();
            if (!PATHS.getSavePath(path).Equals(path2))
                throw new IOException(PATHS.getSavePath(path) + " " + path2);
            long l = Files.getLastModifiedTime(path).toMillis();
            long l2 = g.l();
            if (l != l2)
                throw new IOException(path + " " + l + " " + l2);
        }

        public Path getSourcePath()
        {
            return sourcePath;
        }

        public void copy(Source source)
        {
            int sx = source.x1();
            int sy = source.y1();

            for (int y = 0; y < source.height(); y++)
            {
                for (int x = 0; x < source.width(); x++)
                {
                    buffer[y][x] = TexSource.rgb.get(sx + x, sy + y);
                    if (sx + x + sourceHalf >= TexSource.width)
                    {
                        Console.Error.WriteLine(sx + " " + x + " " + sourceHalf);
                        Console.Error.WriteLine(source.width() + " " + source.height());
                    }
                    buffern[y][x] = TexSource.rgb.get(sx + x + sourceHalf, sy + y);
                }
            }
        }

        public void paste(ComposerDests.Dest dest)
        {
            for (int y = 0; y < dest.height(); y++)
            {
                for (int x = 0; x < dest.width(); x++)
                {
                    int dx = x;
                    int dy = y;

                    int c = buffer[y][x];
                    if ((c & 0x000000FF) == 0)
                        continue;
                    dest.diffuseSet(dest.x1() + dx, dest.y1() + dy, c);

                    int nc = buffern[y][x];
                    if ((nc & 0x000000FF) == 0)
                        continue;
                    dest.normalSet(dest.x1() + dx, dest.y1() + dy, nc);
                }
            }
        }

        public void paste(ComposerDests.Dest dest, double bgBlend)
        {
            for (int y = 0; y < dest.height(); y++)
            {
                for (int x = 0; x < dest.width(); x++)
                {
                    int dx = x;
                    int dy = y;

                    int c = buffer[y][x];
                    if ((c & 0x000000FF) == 0)
                        continue;

                    c = merge(bgBlend, dest.diffuseGet(dest.x1() + dx, dest.y1() + dy), c);

                    dest.diffuseSet(dest.x1() + dx, dest.y1() + dy, c);

                    int nc = buffern[y][x];
                    if ((nc & 0x000000FF) == 0)
                        continue;
                    dest.normalSet(dest.x1() + dx, dest.y1() + dy, nc);
                }
            }
        }

        public void pasteNormalOnly(ComposerDests.Dest dest, int rotation)
        {
            for (int y = 0; y < dest.size(); y++)
            {
                for (int x = 0; x < dest.size(); x++)
                {
                    int dx = x;
                    int dy = y;
                    int r = rotation;

                    int nc = buffern[y][x];
                    if ((nc & 0x000000FF) == 0)
                        continue;

                    while (r > 0)
                    {
                        int odx = dx;
                        dx = dest.size() - dy - 1;
                        dy = odx;
                        r--;

                        int re = 256 - ((nc >> 16) & 0x00FF);
                        int gr = (nc >> 24) & 0x00FF;

                        nc &= 0x0000FFFF;
                        nc |= (gr << 16);
                        nc |= (re << 24);

                    }
                    dest.normalSet(dest.x1() + dx, dest.y1() + dy, nc);
                }
            }
        }

        public void pasteRotated(ComposerDests.Dest dest, int rotation)
        {
            if (rotation < 0 || rotation >= 4)
                throw new ArgumentOutOfRangeException(nameof(rotation), "Rotation must be between 0 and 3");

            for (int y = 0; y < dest.size(); y++)
            {
                for (int x = 0; x < dest.size(); x++)
                {
                    int dx = x;
                    int dy = y;
                    int r = rotation;

                    int c = buffer[y][x];
                    if ((c & 0x000000FF) == 0)
                        continue;

                    int nc = buffern[y][x];

                    while (r > 0)
                    {
                        int odx = dx;
                        dx = dest.size() - dy - 1;
                        dy = odx;
                        r--;

                        int re = 256 - ((nc >> 16) & 0x00FF);
                        int gr = (nc >> 24) & 0x00FF;

                        nc &= 0x0000FFFF;
                        nc |= (gr << 16);
                        nc |= (re << 24);

                    }

                    dest.diffuseSet(dest.x1() + dx, dest.y1() + dy, c);
                    if ((nc & 0xFF000000) != 0)
                        dest.normalSet(dest.x1() + dx, dest.y1() + dy, nc);
                }
            }
        }

        public SnakeImage getSource()
        {
            return TexSource;
        }

        public int sampleSource(int x1, int y1)
        {
            return TexSource.rgb.get(x1, y1);
        }

        public void dispose()
        {
            if (TexSource != null)
            {
                TexSource.dispose();
                TexSource = null;
            }
        }

        private int merge(double blend, int bg, int fg)
        {
            int bgR = (bg >> 16) & 0xFF;
            int bgG = (bg >> 8) & 0xFF;
            int bgB = bg & 0xFF;

            int fgR = (fg >> 16) & 0xFF;
            int fgG = (fg >> 8) & 0xFF;
            int fgB = fg & 0xFF;

            int r = (int)(bgR * (1 - blend) + fgR * blend);
            int g = (int)(bgG * (1 - blend) + fgG * blend);
            int b = (int)(bgB * (1 - blend) + fgB * blend);

            return (r << 16) | (g << 8) | b;
        }
    }
}