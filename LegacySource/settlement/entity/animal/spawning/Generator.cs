using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Entity.Animal.Spawning
{
    using static Settlement.Main.SETT;

    using Init.Constant;
    using Init.Type;
    using Settlement.Entity.Animal;
    using Settlement.Main;
    using Snake2D.Path;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util;

    class Generator
    {
        private readonly int radius = 8;
        private readonly Rec bounds = new Rec(TWIDTH - radius * 2, THEIGHT - radius * 2).MoveX1Y1(radius, radius);

        public Generator(Animals asAnimals, CapitolArea carea, LIST<AnimalSpawnSpot> spots)
        {
            GUTIL.Flooder().Init(this);

            double[] terrains = new double[TERRAINS.ALL().Count];

            double tot = 0;
            foreach (COORDINATE c in bounds)
            {
                double ff = GetValue(c.X(), c.Y());
                if (ff >= 0)
                {
                    tot += ff;
                    if (Valid(c.X(), c.Y()))
                        GUTIL.Flooder().PushSloppy(c, ff * 0 - 5 + RND.rFloat()).SetValue2(0);
                }
                terrains[TERRAINS.sett.Get(c.X(), c.Y()).Index()]++;
            }

            double area = bounds.Width() * bounds.Height();
            tot /= area;
            tot = Math.Pow(tot, 0.5);
            foreach (TERRAIN t in TERRAINS.ALL())
                terrains[t.Index()] /= area;

            double otot = 0;
            double[] occ = new double[asAnimals.Species.Count];

            foreach (AnimalSpecies a in asAnimals.Species)
            {
                double o = 0;
                foreach (TERRAIN t in TERRAINS.ALL())
                    o += terrains[t.Index()] * a.Occurence(t);
                o *= a.Occurence(carea.Climate());
                otot += o;
                occ[a.Index()] = o;
            }

            if (otot <= 0)
                return;

            int[] amounts = Alloc.Ii(asAnimals.Species.Count);
            int[] caveAmouts = Alloc.Ii(asAnimals.Species.Count);
            foreach (AnimalSpecies a in asAnimals.Species)
            {
                occ[a.Index()] /= otot;
                double aa = (occ[a.Index()] * 75 * tot);
                int am = (int)aa;
                aa -= am;
                if (RND.rFloat() < aa)
                    am++;

                amounts[a.Index()] = am;
                caveAmouts[a.Index()] = (int)(am * a.CaveLiving);
            }

            LinkedList<AnimalSpawnSpot> list = new LinkedList<AnimalSpawnSpot>(spots);

            while (GUTIL.Flooder().HasMore() && !list.IsEmpty())
            {
                PathTile c = GUTIL.Flooder().PollGreatest();
                if (c.GetValue2() != 0)
                    continue;
                if (!Place(c.X(), c.Y(), amounts, caveAmouts, list))
                    break;
            }

            GUTIL.Flooder().Done();
        }

        private bool Place(int x, int y, int[] amounts, int[] camounts, LinkedList<AnimalSpawnSpot> list)
        {
            int roff = RND.rInt(SETT.ANIMALS().Species.Count);

            if (SETT.TERRAIN().Get(x, y).RoofIs())
            {
                for (int i = 0; i < SETT.ANIMALS().Species.Count; i++)
                {
                    int k = (roff + i) % amounts.Length;
                    int am = camounts[k];
                    if (am > 0)
                    {
                        AnimalSpawnSpot spot = list.RemoveFirst();
                        int a = Place(x, y, SETT.ANIMALS().Species.Get(k), am, spot);
                        if (a > 0)
                        {
                            spot.Init(x, y, a, SETT.ANIMALS().Species.Get(k));
                        }
                        else
                            list.Add(spot);
                        camounts[k] -= a;
                        amounts[k] -= a;
                        return true;
                    }
                }
                for (int i = 0; i < SETT.ANIMALS().Species.Count; i++)
                {
                    if (amounts[i] > 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            for (int i = 0; i < SETT.ANIMALS().Species.Count; i++)
            {
                int k = (roff + i) % amounts.Length;
                int am = amounts[k];
                if (am > 0)
                {
                    AnimalSpawnSpot spot = list.RemoveFirst();
                    int a = Place(x, y, SETT.ANIMALS().Species.Get(k), am, spot);
                    if (a > 0)
                    {
                        spot.Init(x, y, a, SETT.ANIMALS().Species.Get(k));
                    }
                    else
                        list.Add(spot);
                    amounts[k] -= a;
                    return true;
                }
            }
            return false;
        }

        private int Place(int cx, int cy, AnimalSpecies animal, int am, AnimalSpawnSpot spot)
        {
            Rec rec = new Rec(radius * 2);
            rec.MoveC(cx, cy);

            foreach (COORDINATE c in rec)
            {
                if (SETT.IN_BOUNDS(c))
                    GUTIL.Flooder().SetValue2(c, 1);
            }

            GUTIL.Coos().Set(0);

            int max = 40 + RND.rInt(20);
            if (max > am)
                max = am;
            else if (am - max < 5)
                max = am;
            am = max;

            int a = 0;
            for (int i = 0; GUTIL.Circle().Radius(i) < radius && a < am; i++)
            {
                if (!RND.OneIn(3 + GUTIL.Circle().Radius(i)))
                    continue;
                int x = cx + GUTIL.Circle().Get(i).X();
                int y = cy + GUTIL.Circle().Get(i).Y();
                if (!Valid(x, y))
                {
                    continue;
                }
                GUTIL.Coos().Get().Set(x, y);
                GUTIL.Coos().Inc();
                a++;
            }

            if (a >= am)
            {
                am = 0;
                int m = GUTIL.Coos().GetI();
                for (int i = 0; i < m; i++)
                {
                    GUTIL.Coos().Set(i);
                    int ax = GUTIL.Coos().Get().X() * C.TILE_SIZE + C.TILE_SIZEH;
                    int ay = GUTIL.Coos().Get().Y() * C.TILE_SIZE + C.TILE_SIZEH;
                    Animal an = new Animal(ax, ay, animal, spot);
                    if (!an.IsRemoved())
                        am++;
                }
                return am;
            }
            return 0;
        }

        private double GetValue(int x, int y)
        {
            double ff = SETT.GROUND().MOISTURE_BASE.Get(x, y);
            if (ff > 0.2)
            {
                return 0.5 + (ff - 0.2) / (0.8 * 2);
            }
            return -1;
        }

        private bool Valid(int x, int y)
        {
            if (!SETT.IN_BOUNDS(x, y))
                return false;
            if (SETT.PATH().Availability.Get(x, y).Player < 0)
                return false;
            if (TERRAIN().WATER.is.Is(x, y))
                return false;

            double ff = SETT.GROUND().MOISTURE_BASE.Get(x, y);
            if (ff > 0.2)
            {
                return true;
            }
            return false;
        }
    }
}