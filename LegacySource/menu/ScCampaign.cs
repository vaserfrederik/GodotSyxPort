using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.menu;

namespace menu
{
    class ScCampaign : Shadower, SC
    {
        private Campaign current;

        static readonly CharSequence ¤¤name = "¤campaigns";
        static readonly CharSequence ¤¤go = "go!";
        static
        {
            D.ts(typeof(ScCampaign));
        }

        private readonly Menu menu;
        private readonly HashSet<string> completed = PATHS.local().campaignsUnlocked();

        public ScCampaign(Menu menu)
        {
            this.menu = menu;

            MenuScreen screen = new MenuScreen(¤¤name, GUI.labelColor)
            {
                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };

            Add(screen);

            CLICKABLE b = new MenuScreen.ScreenButton(¤¤go)
            {
                protected override void clickA()
                {
                    start();
                }

                protected override void renAction()
                {
                    activeSet(canStart());
                }
            };

            screen.addButt(b);

            GuiSection s = new GuiSection();

            KeyMap<Campaign> cmap = new KeyMap<Campaign>();

            foreach (string f in PATHS.MISC().CAMPAIGNS.getFiles())
            {
                Campaign c = new Campaign(new Json(PATHS.MISC().CAMPAIGNS.gets(f)), f);
                cmap.put(f, c);
            }

            {
                LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

                foreach (Campaign c in cmap.allSorted())
                {
                    SPRITE sp = new SPRITE.Imp(400, UI.FONT().H2.height() * 2 + 8)
                    {
                        Text t = UI.FONT().H2.getText(17),
                        {
                            t.setMaxWidth(368);
                            t.setMultipleLines(true);
                            t.add(c.info.name);
                        }
                        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            t.renderCY(r, X1 + 32, Y1 + (Y2 - Y1) / 2);

                            COLOR col = COLOR.GREEN100;
                            if (c.locked())
                                col = COLOR.WHITE50;
                            else if (!completed.Contains(c.key))
                                col = COLOR.BLUEISH;
                            col.bind();
                            UI.icons().s.dot.big.renderCY(r, X1, Y1 + (Y2 - Y1) / 2);
                        }
                    };

                    rows.add(new GUI.Button(sp)
                    {
                        {
                            body.incrW(24);
                        }

                        protected override void renAction()
                        {
                            selectedSet(current == c);
                        }

                        protected override void clickA()
                        {
                            current = c;
                            if (MButt.LEFT.isDouble())
                            {
                                start();
                            }
                        }
                    });
                }

                s.add(new GScrollRows(rows, 400).view());
            }

            {
                GuiSection ss = new GuiSection();
                ss.add(new GStat(UI.FONT().H2)
                {
                    public override void update(GText text)
                    {
                        if (current != null)
                        {
                            text.color(COLORS.unclickable);
                            text.add(current.info.name);
                        }
                    }
                }.r(DIR.N));

                GETTER<CharSequence> g = new GETTER<CharSequence>()
                {
                    public CharSequence get()
                    {
                        if (current == null)
                            return Dic.empty;
                        else
                        {
                            Str.TMP.clear();
                            Str.TMP.add(current.info.desc);
                            return Str.TMP;
                        }
                    }
                };

                GTextScroller sc = new GTextScroller(UI.FONT().M, g, 400, 300);

                ss.addRelBody(8, DIR.S, sc);

                SPRITE sp = new SPRITE.Imp(400, UI.FONT().H2.height() * 3 + 8)
                {
                    Text t = UI.FONT().M.getText(17),
                    {
                        t.setMaxWidth(400);
                        t.setMultipleLines(true);
                    }
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        if (current == null)
                            return;

                        COLOR col = COLOR.GREEN100;
                        if (current.locked())
                            col = COLOR.REDISH;
                        else if (!completed.Contains(current.key))
                            col = COLOR.BLUEISH;
                        col.bind();

                        t.clear();
                        if (current.requires.Length > 0)
                        {
                            t.add(Dic.¤¤Requires).add(':');
                            t.s();
                            foreach (string s in current.requires)
                            {
                                if (cmap.containsKey(s))
                                    t.add(cmap.get(s).info.name);
                                else
                                {
                                    t.add('?').s().add(s);
                                }
                            }
                        }

                        t.renderCY(r, X1, Y1 + (Y2 - Y1) / 2);
                    }
                };

                ss.addRelBody(8, DIR.S, sp);

                s.addRelBody(64, DIR.E, ss);
            }

            s.body().centerIn(body());

            Add(s);
        }

        private void start()
        {
            if (canStart())
            {
                GameLoader loader = new GameLoader(current.save, current.scripts);

                menu.start(CutScene.make(current.cutsceneData, current.cutsceneText, loader));
            }
        }

        private bool canStart()
        {
            return current != null && !current.locked();
        }

        public bool back(Menu menu)
        {
            menu.switchScreen(menu.main);
            return true;
        }

        private class Campaign
        {
            public readonly Json cutsceneData;
            public readonly Json cutsceneText;
            public readonly INFO info;
            public readonly string[] requires;
            public readonly Path save;
            public readonly string[] scripts;
            public readonly string key;

            public Campaign(Json json, string key)
            {
                this.key = key;
                Json text = new Json(PATHS.TEXT().getFolder("campaign").gets(key));
                info = new INFO(text);
                cutsceneText = text.json("CUTSCENE");
                cutsceneData = json.json("CUTSCENE");
                requires = json.values("REQUIRES");
                if (json.bool("SAVE_LOCAL"))
                {
                    Path s = PATHS.local().SAVE_CAMPAIGN.exists(key) ? PATHS.local().SAVE_CAMPAIGN.get(key) : null;
                    if (s != null)
                    {
                        GameSpec f = GameSpec.get(s);
                        if (VERSION.versionMajor(f.version) != VERSION.VERSION_MAJOR)
                            s = null;
                    }
                    save = s;
                }
                else
                    save = PATHS.MISC().SAVES_CAMPAIGN.get(key);

                string[] ss = json.values("SCRIPTS");
                ArrayListGrower<ScriptLoad> scripts = new ArrayListGrower<ScriptLoad>();
                for (int i = 0; i < ss.Length; i++)
                {
                    if (!PATHS.SCRIPT().jar.exists(ss[i]))
                    {
                        json.error(PATHS.SCRIPT().jar.get().toAbsolutePath() + " /" + ss[i] + " does not exist", "SCRIPTS");
                    }
                    else
                    {
                        scripts.add(ScriptEngine.getInJar(ss[i]));

                    }
                }
                this.scripts = new string[scripts.size()];
                int ii = 0;
                foreach (ScriptLoad l in scripts)
                    this.scripts[ii++] = l.key;
            }

            public bool locked()
            {
                if (save == null)
                    return true;
                foreach (string s in requires)
                    if (!completed.Contains(s))
                        return true;
                return false;
            }
        }
    }
}