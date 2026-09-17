using System;
using snake2d.util.datatypes;
using view.tool;

namespace world.map.regions.centre
{
    public static class WorldCentrePlacablity
    {
        private WorldCentrePlacablity()
        {
        }

        private static readonly int TILE_DIM = WCentre.TILE_DIM;
        private static readonly RECTANGLEE TILES = new Rec(WCentre.TILE_DIM, WCentre.TILE_DIM);

        public static string TerrainC(int tx, int ty)
        {
            return Terrain(tx - TILE_DIM / 2, ty - TILE_DIM / 2);
        }

        public static string Terrain(int tileX1, int tileY1)
        {
            if (tileX1 < 1 || tileY1 < 1 || tileX1 + TILE_DIM >= WORLD.TWIDTH() || tileY1 + TILE_DIM >= WORLD.THEIGHT())
                return PlacableMessages.¤¤IN_MAP;

            int cx = tileX1 + TILE_DIM / 2;
            int cy = tileY1 + TILE_DIM / 2;

            if (Can(cx, cy) != null)
            {
                return Can(cx, cy);
            }

            bool oneClear = false;
            for (int di = 0; di < DIR.ORTHO.Size && !oneClear; di++)
            {
                if (WTRAV.IsGoodLandTile(cx + DIR.ORTHO.Get(di).X(), cy + DIR.ORTHO.Get(di).Y()) && WTRAV.CanLand(cx, cy, DIR.ORTHO.Get(di), false))
                    oneClear = true;
            }

            if (!oneClear)
                return PlacableMessages.¤¤ONE_CLEAR_TILE;

            return null;
        }

        private static string Can(int tx, int ty)
        {
            if (!WTRAV.IsGoodLandTile(tx, ty))
            {
                if (!WORLD.WATER().RIVER.Is(tx, ty) || WORLD.MOUNTAIN().Is(tx, ty))
                    return PlacableMessages.¤¤TERRAIN_BLOCK;
            }
            return null;
        }

        public static string RegionC(int tx, int ty)
        {
            return Region(tx - TILE_DIM / 2, ty - TILE_DIM / 2);
        }

        public static string RegionMiniC(int tx, int ty)
        {
            return RegionMini(tx - TILE_DIM / 2, ty - TILE_DIM / 2);
        }

        public static string RegionMini(int tileX1, int tileY1)
        {
            string t = Terrain(tileX1, tileY1);
            if (t != null)
                return t;
            Region r = WORLD.REGIONS().Map.Get(tileX1, tileY1);
            if (r == null)
                return PlacableMessages.¤¤REGION;

            for (int y = tileY1; y < tileY1 + TILE_DIM; y++)
            {
                for (int x = tileX1; x < tileX1 + TILE_DIM; x++)
                {
                    if (r != WORLD.REGIONS().Map.Get(x, y))
                    {
                        return PlacableMessages.¤¤SAME_REGION;
                    }
                }
            }

            return null;
        }

        public static string Region(int tileX1, int tileY1)
        {
            string t = RegionMini(tileX1, tileY1);
            if (t != null)
                return t;
            Region r = WORLD.REGIONS().Map.Get(tileX1, tileY1);
            if (r == null)
                return PlacableMessages.¤¤REGION;

            for (int y = tileY1 - 1; y < tileY1 + TILE_DIM + 1; y++)
            {
                for (int x = tileX1 - 1; x < tileX1 + TILE_DIM + 1; x++)
                {
                    if (r != WORLD.REGIONS().Map.Get(x, y) && WORLD.REGIONS().Map.Get(x, y) != null)
                    {
                        return PlacableMessages.¤¤SAME_REGION;
                    }
                }
            }
            return null;
        }

        public static RECTANGLE TilesC(int cx, int cy)
        {
            TILES.MoveX1Y1(cx - TILE_DIM / 2, cy - TILE_DIM / 2);
            return TILES;
        }
    }
}