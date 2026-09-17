using System;
using util.constant;
using snake2d.util.datatypes;
using util.data;

namespace util.rendering
{
    public class RenderData
    {
        private int waters = 0;
        private int vegitations = 0;
        private int caves;
        private readonly Bitmap1D lit;
        private readonly Bitmap1D hid;
        private readonly RANMAP ran = GUTIL.ran1();
        private readonly RANMAP ran2 = GUTIL.ran2();

        private readonly Rec absoluteWin = new Rec();
        private readonly Rec gameWin = new Rec();
        private readonly Rec tileWin = new Rec();
        public bool isLit = false;

        private int tx1, ty1, ty2, tx2;
        private short x1, y1;

        private readonly int TWIDTH;
        private readonly int THEIGHT;

        public RenderData(int twidth, int theight)
        {
            this.TWIDTH = twidth;
            this.THEIGHT = theight;
            lit = new Bitmap1D(twidth * theight, false);
            hid = new Bitmap1D(twidth * theight, false);
        }

        public void init(RECTANGLE renWindow, int offX, int offY)
        {
            gameWin.set(renWindow);
            absoluteWin.set(renWindow).moveX1Y1(offX, offY);

            tx1 = (int)(renWindow.x1() / TILE_SIZE);
            tx2 = (int)(renWindow.x2() / TILE_SIZE);

            ty1 = (int)(renWindow.y1() / TILE_SIZE);
            ty2 = (int)(renWindow.y2() / TILE_SIZE);

            x1 = (short)((offX) - (renWindow.x1() % TILE_SIZE));
            y1 = (short)((offY) - (renWindow.y1() % TILE_SIZE));

            if (tx2 >= TWIDTH)
                tx2 = TWIDTH - 1;
            if (ty2 >= THEIGHT)
                ty2 = THEIGHT - 1;

            if (ty1 < 0)
            {
                y1 -= (short)(ty1 * TILE_SIZE);
                ty1 = 0;
            }

            if (tx1 < 0)
            {
                x1 -= (short)(tx1 * TILE_SIZE);
                tx1 = 0;
            }

            for (int y = ty1 - 5; y <= ty2 + 5; y++)
            {
                for (int x = tx1 - 5; x <= tx2 + 5; x++)
                {
                    if (x >= 0 && x < TWIDTH && y >= 0 && y < THEIGHT)
                    {
                        int i = y * TWIDTH + x;
                        lit.set(i, false);
                        hid.set(i, false);
                    }
                }
            }

            tileWin.set(tx1, tx2 + 1, ty1, ty2 + 1);
            vegitations = 0;
            waters = 0;
            caves = 0;
        }

        public int waters()
        {
            return waters;
        }

        public int caves()
        {
            return caves;
        }

        public int vegitations()
        {
            return vegitations;
        }

        public int area()
        {
            return tileWin.height() * tileWin.width();
        }

        public int offX1()
        {
            return tx1 * C.TILE_SIZE - x1;
        }

        public int offY1()
        {
            return ty1 * C.TILE_SIZE - y1;
        }

        public int x1()
        {
            return x1;
        }

        public int y1()
        {
            return y1;
        }

        public RECTANGLE absBounds()
        {
            return absoluteWin;
        }

        public RECTANGLE gBounds()
        {
            return gameWin;
        }

        public RECTANGLE tBounds()
        {
            return tileWin;
        }

        public int tx1()
        {
            return tx1;
        }

        public int tx2()
        {
            return tx2;
        }

        public int ty1()
        {
            return ty1;
        }

        public int ty2()
        {
            return ty2;
        }

        public int transformGX(int x)
        {
            x -= gameWin.x1();
            x += absoluteWin.x1();
            return x;
        }

        public int transformGY(int x)
        {
            x -= gameWin.y1();
            x += absoluteWin.y1();
            return x;
        }

        public int random(int tx, int ty)
        {
            return ran.get(tx + ty * THEIGHT);
        }

        private readonly RenderIterator iter = new RenderIterator();

        public RenderIterator onScreenTiles()
        {
            return iter.init(0, 0, 0, 0);
        }

        public RenderIterator onScreenTiles(int offX1, int offX2, int offY1, int offY2)
        {
            return iter.init(offX1, offX2, offY1, offY2);
        }

        public sealed class RenderIterator
        {
            private int tStartX, tStartY;
            private int tEndX, tEndY;
            private int tx, ty;
            private int tile;
            private short x, y;
            private int rann;
            private long rann2;
            private int offX, offY;
            private RenderIterator() { }
            private readonly Coo coo = new Coo();

            private RenderIterator init(int offX1, int offX2, int offY1, int offY2)
            {
                tStartX = tx1 - offX1;
                if (tStartX < 0)
                    tStartX = 0;
                tStartY = ty1 - offY1;
                if (tStartY < 0)
                    tStartY = 0;
                tEndX = tx2 + offX2;
                if (tEndX >= TWIDTH)
                    tEndX = TWIDTH - 1;
                tEndY = ty2 + offY2;
                if (tEndY >= THEIGHT)
                    tEndY = THEIGHT - 1;
                x = (short)(x1 - (tx1 - tStartX) * TILE_SIZE);
                y = (short)(y1 - (ty1 - tStartY) * TILE_SIZE);
                tx = tStartX;
                ty = tStartY;
                tile = tx + ty * TWIDTH;
                rann = ran.get(tile);
                rann2 = ((0L | ran2.get(tile)) << 32) | rann;
                if (tEndX <= tStartX)
                {
                    ty = tStartY + 1;
                }
                if (tx >= tEndX)
                    ty = tEndY + 1;
                offX = 0;
                offY = 0;
                return this;
            }

            public bool hasMore()
            {
                return ty <= tEndY;
            }

            public void next()
            {
                tx++;
                if (tx > tEndX)
                {
                    tx = tStartX;
                    ty++;
                }
                tile = tx + ty * TWIDTH;
                rann = ran.get(tile);
                rann2 = ((0L | ran2.get(tile)) << 32) | rann;
            }

            public void count(int count)
            {
                tile += count;
                if (tile >= TWIDTH * THEIGHT)
                {
                    tile %= TWIDTH * THEIGHT;
                }
                rann = ran.get(tile);
                rann2 = ((0L | ran2.get(tile)) << 32) | rann;
            }

            public void count(int dx, int dy)
            {
                tx += dx;
                if (tx < 0)
                {
                    tx += TWIDTH;
                }
                else if (tx >= TWIDTH)
                {
                    tx -= TWIDTH;
                }

                ty += dy;
                if (ty < 0)
                {
                    ty += THEIGHT;
                }
                else if (ty >= THEIGHT)
                {
                    ty -= THEIGHT;
                }

                tile = tx + ty * TWIDTH;
                rann = ran.get(tile);
                rann2 = ((0L | ran2.get(tile)) << 32) | rann;
            }

            public void ranOffset(int dx, int dy)
            {
                rann = ran.get((tile + dx + dy * TWIDTH));
                rann2 = ((0L | ran2.get(tile + dx + dy * TWIDTH)) << 32) | rann;
            }

            public void ranSwap()
            {
                rann = ran2.get(tile);
            }

            public int ran()
            {
                return rann;
            }

            public long bigRan()
            {
                return rann2;
            }

            public int ranGet(int dx, int dy)
            {
                int x = tx + dx;
                if (x < 0)
                {
                    x += TWIDTH;
                }
                else if (x >= TWIDTH)
                {
                    x -= TWIDTH;
                }

                int y = ty + dy;
                if (y < 0)
                {
                    y += THEIGHT;
                }
                else if (y >= THEIGHT)
                {
                    y -= THEIGHT;
                }
                return ran.get(x + y * THEIGHT);
            }

            public int ran(int tx, int ty)
            {
                return ran.get(tx + ty * THEIGHT);
            }

            public bool litIs()
            {
                return lit.get(tile);
            }

            public bool litIs(DIR d)
            {
                int t = tile + d.x() + d.y() * TWIDTH;
                return lit.get(t);
            }

            public void lit()
            {
                isLit = true;
                lit.set(tile, true);
            }

            public void hiddenSet()
            {
                hid.set(tile, true);
            }

            public int tile()
            {
                return tile;
            }

            public int x()
            {
                return (x + offX);
            }

            public int y()
            {
                return (y + offY);
            }

            public int oX()
            {
                return -tx1 * C.TILE_SIZE + x1;
            }

            public int oY()
            {
                return -ty1 * C.TILE_SIZE + y1;
            }

            public COORDINATE realStart => new COORDINATE()
            {
                y = () => gameWin.x1(),
                x = () => gameWin.y1()
            };
        }
    }
}