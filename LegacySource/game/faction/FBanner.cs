using System;
using System.IO;
using System.Text;
using snake2d;

namespace game.faction
{
    public class FBanner : FactionResource
    {
        private static readonly string ¤¤name = "¤Banner";

        static FBanner()
        {
            D.ts(typeof(FBanner));
        }

        public readonly BitmapSprite sprite = new BitmapSprite();

        private byte bannerI = (byte)RND.rInt(SPRITES.icons().l.banners.Length);
        private readonly ColorImp background = new ColorImp(RND.rInt(87), RND.rInt(87), RND.rInt(87));
        private readonly ColorImp foreground = new ColorImp(40 + RND.rInt(87), 40 + RND.rInt(87), 40 + RND.rInt(87));
        private readonly ColorImp border = foreground.shade(0.25f);
        private readonly ColorImp pole = new ColorImp(35 + RND.rInt0(5), 35 + RND.rInt0(5), 35 + RND.rInt(5));
        private static readonly ColorImp tmp = new ColorImp();

        public FBanner(Faction f)
        {
        }

        public ColorImp colorBG()
        {
            return background;
        }

        public ColorImp colorBGBright()
        {
            tmp.set(background).setMinBrightnessSelf(0.75f);
            return tmp;
        }

        public ColorImp colorFG()
        {
            return foreground;
        }

        public ColorImp colorBorder()
        {
            return border;
        }

        public ColorImp colorPole()
        {
            return pole;
        }

        public int bannerType()
        {
            return bannerI;
        }

        public void bannerTypeSet(int i)
        {
            i &= 0xFFFF;
            bannerI = (byte)(i % SPRITES.icons().l.banners.Length);
        }

        protected override void save(FilePutter file)
        {
            sprite.save(file);
            file.b(bannerI);
            background.save(file);
            foreground.save(file);
            border.save(file);
            pole.save(file);
        }

        protected override void load(FileGetter file)
        {
            sprite.load(file);
            bannerI = file.b();
            background.load(file);
            foreground.load(file);
            border.load(file);
            pole.load(file);
        }

        protected override void clear()
        {
        }

        public static string name()
        {
            return ¤¤name;
        }

        public static void render(SPRITE_RENDERER r, Faction f, int X1, int X2, int Y1, int Y2)
        {
            if (f == null)
            {
                UI.icons().s.crazy.render(r, X1, X2, Y1, Y2);
                return;
            }

            int width = X2 - X1;
            int height = Y2 - Y1;

            double dx = (double)width / BitmapSprite.WIDTH;
            double dy = (double)height / BitmapSprite.HEIGHT;

            COLOR col = f.banner().colorBGBright();

            for (double ry = 0; ry < height; ry += dy)
            {
                for (double rx = 0; rx < width; rx += dx)
                {
                    int px = (int)(rx / dx);
                    int py = (int)(ry / dy);

                    int x = (int)(X1 + rx);
                    int y = (int)(Y1 + ry);
                    if (f.banner().sprite.is(px, py))
                    {
                        col.render(r, x, (int)Math.Ceiling(x + dx), y, (int)Math.Ceiling(y + dy));
                    }
                }
            }
        }

        public readonly SPRITE MEDIUM = new SPRITE
        {
            width = () => Icon.M,
            height = () => Icon.M,
            renderTextured = (texture, X1, X2, Y1, Y2) => { },
            render = (r, X1, nopeX, Y1, nopeY) =>
            {
                SPRITES.icons().m.circle_frame.render(r, X1, Y1);
                background.bind();
                SPRITES.icons().m.circle_inner.render(r, X1, Y1);

                int sx = X1 + (width() - BitmapSprite.WIDTH) / 2;
                int sy = Y1 + (height() - BitmapSprite.HEIGHT) / 2;

                for (int y = 0; y < BitmapSprite.HEIGHT; y++)
                {
                    for (int x = 0; x < BitmapSprite.WIDTH; x++)
                    {
                        if (sprite.is(x, y))
                        {
                            foreground.render(r, sx + x, sx + x + 1, sy + y, sy + y + 1);
                        }
                        else if (sprite.is(x, y, DIR.N) || sprite.is(x, y, DIR.E))
                        {
                            border.render(r, sx + x, sx + x + 1, sy + y, sy + y + 1);
                        }
                    }
                }
            }
        };

        public readonly SPRITE BIG = new SPRITE
        {
            width = () => Icon.L,
            height = () => Icon.L,
            renderTextured = (texture, X1, X2, Y1, Y2) =>
            {
                SPRITES.icons().l.banners[bannerI].renderTextured(texture, X1, X2, Y1, Y2);
                SPRITES.icons().l.bannerPole.renderTextured(texture, X1, X2, Y1, Y2);
            },
            render = (r, X1, nopeX, Y1, nopeY) =>
            {
                background.bind();
                SPRITES.icons().l.banners[bannerI].render(r, X1, nopeX, Y1, nopeY);
                pole.bind();
                SPRITES.icons().l.bannerPole.render(r, X1, nopeX, Y1, nopeY);

                sprite.scaled(r, X1 + (width() - BitmapSprite.WIDTH * 2) / 2, Y1 + (height() - BitmapSprite.HEIGHT * 2) / 2 + 1, 2, foreground, border, border);
            }
        };

        public readonly SPRITE HUGE = new SPRITE
        {
            width = () => Icon.HUGE,
            height = () => Icon.HUGE,
            renderTextured = (texture, X1, X2, Y1, Y2) =>
            {
                SPRITES.icons().l.banners[bannerI].renderTextured(texture, X1, X2, Y1, Y2);
                SPRITES.icons().l.bannerPole.renderTextured(texture, X1, X2, Y1, Y2);
            },
            render = (r, X1, nopeX, Y1, nopeY) =>
            {
                background.bind();
                SPRITES.icons().l.banners[bannerI].render(r, X1, nopeX, Y1, nopeY);
                pole.bind();
                SPRITES.icons().l.bannerPole.render(r, X1, nopeX, Y1, nopeY);

                sprite.scaled(r, X1 + (width() - BitmapSprite.WIDTH * 4) / 2, Y1 + (height() - BitmapSprite.HEIGHT * 4) / 2 + 2, 4, foreground, border, border);
            }
        };

        public static class rebel
        {
            public static readonly SPRITE MEDIUM = new SPRITE.Imp(Icon.M)
            {
                render = (r, X1, X2, Y1, Y2) =>
                {
                    SPRITES.icons().m.circle_frame.render(r, X1, Y1);
                    GCOLOR.MAP().F_REBEL.bind();
                    SPRITES.icons().m.circle_inner.render(r, X1, Y1);
                    COLOR.unbind();
                }
            };

            public static readonly SPRITE BIG = new SPRITE.Imp(Icon.L)
            {
                render = (r, X1, X2, Y1, Y2) =>
                {
                    GCOLOR.MAP().F_REBEL.bind();
                    SPRITES.icons().l.banners[0].render(r, X1, X2, Y1, Y2);
                    COLOR.WHITE30.bind();
                    SPRITES.icons().l.bannerPole.render(r, X1, X2, Y1, Y2);
                    COLOR.unbind();
                }
            };

            public static readonly SPRITE HUGE = new SPRITE.Imp(Icon.L * 2)
            {
                render = (r, X1, X2, Y1, Y2) =>
                {
                    GCOLOR.MAP().F_REBEL.bind();
                    SPRITES.icons().l.banners[0].render(r, X1, X2, Y1, Y2);
                    COLOR.WHITE30.bind();
                    SPRITES.icons().l.bannerPole.render(r, X1, X2, Y1, Y2);
                    COLOR.unbind();
                }
            };
        }

        protected override void update(double ds, Faction f)
        {
        }
    }
}