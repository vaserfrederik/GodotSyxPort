using System;
using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.common;
using util.info;

namespace util.gui.misc
{
    public class GHeader : HoverableAbs
    {
        protected SPRITE text;

        public GHeader(CharSequence name)
        {
            this.text = new GText(UI.FONT().H2, name).lablify();
            body.setHeight(text.height());
            body.setWidth(text.width());
        }

        public GHeader(CharSequence name, Font f)
        {
            this.text = new GText(f, name).lablify();
            body.setHeight(text.height());
            body.setWidth(text.width());
        }

        public GHeader(CharSequence name, int max)
        {
            if (name.Length > max)
                name = name.Substring(0, max - 1) + ".";
            this.text = new GText(UI.FONT().H2, name).lablify();
            body.setHeight(text.height());
            body.setWidth(text.width());
        }

        public GHeader(INFO info)
        {
            this.text = new GText(UI.FONT().H2, info.name).lablify();
            body.setHeight(text.height());
            body.setWidth(text.width());
            hoverTitleSet(info.name);
            hoverInfoSet(info.desc);
        }

        public GHeader(SPRITE name)
        {
            this.text = name;
            body.setHeight(text.height());
            body.setWidth(text.width());
        }

        public GHeader Subify()
        {
            ((GText)text).lablifySub();
            return this;
        }

        public void SetSprite(SPRITE name)
        {
            this.text = name;
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            if (text is GStat)
                ((GStat)text).adjust();
            text.render(r, body.x1(), body.y1());
        }

        public HOVERABLE HoverInfoSet(INFO i)
        {
            hoverTitleSet(i.name);
            hoverInfoSet(i.desc);
            return this;
        }

        public class HeaderVertical : GHeader
        {
            private readonly SPRITE s;

            public HeaderVertical(CharSequence name, SPRITE s) : base(name, s.height() <= 16 ? UI.FONT().S : UI.FONT().H2)
            {
                this.s = s;
                body.setHeight(text.height() + C.SG + s.height());
                body.setWidth(text.width() > s.width() ? text.width() : s.width());
            }

            public HeaderVertical(SPRITE name, SPRITE s) : base(name)
            {
                this.s = s;
                body.setHeight(text.height() + C.SG + s.height());
                body.setWidth(text.width() > s.width() ? text.width() : s.width());
            }

            public HeaderVertical(CharSequence name, GStat s) : this(name, (SPRITE)s) { }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                if (s is GStat)
                    ((GStat)s).adjust();
                int cx = body.cX();
                body.setWidth(text.width() > s.width() ? text.width() : s.width());
                body.moveCX(cx);
                int dx = (body.width() - text.width()) / 2;
                text.render(r, body.x1() + dx, body.y1());

                dx = (body.width() - s.width()) / 2;
                s.render(r, body.x1() + dx, body.y1() + text.height() + C.SG);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (s is TITLEABLE)
                    ((TITLEABLE)s).hoverInfoGet((GBox)text);
                base.hoverInfoGet(text);
            }
        }

        public class HeaderHorizontal : GHeader
        {
            private readonly SPRITE s;
            private readonly int fixedWidth;

            public HeaderHorizontal(CharSequence name, SPRITE s) : base(name, s.height() <= 16 ? UI.FONT().S : UI.FONT().H2)
            {
                this.s = s;
                body.setHeight(text.height() > s.height() ? text.height() : s.height());
                body.setWidth(text.width() + C.SG * 6 + s.width());
                fixedWidth = -1;
            }

            public HeaderHorizontal(SPRITE name, SPRITE s) : base(name)
            {
                this.s = s;
                body.setHeight(text.height() > s.height() ? text.height() : s.height());
                body.setWidth(text.width() + C.SG * 6 + s.width());
                fixedWidth = -1;
            }

            public HeaderHorizontal(CharSequence name, SPRITE s, int width) : base(name, s.height() <= 16 ? UI.FONT().S : UI.FONT().H2)
            {
                this.s = s;
                body.setHeight(text.height() > s.height() ? text.height() : s.height());
                body.setWidth(width + s.width());
                fixedWidth = width;
            }

            public HeaderHorizontal(SPRITE name, SPRITE s, int width) : base(name)
            {
                this.s = s;
                body.setHeight(text.height() > s.height() ? text.height() : s.height());
                body.setWidth(width + 32);
                fixedWidth = width;
            }

            public HeaderHorizontal(CharSequence name, GStat s) : this(s.statText.getFont() == UI.FONT().M ? (SPRITE)new GText(UI.FONT().H2, name).lablify() : (SPRITE)new GText(UI.FONT().H2, name).lablify(), s) { }

            public HeaderHorizontal(CharSequence name, GStat s, int width) : this(s.statText.getFont() == UI.FONT().M ? (SPRITE)new GText(UI.FONT().H2, name).lablify() : (SPRITE)new GText(UI.FONT().H2, name).lablify(), s, width) { }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                if (fixedWidth == -1)
                    body.setWidth(text.width() + C.SG * 6 + s.width());

                int dy = (body.height() - text.height()) / 2;
                text.render(r, body.x1(), body.y1() + dy);
                dy = (body.height() - s.height()) / 2;
                int x1 = body().x1();
                if (fixedWidth == -1)
                {
                    body.setWidth(C.SG * 6 + text.width() + s.width());
                    x1 += C.SG * 6 + text.width();
                }
                else
                {
                    body.setWidth(fixedWidth + s.width());
                    x1 += fixedWidth;
                }

                s.render(r, x1, body.y1() + dy);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (s is TITLEABLE)
                    ((TITLEABLE)s).hoverInfoGet((GBox)text);
                base.hoverInfoGet(text);
            }

            public HeaderHorizontal IncreaseWidth(int am)
            {
                body.incrW(am);
                return this;
            }
        }
    }
}