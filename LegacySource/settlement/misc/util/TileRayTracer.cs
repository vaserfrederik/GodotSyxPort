using System;
using System.Collections.Generic;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

public sealed class TileRayTracer
{
    private readonly int radius;
    private readonly Ray[] rays;
    private readonly Ray[][][] raysOnTile;
    private readonly COORDINATE[] allTiles;
    private readonly Ray[] empty = Array.Empty<Ray>();
    private readonly short[,] check;
    private short checkI = 0;

    public TileRayTracer(int radius)
    {
        this.radius = radius;
        LinkedList<Ray> rays = new LinkedList<Ray>();

        bool[,] has = new bool[radius * 2 + 1, radius * 2 + 1];
        raysOnTile = new Ray[radius * 2 + 1][];
        for (int i = 0; i < raysOnTile.Length; i++)
        {
            raysOnTile[i] = new Ray[radius * 2 + 1];
        }

        for (int gy = -radius; gy <= radius; gy++)
        {
            RayTrace(-radius, gy, has, rays);
            RayTrace(radius, gy, has, rays);
        }

        for (int gx = -radius; gx <= radius; gx++)
        {
            RayTrace(gx, -radius, has, rays);
            RayTrace(gx, radius, has, rays);
        }

        this.rays = new Ray[rays.Count];
        int i = 0;
        while (rays.Count > 0)
        {
            this.rays[i] = new Ray(rays.RemoveFirst(), i);
            i++;
        }

        int[][] grid = Alloc.I2(radius * 2 + 1, radius * 2 + 1);
        int all = 0;
        foreach (Ray r in this.rays)
        {
            foreach (COORDINATE c in r.Coordinates)
            {
                if (grid[c.Y() + radius][c.X() + radius] == 0)
                    all++;
                grid[c.Y() + radius][c.X() + radius]++;
            }
        }

        allTiles = new COORDINATE[all];
        int tileI = 0;
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                int c = grid[y][x];
                if (c > 0)
                {
                    allTiles[tileI++] = new Coo(x - radius, y - radius);
                    raysOnTile[y][x] = new Ray[grid[y][x]];
                    grid[y][x] = 0;
                }
            }
        }

        foreach (Ray r in this.rays)
        {
            foreach (COORDINATE c in r.Coordinates)
            {
                raysOnTile[c.Y() + radius][c.X() + radius][grid[c.Y() + radius][c.X() + radius]] = r;
                grid[c.Y() + radius][c.X() + radius]++;
            }
        }

        check = new short[radius * 2 + 1, radius * 2 + 1];
    }

    public void CheckInit()
    {
        checkI++;
        if (checkI == 0)
        {
            for (int y = 0; y < check.GetLength(0); y++)
            {
                for (int x = 0; x < check.GetLength(1); x++)
                {
                    check[y, x] = 0;
                }
            }
            checkI = 1;
        }
    }

    public bool Check(COORDINATE c)
    {
        if (check[c.Y() + radius, c.X() + radius] != checkI)
        {
            check[c.Y() + radius, c.X() + radius] = checkI;
            return true;
        }
        return false;
    }

    public bool Checked(COORDINATE c)
    {
        return check[c.Y() + radius, c.X() + radius] == checkI;
    }

    public Ray[] Rays(int dx, int dy)
    {
        dx += radius;
        dy += radius;
        if (dx < 0 || dy < 0 || dy >= raysOnTile.Length || dx >= raysOnTile[dy].Length)
            return empty;
        return raysOnTile[dy][dx];
    }

    public Ray[] Rays()
    {
        return rays;
    }

    public int Radius()
    {
        return radius;
    }

    public COORDINATE[] Tiles()
    {
        return allTiles;
    }

    private void RayTrace(int fromx, int fromy, bool[,] has, LinkedList<Ray> rays)
    {
        double x = fromx;
        double y = fromy;
        double divider;

        if (Math.Abs(x) > Math.Abs(y))
        {
            divider = Math.Abs(x);
        }
        else if (Math.Abs(x) < Math.Abs(y))
        {
            divider = Math.Abs(y);
        }
        else
        {
            divider = Math.Abs(x);
        }

        double dx = (-x) / divider;
        double dy = (-y) / divider;

        for (int i = 0; i < divider; i++)
        {
            int tx = (int)x;
            int ty = (int)y;
            double r = Math.Floor(Math.Sqrt(x * x + y * y));

            if (r <= radius)
            {
                if (has[ty + radius, tx + radius])
                {
                    return;
                }
                has[ty + radius, tx + radius] = true;
                break;
            }

            x += dx;
            y += dy;
        }

        LinkedList<Coo> coos = new LinkedList<Coo>();
        LinkedList<Coo> offs = new LinkedList<Coo>();

        while (true)
        {
            int tx = (int)x;
            int ty = (int)y;
            if (tx == 0 && ty == 0)
            {
                Ray ray = new Ray(coos.Count);
                for (int i = ray.Coordinates.Length - 1; i >= 0; i--)
                {
                    ray.Coordinates[i] = coos.RemoveFirst();
                    ray.TileOffsets[i] = offs.RemoveFirst();
                    ray.Radii[i] = Math.Sqrt(ray.Coordinates[i].X() * ray.Coordinates[i].X() + ray.Coordinates[i].Y() * ray.Coordinates[i].Y());
                    ray.Areas[i] = ray.TileOffsets[i].X() * ray.TileOffsets[i].Y();
                    ray.Areas[i] /= C.TILE_SIZE * C.TILE_SIZE;
                }

                rays.Add(ray);
                return;
            }
            coos.Add(new Coo(tx, ty));
            offs.Add(new Coo(x - tx, y - ty));

            x += dx;
            y += dy;
        }
    }

    public sealed class Ray
    {
        private readonly COORDINATE[] coordinates;
        private readonly COORDINATE[] tileOffsets;
        private readonly double[] radii;
        private readonly double[] areas;
        public readonly int Index;

        public Ray(Ray other, int index)
        {
            this.Index = index;
            coordinates = other.coordinates;
            tileOffsets = other.tileOffsets;
            radii = other.radii;
            areas = other.areas;
        }

        public Ray(int size)
        {
            coordinates = new COORDINATE[size];
            tileOffsets = new COORDINATE[size];
            radii = new double[size];
            areas = new double[size];
            Index = 0;
        }

        public COORDINATE First()
        {
            return coordinates[0];
        }

        public COORDINATE Last()
        {
            return coordinates[coordinates.Length - 1];
        }

        public int Size()
        {
            return coordinates.Length;
        }

        public COORDINATE Get(int i)
        {
            return coordinates[i];
        }

        public COORDINATE[] Coordinates => coordinates;

        public double Radius(int i)
        {
            return radii[i];
        }

        public double TraverseArea(int i)
        {
            return 1.0;
        }
    }
}