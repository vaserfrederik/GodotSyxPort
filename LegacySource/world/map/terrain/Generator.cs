using System;
using game;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using world;
using world.WorldGen;

namespace world.map.terrain
{
    final class Generator
    {
        public Generator(WorldGen spec, ACTION loadprint)
        {
            generateAll(spec, loadprint);
            loadprint.exe();
            new GeneratorValidator(null);
            loadprint.exe();
            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    WATER().get(x, y).pplace(x, y);
                    WATER().get(x, y - 1).pplace(x, y - 1);
                    WATER().get(x - 1, y).pplace(x - 1, y);
                    MOUNTAIN().fix(x, y);
                }
            }
            WORLD.MINIMAP().repaint();
        }

        public void clear()
        {
            WORLD.GROUND().clear();
            WORLD.MOUNTAIN().clear();
            WORLD.FOREST().clear();
            WORLD.WATER().clear();
        }

        public void generateAll(WorldGen spec, ACTION loadprint)
        {
            loadprint.exe();
            clear();
            RND.setSeed(spec.seed);

            loadprint.exe();

            HeightMap height = new HeightMap(TWIDTH(), THEIGHT(), TWIDTH() / 8, 4);
            if (spec.map != null)
            {
                WorldGenMapType m = new WorldGenMapType(spec.map, TWIDTH());

                foreach (COORDINATE c in TBOUNDS())
                {
                    height.increment(c, m.h(c.x(), c.y(), TWIDTH(), THEIGHT()));
                }
            }
            else
            {
                new GeneratorHeight(height, spec);
            }

            loadprint.exe();
            new GeneratorMountains(height, spec);
            loadprint.exe();

            loadprint.exe();
            new GeneratorOcean(height);

            //new GeneratorElevator(spec, height);
            //RES.loader().print("elevating...");
            //new GeneratorOcean(spec, height);

            loadprint.exe();
            new GeneratorRiver();
            loadprint.exe();
            new GeneratorValidator(null);

            for (int y = 0; y < THEIGHT(); y++)
            {
                for (int x = 0; x < TWIDTH(); x++)
                {
                    WATER().get(x, y).pplace(x, y);
                    WATER().get(x, y - 1).pplace(x, y - 1);
                    WATER().get(x - 1, y).pplace(x - 1, y);
                    MOUNTAIN().fix(x, y);
                }
            }

            float[,] fertility = new float[THEIGHT(), TWIDTH()];
            loadprint.exe();
            new GeneratorSeasoner(GAME.world(), spec.lat, fertility);
            loadprint.exe();
            new GeneratorForest(fertility);
            loadprint.exe();
        }
    }
}