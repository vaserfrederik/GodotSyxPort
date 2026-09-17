using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.colors;
using util.data.INT;
using util.gui.misc;
using view.main;

namespace util.gui.slider
{
    public class GSliderHor : GuiSection
    {
        private readonly int size;
        private readonly SPRITE c, cc;
        private readonly INTE target;

        private GSliderHor(SPRITE b1, SPRITE c, SPRITE cc, SPRITE b2, INTE target, int size)
        {
            this.target = target;

            this.size = b1.width();
            this.c = c;
            this.cc = cc;

            CLICKABLE b;

            b = new GButt.Glow(b1)
            {
                protected override void renAction()
                {
                    activeSet(target().get() > target().min());
                }

                protected override void clickA()
                {
                    if (target().get() > target().min())
                        target().inc(-1);
                }
            }.repetativeSet(true);
            add(b);

            if (size < 3 * this.size)
                size = 3 * this.size;

            size -= 2 * this.size;

            b = new Mid(size);
            addRightC(0, b);

            b = new GButt.Glow(b2)
            {
                protected override void renAction()
                {
                    activeSet(target().get() < target().max());
                }

                protected override void clickA()
                {
                    if (target().get() < target().max())
                        target().inc(1);
                }
            }.repetativeSet(true);
            addRightC(0, b);
        }

        protected INTE target()
        {
            return target;
        }

        public GSliderHor(INTE target, int size) : this(UI.decor().slider.makeSprite(0),
                                                      UI.decor().slider.makeSprite(1),
                                                      UI.decor().slider.makeSprite(2),
                                                      UI.decor().slider.makeSprite(3), target, size)
        {
        }

        private class Mid : CLICKABLE.ClickableAbs
        {
            private bool dragging;

            Mid(int s)
            {
                body.setWidth(s);
                body.setHeight(size);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                dragging &= MButt.LEFT.isDown();

                if (dragging)
                {
                    double sx = body.x1() + cc.width() / 2;
                    double ex = body.x2() - cc.width() / 2;
                    double w = ex - sx;
                    double d = (VIEW.mouse().x() - sx) / w;
                    int k = (int)CLAMP.d(Math.Round(d * target.max()), target().min(), target().max());
                    target().set(k);
                }

                isActive &= target().min() != target().max();

                if (!isActive)
                    GCOLOR.T().INACTIVE.bind();
                else if (isHovered || dragging)
                    COLOR.WHITE100.bind();
                else
                    COLOR.WHITE85.bind();

                int mids = body.width() / size;
                for (int i = 0; i < mids; i++)
                {
                    c.render(r, body.x1() + i * size, body.y1());
                }
                int left = body.width() % size;
                if (left != 0)
                {
                    int x1 = size * mids - size + left;
                    c.render(r, body.x1() + x1, body.y1());
                }

                if (target().max() != 0)
                {
                    double d = target().get() / (double)(target().max());
                    int x1 = (int)((body().width() - size) * d);

                    cc.render(r, body.x1() + x1, body.y1());
                }

                COLOR.unbind();
            }

            public override bool hover(COORDINATE mCoo)
            {
                return base.hover(mCoo);
            }

            protected override void renAction()
            {
                activeSet(target.get() > target.min() || target.get() < target.max());
            }

            protected override void clickA()
            {
                dragging = true;
                base.clickA();
            }
        }
    }
}