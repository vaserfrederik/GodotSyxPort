using System;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.colors;
using util.data.INT;
using util.gui.misc;
using util.info;
using view.keyboard;
using view.main;

namespace util.gui.slider
{
    public class GSliderInt : GuiSection
    {
        protected readonly INTE inValue;
        private const int midWidth = 8;
        private static readonly string ¤¤setAmount = "¤Set amount";
        private static readonly string ¤¤setAmountD = "¤Set amount {0}-{1}";

        static GSliderInt()
        {
            D.ts(typeof(GSliderInt));
        }

        public GSliderInt(INTE inValue, int width, bool input)
            : this(inValue, width, 24, input)
        {
        }

        public GSliderInt(INTE inValue, int width, bool buttons, bool input)
            : this(inValue, width, 24, buttons, input)
        {
        }

        public GSliderInt(INTE inValue, int width, int height, bool buttons, bool input)
        {
            this.inValue = inValue;

            if (input)
            {
                width -= (Icon.S + 2) * 3;
            }

            width -= 4;
            height -= 4;

            if (width < 0)
                width = 0;

            if (buttons)
            {
                addRightC(0, new GButt.ButtPanel(SPRITES.icons().s.minifier)
                {
                    protected override void clickA()
                    {
                        inValue.inc(-1);
                        if (KEYS.MAIN().MOD.isPressed())
                            inValue.set(inValue.min());
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        if (isHovered && MButt.LEFT.isDown())
                        {
                            clickSpeed1 += ds;
                            if (clickSpeed1 > 10)
                                clickSpeed1 = 10;
                            inValue.inc(-(int)clickSpeed1);
                        }
                        else
                        {
                            clickSpeed1 = 0;
                        }
                        base.render(r, ds, isActive, isSelected, isHovered);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.hov(text);
                        base.hoverInfoGet(text);
                    }
                });
            }

            addRightC(4, new Mid(width, height));
            pad(2, 2);

            if (buttons)
            {
                addRightC(4, new GButt.ButtPanel(SPRITES.icons().s.magnifier)
                {
                    protected override void clickA()
                    {
                        inValue.inc(1);
                        if (KEYS.MAIN().MOD.isPressed())
                            inValue.set(inValue.max());
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        if (isHovered && MButt.LEFT.isDown())
                        {
                            clickSpeed2 += ds * 2;
                            if (clickSpeed2 > 10)
                                clickSpeed2 = 10;
                            inValue.inc((int)clickSpeed2);
                        }
                        else
                        {
                            clickSpeed2 = 0;
                        }
                        base.render(r, ds, isActive, isSelected, isHovered);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.hov(text);
                        base.hoverInfoGet(text);
                    }
                });
            }

            if (input)
            {
                addRightC(2, new GButt.ButtPanel(SPRITES.icons().s.pluses)
                {
                    protected override void clickA()
                    {
                        Str.TMP.clear().add(¤¤setAmountD).insert(0, inValue.min()).insert(1, inValue.max());
                        VIEW.inters().input.requestInput(rec, Str.TMP);
                    }
                }.hoverInfoSet(¤¤setAmount));
            }
        }

        public GSliderInt(INTE inValue, int width, int height, bool input)
            : this(inValue, width, height, input, input)
        {
        }

        private readonly STRING_RECIEVER rec = new STRING_RECIEVER
        {
            public void acceptString(CharSequence string)
            {
                string s = "" + @string;
                try
                {
                    int i = int.Parse(s);
                    i = CLAMP.i(i, inValue.min(), inValue.max());
                    inValue.set(i);
                }
                catch (Exception e)
                {
                    // Handle exception if necessary
                }
            }
        };

        public override void render(SPRITE_RENDERER r, float ds)
        {
            activeSet(inValue.max() > 0);
            base.render(r, ds);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.add(GFORMAT.i(b.text(), inValue.get()));
            base.hoverInfoSelf(text);
        }

        protected void renderMidColor(SPRITE_RENDERER r, int x1, int width, int widthFull, int y1, int y2)
        {
            COLOR.WHITE65.render(r, x1, x1 + width, y1, y2);
        }

        private int RI = -2;
        private bool clicked = false;
        private double clickSpeed1 = 0;
        private double clickSpeed2 = 0;

        public void reset()
        {
            clickSpeed1 = 0;
            clickSpeed2 = 0;
            clicked = false;
            RI = -1;
        }

        private class Mid : CLICKABLE.ClickableAbs
        {
            Mid(int width, int height)
                : base(width, height - 4)
            {
            }

            protected override void clickA()
            {
                clicked = true;
                double x = (VIEW.mouse().x() - body().x1()) / (double)body().width();
                inValue.setD(CLAMP.d(x, 0, 1));
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                clicked &= (MButt.LEFT.isDown() && Math.Abs(RI - VIEW.RI()) <= 1);
                RI = VIEW.RI();
                if (clicked)
                {
                    double x = (VIEW.mouse().x() - body().x1()) / (double)body().width();
                    inValue.setD(CLAMP.d(x, 0, 1));
                }

                GCOLOR.UI().border().render(r, body, 2);
                GCOLOR.UI().bg(isActive, isSelected, isHovered).render(r, body, 1);

                int x2 = body().x1() + (int)(inValue.getD() * body().width());
                {
                    int my = isHovered || clicked ? 0 : 2;
                    renderMidColor(r, body().x1(), x2 - body().x1(), body().width(), body().y1() + my, body().y2() - my);
                }

                int cx = (int)(body.x1() + midWidth / 2 + (body().width() - midWidth) * inValue.getD());

                GCOLOR.UI().border().render(r, cx - midWidth / 2, cx + midWidth / 2, body().y1(), body().y2());
                COLOR c = isHovered || clicked ? GCOLOR.T().H1 : GCOLOR.T().H2;
                c.render(r, cx - midWidth / 2 + 1, cx + midWidth / 2 - 1, body().y1() + 1, body().y2() - 1);
                COLOR.BLACK.render(r, cx - 1, cx + 2, body().y1() + 2, body().y2() - 2);
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (base.hover(mCoo))
                {
                    if (KEYS.MAIN().MOD.isPressed() || KEYS.MAIN().UNDO.isPressed())
                    {
                        double d = MButt.clearWheelSpin();
                        if (d < 0)
                            inValue.inc(-1);
                        else if (d > 0)
                            inValue.inc(1);
                    }

                    return true;
                }
                return false;
            }
        }

        public static void renderMid(SPRITE_RENDERER r, int x1, int x2, int y1, int y2, double d, bool isActive, bool isSelected, bool isHovered)
        {
            GCOLOR.UI().border().render(r, x1 - 2, x2 + 2, y1 - 2, y2 + 2);
            GCOLOR.UI().bg(isActive, isSelected, isHovered).render(r, x1 - 1, x2 + 1, y1 - 1, y2 + 1);

            int width = x2 - x1;

            x2 = x1 + (int)(d * width);
            COLOR.WHITE65.render(r, x1, x2, y1, y2);
        }
    }
}