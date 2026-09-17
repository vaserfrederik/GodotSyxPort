using System;
using init.resources;
using init.sprite;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d.util.sprite;

namespace settlement.misc.placers
{
    final class Tiles
    {
        static readonly Tile __ = new Tile
        {
            place = (tx, ty, grid, rx, ry) => { },
            placable = (tx, ty, grid, rx, ry) => !SETT.PATH().solidity.is(tx, ty),
            sprite = (grid, rx, ry, mask) => SPRITES.cons().BIG.dashed.get(0)
        };

        static readonly Tile xx = new Tile
        {
            place = (tx, ty, grid, rx, ry) => SETT.TERRAIN().NADA.placeFixed(tx, ty),
            placable = (tx, ty, grid, rx, ry) => true,
            sprite = (grid, rx, ry, mask) => SPRITES.cons().BIG.dashed.get(0)
        };

        static readonly Tile na = new Tile
        {
            place = (tx, ty, grid, rx, ry) => { },
            placable = (tx, ty, grid, rx, ry) => true,
            sprite = (grid, rx, ry, mask) => SPRITES.cons().BIG.dashed.get(0)
        };

        private Tiles()
        {
        }

        public static class Terrain : Tile
        {
            public readonly TerrainTile t;

            public Terrain(TerrainTile t)
            {
                this.t = t;
            }

            public override bool placable(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                return true;
            }

            public override void place(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                t.placeFixed(tx, ty);
                if (t.clearing().isStructure())
                    SETT.GRASS().current.set(tx, ty, 0);
            }

            public override SPRITE sprite(TileGrid grid, int rx, int ry, int mask)
            {
                return SPRITES.cons().BIG.dashedThick.get(0);
            }
        }

        public static class Floor : Tile
        {
            public readonly settlement.tilemap.floor.Floors.Floor f;
            public readonly double degrade;

            public Floor(settlement.tilemap.floor.Floors.Floor f)
                : this(f, 1)
            {
            }

            public Floor(settlement.tilemap.floor.Floors.Floor f, double degrade)
            {
                this.f = f;
                this.degrade = degrade;
            }

            public override bool placable(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                return true;
            }

            public override void place(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                f.placeFixed(tx, ty);
                if (SETT.TERRAIN().get(tx, ty).clearing().isEasilyCleared())
                    SETT.TERRAIN().NADA.placeFixed(tx, ty);
            }

            public override SPRITE sprite(TileGrid grid, int rx, int ry, int mask)
            {
                return SPRITES.cons().BIG.dashed.get(0);
            }
        }

        public static class Resource : Tile
        {
            public readonly RESOURCE r;
            public readonly int amount;

            public Resource(RESOURCE r, int amount)
            {
                this.r = r;
                this.amount = amount;
            }

            public override bool placable(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                return true;
            }

            public override void place(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                SETT.THINGS().resources.createPrecise(tx, ty, r, amount);
            }

            public override SPRITE sprite(TileGrid grid, int rx, int ry, int mask)
            {
                return SPRITES.cons().ICO.clear;
            }
        }

        public static class Conpound : Tile
        {
            public readonly Tile[] tiles;

            public Conpound(params Tile[] tiles)
            {
                this.tiles = tiles;
            }

            public override bool placable(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                foreach (Tile t in tiles)
                    if (!t.placable(tx, ty, grid, rx, ry))
                        return false;
                return true;
            }

            public override void place(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                foreach (Tile t in tiles)
                    t.place(tx, ty, grid, rx, ry);
            }

            public override SPRITE sprite(TileGrid grid, int rx, int ry, int mask)
            {
                return SPRITES.cons().ICO.clear;
            }
        }
    }
}