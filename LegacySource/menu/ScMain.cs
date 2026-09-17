using System;
using game;
using game.battle.state;
using game.save;
using init.constant;
using init.paths;
using init.settings;
using init.sprite.UI;
using menu;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.gui.misc;
using util.text;
using view.main;
using view.menu;
using world.battle.spec;

namespace menu
{
    class ScMain : SC
    {
        private readonly GuiSection first;
        private readonly GuiSection play;
        private readonly GuiSection load;
        private GuiSection current;
        private readonly RENDEROBJ.Sprite logo;
        private readonly Menu menu;
        private readonly GText version = new GText(UI.FONT().H2, VERSION.VERSION_STRING);

        public ScMain(Menu menu)
        {
            D.t(this);
            this.menu = menu;
            first = GetFirst(menu);
            play = GetPlay(menu);
            play.body().moveY1(first.body().y1());
            load = GetLoad(menu);
            load.body().moveY1(first.body().y1());

            logo = new RENDEROBJ.Sprite(menu.res.s().logo);
            logo.body().moveX2(left.x2());
            logo.body().centerY(left);
            logo.setColor(GUI.COLORS.menu);

            current = first;
        }

        private static readonly string ¤¤continue = "continue";
        private static readonly string ¤¤quit = "quit";
        private static readonly string ¤¤play = "play";
        private static readonly string ¤¤editor = "editor";
        private static readonly string ¤¤battle = "quick battle";
        private static readonly string ¤¤load = "load";
        private static readonly string ¤¤loadB = "debug battle";
        private static readonly string ¤¤tutorial = "tutorial";

        static ScMain()
        {
            D.ts(typeof(ScMain));
        }

        private GuiSection GetFirst(Menu menu)
        {
            GuiSection current = new GuiSection();
            CLICKABLE text;

            text = getNavButt(¤¤play);
            text.clickActionSet(new ACTION(() =>
            {
                SwitchNavigator(play);
            }));
            current.addDown(0, text);

            if (!menu.load.HasSaves())
            {
                text = new Button(UI.FONT().H1.getText(¤¤tutorial))
                {
                    protected override void clickA()
                    {
                        menu.switchScreen(menu.campaigns);
                    }
                };
                current.addDown(8, text);
            }
            else
            {
                text = new Button(UI.FONT().H1.getText(¤¤continue))
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        activeSet(menu.load.HasSaves());
                        base.render(r, ds, isActive, isSelected, isHovered);
                    }

                    protected override void clickA()
                    {
                        if (menu.load.HasSaves())
                            menu.load.LoadSave();
                    }
                };
                current.addDown(8, text);
            }

            text = getNavButt(ScOptions.¤¤name);
            text.clickActionSet(new ACTION(() =>
            {
                menu.switchScreen(menu.options);
            }));
            current.addDown(8, text);

            text = getNavButt(ScCredits.¤¤name);
            text.clickActionSet(new ACTION(() =>
            {
                menu.switchScreen(menu.credits);
            }));
            current.addDown(8, text);

            text = getNavButt(¤¤quit);
            text.clickActionSet(new ACTION(() =>
            {
                CORE.Annihilate();
            }));
            current.addDown(8, text);

            current.body().moveX1(right.x1());
            current.body().centerY(right.y1(), right.y2());

            return current;
        }

        private GuiSection GetLoad(Menu menu)
        {
            GuiSection current = new GuiSection();

            CLICKABLE text;

            text = getNavButt(MenuScreenLoad.¤¤name);
            text.clickActionSet(new ACTION(() =>
            {
                menu.switchScreen(menu.load);
            }));
            current.addDown(0, text);

            if (S.get().developer && PATHS.local().save().Exists(BattleState.debugLoad))
            {
                text = getNavButt(¤¤loadB);
                text.clickActionSet(new ACTION(() =>
                {
                    menu.Start(new GameLoader(PATHS.local().save().Get(BattleState.debugLoad))
                    {
                        protected override void DoAfterSet()
                        {
                            BattleState.SetLoaded(new BattleStateExiter()
                            {
                                public void AfterExit(BattleStateResult res)
                                {
                                }

                                public void Exit(BATTLE_RESULT res, int plosses, int elosses)
                                {
                                    CORE.SetCurrentState(new CORE_STATE.Constructor()
                                    {
                                        public CORE_STATE GetState()
                                        {
                                            return Menu.Make();
                                        }
                                    });
                                }
                            }, saveFile, true);
                        }
                    });
                }));
                current.addDown(8, text);
            }

            foreach (ScLoad l in menu.loads)
            {
                text = getNavButt(l.name);
                text.clickActionSet(new ACTION(() =>
                {
                    menu.switchScreen(l);
                }));
                if (!l.HasSaves())
                    text.activeSet(false);
                current.addDown(8, text);
            }

            text = getBackArrow();
            text.clickActionSet(new ACTION(() =>
            {
                SwitchNavigator(play);
            }));
            current.addDown(10, text);

            current.body().moveX1(right.x1());
            current.body().centerY(right);

            return current;
        }

        private GuiSection GetPlay(Menu menu)
        {
            GuiSection current = new GuiSection();

            CLICKABLE text;

            text = getNavButt(¤¤load);
            text.clickActionSet(new ACTION(() =>
            {
                SwitchNavigator(load);
            }));
            current.addDown(0, text);

            text = getNavButt(ScCampaign.¤¤name);
            text.clickActionSet(new ACTION(() =>
            {
                menu.switchScreen(menu.campaigns);
            }));
            current.addDown(8, text);

            text = getNavButt(ScRandom.¤¤name);
            text.clickActionSet(new ACTION(() =>
            {
                menu.switchScreen(menu.sandbox2);
            }));
            current.addDown(8, text);

            text = getNavButt(¤¤battle);
            text.clickActionSet(new ACTION(() =>
            {
                menu.Start(new Constructor()
                {
                    public CORE_STATE GetState()
                    {
                        CORE_STATE s = GAME.create();

                        VIEW.b().editor.activate();

                        return s;
                    }
                });
            }));
            current.addDown(8, text);

            text = getNavButt(¤¤editor);
            text.clickActionSet(new ACTION(() =>
            {
                menu.Start(new Constructor()
                {
                    public CORE_STATE GetState()
                    {
                        CORE_STATE s = GAME.create();

                        VIEW.world().editor.activate();

                        return s;
                    }
                });
            }));
            current.addDown(8, text);

            text = getBackArrow();
            text.clickActionSet(new ACTION(() =>
            {
                SwitchNavigator(first);
            }));
            current.addDown(10, text);

            current.body().moveX1(right.x1());
            current.body().centerY(right);

            return current;
        }

        private void SwitchNavigator(GuiSection section)
        {
            current = section;
            current.hover(menu.GetMCoo());
        }

        public void render(SPRITE_RENDERER r, float ds)
        {
            logo.render(r, ds);
            current.render(r, ds);
            version.render(r, C.DIM().x2() - 32 - version.width(), 32);
        }

        public bool hover(COORDINATE mCoo)
        {
            return current.hover(mCoo);
        }

        public bool click()
        {
            return current.click();
        }

        public bool back(Menu menu)
        {
            if (current == load)
            {
                SwitchNavigator(play);
                return true;
            }
            if (current != first)
            {
                SwitchNavigator(first);
                return true;
            }
            return false;
        }
    }
}