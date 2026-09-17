using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Placement
{
    public static class UtilWallPlacability
    {
        private UtilWallPlacability()
        {
        }

        public static readonly MAP_BOOLEAN openingIsReal = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                return IN_BOUNDS(tx, ty) && !ROOMS().Map.Is(tx, ty) && Get(tx, ty).RoofIs();
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public static readonly MAP_BOOLEAN openingCanBe = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                if (openingIsReal.Is(tx, ty))
                    return true;
                if (SETT.TERRAIN().MOUNTAIN.Is(tx, ty))
                    return Placable(SETT.JOBS().Clearss.Tunnel, tx, ty) == null;
                else
                    return Placable(SETT.JOBS().BuildStructure[0].Ceiling, tx, ty) == null;
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public static readonly MAP_BOOLEAN openingShouldBuild = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                if (openingIsReal.Is(tx, ty))
                    return false;
                return openingCanBe.Is(tx, ty);
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public static readonly MAP_BOOLEAN wallisReal = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                return IN_BOUNDS(tx, ty) && !ROOMS().Map.Is(tx, ty) && Get(tx, ty).IsMassiveWall();
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public static readonly MAP_BOOLEAN wallCanBe = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                if (wallisReal.Is(tx, ty))
                    return true;
                if (SETT.TERRAIN().CAVE.Is(tx, ty))
                    return Placable(SETT.JOBS().Clearss.CaveFill, tx, ty) == null;
                else
                    return Placable(SETT.JOBS().BuildStructure[0].Wall, tx, ty) == null;
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        public static readonly MAP_BOOLEAN wallShouldBuild = new MAP_BOOLEAN
        {
            Is = (tx, ty) =>
            {
                if (wallisReal.Is(tx, ty))
                    return false;
                return wallCanBe.Is(tx, ty);
            },
            IsTile = tile =>
            {
                return Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        private static TerrainTile Get(int tx, int ty)
        {
            if (JOBS().Getter.Is(tx, ty))
                return JOBS().Getter.Get(tx, ty).Becomes(tx, ty);
            return TERRAIN().Get(tx, ty);
        }

        private static string Placable(Job j, int tx, int ty)
        {
            bool over = Job.Overwrite;
            Job.Overwrite = true;
            string s = j.Placer().IsPlacable(tx, ty, null, null);
            Job.Overwrite = over;
            return s;
        }

        public static void WallBuild(int tx, int ty, TBuilding building)
        {
            if (SETT.ROOMS().Map.Is(tx, ty))
                return;
            if (SETT.TERRAIN().MOUNTAIN.Is(tx, ty))
            {
                SETT.JOBS().Clearer.Set(tx, ty);
                return;
            }
            if (building.Wall.Is(tx, ty))
            {
                SETT.JOBS().Clearer.Set(tx, ty);
                return;
            }

            if (SETT.TERRAIN().CAVE.Is(tx, ty))
            {
                if (Placable(SETT.JOBS().Clearss.CaveFill, tx, ty) == null)
                    SETT.JOBS().Clearss.CaveFill.Placer().Place(tx, ty, null, null);
            }
            else if (SETT.TERRAIN().Get(tx, ty) != building.Wall && Placable(SETT.JOBS().BuildStructure[building.Structure.Index].Wall, tx, ty) == null)
                SETT.JOBS().BuildStructure[building.Structure.Index].Wall.Placer().Place(tx, ty, null, null);
        }

        public static void OpeningBuild(int tx, int ty, TBuilding building)
        {
            if (SETT.ROOMS().Map.Is(tx, ty))
                return;
            if (SETT.TERRAIN().CAVE.Is(tx, ty))
            {
                SETT.JOBS().Clearer.Set(tx, ty);
                return;
            }
            if (building.Roof.Is(tx, ty))
            {
                SETT.JOBS().Clearer.Set(tx, ty);
                return;
            }

            if (SETT.TERRAIN().MOUNTAIN.Is(tx, ty))
            {
                if (Placable(SETT.JOBS().Clearss.Tunnel, tx, ty) == null)
                    SETT.JOBS().Clearss.Tunnel.Placer().Place(tx, ty, null, null);
            }
            else if (Placable(SETT.JOBS().BuildStructure[building.Structure.Index].Ceiling, tx, ty) == null)
                SETT.JOBS().BuildStructure[building.Structure.Index].Ceiling.Placer().Place(tx, ty, null, null);
        }
    }

    public class MAP_BOOLEAN
    {
        public Func<int, int, bool> Is { get; set; }
        public Func<int, bool> IsTile { get; set; }
    }
}