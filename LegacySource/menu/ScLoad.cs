using System;
using game;
using game.battle.state;
using game.save;
using init.paths;
using init.sprite.UI;
using menu.GUI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.menu;
using world.battle.spec;

namespace menu
{
    class ScLoad : SC
    {
        private static readonly string ¤¤showCases = "¤showcases";
        private static readonly string ¤¤custom = "¤scenarios";
        private static readonly string ¤¤battles = "¤battles";

        static ScLoad()
        {
            D.ts(typeof(ScLoad));
        }

        private readonly MenuScreenLoad screen;
        private readonly Menu menu;
        public readonly string name;

        public ScLoad(Menu menu, MenuScreenLoad screen, string name)
        {
            this.menu = menu;
            this.screen = screen;
            this.name = name;
        }

        public override bool hover(COORDINATE mCoo)
        {
            return screen.hover(mCoo);
        }

        public override bool click()
        {
            return screen.click();
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            screen.render(r, ds);
            Shadower.ren(r, ds);
        }

        public override bool back(Menu menu)
        {
            menu.switchScreen(menu.main);
            return true;
        }

        public bool hasSaves()
        {
            return screen.saves().Length != 0;
        }

        public void loadSave()
        {
            if (hasSaves())
            {
                menu.start(new GameLoader(PATHS.local().save().get(screen.saves()[0].fullName)));
            }
        }

        public static ScLoad load(Menu menu)
        {
            MenuScreenLoad screen = new MenuScreenLoad(MenuScreenLoad.¤¤name, GUI.labelColor, true, PATHS.local().save())
            {
                protected override void load(SaveFile f)
                {
                    menu.start(new GameLoader(f.path));
                }

                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };
            return new ScLoad(menu, screen, MenuScreenLoad.¤¤name);
        }

        public static ScLoad showcase(Menu menu)
        {
            MenuScreenLoad screen = new MenuScreenLoad(¤¤showCases, GUI.labelColor, false, PATHS.MISC().EXAMPLES)
            {
                protected override void load(SaveFile f)
                {
                    menu.start(new GameLoader(f.path)
                    {
                        public override void doAfterSet()
                        {
                            GAME.achieve(false);
                            base.doAfterSet();
                        }
                    });
                }

                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }

                protected override void renderInfo(SPRITE_RENDERER r, SaveFile file, RECTANGLE body, double ds)
                {
                    int y1 = renderInfoGen(r, file, body);
                    if (file.specReady() && file.spec().fubar)
                        renderInfoProb(r, file, body.x1(), y1);
                }

                protected override void renderName(SPRITE_RENDERER r, SaveFile s, RECTANGLE body)
                {
                    UI.FONT().H2.render(r, s.name, body.x1() + 64, body.y1());
                    UI.icons().s.human.renderCY(r, body.x1() + 700, body.y1() + UI.FONT().M.height() / 2);
                    Str.TMP.clear().add(s.pop);
                    UI.FONT().M.render(r, Str.TMP, body.x1() + 720, body.y1());
                }
            };
            return new ScLoad(menu, screen, ¤¤showCases);
        }

        public static ScLoad scenarios(Menu menu)
        {
            MenuScreenLoad screen = new MenuScreenLoad(¤¤custom, GUI.labelColor, false, PATHS.MISC().CUSTOM)
            {
                GText t = new GText(UI.FONT().M, 128)
                {
                    MaxWidth = 800,
                    MultipleLines = true
                };

                protected override void load(SaveFile f)
                {
                    menu.start(new GameLoader(f.path));
                }

                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }

                protected override void renderInfo(SPRITE_RENDERER r, SaveFile file, RECTANGLE body, double ds)
                {
                    t.set(file.spec().desc);
                    t.renderC(r, body);
                }

                protected override void renderName(SPRITE_RENDERER r, SaveFile s, RECTANGLE body)
                {
                    UI.FONT().H2.render(r, s.name, body.x1() + 64, body.y1() + UI.FONT().M.height() / 2);
                }
            };
            return new ScLoad(menu, screen, ¤¤custom);
        }

        public static ScLoad battle(Menu menu)
        {
            MenuScreenLoad screen = new MenuScreenLoad(¤¤battles, GUI.labelColor, false, PATHS.MISC().BATTLE)
            {
                GText t = new GText(UI.FONT().M, 128)
                {
                    MaxWidth = 800,
                    MultipleLines = true
                };

                protected override void load(SaveFile f)
                {
                    menu.start(new GameLoader(f.path)
                    {
                        public override void doAfterSet()
                        {
                            base.doAfterSet();
                            BattleState.setLoaded(new BattleStateExiter()
                            {
                                public override void exit(BATTLE_RESULT res, int plosses, int elosses)
                                {
                                    CORE.setCurrentState(new CORE_STATE.Constructor()
                                    {
                                        public override CORE_STATE getState()
                                        {
                                            return Menu.make();
                                        }
                                    });
                                }

                                public override void afterExit(BattleStateResult res)
                                {
                                }
                            }, f.path, true);
                        }
                    });
                }

                protected override void renderInfo(SPRITE_RENDERER r, SaveFile file, RECTANGLE body, double ds)
                {
                    t.clear().add(file.spec().population).s().add('V').add('s').s().add(file.spec().enemies);
                    t.adjustWidth();
                    t.renderCX(r, body.cX(), body.y1());

                    t.set(file.spec().desc);
                    t.renderCX(r, body.cX(), body.y1() + 32);
                }

                protected override void renderName(SPRITE_RENDERER r, SaveFile s, RECTANGLE body)
                {
                    UI.FONT().H2.render(r, s.name, body.x1() + 64, body.y1() + UI.FONT().M.height() / 2);
                }

                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };
            return new ScLoad(menu, screen, ¤¤battles);
        }
    }
}