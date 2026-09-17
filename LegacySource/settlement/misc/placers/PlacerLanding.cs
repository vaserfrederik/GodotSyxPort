using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using init.paths;
using init.resources;
using init.sprite;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.placers;
using settlement.stats;
using settlement.tilemap.terrain;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using view.main;

namespace settlement.misc.placers
{
    internal static class PlacerLanding
    {
        static Placer Get()
        {
            TBuilding b = SETT.TERRAIN().BUILDINGS.MUD;
            if (b == null)
                b = TERRAIN().BUILDINGS.all()[0];

            Tile ww = new Tiles.Terrain(b.wall)
            {
                public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    if (TERRAIN().CAVE.is(tx, ty))
                    {
                        TERRAIN().MOUNTAIN.placeFixed(tx, ty);
                    }
                    else
                    {
                        base.Place(tx, ty, grid, rx, ry);
                    }
                }
            };

            Tile roof = new Tiles.Terrain(b.roof)
            {
                public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    if (TERRAIN().CAVE.is(tx, ty))
                    {
                        return;
                    }
                    base.Place(tx, ty, grid, rx, ry);
                }
            };

            Tile rr = new Tiles.Conpound(roof);

            Tile throne = new Tile()
            {
                public override SPRITE Sprite(TileGrid grid, int rx, int ry, int mask)
                {
                    return SPRITES.cons().ICO.cancel;
                }

                public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    ROOMS().THRONE.init.place(tx, ty, 2);

                    FACTIONS.player().credits().inc((int)(5000 * BOOSTABLES.CIVICS().LANDING.get(HCLASS_RACE.clP(null, null))), FCredits.CTYPE.MISC);
                }

                public override bool Placable(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    return ROOMS().THRONE.init.placableTile(tx, ty);
                }
            };

            Tile thone2 = new Tile()
            {
                public override SPRITE Sprite(TileGrid grid, int rx, int ry, int mask)
                {
                    return SPRITES.cons().ICO.cancel;
                }

                public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                }

                public override bool Placable(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    return ROOMS().THRONE.init.placableTile(tx, ty);
                }
            };

            Tile th = new Conpound(roof, throne);
            Tile to = new Conpound(roof, thone2);
            Json j = new Json(PATHS.INIT().getFolder("config").gets("LandingParty")).json("RESOURCES");
            LIST<string> keys = j.keys();

            Tile R1 = new Conpound(rr, new Resource(j, keys, 0));
            Tile R2 = new Conpound(rr, new Resource(j, keys, 1));
            Tile R3 = new Conpound(rr, new Resource(j, keys, 2));
            Tile R4 = new Conpound(rr, new Resource(j, keys, 3));
            Tile R5 = new Conpound(rr, new Resource(j, keys, 4));
            Tile R6 = new Conpound(rr, new Resource(j, keys, 5));
            Tile R7 = new Conpound(rr, new Resource(j, keys, 6));
            Tile R8 = new Conpound(rr, new Resource(j, keys, 7));
            Tile R9 = new Conpound(rr, new Resource(j, keys, 8));
            Tile RA = new Conpound(rr, new Resource(j, keys, 9));

            Tile dd = new Tile()
            {
                private int am = -1;
                private int ePerTile = -1;

                public override SPRITE Sprite(TileGrid grid, int rx, int ry, int mask)
                {
                    return SPRITES.cons().ICO.cancel;
                }

                public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    if (STATS.POP().POP.data(null).get(null) == 0)
                    {
                        am = 10 + (int)(10 * BOOSTABLES.CIVICS().LANDING.get(HCLASS_RACE.clP(null, null)));

                        ePerTile = (int)Math.Ceiling((double)(am) / 10.0);
                    }
                    for (int i = 0; i < ePerTile; i++)
                    {
                        if (am <= 0)
                            continue;
                        Humanoid h = HUMANOIDS().create(FACTIONS.player().race(), tx, ty, HTYPES.SUBJECT(), CAUSE_ARRIVES.IMMIGRATED());
                        STATS.POP().TYPE.IMMIGRANT.set(h.indu());
                        am--;
                    }
                    VIEW.messages().hide();
                }

                public override bool Placable(int tx, int ty, TileGrid grid, int rx, int ry)
                {
                    return !PATH().solidity.is(tx, ty);
                }
            };

            Tile[,] grid = new Tile[,]
            {
                { ww, ww, ww, ww, ww, ww, ww, ww, ww },
                { ww, rr, th, to, to, to, to, rr, ww },
                { ww, rr, to, to, to, to, to, rr, ww },
                { ww, rr, to, to, to, to, to, rr, ww },
                { ww, rr, R7, R3, R1, R2, R8, rr, ww },
                { ww, rr, R9, R5, R4, R6, RA, rr, ww },
                { ww, rr, rr, rr, rr, rr, rr, rr, ww },
                { ww, ww, ww, rr, rr, rr, ww, ww, ww },
                { __, dd, __, __, __, __, __, dd, __ },
                { __, dd, dd, __, __, __, dd, dd, __ },
                { __, dd, dd, __, __, __, dd, dd, __ },
                { __, __, __, __, __, __, __, __, __ },
            };

            return new Placer("Landing Party", new TileGrid(grid));
        }

        private static class Resource : Tile
        {
            readonly RESOURCE r;
            readonly int amount;

            Resource(Json j, LIST<string> keys, int index)
            {
                if (index >= keys.size())
                {
                    this.r = null;
                    this.amount = 0;
                }
                else
                {
                    r = RESOURCES.map().get(keys.get(index), j);
                    amount = j.i(keys.get(index), 1, 500);
                }
            }

            public override bool Placable(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                return true;
            }

            public override void Place(int tx, int ty, TileGrid grid, int rx, int ry)
            {
                if (r != null)
                    SETT.THINGS().resources.createPrecise(tx, ty, r, amount + (int)(amount * BOOSTABLES.CIVICS().LANDING.get(HCLASS_RACE.clP(null, null))));
            }

            public override SPRITE Sprite(TileGrid grid, int rx, int ry, int mask)
            {
                return SPRITES.cons().ICO.clear;
            }
        }
    }
}