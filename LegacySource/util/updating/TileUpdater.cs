using System;
using System.IO;

namespace util.updating
{
    public abstract class TileUpdater : SAVABLE
    {
        private const int randomizerSize = 64;
        private const int randomizerMask = randomizerSize - 1;
        private static readonly byte[,] randomizerX = new byte[64, 64];
        private static readonly byte[,] randomizerY = new byte[64, 64];

        static TileUpdater()
        {
            for (int y = 0; y < randomizerSize; y++)
            {
                for (int x = 0; x < randomizerSize; x++)
                {
                    randomizerX[y, x] = (byte)x;
                    randomizerY[y, x] = (byte)y;
                }
            }

            for (int y = 0; y < randomizerSize; y++)
            {
                for (int x = 0; x < randomizerSize; x++)
                {
                    byte ax = randomizerX[y, x];
                    byte ay = randomizerY[y, x];

                    int x2 = RND.rInt(randomizerSize);
                    int y2 = RND.rInt(randomizerSize);

                    randomizerX[y, x] = randomizerX[y2, x2];
                    randomizerY[y, x] = randomizerY[y2, x2];

                    randomizerX[y2, x2] = ax;
                    randomizerY[y2, x2] = ay;
                }
            }
        }

        private readonly int width;
        private readonly int height;
        private int x, y, i;
        private readonly double secondsBetween;
        private readonly double tilesPerSecond;
        private double acc = 0;

        public TileUpdater(int width, int height, double secondsBetween)
        {
            this.width = width;
            this.height = height;
            this.secondsBetween = secondsBetween;
            tilesPerSecond = ((width * height) / secondsBetween);
        }

        public void Update(double ds)
        {
            acc += ds * tilesPerSecond;

            int a = (int)acc;
            acc -= a;
            while (a > 0)
            {
                a--;
                Update(x, y, i, secondsBetween);

                i++;
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                }
                if (y >= height)
                {
                    y = 0;
                    x = 0;
                    i = 0;
                }
            }
        }

        public void UpdateRandom(double ds)
        {
            acc += ds * tilesPerSecond;

            int a = (int)acc;
            acc -= a;

            int qw = width / 64;
            int qh = height / 64;

            int divI = (width / 64) * (height / 64);

            while (a > 0)
            {
                a--;

                int di = (i % divI);
                int qx = di % qw;
                int qy = (di / qw) % qh;
                qx *= 64;
                qy *= 64;

                int ri = i / divI;
                int rx = ri & randomizerMask;
                int ry = (ri / 64);

                qx += randomizerX[ry, rx];
                qy += randomizerY[ry, rx];

                int ui = qx + qy * width;
                Update(qx, qy, ui, secondsBetween);

                i++;

                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                }

                if (y >= height)
                {
                    y = 0;
                    x = 0;
                    i = 0;
                }
            }
        }

        protected abstract void Update(int tx, int ty, int i, double timeSinceLast);

        public override void Save(FilePutter fp)
        {
            fp.WriteInt(i);
            fp.WriteInt(x);
            fp.WriteInt(y);
            fp.WriteDouble(acc);
        }

        public override void Load(FileGetter fp)
        {
            i = fp.ReadInt();
            x = fp.ReadInt();
            y = fp.ReadInt();
            acc = fp.ReadDouble();
        }

        public override void Clear()
        {
            i = 0;
            x = 0;
            y = 0;
            acc = 0;
        }
    }
}