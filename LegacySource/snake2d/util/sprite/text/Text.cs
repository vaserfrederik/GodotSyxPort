using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sprite;

namespace snake2d.util.sprite.text
{
    public class Text : Str, SPRITE
    {
        protected int maxWidth = 2000;
        private int width;
        private int height;
        private Font font;
        private double scale = 1;
        private bool multipleLines = true;
        private bool darken = false;

        public Text(Font font, CharSequence text)
            : base(text.Length())
        {
            this.font = font;
            Set(text);
        }

        public Text(Font font, int size)
            : base(size)
        {
            this.font = font;
            AdjustWidth();
        }

        public Text(Font font, int size, int width)
            : base(size)
        {
            this.font = font;
            AdjustWidth();
        }

        public override Text Clear()
        {
            base.Clear();
            AdjustWidth();
            return this;
        }

        public override Text Add(CharSequence str)
        {
            base.Add(str);
            return this;
        }

        public Text DarkBG()
        {
            darken = true;
            return this;
        }

        public Text AdjustWidth()
        {
            COORDINATE c = font.GetDim(this, int.MaxValue, scale);
            width = c.X();
            height = c.Y();
            if (height == 0)
                height = font.Height(scale);

            if (!multipleLines)
            {
                height = font.Height(scale);
                if (width > maxWidth)
                {
                    width = maxWidth;
                }
            }
            else if (width > maxWidth)
            {
                width = maxWidth;
                height = font.GetHeight(this, width);
                width = font.GetDim(this, width + 2, scale).X();
            }

            return this;
        }

        public int MaxWidth()
        {
            return maxWidth;
        }

        public override Text AddBinary(int i)
        {
            base.AddBinary(i);
            return this;
        }

        public override Text AddBinary(long i)
        {
            base.AddBinary(i);
            return this;
        }

        public override Text Add(long i, bool format)
        {
            base.Add(i, format);
            return this;
        }

        public override Text Add(long i)
        {
            base.Add(i);
            return this;
        }

        public override Text Add(double d)
        {
            base.Add(d);
            return this;
        }

        public override Text Add(char chars)
        {
            base.Add(chars);
            return this;
        }

        public Text Para(CharSequence str)
        {
            if (Length() > 0)
                S();
            Add('(');
            Add(str);
            Add(')');
            return this;
        }

        public override Text S()
        {
            base.S();
            return this;
        }

        public override Text S(int i)
        {
            base.S(i);
            return this;
        }

        public override Text Add(bool b)
        {
            base.Add(b);
            return this;
        }

        public Text SetFont(Font font)
        {
            this.font = font;
            this.height = font.Height();
            AdjustWidth();
            return this;
        }

        public Font GetFont()
        {
            return font;
        }

        public Text Set(CharSequence s)
        {
            Clear();
            Add(s);
            return AdjustWidth();
        }

        public int GetHeight(int width)
        {
            return font.GetHeight(this, width);
        }

        public Text SetScale(double scale)
        {
            this.scale = scale;
            AdjustWidth();
            return this;
        }

        public Text SetMaxWidth(int max)
        {
            this.maxWidth = max;
            if (width > maxWidth)
                AdjustWidth();
            return this;
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            int width = X2 - X1;
            if (width <= 0)
                width = maxWidth;

            if (darken)
            {
                OPACITY old = CORE.Renderer().Opacity();
                OPACITY.O50.Bind();
                COLOR.BLACK.Render(r, X1 - 2, X1 + this.width + 2, Y1 - 2, Y1 + height + 2);
                old.Bind();
            }

            if (multipleLines)
                font.Render(r, this, X1, Y1, width, scale);
            else
            {
                font.RenderCropped(r, this, X1, Y1, width, scale);
            }
        }

        public Text SetMultipleLines(bool m)
        {
            this.multipleLines = m;
            AdjustWidth();
            return this;
        }

        public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
        {
            int width = X2 - X1;
            if (width <= 0)
                width = maxWidth;
            //font.RenderTextured(this, X1, Y1, width, scale, texture);
        }

        public override bool Equals(object other)
        {
            if (other is CharSequence)
            {
                CharSequence o = (CharSequence)other;
                return o.ToString().EqualsIgnoreCase(this.ToString());
            }
            return false;
        }

        public override Text ToCamel()
        {
            base.ToCamel();
            AdjustWidth();
            return this;
        }

        public override Text ToLower()
        {
            base.ToLower();
            AdjustWidth();
            return this;
        }

        public override Text ToUpper()
        {
            base.ToUpper();
            AdjustWidth();
            return this;
        }
    }
}