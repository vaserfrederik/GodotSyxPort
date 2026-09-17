using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Tilemap.Floor
{
    class GrassRenderer
    {
        private readonly TILE_SHEET sheet;
        private readonly TILE_SHEET sheetMask;
        private readonly TILE_SHEET moss;
        private readonly Colors colors;
        private readonly TileTextureScroller dis2 = SPRITES.textures().dis_low.scroller(12.0 * 6, -12 * 5.5);
        private readonly Grass grass;
        private readonly int SET = 16;
        private readonly int[] tts = {
            -1, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 7, 7,
        };
        private readonly OPACITY[] op = new OPACITY[Grass.TYPES];

        static GrassRenderer()
        {
            for (int i = 0; i < Grass.TYPES; i++)
            {
                int p = (int)(100 * (i + 1.0) / Grass.TYPES);
                op[i] = new OpacityImp(p);
            }
        }

        public GrassRenderer(Grass grass) : this(grass, PATHS.SPRITE_SETTLEMENT_MAP().get("Grass"), 972, 390)
        {
        }

        private GrassRenderer(Grass grass, string path, int width, int height) : this(grass, path, width, height, PATHS.SPRITE_SETTLEMENT_MAP().get("Moss"), 792, 108)
        {
        }

        private GrassRenderer(Grass grass, string path, int width, int height, string mossPath, int mossWidth, int mossHeight)
        {
            this.grass = grass;
            new ComposerThings.IInit(path, width, height);

            colors = new Colors();

            sheetMask = new ITileSheet()
            {
                init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.s24;
                    final ComposerSources.Full f = s.full;
                    f.init(0, f.body().y2(), 1, 1, 4, 4, t);
                    f.setVar(0).paste(true);
                    return t.saveGame();
                }
            }.get();

            sheet = new ITileSheet()
            {
                init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.s24;
                    final ComposerSources.Singles f = s.singles;
                    f.init(0, s.full.body().y2(), 1, 1, 16, 8, t);
                    f.setVar(0).paste(true);
                    return t.saveGame();
                }
            }.get();

            moss = new ITileSheet(mossPath, mossWidth, mossHeight)
            {
                init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                {
                    ComposerDests.Tile t = d.s24;
                    final ComposerSources.Full f = s.full;
                    f.init(0, 0, 1, 1, 16, 4, t);
                    f.setVar(0).paste(true);
                    return t.saveGame();
                }
            }.get();
        }

        void update(double ds)
        {
            colors.update(ds);
            double w = Math.Pow(SETT.WEATHER().wind.getD(), 1.5) * 0.5;
            if (w > 0.1)
                dis2.update(ds * w);
        }

        public void render(double ds, Renderer r, RenderData data)
        {
            RenderData.RenderIterator it = data.onScreenTiles(1, 1, 1, 1);

            while (it.has())
            {
                render(it, r);
                it.next();
            }
            COLOR.unbind();
        }

        public void render(RenderData.RenderIterator it, Renderer r)
        {
            int tx = it.tx();
            int ty = it.ty();
            int px = it.x();
            int py = it.y();
            int ran = it.ran();
            int tile = it.tile();

            if (pRender(r, tx, ty, px, py, ran, tile))
            {
            }
            else if (SETT.TERRAIN().diagonal.is(it.tx(), it.ty()))
            {
                for (int i = 0; i < DIR.NORTHO.size(); i++)
                {
                    int dx = it.tx() + DIR.NORTHO.get(i).x();
                    int dy = it.ty() + DIR.NORTHO.get(i).y();
                    if (grass.currentI.get(dx, dy) > 0 && grass.currentI.get(it.tx(), dy) > 0 && grass.currentI.get(dx, it.ty()) > 0)
                    {
                        it.setOff((int)(DIR.NORTHO.get(i).xN() * -C.TILE_SIZEH), (int)(DIR.NORTHO.get(i).yN() * -C.TILE_SIZEH));
                        pRender(r, dx, dy, (int)(px + DIR.NORTHO.get(i).xN() * C.TILE_SIZEH), (int)(py + DIR.NORTHO.get(i).yN() * C.TILE_SIZEH), it.ran(), dx + dy * SETT.TWIDTH);
                    }
                }
            }
        }

        private bool pRender(Renderer r, int tx, int ty, int px, int py, int ran, int tile)
        {
            int colC = grass.currentI.get(tile);
            int c = tts[colC];

            if (colC == 1)
                c -= (ran & 0b011);
            if (colC == 2)
                c -= (ran & 0b001);
            if (colC == 3)
                c -= (ran & 0b01);

            if (colC == 5)
                c -= (ran & 0b0001);

            ran = ran >> 2;

            if (c >= 0)
            {
                int m = SETT.MINERALS().amountInt.get(tile) >> 2;
                c = CLAMP.i(c - m, 0, c);

                int d = (int)(((ran & 0x07) - 7) * C.SCALE);
                ran = ran >> 3;
                int x = px + d;
                d = (int)(((ran & 0x07) - 7) * C.SCALE);
                ran = ran >> 3;
                int y = py + d;

                if (SETT.TERRAIN().roof.is(tx, ty))
                {
                    if (SETT.TERRAIN().roof.is(tx, ty))
                    {
                        int roofType = SETT.TERRAIN().roof.get(tx, ty);
                        if (roofType == 1 || roofType == 2)
                        {
                            r.drawImage(ROOFS[roofType - 1], x, y);
                        }
                    }
                }
                else
                {
                    if (SETT.TERRAIN().roof.is(tx, ty))
                    {
                        int roofType = SETT.TERRAIN().roof.get(tx, ty);
                        if (roofType == 1 || roofType == 2)
                        {
                            r.drawImage(ROOFS[roofType - 1], x, y);
                        }
                    }
                }
            }
            return true;
        }

        private COLOR get(int c, int ran)
        {
            return c_current[c][ran & (RAN - 1)];
        }

        void update(double ds)
        {
            double m = SETT.WEATHER().moisture.getD();
            if (m < 0.25)
            {
                m = 0.05;
            }
            else if (m < 0.5)
            {
                m -= 0.25;
                m /= 0.25;
                m = CLAMP.d(m, 0.05, 1);
            }
            else
            {
                m = 1;
            }
            set(m, 1.0 - SETT.WEATHER().growth.getD());
        }

        private void set(double moist, double winter)
        {
            for (int a = 0; a < Grass.TYPES; a++)
            {
                for (int b = 0; b < RAN; b++)
                {
                    tmp.interpolate(dry, c_base[a][b], moist);
                    c_current[a][b].interpolate(tmp, this.winter, winter * 0.75);
                }
            }
        }
    }
}