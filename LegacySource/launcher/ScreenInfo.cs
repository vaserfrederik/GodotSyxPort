using System;
using System.IO;
using System.Globalization;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;

namespace launcher
{
    class ScreenInfo : GuiSection
    {
        private Str hoverInfo = new Str(200);
        private readonly Launcher l;

        public ScreenInfo(Launcher l)
        {
            D.gInit(this);
            this.l = l;

            {
                RENDEROBJ r = new GUI.Header(l.res, D.g("Version"));
                CLICKABLE c = new BText(l.res, VERSION.VERSION_STRING, 200).clickActionSet(new ACTION()
                {
                    public void exe()
                    {
                        l.setLog();
                    }
                });

                add(r, c, 0);
            }

            {
                CharSequence[] keys = new CharSequence[]
                {
                    D.g("Platform"),
                    D.g("JRE"),
                    D.g("GPU"),
                    D.g("GPU-Driver"),
                };
                string[] values = new string[]
                {
                    System.getProperty("os.name", "generic").ToLower(CultureInfo.InvariantCulture),
                    System.getProperty("java.version") + " bits:" + System.getProperty("sun.arch.data.model"),
                    CORE.getGraphics().render(),
                    CORE.getGraphics().renderV(),
                };

                for (int i = 0; i < keys.Length; i++)
                {
                    RENDEROBJ r = new GUI.Header(l.res, keys[i]);
                    add(r, new RENDEROBJ.Sprite(new Text(l.res.font, values[i]).setScale(1)), 2);
                }
            }

            COLOR clink = new ColorImp(20, 100, 100);
            COLOR clinkH = new ColorImp(20, 127, 127);

            {
                CharSequence[] keys = new CharSequence[]
                {
                    D.g("localF", "Local Files"),
                    D.g("Saves"),
                    D.g("Screenshots"),
                    D.g("Mods"),
                };
                string[] values = new string[]
                {
                    "" + PATHS.local().ROOT.get(),
                    "" + PATHS.local().save().get(),
                    "" + PATHS.local().SCREENSHOT.get(),
                    "" + PATHS.local().MODS.get(),
                };

                for (int i = 0; i < keys.Length; i++)
                {
                    string v = values[i];
                    RENDEROBJ r = new GUI.Header(l.res, keys[i]);
                    CLICKABLE c = new ClickableAbs()
                    {
                        Text t = new Text(l.res.font, v).setScale(1);

                        {
                            body.setDim(t);
                        }

                        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                        {
                            if (isHovered)
                                clinkH.bind();
                            else
                                clink.bind();
                            t.render(r, body);
                            COLOR.unbind();
                        }

                        protected override void clickA()
                        {
                            FileManager.openDesctop(v);
                        }
                    };
                    add(r, c, 2);
                }
            }

            {
                RENDEROBJ r;
                CLICKABLE c;

                r = new GUI.Header(l.res, D.g("Contact"));
                c = new ClickableAbs()
                {
                    Text t = new Text(l.res.font, "info@songsofsyx.com").setScale(1);

                    {
                        body.setDim(t);
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        if (isHovered)
                            clinkH.bind();
                        else
                            clink.bind();
                        t.render(r, body);
                        COLOR.unbind();
                    }

                    protected override void clickA()
                    {
                        FileManager.sendEmail("info@songsofsyx.com", "Greetings, oh great dev", "Inquiry");
                    }
                };
                add(r, c, 2);

                r = new GUI.Header(l.res, D.g("Road-map"));
                c = new ClickableAbs()
                {
                    Text t = new Text(l.res.font, "https://trello.com/b/wF5RYqdF/songs-of-syx");

                    {
                        body.setDim(t);
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        if (isHovered)
                            clinkH.bind();
                        else
                            clink.bind();
                        t.render(r, body);
                        COLOR.unbind();
                    }

                    protected override void clickA()
                    {
                        try
                        {
                            ScreenMain.openBrowser("https://trello.com/b/wF5RYqdF/songs-of-syx");
                        }
                        catch (IOException e)
                        {
                            e.printStackTrace();
                        }
                    }
                };
                add(r, c, 2);
            }

            RENDEROBJ b = new BText(l.res, D.g("Back")).clickActionSet(new ACTION()
            {
                public void exe()
                {
                    l.setMain();
                }
            });
            b.body().moveX2(Sett.WIDTH - 40).moveY1(body().y1());
            add(b);

            body().moveX1Y1(8, 8);
        }

        private void add(RENDEROBJ title, RENDEROBJ oo, int dy)
        {
            title.body().moveY1(body().y2() + dy);
            title.body().moveX2(150);
            add(title);
            addRightC(10, oo);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            OPACITY.O75.bind();
            COLOR.BLACK.render(r, 0, Sett.WIDTH, 0, Sett.HEIGHT);
            OPACITY.unbind();
            base.render(r, ds);
            if (hoverInfo.length() != 0)
            {
                GUI.c_label.bind();
                l.res.font.render(r, hoverInfo, 40, 315, 450, 1);
                hoverInfo.clear();
            }
            COLOR.unbind();
        }
    }
}