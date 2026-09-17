using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using launcher.GUI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.text;

namespace launcher
{
    class ScreenMods : GuiSection
    {
        {
            D.gInit(this);
        }

        private ModInfo hoveredMod = null;
        private string errorMod = null;
        private string errorMessage = null;
        private Text hs;
        private readonly GuiSection mods = new GuiSection();
        private readonly Launcher l;

        private readonly CharSequence sOutdated = "(" + D.g("Outdated") + ")";
        private readonly CharSequence sBy = D.g("Author") + ": ";
        private readonly CharSequence sBroken = D.g("mborked", "Unsupported Mod");

        public ScreenMods(Launcher l)
        {
            hs = new Text(l.res.font, 200).setScale(1);
            this.l = l;

            {
                GuiSection butts = new GuiSection();

                CLICKABLE b = new BText(l.res, D.g("Play"), 200)
                {
                    protected override void clickA()
                    {
                        if (PATHS.SCRIPT().hasExternal(l.s.mods.get()) || l.s.mods.get().Length > 0)
                        {
                            l.setModWarning();
                            return;
                        }

                        l.s.save();
                        Launcher.startGame = true;
                        CORE.annihilate();
                    }
                };

                butts.addRightC(64, b);

                b = new BText(l.res, D.g("Back"), 200).clickActionSet(new ACTION()
                {
                    public override void exe()
                    {
                        l.setMain();
                    }
                });
                butts.addRightC(4, b);

                butts.body().moveX2(Sett.WIDTH - 16);
                butts.body().moveY1(0);

                RENDEROBJ rs = new GUI.Header(l.res, D.g("Mods"));
                rs.body().moveX1(64);
                rs.body().moveCY(butts.body().cY());
                butts.add(rs);

                add(butts);
            }

            mods.body().setHeight(Sett.HEIGHT - body().height() - 24);
            add(mods, 10, body().y2() + 16);

            update(0);

            int am = 0;
            foreach (var s in l.s.mods.get())
            {
                if (PATHS.local().MODS.exists(s))
                    am++;
            }
            string[] modsArray = new string[am];
            am = 0;
            foreach (var s in l.s.mods.get())
            {
                if (PATHS.local().MODS.exists(s))
                    modsArray[am++] = s;
            }
            l.s.mods.set(modsArray);

            body().moveX1Y1(10, 10);

            update(0);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            OPACITY.O75.bind();
            COLOR.BLACK.render(r, 0, Sett.WIDTH, 0, Sett.HEIGHT);
            OPACITY.unbind();

            base.render(r, ds);

            int sx = body().x1() + 470;
            int y1 = body().y1() + 70;
            if (hoveredMod != null)
            {
                hs.setMaxWidth(400);

                hs.clear().add(hoveredMod.name).add(' ').add(hoveredMod.version);
                COLOR.GREEN100.bind();
                if (hoveredMod.majorVersion != VERSION.VERSION_MAJOR)
                {
                    COLOR.ORANGE100.bind();
                    hs.add(sOutdated);
                }
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();
                COLOR.unbind();

                hs.clear().add(hoveredMod.desc);
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();

                COLOR.BLUEISH.bind();
                hs.clear().add(sBy).add(hoveredMod.author);
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();
                COLOR.GREENISH.bind();
                hs.clear().add(hoveredMod.info);
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();
                COLOR.unbind();

                hs.clear();
                hs.add(hoveredMod.absolutePath);
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();

                hoveredMod = null;
            }
            else if (errorMod != null)
            {
                hs.clear();
                hs.add(errorMessage);
                hs.adjustWidth();
                hs.render(r, sx, y1);
                y1 += hs.height();

                hs.clear();
                hs.add(errorMod);
                hs.adjustWidth();
                hs.render(r, sx, y1);
            }
            errorMod = null;
        }

        private void update(double ds)
        {
            string[] paths = PATHS.local().MODS.folders();

            ScrollBox labels = new ScrollBox(this.mods.body().height());

            foreach (var st in paths)
            {
                ModInfo i;
                try
                {
                    i = new ModInfo(st);
                    labels.add(new ModButt(i, l));
                }
                catch (ModInfoException e)
                {
                    labels.add(new Borked("" + PATHS.local().MODS.getFolder(st).get().toAbsolutePath(), e.Message, l));

                    if (l.s.mods.get().Length > 0)
                    {
                        bool contains = false;
                        foreach (var s in l.s.mods.get())
                        {
                            if (s == st)
                            {
                                contains = true;
                            }
                        }
                        if (contains)
                        {
                            string[] mods = new string[l.s.mods.get().Length - 1];
                            int k = 0;
                            foreach (var s in l.s.mods.get())
                            {
                                if (s != st)
                                {
                                    mods[k] = s;
                                    k++;
                                }
                            }
                            l.s.mods.set(mods);
                            l.s.save();
                        }
                    }
                }
            }

            labels.body().incr(120, 110);

            CLICKABLE up = new BSprite(l.res.arrowUpDown[0]).clickActionSet(new ACTION()
            {
                public override void exe()
                {
                    labels.scrollUp();
                }
            });

            CLICKABLE down = new BSprite(l.res.arrowUpDown[1]).clickActionSet(new ACTION()
            {
                public override void exe()
                {
                    labels.scrollDown();
                }
            });

            labels.add(up);
            labels.add(down);

            mods.add(labels);
        }

        private void toggle(Butt butt)
        {
            // Implementation for toggle
        }

        private class ModButt : Butt
        {
            private readonly ModInfo i;
            private readonly Launcher l;

            public ModButt(ModInfo i, Launcher l) : base(i.majorVersion == VERSION.VERSION_MAJOR ? COLOR.GREEN100 : COLOR.ORANGE100, l.res, i.name)
            {
                this.i = i;
                this.l = l;
            }

            protected override void clickA()
            {
                int selectedIndex = Array.IndexOf(l.s.mods.get(), i.path);
                if (selectedIndex == -1)
                {
                    string[] mods = new string[l.s.mods.get().Length + 1];
                    l.s.mods.get().CopyTo(mods, 0);
                    mods[mods.Length - 1] = i.path;
                    l.s.mods.set(mods);
                }
                else
                {
                    string[] mods = l.s.mods.get().Where(s => s != i.path).ToArray();
                    l.s.mods.set(mods);
                }

                l.toggleMod(this);
                base.clickA();
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    hoveredMod = i;
                    return true;
                }
                return false;
            }
        }

        private class Borked : Butt
        {
            private readonly string path;
            private readonly string message;

            public Borked(string path, string message, Launcher l) : base(COLOR.REDISH, l.res, sBroken)
            {
                this.path = path;
                this.message = message;
            }

            protected override void clickA()
            {
                FileManager.openDesctop(path);
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    errorMod = path;
                    errorMessage = message;
                    return true;
                }
                return false;
            }
        }

        public static abstract class Butt : GUI.Button
        {
            protected Butt(COLOR col, RES res, CharSequence text) : base(sp(res, text, col))
            {
            }

            private static SPRITE sp(RES res, CharSequence text, COLOR color)
            {
                return new SPRITE.Imp(380, res.font.height() + 8)
                {
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        color.bind();
                        res.font.renderCropped(r, text, X1 + 48, Y1 + 4, width() - 48);
                        COLOR.unbind();
                    }
                };
            }
        }
    }
}