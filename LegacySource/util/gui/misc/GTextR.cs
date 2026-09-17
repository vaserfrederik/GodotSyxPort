using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;

namespace util.gui.misc
{
    public class GTextR : HOVERABLE.Sprite
    {
        private readonly GText text;

        public GTextR(GText text) : this(text, DIR.C)
        {
        }

        public GTextR(Font f, CharSequence text) : this(new GText(f, text))
        {
        }

        public GTextR(Font f, int width) : this(new GText(f, width))
        {
        }

        public GTextR(Font f, int width, DIR replacementStrat) : this(new GText(f, width), replacementStrat)
        {
        }

        public GTextR(GText text, DIR replacementStrat) : base(text)
        {
            SetAlign(replacementStrat);
            this.text = text;
            text.AdjustWidth();
        }

        public GText Text()
        {
            return text;
        }

        public override void Adjust()
        {
            base.Adjust();
        }

        public override GTextR SetAlign(DIR d)
        {
            base.SetAlign(d);
            return this;
        }
    }
}