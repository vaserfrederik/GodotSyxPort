using System;
using game;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util.colors;
using util.gui.common;
using util.gui.misc;
using view.interrupter;
using view.keyboard;
using view.main;
using view.subview;
using view.ui.top;
using world;

namespace view.world.panel
{
    final class UIMinimap : Interrupter
    {
        private readonly UIMinimapW map;
        private bool expanded = true;
        private readonly GuiSection buttons = new GuiSection();
        private readonly GameWindow window;

        public UIMinimap(UIPanelTop top, InterManager m, GameWindow window, string saveKey)
        {
            this.window = window;
            Pin();
            map = new UIMinimapW(window);

            CLICKABLE b;

            if (top != null)
            {
                b = new WorldHeatmaps();
                buttons.AddRight(0, b);
            }

            b = new GButt.ButtPanel(SPRITES.icons().s.camera)
            {
                protected override void ClickA()
                {
                    CORE.GetGraphics().MakeScreenShot();
                }
            };
            b = KeyButt.Wrap(b, KEYS.MAIN().SCREENSHOT);
            buttons.AddRight(0, b);

            b = new GButt.ButtPanel(SPRITES.icons().s.cameraBig)
            {
                private readonly SuperSc sst = new SuperSc("SUPER_WORLD", new SUPER_SCREENSHOT[] { new Shot(2, 2), new Shot(1, 2), new Shot(1, 1) }, saveKey);

                protected override void ClickA()
                {
                    VIEW.Inters().popup.Show(sst, this, true);
                }
            };
            b.HoverInfoSet(SuperSc.¤¤name);
            buttons.AddRightC(8, b);

            b = new GButt.ButtPanel(SPRITES.icons().s.magnifier)
            {
                protected override void ClickA()
                {
                    if (window.Zoomout() > 0)
                        window.SetZoomout(window.Zoomout() - 1);
                }

                protected override void RenAction()
                {
                    ActiveSet(window.Zoomout() > 0);
                }
            };
            b = KeyButt.Wrap(b, KEYS.MAIN().ZOOM_IN);
            buttons.AddRightC(10, b);

            b = new GButt.ButtPanel(SPRITES.icons().s.minifier)
            {
                protected override void ClickA()
                {
                    if (window.Zoomout() < 3)
                        window.SetZoomout(window.Zoomout() + 1);
                }

                protected override void RenAction()
                {
                    ActiveSet(window.Zoomout() < 2);
                }
            };
            b = KeyButt.Wrap(b, KEYS.MAIN().ZOOM_OUT);
            buttons.AddRightC(0, b);

            b = new GButt.ButtPanel(SPRITES.icons().s.arrowUp)
            {
                protected override void ClickA()
                {
                    expanded = !expanded;
                }

                protected override void RenAction()
                {
                    SelectedSet(expanded);
                }
            };
            buttons.AddRightC(0, b);

            RENDEROBJ pan = new RENDEROBJ.RenderImp(map.Body().Width(), 32)
            {
                public override void Render(SPRITE_RENDERER r, float ds)
                {
                    GCOLOR.UI().panBG.Render(r, Body);
                    GCOLOR.UI().Border(r, Body.X1(), Body.X1() + 3, Body.Y1(), Body.Y2());
                    GCOLOR.UI().Border(r, Body.X1(), Body.X2(), Body.Y2(), Body.Y2() + 3);
                }
            };

            buttons.Body().MoveX2(C.WIDTH() - 4);
            buttons.Body().MoveY1(0);
            pan.Body().MoveX2(C.WIDTH());
            pan.Body().CenterY(buttons);
            buttons.Add(pan);
            buttons.MoveLastToBack();
            buttons.Body().MoveY1(top == null ? 0 : UIPanelTop.HEIGHT);

            map.Body().MoveY1(buttons.Body().Y2());
            map.Body().MoveX2(C.DIM().Width());
            Show(m);
        }

        public void Render(SPRITE_RENDERER r, GameWindow window)
        {
            buttons.Render(r, 0);

            if (!expanded)
                return;

            map.Render(r, 0);
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return buttons.Hover(mCoo) | map.Hover(mCoo);
        }

        protected override void MouseClick(MButt button)
        {
            if (button != MButt.LEFT)
                return;
            if (buttons.Click())
                return;
            else if (expanded)
                map.Click();
        }

        protected override bool Render(Renderer r, float ds)
        {
            Render(r, window);
            return true;
        }

        protected override bool Update(float ds)
        {
            return true;
        }

        protected override void HoverTimer(GBox text)
        {
            buttons.HoverInfoGet(text);
        }

        private static class Shot : SUPER_SCREENSHOT
        {
            private readonly int zoomout;
            private readonly int winW;
            private readonly int winH;
            private Rec current;

            public Shot(int scale, int zoomout) : base(scale)
            {
                this.zoomout = zoomout;
                winW = (C.WIDTH()) << zoomout;
                winH = (C.HEIGHT()) << zoomout;
                current = new Rec(winW, winH);
            }

            public override bool RenderAndHasNext()
            {
                if (current.Y1() >= WORLD.PHEIGHT())
                    return false;

                WORLD.OVERLAY().Hide();
                bool t = WORLD.FOW().Toggled.Is();
                WORLD.FOW().Toggled.Set(false);
                GAME.World().Render(CORE.Renderer(), 0, zoomout, current, 0, 0);
                current.IncrX(winW);
                if (current.X1() >= WORLD.PWIDTH())
                {
                    current.IncrY(winH);
                    current.MoveX1(0);
                }
                WORLD.FOW().Toggled.Set(t);
                return true;
            }

            public override int GetWidth()
            {
                return WORLD.PWIDTH() >> zoomout;
            }

            public override int GetHeight()
            {
                return WORLD.PHEIGHT() >> zoomout;
            }

            public override void Init()
            {
                current.Set(0, winW, 0, winH);
            }
        }
    }
}