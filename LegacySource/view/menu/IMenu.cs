using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game;
using init.constant;
using init.sprite.UI;
using menu;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.keyboard;
using view.main;

namespace view.menu
{
    public class IMenu : Interrupter
    {
        private readonly GuiSection main;
        private readonly GuiSection options;
        private readonly ScKeys keys;
        private GuiSection current;
        private readonly Font big;
        private readonly Font small;
        private readonly InterManager manager;

        private static readonly CharSequence ¤¤resume = "resume";
        private static readonly CharSequence ¤¤quicksave = "quick-save";
        private static readonly CharSequence ¤¤saveFirst = "save first?";
        private static readonly CharSequence ¤¤options = "options";
        private static readonly CharSequence ¤¤quitMenu = "quit to menu";
        private static readonly CharSequence ¤¤exit = "exit";
        private static readonly CharSequence ¤¤THEMENU = "THE MENU";

        static IMenu()
        {
            D.ts(typeof(IMenu));
        }

        public IMenu(InterManager manager) : base()
        {
            this.manager = manager;
            Pin().DesturberSet();
            big = UI.FONT().H1;
            small = UI.FONT().H2;
            keys = new ScKeys(this, big, small);
            main = new GuiSection
            {
                Click = () => base.Click()
            };

            GButt tx = new GButt.Glow((SPRITE)big.GetText(¤¤resume))
            {
                ClickAction = () => Hide()
            };
            main.AddDownC(10, tx);

            tx = new GButt.Glow(big.GetText(¤¤quicksave))
            {
                RenAction = () => ActiveSet(VIEW.CanSave()),
                ClickA = () =>
                {
                    GAME.Saver().Quicksave();
                    Hide();
                }
            };
            main.AddDownC(6, tx);

            tx = new GButt.Glow(big.GetText(Dic.¤¤save))
            {
                RenAction = () => ActiveSet(VIEW.CanSave()),
                ClickA = () => current = new IMenuSave(this, big, small, null)
            };
            main.AddDownC(6, tx);

            tx = new GButt.Glow(big.GetText(Dic.¤¤load))
            {
                ClickAction = () =>
                {
                    IMenuLoad i = new IMenuLoad(this);
                    i.Init();
                    current = i;
                }
            };
            main.AddDownC(6, tx);

            tx = new GButt.Glow(big.GetText(¤¤options))
            {
                ClickAction = () => current = options
            };
            main.AddDownC(6, tx);

            tx = new GButt.Glow(big.GetText(keys.¤¤nameBig))
            {
                ClickAction = () => current = keys.Activate()
            };
            main.AddDownC(6, tx);

            ACTION exit2Menu = () => Exit2Menu();
            ACTION exit = () => CORE.Annihilate();

            tx = new GButt.Glow(big.GetText(¤¤quitMenu))
            {
                ClickAction = () =>
                {
                    if (GAME.Saver().GetTimeSinceLastSave() < 5 || !VIEW.CanSave())
                    {
                        Exit2Menu();
                    }

                    GButt yes = new GButt.Glow(big.GetText(Dic.¤¤Yes))
                    {
                        ClickAction = () => current = new IMenuSave(this, big, small, exit2Menu)
                    };
                    GButt no = new GButt.Glow(big.GetText(Dic.¤¤No))
                    {
                        ClickAction = exit2Menu
                    };

                    VIEW.Inters().FullScreen.Activate(¤¤saveFirst, COLOR.WHITE100, null, yes, no);
                }
            };
            main.AddDownC(6, tx);

            tx = new GButt.Glow(big.GetText(¤¤exit))
            {
                ClickAction = () =>
                {
                    if (!VIEW.CanSave())
                    {
                        exit.Exe();
                        return;
                    }

                    if (GAME.Saver().GetTimeSinceLastSave() < 5)
                    {
                        CORE.Annihilate();
                    }

                    GButt yes = new GButt.Glow(big.GetText(Dic.¤¤Yes))
                    {
                        ClickAction = () => current = new IMenuSave(this, big, small, exit)
                    };
                    GButt no = new GButt.Glow(big.GetText(Dic.¤¤No))
                    {
                        ClickAction = exit
                    };

                    VIEW.Inters().FullScreen.Activate(¤¤saveFirst, COLOR.WHITE100, null, yes, no);
                }
            };
            main.AddDownC(6, tx);

            main.Body().CenterIn(C.DIM());

            main.Add(UI.Decor().Frame(main.Body()));
            main.MoveLastToBack();

            {
                RENDEROBJ o = UI.Decor().GetDecored(¤¤THEMENU);
                o.Body().CenterX(main);
                o.Body().MoveY2(main.Body().Y1());
                main.Add(o);
            }

            current = main;

            options = new IMenuOptions(this, big, small);
        }

        void Exit2Menu()
        {
            CORE.SetCurrentState(new CORE_STATE.Constructor
            {
                GetState = () => Menu.Make()
            });
        }

        public void Show()
        {
            base.Show(manager);
            SetMain();
        }

        void SetMain()
        {
            current = main;
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            current.Hover(mCoo);
            return true;
        }

        protected override void HoverTimer(GBox text)
        {
        }

        protected override bool Render(Renderer r, float ds)
        {
            current.Render(r, ds);
            return false;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                current.Click();
            else if (button == MButt.RIGHT)
            {
                if (current != main)
                    current = main;
                else
                    Hide();
            }
        }

        protected override bool Update(float ds)
        {
            if (KEYS.MAIN().ESCAPE.ConsumeClick())
                Hide();
            return false;
        }
    }
}