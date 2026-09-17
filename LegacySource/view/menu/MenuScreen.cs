using System;
using System.Text;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Clickable;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sprite;
using Snake2D.Util.Sprite.Text;
using Util.Colors;
using Util.Text;

namespace View.Menu
{
    public abstract class MenuScreen : GuiSection
    {
        private static readonly CharSequence ¤¤back = "¤< back";

        static MenuScreen()
        {
            D.Ts(typeof(MenuScreen));
        }

        public static readonly Rectangle bounds = new Rectangle(1200, 600);
        public static readonly Rectangle inner = new Rectangle(bounds.Width - 50, bounds.Height - 32).MoveC(C.DIM().cX(), C.DIM().cY());

        private readonly GuiSection bottombutts = new GuiSection();

        public MenuScreen(CharSequence title, COLOR color)
        {
            Body().Set(bounds);
            Body().CenterIn(C.DIM());

            RenderObj s = UI.Decor().Frame(Body(), color);
            s.Body().CenterIn(Body());
            Add(s);

            s = UI.Decor().Decorate(title, color);
            s.Body().CenterIn(C.DIM());
            s.Body().MoveY2(GetLastY1());
            Add(s);

            ScreenButton b = new ScreenButton(UI.FONT().H1.GetText(¤¤back))
            {
                protected override void ClickA()
                {
                    this.back();
                }
            };
            b.Body().MoveX2(Body().X2() - 20);
            b.Body().MoveY1(Body().Y1());
            Add(b);
            AddRelBody(14, DIR.S, bottombutts);
        }

        public void AddButt(RenderObj obj)
        {
            bottombutts.AddRightC(24, obj);
            bottombutts.Body().CenterX(this);
        }

        protected abstract void back();

        public class ScreenButton : Clickable.ClickableAbs
        {
            private readonly SPRITE s;

            public ScreenButton(CharSequence name) : this((SPRITE)UI.FONT().H1.GetText(name))
            {
            }

            public ScreenButton(CharSequence name, Font f) : this((SPRITE)f.GetText(name))
            {
            }

            public ScreenButton(SPRITE s)
            {
                this.s = s;
                Body.SetWidth(s.Width).SetHeight(s.Height);
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                if (!isActive)
                    GCOLOR.T().INACTIVE.Bind();
                else if (isHovered && isSelected)
                    GCOLOR.T().HOVER_SELECTED.Bind();
                else if (isHovered)
                    GCOLOR.T().HOVERED.Bind();
                else if (isSelected)
                    GCOLOR.T().SELECTED.Bind();
                else
                    GCOLOR.T().CLICKABLE.Bind();
                s.Render(r, Body);
                COLOR.Unbind();
            }
        }
    }
}