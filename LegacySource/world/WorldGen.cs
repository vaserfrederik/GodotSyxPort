using System;
using System.IO;
using System.Numerics;
using System.Drawing;
using System.Linq;

namespace World
{
    public class WorldGen : SAVABLE
    {
        public bool IsEditing = false;
        public bool HasGeneratedTerrain;
        public bool IsDone;
        public double Lat = 0.5;
        public string Map = null;
        public int Seed = RND.Seed();
        public int PlayerX, PlayerY;

        public WorldGen(WORLD world)
        {
            Clear();
        }

        public override void Save(FilePutter file)
        {
            file.Bool(HasGeneratedTerrain);
            file.Bool(IsEditing);
            file.D(Lat);
            if (Map != null)
            {
                file.Bool(true);
                file.Chars(Map);
            }
            else
            {
                file.Bool(false);
            }
            file.I(Seed);
            file.I(PlayerX);
            file.I(PlayerY);
            file.Bool(IsDone);
        }

        public override void Load(FileGetter file)
        {
            HasGeneratedTerrain = file.Bool();
            IsEditing = file.Bool();
            Lat = file.D();
            if (file.Bool())
            {
                Map = file.Chars();
            }
            else
            {
                Map = null;
            }
            Seed = file.I();
            PlayerX = file.I();
            PlayerY = file.I();
            IsDone = file.Bool();
        }

        public override void Clear()
        {
            HasGeneratedTerrain = false;
            Lat = 0.5;
            Map = null;
            Seed = RND.RInt(int.MaxValue);
            PlayerX = -1;
            PlayerY = -1;
            IsDone = false;
        }

        public static class WorldGenMapType
        {
            public readonly int DIM;
            private readonly byte[,] map;
            private readonly double ii;

            private static readonly COLOR[] cols = new COLOR[]
            {
                new ColorImp(25, 25, 50),
                new ColorImp(50, 60, 20),
                new ColorImp(30, 25, 25),
            };

            public readonly string Name;

            public WorldGenMapType(string name, int worldDim)
            {
                this.Name = name;
                string path = PATHS.SPRITE().GetFolder("world").GetFolder("generatorMaps").Get(name);
                SnakeImage im = new SnakeImage(path);
                DIM = im.Width;
                if (DIM != im.Height)
                    throw new Errors.DataError(PATHS.SPRITE().GetFolder("world").GetFolder("generatorMaps").Get(name).ToAbsolutePath() + " is not a square. Image must have the same width and height");
                map = new byte[DIM, DIM];
                for (int y = 0; y < im.Height; y++)
                {
                    for (int x = 0; x < im.Width; x++)
                    {
                        map[y, x] = (byte)((im.RGB.Get(x, y) >> 8) & 0x0FF);
                    }
                }
                ii = 1.0 / worldDim;
                im.Dispose();
            }

            private double G(int x, int y)
            {
                return (map[y, x] & 0x0FF) * ii;
            }

            private double DD(double d)
            {
                if (d < 0.5)
                    return -(0.5 - d) * 2;
                return (d - 0.5) * 2;
            }

            public double H(int x, int y)
            {
                return DD(G(x, y));
            }

            public double H(int x, int y, int w, int h)
            {
                double xx = x;
                xx /= w;
                x = (int)(xx * DIM);
                xx -= (int)xx;
                double yy = y;
                yy /= h;
                y = (int)(yy * DIM);
                yy -= (int)yy;

                //C
                double area = 0;
                double res = 0;
                {
                    double a = (1 - xx) * (1 - yy);
                    res += a * G(x, y);
                    area += a;
                }
                if (x + 1 < map.GetLength(0))
                {
                    double a = xx * (1 - yy);
                    res += a * G(x + 1, y);
                    area += a;
                }

                if (y + 1 < map.GetLength(0))
                {
                    double a = (1 - xx) * (yy);
                    res += a * G(x, y + 1);
                    area += a;
                }

                if (x + 1 < map.GetLength(0) && y + 1 < map.GetLength(0))
                {
                    double a = (xx) * (yy);
                    res += a * G(x + 1, y + 1);
                    area += a;
                }

                return DD(res / area);
            }

            public void Save(FilePutter f)
            {
                f.Bs(map);
            }

            public void Render(SPRITE_RENDERER r, int x1, int y1, int i)
            {
                for (int y = 0; y < DIM; y++)
                {
                    for (int x = 0; x < DIM; x++)
                    {
                        int e = (int)((map[y, x] & 0x0FF) * ii * cols.Length);
                        int xx = x1 + x * i;
                        int yy = y1 + y * i;
                        cols[e].Render(r, xx, xx + i, yy, yy + i);
                    }
                }
            }

            public static WorldGenMapType[] GetAll(int worldDim)
            {
                PATH p = PATHS.SPRITE().GetFolder("world").GetFolder("generatorMaps");
                string[] files = p.GetFiles();
                WorldGenMapType[] res = new WorldGenMapType[files.Length];
                for (int i = 0; i < files.Length; i++)
                    res[i] = new WorldGenMapType(files[i], worldDim);
                return res;
            }
        }
    }
}