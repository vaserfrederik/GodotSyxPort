using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.faction.player;
using init.paths;
using init.sprite.UI;
using menu.GUI;
using script;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.table;
using util.info;
using util.text;
using view.menu;

namespace menu
{
    class ScRandom : Shadower, SC
    {
        public readonly KeyMap<ScriptLoad> scripts = new KeyMap<ScriptLoad>();
        private CharSequence hname;
        private CharSequence hdesc;
        static CharSequence ¤¤name = "¤random game";
        private static CharSequence ¤¤Scripts = "Scripts";
        private static CharSequence ¤¤go = "go!";

        private static CharSequence ¤¤custom = "Custom Settings";
        private static CharSequence ¤¤customD = "Configure your own tweaks to the game.";

        private string selectedMode = null;

        static ScRandom()
        {
            D.ts(typeof(ScRandom));
        }

        public ScRandom(Menu menu)
        {
            ResFolder path = PATHS.PLAYER().folder("mode");
            if (PATHS.local().PROFILE.exists("Properties"))
            {
                Json old = new Json(PATHS.local().PROFILE.gets("Properties"));

                if (old.has("MODE"))
                {
                    selectedMode = old.value("MODE");
                }
            }
            else
            {
                PATHS.local().PROFILE.create("Titles2");
            }

            MenuScreen screen = new MenuScreen(¤¤name, GUI.labelColor)
            {
                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };
            add(screen);

            CLICKABLE b = new MenuScreen.ScreenButton(¤¤go)
            {
                protected override void clickA()
                {
                    menu.sandboxSettings.save();
                    menu.start(new CORE_STATE.Constructor()
                    {
                        public CORE_STATE getState()
                        {
                            string[] sc = new string[scripts.all().Count];
                            int si = 0;
                            foreach (ScriptLoad l in scripts.all())
                                sc[si++] = l.key;

                            CORE_STATE s = GAME.create(sc);

                            if (selectedMode == null)
                            {
                                menu.sandboxSettings.apply();
                            }
                            else
                            {
                                FACTIONS.player().bonusesCustom.setMode(selectedMode);
                            }

                            return s;
                        }
                    });
                }
            };

            screen.addButt(b);

            GRows rr = new GRows(6);

            string mode = null;
            foreach (string key in path.init.getFiles())
            {
                Json j = new Json(path.init.gets(key));
                INFO info = new INFO(new Json(path.text.gets(key)));
                SPRITE ico = menu.res.s().modeIcons[j.i("ICON_I")];
                CLICKABLE c = new CLICKABLE.ClickableAbs(IconMaker.WIDTH * 2 + 48, IconMaker.HEIGHT * 2 + 48)
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        GUI.labelColor.renderFrame(r, body, -4, 2);

                        if (isSelected)
                        {
                            COLOR.WHITE100.renderFrame(r, body, -8, 3);
                            COLOR.WHITE150.bind();
                        }
                        else if (isHovered)
                        {
                            COLOR.WHITE50.renderFrame(r, body, -8, 2);
                            COLOR.WHITE150.bind();
                        }

                        ico.renderCScaled(r, body.cX(), body.cY(), 2);

                        COLOR.unbind();
                    }

                    protected override void clickA()
                    {
                        selectedMode = key;
                    }

                    protected override void renAction()
                    {
                        selectedSet(selectedMode == key);
                    }

                    public override bool hover(COORDINATE mCoo)
                    {
                        if (base.hover(mCoo))
                        {
                            hname = info.name;
                            hdesc = info.desc;
                            return true;
                        }
                        return base.hover(mCoo);
                    }
                };
                if (selectedMode != null && key.Equals(selectedMode))
                {
                    mode = selectedMode;
                }
                rr.add(c);
            }

            selectedMode = mode;

            {
                CLICKABLE c = new CLICKABLE.ClickableAbs(IconMaker.WIDTH * 2 + 48, IconMaker.HEIGHT * 2 + 48)
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        GUI.labelColor.renderFrame(r, body, -4, 2);

                        if (isSelected)
                        {
                            COLOR.WHITE100.renderFrame(r, body, -8, 3);
                            COLOR.WHITE150.bind();
                        }
                        else if (isHovered)
                        {
                            COLOR.WHITE50.renderFrame(r, body, -8, 2);
                            COLOR.WHITE150.bind();
                        }

                        UI.icons().m.cog_big.renderCScaled(r, body.cX(), body.cY(), 2);

                        COLOR.unbind();
                    }

                    protected override void clickA()
                    {
                        selectedMode = null;
                        menu.switchScreen(menu.sandboxSettings);
                    }

                    protected override void renAction()
                    {
                        selectedSet(selectedMode == null);
                    }

                    public override bool hover(COORDINATE mCoo)
                    {
                        if (base.hover(mCoo))
                        {
                            hname = ¤¤custom;
                            hdesc = ¤¤customD;
                            return true;
                        }
                        return base.hover(mCoo);
                    }
                };
                rr.add(c);
            }

            GuiSection butts = new GuiSection();

            foreach (RENDEROBJ r in rr.rows())
                butts.addDown(8, r);

            butts.addRelBody(64, DIR.E, new Scripts(300));

            RENDEROBJ r = new RENDEROBJ.RenderImp(100, 48)
            {
                public void render(SPRITE_RENDERER r, float ds)
                {
                    if (hdesc != null)
                    {
                        GUI.labelColor.bind();
                        UI.FONT().H2.renderCX(r, body.cX(), body.y1(), hname);
                        COLOR.unbind();
                        UI.FONT().M.renderCX(r, body.cX(), body.y1() + 24, hdesc, 1.0, GUI.bounds.width());
                        hname = null;
                        hdesc = null;
                    }
                }
            };

            butts.addRelBody(8, DIR.S, r);

            butts.body().centerIn(body());

            add(butts);
        }

        public bool back(Menu menu)
        {
            menu.switchScreen(menu.main);
            return true;
        }

        private class Scripts : GuiSection
        {
            public Scripts(int height)
            {
                LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
                foreach (ScriptLoad l in ScriptEngine.getAll())
                {
                    if (!l.script.isSelectable())
                        continue;
                    CharSequence name = l.script.name();
                    CharSequence desc = l.script.desc();

                    CLICKABLE c = new GUI.CheckBox(name)
                    {
                        public override bool hover(COORDINATE mCoo)
                        {
                            if (base.hover(mCoo))
                            {
                                ScRandom.this.hname = name;
                                ScRandom.this.hdesc = desc;
                                return true;
                            }
                            return false;
                        }

                        protected override void clickA()
                        {
                            if (scripts.containsKey(l.className))
                            {
                                scripts.remove(l.className);
                                selectedSet(false);
                            }
                            else
                            {
                                scripts.put(l.className, l);
                                selectedSet(true);
                            }
                        }
                    };
                    rows.add(c);
                }
                if (rows.size() > 0)
                {
                    add(new GScrollRows(rows, height).view());
                    addRelBody(4, DIR.N, new HOVERABLE.Sprite(UI.FONT().H2.getText(¤¤Scripts), COLORS.label));
                }
            }
        }
    }
}