using init.sprite.UI;
using snake2d;
using util.colors;

namespace init.tech
{
    internal static class TechIcon
    {
        public static SPRITE Icon(TECH t)
        {
            if (t.lockers.all().Count == 1)
            {
                return t.lockers.all()[0].lockable.icon.Resized(Icon.HUGE);
            }
            else if (t.lockers.all().Count == 2 && t.lockers.all()[0].lockable.icon == t.lockers.all()[1].lockable.icon)
            {
                return t.lockers.all()[0].lockable.icon.Resized(Icon.HUGE);
            }
            else if (t.lockers.all().Count > 1)
            {
                SPRITE bg = new SPRITE.Imp(Icon.HUGE)
                {
                    render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                    {
                        int w = (X2 - X1) / 2;
                        int h = (Y2 - Y1) / 2;
                        for (int i = 0; i < t.lockers.all().Count && i < 4; i++)
                        {
                            int dx = (i % 2) * w;
                            int dy = (i / 2) * h;
                            t.lockers.all()[i].lockable.icon.Render(r, X1 + dx, X1 + w + dx, Y1 + dy, Y1 + h + dy);
                        }
                    }
                };
                return bg;
            }
            else if (t.boosters.all().Count == 1)
            {
                SPRITE bg = new SPRITE.Scaled(t.boosters.all()[0].boostable.nativeIcon, 1.5f);
                return Get(bg, UI.Icons().s.plus2.Scaled(2).CreateColored(GCOLOR.T().IGREAT));
            }
            else if (t.boosters.all().Count > 1)
            {
                SPRITE bg = new SPRITE.Imp(Icon.HUGE)
                {
                    render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                    {
                        int w = (X2 - X1) / 2;
                        int h = (Y2 - Y1) / 2;
                        for (int i = 0; i < t.boosters.all().Count && i < 4; i++)
                        {
                            int dx = (i % 2) * w;
                            int dy = (i / 2) * h;
                            t.boosters.all()[i].boostable.icon.Render(r, X1 + dx, X1 + w + dx, Y1 + dy, Y1 + h + dy);
                        }
                    }
                };
                return Get(bg, UI.Icons().s.plus2.Scaled(2).CreateColored(GCOLOR.T().IGREAT));
            }

            return UI.Icons().s.cancel;
        }

        private static SPRITE Get(SPRITE bg, SPRITE fg)
        {
            return new SPRITE.Imp(Icon.HUGE)
            {
                render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                {
                    bg.RenderC(r, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2);
                    fg.Render(r, X2 - fg.Width() + 8, Y1 - 8);
                }
            };
        }
    }
}