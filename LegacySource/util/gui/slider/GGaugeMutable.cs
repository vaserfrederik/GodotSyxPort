using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.info;
using view.main;

namespace util.gui.slider
{
    public class GGaugeMutable : CLICKABLE.ClickableAbs
    {
        private static Font f = UI.FONT().M;
        private static GText text = new GText(UI.FONT().S, 100);
        private bool hideInfo;
        private static Rec rTmp = new Rec();

        private readonly DOUBLE_MUTABLE d;
        private bool clicked = false;
        private static readonly ColorImp col = new ColorImp();

        public GGaugeMutable(DOUBLE_MUTABLE d, int width)
        {
            if (f != UI.FONT().M)
                text = new GText(UI.FONT().S, 100);
            this.d = d;
            body.setDim(width, Icon.M);
            repetativeSet(true);
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            clicked &= MButt.LEFT.isDown();

            if (isHovered)
            {
                GText t = (VIEW.hoverBox()).text();
                t.setFont(UI.FONT().S);
                setInfo(d, t);
                if (t.length() > 0)
                {
                    VIEW.hoverBox().add(t);
                    VIEW.hoverBox().NL();
                }
            }

            rTmp.set(body());

            if (!hideInfo)
            {
                text.clear();
                int w = setInfo(d, text);
                if (w > 0)
                {
                    rTmp.moveX1(rTmp.x2() - w);
                    rTmp.setWidth(w);
                    renderBG(r, rTmp);
                    text.adjustWidth();
                    text.renderC(r, rTmp);
                    rTmp.moveX1(body().x1());
                    rTmp.setWidth(body().width() - w + 1);
                }
            }

            bool big = rTmp.width() > 100;
            if (isActive && big)
            {
                int w = rTmp.width();
                rTmp.set(rTmp.x1(), rTmp.x1() + 16, rTmp.y1(), rTmp.y2());
                renderBG(r, rTmp);
                SPRITES.icons().s.minifier.renderC(r, rTmp);
                rTmp.set(rTmp.x1() + w - 16, rTmp.x1() + w, rTmp.y1(), rTmp.y2());
                renderBG(r, rTmp);
                SPRITES.icons().s.magnifier.renderC(r, rTmp);
                rTmp.set(body());
                rTmp.setWidth(w - 32);
                rTmp.incrX(16);
            }

            if (clicked)
            {
                set(rTmp);
            }

            renderBG(r, rTmp);

            if (isActive && !big && leftHovered(rTmp))
            {
                renderColor(r, GCOLOR.UI().bgHov().hovered, rTmp.x1(), buttonX1(rTmp) + Icon.M / 2);
                SPRITES.icons().s.minus.render(r, rTmp.x1(), rTmp.y1() + 4);
            }
            else
            {
                bad2Good(col, d.getD());
                renderColor(r, col, rTmp.x1(), buttonX1(rTmp) + Icon.M / 2);
            }

            if (isActive && !big && rightHovered(rTmp))
            {
                renderColor(r, GCOLOR.UI().bgHov().hovered, buttonX1(rTmp) + Icon.M / 2, rTmp.x2() - 3);
                SPRITES.icons().s.plus.render(r, rTmp.x2() - Icon.S, rTmp.y1() + 4);
            }
            else
            {
            }

            if (!isActive)
                return;

            int bx1 = buttonX1(rTmp);
            SPRITES.icons().m.circle_frame.render(r, bx1, body().y1());

            bad2Good(col, d.getD());

            if (buttonIsHovered(rTmp))
            {
                col.shadeSelf(1.4);
            }

            col.bind();
            SPRITES.icons().m.circle_inner.render(r, bx1, body().y1());
            COLOR.unbind();
        }

        public static void bad2Good(ColorImp c, double d)
        {
            if (d < 0)
                d = 0;
            if (d > 1)
                d = 1;
            double r = (d > 0.5) ? (1.0 - (d - 0.5) * 2) : 1;
            double g = (d < 0.5) ? d * 2 : 1;
            c.set(30 + (int)(70 * r), 30 + (int)(70 * g), 30);
        }

        private void renderBG(SPRITE_RENDERER r, RECTANGLE rec)
        {
            GCOLOR.UI().border().render(r, rec, -1);
            GCOLOR.UI().bg().render(r, rec, -2);
        }

        private void renderColor(SPRITE_RENDERER r, COLOR c, int x1, int x2)
        {
            ColorImp.TMP.set(c);
            col.set(ColorImp.TMP);
            col.shadeSelf(0.5);
            col.render(r, x1 + 2, x2, body().y1() + 3, body().y2() - 3);
            col.set(ColorImp.TMP);
            col.render(r, x1 + 2, x2, body().y1() + 4, body().y2() - 4);
        }

        int adjustWidth(int width, DOUBLE d)
        {
            return (int)(Icon.M / 2 + (width - Icon.M) * d.getD());
        }

        protected void setColor(DOUBLE d, ColorImp imp, bool hovered)
        {
            bad2Good(imp, d.getD());
            if (hovered)
                imp.shadeSelf(1.4);
        }

        public override bool hover(COORDINATE mCoo)
        {
            return base.hover(mCoo);
        }

        private int buttonX1(RECTANGLE body)
        {
            int w = body.width() - Icon.M;
            return (int)(body.x1() + d.getD() * w);
        }

        private bool leftHovered(RECTANGLE body)
        {
            COORDINATE c = VIEW.mouse();
            if (c.isWithinRec(body))
            {
                return c.x() < buttonX1(body);
            }
            return false;
        }

        private bool rightHovered(RECTANGLE body)
        {
            COORDINATE c = VIEW.mouse();
            if (c.isWithinRec(body))
            {
                return c.x() > buttonX1(body) + Icon.M;
            }
            return false;
        }

        private bool buttonIsHovered(RECTANGLE body)
        {
            COORDINATE c = VIEW.mouse();
            if (c.isWithinRec(body))
            {
                int x1 = buttonX1(body);
                return c.x() > x1 && c.x() < x1 + Icon.M;
            }
            return false;
        }

        protected override final void clickA()
        {
            rTmp.set(body());
            if (!hideInfo)
            {
                rTmp.incrW(-setInfo(d, text));
            }
            if (rTmp.width() > 100)
            {
                rTmp.incrX(16);
                rTmp.incrW(-32);
            }

            if (leftHovered(rTmp) || VIEW.mouse().x() < rTmp.x1())
            {
                d.incD(-double.MinValue);

            }
            else if (rightHovered(rTmp) || VIEW.mouse().x() >= rTmp.x2())
            {
                d.incD(double.MinValue);

            }
            else
            {
                if (VIEW.mouse().isWithinRec(rTmp))
                {
                    clicked = true;

                    set(rTmp);
                }
            }
        }

        private void set(RECTANGLE body)
        {
            COORDINATE c = VIEW.mouse();
            double w = body.width() - Icon.M;
            double de = c.x() - body.x1() - Icon.M / 2;
            de = CLAMP.d(de / w, 0, 1);
            d.setD(de);
        }

        protected int setInfo(DOUBLE d, GText text)
        {
            GFORMAT.perc(text, d.getD());
            return text.getFont().height() * 3;
        }

        public GGaugeMutable hideInfo()
        {
            hideInfo = true;
            return this;
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            base.hoverInfoGet(text);
        }

        public RENDEROBJ hoverInfoSet(INFO info)
        {
            hoverInfoSet(info.desc);
            hoverTitleSet(info.name);
            return this;
        }
    }
}