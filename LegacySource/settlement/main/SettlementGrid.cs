using System;
using System.Collections.Generic;

namespace SettlementMain
{
    public static class SettlementGrid
    {
        /**
         * Nr of tiles that makes up the settlement map
         */
        public const int TILES = WCentre.TILE_DIM;
        /**
         * size of one world tile on the worldmap
         */
        public const int QUAD_SIZE = SETT.TWIDTH / TILES;
        public const int QUAD_AREA = QUAD_SIZE * QUAD_SIZE;
        public const int QUAD_HALF = QUAD_SIZE / 2;
        public const int QUAD_QUATER = QUAD_HALF / 2;
        public const int QUAD_EIGHTH = QUAD_QUATER / 2;

        private readonly RECTANGLE[][] bounds = new RECTANGLE[TILES][];
        private readonly List<Tile> tiles = new List<Tile>(TILES * TILES);

        public SettlementGrid()
        {
            for (int i = 0; i < TILES; i++)
            {
                bounds[i] = new RECTANGLE[TILES];
            }

            STRING_RECIEVER reciever = new STRING_RECIEVER()
            {
                public void acceptString(CharSequence string)
                {
                    //this is what someone types
                }
            };

            ACTION a = new ACTION()
            {
                public void exe()
                {
                    VIEW.inters().input.requestInput(reciever, "type something");
                }
            };

            IDebugPanelSett.add("prort", a);

            for (int y = 0; y < TILES; y++)
                for (int x = 0; x < TILES; x++)
                {
                    bounds[y][x] = new Rec(x * QUAD_SIZE, x * QUAD_SIZE + QUAD_SIZE, y * QUAD_SIZE,
                            y * QUAD_SIZE + QUAD_SIZE);
                    tiles.Add(new Tile(x, y));
                }
        }

        public class Tile
        {
            public readonly RECTANGLE bounds;
            private readonly COORDINATE[][] coos = new COORDINATE[TILES][];

            private readonly COORDINATE[] coosInner = new COORDINATE[DIR.ALL.size()];
            private readonly COORDINATE coosInnerC;
            private readonly List<DIR> dirs = new List<DIR>(4);

            public Tile(int quadX, int quadY)
            {
                bounds = new Rec(quadX * QUAD_SIZE, quadX * QUAD_SIZE + QUAD_SIZE, quadY * QUAD_SIZE,
                        quadY * QUAD_SIZE + QUAD_SIZE);

                dirs.Add(DIR.E);
                dirs.Add(DIR.SE);
                dirs.Add(DIR.S);
                dirs.Add(DIR.C);
                if (quadX == 0)
                {
                    dirs.Add(DIR.W);
                    dirs.Add(DIR.SW);
                }
                if (quadY == 0)
                {
                    dirs.Add(DIR.NE);
                    dirs.Add(DIR.N);
                    if (quadX == 0)
                        dirs.Add(DIR.NW);
                }

                for (int i = 0; i < TILES; i++)
                {
                    coos[i] = new COORDINATE[TILES];
                }

                for (int y = 0; y < TILES; y++)
                {
                    for (int x = 0; x < TILES; x++)
                    {
                        int qx = bounds.x1() + (x * QUAD_HALF);
                        int qy = bounds.y1() + (y * QUAD_HALF);

                        if (qx >= SETT.TWIDTH)
                            qx = SETT.TWIDTH - 1;
                        if (qy >= SETT.THEIGHT)
                            qy = SETT.THEIGHT - 1;

                        coos[y][x] = new Coo(qx, qy);
                    }
                }

                foreach (DIR d in DIR.ALL)
                {
                    int qx = bounds.cX() + d.x() * (QUAD_QUATER + QUAD_EIGHTH);
                    int qy = bounds.cY() + d.y() * (QUAD_QUATER + QUAD_EIGHTH);
                    coosInner[d.id()] = new Coo(qx, qy);
                }

                coosInnerC = new Coo(bounds.cX(), bounds.cY());
            }

            public IEnumerable<DIR> getDirs()
            {
                return dirs;
            }

            public COORDINATE cooInner(DIR d)
            {
                if (d == DIR.C)
                    return coosInnerC;
                return coosInner[d.id()];
            }

            public COORDINATE coo(DIR d)
            {
                return coos[d.y() + 1][d.x() + 1];
            }
        }

        public Tile tile(int tile)
        {
            return tiles[tile];
        }

        public Tile tile(int x, int y)
        {
            return tiles[x + y * TILES];
        }

        public IEnumerable<Tile> tiles()
        {
            return tiles;
        }
    }
}