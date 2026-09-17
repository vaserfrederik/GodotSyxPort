using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sprite.text;
using util.text;

namespace launcher
{
    class ScreenLog : GuiSection
    {
        private readonly Lines lines;

        public ScreenLog(Launcher l)
        {
            D.gInit(this);
            add(new GUI.Header(l.res, D.g("Change-Log")), 20, 16);

            CLICKABLE b = new BText(l.res, "BACK").clickActionSet(new ACTION(() => l.setMain()));
            b.body().moveX2(Sett.WIDTH - 20).moveCY(getLast().cY());
            add(b);

            lines = new Lines(l.res, Sett.HEIGHT - body().y2() - 16);
            lines.body().moveX1Y1(10, body().y2() + 8);
            add(lines);

            CLICKABLE up = new GUI.BSprite(l.res.arrowUpDown[0]).clickActionSet(new ACTION(() => lines.top -= 5));
            up.body().moveY1(60 + 25).moveX2(Sett.WIDTH - 10);
            add(up);

            CLICKABLE down = new GUI.BSprite(l.res.arrowUpDown[1]).clickActionSet(new ACTION(() => lines.top += 5));
            down.body().moveY2(Sett.HEIGHT - 60).moveX2(Sett.WIDTH - 10);
            add(down);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            OPACITY.O75.bind();
            COLOR.BLACK.render(r, 0, Sett.WIDTH, 0, Sett.HEIGHT);
            OPACITY.unbind();

            base.render(r, ds);
        }

        private class Lines : RenderImp
        {
            private readonly CharSequence[] lines = lines();
            public int top = 0;
            private readonly string sep = "-";
            private readonly COLOR[] cols = new COLOR[] {
                COLOR.WHITE100,
                new ColorImp(127, 110, 100),
            };

            private readonly Font font;

            public Lines(RES res, int height)
            {
                font = res.font;
                body().setWidth(Sett.WIDTH - 100).setHeight(height);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                int i = (int)MButt.clearWheelSpin();
                top -= i;

                if (top >= lines.Length - 1)
                    top = lines.Length - 1;
                if (top < 0)
                    top = 0;

                int y1 = body().y1();
                int line = top;
                while (line < lines.Length && y1 < body().y2())
                {
                    y1 = render(y1, lines[line], r, cols[line & 1]);
                    line++;
                }
            }

            private int render(int y1, CharSequence s, SPRITE_RENDERER r, COLOR color)
            {
                int start = 0;
                color.bind();
                while (true)
                {
                    int end = font.getEndIndex(s, start, body().width());
                    int y2 = y1 + font.height();
                    if (y2 < body().y2())
                    {
                        if (start == 0)
                        {
                            if (s.length() > 0 && s.charAt(0) == '!')
                            {
                                start = 1;
                                COLOR.BLUEISH.bind();
                            }
                            else if (s.length() > 0)
                            {
                                font.render(r, sep, body().x1(), y1);
                            }
                        }

                        font.render(r, s, body().x1() + 20, y1, start, end, 1);
                    }
                    y1 = y2;
                    start = end;
                    if (start == s.length())
                        break;
                }
                COLOR.unbind();
                return y1 + 2;
            }

            private CharSequence[] lines()
            {
                try
                {
                    List<string> ss = File.ReadAllLines(PATHS.BASE().TXT.get("Patchnotes"));
                    CharSequence[] res = new CharSequence[ss.Count];
                    for (int i = 0; i < ss.Count; i++)
                        res[i] = ss[i];
                    return res;
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }

                return new CharSequence[] { "error" };
            }
        }
    }
}