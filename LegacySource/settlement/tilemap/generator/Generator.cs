using System;
using System.Collections.Generic;
using static settlement.main.SETT;

namespace settlement.tilemap.generator
{
    public static class Generator
    {
        private static readonly string ¤¤Generating = "Generating";

        static Generator()
        {
            D.ts(typeof(Generator));
        }

        public Generator(CapitolArea area)
        {
            AvailabilityListener.ListenAll(false);
            SPRITES.Loader().Print(¤¤Generating);

            TileMap m = TILE_MAP();
            GeneratorUtil util = new GeneratorUtil();

            print("fertilizing");
            new GeneratorFertilityInit(area, util);

            LinkedList<COORDINATE> caves = new LinkedList<COORDINATE>();
            print("mountainizing");
            new GeneratorMountain(area, util);
            print("making caves");
            new GeneratorCave(area, util, caves);

            print("Generating Rivers");
            new GeneratorRiver(area, util);
            new GeneratorRiverSmall(area, util);
            print("filling lakes");
            new GeneratorLake(area, util);
            new GeneratorWaterFin(area, util);

            print("filling oceans");
            new GeneratorOcean(area, util);

            if (!area.isBattle)
                new GeneratorLakeExtra(area, util);

            SPRITES.Loader().Print("fish...");
            new GeneratorFish(area, util);

            print("mineralizing");
            new GeneratorMinerals(area, util);

            print("fertilizing");
            new GeneratorGround(area, util);

            SPRITES.Loader().Print("fertilizing again...");
            new GeneratorFertilityFin(area, util);

            print("planting seeds");

            new GeneratorGrowth();
            if (!area.isBattle)
                new GeneratorEdibles(area, util, caves);

            foreach (COORDINATE c in TILE_BOUNDS)
            {
                if (TERRAIN().NADA.Is(c) && MINERALS().AmountInt.Get(c) == 0)
                {
                    Grower g = TILE_MAP().Growth.Type(c.X, c.Y);
                    if (g != null)
                    {
                        double a = TILE_MAP().Growth.GrowMaxAmount(c.X, c.Y, g);
                        if (a > 0)
                        {
                            g.SetRoots(c.X, c.Y, a);
                            TerrainTile t = TERRAIN().Get(c);
                            if (t is TGrowable)
                            {
                                TGrowable gg = (TGrowable)t;
                                gg.Resource.Set(c, gg.Size.Get(c));
                            }
                        }
                    }
                }
            }

            print("roads");
            new GeneratorRoads(area);

            for (int y = 0; y < THEIGHT; y++)
            {
                for (int x = 0; x < TWIDTH; x++)
                {
                    if (TERRAIN().NADA.Is(x, y) && !TERRAIN().WATER.GroundWater.Is(x, y) && !TERRAIN().WATER.GroundWaterSalt.Is(x, y))
                    {
                        if (GROUND().MOISTURE_BASE.Get(x, y) + 0.5 + ENV().Map.WATER_SWEET.Get(x, y) > 0.2)
                        {
                            if (RND.OneIn(50))
                                TERRAIN().DECOR_MID.PlaceRaw(x, y);
                        }
                        else if (RND.OneIn(200))
                        {
                            TERRAIN().DECOR_NO.PlaceRaw(x, y);
                        }
                    }
                }
            }

            print("polishing..");

            for (int y = 0; y < THEIGHT; y++)
            {
                for (int x = 0; x < TWIDTH; x++)
                {
                    m.Topology.Get(x, y).PlaceFixed(x, y);
                    PATH().Availability.UpdateAvailability(x, y);
                }
            }

            print("painting minimap...");

            paintMinimap();

            AvailabilityListener.ListenAll(true);
        }

        private int printI = 0;

        private void print(string debug)
        {
            string s = "" + ¤¤Generating;
            for (int i = 0; i < printI; i++)
                s += ".";
            if (S.Get().developer || S.Get().debug)
            {
                s += " " + debug;
            }
            printI++;
            printI %= 6;
            SPRITES.Loader().Print(s);
        }

        public static void paintMinimap()
        {
            byte[] cs = new byte[SETT.TWIDTH * SETT.TWIDTH * 4];
            for (int y = 0; y < SETT.TWIDTH; y++)
            {
                for (int x = 0; x < SETT.TWIDTH; x++)
                {
                    int i = (y * SETT.TWIDTH + x) * 4;

                    COLOR c = TILE_MAP().MiniC(x, y);

                    cs[i + 0] = c.Red();
                    cs[i + 1] = c.Green();
                    cs[i + 2] = c.Blue();
                    cs[i + 3] = (byte)255;
                }
            }
            MINIMAP().PutPixels(cs);
        }
    }
}