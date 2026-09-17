using System;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.rnd;
using world;
using world.map.regions;

namespace world.map.road
{
    class Gen
    {
        public void generateAll(int px, int py, ACTION astep)
        {
            WORLD.ROADS().saver().clear();
            astep.exe();

            MAP_DOUBLE roadCost = new MAP_DOUBLE
            {
                polly = new Polymap(TBOUNDS(), 6, 1),

                get = (int dx, int dy) =>
                {
                    double v = (0.4 + (polly.isEdge(dx, dy) ? 0 : 0.6)) * getTerrainCost(dx, dy);
                    Region r = WORLD.REGIONS().map.get(dx, dy);
                    for (int i = 0; i < DIR.ORTHO.size(); i++)
                        if (r != WORLD.REGIONS().map.get(dx, dy, DIR.ORTHO.get(i)))
                        {
                            v *= 2;
                            break;
                        }
                    if (WORLD.WATER().isBig.is(dx, dy))
                    {
                        v *= 2;
                        if (!WORLD.WATER().coversTile.is(dx, dy))
                            v *= 3;
                    }

                    return v;
                },

                getTerrainCost = (int tx, int ty) =>
                {
                    if (WORLD.WATER().isBig.is(tx, ty))
                        return 1;
                    if (WORLD.MOUNTAIN().heighter.get(tx, ty) >= 1)
                        return 12;
                    if (WORLD.FOREST().amount.get(tx, ty) == 1.0)
                        return 6;
                    return 3;
                },

                getTile = (int tile) =>
                {
                    // TODO Auto-generated method stub
                    return 0;
                }
            };

            astep.exe();
            new GenRoad(astep, roadCost);
            astep.exe();
            new GenPort(astep, roadCost);
            astep.exe();
            new GenPolish(astep, roadCost);
            astep.exe();
            new GenPlayer(astep);
            astep.exe();
        }
    }
}