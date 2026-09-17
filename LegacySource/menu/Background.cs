using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.light;
using snake2d.util.rnd;

namespace menu
{
    internal class Background
    {
        private float speed = 24;
        private readonly int tilesX;
        private readonly int maxWidth;
        private readonly RECTANGLE bounds;
        private readonly Rec bgBounds;
        private readonly Rec fameBounds;

        Fire torch1 = new Fire(7);
        Fire torch2 = new Fire(7);
        Fire torch3 = new Fire(0.2);
        private readonly TILE_SHEET tiles;
        private readonly TILE_SHEET tilescr;

        static RECTANGLE shadow;

        public Background(Menu menu, RECTANGLE bounds2)
        {
            this.tiles = menu.res.s().background;
            tilesX = menu.res.s().backgroundTilesX;
            tilescr = menu.res.s().backgroundCr;
            maxWidth = tilesX * 32 * 2;
            this.bounds = new Rec(0, C.WIDTH(), (C.HEIGHT() - 256 * 3) / 2, (C.HEIGHT() - 256 * 3) / 2 + 256 * 3);
            bgBounds = new Rec(0, bounds.width(), 0, bounds.height());
            bgBounds.moveX1(RND.rInt(maxWidth - bounds.width()));
            fameBounds = new Rec(0, bounds.width(), 0, bounds.height() - 1);
            fameBounds.moveX2(maxWidth);
            bgBounds.moveX1(0);
            torch1.flicker(1f);
            torch2.flicker(1f);

            double d = bounds.width() / C.MIN_WIDTH;

            torch1.setRadius((int)(1300.0 * d));
            torch1.set(bounds.x1() - 150, C.HEIGHT() / 2);
            torch1.setFalloff(2f);
            torch1.setFlickerFactor(20f);
            torch1.setZ(50);
            torch2.setFalloff(3f);
            torch2.setRadius((int)(1300.0 * d));
            torch2.set(bounds.x2() + 150, C.HEIGHT() / 2);
            torch2.setFlickerFactor(20f);
            torch2.setZ(50);

            torch3.setFalloff(3f);
            torch3.setRadius((int)(300.0 * bounds.width() / C.MIN_WIDTH));
            torch3.setFlickerFactor(11f);
            torch3.setZ(60);
        }

        private readonly AmbientLight s = new AmbientLight(0.1f, 0.05f, 0.025f, 90, 20);
        private readonly AmbientLight moon = new AmbientLight(0.1615f, 0.1615f, 0.23f, 90, 35);

        public void render(SPRITE_RENDERER r, float ds)
        {
            torch1.flicker(ds);
            torch2.flicker(ds);

            byte full = -1;
            byte none = 0;
            int fadeW = 100;
            CORE.renderer().registerLight(torch1, bounds.x1(), bounds.x1() + fadeW, bounds.y1(), bounds.y2(), full, full, none, none);
            CORE.renderer().registerLight(torch1, bounds.x1() + fadeW, bounds.x2(), bounds.y1(), bounds.y2(), full, full, full, full);

            CORE.renderer().registerLight(torch2, bounds.x2() - fadeW, bounds.x2(), bounds.y1(), bounds.y2(), none, none, full, full);
            CORE.renderer().registerLight(torch2, bounds.x1(), bounds.x2() - fadeW, bounds.y1(), bounds.y2(), full, full, full, full);

            bgBounds.incrX(speed * ds);
            if (bgBounds.x2() >= maxWidth && speed > 0)
            {
                bgBounds.moveX2(bgBounds.x2());
                if (speed > 0)
                    speed *= -1;
            }
            else if (bgBounds.x1() <= 0 && speed < 0)
            {
                bgBounds.moveX1(-bgBounds.x1());
                if (speed < 0)
                    speed *= -1;
            }
            s.setDir(180);
            s.set(torch1.getRed() * 0.05f, torch1.getGreen() * 0.05f, torch1.getBlue() * 0.05f);

            moon.register(bounds);
            //s.register(bounds);
            //forBounds.incrementX(C.TILE_SIZE*3.0*ds);
            render(r, bounds.x1(), bounds.y1(), bgBounds, tiles, tilesX);
            //pillsmall1.render(ds);
            //pillsmall2.render(ds);
            //pillbig.render(ds);
            //Sprites.foreground.render(0, bounds.getY2()-Sprites.foreground.getGameHeight(), forBounds);

            if (shadow != null)
            {
                OPACITY.O50.bind();
                COLOR.BLACK.render(r, shadow);
                shadow = null;
                OPACITY.unbind();
            }
        }

        public void renderFame(SPRITE_RENDERER r, float ds, COORDINATE mCoo, double ran)
        {
            torch1.flicker(ds);
            torch2.flicker(ds);
            byte full = -1;
            byte none = 0;
            int fadeW = 100;
            CORE.renderer().registerLight(torch1, bounds.x1(), bounds.x1() + fadeW, bounds.y1(), bounds.y2(), full, full, none, none);
            CORE.renderer().registerLight(torch1, bounds.x1() + fadeW, bounds.x2(), bounds.y1(), bounds.y2(), full, full, full, full);

            CORE.renderer().registerLight(torch2, bounds.x2() - fadeW, bounds.x2(), bounds.y1(), bounds.y2(), none, none, full, full);
            CORE.renderer().registerLight(torch2, bounds.x1(), bounds.x2() - fadeW, bounds.y1(), bounds.y2(), full, full, full, full);

            torch3.set(mCoo);
            torch3.flicker(ds);
            torch3.register();

            int ww = 48 * 32 * 2;

            fameBounds.moveX1(ran * (ww - bounds.width()));
            render(r, bounds.x1(), bounds.y1(), fameBounds, tilescr, 48);
        }

        private static void render(SPRITE_RENDERER r, int x1, int y1, RECTANGLE bb, TILE_SHEET tiles, int tilesX)
        {
            int dx = bb.x1() % tiles.size();
            x1 -= dx;

            int sx = bb.x1() / tiles.size();

            int ys = (int)Math.Ceiling((double)bb.height() / tiles.size());
            int xs = (int)Math.Ceiling((double)(bb.width() + dx) / tiles.size());

            for (int y = 0; y < ys; y++)
            {
                for (int x = 0; x < xs && x < tilesX; x++)
                {
                    int t = sx + tilesX * y + x;
                    if (t >= tiles.tiles())
                        continue;

                    tiles.render(r, t, x1 + x * tiles.size(), y1 + y * tiles.size());
                }
            }
        }
    }
}