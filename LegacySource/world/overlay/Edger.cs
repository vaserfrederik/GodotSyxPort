using System;
using util.rendering;

namespace world.overlay
{
    final class Edger
    {
        private readonly byte[][] edgeH;
        private readonly byte[][] edgeV;
        private readonly TILE_SHEET s;

        public Edger(int width, int height)
        {
            s = WORLD.sprites().edge;
            edgeH = new byte[2][width];
            edgeV = new byte[2][height - 2];

            edgeH[0][0] = 0;
            edgeH[0][width - 1] = 1;
            edgeH[1][0] = 2;
            edgeH[1][width - 1] = 3;

            for (int x = 1; x < width - 1; x++)
            {
                edgeH[0][x] = (byte)(20 + RND.rInt(8));
                edgeH[1][x] = (byte)(28 + RND.rInt(8));
            }
            for (int y = 0; y < height - 2; y++)
            {
                edgeV[0][y] = (byte)(4 + RND.rInt(8));
                edgeV[1][y] = (byte)(12 + RND.rInt(8));
            }
        }

        public void render(SPRITE_RENDERER r, RenderData rd, int zoomout)
        {
            int tx1 = rd.tx1();
            int tx2 = rd.tx2();
            int ty1 = rd.ty1();
            int ty2 = rd.ty2();
            int offX = rd.x1();
            int offY = rd.y1();

            int tmpX;
            int tmpY;
            int tileSize = C.TILE_SIZE;

            if (ty1 == 0)
            {
                tmpX = offX;
                tmpY = offY;
                for (int x = tx1; x <= tx2; x++)
                {
                    s.render(r, edgeH[0][x], tmpX, tmpY);
                    tmpX += tileSize;
                }
            }
            if (ty2 == edgeV[0].Length + 1)
            {
                tmpX = offX;
                tmpY = offY + (ty2 - ty1) * tileSize;
                for (int x = tx1; x <= tx2; x++)
                {
                    s.render(r, edgeH[1][x], tmpX, tmpY);
                    tmpX += tileSize;
                }
            }
            if (tx1 == 0)
            {
                tmpX = offX;
                tmpY = offY;
                for (int y = ty1; y <= ty2; y++)
                {
                    if (y > 0 && y <= edgeV[0].Length)
                    {
                        s.render(r, edgeV[0][y - 1], tmpX, tmpY);
                    }
                    tmpY += tileSize;
                }
            }
            if (tx2 == edgeH[0].Length - 1)
            {
                tmpX = offX + (tx2 - tx1) * tileSize;
                tmpY = offY;
                for (int y = ty1; y <= ty2; y++)
                {
                    if (y > 0 && y <= edgeV[0].Length)
                    {
                        s.render(r, edgeV[1][y - 1], tmpX, tmpY);
                    }
                    tmpY += tileSize;
                }
            }

            renderOut(r, offX, offX + tileSize * (tx2 - tx1 + 1), offY, offY + tileSize * (ty2 - ty1 + 1), zoomout);
        }

        private void renderOut(SPRITE_RENDERER r, int x1, int x2, int y1, int y2, int zoomout)
        {
            COLOR.BLACK.render(r, 0, x1, 0, C.HEIGHT() << zoomout);
            COLOR.BLACK.render(r, x2, C.WIDTH() << zoomout, 0, C.HEIGHT() << zoomout);
            COLOR.BLACK.render(r, x1, x2, 0, y1);
            COLOR.BLACK.render(r, x1, x2, y2, C.HEIGHT() << zoomout);
        }
    }
}