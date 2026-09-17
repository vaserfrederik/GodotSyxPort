using System;
using System.Collections.Generic;
using Init.Constant;
using Init.Sprite.UI;
using Snake2D;
using Snake2D.Renderer;
using Snake2D.SpriteRenderer;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Misc;
using Util.Gui.Misc;
using View.Keyboard;

namespace View.Interrupter
{
    public class IPromtScreen : Interrupter
    {
        private Color c = Colors.White100;
        private ICharSequence message;
        private readonly RenderObj.RenderImp text = new RenderObj.RenderImp(600, 100)
        {
            Render = (r, ds) =>
            {
                c.Bind();
                UI.FONT().H2.RenderIn(r, Body, DIR.N, message);
                Color.Unbind();
            }
        };
        private GButt[] butts;
        private GButt hovered;
        private Action deactivateAction;
        private readonly InterManager m;

        public IPromtScreen(InterManager manager)
        {
            Pin();
            this.m = manager;
            Text.Body().CenterIn(C.DIM());
        }

        public void Activate(ICharSequence message, Color c, Action deactivateAction, params GButt[] butts)
        {
            base.Show(m);
            this.message = message;
            this.c = c;
            Text.Body().CenterIn(C.DIM());

            this.butts = butts;
            hovered = null;

            this.deactivateAction = deactivateAction;

            if (butts.Length == 0)
                return;

            int w = C.WIDTH() / 12;
            int y = Text.Body().Y2() + C.SCALE * 10;
            int x = C.WIDTH() / 2;
            x -= butts.Length * w / 2;

            for (int i = 0; i < butts.Length; i++)
            {
                butts[i].Body().MoveX1Y1(x - butts[i].Body().Width() / 2, y);
                x += 2 * w;
            }
        }

        public void Deactivate()
        {
            if (deactivateAction != null)
                deactivateAction();
            Hide();
        }

        protected override void HoverTimer(GBox text)
        {
        }

        protected override bool Render(Renderer r, float ds)
        {
            Text.Render(r, ds);
            if (butts.Length == 0)
                return false;
            foreach (var b in butts)
                b.Render(r, ds);
            return false;
        }

        protected override void MouseClick(MButt button)
        {
            if (butts.Length == 0)
            {
                Deactivate();
            }

            if (hovered != null && hovered.HoveredIs())
            {
                Deactivate();
                hovered.Click();
            }
        }

        protected override bool Hover(Coordinate mCoo, bool mouseHasMoved)
        {
            if (butts.Length == 0)
                return true;

            if (hovered != null && hovered.Hover(mCoo))
                return true;

            foreach (var b in butts)
                if (b.Hover(mCoo))
                    hovered = b;

            return true;
        }

        protected override bool Update(float ds)
        {
            if (KEYS.MAIN().ESCAPE.ConsumeClick() || KEYS.MAIN().ENTER.ConsumeClick())
                Deactivate();
            KEYS.Clear();
            return false;
        }
    }
}