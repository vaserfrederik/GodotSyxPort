using System;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sprite.text;
using util.colors;

namespace util.gui.misc
{
    public class GText : Text
    {
        private ColorImp color = new ColorImp(GCOLOR.T().NORMAL);

        public GText(Font f, string text) : base(f, text)
        {
        }

        public GText(Font f, int length) : base(f, length)
        {
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            color.Bind();
            base.Render(r, X1, X1 + maxWidth, Y1, Y2);
            COLOR.Unbind();
        }

        public COLOR Color()
        {
            return color;
        }

        public override GText Clear()
        {
            base.Clear();
            return this;
        }

        public GText Color(COLOR c)
        {
            this.color.Set(c);
            return this;
        }

        public GText Lablify()
        {
            this.color.Set(GCOLOR.T().H1);
            return this;
        }

        public GText LablifySub()
        {
            this.color.Set(GCOLOR.T().H2);
            return this;
        }

        public GText Normalify()
        {
            this.color.Set(GCOLOR.T().NORMAL);
            return this;
        }

        public GText Normalify2()
        {
            this.color.Set(GCOLOR.T().NORMAL2);
            return this;
        }

        public GText Selectify()
        {
            this.color.Set(GCOLOR.T().HOVER_SELECTED);
            return this;
        }

        public GText Hoverify()
        {
            this.color.Set(GCOLOR.T().HOVERED);
            return this;
        }

        public GText Clickify()
        {
            this.color.Set(GCOLOR.T().CLICKABLE);
            return this;
        }

        public GText Errorify()
        {
            this.color.Set(GCOLOR.T().ERROR);
            return this;
        }

        public GText Warnify()
        {
            this.color.Set(GCOLOR.T().WARNING);
            return this;
        }

        public GText Decrease()
        {
            SetFont(UI.FONT().S);
            return this;
        }

        public GText Increase()
        {
            SetFont(UI.FONT().M);
            return this;
        }

        public override GText SetMaxWidth(int max)
        {
            base.SetMaxWidth(max);
            return this;
        }

        public GTextR R(DIR alignment)
        {
            return new GTextR(this, alignment);
        }

        public override GText ToCamel()
        {
            base.ToCamel();
            return this;
        }

        public override GText ToLower()
        {
            base.ToLower();
            return this;
        }

        public override GText ToUpper()
        {
            base.ToUpper();
            this.AdjustWidth();
            return this;
        }
    }
}