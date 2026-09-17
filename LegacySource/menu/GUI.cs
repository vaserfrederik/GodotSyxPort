using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data.INT;
using util.gui.misc;
using util.text;
using view.menu;

namespace menu
{
    abstract class GUI
    {
        static RECTANGLE bounds;
        static RECTANGLE right;
        static RECTANGLE left;
        static int bottomMarginX;
        static int bottomY;
        static int margin;

        static COLOR labelColor = new ColorImp(127, 127, 80);

        static RECTANGLE inner;
        static CharSequence ¤¤back = "¤< back";

        static
        {
            D.ts(typeof(GUI));
        }

        static void init(RECTANGLE bounds)
        {
            GUI.bounds = bounds;
            D.t(typeof(GUI));
            float width = bounds.width() / 4;
            float height = bounds.height() / 1.4f;
            float dist = bounds.width() / 16;
            bottomMarginX = bounds.width() / 6;

            Rec l = new Rec();
            l.setWidth(width);
            l.setHeight(height);
            l.moveX1(bounds.x1() + width - dist / 2);
            l.moveY1(bounds.y1() + (bounds.height() - height) / 2);
            left = l;

            Rec r = new Rec(l);
            r.moveX1(l.x2() + dist);
            r.moveY1(bounds.y1() + (bounds.height() - height) / 2);
            right = r;

            margin = getSmallText("aaaaaaaaaaaaaaaaa").width();

            r = new Rec(bounds);
            r.incrW(-200);
            r.incrH(-100);
            r.centerIn(bounds);

            bottomY = bounds.y2() + 30;

            inner = r;
        }

        static class COLORS
        {
            static COLOR normal = COLOR.WHITE100;
            static COLOR hover = new ColorShifting(new ColorImp(127, 127, 65), new ColorImp(110, 90, 45));
            static COLOR selected = new ColorImp(127, 127, 65);
            static COLOR hover_selected = COLOR.GREEN100;
            static COLOR inactive = new ColorImp(112, 87, 60); //COLOR.BROWN; //72, 58, 33
            static COLOR menu = new ColorImp(230, 220, 220);
            static COLOR unclickable = new ColorImp(127, 127, 80);
            static COLOR copper = new ColorImp(127, 127, 100);
            static COLOR hoverable = new ColorImp(115, 95, 55);
            static COLOR label = new ColorImp(127, 127, 80); //105,65,7
            static COLOR portrait = new ColorImp(220, 220, 220);
            static COLOR error = new ColorImp(127, 90, 90);
        }

        static class Button : CLICKABLE.ClickableAbs
        {
            private readonly SPRITE s;

            Button(SPRITE s) : base()
            {
                this.s = s;
                body.setWidth(s.width()).setHeight(s.height());
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                if (!isActive)
                    COLORS.inactive.bind();
                else if (isHovered && isSelected)
                    COLORS.hover_selected.bind();
                else if (isHovered)
                    COLORS.hover.bind();
                else if (isSelected)
                    COLORS.selected.bind();
                else
                    COLORS.normal.bind();
                s.render(r, body);
                COLOR.unbind();
            }
        }

        static class TEXT
        {
        }

        static CLICKABLE getNavButt(CharSequence name)
        {
            return new Button(UI.FONT().H1.getText(name));
        }

        static SPRITE getSmallText(CharSequence name)
        {
            return UI.FONT().M.getText(name);
        }

        static Text getSmallText(int width)
        {
            return UI.FONT().M.getText(width);
        }

        static SPRITE getBigTexts(CharSequence name)
        {
            return UI.FONT().H1.getText(name);
        }

        static HOVERABLE getBigText(CharSequence name)
        {
            return new HOVERABLE.Sprite(UI.FONT().H1.getText(name), COLORS.label);
        }

        static CLICKABLE.ClickableAbs getSmallButt(String name)
        {
            return new Button(UI.FONT().M.getText(name));
        }

        static CLICKABLE getBackArrow()
        {
            Button b = new Button(UI.FONT().H1.getText(¤¤back));
            b.body().moveX2(bounds.x2() - 80);
            b.body().moveY2(bounds.y1() - 25);
            return b;
        }

        static void addTitleText(GuiSection s, CharSequence title)
        {
            HOVERABLE.Sprite r = new HOVERABLE.Sprite(UI.FONT().H1.getText((object)title).toUpper(), COLORS.label);
            r.body().centerIn(bounds);
            r.body().moveY1((int)(left.y1() - r.body().height() - 10));
            s.add(r);
        }

        static abstract class OptionLine : GuiSection
        {
            private readonly CLICKABLE left;
            private readonly CLICKABLE right;
            private GText value = new GText(UI.FONT().M, 16);

            OptionLine(INTE ii, CharSequence l) : base()
            {
                body().setWidth(550);
                body().setHeight(1);

                GText label = new GText(UI.FONT().H2, l);
                label.color(COLORS.unclickable);
                left = new Button(getBigTexts("<<"))
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        activeSet(ii.get() > ii.min());
                        base.render(r, ds, activeIs(), isSelected, isHovered);
                    }
                };
                left.clickActionSet(new ACTION()
                {
                    public void exe()
                    {
                        ii.inc(-1);
                    }
                });
                //left.bodyM().moveX1(bodyM().gX2() + margin/6);
                add(left, 550 / 2 - margin / 20 - left.body().width(), 0);

                right = new Button(getBigTexts(">>"))
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        activeSet(ii.get() < ii.max());
                        base.render(r, ds, activeIs(), isSelected, isHovered);
                    }
                };
                right.clickActionSet(new ACTION()
                {
                    public void exe()
                    {
                        ii.inc(1);
                    }
                });
                //right.bodyM().moveX1(bodyM().gX2() + 5);
                addRightC(margin / 10, right);

                addCentredY(label, left.body().x1() - label.width() - 7);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                base.render(r, ds);

                Str.TMP.clear();
                value.clear();
                setValue(value);
                value.renderCY(r, right.body().x2() + 7, body().cY());
                COLORS.normal.bind();
                COLORS.unbind();
            }

            protected abstract void setValue(GText value);
        }

        public static class Shadower : GuiSection
        {
            private static readonly Rec shadow = new Rec(MenuScreen.bounds.width(), MenuScreen.bounds.height() - 50).moveC(C.DIM().cX(), C.DIM().cY());

            public Shadower() : base()
            {
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                Background.shadow = shadow;
                base.render(r, ds);
            }

            public static void ren(SPRITE_RENDERER r, float ds)
            {
                Background.shadow = shadow;
            }
        }
    }
}