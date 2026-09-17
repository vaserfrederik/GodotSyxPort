using System;
using System.Collections.Generic;
using game;
using settlement.main;
using settlement.tilemap;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.rendering;

namespace settlement.tilemap
{
    internal class MinimapColorGetter
    {
        private readonly ColorImp col = new ColorImp();
        private readonly DIR[] dirNorth = new DIR[] { DIR.W, DIR.NW, DIR.N, DIR.NE };
        private readonly DIR[] dirShade = new DIR[] { DIR.E, DIR.SE, DIR.S, DIR.SW };

        private readonly int qs = 8;
        private readonly int sc = BitOperations.TrailingZeroCount(qs);
        private readonly int ww = SETT.TWIDTH >> sc;

        private readonly Bitmap1D bits = new Bitmap1D(SETT.TWIDTH * SETT.THEIGHT / (qs * qs), false);
        private readonly ArrayListShort queue = new ArrayListShort(SETT.TWIDTH * SETT.THEIGHT / (qs * qs));
        private readonly TextureHolderChunk chunk = new TextureHolderChunk(qs, qs);

        public COLOR Get(int x, int y)
        {
            SMinimapGetter r = miniR(x, y);
            COLOR c = r.miniC(x, y);
            bool n = false;
            bool s = false;

            foreach (DIR d in dirNorth)
            {
                int dx = x + d.x();
                int dy = y + d.y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    SMinimapGetter r2 = miniR(dx, dy);
                    if (r2 == null || r != r2)
                    {
                        n = true;
                        break;
                    }
                }
            }

            foreach (DIR d in dirShade)
            {
                int dx = x + d.x();
                int dy = y + d.y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    SMinimapGetter r2 = miniR(dx, dy);
                    if (r2 == null || r != r2)
                    {
                        s = true;
                        break;
                    }
                }
            }

            col.set(c);

            return r.miniColorPimped(col, x, y, n, s);
        }

        private SMinimapGetter miniR(int x, int y)
        {
            if (!SETT.IN_BOUNDS(x, y))
                return null;
            COLOR c = SETT.ROOMS().miniC.miniC(x, y);
            if (c != null)
                return SETT.ROOMS().miniC;
            c = SETT.TERRAIN().get(x, y).miniC(x, y);
            if (c != null)
                return SETT.TERRAIN().get(x, y);
            c = SETT.FLOOR().minimap.miniC(x, y);
            if (c != null)
                return SETT.FLOOR().minimap;
            return SETT.GROUND().minimap;
        }

        public void update(int tx, int ty)
        {
            for (int di = 0; di < DIR.ALLC.size(); di++)
            {
                DIR dir = DIR.ALLC.get(di);
                int dx = tx + dir.x();
                int dy = ty + dir.y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    dx = dx >> sc;
                    dy = dy >> sc;
                    int i = dx + dy * ww;
                    if (!bits.get(i))
                    {
                        queue.add(i);
                        bits.setTrue(i);
                    }
                }
            }

            //SETT.MINIMAP().putPixel(tx, ty, get(tx, ty));
        }

        void clear()
        {
            queue.clear();
            bits.clear();
        }

        void update()
        {
            if (queue.size() > 0)
            {
                int q = queue.remove(queue.size() - 1);
                bits.setFalse(q);
                int tx = q % ww;
                int ty = q / ww;
                tx = tx << sc;
                ty = ty << sc;

                int i = 0;
                for (int dy = 0; dy < qs; dy++)
                {
                    for (int dx = 0; dx < qs; dx++)
                    {
                        COLOR c = get(tx + dx, ty + dy);

                        chunk.put(i++, Minimap.getC(c.red()), Minimap.getC(c.green()), Minimap.getC(c.blue()), (byte)0x0FF);
                    }
                }

                TextureCoords c = SETT.MINIMAP().texture(tx, ty, chunk.width, chunk.height);

                GAME.texture().addChunk(c.x1, c.y1, chunk.width, chunk.width * chunk.height, chunk);
            }
        }
    }
}