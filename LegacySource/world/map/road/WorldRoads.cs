using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static world.WORLD;

namespace world.map.road
{
    public sealed class WorldRoads : WorldResource, MAP_BOOLEANE
    {
        private COLOR[] rColors;

        private readonly Bitmap2D isMap = new Bitmap2D(TBOUNDS(), false);
        private readonly Bitmap2D miniMap = new Bitmap2D(TBOUNDS(), false);
        public readonly Bitmap2D bridgeMap = new Bitmap2D(TBOUNDS(), false);
        private readonly Bitsmap1D data = new Bitsmap1D(0, 4, TAREA());
        private readonly Bitmap2D miniHack = new Bitmap2D(TBOUNDS(), false);

        public WorldRoads(WORLD data2) : base("roads", "ROADS")
        {
            Json j = ConfigWorld.json("Road");
            ColorImp low = new ColorImp(j, "COLOR_SMALL");
            ColorImp hi = new ColorImp(j, "COLOR_BIG");
            rColors = COLOR.interpolate(low, hi, 16);
        }

        public void render(WRenContext data, RenderIterator it)
        {
            if (!isMap.is(it.tile()))
                return;

            if (miniHack.is(it.tile()))
            {
                int m = 0;
                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    if (isMap.is(it.tx(), it.ty(), DIR.ORTHO.get(di)) && miniMap.is(it.tx(), it.ty(), DIR.ORTHO.get(di)))
                    {
                        m |= DIR.ORTHO.get(di).mask();
                    }
                }
                rColors[0].bind();
                WORLD.BUILDINGS().sprites.roadsMini.render(data.r,
                        m + 16 * (it.ran() & 0b0111), it.x(), it.y());
            }

            double d = levelRoad(it.tile());
            TILE_SHEET sheet = WORLD.BUILDINGS().sprites.roads;
            if (miniMap.is(it.tile()))
            {
                sheet = WORLD.BUILDINGS().sprites.roadsMini;
                d *= 0.25;
            }
            else
                d = 0.75 + 0.25 * d;
            d = CLAMP.d(d, 0, 1);
            rColors[(int)(d * 15)].bind();
            sheet.render(data.r,
                    this.data.get(it.tile()) + 16 * (it.ran() & 0b0111), it.x(), it.y());

            COLOR.unbind();
        }

        private double levelRoad(int tile)
        {
            Region reg = REGIONS().map.get(tile);
            if (reg != null)
            {
                return RD.BUILDINGS().levelRoad.get(reg);
            }
            return 0;
        }

        private readonly DIR[] dds = new DIR[] { DIR.N, DIR.E };

        public void renderBridge(WRenContext con, RenderIterator it)
        {
            if (!isMap.is(it.tile()))
                return;

            if (miniMap.is(it.tile()))
                return;

            if (!WORLD.WATER().isBig.is(it.tile()))
                return;

            if (bridgeMap.is(it.tile()))
            {
                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    if (!WORLD.WATER().isBig.is(it.tx(), it.ty(), d) && !WORLD.WATER().isBig.is(it.tx(), it.ty(), d.perpendicular()))
                    {
                        if (WORLD.PATH().map.isOnly(it.tx(), it.ty(), d))
                        {
                            int level = (int)(levelRoad(it.tile()) * 4);
                            WORLD.BUILDINGS().sprites.bridge.render(con.r, level + di, it.x(), it.y());
                            return;
                        }
                    }
                }
            }

            if (WORLD.REGIONS().cTile.is(it.tile()))
                return;

            int data = this.data.get(it.tile());
            con.s.setDistance2Ground(0).setHeight(1);
            DIR d = pDir(data).perpendicular();
            int x = it.x() - 4 * C.SCALE;
            int y = it.y() - 4 * C.SCALE;
            int tile = d.orthoID() * 16 + (it.ran() & 0b11);

            if (Integer.bitCount(data) != 1)
            {
                tile += 3 * 16;
            }
            else
            {
                tile += (data & 1) * 16;
            }

            WORLD.BUILDINGS().sprites.bridge.render(con.r, tile, it.x(), it.y());
        }

        private DIR pDir(int data)
        {
            // Assuming DIR is an enum with values N, E, S, W
            // and data is a bitmask representing directions
            if ((data & 1) != 0) return DIR.N;
            if ((data & 2) != 0) return DIR.E;
            if ((data & 4) != 0) return DIR.S;
            if ((data & 8) != 0) return DIR.W;
            return DIR.N; // Default direction
        }

        private bool hasLand(int tx, int ty)
        {
            for (int di = 0; di < DIR.ORTHO.size(); di++)
            {
                DIR d = DIR.ORTHO.get(di);
                if (WORLD.IN_BOUNDS(tx, ty, d) && !WORLD.WATER().isBig.is(tx, ty, d))
                {
                    return true;
                }
            }
            return false;
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            placers = new Placer(),
            save = f =>
            {
                isMap.save(f);
                miniMap.save(f);
                data.save(f);
                miniHack.save(f);
                bridgeMap.save(f);
            },
            load = f =>
            {
                isMap.load(f);
                miniMap.load(f);
                data.load(f);
                miniHack.load(f);
                bridgeMap.load(f);
            },
            clear = () =>
            {
                isMap.clear();
                miniMap.clear();
                data.clear();
                miniHack.clear();
                bridgeMap.clear();
            },
            validateInit = error =>
            {
                if (!WORLD.IN_BOUNDS(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy()))
                {
                    error.problem = "The world has no player region centre";
                    error.coo.set(WORLD.TBOUNDS().cX(), WORLD.TBOUNDS().cY());
                    return;
                }
            },
            generate = loadPrint =>
            {
                clear();
                new Gen().generateAll(WORLD.REGIONS().player.cx(), WORLD.REGIONS().player.cy(), loadPrint);
            },
            makePlacers = tm => placers.placers
        };

        public override WorldResourceManager saver()
        {
            return saver;
        }
    }
}