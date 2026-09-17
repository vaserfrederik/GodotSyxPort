using System;
using init.sprite.UI;
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

namespace util.gui.slider
{
    public class GSliderVer : GuiSection
    {
        private readonly INTE target;
        private readonly int size;
        private readonly SPRITE c, cc;

        private GSliderVer(SPRITE b1, SPRITE c, SPRITE cc, SPRITE b2, INTE target, int width)
        {
            this.target = target;

            this.size = b1.width();
            this.c = c;
            this.cc = cc;

            GButt.Glow b;

            b = new GButt.Glow(b1)
            {
                protected override void renAction()
                {
                    activeSet(target.get() > target.min());
                }

                protected override void clickA()
                {
                    if (target.get() > target.min())
                        target.inc(-1);
                }
            };
            b.body.setDim(b1.width());
            b.repetativeSet(true);
            add(b);

            if (width < 3 * this.size)
                width = 3 * this.size;

            width -= 2 * this.size;

            addDownC(0, new Mid(width));

            b = new GButt.Glow(b2)
            {
                protected override void renAction()
                {
                    activeSet(target.get() < target.max());
                }

                protected override void clickA()
                {
                    if (target.get() < target.max())
                        target.inc(1);
                }
            };
            b.body.setDim(b1.width());
            b.repetativeSet(true);
            addDownC(0, b);
        }

        public GSliderVer(INTE target, int size)
            : this(UI.decor().slider.makeSprite(4),
                  UI.decor().slider.makeSprite(5),
                  UI.decor().slider.makeSprite(6),
                  UI.decor().slider.makeSprite(7), target, size)
        {
        }

        public static int WIDTH()
        {
            return UI.decor().slider.size();
        }

        private class Mid : CLICKABLE.ClickableAbs
        {
            private bool dragging;

            public Mid(int s)
            {
                body.setHeight(s);
                body.setWidth(size);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                dragging &= MButt.LEFT.isDown();

                if (dragging)
                {
                    double d = (CORE.getInput().getMouse().getCoo().y() - body.y1()) / (double)body.height();
                    int k = (int)CLAMP.d(Math.Round(d * target.max()), target.min(), target.max());
                    target.set(k);
                }

                isActive &= target.min() != target.max();

                if (!isActive)
                    GCOLOR.T().INACTIVE.bind();
                else if (isHovered || dragging)
                    COLOR.WHITE100.bind();
                else
                    COLOR.WHITE65.bind();

                int mids = body.height() / size;
                for (int i = 0; i < mids; i++)
                {
                    c.render(r, body.x1(), body.y1() + i * size);
                }
                int left = body.height() % size;
                if (left != 0)
                {
                    int y1 = size * mids - size + left;
                    c.render(r, body.x1(), body.y1() + y1);
                }
                if (target.max() > 0)
                {
                    int y1 = (body().height() - size) * target.get() / (target.max());
                    cc.render(r, body.x1(), body.y1() + y1);
                }

                COLOR.unbind();
            }

            protected override void renAction()
            {
                activeSet(target.get() > target.min() || target.get() < target.max());
            }

            public override bool hover(COORDINATE mCoo)
            {
                dragging &= MButt.LEFT.isDown();

                if (dragging)
                {
                    double d = (CORE.getInput().getMouse().getCoo().y() - body.y1()) / (double)body.height();
                    int k = (int)CLAMP.d(d * target.max(), target.min(), target.max());
                    target.set(k);
                }
                return base.hover(mCoo);
            }

            protected override void clickA()
            {
                dragging = true;
                base.clickA();
            }
        }
    }
}