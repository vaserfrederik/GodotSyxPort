using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;

namespace view.ui.top
{
    public class UIPanelTop : Interrupter
    {
        public static readonly int WIDTH = C.WIDTH();
        public static readonly int HEIGHT = Icon.M * 2 + 3;

        private readonly GuiSection section = new GuiSection();
        private readonly GuiSection time;
        private readonly GuiSection right = new GuiSection();
        private readonly GuiSection noti;

        private static readonly CharSequence ¤¤bView = "Toggle Battle Mode";

        static UIPanelTop()
        {
            D.ts(typeof(UIPanelTop));
        }

        public UIPanelTop(InterManager manager) : this(manager, false, false) { }

        public UIPanelTop(InterManager manager, bool battleview, bool battle)
        {
            Pin();
            section.Body().SetDim(WIDTH, HEIGHT);
            section.Body().MoveX2(C.WIDTH());
            section.Body().MoveY1(0);

            time = SPRITES.specials().BuildTimeThing(battleview);
            time.Body().CenterX(section.Body());
            time.Body().MoveY1(battleview ? 6 : 0);

            if (!battleview && !battle)
            {
                noti = new UINotifications();
            }
            else
            {
                noti = new GuiSection();
            }

            right.AddRightC(0, Sep());

            right.AddRightC(0, new Butt(SPRITES.icons().l.book, 18)
            {
                protected override void ClickA()
                {
                    VIEW.UI().wiki.Activate();
                }
            }.HoverInfoSet(WIKI.¤¤name));

            right.AddRightC(0, new Butt(SPRITES.icons().l.menu, 18)
            {
                protected override void ClickA()
                {
                    VIEW.inters().menu.Show();
                }
            }.HoverInfoSet(Dic.¤¤Menu));

            right.Body().CenterIn(section);
            right.Body().MoveX2(C.WIDTH() - 4);
            right.Body().MoveY1(1);

            section.Add(right);

            noti.Body().MoveX1Y1(C.DIM().width() / 2 + 100, HEIGHT);

            Show(manager);
        }

        public void Hide(bool yes)
        {
            section.VisableSet(yes);
        }

        protected override bool Render(Renderer r, float ds)
        {
            manager().viewPort().MoveY1(section.Body().Y2());
            if (manager().viewPort().Y2() > C.HEIGHT())
            {
                manager().viewPort().SetHeight(C.HEIGHT() - section.Body().Height());
            }

            GCOLOR.UI().PanBG.Render(r, section.Body());
            section.Render(r, ds);
            GCOLOR.UI().Border(r, 0, C.WIDTH(), section.Body().Y2() - 3, section.Body().Y2());
            time.Render(r, ds);
            noti.Render(r, ds);
            return true;
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return section.Hover(mCoo) | time.Hover(mCoo) | noti.Hover(mCoo) || mCoo.TouchesRec(section);
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                section.Click();
                time.Click();
                noti.Click();
            }
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
            time.HoverInfoGet(text);
            noti.HoverInfoGet(text);
        }

        protected override bool Update(float ds)
        {
            return true;
        }

        public static int Y2()
        {
            return 48 - 2;
        }

        public static class Butt : GButt.ButtPanel
        {
            public Butt(SPRITE sprite, int width = 40, int height = 48) : base(sprite)
            {
                SetDim(width, height);
            }
        }

        public static RENDEROBJ Sep()
        {
            return new RENDEROBJ()
            {
                protected override void Render(SPRITE_RENDERER r, float ds)
                {
                    OpacityImp opa = new OpacityImp(0);
                    COLOR normal = new ColorImp(47, 20, 0).ShadeSelf(1.2);
                    COLOR active = new ColorImp(127, 40, 20);

                    int di = 0;
                    bool blink = false;
                    bool nextBlink = false;

                    if (di >= Config.battle().DIVISIONS_PER_ARMY)
                    {
                        blink = nextBlink;
                        nextBlink = false;
                        di = 0;
                    }

                    if (GAME.ARMIES().player().divisions().Get(di).Men() > 0 && GAME.ARMIES().player().divisions().Get(di).Settings().Mustering())
                    {
                        blink = true;
                        nextBlink = true;
                    }

                    di++;

                    if (blink || (!IsHovered && !Selected && GAME.ARMIES().enemy().Men() > 0 || SETT.INVADOR().Invading()))
                    {
                        opa.Set(0.25 + VIEW.RenderSecond() % 0.75);
                        opa.Bind();
                        active.Render(r, Body, -3);
                        OPACITY.Unbind();
                    }
                }
            };
        }

        public static RENDEROBJ bToggle()
        {
            ACTION a = new ACTION()
            {
                public override void Exe()
                {
                    if (VIEW.s().IsActive())
                        VIEW.s().Battle.Activate();
                    else if (VIEW.s().Battle.IsActive())
                        VIEW.s().Activate();
                }
            };

            COLOR normal = new ColorImp(47, 20, 0).ShadeSelf(1.2);
            COLOR active = new ColorImp(127, 40, 20);

            Butt c = new Butt(SPRITES.icons().l.battle)
            {
                protected override void ClickA()
                {
                    a.Exe();
                }

                protected override void RenAction()
                {
                    SelectedSet(VIEW.s().Battle.IsActive());
                }
            }.Bg(normal).HoverInfoSet(Dic.¤¤Battle);

            c = KeyButt.Wrap(a, c, KEYS.MAIN(), "enablebattle", ¤¤bView, ¤¤bView, KEYCODES.KEY_LEFT_SHIFT, KEYCODES.KEY_B);

            return c;
        }

        public static RENDEROBJ wLog()
        {
            Butt b = new Butt(SPRITES.icons().m.factions)
            {
                int current = WORLD.LOG().All().Count;

                protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    if (VIEW.b().IsActive())
                        return;
                    isActive = WORLD.LOG().All().Count > current;

                    isSelected = VIEW.UI().log.IsActivated();
                    base.Render(r, ds, isActive, isSelected, isHovered);
                }

                protected override void ClickA()
                {
                    if (!VIEW.b().IsActive())
                    {
                        VIEW.UI().log.Activate();
                        current = WORLD.LOG().All().Count;
                    }
                }
            };
            b.SetDim(40, 48);
            b.HoverInfoSet(UILog.¤¤name);
            return b;
        }

        public static RENDEROBJ vToggle()
        {
            COLOR cw = new ColorImp(0, 47, 20);
            COLOR cc = new ColorImp(0, 47, 47);

            ACTION a = new ACTION()
            {
                public override void Exe()
                {
                    if (VIEW.s().IsActive())
                        VIEW.world().Activate();
                    else if (VIEW.world().IsActive())
                        VIEW.s().Activate();
                }
            };

            Butt b = new Butt(SPRITES.icons().l.city, 18)
            {
                protected override void ClickA()
                {
                    a.Exe();
                }

                protected override void RenAction()
                {
                    ReplaceLabel(VIEW.s().IsActive() ? SPRITES.icons().l.world : SPRITES.icons().l.city, DIR.C);
                    Bg(VIEW.s().IsActive() ? cc : cw);
                    SelectedSet(false);
                }
            };

            CLICKABLE c = KeyButt.Wrap(b, KEYS.MAIN().SWAP);
            return c;
        }

        public static RENDEROBJ Junk()
        {
            return VIEW.UI().manager.butt();
        }

        private void Dev(GuiSection s)
        {
            if (S.Get().developer)
            {
                s.AddRelBody(0, DIR.W, new Butt(SPRITES.icons().s.cog)
                {
                    protected override void ClickA()
                    {
                        VIEW.inters().debugpanel.Show();
                    }

                    protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        SelectedSet(VIEW.inters().debugpanel.IsActivated());
                        base.Render(r, ds, isActive, isSelected, isHovered);
                    }
                }.HoverInfoSet("developer-tools"));
            }
        }
    }
}