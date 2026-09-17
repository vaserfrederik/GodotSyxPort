using System;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.common;

namespace util.gui.misc
{
    public abstract class GStat : TITLEABLE
    {
        protected readonly GText statText;
        private bool bg = false;

        public GStat() : this(64)
        {
        }

        public GStat(Font f) : this(new GText(f, 64))
        {
        }

        public GStat(int size) : this(new GText(UI.FONT().S, size))
        {
        }

        public GStat Increase()
        {
            statText.setFont(UI.FONT().M);
            return this;
        }

        public GStat Decrease()
        {
            statText.setFont(UI.FONT().S);
            return this;
        }

        public GStat(GText text)
        {
            this.statText = text;
        }

        public override int width()
        {
            return statText.width();
        }

        public override int height()
        {
            return statText.height();
        }

        public abstract void update(GText text);

        public GStat Bg()
        {
            this.bg = true;
            return this;
        }

        public void Adjust()
        {
            statText.Clear();
            update(statText);
            statText.AdjustWidth();
        }

        public GStat SetFont(Font f)
        {
            statText.setFont(f);
            return this;
        }

        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            Adjust();
            if (bg)
            {
                OPACITY.O50.bind();
                COLOR.BLACK.render(r, X1 - 1, X2 + 1, Y1 - 1, Y2 + 1);
                OPACITY.unbind();
            }
            statText.render(r, X1, X1 + statText.width(), Y1, Y2);
        }

        public override void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
        {
            throw new Exception();
        }

        public final HOVERABLE R(DIR alignment)
        {
            HOV h = new HOV();
            h.setSprite(this);
            h.setAlign(alignment);
            return h;
        }

        public final HOVERABLE R()
        {
            return R(DIR.NW);
        }

        private class HOV : HOVERABLE.Sprite
        {
            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                ((GStat)sprite).Adjust();
                base.render(r, ds, isHovered);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                ((GStat)sprite).hoverInfoGet((GBox)text);
                base.hoverInfoGet(text);
            }
        }
    }
}