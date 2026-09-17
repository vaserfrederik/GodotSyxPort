using System;
using System.IO;
using System.Linq;

namespace World.Map.Regions
{
    using static World.WORLD;
    using GAME = Game.GAME;
    using CLIMATE = Init.Type.CLIMATE;
    using CLIMATES = Init.Type.CLIMATES;
    using TERRAIN = Init.Type.TERRAIN;
    using TERRAINS = Init.Type.TERRAINS;
    using LOG = Snake2D.LOG;
    using PathTile = Snake2D.PathTile;
    using COORDINATE = Snake2D.Util.DataTypes.COORDINATE;
    using DIR = Snake2D.Util.DataTypes.DIR;
    using RECTANGLE = Snake2D.Util.DataTypes.RECTANGLE;
    using Rec = Snake2D.Util.DataTypes.Rec;
    using Alloc = Snake2D.Util.File.Alloc;
    using FileGetter = Snake2D.Util.File.FileGetter;
    using FilePutter = Snake2D.Util.File.FilePutter;
    using CLAMP = Snake2D.Util.Misc.CLAMP;
    using Str = Snake2D.Util.Sprite.Text.Str;
    using GUTIL = Util.GUTIL;
    using WORLD = World.WORLD;
    using Region = World.Map.Regions.Region;
    using WCentre = World.Map.Regions.Centre.WCentre;
    using WorldCentrePlacablity = World.Map.Regions.Centre.WorldCentrePlacablity;

    public sealed class RegionInfo
    {
        private static int[] countTerrain = Alloc.Ii(TERRAINS.ALL().Size());

        private static double bi = 1.0 / 0xFF;

        public const int nameSize = 24;
        private readonly Str name = new Str(nameSize);
        private int area;
        private readonly Rec bounds = new Rec();
        private short cx, cy;
        private byte fertility;
        private readonly byte[] climateTerrMin = Alloc.Bb(1 + TERRAINS.ALL().Size());

        private static Averages ave;

        public RegionInfo()
        {
            ave = null;
            Clear();
        }

        void Save(FilePutter f)
        {
            name.Save(f);
            f.S(cx).S(cy);
            bounds.Save(f);
            f.I(area);
            f.B(fertility);
            f.Bs(climateTerrMin);
        }

        void Load(FileGetter f) throws IOException
        {
            name.Load(f);
            cx = (short)f.S();
            cy = (short)f.S();
            bounds.Load(f);
            area = f.I();
            fertility = f.B();
            f.Bs(climateTerrMin);
            ave = null;
        }

        void Clear()
        {
            name.Clear();
            cx = -1;
            cy = -1;
            bounds.Clear();
            area = 0;
            fertility = 0;
            Array.Fill(climateTerrMin, (byte)0);
            ave = null;
        }

        public Str Name()
        {
            return name;
        }

        public int Cx()
        {
            return cx;
        }

        public int Cy()
        {
            return cy;
        }

        void CentreSet(int tx, int ty)
        {
            cx = (short)tx;
            cy = (short)ty;
            WORLD.REGIONS().dirty = true;
        }

        public int Area()
        {
            return area;
        }

        public RECTANGLE Bounds()
        {
            return bounds;
        }

        public double Moisture()
        {
            return (fertility & 0xFF) * bi;
        }

        public double Climate(CLIMATE c)
        {
            double ci = ClimateI();

            if ((int)ci == c.Index())
            {
                return 1 - (ci - (int)ci);
            }
            else if ((int)ci == c.Index() - 1)
            {
                return (ci - (int)ci);
            }
            return 0;
        }

        public double ClimateI()
        {
            return (CLIMATES.ALL().Size() - 1) * (climateTerrMin[0] & 0xFF) * bi;
        }

        public double Terrain(TERRAIN c)
        {
            return (climateTerrMin[1 + c.Index()] & 0xFF) * bi;
        }

        public bool Init(int sx, int sy, RECTANGLE body)
        {
            ave = null;
            WORLD.REGIONS().dirty = true;
            double climate = 0;
            Array.Fill(countTerrain, 0);

            double fertility = 0;

            if (WORLD.REGIONS().Map.Get(sx, sy).Info != this)
                throw new Exception();

            Region a = REGIONS().Map.Get(sx, sy);

            bounds.MoveX1Y1(sx, sy).SetDim(1);
            area = 0;

            foreach (COORDINATE c in body)
            {
                if (!REGIONS().Map.Is(c, a))
                {
                    continue;
                }
                climate += WORLD.CLIMATE().Getter.Get(c).Index();
                countTerrain[TERRAINS.World.Get(c).Index()]++;
                fertility += WORLD.MOISTURE().Get(c.X(), c.Y());
                area++;
                bounds.Unify(c.X(), c.Y());
            }

            Rec tmp = new Rec(bounds);
            tmp.Incr(-1, -1).Incr(1, 1);

            GUTIL.Flooder.Flood(new COORDINATE(sx, sy), tmp, (x, y) =>
            {
                // Flooding logic
                return true;
            });

            foreach (COORDINATE c in body)
            {
                // Processing logic
            }

            return true;
        }

        public static abstract class RegValue
        {
            public readonly double weight;
            public readonly double ave;
            public readonly double aveAI;
            public readonly double max;

            public RegValue()
            {
                double ave = 0;
                double aveAI = 0;
                double ma = 0;

                for (int ri = 0; ri < WORLD.REGIONS().Active().Size(); ri++)
                {
                    Region reg = WORLD.REGIONS().Active().Get(ri);
                    double v = Raw(reg);
                    ave += v;
                    aveAI += RawAI(reg);
                    ma = Math.Max(v, ma);
                }

                this.ave = ave / WORLD.REGIONS().Active().Size();
                this.aveAI = aveAI / WORLD.REGIONS().Active().Size();
                this.max = ma;
                if (Averages.log)
                {
                    LOG.Ln(this.ave + " " + this.max);
                }
                double w = 1 - this.ave / max;
                this.weight = 1.0 / w;
            }

            public abstract double Raw(Region reg);
            public abstract double RawAI(Region reg);

            public double GetAi(double v)
            {
                return v / aveAI;
            }

            public double GetAi(Region reg)
            {
                return GetAi(RawAI(reg));
            }

            public double Get(double v)
            {
                return CLAMP.D(Math.Sqrt(v / max), 0, 1);
            }

            public double Get(Region reg)
            {
                return Get(Raw(reg));
            }
        }
    }
}