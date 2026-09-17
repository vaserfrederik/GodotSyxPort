using System;
using System.IO;

namespace World.Map.Terrain
{
    public enum CLIMATE
    {
        // Define your climates here
    }

    public static class CLIMATES
    {
        public static CLIMATE[] ALL()
        {
            // Return all climates
            throw new NotImplementedException();
        }
    }

    public enum TERRAIN
    {
        NONE,
        // Define other terrains here
    }

    public static class TERRAINS
    {
        public static TERRAIN[] ALL()
        {
            // Return all terrains
            throw new NotImplementedException();
        }
    }

    public static class CLAMP
    {
        public static double d(double value, double min, double max)
        {
            return Math.Clamp(value, min, max);
        }
    }

    public class DoubleImp
    {
        private double value;

        public void setD(double value)
        {
            this.value = value;
        }

        public void incD(double increment)
        {
            this.value += increment;
        }

        public double getD()
        {
            return this.value;
        }
    }

    public class WORLD
    {
        public static DoubleImp MOISTURE()
        {
            // Return moisture data
            throw new NotImplementedException();
        }

        public static DoubleImp FOREST()
        {
            // Return forest data
            throw new NotImplementedException();
        }

        public static DoubleImp MOUNTAIN()
        {
            // Return mountain data
            throw new NotImplementedException();
        }

        public static DoubleImp WATER()
        {
            // Return water data
            throw new NotImplementedException();
        }

        public static DoubleImp CLIMATE()
        {
            // Return climate data
            throw new NotImplementedException();
        }
    }

    public class WCentre
    {
        public const int TILE_DIM = 10; // Define the tile dimension
    }

    public sealed class WorldTerrainInfo
    {
        private readonly DoubleImp[] terrain;
        private readonly DoubleImp fertility;
        private readonly DoubleImp[] climates;
        public int tx;
        public int ty;

        public WorldTerrainInfo()
        {
            terrain = new DoubleImp[TERRAINS.ALL().Length];
            for (int i = 0; i < terrain.Length; i++)
            {
                terrain[i] = new DoubleImp();
            }

            climates = new DoubleImp[CLIMATES.ALL().Length];
            for (int i = 0; i < climates.Length; i++)
            {
                climates[i] = new DoubleImp();
            }

            fertility = new DoubleImp();
        }

        public void Clear()
        {
            for (int i = 0; i < terrain.Length; i++)
            {
                terrain[i].setD(0);
            }

            for (int i = 0; i < climates.Length; i++)
            {
                climates[i].setD(0);
            }

            fertility.setD(0);
        }

        public void InitCity(int x1, int y1)
        {
            Clear();
            for (int y = 0; y < WCentre.TILE_DIM; y++)
            {
                for (int x = 0; x < WCentre.TILE_DIM; x++)
                {
                    int tx = x + x1;
                    int ty = y + y1;
                    Add(tx, ty);
                }
            }

            tx = x1 + 1;
            ty = y1 + 1;
            double d = WCentre.TILE_DIM * WCentre.TILE_DIM;
            Divide(d);
        }

        public void Add(int tx, int ty)
        {
            fertility.incD(WORLD.MOISTURE().getD());

            double f = 0;
            f += WORLD.FOREST().add(this, tx, ty).getD();
            f += WORLD.MOUNTAIN().add(this, tx, ty).getD();
            f += WORLD.WATER().add(this, tx, ty).getD();

            Add(TERRAIN.NONE, CLAMP.d(1.0 - f, 0, 1));

            climates[WORLD.CLIMATE().get(tx, ty).index()].incD(1);
        }

        public void Add(TERRAIN t, double v)
        {
            terrain[(int)t].incD(v);
        }

        public DoubleImp Get(TERRAIN t)
        {
            return terrain[(int)t];
        }

        public DoubleImp Get(CLIMATE c)
        {
            return climates[(int)c];
        }

        public DoubleImp Fertility()
        {
            return fertility;
        }

        public void Divide(double d)
        {
            for (int i = 0; i < terrain.Length; i++)
            {
                terrain[i].setD(terrain[i].getD() / d);
            }

            for (int i = 0; i < climates.Length; i++)
            {
                climates[i].setD(climates[i].getD() / d);
            }

            fertility.setD(fertility.getD() / d);
        }
    }
}