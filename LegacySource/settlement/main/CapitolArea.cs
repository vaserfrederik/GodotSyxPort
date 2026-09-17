using System;
using System.Collections.Generic;
using System.IO;
using init.constant;
using init.type;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using world;
using world.map.regions.centre;
using world.map.road;
using world.map.terrain;

public sealed class CapitolArea
{
    private static int TILES = WCentre.TILE_DIM;

    private readonly ArrayList<COORDINATE> tiles = new ArrayList<COORDINATE>(TILES * TILES);
    private int arrivalTile = -1;

    private readonly Rec worldPixels = new Rec(TILES * C.TILE_SIZE);
    private readonly Rec worldTiles = new Rec(TILES);

    private CLIMATE climate;
    public readonly WorldTerrainInfo info = new WorldTerrainInfo();

    public bool isBattle;

    public readonly SAVABLE saver = new SAVABLE()
    {
        public void Save(FilePutter file)
        {
            tiles.Save(file);
            file.I(arrivalTile);
            worldPixels.Save(file);
            worldTiles.Save(file);
            file.I(climate.Index());
            file.Bool(isBattle);
        }

        public void Load(FileGetter file)
        {
            tiles.Load(file);
            arrivalTile = file.I();
            worldPixels.Load(file);
            worldTiles.Load(file);
            climate = CLIMATES.ALL().Get(file.I());
            isBattle = file.Bool();
        }

        public void Clear()
        {
            // TODO Auto-generated method stub
        }
    };

    void Init(int worldtileX1, int worldtileY1, bool isBattle)
    {
        this.isBattle = isBattle;
        worldTiles.MoveX1Y1(worldtileX1, worldtileY1);
        worldPixels.MoveX1Y1(worldtileX1 * C.TILE_SIZE, worldtileY1 * C.TILE_SIZE);

        tiles.Clear();
        info.InitCity(worldtileX1, worldtileY1);

        for (int y = 0; y < TILES; y++)
        {
            for (int x = 0; x < TILES; x++)
            {
                int tx = x + worldtileX1;
                int ty = y + worldtileY1;
                int i = tiles.Add(new ShortCoo(worldtileX1 + x, worldtileY1 + y));

                if (arrivalTile == -1 && WTRAV.IsGoodLandTile(tx, ty))
                {
                    if (x == 0 || y == 0 || x == TILES - 1 || y == TILES - 1)
                        arrivalTile = i;
                }
            }
        }

        if (arrivalTile == -1)
        {
            //you're on an island or such.
            arrivalTile = TILES * TILES / 2;
        }

        climate = WORLD.CLIMATE().Getter.Get(worldTiles.CX(), worldTiles.CY());
    }

    public CapitolArea()
    {
    }

    public float GetWatertabe()
    {
        double table = 0;
        foreach (COORDINATE c in tiles)
        {
            table += WORLD.GROUND().Getter.Get(c).Moisture();
        }

        table /= (double)(TILES * TILES);

        table = 0.05 + table * 0.1;
        return (float)table;
    }

    public int ArrivalTile()
    {
        return arrivalTile;
    }

    public CLIMATE Climate()
    {
        return climate;
    }

    public RECTANGLE Tiles()
    {
        return worldTiles;
    }

    public LIST<COORDINATE> Ts()
    {
        return tiles;
    }

    public RECTANGLE Pixels()
    {
        return worldPixels;
    }
}