using System;
using System.Collections.Generic;
using game.battle;
using init.constant;
using init.type;
using settlement.main;
using settlement.room.main;
using settlement.room.military.artillery;
using settlement.room.spirit.grave;
using settlement.stats;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;
using snake2d.util.map;
using view.sett;
using view.tool;

public sealed class TargetMap
{
    public TargetMap()
    {
        IDebugPanelSett.Add(new PlacableMulti("break something")
        {
            public void Place(int tx, int ty, AREA a, PLACER_TYPE t)
            {
                BreakIt(tx, ty);
            }

            public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
            {
                return Attackable.Is(tx, ty, GAME.ARMIES().Player()) || Attackable.Is(tx, ty, GAME.ARMIES().Enemy()) ? null : E;
            }
        });
    }

    public readonly MAP_OBJECT<Army> Army = new MAP_OBJECT<Army>()
    {
        public Army Get(int tile)
        {
            Room r = SETT.ROOMS().map.Get(tile);
            if (r != null)
            {
                if (r is ArtilleryInstance)
                {
                    return ((ArtilleryInstance)r).Army();
                }
                return GAME.ARMIES().Player();
            }

            return SETT.PATH().availability.Get(tile).Player < 0 ? GAME.ARMIES().Player() : null;
        }

        public Army Get(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return null;
            return Get(tx + ty * SETT.TWIDTH);
        }
    };

    public readonly MAP_OBJECT_ISSER<Army> Attackable = new MAP_OBJECT_ISSER<Army>()
    {
        public bool Is(int tx, int ty, Army value)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return false;
            return Is(tx + ty * SETT.TWIDTH, value);
        }

        public bool Is(int tile, Army value)
        {
            Room r = SETT.ROOMS().map.Get(tile);
            if (r != null)
            {
                if (r is ArtilleryInstance)
                {
                    return ((ArtilleryInstance)r).Army() != value;
                }
                return GAME.ARMIES().Player() != value;
            }

            if (value == GAME.ARMIES().Player())
            {
                if (SETT.TERRAIN().Get(tile).Clearing().IsStructure())
                    return false;
            }

            if (SETT.PATH().availability.Get(tile).IsSolid(value) && SETT.TERRAIN().Get(tile).Clearing().CanDestroy(tile % SETT.TWIDTH, tile / SETT.TWIDTH))
                return true;

            return false;
        }
    };

    public readonly MAP_OBJECT_ISSER<Induvidual> AttackableI = new MAP_OBJECT_ISSER<Induvidual>()
    {
        public bool Is(int tx, int ty, Induvidual value)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return false;
            return Is(tx + ty * SETT.TWIDTH, value);
        }

        public bool Is(int tile, Induvidual in)
        {
            Army value = in.Army();
            Room r = SETT.ROOMS().map.Get(tile);
            if (r != null)
            {
                if (in.HType() == HTYPES.RIOTER() && r.Blueprint() is GraveData.GRAVE_DATA_HOLDER)
                    return false;
                if (r is ArtilleryInstance)
                {
                    return ((ArtilleryInstance)r).Army() != value;
                }
                return GAME.ARMIES().Player() != value;
            }

            if (value == GAME.ARMIES().Player())
            {
                if (SETT.TERRAIN().Get(tile).Clearing().IsStructure())
                    return false;
            }

            if (SETT.PATH().availability.Get(tile).IsSolid(value) && SETT.TERRAIN().Get(tile).Clearing().CanDestroy(tile % SETT.TWIDTH, tile / SETT.TWIDTH))
                return true;

            return false;
        }
    };

    public readonly MAP_DOUBLE Strength = new MAP_DOUBLE()
    {
        public double Get(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
            {
                return 0;
            }
            return Get(tx + ty * SETT.TWIDTH);
        }

        public double Get(int tile)
        {
            RoomBlueprint p = SETT.ROOMS().map.Blueprint.Get(tile);
            if (p != null)
            {
                return p.Strength(tile);
            }
            return SETT.TERRAIN().Get(tile).Clearing().Strength();
        }
    };

    public void BreakIt(int x, int y)
    {
        Room r = ROOMS().map.Get(x, y);
        if (r != null && r.DestroyTileCan(x, y))
        {
            if (r.DestroyTileCan(x, y))
            {
                THINGS().gore.Debris((x << C.T_SCROLL) + C.TILE_SIZEH, (y << C.T_SCROLL) + C.TILE_SIZEH, 0, 0);
                r.DestroyTile(x, y);
                SETT.JOBS().tool_repair.Place(x, y, null, null);
            }
            return;
        }

        TerrainTile b = SETT.TERRAIN().Get(x, y);
        if (b.Clearing().CanDestroy(x, y))
        {
            THINGS().gore.Debris((x << C.T_SCROLL) + C.TILE_SIZEH, (y << C.T_SCROLL) + C.TILE_SIZEH, 0, 0);
            b.Clearing().Destroy(x, y);
            SETT.JOBS().tool_repair.Place(x, y, null, null);
            return;
        }
    }
}