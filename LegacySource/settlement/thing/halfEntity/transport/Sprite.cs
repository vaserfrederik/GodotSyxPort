using System;
using System.IO;
using Init.Constant;
using Init.Paths;
using Init.Resources;
using Snake2D;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Sprite;
using Util.Rendering;
using Util.SpriteComposer;

namespace Settlement.Thing.HalfEntity.Transport
{
    public sealed class Sprite
    {
        private readonly TileSheet sheetCart;
        private readonly TileSheet sheetHarness;
        private const int M = 1 * C.SCALE;

        public Sprite() : this(null)
        {
        }

        public Sprite(Stream stream)
        {
            sheetCart = new TileSheet(PATHS.SETT().sprite.getFolder("thing").get("CART"), 164, 234)
            {
                Init = (c, s, d) =>
                {
                    s.singles.init(0, 0, 1, 1, 2, 6, d.s32);
                    for (int i = 0; i < 6; i++)
                    {
                        for (int r = 0; r < 4; r++)
                        {
                            s.singles.setSkip(i * 2, 1).pasteRotated(r, true);
                            s.singles.setSkip(i * 2 + 1, 1).pasteRotated(r, true);
                        }
                    }

                    return d.s32.saveGame();
                }
            }.Get(stream);

            sheetHarness = new TileSheet(PATHS.SETT().sprite.getFolder("thing").get("CART"), 164, 234)
            {
                Init = (c, s, d) =>
                {
                    s.singles.init(0, 0, 1, 1, 2, 7, d.s32);
                    for (int r = 0; r < 4; r++)
                    {
                        s.singles.setSkip(6 * 2, 1).pasteRotated(r, true);
                        s.singles.setSkip(6 * 2 + 1, 1).pasteRotated(r, true);
                    }

                    return d.s32.saveGame();
                }
            }.Get(stream);
        }

        public void RenderBelow(SpriteRenderer r, ShadowBatch s, int rot, int cx, int cy, double mov, int ran, double degrade, Resource res, double resamount)
        {
            int i = rot;
            i += ((int)(mov * 3) & 3) * 8;
            Dir d = DIR.ALL.get(rot).perpendicular();

            int x = (int)(cx + d.xN() * M);
            int y = (int)(cy + d.yN() * M);
            sheetCart.renderC(r, i, x, y);
            s.setHeight(4).setDistance2Ground(0);
            sheetCart.renderC(s, i, x, y);

            if (res != null && resamount > 0)
            {
                x = (int)(d.xN() * M * 2 + cx - C.TILE_SIZEH);
                y = (int)(d.yN() * M * 2 + cy - C.TILE_SIZEH);
                res.renderLaying(r, x, y, ran, (resamount * RESOURCE.renderMax) + (ran & 0b11));
            }
        }

        public void Render(SpriteRenderer r, ShadowBatch s, int rot, int cx, int cy, double degrade, bool military)
        {
            int i = rot + (military ? 10 * 4 : 8 * 4);
            Dir d = DIR.ALL.get(rot).perpendicular();
            int x = (int)(cx + d.xN() * M);
            int y = (int)(cy + d.yN() * M);
            sheetCart.renderC(r, i, x, y);
            s.setHeight(1).setDistance2Ground(0);
            sheetCart.renderC(s, i, x, y);
            if (military)
                sheetHarness.renderC(r, rot, x, y);
        }

        public void RenderHarness(SpriteRenderer r, ShadowBatch s, int rot, int cx, int cy)
        {
            Dir d = DIR.ALL.get(rot).perpendicular();
            int x = (int)(cx + d.xN() * M);
            int y = (int)(cy + d.yN() * M);
            sheetHarness.renderC(r, rot, x, y);
        }
    }
}